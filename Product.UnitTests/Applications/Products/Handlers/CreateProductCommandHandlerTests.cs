using FluentAssertions;
using MediatR;
using Moq;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Domain.Entities;
namespace Product.UnitTests.Applications.Products.Handlers;

public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IDomainEventPublisher> _domainEventPublisherMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<ILoggerService<CreateProductCommandHandler>> _loggerMock;
        private readonly Mock<IRedisCacheService> _redisCacheServiceMock;

        private readonly CreateProductCommandHandler _handler;
        private const string CachePrefixAll = "product:all";
        
        public CreateProductCommandHandlerTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _domainEventPublisherMock = new Mock<IDomainEventPublisher>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _loggerMock = new Mock<ILoggerService<CreateProductCommandHandler>>();
            _redisCacheServiceMock = new Mock<IRedisCacheService>();

            _handler = new CreateProductCommandHandler(
                _productRepositoryMock.Object,
                _domainEventPublisherMock.Object,
                _categoryServiceMock.Object,
                _redisCacheServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductAndReturnResponse_WhenCommandIsValid()
        {
            var categoryId = Guid.NewGuid();
            var category = new Category("Category 1", 10);
            var command = new CreateProductCommand("Product 1", "Desc 1", categoryId, 25);

            _categoryServiceMock
                .Setup(s => s.GetCategoryAsync(categoryId))
                .ReturnsAsync(category);

            Api.Domain.Entities.Product capturedProduct = null!;
            _productRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Api.Domain.Entities.Product>()))
                .Callback<Api.Domain.Entities.Product>(p => capturedProduct = p)
                .Returns(Task.CompletedTask);

            _domainEventPublisherMock
                .Setup(p => p.PublishAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _redisCacheServiceMock
                .Setup(c => c.RemoveAsync(CachePrefixAll))
                .Returns(Task.CompletedTask);
            var response = await _handler.Handle(command, CancellationToken.None);
            
            capturedProduct.Should().NotBeNull();
            capturedProduct.Title.Should().Be(command.Title);
            capturedProduct.Description.Should().Be(command.Description);
            capturedProduct.CategoryId.Should().Be(command.CategoryId);
            capturedProduct.Category.Should().Be(category);
            capturedProduct.StockQuantity.Should().Be(command.StockQuantity);

            response.Should().NotBeNull();
            response.Message.Should().Be(ResponseMessages.CreatedSuccess);

            response.ProductDto.Should().NotBeNull();
            response.ProductDto.Id.Should().Be(capturedProduct.Id);
            response.ProductDto.Title.Should().Be(command.Title);
            response.ProductDto.Description.Should().Be(command.Description);
            response.ProductDto.Category.Should().Be(category);
            response.ProductDto.StockQuantity.Should().Be(command.StockQuantity);
            response.ProductDto.IsLive.Should().BeTrue();

            _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Api.Domain.Entities.Product>()), Times.Once);
            _domainEventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(capturedProduct.DomainEvents.Count));
            _redisCacheServiceMock.Verify(c => c.RemoveAsync(CachePrefixAll), Times.Once);
            _loggerMock.Verify(l => l.LogInfo(It.Is<string>(msg => msg.Contains("Product created"))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
        {
            var categoryId = Guid.NewGuid();
            var expectedMessage = ExceptionMessages.CategoryNotFound;

            var command = new CreateProductCommand("Product 1", "Desc 1", categoryId, 25);

            _categoryServiceMock
                .Setup(s => s.GetCategoryAsync(categoryId))
                .ThrowsAsync(new KeyNotFoundException(expectedMessage));

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
            exception.Message.Should().Be(expectedMessage);

            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(msg => msg.Contains(expectedMessage))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var categoryId = Guid.NewGuid();
            var category = new Category("Category 1", 10);
            var command = new CreateProductCommand("Product 1", "Desc 1", categoryId, 25);

            _categoryServiceMock
                .Setup(s => s.GetCategoryAsync(categoryId))
                .ReturnsAsync(category);

            _productRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Api.Domain.Entities.Product>()))
                .ThrowsAsync(new Exception("Database error"));

            var act = async () => await _handler.Handle(command, CancellationToken.None);
            
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _loggerMock.Verify(
                l => l.LogError(It.Is<string>(s => s.Contains("Unexpected error occurred while creating product.")),
                It.IsAny<Exception>()),
                Times.Once);
        }
    }




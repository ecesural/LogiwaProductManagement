using FluentAssertions;
using MediatR;
using Moq;
using Product.Api.Application.Common;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Domain.Entities;
using Product.Api.Presentation.Extensions;

namespace Product.UnitTests.Applications.Products.Handlers;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IDomainEventPublisher> _domainEventPublisherMock = new();
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock = new();
    private readonly Mock<ILoggerService<CreateProductCommandHandler>> _loggerMock = new();

    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _domainEventPublisherMock.Object,
            _categoryServiceMock.Object,
            _redisCacheServiceMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldCreateProductAndReturnResponse_WhenValid()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category("Shoes", 100);
        var command = new CreateProductCommand("Sneaker", "Stylish sneaker", categoryId, 50);

        _categoryServiceMock
            .Setup(s => s.GetCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        Api.Domain.Entities.Product? addedProduct = null;

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Api.Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
            .Callback<Api.Domain.Entities.Product, CancellationToken>((p, _) => addedProduct = p)
            .Returns(Task.CompletedTask);
        
        _redisCacheServiceMock
            .Setup(c => c.RemoveAsync(CacheKeys.ProductAll, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.Message.Should().Be(ResponseMessages.CreatedSuccess);

        response.ProductDto.Should().NotBeNull();
        response.ProductDto.Title.Should().Be(command.Title);
        response.ProductDto.Description.Should().Be(command.Description);
        response.ProductDto.Category.Should().Be(category);
        response.ProductDto.StockQuantity.Should().Be(command.StockQuantity);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Api.Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _redisCacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProductAll, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Product created"))), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCategoryNotFound()
    {
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand("Boots", "Leather boots", categoryId, 10);
        var exceptionMessage = "Category not found";

        _categoryServiceMock
            .Setup(s => s.GetCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException(exceptionMessage));

        var act = () => _handler.Handle(command, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        exception.Message.Should().Be(exceptionMessage);

        _loggerMock.Verify(l => l.LogWarning(It.Is<string>(m => m.Contains(exceptionMessage))), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_AndLog_WhenUnexpectedErrorOccurs()
    {

        var command = new CreateProductCommand("Jacket", "Winter jacket", Guid.NewGuid(), 5);
        var category = new Category("Clothing", 10);

        _categoryServiceMock
            .Setup(s => s.GetCategoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Api.Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Failure"));
        
        var act = () => _handler.Handle(command, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<Exception>(act);
        ex.Message.Should().Be("DB Failure");

        _loggerMock.Verify(l => l.LogError("Unexpected error occurred while creating product.", It.IsAny<Exception>()), Times.Once);
    }
}

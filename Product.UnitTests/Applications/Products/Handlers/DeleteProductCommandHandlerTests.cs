using FluentAssertions;
using MediatR;
using Moq;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Domain.Entities;
namespace Product.UnitTests.Applications.Products.Handlers;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IDomainEventPublisher> _domainEventPublisherMock;
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
    private readonly Mock<ILoggerService<DeleteProductCommandHandler>> _loggerMock;
    private readonly DeleteProductCommandHandler _handler;
    private const string CachePrefix = "product:";
    public DeleteProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _domainEventPublisherMock = new Mock<IDomainEventPublisher>();
        _redisCacheServiceMock = new Mock<IRedisCacheService>();
        _loggerMock = new Mock<ILoggerService<DeleteProductCommandHandler>>();

        _handler = new DeleteProductCommandHandler(
            _productRepositoryMock.Object,
            _domainEventPublisherMock.Object,
            _redisCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_WhenProductDeletedSuccessfully()
    {
        var productId = Guid.NewGuid();
        var category = new Category("Category 1", 10);

        var product = new Api.Domain.Entities.Product("Product 1", "Desc 1", category.Id, 30, category);

        typeof(Api.Domain.Entities.Product).GetProperty("Id")!.SetValue(product, productId);

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(repo => repo.DeleteAsync(product))
            .Returns(Task.CompletedTask);

        _domainEventPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheKey = CachePrefix + productId;

        _redisCacheServiceMock
            .Setup(cache => cache.RemoveAsync(cacheKey))
            .Returns(Task.CompletedTask);

        var command = new DeleteProductCommand(productId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.ProductId.Should().Be(productId);
        result.Message.Should().Be(ResponseMessages.DeletedSuccess);

        _productRepositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        _productRepositoryMock.Verify(repo => repo.DeleteAsync(product), Times.Once);
        _redisCacheServiceMock.Verify(cache => cache.RemoveAsync(cacheKey), Times.Once);
        _loggerMock.Verify(log => log.LogInfo(It.Is<string>(msg => msg.Contains("Product deleted"))), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        var expectedMessage = ExceptionMessages.ProductNotFound;

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Api.Domain.Entities.Product)null!);

        var command = new DeleteProductCommand(productId);
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        exception.Message.Should().Be(expectedMessage);

        _loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(msg => msg.Contains(expectedMessage))),
            Times.Exactly(2) 
        );
    }
    
    [Fact]
    public async Task Handle_ShouldThrowException_WhenUnexpectedErrorOccurs()
    {
        var productId = Guid.NewGuid();
        var category = new Category("Category 1", 10);
        var product = new Api.Domain.Entities.Product("Product 1", "Desc 1", category.Id, 30,category);

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(repo => repo.DeleteAsync(product))
            .ThrowsAsync(new Exception("Database failure"));

        var command = new DeleteProductCommand(productId);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database failure");

        _loggerMock.Verify(x => x.LogError(It.Is<string>(s => s.Contains("Unexpected error occurred")), It.IsAny<Exception>()), Times.Once);
    }
}
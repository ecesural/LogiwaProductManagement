using FluentAssertions;
using Moq;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Application.Features.Products.Queries;

namespace Product.UnitTests.Applications.Products.Handlers;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock = new();
    private readonly Mock<ILoggerService<GetProductByIdQueryHandler>> _loggerMock = new();
    private const string CachePrefix = "product:";

    private GetProductByIdQueryHandler CreateHandler() =>
        new(
            _productRepositoryMock.Object,
            _redisCacheServiceMock.Object,
            _loggerMock.Object
        );

    [Fact]
    public async Task Handle_ShouldReturnProductFromCache_WhenExistsInCache()
    {
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            Title = "Cached Title",
            Description = "Cached Desc",
            StockQuantity = 10,
            IsLive = true
        };

        var cacheKey = CachePrefix + productId;
        _redisCacheServiceMock.Setup(x => x.GetAsync<ProductDto>(cacheKey))
            .ReturnsAsync(productDto);

        var handler = CreateHandler();

        var result = await handler.Handle(new GetProductByIdQuery(productId), CancellationToken.None);

        result.Should().BeEquivalentTo(productDto);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _loggerMock.Verify(x => x.LogInfo(It.Is<string>(s => s.Contains("returned from cache"))), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductFromDbAndCacheIt_WhenNotInCache()
    {
        var productId = Guid.NewGuid();

        var product = new Api.Domain.Entities.Product("Product 1", "Desc 1", null, 5, null);
        var idProp = typeof(Api.Domain.Entities.Product).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        idProp!.SetValue(product, productId);

        var cacheKey = CachePrefix + productId;
        _redisCacheServiceMock.Setup(x => x.GetAsync<ProductDto>(cacheKey))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _redisCacheServiceMock.Setup(x => x.SetAsync(cacheKey, It.IsAny<ProductDto>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var result = await handler.Handle(new GetProductByIdQuery(productId), CancellationToken.None);

        result.Id.Should().Be(productId);
        result.Title.Should().Be("Product 1");

        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId), Times.Once);
        _redisCacheServiceMock.Verify(x => x.SetAsync(cacheKey, It.IsAny<ProductDto>(), It.IsAny<TimeSpan>()), Times.Once);
        _loggerMock.Verify(x => x.LogInfo(It.Is<string>(s => s.Contains("cached and returned"))), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        var cacheKey = CachePrefix + productId;

        _redisCacheServiceMock.Setup(x => x.GetAsync<ProductDto>(cacheKey))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Api.Domain.Entities.Product?)null);

        var handler = CreateHandler();

        var act = async () => await handler.Handle(new GetProductByIdQuery(productId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Exactly(2)); 
    }
}
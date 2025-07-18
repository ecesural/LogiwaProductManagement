using System.Reflection;
using FluentAssertions;
using Moq;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Application.Features.Products.Queries;

namespace Product.UnitTests.Applications.Products.Handlers;

public class FilterProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock = new();
    private readonly Mock<ILoggerService<FilterProductsQueryHandler>> _loggerMock = new();
    private const string FilterCachePrefix = "products:filter:";

    private FilterProductsQueryHandler CreateHandler() =>
        new(_productRepositoryMock.Object, _redisCacheServiceMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ShouldReturnProductsFromCache_WhenCacheExists()
    {
        var query = new FilterProductsQuery("product 1", 5, 100);
        var cacheKey =$"{FilterCachePrefix}product1:5:100";

        var cachedList = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Title = "CachedProduct", StockQuantity = 10 }
        };

        _redisCacheServiceMock.Setup(x => x.GetAsync<List<ProductDto>>(cacheKey))
            .ReturnsAsync(cachedList);

        var handler = CreateHandler();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(cachedList);
        _productRepositoryMock.Verify(x => x.FilterAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
        _loggerMock.Verify(x => x.LogInfo("Returned product list from cache."), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductsFromDbAndCacheThem_WhenCacheIsEmpty()
    {
        var query = new FilterProductsQuery("product", 1, 10);
        var cacheKey = $"{FilterCachePrefix}product:1:10";

        _redisCacheServiceMock.Setup(x => x.GetAsync<List<ProductDto>>(cacheKey))
            .ReturnsAsync((List<ProductDto>?)null);

        var dbProducts = new List<Api.Domain.Entities.Product>
        {
            new("Product 1", "desc 1", null, 5, null),
            new("Product 2", "desc 2", null, 7, null)
        };
        
        foreach (var t in dbProducts)
        {
            typeof(Api.Domain.Entities.Product).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(t, Guid.NewGuid());
        }

        _productRepositoryMock.Setup(x => x.FilterAsync(query.Keyword, query.MinStock, query.MaxStock))
            .ReturnsAsync(dbProducts);

        _redisCacheServiceMock.Setup(x => x.SetAsync(cacheKey, It.IsAny<List<ProductDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        _productRepositoryMock.Verify(x => x.FilterAsync(query.Keyword, query.MinStock, query.MaxStock), Times.Once);
        _redisCacheServiceMock.Verify(x => x.SetAsync(cacheKey, It.IsAny<List<ProductDto>>(), It.IsAny<TimeSpan>()), Times.Once);
        _loggerMock.Verify(x => x.LogInfo("Product list cached and returned."), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogErrorAndThrow_WhenExceptionOccurs()
    {
        var query = new FilterProductsQuery("product 1", null, null);
        var cacheKey = $"{FilterCachePrefix}product1:minnull:maxnull";

        _redisCacheServiceMock.Setup(x => x.GetAsync<List<ProductDto>>(cacheKey))
            .ReturnsAsync((List<ProductDto>?)null);

        _productRepositoryMock.Setup(x => x.FilterAsync(query.Keyword, query.MinStock, query.MaxStock))
            .ThrowsAsync(new Exception("Database error"));

        var handler = CreateHandler();

        var act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        _loggerMock.Verify(x => x.LogError("Error occurred while filtering products.", It.IsAny<Exception>()), Times.Once);
    }
}
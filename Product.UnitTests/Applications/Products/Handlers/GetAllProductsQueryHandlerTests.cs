using FluentAssertions;
using Moq;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Application.Features.Products.Queries;
using Product.Api.Domain.Entities;

namespace Product.UnitTests.Applications.Products.Handlers;

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
    private readonly Mock<ILoggerService<GetAllProductsQueryHandler>> _loggerMock;
    private readonly GetAllProductsQueryHandler _handler;
    private const string CacheKey = "product:all";
    
    public GetAllProductsQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _redisCacheServiceMock = new Mock<IRedisCacheService>();
        _loggerMock = new Mock<ILoggerService<GetAllProductsQueryHandler>>();
        _handler = new GetAllProductsQueryHandler(
            _productRepositoryMock.Object,
            _redisCacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnProducts_FromCache_WhenCacheExists()
    {
        var cachedProducts = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Title = "Cached Product",
                Description = "From cache",
                Category = new Category("Shirts", 10),
                StockQuantity = 10,
                IsLive = true
            }
        };

        _redisCacheServiceMock
            .Setup(c => c.GetAsync<List<ProductDto>>(CacheKey))
            .ReturnsAsync(cachedProducts);

        var query = new GetAllProductsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(cachedProducts);
        _loggerMock.Verify(l => l.LogInfo("All products returned from cache."), Times.Once);
        _productRepositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFetchFromRepository_WhenCacheIsNull()
    {
        _redisCacheServiceMock
            .Setup(c => c.GetAsync<List<ProductDto>>(CacheKey))
            .ReturnsAsync((List<ProductDto>)null!);

        var category = new Category("Category", 10);
        var product = new Api.Domain.Entities.Product("Product A", "Desc A", category.Id, 5, category);
        var products = new List<Api.Domain.Entities.Product> { product };

        _productRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        _redisCacheServiceMock
            .Setup(c => c.SetAsync(CacheKey, It.IsAny<List<ProductDto>>(), TimeSpan.FromHours(1)))
            .Returns(Task.CompletedTask);

        var query = new GetAllProductsQuery();
        
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Product A");
        result[0].Category?.Name.Should().Be("Category");

        _productRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _redisCacheServiceMock.Verify(c => c.SetAsync(CacheKey, It.IsAny<List<ProductDto>>(), TimeSpan.FromHours(1)), Times.Once);
        _loggerMock.Verify(l => l.LogInfo("All products cached and returned."), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogAndThrow_WhenExceptionOccurs()
    {
        _redisCacheServiceMock
            .Setup(c => c.GetAsync<List<ProductDto>>(CacheKey))
            .ThrowsAsync(new Exception("Redis down"));

        var query = new GetAllProductsQuery();

        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Redis down");
        _loggerMock.Verify(l => l.LogError("Unexpected error occurred while getting all products.", It.IsAny<Exception>()), Times.Once);
    }
}

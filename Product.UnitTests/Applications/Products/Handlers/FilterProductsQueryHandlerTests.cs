using System.Reflection;
using FluentAssertions;
using Moq;
using Product.Api.Application.Common;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Application.Features.Products.Queries;
using Product.Api.Domain.Entities;
using Product.Api.Presentation.Extensions;

namespace Product.UnitTests.Applications.Products.Handlers;

public class FilterProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
    private readonly Mock<ILoggerService<FilterProductsQueryHandler>> _loggerMock;

    private readonly FilterProductsQueryHandler _handler;

    public FilterProductsQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _redisCacheServiceMock = new Mock<IRedisCacheService>();
        _loggerMock = new Mock<ILoggerService<FilterProductsQueryHandler>>();

        _handler = new FilterProductsQueryHandler(
            _productRepositoryMock.Object,
            _redisCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsProducts_FromCache()
    {
        // Arrange
        var query = new FilterProductsQuery("test", 1, 10);
        var expectedCacheKey = $"{CacheKeys.ProductFilter}k=test&min=1&max=10";
        var cachedProducts = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Title = "test",
                Description = "Cached Description",
                Category = null,
                StockQuantity = 3,
                IsLive = true
            }
        };

        _redisCacheServiceMock
            .Setup(c => c.GetAsync<List<ProductDto>>(expectedCacheKey,It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedProducts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("test", result[0].Title);
        _productRepositoryMock.Verify(x => x.FilterAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsProducts_FromRepository_WhenCacheMiss()
    {
        // Arrange
        var query = new FilterProductsQuery("test", 1, 10);
        var expectedCacheKey = $"{CacheKeys.ProductFilter}k=test&min=1&max=10";
        var category = new Category("Electronics", 5);

        var repoProducts = new List<Api.Domain.Entities.Product>
        {
            new Api.Domain.Entities.Product(
                title: "Repo Product",
                description: "Repository Description",
                categoryId: category.Id,
                stockQuantity: 5,
                category: category)
        };

        _redisCacheServiceMock
            .Setup(c => c.SetAsync(expectedCacheKey, It.IsAny<List<ProductDto>>(), TimeSpan.FromHours(1),It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(x => x.FilterAsync(query.Keyword, query.MinStock, query.MaxStock, It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoProducts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Repo Product", result[0].Title);
        Assert.Equal("Repository Description", result[0].Description);
        Assert.Equal(category, result[0].Category);
        Assert.Equal(5, result[0].StockQuantity);
        _loggerMock.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("DB"))), Times.Once);
    }

    [Fact]
    public async Task Handle_LogsErrorAndThrows_WhenExceptionThrown()
    {
        // Arrange
        var query = new FilterProductsQuery("test", 1, 10);
        var expectedCacheKey = $"{CacheKeys.ProductFilter}k=test&min=1&max=10";
        _redisCacheServiceMock
            .Setup(c => c.GetAsync<List<ProductDto>>(expectedCacheKey,It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis down"));

        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Redis down");
    }
}
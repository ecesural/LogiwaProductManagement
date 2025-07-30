using Moq;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Domain.Entities;

namespace Product.UnitTests.Applications.Products.Handlers;
public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IDomainEventPublisher> _domainEventPublisherMock = new();
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock = new();
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly Mock<ILoggerService<UpdateProductCommandHandler>> _loggerMock = new();

    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(
            _productRepositoryMock.Object,
            _domainEventPublisherMock.Object,
            _redisCacheServiceMock.Object,
            _categoryServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductNotFound()
    {
        var productId = Guid.NewGuid();
        var request = new UpdateProductCommand(productId, "Product 1", "Desc 1", Guid.NewGuid(), 5);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId,It.IsAny<CancellationToken>())).ReturnsAsync((Api.Domain.Entities.Product)null!);
        
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCategoryNotFound()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category("Category", 5);
        typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(category, categoryId);

        var product = new Api.Domain.Entities.Product("Old Title", "Old Desc", categoryId, 10, category);

        var request = new UpdateProductCommand(productId, "New Title", "New Desc", categoryId, 15);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId,It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _categoryServiceMock.Setup(x => x.GetCategoryAsync(categoryId,It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException(ExceptionMessages.CategoryNotFound));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(request, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenAllDataValid()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category("Category", 1);
        typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(category, categoryId);

        var product = new Api.Domain.Entities.Product("Old Title", "Old Desc", categoryId, 5, category);
        var request = new UpdateProductCommand(productId, "New Title", "New Desc", categoryId, 99);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId,It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _categoryServiceMock.Setup(x => x.GetCategoryAsync(categoryId,It.IsAny<CancellationToken>())).ReturnsAsync(category);
        
        await _handler.Handle(request, CancellationToken.None);
        Assert.Equal("New Title", product.Title);
        Assert.Equal("New Desc", product.Description);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal(99, product.StockQuantity);
    }

    [Fact]
    public async Task Handle_ShouldSetCategoryToNull_WhenCategoryIdIsEmptyGuid()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var oldCategory = new Category("Category", 4);
        typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(oldCategory, categoryId);

        var product = new Api.Domain.Entities.Product("Title", "Desc", categoryId, 10, oldCategory);
        var request = new UpdateProductCommand(productId, "New Title", "New Desc", Guid.Empty, 1);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId,It.IsAny<CancellationToken>())).ReturnsAsync(product);
        
        await _handler.Handle(request, CancellationToken.None);
        Assert.Null(product.Category);
        Assert.Equal(Guid.Empty, product.CategoryId);
    }
}

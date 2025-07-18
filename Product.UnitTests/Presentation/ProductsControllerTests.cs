using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;
using Product.Api.Application.Features.Products.Responses;
using Product.Api.Domain.Entities;
using Product.Api.Presentation.Controllers;

namespace Product.UnitTests.Presentation;

public class ProductsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturn201Created_WithLocationHeaderAndResponseBody()
    {
        var productId = Guid.NewGuid();

        var command = new CreateProductCommand(
            Title: "T-shirt",
            Description: "Cotton T-shirt",
            CategoryId: Guid.NewGuid(),
            StockQuantity: 100
        );

        var expectedResponse = new CreateAndUpdateProductResponse
        {
            ProductDto = new ProductDto
            {
                Id = productId,
                Title = command.Title,
                Description = command.Description,
                Category = null!,
                StockQuantity = command.StockQuantity,
                IsLive = true
            },
            Message = "Product created successfully"
        };

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        
        var result = await _controller.Create(command);
        
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAt = result as CreatedAtActionResult;

        createdAt!.StatusCode.Should().Be(201);
        createdAt.ActionName.Should().Be(nameof(_controller.GetById));
        createdAt.Value.Should().BeEquivalentTo(expectedResponse);
    }
    
    [Fact]
    public async Task Update_ShouldReturn200Ok_WithUpdatedProduct()
    {
        var productId = Guid.NewGuid();
        var originalCommand = new UpdateProductCommand(
            Title: "New Title",
            Description: "New Desc",
            CategoryId: Guid.NewGuid(),
            StockQuantity: 50,
            ProductId: Guid.Empty 
        );

        var updatedCommand = originalCommand with { ProductId = productId };

        var expectedResponse = new CreateAndUpdateProductResponse
        {
            ProductDto = new ProductDto
            {
                Id = productId,
                Title = "New Title",
                Description = "New Desc",
                Category = null!,
                StockQuantity = 10,
                IsLive = true
            },
            Message = "Product updated successfully"
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(updatedCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new ProductsController(mediatorMock.Object);
        var result = await controller.Update(productId, originalCommand);
        
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Delete_ShouldReturn200Ok_WithDeleteProductResponse()
    {
        var productId = Guid.NewGuid();
        var expectedResponse = new DeleteProductResponse
        {
            ProductId = productId,
            Message = "Product deleted successfully"
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new ProductsController(mediatorMock.Object);

        var result = await controller.Delete(productId);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }
    
    [Fact]
    public async Task GetById_ShouldReturn200Ok_WithProductDto()
    {
        var productId = Guid.NewGuid();

        var expectedCategory = new Category("T-Shirts", 10);

        var expectedProduct = new ProductDto
        {
            Id = productId,
            Title = "Test Product",
            Description = "Test Description",
            Category = expectedCategory,
            StockQuantity = 50,
            IsLive = true
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        var controller = new ProductsController(mediatorMock.Object);
        
        var result = await controller.GetById(productId);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedProduct);
    }
    
    [Fact]
    public async Task GetAll_ShouldReturn200Ok_WithListOfProductDto()
    {
        var expectedCategory = new Category("T-Shirts", 10);

        var productList = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Title = "Product 1",
                Description = "Desc 1",
                Category = expectedCategory,
                StockQuantity = 20,
                IsLive = true
            },
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Title = "Product 2",
                Description = "Desc 2",
                Category = expectedCategory,
                StockQuantity = 40,
                IsLive = true
            }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productList);

        var controller = new ProductsController(mediatorMock.Object);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(productList);
    }

    [Fact]
    public async Task GetFiltered_ShouldReturn200Ok_WithListOfProductDto()
    {
        var expectedCategory = new Category("T-Shirts", 10);

        var query = new FilterProductsQuery(
            Keyword: "shirt",
            MinStock: 0,
            MaxStock: 100
        );

        var productList = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Title = "White T-shirt",
                Description = "Cotton fabric",
                Category = expectedCategory,
                StockQuantity = 50,
                IsLive = true
            },
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Title = "Black T-shirt",
                Description = "Polyester fabric",
                Category = expectedCategory,
                StockQuantity = 80,
                IsLive = true
            }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productList);

        var controller = new ProductsController(mediatorMock.Object);

        var result = await controller.GetFiltered(query);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(productList);
    }
}
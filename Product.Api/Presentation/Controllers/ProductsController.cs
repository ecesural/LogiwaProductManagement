using System.Net.Mime;
using MediatR;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Queries;
using Microsoft.AspNetCore.Mvc;
namespace Product.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="command">Product creation data transfer object.</param>
    /// <returns>Returns the ID of the newly created product.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.ProductDto.Id }, result);
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    /// <param name="id">The product's unique identifier.</param>
    /// <param name="command">Updated product data.</param>
    /// <returns>Returns the updated product information.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        var updatedCommand = command with { ProductId = id };
        var result = await mediator.Send(updatedCommand);
        return Ok(result);
    }

    /// <summary>
    /// Delete a product by its ID.
    /// </summary>
    /// <param name="id">The product's unique identifier.</param>
    /// <returns>No content if deletion is successful.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand(id);
        var result= await mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get a product by its ID.
    /// </summary>
    /// <param name="id">The product's unique identifier.</param>
    /// <returns>Returns the product details.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query);
        return Ok(result);
    }
   
    /// <summary>
    /// Get all products.
    /// </summary>
    /// <returns>Returns a list of all products.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllProductsQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
 
    /// <summary>
    /// Get filtered products by keyword and stock range.
    /// </summary>
    /// <param name="query">Filtering criteria including keyword and stock range.</param>
    /// <returns>Returns the list of matching products.</returns>
    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered([FromQuery] FilterProductsQuery query)
    {
        var results = await mediator.Send(query);
        return Ok(results);
    }
}

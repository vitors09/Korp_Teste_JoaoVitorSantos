using Korp.Inventory.Api.Application;
using Korp.Inventory.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Inventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IInventoryService inventoryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await inventoryService.CreateProductAsync(
            request.Code,
            request.Description,
            request.Balance,
            cancellationToken);

        var response = ProductResponse.FromDomain(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(
        CancellationToken cancellationToken)
    {
        var products = await inventoryService.ListProductsAsync(cancellationToken);
        return Ok(products.Select(ProductResponse.FromDomain).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await inventoryService.GetProductAsync(id, cancellationToken);
        return Ok(ProductResponse.FromDomain(product));
    }
}

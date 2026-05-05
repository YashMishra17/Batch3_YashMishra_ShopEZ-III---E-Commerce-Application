using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopEZ.API.DTOs;
using ShopEZ.API.Exceptions;
using ShopEZ.API.Services.Interfaces;

namespace ShopEZ.API.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET /api/products — no token needed (browsing is public)
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductDTO>), 200)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                IEnumerable<ProductDTO> products = await _productService.GetAllProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        // GET /api/products/{id} — no token needed
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                ProductDTO? product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(new { success = false, message = $"Product with ID {id} was not found." });
                return Ok(new { success = true, data = product });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        // POST /api/products — Admin role required
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });
            try
            {
                ProductDTO created = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(nameof(GetProductById), new { id = created.ProductId }, new { success = true, data = created });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        // PUT /api/products/{id} — Admin role required
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });
            try
            {
                ProductDTO? updated = await _productService.UpdateProductAsync(id, dto);
                if (updated == null)
                    return NotFound(new { success = false, message = $"Product with ID {id} was not found." });
                return Ok(new { success = true, data = updated });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        // DELETE /api/products/{id} — Admin role required
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                bool deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = $"Product with ID {id} was not found." });
                return Ok(new { success = true, message = $"Product with ID {id} was deleted successfully." });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Repositries;
using System.Security.Claims;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _repo;

        public ProductsController(ProductRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.GetAll());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _repo.Get(id);
            return product == null ? NotFound() : Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(Product p)
        {
            // Extract user id from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

            if (userIdClaim == null)
                return Unauthorized("User ID missing in token");

            p.CreatedByUserId = int.Parse(userIdClaim);

            await _repo.Add(p);
            return Ok(new
            {
                Message = "Product Added Successfully",
                CreatedBy = p.CreatedByUserId
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(Product p)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("UserId")?.Value;

            if (userIdClaim == null)
                return Unauthorized("User ID missing in token");

            p.CreatedByUserId = int.Parse(userIdClaim);

            await _repo.Update(p);
            return Ok("Product Updated");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _repo.Get(id);
            if (p == null) return NotFound();
            await _repo.Delete(p);
            return Ok("Product Deleted");
        }
    }
}

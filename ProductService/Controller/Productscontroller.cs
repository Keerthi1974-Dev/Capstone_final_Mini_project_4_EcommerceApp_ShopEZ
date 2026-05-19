using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly CloudinaryService _cloudinaryService;

        public ProductsController(IProductService service, CloudinaryService cloudinaryService)
        {
            _service = service;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid product id");
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found");
            return Ok(product);
        }

        // POST with JSON body (no image)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ProductDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _service.AddAsync(dto);
                return Ok("Product created successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST with image upload (form-data)
        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWithImage([FromForm] ProductDTO dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (imageFile != null)
                    dto.ImageUrl = await _cloudinaryService.UploadImageAsync(imageFile);
                await _service.AddAsync(dto);
                return Ok("Product created successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDTO dto)
        {
            if (id <= 0) return BadRequest("Invalid product id");
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (!result) return NotFound("Product not found");
                return Ok("Updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT with image upload (form-data)
        [HttpPut("upload/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateWithImage(int id, [FromForm] ProductDTO dto, IFormFile? imageFile)
        {
            if (id <= 0) return BadRequest("Invalid product id");
            try
            {
                if (imageFile != null)
                    dto.ImageUrl = await _cloudinaryService.UploadImageAsync(imageFile);
                var result = await _service.UpdateAsync(id, dto);
                if (!result) return NotFound("Product not found");
                return Ok("Updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid product id");
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound("Product not found");
                return Ok("Deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
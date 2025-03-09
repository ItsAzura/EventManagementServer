using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        //Phương thức GetCategories trả về danh sách các Category theo trang và kích thước trang
        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int page = 1, int pageSize = 10, string? search = null)
        {
            var categories = await _categoryRepository.GetCategoriesAsync(page, pageSize, search);

            var totalCount = categories.Count();
            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Categories = categories
            };

            return Ok(response);
        }

        //Phương thức GetTopCategories trả về 4 Category mới nhất
        [HttpGet("top")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Category>>> GetTopCategories()
        {
            return Ok(await _categoryRepository.GetTopCategoriesAsync());
        }

        //Phương thức GetCategoryById trả về Category theo ID
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        //Phương thức CreateCategory tạo mới một Category
        [Authorize(Roles = "1")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newCategory = new Category
            {
                CategoryName = categoryDto.CategoryName,
                CategoryDescription = categoryDto.CategoryDescription,
            };

            var createdCategory = await _categoryRepository.CreateCategoryAsync(newCategory);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.CategoryID }, createdCategory);
        }

        //Phương thức UpdateCategory cập nhật thông tin Category
        [Authorize(Roles = "1")]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = new Category
            {
                CategoryName = categoryDto.CategoryName,
                CategoryDescription = categoryDto.CategoryDescription
            };

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(id, category);
            return updatedCategory == null ? NotFound() : Ok(updatedCategory);
        }

        //Phương thức DeleteCategory xóa Category
        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var deletedCategory = await _categoryRepository.DeleteCategoryAsync(id);
            return deletedCategory == null ? NotFound() : Ok(deletedCategory);
        }
    }
}

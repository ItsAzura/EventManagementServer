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
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet("all")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var results = await _categoryRepository.GetAllCategoryAsync();

            _logger.LogInformation($"Get all categories: {results}");

            return Ok(results);
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

            _logger.LogInformation($"Get categories: {response}");

            return Ok(response);
        }

        //Phương thức GetTopCategories trả về 4 Category mới nhất
        [HttpGet("top")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Category>>> GetTopCategories()
        {
            var respone = await _categoryRepository.GetTopCategoriesAsync();

            _logger.LogInformation($"Get top categories: {respone}");

            return Ok(respone);
        }

        //Phương thức GetCategoryById trả về Category theo ID
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            _logger.LogInformation($"Get category by id: {category}");

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

            _logger.LogInformation($"Create category: {createdCategory}");

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

            _logger.LogInformation($"Update category: {updatedCategory}");

            return updatedCategory == null ? NotFound() : Ok(updatedCategory);
        }

        //Phương thức DeleteCategory xóa Category
        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var deletedCategory = await _categoryRepository.DeleteCategoryAsync(id);

            _logger.LogInformation($"Delete category: {deletedCategory}");

            return deletedCategory == null ? NotFound() : Ok(deletedCategory);
        }
    }
}

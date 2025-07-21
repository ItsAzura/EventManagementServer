using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Category")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var results = await _categoryRepository.GetAllCategoryAsync();

            if (results == null) return NotFound();

            var output = results.Select(c => new CategoryResponseDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription
            });

            _logger.LogInformation($"Get all categories: {output}");

            return Ok(output);
        }

        //Phương thức GetCategories trả về danh sách các Category theo trang và kích thước trang
        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int page = 1, int pageSize = 10, string? search = null)
        {
            var categories = await _categoryRepository.GetCategoriesAsync(page, pageSize, search);

            if (categories == null) return NotFound();

            var totalCount = categories.Count();
            var categoriesList = categories.Select(c => new CategoryResponseDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription
            }).ToList();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Categories = categoriesList
            };

            _logger.LogInformation($"Get categories: {response}");

            return Ok(response);
        }

        //Phương thức GetTopCategories trả về 4 Category mới nhất
        [HttpGet("top")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Category>>> GetTopCategories()
        {
            var respone = await _categoryRepository.GetTopCategoriesAsync();

            if (respone == null) return NotFound();

            var categoriesList = respone.Select(c => new CategoryResponseDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription
            }).ToList();

            _logger.LogInformation($"Get top categories: {categoriesList}");

            return Ok(categoriesList);
        }

        //Phương thức GetCategoryById trả về Category theo ID
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null) return NotFound();

            _logger.LogInformation($"Get category by id: {category}");

            return category == null ? NotFound() : Ok(category);
        }

        //Phương thức CreateCategory tạo mới một Category
        [Authorize(Roles = "1")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var deletedCategory = await _categoryRepository.DeleteCategoryAsync(id);

            _logger.LogInformation($"Delete category: {deletedCategory}");

            return deletedCategory == null ? NotFound() : Ok(deletedCategory);
        }
    }
}

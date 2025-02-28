using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly EventDbContext _context;

        public CategoryController(EventDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int page = 1, int pageSize = 10, string? search = null)
        {
            if(page < 1 || pageSize < 1) return BadRequest("Invalid page or pageSize");

            var query = _context.Categories.AsQueryable();

            if(!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.CategoryName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.CategoryID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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

        [Authorize(Roles = "1")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            return Ok(category);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDto category)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Category newCategory = new Category
            {
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.CategoryID }, newCategory);
        }

        [Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, [FromBody] CategoryDto category)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (existingCategory == null) return NotFound();

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;

            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }
        
    }
}

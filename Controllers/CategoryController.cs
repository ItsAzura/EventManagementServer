using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Category newCategory = new Category
            {
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.CategoryID }, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, [FromBody] CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;

            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }
        
    }
}

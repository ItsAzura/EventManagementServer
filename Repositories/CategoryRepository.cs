using EventManagementServer.Data;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly EventDbContext _context;

        public CategoryRepository(EventDbContext context)
        {
            _context = context;
        }

        //Phương thức GetCategoriesAsync trả về danh sách các Category theo trang và kích thước trang
        public async Task<IEnumerable<Category>> GetCategoriesAsync(int page, int pageSize, string? search)
        {
            if (page < 1 || pageSize < 1) return Enumerable.Empty<Category>();

            //Lấy danh sách Category từ database
            var query = _context.Categories.AsQueryable();

            //Nếu có từ khóa tìm kiếm thì lọc theo từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.CategoryName.Contains(search));
            }

            return await query
                .OrderBy(c => c.CategoryID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        //Phương thức GetTopCategoriesAsync trả về 4 Category mới nhất
        public async Task<IEnumerable<Category>> GetTopCategoriesAsync()
        {
            //Lấy 4 Category mới nhất từ database
            return await _context.Categories
                .OrderByDescending(c => c.CategoryID)
                .Take(4)
                .ToListAsync();
        }

        //Phương thức GetCategoryByIdAsync trả về Category theo ID
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
        }

        //Phương thức CreateCategoryAsync tạo mới một Category
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        //Phương thức UpdateCategoryAsync cập nhật thông tin Category
        public async Task<Category?> UpdateCategoryAsync(int id, Category category)
        {
            //Kiểm tra xem Category có tồn tại không
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
            if (existingCategory == null) return null;

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;

            await _context.SaveChangesAsync();
            return existingCategory;
        }

        //Phương thức DeleteCategoryAsync xóa Category
        public async Task<Category?> DeleteCategoryAsync(int id)
        {
            //Kiểm tra xem Category có tồn tại không
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) return null;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}

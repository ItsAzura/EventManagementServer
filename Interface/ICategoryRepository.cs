using EventManagementServer.Models;

namespace EventManagementServer.Interface
{
    //Xác định các phương thức cần thiết cho CategoryRepository
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetCategoriesAsync(int page, int pageSize, string? search);
        Task<IEnumerable<Category>> GetTopCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(int id, Category category);
        Task<Category?> DeleteCategoryAsync(int id);
    }
}

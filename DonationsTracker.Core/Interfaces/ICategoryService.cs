using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetUserCategoriesAsync();
        Task<Category?> GetCategoryAsync(string id);
        Task<Category> CreateUpdateCategoryAsync(CategoryRequest category);
        Task<bool> DeleteCategoryAsync(string id);
    }
}

using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserContextService _userContext;

        public CategoryService(ICategoryRepository categoryRepository, IUserContextService userContext)
        {
            _categoryRepository = categoryRepository;
            _userContext = userContext;
        }

        public Task<IEnumerable<Category>> GetUserCategoriesAsync()
        {
            var userId = _userContext.GetUserId();
            return _categoryRepository.GetCategoriesByUserAsync(userId);
        }

        public Task<Category?> GetCategoryAsync(string id)
        {
            return _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task<Category> CreateUpdateCategoryAsync(Category category)
        {
            var userId = _userContext.GetUserId();
            category.UserId = userId;

            if (!string.IsNullOrEmpty(category.Id))
            {
                // Update existing
                var existing = await _categoryRepository.GetCategoryByIdAsync(category.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Category not found.");

                existing.Name = category.Name;
                existing.Icon = category.Icon;
                existing.Color = category.Color;
                existing.Type = category.Type;
                existing.UpdatedAt = DateTime.UtcNow;

                return await _categoryRepository.UpdateCategoryAsync(existing);
            }

            // Create new
            category.Id = Guid.NewGuid().ToString();
            category.CreatedAt = DateTime.UtcNow;
            return await _categoryRepository.CreateCategoryAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            return await _categoryRepository.DeleteCategoryAsync(id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Categorizer.Models;
using Jellyfin.Categorizer.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Categorizer.Api
{
    /// <summary>
    /// Jellyfin API controller for Netflix-style categories.
    /// Exposes endpoints for browsing dynamic categories.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Administrator")]
    [Route("Categorizer")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserDataManager _userDataManager;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<CategoriesController> _logger;
        private readonly CategoryService _categoryService;

        public CategoriesController(
            ILibraryManager libraryManager,
            IUserDataManager userDataManager,
            IConfigurationService configurationService,
            ILogger<CategoriesController> logger)
        {
            _libraryManager = libraryManager;
            _userDataManager = userDataManager;
            _configurationService = configurationService;
            _logger = logger;
            _categoryService = new CategoryService(libraryManager, userDataManager, configurationService);
        }

        /// <summary>
        /// Get all Netflix-style categories.
        /// </summary>
        /// <param name="libraryId">Optional library filter.</param>
        /// <returns>List of categories with metadata.</returns>
        [HttpGet("Categories")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] string? libraryId = null)
        {
            var categories = await _categoryService.BuildCategoriesAsync(libraryId);

            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString(),
                SortOrder = c.SortOrder,
                IsDynamic = c.IsDynamic,
                TotalItemCount = c.TotalItemCount,
                ItemCount = c.Items.Count
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Get items in a specific category.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="startIndex">Number of items to skip.</param>
        /// <param name="limit">Maximum items to return.</param>
        /// <param name="libraryId">Optional library filter.</param>
        [HttpGet("Categories/{categoryId}/Items")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<QueryResult<BaseItem>>> GetCategoryItems(
            string categoryId,
            [FromQuery] int startIndex = 0,
            [FromQuery] int limit = 20,
            [FromQuery] string? libraryId = null)
        {
            var result = await _categoryService.GetCategoryItemsAsync(categoryId, startIndex, limit, libraryId);
            return Ok(result);
        }

        /// <summary>
        /// Get all available category definitions.
        /// </summary>
        [HttpGet("CategoryDefinitions")]
        [Authorize]
        [Produces("application/json")]
        public ActionResult<Dictionary<string, CategoryDefinitionDto>> GetCategoryDefinitions()
        {
            var definitions = CategoryService.GetCategoryDefinitions();

            var dtos = definitions.ToDictionary(
                kvp => kvp.Key,
                kvp => new CategoryDefinitionDto
                {
                    Id = kvp.Value.Id,
                    Name = kvp.Value.Name,
                    Description = kvp.Value.Description,
                    Type = kvp.Value.Type.ToString(),
                    SortOrder = kvp.Value.SortOrder,
                    IsDynamic = kvp.Value.IsDynamic
                });

            return Ok(dtos);
        }

        /// <summary>
        /// Get a specific category by ID.
        /// </summary>
        /// <param name="categoryId">The category ID to retrieve.</param>
        /// <param name="libraryId">Optional library filter.</param>
        [HttpGet("Categories/{categoryId}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<CategoryDto>> GetCategory(
            string categoryId,
            [FromQuery] string? libraryId = null)
        {
            var result = await _categoryService.GetCategoryItemsAsync(categoryId, 0, int.MaxValue, libraryId);

            if (result.Items == null || !result.Items.Any())
            {
                return NotFound($"Category '{categoryId}' not found.");
            }

            // Parse category ID into display name
            var displayName = categoryId
                .Replace("-", " ")
                .Replace("tv-genre-", "")
                .Replace("beloved-", "")
                .Replace("genre-", "")
                .Split(' ')
                .Select(word => char.ToUpper(word[0]) + word[1..])
                .Aggregate((a, b) => a + " " + b);

            var category = new CategoryDto
            {
                Id = categoryId,
                Name = displayName,
                Description = $"{result.Items.Count} items in this category",
                Type = CategoryType.Metadata.ToString(),
                SortOrder = 0,
                IsDynamic = true,
                TotalItemCount = result.TotalItemCount,
                ItemCount = result.Items.Count
            };

            return Ok(category);
        }
    }

    /// <summary>
    /// DTO for category data returned to the Jellyfin frontend.
    /// </summary>
    public class CategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDynamic { get; set; }
        public int TotalItemCount { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// DTO for category definition data.
    /// </summary>
    public class CategoryDefinitionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDynamic { get; set; }
    }
}

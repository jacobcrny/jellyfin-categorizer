using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Categorizer.Models;
using Jellyfin.Categorizer.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jellyfin.Categorizer.Plugins
{
    /// <summary>
    /// Main plugin entry point for Jellyfin Categorizer.
    /// Provides Netflix-style dynamic category browsing based on library metadata.
    /// </summary>
    public class CategorizerPlugin : IServerEntryPoint
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserDataManager _userDataManager;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<CategorizerPlugin> _logger;
        private readonly CategoryService _categoryService;

        public CategorizerPlugin(
            ILibraryManager libraryManager,
            IUserDataManager userDataManager,
            IConfigurationService configurationService)
        {
            _libraryManager = libraryManager;
            _userDataManager = userDataManager;
            _configurationService = configurationService;
            _logger = NullLogger<CategorizerPlugin>.Instance;
            _categoryService = new CategoryService(libraryManager, userDataManager, configurationService);
        }

        /// <summary>
        /// Get all Netflix-style categories for a given library or the entire library.
        /// </summary>
        public async Task<List<Category>> GetCategoriesAsync(string? libraryId = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Requesting categories. Library: {LibraryId}", libraryId ?? "all");

            var categories = await _categoryService.BuildCategoriesAsync(libraryId, cancellationToken);
            
            _logger.LogInformation("Generated {Count} categories", categories.Count);
            
            return categories;
        }

        /// <summary>
        /// Get items within a specific category.
        /// </summary>
        public async Task<QueryResult<BaseItem>> GetCategoryItemsAsync(
            string categoryId,
            int startIndex = 0,
            int limit = 20,
            string? libraryId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Requesting items for category: {CategoryId}", categoryId);

            var result = await _categoryService.GetCategoryItemsAsync(
                categoryId, startIndex, limit, libraryId, cancellationToken);

            return result;
        }

        /// <summary>
        /// Get all available category definitions and their rules.
        /// </summary>
        public Dictionary<string, CategoryDefinition> GetCategoryDefinitions()
        {
            return CategoryService.GetCategoryDefinitions();
        }

        /// <summary>
        /// This is called when Jellyfin starts up.
        /// </summary>
        public Task OnStartedAsync()
        {
            _logger.LogInformation("Jellyfin Categorizer plugin started");
            return Task.CompletedTask;
        }

        /// <summary>
        /// This is called when Jellyfin shuts down.
        /// </summary>
        public Task OnStoppingAsync()
        {
            _logger.LogInformation("Jellyfin Categorizer plugin stopping");
            return Task.CompletedTask;
        }
    }
}

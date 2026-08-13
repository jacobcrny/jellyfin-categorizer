using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Categorizer.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jellyfin.Categorizer.Plugins
{
    /// <summary>
    /// Configuration plugin for Jellyfin Categorizer.
    /// Handles plugin metadata and configuration persistence.
    /// </summary>
    public class CategorizerConfigurationPlugin : BasePlugin<PluginConfiguration>, IServerPlugin
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<CategorizerConfigurationPlugin> _logger;

        /// <summary>
        /// Unique GUID for this plugin.
        /// </summary>
        public override Guid Id => new Guid("a8f5c3e2-9b1d-4e7f-a6c8-3d2e1f0b9a8c");

        /// <summary>
        /// Plugin display name.
        /// </summary>
        public override string Name => "Jellyfin Categorizer";

        /// <summary>
        /// Short description of the plugin.
        /// </summary>
        public override string Description => "Netflix-style category sorting for Jellyfin libraries using metadata";

        public CategorizerConfigurationPlugin(IConfigurationService configurationService, ILogger<CategorizerConfigurationPlugin> logger)
        {
            _configurationService = configurationService;
            _logger = logger ?? NullLogger<CategorizerConfigurationPlugin>.Instance;
        }

        /// <summary>
        /// Called when the plugin is first loaded.
        /// </summary>
        public override void Start()
        {
            _logger.LogInformation("Jellyfin Categorizer configuration plugin started");
        }

        /// <summary>
        /// Called when the plugin is unloaded.
        /// </summary>
        public override void Stop()
        {
            _logger.LogInformation("Jellyfin Categorizer configuration plugin stopped");
        }
    }

    /// <summary>
    /// Plugin configuration stored by Jellyfin.
    /// </summary>
    public class PluginConfiguration
    {
        /// <summary>
        /// Number of items to show per category row.
        /// </summary>
        public int MaxItemsPerCategory { get; set; } = 30;

        /// <summary>
        /// Minimum community rating for "Top Picks" category.
        /// </summary>
        public decimal MinRatingForTopPicks { get; set; } = 7.0m;

        /// <summary>
        /// Minimum community rating for "Award Winning" category.
        /// </summary>
        public decimal MinRatingForAwardWinning { get; set; } = 8.0m;

        /// <summary>
        /// Minimum critic rating percentage for "Critically Acclaimed" category.
        /// </summary>
        public int MinCriticRating { get; set; } = 80;

        /// <summary>
        /// Number of days to look back for "New Releases".
        /// </summary>
        public int NewReleasesDays { get; set; } = 30;

        /// <summary>
        /// Minimum play count for "Trending" category.
        /// </summary>
        public int MinPlayCount { get; set; } = 1;

        /// <summary>
        /// Whether to include genre categories.
        /// </summary>
        public bool IncludeGenreCategories { get; set; } = true;

        /// <summary>
        /// Minimum number of items for a genre to be shown.
        /// </summary>
        public int MinGenreItemCount { get; set; } = 3;

        /// <summary>
        /// Maximum number of genre categories to show.
        /// </summary>
        public int MaxGenreCategories { get; set; } = 20;

        /// <summary>
        /// Whether to include TV-specific categories.
        /// </summary>
        public bool IncludeTvCategories { get; set; } = true;

        /// <summary>
        /// Whether to include behavioral categories (Continue Watching, Watch Again, etc.).
        /// </summary>
        public bool IncludeBehavioralCategories { get; set; } = true;

        /// <summary>
        /// Whether to include metadata-based categories (Genre, Rating, etc.).
        /// </summary>
        public bool IncludeMetadataCategories { get; set; } = true;

        /// <summary>
        /// Shuffle top picks for variety.
        /// </summary>
        public bool ShuffleTopPicks { get; set; } = true;

        /// <summary>
        /// Custom categories defined by the user.
        /// </summary>
        public List<CustomCategory> CustomCategories { get; set; } = new();
    }

    /// <summary>
    /// A user-defined custom category.
    /// </summary>
    public class CustomCategory
    {
        /// <summary>Unique ID.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Display name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Description shown to users.</summary>
        public string? Description { get; set; }

        /// <summary>Sorting rules for this category.</summary>
        public List<CategoryRule> Rules { get; set; } = new();

        /// <summary>Maximum items to include.</summary>
        public int MaxItems { get; set; } = 50;

        /// <summary>Sort order in the category list.</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>Whether this category is enabled.</summary>
        public bool Enabled { get; set; } = true;
    }
}

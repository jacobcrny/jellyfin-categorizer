using Jellyfin.Categorizer.Data;
using Jellyfin.Categorizer.Models;
using Jellyfin.Categorizer.Services;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Enums;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Categorizer;

/// <summary>
/// The root plugin.
/// </summary>
public class Plugin
    : BasePlugin<PluginConfiguration>
    , IHasWebPages
{
    private readonly ILogger<Plugin> _logger;
    private readonly IDtoService _dtoService;
    private readonly ILibraryManager _libraryManager;

    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        IDtoService dtoService,
        ILibraryManager libraryManager,
        ILogger<Plugin> logger)
        : base(applicationPaths, xmlSerializer)
    {
        _logger = logger;
        _dtoService = dtoService;
        _libraryManager = libraryManager;

        Instance = this;

        ArgumentNullException.ThrowIfNull(applicationPaths);

        var pluginDirectory = Path.Join(applicationPaths.DataPath, "categorizer");
        Directory.CreateDirectory(pluginDirectory);
        ConfigurationDirectory = pluginDirectory;

        // Initialize default categories if not set
        if (Configuration.Categories == null || Configuration.Categories.Count == 0)
        {
            InitializeDefaultCategories();
            SaveConfiguration();
        }
    }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets the directory where plugin configuration is stored.
    /// </summary>
    public string ConfigurationDirectory { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the DTO service for converting items to DTOs.
    /// </summary>
    public IDtoService DtoService => _dtoService;

    /// <summary>
    /// Gets the library manager.
    /// </summary>
    public ILibraryManager LibraryManager => _libraryManager;

    /// <inheritdoc />
    public override string Name => PluginManifest.Categorizer.Name;

    /// <inheritdoc />
    public override Guid Id => PluginManifest.Categorizer.Id;

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
            }
        };
    }

    private void InitializeDefaultCategories()
    {
        Configuration.Categories = new List<CategoryDefinition>
        {
            new() { Type = CategoryType.ContinueWatching, DisplayName = "Continue Watching", Enabled = true, MaxItems = 20 },
            new() { Type = CategoryType.TopPicks, DisplayName = "Top Picks", Enabled = true, MaxItems = 20 },
            new() { Type = CategoryType.Trending, DisplayName = "Trending Now", Enabled = true, MaxItems = 20 },
            new() { Type = CategoryType.NewReleases, DisplayName = "New Releases", Enabled = true, MaxItems = 20 },
            new() { Type = CategoryType.AwardWinning, DisplayName = "Award-Winning", Enabled = false, MaxItems = 15 },
            new() { Type = CategoryType.CriticallyAcclaimed, DisplayName = "Critically Acclaimed", Enabled = false, MaxItems = 15 },
            new() { Type = CategoryType.WatchItAgain, DisplayName = "Watch It Again", Enabled = false, MaxItems = 20 },
            new() { Type = CategoryType.CompletedSeries, DisplayName = "Completed Series", Enabled = false, MaxItems = 15 },
            new() { Type = CategoryType.BelovedGenre, DisplayName = "Beloved Action", GenreFilter = "Action", Enabled = false, MaxItems = 15 }
        };
    }
}

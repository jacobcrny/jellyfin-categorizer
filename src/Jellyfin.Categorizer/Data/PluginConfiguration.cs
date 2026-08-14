using System.Collections.Generic;
using Jellyfin.Categorizer.Models;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Categorizer.Data;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the name of this plugin.
    /// </summary>
    public string Name => "Jellyfin Categorizer";

    /// <summary>
    /// Gets or sets the categories configuration.
    /// </summary>
    public List<CategoryDefinition> Categories { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default maximum items per row.
    /// </summary>
    public int DefaultMaxItems { get; set; } = 18;
}

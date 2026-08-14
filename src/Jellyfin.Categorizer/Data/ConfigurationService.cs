using System.IO;
using System.Text.Json;
using Jellyfin.Categorizer.Data;
using Jellyfin.Categorizer.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Categorizer.Data;

/// <summary>
/// Persists plugin configuration to JSON.
/// </summary>
public class ConfigurationService
{
    private const string FileName = "config.json";
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
    }

    public PluginConfiguration Load(string pluginDirectory)
    {
        var filePath = Path.Join(pluginDirectory, FileName);
        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<PluginConfiguration>(json);
                return config ?? new PluginConfiguration();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading configuration from {FilePath}. Using defaults.", filePath);
        }

        return new PluginConfiguration();
    }

    public void Save(string pluginDirectory, PluginConfiguration config)
    {
        try
        {
            var directory = Path.GetDirectoryName(pluginDirectory);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Join(pluginDirectory, FileName);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to {Directory}", pluginDirectory);
        }
    }
}

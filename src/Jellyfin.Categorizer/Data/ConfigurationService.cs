using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Categorizer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jellyfin.Categorizer.Data
{
    /// <summary>
    /// Configuration service for plugin settings persistence.
    /// </summary>
    public interface IConfigurationService
    {
        Task<PluginConfiguration> GetConfigurationAsync();
        Task SetConfigurationAsync(PluginConfiguration config);
    }

    /// <summary>
    /// File-based configuration service.
    /// </summary>
    public class FileConfigurationService : IConfigurationService
    {
        private readonly string _configPath;
        private readonly ILogger<FileConfigurationService> _logger;
        private PluginConfiguration? _cachedConfig;

        public FileConfigurationService(ILogger<FileConfigurationService>? logger = null)
        {
            _logger = logger ?? NullLogger<FileConfigurationService>.Instance;
            
            // Default config path in Jellyfin's data directory
            var dataDir = Environment.GetEnvironmentVariable("JELLYFIN_DATA_DIR") 
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jellyfin", "Configuration");
            
            _configPath = Path.Combine(dataDir, "jellyfin-categorizer-config.json");
        }

        public async Task<PluginConfiguration> GetConfigurationAsync()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            try
            {
                if (File.Exists(_configPath))
                {
                    var json = await File.ReadAllTextAsync(_configPath);
                    _cachedConfig = System.Text.Json.JsonSerializer.Deserialize<PluginConfiguration>(json) 
                        ?? new PluginConfiguration();
                    return _cachedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading configuration from {Path}", _configPath);
            }

            // Return defaults
            return new PluginConfiguration();
        }

        public async Task SetConfigurationAsync(PluginConfiguration config)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(_configPath, json);
                _cachedConfig = config;
                _logger.LogInformation("Configuration saved to {Path}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing configuration to {Path}", _configPath);
                throw;
            }
        }
    }
}

using MediaBrowser.Model.Plugins;

namespace Jellyfin.Categorizer
{
    /// <summary>
    /// Jellyfin Categorizer plugin manifest.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasPluginInfo
    {
        public override Guid Id => Guid.Parse("a8f5c3e2-9b1d-4e7f-a6c8-3d2e1f0b9a8c");
        public override string Name => "Jellyfin Categorizer";
        public override string Description => "Netflix-style category sorting for Jellyfin libraries using metadata";
        public string AssemblyVersion => "0.1.0";
        public string Identifier => "Jellyfin.Categorizer";
    }
}

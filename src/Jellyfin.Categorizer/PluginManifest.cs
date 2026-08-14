using System;
using System.Collections.Generic;

namespace Jellyfin.Categorizer;

/// <summary>
/// Centralized plugin manifest.
/// Copy this file to a new plugin and update the values.
/// The Plugin class reads from this manifest to avoid duplication.
/// </summary>
public static class PluginManifest
{
    public static readonly PluginInfo Categorizer = new(
        Name: "Jellyfin Categorizer",
        Id: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
        Version: new Version(1, 0, 0),
        Description: "Netflix-style dynamic category sorting for Jellyfin libraries.",
        Author: "jacobcrny",
        TargetJellyfinVersion: "10.11.0"
    );

    // Add new plugins here:
    // public static readonly PluginInfo MyNewPlugin = new(
    //     Name: "My New Plugin",
    //     Id: new Guid("00000000-0000-0000-0000-000000000000"),
    //     Version: new Version(1, 0, 0),
    //     Description: "Description here.",
    //     Author: "jacobcrny",
    //     TargetJellyfinVersion: "10.11.0"
    // );
}

/// <summary>
/// Immutable plugin metadata record.
/// </summary>
public readonly record struct PluginInfo(
    string Name,
    Guid Id,
    Version Version,
    string Description,
    string Author,
    string TargetJellyfinVersion
);

# Jellyfin Categorizer

Netflix-style dynamic category sorting for Jellyfin libraries.

Automatically groups your movies and TV shows into browsable rows like "Continue Watching", "Top Picks", "Trending", "Award-Winning", and more — using the metadata already in your library.

## Features

- **9 dynamic category types** — generated from library metadata, not hard-coded
  - Continue Watching (active series with episodes in progress)
  - Top Picks (high user rating or high IMDb/TMDB score)
  - Trending (recently added or recently played)
  - New Releases (released in the last 90 days)
  - Award-Winning (contains Oscar/Golden Globe/etc. tags)
  - Critically Acclaimed (high review scores)
  - Watch It Again (high-rated movies you've already seen)
  - Completed Series (series with all episodes watched)
  - Beloved [Genre] (genre-based rows, e.g. "Beloved Sci-Fi")
- **REST API** — query categories and items via HTTP
- **Web config UI** — adjust thresholds, toggle category types, customize rules
- **No external dependencies** — works entirely with your existing Jellyfin metadata

## Installation

1. Build the plugin:
   ```bash
   cd src/Jellyfin.Categorizer
   dotnet build -c Release
   ```
2. Copy the generated `.dll` into your Jellyfin plugins directory:
   ```bash
   cp bin/Release/net8.0/Jellyfin.Categorizer.dll \
      /usr/lib/jellyfin-server/plugins/
   ```
3. Restart Jellyfin. The plugin will load automatically.

## Configuration

After installing, go to **Dashboard → Plugins → Jellyfin Categorizer** to:

- Enable or disable individual category types
- Set rating thresholds for "Top Picks" and "Critically Acclaimed"
- Define the lookback window for "Trending" and "New Releases"
- Customize genre-specific row titles

## API

### List available categories

```
GET /jellyfin/Plugins/Jellyfin.Categorizer/Categorizer/Categories
```

Returns all enabled category types with their display names and descriptions.

### Get items in a category

```
GET /jellyfin/Plugins/Jellyfin.Categorizer/Categorizer/Categories/{id}/Items
```

Returns the media items assigned to the given category.

## Building

```bash
cd src/Jellyfin.Categorizer
dotnet build -c Release
```

The compiled `.dll` is ready to drop into the Jellyfin plugins directory.

## Development

### Project structure

```
src/Jellyfin.Categorizer/
├── Jellyfin.Categorizer.csproj   # .NET 8 project
├── Plugin.cs                      # Plugin manifest
├── Plugins/
│   ├── CategorizerPlugin.cs       # Main entry point
│   └── CategorizerConfigurationPlugin.cs  # Config web UI
├── Models/
│   ├── Category.cs                # Category data model
│   └── CategoryDefinition.cs      # Sorting rules & types
├── Services/
│   └── CategoryService.cs         # Dynamic categorization logic
├── Api/
│   └── CategoriesController.cs    # REST endpoints
└── Data/
    └── ConfigurationService.cs    # JSON config persistence
```

### Adding a new category type

1. Add the new type to `CategoryType` enum in `CategoryDefinition.cs`
2. Add sorting logic to `CategoryService.cs`
3. Update the `GetCategories()` method to register the new type

## License

MIT

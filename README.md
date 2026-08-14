# Jellyfin Categorizer

Netflix-style dynamic category sorting for Jellyfin libraries.

Automatically groups your movies and TV shows into browsable rows like "Continue Watching", "Top Picks", "Trending", "Award-Winning", and more — using the metadata already in your library.

## Features

- **9 dynamic category types** — generated from library metadata, not hard-coded
  - **Continue Watching** — series and episodes you've started but not finished
  - **Top Picks** — highest-rated items in your library
  - **Trending** — items recently added (last 30 days)
  - **New Releases** — items released in the last 90 days
  - **Award-Winning** — items tagged with awards (Oscar, Golden Globe, etc.)
  - **Critically Acclaimed** — high community and critic ratings
  - **Watch It Again** — highly-rated items you've already played
  - **Completed Series** — series marked as "Ended" with good ratings
  - **Beloved [Genre]** — top-rated items in a specific genre
- **REST API** — query categories and items via HTTP
- **Web config UI** — adjust thresholds, toggle category types, customize rules
- **No external dependencies** — works entirely with your existing Jellyfin metadata

## Installation

### Via Jellyfin GitHub Repository (Recommended)

1. Open Jellyfin Dashboard
2. Navigate to **Plugins → Repositories**
3. Add the repository URL: `https://github.com/jacobcrny/jellyfin-categorizer`
4. Find **Jellyfin Categorizer** in the plugins list and install
5. Restart Jellyfin

### Manual Installation

1. Clone or download this repository
2. Build the release DLL:
   ```bash
   cd src/Jellyfin.Categorizer
   dotnet build -c Release
   ```
3. Copy the generated DLL into your Jellyfin plugins directory:
   ```bash
   cp bin/Release/net9.0/Jellyfin.Categorizer.dll \
      /usr/lib/jellyfin-server/plugins/
   ```
4. Restart Jellyfin. The plugin will load automatically.

## Configuration

After installing, go to **Dashboard → Plugins → Jellyfin Categorizer** to:

- Enable or disable individual category types
- Set rating thresholds for "Top Picks", "Critically Acclaimed", and other categories
- Define the lookback window for "Trending" and "New Releases"
- Customize genre-specific row titles
- Configure the maximum number of items per category

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

## Requirements

- **Jellyfin 10.11.x** (targeted and tested)
- **.NET 9.0 SDK** for building from source

## Project Structure

```
src/Jellyfin.Categorizer/
├── Jellyfin.Categorizer.csproj   # .NET 9 project with Jellyfin 10.11.x NuGet refs
├── Plugin.cs                      # Plugin manifest
├── PluginServiceRegistrator.cs    # DI registration
├── Api/
│   └── CategoriesController.cs    # REST endpoints
├── Configuration/
│   └── configPage.html            # Web config UI
├── Data/
│   ├── ConfigurationService.cs    # JSON config persistence
│   └── PluginConfiguration.cs     # Config data model
├── Models/
│   ├── Category.cs                # Category data model
│   └── CategoryDefinition.cs      # Sorting rules & types
└── Services/
    └── CategoryService.cs         # Dynamic categorization logic
```

## Adding a New Category Type

1. Add the new type to `CategoryType` enum in `CategoryDefinition.cs`
2. Add sorting logic as a `GetXxx` method in `CategoryService.cs`
3. Add the switch case in `GetItemsForCategory()`
4. Update `PluginConfiguration.cs` with any new config fields

## License

MIT

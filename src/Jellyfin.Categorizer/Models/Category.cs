using System.Collections.Generic;

namespace Jellyfin.Categorizer.Models;

/// <summary>
/// A single category row returned by the API.
/// </summary>
public class Category
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category type.
    /// </summary>
    public string CategoryType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the items in this category.
    /// </summary>
    public List<CategoryItem> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the item count.
    /// </summary>
    public int ItemCount => Items.Count;
}

/// <summary>
/// A single media item within a category.
/// </summary>
public class CategoryItem
{
    /// <summary>
    /// Gets or sets the item ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item type (Movie, Series, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the premiere date.
    /// </summary>
    public string? PremiereDate { get; set; }

    /// <summary>
    /// Gets or sets the community rating.
    /// </summary>
    public double? Rating { get; set; }

    /// <summary>
    /// Gets or sets the number of likes.
    /// </summary>
    public int LikeCount { get; set; }

    /// <summary>
    /// Gets or sets the play count.
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// Gets or sets the percentage played.
    /// </summary>
    public double? PlayedPercentage { get; set; }

    /// <summary>
    /// Gets or sets the genres.
    /// </summary>
    public List<string> Genres { get; set; } = new();

    /// <summary>
    /// Gets or sets the IMDB provider ID if available.
    /// </summary>
    public string? ImdbId { get; set; }

    /// <summary>
    /// Gets or sets the TMDB provider ID if available.
    /// </summary>
    public string? TmdbId { get; set; }
}

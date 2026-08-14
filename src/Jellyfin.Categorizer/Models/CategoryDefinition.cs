namespace Jellyfin.Categorizer.Models;

/// <summary>
/// Available category types that determine sorting logic.
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// Items the user has started but not finished.
    /// </summary>
    ContinueWatching,

    /// <summary>
    /// Highest rated or most liked items.
    /// </summary>
    TopPicks,

    /// <summary>
    /// Recently active or popular items.
    /// </summary>
    Trending,

    /// <summary>
    /// Items released within the last 90 days.
    /// </summary>
    NewReleases,

    /// <summary>
    /// Items with awards or high critic scores.
    /// </summary>
    AwardWinning,

    /// <summary>
    /// Critically acclaimed content (high IMDB/TMDB ratings).
    /// </summary>
    CriticallyAcclaimed,

    /// <summary>
    /// Highly rated completed items the user has finished.
    /// </summary>
    WatchItAgain,

    /// <summary>
    /// TV series that have ended.
    /// </summary>
    CompletedSeries,

    /// <summary>
    /// Beloved items within a specific genre.
    /// </summary>
    BelovedGenre
}

/// <summary>
/// Configuration for a single category definition.
/// </summary>
public class CategoryDefinition
{
    /// <summary>
    /// Gets or sets the category type.
    /// </summary>
    public CategoryType Type { get; set; }

    /// <summary>
    /// Gets or sets the display name shown in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum number of items required to display the row.
    /// </summary>
    public int MinItems { get; set; } = 9;

    /// <summary>
    /// Gets or sets the maximum number of items in the row.
    /// </summary>
    public int MaxItems { get; set; } = 18;

    /// <summary>
    /// Gets or sets whether this category is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the genre filter for BelovedGenre type.
    /// </summary>
    public string? GenreFilter { get; set; }
}

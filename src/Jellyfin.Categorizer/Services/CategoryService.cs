using Jellyfin.Categorizer.Data;
using Jellyfin.Categorizer.Models;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Categorizer.Services;

/// <summary>
/// Core service that ingests library metadata and sorts items into categories.
/// </summary>
public class CategoryService
{
    private readonly ILibraryManager _libraryManager;
    private readonly IDtoService _dtoService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ILibraryManager libraryManager,
        IDtoService dtoService,
        ILogger<CategoryService> logger)
    {
        _libraryManager = libraryManager;
        _dtoService = dtoService;
        _logger = logger;
    }

    public List<Category> GetCategoriesForUser(Guid userId, PluginConfiguration configuration)
    {
        var categories = new List<Category>();
        var allItems = GetLibraryItems(userId);
        var allDtoItems = GetDtoItems(allItems);

        foreach (var def in configuration.Categories?.Where(d => d.Enabled) ?? Array.Empty<CategoryDefinition>())
        {
            var items = GetItemsForCategory(def.Type, def, allDtoItems);
            if (items.Count > 0)
            {
                categories.Add(new Category
                {
                    Id = def.Type.ToString().ToLowerInvariant(),
                    DisplayName = def.DisplayName,
                    CategoryType = def.Type.ToString(),
                    Items = items.Take(def.MaxItems).ToList()
                });
            }
        }

        return categories;
    }

    private List<CategoryItem> GetItemsForCategory(CategoryType type, CategoryDefinition def, List<BaseItemDto> allItems)
    {
        return type switch
        {
            CategoryType.ContinueWatching => GetContinueWatching(allItems),
            CategoryType.TopPicks => GetTopPicks(allItems),
            CategoryType.Trending => GetTrending(allItems),
            CategoryType.NewReleases => GetNewReleases(allItems),
            CategoryType.AwardWinning => GetAwardWinning(allItems),
            CategoryType.CriticallyAcclaimed => GetCriticallyAcclaimed(allItems),
            CategoryType.WatchItAgain => GetWatchItAgain(allItems),
            CategoryType.CompletedSeries => GetCompletedSeries(allItems),
            CategoryType.BelovedGenre => GetBelovedGenre(allItems, def),
            _ => new List<CategoryItem>()
        };
    }

    private List<CategoryItem> GetContinueWatching(List<BaseItemDto> items)
    {
        var result = items.Where(i => i.UserData != null && !i.UserData.Played && i.UserData.PlayCount == 0 && i.UserData.PlaybackPositionTicks > 0)
            .OrderByDescending(i => i.UserData.PlaybackPositionTicks)
            .Select(ConvertToCategoryItem)
            .ToList();
        return result;
    }

    private List<CategoryItem> GetTopPicks(List<BaseItemDto> items)
    {
        #pragma warning disable CS8629 // Nullable value type may be null
        var result = items.Where(i => i.CommunityRating.HasValue && i.CommunityRating.Value > 7.5f)
            .OrderByDescending(i => i.CommunityRating.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        #pragma warning restore CS8629
        return result;
    }

    private List<CategoryItem> GetTrending(List<BaseItemDto> items)
    {
        var now = DateTime.UtcNow;
        #pragma warning disable CS8629 // Nullable value type may be null
        var result = items.Where(i => i.DateCreated.HasValue && i.DateCreated.Value > now.AddDays(-30))
            .OrderByDescending(i => i.DateCreated.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        #pragma warning restore CS8629
        return result;
    }

    private List<CategoryItem> GetNewReleases(List<BaseItemDto> items)
    {
        var now = DateTime.UtcNow;
        #pragma warning disable CS8629 // Nullable value type may be null
        var result = items.Where(i => i.PremiereDate.HasValue && i.PremiereDate.Value > now.AddDays(-90))
            .OrderByDescending(i => i.PremiereDate.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        #pragma warning restore CS8629
        return result;
    }

    private List<CategoryItem> GetAwardWinning(List<BaseItemDto> items)
    {
        #pragma warning disable CS8629 // Nullable value type may be null
        var result = items.Where(i => i.CommunityRating.HasValue && i.CommunityRating.Value > 8.0f)
            .Where(i => i.Tags != null && i.Tags.Any(t => t.Contains("award", StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(i => i.CommunityRating.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        #pragma warning restore CS8629
        return result;
    }

    private List<CategoryItem> GetCriticallyAcclaimed(List<BaseItemDto> items)
    {
        var result = items.Where(i =>
            (i.CommunityRating.HasValue && i.CommunityRating.Value > 7.5f) ||
            (i.CriticRating.HasValue && i.CriticRating.Value > 80))
            .OrderByDescending(i => i.CommunityRating.GetValueOrDefault() > i.CriticRating.GetValueOrDefault() ? i.CommunityRating!.Value : i.CriticRating!.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        return result;
    }

    private List<CategoryItem> GetWatchItAgain(List<BaseItemDto> items)
    {
        #pragma warning disable CS8629 // Nullable value type may be null
        var result = items.Where(i => i.UserData != null && i.UserData.Played && i.CommunityRating.HasValue && i.CommunityRating.Value > 7.0f)
            .OrderByDescending(i => i.CommunityRating.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        #pragma warning restore CS8629
        return result;
    }

    private List<CategoryItem> GetCompletedSeries(List<BaseItemDto> items)
    {
        var result = items.Where(i => i.Type == BaseItemKind.Series || i.Type == BaseItemKind.Episode)
            .Where(i => i.Status == "Ended")
            .OrderByDescending(i => i.CommunityRating.GetValueOrDefault())
            .Select(ConvertToCategoryItem)
            .ToList();
        return result;
    }

    private List<CategoryItem> GetBelovedGenre(List<BaseItemDto> items, CategoryDefinition def)
    {
        var genre = def.GenreFilter ?? "Action";
        var result = items.Where(i => i.Genres != null && i.Genres.Any(g => g.Contains(genre, StringComparison.OrdinalIgnoreCase)) && i.CommunityRating.HasValue && i.CommunityRating.Value > 6.5f)
            .OrderByDescending(i => i.CommunityRating!.Value)
            .Select(ConvertToCategoryItem)
            .ToList();
        return result;
    }

    private List<BaseItem> GetLibraryItems(Guid userId)
    {
        var query = new InternalItemsQuery(null)
        {
            IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series, BaseItemKind.Episode, BaseItemKind.Video },
            Recursive = true,
            DtoOptions = new DtoOptions(true)
        };

        return _libraryManager.QueryItems(query).Items.ToList();
    }

    private List<BaseItemDto> GetDtoItems(List<BaseItem> items)
    {
        var dtoOptions = new DtoOptions(true);

        var result = new List<BaseItemDto>();
        foreach (var item in items)
        {
            try
            {
                var dto = _dtoService.GetBaseItemDto(item, dtoOptions);
                if (dto != null) result.Add(dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert item {ItemName} to DTO", item.Name);
            }
        }
        return result;
    }

    private CategoryItem ConvertToCategoryItem(BaseItemDto dto)
    {
        string? imdbId = null;
        if (dto.ProviderIds != null)
        {
            foreach (var kv in dto.ProviderIds)
            {
                if (kv.Key.Equals("imdb", StringComparison.OrdinalIgnoreCase))
                {
                    imdbId = kv.Value;
                    break;
                }
            }
        }

        return new CategoryItem
        {
            Id = dto.Id.ToString(),
            Name = dto.Name ?? string.Empty,
            Type = dto.Type.ToString(),
            PremiereDate = dto.PremiereDate?.ToString("yyyy-MM-dd"),
            Rating = dto.CommunityRating,
            PlayCount = dto.UserData?.PlayCount ?? 0,
            PlayedPercentage = dto.UserData?.PlayedPercentage,
            Genres = dto.Genres?.ToList() ?? new List<string>(),
            ImdbId = imdbId
        };
    }
}

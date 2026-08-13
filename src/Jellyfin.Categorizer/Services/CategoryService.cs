using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Categorizer.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jellyfin.Categorizer.Services
{
    /// <summary>
    /// Core service that builds Netflix-style categories from Jellyfin library metadata.
    /// Analyzes genres, ratings, user activity, and other metadata to create dynamic categories.
    /// </summary>
    public class CategoryService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserDataManager _userDataManager;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ILibraryManager libraryManager,
            IUserDataManager userDataManager,
            IConfigurationService configurationService)
        {
            _libraryManager = libraryManager;
            _userDataManager = userDataManager;
            _configurationService = configurationService;
            _logger = NullLogger<CategoryService>.Instance;
        }

        /// <summary>
        /// Build all Netflix-style categories for the given library scope.
        /// </summary>
        public async Task<List<Category>> BuildCategoriesAsync(string? libraryId = null, CancellationToken ct = default)
        {
            var categories = new List<Category>();

            // Core categories - always available
            categories.Add(await BuildContinueWatchingAsync(libraryId, ct));
            categories.Add(await BuildTopPicksAsync(libraryId, ct));
            categories.Add(await BuildTrendingAsync(libraryId, ct));
            categories.Add(await BuildNewReleasesAsync(libraryId, ct));

            // Genre categories
            categories.AddRange(await BuildGenreCategoriesAsync(libraryId, ct));

            // Quality-based categories
            categories.Add(await BuildAwardWinningAsync(libraryId, ct));
            categories.Add(await BuildCriticallyAcclaimedAsync(libraryId, ct));

            // Behavior-based categories
            categories.Add(await BuildWatchAgainAsync(libraryId, ct));

            // TV-specific categories
            categories.Add(await BuildCompletedSeriesAsync(libraryId, ct));
            categories.AddRange(await BuildBelovedGenreCategoriesAsync(libraryId, ct));

            // Filter out empty categories and sort
            categories = categories.Where(c => c.Items.Any()).OrderBy(c => c.SortOrder).ToList();

            return categories;
        }

        /// <summary>
        /// Get items for a specific category by ID with pagination.
        /// </summary>
        public async Task<QueryResult<BaseItem>> GetCategoryItemsAsync(
            string categoryId,
            int startIndex = 0,
            int limit = 20,
            string? libraryId = null,
            CancellationToken ct = default)
        {
            var categories = await BuildCategoriesAsync(libraryId, ct);
            var category = categories.FirstOrDefault(c => c.Id == categoryId);

            if (category == null)
            {
                return new QueryResult<BaseItem>(startIndex, 0, 0, new List<BaseItem>());
            }

            var items = category.Items.Skip(startIndex).Take(limit).ToList();
            return new QueryResult<BaseItem>(startIndex, category.TotalItemCount, category.TotalItemCount, items);
        }

        /// <summary>
        /// Get all available category definitions.
        /// </summary>
        public static Dictionary<string, CategoryDefinition> GetCategoryDefinitions()
        {
            return new Dictionary<string, CategoryDefinition>
            {
                ["continue-watching"] = new CategoryDefinition
                {
                    Id = "continue-watching",
                    Name = "Continue Watching",
                    Description = "Items you started but haven't finished yet",
                    Type = CategoryType.Behavioral,
                    SortOrder = 0
                },
                ["top-picks"] = new CategoryDefinition
                {
                    Id = "top-picks",
                    Name = "Top Picks for You",
                    Description = "Personalized picks based on your viewing history",
                    Type = CategoryType.Behavioral,
                    SortOrder = 1
                },
                ["trending"] = new CategoryDefinition
                {
                    Id = "trending",
                    Name = "Trending Now",
                    Description = "What's popular in your library right now",
                    Type = CategoryType.Behavioral,
                    SortOrder = 2
                },
                ["new-releases"] = new CategoryDefinition
                {
                    Id = "new-releases",
                    Name = "New Releases",
                    Description = "Recently added to your library",
                    Type = CategoryType.Metadata,
                    SortOrder = 3
                },
                ["award-winning"] = new CategoryDefinition
                {
                    Id = "award-winning",
                    Name = "Award-Winning Dramas",
                    Description = "Critically acclaimed movies with high ratings",
                    Type = CategoryType.Metadata,
                    SortOrder = 4
                },
                ["critically-acclaimed"] = new CategoryDefinition
                {
                    Id = "critically-acclaimed",
                    Name = "Critically Acclaimed",
                    Description = "Highly rated movies and shows",
                    Type = CategoryType.Metadata,
                    SortOrder = 5
                },
                ["watch-again"] = new CategoryDefinition
                {
                    Id = "watch-again",
                    Name = "Watch It Again",
                    Description = "Your most-played favorites",
                    Type = CategoryType.Behavioral,
                    SortOrder = 6
                },
                ["completed-series"] = new CategoryDefinition
                {
                    Id = "completed-series",
                    Name = "Completed Series",
                    Description = "Series that have ended",
                    Type = CategoryType.Metadata,
                    SortOrder = 7
                },
                ["beloved-genres"] = new CategoryDefinition
                {
                    Id = "beloved-genres",
                    Name = "Beloved Genres",
                    Description = "Your most-played genres across all content",
                    Type = CategoryType.Behavioral,
                    SortOrder = 8
                }
            };
        }

        // ================================================================
        // Individual category builders
        // ================================================================

        private async Task<Category> BuildContinueWatchingAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "continue-watching",
                Name = "Continue Watching",
                Description = "Items you started but haven't finished",
                Type = CategoryType.Behavioral,
                SortOrder = 0,
                IsDynamic = true
            };

            var items = new List<BaseItem>();

            // Get incomplete movies (never played)
            var incompleteMovies = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                IncludeItemTypes = new[] { typeof(Movie).Name },
                IsPlaceHolder = false
            }, false).OfType<Movie>().Where(m => !m.GetUserData(_userDataManager).Played).ToList();

            // Get incomplete TV episodes (never played)
            var incompleteEpisodes = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                IncludeItemTypes = new[] { "Episode" },
                IsPlaceHolder = false
            }, false).OfType<BaseItem>().Where(e => !e.GetUserData(_userDataManager).Played).ToList();

            items.AddRange(incompleteMovies.Cast<BaseItem>());
            items.AddRange(incompleteEpisodes);

            // Sort by latest playback time
            items.Sort((a, b) =>
            {
                var aData = a.GetUserData(_userDataManager);
                var bData = b.GetUserData(_userDataManager);
                return (aData.LastPlayedTime ?? DateTime.MinValue).CompareTo(bData.LastPlayedTime ?? DateTime.MinValue);
            });

            category.Items = items.Take(50).ToList();
            category.TotalItemCount = items.Count;
            return category;
        }

        private async Task<Category> BuildTopPicksAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "top-picks",
                Name = "Top Picks for You",
                Description = "Personalized picks based on your viewing history",
                Type = CategoryType.Behavioral,
                SortOrder = 1,
                IsDynamic = true
            };

            // Get items the user has rated highly (7/10 or above)
            var items = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                MinCommunityRating = 7.0m
            }, false).ToList();

            // Shuffle for variety
            var shuffled = items.OrderBy(x => Guid.NewGuid()).Take(30).ToList();

            category.Items = shuffled;
            category.TotalItemCount = shuffled.Count;
            return category;
        }

        private async Task<Category> BuildTrendingAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "trending",
                Name = "Trending Now",
                Description = "What's popular in your library right now",
                Type = CategoryType.Behavioral,
                SortOrder = 2,
                IsDynamic = true
            };

            // Get items with highest recent play count
            var items = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                MinPlayCount = 1
            }, false).ToList();

            // Sort by play count descending
            items.Sort((a, b) => b.GetUserData(_userDataManager).PlayCount.CompareTo(a.GetUserData(_userDataManager).PlayCount));

            category.Items = items.Take(30).ToList();
            category.TotalItemCount = items.Count;
            return category;
        }

        private async Task<Category> BuildNewReleasesAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "new-releases",
                Name = "New Releases",
                Description = "Recently added to your library",
                Type = CategoryType.Metadata,
                SortOrder = 3,
                IsDynamic = true
            };

            // Get items added in the last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var items = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                DateCreatedAfter = thirtyDaysAgo
            }, false).ToList();

            // Sort by date added, newest first
            items.Sort((a, b) => b.DateCreated.CompareTo(a.DateCreated));

            category.Items = items.Take(50).ToList();
            category.TotalItemCount = items.Count;
            return category;
        }

        private async Task<Category> BuildAwardWinningAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "award-winning",
                Name = "Award-Winning Dramas",
                Description = "Critically acclaimed movies with high ratings",
                Type = CategoryType.Metadata,
                SortOrder = 4,
                IsDynamic = true
            };

            // Get movies with community rating >= 8.0
            var items = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                IncludeItemTypes = new[] { typeof(Movie).Name },
                MinCommunityRating = 8.0m
            }, false).ToList();

            category.Items = items.Take(50).ToList();
            category.TotalItemCount = items.Count;
            return category;
        }

        private async Task<Category> BuildCriticallyAcclaimedAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "critically-acclaimed",
                Name = "Critically Acclaimed",
                Description = "Highly rated movies and shows",
                Type = CategoryType.Metadata,
                SortOrder = 5,
                IsDynamic = true
            };

            // Get items with high critic and community ratings
            var items = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                MinCriticRating = 80,
                MinCommunityRating = 8.5m
            }, false).ToList();

            category.Items = items.Take(50).ToList();
            category.TotalItemCount = items.Count;
            return category;
        }

        private async Task<Category> BuildWatchAgainAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "watch-again",
                Name = "Watch It Again",
                Description = "Your most-played favorites",
                Type = CategoryType.Behavioral,
                SortOrder = 6,
                IsDynamic = true
            };

            // Get items with highest play count
            var items = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                MinPlayCount = 2
            }, false).ToList();

            items.Sort((a, b) => b.GetUserData(_userDataManager).PlayCount.CompareTo(a.GetUserData(_userDataManager).PlayCount));

            category.Items = items.Take(30).ToList();
            category.TotalItemCount = items.Count;
            return category;
        }

        private async Task<List<Category>> BuildGenreCategoriesAsync(string? libraryId, CancellationToken ct)
        {
            var categories = new List<Category>();

            // Get all genres from movies in the library
            var movieQuery = new InternalItemsQuery(libraryId)
            {
                IncludeItemTypes = new[] { typeof(Movie).Name },
                Recursive = true
            };

            var movies = _libraryManager.GetItemList(movieQuery, false).OfType<Movie>().ToList();

            // Collect all unique genres
            var allGenres = movies
                .SelectMany(m => m.Genres)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            // Skip rare genres (less than 3 items)
            var commonGenres = allGenres
                .Where(g => movies.Count(m => m.Genres.Contains(g)) >= 3)
                .Take(20) // Limit to 20 genre categories
                .ToList();

            foreach (var genre in commonGenres)
            {
                var genreItems = movies
                    .Where(m => m.Genres.Contains(genre))
                    .OrderByDescending(m => m.CommunityRating ?? 0)
                    .Take(50)
                    .Cast<BaseItem>()
                    .ToList();

                if (genreItems.Any())
                {
                    var category = new Category
                    {
                        Id = $"genre-{genre.ToLower().Replace(" ", "-")}",
                        Name = genre,
                        Description = $"{genre} movies in your library",
                        Type = CategoryType.Metadata,
                        SortOrder = 100 + commonGenres.IndexOf(genre),
                        Items = genreItems,
                        TotalItemCount = genreItems.Count,
                        IsDynamic = true
                    };

                    categories.Add(category);
                }
            }

            return categories;
        }

        private async Task<Category> BuildCompletedSeriesAsync(string? libraryId, CancellationToken ct)
        {
            var category = new Category
            {
                Id = "completed-series",
                Name = "Completed Series",
                Description = "Series that have ended",
                Type = CategoryType.Metadata,
                SortOrder = 7,
                IsDynamic = true
            };

            var series = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                IncludeItemTypes = new[] { typeof(Series).Name },
                StatusEquals = SeriesStatusType.Ended
            }, false).OfType<Series>().ToList();

            category.Items = series.Take(50).Cast<BaseItem>().ToList();
            category.TotalItemCount = series.Count;
            return category;
        }

        private async Task<List<Category>> BuildBelovedGenreCategoriesAsync(string? libraryId, CancellationToken ct)
        {
            var categories = new List<Category>();

            // Analyze user behavior to find most-played genres
            var userDataMap = new Dictionary<string, Dictionary<string, int>>();
            var movies = _libraryManager.GetItemList(new InternalItemsQuery(libraryId)
            {
                IncludeItemTypes = new[] { typeof(Movie).Name }
            }, false).OfType<Movie>().ToList();

            foreach (var movie in movies)
            {
                var userData = movie.GetUserData(_userDataManager);
                var playCount = userData.PlayCount;

                if (playCount > 0)
                {
                    foreach (var genre in movie.Genres)
                    {
                        if (!userDataMap.ContainsKey(genre))
                            userDataMap[genre] = new Dictionary<string, int>();

                        if (!userDataMap[genre].ContainsKey(movie.Id))
                            userDataMap[genre][movie.Id] = 0;

                        userDataMap[genre][movie.Id] += playCount;
                    }
                }
            }

            // Get top genres by total plays
            var topGenres = userDataMap
                .Select(g => new { Genre = g.Key, TotalPlays = g.Value.Values.Sum() })
                .OrderByDescending(g => g.TotalPlays)
                .Take(10)
                .ToList();

            foreach (var genreInfo in topGenres)
            {
                var genreMovies = movies
                    .Where(m => m.Genres.Contains(genreInfo.Genre))
                    .OrderByDescending(m => m.GetUserData(_userDataManager).PlayCount)
                    .Take(50)
                    .Cast<BaseItem>()
                    .ToList();

                if (genreMovies.Any())
                {
                    var category = new Category
                    {
                        Id = $"beloved-{genreInfo.Genre.ToLower().Replace(" ", "-")}",
                        Name = $"Beloved {genreInfo.Genre}",
                        Description = $"Your most-played {genreInfo.Genre} content",
                        Type = CategoryType.Behavioral,
                        SortOrder = 300 + topGenres.IndexOf(genreInfo),
                        Items = genreMovies,
                        TotalItemCount = genreMovies.Count,
                        IsDynamic = true
                    };

                    categories.Add(category);
                }
            }

            return categories;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Jellyfin.Categorizer.Models
{
    /// <summary>
    /// Definition of a Netflix-style category with its sorting rules.
    /// </summary>
    public class CategoryDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CategoryType Type { get; set; } = CategoryType.Metadata;
        public int SortOrder { get; set; } = 0;
        public bool IsDynamic { get; set; } = true;
        public CategoryRule? Rule { get; set; }
    }

    /// <summary>
    /// A single rule that determines what items belong to a category.
    /// </summary>
    public class CategoryRule
    {
        /// <summary>Rule type: genre, rating, year, user_score, etc.</summary>
        public RuleType Type { get; set; }

        /// <summary>Value to match (e.g., "Action", "9.0", "2023").</summary>
        public string? Value { get; set; }

        /// <summary>Multiple values allowed (e.g., multiple genres).</summary>
        public List<string>? Values { get; set; }

        /// <summary>Comparator: Equals, Contains, GreaterThan, LessThan, In.</summary>
        public ComparisonType Comparison { get; set; } = ComparisonType.Contains;

        /// <summary>Optional negation: true means items that DON'T match.</summary>
        public bool Negate { get; set; } = false;

        /// <summary>Minimum number of items required in the category.</summary>
        public int MinItemCount { get; set; } = 1;
    }

    public enum RuleType
    {
        Genre,
        Year,
        Rating,
        UserScore,
        PlayCount,
        LastPlayed,
        Runtime,
        ContentRating,
        Status,
        CustomTag,
        All,
        None,
        And,
        Or
    }

    public enum ComparisonType
    {
        Equals,
        Contains,
        GreaterThan,
        LessThan,
        In,
        NotIn,
        Between
    }
}

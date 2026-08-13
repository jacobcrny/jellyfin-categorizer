using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Categorizer.Models
{
    /// <summary>
    /// Represents a Netflix-style category with items.
    /// </summary>
    public class Category
    {
        /// <summary>Unique identifier for this category.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Display name shown to users.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Short description of what this category contains.</summary>
        public string? Description { get; set; }

        /// <summary>Items that belong to this category.</summary>
        public List<BaseItem> Items { get; set; } = new();

        /// <summary>Total count (may be more than Items if pagination is needed).</summary>
        public int TotalItemCount { get; set; }

        /// <summary>Order in which this category appears (lower = first).</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>Category type for grouping.</summary>
        public CategoryType Type { get; set; } = CategoryType.Metadata;

        /// <summary>Whether this category was auto-generated or user-defined.</summary>
        public bool IsDynamic { get; set; } = true;
    }

    /// <summary>Type of category generation.</summary>
    public enum CategoryType
    {
        /// <summary>Generated from metadata (genres, ratings, etc.).</summary>
        Metadata,
        /// <summary>Generated from user behavior (continue watching, top 10, etc.).</summary>
        Behavioral,
        /// <summary>User-defined custom category.</summary>
        UserDefined
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchApi
{
    // Represents the input parameters for the search operation.
    public record SearchRequest(
        string? Query,
        string? Category,
        decimal? MinPrice,
        decimal? MaxPrice,
        string SortBy,
        bool SortDescending,
        int Page,
        int PageSize
    );

    // Represents the paginated result of the search operation.
    public record SearchResult(
        IReadOnlyList<ProductSummary> Items,
        int TotalCount,
        int Page,
        int TotalPages
    );

    // Represents a simplified view of a product for search results.
    public record ProductSummary(
        int Id,
        string Name,
        string Category,
        decimal Price
    );

    /// <summary>
    /// Engine responsible for filtering, sorting, and paginating product data.
    /// </summary>
    public class ProductSearchEngine
    {
        private readonly IReadOnlyList<ProductSummary> _products;

        public ProductSearchEngine(IEnumerable<ProductSummary> products)
        {
            // Materialize the list to ensure we don't enumerate multiple times if the source is a query.
            _products = products?.ToList() ?? throw new ArgumentNullException(nameof(products));
        }

        /// <summary>
        /// Executes a search based on the provided request parameters.
        /// </summary>
        /// <param name="request">The search request parameters.</param>
        /// <returns>A search result containing items and pagination metadata.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when Page or PageSize are invalid.</exception>
        public SearchResult Search(SearchRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Validation: Page must be >= 1
            if (request.Page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Page), "Page must be greater than or equal to 1.");
            }

            // Validation: PageSize must be between 1 and 100
            if (request.PageSize < 1 || request.PageSize > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(request.PageSize), "PageSize must be between 1 and 100.");
            }

            // 1. Filtering
            var query = _products.AsQueryable();

            // Filter by Query (case-insensitive name contains)
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                query = query.Where(p => p.Name.Contains(request.Query, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Category (exact match)
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(p => p.Category == request.Category);
            }

            // Filter by MinPrice
            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            }

            // Filter by MaxPrice
            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice.Value);
            }

            // 2. Sorting
            // Default sort field is "name" if invalid or empty, though we handle specific cases.
            // We use a switch expression to determine the key selector.
            var sortKey = string.IsNullOrWhiteSpace(request.SortBy) ? "name" : request.SortBy.ToLowerInvariant();

            // Apply sorting based on direction
            query = (sortKey, request.SortDescending) switch
            {
                ("name", false) => query.OrderBy(p => p.Name),
                ("name", true) => query.OrderByDescending(p => p.Name),

                ("price", false) => query.OrderBy(p => p.Price),
                ("price", true) => query.OrderByDescending(p => p.Price),

                ("category", false) => query.OrderBy(p => p.Category),
                ("category", true) => query.OrderByDescending(p => p.Category),

                // Default fallback: Name ascending
                _ => query.OrderBy(p => p.Name)
            };

            // 3. Pagination
            var totalCount = query.Count();

            // Calculate total pages (ensure ceiling division)
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            if (totalPages == 0 && totalCount > 0) totalPages = 1; // Should not happen with math, but safe check

            // Apply pagination (1-based page logic)
            // Skip: (Page - 1) * PageSize
            var items = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new SearchResult(items, totalCount, request.Page, totalPages);
        }
    }
}
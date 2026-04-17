using System;
using System.Collections.Generic;
using System.Linq;

public record Review(int Id, string ProductId, string Author, int Rating, string Comment);

public class ReviewAggregator
{
    private readonly List<Review> _reviews = new List<Review>();

    public void AddReview(Review review)
    {
        _reviews.Add(review);
    }

    public double AverageRating(string productId)
    {
        var productReviews = _reviews.Where(r => r.ProductId == productId).ToList();

        if (productReviews.Count == 0)
        {
            return 0;
        }

        return productReviews.Average(r => r.Rating);
    }

    public List<Review> GetTopReviews(string productId, int count)
    {
        return _reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.Rating)
            .Take(count)
            .ToList();
    }
}
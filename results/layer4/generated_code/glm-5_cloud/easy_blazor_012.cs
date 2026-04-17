using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public class RatingComponent : ComponentBase
{
    [Parameter]
    public int MaxStars { get; set; } = 5;

    public int CurrentRating { get; private set; } = 0;

    [Parameter]
    public EventCallback<int> OnRatingChanged { get; set; }

    protected async Task SetRating(int rating)
    {
        // Clamp the rating to be between 1 and MaxStars
        if (rating < 1)
        {
            rating = 1;
        }
        else if (rating > MaxStars)
        {
            rating = MaxStars;
        }

        CurrentRating = rating;

        if (OnRatingChanged.HasDelegate)
        {
            await OnRatingChanged.InvokeAsync(rating);
        }
    }
}
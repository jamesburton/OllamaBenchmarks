using Microsoft.AspNetCore.Components;

public class StarRatingComponent : ComponentBase
{
    [Parameter]
    public int MaxStars { get; set; } = 5;

    protected int SelectedStars { get; set; } = 0;

    protected void Rate(int stars)
    {
        if (stars < 0)
        {
            SelectedStars = 0;
        }
        else if (stars > MaxStars)
        {
            SelectedStars = MaxStars;
        }
        else
        {
            SelectedStars = stars;
        }
    }
}
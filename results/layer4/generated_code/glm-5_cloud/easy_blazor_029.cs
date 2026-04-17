using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

public class ImageCarouselComponent : ComponentBase
{
    [Parameter]
    public List<string> ImageUrls { get; set; } = new();

    protected int CurrentIndex { get; set; } = 0;

    protected string CurrentImage => ImageUrls.Count > 0 ? ImageUrls[CurrentIndex] : "";

    protected void Next()
    {
        if (ImageUrls.Count == 0) return;
        CurrentIndex = (CurrentIndex + 1) % ImageUrls.Count;
    }

    protected void Previous()
    {
        if (ImageUrls.Count == 0) return;
        CurrentIndex = (CurrentIndex - 1 + ImageUrls.Count) % ImageUrls.Count;
    }
}
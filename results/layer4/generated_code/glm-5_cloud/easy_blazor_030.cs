using Microsoft.AspNetCore.Components;

public class WordCountComponent : ComponentBase
{
    [Parameter]
    public string Text { get; set; } = "";

    protected int WordCount => string.IsNullOrWhiteSpace(Text) 
        ? 0 
        : Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    protected int CharCount => Text.Length;
}
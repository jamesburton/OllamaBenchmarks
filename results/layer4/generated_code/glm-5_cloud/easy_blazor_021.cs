using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

public class DropdownComponent : ComponentBase
{
    [Parameter]
    public List<string> Options { get; set; } = new();

    public string SelectedOption { get; set; } = "";

    public bool IsOpen { get; set; } = false;

    public void Open()
    {
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    public void Select(string option)
    {
        SelectedOption = option;
        Close();
    }
}
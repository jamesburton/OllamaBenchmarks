using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace BlazorComponents
{
    public class MultiSelectComponent : ComponentBase
    {
        [Parameter]
        public List<string> Options { get; set; } = new List<string>();

        public List<string> Selected { get; set; } = new List<string>();

        public void Select(string option)
        {
            if (!IsSelected(option))
            {
                Selected.Add(option);
            }
        }

        public void Deselect(string option)
        {
            if (IsSelected(option))
            {
                Selected.Remove(option);
            }
        }

        public bool IsSelected(string option)
        {
            return Selected.Contains(option);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }

    public TodoItem()
    {
        // default values are not used, they will be set by the constructor
    }
}
using System;

public record Interval(DateTime Start, DateTime End)
{
    public bool Overlaps(Interval other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        // Touching endpoints count as overlapping.
        // Overlap occurs if: Start1 <= End2 AND Start2 <= End1
        return Start <= other.End && other.Start <= End;
    }
}
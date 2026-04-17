using System;

public record Coordinate(int Row, int Column);

public static class GridHelper
{
    public static bool IsCorner(Coordinate coord, int gridSize)
    {
        if (gridSize <= 0)
        {
            return false;
        }

        int maxIndex = gridSize - 1;

        return (coord.Row == 0 && coord.Column == 0) ||
               (coord.Row == 0 && coord.Column == maxIndex) ||
               (coord.Row == maxIndex && coord.Column == 0) ||
               (coord.Row == maxIndex && coord.Column == maxIndex);
    }
}
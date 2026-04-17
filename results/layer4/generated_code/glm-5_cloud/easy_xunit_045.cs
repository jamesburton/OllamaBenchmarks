using System;

public static class MatrixUtils
{
    public static int[,] Transpose(int[,] matrix)
    {
        if (matrix == null)
        {
            throw new ArgumentNullException(nameof(matrix));
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int[,] result = new int[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }

        return result;
    }

    public static int[] FlattenRow(int[,] matrix, int rowIndex)
    {
        if (matrix == null)
        {
            throw new ArgumentNullException(nameof(matrix));
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        if (rowIndex < 0 || rowIndex >= rows)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index is out of range.");
        }

        int[] result = new int[cols];
        for (int j = 0; j < cols; j++)
        {
            result[j] = matrix[rowIndex, j];
        }

        return result;
    }
}
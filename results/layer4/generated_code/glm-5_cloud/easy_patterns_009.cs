using System.Collections.Generic;

public static class CollectionHelper
{
    public static List<int> CreateList(params int[] values)
    {
        List<int> list = [..values];
        return list;
    }
}
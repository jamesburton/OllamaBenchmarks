using System;
using System.Collections.Generic;
using System.Linq;

public static class Flattener
{
    public static List<string> FlattenWords(IEnumerable<string> sentences)
    {
        return sentences.SelectMany(sentence => sentence.Split(' ')).ToList();
    }
}
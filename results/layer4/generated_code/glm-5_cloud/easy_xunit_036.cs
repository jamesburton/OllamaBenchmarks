using System;
using System.Text;
using System.Linq;

public static class StringTransformer
{
    /// <summary>
    /// Converts a PascalCase or camelCase string to snake_case.
    /// Example: "HelloWorld" or "helloWorld" -> "hello_world"
    /// </summary>
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder();

        // Handle the first character separately to avoid leading underscore
        sb.Append(char.ToLower(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            char c = input[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLower(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a snake_case string to camelCase.
    /// Example: "hello_world" -> "helloWorld"
    /// </summary>
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder();
        bool nextUpper = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '_')
            {
                nextUpper = true;
            }
            else
            {
                if (nextUpper)
                {
                    sb.Append(char.ToUpper(c));
                    nextUpper = false;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        return sb.ToString();
    }
}
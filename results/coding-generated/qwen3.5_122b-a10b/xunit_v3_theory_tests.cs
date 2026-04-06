public class StringProcessor
        {
            public string Truncate(string input, int maxLength)
            {
                if (input is null) return "";
                return input.Length <= maxLength ? input : input[..maxLength] + "...";
            }

            public int CountWords(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) return 0;
                return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            }

            public bool IsPalindrome(string input)
            {
                if (string.IsNullOrEmpty(input)) return false;
                var cleaned = input.ToLowerInvariant().Replace(" ", "");
                return cleaned.SequenceEqual(cleaned.Reverse());
            }
        }
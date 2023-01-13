using System.Text;
using System.Globalization;

public static class Extensions
{
    public static bool TryGet(this string a, int index, out char result)
    {
        if (index < 0 || index >= a.Length)
        {
            result = default;
            return false;
        }

        result = a[index];
        return true;
    }
    public static bool TryGet<T>(this ReadOnlySpan<T> a, int index, out T? result)
    {
        if (index < 0 || index >= a.Length)
        {
            result = default;
            return false;
        }

        result = a[index];
        return true;
    }
    public static bool TryGet<T>(this IReadOnlyList<T> a, int index, out T? result)
    {
        if (index < 0 || index >= a.Count)
        {
            result = default;
            return false;
        }

        result = a[index];
        return true;
    }
    public static bool TryParse(this Span<char> a, out string result, out Error err)
    {
        err = default!;
        result = default!;

        var builder = new StringBuilder(a.Length);
        var iterator = new Iterator<char[], char>(a.ToArray());

        for (; iterator.PeekNext(out var ch); iterator.Index++)
        {
            if (ch == '\\')
            {
                iterator.Index++;
                iterator.PeekNext(out var ch1);

                switch (ch1)
                {
                    case 'a':
                        ch = '\a';
                        break;
                    case 'b':
                        ch = '\b';
                        break;
                    case 'n':
                        ch = '\n';
                        break;
                    case 'r':
                        ch = '\r';
                        break;
                    case 't':
                        ch = '\t';
                        break;
                    case '\'':
                        ch = '\'';
                        break;
                    case '"':
                        ch = '"';
                        break;
                    case 'x':
                        var start = iterator.Index + 2;
                        iterator.Index++;
                        for (var i = 0; i < 4; i++)
                        {
                            if (!iterator.PeekNext(out ch1)) break;
                            if (!char.IsDigit(ch1) && char.ToLower(ch1) is < 'a' or > 'f') break;
                            iterator.Index++;
                        }
                        if (iterator.Index + 1 == start)
                        {
                            err = new("Expected hex code after escape character 'x'", start);
                            return false;
                        }
                        ch = (char)ushort.Parse(iterator.List.AsSpan(start, iterator.Index - start + 1), NumberStyles.HexNumber);
                        break;
                    default:
                        err = new("Unexpected escape character", iterator.Index);
                        return false;
                }
            }
            builder.Append(ch);
        }

        result = builder.ToString();
        return true;
    }
}
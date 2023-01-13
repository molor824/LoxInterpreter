﻿public class Error : Exception
{
    public string Reason;
    public int Index;

    public string OutputError(string source, string? path)
    {
        var (line, column) = GetLocation(source, Index);
        var lineCpy = line;
        var len = 0;
        do
        {
            len++;
            lineCpy /= 10;
        }
        while (lineCpy > 0);

        return $"Error at: [{(path != null ? $"{Path.GetFullPath(path)}, " : "")}line: {line}, column: {column}]\n" +
        $"{line} | {GetLine(source, line)}\n" +
        new string(' ', len + 2 + column) +
        $"^ {Reason}";
    }

    private static (int, int) GetLocation(string source, int index)
    {
        var line = 1;
        var column = 1;

        for (var i = 0; i < index; i++)
        {
            var ch = source[i];

            if (ch == '\n')
            {
                line++;
                column = 0;
                continue;
            }
            column++;
        }

        return (line, column);
    }

    private static ReadOnlySpan<char> GetLine(string source, int line)
    {
        var start = 0;

        for (var i = 0; i < source.Length; i++)
        {
            var ch = source[i];

            if (ch != '\n' && i < source.Length - 1) continue;

            line--;

            if (line <= 0)
            {
                return source.AsSpan(start, i - start + (i == source.Length - 1 ? 1 : 0));
            }

            start = i + 1;
        }

        return null;
    }

    public Error(string reason, int index)
    {
        Reason = reason;
        Index = index;
    }
}
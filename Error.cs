public class Error : Exception
{
    public static Error Ident(int index) => new("Expected identifier", index);
    public static Error Expression(int index) => new("Expected expression", index);
    public static Error RightCurly(int index) => new("Expected '}'", index);
    public static Error LeftCurly(int index) => new("Expected '{'", index);
    public static Error RightBracket(int index) => new("Expected ')'", index);
    public static Error LeftBracket(int index) => new("Expected '('", index);
    public static Error Semicolon(int index) => new("Expected ';'", index);
    public static Error ArgLimit(int index) => new($"Cannot have more than {ArgLimit} arguments", index);
    public static Error SameField(int index) => new("Cannot have same field", index);
    public static Error SameMethod(int index) => new("Cannot have same method", index);
    public static Error MethodAssign(int index) => new($"Cannot assign as it is a method group", index);
    public static Error Property(int index) => new("Property does not exist", index);
    public static Error Declared(int index) => new("Variable is already declared", index);
    public static Error Undefined(int index) => new("Variable is undefined", index);
    public static Error ReadOwnInit(int index) => new("Cannot read in it's own initializer", index);

    public string Reason;
    public int Index;

    public string OutputError(string source, string? path)
    {
        if (Index < 0) return $"Error: {Reason}";

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
        new string(' ', len + 3 + column) +
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
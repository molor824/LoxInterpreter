using System.Text;
using System.Globalization;

public class Lexer
{
    enum NumType
    {
        Int,
        Float,
        Hex,
        Binary,
        Scientific
    }
    private static HashSet<string> _symbols = new()
    {
        // arithmetic
        "+", "-", "*", "/", "%", "++", "--", "**", "+=", "-=", "*=", "/=", "%=", "**=",
        // comparison
        "==", "!=", "<", ">", "<=", ">=",
        // logical
        "&&", "||", "!",
        // bitwise
        "&", "|", "~", "^", "<<", ">>", "&=", "|=", "^=", "<<=", ">>=",
        // brackets
        "{", "}", "<", ">", "[", "]", "(", ")",
        // other
        ",", ";", ".", "=", "<-",
    };
    private static int _maxLength;

    public string Source => new(_source.List);
    private Iterator<char[], char> _source;

    static Lexer()
    {
        foreach (var symbol in _symbols)
        {
            if (symbol.Length > _maxLength) _maxLength = symbol.Length;
        }
    }
    public static Lexer FromPath(string path)
    {
        var source = File.ReadAllText(path);
        var builder = new List<char>(source.Length);

        for (var i = 0; i < source.Length; i++)
        {
            var ch = source[i];
            if (ch == '\r')
            {
                if (source.TryGet(i + 1, out ch) && ch == '\n')
                    continue;
                ch = '\n';
            }

            builder.Add(ch);
        }

        return new() { _source = new(builder.ToArray()) };
    }
    Lexer()
    {
    }
    public Lexer(string source)
    {
        _source = new(source.ToArray());
    }

    public bool Parse(out Token? token, out Error error)
    {
        error = null!;
        token = null;

        while (_source.TryForward(out var result))
        {
            char result1;

            if (char.IsWhiteSpace(result)) continue;
            if (result == '/' && _source.PeekNext(out result1))
            {
                if (result1 == '/')
                {
                    while (_source.TryForward(out result1) && result1 != '\n')
                    {
                    }

                    continue;
                }

                if (result1 == '*')
                {
                    while (_source.TryForward(out result1) && (result1 != '/' || !_source.PeekPrevious(out result1) || result1 != '*'))
                    {
                    }
                    continue;
                }
            }

            if (char.IsLetter(result) || result == '_')
            {
                var start = _source.Index;
                for (; _source.PeekNext(out result1) && (char.IsLetter(result1) || char.IsDigit(result1) || result1 == '_'); _source.Index++)
                {
                }

                var length = _source.Index - start + 1;

                token = new Token.Ident(_source.List.AsSpan(start, length).ToString(), start, length);
                return true;
            }

            if (char.IsDigit(result) || (result == '.' && _source.PeekNext(out result1) && char.IsDigit(result1)))
            {
                var start = _source.Index;
                var numType = result == '.' ? NumType.Float : NumType.Int;
                if (result == '0' && _source.PeekNext(out result1))
                {
                    if (result1 == 'x')
                    {
                        numType = NumType.Hex;
                        _source.Index += 2;
                        start += 2;
                    }

                    if (result1 == 'b')
                    {
                        numType = NumType.Binary;
                        _source.Index += 2;
                        start += 2;
                    }
                }

                for (; _source.PeekNext(out result1); _source.Index++)
                {
                    result1 = char.ToLower(result1);
                    if (result1 is '0' or '1' && numType == NumType.Binary) continue;
                    if ((char.IsDigit(result1) || result1 is >= 'a' and <= 'f') && numType == NumType.Hex) continue;
                    if (char.IsDigit(result1)) continue;
                    if (result1 == '.')
                    {
                        if (numType != NumType.Int) break;
                        numType = NumType.Float;
                        continue;
                    }

                    if (result1 == 'e')
                    {
                        if (numType is not NumType.Int and not NumType.Float) break;
                        numType = NumType.Scientific;
                        if (_source.TryGet(_source.Index + 2, out result1) && result1 is '+' or '-')
                            _source.Index++;
                        continue;
                    }

                    break;
                }

                var length = _source.Index - start + 1;
                var source = _source.List.AsSpan(start, length);

                token = numType switch
                {
                    NumType.Int => new Token.Int(long.Parse(source), start, length),
                    NumType.Float => new Token.Float(double.Parse(source), start, length),
                    NumType.Scientific => new Token.Float(double.Parse(source, NumberStyles.Float), start, length),
                    NumType.Hex => new Token.Int(long.Parse(source, NumberStyles.HexNumber), start, length),
                    NumType.Binary => new Token.Int(Convert.ToInt64(source.ToString(), 2), start, length),
                    _ => default,
                };
                return true;
            }

            if (result == '"')
            {
                var start = _source.Index + 1;

                while (_source.TryForward(out result1) && result1 != '"')
                {
                    if (result1 == '\n')
                    {
                        error = new Error("Incomplete string", _source.Index - 1);
                        return false;
                    }
                    if (result1 == '\\' && _source.PeekNext(out result1) && result1 == '"')
                        _source.Index++;
                }
                var length = _source.Index - start;

                if (!_source.List.AsSpan(start, length).TryParse(out var str, out error))
                {
                    error.Index += start;
                    return false;
                }

                token = new Token.String(str, start, length);
                return true;
            }

            if (result == '\'')
            {
                var start = _source.Index + 1;

                while (_source.TryForward(out result1) && result1 != '\'')
                {
                    if (result1 == '\\' && _source.PeekNext(out result1) && result1 == '\'')
                        _source.Index++;
                }

                if (start == _source.Index)
                {
                    error = new("Empty character literal", start - 1);
                    return false;
                }

                var end = _source.Index;

                if (!_source.List.AsSpan(start, end - start).TryParse(out var str, out error))
                {
                    error.Index += start;
                    return false;
                }
                if (str.Length != 1)
                {
                    error = new(str.Length > 1 ? "Too many characters in char literal" : "Expected 1 character in char literal", end);
                    return false;
                }

                token = new Token.Char(str[0], start, 1);
                return true;
            }

            for (int i = _maxLength; i > 0; i--)
            {
                if (i + _source.Index > _source.List.Length) continue;
                var symbol = _source.List.AsSpan(_source.Index, i).ToString();
                if (!_symbols.Contains(symbol)) continue;

                token = new Token.Symbol(symbol, _source.Index, i);
                _source.Index += i - 1;
                return true;
            }

            error = new("Unexpected symbol", _source.Index);
            return false;
        }

        return true;
    }
}
using System.Text;
using System.Collections;

public class String8 : IEnumerable<Rune>
{
    List<byte> _raw = new();

    public IReadOnlyList<byte> Raw => _raw;

    public String8() { }
    public String8(string a)
    {
        _raw = new(Encoding.UTF8.GetBytes(a));
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Rune> GetEnumerator()
    {
        for (int i = 0; i < _raw.Count; i++)
        {
            if ((_raw[i] & 1 << 7) == 0)
            {
                yield return new Rune(_raw[i]);
            }
            if ((_raw[i] & 1 << 6) == 0) continue;

            int utfLength = UtfLengthFromFirstByte(_raw[i]);
            int ch = ((0b11111 >> (utfLength - 2)) & _raw[i]) << (6 * (utfLength - 1));

            for (int j = 0; j < utfLength - 1; j++)
                ch |= (_raw[i + (utfLength - 1 - j)] & 0b11_1111) << (6 * j);

            yield return (Rune)ch;
        }
    }
    public void DebugPrint()
    {
        foreach (var r in _raw)
        {
            Console.WriteLine(Convert.ToString(r, 2));
        }
    }
    static int UtfLengthFromFirstByte(byte first)
    {
        for (int i = 5; i >= 0; i--)
        {
            if ((first & 1 << i) == 0) return 7 - i;
        }

        return 8;
    }
    static int UtfLength(int digitAmount)
    {
        int utfLength = 1;
        while (digitAmount > (7 - utfLength))
        {
            utfLength++;
            digitAmount -= 6;
        }
        return utfLength;
    }
    static int Digits(int a, int numBase)
    {
        int total = 0;
        do
        {
            total++;
            a /= numBase;
        }
        while (a > 0);
        return total;
    }
    public override string ToString()
    {
        return Encoding.UTF8.GetString(_raw.ToArray());
    }
}
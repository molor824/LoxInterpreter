public abstract class Token
{
    public int Index, Length;
    public Token(int index, int length) { Index = index; Length = length; }
    public class Symbol : Token
    {
        public string Value;
        public Symbol(string value, int index, int length) : base(index, length)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return $"Symbol({Value})";
        }
    }
    public class String : Token
    {
        public string Value;
        public String(string value, int index, int length) : base(index, length)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return $"String({Value})";
        }
    }
    public class Char : Token
    {
        public char Value;
        public Char(char value, int index, int length) : base(index, length)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return $"Char({Value})";
        }
    }
    public class Ident : Token
    {
        public string Value;
        public Ident(string value, int index, int length) : base(index, length)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return $"Ident({Value})";
        }
    }
    public class Int : Token
    {
        public long Value;
        public Int(long value, int index, int length) : base(index, length)
        {
            Value = value;
        }
        public override string ToString()
        {
            return $"Int({Value})";
        }
    }
    public class Float : Token
    {
        public double Value;
        public Float(double value, int index, int length) : base(index, length)
        {
            Value = value;
        }
        public override string ToString()
        {
            return $"Float({Value})";
        }
    }
}
public class LoxClass : ICallable
{
    public Stmt.Class ParserInfo;
    public Dictionary<string, ICallable> Methods = new();
    public Dictionary<string, object> Fields = new();

    public LoxClass(Stmt.Class parserInfo, Interpreter interpreter)
    {
        ParserInfo = parserInfo;

        foreach (var method in parserInfo.Methods)
            Methods.Add(method.Key, (ICallable)method.Value.Expr.Accept(interpreter));
        foreach (var field in parserInfo.Fields)
            Fields.Add(field.Key, field.Value.Expr.Accept(interpreter));
    }

    public int Arity
    {
        get
        {
            if (Methods.TryGetValue("__init", out var value))
                return value.Arity - 1;

            return 0;
        }
    }
    public object Call(Interpreter interpreter, List<object> args, int index)
    {
        var instance = new LoxInstance(this);

        if (Methods.TryGetValue("__init", out var value))
        {
            args.Insert(0, instance);
            value.Call(interpreter, args, index);
        }

        return instance;
    }

    public override string ToString()
    {
        return $"{ParserInfo.Name.Value}";
    }
}
public class LoxInstance
{
    public LoxClass ClassInfo;
    public Dictionary<string, object> Fields;

    public LoxInstance(LoxClass classInfo)
    {
        ClassInfo = classInfo;
        Fields = new(classInfo.Fields);
    }
    public object Get(Token.Ident name)
    {
        if (Fields.TryGetValue(name.Value, out var value))
            return value;
        if (ClassInfo.Methods.TryGetValue(name.Value, out var funcValue))
            return funcValue;

        throw Error.Property(name.Index);
    }
    public void Set(Token.Ident name, object value)
    {
        if (ClassInfo.Methods.ContainsKey(name.Value))
            throw Error.MethodAssign(name.Index);
        if (!Fields.ContainsKey(name.Value))
            throw Error.Property(name.Index);

        Fields[name.Value] = value;
    }

    public override string ToString()
    {
        var output = "{\n";

        foreach (var field in Fields)
        {
            output += $"  {field.Key} = {(field.Value.ToString() ?? "").Replace("\n", "\n  ")};\n";
        }

        output += '}';

        return $"{ClassInfo.ParserInfo.Name.Value} {output}";
    }
}
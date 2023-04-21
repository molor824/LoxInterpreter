public class LoxClass : ICallable
{
    public string Name;
    public LoxClass? BaseClass;
    public Dictionary<string, ICallable> Methods = new();
    public Dictionary<string, object> Fields = new();

    public LoxClass(Stmt.Class parserInfo, Interpreter interpreter)
    {
        Name = parserInfo.Name.Value;

        if (parserInfo.BaseClass != null)
        {
            if (parserInfo.BaseClass.Accept(interpreter) is not LoxClass baseClass)
                throw Error.BaseClass(parserInfo.BaseClass.Index);

            BaseClass = baseClass;
        }

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
                return value.Arity - 2;

            return 0;
        }
    }
    public object Call(Interpreter interpreter, List<object> args, int index)
    {
        var instance = new LoxInstance(this, null);
        var classInfo = BaseClass;
        var crntInstance = instance;

        while (classInfo != null)
        {
            crntInstance.BaseInstance = new(classInfo, null);
            crntInstance = crntInstance.BaseInstance;
            classInfo = classInfo.BaseClass;
        }

        if (Methods.TryGetValue("__init", out var value))
        {
            args.Insert(0, (object?)instance.BaseInstance ?? Interpreter.NilVal);
            args.Insert(0, instance);
            value.Call(interpreter, args, index);
        }

        return instance;
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}
public class LoxInstance
{
    public LoxClass ClassInfo;
    public LoxInstance? BaseInstance;
    public Dictionary<string, object> Fields;

    public LoxInstance(LoxClass classInfo, LoxInstance? baseInstance)
    {
        ClassInfo = classInfo;
        BaseInstance = baseInstance;
        Fields = new(classInfo.Fields);
    }
    public object Get(Token.Ident name)
    {
        if (name.Value == "base") return (object?)BaseInstance ?? Interpreter.NilVal;

        var instance = this;
        while (instance != null)
        {
            if (instance.Fields.TryGetValue(name.Value, out var value))
                return value;
            if (instance.ClassInfo.Methods.TryGetValue(name.Value, out var funcValue))
                return funcValue;
            instance = instance.BaseInstance;
        }

        throw Error.Property(name.Index);
    }
    public void Set(Token.Ident name, object value)
    {
        if (name.Value == "base")
        {
            if (value is not LoxInstance newInstance || newInstance.ClassInfo != ClassInfo.BaseClass)
                throw Error.WrongBaseClass(ClassInfo.BaseClass?.ToString() ?? "nil", name.Index);

            BaseInstance = newInstance;
            return;
        }

        var instance = this;
        while (instance != null)
        {
            if (instance.ClassInfo.Methods.ContainsKey(name.Value))
                throw Error.MethodAssign(name.Index);
            if (instance.Fields.ContainsKey(name.Value))
            {
                instance.Fields[name.Value] = value;
                return;
            }

            instance = instance.BaseInstance;
        }

        throw Error.Property(name.Index);
    }

    public override string ToString()
    {
        var output = "{\n";

        foreach (var field in Fields)
        {
            output += $"  {field.Key} = {(field.Value.ToString() ?? "").Replace("\n", "\n  ")};\n";
        }

        output += '}';

        return $"{ClassInfo.Name} {output}";
    }
}
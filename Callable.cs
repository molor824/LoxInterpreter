public interface ICallable
{
    int Arity { get; }
    object Call(Interpreter interpreter, List<object> args, int index);
}
public class LoxFunction : ICallable
{
    Expr.Function _declaration;
    Environment _closure;
    public LoxFunction(Expr.Function declaration, Environment closure)
    {
        _declaration = declaration;
        _closure = closure;
    }
    public object Call(Interpreter interpreter, List<object> args, int index)
    {
        var old = Interpreter.Environment;
        Interpreter.Environment = new Environment(_closure);

        for (var i = 0; i < _declaration.Parameters.Count; i++)
        {
            var parameter = _declaration.Parameters[i];
            Interpreter.Environment.Declare(parameter, args[i]);
        }

        _declaration.Body.Accept(interpreter);

        Interpreter.Environment = old;

        var returnVal = Interpreter.ReturnValue;
        Interpreter.ReturnValue = null;

        return returnVal ?? Interpreter.NilVal;
    }
    public int Arity => _declaration.Parameters.Count;

    public override string ToString()
    {
        var parameters = "";

        for (var i = 0; i < _declaration.Parameters.Count; i++)
        {
            if (i != 0) parameters += ", ";
            parameters += _declaration.Parameters[i].Value;
        }

        return $"func({parameters})";
    }
}
public class Clock : ICallable
{
    public int Arity => 0;
    public object Call(Interpreter interpreter, List<object> args, int index) => Interpreter.Stopwatch.ElapsedMilliseconds;
}
public class Printf : ICallable
{
    public int Arity => -2; // -2 means it can have any amount of parameter. But there must be atleast 1
    public object Call(Interpreter interpreter, List<object> args, int index)
    {
        try
        {
            Console.WriteLine(Interpreter.TryCast<string>(args[0], index), args.Skip(1).ToArray());
        return Interpreter.NilVal;
        }
        catch (Exception e)
        {
            throw new Error(e.Message, index);
        }
    }
}
public class Print : ICallable
{
    public int Arity => -1; // -1 means i can have any amount of parameter including none
    public object Call(Interpreter interpreter, List<object> args, int index)
    {
        foreach (var arg in args)
            Console.Write(arg + " ");
        Console.WriteLine();

        return Interpreter.NilVal;
    }
}
public class Sprintf : ICallable
{
    public int Arity => -2;
    public object Call(Interpreter interpreter, List<object> args, int index)
    {
        try
        {
            return string.Format(Interpreter.TryCast<string>(args[0], index), args.Skip(1).ToArray());
        }
        catch (Exception e)
        {
            throw new Error(e.Message, index);
        }
    }
}
public interface ICallable
{
    int Arity { get; }
    object Call(Interpreter interpreter, List<object> args);
}
public class LoxFunction : ICallable
{
    public Expr.Function Declaration;
    public Environment Closure;
    public LoxFunction(Expr.Function declaration, Environment closure)
    {
        Declaration = declaration;
        Closure = closure;
    }
    public object Call(Interpreter interpreter, List<object> args)
    {
        var old = Interpreter.Environment;
        Interpreter.Environment = new Environment(Closure);

        for (var i = 0; i < Declaration.Parameters.Count; i++)
        {
            var parameter = Declaration.Parameters[i];
            Interpreter.Environment.Declare(parameter.Value, args[i], 0);
        }

        Declaration.Body.Accept(interpreter);

        Interpreter.Environment = old;

        var returnVal = Interpreter.ReturnValue;
        Interpreter.ReturnValue = null;

        return returnVal ?? Interpreter.NilVal;
    }
    public int Arity => Declaration.Parameters.Count;

    public override string ToString()
    {
        var parameters = "";

        for (var i = 0; i < Declaration.Parameters.Count; i++)
        {
            if (i != 0) parameters += ", ";
            parameters += Declaration.Parameters[i].Value;
        }

        return $"func({parameters})";
    }
}
public class Clock : ICallable
{
    public int Arity => 0;
    public object Call(Interpreter interpreter, List<object> args) => Interpreter.Stopwatch.ElapsedMilliseconds;
}
public class Printf : ICallable
{
    public int Arity => -2; // -2 means it can have any amount of parameter. But there must be atleast 1
    public object Call(Interpreter interpreter, List<object> args)
    {
        Console.WriteLine((string)args[0], args.Skip(1).ToArray());
        return Interpreter.NilVal;
    }
}
public class Print : ICallable
{
    public int Arity => -1; // -1 means i can have any amount of parameter including none
    public object Call(Interpreter interpreter, List<object> args)
    {
        foreach (var arg in args)
            Console.Write(arg + " ");
        Console.WriteLine();

        return Interpreter.NilVal;
    }
}
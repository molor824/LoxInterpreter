using System.Diagnostics;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Void>
{
    public static Dictionary<Type, List<Type>> ImplicitCastMap = new();
    public static Dictionary<(Type, Type), Func<object, object>> ImplicitCastings = new()
    {
        {(typeof(long), typeof(double)), a => (double)(long)a},
        {(typeof(char), typeof(long)), a => (long)(char)a},
        {(typeof(char), typeof(double)), a => (double)(char)a},
        {(typeof(bool), typeof(long)), a => (long)((bool)a ? 1 : 0)},
    };
    static Dictionary<(Type, string), Func<object, object>> UnaryOperators = new()
    {
        {(typeof(bool), "!"), a => !((bool)a)},
        {(typeof(long), "!"), a => ~((long)a)},
        {(typeof(long), "-"), a => -((long)a)},
        {(typeof(double), "-"), a => -((double)a)},
    };
    static Dictionary<(Type, string, Type), Func<object, object, object>> BinaryOperators = new()
    {
        {(typeof(object), "==", typeof(object)), (a, b) => a.Equals(b)},
        {(typeof(object), "!=", typeof(object)), (a, b) => !a.Equals(b)},
        {(typeof(long), "*", typeof(long)), (a, b) => (long)a * (long)b},
        {(typeof(long), "/", typeof(long)), (a, b) => (long)a / (long)b},
        {(typeof(long), "%", typeof(long)), (a, b) => (long)a % (long)b},
        {(typeof(long), "+", typeof(long)), (a, b) => (long)a + (long)b},
        {(typeof(long), "-", typeof(long)), (a, b) => (long)a - (long)b},
        {(typeof(long), "<", typeof(long)), (a, b) => (long)a < (long)b},
        {(typeof(long), ">", typeof(long)), (a, b) => (long)a > (long)b},
        {(typeof(long), "<=", typeof(long)), (a, b) => (long)a <= (long)b},
        {(typeof(long), ">=", typeof(long)), (a, b) => (long)a >= (long)b},
        {(typeof(char), "*", typeof(char)), (a, b) => (char)((char)a * (char)b)},
        {(typeof(char), "/", typeof(char)), (a, b) => (char)((char)a / (char)b)},
        {(typeof(char), "%", typeof(char)), (a, b) => (char)((char)a % (char)b)},
        {(typeof(char), "+", typeof(char)), (a, b) => (char)((char)a + (char)b)},
        {(typeof(char), "-", typeof(char)), (a, b) => (char)((char)a - (char)b)},
        {(typeof(char), "<", typeof(char)), (a, b) => (char)a < (char)b},
        {(typeof(char), ">", typeof(char)), (a, b) => (char)a > (char)b},
        {(typeof(char), "<=", typeof(char)), (a, b) => (char)a <= (char)b},
        {(typeof(char), ">=", typeof(char)), (a, b) => (char)a >= (char)b},
        {(typeof(char), "*", typeof(long)), (a, b) => new string((char)a, (int)(long)b)},
        {(typeof(double), "*", typeof(double)), (a, b) => (double)a * (double)b},
        {(typeof(double), "/", typeof(double)), (a, b) => (double)a / (double)b},
        {(typeof(double), "%", typeof(double)), (a, b) => (double)a % (double)b},
        {(typeof(double), "+", typeof(double)), (a, b) => (double)a + (double)b},
        {(typeof(double), "-", typeof(double)), (a, b) => (double)a - (double)b},
        {(typeof(double), "<", typeof(double)), (a, b) => (double)a < (double)b},
        {(typeof(double), ">", typeof(double)), (a, b) => (double)a > (double)b},
        {(typeof(double), "<=", typeof(double)), (a, b) => (double)a <= (double)b},
        {(typeof(double), ">=", typeof(double)), (a, b) => (double)a >= (double)b},
        {(typeof(string), "*", typeof(long)), (a, b) => {
            var aCpy = (string)a;
            var output = (string)a;
            var amount = (long)b - 1;
            for (var i = 0; i < amount; i++)
                output += aCpy;
            return output;
        }},
        {(typeof(string), "+", typeof(object)), (a, b) => (string)a + b},
        {(typeof(object), "+", typeof(string)), (a, b) => a + (string)b},
        {(typeof(bool), "&&", typeof(bool)), (a, b) => (bool)a && (bool)b},
        {(typeof(bool), "||", typeof(bool)), (a, b) => (bool)a || (bool)b},
    };
    public static Expr.Nil NilVal = new(0, 0);
    public static Stopwatch Stopwatch = Stopwatch.StartNew();
    public static Environment Environment = new();
    public static Dictionary<Expr, int> Locals = new();
    static bool Break, Continue;
    public static object? ReturnValue; // im not using exception for returning value, seems too insane and slow
    static int Loops;
    static Interpreter()
    {
        foreach (var ((type0, type1), _) in ImplicitCastings)
        {
            if (ImplicitCastMap.TryGetValue(type0, out var value))
                value.Add(type1);
            else ImplicitCastMap.Add(type0, new() { type1 });
        }

        InitNativeFunc();
    }
    public static T TryCast<T>(object value, int index)
    {
        if (value is not T) throw new Error($"Cannot implicitly cast {value.GetType().Name} to {typeof(T).Name}", index);

        return (T)value;
    }
    public static void InitNativeFunc()
    {
        Environment.RootScope.Declare(new("printf", 0, 0), new Printf());
        Environment.RootScope.Declare(new("print", 0, 0), new Print());
        Environment.RootScope.Declare(new("clock", 0, 0), new Clock());
        Environment.RootScope.Declare(new("sprintf", 0, 0), new Sprintf());
    }
    public void Resolve(Expr expr, int depth)
    {
        if (Program.Debug) Console.WriteLine($"{expr.Accept(new AstPrinter())}, {depth}");
        Locals.TryAdd(expr, depth);
    }
    public void Interpret(List<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            stmt.Accept(this);
        }
    }
    Void Stmt.IVisitor<Void>.visitReturn(Stmt.Return stmt)
    {
        ReturnValue = stmt.Value?.Accept(this);
        return new();
    }
    Void Stmt.IVisitor<Void>.visitBlock(Stmt.Block stmt)
    {
        var parent = Environment;
        Environment = new Environment(Environment);

        for (var i = 0; i < stmt.Statements.Count; i++)
        {
            stmt.Statements[i].Accept(this);

            if (ReturnValue != null) break;
            if (Loops > 0 && (Break || Continue)) break;
        }

        Environment = parent;

        return new();
    }
    Void Stmt.IVisitor<Void>.visitExpression(Stmt.Expression stmt)
    {
        stmt.Expr.Accept(this);
        return new();
    }
    Void Stmt.IVisitor<Void>.visitVarDecl(Stmt.VarDecl stmt)
    {
        Environment.Declare(stmt.Name, stmt.Expr.Accept(this));
        return new();
    }
    Void Stmt.IVisitor<Void>.visitIf(Stmt.If stmt)
    {
        var evalCondition = stmt.Condition.Accept(this);

        if (evalCondition is not bool)
            if (!ImplicitCastMap.TryGetValue(evalCondition.GetType(), out var types) || !types.Contains(typeof(bool)))
                throw new Error($"Expression cannot implicitly converted to bool", stmt.Condition.Index);
            else evalCondition = ImplicitCastings[(evalCondition.GetType(), typeof(bool))](evalCondition);

        if (evalCondition is true)
        {
            stmt.MetStmt.Accept(this);
            return new();
        }
        if (stmt.ElseStmt != null) stmt.ElseStmt.Accept(this);

        return new();
    }
    Void Stmt.IVisitor<Void>.visitBreak(Stmt.Break stmt)
    {
        Break = true;
        return new();
    }
    Void Stmt.IVisitor<Void>.visitContinue(Stmt.Continue stmt)
    {
        Continue = true;
        return new();
    }
    Void Stmt.IVisitor<Void>.visitFor(Stmt.For stmt)
    {
        var parent = Environment;
        Environment = new(parent);

        stmt.Initial?.Accept(this);

        Loops++;
        while (true)
        {
            var evalCondition = stmt.Condition?.Accept(this);

            if (evalCondition is not bool and not null)
                if (!ImplicitCastMap.TryGetValue(evalCondition.GetType(), out var types) || !types.Contains(typeof(bool)))
                    throw new Error($"Expression cannot implicitly convert to bool", stmt.Condition!.Index);
                else evalCondition = ImplicitCastings[(evalCondition.GetType(), typeof(bool))](evalCondition);

            if (evalCondition is true or null)
            {
                stmt.LoopStmt.Accept(this);

                if (ReturnValue != null) break;
                if (Break)
                {
                    Break = false;
                    stmt.ElseStmt?.Accept(this);
                    break;
                }

                Continue = false;
                stmt.Increment?.Accept(this);

                continue;
            }
            break;
        }

        Loops--;
        Environment = parent;

        return new();
    }
    Void Stmt.IVisitor<Void>.visitWhile(Stmt.While stmt)
    {
        Loops++;
        while (true)
        {
            var evalCondition = stmt.Condition.Accept(this);

            if (evalCondition is not bool)
                if (!ImplicitCastMap.TryGetValue(evalCondition.GetType(), out var types) || !types.Contains(typeof(bool)))
                    throw new Error($"Expression cannot implicitly converted to bool", stmt.Condition.Index);
                else evalCondition = ImplicitCastings[(evalCondition.GetType(), typeof(bool))](evalCondition);

            if (evalCondition is true)
            {
                stmt.LoopStmt.Accept(this);

                if (ReturnValue != null)
                    break;
                if (Break)
                {
                    Break = false;
                    if (stmt.ElseStmt != null) stmt.ElseStmt.Accept(this);
                    break;
                }

                Continue = false;
                continue;
            }
            break;
        }
        Loops--;
        return new();
    }
    public object visitProperty(Expr.Property expr)
    {
        var instance = expr.Instance.Accept(this);
        if (instance is LoxInstance lInstance)
        {
            return lInstance.Get(expr.Name);
        }

        throw Error.Property(expr.Name.Index);
    }
    object Expr.IVisitor<object>.visitCall(Expr.Call expr)
    {
        var callee = expr.Callee.Accept(this);

        if (callee is not ICallable callable) throw new Error("Expected method name", expr.Callee.Index);

        var args = new List<object>();

        foreach (var arg in expr.Args)
            args.Add(arg.Accept(this));

        if (callable.Arity >= 0 && callable.Arity != args.Count) throw new Error($"Function does not take {args.Count} parameters", expr.Callee.Index);
        else if (callable.Arity < 0 && ~callable.Arity > args.Count) throw new Error($"Function must have atleast {~callable.Arity} parameters", expr.Callee.Index);

        return callable.Call(this, args, expr.Index);
    }
    object Expr.IVisitor<object>.visitAssign(Expr.Assign expr)
    {
        var value = expr.Value.Accept(this);

        if (expr.Name is Expr.Variable varExpr)
        {
            if (Locals.TryGetValue(expr, out var depth))
                Environment.AssignAt(depth, varExpr.Name, value);
            else Environment.RootScope.AssignAt(0, varExpr.Name, value);
        }
        else
        {
            var propertyExpr = (Expr.Property)expr.Name;
            var property = (LoxInstance)(propertyExpr.Instance.Accept(this));

            property.Set(propertyExpr.Name, value);
        }

        return value;
    }
    public Void visitClass(Stmt.Class stmt)
    {
        Environment.Declare(stmt.Name, new LoxClass(stmt, this));

        return new();
    }
    object Expr.IVisitor<object>.visitFunction(Expr.Function expr) => new LoxFunction(expr, Environment);
    object Expr.IVisitor<object>.visitBoolean(Expr.Boolean expr) => expr.Value;
    object Expr.IVisitor<object>.visitFloat(Expr.Float expr) => expr.Value;
    object Expr.IVisitor<object>.visitInteger(Expr.Integer expr) => expr.Value;
    object Expr.IVisitor<object>.visitString(Expr.String expr) => expr.Value;
    object Expr.IVisitor<object>.visitChar(Expr.Char expr) => expr.Value;
    object Expr.IVisitor<object>.visitNil(Expr.Nil expr) => expr;
    object Expr.IVisitor<object>.visitVariable(Expr.Variable expr)
    {
        if (Locals.TryGetValue(expr, out var depth))
            return Environment.GetAt(depth, expr.Name);

        return Environment.RootScope.GetAt(0, expr.Name);
    }
    object Expr.IVisitor<object>.visitUnary(Expr.Unary expr)
    {
        var a = expr.Expr.Accept(this);
        if (UnaryOperators.TryGetValue((a.GetType(), expr.Op.Value), out var value))
            return value(a);
        if (UnaryOperators.TryGetValue((typeof(object), expr.Op.Value), out value))
            return value(a);

        var casts = ImplicitCastMap.GetValueOrDefault(a.GetType());

        if (casts != null)
        {
            foreach (var cast in casts)
            {
                var aCastFunc = ImplicitCastings[(a.GetType(), cast)];

                if (UnaryOperators.TryGetValue((cast, expr.Op.Value), out value))
                    return value(aCastFunc(a));
            }
        }

        throw new Error($"Operator '{expr.Op.Value}' cannot be applied to '{a.GetType().Name}'", expr.Op.Index);
    }
    object Expr.IVisitor<object>.visitBinary(Expr.Binary expr)
    {
        var a = expr.Left.Accept(this);
        var b = expr.Right.Accept(this);
        if (BinaryOperators.TryGetValue((a.GetType(), expr.Op.Value, b.GetType()), out var value))
        {
            var result = value(a, b);
            return result;
        }
        if (BinaryOperators.TryGetValue((a.GetType(), expr.Op.Value, typeof(object)), out value))
        {
            var result = value(a, b);
            return result;
        }
        if (BinaryOperators.TryGetValue((typeof(object), expr.Op.Value, b.GetType()), out value))
        {
            var result = value(a, b);
            return result;
        }
        if (BinaryOperators.TryGetValue((typeof(object), expr.Op.Value, typeof(object)), out value))
        {
            var result = value(a, b);
            return result;
        }

        var aCasts = ImplicitCastMap.GetValueOrDefault(a.GetType());
        var bCasts = ImplicitCastMap.GetValueOrDefault(b.GetType());

        if (aCasts != null)
        {
            foreach (var aCast in aCasts)
            {
                var aCastFunc = ImplicitCastings[(a.GetType(), aCast)];

                if (BinaryOperators.TryGetValue((aCast, expr.Op.Value, b.GetType()), out value))
                {
                    var result = value(aCastFunc(a), b);
                    return result;
                }
                if (bCasts != null)
                {
                    foreach (var bCast in bCasts)
                    {
                        var bCastFunc = ImplicitCastings[(b.GetType(), bCast)];

                        if (BinaryOperators.TryGetValue((aCast, expr.Op.Value, bCast), out value))
                        {
                            var result = value(aCastFunc(a), bCastFunc(b));
                            return result;
                        }
                    }
                }
            }
        }
        else if (bCasts != null)
        {
            foreach (var bCast in bCasts)
            {
                var bCastFunc = ImplicitCastings[(b.GetType(), bCast)];

                if (BinaryOperators.TryGetValue((a.GetType(), expr.Op.Value, bCast), out value))
                {
                    var result = value(a, bCastFunc(b));
                    return result;
                }
            }
        }

        throw new Error($"Operator '{expr.Op.Value}' cannot be applied to '{a.GetType().Name}' and '{b.GetType().Name}'", expr.Op.Index);
    }
}
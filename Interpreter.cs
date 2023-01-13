using System.Collections.Generic;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Void>
{
    static Dictionary<Type, HashSet<Type>> ImplicitCastMap = new();
    static Dictionary<(Type, Type), Func<object, object>> ImplicitCastings = new()
    {
        {(typeof(long), typeof(double)), a => (double)(long)a},
        {(typeof(char), typeof(long)), a => (long)(char)a},
        {(typeof(char), typeof(double)), a => (double)(char)a},
        {(typeof(bool), typeof(long)), a => (long)((bool)a ? 1 : 0)},
    };
    static Dictionary<(Type, string), Func<object, object>> UnaryOperators = new()
    {
        {(typeof(bool), "!"), a => !(bool)a!},
        {(typeof(long), "!"), a => ~(long)a!},
        {(typeof(long), "-"), a => -(long)a!},
        {(typeof(double), "-"), a => -(double)a!},
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
        {(typeof(bool), "&&", typeof(bool)), (a, b) => (bool)a && (bool)b},
        {(typeof(bool), "||", typeof(bool)), (a, b) => (bool)a || (bool)b},
    };
    static Environment Environment = new();
    static bool Break, Continue;
    static int Loops;
    static Interpreter()
    {
        foreach (var ((type0, type1), _) in ImplicitCastings)
        {
            if (ImplicitCastMap.TryGetValue(type0, out var value))
                value.Add(type1);
            else ImplicitCastMap.Add(type0, new() { type1 });
        }
    }
    public void Interpret(List<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            Execute(stmt);
        }
    }
    void Execute(Stmt stmt) => stmt.Accept(this);
    public object Eval(Expr expr) => expr.Accept(this);
    Void Stmt.IVisitor<Void>.visitBlock(Stmt.Block stmt)
    {
        var parent = Environment;
        Environment = new Environment(Environment);

        foreach (var statement in stmt.Statements)
        {
            Execute(statement);
            if (Loops > 0 && (Break || Continue)) break;
        }

        Environment = parent;

        return new();
    }
    Void Stmt.IVisitor<Void>.visitExpression(Stmt.Expression stmt)
    {
        Eval(stmt.Expr);
        return new();
    }
    Void Stmt.IVisitor<Void>.visitPrint(Stmt.Print stmt)
    {
        Console.WriteLine(Eval(stmt.Expr));
        return new();
    }
    Void Stmt.IVisitor<Void>.visitVarDecl(Stmt.VarDecl stmt)
    {
        Environment.Declare(stmt.Name.Value, Eval(stmt.Expr), stmt.Name.Index);
        return new();
    }
    Void Stmt.IVisitor<Void>.visitIf(Stmt.If stmt)
    {
        var evalCondition = Eval(stmt.Condition);

        if (evalCondition is not bool)
            if (!ImplicitCastMap.TryGetValue(evalCondition.GetType(), out var types) || !types.Contains(typeof(bool)))
                throw new Error($"Expression cannot implicitly converted to bool", stmt.Condition.Index);
            else evalCondition = ImplicitCastings[(evalCondition.GetType(), typeof(bool))](evalCondition);

        if (evalCondition is true)
        {
            Execute(stmt.MetStmt);
            return new();
        }
        if (stmt.ElseStmt != null) Execute(stmt.ElseStmt);

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

        if (stmt.Initial != null) Execute(stmt.Initial);

        Loops++;
        while (true)
        {
            var evalCondition = stmt.Condition != null ? Eval(stmt.Condition) : (object)true;

            if (evalCondition is not bool)
                if (!ImplicitCastMap.TryGetValue(evalCondition.GetType(), out var types) || !types.Contains(typeof(bool)))
                    throw new Error($"Expression cannot implicitly convert to bool", stmt.Condition!.Index);
                else evalCondition = ImplicitCastings[(evalCondition.GetType(), typeof(bool))](evalCondition);

            if (evalCondition is true)
            {
                Execute(stmt.LoopStmt);

                if (Break)
                {
                    Break = false;
                    if (stmt.ElseStmt != null) Execute(stmt.ElseStmt);
                    break;
                }
                Continue = false;

                if (stmt.Increment != null) Eval(stmt.Increment);

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
            var evalCondition = Eval(stmt.Condition);

            if (evalCondition is not bool)
                if (!ImplicitCastMap.TryGetValue(evalCondition.GetType(), out var types) || !types.Contains(typeof(bool)))
                    throw new Error($"Expression cannot implicitly converted to bool", stmt.Condition.Index);
                else evalCondition = ImplicitCastings[(evalCondition.GetType(), typeof(bool))](evalCondition);

            if (evalCondition is true)
            {
                Execute(stmt.LoopStmt);
                if (Break)
                {
                    Break = false;
                    if (stmt.ElseStmt != null) Execute(stmt.ElseStmt);
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
    object Expr.IVisitor<object>.visitAssign(Expr.Assign expr)
    {
        var value = Eval(expr.LValue);
        Environment.Assign(expr.RValue.Name, value);
        return value;
    }
    object Expr.IVisitor<object>.visitBoolean(Expr.Boolean expr) => expr.Value;
    object Expr.IVisitor<object>.visitFloat(Expr.Float expr) => expr.Value;
    object Expr.IVisitor<object>.visitInteger(Expr.Integer expr) => expr.Value;
    object Expr.IVisitor<object>.visitString(Expr.String expr) => expr.Value;
    object Expr.IVisitor<object>.visitChar(Expr.Char expr) => expr.Value;
    object Expr.IVisitor<object>.visitNil(Expr.Nil expr) => expr;
    object Expr.IVisitor<object>.visitVariable(Expr.Variable expr) => Environment.Get(expr.Name);
    object Expr.IVisitor<object>.visitUnary(Expr.Unary expr)
    {
        var a = Eval(expr.Expr);
        if (UnaryOperators.TryGetValue((a.GetType(), expr.Op.Value), out var value))
        {
            var result = value(a);
            return a;
        }

        throw new Error($"Operator '{expr.Op.Value}' cannot be applied to '{a.GetType().Name}'", expr.Op.Index);
    }
    object Expr.IVisitor<object>.visitBinary(Expr.Binary expr)
    {
        var a = Eval(expr.Left);
        var b = Eval(expr.Right);
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
        if (BinaryOperators.TryGetValue((typeof(object), expr.Op.Value, a.GetType()), out value))
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
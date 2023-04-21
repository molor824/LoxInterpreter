public class Resolver : Stmt.IVisitor<Void>, Expr.IVisitor<Void>
{
    static Token.Ident ThisParam = new("this", 0, 0);
    static Token.Ident BaseParam = new("base", 0, 0);

    Interpreter _interpreter;
    Stack<Dictionary<string, bool>> _scopes = new();
    ScopeType _scopeType = ScopeType.Global;
    enum ScopeType
    {
        Global,
        Function,
        Method
    }

    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }
    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements) statement.Accept(this);
    }
    void Declare(Token.Ident name)
    {
        if (!_scopes.Any()) return;
        if (!_scopes.Peek().TryAdd(name.Value, false)) throw Error.Declared(name.Index);
    }
    void Define(Token.Ident name)
    {
        if (!_scopes.Any()) return;
        _scopes.Peek()[name.Value] = true;
    }
    public Void visitVarDecl(Stmt.VarDecl stmt)
    {
        Declare(stmt.Name);

        if (stmt.Expr is Expr.Function) Define(stmt.Name); // to allow recursion inside the function

        stmt.Expr.Accept(this);

        Define(stmt.Name);

        return new();
    }
    public Void visitBlock(Stmt.Block stmt)
    {
        _scopes.Push(new());

        foreach (var statement in stmt.Statements)
            statement.Accept(this);

        _scopes.Pop();

        return new();
    }
    public Void visitWhile(Stmt.While stmt)
    {
        stmt.Condition.Accept(this);
        stmt.LoopStmt.Accept(this);
        stmt.ElseStmt?.Accept(this);
        return new();
    }
    public Void visitFor(Stmt.For stmt)
    {
        _scopes.Push(new());

        stmt.Initial?.Accept(this);
        stmt.Condition?.Accept(this);
        stmt.Increment?.Accept(this);
        stmt.LoopStmt.Accept(this);
        stmt.ElseStmt?.Accept(this);

        _scopes.Pop();

        return new();
    }
    public Void visitClass(Stmt.Class stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        if (stmt.BaseClass != null)
        {
            if (stmt.Name.Value == stmt.BaseClass.Name.Value)
                throw Error.SelfInheritance(stmt.BaseClass.Index);

            stmt.BaseClass.Accept(this);
        }

        var old = _scopeType;
        _scopeType = ScopeType.Method;

        foreach (var method in stmt.Methods)
            method.Value.Accept(this);

        _scopeType = old;

        return new();
    }
    public Void visitIf(Stmt.If stmt)
    {
        stmt.Condition.Accept(this);
        stmt.MetStmt.Accept(this);
        stmt.ElseStmt?.Accept(this);

        return new();
    }
    public Void visitReturn(Stmt.Return stmt)
    {
        stmt.Value?.Accept(this);
        return new();
    }
    public Void visitBreak(Stmt.Break _) => new();
    public Void visitContinue(Stmt.Continue _) => new();
    public Void visitExpression(Stmt.Expression stmt) => stmt.Expr.Accept(this);
    public Void visitAssign(Expr.Assign expr)
    {
        expr.Value.Accept(this);
        if (expr.Name is Expr.Variable varExpr)
            ResolveVariable(expr, varExpr.Name);
        else expr.Name.Accept(this);

        return new();
    }
    public Void visitVariable(Expr.Variable expr)
    {
        if (!_scopes.Any()) return new();
        if (_scopes.Peek().TryGetValue(expr.Name.Value, out var value) && !value)
            throw Error.ReadOwnInit(expr.Index);

        ResolveVariable(expr, expr.Name);

        return new();
    }
    public Void visitProperty(Expr.Property expr)
    {
        expr.Instance.Accept(this);
        return new();
    }
    public Void visitFunction(Expr.Function expr)
    {
        _scopes.Push(new());

        var old = _scopeType;
        if (_scopeType != ScopeType.Method)
            _scopeType = ScopeType.Function;
        else
        {
            expr.Parameters.Insert(0, BaseParam);
            expr.Parameters.Insert(0, ThisParam);
        }

        foreach (var parameter in expr.Parameters)
        {
            Declare(parameter);
            Define(parameter);
        }
        expr.Body.Accept(this);

        _scopeType = old;

        _scopes.Pop();

        return new();
    }
    public Void visitCall(Expr.Call expr)
    {
        expr.Callee.Accept(this);

        if (expr.Callee is Expr.Property prop)
        {
            expr.Args.Insert(0, new Expr.Property(BaseParam, prop.Instance, prop.Index, prop.Length));
            expr.Args.Insert(0, prop.Instance);
        }
        foreach (var arg in expr.Args) arg.Accept(this);

        return new();
    }
    public Void visitBinary(Expr.Binary expr)
    {
        expr.Left.Accept(this);
        return expr.Right.Accept(this);
    }
    public Void visitUnary(Expr.Unary expr) => expr.Expr.Accept(this);
    public Void visitInteger(Expr.Integer _) => new();
    public Void visitFloat(Expr.Float _) => new();
    public Void visitString(Expr.String _) => new();
    public Void visitChar(Expr.Char _) => new();
    public Void visitBoolean(Expr.Boolean _) => new();
    public Void visitNil(Expr.Nil _) => new();

    void ResolveVariable(Expr expr, Token.Ident name)
    {
        var depth = 0;
        foreach (var element in _scopes)
        {
            if (element.ContainsKey(name.Value))
            {
                _interpreter.Resolve(expr, depth);
                return;
            }

            depth++;
        }
    }
}
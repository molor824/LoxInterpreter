public class AstPrinter : Expr.IVisitor<Node<string>>, Stmt.IVisitor<Node<string>>
{
    public Node<string> visitFunction(Expr.Function stmt)
    {
        var output = new Node<string>("func");
        var parametersNode = new Node<string>("parameters", output);

        foreach (var parameter in stmt.Parameters)
            new Node<string>(parameter.Value, parametersNode);

        return output;
    }
    public Node<string> visitProperty(Expr.Property expr)
    {
        return new Node<string>(expr.Name.Value, expr.Instance.Accept(this));
    }
    public Node<string> visitClass(Stmt.Class stmt)
    {
        var output = new Node<string>($"class {stmt.Name}");
        var fieldsNode = new Node<string>("fields", output);
        var methodsNode = new Node<string>("methods", output);

        foreach (var field in stmt.Fields)
            field.Value.Accept(this).Parent = fieldsNode;
        foreach (var method in stmt.Methods)
            method.Value.Accept(this).Parent = methodsNode;

        return output;
    }
    public Node<string> visitReturn(Stmt.Return stmt) => new Node<string>("return", stmt.Accept(this));
    public Node<string> visitBlock(Stmt.Block stmt)
    {
        var output = new Node<string>("scope");

        foreach (var statement in stmt.Statements)
            statement.Accept(this).Parent = output;

        return output;
    }
    public Node<string> visitBreak(Stmt.Break stmt) => new("break");
    public Node<string> visitContinue(Stmt.Continue stmt) => new("continue");
    public Node<string> visitExpression(Stmt.Expression stmt) => stmt.Expr.Accept(this);
    public Node<string> visitFor(Stmt.For stmt)
    {
        var output = new Node<string>("for");

        if (stmt.Initial != null) stmt.Initial.Accept(this).Parent = new("initial", output);
        if (stmt.Condition != null) stmt.Condition.Accept(this).Parent = new("condition", output);
        if (stmt.Increment != null) stmt.Increment.Accept(this).Parent = new("increment", output);

        stmt.LoopStmt.Accept(this).Parent = new("loop", output);

        if (stmt.ElseStmt != null) stmt.ElseStmt.Accept(this).Parent = new("else", output);

        return output;
    }
    public Node<string> visitIf(Stmt.If stmt)
    {
        var output = new Node<string>("if");

        stmt.Condition.Accept(this).Parent = new("condition", output);
        stmt.MetStmt.Accept(this).Parent = new("met", output);
        if (stmt.ElseStmt != null) stmt.ElseStmt.Accept(this).Parent = new("else", output);

        return output;
    }
    public Node<string> visitVarDecl(Stmt.VarDecl stmt)
    {
        var output = new Node<string>("variable declaration");
        new Node<string>(stmt.Name.Value, output);
        stmt.Expr.Accept(this).Parent = output;

        return output;
    }
    public Node<string> visitWhile(Stmt.While stmt)
    {
        var output = new Node<string>("while");
        stmt.Condition.Accept(this).Parent = new("condition", output);
        stmt.LoopStmt.Accept(this).Parent = new("loop", output);
        if (stmt.ElseStmt != null) stmt.ElseStmt.Accept(this).Parent = new("else", output);

        return output;
    }
    public Node<string> visitCall(Expr.Call expr)
    {
        var output = new Node<string>("call ");
        expr.Callee.Accept(this).Parent = output;

        var args = new Node<string>("arguments", output);

        foreach (var arg in expr.Args)
            arg.Accept(this).Parent = args;

        return output;
    }
    public Node<string> visitAssign(Expr.Assign expr)
    {
        var output = new Node<string>("assign");
        expr.Name.Accept(this).Parent = output;
        expr.Value.Accept(this).Parent = output;

        return output;
    }
    public Node<string> visitBinary(Expr.Binary expr)
    {
        var output = new Node<string>($"binary {expr.Op.Value}");
        expr.Left.Accept(this).Parent = output;
        expr.Right.Accept(this).Parent = output;

        return output;
    }
    public Node<string> visitUnary(Expr.Unary expr)
    {
        var output = new Node<string>($"unary {expr.Op.Value}");
        expr.Expr.Accept(this).Parent = output;

        return output;
    }
    public Node<string> visitBoolean(Expr.Boolean expr) => new("bool " + expr.Value);
    public Node<string> visitChar(Expr.Char expr) => new("char " + expr.Value);
    public Node<string> visitString(Expr.String expr) => new("string " + expr.Value);
    public Node<string> visitInteger(Expr.Integer expr) => new("int " + expr.Value);
    public Node<string> visitFloat(Expr.Float expr) => new("float " + expr.Value);
    public Node<string> visitNil(Expr.Nil _) => new("nil");
    public Node<string> visitVariable(Expr.Variable expr) => new("variable " + expr.Name.Value);
}
public class Node<T>
{
    public static string LastChildPrint = "\u2514\u2500";
    public static string ChildPrint = "\u251C\u2500";
    public static string ChildMiddlePrint = "\u2502 ";
    public static string LastChildMiddlePrint = "  ";

    public T Value;
    public Node<T>? Parent
    {
        get => _parent;
        set
        {
            if (value == _parent) return;

            _parent?._children.Remove(this);
            value?._children.Add(this);

            _parent = value;
        }
    }
    public IReadOnlyList<Node<T>> Children => _children;

    Node<T>? _parent;
    List<Node<T>> _children = new();

    public Node(T value)
    {
        Value = value;
    }
    public Node(T value, Node<T> parent)
    {
        Value = value;
        Parent = parent;
    }
    public Node(T value, IEnumerable<Node<T>> children)
    {
        Value = value;
        foreach (var child in children)
        {
            child.Parent = this;
        }
    }
    public override string ToString()
    {
        var output = SelfToString();
        foreach (var child in Children)
        {
            output += '\n' + child.ToString();
        }

        return output;
    }
    public string SelfToString()
    {
        var output = "";
        var parent = Parent;
        var child = this;

        while (parent != null)
        {
            var index = parent._children.FindIndex(n => n == child);

            if (index == parent.Children.Count - 1)
            {
                if (child == this) output = LastChildPrint + output;
                else output = LastChildMiddlePrint + output;
            }
            else if (child == this) output = ChildPrint + output;
            else output = ChildMiddlePrint + output;

            child = child!.Parent;
            parent = parent.Parent;
        }

        return output + Value?.ToString();
    }
}
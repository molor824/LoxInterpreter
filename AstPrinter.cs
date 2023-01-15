public class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
{
    string Expr.IVisitor<string>.visitFunction(Expr.Function stmt)
    {
        var parameters = "";
        for (var i = 0; i < stmt.Parameters.Count; i++)
        {
            if (i != 0) parameters += ", ";
            parameters += stmt.Parameters[i].Value;
        }

        return $"func({parameters}) {stmt.Body.Accept(this)}";
    }
    string Stmt.IVisitor<string>.visitReturn(Stmt.Return stmt) => $"return {stmt.Value?.Accept(this)};";
    string Stmt.IVisitor<string>.visitBlock(Stmt.Block stmt)
    {
        var output = "{\n";
        foreach (var statement in stmt.Statements)
        {
            output += "  " + statement.Accept(this).Replace("\n", "\n  ") + '\n';
        }
        return output + "\n}";
    }
    string Stmt.IVisitor<string>.visitBreak(Stmt.Break stmt)
    {
        return "break;";
    }
    string Stmt.IVisitor<string>.visitContinue(Stmt.Continue stmt)
    {
        return "continue;";
    }
    string Stmt.IVisitor<string>.visitExpression(Stmt.Expression stmt)
    {
        return $"ExprStmt({stmt.Expr.Accept(this)})";
    }
    string Stmt.IVisitor<string>.visitFor(Stmt.For stmt)
    {
        return $"For({stmt.Initial?.Accept(this)};{stmt.Condition?.Accept(this)};{stmt.Increment?.Accept(this)}){stmt.LoopStmt.Accept(this)}{(stmt.ElseStmt != null ? " else " + stmt.ElseStmt.Accept(this) : "")}";
    }
    string Stmt.IVisitor<string>.visitIf(Stmt.If stmt)
    {
        return $"If({stmt.Condition.Accept(this)}) {stmt.MetStmt.Accept(this)}{(stmt.ElseStmt != null ? " else " + stmt.ElseStmt.Accept(this) : "")}";
    }
    string Stmt.IVisitor<string>.visitVarDecl(Stmt.VarDecl stmt)
    {
        return $"VarDeclare({stmt.Name.Value}, {stmt.Expr.Accept(this)})";
    }
    string Stmt.IVisitor<string>.visitWhile(Stmt.While stmt)
    {
        return $"While({stmt.Condition.Accept(this)}) {stmt.LoopStmt.Accept(this)}{(stmt.ElseStmt != null ? " else " + stmt.ElseStmt.Accept(this) : "")}";
    }
    string Expr.IVisitor<string>.visitCall(Expr.Call expr)
    {
        var args = "";

        for (var i = 0; i < expr.Args.Count; i++)
        {
            if (i != 0) args += ", ";
            args += expr.Args[i].Accept(this);
        }

        return $"{expr.Callee.Accept(this)}({args})";
    }
    string Expr.IVisitor<string>.visitAssign(Expr.Assign expr)
    {
        return $"Assign({expr.LValue.Accept(this)}, {expr.RValue.Accept(this)})";
    }
    string Expr.IVisitor<string>.visitBinary(Expr.Binary expr)
    {
        return $"Binary({expr.Op.Value}, {expr.Left.Accept(this)}, {expr.Right.Accept(this)})";
    }
    string Expr.IVisitor<string>.visitUnary(Expr.Unary expr)
    {
        return $"Unary({expr.Op.Value}, {expr.Expr.Accept(this)})";
    }
    string Expr.IVisitor<string>.visitBoolean(Expr.Boolean expr)
    {
        return $"Bool({expr.Value})";
    }
    string Expr.IVisitor<string>.visitChar(Expr.Char expr)
    {
        return $"Char({expr.Value})";
    }
    string Expr.IVisitor<string>.visitString(Expr.String expr)
    {
        return $"String({expr.Value})";
    }
    string Expr.IVisitor<string>.visitInteger(Expr.Integer expr)
    {
        return $"Int({expr.Value})";
    }
    string Expr.IVisitor<string>.visitFloat(Expr.Float expr)
    {
        return $"Float({expr.Value})";
    }
    string Expr.IVisitor<string>.visitNil(Expr.Nil expr)
    {
        return "nil";
    }
    string Expr.IVisitor<string>.visitVariable(Expr.Variable expr)
    {
        return $"Variable({expr.Name.Value})";
    }
}
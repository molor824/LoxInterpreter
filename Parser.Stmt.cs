partial class Parser
{
    const int ArgLimit = 255;
    static Expr.Integer One = new(1, 0, 0);

    bool Match(Func<Token, bool> compare, out Token result)
    {
        var peekSuccess = _token.PeekNext(out result) && compare(result);
        if (peekSuccess) _token.Index++;
        return peekSuccess;
    }
    Token Consume(Func<Token, bool> compare, Error err)
    {
        if (!_token.TryForward(out var result) || !compare(result))
            throw err;
        return result;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();

        while (Declaration(out var stmt)) statements.Add(stmt);

        return statements;
    }
    bool Declaration(out Stmt stmt)
    {
        if (VarDecl(out stmt)) return true;
        if (FuncDecl(out stmt)) return true;
        if (ClassDecl(out stmt)) return true;
        return Statement(out stmt);
    }
    bool ClassDecl(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "class" }, out var result)) return false;

        var name = Consume(t => t is Token.Ident, Error.Ident(result.Index + result.Length));
        var classStmt = new Stmt.Class((Token.Ident)name, new(), new(), null, result.Index, name.Index + name.Length - result.Index);

        if (Match(t => t is Token.Symbol { Value: ":" }, out result))
        {
            if (!Primary(out var expr)) throw Error.Ident(result.Index);
            if (expr is not Expr.Variable varExpr) throw Error.Ident(result.Index);

            classStmt.BaseClass = varExpr;
        }

        result = Consume(t => t is Token.Symbol { Value: "{" }, Error.LeftCurly(name.Index + name.Length));

        while (true)
        {
            if (VarDecl(out stmt))
            {
                var varDecl = (Stmt.VarDecl)stmt;

                if (!classStmt.Fields.TryAdd(varDecl.Name.Value, varDecl))
                    throw Error.SameField(varDecl.Name.Index);
            }
            else if (FuncDecl(out stmt))
            {
                var varDecl = (Stmt.VarDecl)stmt;

                if (!classStmt.Methods.TryAdd(varDecl.Name.Value, varDecl))
                    throw Error.SameMethod(varDecl.Name.Index);
            }
            else break;

            classStmt.Length = stmt.Index + stmt.Length - classStmt.Index;
        }

        result = Consume(t => t is Token.Symbol { Value: "}" }, Error.RightCurly(classStmt.Index + classStmt.Length));

        stmt = classStmt;
        return true;
    }
    bool FuncDecl(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "fn" }, out var result)) return false;
        if (!Match(t => t is Token.Ident, out result)) throw Error.Ident(result.Index + result.Length);
        if (!Parameter(out var expr)) throw Error.LeftBracket(result.Index + result.Length);

        var funcExpr = (Expr.Function)expr;

        if (!Block(out funcExpr.Body) && !ExprStmt(out funcExpr.Body)) throw Error.LeftCurly(expr.Index + expr.Length);
        if (funcExpr.Body is Stmt.Expression exprStmt) funcExpr.Body = new Stmt.Return(exprStmt.Expr, exprStmt.Index, exprStmt.Length);

        funcExpr.Length = funcExpr.Body.Index + funcExpr.Body.Length - funcExpr.Index;

        stmt = new Stmt.VarDecl((Token.Ident)result, expr, result.Index, expr.Index + expr.Length - result.Index);
        return true;
    }
    bool Statement(out Stmt stmt)
    {
        if (IfStmt(out stmt))
            return true;
        if (WhileStmt(out stmt))
            return true;
        if (ForStmt(out stmt))
            return true;
        if (BreakStmt(out stmt))
            return true;
        if (ContinueStmt(out stmt))
            return true;
        if (Block(out stmt))
            return true;
        if (ReturnStmt(out stmt))
            return true;

        return ExprStmt(out stmt);
    }
    bool ReturnStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Symbol { Value: "<-" }, out var result)) return false;

        Expression(out var expr);

        stmt = new Stmt.Return(expr, result.Index, expr.Index + expr.Length + 1 - result.Index);

        Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(expr.Index + expr.Length));

        return true;
    }
    bool ForStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "for" }, out var result)) return false;
        result = Consume(t => t is Token.Symbol { Value: "(" }, Error.LeftBracket(result.Index + 3));

        var startIndex = result.Index;
        Stmt? init = null;

        if (VarDecl(out var decl)) init = decl;
        else if (ExprStmt(out var expr)) init = expr;
        else { result = Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(result.Index + 3)); }

        Expression(out var condition);
        result = Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(condition != null ? condition.Index + condition.Length : result.Index + 1));

        Expression(out var increment);
        result = Consume(t => t is Token.Symbol { Value: ")" }, Error.RightBracket(increment != null ? increment.Index + increment.Length : result.Index + 1));

        if (!Statement(out var loopStmt)) throw Error.LeftCurly(increment != null ? increment.Index + increment.Length : result.Index + 1);

        var forStmt = new Stmt.For(init, condition, increment, loopStmt, null, startIndex, loopStmt.Index + loopStmt.Length - startIndex);
        if (Match(t => t is Token.Ident { Value: "else" }, out result))
        {
            if (!Statement(out var elseStmt)) throw Error.LeftCurly(result.Index + 4);

            forStmt.ElseStmt = elseStmt;
            forStmt.Length = elseStmt.Index + elseStmt.Length - forStmt.Index;
        }

        stmt = forStmt;
        return true;
    }
    bool WhileStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "while" }, out var result)) return false;

        result = Consume(t => t is Token.Symbol { Value: "(" }, Error.LeftBracket(result.Index + 5));
        if (!Expression(out var condition)) throw Error.Expression(result.Index + 1);
        result = Consume(t => t is Token.Symbol { Value: ")" }, Error.RightBracket(condition.Index + condition.Length));
        if (!Statement(out var loopStmt)) throw Error.LeftCurly(result.Index + 1);

        var whileStmt = new Stmt.While(condition, loopStmt, null, result.Index, loopStmt.Index + loopStmt.Length - result.Index);
        if (Match(t => t is Token.Ident { Value: "else" }, out var result1))
        {
            if (!Statement(out var elseStmt)) throw Error.LeftCurly(whileStmt.Index + whileStmt.Length);

            whileStmt.ElseStmt = elseStmt;
            whileStmt.Length = elseStmt.Index + elseStmt.Length - whileStmt.Index;
        }

        stmt = whileStmt;
        return true;
    }
    bool IfStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "if" }, out var result)) return false;

        result = Consume(t => t is Token.Symbol { Value: "(" }, Error.LeftBracket(result.Index + 2));
        if (!Expression(out var condition)) throw Error.Expression(result.Index + 1);
        result = Consume(t => t is Token.Symbol { Value: ")" }, Error.RightBracket(condition.Index + condition.Length));
        if (!Statement(out var metStmt)) throw Error.LeftCurly(result.Index + 1);

        var ifStmt = new Stmt.If(condition, metStmt, null, result.Index, metStmt.Index + metStmt.Length - result.Index);

        if (Match(t => t is Token.Ident { Value: "else" }, out var result1))
        {
            if (!Statement(out ifStmt.ElseStmt)) throw Error.LeftCurly(ifStmt.Index + ifStmt.Length);

            ifStmt.Length = ifStmt.ElseStmt.Index + ifStmt.ElseStmt.Length - ifStmt.Index;
        }

        stmt = ifStmt;

        return true;
    }
    bool BreakStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "break" }, out var result)) return false;
        var result1 = Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(result.Index + result.Length));

        stmt = new Stmt.Break(result.Index, result1.Index + result1.Length - result.Index);
        return true;
    }
    bool ContinueStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "continue" }, out var result)) return false;
        var result1 = Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(result.Index + result.Length));

        stmt = new Stmt.Continue(result.Index, result1.Index + result1.Length - result.Index);
        return true;
    }
    bool VarDecl(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "var" }, out var result)) return false;
        var name = Consume(t => t is Token.Ident, Error.Ident(result.Index + 3));

        var varDecl = new Stmt.VarDecl((Token.Ident)name, Interpreter.NilVal, name.Index, name.Length);
        if (Match(t => t is Token.Symbol { Value: "=" }, out result))
        {
            if (!Expression(out varDecl.Expr)) throw Error.Expression(result.Index + 1);
            varDecl.Length = varDecl.Expr.Index + varDecl.Expr.Length;
        }

        Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(varDecl.Expr.Index + varDecl.Expr.Length));

        stmt = varDecl;
        return true;
    }
    bool Block(out Stmt stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Symbol { Value: "{" }, out var result)) return false;

        var errIndex = 0;
        errIndex = result.Index + result.Length;

        var statements = new List<Stmt>();

        while (Declaration(out stmt))
        {
            statements.Add(stmt);
            errIndex = stmt.Index + stmt.Length;
        }

        Consume(t => t is Token.Symbol { Value: "}" }, Error.RightCurly(errIndex));

        stmt = new Stmt.Block(statements, result.Index, errIndex + 1 - result.Index);
        return true;
    }
    bool ExprStmt(out Stmt stmt)
    {
        stmt = default!;

        if (!Expression(out var expr)) return false;

        stmt = new Stmt.Expression(expr, expr.Index, expr.Length);

        Consume(t => t is Token.Symbol { Value: ";" }, Error.Semicolon(stmt.Index + stmt.Length));

        return true;
    }
}
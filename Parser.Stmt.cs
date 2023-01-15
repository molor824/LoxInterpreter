partial class Parser
{
    const int ArgLimit = 255;

    Error IdentErr(int index) => new("Expected identifier", index);
    Error ExpressionErr(int index) => new("Expected expression", index);
    Error RightCurlyErr(int index) => new("Expected '}'", index);
    Error LeftCurlyErr(int index) => new("Expected '{'", index);
    Error RightBracketErr(int index) => new("Expected ')'", index);
    Error LeftBracketErr(int index) => new("Expected '('", index);
    Error SemicolonErr(int index) => new("Expected ';'", index);
    Error ArgLimitErr(int index) => new($"Cannot have more than {ArgLimit} arguments", index);

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
        if (VarDecl(out var varStmt))
        {
            stmt = varStmt;
            return true;
        }
        return Statement(out stmt);
    }
    bool Statement(out Stmt stmt)
    {
        if (IfStmt(out var ifStmt))
        {
            stmt = ifStmt;
            return true;
        }
        if (WhileStmt(out var whileStmt))
        {
            stmt = whileStmt;
            return true;
        }
        if (ForStmt(out var forStmt))
        {
            stmt = forStmt;
            return true;
        }
        if (BreakStmt(out var breakStmt))
        {
            stmt = breakStmt;
            return true;
        }
        if (ContinueStmt(out var continueStmt))
        {
            stmt = continueStmt;
            return true;
        }
        if (Block(out var blockStmt))
        {
            stmt = blockStmt;
            return true;
        }
        if (ReturnStmt(out var returnStmt))
        {
            stmt = returnStmt;
            return true;
        }
        if (ExprStmt(out var exprStmt))
        {
            stmt = exprStmt;
            return true;
        }
        stmt = default!;
        return false;
    }
    bool ReturnStmt(out Stmt.Return stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Symbol { Value: "<-" }, out var result)) return false;

        Expression(out var expr);

        stmt = new Stmt.Return(expr, result.Index, expr.Index + expr.Length + 1 - result.Index);

        Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(expr.Index + expr.Length));

        return true;
    }
    bool ForStmt(out Stmt.For stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "for" }, out var result)) return false;
        result = Consume(t => t is Token.Symbol { Value: "(" }, LeftBracketErr(result.Index + 3));

        var startIndex = result.Index;
        Stmt? init = null;

        if (VarDecl(out var decl)) init = decl;
        else if (ExprStmt(out var expr)) init = expr;
        else { result = Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(result.Index + 3)); }

        Expression(out var condition);
        result = Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(condition != null ? condition.Index + condition.Length : result.Index + 1));

        Expression(out var increment);
        result = Consume(t => t is Token.Symbol { Value: ")" }, RightBracketErr(increment != null ? increment.Index + increment.Length : result.Index + 1));

        if (!Statement(out var loopStmt)) throw LeftCurlyErr(increment != null ? increment.Index + increment.Length : result.Index + 1);

        stmt = new(init, condition, increment, loopStmt, null, startIndex, loopStmt.Index + loopStmt.Length - startIndex);
        if (!Match(t => t is Token.Ident { Value: "else" }, out result)) return true;
        if (!Statement(out var elseStmt)) throw LeftCurlyErr(result.Index + 4);

        stmt.ElseStmt = elseStmt;
        stmt.Length = elseStmt.Index + elseStmt.Length - stmt.Index;
        return true;
    }
    bool WhileStmt(out Stmt.While stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "while" }, out var result)) return false;

        result = Consume(t => t is Token.Symbol { Value: "(" }, LeftBracketErr(result.Index + 5));
        if (!Expression(out var condition)) throw ExpressionErr(result.Index + 1);
        result = Consume(t => t is Token.Symbol { Value: ")" }, RightBracketErr(condition.Index + condition.Length));
        if (!Statement(out var loopStmt)) throw LeftCurlyErr(result.Index + 1);

        stmt = new(condition, loopStmt, null, result.Index, loopStmt.Index + loopStmt.Length - result.Index);
        if (!Match(t => t is Token.Ident { Value: "else" }, out var result1)) return true;

        if (!Statement(out var elseStmt)) throw LeftCurlyErr(stmt.Index + stmt.Length);

        stmt.ElseStmt = elseStmt;
        stmt.Length = elseStmt.Index + elseStmt.Length - stmt.Index;
        return true;
    }
    bool IfStmt(out Stmt.If stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "if" }, out var result)) return false;

        result = Consume(t => t is Token.Symbol { Value: "(" }, LeftBracketErr(result.Index + 2));
        if (!Expression(out var condition)) throw ExpressionErr(result.Index + 1);
        result = Consume(t => t is Token.Symbol { Value: ")" }, RightBracketErr(condition.Index + condition.Length));
        if (!Statement(out var metStmt)) throw LeftCurlyErr(result.Index + 1);

        stmt = new(condition, metStmt, null, result.Index, metStmt.Index + metStmt.Length - result.Index);
        if (!Match(t => t is Token.Ident { Value: "else" }, out var result1)) return true;

        if (!Statement(out var elseStmt)) throw LeftCurlyErr(stmt.Index + stmt.Length);

        stmt.ElseStmt = elseStmt;
        stmt.Length = elseStmt.Index + elseStmt.Length - stmt.Index;
        return true;
    }
    bool BreakStmt(out Stmt.Break stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "break" }, out var result)) return false;
        var result1 = Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(result.Index + result.Length));

        stmt = new(result.Index, result1.Index + result1.Length - result.Index);
        return true;
    }
    bool ContinueStmt(out Stmt.Continue stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "continue" }, out var result)) return false;
        var result1 = Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(result.Index + result.Length));

        stmt = new(result.Index, result1.Index + result1.Length - result.Index);
        return true;
    }
    bool VarDecl(out Stmt.VarDecl stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Ident { Value: "var" }, out var result)) return false;
        var name = Consume(t => t is Token.Ident, IdentErr(result.Index + 3));

        stmt = new Stmt.VarDecl((Token.Ident)name, Interpreter.NilVal, name.Index, name.Length);
        if (Match(t => t is Token.Symbol { Value: "=" }, out result))
        {
            if (!Expression(out stmt.Expr)) throw ExpressionErr(result.Index + 1);
            stmt.Length = stmt.Expr.Index + stmt.Expr.Length;
        }

        Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(stmt.Expr.Index + stmt.Expr.Length));

        return true;
    }
    bool Block(out Stmt.Block stmt)
    {
        stmt = default!;

        if (!Match(t => t is Token.Symbol { Value: "{" }, out var result)) return false;

        var errIndex = 0;
        errIndex = result.Index + result.Length;

        var statements = new List<Stmt>();

        while (Declaration(out var decl))
        {
            statements.Add(decl);
            errIndex = decl.Index + decl.Length;
        }

        Consume(t => t is Token.Symbol { Value: "}" }, RightCurlyErr(errIndex));
        stmt = new(statements, result.Index, errIndex + 1 - result.Index);

        return true;
    }
    bool ExprStmt(out Stmt.Expression stmt)
    {
        stmt = default!;

        if (!Expression(out var expr)) return false;
        stmt = new Stmt.Expression(expr, expr.Index, expr.Length);

        Consume(t => t is Token.Symbol { Value: ";" }, SemicolonErr(stmt.Index + stmt.Length));

        return true;
    }
}
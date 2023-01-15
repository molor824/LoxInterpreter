public partial class Parser
{
    Iterator<List<Token>, Token> _token;

    public Parser(List<Token> tokens)
    {
        _token = new(tokens);
    }

    bool Expression(out Expr expr) => Assignment(out expr);
    bool Assignment(out Expr expr)
    {
        if (!OrLogic(out expr)) return false;
        if (expr is not Expr.Variable) return true;
        if (!Match(t => t is Token.Symbol { Value: "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "||=" or "&&=" }, out var result)) return true;
        if (!Assignment(out var lvalue)) throw ExpressionErr(result.Index);

        var assignOp = (Token.Symbol)result;

        expr = new Expr.Assign((Expr.Variable)expr, lvalue, expr.Index, lvalue.Index + lvalue.Length - expr.Index);
        if (assignOp.Value == "=")
            return true;

        assignOp.Value = assignOp.Value.Substring(0, assignOp.Value.Length - 1);

        var assignExpr = (Expr.Assign)expr;
        assignExpr.LValue = new Expr.Binary(assignExpr.RValue, lvalue, assignOp, 0, 0);

        return true;
    }
    bool OrLogic(out Expr expr)
    {
        if (!AndLogic(out expr)) return false;

        while (_token.PeekNext(out var result) && result is Token.Symbol { Value: "||" })
        {
            _token.Index++;

            if (!AndLogic(out var right)) throw ExpressionErr(result.Index + 1);

            expr = new Expr.Binary(expr, right, (Token.Symbol)result, expr.Index, right.Index + right.Length - expr.Index);
        }

        return true;
    }
    bool AndLogic(out Expr expr)
    {
        if (!Equality(out expr)) return false;

        while (_token.PeekNext(out var result) && result is Token.Symbol { Value: "&&" })
        {
            _token.Index++;

            if (!Equality(out var right)) throw ExpressionErr(result.Index + 1);

            expr = new Expr.Binary(expr, right, (Token.Symbol)result, expr.Index, right.Index + right.Length - expr.Index);
        }

        return true;
    }
    bool Equality(out Expr expr)
    {
        if (!Comparison(out expr)) return false;

        while (_token.PeekNext(out var result) && result is Token.Symbol { Value: "==" or "!=" })
        {
            _token.Index++;

            if (!Comparison(out var right)) throw ExpressionErr(result.Index + 1);

            expr = new Expr.Binary(expr, right, (Token.Symbol)result, expr.Index, right.Index + right.Length - expr.Index);
        }

        return true;
    }
    bool Comparison(out Expr expr)
    {
        if (!Term(out expr)) return false;

        while (_token.PeekNext(out var result) && result is Token.Symbol { Value: "<" or ">" or "<=" or ">=" })
        {
            _token.Index++;

            if (!Term(out var right)) throw ExpressionErr(result.Index + 1);

            expr = new Expr.Binary(expr, right, (Token.Symbol)result, expr.Index, right.Index + right.Length - expr.Index);
        }

        return true;
    }
    bool Term(out Expr expr)
    {
        if (!Factor(out expr)) return false;

        while (_token.PeekNext(out var result) && result is Token.Symbol { Value: "+" or "-" })
        {
            _token.Index++;

            if (!Factor(out var right)) throw ExpressionErr(result.Index + 1);

            expr = new Expr.Binary(expr, right, (Token.Symbol)result, expr.Index, right.Index + right.Length - expr.Index);
        }

        return true;
    }
    bool Factor(out Expr expr)
    {
        if (!Unary(out expr)) return false;

        while (_token.PeekNext(out var result) && result is Token.Symbol { Value: "*" or "/" or "%" })
        {
            _token.Index++;

            if (!Unary(out var right)) throw ExpressionErr(result.Index + 1);

            expr = new Expr.Binary(expr, right, (Token.Symbol)result, expr.Index, right.Index + right.Length - expr.Index);
        }

        return true;
    }
    bool Unary(out Expr expr)
    {
        expr = default!;

        if (!_token.PeekNext(out var result)) return false;

        if (result is not Token.Symbol { Value: "!" or "-" })
            return Increment(out expr);

        _token.Index++;

        if (!Unary(out expr)) throw ExpressionErr(result.Index + 1);

        expr = new Expr.Unary(expr, (Token.Symbol)result, result.Index, expr.Index + expr.Length - result.Index);
        return true;
    }
    bool Increment(out Expr expr)
    {
        expr = default!;

        if (!Call(out expr)) return false;
        if (expr is not Expr.Variable) return true;
        if (!Match(t => t is Token.Symbol { Value: "++" or "--" }, out var result)) return true;

        var op = (Token.Symbol)result;
        op.Value = op.Value.Substring(0, op.Value.Length - 1);

        var variable = (Expr.Variable)expr;
        expr = new Expr.Binary(expr, new Expr.Integer(1, 0, 0), op, expr.Index, expr.Length);
        expr = new Expr.Assign(variable, expr, variable.Index, result.Index + result.Length - variable.Index);

        return true;
    }
    bool Call(out Expr expr)
    {
        expr = default!;

        if (!Primary(out expr)) return false;

        while (Match(t => t is Token.Symbol { Value: "(" }, out var result))
            expr = Args(expr);

        return true;
    }
    Expr Args(Expr callee)
    {
        var args = new List<Expr>();
        var startIndex = callee.Index;
        var endIndex = callee.Index + 2;

        if (!Match(t => t is Token.Symbol { Value: ")" }, out var result))
        {
            do
            {
                if (!Expression(out var arg)) throw ExpressionErr(_token.TryCurrent(out result) ? result.Index + result.Length : 0);

                args.Add(arg);
                endIndex = arg.Index + arg.Length;

                if (args.Count > ArgLimit) throw ArgLimitErr(endIndex);
            } while (Match(t => t is Token.Symbol { Value: "," }, out result));

            result = Consume(t => t is Token.Symbol { Value: ")" }, RightBracketErr(endIndex));
        }

        return new Expr.Call(callee, (Token.Symbol)result, args, startIndex, endIndex - startIndex);
    }
    bool Parameter(out Expr.Function expr)
    {
        expr = default!;

        if (!Match(t => t is Token.Symbol { Value: "(" }, out var result)) return false;

        var startIndex = result.Index;
        var endIndex = startIndex;
        var parameters = new List<Token.Ident>(ArgLimit);

        if (!Match(t => t is Token.Symbol { Value: ")" }, out result))
        {
            do
            {
                if (!Match(t => t is Token.Ident, out result)) throw IdentErr(endIndex);

                parameters.Add((Token.Ident)result);
                endIndex = result.Index + result.Length;
            } while (Match(t => t is Token.Symbol { Value: "," }, out var _));

            result = Consume(t => t is Token.Symbol { Value: ")" }, RightBracketErr(endIndex));
        }

        endIndex = result.Index + result.Length;

        expr = new(parameters, null!, startIndex, endIndex - startIndex);

        return true;
    }
    bool Primary(out Expr expr)
    {
        expr = default!;

        if (!_token.PeekNext(out var result)) return false;
        _token.Index++;

        if (result is Token.Float floatToken)
        {
            expr = new Expr.Float(floatToken.Value, result.Index, result.Length);
            return true;
        }
        if (result is Token.Int intToken)
        {
            expr = new Expr.Integer(intToken.Value, result.Index, result.Length);
            return true;
        }
        if (result is Token.Char charToken)
        {
            expr = new Expr.Char(charToken.Value, result.Index, result.Length);
            return true;
        }
        if (result is Token.String strToken)
        {
            expr = new Expr.String(strToken.Value, result.Index, result.Length);
            return true;
        }
        if (result is Token.Ident { Value: "true" or "false" } boolToken)
        {
            expr = new Expr.Boolean(boolToken.Value == "true", result.Index, result.Length);
            return true;
        }
        if (result is Token.Ident { Value: "nil" })
        {
            expr = new Expr.Nil(result.Index, result.Length);
            return true;
        }
        if (result is Token.Ident { Value: "fn" })
        {
            if (!Parameter(out var funcExpr)) throw LeftBracketErr(result.Index + result.Length);
            if (!Block(out var stmt))
            {
                result = Consume(t => t is Token.Symbol { Value: "=>" }, LeftCurlyErr(funcExpr.Index + funcExpr.Length));

                if (!Expression(out expr)) throw ExpressionErr(result.Index + result.Length);

                funcExpr.Body = new Stmt.Return(expr, expr.Index, expr.Length);
            }
            else funcExpr.Body = stmt;

            funcExpr.Length = funcExpr.Index + funcExpr.Length - result.Index;
            funcExpr.Index = result.Index;

            expr = funcExpr;

            return true;
        }
        if (result is Token.Symbol { Value: "(" })
        {
            var exprResult = Expression(out expr);
            Consume(token => token is Token.Symbol { Value: ")" }, LeftBracketErr(expr.Index + expr.Length));
            return exprResult;
        }
        if (result is Token.Ident varName)
        {
            expr = new Expr.Variable(varName, result.Index, result.Length);
            return true;
        }

        _token.Index--;
        return false;
    }
}
//this file was generated by exprGen.js
public abstract class Expr {
public abstract T Accept<T>(IVisitor<T> visitor);
public abstract int Index { get; }
public abstract int Length { get; }
public class Binary : Expr {
public Expr Left;
public Expr Right;
public Token.Symbol Op;
int _index, _length;
public Binary(Expr left, Expr right, Token.Symbol op, int index, int length) {
this.Left = left;
this.Right = right;
this.Op = op;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitBinary(this);
public override int Index => _index;
public override int Length => _length;
}
public class Unary : Expr {
public Expr Expr;
public Token.Symbol Op;
int _index, _length;
public Unary(Expr expr, Token.Symbol op, int index, int length) {
this.Expr = expr;
this.Op = op;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitUnary(this);
public override int Index => _index;
public override int Length => _length;
}
public class String : Expr {
public string Value;
int _index, _length;
public String(string value, int index, int length) {
this.Value = value;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitString(this);
public override int Index => _index;
public override int Length => _length;
}
public class Char : Expr {
public char Value;
int _index, _length;
public Char(char value, int index, int length) {
this.Value = value;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitChar(this);
public override int Index => _index;
public override int Length => _length;
}
public class Integer : Expr {
public long Value;
int _index, _length;
public Integer(long value, int index, int length) {
this.Value = value;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitInteger(this);
public override int Index => _index;
public override int Length => _length;
}
public class Float : Expr {
public double Value;
int _index, _length;
public Float(double value, int index, int length) {
this.Value = value;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitFloat(this);
public override int Index => _index;
public override int Length => _length;
}
public class Boolean : Expr {
public bool Value;
int _index, _length;
public Boolean(bool value, int index, int length) {
this.Value = value;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitBoolean(this);
public override int Index => _index;
public override int Length => _length;
}
public class Variable : Expr {
public Token.Ident Name;
int _index, _length;
public Variable(Token.Ident name, int index, int length) {
this.Name = name;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitVariable(this);
public override int Index => _index;
public override int Length => _length;
}
public class Assign : Expr {
public Expr.Variable RValue;
public Expr LValue;
int _index, _length;
public Assign(Expr.Variable rvalue, Expr lvalue, int index, int length) {
this.RValue = rvalue;
this.LValue = lvalue;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitAssign(this);
public override int Index => _index;
public override int Length => _length;
}
public class Nil : Expr {
int _index, _length;
public Nil(int index, int length) {
_index = index;
_length = length;
}
public override string ToString() {
return "nil";
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitNil(this);
public override int Index => _index;
public override int Length => _length;
}
public interface IVisitor<T> {
T visitBinary(Expr.Binary expr);
T visitUnary(Expr.Unary expr);
T visitString(Expr.String expr);
T visitChar(Expr.Char expr);
T visitInteger(Expr.Integer expr);
T visitFloat(Expr.Float expr);
T visitBoolean(Expr.Boolean expr);
T visitVariable(Expr.Variable expr);
T visitAssign(Expr.Assign expr);
T visitNil(Expr.Nil expr);
}
}
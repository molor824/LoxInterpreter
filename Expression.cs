//this file was generated by exprGen.js
public abstract class Expr {
public abstract T Accept<T>(IVisitor<T> visitor);
public abstract int Index { get; set; }
public abstract int Length { get; set; }
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
}
public class Assign : Expr {
public Expr Name;
public Expr Value;
int _index, _length;
public Assign(Expr name, Expr value, int index, int length) {
this.Name = name;
this.Value = value;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitAssign(this);
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
}
public class Call : Expr {
public Expr Callee;
public Token.Symbol Paren;
public List<Expr> Args;
int _index, _length;
public Call(Expr callee, Token.Symbol paren, List<Expr> args, int index, int length) {
this.Callee = callee;
this.Paren = paren;
this.Args = args;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitCall(this);
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
}
public class Property : Expr {
public Token.Ident Name;
public Expr Instance;
int _index, _length;
public Property(Token.Ident name, Expr instance, int index, int length) {
this.Name = name;
this.Instance = instance;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitProperty(this);
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
}
public class Function : Expr {
public List<Token.Ident> Parameters;
public Stmt Body;
int _index, _length;
public Function(List<Token.Ident> parameters, Stmt body, int index, int length) {
this.Parameters = parameters;
this.Body = body;
_index = index;
_length = length;
}
public override T Accept<T>(IVisitor<T> visitor) => visitor.visitFunction(this);
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
public override int Index {get=>_index;set=>_index=value;}
public override int Length {get=>_length;set=>_length=value;}
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
T visitCall(Expr.Call expr);
T visitProperty(Expr.Property expr);
T visitFunction(Expr.Function expr);
T visitNil(Expr.Nil expr);
}
}
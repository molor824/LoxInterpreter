const fs = require('fs');

function iterDict(dict, iterLambda) {
    Object.keys(dict).forEach((key, i, a) => iterLambda(key, dict[key], i, a.length));
}
let statements = {
    Expression: { Expr: "Expr" },
    VarDecl: { Name: "Token.Ident", Expr: "Expr" },
    Block: { Statements: "List<Stmt>" },
    If: { Condition: "Expr", MetStmt: "Stmt", ElseStmt: "Stmt?" },
    While: { Condition: "Expr", LoopStmt: "Stmt", ElseStmt: "Stmt?" },
    For: { Initial: "Stmt?", Condition: "Expr?", Increment: "Expr?", LoopStmt: "Stmt", ElseStmt: "Stmt?" },
    Return: { Value: "Expr?" },
    Class: { Name: "Token.Ident", Methods: "Dictionary<string, Stmt.VarDecl>", Fields: "Dictionary<string, Stmt.VarDecl>", BaseClass: "Expr.Variable?" },
    Break: {},
    Continue: {},
};
let output = String.raw`//this file was generated by stmtGen.js
public abstract class Stmt {
public abstract T Accept<T>(IVisitor<T> visitor);
public abstract int Index {get;set;}
public abstract int Length {get;set;}
`;
iterDict(statements, (expr, fields) => {
    output += `public class ${expr} : Stmt {\n`;
    iterDict(fields, (name, type) => {
        output += `public ${type} ${name};\n`;
    });
    output += 'int _index;\nint _length;\n';
    output += `public ${expr}(`;
    iterDict(fields, (name, type, i, len) => {
        output += `${type} ${name.toLowerCase()}, `;
    });
    output += 'int index, int length';
    output += ') {\n';
    iterDict(fields, (name, _) => {
        output += `this.${name} = ${name.toLowerCase()};\n`;
    });
    output += '_index = index;\n_length = length;\n';
    output += '}\n';
    output += `public override T Accept<T>(IVisitor<T> visitor) => visitor.visit${expr}(this);\n`;
    output += 'public override int Index{get=>_index;set=>_index=value;}\npublic override int Length{get=>_length;set=>_length=value;}\n';
    output += '}\n';
});
output += 'public interface IVisitor<T> {\n';
iterDict(statements, (expr) => {
    output += `T visit${expr}(Stmt.${expr} stmt);\n`;
});
output += '}\n}';

fs.writeFile('Statement.cs', output, (err) => {
    if (err) console.log(err);
    else console.log('SUCCESS');
});

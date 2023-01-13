const fs = require('fs');

function iterDict(dict, iterLambda) {
    Object.keys(dict).forEach((key, i, a) => iterLambda(key, dict[key], i, a.length));
}
let expressions = {
    Binary: { Left: "Expr", Right: "Expr", Op: "Token.Symbol" },
    Unary: { Expr: "Expr", Op: "Token.Symbol" },
    String: { Value: "string" },
    Char: { Value: "char" },
    Integer: { Value: "long" },
    Float: { Value: "double" },
    Boolean: { Value: "bool" },
    Variable: { Name: "Token.Ident" },
    Assign: { RValue: "Expr.Variable", LValue: "Expr" },
    Nil: { _STR: 'return "nil";' },
};

let output = String.raw`//this file was generated by exprGen.js
public abstract class Expr {
public abstract T Accept<T>(IVisitor<T> visitor);
public abstract int Index { get; }
public abstract int Length { get; }
`;
iterDict(expressions, (expr, fields) => {
    output += `public class ${expr} : Expr {\n`;
    iterDict(fields, (name, type) => {
        if (name.startsWith('_')) return;
        output += `public ${type} ${name};\n`;
    });
    output += 'int _index, _length;\n'
    output += `public ${expr}(`;
    iterDict(fields, (name, type, i, len) => {
        if (name.startsWith('_')) return;
        output += `${type} ${name.toLowerCase()}, `;
    });
    output += 'int index, int length';
    output += ') {\n';
    iterDict(fields, (name, _) => {
        if (name.startsWith('_')) return;
        output += `this.${name} = ${name.toLowerCase()};\n`;
    });
    output += '_index = index;\n_length = length;\n';
    output += '}\n';
    iterDict(fields, (name, type) => {
        if (name == '_STR') {
            output += 'public override string ToString() {\n';
            output += type;
            output += '\n}\n';
        }
    });
    output += `public override T Accept<T>(IVisitor<T> visitor) => visitor.visit${expr}(this);\n`;
    output += 'public override int Index => _index;\npublic override int Length => _length;\n';
    output += '}\n';
});
output += 'public interface IVisitor<T> {\n';
iterDict(expressions, (expr) => {
    output += `T visit${expr}(Expr.${expr} expr);\n`;
});
output += '}\n}';

fs.writeFile('Expression.cs', output, (err) => {
    if (err) console.log(err);
    else console.log('SUCCESS');
});

public class Environment
{
    public Environment? ParentScope;
    public Dictionary<string, object> Variables = new();

    public Environment RootScope
    {
        get
        {
            var root = this;

            while (root.ParentScope != null) root = root.ParentScope;

            return root;
        }
    }

    public Environment(Environment? parentScope = null)
    {
        ParentScope = parentScope;
    }
    public void Declare(Token.Ident name, object value)
    {
        if (Variables.ContainsKey(name.Value))
            throw Error.Undefined(name.Index);

        Variables.Add(name.Value, value);
    }
    Environment? Ancestor(int distance)
    {
        var scope = this;
        for (var i = 0; i < distance && scope != null; i++)
            scope = scope.ParentScope;

        return scope;
    }
    public object GetAt(int distance, Token.Ident name)
    {
        if (Ancestor(distance)!.Variables.TryGetValue(name.Value, out var value)) return value;
        throw Error.Undefined(name.Index);
    }
    public void AssignAt(int distance, Token.Ident name, object value)
    {
        var vars = Ancestor(distance)!.Variables;

        if (!vars.ContainsKey(name.Value))
            throw Error.Undefined(name.Index);

        vars[name.Value] = value;
    }
}
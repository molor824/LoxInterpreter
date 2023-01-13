public class Environment
{
    public Environment? ParentScope;
    public Dictionary<string, object> Variables = new();

    public Environment(Environment? parentScope = null)
    {
        ParentScope = parentScope;
    }
    public object Get(Token.Ident name)
    {
        for (var scope = this; scope != null; scope = scope.ParentScope)
        {
            if (scope.Variables.TryGetValue(name.Value, out var value)) return value;
        }

        throw new Error($"Variable {name.Value} does not exist", name.Index);
    }
    public void Declare(string name, object value, int index)
    {
        for (var scope = this; scope != null; scope = scope.ParentScope)
        {
            if (scope.Variables.ContainsKey(name))
                throw new Error($"Variable {name} is already declared", index);
        }

        Variables.Add(name, value);
    }
    public void Assign(Token.Ident name, object value)
    {
        for (var scope = this; scope != null; scope = scope.ParentScope)
        {
            if (scope.Variables.ContainsKey(name.Value))
            {
                scope.Variables[name.Value] = value;
                return;
            }
        }

        throw new Error($"Variable {name.Value} is not declared", name.Index);
    }
}
using System.Diagnostics;

static class Program
{
    public static bool Debug;

    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var path = (string?)null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--debug")
            {
                Debug = true;

                Console.WriteLine("Debug mode activated. Will print progress of the entire compilation. Expect to see huge performance drops :)");
                continue;
            }

            path = args[i];
        }

        if (path != null && !File.Exists(path))
        {
            Console.WriteLine("Invalid path");
            return;
        }

        do
        {
            Lexer lexer;
            if (path == null)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input == "exit") break;
                if (input == null) continue;
                lexer = new(input);
            }
            else lexer = Lexer.FromPath(path);

            var source = lexer.Source;
            var tokens = new List<Token>();
            var errors = new List<Error>();

            if (Debug) Console.WriteLine("\nTokens:");

            while (true)
            {
                if (lexer.Parse(out var token, out var error))
                {
                    if (token == null) break;
                    if (Debug)
                        Console.WriteLine(token);
                    tokens.Add(token);
                }
                else
                    errors.Add(error);
            }

            foreach (var e in errors)
                Console.WriteLine(e.OutputError(source, path));
            if (errors.Any()) return;

            var parser = new Parser(tokens);
            var interpreter = new Interpreter();

            try
            {
                var statements = parser.Parse();

                // cleaning up useless memory
                parser = null;
                lexer = null!;
                tokens = null;
                errors = null;

                GC.Collect();

                if (Debug)
                {
                    var printer = new AstPrinter();

                    Console.WriteLine("\nAST:");

                    foreach (var stmt in statements)
                    {
                        Console.WriteLine(stmt.Accept(printer));
                    }

                    Console.WriteLine("\nInterpretation:");
                }

                interpreter.Interpret(statements);
            }
            catch (Error e)
            {
                Console.WriteLine(e.OutputError(source, path));
            }
        } while (path == null);
    }
}
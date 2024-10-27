using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

class CodeReviewTool
{
    static void Main(string[] args)
    {
        var code = @"
            public class SampleClass {
                public void SampleMethod() {
                    int example = 10;
                    int ValidVariable = 10;
                    // int 1xample = 20;
                }
                public void print(int x) {
                    Console.Write(x);
                }
            }";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        bool flag = false;

        // Check for syntax errors (like invalid identifiers)
        var diagnostics = syntaxTree.GetDiagnostics();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                Console.WriteLine($"Syntax Error: {diagnostic.GetMessage()}");
                flag = true;
            }
        }

        // Check for naming convention violations for fields (class-level variables)
        var fieldDeclarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
        foreach (var field in fieldDeclarations)
        {
            var variableName = field.Declaration.Variables.FirstOrDefault()?.Identifier.Text;
            if (!string.IsNullOrEmpty(variableName) && !char.IsLower(variableName[0]))
            {
                flag = true;
                Console.WriteLine($"Field '{variableName}' does not follow camelCase naming convention.");
            }
        }

        // Check for naming convention violations for local variables in methods
        var variableDeclarations = root.DescendantNodes().OfType<VariableDeclarationSyntax>();
        foreach (var variable in variableDeclarations)
        {
            foreach (var variableName in variable.Variables)
            {
                var name = variableName.Identifier.Text;

                // Skip if the identifier is empty
                if (string.IsNullOrEmpty(name) || !SyntaxFacts.IsValidIdentifier(name))
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        flag = true;
                        Console.WriteLine($"Variable '{name}' is an invalid C# identifier.");
                    }
                    continue;
                }

                // Check if the name is a valid identifier and follows camelCase
                if (SyntaxFacts.IsValidIdentifier(name) && !char.IsLower(name[0]))
                {
                    flag = true;
                    Console.WriteLine($"Variable '{name}' does not follow camelCase naming convention.");
                }
                else if (!SyntaxFacts.IsValidIdentifier(name))
                {
                    flag = true;
                    Console.WriteLine($"Variable '{name}' is an invalid C# identifier.");
                }
            }
        }

        if (!flag)
        {
            Console.WriteLine("Clean Code :)");
        }
    }
}

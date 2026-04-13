using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Dotnet MCP Server started...");

        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line);

                if (request?.Method == "tools/list")
                {
                    var response = new JsonRpcResponse
                    {
                        Id = request.Id,
                        Result = new
                        {
                            tools = new[]
                            {
                                new { name = "dotnet-standards", description = "Check .NET naming conventions" },
                                new { name = "dotnet-scaffold", description = "Suggest dotnet new templates" }
                            }
                        }
                    };
                    Console.WriteLine(JsonSerializer.Serialize(response));
                }
                else if (request?.Method == "tools/call")
                {
                    var toolName = request.Params.GetProperty("name").GetString();
                    var input = request.Params.GetProperty("arguments").GetProperty("input").GetString();

                    string result = toolName switch
                    {
                        "dotnet-standards" => StandardsCheck(input),
                        "dotnet-scaffold" => ScaffoldSuggest(input),
                        _ => $"Unknown tool: {toolName}"
                    };

                    var response = new JsonRpcResponse
                    {
                        Id = request.Id,
                        Result = new { output = result }
                    };
                    Console.WriteLine(JsonSerializer.Serialize(response));
                }

            }
            catch (Exception ex)
            {
                var error = new JsonRpcResponse
                {
                    Id = null,
                    Error = new { code = -32603, message = ex.Message }
                };
                Console.WriteLine(JsonSerializer.Serialize(error));
            }
        }
    }

    static string StandardsCheck(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "No code provided.";

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var warnings = new List<string>();

        // Rule 1: Async methods must end with "Async"
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            if (method.Modifiers.Any(m => m.Text == "async") &&
                !method.Identifier.Text.EndsWith("Async"))
            {
                warnings.Add($"Warning: Async method '{method.Identifier.Text}' should end with 'Async'.");
            }
        }

        // Rule 2: Classes must be PascalCase
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var cls in classes)
        {
            var name = cls.Identifier.Text;
            if (!char.IsUpper(name[0]))
            {
                warnings.Add($"Warning: Class '{name}' should be PascalCase.");
            }
        }

        // Rule 3: Private fields must be _camelCase
        var fields = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
        foreach (var field in fields)
        {
            if (field.Modifiers.Any(m => m.Text == "private"))
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    var name = variable.Identifier.Text;
                    if (!name.StartsWith("_") || (name.Length > 1 && char.IsUpper(name[1])))
                    {
                        warnings.Add($"Warning: Private field '{name}' should follow _camelCase convention.");
                    }
                }
            }
        }

        // Rule 4: Namespaces must be PascalCase
        var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
        foreach (var ns in namespaces)
        {
            var name = ns.Name.ToString();
            if (!char.IsUpper(name[0]))
            {
                warnings.Add($"Warning: Namespace '{name}' should be PascalCase.");
            }
        }

        // Rule 5: Interfaces must start with 'I' and be PascalCase
        var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
        foreach (var iface in interfaces)
        {
            var name = iface.Identifier.Text;
            if (!name.StartsWith("I") || name.Length < 2 || !char.IsUpper(name[1]))
            {
                warnings.Add($"Warning: Interface '{name}' should start with 'I' followed by PascalCase.");
            }
        }

        // Rule 6: Properties must be PascalCase
        var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        foreach (var prop in properties)
        {
            var name = prop.Identifier.Text;
            if (!char.IsUpper(name[0]))
            {
                warnings.Add($"Warning: Property '{name}' should be PascalCase.");
            }
        }

        return warnings.Count > 0 ? string.Join("\n", warnings) : "No issues found.";
    }

    static string ScaffoldSuggest(string? template)
    {
        if (string.IsNullOrWhiteSpace(template))
            return "No template provided.";

        return template.ToLower() switch
        {
            "webapi" => "Run: dotnet new webapi -n MyApiProject",
            "console" => "Run: dotnet new console -n MyConsoleApp",
            "classlib" => "Run: dotnet new classlib -n MyLibrary",
            _ => "Available templates: webapi, console, classlib"
        };
    }
}

public class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")] public string? JsonRpc { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("method")] public string? Method { get; set; }
    [JsonPropertyName("params")] public JsonElement Params { get; set; }
}

public class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; } = "2.0";
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("result")] public object? Result { get; set; }
    [JsonPropertyName("error")] public object? Error { get; set; }
}

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LowCodeHub.Operations.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class OperationGenerator : IIncrementalGenerator
{
    private const string OperationAttributeName = "LowCodeHub.Operations.OperationAttribute";
    private const string IOperationFullName = "LowCodeHub.Operations.IOperation`2";
    private const string ErrorOrFullName = "ErrorOr.ErrorOr`1";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0 && c.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)),
                static (ctx, ct) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, ct) as INamedTypeSymbol;
                    if (symbol == null) return default((INamedTypeSymbol, AttributeData)?);
                    var attr = symbol.GetAttributes().FirstOrDefault(a =>
                        a.AttributeClass?.Name == "OperationAttribute" &&
                        (a.AttributeClass.ContainingNamespace?.ToDisplayString() == "LowCodeHub.Operations" ||
                         a.AttributeClass?.ToDisplayString()?.StartsWith("LowCodeHub.Operations.", StringComparison.Ordinal) == true));
                    if (attr == null) return default((INamedTypeSymbol, AttributeData)?);
                    return ((INamedTypeSymbol, AttributeData)?)(symbol, attr);
                })
            .Where(static x => x.HasValue)
            .Select(static (x, _) => x!.Value);

        context.RegisterSourceOutput(provider.Collect(), (spc, list) =>
        {
            foreach (var (symbol, attr) in list)
            {
                if (TryGetExecuteAsync(symbol, out var requestType, out var responseType, out var noRequest))
                {
                    var interfaceName = GetInterfaceName(symbol, attr);
                    var ns = symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString();
                    var className = symbol.Name;
                    var requestTypeName = GetShortTypeName(requestType, className);
                    var responseTypeName = GetShortTypeName(responseType, className);

                    var (interfaceSource, partialSource) = Emit(ns, className, interfaceName, requestTypeName, responseTypeName, noRequest);
                    spc.AddSource($"{interfaceName}.g.cs", SourceText.From(interfaceSource, Encoding.UTF8));
                    spc.AddSource($"{className}.Operation.g.cs", SourceText.From(partialSource, Encoding.UTF8));
                }
            }
        });
    }

    private static string GetShortTypeName(ITypeSymbol type, string operationClassName)
    {
        if (type is INamedTypeSymbol named && named.ContainingType != null)
            return named.ContainingType.Name + "." + named.Name;
        var display = type.ToDisplayString();
        var dot = display.LastIndexOf('.');
        return dot >= 0 ? display.Substring(dot + 1) : display;
    }

    private static string GetInterfaceName(INamedTypeSymbol symbol, AttributeData attr)
    {
        var nameArg = attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s ? s : null;
        if (!string.IsNullOrEmpty(nameArg)) return nameArg!;
        var className = symbol.Name;
        if (className.EndsWith("Operation", StringComparison.Ordinal))
            return "I" + className;
        return "I" + className + "Operation";
    }

    private static bool TryGetExecuteAsync(INamedTypeSymbol type, out ITypeSymbol requestType, out ITypeSymbol responseType, out bool noRequest)
    {
        requestType = null!;
        responseType = null!;
        noRequest = false;

        var method = type.GetMembers("ExecuteAsync").OfType<IMethodSymbol>().FirstOrDefault(m =>
            m.DeclaredAccessibility == Accessibility.Public && !m.IsStatic && m.Parameters.Length >= 1 && m.Parameters.Length <= 2);
        if (method == null) return false;

        if (method.ReturnType is not INamedTypeSymbol taskType || !taskType.IsGenericType) return false;
        var taskDisplay = taskType.ConstructedFrom?.ToDisplayString() ?? taskType.OriginalDefinition?.ToDisplayString() ?? "";
        if (!taskDisplay.StartsWith("System.Threading.Tasks.Task<", StringComparison.Ordinal)) return false;
        var inner = taskType.TypeArguments[0];
        if (inner is not INamedTypeSymbol errorOr || !errorOr.IsGenericType) return false;
        var errorOrDisplay = errorOr.ConstructedFrom?.ToDisplayString() ?? errorOr.OriginalDefinition?.ToDisplayString() ?? "";
        if (errorOrDisplay.IndexOf("ErrorOr", StringComparison.Ordinal) < 0) return false;
        responseType = errorOr.TypeArguments[0];

        if (method.Parameters.Length == 1)
        {
            if (method.Parameters[0].Type.ToDisplayString() != "System.Threading.CancellationToken") return false;
            noRequest = true;
            requestType = method.Parameters[0].Type; // placeholder; Emit uses "Unit" when noRequest
            return true;
        }

        if (method.Parameters.Length == 2 &&
            method.Parameters[1].Type.ToDisplayString() == "System.Threading.CancellationToken")
        {
            requestType = method.Parameters[0].Type;
            return true;
        }

        return false;
    }

    private static (string InterfaceSource, string PartialSource) Emit(
        string namespaceName,
        string className,
        string interfaceName,
        string requestTypeName,
        string responseTypeName,
        bool noRequest)
    {
        if (noRequest) requestTypeName = "Unit";
        var ns = string.IsNullOrEmpty(namespaceName) ? "" : $"namespace {namespaceName};\n\n";
        var interfaceSource = $@"// <auto-generated/>
{ns}using LowCodeHub.Operations;

/// <summary>Generated interface for {className}.</summary>
public interface {interfaceName} : IOperation<{requestTypeName}, {responseTypeName}>
{{
}}
";

        var partialSource = $@"// <auto-generated/>
{ns}using LowCodeHub.Operations;

public partial class {className} : {interfaceName}
{{
}}
";

        return (interfaceSource, partialSource);
    }
}

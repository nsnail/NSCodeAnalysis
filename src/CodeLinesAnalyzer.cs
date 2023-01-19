using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSCodeAnalysis.Languages;

namespace NSCodeAnalysis;

/// <summary>
///     代码行数分析器
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CodeLinesAnalyzer : DiagnosticAnalyzer
{
    private const int _CLASS_LINES_LIMIT  = 500;
    private const int _METHOD_LINES_LIMIT = 50;

    private static readonly DiagnosticDescriptor _rule = new(nameof(CodeLinesAnalyzer), nameof(CodeLinesAnalyzer)
                                                           , (LocalizableString)Ln.Rows_of_code_cannot_be_exceeded
                                                           , "Readability", DiagnosticSeverity.Error, true
                                                           , (LocalizableString)Ln.Code_line_number_analyzer);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(_rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(VerifyLines);
    }

    private static void Report(SyntaxTreeAnalysisContext context, SyntaxNode node, string typeName, int linesLimit)
    {
        var location   = Location.Create(context.Tree, node.Span);
        var diagnostic = Diagnostic.Create(_rule, location, typeName, linesLimit);
        context.ReportDiagnostic(diagnostic);
    }

    private static void VerifyLines(SyntaxTreeAnalysisContext context)
    {
        var members = context.Tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>();
        foreach (var member in members) {
            switch (member) {
                case MethodDeclarationSyntax method:
                    if (method.GetText().Lines.Count > _METHOD_LINES_LIMIT) {
                        Report(context, method, nameof(MethodDeclarationSyntax), _METHOD_LINES_LIMIT);
                    }

                    break;
                case ClassDeclarationSyntax @class:
                    if (@class.GetText().Lines.Count > _CLASS_LINES_LIMIT) {
                        Report(context, @class, nameof(ClassDeclarationSyntax), _CLASS_LINES_LIMIT);
                    }

                    break;
            }
        }
    }
}
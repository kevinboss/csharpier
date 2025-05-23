using System.Diagnostics;
using CSharpier.Core.DocTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core.CSharp.SyntaxPrinter.SyntaxNodePrinters;

internal static class ParenthesizedLambdaExpression
{
    public static Doc Print(ParenthesizedLambdaExpressionSyntax node, PrintingContext context)
    {
        return Doc.Concat(PrintHead(node, context), PrintBody(node, context));
    }

    public static Doc PrintHead(ParenthesizedLambdaExpressionSyntax node, PrintingContext context)
    {
        return Doc.Concat(
            AttributeLists.Print(node, node.AttributeLists, context),
            Modifiers.PrintSorted(node.Modifiers, context),
            node.ReturnType != null
                ? Doc.Concat(Node.Print(node.ReturnType, context), " ")
                : Doc.Null,
            ParameterList.Print(node.ParameterList, context),
            " ",
            Token.Print(node.ArrowToken, context)
        );
    }

    public static Doc PrintBody(ParenthesizedLambdaExpressionSyntax node, PrintingContext context)
    {
        return node.Body switch
        {
            BlockSyntax block => Doc.Concat(
                block.Statements.Count > 0 ? Doc.HardLine : " ",
                Block.Print(block, context)
            ),
            _ => Doc.Group(Doc.Indent(Doc.Line, Node.Print(node.Body, context))),
        };
    }
}

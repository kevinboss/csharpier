using CSharpier.Core.DocTypes;
using Microsoft.CodeAnalysis;

namespace CSharpier.Core.CSharp.SyntaxPrinter;

internal static class Modifiers
{
    private class DefaultOrder : IComparer<SyntaxToken>
    {
        // use the default order from https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0036
        private static readonly string[] DefaultOrdered =
        [
            "public",
            "private",
            "protected",
            "internal",
            "file",
            "static",
            "extern",
            "new",
            "virtual",
            "abstract",
            "sealed",
            "override",
            "readonly",
            "unsafe",
            "required",
            "volatile",
            "async",
        ];

        public int Compare(SyntaxToken x, SyntaxToken y)
        {
            return GetIndex(x.Text) - GetIndex(y.Text);
        }

        private static int GetIndex(string? value)
        {
            var result = Array.IndexOf(DefaultOrdered, value);
            return result == -1 ? int.MaxValue : result;
        }
    }

    private static readonly DefaultOrder Comparer = new();

    public static Doc Print(SyntaxTokenList modifiers, PrintingContext context)
    {
        if (modifiers.Count == 0)
        {
            return Doc.Null;
        }

        return Doc.Group(Doc.Join(" ", modifiers.Select(o => Token.Print(o, context))), " ");
    }

    public static Doc PrintSorted(SyntaxTokenList modifiers, PrintingContext context)
    {
        return PrintWithSortedModifiers(
            modifiers,
            context,
            sortedModifiers =>
                Doc.Group(Doc.Join(" ", sortedModifiers.Select(o => Token.Print(o, context))), " ")
        );
    }

    public static Doc PrintSorterWithoutLeadingTrivia(
        SyntaxTokenList modifiers,
        PrintingContext context
    )
    {
        return PrintWithSortedModifiers(
            modifiers,
            context,
            sortedModifiers =>
                Doc.Group(
                    Token.PrintWithoutLeadingTrivia(sortedModifiers[0], context),
                    " ",
                    sortedModifiers.Count > 1
                        ? Doc.Concat(
                            sortedModifiers
                                .Skip(1)
                                .Select(o => Token.PrintWithSuffix(o, " ", context))
                                .ToArray()
                        )
                        : Doc.Null
                )
        );
    }

    private static Doc PrintWithSortedModifiers(
        in SyntaxTokenList modifiers,
        PrintingContext context,
        Func<IReadOnlyList<SyntaxToken>, Doc> print
    )
    {
        if (modifiers.Count == 0)
        {
            return Doc.Null;
        }

        // reordering modifiers inside of #ifs can lead to code that doesn't compile
        var willReorderModifiers =
            modifiers.Count > 1
            && !modifiers.Any(o => o.LeadingTrivia.Any(p => p.IsDirective || p.IsComment()));

        var sortedModifiers = modifiers.ToArray();
        if (willReorderModifiers)
        {
            Array.Sort(sortedModifiers, Comparer);
        }

        if (willReorderModifiers && !sortedModifiers.SequenceEqual(modifiers))
        {
            context.State.ReorderedModifiers = true;
        }

        return print(sortedModifiers);
    }
}

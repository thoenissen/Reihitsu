using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class RH0324
{
    // Valid: entire chain on a single line
    void ValidSingleLine()
    {
        var a = new[] { 1, 2, 3 }.Where(x => x > 0).Select(x => x * 2).ToList();
        var b = "hello".Trim().ToUpper();
    }

    // Valid: first call inline, subsequent wrapped and aligned
    void ValidFirstInlineRestWrapped()
    {
        var a = new[] { 1, 2, 3 }.Where(x => x > 0)
                                 .Select(x => x * 2)
                                 .ToList();

        var b = "hello".Trim()
                       .ToUpper();
    }

    // Valid: all wrapped and aligned (first call also on own line)
    void ValidAllWrapped()
    {
        var a = new[] { 1, 2, 3 }
                .Where(x => x > 0)
                .Select(x => x * 2)
                .ToList();
    }

    // Valid: single member access (no chain)
    void ValidSingleAccess()
    {
        var a = "hello".Length;
    }

    // Valid: indexer transparent
    void ValidIndexer()
    {
        var a = new[] { new[] { 1 } }
                .First()[0]
                .ToString();
    }

    // Valid: nested chain in lambda (inner chain single-line)
    void ValidNestedLambda()
    {
        var a = new[] { "hello", "world" }.Where(x => x.Trim().Length > 0)
                                          .ToList();
    }

    // Valid: namespace-qualified method chain (namespace dots are not chain links)
    void ValidNamespaceQualified()
    {
        System.Linq.Enumerable.Range(0, 10)
                              .Where(x => x > 0)
                              .ToList();
    }

    // Valid: namespace-qualified method chain with continuation
    void ValidNamespaceQualifiedContinuation(CancellationTokenSource cancellationTokenSource)
    {
        System.Threading.Tasks.Task.Delay(60_000, cancellationTokenSource.Token)
                                   .ContinueWith(obj => { }, cancellationTokenSource.Token);
    }

    // Valid: property access before method chain (property dots are not chain links)
    void ValidPropertyAccess()
    {
        var a = DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture)
                                 .Trim();
    }

    // Valid: multiple property accesses before method chain
    void ValidMultiplePropertyAccess()
    {
        var a = DateTime.Now.Date.TimeOfDay.ToString()
                                           .Trim();
    }

    // Invalid: dots not aligned (misalignment)
    void InvalidMisaligned()
    {
        var a = new[] { 1, 2, 3 }.Where(x => x > 0)
                                 .Select(x => x * 2)
                                     {|#0:.|}ToList();
    }

    // Invalid: wrapped calls not aligned with first dot
    void InvalidNotAlignedWithFirst()
    {
        var b = new[] { 1, 2, 3 }.Where(x => x > 0)
            {|#1:.|}OrderBy(x => x)
            {|#2:.|}ToList();
    }

    // Invalid: one outlier dot
    void InvalidOutlier()
    {
        var d = "hello".Trim()
                       .ToUpper()
                         {|#3:.|}ToString();
    }

    // Invalid: middle call not wrapped when subsequent wraps
    void InvalidMiddleNotWrapped()
    {
        var e = new[] { 1, 2, 3 }.Where(x => x > 0){|#4:.|}Select(x => x)
                                 .ToList();
    }

    // Valid: null-forgiving operator in chain (aligned)
    void ValidNullForgiving()
    {
        var a = default(string[])!.Where(x => x.Length > 0)
                                 .Select(x => x.Trim())
                                 .ToList();
    }

    // Invalid: null-forgiving operator in chain (misaligned)
    void InvalidNullForgiving()
    {
        var a = default(string[])!.Where(x => x.Length > 0)
                                  {|#5:.|}Select(x => x.Trim())
                                  {|#6:.|}ToList();
    }
}

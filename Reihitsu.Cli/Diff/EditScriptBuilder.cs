using System.Collections.Generic;

using Reihitsu.Cli.Enumerations;

namespace Reihitsu.Cli.Diff;

/// <summary>
/// Builds an edit script (sequence of edit operations) from an LCS table
/// </summary>
internal static class EditScriptBuilder
{
    #region Constants

    /// <summary>
    /// The maximum number of cells the LCS table is allowed to occupy before the changed
    /// region is emitted as a delete-then-insert fallback that keeps memory bounded
    /// </summary>
    private const long MaximumTableCellCount = 4_000_000;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Computes an edit script from the original and formatted lines
    /// </summary>
    /// <param name="originalLines">The original lines</param>
    /// <param name="formattedLines">The formatted lines</param>
    /// <returns>A list of edit operations describing the differences</returns>
    public static List<EditOperation> Build(string[] originalLines, string[] formattedLines)
    {
        var originalLength = originalLines.Length;
        var formattedLength = formattedLines.Length;

        // Trim the common prefix and suffix before computing the LCS. Formatting diffs are
        // localized, so this keeps the (otherwise O(n×m)) table proportional to the changed
        // region rather than the whole file, avoiding huge allocations on large inputs.
        var prefixLength = ComputeCommonPrefixLength(originalLines, formattedLines);
        var suffixLength = ComputeCommonSuffixLength(originalLines, formattedLines, prefixLength);

        var operations = new List<EditOperation>(originalLength + formattedLength);

        for (var index = 0; index < prefixLength; index++)
        {
            operations.Add(new EditOperation(EditKind.Equal, index, index));
        }

        AppendChangedRegion(operations, originalLines, formattedLines, prefixLength, suffixLength);

        for (var offset = suffixLength; offset > 0; offset--)
        {
            operations.Add(new EditOperation(EditKind.Equal, originalLength - offset, formattedLength - offset));
        }

        return operations;
    }

    /// <summary>
    /// Computes the number of leading lines that are identical in both inputs
    /// </summary>
    /// <param name="originalLines">The original lines</param>
    /// <param name="formattedLines">The formatted lines</param>
    /// <returns>The length of the common prefix</returns>
    private static int ComputeCommonPrefixLength(string[] originalLines, string[] formattedLines)
    {
        var maximum = Math.Min(originalLines.Length, formattedLines.Length);
        var length = 0;

        while (length < maximum && string.Equals(originalLines[length], formattedLines[length], StringComparison.Ordinal))
        {
            length++;
        }

        return length;
    }

    /// <summary>
    /// Computes the number of trailing lines that are identical in both inputs, without overlapping the common prefix
    /// </summary>
    /// <param name="originalLines">The original lines</param>
    /// <param name="formattedLines">The formatted lines</param>
    /// <param name="prefixLength">The length of the common prefix already consumed</param>
    /// <returns>The length of the common suffix</returns>
    private static int ComputeCommonSuffixLength(string[] originalLines, string[] formattedLines, int prefixLength)
    {
        var maximum = Math.Min(originalLines.Length, formattedLines.Length) - prefixLength;
        var length = 0;

        while (length < maximum)
        {
            if (string.Equals(originalLines[originalLines.Length - 1 - length], formattedLines[formattedLines.Length - 1 - length], StringComparison.Ordinal) == false)
            {
                break;
            }

            length++;
        }

        return length;
    }

    /// <summary>
    /// Appends the edit operations for the changed region between the common prefix and suffix
    /// </summary>
    /// <param name="operations">The list to append operations to</param>
    /// <param name="originalLines">The original lines</param>
    /// <param name="formattedLines">The formatted lines</param>
    /// <param name="prefixLength">The length of the common prefix</param>
    /// <param name="suffixLength">The length of the common suffix</param>
    private static void AppendChangedRegion(List<EditOperation> operations, string[] originalLines, string[] formattedLines, int prefixLength, int suffixLength)
    {
        var originalCount = originalLines.Length - suffixLength - prefixLength;
        var formattedCount = formattedLines.Length - suffixLength - prefixLength;

        if (originalCount == 0 && formattedCount == 0)
        {
            return;
        }

        if (originalCount == 0 || formattedCount == 0 || (long)originalCount * formattedCount > MaximumTableCellCount)
        {
            // Either one side is empty (pure insert/delete) or the changed region is so large that the
            // LCS table would be unbounded. Emit deletes followed by inserts: not necessarily minimal,
            // but a correct diff produced with bounded memory.
            AppendDeletesThenInserts(operations, prefixLength, originalCount, formattedCount);

            return;
        }

        var originalRegion = new string[originalCount];
        var formattedRegion = new string[formattedCount];

        Array.Copy(originalLines, prefixLength, originalRegion, 0, originalCount);
        Array.Copy(formattedLines, prefixLength, formattedRegion, 0, formattedCount);

        AppendLcsBacktrack(operations, originalRegion, formattedRegion, prefixLength);
    }

    /// <summary>
    /// Appends the changed region as all deletions followed by all insertions
    /// </summary>
    /// <param name="operations">The list to append operations to</param>
    /// <param name="prefixLength">The length of the common prefix, used to offset indices back into the full arrays</param>
    /// <param name="originalCount">The number of original lines in the changed region</param>
    /// <param name="formattedCount">The number of formatted lines in the changed region</param>
    private static void AppendDeletesThenInserts(List<EditOperation> operations, int prefixLength, int originalCount, int formattedCount)
    {
        for (var index = 0; index < originalCount; index++)
        {
            operations.Add(new EditOperation(EditKind.Delete, prefixLength + index, -1));
        }

        for (var index = 0; index < formattedCount; index++)
        {
            operations.Add(new EditOperation(EditKind.Insert, -1, prefixLength + index));
        }
    }

    /// <summary>
    /// Computes the LCS table for the changed region and backtracks it into edit operations
    /// </summary>
    /// <param name="operations">The list to append operations to</param>
    /// <param name="originalRegion">The original lines of the changed region</param>
    /// <param name="formattedRegion">The formatted lines of the changed region</param>
    /// <param name="offset">The offset to add to indices so they refer back into the full arrays</param>
    private static void AppendLcsBacktrack(List<EditOperation> operations, string[] originalRegion, string[] formattedRegion, int offset)
    {
        var table = LcsComputer.ComputeTable(originalRegion, formattedRegion);
        var changes = new List<EditOperation>();
        var originalPosition = originalRegion.Length;
        var formattedPosition = formattedRegion.Length;

        while (originalPosition > 0 || formattedPosition > 0)
        {
            if (originalPosition > 0 && formattedPosition > 0 && string.Equals(originalRegion[originalPosition - 1], formattedRegion[formattedPosition - 1], StringComparison.Ordinal))
            {
                changes.Add(new EditOperation(EditKind.Equal, offset + originalPosition - 1, offset + formattedPosition - 1));

                originalPosition--;
                formattedPosition--;
            }
            else if (formattedPosition > 0 && (originalPosition == 0 || table[originalPosition, formattedPosition - 1] >= table[originalPosition - 1, formattedPosition]))
            {
                changes.Add(new EditOperation(EditKind.Insert, -1, offset + formattedPosition - 1));

                formattedPosition--;
            }
            else
            {
                changes.Add(new EditOperation(EditKind.Delete, offset + originalPosition - 1, -1));

                originalPosition--;
            }
        }

        changes.Reverse();
        operations.AddRange(changes);
    }

    #endregion // Methods
}
using System.Collections.Generic;

namespace Reihitsu.Cli.Diff;

/// <summary>
/// Builds an edit script (sequence of edit operations) from an LCS table
/// </summary>
internal static class EditScriptBuilder
{
    #region Methods

    /// <summary>
    /// Computes an edit script from the LCS table
    /// </summary>
    /// <param name="originalLines">The original lines</param>
    /// <param name="formattedLines">The formatted lines</param>
    /// <returns>A list of edit operations describing the differences</returns>
    public static List<EditOperation> Build(string[] originalLines, string[] formattedLines)
    {
        var table = LcsComputer.ComputeTable(originalLines, formattedLines);
        var operations = new List<EditOperation>();
        var originalPosition = originalLines.Length;
        var formattedPosition = formattedLines.Length;

        while (originalPosition > 0 || formattedPosition > 0)
        {
            if (originalPosition > 0 && formattedPosition > 0 && string.Equals(originalLines[originalPosition - 1], formattedLines[formattedPosition - 1], StringComparison.Ordinal))
            {
                operations.Add(new EditOperation(EditKind.Equal, originalPosition - 1, formattedPosition - 1));

                originalPosition--;
                formattedPosition--;
            }
            else if (formattedPosition > 0 && (originalPosition == 0 || table[originalPosition, formattedPosition - 1] >= table[originalPosition - 1, formattedPosition]))
            {
                operations.Add(new EditOperation(EditKind.Insert, -1, formattedPosition - 1));

                formattedPosition--;
            }
            else
            {
                operations.Add(new EditOperation(EditKind.Delete, originalPosition - 1, -1));

                originalPosition--;
            }
        }

        operations.Reverse();

        return operations;
    }

    #endregion // Methods
}
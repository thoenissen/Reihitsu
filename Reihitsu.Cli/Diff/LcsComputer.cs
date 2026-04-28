namespace Reihitsu.Cli.Diff;

/// <summary>
/// Computes the Longest Common Subsequence (LCS) table for two line arrays
/// </summary>
internal static class LcsComputer
{
    #region Methods

    /// <summary>
    /// Computes the LCS length table
    /// </summary>
    /// <param name="originalLines">The original lines</param>
    /// <param name="formattedLines">The formatted lines</param>
    /// <returns>A two-dimensional LCS length table</returns>
    public static int[,] ComputeTable(string[] originalLines, string[] formattedLines)
    {
        var originalLength = originalLines.Length;
        var formattedLength = formattedLines.Length;
        var table = new int[originalLength + 1, formattedLength + 1];

        for (var row = 1; row <= originalLength; row++)
        {
            for (var column = 1; column <= formattedLength; column++)
            {
                if (string.Equals(originalLines[row - 1], formattedLines[column - 1], StringComparison.Ordinal))
                {
                    table[row, column] = table[row - 1, column - 1] + 1;
                }
                else
                {
                    table[row, column] = Math.Max(table[row - 1, column], table[row, column - 1]);
                }
            }
        }

        return table;
    }

    #endregion // Methods
}
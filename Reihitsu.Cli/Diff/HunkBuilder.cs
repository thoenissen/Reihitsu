using System.Collections.Generic;

namespace Reihitsu.Cli.Diff;

/// <summary>
/// Groups edit operations into diff hunks with surrounding context lines.
/// </summary>
internal static class HunkBuilder
{
    #region Constants

    /// <summary>
    /// The number of unchanged context lines to include around each change hunk.
    /// </summary>
    public const int ContextLines = 3;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Groups edit operations into hunks.
    /// </summary>
    /// <param name="operations">The edit operations.</param>
    /// <param name="originalLineCount">The total number of original lines.</param>
    /// <param name="formattedLineCount">The total number of formatted lines.</param>
    /// <returns>A list of diff hunks.</returns>
    public static List<DiffHunk> Build(List<EditOperation> operations, int originalLineCount, int formattedLineCount)
    {
        var changeIndices = new List<int>();

        for (var operationIndex = 0; operationIndex < operations.Count; operationIndex++)
        {
            if (operations[operationIndex].Kind != EditKind.Equal)
            {
                changeIndices.Add(operationIndex);
            }
        }

        if (changeIndices.Count == 0)
        {
            return [];
        }

        var hunks = new List<DiffHunk>();
        var currentStart = Math.Max(0, changeIndices[0] - ContextLines);
        var currentEnd = Math.Min(operations.Count - 1, changeIndices[0] + ContextLines);

        for (var changePosition = 1; changePosition < changeIndices.Count; changePosition++)
        {
            var nextStart = Math.Max(0, changeIndices[changePosition] - ContextLines);
            var nextEnd = Math.Min(operations.Count - 1, changeIndices[changePosition] + ContextLines);

            if (nextStart > currentEnd + 1)
            {
                hunks.Add(CreateHunk(operations, currentStart, currentEnd));

                currentStart = nextStart;
            }

            currentEnd = nextEnd;
        }

        hunks.Add(CreateHunk(operations, currentStart, currentEnd));

        return hunks;
    }

    /// <summary>
    /// Creates a single diff hunk from a range of operations.
    /// </summary>
    /// <param name="operations">The full list of edit operations.</param>
    /// <param name="start">The start index in the operations list.</param>
    /// <param name="end">The end index in the operations list.</param>
    /// <returns>A <see cref="DiffHunk"/> representing the range.</returns>
    private static DiffHunk CreateHunk(List<EditOperation> operations, int start, int end)
    {
        var originalStart = -1;
        var formattedStart = -1;
        var originalCount = 0;
        var formattedCount = 0;
        var hunkOperations = new List<EditOperation>();

        for (var operationIndex = start; operationIndex <= end; operationIndex++)
        {
            var operation = operations[operationIndex];

            hunkOperations.Add(operation);

            switch (operation.Kind)
            {
                case EditKind.Equal:
                    {
                        if (originalStart == -1)
                        {
                            originalStart = operation.OriginalIndex;
                            formattedStart = operation.FormattedIndex;
                        }

                        originalCount++;
                        formattedCount++;
                    }
                    break;

                case EditKind.Delete:
                    {
                        if (originalStart == -1)
                        {
                            originalStart = operation.OriginalIndex;
                            formattedStart = FindFormattedStart(operations, operationIndex);
                        }

                        originalCount++;
                    }
                    break;

                case EditKind.Insert:
                    {
                        if (originalStart == -1)
                        {
                            originalStart = FindOriginalStart(operations, operationIndex);
                            formattedStart = operation.FormattedIndex;
                        }

                        formattedCount++;
                    }
                    break;
            }
        }

        if (originalStart == -1)
        {
            originalStart = 0;
        }

        if (formattedStart == -1)
        {
            formattedStart = 0;
        }

        return new DiffHunk(originalStart, originalCount, formattedStart, formattedCount, hunkOperations);
    }

    /// <summary>
    /// Finds the formatted line start index by scanning nearby operations.
    /// </summary>
    /// <param name="operations">The full list of edit operations.</param>
    /// <param name="currentIndex">The current index in the operations list.</param>
    /// <returns>The formatted line index, or 0 if none found.</returns>
    private static int FindFormattedStart(List<EditOperation> operations, int currentIndex)
    {
        for (var searchIndex = currentIndex + 1; searchIndex < operations.Count; searchIndex++)
        {
            if (operations[searchIndex].FormattedIndex >= 0)
            {
                return operations[searchIndex].FormattedIndex;
            }
        }

        for (var searchIndex = currentIndex - 1; searchIndex >= 0; searchIndex--)
        {
            if (operations[searchIndex].FormattedIndex >= 0)
            {
                return operations[searchIndex].FormattedIndex + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Finds the original line start index by scanning nearby operations.
    /// </summary>
    /// <param name="operations">The full list of edit operations.</param>
    /// <param name="currentIndex">The current index in the operations list.</param>
    /// <returns>The original line index, or 0 if none found.</returns>
    private static int FindOriginalStart(List<EditOperation> operations, int currentIndex)
    {
        for (var searchIndex = currentIndex + 1; searchIndex < operations.Count; searchIndex++)
        {
            if (operations[searchIndex].OriginalIndex >= 0)
            {
                return operations[searchIndex].OriginalIndex;
            }
        }

        for (var searchIndex = currentIndex - 1; searchIndex >= 0; searchIndex--)
        {
            if (operations[searchIndex].OriginalIndex >= 0)
            {
                return operations[searchIndex].OriginalIndex + 1;
            }
        }

        return 0;
    }

    #endregion // Methods
}
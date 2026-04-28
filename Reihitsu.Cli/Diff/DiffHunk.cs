using System.Collections.Generic;

namespace Reihitsu.Cli.Diff;

/// <summary>
/// Represents a contiguous hunk in a unified diff
/// </summary>
/// <param name="OriginalStart">The starting line index in the original content</param>
/// <param name="OriginalCount">The number of lines from the original content</param>
/// <param name="FormattedStart">The starting line index in the formatted content</param>
/// <param name="FormattedCount">The number of lines from the formatted content</param>
/// <param name="Operations">The edit operations within this hunk</param>
internal readonly record struct DiffHunk(int OriginalStart, int OriginalCount, int FormattedStart, int FormattedCount, List<EditOperation> Operations);
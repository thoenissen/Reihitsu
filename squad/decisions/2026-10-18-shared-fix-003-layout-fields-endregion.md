# Layout inside comment text, which types field order covers, and the matching #endregion comment

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 1 |
| Run | issue #1068 |

## Question

Three questions the spec author raised after the SHARED-FIX-003 revision:

A. Does "layout does not count" cover whitespace and line breaks inside the text of a `//` or `/* */` comment or a directive (RH5011 inserts a line inside `/* a⏎b */`)? Options: layout means only the whitespace around a comment and between its lines; or any whitespace change, inside the text too.
B. Which types does "field order determines the layout" cover? Options: every type whose field order actually sets its layout (structs by default, classes with sequential layout; not explicit layout); only types with an explicit sequential `[StructLayout]`; or every struct and every `[StructLayout]` type, explicit included.
C. RH7302 renames `#region members` and also rewrites the matching `#endregion // members` comment, which its diagnostic does not report. Options: part of the rename; or a bug.

## Answer

A. Layout means the whitespace around a comment and the indentation between its lines. Whitespace and line breaks inside the text of a comment or directive are content; changing them counts.
B. Every type whose field order actually sets its layout: structs (sequential by default) and types with sequential layout. Not `LayoutKind.Explicit`, where `[FieldOffset]` sets the offsets.
C. The `#endregion` comment that repeats the region's name is renamed together with the region; that is part of the rename.

## Reason

none given

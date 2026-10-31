Role: cleaner
Commit: see the commit that adds this file (comment edit on top of 6e44a70)
Verdict: clean

## Comment fix (audit finding 10)
`RawStringLineShifts.ApplyEdits` summary now also names the edit at the token's end. Derived from the code: `LineStart > token.SpanStart` includes `LineStart == token.Span.End`, `ReplaceIndentation` then replaces `OldLength` 0 characters at the end of the text, so it appends the moved indentation of the next line, which starts with a hole's opening brace.

## Formatted
_None._ `scripts/format.sh` on the two changed C# files (`RawStringMovedLineStartTests.cs`, `RawStringLineShifts.cs`): `Formatted 0 of 2 file(s).` Its build step printed S1125 warnings for the unchanged `src/Reihitsu.Cli/FormatCommandHandler.cs` (lines 430, 441, 452), which the Release build below does not report.

## Warnings resolved
_None._

## Unresolved
_None._

## Checks
- `scripts/verify-text-only.sh --strict-docs --base HEAD --head worktree` (HEAD = 6e44a70, before the edit): `TEXT-ONLY PROOF: PASS — base=6e44a70 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), no public API documentation`
- `scripts/build.sh`: `Build succeeded. 0 Warning(s) 0 Error(s)`
- No focused tests run: the only change since the tested commit is a proven text-only comment.

Role: cleaner
Commit: see the commit that adds this file
Verdict: clean

## Formatted
_None._ `scripts/format.sh` reported "Formatted 0 of 4 file(s)" over the run's changed C# files, including `ReihitsuFormatter.cs`. (Its own CLI build printed an S1125 note for `src/Reihitsu.Cli/FormatCommandHandler.cs`, a file this run does not change; the Release build below has no warnings.)

## Remarks correction (repair round 1, finding #1 part b)
The `<remarks>` of `ReihitsuFormatter.FormatNode` no longer say that a detached node carries the default parse options. They now say that version-dependent rules follow the language version of the tree Roslyn reports as the node's syntax tree, whether or not the node has a parent, and that a node reporting a tree with the default parse options is formatted for the newest supported version. This matches FMT-PIPE-008. Two comment lines changed; nothing else in the file.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| _None._ | | |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/verify-text-only.sh --base HEAD --head worktree`: `TEXT-ONLY PROOF: PASS — base=02420ce head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), including public API documentation`. Run without `--strict-docs`, since public API documentation changing is the point of the edit. Per file: 1446 tokens identical, changed trivia: documentation.
- No focused tests run: no compiled change.
- Lane: the maintainer accepted that the comment edit may fail the tooling lane check (escalation 7); this is the state to report.

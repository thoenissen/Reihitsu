Role: implementer
Commit: (this commit; parent 5672b19)
Verdict: tests pass

Repair round 1, decisions 24 and 25 in `00-lead.md` (auditor findings #2 and #1 in `10-auditor.md`), RH8402-006 as widened in `11-spec-author.md`, tests from `12-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the work items.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs` | `GetLeadingHeaderSpan` takes whether the header ends with a line break. It now also tracks the end of the line break that ends the run's last comment's line (`commentRunLineEnd`); on the replace path a header ending with a line break replaces through that line break, so exactly one line break stays there. Nothing changes for a header without a trailing line break, for code behind the comment on its line (no line break to take), or on the insert and no-comment paths. The trailing-break check is one helper, `EndsWithLineBreak`, shared with `EnsureHeaderIsSeparatedFromCode`. Summary and inline comments updated | RH8402-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8204DoNotUsePlaceholderElementsCodeFixProvider.cs` | Deleted the unreachable textual fallback (`default:` branch) and `StripPlaceholderTags`. `ExtractInnerContent` returns the source text between the tags for an `XmlElementSyntax` and the empty string otherwise (the analyzer reports only an element or an empty element, whose node `FindNode` returns for that exact span); the remarks say so | RH8204-003 (finding #1, decision 25) |

## Policy owners

- Header ends with a line break: `rg -n 'EndsWith\("\\\\[nr]"' src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402*` — before the change the check was inline in `EnsureHeaderIsSeparatedFromCode` only; it is now `EndsWithLineBreak`, used by both call sites. One owner.
- Old-header span: `GetLeadingHeaderSpan` only (as the auditor found).
- Placeholder content extraction: `ExtractInnerContent` only; `rg -n StripPlaceholderTags src` finds nothing after the change.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (at parent 5672b19): 16 files changed, 480 insertions(+), 226 deletions(-). Delta of this round: 2 files changed, 44 insertions(+), 58 deletions(-). No new type.

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests|FullyQualifiedName~RH8204"` — Passed! Failed: 0, Passed: 71 (the five failing `FixWithHeaderEndingInLineBreak…` rows now pass).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 8098.
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 117.
- `scripts/format.sh` over both files: formatted 0 of 2.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

- RH8402 insert path (non-copyright leading comment) with a file that starts with a line break and a header ending in one: the empty span keeps the leading line break behind the header. RH8402-005, outside this finding; not probed.

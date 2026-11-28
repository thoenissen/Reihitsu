Role: spec author
Commit: 377862b
Verdict: change doc written

The prompt carried no intended fix or verdict beyond pointing at the maintainer's answer in `00-lead.md` (option A), which is binding and is what the spec implements.

## Changed requirements

- SHARED-USING-009 — Blocks with preprocessor directives among their directives are not reordered — changed. The boundary is stated by cause: a preprocessor directive makes the block unsafe when it stands in the leading trivia of any token of the block other than the first token of its first directive. A directive in the leading trivia of that token stands in front of the block and does not make it unsafe. The title changed; the ID stays. The old example ("a directive in front makes the block unsafe") is replaced by its opposite, with fix output and the RH7207 blank-line consequence. Added: a conditional group in front of the block (non-DEBUG build: safe; DEBUG build: unsafe), and a directive between the tokens of the *first* directive (`using` / `#pragma` / `System.Linq;`), which stays unsafe. That is the other side of the qualifier "first token of the first directive" (FC-002). `Uses` gains FMT-USING-012.
- FMT-USING-004 — Unsafe blocks only get one directive per line — example changed. The old example (`#nullable enable` header) is now a safe block, so it is replaced by a `#pragma` between the directives. The normative text is unchanged.
- FMT-USING-012 — Preprocessor directives in front of the block stay at the top — added, ID inserted in `requirement-ids.txt`. The trivia through the last preprocessor directive in the original first directive's leading trivia stays at the top. FMT-USING-009's blank-line split applies only to the trivia behind that directive.
- RH7201-001 to RH7207-001 — clarified wording only: "a block with a preprocessor directive *between its directives* is reported as well, without a code fix". RH7207-001 also gains an example: a header-directive block in canonical order without its group blank line is now reported.
- RH7201-004 to RH7207-004 — changed. The fix keeps every preprocessor directive in front of the block at its top (FMT-USING-012). The "no fix" sentence now names the unsafe shapes, and a new sentence says a block whose directives all stand in front of it is fixed. Each spec gets one example (`#nullable enable` header, rule-specific block) and FMT-USING-012 in `Uses`.
- Change doc: `sdd/changes/2026-11/using-header-directive-reorder.md`, `Status: Draft`, depends on `using-block-directive-report.md`.

Rule pages (`documentation/rules/RH7201.md` to `RH7207.md`, `general-notes.md`) are unchanged. What the rules report and how to fix a block in the normal case do not change, and general notes already cover a fix withheld for a directive.

## Examples for the tests

- SHARED-USING-009: every example. The new ones are the header reorder, the header plus missing group blank line (RH7207 reported, fix and formatter add it), the conditional group in front (both builds), and the directive between the tokens of the first directive (still unsafe).
- FMT-USING-012: all five examples: conditional group, directive behind a header's blank line, comment directly below the directive, comment behind a blank line below the directive, and `#region` in front (formatter output vs. FMT-PIPE-030 output). Nothing covers FMT-USING-012 yet, and that is the only failure of `scripts/test.sh --project architecture` (`RequirementCoverageTests`); the test implementer tags it.
- FMT-USING-004: the new `#pragma` example.
- RH7201-004 … RH7207-004: the new header example at the end of each spec, which is the seed for the per-rule fix and Fix All tests. RH7207-001: the new reported header block.
- Not this role's to write, noted for the test implementer: `tests/Reihitsu.Core.Test/UsingDirectiveOrderingSafetyTests.cs` and the existing tests that assert "header directive → no fix / formatter keeps" (the tests tagged SHARED-USING-009 or FMT-USING-004, in the RH7201–RH7207 analyzer tests, `RH7207UsingDirectivesShouldBeOrganizedIntoGroupsFormatterTests.cs`, `UsingDirectiveOrderingTests.cs`, `UsingDirectiveCommentTextTests.cs`, and `UsingDirectiveRequirementTests.cs`) will now be wrong and must be updated, not just added to.

Checks run on `main` code (377862b):
- `scripts/format.sh` on every input of the change doc: today the block is kept as written in every case, and the `#region` case only gets framed. The expected after-outputs of SHARED-USING-009 and FMT-USING-012 are all stable under today's formatter, and the FMT-USING-004 example is today's output.
- `scripts/apply-fix.sh RH7203` and `RH7207` on cases 1, 2, 3, 7: before is `no-fix-offered` and, for case 2, `no-diagnostic`.
- `scripts/apply-fix.sh RH7201` … `RH7207` on the headerless form of each new -004 example: the result equals the expected after minus the header line.

## Output questions

The maintainer's answer (option A) covers the issue's case, change-doc case 1. The change doc lists six more visible cases that follow from it. The lead decides whether each is covered by the answer or needs asking. Two of them have a competing reading with different output:

**Q1 · RH7201–RH7207 fixes and formatter: comment directly below a header directive (change doc case 5, FMT-USING-012)**

Input:
```csharp
#nullable enable
// About Linq
using System.Linq;
using System;
```
Before: reported, no fix, formatter keeps it.
After A (recommended, written into the spec):
```csharp
#nullable enable
// About Linq
using System;
using System.Linq;
```
After B:
```csharp
#nullable enable
using System;
// About Linq
using System.Linq;
```
Why A: FMT-USING-009 keeps a comment with no blank line below a header at the top, as it does today for `// About Linq` directly above the first using without a directive. A directive above the comment separates nothing the way a blank line does. B would add a second split criterion.

**Q2 · RH7201–RH7207 fixes and formatter: conditional group in front of the block (change doc case 7, SHARED-USING-009)**

Input, analyzed and formatted in a build without `DEBUG` (the CLI's build):
```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System;
```
Before: reported, no fix, formatter keeps it.
After A (recommended, written into the spec):
```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System;
using System.Linq;
```
After B: unchanged, no fix offered. Disabled text in front of the block would count as unsafe, which is also a cheap check.
Why A: the whole group stands in front of the build's first using, the reorder moves no using across a directive in any build, and in a `DEBUG` build the block stays unsafe and unfixed as before.

Cases 2, 3, 4, and 6 have one reading under the answer and existing requirements: RH7207's blank-line check, the region framing of FMT-BLANK-013, and FMT-USING-009/010.

## Internal choices

- The header rule is a new requirement FMT-USING-012 rather than an amendment of FMT-USING-009, so tests can tag it separately. Alternative: widen FMT-USING-009.
- One header example is added to each of the seven rule specs' -004. Alternative: examples only in RH7203 and RH7207, with the rest deferring to SHARED-USING-009.
- FMT-USING-010 and FMT-USING-011 are not edited. FMT-USING-012 states that the trivia kept at the top is the FMT-USING-009 header those requirements already treat.

## Follow-up candidates

_None._

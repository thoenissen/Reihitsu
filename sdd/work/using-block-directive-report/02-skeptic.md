Role: skeptic
Commit: 28111dd
Verdict: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at the inputs.

The change is a pure output decision, the issue's own A/B question, so the verdict is `output question`. Nothing found argues for `do not make the change` or a narrower change: option A matches the precedent the maintainer already set in `report-across-conditional-directives.md`. Below are three corrections to the spec author's question set (Q4 is already decided, Q2-B has no coherent reason, and the "Why A" rests on the wrong basis). There are also two cases the maintainer should see before answering, because they show what A costs on code people commonly write.

## Premise

- **Real, and re-observed.** On `main` behavior (tree at 28111dd, production code unchanged), `scripts/apply-fix.sh RH7201|RH7203|RH7207 <dir>` reports `no-diagnostic` under LF and CRLF for the issue's input. It does the same for the trailing conditional group with `#define DEBUG` at the top (fixtures below). The issue says its "before" came from the spec, not from running the tool. It is now confirmed.
- **Mechanism of today's silence.** `UsingDirectiveOrderingSafety.CanSafelyReorder` (`src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs:23`) returns `usingDirectives.Any(u => u.ContainsDirectives) == false`. Each of the seven analyzers returns early when it is false: RH7201 `:49`, RH7202 `:49`, RH7203 `:50`, RH7204 `:50`, RH7205 `:122`, RH7206 `:50`, RH7207 `:99`. The same helper guards the formatter (`UsingDirectiveOrderingRewriter.cs:76`), both fix providers (`UsingDirectiveOrderingCodeFixProviderBase.cs:89`, `RH7207...CodeFixProvider.cs:68`), which register no action, and `UsingDirectiveCodeFixUtilities.cs:31`. So the "no fix offered" half of the change already holds in code.
- **Caller.** The analyzers themselves, on any compilation (IDE or build). Every input below is a whole document.
- **Self-hosting.** No `.cs` file of the repository has a preprocessor directive in its own using block. Every hit of a BOM-tolerant search is inside a test's raw-string input. So `Reihitsu.SelfHosting.Test` is not affected.

### Fixtures (scratch, deleted)

`issue.cs`: the issue's input. `trailing_debug.cs`:
```csharp
#define DEBUG
using System;
using Microsoft.Win32;
#if DEBUG
using System.Diagnostics;
#endif

class C
{
}
```
`trailing_nodebug.cs`: the same without `#define DEBUG`. Results: RH7201 and RH7203 gave `no-diagnostic` for all three. RH7207 gave `no-diagnostic` for `issue.cs` and `trailing_debug.cs`, and `fixed` for `trailing_nodebug.cs`, where the `#if` group is the leading trivia of `class C`, so the block is safe. The fix adds the blank line in front of `Microsoft.Win32`.

## Mechanism and candidates

_N/A._ This is a behavior change, not a bug. The analyzers register on `SyntaxKind.CompilationUnit`, `NamespaceDeclaration`, and `FileScopedNamespaceDeclaration`, so those are the three scopes a block can sit in.

## Breaking inputs

Failure classes that reach the change:

- **FC-020**: directive kind × position. The candidate set comes from `ContainsDirectives`, which covers every directive trivia.
- **FC-003**: RH7207's new boundary, where order is checked and blank lines are not.
- **FC-009 / FC-008**: no action is registered while the diagnostic is reported, and Fix All runs over a document with one safe and one unsafe block.
- **FC-013**: settled under *Output questions*, Q4.
- **FC-014**: using order is named there as a concern the formatter judges. See *Refuse option*.

FC-044 does not reach the change: the analyzers judge the compiled build, and the fix guard stays a cheap trivia check.

Matrix, axis by axis. "Pinned" names an existing test that covers the cell today with the old expectation, which then flips (the spec author's list). "None found" means a new test is needed.

| Axis | Members | Existing test |
|---|---|---|
| Directive kind | `#if/#endif` with the branch compiled; same, branch excluded; `#if false`; `#if/#else` alternatives; `#nullable`; `#pragma`; `#region/#endregion`; `#define` (file top only) | `#if` excluded: `DisabledConditionalUsingBlocksAreNotReported...` (all 7). `#if` mid-block: `BlockWithPreprocessorDirectiveIsNotReported` (all 7). `#region`: RH7207 `NoDiagnosticWhenRegionDirectivePreventsSafeReordering`. `#nullable`: RH7207 `BlockLedByANullableDirectiveIsNotReported`. `#pragma` between tokens: RH7203 `PragmaBetweenTokensOfDirectiveIsNotReported`. `#if` compiled, `#if/#else`, `#if false`, `#define`: none found |
| Position | leading trivia of the first using; between two usings; between the tokens of one using; across the tokens of two usings; after the last using (safe, must stay reported **and fixed**) | between tokens: RH7203 `ConditionalGroupBetweenTokensOfDirectiveIsNotReported`, `ConditionalGroupAcrossTokensOfTwoDirectivesIsNotReported`. After the block: formatter `FormatterOrganizesABlockFollowedByAConditionalDirective`, and the SHARED-USING-009 example. Analyzer + fix after the block: none found |
| Build | the symbol defined / not defined (tests can pass symbols, e.g. `GetCodeFixActionsAsync(..., "DEBUG")` in RH7201 `NoCodeFixWhenUsingDirectivesCannotBeSafelyReordered`) | DEBUG-defined report: none found |
| Scope | compilation unit; block-scoped namespace; file-scoped namespace; `global using` section next to the local one | none found for unsafe blocks |
| Rule | RH7201 to RH7207 | see above |
| RH7207 aspect | order wrong, blank lines right; order right, group blank line missing (not reported); order right, extra blank line inside a group (not reported); both wrong (one diagnostic) | none found for the not-reported side |
| Fix | no action registered for a reported diagnostic in each of the 7 rules; Fix All over a document with an unsafe compilation-unit block and a safe namespace block fixes only the latter | `NoCodeFixWhenUsingDirectivesCannotBeSafelyReordered` (RH7201), `NoCodeFixWhenConditionalDirectiveWouldBeMoved` (RH7207). Others and Fix All: none found |
| Comment text `#` | a `/* */` line starting with `#` stays safe, reported and fixed | RH7203 `CommentedOutConditionalDirectivesDoNotPreventReportAndFix` |

Notes for the test implementer:

- In the between-tokens cases, the location is the name (RH7203-003), so the reported span runs across the `#pragma` line. Pin the location as -003 states it; it is not a new decision.
- `ConditionalGroupAcrossTokensOfTwoDirectivesIsNotReported` (`#if !A`) is not a single report. Under the new spec, the build without `A` compiles `System.Text`, `System.Collections`, `System.Buffers`, so RH7203 reports the last two. The build with `A` compiles `System`, `System.Buffers`, which is in order. This prediction comes from the spec and the token-based `GetNameValue`, not from running it.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7201 to RH7206 analyzers | must change: report unsafe blocks |
| RH7207 analyzer | must change: in an unsafe block, check order only, not blank lines |
| RH7201 to RH7207 code fixes and Fix All | must stay: already withheld by `CanSafelyReorder`; tests must now show the diagnostic **with** no action |
| `UsingDirectiveOrderingSafety` (`Reihitsu.Core`) | must stay: the formatter rewriter and both fix providers depend on its current answer. A change to it changes formatter output (FMT-USING-004) |
| Formatter using phase (FMT-USING-004) | must stay; spec unchanged |
| Other formatter phases | not involved. Fact: the blank-line phase writes blank lines inside an unsafe block around `#region`/`#endregion` (FMT-BLANK-013), see Q4 |
| Rule pages RH7201 to RH7207 | not involved: `general-notes.md` already covers "reported, no fix" and "judges the current build" |
| `general-notes.md` | not involved |
| Self-hosting scan | not involved (see Premise) |

## Refuse option

- **Refusing (issue option B: keep the silence)** costs nothing in code, since it is today's behavior. Its reason is "the fix and the formatter may not reorder the block". The maintainer rejected that same reason in `report-across-conditional-directives.md` for every other rule, trailing commas and RH8201 included, where the code usually exists for another build. B is consistent only as an explicit exception, the way SHARED-JOIN-001 is one.
- **Requested change (A).** It removes an early return in seven analyzers and splits RH7207's check. No new type and no new mechanism. The input class is **not rare**. It includes:
  - `#nullable enable` / `#pragma warning disable` headers;
  - `#region Usings … #endregion` around the block;
  - conditional usings appended at the end of a block (`#if DEBUG`, `#if NET…`) in the build that compiles them;
  - `#if/#else` alternative usings in multi-target code.

  Under A, all of these are reported when out of order, with no fix, and **the formatter never corrects them**. CLI users keep these diagnostics until they edit by hand. FC-014 names using order as a concern the formatter judges, so this cost is real. It is the same cost the maintainer already accepted for trailing commas, attribute-list shapes, and field splitting next to a conditional directive.
- **Unsupported shape.** Declaring "a preprocessor directive in the trivia of a using block" unsupported is possible, as a cheap `ContainsDirectives` check over the block's directives. It would delete no special case: the formatter's `CanSafelyReorder` branch would still serve node-level `OrganizeUsingDirectivesInDocumentAsync`, and the fixes' guard would become SHARED-FIX-010. It would refuse every file with a `#nullable enable` header or a conditional using, which is common code. A shape that deletes nothing is not declared (`AGENTS.md`), so I do not recommend escalating it. I name it only because the rules require that.
- **Simpler or narrower options.** Q3-B (below) is the one narrower reading. It reports only disorder within a run of usings that no preprocessor directive separates. The spec author's follow-up candidate is the cheapest way to cut A's cost for the most common case: a fix, and formatter reordering, when every directive stands before the first using. It changes formatter output, so it stays a follow-up, but it should be filed together with this change.

## Output questions

These are for the lead's batch. They amend the spec author's Q1 to Q4 in `01-spec-author.md`; I do not repeat their inputs.

**Q1, the issue's question: add two cases the maintainer should see.** The spec author's "Why A" says the order is content, not layout. That is not the right basis. FC-014 and SHARED-DIAG-010's own Why treat using order as something the formatter judges, unlike member order. The basis that holds is SHARED-DIAG-008 and the maintainer's precedent for trailing commas and attribute-list shapes: a directive keeps the fix and the formatter away, not the rule. I recommend that the question's Why say that, and that it show these two cases:

*Case 1a: conditional usings at the end of a block, analyzed in a build that defines `DEBUG` (the usual IDE configuration). This is the formatter's own output, because the CLI formats without `DEBUG` and adds the blank line (fixture `d.cs` below).*

```csharp
using System;

using Microsoft.Win32;
#if DEBUG
using System.Diagnostics;
#endif

class C
{
}
```

Before: not reported. After A (by the spec diff): RH7201 on `System.Diagnostics` and RH7207 on `System`, no fix. The hand correction is to move the group up behind `using System;`, after which neither build reports anything. After B: not reported.

*Case 1b: `#if/#else` alternatives in multi-target code, analyzed in the build that defines `NETFRAMEWORK`.*

```csharp
using System;
#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif
using Microsoft.Extensions.Logging;

class C
{
}
```

Before: not reported. After A (by the spec diff): RH7207 on `System`, no fix, in the `NETFRAMEWORK` build, because the `Microsoft` root sorts before `Newtonsoft`. The other build is in order. No single position of the `#if/#else` group satisfies both builds. The hand correction splits it into an `#if !NETFRAMEWORK` group behind `System` and an `#if NETFRAMEWORK` group behind `Microsoft.Extensions.Logging`. A canonical order always exists: every build's directives form a subsequence of one total order, so this case can always be corrected by hand. After B: not reported.

Recommendation: A, for consistency with SHARED-DIAG-008. Case 1b is the one A makes awkward, and the maintainer's answer to trailing commas ("the diagnostic tells the author what the analyzed build has") covers it.

**Q2: keep it, but note that B has no coherent reason.** B lifts the silence for conditional directives and keeps it for `#nullable`, `#pragma`, and `#region`. That reverses SHARED-DIAG-008, which already says a `#region`, `#endregion`, or `#pragma` MUST NOT by itself keep a construct from being reported. The only reason B could cite, that the fix may not reorder, holds equally for the conditional directives B reports. Recommendation: A. The question's After A should say plainly that `#nullable enable` headers and `#region Usings` wrappers are common, so A adds fix-less diagnostics there until the follow-up lands.

**Q3: keep it.** B is a real middle option: it compares only usings that no preprocessor directive separates. It is the only reading that leaves cases 1a and 1b unreported while still reporting a disordered `#nullable enable` header block. Recommendation: A. B conflicts with SHARED-DIAG-008 ("a conditional directive … inside the judged span MUST NOT keep a construct from being reported") and with SHARED-USING-007's whole-block comparison.

**Q4: an unchanged requirement decides it, so I recommend dropping it from the batch.** B (RH7207 checks blank lines in an unsafe block) would make RH7207 report the formatter's own, stable output. Fixture `e.cs`:

```csharp
using System;
#region Extra
using System.Linq;
#endregion
using Microsoft.Win32;
using Alpha;

class C
{
    int x;
}
```

`scripts/format.sh` writes:

```csharp
using System;

#region Extra

using System.Linq;

#endregion // Extra

using Microsoft.Win32;
using Alpha;

class C
{
    int x;
}
```

A second run reports `Formatted 0 of 2 file(s)`. The blank lines come from FMT-BLANK-013 (region frame). Under B, RH7207 would report the blank line between `System` and `System.Linq` (same group, not led by a comment). The rule's fix lays the block out as the formatter does, so SHARED-DIAG-010 forbids that report. A is demanded by an unchanged requirement, and nothing visible is in question. The lead can record it as a decision.

Formatter facts used above. Each fixture was run through `scripts/format.sh` on a copy, and the diff below was observed.

- `b.cs`: blank lines in front of and behind `#if`/`#endif` inside a block, and a blank line inside a group. Every blank line is kept; only `class C {}` changes.
- `c.cs`: `#region Usings` / `using System.Linq;` / `using System;` / `#endregion`. Blank lines are added inside the region and `#endregion // Usings` is written. The order is kept.
- `d.cs`: `using System;` / `using Microsoft.Win32;` / `#if DEBUG` group / `class C`, with no `DEBUG` defined. The block is reorganized with a blank line in front of `Microsoft.Win32`, which is the input of case 1a.

## Internal choices

- **Wording of the Why in SHARED-USING-009 and the change doc.** "The formatter keeps those blank lines (FMT-USING-004)" is not quite true: `e.cs` shows the formatter writing blank lines inside such a block through FMT-BLANK-013. The conclusion gets stronger, not weaker. I recommend rewording it to say that the using phase does not lay out the group boundaries of such a block (FMT-USING-004), and that blank lines other requirements write there, such as a region's frame (FMT-BLANK-013), would otherwise be reported (SHARED-DIAG-010). No visible change.
- **"The order is content" in the change doc's Why and the SHARED-USING-009 Why.** I recommend replacing it with the SHARED-DIAG-008 basis (see Q1). SHARED-DIAG-010's literal text does exempt order rules, but its Why ("not the formatter's decision") does not fit using order, which the formatter judges (FC-014). No visible change.
- **SHARED-USING-009 example "a directive after the block does not count, so the code fix and the formatter reorder / to".** This is an input/output pair with no colon in front of either block. Cosmetic.
- The spec author's in-place rewrite of SHARED-USING-009 and the one-sentence restatement in each RH720x-004: agreed.

## Issue goals

The issue is `agent-filed`. Its single goal ("the maintainer chooses A or B; for A, rewrite SHARED-USING-009 and add tests") stands. One premise needs correcting: "the ordering rules are content rules, which SHARED-DIAG-010 lets report". Using order is a concern the formatter judges (FC-014), so the basis for A is SHARED-DIAG-008 and the maintainer's precedent, as above. This changes no visible output.

## Architecture findings

_None._ The analyzers, the fixes, and the formatter read one helper for "unsafe block". After the change, the analyzers stop consulting it for whether to report, except for RH7207's aspect split, and the fixes and the formatter keep it. That is no second statement of the rule.

## Follow-up candidates

1. **The spec author's candidate (fix and formatter for a block whose every directive stands before its first using).** I support it and recommend filing it with priority: under A it removes the fix-less diagnostics for the most common inputs (`#nullable enable`, `#pragma warning disable`, `#region Usings`). It changes formatter output, so it is an output question in that issue.
2. **FMT-USING-004 vs FMT-BLANK-013, spec wording.** FMT-USING-004 says the formatter MUST keep the blank lines of an unsafe block. FMT-BLANK-013 adds blank lines around a `#region` inside one, and `c.cs` and `e.cs` show the latter in effect. FMT-USING-004 should be scoped to the using phase, or should name FMT-BLANK-013 as the exception. Internal, no output change.

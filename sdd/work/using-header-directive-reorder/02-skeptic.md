Role: skeptic
Commit: 616e3b6
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It only pointed at the maintainer's answer in `00-lead.md`, which is binding. On the merits the change should go ahead as specified: the premise holds, the spec diff is consistent with the shared specs, and the requested change is small. The verdict is `output question` because four visible cases are not explicitly covered by the maintainer's answer (below). The lead decides which of them the answer or an unchanged requirement already covers.

## Premise

Real. A caller reaches the case through each of these paths:

- whole-document formatting (CLI, `FormatDocumentAsync`), which runs `UsingDirectiveOrderingPhase` (`FormattingPipeline.CreatePhases`);
- the RH7201–RH7206 code fixes (`UsingDirectiveOrderingCodeFixProviderBase.RegisterCodeFixesCoreAsync:89`) and the RH7207 fix (`RH7207…CodeFixProvider:68`). All of them, Fix All included, go through `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync:31` and `OrganizeUsingDirectivesInDocumentAsync` (FMT-PIPE-030), with the reported directive's scope as input.

No other code fix formats a compilation unit or a namespace: `rg` over `src/Reihitsu.Analyzer.CodeFixes` finds no `FormatNode*`/`FormatDocument*` call on a `NamespaceDeclarationSyntax`, `FileScopedNamespaceDeclarationSyntax`, or `CompilationUnitSyntax`.

The behavior follows from the code, not only from the issue's reading of it. `UsingDirectiveOrderingSafety.CanSafelyReorder` (`src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs:23-26`) returns `usingDirectives.Any(u => u.ContainsDirectives) == false`. `ContainsDirectives` covers the leading trivia of the first directive, so any header directive makes the block unsafe. All five consumers read this one predicate:

- the formatter rewriter, `UsingDirectiveOrderingRewriter.cs:76`;
- both fix registrations;
- the fix helper;
- `RH7207…Analyzer.IsOrganized:67`, which skips the blank-line check.

Confirmed on 616e3b6 (the code equals `main`) with fixtures in a scratch directory. Each fixture is followed by an empty `class C { int x; }`, given here as `…class C`:

| Fixture | Input (usings part) |
|---|---|
| c1 | `#nullable enable` / `using System.Linq;` / `using System;` (the issue's case) |
| c2 | `#nullable enable` / *blank* / `using System.Linq;` / `using System;` |
| c3 | `namespace N` / `{` / `    #nullable enable` / `    using System.Linq;` / `    using System;` / … `}` |
| c4 | `#if !NETSTANDARD` / `using System.Linq;` / `using System;` / `#endif` / *blank* / `class C` |
| c5 | `#define TRACE_X` / `using System.Linq;` / `using System;` |
| c6 | `extern alias Foo;` / `#nullable enable` / `using System.Linq;` / `using System;` |
| c7 | `// <copyright file="C.cs">` / `// Header` / `// </copyright>` / *blank* / `#nullable enable` / `#pragma warning disable CS0105` / `using System.Linq;` / `using System;` / `#pragma warning restore CS0105` / *blank* / `class C` |

- `scripts/apply-fix.sh RH7203 <dir>` gave `no-fix-offered` for all 7 fixtures under LF and CRLF ("0 fixed, 14 no-fix-offered"), so the diagnostic is reported and no fix is offered.
- `scripts/format.sh <dir>` left every using block unchanged. It only removed the final newline, and in c3 it moved `#nullable enable` to column 0.

The issue's dependency, `sdd/changes/2026-11/using-block-directive-report.md`, is `Accepted`, so its premise ("once the rules report…") is current.

## Mechanism and candidates

_N/A._ This is a behavior change, not a bug.

The dispatch the change reaches is the one predicate `CanSafelyReorder` and its five consumers named above, plus the header split `UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia`. The rewriter calls the split through `DetachHeaderOfOriginalFirst` only when the canonical first directive differs from the original first. The scopes the rewriter and `TryGetUsingDirectiveScope` visit are `CompilationUnitSyntax`, `NamespaceDeclarationSyntax`, and `FileScopedNamespaceDeclarationSyntax`.

## Breaking inputs

Failure classes that reach the change:

- **Guard:** FC-001, FC-002, FC-003.
- **Trivia:** FC-004, FC-005, FC-020. FC-005 matters because FMT-USING-009's split was written for comments and now receives directives. That is exactly what FMT-USING-012 exists for.
- **Shared owner:** FC-015. The single shared predicate is the right shape; the change must not add a second copy, for example a private check in the analyzer.
- **Scope:** FC-027 (another enclosing scope).
- **Fix and formatter:** FC-007, FC-008, FC-010, FC-013, FC-028. For FC-028, the fix (FMT-PIPE-030) and the whole-document formatter differ by design for `#region` (change doc case 3).
- **Tests and evidence:** FC-017, FC-031, FC-032, FC-033.

FC-031 applies to these XML summaries, which describe today's guard and become stale:

- `CanSafelyReorder`;
- `RH7207…Analyzer.IsOrganized` ("In a block that holds a preprocessor directive only the order is checked");
- `UsingDirectiveOrderingRewriter.SeparateDirectivesSharingALine` ("every leading trivia — which holds every preprocessor directive").

FC-032 applies to other tests: test files outside the using rules have `#nullable enable` headers in front of usings (`RH3004UseLambdaSyntaxMultiLineTokenAndDirectiveTests.cs`, `AttachedPartLineStartTests.cs`, `RH5201…`, `RH5116…`, …). Through the formatter or a fix they now get reordered and regrouped. The test implementer runs the analyzer and formatter projects after the implementation, not only the using tests. No file under `src/`, `tests/`, or `tools/` starts with a directive header in front of its usings, so self-hosting is unaffected.

Matrices: the catalog has none for this class, so I spanned the axes from the dispatch.

**Axis 1: where the directive stands (the predicate, FC-003).** Members:

- (a) leading trivia of the first token of the first directive: safe (new);
- (b) leading trivia of a later token of the first directive: unsafe;
- (c) leading trivia of a later directive: unsafe;
- (d) leading trivia of the token after the block: safe (today);
- (e) (a) and (c) together: unsafe. This is FC-001: the narrowed predicate must still inspect the rest of the block when the first token already holds a directive.

| # | Input | Expected | Existing test |
|---|---|---|---|
| 1 | c1 (case 1) | reorder, directive on top | flips: `UsingDirectiveOrderingSafetyTests.NullableDirectiveBeforeFirstDirectiveIsUnsafe`, `RH7203…AnalyzerTests.BlockLedByANullableDirectiveIsReportedWithoutCodeFix`, `RH7207…AnalyzerTests.BlockLedByANullableDirectiveIsReportedWithoutCodeFix`, `RH7207…FormatterTests.FormatterKeepsAReportedBlockLedByANullableDirective`, `UsingDirectiveRequirementTests.UnsafeBlockOnlyGetsOneDirectivePerLine` (FMT-USING-004 example changed) |
| 2 | `#nullable enable` / `using System.Linq;` / `#pragma warning disable CS0105` / `using System;` (member e) | unsafe: reported, no fix, formatter keeps | none found |
| 3 | `using` / `#pragma warning disable CS0105` / `    System.Linq;` / `using System;` (member b, the spec example) | unsafe | none found for the *first* directive (`PragmaBetweenTokensOfDirectiveIsUnsafe` covers the second) |
| 4 | member c | unsafe | `DirectiveInLeadingTriviaOfLaterDirectiveIsUnsafe`, and the many `…WithoutCodeFix` tests with `#if` between, all unchanged |
| 5 | member d | safe | `DirectiveAfterLastDirectiveIsSafe`, `DirectiveAfterTheBlockDoesNotPreventReportAndFix` (RH7203/RH7207) |
| 6 | c4: `#if` in front, `#endif` behind the last using (common in multi-target code) | reorder inside the group | none found |

**Axis 2: what stays at the top when the original first directive moves (FMT-USING-012 and FMT-USING-009).** The only effect FMT-USING-012 adds to FMT-USING-009 is that the split point is never in front of the end of the last directive. Cases 5 and 6 and c2 give the same output under FMT-USING-009 alone. Members:

- directive only (c1);
- directive + blank line (c2);
- comment + blank line + directive (case 4);
- directive + comment (case 5);
- directive + blank line + comment (case 6);
- two directives separated by a blank line;
- comment + blank line + comment + directive;
- a conditional group with disabled text (case 7, where the line break sits inside the disabled-text trivia);
- `#region` (case 3);
- the original first keeps its position.

| # | Input | Expected | Existing test |
|---|---|---|---|
| 7 | c2 | `#nullable enable` / *blank* / `using System;` / `using System.Linq;` | none found; no spec example (FMT-USING-012's text decides it) |
| 8 | `#nullable enable` / *blank* / `#pragma warning disable CS1591` / `using System.Linq;` / `using System;` | everything through the `#pragma`, blank line included, stays on top | none found |
| 9 | `// Header` / *blank* / `// About nullable` / `#nullable enable` / `using System.Linq;` / `using System;` | the comment and the directive stay on top | none found |
| 10 | change doc cases 3–7 | spec outputs (fix and formatter separately for case 3) | none found |
| 11 | c7 (copyright header, two directives, `#pragma restore` behind the block) | reorder; header and both directives stay on top | none found |

**Axis 3: scope (FC-027).** Members: compilation unit, block namespace indented, file-scoped namespace.

| # | Input | Expected | Existing test |
|---|---|---|---|
| 12 | c3 | fix: `    #nullable enable` stays, `using System;` and `using System.Linq;` at 4 spaces (FMT-USING-011); formatter: same, with the directive at column 0 as today | none found (the `CodeFixKeepsNamespaceIndentationBelowAHeader` tests use comment headers) |
| 13 | file-scoped `namespace N;` / `#nullable enable` / `using System.Linq;` / `using System;` | reorder | none found |

**Axis 4: directive kind (the split only; the predicate does not depend on the kind).**

| # | Input | Expected | Existing test |
|---|---|---|---|
| 14 | c5, `#define` header | `#define` must stay in front of the first token (the compiler rejects `#define` after the first token), so it must stay on top | none found (`RH7201…BlockLedByADefineDirectiveIsReportedWithoutCodeFix` has an `#if` between, unchanged) |
| 15 | `#nullable enable` / `global using System.Linq;` / `global using System;` | the first token is `global`; safe, reorder | none found |

**Axis 5: RH7207 blank lines and Fix All.**

| # | Input | Expected | Existing test |
|---|---|---|---|
| 16 | case 2 | RH7207 reports, fix and formatter add the blank line | none found. `BlockWithPreprocessorDirectiveInCanonicalOrderIsNotReportedForAMissingBlankLine` and `BlockWithRegionDirectiveInCanonicalOrderIsNotReportedForItsBlankLines` have directives between and stay unchanged |
| 17 | Fix All over a header block and an unsafe block in one document, and over a header block in the compilation unit and one in a namespace | each header block fixed, the unsafe one unchanged | `RH7203…FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective` has an `#if` between and stays valid; none for a header block |
| 18 | every row | under CRLF; a second formatter pass and a second fix iteration are no-ops (FC-010, FC-017) | — |

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `UsingDirectiveOrderingSafety.CanSafelyReorder` | must change (the one owner of the predicate, FC-015) |
| Formatter using phase: `UsingDirectiveOrderingRewriter` and `UsingLeadingTriviaBuilder` header split | must change (FMT-USING-012) |
| RH7201–RH7206 fix registration (`UsingDirectiveOrderingCodeFixProviderBase`), RH7207 fix provider, `UsingDirectiveCodeFixUtilities` | must stay; they follow the predicate (code unchanged; comments may need updating, FC-031) |
| RH7207 analyzer `IsOrganized` | must stay as code; its behavior changes through the predicate (case 2); summary stale (FC-031) |
| RH7201–RH7206 analyzers | not involved (they report regardless of directives) |
| Fix All (`WellKnownFixAllProviders.BatchFixer`) | must stay |
| `OrganizeUsingDirectivesInDocumentAsync` / FMT-PIPE-030 | must stay |
| Other formatter phases (blank lines, FMT-BLANK-013 region framing, indentation) | must stay; they act on the reordered block as they act today |
| Rule pages `documentation/rules/RH7201.md`–`RH7207.md`, `general-notes.md` | must stay (none mentions a header directive; general notes already cover a withheld fix) |
| `sdd/specs/formatter/pipeline.md` (FMT-PIPE-030 cites SHARED-USING-009 abstractly) | must stay |
| CLI | not involved beyond the formatter's output |
| Tests | flipped and new, listed above |

## Refuse option

**For this issue.** Keep every header-directive block unsafe: reported, no fix, formatter keeps it. That is the issue's option B, which the maintainer rejected.

The class is not rare. `#nullable enable`, `#pragma warning disable`, and a `#region` around the usings are common, and so is a copyright header + blank line + `#nullable enable` (case 4).

The requested change is cheap by comparison:

- a predicate that skips the leading trivia of the first token of the first directive;
- one more split bound in the header split.

It needs no new type or mechanism and stays a cheap syntactic check, as the maintainer required. I see no cost argument for refusing.

**Narrower options**, both rejected:

- (i) Withhold the fix when a directive follows the header's blank line (case 4). This saves FMT-USING-012's split bound but refuses the common copyright-header shape.
- (ii) Treat disabled text in front as unsafe (case 7). This saves nothing, since the predicate does not look at disabled text, and refuses a safe reorder.

**Unsupported shape (escalation 10).** This shape is not the issue's; it is named here as the rule requires, and I recommend not declaring it.

- **Span:** a preprocessor directive or disabled text from the first token of a using block's first directive through the `;` of its last directive, excluding the leading trivia of that first token. That is: a directive between two using directives or between the tokens of one.
- **It would delete:** `UsingDirectiveOrderingRewriter.SeparateDirectivesSharingALine` (the FMT-USING-004 path), and the `CanSafelyReorder` checks in the fix registrations and `OrganizeScopeUsingsAsync`, which SHARED-FIX-010 would replace. The RH7207 blank-line exemption in `IsOrganized` is an analyzer decision and would need its own answer.
- **It would refuse common code:** every file with an `#if NET8_0_OR_GREATER … #endif` or a `#pragma warning disable` around one using between others, which is common in multi-target libraries. Such files would get no formatting at all.
- **Recommendation:** do not declare. If declared, it belongs in a change of its own.

## Output questions

The maintainer's answer explicitly covers case 1 (fix and formatter reorder; directive stays on top). The cases below change visible output. For each I say whether I think the answer or an unchanged requirement already covers it; the lead decides.

**1 · RH7207 · Using directives should be organized into groups — header directive in front of a block in canonical order without its group blank line** (change doc case 2). Not covered by the answer, which speaks of fixes and the formatter, not of a new diagnostic. The alternative is real: keep the blank-line exemption for every block that holds a directive anywhere.

Input:
```csharp
#nullable enable
using System;
using Microsoft.Win32;

class C
{
}
```

Before (`main`): not reported; the formatter keeps the block.

After, A (recommended): RH7207 reports `System` in `using System;`; the fix and the formatter give
```csharp
#nullable enable
using System;

using Microsoft.Win32;

class C
{
}
```

After, B: not reported; the formatter still adds the blank line.

Why A: the same block without the header is reported today, and once the formatter lays the block out, staying silent would be RH7207's own gap.

**2 · RH7201–RH7207 fixes and formatter — comment directly below a header directive** (change doc case 5; the spec author's Q1).

Input:
```csharp
#nullable enable
// About Linq
using System.Linq;
using System;
```

Before (`main`): reported, no fix offered; the formatter keeps the block.

After, A (recommended):
```csharp
#nullable enable
// About Linq
using System;
using System.Linq;
```

After, B:
```csharp
#nullable enable
using System;
// About Linq
using System.Linq;
```

Why A: no blank line separates the comment from what is above it, and a comment directly above the first using stays on top in the same way today. FMT-USING-009 applied unchanged to the whole leading trivia already gives A, so I consider it covered. B needs a new split criterion.

**3 · RH7201–RH7207 fixes and formatter — comment behind a blank line below a header directive** (change doc case 6). It is in question only because the issue's "Done when" says "with the header directive and its trivia unchanged".

Input:
```csharp
#nullable enable

// About Linq
using System.Linq;
using System;
```

Before (`main`): reported, no fix offered; the formatter keeps the block.

After, A (recommended):
```csharp
#nullable enable

using System;

// About Linq
using System.Linq;
```

After, B:
```csharp
#nullable enable

// About Linq
using System;
using System.Linq;
```

Why A: the comment behind the blank line belongs to `System.Linq`, exactly as for a comment header without a directive. FMT-USING-009 applied unchanged gives A, so I consider it covered.

**4 · RH7201–RH7207 fixes and formatter — conditional group in front of the block, in a build without `DEBUG`** (change doc case 7; the spec author's Q2).

Input:
```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System;
```

Before (`main`): reported, no fix offered; the formatter keeps the block.

After, A (recommended):
```csharp
#if DEBUG
using System.Diagnostics;
#endif
using System;
using System.Linq;
```

After, B: unchanged, no fix offered.

Why A: in that build every directive stands in front of the first using, and no using crosses a directive in any build. The answer's literal wording covers it. A build with `DEBUG` stays unfixed, as today.

**Reading of the maintainer's answer, "between or after its usings".** Today a directive after the last using belongs to the next declaration and does not block the fix (SHARED-USING-009, unchanged; tested by `DirectiveAfterTheBlockDoesNotPreventReportAndFix`). Read literally, "after its usings" would withdraw that fix, which is a regression nobody asked for. Read against the issue's contrast "before its first using", it means "after the first using", which is the spec's reading. I recommend that reading and the lead deciding it without asking. If the lead disagrees, it is an output question.

## Internal choices

- **Wording of FMT-USING-012.** "the reorder moves that directive away from the top" uses "directive" for both the using directive and the preprocessor directive in one sentence. Recommendation: write "that using directive". No output changes.
- **RH7201-001 … RH7207-001.** These now say "a preprocessor directive *between its directives*", while -004 and SHARED-USING-009 also name "between the tokens of one". Recommendation: say "among its directives", like the SHARED-USING-009 title, or reference SHARED-USING-009. No output changes.
- **Spec example for c2.** Adding an FMT-USING-012 example for c2 (directive + blank line, no comment; very common) would make the tests' seed explicit. Optional; the requirement text already decides it.
- **FMT-USING-012 as its own requirement.** The spec author chose a separate requirement over widening FMT-USING-009. I agree: it keeps the directive rule taggable on its own.

## Issue goals

The issue is `agent-filed`.

- **"with the header directive and its trivia unchanged"** — change to: every preprocessor directive in front of the block stays on top, and the trivia behind the last one follows FMT-USING-009. Case 6 moves a comment, which the literal goal would forbid. Visible: yes, raised as output question 3.
- **"a block with a directive after or between usings stays reported without a fix"** — change "after" to mean "after the first using". A directive behind the last using never blocked the fix and must not start to. Read as recommended, nothing visible changes (see above).
- **"Affected: … `sdd/specs/shared/code-fixes.md` (SHARED-USING-009)"** — stale; the maintainer already corrected it to `using-directive-ordering.md`. Not visible.
- **The other goals** (fix, Fix All, formatter reorder; the specs state the narrowed guard; covering tests) serve the purpose and stay.
- **Severity** is right: an enhancement for a common input. No severity argument is overstated; the issue does not claim that a fix-less diagnostic is a defect.

## Architecture findings

_None._ One shared predicate decides safety for the analyzer, the fixes, and the formatter, and one helper decides the header split. The change keeps both single owners.

The fix registrations and the RH7207 analyzer call the phase's own predicate rather than predicting a layout decision, so question 10 finds no prediction. No node-level entry point changes.

## Follow-up candidates

_None._ The unsupported-shape option above goes to the lead as escalation 10 with the recommendation not to declare. It is not a follow-up issue unless the maintainer chooses to declare it in a change of its own.

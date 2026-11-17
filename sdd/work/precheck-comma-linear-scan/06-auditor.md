Role: auditor
Commit: 6af1ef208486f45c551af81666ecab4b999f090d
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `6af1ef2`, which is the checkout (`git rev-parse HEAD`). Base `origin/main` = `17fb08c5a5ab7a84f58db1559fd13163279ffbcb`, which matches `git ls-remote origin refs/heads/main` and is an ancestor of the head. The diff against the base is what the handovers describe. Production: `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs`, +46/-42, private members only. Test: the new `src/Reihitsu.Formatter.Test/Regression/Precheck/LongListPrecheckCostTests.cs`. Text: `sdd/work/precheck-comma-linear-scan/*`. `scripts/lane-check.sh bug-fix` reports `confirmed — claimed bug-fix, diff needs bug-fix`.

Issue: thoenissen/Reihitsu#1802 (`agent-filed`). Its "Done when" has two parts, and both are met by evidence (see *Evidence*):

- The verdicts are identical on the repository's own sources and on the existing tests.
- The precheck of a 20 000-element initializer is well under one second.

The prescribed single-walk mechanism was dropped as a requirement (00-lead decision 2). The diff happens to use it anyway.

## Evidence

**Differential probe.** This is a throw-away console program in the scratchpad, outside the repository. It is built against the Release `Reihitsu.Formatter.dll` of the head. It calls the new private `FindTriviaInFrontOfComma` by reflection and compares it with a verbatim copy of `main`'s `FindTriviaInFrontOfComma` and `EnumerateGapTrivia`. The comparison covers the trivia span and the declarator flag.

The corpus:

- **The repository's own sources.** That is every `*.cs` file under `src/`, 1 936 files.
- **Four mutations of each source file.** Each mutation inserts one of these before a random quarter of the file's commas: ` /* m */\n`, ` // m\n`, `\n#if NEVER\n#endif\n`, or `\n`.
- **16 hand-made shapes, under LF and CRLF.** These cover:
  - omitted type arguments with a comment in the gap (`Dictionary<\n// c\n,>`);
  - omitted array sizes;
  - declarator commas;
  - `#if` and `#pragma` in front of an argument comma;
  - empty arguments (`M(1, /* c */\n, 3)`);
  - deconstruction and tuples;
  - enums;
  - commas inside documentation comments;
  - interpolation alignment;
  - `for` initializers and incrementors;
  - two inputs with a syntax error, each with an inserted missing comma.

Each input was compared as the compilation unit, and also as every node that owns a comma or whose parent owns one. That second set is the node-level inputs (FMT-UNSUP-003).

The result was 605 779 comparisons and 8 differences. All 8 come from the two syntax-error shapes (see *Hints*). There are 0 differences on any input without a syntax error, so the gap, the first-match order, the reason key, and the node boundary are unchanged.

**Timing.** Same probe, 30 000-element array initializer with the shape at the last comma, Release:

| Measurement | Time |
|---|---|
| `FindUnsupportedSyntax` on the root | 14–53 ms |
| `FormatSyntaxTree` (refused) | 36–62 ms |

The budget in the tests is 3 s; the skeptic and the test implementer measured about 20 s on `main`.

**Focused tests.** `scripts/test.sh --project formatter --filter "…Regression.Precheck.LongListPrecheckCostTests|…Unit.Precheck"` (Release) gave 469 passed, 0 failed. The five cost tests took 0.5–1 s each, and that includes two parses of a 30 000-element document per test.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no confirmed finding._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Start of the gap in front of a comma (FMT-UNSUP-009) | From the last token of `node.DescendantTokens()` with `Span.IsEmpty == false` to the comma | `main`: `comma.GetPreviousToken()`, whose default predicate is non-zero width, without skipped, directive, or documentation tokens | Same token: both exclude structured trivia, and "empty span" means "zero width". The probe found 0 differences outside syntax errors | holds |
| Tokens inside the gap | Tokens with an empty span between that token and the comma, in text order; their leading then trailing trivia | `main`: `GetNextToken(includeZeroWidth: true)` from the previous token to the comma | Same set and order: every token between the last token with text and the comma has zero width by construction | holds |
| Node boundary (FMT-UNSUP-003) | A comma with no token with text in front of it inside the node is skipped (`previousToken == null`) | `main`: `previousToken.SpanStart < node.FullSpan.Start` | Equivalent: a token in front of the comma that starts inside the node's full span is one of the node's tokens. Covered by `ElementTargetWithoutTheCommaIsNotRefused` and `FindUnsupportedSyntaxAnswersForTheListAndNotForPartOfIt` | holds |
| Which token is a comma | `token.IsKind(CommaToken)`, checked only for tokens with text | `main`: every `CommaToken`, the zero-width missing comma included | Differs only for a missing comma, which only a syntax error produces. No caller reaches it (*Hints*) | holds for supported inputs |
| Line break and first trivia in the gap | `ToFullString()` contains `\n` or `\r`; the first trivia for which `IsCommentTrivia` or `IsDirectiveOrDisabledTextTrivia` holds | `main`: the same two expressions | Identical code | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The gap in front of a comma for FMT-UNSUP-009 | `PrecheckPhase.FindTriviaInFrontOfComma` and `FindTriviaInGap` (`rg -n "FindTriviaInGap\|EnumerateGapTrivia\|FindTriviaInFrontOfComma" src/Reihitsu.Formatter src/Reihitsu.Core`). `EnumerateGapTrivia` is deleted with no remaining caller | One owner, unchanged |
| Precheck entry (FMT-UNSUP-007) | `PrecheckPhase.FindUnsupportedSyntax`, delegated by `ReihitsuFormatter.FindUnsupportedSyntax`. The code-fix callers are `FluentChainCodeFixHelper`, `ExpressionBodyToBlockCodeFixProviderBase`, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`, and 25 providers under `Rules/` | Unchanged; no private copy |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LongArrayInitializerWithoutShapeIsCheckedWithinBudget` | Whole-root precheck of a 30 000-element list without the shape answers `null` within 3 s, under LF and CRLF | `main`'s scan takes about 20 s, so the budget assertion fails, and any answer other than `null` fails as well | `Stopwatch` around `FindUnsupportedSyntax` only, with the parse outside the clock; `Assert.IsLessThanOrEqualTo` | adequate |
| `LongArrayInitializerWithShapeAtLastCommaIsRefusedWithinBudget` | Whole-document formatting refuses with the FMT-UNSUP-009 reason at `/* c */`, returns the text unchanged, and stays within budget | A different reason, location, or output, or more than 3 s | `FormatSyntaxTree`; `AssertLocationIsShape`; full-text equality | adequate |
| `LongCollectionInitializerWithShapeAtLastCommaIsFoundByTheNodeLevelCheckWithinBudget` | The node-level answer for the RH5303 kind of target has the reason and location, within budget | As above | `FindUnsupportedSyntax(InitializerExpressionSyntax)` | adequate |
| `LongArgumentListWithShapeAtLastCommaIsFoundByTheNodeLevelCheckWithinBudget` | The same, for an argument list (RH5101 and RH5102 kind) | As above | `FindUnsupportedSyntax(ArgumentListSyntax)` | adequate |
| `LongEnumWithoutShapeIsCheckedByTheNodeLevelCheckWithinBudget` | Node-level `null` for a 30 000-member enum, within budget | As above | `FindUnsupportedSyntax(EnumDeclarationSyntax)` | adequate |

Margin: the scan measures 14–62 ms against the 3 s budget, which is at least 50x below it, and `main` takes about 20 s, about 7x above it. So the test can fail on a regression and is unlikely to flake under CI contention.

Verdict identity across the gap-boundary facts rests on the 464 existing, unchanged precheck tests (01-skeptic.md facts 1–5) plus the differential probe above.

## Failure classes dismissed

- **FC-001, FC-002** — The span and every qualifier of the decision are unchanged (*Guard scope*; differential probe).
- **FC-003** — The changed mechanism has two boundaries: the node boundary and the zero-width gap start. The existing tests cover both sides of each, for example the omitted type-argument rows and `ElementTargetWithoutTheCommaIsNotRefused`.
- **FC-004, FC-019, FC-020, FC-021** — No edit, join, or move. The precheck changes nothing. The probe's comment and directive mutations show identical answers.
- **FC-005** — The comment and directive predicates are the same calls as on `main`.
- **FC-006, FC-022 to FC-030, FC-036** — There is no rewrite and no code-fix change.
- **FC-007, FC-008, FC-009, FC-018** — No code fix changed. The callers get the same answer faster.
- **FC-010, FC-011, FC-012** — No phase output changes. The walk is a single `foreach` over a finite token sequence.
- **FC-013, FC-014** — No rule or formatter ownership changes.
- **FC-015, FC-016** — There is one owner (*Policy ownership*). Sibling uses of per-token `GetPreviousToken`/`GetNextToken` in other phases do not decide this shape. Their cost belongs to the whole-document follow-up already in `00-lead.md`.
- **FC-017** — See *Assertion adequacy*.
- **FC-031** — Both changed summaries describe the new code. The single walk, the carried token, and the skip at the node boundary are in `FindTriviaInFrontOfComma`, and the gap order and the line-break condition are in `FindTriviaInGap`. No stale text remains.
- **FC-032** — No earlier project result is reused. The tests were run on this head.
- **FC-033** — The tests run LF and CRLF on the surfaces the callers use: whole document and node-level.
- **FC-034** — The reported shape is a long single array initializer. The tests use it at 30 000 elements, plus two other parent kinds.
- **FC-035** — The expected reasons are the spec constants in `FormatterTestsBase`. The test implementer checked the expected locations against `main` at 2 000 elements.
- **FC-037** — Not a text-only claim.
- **FC-038** — The precheck produces nothing.
- **FC-039** — `DescendantTokens()` descends into every nested list, as `main` did. The probe compared nested node-level inputs.
- **FC-040** — Structured documentation trivia is excluded on both sides. A doc-comment comma shape is in the probe.
- **FC-041** — The new walk reads only the node's own tokens and their trivia. It is stricter than `main`, which looked outside and then discarded the result, and it gives the same answers at node level (probe).
- **FC-042** — No guard added or kept for a declared shape.
- **FC-043** — No code fix changes which node it asks about.
- **FC-044** — Nothing modeled.

## Architecture findings

_None._

## Hints

- **The new walk no longer treats a missing comma as a gap end.** A missing comma is the zero-width `CommaToken` the parser inserts on a syntax error. Two inputs show it:
  - In `class C { int[] a = { 1 /* c */\n 2 }; }`, `main` reports `/* c */` and the head returns `null`.
  - The same happens for `void M(int a /* c */\n int b)`.

  No caller reaches this:
  - Every whole-document and in-document entry point returns before the pipeline on a syntax error (`ReihitsuFormatter.cs:62, 96, 205, 309`).
  - Every code-fix caller of `FindUnsupportedSyntax` or `FormatNode` declares `FormatsThroughFormatter => true`, either itself or through its base. `ReihitsuCodeFixProviderBase.MayOfferFixesAsync` then registers nothing in a document with a syntax error (SHARED-FIX-011). I checked by comparing `rg -l "FindUnsupportedSyntax"` against `rg -l "FormatsThroughFormatter => true"`; the only file outside the second list is `FluentChainCodeFixHelper`, whose users RH5112 and RH5201 override it.
  - A detached node that a code fix builds holds no missing token.

  So this is not a finding (`AGENTS.md`, *Supported inputs*).
- **`FindTriviaInGap` allocates a closure per comma.** The local `Visit` function captures `firstTrivia` and `hasLineBreak`. The cost is linear and small next to `DescendantTokens()`, so I am not reporting it.

## Follow-up candidates

_None_ beyond those in `00-lead.md`.

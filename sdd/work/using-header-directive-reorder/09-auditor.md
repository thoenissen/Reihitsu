Role: auditor
Commit: 55bb071ba3dea2c3d5cba52e730d18b2116694dc
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1971, lane `behavior-change`.

- Head: `55bb071` (checked out; `git rev-parse HEAD` matches).
- Base: `origin/main` = `772e92e9a977c13a61d730b8a04e5bece771457d`, the SHA in the prompt after `git fetch`. It is an ancestor of HEAD, so it is merged.
- The diff matches the handovers:
  - production: `UsingDirectiveOrderingSafety.CanSafelyReorder`, `UsingLeadingTriviaBuilder.GetHeaderSplitIndex` and its summaries, and the summary of the RH7207 analyzer's `IsOrganized`;
  - the specs and change doc named in handovers 01, 03, and 04;
  - the tests listed in 05.
- `scripts/lane-check.sh behavior-change`: confirmed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no confirmed finding._

What I checked against the specs:

- **SHARED-USING-009** holds in `CanSafelyReorder`:
  - The first token of the first directive is unsafe only for `IsConditionalDirective`, which covers `#if`, `#elif`, `#else`, and `#endif`.
  - Every other token of every directive is unsafe for any directive.
  - A directive behind the block is the next token's leading trivia, which the predicate never inspects.
  - The old predicate also covered trailing trivia. Dropping that loses nothing, because Roslyn attaches directives and disabled text only as leading trivia, and disabled text always follows a conditional directive in the same leading trivia.
- **FMT-USING-012** holds in `GetHeaderSplitIndex`: when the leading trivia holds a directive, it returns `Count`. `SplitOriginalFirstHeaderTrivia` then hands only the whitespace of the using's own line to the remainder (FMT-USING-011). That makes the whole significant trivia the header, and the branch in `DetachHeaderOfOriginalFirst` for "comment left to lead the detached directive" cannot fire.
- **FMT-USING-009** is unchanged for trivia without a directive.
- **The issue's "Done when" items**, as adjusted by the maintainer's answers 1–4 and decision rows 1 and 4 in `00-lead.md`, are each met:
  - The fix, Fix All, and the formatter reorder a header-led block.
  - A directive between the usings, or between the tokens of one, keeps the block unfixed.
  - A conditional directive in front of the block keeps it unfixed (5B and answer B).
  - The specs carry the narrowed guard, and FMT-USING-012 has 18 tagged tests.

Probes outside the tests (temporary fixtures in the scratchpad, each under LF and CRLF):

- **Shapes:**
  - a `///` comment below the header directive;
  - a multi-line `/* */` comment above the directive;
  - `/* c */` on the line of the first using behind the directive;
  - two blank lines behind the directive;
  - `#region` / `#endregion` both in front of the block;
  - a header directive with the canonical first led by a comment behind a blank line;
  - alias, static, and regular usings behind `#nullable`;
  - comment header + blank line + directive + comment + blank line inside a namespace;
  - `#pragma warning disable` in front and `#pragma warning restore` behind the block.
- **Code fixes:** `scripts/apply-fix.sh` RH7201, RH7202, RH7203, RH7207: every fixture is fixed in 1 iteration, with 0 not-converged and 0 line-ending drift. Every header directive and comment stayed at the top as FMT-USING-012 states.
- **Formatter:** `scripts/format.sh` twice. The second pass changed 0 of 9 files, and the output matches the fix output apart from the whole-document phases (region framing, column 0 for a directive, doc-comment layout).
- **Parity:** `scripts/apply-fix.sh` RH7202, RH7203, RH7205, RH7207 over the formatter output gave 18 of 18 no-diagnostic (FC-013).

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Block may be reordered (SHARED-USING-009) | Leading trivia of every token of every using directive whose `ContainsDirectives` is true. For the first token of `usingDirectives[0]`, only conditional directive kinds count | The same predicate in all five consumers: `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives:76`, `UsingDirectiveOrderingCodeFixProviderBase:89`, `RH7207…CodeFixProvider:68`, `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync:31`, `RH7207…Analyzer.IsOrganized:67` | All read the same list: `node.Usings`, through `GetUsings(scope)` for the compilation unit and `BaseNamespaceDeclarationSyntax`. The rewriter visits the same three scope kinds | holds |
| What stays at the top when the original first moves (FMT-USING-012 / -009) | `originalFirst.GetLeadingTrivia()`, the leading trivia of the first token of `usingDirectives[0]`; `Any(IsDirective)` | The SHARED-USING-009 predicate on the same token. In a block the predicate admits, the only directives that token's trivia can hold are non-conditional ones, so "holds a directive" in the split means exactly "header directive in front of the block" | Same span, same token. Both sides of the boundary are tested: directive vs. no directive in the header (`UsingDirectiveHeaderDirectiveTests` vs. the existing FMT-USING-009 tests) | holds |
| RH7207 checks blank lines | Delegates to `CanSafelyReorder` | Formatter lays out group boundaries only where `CanSafelyReorder` holds (FMT-USING-004) | Same predicate; parity confirmed by the probe above and by `FormatterAddsTheGroupBlankLineToABlockLedByANullableDirective` | holds |
| Qualifier "first token of the first directive" (FC-002) | `directiveIndex == 0 && token == usingDirective.GetFirstToken()` | Spec text | The member just outside it fails both with and without a header directive, as it must: a directive in front of the *second* token of the first directive (`PragmaBetweenTokensOfFirstDirectiveIsUnsafe`, `HeaderDirectiveWithPragmaBetweenTokensOfFirstDirectiveIsUnsafe`) | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Using block may be reordered | `rg -n "CanSafelyReorder\|ContainsDirectives" src`. One owner, `UsingDirectiveOrderingSafety.CanSafelyReorder`. The other `ContainsDirectives` hits (`OrderingMoveSafety`, `AccessorOrderingUtilities`, RH2001/RH3004/RH7107/RH7108/RH8201 fixes, `ReihitsuFormatterHelpers`, `ExcludedCodeUtilities`) are other decisions that do not involve using blocks | single owner |
| Which directives are conditional | `rg -n "IsConditionalDirective" src`. `SyntaxTriviaUtilities.IsConditionalDirective` is reused by the predicate, with no private copy (the implementer removed one) | single owner |
| The header kept at the top of a reordered block | `rg -n -i "header" src/Reihitsu.Formatter/Pipeline/UsingDirectives src/Reihitsu.Core/UsingDirective*.cs src/Reihitsu.Analyzer*/Rules/Organization/RH72*.cs`. Only `UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia`/`GetHeaderSplitIndex`, with one caller, `UsingDirectiveOrderingRewriter.DetachHeaderOfOriginalFirst` | single owner |
| Directive checks elsewhere in the using code | `rg -n "IsDirective\|DirectiveTrivia"` over the using phase, `UsingDirectiveOrderingUtilities`, `UsingGrouping*`, and the RH72xx analyzers: only the new line in `UsingLeadingTriviaBuilder.cs:172` | single owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Core `UsingDirectiveOrderingSafetyTests` (10 new safe, 8 unsafe, LF/CRLF) | Predicate result per member of the directive-position and directive-kind axes | `CanSafelyReorder` returns the other boolean | `Assert.IsTrue/IsFalse` on the predicate itself | adequate. `Else…`/`Elif…` cannot isolate `#else`/`#elif` from `#if`/`#endif`; see Hints |
| `UsingDirectiveHeaderDirectiveTests` (25) | Exact whole-document output | Any text difference, a second pass that changes the output, or a phase run alone that changes the output | `AssertRuleResult`: exact text, LF and CRLF, second pass, `PhaseStabilityCheck` | adequate (FC-010, FC-017) |
| `ReihitsuFormatterUsingDirectivesTests` (6 new) | Exact FMT-PIPE-030 output, i.e. the code-fix path | Text difference | `AssertOrganizes`: exact text, line endings | adequate |
| `UsingDirectiveRequirementTests.UnsafeBlockOnlyGetsOneDirectivePerLine` (input changed) | The FMT-USING-004 example, unchanged in its normative text | Reorder, or a shared line left unsplit | `AssertRuleResult` | adequate. The new input is the spec's new example |
| RH7201–RH7207 `…IsFixedWithTheDirectiveAtTheTop` / `…ReportedAndFixed` and the RH7203 trivia variants | Diagnostic location plus exact fix output | Wrong span, no fix, other text, or a diagnostic left after the fix | `VerifyCodeFixWithEndOfLine`: incremental and Fix All, re-analysis, exact text, LF/CRLF | adequate (FC-007) |
| RH7203 `CodeFixOfABlockLedByANullableDirectiveMatchesTheFormatter` | Fix equals formatter | Divergence | `VerifyCodeFixMatchesFormatter` | adequate (FC-028) |
| RH7203/RH7207 `…ReportedWithoutCodeFix` (conditional, between tokens, header plus later directive) | Diagnostic reported and no action registered | An action offered | `VerifyNoCodeFixIsOffered` | adequate (FC-009) |
| RH7203/RH7207 Fix All tests | One Fix All pass fixes the header blocks and leaves the unsafe one | Unsafe block changed, or more iterations needed | `Verify` with explicit iteration counts and the remaining diagnostic marked | adequate (FC-008) |
| RH7207 formatter tests (4 new or flipped, 2 kept) | Formatter output is clean for RH7207, or keeps the reported shape | Diagnostic left on the formatter output, or the block changed | `VerifyFormatter` / `VerifyFormatterKeepsReportedShape` | adequate (FC-013) |

Visible output (check 10): every changed or new expected value is covered.

| Shape | Covered by |
|---|---|
| Header reorder | Answer 1 |
| RH7207 reporting a header-led block for its blank line | 1A |
| Comment header + blank line + directive | 2A |
| Comment directly below the directive | 3A |
| Comment behind a blank line below the directive | 4B, whole leading trivia |
| Conditional group in front of the block | 5B |
| Group enclosing the block | Answer 4 (B) |
| `#region` case | Decision 2: answer 1 plus the unchanged FMT-BLANK-013 |

Every other variant follows from FMT-USING-012's "whole leading trivia" text, which these answers set:

- `#pragma`, `#define`, and `global` blocks;
- namespace and file-scoped blocks;
- two directives separated by a blank line;
- the copyright header with two directives.

The change doc's `Decisions` section records all of them. No rule page changed.

## Failure classes dismissed
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036: no literal, identifier, member, expression body, paired layout, or rename is touched. Reordering usings below a non-conditional directive keeps every using after that directive in every build, and `#define` stays in front of the first token (CS1032 holds).
- FC-011, FC-012: no loop is added and no neighboring phase changes. Phase stability is asserted by `AssertRuleResult`, and the two-pass probe was stable.
- FC-014: no new rule.
- FC-018: no rename.
- FC-030: the fix scope is unchanged (`OrganizeUsingDirectivesInDocumentAsync` replaces only the scope's usings).
- FC-033, FC-034: the reproductions are the issue's own input and the spec examples, run under LF and CRLF through the real fix and formatter paths.
- FC-035: per handover 05, the expected values come from the specs; the scratch worktree only listed which tests flip.
- FC-037: no text-only claim is made.
- FC-038 to FC-043: no shape is declared unsupported (answer 6A), and the precheck is unchanged.
- FC-040 specifically: the predicate and the split read directive kinds, which the documentation mode does not change. A `///` below the header stayed at the top in the probe.
- FC-044: the guard reads trivia kinds on one token. It models no build configuration: the conditional case is refused by kind, as the maintainer decided.
- FC-031: checked. The changed summaries describe the new code, and the untouched comments in `UsingDirectiveOrderingRewriter` are still accurate:
  - `SeparateDirectivesSharingALine`: "every leading trivia — which holds every preprocessor directive";
  - `DetachHeaderOfOriginalFirst`: "the header ends with a blank line" is reachable only on the FMT-USING-009 split.
- FC-032: the implementer ran the core, formatter, analyzer, CLI, and architecture projects in full after the last compiled change. No file under `src/`, `tests/`, or `tools/` starts with a directive (checked with a scan of the first non-comment line).

Classes traced and holding: FC-001, FC-002, FC-003, FC-004, FC-005, FC-007, FC-008, FC-009, FC-010, FC-013, FC-015, FC-016, FC-017, FC-019, FC-020, FC-021, FC-027, FC-028. For each, see the tables above.

## Architecture findings
_None._ One predicate decides safety for the analyzer, both fix registrations, the fix helper, and the formatter. One helper decides the header split. Nothing predicts another component's result.

## Hints
- `UsingDirectiveOrderingSafetyTests.ElseDirectiveBeforeFirstDirectiveIsUnsafe` and `ElifDirectiveBeforeFirstDirectiveIsUnsafe` always hold `#endif` (or `#if`) in the same leading trivia, so they would also pass if `#else`/`#elif` were not treated as conditional. No input can isolate those kinds in that trivia (handover 05, *Not covered*), and the kind set is the shared `SyntaxTriviaUtilities.IsConditionalDirective`. This is not a finding.
- The new sentences in RH7201-004 … RH7207-004 and RH7207-001 say "a block whose preprocessor directives all stand in front of its first directive". A directive behind the last using does not belong to the block (SHARED-USING-009), so the sentence reads correctly with that requirement, but on its own it could be read as excluding a `#pragma warning restore` behind the block. Wording only; no output effect.

## Follow-up candidates
_None._

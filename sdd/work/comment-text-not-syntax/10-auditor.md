Role: auditor
Commit: 8c4b1654745906171be4893c13b4fb4620bb7276
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `8c4b165` (checkout verified with `git rev-parse HEAD`). Base `origin/main` = `846aca462c94c0d85bc19a1d5ad9d78605590c29` (`git ls-remote origin refs/heads/main` agrees). The base is an ancestor of the head (`git merge-base --is-ancestor`). The diff is what the handovers describe: one production file (`src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs`, +6/-41), the spec and change-doc edits from `02`/`04`/`06-spec-author.md`, and the tests from `05-test-implementer.md`. `scripts/lane-check.sh behavior-change` reports `confirmed`. The working tree is clean apart from this handover.

## thoenissen/Reihitsu#966

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Checks

1. **Against the specs.** SHARED-USING-009 as clarified says a block is unsafe exactly when a preprocessor directive stands in the leading trivia of its first directive, in the leading or trailing trivia of any of its directives, or between a directive's tokens, and that a `#`-led line of a `/* */` or `/** */` comment is not one. `CanSafelyReorder` is now `usingDirectives.Any(d => d.ContainsDirectives) == false`. Roslyn's `ContainsDirectives` flag covers every token's leading and trailing trivia inside the node, the first token's leading trivia included. That is the same span the removed regex read (`usingDirectives.ToFullString()`), and the check is now by trivia kind instead of by text. A directive after the last using is in the next token's leading trivia (directives are never trailing trivia), so it stays outside, as the requirement says. Both spec examples are tested on all three surfaces: B1 is unchanged and not reported, and `# Header` is reported, fixed, and reordered. The issue's own input (`/*` / `# not a directive` / `*/` before `using System.Linq; using System;`) has the shape of A1, which is tested on Core, RH7203 + fix, and the formatter under LF and CRLF (FC-034 holds).
2. **Nothing the old guard protected is lost.** I enumerated what the regex could match that is not a `DirectiveTriviaSyntax` inside the block: disabled text, a delimited comment line, a skipped token, and a literal. Disabled text always follows its `#if`/`#elif`/`#else` in the same token's leading trivia, so `ContainsDirectives` already holds there. A line-starting `#` outside a comment is lexed as a directive (bad directives included), so it is directive trivia. A using directive holds no literal. Only comment text changes its verdict, which is the change that was asked for. `HasUnsafeTrivia` (edge trivia only) is a subset of `ContainsDirectives`.
3. **Comment text that another build reads as a directive (skeptic A9 and its class).** In the analyzed build the block holds no directive, so the block is safe. The protection is the document-level hiding-carrier check (`ReihitsuFormatterHelpers.HoldsHidingCarrier`, FMT-PIPE-033, SHARED-FIX-012), which covers `/* */` and `/** */` and `#elif`/`#else`/`#endif`. I enumerated the class along three axes: comment kind × directive word × scope. I constructed and ran four members with `scripts/apply-fix.sh RH7203` (LF and CRLF) and with `scripts/format.sh` (two passes):
   - `/** */` with `#endif`, file-scoped namespace;
   - `/* */` with `#else`, compilation unit, group opened before an `extern alias`;
   - `/* */` with `#elif`, block namespace;
   - plus the test's `/* */` with `#else`, block namespace.

   Every member is `no-fix-offered`, and the formatter skips it ("code hidden in another build configuration"). In the compilation unit without an `extern alias`, the group's `#if` stands in the first using's leading trivia, so the block stays unsafe. A string or comment that only an excluded branch opens is SHARED-FIX-012's decided gap, and the regex did not cover it either (no `#` line inside the block).
4. **More members checked.** All were run with `scripts/apply-fix.sh` (LF and CRLF) and `scripts/format.sh` (two passes, the second makes no change):
   - a `#`-led comment line inside a qualified name (`using System./*\n# x\n*/Text;`): RH7203 is fixed in one iteration, and the sort key reads token `ValueText` only (`UsingDirectiveOrderingUtilities.GetNameValue`), so the comment does not reach the key;
   - a `#` comment that leads an alias directive: RH7202 is fixed, and the comment travels with its directive (FMT-USING-008);
   - `#region`/`#endregion`-looking lines in a header comment above a static directive: RH7205 is fixed, and the header stays on top (FMT-USING-009).

   No comment was dropped or relocated against FMT-USING-006/-008/-009.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Using block may be reordered (`UsingDirectiveOrderingSafety.CanSafelyReorder`) | Full span of every `UsingDirectiveSyntax` in the scope's `Usings`: the first directive's leading trivia, every token and its trivia, and the last `;`'s trailing trivia. Checked by trivia kind (`ContainsDirectives`) | What the reorder moves (`UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`, `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync` → `OrganizeUsingDirectivesInDocumentAsync`): exactly those directives with their trivia; the header split (`SplitOriginalFirstHeaderTrivia`) works inside the first directive's leading trivia | The span equals the moved span and the removed regex's span. The qualifier "preprocessor directive" (FC-002) is now the predicate itself | ok |
| Fix offered for a safe block in a document whose comment text another build reads as a directive | Whole document (`HoldsHidingCarrier`) | Formatter whole-document skip (FMT-PIPE-033), the same definition | One definition shared by fix and formatter, unchanged by this run | ok |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Using-block reorder safety | `rg CanSafelyReorder src -g '!*Test*'`: one owner `Reihitsu.Core/UsingDirectiveOrderingSafety.cs:23`; callers RH7201–RH7207 analyzers, `UsingDirectiveCodeFixUtilities:31`, `UsingDirectiveOrderingCodeFixProviderBase:89`, `RH7207…CodeFixProvider:68`, `UsingDirectiveOrderingRewriter:76` | one owner |
| Other textual `#` / directive checks over using blocks | `rg 'IsDirective\|DirectiveTriviaSyntax\|ContainsDirectives' src -g '!*Test*' \| rg -i using`: only `RH7004…CodeFixProvider:197` (using *declarations*, a different decision); RH5114 analyzer and fix decide on line sharing, not safety; `rg '\\s\*#\|StartsWith\(.#'`: only `RH6004` (its own rule, unrelated) | no private copy |
| Hiding-carrier definition | `ReihitsuFormatterHelpers.HoldsHidingCarrier`, used by `ReihitsuCodeFixProviderBase` and the formatter entry points | one owner, unchanged |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UsingDirectiveOrderingSafetyTests` A1–A8 (Core) | a `#`-led delimited comment line in any position (header, between directives, trailing, between tokens, block or file-scoped namespace, commented-out `#if` block) leaves the block safe | `false` returned | direct call with an assertion that the fixture parses without error and holds ≥ 2 directives; LF and CRLF | adequate |
| `…Safety` B1–B3, directive between directives, `#nullable` before first, directive in a later directive's leading trivia | a real directive anywhere inside the directives makes the block unsafe, between tokens too (pins FC-001 against a narrowed guard) | `true` returned | same | adequate |
| `…Safety` `DirectiveAfterLastDirectiveIsSafe` | the other side of the boundary (FC-003) | `false` returned | same | adequate |
| RH7203 A1–A5, A7, A8 `…DoesNotPreventReportAndFix` | reported, and the fix writes exactly the formatter's output | 0 diagnostics or a different fixed text | `VerifyCodeFixWithEndOfLine`, LF and CRLF | adequate |
| RH7203 `HashLineInCommentOfBlockNamespaceDoesNotPreventReport` (A6) | reported | 0 diagnostics | `Verify` | adequate for the diagnostic; the fix text is deliberately not asserted because of the pre-existing column defect (see Hints); the formatter test asserts the full A6 reorder |
| RH7203 `HashLineInFileHeaderCommentDoesNotPreventFixAll` | two diagnostics, one-at-a-time and Fix All converge | 0 of 2, or a different text | `Verify(source, fixedSource)` on the batch base | adequate (LF only, but the CRLF single-fix members cover the line-ending axis of the same path) |
| RH7203 `HashLineReadAsDirectiveByAnotherBuildIsReportedWithoutFix` (A9) | reported, no fix | no diagnostic, or a fix offered | `VerifyNoCodeFixIsOffered`, LF and CRLF | adequate |
| RH7203 B1–B3 `…IsNotReported` | a real directive between tokens silences the rule | any diagnostic | `Verify`, LF and CRLF | adequate |
| RH7201, RH7202, RH7204, RH7205, RH7206, RH7207 `HashLineInCommentDoesNotPreventReportAndFix` | every gated analyzer and its fix see the comment as text | 0 diagnostics or a different fixed text | `VerifyCodeFixWithEndOfLine`, LF and CRLF | adequate |
| `UsingDirectiveCommentTextTests` A1–A8 | exact formatter output, a second pass makes no change, every phase stable, both `ApplyRule` and `FormatSyntaxTree`, not reported unsupported | any text difference | `AssertRuleResult(input, expected)` plus `FormatSyntaxTree`, LF and CRLF | adequate (FC-017) |
| `UsingDirectiveCommentTextTests` B1–B3 | output equals input | any change | `AssertRuleResult(input)` (asserts output == input) plus `FormatSyntaxTree` | adequate |
| `UsingDirectiveCommentTextTests` A9 | whole-document formatting returns the input | any change, or reported unsupported | `FormatSyntaxTree`, LF and CRLF, fixture parses without error | adequate |

### Visible output

Every new expected value is demanded by unchanged requirements:

- Matrix A reports, fixes, and reorders: SHARED-USING-009 limits the block refusal to preprocessor directives, as on `main`, and SHARED-DIAG-003 makes comment text text.
- The layouts follow FMT-USING-009 (A1, A4, A6–A8 header with no blank line stays on top), FMT-USING-008 (A2, RH7207 test, group blank line above the leading comment), and FMT-USING-006 (A3).
- A9 follows SHARED-FIX-012 and FMT-PIPE-033.
- Matrix B keeps `main`'s output. The other reading of the old wording would move a `#pragma` and split an `#if`/`#endif` pair, which SHARED-FIX-003 and the formatter's non-destructiveness forbid, so it is no output option (decision 2).

No rule page changed.

### Comments, scope, special cases

- FC-031: the new `CanSafelyReorder` summary describes the new decision. The regex field and its summary are gone. `rg` finds no stale mention of the regex, `HasUnsafeTrivia`, or `ContainsPreprocessorDirective` in `src`, `sdd/specs`, or `documentation`. The call-site comments are unchanged and still accurate.
- No `TODO`, no tracker numbers, and no edit outside the run's scope.
- The production diff only removes a textual check. It adds no special case and no new type.

### Failure classes dismissed

- FC-004/FC-021: the safe path is the existing one that ordinary comments already took; the probes and tests show comments kept under FMT-USING-006/-008/-009.
- FC-005/FC-020: checked under Checks 2–3; the directive side is pinned by B1–B3.
- FC-006: reordering usings changes no meaning; a comment inside a name does not reach the sort key.
- FC-007/FC-008: one iteration in every probe; Fix All is tested.
- FC-009: the fix is offered only where `OrganizeScopeUsingsAsync` reorders, through the same helper.
- FC-010/FC-011/FC-012: second pass and phase stability are asserted; the probes are stable on a second `format.sh`.
- FC-013/FC-014: analyzer, fix, and formatter share the one helper; the A9 report-without-fix holds for every rule in a hiding-carrier document (SHARED-FIX-012).
- FC-015/FC-016: one owner; no sibling textual check.
- FC-017: see Assertion adequacy.
- FC-018/FC-023/FC-024/FC-025/FC-026/FC-029/FC-036: no rename, identifier, member-kind, or key/value layout logic is touched.
- FC-022: a using directive holds no literal.
- FC-027: block namespace, file-scoped namespace, and compilation unit are all covered; nested namespaces each have their own block through the same helper.
- FC-028: the predicate reads only the node's own flags, so it does not depend on position or detachment.
- FC-030: the fix formats the scope's usings only (FMT-PIPE-030), unchanged.
- FC-032: the implementer's full-project runs come after the production commit.
- FC-033: LF and CRLF everywhere.
- FC-034: the issue shape is tested.
- FC-035: expected values come from the spec examples and `main` controls, not patched code.
- FC-037: no text-only claim.
- FC-038–FC-043: no unsupported shape is declared or touched.
- FC-040 separately: `ContainsDirectives` does not depend on the documentation mode (`/** */` structured or not holds no directive trivia).

## thoenissen/Reihitsu#1289

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Checks

- **Against the specs.** No production change, as the maintainer's answer A asks. I traced RH2109-004 against `FindCodeDirectives` (`src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs`):
  - the scanner keeps no C# state, so a line-starting `@code` + `{` (brace on the same or a later line, `\s*`) is reported in any C# extent;
  - `@*` and `<!--` with `StartsLine` skip to their own closer only (`SkipPast(…, "*@")`, `SkipPast(…, "-->")`), or to the end of the text;
  - the single-line sentence holds because `StartsLine` demands only blanks before the `@`.

  All three examples hold. The method summary already states that C# is not lexed and that a line-starting directive inside a C# comment or literal is reported, so it agrees with RH2109-004 (FC-031). RH2109-001 points to RH2109-004. SHARED-DIAG-003 names the exception and lists RH2109-004 under `Uses`. The change doc is `Accepted`, has `Visible output changes: None`, and records answer A.
- **The issue's shape.** `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` puts `@code {` on its own line inside a raw string in a `@code` host and is now tagged RH2109-004 (FC-034).
- **Visible output.** Every expected value equals `main`'s output and is covered by maintainer answer A (`00-lead.md`, *Maintainer answers*; change doc *Decisions*), including the converse misses and the closer pairing. `VerifyCodeBlockTextInsideSingleLineCommentIsNotReported` is demanded by unchanged RH2109-001. No rule page changed.

### Guard scope

_N/A — no predicate is added or changed; the scanner is untouched._

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Where a Razor `@code` directive is recognized | `FindCodeDirectives` and `_codeDirectivePattern` in the RH2109 analyzer only (`rg '@code' src -g '!*Test*' --type cs`) | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 9 retagged `…IsReported` tests | a line-starting `@code {` in a C# extent is reported, whatever the host and extent kind | a missing diagnostic | `Verify` with the exact diagnostic set | adequate |
| `VerifySpecExample…AreReported` (+ CRLF) | example 1: lines 1, 3, 7 | any other set | `Verify`, LF and CRLF | adequate |
| `VerifySpecExample…HidesTheRestOfTheFile` (+ CRLF), `VerifyHtmlCommentOpenerLine…HidesTheRestOfTheFile`, `VerifyRazorCommentOpenerLineInsideVerbatimString…` | a converse opener hides the rest of the file | an extra diagnostic | `Verify` | adequate |
| `…IsNotClosedByHtmlCommentCloser` / `…IsNotClosedByRazorCommentCloser` and `…EndsAtRazorCommentCloser` / `…EndsAtHtmlCommentCloser` | closer pairing, both sides of the boundary for each opener (FC-003) | a diagnostic where none is expected, or a missing one | `Verify` | adequate |
| Host members `@{ }`, `@if`, brace on the next line | the host and brace axes | a missing diagnostic | `Verify` | adequate |
| `VerifyCodeBlockTextInsideSingleLineCommentIsNotReported` | a single-line comment's text is not reported | an extra diagnostic | `Verify` | adequate |

### Failure classes dismissed

- FC-001–FC-003, FC-015/FC-016: no guard changes; one owner.
- FC-022/FC-027: covered as test axes (extent kinds, hosts).
- FC-033: CRLF tested.
- FC-034: issue shape tested.
- FC-035: expected values are `main`'s output as decided by answer A.
- FC-031: the summaries no longer say "pins today's behavior"; `VerifyFunctionsBlockIsNotReported` keeps its characterization wording, correctly, because it is thoenissen/Reihitsu#1452's case.
- Code-fix, formatter, unsupported-shape, and rename classes (FC-004–FC-014, FC-018, FC-023–FC-026, FC-028–FC-030, FC-036, FC-038–FC-043): RH2109 has no fix, and `.razor` is never formatted.

## Whole run

### Architecture findings

_None._

### Hints

- **Pre-existing, unrelated to the guard (already a follow-up candidate in `00-lead.md`).** RH7203's fix for a block-scoped namespace whose first using carries a leading comment writes the moved directive at column 0; the formatter writes column 4. Confirmed on head with the A6 input (`scripts/apply-fix.sh RH7203`, LF and CRLF: `+using System.Linq;` at column 0 under `    using System;`). The test implementer showed the same on `main` with `// Header` and `/* Header */`, so the defect is reachable today for ordinary comments. This run only adds the rare `#`-led subset to its inputs. It is layout only (no trivia loss), so it is not a blocking finding of this run.

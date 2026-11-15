Role: auditor
Commit: 8333c46d3e240bfe9cd9f27da2302a48b9157603
Verdict: findings — #1140: findings (#2); #1687: findings (#1); #1748: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. M1–M9 and D1–D24 in `00-lead.md` are taken as binding.

## Scope

- Full audit. Head `8333c46` (checked out, clean tree). Base `origin/main` = `4838e9a03f0736d9e4593286d0f251f3a92fb704`. `git ls-remote origin refs/heads/main` returns the same SHA, and it is an ancestor of the head (merged).
- The diff matches the handovers. It has 93 files: 33 production files (+281/−683, `RootListPosition` deleted, no new type), 29 test files, 20 spec/change-doc files, and `sdd/work/`.
- The production paths are the ones 07-implementer lists, and the Fix All switch is D21.
- The cleaner's Release build (0 warnings) and the roles' focused test results are trusted. No test was re-run.
- Probes were run with `scripts/apply-fix.sh` under LF and CRLF, on fixtures in `/tmp/claude-0/-home-user-Reihitsu/ba68e92d-4c14-5cfe-a5c5-81fb8298ae73/scratchpad/au11/` (outside the repository).

## thoenissen/Reihitsu#1140

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider.cs` (`FormatsThroughFormatter => true`, `RegisterCodeFixesCoreAsync` `FindUnsupportedSyntax(declarationNode)`, `CanApplyCodeFix` summary); `src/Reihitsu.Analyzer.Test/Formatting/RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzerTests.cs` `NoFixIsOfferedInAFileWithASyntaxError` | FC-042, FC-043, FC-031 | – | PR-introduced | See *Finding #2* below the table. | Either ask an output question: offer the RH5113 fix in a file with a syntax error and for a declaration that holds a refused shape, then delete both guards, the test, and its tag. Or keep the withholding as a decision written into RH5113's spec, rename the guards' reason, and fix the comments and the test summary. D22 keeps `main`'s output, but nothing records it outside `00-lead.md`. |

**Finding #2 — what goes wrong.** The fix no longer formats anything. Both of its steps are text edits: `FormatMemberOnLinesOfItsOwnAsync` with `MoveSemicolonAsync`. Yet it keeps the formatter's guards:

- `FormatsThroughFormatter => true` withholds the fix in a file with a syntax error. SHARED-FIX-011 covers only a fix "whose result a formatter entry point lays out", so it no longer covers this one.
- `FindUnsupportedSyntax(declarationNode)` asks about a node no formatter receives (SHARED-FIX-010, FC-043). It now serves only the declared shapes (FC-042), so it withholds a safe edit. Counterexample:

  ```csharp
  int /* c */ _a
      ;
  ```

  This is FMT-UNSUP-004, a comment inside the variable declaration. The diagnostic is reported (the gap holds only a line break), and no fix is offered. Removing the gap in front of `;` would never touch the comment.
- The `CanApplyCodeFix` summary still says it "mirrors the per-token guard used by the formatter's line-break rewriter so the fix is offered whenever the formatter would also collapse the join" (FC-031).
- The test `NoFixIsOfferedInAFileWithASyntaxError` still describes "the fix, which lays out its result through the formatter" and is tagged SHARED-FIX-011.

RH5113's spec names neither withholding.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Target leading trivia written back (`FormatNodeInDocumentAsync`, `ReihitsuFormatter.cs:250`) | The original first token's whole leading trivia, every non-root target | `FormatNodeInDocumentWithContextAsync:352` (same write-back); FMT-PIPE-018 "in every case" | Agree. The `targetNode == root` branch does not write back. No caller passes the compilation unit: all 22 call sites in `src/Reihitsu.Analyzer.CodeFixes` pass a statement, member, expression, or list node | ok |
| First-token column (`ReihitsuFormatterHelpers.ComputeDocumentColumn`) | `token.SpanStart - lineStart`, BOM excluded | FMT-PIPE-017 (every character one column, a tab included); `main`'s `prefix` computation with `countsReplaced: false` | Equivalent to `main`'s non-replacing branch (same line start, same BOM rule) | ok |
| RH5308 target climb (`GetFormatTarget`: `Parent is ConditionalExpressionSyntax`) | Direct conditional parents only | Analyzer `RH5308…Analyzer.cs:130` outermost test (`conditional.Parent is ConditionalExpressionSyntax`); RH5308-003 "not itself a branch" | Same predicate; a parenthesized inner conditional is its own outermost on both sides (probe `var u = a ? (b ?⏎1 :⏎2) : 3;` converged) | ok |
| RH5308/RH5303–RH5307/RH5206 `FindUnsupportedSyntax(target)` | The node the fix formats | `FormatNodeInDocumentAsync(document, target)` in each `ApplyCodeFixAsync` | Same node from the same `TryGetFormatTarget`/`GetFormatTarget` (SHARED-FIX-010) | ok |
| RH5304 target (`AssignmentExpression` whose parent is an `InitializerExpression`) | Ancestors of the token at the diagnostic start | RH5304-003 location: the whole member assignment | The diagnostic starts at the assignment's first token, so the first match is the reported assignment | ok |
| RH5111 `GetFormattingNode` (innermost of assignment/declarator/property/parameter/enum member) | Ancestors of `FindNode(span)` | Analyzer location `= node.GetLocation()` of exactly those five kinds (`CheckAssignment`) | `FindNode` returns the reported node; the innermost match is it | ok |
| RH5113 `CanApplyCodeFix` (comments in the gap) | Trailing trivia of the previous token plus leading trivia of `;` | Analyzer `WouldJoinAcrossUnjoinableTrivia` (comments, directives, disabled text) | The fix guard is narrower, but the analyzer never reports a directive or disabled text in the gap, so the text edit cannot drop one | ok (FC-005 checked) |
| RH5113 `FindUnsupportedSyntax(declarationNode)`, `FormatsThroughFormatter` | The declaration | none: no formatter call remains | No counterpart | finding #2 |
| `FluentChainCodeFixHelper.FormatChainAsync` (fallback deleted) | Context node from `GetContextNode` | SHARED-CHAIN-009 (M2) | One path per case: the context entry point, or the node entry point without a context | ok |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Leading trivia of a node-in-document target | `rg -n "originalFirstToken.LeadingTrivia" src` finds `ReihitsuFormatter.cs:250` (`FormatNodeInDocumentAsync`), `:352` (context entry point), and `:794–795` (FMT-PIPE-029 split exception). `rg "WithLeadingTrivia\((original\|target\|node)\w*\.(GetLeadingTrivia\|LeadingTrivia)"` in code fixes finds only RH3001 (unrelated operand trivia) | one owner |
| Root preceding-token and list-position facts | `rg -n "RootPrecedingToken\|RootListPosition\|PrecedingTokenFacts.Resolve" src sdd/specs AGENTS.md` returns no hit | removed everywhere |
| Chain write-back | `FluentChainCodeFixHelper.FormatChainAsync` only | one owner |
| Node a list/conditional/switch fix formats | Each fix's `TryGetFormatTarget`/`GetFormatTarget`; one per rule, the same node for registration and apply | ok |
| Semicolon join of RH5113 | `MoveSemicolonAsync` (text) and the formatter's FMT-BREAK-009 join | see Hints |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterNodeBlankLineTests` (65 rewritten and 5 new) | Leading trivia written back as in the input; target formatted | Any byte difference, under LF or CRLF; a second pass that changes the output | `AssertFormatsTarget`: exact text, both endings, `AssertUsesLineEnding`, second pass | adequate |
| `KeepsWhitespaceOnBlankLineAboveTarget`, `KeepsForeignLineBreakInLeadingTriviaOfTarget` | Whitespace-only blank line and foreign break kept | Normalized trivia | `FormatTarget`, both endings, exact text | adequate (no line-ending assertion, by design: the foreign break is kept) |
| `ReihitsuFormatterNodeColumnTests.StatementTargetIndentedWithTabs…` | A tab counts as one column (FMT-PIPE-017) | Block at column 8 | `AssertFormatsTarget` / `…WithForeignLineBreaks` | adequate |
| RH5110/5111/5113/5401/5403/5404/5408/5412/5417/2001 leading-trivia tests | No blank line inserted; comment and documentation comment unchanged | An inserted blank line, or a three-line `<summary>` | `VerifyCodeFixWithEndOfLine` / `Verify`: exact fixed text, LF and CRLF, remaining diagnostics checked | adequate |
| RH5206, RH5303–RH5308, RH5112, RH5201 "code around stays" tests | The code in front of and behind the construct is unchanged; the construct is laid out from its first-token column | `var r = ` respaced; a wrapped conditional; a joined outer chain | Same verifiers | adequate |
| RH5303/5305/5308 nested Fix All pins, RH5306/5308 `FixAllCorrectsEveryDiagnostic`, `VerifySiblingConditionalsInOneStatement…` | One Fix All application corrects every diagnostic (SHARED-FIX-002) | A second iteration needed | `NumberOfFixAllIterations = 1` | adequate |
| `RH5308…SpecExampleFixIsOfferedForAConditionalInAnAssemblyAttribute`, `…InAModuleAttribute` | Fix offered and lays out the conditional (M9) | No fix; or a different layout | `VerifyCodeFixWithEndOfLine` (module: LF only) | adequate |
| `RH5113…NoFixIsOfferedInAFileWithASyntaxError` | Fix withheld with a syntax error | Fix offered | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | the assertion is strong enough, but its premise is gone (finding #2) |
| `RH5201…VerifyFixMovesBreakInsideOperatorOfChainInConditionalExpression`, `…AlignsInnerChainInsideWrappedOuterChain` (08) | Chain-only layout (M2) | Conditional or outer chain changed | `Verify` | adequate |

### Visible output is decided (check 10)

Every changed expected value maps to one of these. I sampled every test 06 and 08 list, and the formatter class diff.

- **Kept leading trivia.** Unchanged SHARED-FIX-006 core sentence and *Blank lines* bullet, plus M1.
- **Tabs counted as one column.** Unchanged SHARED-FIX-007 first bullet. Covers the RH3202, RH5403, and RH5112 tab tests, change 10, and the probe `→→var x = a.Trim()⏎           .Length;`.
- **The chain only.** M2.
- **The switch expression only.** M4.
- **The list or conditional construct only.** M7.
- **A fix offered where another shape in the statement is refused.** M8.
- **The assembly/module attribute.** M9.
- **The RH5308 stair at 28.** Unchanged RH5308-002 (D17).
- **RH5111/RH5113 scope.** Unchanged SHARED-FIX-006 (change 9).

No rule page changed. No undecided output was found.

### Failure classes dismissed

FC-001, FC-002, FC-005, FC-015, FC-043, and FC-031 are handled in the tables above and in finding #2.

- **FC-004 / FC-019 / FC-020 / FC-021** (trivia in a joined gap).
  - The entry-point write-back restores the original leading trivia instead of the phases' copy.
  - Every phase that could move leading trivia of the root's first token elsewhere was checked: the documentation phase relocates only comments written behind code inside the root, and adds line breaks to the token in front; the attribute-list split and merge keep trivia on the list tokens; field splitting of a root field does not run in the code-fix entry point. None moves it, so no comment is dropped or doubled.
  - RH5113's text edit removes only a gap the analyzer guarantees holds no comment, directive, or disabled text.
- **FC-006 / FC-022 / FC-023 / FC-024 / FC-025 / FC-036 / FC-018** (meaning-changing rewrites). No token, literal, identifier, or rename logic changed. The expression-body base still converts through `ConvertBodyAsync`, and only the member's shared-line gap is new.
- **FC-007** (convergence). Every probe converged, under LF and CRLF:
  - RH5308 in an argument, a named argument, a lambda, a parenthesized branch, a field initializer, an expression body, an interpolation hole, and a switch arm;
  - RH5303 in an argument, a nested element, a field, an expression body, a lambda, and an array element;
  - RH5305 in an argument and a switch arm;
  - RH5206 in an argument, nested, and in a lambda;
  - RH5307 in an argument;
  - RH5304 in an argument;
  - RH5306 with two patterns on one line;
  - RH5112/RH5201 with a chain in a conditional, a chain behind misspacing, an inner chain inside an outer chain, and a chain behind tabs.
- **FC-008** (Fix All). `DocumentOrderFixAllProvider` re-registers each fix on the output of the fix in front of it. The nested and same-line pins assert one iteration.
- **FC-009** (offered no-op). Each narrowed fix asks about exactly the node it formats, and no RH5308-006 guard is left.
- **FC-010 / FC-011 / FC-012 / FC-013 / FC-014** (formatter stability). Document-level formatting is unchanged:
  - `PrecedingTokenFacts.From(None)` equals the `default` that `Resolve` returned for every entry point other than `FormatNodeInDocumentAsync`, so the removed root branches ran only for that entry point.
  - `IsBlockStatementAfterStatement` is null-parent safe.
- **FC-016 / FC-027** (sibling shapes, enclosing scopes). For #1140, see the FC-007 probes. For #1687, see finding #1.
- **FC-017** (assertion strength). See the table above.
- **FC-026** (re-resolved tokens). RH5113 resolves the semicolon from the annotated node of the updated document.
- **FC-028** (detached vs document-scoped). `FormatNode` still decides its node's leading trivia, by design (FMT-PIPE-012, FMT-PIPE-024 "the caller of `FormatNode` decides"). `FormatNodeOnStatementBehindLocalDeclarationAddsNoBlankLine` pins it.
- **FC-029** (key/value layout). Not involved.
- **FC-030** (a fix formats too much). This is the issue itself. The fix narrowed it as specified, and D6 records the remaining RH5110/RH5403/RH5404 headers.
- **FC-032 / FC-033 / FC-034 / FC-035 / FC-037** (evidence). Tests were run after the last compiled change (09). The scenarios are the issue's own inputs. Expected values come from the specs (06/08 state no patched copy was used). No text-only claim is made.
- **FC-038 – FC-042** (unsupported shapes). No shape is declared. FC-042 applies only to RH5113 (finding #2), and the RH5308-006 guard was deleted (M9).

### Architecture findings

_None new._

The leading-trivia ownership D5 is now one owner (the entry points).

### Hints

- RH5113's `MoveSemicolonAsync` writes the semicolon join itself (an empty gap), next to the formatter's FMT-BREAK-009. It is trivially in parity, so it is no finding, but it is a second statement of the join.
- A hanging collection initializer stays inside an object initializer on one line: `M(new D { X=1, Items = {⏎1,⏎2⏎} });`, with the braces at column 40 and the elements at 44. It converges and is decided by M7 and RH5304-004. The lead may want it visible in the PR's review notes.

## thoenissen/Reihitsu#1687

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (`ReplaceAnnotatedPropertyWithFieldAsync`); `Rules/Layout/RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider.cs`; `Base/EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.cs:56` (RH5412–RH5416); `Rules/Layout/RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.cs` (property target) | FC-016 | – | same requirement | See *Finding #1* below the table. | Same as D8 for these members: route each through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (a call-site change, as for RH5401/RH5408), or withhold the fix when `SharesLineWithSiblingMember` holds (the rare class). Otherwise declare them out of scope and link #1687 with `Refs`. |

**Finding #1 — what goes wrong.** These are members of #1687's class, "a fix that changes a member leaves a sibling member on its line" (SHARED-FIX-007 *Shared line*), that the run does not handle. 02-skeptic left them unmeasured. Each fails on the head, under LF and CRLF:

- **RH2001**, both sides. `private int _a; private int Value { get; set; } private int _b;` becomes `private int _a; private int _value; private int _b;`.
- **RH5110**, sibling in front. `private int _a; public void M<T>() where T : class⏎{⏎}` becomes `private int _a; public void M<T>()⏎                        where T : class⏎                    {⏎                    }`. The body is laid out from `M`'s column 20, the same defect the run fixed for RH3203.
- **RH5110**, sibling behind. `public void N<T>() where T : class⏎{⏎} private int _c;` keeps `} private int _c;`.
- **RH5412 and RH5413** (shared base), sibling behind. `public class A { } public class B` becomes `public class A; public class B`, and `struct` behaves the same way.
- **RH5111**, property target, sibling in front. `private int _a; public int P { get; } =⏎1;` becomes `private int _a; public int P { get; } = 1;`.
- **RH5111**, property target, sibling behind. `public int P { get; } =⏎1; private int _b;` becomes `public int P { get; } = 1; private int _b;`.

These members hold on the head:

- RH3202, RH3203 (both sides), RH3205, RH3207, RH5401, RH5404 (both sides), RH5408, RH5113, RH5417, RH5513, and RH5517 (tests in 06);
- RH3206, through the shared base (not run separately, D19).

RH3208 (a statement) is out of the class (D9). Enum members of RH5111 are left out: see Hints.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Expression-body base routes `MemberDeclarationSyntax` nodes through the helper | `node is MemberDeclarationSyntax` | The helper's `lineBeginsWithMember` and `SharesLineWithSiblingMember` (whitespace-only gap, same list) | Accessors and local functions fall back to `ConvertBodyAsync`, as before | ok |
| Attribute base after-break (`placementMode == SeparateLine && attributeList.Parent is MemberDeclarationSyntax`) | The placed member, found through the close-bracket annotation | The helper (behind side) and the base's private before-break (in-front side) | SingleLine mode is used only by RH5523 (parameters) and RH5530 (accessors), which are not members. The assembly/module lists have a compilation unit as parent | ok; the before-break is the known second owner (D23) |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line break in front of or behind a member that shares its line (SHARED-FIX-007) | `rg -n "SharesLineWithSiblingMember\|FormatMemberOnLinesOfItsOwnAsync\(" src -g '!*Test*'` finds `BlankLineCodeFixUtilities` (helper, l.110 and l.149) and its callers: RH3202, RH3203, the expression-body base, RH5113, RH5401, RH5404, RH5408, RH5417, and the attribute base (behind). The attribute base's private before-break is a second owner | two owners for the in-front break, already decided (D23, FU5). The member fixes that never call the helper are finding #1 |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 10 `…ALineOfItsOwn` tests of 06 (RH3203 ×2, RH3205, RH3207, RH5113, RH5401, RH5404, RH5408, RH5513, RH5517) | The changed member and its sibling each start a line; the member is laid out from its line's column | The sibling is left on the line, or the body is laid out from the shared column | `VerifyCodeFixWithEndOfLine`: exact text, LF and CRLF | adequate |

### Failure classes dismissed

- FC-019/FC-021: a comment in the gap to the sibling is a rare class that is not pinned (D18). The helper requires a whitespace-only gap, so such a line stays shared and nothing is dropped.
- FC-008: two reported members on one line with `BatchFixer` was dropped as a rare class (D24).
- All other classes: as under #1140.

### Architecture findings

_None new._ The in-front break has two owners, recorded as D23/FU5.

Finding #1 shows the structure behind the class: each member-changing fix has to opt into the helper. The members that do not opt in are the ones that fail.

### Hints

- RH5111 also formats enum members. Enum members commonly share a line (`A, B =⏎2, C`), and reading SHARED-FIX-007 literally would separate them. This is FU1's reading question (gap-only fixes), not a finding.

## thoenissen/Reihitsu#1748

### Findings

_None._

The RH5403 and RH5404 fixes keep the target's leading trivia through the entry point (D5), as M1 asks. The tests cover these inputs, under LF and CRLF:

- the issue's input;
- no comment;
- a switch section;
- a comment in another column;
- a blank line behind `{`;
- two blank lines;
- a directive;
- tabs;
- an RH5404 comment and an RH5404 documentation comment.

### Guard scope

_N/A — no guard specific to #1748; it is the entry-point write-back under #1140._

### Policy ownership

_N/A — see #1140 (one owner)._

### Assertion adequacy

_N/A — the 11 tests in 06 use `VerifyCodeFixWithEndOfLine` with exact text, and are covered by the #1140 table._

### Failure classes dismissed

As under #1140.

### Architecture findings

_None._

### Hints

_None._

## Whole run

- Lane: behavior-change, with the change doc `sdd/changes/2026-11/fix-layout-scope.md` (Accepted). No path outside the run's scope:
  - the BOM added to `RH5113…CodeFixProvider.cs` matches `.editorconfig` `charset = utf-8-bom`;
  - `BlankLineCollapserTests.cs` changes only the constructor.
- `TODO`s, issue numbers, and commented-out code: none in the changed source.
  - `git diff 4838e9a HEAD -- src | grep "^+" | grep -E "TODO|issue #|PR #|#[0-9]{3,}"` found nothing.
  - Changed XML summaries describe the new code, except RH5113 (finding #2).
- Probe outputs: `/tmp/claude-0/-home-user-Reihitsu/ba68e92d-4c14-5cfe-a5c5-81fb8298ae73/scratchpad/au11/*.out`.

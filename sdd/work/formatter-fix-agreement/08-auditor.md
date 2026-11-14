Role: auditor
Commit: 01672a19c51926de0f233c7c1075b5918925af3a
Verdict: findings (#1529: one low finding; #1543: no finding)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1529

### Scope

Head 01672a1 (checked out, clean tree), base `origin/main` b34c0f1 (fetched; equals the SHA in my prompt and is an ancestor of head). Full audit. The diff is what `03-spec-author.md`, `04-test-implementer.md`, and `05-implementer.md` describe: RH5111-006 in `sdd/specs/analyzer/RH5111.md`, the ledger line, the change doc, one production line plus summary in `RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.GetFormattingContextNode`, and the RH5111/RH5201/RH5112 tests.

Probes (scratchpad fixtures, `scripts/apply-fix.sh RH5111` under LF and CRLF, `scripts/format.sh` for the whole-document comparison, `apply-fix.sh RH5201`/`RH5112` on inputs and fix outputs):

| Input | RH5111 fix | Whole document | Verdict |
|---|---|---|---|
| `x.N(1,⏎ 2)?.B⏎ .C =⏎ 5;` (anchor on the receiver's first line, receiver aligned and misaligned) | `.C = 5` in column 9 under `.N`; `2)?.B` kept | `?.B` moved onto its own line, `.C` column 9 | the code in front is not changed (RH5111-006); `2)?.B` is reported by RH5201 on the input already — holds |
| `Get(() =>⏎{⏎return a;⏎})?.B⏎ .C =⏎ 5;` (misindented lambda receiver) | `.C` under `?` of `})?.B` | same column | holds; RH5201 silent on the result |
| `void M(A a) => a?.B⏎ .C =⏎ 5;` | `.C` under `?` | (converted to block) same relative column | holds |
| `this._a?.B⏎ .C =⏎ 5;` | `.C` under `.` of `._a` | same | holds |
| value is a chain, an object creation, a lambda | value laid out as the document | same | holds |
| `M(a?.B⏎ .C =⏎ 5);`, `a?[i⏎ + 1]⏎ .C`, `a?.B!.B⏎ .C` | converge in one iteration | — | holds |
| comment in the context, not the target: `a /* x */?.B`, `a⏎ // y⏎ ?.B` | converges; comments untouched (not written back) | — | holds (rare: comment inside a chain) |
| `a ?.B⏎ .C`, `a⏎ ?.B⏎ .C`, tab-indented `\t\ta?.B` | see *Hints* | — | inputs that violate another rule |

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Core/FluentChainBoundary.cs` (`RootIsConditionalAccessBranch`, its constructor parameter, the second argument in `From`); `src/Reihitsu.Core/FluentChain.cs` `IsConditionalAccessBranch` (the `node.Parent == null ? boundary.RootIsConditionalAccessBranch` branch); `sdd/specs/formatter/pipeline.md` FMT-PIPE-024 row *Conditional-access branch* and the bullet that names "the chain-continuation and conditional-access-branch facts" | — (check 7, dead branch; `AGENTS.md`, *Supported inputs*) | – | PR-introduced | The *conditional-access branch* fact is read only when a `FormatNodeInDocumentAsync` root is an assignment that is a conditional access's `WhenNotNull` (`ContinuesChain`'s assignment arm with the assignment as root). Its only callers were the RH5111 fix and the RH6023 fix (`node-root-detached.md`, audit stage). RH6023 no longer formats (SHARED-OPSPACE-004), and this PR moves the RH5111 fix for exactly those assignments to `FormatNodeInDocumentWithContextAsync` with the conditional access as context, where the entry point supplies no chain facts (FMT-PIPE-024 table, *none*). Every other caller of `FormatNodeInDocumentAsync` (listed in *Policy ownership*) passes a statement, member, argument list, lambda, object creation, outermost chain node, or a binary expression, none of which can be an assignment behind `?`. No caller reaches the fact any more, and no test sets it: the three tests tagged FMT-PIPE-024 that exercised it were replaced by RH5111-006 tests, and `rg FluentChainBoundary` finds no test. The change doc's alternative E is rejected "to delete a node-level fact" — that fact is now deletable without E. | Delete the flag, its boundary branch, and the FMT-PIPE-024 row (a few lines in components the run's change made dead), or record it as a follow-up; the lead decides. `RootContinuesChain` stays: RH5301 formats an object creation that can be the expression of a member access. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the fix formats the assignment with the chain as context (`GetFormattingContextNode`) | the assignment's ancestors while `FluentChain.ContinuesChain(parent, child, default)` holds | the formatter's outermost-chain rule, `FluentChain.IsOutermostChainNode`/`ContinuesChain`, read by `ChainLineBreakRewriter` and `MethodChainAlignmentContributor` | same function, same boundary (`default`, the node is in its tree) | agree |
| The spec qualifier "a link of the left side starts a line" (FC-002) | not a condition of the code: every assignment behind `?` gets the context | RH5111-004 "in place" for the unwrapped case | output is the same either way (`VerifyFixJoinsTheValueOfAnAssignmentBehindAConditionalAccessWhoseLinksShareALine`, `VerifyFixJoinsTheValueOfAnAssignmentToABareBinding` green; both sides of the qualifier tested) | agree |
| Unsupported refusal at registration | `FindUnsupportedSyntax(GetFormattingContextNode(node))` | SHARED-FIX-010: for `FormatNodeInDocumentWithContextAsync`, the context | same node at register and apply time (FC-043) | agree |
| Join-blocking trivia | `ContainsJoinBlockingTrivia(assignment)`: the assignment's interior | what the context entry point writes back: the target only, with its first token's leading trivia and its last token's trailing trivia from the document | comments or directives outside the interior are never written back, so they cannot be moved | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether an assignment continues the chain of its left side | `rg -n "WhenNotNull ==\|IsConditionalAccessBranch" src --glob '!*Test*'`: `FluentChain.ContinuesChain`/`IsConditionalAccessBranch` (Core) only; the fix calls `FluentChain.GetOutermostChainNode` | one owner |
| Column of a wrapped chain link | `MethodChainAlignmentContributor` (indentation phase); the fix delegates through the context entry point and predicts nothing | one owner |
| Context for a code fix that formats part of a chain | RH5111 `GetFormattingContextNode` (assignment → outermost chain node); `FluentChainCodeFixHelper.GetContextNode`/`FormatChainAsync` (chain → statement or member, with a fallback to formatting the whole context when the context's front is not formatted) | different targets, not the same decision; the fallback's class is inputs that violate another rule (see *Hints*) |
| Callers of `FormatNodeInDocumentAsync` that capture `FluentChainBoundary` | `rg -n "FormatNodeInDocumentAsync\(" src/Reihitsu.Analyzer.CodeFixes`: FluentChainCodeFixHelper (outermost chain, statement/member context), BlankLineCodeFixUtilities (member), ExpressionBodyToBlock base, EmptyType base, RH5404, RH5403, RH5102, RH5101 (argument list), RH5307/RH5303/RH5304/RH5305/RH5306 (statement, equals-value, arrow, lambda, arm, argument), RH5110, RH5206, RH5205, RH5401, RH5301 (object creation), RH5308 (statement/member), RH5408, RH5113, RH5302 (outermost logical expression), RH2001, RH3203, RH3004, RH3208, RH5111 (non-chain assignments). No target can be an assignment behind `?` | basis of finding #1 |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 17 RH5111-006 `Verify…` tests (`.C` columns 8/9/10/12/13/14, element binding joins, nested, invoked binding, `=` wrapped, two links, `a.B?.B`, `a?.B?[0]`, lambda body, comment above) | exact fix output, Fix All, re-analysis silent | any column or join that differs; a change in front of the assignment (FC-030) | `VerifyUnderBothLineEndings` (LF and CRLF, Fix All, exact text) | adequate |
| `VerifyFixBehind…MatchesFormattingTheDocument` (5) | fix output equals `FormatSyntaxTree` of the same document; fixed source not reported | any divergence from whole-document formatting (FC-028) | `VerifyCodeFixMatchesFormatter`, LF and CRLF | adequate |
| RH5201/RH5112 `VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument` | the layout the fix now writes is silent under both rules (FC-007) | a report on the result | `Verify(source)` LF and CRLF | adequate |
| Boundary tests (`…RequirementBoundaries…`, `…LinksShareALine`, `…BareBinding`, `…NotBehindAConditionalAccess…`) | the other side of each qualifier is unchanged (FC-003) | a change of the unwrapped or non-`?` output | `VerifyUnderBothLineEndings` | adequate |
| `VerifyNoFixIsOffered…` (comment line, comment behind binding, directive) | RH5111-005 withholds the fix | an offered fix | `VerifyNoCodeFixIsOffered` | adequate |

### Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021 — the fix writes back only the assignment; comments and directives inside it withhold the fix (three tests), those outside it are not written back (probed: `a /* x */?.B`, a comment line in front of `?.B`).
- FC-006, FC-022, FC-023, FC-024, FC-029, FC-036 — layout only; no literal, identifier, member kind, key/value, or rename involved.
- FC-025 — an expression-bodied method holding the assignment probed; holds.
- FC-007 — every probe converges; the fix's results are silent under RH5201/RH5112 except where the code in front, which the fix keeps by RH5111-006, is reported already on the input (*Hints*).
- FC-008 — Fix All test with six diagnostics; nested assignments in one chain are rare.
- FC-009 — RH5111 reports only a misplaced `=` or value, which every offered fix changes.
- FC-010–FC-014 — no formatter phase changed for this issue; no new rule.
- FC-016, FC-026, FC-027 — every chain shape and nesting of the skeptic's matrix is tested; arguments, lambda bodies, and expression bodies probed.
- FC-028 — five parity tests.
- FC-030 — the context entry point writes back only the target (`ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`).
- FC-031 — the changed method's summary describes the new code.
- FC-032 — no project result reused; the validator runs after me.
- FC-033, FC-034, FC-035 — tests run under both line endings on the reported shape; expected values from the spec and unchanged whole-document formatting; failures before the implementation recorded in `04-test-implementer.md`.
- FC-037 — no text-only claim.
- FC-038–FC-042 — no shape declared unsupported. FC-043 — see *Guard scope*.
- Tab-width matrix — no raw-string hole column involved; a tab-indented input is under *Hints*.

### Architecture findings

_None._ The fix reads the chain decision from the formatter through the context entry point; it neither predicts a column nor widens what it writes back (the skeptic's two shapes to watch in `02-skeptic.md` are both absent).

### Hints

- **Receiver code that whole-document formatting rewrites.** The fix keeps the code in front of the assignment (RH5111-006) and lays the target out from the formatted context. Where the formatter would change that code, the result can differ from whole-document formatting: `a ?.B⏎ .C` gives `.C` in column 9 under the formatted `?`, one left of the document's `?`; `a⏎ ?.B⏎ .C` gives `.C` in column 9 while `?.B` stays in column 12, and RH5201 then reports it (RH5112 reports the input); `x.N(1,⏎ 2)?.B` keeps `?.B` behind `2)`, which RH5201 reports on the input already; a tab-indented line counts the kept tabs as one column each (FMT-PIPE-017), which RH5201 also does. Every such input violates another rule (RH5201, RH5112, a spacing rule, RH5601), so it is a rare class, and `main` placed `.C` under the assignment's first token there, no closer to the anchor. Not a finding; `FluentChainCodeFixHelper.FormatChainAsync` handles the comparable class for chain fixes with a whole-context fallback, which RH5111 does not need for common code.
- The implementer's newly withheld class (an unsupported shape in the receiver in front of `?`, now inside the context the fix asks `FindUnsupportedSyntax` about) is rare and belongs in the PR's *Review notes*, as `00-lead.md` already records.

## thoenissen/Reihitsu#1543

### Scope

Same head and base; full audit. The diff is what the handovers describe: FMT-BLANK-020 widened, one example added, the change doc; `BraceBlankLinePolicy.RemovesBlankLinesBehindOwnLineComment` deleted, `BlankLineTokenCleanupRewriter.VisitToken` runs `RemoveBlankLinesBehindLastOwnLineComment` under `RemovesBlankLinesDirectlyBefore` in the same order as before (FMT-BLANK-020 removal, then FMT-BLANK-018 glued comment); summaries in `RootConstruct`/`FormattingContext`; the regression and RH5024–RH5027 round-trip tests.

The old FMT-BLANK-020 set was a subset of `RemovesBlankLinesDirectlyBefore`; the new set adds exactly the array-initializer, extension-block, interpolation, and property-pattern braces, the assigned collection initializer's `}`, and `else`/`catch`/`finally`/do-`while`. Probed with `scripts/format.sh` twice (stable): `else if` behind a comment, a hole of a verbatim interpolated string, a nested property pattern in a switch-expression arm, a nested `new[]` element, `/* */` before `catch … when`, two comment lines before do-`while` — each loses only the blank line behind the last comment — and a `while` statement behind a comment and a blank line, which keeps it (statement rules).

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Tokens behind an own-line comment without a blank line (FMT-BLANK-020) | `RemovesBlankLinesDirectlyBefore`: token kind; for `{` whether it opens a block statement after a statement; for `while` whether its parent is a do statement | RH5024 (`}`), RH5025 (`{`, excluding the same block-statement brace through the shared `BlankLineSpacingPolicy.IsBlockStatementAfterStatement`), RH5026 (`else`/`catch`/`finally`), RH5027 (do-`while`) | the formatter's set covers every token the four rules report; the FMT-BLANK-017 brace is excluded on both sides by the same predicate | agree |
| Which blank lines go | `RemoveBlankLinesBehindLastOwnLineComment`: the last content of the gap is an ordinary comment that starts its line and no content is on the token's line | FMT-BLANK-011 (blank line above the first comment), FMT-BLANK-018 (glued comment), FMT-BLANK-009 (directive last) | unchanged predicate; tests pin the comment-then-directive and directive-then-comment gaps and two comments | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Token set for FMT-BLANK-007/008/018/019/020 | `rg -n "RemovesBlankLinesDirectlyBefore\|RemovesBlankLinesBehindOwnLineComment" src --glob '!*Test*'`: `BraceBlankLinePolicy.RemovesBlankLinesDirectlyBefore`, one caller `BlankLineTokenCleanupRewriter.VisitToken` | one owner (was two) |
| Exclusions for array-initializer, extension, interpolation, property-pattern braces | `rg -n "ArrayInitializerExpression\|ExtensionBlockDeclaration\|PropertyPatternClause\|IsAssignedCollectionInitializer" src/Reihitsu.Formatter/Pipeline/BlankLines`: none left | none survive |
| `RootConstruct.AssignedCollectionInitializer` | `LineBreakInitializerRewriter.IsAssignedCollectionInitializer` and `ReihitsuFormatterHelpers.CaptureRootConstruct` only; FMT-PIPE-024 names only the line-break use | one reader, summaries updated |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 14 new or rewritten `CommentBeforeBraceBlankLineTests` (each brace kind, implicit array and `stackalloc`, nested element, raw interpolation, chained keywords, `/* */` and two comments, directive gaps, the spec example verbatim) | exact output; second pass and each phase stable; LF and CRLF | a kept blank line, a removed blank line above a comment or between two comments, a phase undoing another (FC-012) | `AssertRuleResult` | adequate |
| `RemovesBlankLineBetweenCommentAndClosingBraceOfRegularStringInterpolation` | blank lines only; the comment's column is a recorded follow-up | a kept blank line | `AssertBlankLinesAbove` | adequate for the decided part |
| `KeepsBlankLineBetweenCommentAndBlockStatementAfterStatementButRemovesItBehindCaseLabel` | the FMT-BLANK-017 side stays (FC-003) | a removed blank line above the block statement | `AssertRuleResult` | adequate |
| RH5024–RH5027 `VerifyFormatterRemovesBlankLineBehindComment…` (10) | the rule reports the marked blank line before formatting and nothing after; exact output; LF and CRLF | a report on the formatter's output (FC-013) | `VerifyFormatter` | adequate |

### Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021 — only blank lines are removed; no comment, directive, or disabled text moves; directive gaps and comment shapes tested.
- FC-006, FC-023–FC-026, FC-028, FC-029, FC-036 — no rewrite of syntax, identifiers, or member kinds; whole-document only (no node-level entry point reads the changed set beyond what it read before).
- FC-007–FC-009, FC-018, FC-030 — no code fix changed; the RH5024–RH5027 fixes are unchanged.
- FC-010, FC-011, FC-012 — `AssertRuleResult` checks the second pass and each phase; probes stable.
- FC-013 — round trips for all four rules.
- FC-014 — no new rule.
- FC-016, FC-027 — every excluded kind and keyword, declaration and assignment forms, nested element, implicit array, `stackalloc`, raw and regular interpolation tested; `else if`, verbatim interpolation, nested property pattern probed.
- FC-022 — interpolation braces only; string content untouched.
- FC-031 — every changed summary and the inline comment describe the one token set.
- FC-032–FC-035, FC-037 — as for #1529.
- FC-038–FC-043 — no shape declared; the issue's own input stays refused by FMT-UNSUP-004 (change doc *Decisions*).
- Tab-width matrix — no column decision changed.

### Architecture findings

_None._ The change removes the second statement of the brace token set and the blank-line phase's read of a line-break rewriter's predicate.

### Hints

- The comment in front of a regular interpolated string hole's `}` goes to column 12: already a follow-up candidate in `00-lead.md`.

## Whole run

- `scripts/lane-check.sh behavior-change`: confirmed.
- No `TODO`, no tracker number in changed source.
- Check 10 (visible output decided): every changed or added expected value of RH5111, RH5201, RH5112, the formatter regression tests, and the RH5024–RH5027 round trips is covered by maintainer answers 1B and 2A (`00-lead.md`, both change docs' *Decisions*) or demanded by unchanged requirements (RH5201/RH5112 silence on whole-document output). No rule page changed.
- Check 11 (cost of special cases): the run deletes special cases (#1543) and adds one production line (#1529); see finding #1 for the fact the #1529 change leaves without a caller.
- Tests not re-run: no finding turns on a test result.

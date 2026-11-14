Role: skeptic
Commit: a84ae1f
Verdict: #1529: output question (recommend B, add one case to the question); #1543: output question (recommend A)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Every output below was run at a84ae1f with `scripts/format.sh` (twice where stability matters) and `scripts/apply-fix.sh` (LF and CRLF); fixtures lived in the scratchpad and are deleted.

## thoenissen/Reihitsu#1529

### Premise

Real, reached through a code fix: the RH5111 fix formats the `AssignmentExpressionSyntax` that a conditional access holds behind its `?` (`RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider`, `FirstAncestorOrSelf<AssignmentExpressionSyntax>` → `FormatNodeInDocumentAsync`). Whole-document formatting is reached through the CLI. The RH6023 half is moot: the RH6023 fix changes only the operator gaps (SHARED-OPSPACE-004), and `RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests` (lines ~500–780) already pin that it leaves the member where the author wrote it.

The issue's argument for its option A is wrong on both counts:

- "the fix's column rule is the one the chain alignment uses elsewhere" — no: whole-document formatting puts every wrapped link of these chains in column 9 (the first link token after the receiver), the called link `a?.B⏎ .N()` included (spec author).
- "it keeps the user's wrap" — it keeps the line break, not the column. The fix moves `.C` to the column of the root's first token whatever column the author wrote: input column 9 → 10, 12 → 10, 14 → 10. With `.C` in column 9, the input is the column whole-document formatting gives and RH5201 is silent on it; the RH5111 fix moves it to column 10, where RH5201 reports it. The fix creates a new layout diagnostic on a line that had none (FC-007).

The maintainer's earlier answer (`sdd/changes/2026-10/node-root-detached.md`, *A wrapped member … keeps its line*, repair round 2) chose between `main`'s join and keeping the line break, on the stated premise "as whole-document formatting does". The column was not part of that choice and the premise is false for the column and for the element binding. RH5111-004 ("as the formatter formats it in place") and SHARED-FIX-007 ("formatting the result changes nothing in that code") as written demand reading B; only that maintainer answer covers today's output, so it stays an output question, as the spec author framed it.

Measured (input → RH5111 fix → whole-document formatting; `a` in column 8):

| Cell | Input | RH5111 fix | Whole document | Fix result reported by |
|---|---|---|---|---|
| c01 | `a?.B⏎` `.C` col 10, value wrapped | `.C = 5` col 10 | col 9 | RH5201 |
| c02 | `a?.B⏎` `.C` col 9, value wrapped | col 10 | col 9 | RH5201 (input: none) |
| c03 | `.C` col 14 | col 10 | col 9 | RH5201 |
| c04 | `.C` col 12 | col 10 | col 9 | RH5201 |
| c05 | `a?[0]⏎` `.C` col 10 | wrap kept, col 10 | `a?[0].C = 5;` | RH5112 |
| c06 | `a?.B?.B⏎` `.C` col 12 | col 13 | col 9 | RH5201 |
| c07 | `a?.B⏎` `.C⏎ = 5` (`=` wrapped) | col 10 | col 9 | RH5201 |
| c08 | `a?.B⏎ .B⏎ .C =⏎ 5` (two wrapped links, col 10) | both col 10 | both col 9 | RH5201 |
| c09 | `a?.N()⏎` `.C` col 10 | col 10 | col 9 | RH5201 |
| c10 | `a?.B.C =⏎ 5` (member on the `?` line) | `a?.B.C = 5;` | same | — holds |
| c11 | `a?.B?.B⏎` `.C` col 13 | col 13 | col 9 | RH5201 |
| c12 | `a.B?.B⏎` `.C` col 11 | col 12 | col 9 | RH5201 |

All under LF and CRLF alike, one iteration. Whole-document output is stable on a second pass and silent under RH5111, RH5112, RH5201, and RH6023.

### Mechanism and candidates

Mechanism: when the RH5111 fix formats an assignment that is the `WhenNotNull` branch of a conditional access, the chain's outermost node and its anchor (the link token after the receiver) lie above the node-level root; the root carries the facts that it continues the chain (FMT-PIPE-024 *Chain continuation*, *Conditional-access branch*), so no node inside it is the outermost chain node that `MethodChainAlignmentContributor` aligns, and a wrapped link inside it is placed by depth, which FMT-PIPE-017 shifts to the column of the root's first token; whole-document formatting sees the outermost node and aligns the link to the anchor, and joins it behind an element binding. Every cell above fits: the fix's column is always the root's first-token column (`.` of the first binding, `[` of an element binding).

Membership is decidable: a node-level target that is a non-outermost part of a member-access chain and holds a link that starts a line. Dispatch (callers of `FormatNodeInDocument*Async`/`FormatNode` in `Reihitsu.Analyzer.CodeFixes`), checked against that test:

- RH5111 — the only member: its assignment target is a chain part when it is the branch of a conditional access (only an assignment is absorbed into `WhenNotNull`; a binary or other expression contains the whole conditional access).
- `FluentChainCodeFixHelper` (RH5112, RH5201, …) formats `FluentChain.GetOutermostChainNode` — not a member.
- RH5101, RH5102 (`ArgumentListSyntax`), RH5307 (bracketed argument list / element access), RH5302 (binary), RH5308 (conditional), RH5301/5303/5304/5305/5306 (initializers, collections, patterns), RH5403/5404/5408/5401, RH3004/3203/3208, RH2001, RH5110, RH5113, RH5205, RH5206, RH7101, the expression-body and blank-line helpers — their targets are not chain parts, or contain the whole chain.
- RH6023 — no longer formats (SHARED-OPSPACE-004).

### Breaking inputs

Classes: FC-007 (the fix raises RH5201/RH5112 in the edited span), FC-028 (node-level and whole-document formatting disagree), FC-027 (nesting: nested conditional access, receiver with a member access before `?`), FC-030 (a B implementation that formats more than the assignment — see *Internal choices*), FC-017 (LF and CRLF, two passes).

Class matrix — axes and every member (cells above):

| Axis | Members |
|---|---|
| Link in front of the wrap | member binding `?.B`; element binding `?[0]`; invoked binding `?.N()`; nested conditional access `?.B?.B`; receiver with member access `a.B?.B` |
| Column of the wrapped member in the input | the whole-document column (9); the root's first-token column; left of it; right of it |
| What RH5111 reports | value wrapped; `=` wrapped |
| Wrapped links in the target | one; two |
| Member on the `?` line | yes (holds); no |

Every cell with a wrapped link fails; c10 holds. The test implementer should cover at least c01, c02 (the column the formatter already gives — the cell that shows the fix moving a correct line), c05, c06, c07, c08, c12, and c10 as the holding boundary, under LF and CRLF, and check each fix result against RH5201 and RH5112 (silent) and against whole-document formatting of the same input (equal).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5111 analyzer | must stay (it reports every cell) |
| RH5111 code fix (and Fix All) | must change on B; must stay on A and C |
| Formatter (whole document, CLI) | must stay on B and C; must change on A |
| Node-level entry point `FormatNodeInDocumentAsync` / its facts | possibly involved on B, depending on the implementation (see *Internal choices*) |
| RH6023 analyzer and fix | not involved (gap-only fix; existing tests pin it) |
| RH5201, RH5112 analyzers and fixes | must stay on B and C; must change on A |
| `Reihitsu.Core` (`FluentChain`, `FluentChainBoundary`) | not involved on B unless the implementation touches the boundary facts |
| Rule page RH5111 | not involved |
| Specs | RH5111 (new requirement, on B); `node-root-detached.md` stays as history, the new change doc supersedes its repair-round-2 output |
| Tests | the three RH5111 tests at `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.cs` ~2060–2198 change their expected output on B |

### Refuse option

- **D — withhold the RH5111 fix** where the reported assignment is the branch of a conditional access and a line break stands between its first token and its `=`. A guard of a few lines in `RegisterCodeFixesAsync`, no type. The class is rare: a C# 14 null-conditional assignment whose target is wrapped. The diagnostic stays without a fix, which `general-notes.md` already covers. B's cost is not known until implemented; if it needs a new fact, a new type, or more than about 100 lines, that is escalation 9 with D as its refuse option. D changes a maintainer-chosen output (fix offered → withheld), so it belongs in the output question as an option, not as a lead decision.
- **Declare the shape unsupported** — exact shape: a `ConditionalAccessExpressionSyntax` whose `WhenNotNull` is a simple assignment with a line break between the assignment's first token and its `=` token, comments and directives anywhere, nothing excepted. It would likely delete the *Conditional-access branch* fact (its FMT-PIPE-024 row, `FluentChainBoundary.RootIsConditionalAccessBranch`, the assignment arm of `FluentChain.ContinuesChain`), which today serves only such roots (not verified that a one-line left side needs no such fact). It would refuse, in the CLI, whole files holding a null-conditional assignment with a wrapped target — code that whole-document formatting handles correctly today without a special case — and withhold the RH5111 fix on it. Not recommended: it refuses correctly formatted files to remove a node-level fact. Named under escalation 10.
- Simpler: none found. C (leave both) keeps a fix that moves a correctly placed line into a reported column.

### Output questions

Question 1 of `01-spec-author.md` stands, with one addition: its inputs all put `.C` in column 10, which is the fix's column, so A reads as "keeps what the author wrote". Add the column-9 input, which shows that today's fix moves a line whole-document formatting and RH5201 already accept:

```csharp
        a?.B
         .C =
            5;
```

Before (`main`), RH5111 fix — `.C` moves from column 9 to column 10, where RH5201 reports it:
```csharp
        a?.B
          .C = 5;
```

After, B (recommended):
```csharp
        a?.B
         .C = 5;
```

Also add D (fix withheld, diagnostic stays) as an option, labeled as such. Recommendation stays B; D if B turns out to need escalation 9.

### Internal choices

- **Where B's layout comes from (question 10).** Two shapes of a B implementation predict or over-reach, and the auditor should look for both: (1) an entry-point fact that carries the anchor column of the outer chain — a prediction of `MethodChainAlignmentContributor`/`LayoutComputer`'s decision, which the indentation phase owns; (2) widening the fix's target to the conditional access or the statement — it reads the phase's result but formats `a?` and the spacing around `?` (`a ?.B` → `a?.B`, an RH6016 finding) outside the diagnostic, FC-030 / SHARED-FIX-006. Which shape is acceptable is the lead's decision; the visible output is the same.
- **Change doc #1529:** add the column-9 input as a visible output change entry (its output changes on B), and drop RH6023 from *Affected* (spec author already says its half has no output in question). Recommend.
- **Formatter test for whole-document output** (issue's "Done when"): no formatter output changes on B, but a parity test that pins column 9 and the element-binding join for c01/c05/c06 is cheap and guards SHARED-FIX-007's reference. Recommend keeping it.

### Issue goals

- Option A as recommended by the issue: its two reasons are false (above). Dropping it changes nothing by itself; the choice is the output question.
- "pinned by a code-fix test (RH6023)": drop — RH6023 has no layout in question, and existing tests pin its gap-only output. No visible change.
- "For C, the divergence is recorded in the formatter spec": would belong in RH5111's spec, not the formatter's, because only the fix differs. No visible change.

### Architecture findings

_None._ The cause is that a node-level root does not hold the anchor of a chain decision (FC-028); no two components own the decision, and no component predicts another today. A B implementation can introduce a prediction (see *Internal choices*).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1543

### Premise

Real, reached through the CLI (whole-document formatting). The issue's own input is stale: a comment between a field's `=` and its initializer is refused by the precheck (FMT-UNSUP-004), and "the opening brace of a complex element initializer" is already named by FMT-BLANK-020 (removed). Confirmed on other inputs, every one under LF and CRLF, formatter output stable:

| Token behind `// c⏎⏎` | Formatter keeps the blank line | Reported by |
|---|---|---|
| array initializer `{` (`g = new int[]`) | yes | RH5025 |
| array initializer `}` (assignment, field `int[] G = { … // d⏎⏎ }`, `new[] { }`, `stackalloc int[] { }`) | yes | RH5024 |
| nested array-initializer element `{` (`int[,] H = { { 1 }, // e⏎⏎ { 2 } }`) | yes | RH5025 |
| extension block `{` and `}` | yes | RH5025, RH5024 |
| property pattern `{` (`o is C // c⏎⏎ { }`) | yes | RH5025 |
| assigned collection initializer `}` (`L = { 1 // d⏎⏎ }`) | yes | RH5024 |
| interpolation `}`, regular and raw interpolated string | yes | RH5024 |
| `else`, `catch`, `finally` | yes | RH5026 |
| do-statement `while` | yes | RH5027 |
| block statement `{` after a statement (FMT-BLANK-017) | yes | none — no conflict |
| block `{` behind `case 1:` and behind `if (…)` | no (removed) | none |

The nested array-initializer element brace and the declaration-form array initializer are not in the change doc's entries; they are array-initializer braces and fall under its text, but deserve tests.

The disagreement violates no requirement today: RH5024–RH5027 report as RH5024-001 … RH5027-001 state, and FMT-BLANK-020 says it does not decide these braces. The issue's lens SHARED-DIAG-010 does not apply formally: it covers a layout rule whose fix lays code out as the formatter does, and the fixes of RH5024–RH5027 delete blank lines (SHARED-WS-001). So it is an undecided case, which makes it an output question, as the spec author framed it.

The change doc's *Why* says alternating the fix and the formatter "gives a different file each time" — false. After a fix removes the blank line, formatting keeps the removal (checked on the array-initializer case: only the final newline differs, which is the formatter's ordinary cleanup). The cost is permanent diagnostics on formatted output for a user who does not run the fixes, not oscillation. Correct the sentence.

### Mechanism and candidates

Mechanism: `BraceBlankLinePolicy.RemovesBlankLinesBehindOwnLineComment` decides FMT-BLANK-020 by token kind and parent, and returns false for the braces of `ArrayInitializerExpression`, `ExtensionBlockDeclarationSyntax`, `InterpolationSyntax`, for `PropertyPatternClauseSyntax` `{`, for an assigned collection initializer's `}` (via `LineBreakInitializerRewriter.IsAssignedCollectionInitializer` and the `RootConstruct.AssignedCollectionInitializer` fact), and handles no keyword, so the chained keywords fall to `default` (false); while RH5024–RH5027 report the blank line above any line that starts with such a token.

Dispatch candidates (the `switch` in that method, the `Parent` type of `{`/`}`): `InitializerExpressionSyntax` (array — excluded; object, collection, complex element, `with` — named; assigned collection `}` — excluded), `PropertyPatternClauseSyntax`, `ExtensionBlockDeclarationSyntax`, `InterpolationSyntax`, `BlockSyntax` (FMT-BLANK-017 exclusion), type/namespace/accessor-list/switch braces, anonymous object, switch expression (named); keywords `else`, `catch`, `finally`, `while` of a do statement; the closing tokens of collection expressions, list patterns, and parenthesized patterns (named).

### Breaking inputs

Classes: FC-013 in substance (formatted output reported; not formally, see *Premise*), FC-016 (siblings: every excluded brace kind and every chained keyword), FC-027 (declaration vs. assignment form, nested element brace, implicit array and `stackalloc`), FC-021/FC-019 (comment shapes: `//`, `/* */` on its own line, two comments with a blank line between them — that blank line stays), FC-017 (LF/CRLF, two passes), FC-010.

Matrix — axes: token (each row of the table above); comment shape (`//`; `/* */` on its own line; two own-line comments with a blank line between — the inner blank line stays, only the one behind the last goes; comment that trails the previous token — FMT-BLANK-007/008 already); construct form (assignment, field initializer value, nested element, implicit array, `stackalloc`; regular vs raw interpolation). Every excluded-brace row fails today; the FMT-BLANK-017 row and the `case`/`if` block rows hold and must stay.

Test advice: for the interpolation `}`, use a raw interpolated string. In a regular interpolated string the formatter puts the comment in front of the hole's `}` in column 12 while the hole's other lines and the `}` go to column 8 (also when the author wrote the comment in column 8), so a test there would pin a column that is not decided (follow-up below). The change doc's entry 5 shows that column; switch it to the raw form or say the column is not part of this change.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter blank-line phase (`BraceBlankLinePolicy`) | must change on A (deletes exclusions); must stay on B |
| `RootConstruct.AssignedCollectionInitializer` read in the blank-line phase, and its doc comments in `FormattingContext` and `RootConstruct` | on A the blank-line phase's read goes; the fact stays for the line-break phase |
| RH5024, RH5025, RH5026, RH5027 analyzers and fixes | must stay on A; must change on B (four exemptions) |
| Line-break, indentation phases | not involved |
| Node-level entry points | not involved beyond the fact above |
| Rule pages | not involved under either reading |
| Spec | FMT-BLANK-020 (A) or RH5024–RH5027 (B) |

### Refuse option

The formatter's refusal — leaving the construct as written — is today's behavior, and it is what the four rules report; refusing does not end the disagreement. A deletes special cases rather than adding them: the exclusion arms in `BraceBlankLinePolicy` and the blank-line phase's call into a line-break rewriter's predicate; it adds only the four keywords to the token set, which FMT-BLANK-008 already names. B adds an exemption to each of four analyzers. Declaring a shape unsupported: the narrowest shape ("an own-line comment directly in front of `else`, `catch`, `finally`, or do-`while`", which RH5037 already reports) would delete nothing that A does not delete anyway, so it is not declared. _No simpler option._

### Output questions

Question 2 of `01-spec-author.md` stands; recommendation A. No addition needed beyond the interpolation remark under *Breaking inputs*.

### Internal choices

- Correct the change doc's oscillation claim (*Premise*). Recommend.
- Add the declaration-form array initializer `}` and the nested array-initializer element `{` to the change doc's entry 1 or as entries of their own, so the tests derived from it cover them. Recommend.
- The issue's "round trips of RH5024 and RH5025" extend to RH5026 and RH5027. Recommend.

### Issue goals

- "the blank lines in the input above are removed": the input is refused by the precheck and stays unchanged under every reading; replace with the assignment and initializer-interior forms. No visible change.
- "each brace kind named above has a regression test": the complex element initializer is already decided and tested; keep its existing coverage, add the kinds above. No visible change.
- Chained keywords (issue comment) belong in the same change, with RH5027 (not RH5026) for do-`while`. No visible change beyond the question.

### Architecture findings

_None as a defect of this run._ The blank-line rule for braces is stated twice — FMT-BLANK-007/008/019 for a brace directly behind code, FMT-BLANK-020 with its own exclusion list behind a comment — and the blank-line phase reads a line-break rewriter's predicate (`LineBreakInitializerRewriter.IsAssignedCollectionInitializer`) for one exclusion. Reading A removes both; reading B keeps them and adds four analyzer copies of the exclusion list.

### Follow-up candidates

**Formatter · lines of a multi-line interpolation hole in a regular interpolated string** (outside #1543; refines the spec author's candidate: the column is not merely kept, the formatter moves it). Reached through the CLI.

Input:
```csharp
        var s = $"{
        a
        // f
        }";
```

Real output:
```csharp
        var s = $"{
        a

            // f
        }";
```

The comment in front of the hole's `}` goes to column 12, also when written in column 8, while `a`, a comment in front of `a`, and `}` go to column 8; with the hole written in column 12, `a` moves from 12 to 8. In a raw interpolated string the hole's lines are consistent (content one level deeper than the braces). Readings: A — the comment goes to the column of the hole's other content lines; B — keep. Recommendation A, low confidence: which indentation requirement owns hole lines of a regular interpolated string was not checked.

## Whole run

No repository file other than this handover changed.

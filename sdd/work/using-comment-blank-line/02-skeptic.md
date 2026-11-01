Role: skeptic
Commit: ea15bb5
Verdict: output question

Scope read: thoenissen/Reihitsu#1497 and its comment, `00-lead.md`, `01-spec-author.md`, the change doc `sdd/changes/2026-11/using-comment-blank-line.md`, the spec diff `db61ed3..ea15bb5`, SHARED-FIX-004/006/007, SHARED-DIAG-010, FMT-BLANK-018, RH5020-001..005, and the deciding code: `RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.HasBlankLineBefore`/`IsOrganized`, `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`, `UsingLeadingTriviaBuilder`, `UsingDirectiveOrderingSafety`, `BraceBlankLinePolicy.FindGluedCommentIndex`. The prompt carried no intended fix or hoped-for verdict.

## Premise

Real, and reached by all three callers.

- **Whole-document formatting (CLI).** `scripts/format.sh` twice on `using System;⏎⏎// About Linq⏎using System.Linq;` keeps the blank line, and the run is stable over two passes. The reversal is inside each pass: the using phase drops the layout in front of the first significant trivia (`UsingLeadingTriviaBuilder.CreateLeadingTrivia` with `startsNewGroup: false`), and the blank-line phase inserts it again. `PhaseReversalExceptionList` has two entries under `PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective`.
- **RH7207 on formatter output (FC-013).** `IsOrganized` reports when `AreInSameGroup == HasBlankLineBefore`. `HasBlankLineBefore` counts an end-of-line before the first significant trivia. So RH7207 reports the formatter's output for every comment that the blank-line phase separates. Confirmed with `scripts/apply-fix.sh RH7207`: the fix removes the blank line above `// About Linq`.
- **Code fixes (SHARED-FIX-004).** `scripts/apply-fix.sh RH5020` on `using System;⏎// c⏎using System.Linq;` inserts the blank line, and RH7207's fix removes it again. So on `main` the two fixes undo each other. The change resolves this for comments on their own line (see the class matrix for the member it does not resolve).

The issue's statements about today's code hold on `ea15bb5`, as the spec author recorded.

## Mechanism and candidates

Mechanism: the using rules (RH7207's `HasBlankLineBefore` and the using phase's leading-trivia rebuild) treat any line break in front of the first leading trivia of a same-group directive as a group separator. The comment rule (FMT-BLANK-011 in the formatter, RH5020-001/005 in the analyzer and its fix) treats the same line break as the comment's separator. Each surface removes what the other writes.

What is dispatched: the first significant leading trivia of a `UsingDirectiveSyntax` that continues its predecessor's group, in a block that `UsingDirectiveOrderingSafety.CanSafelyReorder` admits. Directive trivia, and any `#` at a line start in the block, make the block unsafe, so no preprocessor member exists. The axes are the comment's trivia kind (`SingleLineCommentTrivia`, `MultiLineCommentTrivia`, `SingleLineDocumentationCommentTrivia`, `MultiLineDocumentationCommentTrivia`), its position (own line, or on the directive's line as FMT-BLANK-018 classifies it, single-line or spanning lines), and the scope (compilation unit, block namespace, file-scoped namespace).

## Breaking inputs

Failure classes reaching the change: FC-012 (two phases undo each other, the core of the issue), FC-013 (RH7207 on formatter output), FC-010 (phase run alone on its own output, LF and CRLF), FC-015/FC-016 (the comment classification has more than one owner; sibling fixes RH7201–RH7206 share the phase), FC-004/FC-021 (comments moved by the reorder), FC-017 (phase-level stability, not only a second full pass).

Class matrix: "no layout of a comment leading a same-group directive that every surface accepts". All rows were observed on `ea15bb5` with `scripts/format.sh` (twice) and `scripts/apply-fix.sh RH5020` / `RH7207`. The last column is the spec as written.

| # | Comment (first leading) | Formatter output | RH5020 | RH7207 today | After the spec as written |
|---|---|---|---|---|---|
| 1 | `//` on its own line | blank line above | reports without blank; fix inserts it | reports blank, fix removes it: undo cycle | holds |
| 2 | `/* */` on its own line | as 1 | as 1 | as 1 | holds |
| 3 | `///` on its own line | blank line above | not involved (doc comments are RH5020's non-goal) | reports formatter output | holds |
| 4 | `/** */` on its own line | as 3 | not involved | reports formatter output | holds |
| 5 | `/** c */ using …` on the directive's line, also `/** a⏎ b */ using …` | blank line **inserted** above (`using System;⏎/** c */ using System.Linq;` gains one) | no diagnostic | reports formatter output | holds (SHARED-USING-011 "also one on the directive's line") |
| 6 | `/* c */ using …` glued (also `/* a⏎ */ using …`, also `/* a */ /** b */ using …`) | no blank line (the using phase removes the author's, the blank-line phase would keep it: `#nullable enable` in front of the block makes it unsafe, and the formatter then keeps `⏎⏎/* c */ using System.Linq;`) | reports without blank; **fix inserts it** (`apply-fix RH5020` on `using System;⏎/* c */ using System.Linq;`) | reports the RH5020 fix's output; fix removes it (`apply-fix RH7207`) | **fails**: RH7207 still reports the RH5020 fix's output and its fix restores RH5020's input, against SHARED-FIX-004. Output question 2 below decides it |
| 7 | `//` glued | impossible: `//` runs to the end of the line | – | – | – |

Other shapes, all formatter output that RH7207 reports today and must not report after the change:

- A blank line below the comment: `using System;⏎⏎// About Linq⏎⏎using System.Linq;`. The formatter keeps both blank lines.
- Two comment paragraphs: `using System;⏎⏎// A⏎⏎// B⏎using System.Linq;`, and `using System;⏎// A⏎⏎// B⏎using System.Linq;`, where the formatter adds the blank line above `// A`.
- The block-scoped namespace form (verified). The file-scoped form takes the same path through `VisitFileScopedNamespaceDeclaration`.
- A trailing comment on the predecessor: `using System; // trailing⏎// About Linq⏎using System.Linq;`. The formatter adds the blank line above `// About Linq`.

Candidate inputs the test implementer must cover:

- Each matrix row 1–5 as formatter output not reported by RH7207. Row 6 follows the answer to question 2.
- The RH5020-fix-then-RH7207 sequence for rows 1 and 6.
- The shapes listed above.
- Change doc changes 2–5 through RH7207 and at least one sibling fix (RH7201–RH7206 share `UsingDirectiveCodeFixUtilities` and the phase, FC-016), under LF and CRLF.
- The using phase run alone on the formatter output of rows 1–5, under LF and CRLF (FC-010).
- The two `PhaseReversalExceptionList` entries removed.

Two existing tests use this reversal as the sample a stability check must catch, so they lose their subject when the using phase stops removing the line: `PhaseStabilityCheckTests.CheckCatchesUsingPhaseRemovingBlankLineAboveDelimitedDocumentationLedUsingDirective` and `CheckCatchesUsingPhaseRemovingBlankLineAboveCommentLedUsingDirective`, both in `src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs`. The cause `PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective` becomes unused with them.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7207 analyzer (`HasBlankLineBefore`/`IsOrganized`) | Must change |
| Using phase (`UsingDirectiveOrderingRewriter`, `UsingLeadingTriviaBuilder`, header split) | Must change |
| RH7201–RH7207 code fixes and Fix All | Not involved in code (they delegate to the phase); their outputs and tests change |
| RH7201–RH7206 analyzers | Not involved (they check order only) |
| Blank-line phase (FMT-BLANK-011/018) | Must stay; its output for every row is already the target |
| RH5020 analyzer and fix | Must stay (issue: "RH5020 does not change"); row 6 is affected only through RH7207 |
| `Reihitsu.Core` | Involved only if the glued/own-line classification is shared (see *Architecture findings*); otherwise not involved |
| Rule page `documentation/rules/RH7207.md` | Must stay. It says "keep all directives that share a root namespace together" and nothing about blank lines inside a group; I agree with the spec author |
| Formatter test helpers (`PhaseReversalExceptionList`, `PhaseReversalCauses`, `PhaseStabilityCheckTests`) | Must change, as listed above |

## Simpler option

Reading B of output question 2 is also the simpler specification. It lets a blank line stand above any first leading comment of a same-group directive, which is literally the issue's proposed SHARED-USING-008 text ("may be preceded by one blank line when a comment leads it"). The using rules then need no comment classification. The `Uses: FMT-BLANK-011, FMT-BLANK-018` of the shared spec becomes unnecessary, and so does any copy of FMT-BLANK-018's glued test in the analyzer.

## Output questions

**1 · RH7201–RH7207 code fixes · a comment-led directive moved into the middle of its group, no blank line above the comment in the source.** I agree with the spec author's question and recommendation A. Two facts must be added to it:

- Under A, the issue's motivation that "formatting the output of an RH7201–RH7207 fix changes nothing" is not met for this input: formatting the fix output inserts the blank line above `// About Linq`. This is a narrowed goal, and the visible output is what the question shows.
- SHARED-FIX-007 requires the code a fix changes to be laid out as the formatter lays it out. Its moved-code bullet explicitly excludes the using block, so the block counts as changed code. A is justified only through SHARED-FIX-006 and the *Blank lines* bullet of SHARED-FIX-007: the blank line above the comment is not what RH7207 reports, and the comment was already reported by RH5020 in the input. The spec should say so (amendment 3).

Why A: the fix leaves the comment as RH5020 already reported it, and RH5020's own fix then adds the blank line, which RH7207 accepts. So no fix undoes another.

**2 · RH7207, its code fix, and the formatter · a blank line above an ordinary comment glued to a directive of the same group.** I disagree with the spec author's recommendation and recommend B.

Input:
```csharp
using System;

/* c */ using System.Linq;

class C
{
}
```

This is also exactly what the RH5020 code fix produces from `using System;⏎/* c */ using System.Linq;`.

Before (`main`): RH7207 is reported on `System`. Its fix and the formatter produce:
```csharp
using System;
/* c */ using System.Linq;

class C
{
}
```
RH5020 then reports `/* c */`, and its fix restores the input.

After, A (spec as written): unchanged from `main`. RH7207 reports the RH5020 fix's output, and the RH5020 fix and the RH7207 fix keep undoing each other.

After, B (recommended): not reported. The RH7207 fix and the formatter keep the input:
```csharp
using System;

/* c */ using System.Linq;

class C
{
}
```
The same input without the blank line keeps its output in both readings: the formatter adds none, and RH5020 reports it.

Why B: under A the glued comment keeps the defect this issue exists to remove: no layout that both RH5020 and RH7207 accept, and two fixes that undo each other (SHARED-FIX-004). B matches the issue's proposed SHARED-USING-008 wording, and it matches what FMT-BLANK-018 already does above any other token ("the author's are kept"). Its cost is one formatter output change, for a shape FMT-BLANK-018 calls unwanted. If A is chosen, the cycle stays and becomes a follow-up (see *Follow-up candidates*).

The change doc's visible output changes 1–5 still have to be asked, as the spec author notes. Under B, the change doc's "Unchanged" bullet on the glued comment becomes a sixth visible output change.

## Internal choices

- **Amendments for the spec author** (wording only; nothing visible changes beyond what the questions decide):
  1. SHARED-USING-011 says "The blank lines MUST stand above that comment, and the comment stays directly above the directive". Read as a condition, this makes RH7207 report formatter output: the formatter keeps `// About Linq⏎⏎using System.Linq;` and the blank line between two comment paragraphs. State instead that only the line breaks in front of the first leading comment count (as SHARED-USING-008 already says), and that blank lines between the comments and the directive are not decided by the using rules. Add both shapes as not-reported examples (RH7207-002 or SHARED-USING-011).
  2. The `Why:` of SHARED-USING-011 and of RH7207-002, and the change doc's *Why*, attribute the separating blank line to RH5020 for every comment. RH5020 covers ordinary comments only (its non-goal sends documentation comments to RH8303, which covers element documentation, not using directives). For documentation comments, only the formatter (FMT-BLANK-011) separates them. Name RH5020 for ordinary comments only.
  3. Under question 1 A, record in FMT-USING-010 or RH7207-004 `Why:`, and in the change doc's *Decisions*, that the fix leaves a comment without a blank line by SHARED-FIX-006 and the *Blank lines* bullet of SHARED-FIX-007, so that formatting the fix output may add it. Otherwise RH7207-004 silently contradicts SHARED-FIX-007's "formatting the result changes nothing".
  4. Under question 2 B: SHARED-USING-011 and FMT-USING-010 drop the comment classification; RH7207-001 drops the glued example; RH7207-002 gains it; the change doc moves the glued case from *Unchanged* to *Visible output changes*. Under question 2 A: the change doc must narrow "gives the comment a layout that every rule accepts" and record the remaining RH5020/RH7207 fix cycle as a known gap.
  5. Optional: the `Why:` of RH7201-004 to RH7207-004 says "A block has one organized form". With an allowed-not-required blank line, the block has one form per source. Reword, or leave it as the lead decides.
- I agree with the spec author's other internal choices: separate requirements SHARED-USING-011 and FMT-USING-010, RH7201–RH7206 `-004` reworded alike, and the rule page unchanged.

## Issue goals

The issue is `agent-filed`, but the maintainer revised it. `00-lead.md` treats its target and *Done when* as binding.

- "May be preceded by one blank line when a comment leads it": the spec narrows this to comments that are not glued. That is output question 2.
- "One blank line": the spec accepts several blank lines (RH7207 does not report them) and writes one. This is consistent with RH7207-002 at group boundaries, it is visible, and it is covered by visible output change 1 and 4 of the change doc. Keep it.
- "Formatting the output of an RH7201–RH7207 fix changes nothing" (motivation): this is narrowed under question 1 A. That is visible and already in question 1.
- "The formatter's output does not change": this holds under question 2 A and breaks for one shape under B. It is in question 2.
- *Done when*: the stability-check tests that use this reversal as their sample (see *Breaking inputs*) have to be rewritten. That is an internal consequence of "the matching phase-level idempotency exceptions are removed".

## Architecture findings

**Conditional on question 2 A: the glued/own-line classification of FMT-BLANK-018 would get further owners.**

- **Components:** `BraceBlankLinePolicy.FindGluedCommentIndex` in `Reihitsu.Formatter`; RH5020's own "starts its line" test (RH5020-001, SHARED-BLANK-002 via `Reihitsu.Core`); and, new under A, the RH7207 analyzer in `Reihitsu.Analyzer` and the using phase.
- **Shared decision:** under A, all of them decide whether the first leading comment of a same-group directive is glued (ordinary comment on the directive's line) or separated (own line, or a documentation comment, also on the directive's line).
- **Where it shows:** row 5 against row 6 of the matrix. `/** c */ using …` is not glued, while `/* c */ using …` and `/* a */ /** b */ using …` are glued.
- **Why it is structural:** the analyzer cannot reference the formatter, so a narrow implementation of A adds a private copy of that classification. README *Architecture findings* calls for the structural change in this run in that case: one helper in `Reihitsu.Core` used by `FindGluedCommentIndex`, the using phase, and RH7207.

Under B, no classification is needed and the finding does not arise.

## Follow-up candidates

- **Only if question 2 is answered A:** the RH5020 code fix inserts a blank line above an ordinary comment glued to a using directive of the same group, and the RH7207 code fix removes it again (SHARED-FIX-004). Reproduce with `scripts/apply-fix.sh RH5020` on `using System;⏎/* c */ using System.Linq;⏎⏎class C⏎{⏎    int x;⏎}`, then `scripts/apply-fix.sh RH7207` on its output. The open question is the one in output question 2.

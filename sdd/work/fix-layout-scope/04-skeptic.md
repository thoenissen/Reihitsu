Role: skeptic
Commit: 6f690ae
Verdict: #1140: output question (two new, plus an escalation-9 risk); #1687: proceed (one spec-wording conflict to fix); #1748: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The branch changes no source (`git diff origin/main...HEAD -- src` is empty), so every measurement below is `main`'s behavior. Fix outputs come from `scripts/apply-fix.sh <ID> <dir>` on fixtures in the session scratchpad, under LF and CRLF; both endings gave identical diffs in every run, and every run exited 0 with no `not-converged`, `invalid-result`, or `line-ending-drift`. Maintainer answers M1–M5 and decisions D1–D13 are taken as binding. The premises are settled in `02-skeptic.md` and are not repeated here. This pass challenges the change doc `sdd/changes/2026-11/fix-layout-scope.md` and the spec diff of d1884a0 and 8349028.

## thoenissen/Reihitsu#1140

### Premise

The change doc's premises hold where I checked them:

- **The preceding-token and list-position facts are dead once the leading trivia is kept** (FMT-PIPE-024, D13). Every reader found by `rg "PrecedingToken|RootListPosition" src/Reihitsu.Formatter` reads the facts for the root's first token only. The readers are `BlankLineEditor`, `BraceBlankLinePolicy.IsOpeningBraceOfBlockStatementAfterStatement` (only for `block.Parent == null`), `BlankLineBreakSpacingRewriter.Visit` and `BlankLineStatementSpacingRewriter` (root only), `BlankLineTokenCleanupRewriter`, `BlankLineCollapser`, `BlankLineRegionDirectiveRewriter`, `BlankLineTriviaBoundaryRewriter`, `DocumentationCommentFormattingPhase.IsWrittenBehindRootPrecedingToken`, and `CommentIndentationContributor`. All of these decide the root's leading trivia or the comments in it, and that trivia is no longer written back. The claim holds as far as reading shows. A test has to prove it.
- **Code fixes are the only callers.** No caller of `FormatNodeInDocumentAsync` sits outside `Reihitsu.Analyzer.CodeFixes` apart from `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync:311–314`, which delegates to it when the target is the context. That path contradicted FMT-PIPE-018 on `main` ("the context entry point keeps its leading trivia in every case"). The change removes the contradiction.
- **No fix relies on the entry point to indent a target it built.** I checked every call site whose target is a node the fix creates:
  - RH2001 (`TryCreateFieldDeclaration`) carries the property's attribute lists, modifiers, and type trivia over.
  - RH3004 uses `WithTriviaFrom`.
  - `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` takes only the body from the formatted member.
  - `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` copies the line's own indentation (`GetLineIndentation`) into the new line, so it does not predict a column.

  So keeping the leading trivia does not drop a target into column 0.
- **Change 9 (tabs) is decided by an unchanged requirement.** SHARED-FIX-007's first bullet already counts a tab in front of the changed code as one column and forbids replacing it. Change 10 of `context-target-first-line.md` already shipped the same behavior for the context entry point. Nothing visible is in question. The output does look odd for a whole block: `→→if (a > 0)` followed by `  {` in column 2. The lead may still show it. I do not ask it.
- **Output question 1 of `03-spec-author.md` (an argument that spans lines) rests on a wrong "Before".** Measured on `main` with RH5101 `M(⏎Foo(1,⏎        2),⏎3);`, the wrap is kept, not joined: `M(Foo(1,` / `      2),` / `  3);`, with `2` under `1` in column 14. The spec author's option A is `main`'s output, so that question puts nothing in question as posed. The real open question is wider (question 1 below).

### Mechanism and candidates

_N/A_: a behavior change. The dispatch for *Inside the reported construct* is the set of fixes whose spec now references it. The table lists each with the node it hands to `FormatNodeInDocumentAsync` today (read from source):

| Fix | Node formatted today | Parts |
|---|---|---|
| RH5101, RH5102 | the argument list | arguments |
| RH5303, RH5304 | the innermost statement, `EqualsValueClause`, or arrow clause around the object creation (`TryGetFormatTarget`) | elements |
| RH5305 | same, around the collection expression | elements |
| RH5306 | same, around the list pattern | inner patterns |
| RH5307 | same, or an `ArgumentSyntax` | arguments |
| RH5308 | the innermost statement, else the member (`GetFormatTarget`) | condition, branches |
| RH5403 | the owning statement | statements of the block |
| RH5404 | the declaration | members |
| RH5417 | the member (`FormatMemberOnLinesOfItsOwnAsync`) | statements of the body |

### Breaking inputs

Failure classes that reach the change are FC-030, FC-015, FC-016, FC-008, FC-027, FC-007, FC-020/FC-021, FC-022, FC-043, and FC-028. FC-028 now applies by design: `FormatNode` still decides its node's leading trivia, and the node-in-document entry points no longer do. A test should pin that split rather than let it read as drift.

**A. A part that spans lines (FC-027, FC-022).** RH5403, RH5404, and RH5417 report only bodies written on one line, so all of their parts fit on one line. The list rules have no such limit. RH5307 is the exception: RH5307-002 does not report a list with an argument that spans lines. For RH5101, RH5102, RH5303–RH5306, and RH5308, a part can span lines: a lambda with a block body, a wrapped invocation, a nested initializer, or a raw or verbatim string. Measured with RH5101:

| Input argument | `main` |
|---|---|
| `Foo(1,⏎                    2)` moved from column 12 to 10 | wrap kept; `2` re-aligned under `1` (column 14) |
| `x =>⏎{⏎    int  y = x;⏎    return y;⏎}` moved from column 12 to 10 | body re-laid out from column 10: `int y`, and a blank line inserted between the two statements |
| RH5308 branch `N(1,⏎        2)` | `2` aligned under `1` at the branch's new place, and the statement respaced |

The new bullet keeps a part's "tokens and the spacing between them, … and its shape". It does not say where the later lines of a part go when the part's first token moves. Three readings give three outputs: the lines shift with the part, the formatter lays out their columns, or they keep their columns. That is question 1. The SHARED-FIX-007 RH5101 raw-string example has to stay true under whichever reading is chosen. A shift and the formatter's columns agree on it, but a kept column does not.

**B. Nested constructs of the same rule (FC-027, FC-008).** On `main`, the outer fix formats the inner construct as well, so the inner diagnostic disappears in the same pass. Measured: RH5403 on `if (a > 0) { if (a > 1) { a++; } }` and RH5404 on `class A { class B { int _x; } }` each needed one iteration. Under the *Inside* bullet (M3/M5), the inner block, type, or argument list keeps its own code and stays reported. The class has these members:

- RH5403: a block in the reported block.
- RH5404: a type in the reported body. The change doc lists only this member, under change 7.
- RH5101/RH5102: an argument list inside an argument.
- RH5305/RH5306: a collection expression or list pattern as an element.
- Cross-rule: RH5417 leaves a one-line block inside the body, which RH5403 then reports.

SHARED-FIX-002 (unchanged) requires one Fix All application to correct the inner diagnostic as well. RH5403 and RH5404 return `WellKnownFixAllProviders.BatchFixer`. The outer and inner fixes then overlap, and I did not run whether that batch still meets SHARED-FIX-002. The repository's `DocumentOrderFixAllProvider` exists for fixes "that rewrite or move code holding other diagnostics". The test implementer must pin a Fix All test per member against SHARED-FIX-002. The test-base choice may change with the provider; both are batch bases.

**C. The code around the construct (RH5303–RH5308).** These fixes format the statement, `EqualsValueClause`, arrow, or argument around the construct. Only the parts are kept, so the result is mixed. Measured, RH5308 `var  r=a ? b+1⏎    : 2;` gives on `main`: `var r = a` / `? b + 1` / `: 2;`, with the operators in column 20. RH5303 `var  x=new List<int> { 1+2,⏎3 };` gives `var  x= new List<int>`: the gap in front of `new`, which lies in the `EqualsValueClause`, is respaced. Under the new RH5308-004 sentence, the result is `var r = a` with `? b+1`: the statement is formatted and the branch is not. That is question 2.

**D. The leading trivia (D5).** The M-A list of `02-skeptic.md` still applies, per fix touched, under LF and CRLF. Add these inputs:

- RH2001, which is not in the change doc. Measured: `private int _a;⏎/// <summary>The value.</summary>⏎private int Value { get; set; }` gets a blank line inserted and the one-line documentation comment spread over three lines on `main`. Under D5 both stay.
- A blank line holding whitespace, and a foreign line ending, inside the kept leading trivia. Both are now written back as they stand (FMT-PIPE-018, "restores the tabs and line endings"). On `main`, the phases cleaned them for a target that starts its line. This is rare, and SHARED-FIX-008 scopes the line ending to the span, so the output is allowed. The tests must still pin it.
- Two blank lines above the target, which `main` collapses. They now stay.
- A member that shares its line with a sibling in front of it, through `FormatMemberOnLinesOfItsOwnAsync` (#1687). The line break the helper writes is in the document before the entry point runs, so the entry point keeps it. See *Internal choices* 1 for the spec wording.

**E. Chain and switch expression.**

- A chain whose first token starts its line, as in an expression statement `a⏎.B()⏎.C();`. Its leading trivia is newly kept.
- `var r=a⏎.B()⏎.C();`, as in the spec example.
- A tab in front of the chain.
- Each fixed chain must not be reported again by RH5112, RH5201, or RH5048 (SHARED-FIX-001, FC-007).
- RH5206 with the switch expression as an argument. Change 4 is that input. The root is then a list element (FMT-PIPE-024 *List element*), and the result must equal change 4's "after", which was derived with `trace.sh` and has not been run through the fix.

**F. Inside a part, what the rule names** (FC-002):

- RH5303's wrapped complex element: its expressions go on lines of their own, as the rule names. Does `1+2` inside one of them keep its spacing?
- RH5308's nested conditional as a branch.
- A comment between two parts of a one-line block: `if (a) { a++; /* c */ a--; }`.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ReihitsuFormatter.FormatNodeInDocumentAsync`; the readers of the facts listed under Premise | must change (D5, D13) |
| `FluentChainCodeFixHelper` (the fallback comparison) | must change; the comparison becomes dead code (M2) |
| RH5206 fix target | must change (M4) |
| RH5101, RH5102, RH5303–RH5308, RH5403, RH5404, RH5417 fixes | must change (M3/M5), through one shared step, not nine copies (Architecture findings) |
| RH5403, RH5404 (and every list fix above with `BatchFixer`) Fix All provider | must change, or be shown by a test to meet SHARED-FIX-002 on nested inputs (Breaking inputs B) |
| RH5111 node choice, RH5113 semicolon | must change (D10) |
| RH2001, RH3004, RH5205, RH5301, RH5302, RH5412–RH5416 fixes | must stay as code; their output changes through the entry point (D5) |
| RH3202, RH3203, RH3205–RH3208 | not involved by D5 (`ConvertBodyAsync` keeps the member's trivia already); involved by #1687 |
| RH5110, RH5403/RH5404 header and branches | must stay as on `main` (D6) |
| `sdd/specs/formatter/pipeline.md` *Code owners* | must change when `PrecedingTokenFacts` and `RootListPosition` are deleted. The row still names both |
| Analyzers, rule pages, `Reihitsu.Core` | not involved |

### Refuse option

- **D5 (leading trivia).** Refusing would mean withholding every fix applied below a statement or member. That is common code, so it is not an option. The change deletes code (two fact types and their readers), so the requested change costs less than refusing would.
- **M2 and M4 (chain, switch expression).** Cheap. The chain's comparison fallback is deleted, and the switch expression goes through the path FMT-PIPE-017 already provides. Nothing to refuse.
- ***Inside the reported construct* (M3, M5).** This is the expensive part, and the change doc states no cost for it. Withholding is not possible: one-line blocks, one-line types, and wrapped argument lists are common code. The cheapest refusal is to keep `main`'s output for the parts that span lines (question 1, option C). That needs only a check of whether a part spans lines. Keeping `main` for all parts would contradict M3 and M5.
- **Declaring a shape unsupported.** Not an option. No input shape is involved: what is in question is how much of a node a fix writes back.
- **Simpler option.** For RH5303–RH5308, target the construct itself, as M4 does for RH5206 (question 2, option C). That removes the mixed result without a new mechanism.

### Output questions

**1 · RH5101 · The first argument should be on the same line as the opening parenthesis — an argument that spans lines** (the same holds for RH5102, RH5303–RH5306, and RH5308)

Input:
```csharp
        Q(
            x =>
            {
                int  y = x;
                return y;
            },
            3);
        M(
            Foo(1,
                    2),
            3);
```

Before (`main`, measured):
```csharp
        Q(x =>
          {
              int y = x;

              return y;
          },
          3);
        M(Foo(1,
              2),
          3);
```

After, A (recommended): the argument keeps its code, and its later lines move by as many columns as its first line.
```csharp
        Q(x =>
          {
              int  y = x;
              return y;
          },
          3);
        M(Foo(1,
                  2),
          3);
```

After, B: the argument keeps its code, and its later lines are put where the formatter puts them.
```csharp
        Q(x =>
          {
              int  y = x;
              return y;
          },
          3);
        M(Foo(1,
              2),
          3);
```

After, C: an argument that spans lines is laid out as on `main`; only arguments on one line keep their code.
```csharp
        Q(x =>
          {
              int y = x;

              return y;
          },
          3);
        M(Foo(1,
              2),
          3);
```

Why A: the argument stays exactly as the author wrote it and moves as one piece, as code a fix moves already does. If the change grows too large, C is the cheap fallback.

**2 · RH5308 · Conditional expressions should be formatted correctly — the code around the conditional** (the same holds for RH5303–RH5307, whose fixes also format the statement or initializer around the construct)

Input:
```csharp
        var  r=a ? b+1
            : 2;
```

Before (`main`, measured):
```csharp
        var r = a
                    ? b + 1
                    : 2;
```

After, A (what the spec diff says now: the statement is formatted, and the branches keep their code):
```csharp
        var r = a
                    ? b+1
                    : 2;
```

After, B: as `main`.

After, C (recommended): only the conditional is laid out, from the column its first token keeps; the rest of the statement stays (derived, not measured).
```csharp
        var  r=a
                   ? b+1
                   : 2;
```

Why C: it is the same answer as for the chain and the switch expression, so the user never gets half of a statement formatted.

### Internal choices

1. **SHARED-FIX-006 *Leading trivia* and SHARED-FIX-007 *Shared line* contradict each other on their wording.** *Leading trivia* says the whitespace in front of the formatted node's first token "MUST stay". *Shared line* (and D8, #1687) requires a line break in front of a member that shares its line with a sibling in front of it. `FormatMemberOnLinesOfItsOwnAsync` writes that line break into the member's leading trivia (`rewritten.WithLeadingTrivia(indentation)`). Recommendation: add to the *Leading trivia* bullet "apart from the line break SHARED-FIX-007 (*Shared line*) writes in front of a member that shares its line". Alternative: let *Shared line* override implicitly. Rejected: two requirements would then disagree on their face. Nothing visible changes either way.
2. **D6's rejection reasoning in `Alternatives considered`.** The doc rejects narrowing RH5110, RH5403, and RH5404 because "a shared write-back of part of a formatted node is a mechanism of its own". M3 and M5 need such a write-back: the formatter lays out the delimiters, and each part's own code is written back into that layout (or the fix computes the columns itself, which predicts the indentation phase; role question 10). Recommendation: rewrite that bullet. It should name the mechanism *Inside* needs and say that the header narrowing is left out for scope, not because it is impossible. The header narrowing becomes a follow-up candidate (below). Internal text.
3. **Change-doc completeness** (no new decision; each item is demanded by an unchanged requirement or by M1/M5):
   - Change 2 should name RH2001 (measured) and the expression targets that can start their line: RH3004, RH5205, RH5301, RH5302.
   - Change 1 or 2 should state that a blank line between `{` and the target stays, and that two blank lines above the target stay.
   - Change 7 should name the nested block of RH5403, the one-line block inside an RH5417 body, and a nested argument list (Breaking inputs B), together with the Fix All consequence.
   - Change 6 should not say "the same holds for RH5308" without the mixed result of question 2.

### Issue goals

Unchanged from `02-skeptic.md`. One addition: once the answers are in, the goal "each input above changes only the code its diagnostic is about" also covers RH5303–RH5308 if question 2 is answered C. That would close part of FU2 in this run. The lead decides whether that scope growth is acceptable. The visible part is question 2.

### Architecture findings

- ***Inside the reported construct* needs one owner.** Nine fixes (the table under *Mechanism and candidates*) must each lay out a construct's delimiters with the formatter and keep each part's code. Two shapes are possible:
  - Write back the parts into the formatted layout. This reads the phases' result.
  - Compute each part's line and column in the fix. This predicts the indentation phase (role question 10), and the run must not take that shape.

  Either way the decision "which text of the part survives" must have one owner, a shared step in the code-fix layer. Otherwise it is FC-015 nine times over. Its size depends on question 1: A adds a shift of a part's later lines, and B adds a per-line mapping that is undefined where the formatter changes a part's line structure (blank line inserted, block conversion). Escalation-9 risk: a shared helper is likely a new type, and with A or B it may exceed about 100 production lines. The change doc should state the cost, or the lead should get it from the implementer before tests lock outputs.
- The two-owner finding for the leading trivia (D5) is resolved by the spec diff as recorded.

### Follow-up candidates

- **D6 instances with the same write-back.** Once the *Inside* write-back exists, it could also keep the header of RH5403 and RH5404 and the code in front of RH5110's first clause (`sealed  class B` stays). That would change output from `main`, so it is an output question. It belongs on #1140 (FU3), not in this run.
- FU1–FU3 of `00-lead.md` are unchanged.

## thoenissen/Reihitsu#1687

### Premise

Unchanged (`02-skeptic.md`). The change doc does not touch it. D5 does not break the helper path. `FormatMemberOnLinesOfItsOwnAsync` writes the line break and copies the line's own indentation before it calls the entry point, so keeping the leading trivia keeps that indentation.

### Mechanism and candidates

_N/A_ for this pass.

### Breaking inputs

A sibling in front of the member (`int _a; public C() => …`) through the helper, after D5: the member must start in the column of the line it was on, and its block must be laid out from that column. On `main`, RH3203 laid the block out from column 12.

### Surfaces

As in `02-skeptic.md`.

### Refuse option

As in `02-skeptic.md`.

### Output questions

_None._

### Internal choices

The wording conflict between *Leading trivia* and *Shared line* (#1140, Internal choices 1).

### Issue goals

Unchanged.

### Architecture findings

_None_ beyond D8.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1748

### Premise

M1 decides it. The change doc's change 1 and the spec diff (SHARED-FIX-006 *Leading trivia*, FMT-PIPE-018, and the comment examples of RH5403-003 and RH5404-003) state it consistently. The RH5404 example now keeps the header unchanged, which matches D6.

### Mechanism and candidates

_N/A_.

### Breaking inputs

As in `02-skeptic.md`, plus nested blocks (#1140, Breaking inputs B), which change what RH5403 leaves behind.

### Surfaces

RH5403 and RH5404 fixes and the entry point (D5), as under #1140.

### Refuse option

None cheaper (common code).

### Output questions

_None._

### Internal choices

_None._

### Issue goals

Unchanged.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Fixtures used: RH5101 (wrapped argument, lambda argument), RH5102 (lambda on the `(` line: no diagnostic), RH5308 (twice), RH5303, RH5403 (nested), RH5404 (nested), RH2001 (documentation comment). All are in `/tmp/claude-0/-home-user-Reihitsu/ba68e92d-4c14-5cfe-a5c5-81fb8298ae73/scratchpad/sk04`, outside the repository.
- No repository file was changed besides this handover.

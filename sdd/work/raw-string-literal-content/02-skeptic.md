Role: skeptic
Commit: 2178115
Verdict: #951: question for the maintainer; #1133: question for the maintainer; #1146: question for the maintainer

Inputs: thoenissen/Reihitsu#951, thoenissen/Reihitsu#1133, thoenissen/Reihitsu#1146 and their comments, the decision comment on thoenissen/Reihitsu#922 that #951 cites, `00-lead.md`, `01-spec-author.md`, and the spec diff against `origin/main`. The lead's prompt carried no intended fix. The #951 body names a cause ("the indentation phase gives the tokens … block columns", "`ReplaceNodes` … drops the inner one") and an intended mechanism ("the indentation phase skips every token …"), and the #1146 body proposes rewordings for Q3, Q5, and Q6. I treated all of them as claims and checked them against the code and the runs below; Q3, Q5, and Q6 are out of scope (thoenissen/Reihitsu#1433).

All fixtures were written to a scratch directory outside the repository and deleted afterwards. In the examples, `→` marks a tab.

## thoenissen/Reihitsu#951

### Premise

The bug is real for both cases in scope. `scripts/trace.sh <fixture> --passes 3`:

- **Case 1** (the issue's input). Pass 1: `IndentationPhase` moves `}` from column 4 to 8, and `RawStringAlignmentPhase` then shifts every line after the opening line by +13, so `name` and `}` end at 21. Pass 2: `IndentationPhase` puts `name` and `}` back at 8. The literal is now aligned, so the raw-string phase adds nothing. Pass 3 is stable. The output of pass 1 is not a fixed point (FMT-PIPE-004). The output matches the issue's "Actual" section. CRLF gives the same result.
- **Case 2** (the issue's input). Pass 1 aligns only the outer literal, leaving `inner` at 21 and the inner closing quotes at 21. Pass 2 moves the inner literal to 24. Pass 3 is stable.
- **FMT-RAW-007, second example** (aligned literal, `name` at 25). Fails today on pass 1: `IndentationPhase` moves `name` and `}` to 8.
- **FMT-RAW-008, second example** (outer literal aligned, inner literal misaligned). Already passes today in one pass, so its test is a characterization test, not a reproduction.

How the spec diff fits with the code and with the other specs (findings for the spec author; none needs a maintainer decision):

- **FMT-RAW-007 contradicts FMT-RAW-008.**
  - FMT-RAW-007's main clause covers "every line that starts inside one of its interpolation holes", and its second sentence says such lines "MUST stay as written" when the outer literal is aligned.
  - FMT-RAW-008's second example moves exactly such lines: `inner` and the inner closing quotes go from 21 to 24 inside an aligned outer literal.
  - FMT-RAW-006 received the exception for a nested literal; FMT-RAW-007 needs the same one.
- **FMT-RAW-007's parenthetical misses some line starters.** "Whether a token of the hole's expression or the hole's closing `}` starts it" omits three things that start lines inside a hole and flip the same way today:
  - the `,` of an alignment clause;
  - the `:` of a format clause (`Hello {x⏎,10⏎:N2}`);
  - a comment that starts a line (`{⏎// c⏎name⏎}`).
- **FMT-RAW-007 conflicts with FMT-CLEAN-002.** FMT-RAW-007 says a hole line "MUST NOT move … in any other way". A tab in the indentation of a hole line is code whitespace, and FMT-CLEAN-002 replaces it with four spaces. Observed: `Hello {1→// c` becomes `Hello {1    // c` in `CleanupPhase`. So a tab-indented hole line changes its column in the cleanup phase, and one of the two requirements has to name the other.
- **FMT-RAW-007, read inclusively, conflicts with FMT-PIPE-002, FMT-INDENT-007, and FMT-INDENT-011.** It would cover lines that start inside the text of a verbatim string or of a `/* */` comment nested in a hole. That is Question 2.
- **Under the spec as written, FMT-RAW-007 and FMT-INDENT-011 change formatter output that is stable today.** Under the analyzers, they also conflict with SHARED-DIAG-010. That is Question 1.

### Mechanism and candidates

Mechanism, case 1: a line that starts inside a hole of a multi-line interpolated raw string is placed twice in one pass. First the indentation phase gives it a column. Then the raw-string phase shifts it by the literal's delta. When the indentation column does not itself depend on the literal's position — a block column from the scope depth — the next pass, in which the literal is aligned and the shift is zero, leaves a different column.

Lines whose indentation column is measured from a token on a content line of the literal do move with the literal, and they are stable today. Observed with a lambda block body (FMT-INDENT-028), switch-expression braces (FMT-INDENT-027), and a wrapped argument (FMT-INDENT-012) in a hole, misaligned literal and aligned alike.

Mechanism, case 2: `RawStringAlignmentPhase` computes the replacement of every misaligned multi-line raw literal from the unmodified tree. For literals nested in a hole it keeps only the outermost replacement (`ReplaceNodes(…, (originalNode, _) => replacements[originalNode])` ignores the rewritten node). So one pass aligns only the outermost misaligned literal of a nest. Depth 3 needs two passes (observed).

Dispatch:

- `RawStringAlignmentPhase.CollectReplacements` matches two node types:
  - `LiteralExpressionSyntax` with `MultiLineRawStringLiteralToken` or `Utf8MultiLineRawStringLiteralToken`;
  - `InterpolatedStringExpressionSyntax` whose `StringStartToken` is `InterpolatedMultiLineRawStringStartToken`, with `$` or `$$`, which enters through the quote offset.
- `AdjustContentLines` shifts every line after the first line of the node's text and counts only leading spaces.
- `LayoutComputer.Compute` runs three passes: block indentation for every token that `SyntaxTokenPositionUtilities.IsFirstOnLine` accepts, the alignment contributors, and `CommentIndentationContributor`.

Candidates: every line after the opening line of a multi-line interpolated raw string, by what starts it.

1. **Literal text.** A content line (`InterpolatedStringTextToken` text). The indentation phase does not touch it (observed). Owned by FMT-RAW-001 and FMT-RAW-002.
2. **The closing line.** The indentation is in the text of `InterpolatedRawStringEndToken`. Owned by FMT-RAW-001.
3. **A token inside a hole**, first on its line through end-of-line trivia: any expression token, the hole's `CloseBraceToken` (`}` or `}}`), the `CommaToken` of `InterpolationAlignmentClauseSyntax`, the `ColonToken` of `InterpolationFormatClauseSyntax`, or the opening quotes of a nested literal. Observed flipping:
   - `name`/`}` (case 1);
   - `name}`;
   - `,10`/`:N2}`;
   - `}}}` under `$$"""`;
   - an inner `"""` that opens a hole line (the inner literal then ends at column 8, left of the outer content);
   - the same shapes as a method argument.
4. **A comment that starts a line inside a hole.** Owned by `CommentIndentationContributor`. Observed flipping.
5. **A line that starts inside the text of a token or a comment that began on an earlier line within the hole:**
   - the content and closing lines of a nested raw literal: plain, `u8`, or interpolated, at any depth (FMT-RAW-008);
   - the continuation line of a nested verbatim string `@"…"` or `$@"…"`. Today it is shifted, and the nested string's value changes: `b"}` at column 0 moves to 13;
   - the continuation line of a nested `/* */` comment. Today it is shifted, which FMT-INDENT-007 forbids.
6. **A line that an earlier phase of the same pass creates inside a hole.** Observed:
   - `LineBreakPhase` breaks a one-line switch expression (`x switch { 1 => "a", _ => "b" }`), a block lambda (`() => { return 1; }`), and an anonymous method (`delegate { return 1; }`) inside a hole onto new lines starting at column 0;
   - `BlankLinePhase` splits `N(1); return 1;` inside a lambda body in a hole.

   Today `IndentationPhase` then aligns them.

Not candidates:

- A directive inside a hole does not parse; the formatter skips the file.
- Single-line raw strings and non-raw interpolated strings are outside FMT-RAW.

### Breaking inputs

Failure classes:

- **FC-012:** the mechanism itself, two phases each moving the same lines.
- **FC-017:** every FMT-RAW-007 and FMT-RAW-008 case needs a second-pass assertion under LF and CRLF.
- **FC-010:** the CRLF variants of case 1 and case 2 reproduce identically.
- **FC-003:** for each hole shape, test both a misaligned literal (moving right and left) and an aligned one.
- **FC-001 and FC-016:** every line starter in candidates 3 and 4 goes through a different indentation pass (block, contributor, comment). A guard on one pass leaves the others re-indenting.
- **FC-027:** nesting depth 2 and 3, and the literal as an argument (`M2($"""…""", x)`), as an initializer, or in a declaration.
- **FC-022 and FC-006:** a nested verbatim string and a nested `/* */` comment in a hole (Question 2).
- **FC-034:** use the issue's two inputs verbatim.
- **FC-013 and FC-032:** see the surfaces below.

Inputs:

- **Case 1 and case 2** verbatim, LF and CRLF.
- **FMT-RAW-007, second example.** Fails today.
- **`$$"""` with a hole that spans lines.** `{"a": {{⏎name⏎}}}` flips today.
- **The hole's `}` on the same line as the expression.** `Hello {⏎name}` flips today.
- **Alignment and format clauses on their own lines.** `{x⏎,10⏎:N2}` flips today.
- **A `//` comment on its own line inside a hole.** Flips today.
- **The inner literal's opening quotes starting a hole line.** `Outer {⏎"""⏎inner⏎"""}` flips today; this combines FMT-RAW-007 and FMT-RAW-008.
- **Depth 3.** `A {$"""⏎B {"""⏎inner⏎"""}⏎"""}` needs two passes today.
- **A leftward shift** whose hole lines stay right of column 0. Today the statement indented by 24 flips through a clamp to column 0.
- **The hole shapes in candidate 6.** Their expected output depends on Question 1.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RawStringAlignmentPhase` | Must change (case 2; the hole lines; Question 2) |
| `IndentationPhase` (`LayoutComputer` passes, the contributors, `CommentIndentationContributor`, `IndentationRewriter`) | Must change; how far depends on Question 1 |
| `SyntaxTokenPositionUtilities.IsFirstOnLine` (Core) | Must stay: shared with RH5204 and RH5203; changing it changes those analyzers (escalation 5) |
| `LineBreakPhase`, `BlankLinePhase` | Must stay (owned by run R1); only a variant of 1A that keeps holes unbroken would reach them |
| `CleanupPhase` | Must stay (owned by run R2); see the FMT-CLEAN-002 finding |
| RH5202 analyzer and code fix | Must stay. Checked with `scripts/apply-fix.sh RH5202`: the fix produces FMT-RAW-007's output for case 1 and FMT-RAW-008's for case 2 (2 iterations), and the expected outputs raise no RH5202 |
| RH5204 | Must stay: does not report the expected FMT-RAW-007 outputs or frozen hole layouts (apply-fix: no-diagnostic) |
| RH5203, RH5205, RH5206 | Must stay under 1B. Under 1A they report shapes the formatter would keep: apply-fix on frozen hole layouts reports RH5203 (`M(1,⏎2)`) and RH5205 (lambda brace); RH5206 reports, and its fix makes no progress. That conflicts with SHARED-DIAG-010 (FC-013). RH5205 and RH5206 fix through `ReihitsuFormatter.FormatNodeInDocumentAsync`, so a formatter that freezes hole lines also stops their fixes (FC-032) |
| RH5201 | Not involved (no diagnostic on a frozen chain in a hole; owned by run R1) |
| Rule docs | Not involved |
| Tests | Formatter regression tests over two passes, LF and CRLF |

### Simpler option

1B in Question 1 is the narrower spec. It removes only the shape that flips: hole lines at a block column. Hole lines whose alignment already moves with the literal keep their alignment.

### Questions for the maintainer

**Question 1 — How far does "hole lines move only with the literal" reach?**

**Context.** FMT-RAW-007 and FMT-INDENT-011 (spec diff) freeze every line that starts inside a hole of a multi-line interpolated raw string: it moves by the literal's delta and in no other way. This follows the decision on thoenissen/Reihitsu#922, Q1 ("the indentation phase skips every token that starts a line inside a multi-line raw string literal, including the tokens of its interpolation holes"). Runs on the current code show that the freeze also reaches two classes of input that are stable today.

(a) **Hole lines aligned to an anchor on the literal's lines** (FMT-INDENT-012, FMT-INDENT-027, FMT-INDENT-028). Today they are aligned, and they stay put on every further pass. Frozen, they keep the author's columns, and three layout rules report them: RH5203 and RH5205 (their fixes apply), and RH5206 (its fix makes no progress). Those rules lay code out as the formatter does, so SHARED-DIAG-010 forbids them to report a shape the formatter keeps.

(b) **Lines an earlier phase breaks inside a hole in the same pass.** Input, aligned literal:

```csharp
        var a = $"""
                 Hello {M(() => { return 1; })}
                 """;
```

Today the formatter's output, stable from then on:

```csharp
        var a = $"""
                 Hello {M(() =>
                          {
                              return 1;
                          })}
                 """;
```

**Question.** Should every hole line be frozen, or only the hole lines whose column comes from the scope depth alone?

- **1A — Freeze every hole line (the spec diff as written).** Output for (b):

  ```csharp
          var a = $"""
                   Hello {M(() =>
  {
  return 1;
  })}
                   """;
  ```

  Inputs of class (a) keep the author's columns. Consequences:
  - RH5203, RH5205, and RH5206 must stop reporting inside holes of multi-line raw strings — three analyzers outside the run, escalation 5 — or they report formatter output for good.
  - The lines (b) creates start at column 0, unless `LineBreakPhase` and `BlankLinePhase` (owned by R1) stop breaking lines inside holes.
- **1B — Freeze only the hole lines whose FMT-INDENT column comes from the scope depth alone** (the block column of FMT-INDENT-002), such as `name` and `}` in case 1. A hole line that an alignment requirement places relative to a token on the literal's lines, directly or through the construct around it (FMT-INDENT-012 to FMT-INDENT-029), keeps that alignment, measured at the token's column after the literal's shift.
  - Case 1 and case 2 give the outputs FMT-RAW-007 and FMT-RAW-008 state.
  - (a) and (b) keep today's output.
  - Consequences: FMT-RAW-007 and FMT-INDENT-011 are rewritten more narrowly, which departs from the wording of the #922 Q1 decision. No analyzer changes. A line with no anchor that the line-break phase creates, such as the braces of `delegate { return 1; }`, still stays at the column that phase gives it.

**Recommendation: 1B.** It removes exactly the shape that is not idempotent and keeps both the stable alignment of today and the agreement between RH5203/RH5205/RH5206 and the formatter. 1A is the pick if the maintainer wants a hole's layout to stay the author's own; the price is narrowing three alignment rules in the same release.

**Question 2 — Lines inside a verbatim string or a `/* */` comment nested in a hole.**

**Context.** Today the raw-string phase shifts every line after the opening line, including the continuation line of a nested verbatim string. Input:

```csharp
        var a = $"""
    Hello {@"a
b"}
    """;
```

becomes `             b"}` on that line: the verbatim string's value changes from `a⏎b` to `a⏎             b`. A `/* a⏎   b */` comment in a hole has its second line shifted the same way. FMT-RAW-007's main clause, read inclusively, requires that shift. That contradicts FMT-PIPE-002 (a literal's text), FMT-INDENT-011 (a verbatim string's continuation lines), and FMT-INDENT-007 (a comment's following lines are kept as written). #951 does not report this; the maintainer limited the run to cases 1 and 2.

**Question.** Does this run make these lines stay as written, or leave the shift for a follow-up?

- **2A — In scope.** FMT-RAW-007 states that lines starting inside the text of a nested literal (other than a raw string, FMT-RAW-008) or of a nested comment stay as written, and tests and the phase change cover them. Output: `b"}` stays at column 0. Consequence: the PR fixes a value-changing defect that #951 does not name, and says so.
- **2B — Out of scope.** FMT-RAW-007 excludes these lines, a coverage sentence leaves them unspecified, and a follow-up issue tracks them. Consequence: the formatter keeps changing such values until the follow-up lands.

**Recommendation: 2A.** The phase change has to decide every line after the opening line anyway. A formatter that changes a literal's value breaks FMT-PIPE-002, the formatter's basic invariant.

### Follow-up candidates

- **RH5202's fix changes literal values.** It shifts every line after the first line of the literal, and so changes the value of a verbatim string, or the text of a `/* */` comment, nested in a hole. Observed with apply-fix: `b"}` moves 13 columns. That violates SHARED-FIX-003. The fix's `RealignContent` is a second copy of the phase's shift (FC-015).
- **RH5206's fix makes no progress for a switch expression inside a hole** of an interpolated raw string, on `main` today (apply-fix: `no-progress (2 iteration(s))`; FC-009).
- **Under 1A only:** RH5203, RH5205, and RH5206 stop reporting inside holes of multi-line raw strings.

## thoenissen/Reihitsu#1133

### Premise

The bug is real. `scripts/apply-fix.sh RH5601` over the issue's input (`$"""`, `→→a`, `→→""";`) reports `invalid-result (1 iteration(s))` under LF and CRLF: the closing line becomes `    →""";`, which no longer matches `→→a`. The non-interpolated twin is `no-diagnostic`. The formatter keeps the tabs inside the literal (FMT-CLEAN-004). RH5601 is a layout rule whose fix does what the formatter does (FMT-CLEAN-002), so reporting the tabs is also an FC-013 conflict under SHARED-DIAG-010. The fix removes that conflict.

Characterizations of the spec diff, which hold today:

- RH5601-001's new example `{1⏎→+ 2}` is reported and fixed.
- RH5602-002's new example `{1}··` in an interpolated raw string is not reported.

### Mechanism and candidates

Mechanism: the analyzer, and the fix through the same predicate, treat a tab as literal content only when the token whose full span contains it, trivia included, is one of a fixed list of literal token kinds (`IsStringContentToken` with `root.FindToken`). The end token of an interpolated raw string, whose text carries the closing line's indentation, is not in that list.

Dispatch: `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken`, which `RH5601UseTabsCorrectlyCodeFixProvider` calls as well.

- In the list: `StringLiteralToken`, `CharacterLiteralToken`, `InterpolatedStringTextToken`, `SingleLineRawStringLiteralToken`, `MultiLineRawStringLiteralToken`, `Utf8StringLiteralToken`, `Utf8SingleLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`.
- Not in the list: `InterpolatedStringStartToken`, `InterpolatedVerbatimStringStartToken`, `InterpolatedSingleLineRawStringStartToken`, `InterpolatedMultiLineRawStringStartToken`, `InterpolatedStringEndToken`, `InterpolatedRawStringEndToken`.

Of these, two can hold a tab in their text:

- `InterpolatedRawStringEndToken` of the multi-line form, in the closing line's indentation (the issue);
- `InterpolatedMultiLineRawStringStartToken`, after the opening quotes: `$"""→` is reported and fixed, while `"""→` is not, and the formatter keeps it (Question 3).

Every token in both lists can also hold a tab in its trivia. `FindToken` returns the token for its trivia too.

### Breaking inputs

Failure classes:

- **FC-001 and FC-003:** a tab inside the token's text is a different position from a tab in its trivia, and RH5601-001 keeps the trivia reported.
- **FC-016:** the start token, and `$$"""`.
- **FC-006:** the fix's result must compile.
- **FC-013:** the formatter's output.

Inputs:

- **Exempt now.**
  - The issue's input, verbatim.
  - The closing line after a hole line (`→→{x}`, `→→""";`).
  - `$$"""` with `→→{{x}}` and `→→""";`.
  - Mixed indentation `→··a` / `→··"""`.
- **Stay reported and fixed (RH5601-001, and the escalation answer: whitespace next to a literal keeps its current behavior).** All four are reported and fixed today:
  - a tab in the trailing trivia of the interpolated raw end token, `"""→;`;
  - `$"a"→;`;
  - `→$"""` at the start of a continuation line, in the start token's leading trivia;
  - the hole-line tab `→+ 2}`.
- **Stay not reported.** The non-interpolated `"""→;` and the non-interpolated twin of the issue's input.
- **`$"""→` after the opening quotes.** Depends on Question 3.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5601 analyzer | Must change |
| RH5601 code fix | Changes only through the shared predicate; otherwise must stay (Fix All provider unchanged) |
| RH5602 analyzer and code fix | Must stay: spec clarification and characterization only; RH5602-001's touched-line rule already exempts these lines |
| `FormattingTextAnalysisUtilities.GetStringLineIndices` (Core, also used by RH5204 and RH5602; touched by R8) | Must stay (escalation answer A) |
| Formatter | Not involved |
| Rule docs | `RH5601.md` changed by the spec author; `RH5602.md` is net unchanged |
| Tests | Analyzer and code-fix tests with real tabs (the test base maps `→` to a tab) |

### Simpler option

_None._ The change is already confined to the token positions the escalation answer exempts.

### Questions for the maintainer

**Question 3 — A tab after the opening quotes of an interpolated multi-line raw string.**

**Context.** RH5601 reports `string s = $"""→` (a tab after the opening quotes, inside the start token) and fixes it to four spaces. The non-interpolated `string s = """→` is not reported. The formatter keeps both: `scripts/trace.sh` leaves the tab untouched. RH5601-002 (spec diff) names only "the indentation of the content lines and of the closing line". Escalation answer A lists "raw-string line indentation, the closing line included, interpolated or not; hole content".

**Question.** Is the whitespace after the opening quotes of an interpolated multi-line raw string part of the literal's text for RH5601?

- **3A — Yes.** Not reported, like `"""→`, and RH5601-002 names it. Consequence: RH5601 stops reporting a shape the formatter keeps (SHARED-DIAG-010), at one more token position.
- **3B — No.** Still reported, and fixed to spaces. Consequence: the asymmetry with the plain raw string stays, and RH5601 keeps reporting formatter output for this input (FC-013).

**Recommendation: 3A.** SHARED-DIAG-010 does not let RH5601 report a tab that the formatter keeps inside a literal (FMT-CLEAN-004), and the escalation answer's "interpolated or not" asks for parity with the plain raw string.

### Follow-up candidates

_None_ beyond the RH5601/RH5602 gap against SHARED-DIAG-003 that `01-spec-author.md` already drafts.

## thoenissen/Reihitsu#1146

### Premise

Both cases in scope are real. Checked with `scripts/apply-fix.sh RH5104`, under LF and CRLF.

**Q1.** Today the fix writes the comment into the literal or the documentation comment, and adds a blank line:

- verbatim string `@"a⏎b" + /* c */ x;`: the comment is inserted inside the string, changing its value;
- raw string, closing line `""" + /* c */ x;`;
- interpolated raw string;
- UTF-8 raw string `"""u8.Length + /* c */ x;`;
- interpolated verbatim string `$@"a⏎{x}b" + /* c */ x;`;
- `/** <summary>⏎M</summary> */ void M() { } // c`: `// c` lands inside the documentation comment.

**Q1, other side.** `var f = /* c */ @"a⏎b" + x;` is fixed correctly.

**Q4.** `/* a */ /* b */` and `/* c */ // d` on code-free lines are reported. In the runner's order the first comment is fixed first, which keeps the order. A single fix on the second comment moves it above the first. `/** a */ /* b */` above a member is reported and fixed to `/* b */` above `/** a */`, a reordering even with a single fix.

**RH5104-004.** On `main`, RH5104-004 already offers the fix only when the comment's line does not start inside a literal or a comment. The fix has no such guard, so Q1 is a bug against RH5104-004 and SHARED-FIX-003. Q4 is a bug against RH5104-001 ("a token precedes it or follows it").

**RH5104-006's list (finding for the spec author).** Its main clause covers every line that begins inside the text of a literal or inside a multi-line comment. Its dash list names verbatim, raw, and interpolated raw strings and `/** */`, but omits three kinds:

- plain `/* */`: in `/* a⏎   b */ M(); // c`, a single fix on `// c` inserts it inside the block comment;
- interpolated verbatim `$@"`: observed above;
- UTF-8 raw `"""u8`: observed above.

**J1 (RH5104-005).** Existing tests assert the old blank-line insertion, for example `VerifyRelocatedCommentGetsPrecedingBlankLineWhenPreviousLineIsCode`. After the fix, RH5020 reports the moved comment below a code line: checked with `scripts/apply-fix.sh RH5020` on `M();⏎// second⏎M();`. The fix thus raises another RH diagnostic in the edited span (FC-007). J1 and SHARED-FIX-006 accept that by design, and RH5104-005's `Why` says so.

### Mechanism and candidates

**Mechanism, Q4.** `IsInlineComment` decides whether a comment shares a line with code from the line's text: any non-whitespace character outside the comment counts. So another comment on the line counts as code.

Candidates, from the trivia and token kinds that can put non-whitespace on a line next to a comment:

- **Other comments:**
  - `SingleLineCommentTrivia`, `MultiLineCommentTrivia`, `MultiLineDocumentationCommentTrivia`;
  - the first or the last line of a multi-line `/* */` or `/** */` that shares the line.
- **Zero-width tokens**, whose risk is the reverse: `EndOfFileToken` on the last line, for a comment-only last line with no line break after it. It is a token without text and must not count as code (FC-002).
- **Directives** are excluded separately and cannot share a line with a comment.

**Mechanism, Q1.** The fix inserts the comment at the start of the line on which the comment starts. It does not check whether that position lies inside the text of a token or inside comment trivia that began on an earlier line, so the insertion becomes part of that text.

Candidates for the position in which that line begins:

- **Token text:**
  - `StringLiteralToken` (verbatim) and `Utf8StringLiteralToken` (verbatim);
  - `MultiLineRawStringLiteralToken` and `Utf8MultiLineRawStringLiteralToken`, whose closing line is part of the token;
  - `InterpolatedStringTextToken` of `$@"` and `$"""`;
  - `InterpolatedRawStringEndToken`, the closing line of `$"""` and `$$"""`.
- **Trivia:** `MultiLineCommentTrivia` and `MultiLineDocumentationCommentTrivia`.
- **An interpolation hole of a multi-line `$"` or `$@"` string** — code, not text (Question 4).

Not candidates:

- `SingleLineDocumentationCommentTrivia`, because a code-sharing comment cannot start on a line inside it;
- disabled text, which holds no comment trivia;
- a hole of `$"""`, because a comment there is exempt (RH5104-002) or is text.

### Breaking inputs

Failure classes:

- **FC-002:** "token" means a token with text; the end-of-file case.
- **FC-003:**
  - a line that begins inside a literal versus a line on which the literal starts (the other side of RH5104-006);
  - a comment-only line versus a line where one of the comment's lines holds a token.
- **FC-016:** each kind in the lists above.
- **FC-009:** a withheld fix registers no action.
- **FC-008:** Fix All with a withheld diagnostic beside a fixable one, and the existing `GetFixAllScenario` collision.
- **FC-022:** literal kinds.
- **FC-004 and FC-021:** the relocation itself.

Inputs:

- **Withheld, reported with no action (RH5104-006).** Each Q1 shape above, plus plain `/* a⏎   b */ M(); // c`. In that shape the `/* a…b */` comment itself is reported, and its fix is still offered.
- **Fixed (RH5104-004):** `var f = /* c */ @"a⏎b" + x;`.
- **Not reported (RH5104-002):**
  - `/* a */ /* b */`;
  - `/* c */ // d`;
  - `/* x⏎   y */ /* b */` (a tail of a multi-line comment);
  - `/** a */ /* b */` and `/** a */ // b` above a member;
  - a comment-only last line of the file without a line break.
- **Still reported:**
  - `/* a */ M(); // b`;
  - `/* a */ /* b */ M();`.
- **RH5104-005:** the spec's example. The test implementer updates the existing blank-line tests to the new RH5104-005.
- **Do not codify `/* a */ /* b⏎   c */ M();`.** After the Q4 clarification, only `/* b…c */` is reported, and RH5104-004 moves it above `/* a */` — a reordering. Today the runner fixes `/* a */` first and the order holds. See the follow-up candidates.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5104 analyzer | Must change (Q4) |
| RH5104 code fix | Must change (Q1 guard; J1 removes the blank-line insertion) |
| Fix All | Must stay (`BatchFixer`; a withheld diagnostic gets no action) |
| `BlankLineEditor.IsFirstInBlock` (formatter blank-line phase, owned by R1) | Must stay. Only the RH5104 fix stops calling it; another consumer exists (`BlankLineTriviaBoundaryRewriter`) |
| `FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex` (Core, touched by R8) | Must stay (also used by RH5020 and RH8303) |
| RH5020 | Not involved; it reports the moved comment below code by design (J1) |
| Formatter | Not involved |
| Rule doc `RH5104.md` | Changed by the spec author |
| Tests | New tests; the existing tests that assert the old RH5104-005 are updated |

### Simpler option

_None._

### Questions for the maintainer

**Question 4 — A comment whose line begins inside an interpolation hole.**

**Context.** Input, a regular interpolated string whose hole spans lines:

```csharp
        var g = $"{
            x}" + /* c */ y;
```

Today it is reported, and the fix moves the comment into the hole (apply-fix: the result parses):

```csharp
        var g = $"{
            /* c */
            x}" + y;
```

A `// c` at the end of that line, and the same shape with `$@"`, behave the same way. RH5104-004 offers the fix only "when the line on which the comment starts does not start inside a literal". The new RH5104-006 says "inside the text of a literal" and offers the fix otherwise. Since a hole is code (SHARED-DIAG-003), RH5104-006 offers the fix here. The two requirements read differently for this input.

**Question.** Is the fix withheld when the comment's line begins inside an interpolation hole?

- **4A — Offered**, with the output above. Consequence: the comment lands in the string's hole, where RH5104-002 no longer reports it. No code change.
- **4B — Withheld.** Reported, no fix, and RH5104-006 names the hole case. Consequence: one more condition in the fix's guard. The rule page's "inside a multi-line string literal" already reads as covering it.

**Recommendation: 4B.** The moved comment ends up inside the interpolated string, away from the statement it described — the outcome that RH5104-006's `Why` rejects for literals. RH5104-004's own condition already says "inside a literal".

### Follow-up candidates

- **RH5104: the fix moves a comment above another comment that precedes it on its start line.**
  - In `M(); /* a */ // b` (the candidate from `01-spec-author.md`), fixing `// b` first gives `// b` above `/* a */`. The runner's first-diagnostic order keeps the order, but a single fix on `// b` reorders.
  - After the Q4 clarification, `/* a */ /* b⏎   c */ M();` reorders whenever it is fixed.
  - Both are outside Q1 and Q4 as scoped.
- **thoenissen/Reihitsu#1433, Q6, is superseded by J1.** The fix no longer inserts any blank line, so the blank line between `/* note */` and the moved comment that Q6 reports can no longer occur. #1433 needs updating.

## Whole run

Commands, all over scratch fixtures outside the repository, which were deleted afterwards:

- `scripts/trace.sh <fixture> --passes 3 --no-install`: about 25 raw-string fixtures, two of them CRLF.
- `scripts/apply-fix.sh RH5601 <dir> --no-install`: 13 fixtures in three runs. The first run, 10 fixtures × 2 line endings, gave 8 fixed, 6 no-diagnostic, 6 invalid-result; all the invalid results are closing-line tabs of interpolated raw strings.
- `scripts/apply-fix.sh RH5104 <dir> --no-install`: 17 fixtures.
- `scripts/apply-fix.sh` for RH5602, RH5020, RH5202, RH5204, RH5201, RH5203, RH5205, RH5206 over the fixtures named above.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests" --no-install`: 22 passed, 0 failed (baseline on `2178115`).

`scripts/trace.sh` has no execute bit in this checkout (`-rw-r--r--`; the other scripts are executable), so I ran it through `bash`. Follow-up candidate (tooling, outside this run): restore its execute bit.

No repository file other than this handover was changed.

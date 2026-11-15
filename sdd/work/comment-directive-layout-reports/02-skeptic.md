Role: skeptic
Commit: 123f3b8
Verdict: #1147: proceed (architecture finding); #1152: proceed; #1386: proceed; #1387: proceed narrower; #1594: proceed narrower; #1655: output question; #1736: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The code under review is `main`'s code: the branch changes only specs and docs up to 123f3b8, whose spec diff is 3b1b3d3. Every "today" below comes from `scripts/apply-fix.sh` or `scripts/format.sh` on fixtures in a scratch directory outside the repository, or from `dotnet build` of a scratch project, under LF and CRLF alike unless stated. The scratch directory has been deleted.

## thoenissen/Reihitsu#1147

### Premise
The issue is real. A caller reaches it through whole-document formatting: the formatter produces the comment-led shapes, and the rules report them. I built one fixture with the comment in the line's correct column ("ok") and one with the comment misplaced while the token behind it lands in the correct column ("bad").

| Rule | ok on `main` | bad on `main` | Requirement says |
|---|---|---|---|
| RH5110 `void N<T>()⏎/* c */ where T : class` | reported, `no-progress` | not reported | ok: not reported; bad: reported |
| RH5203 `M(1,⏎/* two */ 2)` | reported, `no-fix-offered` | not reported | same |
| RH5205 `x =>⏎/* body */ {` | reported, `no-progress` | not reported | same |
| RH5206 arm `/* idle */ 0 =>` | reported, `no-progress` | not reported | same |
| RH5206 brace `switch⏎/* map */ {` | reported, `no-progress` | not reported | same |
| RH5302 `return a⏎/* and */ && b` | reported, `no-progress` | not reported | same |
| RH5305 `/* end */ ];` | reported, `no-progress` | not reported | same |
| RH5306 `/* e */ ];` (bracket) | not reported | **not reported** | bad: reported |
| RH5308 `/* t */ ? 1` | reported, `no-progress` | not reported | same |
| RH5301 `/* a */ /* b */ A = 1,` | **reported, `no-progress`** | reported | ok: not reported (SHARED-INIT-001 as amended) |
| RH5301 `/* a */ /* b */ };` at the `{` column | **reported, `no-progress`** | – | not reported (amended SHARED-INIT-001) |
| RH5303 `/* a */ /* b */ 1,` | **reported, `no-progress`** | reported | same as RH5301 |
| RH5304 `/* a */ /* b */ 1,` in `L = { … }` | **reported, `no-progress`** | (one comment: reported, fixed) | same as RH5301 |
| RH5301 with one comment | not reported | – | holds |

On every "bad" fixture, `scripts/format.sh` moves the comment to the correct column (checked for RH5110, RH5206, RH5304). The "ok" shape is therefore formatter output, and `main` reports it. That violates SHARED-DIAG-010 as well as SHARED-COMMENT-011.

The RH5301 fix on the two-comment "bad" fixture writes the "ok" shape in its first iteration. `main` still reports that shape, so the second iteration makes no progress. With the amended SHARED-INIT-001, the fix converges in one iteration.

### Mechanism and candidates
Mechanism: the rule takes the column, or the line start, of the token it judges, instead of the column of a comment that starts the token's line (SHARED-COMMENT-011). Dispatch: the requirement applies to every rule spec whose `Uses` lists SHARED-COMMENT-011:
- RH5110, RH5112 (exempt under SHARED-CHAIN-004), RH5201, RH5203, RH5204, RH5205, RH5206;
- RH5301 to RH5306, RH5308;
- RH7304 (see #1594).

RH5201, RH5204, and the one-comment case of RH5301/RH5303 already hold.

### Breaking inputs
FC-013, FC-015, FC-016, FC-003, FC-007, FC-021. Matrix of the class:

| Axis | Members |
|---|---|
| Comment column | the line's correct column (not reported); another column with the token behind it in the correct column (reported); another column with the token in another column (reported) |
| Comments in front of the token on its line | one; two or more (SHARED-INIT-001 amendment) |
| Comment shape | `/* */`; `/** */`; a multi-line `/* */` whose tail ends on the token's line (P33: the tail is not a first element, and SHARED-INIT-001 checks the comment's start column) |
| Token | each rule's judged token: constraint, argument, lambda brace, switch arm, switch brace, operator, `?`/`:`, collection-expression and list-pattern brackets, initializer member, element, and braces |

Failing members: every row in bold or marked "not reported" for bad, in the table above.

For each rule, the test implementer needs the "ok" input as not reported and the "bad" input as reported, which is the P12 clarification in the issue comment of 2026-10-22. Each newly reported "bad" input also needs a code-fix result: the fix must converge in one pass (FC-007), or `no-fix-offered`. Before this run, nothing reported these inputs, so no fix behavior for them exists to inherit.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH5110, RH5203, RH5205, RH5206, RH5302, RH5305, RH5306, RH5308 | must change (both directions) |
| `InitializerLayoutAnalysisUtilities` (RH5301, RH5303, RH5304) | must change (two or more comments) |
| RH5201, RH5204, RH5112 | must stay |
| Code fixes of the rules above | must stay; test the newly reported misplaced-comment inputs for convergence |
| Formatter | not involved (it already lays these lines out as the specs say) |
| Rule pages | not involved |

### Refuse option
Withholding a fix does not help here: each defect is a report, not a fix. Not reporting the misplaced-comment direction would leave the rule's own gap in place. P11 classifies that gap as the rule's defect, and SHARED-COMMENT-011 demands the report. Declaring a comment that leads a line unsupported is not an option: it would refuse code people commonly write.

### Output questions
_None._ SHARED-COMMENT-011 and the amended SHARED-INIT-001 decide every row. The amendment applies the maintainer's decision #1362 Q7.

### Internal choices
_None._

### Issue goals
- The "Done when" item "the same shapes without the leading comment are still reported and fixed" says nothing: without the comment, the token stands in the right column and is not reported. Replace it with the misplaced-comment variant from the issue comment of 2026-10-22, which is reported. This changes no visible output beyond what SHARED-COMMENT-011 demands.
- The lead's row for #1147 in `00-lead.md` omits RH5304 and RH5306. Both have failing members (above), and both specs use SHARED-COMMENT-011 and SHARED-INIT-001.

### Architecture findings
The rules decide SHARED-COMMENT-011 privately: what a line's first element is, which column it stands in, and whether a multi-line comment's tail counts.
- RH5201 has a private `GetLineStartColumn` (`src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs:59`).
- RH5204 has its own `endsMultiLineCommentOnLine` logic in `AlignCommentIndentation`.
- `InitializerLayoutAnalysisUtilities.IsAlignedAt` serves the initializer rules.
- RH5110, RH5203, RH5205, RH5206, RH5302, RH5305, RH5306, RH5308, and RH7304 each measure the token.

The rules that already hold are the ones with their own copy. The rules that fail are the ones without one. A narrow fix adds about nine more private copies (FC-015, FC-016). This is "one rule stated in more than one place" at the code level (`squad/README.md`, *Architecture findings*), within the components the run changes. The lead decides.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1152

### Premise
The issue is real. RH5204's comment branch locates the diagnostic with `trivia.GetLocation()` (`RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs:113`). For structured documentation trivia, that span leaves out the `///` or `/**` exterior and includes the comment's final line break. Existing tests lock in exactly this:
- `VerifySingleLineDocumentationCommentOnOwnLineIsDetectedAndFixed` marks `///{|#0: Doc⏎|}`.
- `VerifyMultiLineSingleLineDocumentationCommentIsDetectedAndFixed` marks the same shape.
- Four `/**{|#0: Doc */|}` tests mark the span starting after `/**`.

Whole-document formatting is not the caller here. The analyzer reports the location.

### Mechanism and candidates
Mechanism: the location is the span of the structured trivia, which excludes the leading exterior token. With `DocumentationMode.None`, the same text is ordinary comment trivia, whose span includes `///` and excludes the line break. So today's location also depends on the documentation mode.

Candidates: `SingleLineDocumentationCommentTrivia` with one line, with several lines, and with indented continuation lines; `MultiLineDocumentationCommentTrivia` on one line and across several lines. Each one in Parse mode and in None mode.

### Breaking inputs
FC-040 (documentation mode), FC-017. The `/** */` tests change too: RH5204-005 as clarified names `/**`.

The code fix must stay correct. It finds the comment with `root.FindTrivia(diagnostic.Location.SourceSpan.Start)` and re-indents through `trivia.FullSpan.End` (`RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider.cs:57-61`). Once the span starts at `///`, a multi-line `///` block must still be re-indented as a whole, under LF and CRLF.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| RH5204 analyzer (location) | must change |
| RH5204 code fix | must stay (test a multi-line `///` block and `/** */` against the new start) |
| Formatter, rule page | not involved |

### Refuse option
_None._ The change is a location only.

### Output questions
_None._ RH5204-005 and the maintainer's decision #1124 Q2 decide it, and the clarification covers `/**` by the same reading.

### Internal choices
_None._

### Issue goals
"Existing RH5204 tests stay green" cannot hold: six existing tests assert today's span and must change, the `/** */` ones included. This puts no visible output in question.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1386

### Premise
The issue is real. `CommentPositionUtilities.IsOrdinaryComment` (`src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs:23`) returns false for every documentation-comment trivia. In None mode, it also returns false for every `///` or `/**` text, whether or not the comment documents anything. Three places use it:
- `CommentPositionAnalyzerBase`, which RH5033 to RH5049 use directly or through `CommentRegionAnalyzerBase`;
- RH5048, directly;
- RH5034, through `IsDocumentationComment`.

So all 17 rules share one decision. I compiled a scratch library with documentation output. CS1587 is reported for:
- `[Obsolete]⏎/// …⏎void M()` and the `/** */` variant (instances 1–2);
- `/// a⏎// note⏎/// b⏎void N()`: the compiler reports CS1587 for `/// a`, and N's documentation is `b` alone (instance 4).

The compiler's rule, observed in the same probe:
- A documentation comment followed by a later documentation comment in the same leading trivia, with an ordinary comment or a directive (`#region`) between them, documents nothing.
- A blank line does not detach it.
- A documentation comment followed only by ordinary comments still documents the declaration (`/// A1⏎// note⏎void A()` documents `A`).

### Mechanism and candidates
Mechanism: the filter decides by the comment's kind or text, where SHARED-COMMENT-001 decides by whether the comment documents a declaration.

Candidates:
- rules RH5033 to RH5049;
- kinds `///` and `/** */`;
- modes Parse and None;
- for RH5034, the detached-by-a-later-documentation-comment shape (separated by a `//`, a `/* */`, or a directive) against the blank-line shape, which stays attached.

### Breaking inputs
FC-040, FC-016, FC-005. For RH5034, the issue's fourth instance under SHARED-COMMENT-001 and RH5034-001: `/// a` documents nothing, so it is an ordinary comment in front of the documentation comment `/// b`, and `// note` is not behind a documentation comment. RH5034 then reports neither of them. Today it reports `// note`. That is a visible change, decided by the unchanged RH5034-001 ("a comment before the documentation comment … MUST NOT be reported"). It is not the issue's reading.

In None mode, the analyzer has to reproduce that detachment by text (SHARED-COMMENT-001, second paragraph).

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `CommentPositionUtilities` (one owner for all 17 rules) | must change |
| RH5034 | must change (instance 4 is no longer reported) |
| Formatter, code fixes (none exist), rule pages | not involved |

### Refuse option
_None._ These rules have no code fix.

### Output questions
_None._ SHARED-COMMENT-001 and RH5034-001 decide every instance, instance 4 included.

### Internal choices
- SHARED-COMMENT-001 lists where a `///` "documents nothing" by position. It does not name the compiler's detachment of an earlier documentation comment by a later one across an ordinary comment or a directive, yet None mode has to decide that case by text. Recommend adding it to the list as a clarification. It is decided by the compiler's verdict, so it puts nothing visible in question.

### Issue goals
Instance 4's expected result is "judged as an ordinary comment as well". Judged that way, the result is "not reported, and `// note` no longer reported either". The PR should state this, because it reads differently from the issue.

### Architecture findings
_None._ One owner already exists.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1387

### Premise
Partly stale.
- RH5102 `M(a, b,⏎#if DEBUG⏎c,⏎#endif⏎d);`: `no-diagnostic` on `main`.
- RH5109 `void M(int a, int b,⏎#if …)`: `no-diagnostic`. `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundaryInElementGaps` already applies SHARED-DIAG-008.
- RH5111 `int value⏎#if DEBUG⏎#endif⏎= 1;`: `no-diagnostic`.
- Real: RH5506 `[A,⏎#if DEBUG⏎ B,⏎#endif⏎ C]` and RH5524 `[A]⏎#if DEBUG⏎[B]⏎#endif⏎[D] int a` are both reported with `no-fix-offered`.
- `#pragma` variants: RH5102, RH5109, and RH5506 report without a fix, as asked.

### Mechanism and candidates
Mechanism: `TargetAttributeListShapeAnalyzerBase` withholds the fix for any directive in the list but never drops the report for a conditional one. All 16 list-shape rules derive from it:
- 14 one-attribute-per-list rules (SHARED-ATTR-019): RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, RH5531;
- 2 merge rules (SHARED-ATTR-020): RH5524, RH5529.

The placement rules under SHARED-ATTR-011 are not candidates: a directive inside the list is outside the span they judge.

### Breaking inputs
FC-020, FC-005, FC-003.
- Split rules: `#if` between attributes (not reported); `#if` inside an attribute's argument (reported, no fix); `#pragma`/`#region` between attributes (reported, no fix); disabled text that holds a whole attribute.
- Merge rules: `#if` between two lists of the target (not reported); `#if` in front of the first list (reported, as the `P` example says); a directive behind the reported list.

The formatter keeps every one of these lists as written, `#pragma` included (checked). SHARED-DIAG-010 does not cover a list-shape rule, so the `#pragma` report stands.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `TargetAttributeListShapeAnalyzerBase` | must change |
| RH5102, RH5109, RH5111 conditional-directive behavior | must stay (characterization tests only) |
| Attribute-list code fixes | must stay |
| Formatter, rule pages | not involved |

### Refuse option
_None._ Nothing is fixed here, and the fix is already withheld.

### Output questions
_None._ The RH5111 `#pragma` case belongs to #1655.

### Internal choices
_None._

### Issue goals
- "Each input above behaves as stated" already holds on `main` for the RH5102, RH5109, and RH5111 instances. Give them characterization tests, not fixes.
- "`#region`/`#pragma` variants stay reported" holds for RH5102, RH5109, and the attribute lists. For RH5111 it contradicts SHARED-DIAG-010: the formatter keeps such a gap (checked; see #1655). Drop it for RH5111, which output question 2 covers.
- The issue's "tests `VerifyDirectiveInside…WithholdsTheCodeFix` of these rules change" applies only to the attribute rules.

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond the spec author's F2 (RH3002-009). I agree that F2 is out of scope.

## thoenissen/Reihitsu#1594

### Premise
The issue's input is invalid C#: a scratch build gives CS1040 at `/* c */ #region` and CS1028 at `#endregion`. The formatter and the CLI leave syntax-invalid files alone, so no caller reaches it.

Two valid inputs reach the case the issue names (the region's first token does not start its line). In both, whole-document formatting places the directives in the comment's column, and RH7304 then reports both directives on the formatter's output (`no-fix-offered`). That violates SHARED-DIAG-010.
- One-line comment: `#region Fields⏎/* c */ int _a;` is reported on `main`.
- Multi-line comment, the issue's named "multi-line variant": `#region Fields⏎/* c⏎ */ int _a;` is reported on `main`.

`GetIndentation` takes the line's text up to the token, `/* c */ ` or ` */ `, as the indentation (`RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.cs:58`), so that text can never match a directive's whitespace.

### Mechanism and candidates
Mechanism: the expected indentation is the line's text in front of the first token, not the column of the line's first element.

Candidates:
- a one-line comment in front of the token (spec covered);
- two comments in front of it (`/* a */ /* b */ int _a;` is reported on `main`, and the spec's wording covers it);
- a multi-line comment that ends on the token's line (spec author's F3, left unspecified);
- an own-line `// c` above the token (main compares with the token and fixes it; must stay);
- the directives in the token's column but not the comment's (must be reported).

### Breaking inputs
FC-013, FC-021, FC-003.
- The spec's `Names` example does not show "the token behind the comment does not count": its directives match neither column. Add a test where the directives stand in the token's column and the comment stands left of it, for example `#region` at column 12 with `    /* e */ int _a;` (column 4). It is reported, and already is on `main`.
- Test the multi-line variant formatted (not reported) and misaligned (see *Internal choices*).

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| RH7304 analyzer (`GetExpectedIndentation`) | must change |
| RH7304 code fix | depends on output question 1. It calls `GetExpectedIndentation`, so A needs no change of its own beyond that helper. |
| Formatter | not involved (it already places these directives in the comment's column, also for the multi-line comment) |

### Refuse option
Withholding the fix (option B of output question 1) is the refusal. Its cost is a guard in place of no change: under A, the fix already follows the analyzer's helper. The class is rare.

### Output questions
- Spec author's question 1 (RH7304 fix for a comment-led region line): I agree with A. Evidence: `scripts/format.sh` on the input of that question gives exactly the output under A (directives at column 4). Recommend adding the multi-line comment input to the same question: its output under A is again the formatter's output (checked).

### Internal choices
The multi-line comment (`#region Fields⏎    /* c⏎     */ int _a;`) has two readings:
- (i) Compare the directives with the column where that comment starts, as SHARED-INIT-001's first bullet already does for a comment that starts on an earlier line.
- (ii) Do not report the region at all, as the issue asks.

On formatter output, both readings give "not reported", as SHARED-DIAG-010 demands, which `main` violates. With misaligned directives, (i) keeps `main`'s report and (ii) removes it. So (i) changes nothing visible that SHARED-DIAG-010 does not demand, and (ii) does.

Recommend (i): it is a cheap trivia check, it is consistent with SHARED-INIT-001, and it lets RH7304-001 include this input instead of leaving it unspecified. If the lead takes (ii), it is an output question. I disagree with the spec author's F3 ("candidate to drop"): the input is the issue's own named variant, and the formatter's output is reported today.

### Issue goals
- Change the "Done when" item, which asks for invalid inputs, to the two valid inputs above.
- The issue's target ("not reported" whenever the first token does not start its line) holds for formatter output under reading (i), not for misaligned directives. This does not change visible output beyond SHARED-DIAG-010.

### Architecture findings
RH7304 is one of the private copies in the #1147 finding.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1655

### Premise
The issue is real. The enum member `A // c⏎= 1,` is reported with `no-fix-offered`. `scripts/format.sh` keeps the line break (it re-indents `= 1` to the member column), and RH5111 still reports the formatted output. The same holds for:
- the property `Q { get; set; } // c⏎= 5;`;
- the parameter default `int a // c⏎= 1`;
- `y = /* c */⏎2;`, `y =⏎/* c */ 2;`, and `y =⏎// c⏎2;`;
- `#pragma` in front of `=`, a `#region` behind `=`, and a `#pragma` in an enum member.

In each case RH5111 reports formatted code, against SHARED-DIAG-010.

The mixed gaps behave as the new RH5111-005 says. `y // c⏎=⏎2` and `y /* c */ =⏎3` are reported on `main`. The formatter joins only the gap without a comment, and the result has every wrapped gap holding a comment, which the new text does not report.

The declarator `int x // c⏎= 1;` and the field `int _x // c⏎= 1;` are refused as unsupported ("a comment between a variable declaration's own tokens"). So are `int x = /* c */⏎1;`, `int x =⏎/* c */ 1;`, and `int x = // c⏎1;`. A declarator with only `#pragma` in front of `=` is formatted (gap kept).

### Mechanism and candidates
Mechanism: RH5111 reports any `=` or value off its line, without asking whether the formatter keeps that gap because of a comment or directive.

Candidates:
- constructs: assignment, enum member, property initializer, parameter default, variable declarator;
- what the gap holds: `//`, `/* */` before the line break, `/* */` after the line break, own-line `//`, `#pragma`, `#region`/`#endregion`, conditional directives (already unreported);
- which gap: target–`=`, `=`–value, both, mixed with a bare wrap.

### Breaking inputs
FC-013, FC-019, FC-020, FC-021, FC-005, FC-003. Every cell above, plus the declarator column (reported, no fix) for comments and the declarator with only a directive (not reported).

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| RH5111 analyzer | must change |
| RH5111 code fix | must stay (withholding is unchanged) |
| Formatter | not involved |
| SHARED-DIAG-010 | must stay (option A) |

### Refuse option
Withholding the fix is already today's state, and it is no remedy: the defect is the report. Declaring the shape unsupported, for a comment or directive around the `=` of an assignment, enum member, property, or parameter, is possible. It would refuse files that today format fine, and it deletes no formatter special case I can name: the line-break phase's general FMT-BREAK-001 rule handles the gap. Not recommended.

### Output questions
- Spec author's question 2: I agree with A. SHARED-DIAG-010, an unchanged requirement, demands it, and so does the maintainer's triage of #1655, which keeps the enum example as the case to fix. The lead may treat it as decided. It is listed as open only because RH5111-005 on `main` showed two of these inputs as reported.
- **3 · RH5111 · Assignments must have proper line breaks: a declarator whose wrapped `=` gap holds only a directive, while a comment stands elsewhere in its declaration.** This is a narrow case.

  Input:
  ```csharp
  class C
  {
      void M()
      {
          int /* t */ x
  #pragma warning disable CS0168
              = 1;
      }
  }
  ```
  Before (`main`): reported, no fix offered. The formatter refuses the file as unsupported (the comment `/* t */`).

  After, A (recommended): not reported, like a directive-only gap in any other declarator.

  After, B: reported, no fix offered, as the new RH5111-005 text reads ("where the formatter refuses the declaration").

  Why A: RH5111 decides from its own `=` gaps. B needs the analyzer to repeat the formatter's whole-declaration comment check, which `Reihitsu.Analyzer` cannot call, for a shape nobody writes. If A is taken, word the exception as "a variable declarator with a comment in the judged span". Every comment there lies inside the span FMT-UNSUP-004 refuses: the leading trivia of the value's first token is not excepted (checked).

### Internal choices
_None_ beyond question 3.

### Issue goals
_N/A._ The goals stand.

### Architecture findings
Under option B of question 3, the RH5111 analyzer would carry a copy of FMT-UNSUP-004's check (FC-015, FC-042). Option A avoids it.

### Follow-up candidates
The formatter writes a blank line between `=` and an own-line comment: `y =⏎// c⏎2;` becomes `y =⏎⏎// c⏎2;`. This may be the FMT-BLANK-011 behavior that #1147 says is tracked separately. If so, it is not a new follow-up.

## thoenissen/Reihitsu#1736

### Premise
The issue is real. RH5109 reports and splits these shapes, and the formatter joins them (`void N(int a, int b)`, `record R(int A, int B);`, `(int a, int b) =>`, `delegate(int a, int b)`):
- `void N(⏎int a, int b)`;
- `void N(int a, int b⏎)`;
- the lambda `(⏎int a, int b) =>`;
- the anonymous method `delegate(⏎int a, int b)`;
- the record primary constructor `record R(⏎int A, int B);`.

The issue's claim "the fix result is not a fixed point of the formatter" is wrong. `scripts/format.sh` keeps the fix result `void N(int a,⏎int b)` unchanged, because its parameters start on different lines. The real disagreement is two different stable results from one input. The issue is real for that reason, not for the stated one.

Also checked:
- `void N(int a, int b // c⏎)` is reported with `no-fix-offered`, and the formatter keeps the `)` on its line. So `main` reports formatter output (SHARED-DIAG-010), and the change removes that.
- RH5108 reports and fixes `void N(⏎int a, int b)` to the formatter's result.
- RH5106 reports and fixes `void N(int a, int b⏎)`.
- Neither RH5108 nor RH5106 reports the lambda or the anonymous method.

### Mechanism and candidates
Mechanism: RH5109 decides "multi-line" by the parentheses (`openParenLine != closeParenLine`), not by the parameters, unlike FMT-BREAK-012.

Candidates (ParameterListSyntax owners): method, constructor, local function, operator, delegate, record and primary constructor, parenthesized lambda, anonymous method. Shapes:
- a break behind `(`;
- a break in front of `)`;
- a `)` held by a comment;
- a first parameter held by a comment or by `#pragma` (entry 3);
- a parameter that spans lines (still reported);
- a break between parameters (still reported).

### Breaking inputs
FC-013, FC-003, FC-021, FC-020.
- Entry 3 on `main`: `void N( // c⏎int a, int b)` and `void N(⏎#pragma …⏎int a, int b)` are reported with `no-fix-offered`. The formatter writes one parameter per line for both (FMT-BREAK-012, third bullet).
- The `)`-held-by-comment shape (`no-fix-offered` today, not reported after the change) is not in the change doc's visible output changes. Add it to entry 2 or as its own entry.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| RH5109 analyzer | must change |
| RH5109 code fix | must stay |
| Rule page RH5109 | must change (change doc entry 5) |
| RH5108, RH5106 | must stay; they report entries 1 and 2 for declarations (checked) |
| Formatter | not involved |

### Refuse option
Not applicable. The maintainer has decided the main shape.

### Output questions
- **Change doc entry 3 · RH5109 · Parameters must be on same line or separate lines: a comment or directive keeps the first parameter off the `(` line.** The change doc reads the maintainer's answer literally and reports nothing.

  Input:
  ```csharp
  class C
  {
      void N( // c
          int a, int b)
      {
      }
  }
  ```
  Before (`main`): reported on `(`, no fix offered. The formatter gives
  ```csharp
      void N( // c
             int a,
             int b)
  ```
  After, A: not reported (change doc as written).

  After, B (recommended): reported on `(`, no fix offered, as on `main`. The entry then leaves the change doc.

  Why B: FMT-BREAK-012 reads this list as multi-line. B makes RH5109's reading identical to the formatter's, which is the claim RH5109-001 makes ("as FMT-BREAK-012 decides it") and the change doc's Decisions makes ("exactly when FMT-BREAK-012 does"). Under A both claims are false. A also creates a P11 "rule's own gap" for the `#pragma` variant, which no comment rule reports. The check B needs is cheap, and RH5109-004 already looks at that gap for the fix.

  The maintainer's answer had no comment in it. If the lead reads that answer as covering this input, A stands, and the two sentences must be corrected.

### Internal choices
_None._

### Issue goals
- "A test that the fix result is stable under the formatter": already true on `main` for the split result. Keep it as a cheap regression on RH5109-003's output, as the spec author proposes. No visible change.
- The issue's statement "the fix result is not a fixed point" should not be repeated in the PR.

### Architecture findings
_None._

### Follow-up candidates
- **F4 · parameter list of a parenthesized lambda or anonymous method with a line break behind `(`.** After this change no rule reports `(⏎int a, int b) =>` or `delegate(⏎int a, int b)`, and the formatter joins them. RH5108 covers declarations, not lambdas. Under P11 this is a new-rule candidate, or RH5108's gap if its spec meant to cover them.

## Whole run

- Lane: behavior change, as claimed. Only #1736 carries a change doc; the others are bug fixes against unchanged requirements, plus clarifications.
- Output questions for the lead's batch:
  - question 1 (RH7304 fix), with the multi-line comment input added;
  - question 2 (RH5111), which may be treated as decided by SHARED-DIAG-010 and the triage;
  - question 3 (RH5111 declarator with a directive-only gap and a comment elsewhere);
  - #1736 entry 3, where I recommend B.
- Architecture finding: SHARED-COMMENT-011 is implemented as about a dozen private per-rule decisions (#1147, #1594). A narrow fix in each failing rule adds copies. One shared helper for "column of the line's first element", covering one or more comments and a multi-line comment's start, would replace them.
- Follow-up candidates:
  - F1 (RH5102, argument lists): agree.
  - F2 (RH3002-009): agree.
  - F3: disagree; it belongs in #1594 (above).
  - F4 (lambda/anonymous method, above).
  - The possible blank-line follow-up under #1655.
- Spec wording to correct, whatever the answers:
  - RH5109-001 and the change doc's Decisions, if entry 3 stays A;
  - SHARED-COMMENT-001's list of where a `///` documents nothing (#1386).

Role: skeptic
Commit: 461faa4
Verdict: #1075: proceed narrower; #1261: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. `00-lead.md` repeats the triage comment on thoenissen/Reihitsu#1261 ("the fix follows SHARED-FIX-007; SHARED-BRACE-002 is aligned ... in the same PR"). That comment is part of an `agent-filed` issue, so I challenge it below like any other goal.

I established every fact below with `scripts/apply-fix.sh` (LF and CRLF, which agreed everywhere) or `scripts/format.sh` on throw-away fixtures in the scratchpad, at `461faa4`. Where I only read code, the entry says so.

## thoenissen/Reihitsu#1075

### Premise

Real for RH5025, RH5027, RH5011, and RH5030. Each one is reached by the analyzer on any document, and the fix through the IDE's code action or Fix All. The RH5023 hole case is a separate matter (see *Issue goals*).

- **RH5025.** Reported, and the fix deletes the empty line inside the literal. Cases: `$"""⏎a⏎⏎{x}⏎"""`, `$$"""⏎a⏎⏎{{x}}⏎"""`, and `@$"a⏎⏎{x}"`. A regular `$"a⏎⏎{x}"` gives `parse-error`, because literal text in a regular interpolated string cannot hold a line break. So "regular" in RH5025-006 has no member. A lambda `{` inside a hole with a blank hole line above it (`a{((Func<int>)(() =>⏎⏎{ return x; }))()}`) is reported and fixed today. That is correct under SHARED-DIAG-003 and RH5025-006's last sentence.
- **RH5027.** Reported, and the line above is deleted, for each of these cases:
  - `a++;⏎⏎do a++; while (a < 3);`
  - `do s = $"""⏎x{a}⏎⏎"""; while (…);`
  - `do⏎    a++;⏎⏎/* c */ while (a < 3);`
  - `do⏎{⏎    a++;⏎⏎} /* c */ while (a < 3);`

  The last two are not in the issue or in the spec author's handover. RH5027-001, which is unchanged ("nothing but whitespace, or whitespace and the closing brace ... in front of the `while` keyword"), already says they are not reported, so they are failing members.
- **RH5011.** Not converged after 4 iterations in all six comment-spanning shapes (see *Breaking inputs*). Each application adds one more empty line inside the comment. The control `break; /* a */⏎a++;` is fixed in one application.
- **RH5030.** Shapes where a comment holds every line break are fixed in 2 iterations, and the first one leaves `b */ ` with trailing whitespace and the statement at column 0. With `/** a⏎b */ a++;` the first application also moves the doc comment to column 0 on a line of its own.
- **Analyzer control.** `break; /* a⏎b */⏎⏎a++;` and `} /* a⏎b */⏎⏎a++;` are not reported. So once a fix puts the blank line below the comment's last line, the rule converges.

### Mechanism and candidates

- **RH5025** (analyzer). `OnSyntaxTree` collects every `OpenBraceToken` that starts its text line (`FormattingTextAnalysisUtilities.GetLineIndicesBeginningWithToken`). That set includes `InterpolationSyntax.OpenBraceToken`, and the line above that token is literal text.
  - Dispatch: token kind `OpenBraceToken`.
  - Failing candidates: an interpolation `{` in a verbatim interpolated string (`$@"`, `@$"`), in a raw one (`$"""`), and in a multi-dollar raw one (`$$"""`, token text `{{`).
  - Not failing: every other `OpenBraceToken`, a `{` inside hole code included.
- **RH5027** (analyzer). `OnDoStatement` reports the text line above the line of `WhileKeyword` without checking what stands in front of `while` on that line. Each value of "what stands in front of `while`" is a candidate:
  - nothing: reported, correct;
  - `}`: reported, correct;
  - code: wrongly reported;
  - the end of a literal of any kind: wrongly reported;
  - the end of a multi-line comment: wrongly reported;
  - a complete `/* */` comment: wrongly reported;
  - `}` plus a comment: wrongly reported.

  This is FC-002: the requirement's qualifier never became a condition.
- **RH5011** (code fix). `ApplyCodeFixAsync` inserts its line break at `breakLine.EndIncludingLineBreak`. When a comment that starts behind the semicolon ends on a later line, that position lies inside the comment's text. `CanRegisterCodeFix` guards only the same-line case. Only RH5011 inserts at a token's text-line end: I searched the code fixes for `EndIncludingLineBreak` and every other hit deletes lines.
- **RH5030** (code fix, `BlankLineCodeFixUtilities.InsertBlankLineBefore`, SHARED-AFTERBRACE-001). The fix inserts one line break at the start of the next statement's leading trivia. That only separates the statements when a line break outside any comment already ends the line before the statement. When every line break lies inside a comment, the inserted break lands on the comment's last line. The helper has one other consumer, RH3208, which fails the same way (see *Follow-up candidates*).
- **RH5023 hole** (code read). `GetNonFormattableLineIndices` → `GetStringLineIndices` exempts the whole span of an `InterpolatedStringExpressionSyntax`, holes included. The consumers of `GetStringLineIndices` are RH5204, RH5602, and, through `GetNonFormattableLineIndices`, RH5023 and RH6004.

### Breaking inputs

Failure classes: FC-002 (RH5027), FC-021 and FC-004 (a `/* */` that ends mid-line, a comment in the gap the fix edits), FC-007 (RH5011 and RH5030 need several passes), FC-016 (the RH3208 sibling of the shared helper), FC-022 (raw, verbatim, and interpolated raw literals), FC-033 (cover CRLF too).

**RH5025-006.** Not reported, LF and CRLF:
- `$"""⏎    a⏎⏎    {x}⏎    """`
- `$$"""⏎    a⏎⏎    {{x}}⏎    """`
- `@$"a⏎⏎{x}"`

Control that stays reported: a lambda `{` inside a hole with a blank line above it in the hole code.

**RH5027-001.** Not reported:
- `a++;⏎⏎do a++; while (…);`
- `do s = @"x⏎⏎".Trim(); while (…);`
- `do s = $"""⏎x{a}⏎⏎"""; while (…);`
- `do a++; /* x⏎⏎ */ while (…);`
- `do⏎    a++;⏎⏎/* c */ while (…);`
- `do⏎{⏎    a++;⏎⏎} /* c */ while (…);` (RH5024 reports this blank line)

Controls that stay reported: `do⏎    a++;⏎⏎while (…);` and `} while`.

**RH5011 matrix.** The comment starts behind `break;` and ends on a later line. Two axes: where the next code starts, and what that next code is. Each member also has comment variants. Today all six members fail.

| # | Next code | Shape | Today |
|---|---|---|---|
| a1 | statement on its own line below the comment | `break; /* a⏎b */⏎a++;` | not converged |
| a2 | statement on the comment's last line | `break; /* a⏎b */ a++;` | not converged |
| a3 | switch label on its own line | `break; /* a⏎b */⏎default:` | not converged |
| a4 | switch label on the comment's last line | `break; /* a⏎b */ default:` | not converged |
| a5 | two comments, the second spans lines, then `// c` | `break; /* x */ /* a⏎b */ // c⏎a++;` | not converged |
| a6 | `/** */` spanning lines | `break; /** a⏎b */⏎a++;` | not converged |
| a7 (control) | single-line `/* a */` | `break; /* a */⏎a++;` | fixed, 1 application |

RH5011-007 as written speaks of "the next statement". a3 and a4 have a switch label next instead. The wording should cover any following code, and the tests should include a3 and a4.

**RH5030 matrix.** A comment follows the closing brace.

| # | Shape | Today | RH5030-003 |
|---|---|---|---|
| b1 | `} /* a⏎b */⏎a++;` | fixed, 1 application | fix offered |
| b4 | `} /* x */ /* a⏎b */ // c⏎a++;` | fixed, 1 application | fix offered |
| b2 | `} /* a⏎b */ a++;` | 2 iterations, column 0 | withheld |
| b3 | b2 inside a switch section | 2 iterations | withheld |
| b6 | `} /** a⏎b */ a++;` | 2 iterations, comment moved to column 0 | withheld |
| b7 | `} /* a⏎b */ /* c⏎d */ a++;` | 2 iterations | withheld |

RH5030-003's condition ("every line break ... lies inside a comment") splits this matrix correctly.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5025 analyzer | must change |
| RH5025 code fix | must stay |
| RH5027 analyzer | must change |
| RH5027 code fix | must stay |
| RH5011 code fix (registration and insertion) | must change |
| RH5011 analyzer | must stay |
| RH5030 code fix | must change |
| `BlankLineCodeFixUtilities` | must stay for RH3208's output, which is visible and outside the run (see *Follow-up candidates*) |
| RH5023 analyzer | not involved if the hole instance is dropped; otherwise must change, scoped to RH5023 |
| `FormattingTextAnalysisUtilities.GetStringLineIndices` / `GetNonFormattableLineIndices` (`Reihitsu.Core`) | must stay: RH5204 is blocked on thoenissen/Reihitsu#951, and RH5602 and RH6004 are outside the issue |
| Fix All (RH5011, RH5030) | not involved: withheld diagnostics drop out of the batch |
| Formatter | not involved |
| Rule pages | not involved: `general-notes.md` already covers literal content and withheld fixes |

### Simpler option

For RH5011 a1, a3, a5, and a6, a fix that puts the blank line below the comment's last line is safe. RH5030 already does exactly this for b1 and b4 through SHARED-AFTERBRACE-001. See *Internal choices*.

### Output questions

The spec author's question 1 (RH5023 hole) is a real output question, because two specs give different output for the same input: RH5023-002 reads "including ... interpolated strings", and SHARED-DIAG-003's last bullet says a hole is code. I do not pick between them. It needs asking only if the instance stays in the run. I recommend dropping it from the run (see *Issue goals*). Then it travels as a follow-up issue carrying the same question in output form, and nothing visible changes in this run.

### Internal choices

- **RH5011, comment-spanning shapes.** The maintainer's decision allows either "insert after the comment's end" or "withhold", so whichever the lead picks is covered. Today the fix is offered for these inputs, so withholding is just as much a change as inserting. Neither one needs a change doc on that ground: both carry out the decision recorded in the issue, which is a bug fix against SHARED-FIX-001 and SHARED-FIX-003.
  - **Recommendation.** For a1, a3, a5, and a6, insert the blank line below the comment's last line. For a2 and a4, withhold.
  - **Why.** RH5030 b1 and b4 show that the own-line shape is fixed safely by a sibling rule. `AGENTS.md` (*Code-fix coverage*) calls a guard that refuses safe inputs worth narrowing. Withholding a2 and a4 matches RH5011-006 and RH5030-003.
  - **Consequence.** RH5011-007 would need rewording: withheld only when the next code starts on the comment's last line, and otherwise the blank line goes below the comment's last line. The tests double accordingly.
  - **If the lead keeps "withhold all".** RH5011-007 as written is correct, apart from the switch-label wording.
- **RH5025-006 "regular".** Recommend dropping "regular" from the list, or stating that it cannot occur. It has no member: the input gives a parse error.
- **RH5027.** Recommend adding the `/* c */ while` and `} /* c */ while` shapes to the new negative example. RH5027-001 decides both, and they are part of the failing class.

### Issue goals

- **"No blank-line fix changes literal or comment text."** Keep, scoped to the class members above. Other text-line rules that work on brace tokens (RH5022, RH5024, RH5026) cannot see a literal line next to their token. I settled that by reading the code, not by running it, and claim nothing beyond it.
- **"Each input converges in one application."** Keep. Withholding or the below-comment insertion both meet it.
- **RH5023 hole instance (the comment of 2026-10-19, not marked as a maintainer decision).** Recommend dropping it from this run as a follow-up. This keeps today's output, so no visible change is in question. Reasons:
  - It is the opposite of the issue's decision: code inside a literal goes unchecked, rather than content being edited.
  - Its cause is a `Reihitsu.Core` helper with four consumers, one of which must not change (thoenissen/Reihitsu#951). A RH5023-only exemption would add a second statement of where a literal's text ends (FC-015).
  - The case is rare: two or more blank lines inside a hole's expression.

### Architecture findings

_None._ The RH5030/RH3208 shared helper is one owner with two consumers. That is a sibling, not a second owner.

### Follow-up candidates

**RH3208 · Expression-style local functions should not be used — multi-line comment between the function and the next statement.**

Input:
```csharp
void M()
{
    int L() => 1; /* a
    b */ L();
}
```

Before (`main`):
```csharp
void M()
{
    int L()
    {
        return 1;
    } /* a
    b */ 
L();
}
```

The comment line ends with trailing whitespace, and `L();` sits at column 0. RH3208-002 and SHARED-AFTERBRACE-001 do not decide this input. The sibling `… /* a⏎b */⏎L();` is fixed correctly. Because the shared helper is involved, the RH5030 change must leave RH3208's output as it is.

**RH5023 hole question.** Only if the lead drops it from this run, as the spec author's question 1.

## thoenissen/Reihitsu#1261

### Premise

- **Region text (#1169 Q4, #1170 Q2).** Does not reproduce on `main`, in the issue's exact shapes:
  - RH5412 `using System;⏎⏎[Obsolete]⏎#region legacy⏎#endregion⏎class A⏎{⏎}`, also inside a block namespace;
  - RH5417 `#region getters` around `get { return _x; }` in an accessor list;
  - RH5417 `#region store` inside a `set` body where only `get` is reported.

  In every case the region text stays as written. Since PR thoenissen/Reihitsu#1578 (commit `21c2ff4`, FMT-PIPE-028), the node-level entry points skip the region phase (`formatsRegionDirectives: false`). Formatter tests in `ReihitsuFormatterNodeRegionTests` already cover this. RH5413–RH5416 share `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` and the same entry point. I ran RH5412 only.

  What the issue's shapes still show is blank lines the fix inserts: around `#region`/`#endregion` between `[Obsolete]` and `class`; around the region lines in the accessor list; inside the unreported `set` body; and after a `#region` above a property. These are P9/P14a instances of thoenissen/Reihitsu#1140, not of this issue (*Follow-up candidates*).
- **Comment duplicated by the statement-brace fixes (#1209 Q1).** Real. Reached by the RH5405, RH5406, and RH5407 code fixes, and by Fix All. `scripts/format.sh` on the same inputs writes the comment once, so the CLI is not affected.

### Mechanism and candidates

`StatementBracesCodeFixProviderBase` passes the owning `if` statement to `ReihitsuFormatter.FormatNodeInDocumentAsync`. Inside the pipeline, `ControlFlowBraceTransform.WrapWithBraces` keeps the body's trailing trivia on the body's last token, now inside the new block, and gives the new `}` none. After the pipeline, the entry point writes the original last token's trailing trivia onto the formatted target's last token (`ReihitsuFormatter.cs`, the `formattedLastToken.WithTrailingTrivia(originalLastToken.TrailingTrivia)` lines). That token is now the inserted `}`.

**Mechanism.** The comment is duplicated whenever the formatted target's last token is a token the pipeline inserted, while the original last token stays inside the target with its trailing trivia.

Candidates. Every node-level fix whose transform changes the target's last token:

| Fix | What the transform does | Duplicates |
|---|---|---|
| RH5405, RH5406, RH5407 (`ControlFlowBraceTransform`) | inserts `}` behind the original last token | yes |
| RH3202, RH3203, RH3205, RH3207, RH3208 (expression-body transforms) | removes `;`, adds `}` | no |
| RH5412 (empty type) | removes `}`, adds `;` | no |
| RH7004 (using block) | — | no |
| RH7101 (field split) | — | no |
| RH5403 | — | no |
| RH5417 | — | no |
| RH5204 (formats the enclosing block, whose last token stays) | — | no |

All of these were run with `// trail` behind the target. `FormatNodeInDocumentWithContextAsync` has the same write-back, but none of its callers (the RH5111, RH7101, and RH3208 fixes, and the chain helper) passes a target whose body this transform braces.

### Breaking inputs

Failure classes: FC-028 (node-level and document formatting disagree), FC-021 (an end-of-line comment, a `/* */` that ends mid-line), FC-026 (`else if` and nested chains for brace edits), FC-016 (the three rules share a base), FC-008 (Fix All with both branches reported), FC-034 (use the issue's shape, not a cousin).

**Matrix.** Two axes: which body is last in the owning `if`, and the comment kind.

| Body last in the owning `if` | Comment | Rule | Today |
|---|---|---|---|
| `if` body without `else`, body below | `// trail` | RH5405 | duplicated |
| `if` body without `else`, body on the `if` line | `// trail` | RH5405 | duplicated |
| `if` body without `else` | `// one` plus a `// two` line below | RH5405 | `// one` duplicated; `// two` (the next statement's leading trivia) stays in place |
| `if` body without `else` | multi-line `/* a⏎b */` | RH5405 | duplicated, both lines |
| multi-line body | `// trail` | RH5406 | duplicated |
| `else` body, `if` body braced | `// trail` | RH5407 | duplicated |
| `if (b)` body nested as the body of `if (a)` | `// trail` | RH5405 | duplicated |
| `else if (b)` body | `// trail` | RH5405 | duplicated; also braced in the `if` column (thoenissen/Reihitsu#1144) |

Controls, each written once today:
- an `if` body followed by `else` (`M(a, b); /* trail */⏎else`);
- a comment line above the body;
- a comment behind the condition.

**Test caution.** Today's fix also inserts a blank line above the `if` when a statement precedes it, braces and reformats the other branch, reformats the body, and puts an `else if` body's braces in the inner `if` column. The maintainer decided all four are wrong (#1209 Q2–Q4 → P9, P14a, P10; routed to #1140 and #1144). A regression test whose expected output shows any of them locks a decided-wrong output. Inputs that avoid this:
- the `if` is the first statement of its block;
- the other branch is already braced;
- the body's spacing is already canonical.

The `else if` member cannot avoid the column. Leave it to the shared mechanism and name it in #1144, or take #1209 Q4 into the run (see *Simpler option*).

**Region characterization.** The issue's shapes pass today, but their output contains the inserted blank lines (P14a says those are wrong). The RH5412 shape `#region legacy⏎class A⏎{⏎}⏎#endregion` (region around a top-level class) produces no blank lines: `class A;` between the unchanged directives. For RH5417 I found no shape free of inserted blank lines.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5405, RH5406, RH5407 code fix result | must change: the comment is written once |
| `ReihitsuFormatter` node-level write-back / `ControlFlowBraceTransform` | the location of the mechanism (see *Architecture findings*); document-level output must stay |
| FMT-PIPE-010 ("Both MUST keep the target's trailing trivia") | clarification needed if the fix lands in the entry point: the trivia is kept once |
| RH5412–RH5416, RH5417 code fixes | must stay (region text already kept); characterization tests only |
| Analyzers RH5405–RH5407, RH5412–RH5417 | not involved |
| Rule pages | not involved |
| CLI / document formatting | must stay (verified correct) |

### Simpler option

Fix only the duplication. Keep SHARED-BRACE-002 as it is, or add only a "comment behind the body appears once" sentence and example as a clarification. Two reasons:

- The current SHARED-BRACE-002 already demands the braced layout "as the formatter does (FMT-STRUCT-001)", and document formatting writes the comment once. So the duplication is a bug against an unchanged requirement and needs no spec change.
- The spec author's full rewrite of SHARED-BRACE-002 also states #1209 Q2 and Q3 (thoenissen/Reihitsu#1140, milestone 1.2.1) and #1209 Q4 (thoenissen/Reihitsu#1144, milestone 1.2.1). Once the spec says that, the run must implement it. That means changing the fix from "format the whole `if`" to "brace the body only", and changing at least the RH5405 blank-line tests and the `ElseIfWithTrailingComment` pair. This is more than double the production paths the handovers name, in issues the maintainer routed elsewhere.

**Recommendation.** Take the narrow option. Revert SHARED-BRACE-002 to its `main` text plus, at most, the comment-once clarification. Record the SHARED-BRACE-002 alignment as part of #1140/#1144.

### Output questions

_None._ The spec author's questions 2–6 are already answered by the maintainer: the #1209 decision comment of 2026-10-20 records Q1 as P1, Q2 as P9, Q3 as P14a ("the tests that pin it change"), and Q4 as P10. Unchanged SHARED-FIX-006 and SHARED-FIX-007 also demand them; the moved-code bullet names the brace fix's embedded statement. They must not be asked.

**If the lead takes the full alignment instead.** One input is then undecided by the spec author's text: `if (a) M(a, b); else M(b, a);` on one line, fix applied to the `if` body.
- "The braces each start their own line" does not say whether `else` stays behind the new `}` (`} else M(b, a);`) or starts the next line.
- "Everything else stays as written" leaves the same gap.

The two readings give different output, so it would be an output question.

### Internal choices

- **Where the duplicate is prevented.** In the node-level write-back or in the transform's handling of the body's trailing trivia: one owner either way (see *Architecture findings*). Not in the code fix base, where removing the comment from the result's `}` would predict what the entry point writes back.
- **SHARED-BRACE-002 wording.** If the lead keeps the spec author's full text instead, it needs one more sentence that decides the `} else` case above.

### Issue goals

- **"Each input keeps its comment and directive text."** Keep. Region text is already met on `main`; the comment is this run's fix.
- **"The fix is still offered where it is safe."** Keep. Nothing here withholds a fix.
- **"Each input has a regression test."** Keep. The region inputs get characterization tests, subject to the blank-line caution above.
- **Triage: "SHARED-BRACE-002 is aligned to SHARED-FIX-007 in the same PR."** Recommend dropping it from this run and moving it to #1140/#1144 (*Simpler option*). Dropping keeps today's output apart from the duplication, so nothing visible is in question. The PR states the difference.

### Architecture findings

Two components own one decision: where the target's original trailing trivia ends up in a node-level fix result.

- **Owners:**
  - `ControlFlowBraceTransform.WrapWithBraces` keeps the trivia on the body's last token, inside the new block;
  - `ReihitsuFormatter.FormatNodeInDocumentAsync`, and the same lines in `FormatNodeInDocumentWithContextAsync`, write it again onto the formatted target's last token (FMT-PIPE-010, "keep the target's trailing trivia").
- **Input that shows it:** RH5405 on `if (a)⏎    M(a, b); // trail` gives `} // trail` and `M(a, b); // trail`. Document formatting gives it once.
- **On the narrow fix:** a fix in `StatementBracesCodeFixProviderBase` that strips the comment from the result would add a third owner. That is a prediction of the write-back.

### Follow-up candidates

- **Extend thoenissen/Reihitsu#1140 (P9, P14a)** with new instances. In each, the region text is kept, but the fix inserts blank lines in code its diagnostic does not report:
  - RH5412 `[Obsolete]⏎#region legacy⏎#endregion⏎class A⏎{⏎}`: blank lines around `#region`/`#endregion` and above `class A;`;
  - RH5417 with a region around `get { return _x; }` in an accessor list: blank lines inside the region;
  - RH5417 with a region inside a `set` body while only `get` is reported;
  - RH5417 with `#region getters` above `int P { get { return _x; } }`: a blank line between `#region` and `P`.
- **Extend #1140/#1144:** SHARED-BRACE-002 still says "laying out the `if` statement ... as the formatter does", which contradicts SHARED-FIX-006 and SHARED-FIX-007. Align it there, together with #1209 Q2–Q4. This applies only if the lead takes the narrow option.

## Whole run

Each issue's verdict (proceed narrower) does not depend on the other.

Role: spec author
Commit: 73b92b4
Verdict: #1075: spec written, open question (RH5023 hole case); #1261: spec written, open question (consequences of the SHARED-BRACE-002 alignment)

No intended fix, suspected cause, or hoped-for verdict was carried by the prompt beyond the lead's summary of the triage comment, which the issue itself states.

No change doc. Every requirement this run adds or changes writes down an outcome that an unchanged requirement already decides, or records a withheld fix the existing fix requirements already leave out (`sdd/README.md`, *When a change doc is required*: a clarification needs none). RH5011-007 and RH5030-003 record a withheld fix as `AGENTS.md`, *Code-fix coverage*, asks. Offering a fix for those inputs instead (the maintainer's other permitted outcome) would be new fix behavior and would need a change doc — see *Internal choices*.

Facts established with `scripts/apply-fix.sh` (on `73b92b4`, LF and CRLF alike) and `scripts/format.sh`; fixtures were throw-away files in the scratchpad.

## thoenissen/Reihitsu#1075

### Changed requirements

- RH5011-007 — A comment that spans the end of the `break` line withholds the fix — added (clarification; `Uses` gains SHARED-FIX-003)
- RH5025-006 — The brace of an interpolation hole is not reported — added (clarification of RH5025-001's list and SHARED-DIAG-003; `Uses` gains SHARED-DIAG-003)
- RH5027-001 — A blank line above a `do` statement's footer is reported — clarified: a negative example for its "nothing but whitespace or `}` in front of `while`" condition (FC-002); normative text unchanged; `Uses` gains SHARED-DIAG-003
- RH5030-003 — A comment that holds every line break withholds the fix — added (clarification; `Uses` gains SHARED-FIX-001)
- RH5023-002 — not changed; see the output question.
- `sdd/specs/requirement-ids.txt` gains RH5011-007, RH5025-006, RH5030-003 at their sorted positions. SHARED-DIAG-003, SHARED-BLANK-001, SHARED-WS-001, SHARED-AFTERBRACE-001, SHARED-FIX-001, SHARED-FIX-003 needed no change.

Observed today (reproductions of each instance):

- RH5025: `$"""⏎a⏎⏎{x}⏎"""` and `$@"a⏎⏎{x}"` — reported, fix deletes the empty line inside the literal.
- RH5027: `a++;⏎⏎do a++; while (a < 3);`, `do s = @"x⏎⏎".Trim(); while (…);`, `do a++; /* x⏎⏎ */ while (…);` — each reported, fix deletes the line above (inside the literal / comment in the last two).
- RH5011: `break; /* a⏎b */⏎a++;` and `break; /* a⏎b */ a++;` — fix inserts an empty line inside the comment on each application; not converged after 10 iterations.
- RH5030: `} /* a⏎b */ a++;` — first application leaves `b */ ` with trailing whitespace and `a++;` at column 0 below a blank line (2 iterations).
- RH5023 hole case — not reported (matches the issue comment).

### Examples for the tests

RH5011-007, RH5025-006, RH5027-001 (the new negative example), RH5030-003. All three new IDs are in enforced specs and need covering tests in this PR.

### Output questions

**1 · RH5023 · Code must not contain multiple blank lines in a row — blank lines inside an interpolation hole's expression**

Input:
```csharp
class C
{
    void M(int x)
    {
        var s = $"""
            a{x


            + 1}b
            """;
    }
}
```

Before (`main`): not reported.

After, A (recommended): the second blank line inside the hole is reported, and the fix deletes it:
```csharp
class C
{
    void M(int x)
    {
        var s = $"""
            a{x

            + 1}b
            """;
    }
}
```

After, B: not reported (today).

Why A: SHARED-DIAG-003 states that an interpolation hole's expression is code and checked like any other code; RH5023-002 says lines of "interpolated strings" are not considered, which read literally also covers the hole, so the two requirements give different outputs. The issue comment counts this as an instance of #1075, but it is not marked as a maintainer decision the way the other instances are, so I did not write RH5023-002's clarification. On A, RH5023-002 becomes "lines of the text of a literal" with a hole example (clarification, no change doc). Scope note for the lead: the exemption comes from a shared helper the issue comment names (`FormattingTextAnalysisUtilities.GetStringLineIndices`) that RH5204, RH5602, RH6004, RH8301–RH8303 also use; RH5204's indentation inside holes is undecided (thoenissen/Reihitsu#951) and must not change, and the other rules' behavior inside holes is outside this issue.

### Internal choices

- RH5011 and RH5030 multi-line-comment inputs: the maintainer's decision allows either a correct one-application fix or withholding. I wrote **withholding** (RH5011-007, RH5030-003): the existing fix requirements (RH5011-004's "line ends outside a comment", SHARED-AFTERBRACE-001's "line break outside any comment") already exclude these inputs, so withholding is a clarification, and the visible outcome is one the maintainer named. Alternative: insert the blank line below the comment's last line (RH5011: `break; /* a⏎b */⏎⏎a++;`; RH5030: move `a++;` to its own line below a blank line) — new fix behavior, needs a change doc and the behavior-change lane.
- RH5025: written as a new requirement RH5025-006 rather than widening RH5025-002, because RH5025-002 is about the line above the brace and non-brace `{`, while an interpolation `{` is an opening-brace token; one decision per heading.
- RH5027: no new requirement; the existing condition decides all three inputs, so only the missing negative example was added.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1261

### Changed requirements

- SHARED-BRACE-002 — The fix wraps the body in braces — clarified, aligned to SHARED-FIX-007 as the triage comment on the issue asks: the braces the fix creates are laid out as document formatting lays them out (column of the first token on the line of the owning `if`/`else`); the body is moved code (SHARED-FIX-007, moved-code bullet, which already names "the embedded statement of an `if`, `else`, or loop that a brace fix encloses") and keeps its own layout, its comments travelling with it once; the condition, a comment behind it, another unbraced branch, and the blank lines around the statement stay as written (SHARED-FIX-006). `Uses` gains SHARED-FIX-003, SHARED-FIX-006, SHARED-FIX-007. The original example is kept; two examples added.
- SHARED-EMPTY-003, RH5417-003, SHARED-FIX-003 — no clarification needed. Comment and directive text is decided by SHARED-FIX-003; blank lines a fix inserts into the leading trivia of the node it changes are decided by SHARED-FIX-006's blank-lines bullet.

Observed today:

- RH5405 `if (a)⏎    M(a, b); // trail` — `// trail` written twice: behind `M(a, b);` and behind the new `}` (instance reproduced).
- RH5405 `var c = b;⏎if (a) M( a,b );` — fix inserts a blank line above the `if` and rewrites the body to `M(a, b);`.
- RH5405 `if (a)⏎    M( a,b );⏎else⏎    M(a,  b);` — one application braces and reformats both branches.
- RH5405 `else if (y) // note⏎    M(x, y);` — braces placed in the column of the `if` (5 columns right of `else`); `scripts/format.sh` on the same input places them in the column of `else`. The existing test `ElseIfWithTrailingCommentFixedCode` in `RH5405BracesMustNotBeOmittedAnalyzerTests` asserts today's column.
- RH5412 with `#region legacy⏎#endregion` directly above a nested `class A { }`, and with the region around a top-level `class A { }`: the region text is **not** rewritten in these shapes; in the nested shape the fix inserts a blank line between `#region legacy` and `#endregion` and another between `#endregion` and `class A;`. RH5417 with a region above `get { return _x; }` in a multi-line accessor list: the same two blank lines are inserted, region text unchanged. The issue's exact inputs (#1169 Q4, #1170 Q2) are not in the issue; the region-text premise did not reproduce in the shapes tried.

### Examples for the tests

SHARED-BRACE-002 (its three examples).

### Output questions

These follow from the alignment. Each "A" is what SHARED-FIX-006/SHARED-FIX-007 already demand and what the clarified SHARED-BRACE-002 now states; each "Before" is what today's fix writes and, for 3–6, what existing tests in `RH5405…`/`RH5406…`/`RH5407…AnalyzerTests` assert (e.g. `VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration`, `VerifyBlankLineIsInsertedAboveFixedStatementAfterClosingBrace`, the `ElseIfWithTrailingComment` pair). Since a test whose asserted output turns out to be wrong is in question, I list them; if the lead counts the triage comment plus SHARED-FIX-006/007 as deciding them, they need not be asked.

**2 · RH5405 · Braces must not be omitted — trailing comment behind the body** (the issue's instance)

Input:
```csharp
if (a)
    M(a, b); // trail
```
Before (`main`):
```csharp
if (a)
{
    M(a, b); // trail
} // trail
```
After, A (recommended):
```csharp
if (a)
{
    M(a, b); // trail
}
```
Why A: the comment is written once; duplicating it changes the author's text (SHARED-FIX-003).

**3 · RH5405 · Braces must not be omitted — layout of the body**

Input:
```csharp
if (a) M( a,b );
```
Before (`main`):
```csharp
if (a)
{
    M(a, b);
}
```
After, A (recommended):
```csharp
if (a)
{
    M( a,b );
}
```
Why A: the body is moved code and keeps its layout (SHARED-FIX-007 names the brace fix's embedded statement explicitly); spacing is other rules' diagnostics.

**4 · RH5405 · Braces must not be omitted — blank line above the statement**

Input:
```csharp
var c = b;
if (a)
    M(a, b);
```
Before (`main`):
```csharp
var c = b;

if (a)
{
    M(a, b);
}
```
After, A (recommended):
```csharp
var c = b;
if (a)
{
    M(a, b);
}
```
Why A: a fix does not insert a blank line before the node it changes (SHARED-FIX-006); the blank line is another rule's diagnostic.

**5 · RH5405 · Braces must not be omitted — the other branch**

Input (fix applied to the `if` body's diagnostic only):
```csharp
if (a)
    M(a, b);
else
    M(b, a);
```
Before (`main`):
```csharp
if (a)
{
    M(a, b);
}
else
{
    M(b, a);
}
```
After, A (recommended):
```csharp
if (a)
{
    M(a, b);
}
else
    M(b, a);
```
Why A: the `else` body has its own diagnostic and fix; Fix All still braces both (SHARED-FIX-006).

**6 · RH5405 · Braces must not be omitted — braces of an `else if` body behind a comment**

Input:
```csharp
else if (y) // note
    M(x, y);
```
Before (`main`):
```csharp
else if (y) // note
     {
         M(x, y);
     }
```
After, A (recommended):
```csharp
else if (y) // note
{
    M(x, y);
}
```
Why A: that is where formatting the document puts the braces, so formatting the fix's result changes nothing (SHARED-FIX-007).

### Internal choices

- SHARED-BRACE-002 is aligned by rewriting its normative text in place (clarified), not by retiring it and adding a new ID: the requirement's decision (the fix wraps the body in braces) is unchanged, and SHARED-FIX-007 already decides the layout. Alternative: retire SHARED-BRACE-002 and add SHARED-BRACE-003 — more churn in test tags, no difference in behavior.
- The braces' column is stated as "the first token on the line of the owning `if`/`else`" with formatting the document as the general rule; positions such as an `if` behind a label are left to that general rule rather than enumerated.
- No clarification of SHARED-EMPTY-003 or RH5417-003: the blank lines observed are decided by SHARED-FIX-006, and the region-text rewrite was not observed.

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd&FullyQualifiedName!~RequirementCoverage"`: 26 passed. `RequirementCoverageTests` will fail for RH5011-007, RH5025-006, RH5030-003 until their tests exist.
- Rule pages unchanged: a withheld fix and a narrowed report in a corner case do not change what is reported or how to fix it in the normal case.
- Non-goals (for the run, not written into specs): no change to which inputs RH5023 reports outside holes; no change to RH5204, RH5602, RH6004, RH8301–RH8303; no new fix for the multi-line-comment inputs of RH5011 and RH5030; no change to the bodies the statement-brace rules report (SHARED-BRACE-001).

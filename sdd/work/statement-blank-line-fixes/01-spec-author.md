Role: spec author
Commit: e02d0e7
Verdict: #946: spec written; #972: spec written; #1096: spec written; #1076: spec written; #1626: nothing to write; #1625: open question

No change doc: every behavior written here was decided by the maintainer as a bug (#1076, #1096) or is already required by a shared spec (#946, #972). The spec diffs are clarifications that let a spec decide each instance.

## thoenissen/Reihitsu#946

### Changed requirements

- SHARED-STMT-004 — The fix inserts one blank line above the statement and its comments — clarified: the blank line goes in front of the statement's first token, for `await using` / `await foreach` in front of `await`, wherever the rule puts its diagnostic.

### Examples for the tests

SHARED-STMT-004 (sentence on `await`). The issue's own inputs are the seeds: `var x = 1;⏎await using (resource) { x++; }` and the `await foreach` form, LF and CRLF; expected output has the blank line above `await`. Tag RH5005/RH5006 tests with SHARED-STMT-004. Mark the diagnostic where the analyzer puts it today (on `using` / `foreach`); see the output question below.

### Output questions

**1 · RH5005 · using-Statement should be preceded by a blank line (and RH5006 · foreach-Statement …) — where the diagnostic sits on `await using` / `await foreach`** — non-blocking: the fix's output does not depend on it.

RH5005-002 and RH5006-002 specify the location only "when the statement starts with the `using` / `foreach` keyword"; the `await` forms are left open, deliberately it seems, since their Why argues from "the keyword is the statement's first token".

Input:
```csharp
var x = 1;
await using (resource) { x++; }
```

Before (`main`): reported on `using`.

After, A (recommended): unchanged — reported on `using`:
```csharp
var x = 1;
await {|RH5005:using|} (resource) { x++; }
```

After, B: reported on `await`:
```csharp
var x = 1;
{|RH5005:await|} (resource) ...
```

Why A: the keyword that names the statement carries it, as RH7004 already does for an `await using` declaration ("the diagnostic does not include `await`"), and A changes nothing. With A, RH5005-002/RH5006-002 gain one sentence stating it; with B it is a behavior change outside this bug-fix run.

### Internal choices

- The await example was left out of SHARED-STMT-004 until question 1 is answered, because its markup would decide the location. Alternative: write today's location now.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#972

### Changed requirements

- SHARED-STMT-004 — clarified: a comment that starts the statement's line and stands in front of the statement (`/* block */ if (…)`) keeps its place, with exactly one blank line above that line, also for a block comment. Example added (second block of the new SHARED-STMT-004 example, RH5002).

The RH5002 non-goal the maintainer comment asks to delete ("A block comment in front of the `if` on the same line …") is **already absent** from `sdd/specs/analyzer/RH5002.md` on `e02d0e7`; nothing to delete. The lead should mention this in the PR so the comment is answered.

### Examples for the tests

SHARED-STMT-004 — `a++;⏎/* block */ if (a > 7) { }` → exactly one blank line (issue's expected output). Reproduce on RH5002 and at least one other rule on the base (for example RH5008 `return`, RH5029 local declaration).

### Output questions

_None._

### Internal choices

- The comment sentence covers only a comment that **starts** the statement's line. A multi-line comment that starts behind the previous statement and ends in front of the reported one (`a++; /* a⏎b */ if (…)`) is not decided by SHARED-STMT-004; it is the shape of #1625 and stays with that question. Alternative: write today's base output for it now — rejected, because question 2 decides the same shape for RH3208 and the two should be decided together.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1096

### Changed requirements

- SHARED-STMT-004 — clarified: "indented like the statement it follows" means the column in which the formatter puts the statements of the block or switch section, also when the statement it follows does not start its line — in particular behind a `case` label whose `when` clause continues on a second line: the moved statement takes the section's statement column (FMT-INDENT-003), not a column read from the label's continuation line. A label on the line of the switch statement's opening brace is explicitly not decided (see follow-up candidates). `FMT-INDENT-003` added to *Uses*.
- SHARED-BLANK-001 — A blank line holds nothing but whitespace — clarified: the line directly below a `///` comment is not inside the comment's text and counts as blank when it holds nothing but whitespace, empty or with spaces. Example added (RH5002 and RH5011 report nothing).

Rule pages: the paragraph "Inside an object initializer or anonymous object, the fix reads the inserted blank line's indentation … including that fix's documented limitation …" was removed from the 20 rule pages of the shared base (RH5001–RH5010, RH5012–RH5019, RH5021, RH5029). It documented, through RH5103's limitation, the indentation of the moved statement after a switch label as a limitation of these fixes — the sentence #1096 says to remove — and it described implementation detail, which rule pages must not. `RuleDocumentationConventionTests` pass.

`documentation/rules/RH5103.md` ("… or whose `case` label spans several lines, check the indentation of the moved statement") was **not** changed: it describes RH5103's own fix, which RH5103-003 explicitly leaves undecided for a multi-line label and which no issue of this run covers. See follow-up candidates.

RH5011: RH5011-001 already says a `break` is reported only when the trivia "holds no documentation comment"; so `break;⏎/// doc⏎⏎a++;` is already not reported by RH5011's spec — the RH5011 half of #1096 Q2 is a code-vs-spec bug, no spec change.

### Examples for the tests

- SHARED-STMT-004 — first block of the new example (RH5029, `case 1 when⏎    Check(value): Method(1); var y = 1;` → `var y` at the column of `break;`). The decided instance is inside an object initializer's lambda (the anchor-scope branch); the existing characterization test `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` pins the wrong column and must be turned into the regression test. Cover both the plain-method shape (spec example) and the initializer shape.
- SHARED-BLANK-001 — the new `///` example, once with an empty line and once with a line holding spaces (`break;⏎/// doc⏎    ⏎a++;` from the issue), for RH5011 and for a SHARED-STMT rule.

Note for the test implementer and cleaner: test and source comments that call the multi-line-label behavior a "documented limitation (see RH5103.md)" — in `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests` and `StatementShouldBePrecededByABlankLineCodeFixProviderBase.GetIndentation` — become stale for the shared base once fixed. `SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion` still serves RH5103, whose page keeps the sentence.

### Output questions

_None._

### Internal choices

- Removed the shared-base paragraph from all 20 pages rather than only rewording it. Alternative: keep it for the single-line-switch shape — rejected, a rule page lists no corner cases (`AGENTS.md`, *Key conventions*).
- Left the single-line `switch` inside an initializer outside SHARED-STMT-004 ("does not decide a switch section whose label stands on the line of the switch statement's opening brace") instead of extending the decided column rule to it. That shape is not #1096's instance, and today's output for it (`switch (value) { case 0: Method(1);⏎⏎var y = 1; break; }` at the line's own indentation, pinned by `VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine`) is outside the decision.

### Follow-up candidates

- **RH5103 · Code must not contain multiple statements on one line — statement behind a `case` label that spans two lines.** RH5103-003 does not cover a multi-line label; the RH5103 fix puts the moved statement one level too deep inside an initializer (characterized by `RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzerTests` around line 678), and the rule page tells the user to check it. #1096 decided the same input for the statement blank-line fixes: the section's statement column.

  Input (inside an object initializer's lambda):
  ```csharp
  case 1 when
      Check(value): Method(1); var y = 1;
  ```
  Before (`main`): `var y = 1;` moved to its own line, 4 columns deeper than `break;`.
  After, A (recommended): `var y = 1;` at the column of `break;`, and the page sentence about a multi-line label removed.
  After, B: unchanged, RH5103-003 keeps the shape undecided.
  Why A: same layout the formatter and, after this run, the statement blank-line fixes produce.
- **SHARED-STMT-004 and RH5103 — a switch written on one line inside an object initializer.** Today the moved statement takes the line's own indentation (see the characterization tests named above); the formatter would put it at the section's statement column. Rare (it violates RH5103 already), so withholding the fix there is also an option.

## thoenissen/Reihitsu#1076

### Changed requirements

- RH5022-004, RH5024-005, RH5025-004, RH5026-004, RH5027-004 — "The fix deletes the blank line" → "The fix deletes the run of blank lines" — changed to the decided reading A of #1066 Q1: the fix deletes the reported line together with the rest of its run (below the brace for RH5022, above the token for the others) and nothing else. The diagnostic span is unchanged. Run examples added to each; the intros and the "-003/-004 one diagnostic" Why sentences adjusted; `SHARED-FIX-001` added to *Uses* (the architecture test requires it).
- RH5023-001 — clarified with an end-of-file example: `"class C\n{\n}\n\n"` is not reported (the position after the last line break holds no line break; the requirement already says "a blank line that ends with a line break").
- RH5011-008 — A `break` before a keyword that continues its statement is not reported — **added**: not reported when the next token is the `else` of an `if` or the `while` of a `do`; a `while` that starts a `while` statement stays reported (both sides exemplified).
- RH5024-001 — clarified: "the line above that blank line" now reads "the nearest line above it that is not blank — the line above the run"; a run directly below an active `#region` is not reported (SHARED-BLANK-006 requires its first line; RH5023 reports the rest). Negative example added. `SHARED-BLANK-006` added to *Uses*. (The `#region` exemption was already in the requirement; the RH5024 half of the cycle is a code-vs-spec bug — the analyzer checks no line above.)
- RH5025-007 — A blank line below a region directive is not reported — **added**: not reported when the nearest non-blank line above the blank line holds an active `#region` or `#endregion` (SHARED-BLANK-006 requires the line in front of `{` in both cases; deleting it undoes RH5032, SHARED-FIX-004). The example is the decided instance (`{ "b", 2 },⏎#region A⏎⏎{ "c", 3 },`).

IDs registered in `sdd/specs/requirement-ids.txt`. The SDD architecture tests pass except `EveryRequirementOfAnEnforcedSpecIsCovered`, which waits for tests tagged RH5011-008 and RH5025-007.

### Examples for the tests

RH5022-004, RH5024-005, RH5025-004, RH5026-004, RH5027-004 (run examples; one application, `NumberOfFixAllIterations`/single-fix iterations = 1), RH5023-001 (end-of-file text, as a raw string without and with a third `\n`), RH5011-008, RH5024-001 (region example), RH5025-007.

### Output questions

_None._

### Internal choices

- The run-deleting fixes keep "as SHARED-WS-001 specifies for the span of that run" rather than changing SHARED-WS-001 itself. Alternative: widen SHARED-WS-001 to "the reported span or the run it belongs to" — rejected, it would touch every other user of SHARED-WS.
- RH5025-007 includes `#endregion` as well as `#region`: SHARED-BLANK-006 requires the blank line below both when `{` follows, so excluding `#endregion` would leave the same RH5032 cycle. Alternative: `#region` only, the literal decided instance.
- RH5011's rule page was not given an exception bullet for `else` / `do … while`: a narrowed report is not a page change in the normal case. The lead may add one; the page already has an Exceptions list.
- RH5025-001 still does not list initializer-element braces (that is #1081); RH5025-007's example uses them because it is the decided instance, and the requirement is stated for any `{`.

### Follow-up candidates

- **RH5026 / RH5027 — a region directive between the statement and `else` / the `do` footer** (`}⏎#region R⏎⏎else`): the same cycle with RH5032 is possible. Rare (it violates RH7303), so I did not specify it; the skeptic may check whether it reproduces.

## thoenissen/Reihitsu#1626

Nothing to write: one owner for the blank line in front of a statement is an internal refactor with no visible output of its own. Its only visible consequence is RH3208's output for #1625 (question 2). If ownership moves (for example `BlankLineCodeFixUtilities` gains the gap rebuild now in `StatementShouldBePrecededByABlankLineCodeFixProviderBase`), the *Code owners* rows of `shared/statement-blank-lines.md` and `shared/blank-line-after-brace-fixes.md` and `sdd/specs/README.md` must follow; that is a repair-round item for me once the implementer has decided the type.

### Follow-up candidates

- **RH5030 · Blank line after closing brace — narrow RH5030-003** once the shared owner can rebuild the gap. #1626 says "could later be narrowed"; it is a visible change and not this run's.

  Input:
  ```csharp
  if (a > 0)
  {
      a++;
  } /* a
  b */ a++;
  ```
  Before (`main`): reported, no fix offered (RH5030-003).
  After, A: `} /* a⏎b */⏎⏎a++;` with `a++;` at the block's statement column.
  After, B: unchanged.

## thoenissen/Reihitsu#1625

### Changed requirements

None yet. Once question 2 is answered, SHARED-AFTERBRACE-001 gains a paragraph for the case it leaves open ("When a line break outside any comment stands between …"): for A, "When every line break between the closing brace and the next statement lies inside a comment, the RH3208 fix MUST move the statement onto a line of its own below the comment's last line, with one blank line above it and indented like the statements of its block; RH5030 withholds its fix there (RH5030-003)"; for C, an RH3208 requirement that withholds the fix, mirroring RH5030-003.

### Examples for the tests

After the answer: the issue's input, LF and CRLF, one application.

### Output questions

**2 · RH3208 · Expression style local functions should not be used — next statement behind a multi-line comment** — **blocking for #1625** (the test's expected output depends on it).

Input:
```csharp
void M()
{
    int L() => 1; /* a
    b */ L();
}
```

Before (`main`): trailing space after `b */` and `L();` at column 0:
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

After, A (recommended):
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

After, B (the issue's suggested output, no blank line; RH5030 then reports `}`):
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

After, C: the diagnostic stays and no fix is offered.

Why A: RH3208-002 already requires the blank line after the new block, and A is what the statement blank-line fixes produce for the same shape (`a++; /* a⏎b */ if (…)`, per #1626) — the single owner #1626 asks for gives this output; B leaves a diagnostic the fix exists to avoid, and C gives up a case the shared code can fix.

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

- Spec checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` — 50 passed, 1 failed (`EveryRequirementOfAnEnforcedSpecIsCovered`: RH5011-008 and RH5025-007 have no tagged tests yet — expected until the test implementer adds them).
- Rule-page checks: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"` — 7 passed.

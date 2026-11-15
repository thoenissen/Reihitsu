Role: spec author
Commit: e3d9351
Verdict: #1140: spec written (three output questions open); #1687: no spec change; #1748: spec written

The prompt carried no intended fix or suspected cause. No change doc: every changed requirement drops or narrows rule text that states a wider scope than the unchanged SHARED-FIX-006 (maintainer principle P9) or adds what the maintainer's answer M1 decides. That is the clarification the bug-fix lane admits, as #1686 did for RH5204-006 and as the maintainer's triage on #1140 states for RH5111-004 and RH5401-003. No new requirement IDs. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` passed with 51 of 51 tests, which covers the `Uses` rows and the ledger. No rule page changes: for the user, what is reported and how to fix it stay the same in the normal case.

## thoenissen/Reihitsu#1140

### Changed requirements

- RH5110-003 — The fix puts each constraint clause on its own line — clarified: the fix lays out the constraint clauses only. The declaration's leading trivia, its code in front of the first clause, and its body (braces or semicolon, members or statements) stay as written. The refusal sentence now names "the formatting the fix asks of it" instead of "the declaration". Non-goal wording follows. New example: a member `int  x  =  1;` and an empty interface's braces stay.
- RH5111-004 — The fix lays out the construct — clarified: the condition "when the reported construct is not inside the value of an assignment" is removed. The fix MUST NOT format a construct that contains the reported one, such as an assigned lambda around a declarator. The construct's leading trivia stays, including a documentation comment above a property. New examples: the lambda declarator, and the documented property (#1120 Q3).
- RH5113-003 — The fix lays out the declaration — clarified: the fix moves only the semicolon. The leading trivia (documentation comment included) and the declaration's code in front of the semicolon stay. The intro sentence follows. New example: a documentation comment plus `int  _a` (#1121 Q1).
- RH5206-003 — The fix lays out the switch expression — clarified, partly: the leading trivia of the code the fix formats stays, and no blank line is inserted above it. New example: `status++;` followed by `return status switch …`. The sentence "format the innermost statement, initializer, or expression body" is unchanged and waits for output question 3.
- RH5401-003 — The fix puts the property on one line — clarified: the property's leading trivia stays. New example: a documentation comment.
- RH5403-003 — The fix lays out the statement — clarified: the fix lays out the reported block. The owning statement's header and the other branches of an `if` stay, as does the statement's leading trivia, and no blank line is inserted above the statement or above a comment that leads it (M1). New examples: an unbraced `if` branch next to the reported `else` block stays, and the #1748 comment input.
- RH5404-003 — The fix lays out the declaration — clarified: the declaration's header and leading trivia stay, and no blank line is inserted above the declaration or above a leading comment (M1). New example: a `// c` comment plus `sealed  class B { int _x; }`.
- RH5408-005 — The fix puts the declaration on one line — clarified: the property's leading trivia stays. New example: a one-line `<summary>` documentation comment.

Left unchanged, with the reason:

- RH5101-003, RH5102-003, RH5303-005, RH5304-004, RH5305-004, RH5306-004, RH5307-004, RH5308-004, RH5417-003, and the interior of the members (RH5404-003) and of the statements (RH5403-003): these do not format a node around the reported construct. Whether the fix may change code inside that construct (element spacing, statements inside a block) is output question 2.
- SHARED-CHAIN-009, RH5112-003, RH5201-005: output question 1.
- RH5206-003, innermost-statement sentence: output question 3.
- SHARED-BRACE-002 (RH5405–RH5407) already says "change nothing else (SHARED-FIX-006)". SHARED-EMPTY-003 (RH5412–RH5416) states only that the braces are replaced. RH3201-005 states only the removal. RH5417-003 was aligned in #1686. None of these restates a wider scope. The instances in #1140's comments of 2026-10-20 and 2026-11-07 (blank lines around regions, P14a) are measured against SHARED-FIX-006 and SHARED-FIX-007 directly.
- FMT-PIPE-010 and FMT-PIPE-024: these state what the node-level entry points do, including the blank lines `FormatNodeInDocumentAsync` decides for its target. They are formatter requirements, not fix scope. The fix requirements above carry SHARED-FIX-006.

Existing tests whose expected output contradicts the clarified text (read, not run):

- RH5408: `VerifyDocumentedPropertyWithoutTrailingInitializerCommentIsDetectedAndFixed` and `VerifyDocumentedPropertyWithTrailingInitializerCommentIsDetectedAndFixed` expect the one-line `<summary>` spread over three lines (neither is tagged RH5408-005).
- RH5412: `VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` expects an inserted blank line (named on #1140; this run changes no RH5412 requirement).
- RH5110: `NoFixIsOfferedWhenTheConstrainedDeclarationHoldsACommentTheFormatterRefuses` (SHARED-FIX-010).
- RH5403: `NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses` (SHARED-FIX-010).

In both SHARED-FIX-010 tests the refused shape lies outside the reported clause or block. SHARED-FIX-010 keys the withholding on the node the formatter receives, so whether these tests keep their expectation depends on what the fix passes. Either outcome conforms to the spec.

### Examples for the tests

RH5110-003 (members and empty interface), RH5111-004 (lambda declarator; documented property), RH5113-003 (documentation comment and spacing), RH5206-003 (no blank line in front of `return`), RH5401-003 (documentation comment), RH5403-003 (unbraced other branch), RH5404-003 (header spacing), RH5408-005 (documentation comment).

The texts marked "Formatting the document would …" under each example describe document formatting, read from the formatter spec (FMT-BLANK-001, FMT-STRUCT-001, FMT-STRUCT-017) and from the instances in the issue. The fix's current output on these inputs was not measured.

### Output questions

These are the two questions #1140 already carries (unanswered, quoted from the issue's "Output questions still open"; its Before outputs are the issue's, not re-measured), plus a third with the same mechanism.

**1 · RH5112 · Wrapped fluent calls should keep the first call on the original line — chain in an `if` header** (SHARED-CHAIN-009, RH5112-003, RH5201-005)

Input:
```csharp
        if (x==1 && a
            .B())
        {
            int  y = 1;
        }
```
Before (`main`): the SHARED-CHAIN-009 fallback formats the statement.
```csharp
        if (x == 1 && a.B())
        {
            int y = 1;
        }
```
After, A (recommended): only the chain's line is laid out.
```csharp
        if (x == 1 && a.B())
        {
            int  y = 1;
        }
```
After, B: as `main`.
Why A: SHARED-FIX-006; the body is not what the diagnostic is about.

**2 · RH5101 · Argument list line breaks — operator spacing inside an element** (RH5101-003, RH5102-003; the answer decides the same question for RH5303–RH5308, RH5417-003, and the interior of RH5403-003 and RH5404-003)

Input:
```csharp
M(
    1+2,
    3)
```
Before (`main`):
```csharp
M(1 + 2,
  3)
```
After, A (recommended):
```csharp
M(1+2,
  3)
```
After, B: as `main`.
Why A: SHARED-FIX-006; RH6023 reports the operator spacing separately.

**3 · RH5206 · Switch expression braces should be anchored — switch expression in an `if` header** (RH5206-003, the "innermost statement" sentence; same mechanism as question 1)

Input:
```csharp
class C
{
    bool Check(string s) => true;

    void M(int status)
    {
        if (Check(status switch
            {
              0 => "Idle",
                _ => "Other"
            }))
        {
            int  y = 1;
        }
    }
}
```
Before (`main`, per RH5206-003 as written: the fix formats the innermost statement, the `if`. The columns are the ones document formatting gives, measured with `scripts/trace.sh`. The fix itself was not run):
```csharp
        if (Check(status switch
                  {
                      0 => "Idle",
                      _ => "Other"
                  }))
        {
            int y = 1;
        }
```
After, A (recommended): only the switch expression and its line are laid out; the body stays.
```csharp
        if (Check(status switch
                  {
                      0 => "Idle",
                      _ => "Other"
                  }))
        {
            int  y = 1;
        }
```
After, B: as `main`.
Why A: same answer as question 1. The braces depend only on the code in front of the switch expression on its line, not on the body.

The run can write specs and tests for everything else without these answers. They block only the requirements listed in each heading.

### Internal choices

- No change doc (see the top). Alternative: a change doc for the eight clarified requirements, rejected because intended behavior (SHARED-FIX-006, M1) does not change.
- Recommendation for the lead: keep a clarified requirement only where this run brings that fix's code in line. Revert the hunk for a rule whose fix the run leaves as on `main`, and let #1140 keep that instance (`Refs`). The reason: a spec that the same pull request's code contradicts fails the spec checker and the auditor, and the bug-fix lane admits a clarification only of behavior the code has after the run. Each requirement's hunk, with its `Uses` entry, stands alone. Alternative: keep every clarification as a statement of intent, since SHARED-FIX-006 demands the behavior already.
- Where the wider scope stays inside the reported construct, or depends on the code in front of it, the text is left unchanged (questions 1 to 3). It is not written on my own recommendation.
- In RH5403-003, RH5404-003, and RH5206-003, M1 is applied to every leading trivia ("blank lines included"), not only to a comment line. This follows the M1 wording and the SHARED-FIX-006 blank-line bullet.

### Follow-up candidates

**SHARED-FIX-007 · Shared line — does it apply to every fix that changes something inside a member, or only to fixes that lay out a member?** The bullet reads literally: "When the member a fix changes shares its line with another member after the fix, the changed member MUST get its own line". Read that way, it covers a fix that only changes a gap, too. Input (RH6024 · operator spacing):
```csharp
class C
{
    int _a = 1+2; int _b;
}
```
Today: not measured.
- A (recommended): `    int _a = 1 + 2; int _b;`. Only the gaps change (SHARED-FIX-006, P9).
- B: `    int _a = 1 + 2;` and then `    int _b;` on the next line, the literal reading of SHARED-FIX-007.

Why A: the fix does not lay out the member's lines, so the shared line is not the fix's to change. The case is outside the run's issues, which concern fixes that format a member.

## thoenissen/Reihitsu#1687

### Changed requirements

_None._ SHARED-FIX-007 (*Shared line*) already requires the issue's output. SHARED-ATTR-008 and SHARED-BODY-003 do not contradict it: SHARED-BODY-003 hands the lines around the member to SHARED-FIX-006 and SHARED-FIX-007 explicitly.

Facts for the skeptic, read from the specs:
- The issue's fallback argument says the input "already violates another rule (RH5417 reports a member sharing a line)". That does not hold. RH5417 reports a body on one line. No rule reports two members on one line: RH5404's non-goals send that case to thoenissen/Reihitsu#1446. So `[Obsolete] int _a; int _b;` is not rare by the definition in `AGENTS.md`.
- RH3202-002 and RH5417-003 restate SHARED-FIX-007 with a qualifier: they separate only when the line on which the member starts begins with a member. If the attribute-placement fix or the expression-body fixes get the same qualified behavior, the qualifier has to be written into SHARED-ATTR-008 or SHARED-BODY-003 (or the rule specs). That would be a spec amendment after the implementation, routed back to me.
- The comment of 2026-11-14 on the issue also names an RH3208 local function that shares a line with a statement. SHARED-FIX-007 (*Shared line*) covers members only, so that case is not a defect under any spec.

### Examples for the tests

SHARED-FIX-007 (*Shared line*) example (RH5417), and the issue's attribute input, under LF and CRLF.

### Output questions

_None._

### Internal choices

- No spec text is written before the reproduction. Which fixes reproduce, and whether a qualifier is added, decides what the text has to say. Alternative: copy the RH3202-002 qualifier into SHARED-ATTR-008 and SHARED-BODY-003 now. Rejected because it would specify behavior no fix has yet.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1748

### Changed requirements

- RH5403-003 — clarified with M1 (see #1140 above): no blank line above the statement or above a comment that leads it; leading trivia stays as written.
- RH5404-003 — clarified with M1 (see #1140 above): the same for the declaration.

### Examples for the tests

RH5403-003, the example "no blank line is inserted above a comment that leads the statement" (the issue's input inside a method). RH5404-003, the example "the header and a comment that leads the declaration stay as written". The issue asks for each under LF and CRLF.

### Output questions

_None._ M1 answers the issue's question.

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

- Changed files: `sdd/specs/analyzer/RH5110.md`, `RH5111.md`, `RH5113.md`, `RH5206.md`, `RH5401.md`, `RH5403.md`, `RH5404.md`, `RH5408.md` (requirement text, examples, `Uses` rows), and this handover.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: passed, 51/51.

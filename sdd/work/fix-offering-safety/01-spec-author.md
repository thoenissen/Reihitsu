Role: spec author
Commit: 9aac1b7
Verdict: #1600: open question; #1150: spec written; #1274: open question; #1143: no spec change; #1673: no spec change; #1678: open question

My prompt carried no intended fix or hoped-for verdict beyond the issues' own text; nothing was ignored.

## thoenissen/Reihitsu#1600

### Changed requirements

None.

### Examples for the tests

- Code-fix side: SHARED-FIX-003 (*Build configurations*: "a run-time difference that only a build defining, or not defining, a conditional compilation symbol would show counts as well") already decides it. A fix whose edit changes the value of a string in another build MUST be withheld, and the diagnostic stays. Tests tag SHARED-FIX-003. RH3004-008 only covers carriers inside the anonymous method. It says outright that "a string or a comment that starts before the anonymous method's first token is not decided by it", so the issue's input is SHARED-FIX-003's case and RH3004-008 does not contradict it. No clarification is needed: the guard is new code that brings the fixes back to SHARED-FIX-003. It does not write down behavior that already exists, so it is not a bug-fix clarification. RH3004-008 already defines *spanning carrier* and *`#` line*, and a shared guard can reuse those definitions.
- Formatter side: undecided. See the output question below.

Reachability (`AGENTS.md`, *Supported inputs*):
- **Code fixes.** The IDE analyzes with the project's symbols, so the issue's `#if A` input (with `A`, or `DEBUG`, defined) reaches RH3004's fix and every other fix that edits behind the carrier.
- **CLI.** `FormatCommandHandler` parses with `CSharpParseOptions.Default` and no symbols, so the issue's input as written does not reach whole-document formatting: without `A`, the text is a string literal and the formatter leaves it alone. The CLI reaches the mirrored shape `#if !A` ⏎ `/*` ⏎ `#else` ⏎ `var s = @"` ⏎ `*/` ⏎ … ⏎ `#endif`. The test implementer should use that shape for the formatter test.

### Output questions

**1 · Whole-document formatting (CLI) — a line in a comment that is a directive in another build**

Input (formatted by the CLI, which defines no symbols):
```csharp
class C
{
    void M()
    {
#if !A
        /*
#else
        var s = @"
        */
        Action a = delegate ()
            {
                Console.WriteLine(1);
            }; //";
#endif
    }
}
```

Before (`main`): per the issue, the formatter re-lays out the `Action a = …` lines (re-indents the block). In a build with `A`, those lines are the content of `s`, so `s` changes. (Today's exact output was not run; the test implementer captures it.)

After, A (recommended): the input unchanged.
The CLI reports: `C.cs: unsupported syntax — a string or comment that spans lines holds a line starting with '#' inside a conditional block (line 5)` (wording is the implementer's).
Code fixes no longer offered: every fix whose formatted node holds such a comment or string. Fixes whose edit lies behind a carrier outside their node are withheld by their own SHARED-FIX-003 guard either way.
Declared in this work (#1600 is about this shape).

After, B: the input unchanged, silently, like a file with a syntax error. Nothing is reported.

After, C: unchanged from today. The formatter keeps editing such text, and the gap is recorded as a decision.

Why A: non-destructiveness comes first. A whole-file refusal is the only skip that needs no guards in every phase, and the declaring mechanism is the one that reports a whole-file refusal. B would add a second silent refusal next to syntax errors, and C keeps a meaning change in another build.
Special cases the shape would delete: probably none in the formatter, which has no guard for it today. Possibly the anonymous-method fix's check for such lines inside the method it rewrites, but only where that fix formats the method through the formatter. The implementer has to confirm this.

Notes for the lead:
- `AGENTS.md` says "a shape that deletes nothing is not declared". If the check above finds nothing to delete, A conflicts with that line. The question still has to reach the maintainer, because declaring is an option and only the maintainer decides it.
- Define the shape as carriers that **start inside a conditional group**. Without that condition, a raw string holding a Markdown `# Heading` line, which is common, would refuse the file.
- Requirement IDs touched: FMT-PIPE-002, FMT-PIPE-003 ("what the formatted code compiles to" does not say "in every build configuration"), FMT-UNSUP-001, FMT-UNSUP-003, SHARED-FIX-003, SHARED-FIX-010.
- Under A, the run widens to a behavior change: a new FMT-UNSUP requirement and a change doc with *Special cases*. Under B, a new FMT-PIPE requirement and a change doc. Under C, FMT-PIPE-003 gets a clarification that names the gap.

### Internal choices

- The code-fix guard's scope is a rare-class withholding decided by the run, not an output question. Recommendation: one shared Core check that reuses RH3004-008's *spanning carrier* and *`#` line* definitions. Withhold a fix when such a carrier starts inside a conditional group at or before the end of the fix's edit. Alternative: withhold every fix in a document that holds such a carrier. That is cheaper, but it gives away more safe ground.

### Follow-up candidates

None.

## thoenissen/Reihitsu#1150

### Changed requirements

- SHARED-FIX-011 — A fix that formats through the formatter is offered only where the formatter formats — added (maintainer-decided text, item 1). It is the next free ID; SHARED-FIX-009 and SHARED-FIX-010 are taken. It is inserted after SHARED-FIX-005 in `sdd/specs/shared/code-fixes.md` and at its sorted position in `sdd/specs/requirement-ids.txt`.
- FMT-PIPE-005 — Why clarified (item 3); it now names SHARED-FIX-011.
- FMT-PIPE-006 — Why clarified (item 2).
- SHARED-JOIN-002 — Why clarified (item 4).
- RH7101-001 — Why clarified (item 5).
- `code-fixes.md` non-goal "What a fix produces and when it is withheld" — extended (item 6). It now names SHARED-FIX-003, SHARED-FIX-005, and SHARED-FIX-011.
- `Uses` headers kept exact:
  - `code-fixes.md` adds FMT-PIPE-005, FMT-PIPE-031, FMT-STRUCT-003, FMT-STRUCT-023.
  - `pipeline.md` adds SHARED-FIX-011.
  - `line-join-rules.md` adds FMT-PIPE-005, SHARED-FIX-011.
  - `RH7101.md` adds SHARED-FIX-011.
- `scripts/test.sh --project architecture --filter Sdd`: the spec-structure tests pass. `RequirementCoverageTests` fails only with "SHARED-FIX-011: no test covers it", which the test implementer's tags resolve.

### Examples for the tests

- SHARED-FIX-011 example: RH6023 in a file with a syntax error elsewhere (`void N(` unclosed). The diagnostic is reported and no fix is offered; once the error is gone, the fix writes `a = 1;`.
- The prose sentence names RH7105 (`ModifierOrderingCodeFixProviderBase`) as an edit-then-format fix that is not offered in such a file.
- The issue's *Done when* asks for one test per affected base class and provider, tagged SHARED-FIX-011.

### Output questions

None.

### Internal choices

- I wrote the maintainer's text verbatim, replacing "the new SHARED-FIX requirement" with `SHARED-FIX-011`. The alternative was to rephrase it.
- The new requirement's "such as FMT-STRUCT-003 or FMT-STRUCT-023" also stays verbatim, although no fix relies on FMT-STRUCT-003 today (see the next item). It is only an example of a declining formatter requirement.
- **Issue item 2 (RH5405–RH5407 directive body) needs no spec change. Its premise looks outdated, and the skeptic should confirm.** `StatementBracesCodeFixProviderBase` does not call the formatter: SHARED-BRACE-002 wraps the body itself. SHARED-BRACE-003's third bullet already withholds the fix for "a directive or disabled text between the header and the body", which is the issue's input. A directive behind the body is always leading trivia of the next token, so FMT-STRUCT-003's "trailing trivia" case does not arise for the fix. For the same reason, RH5405–RH5407 are not affected by item 1 (syntax error): their fix changes the document without the formatter. That makes the issue's "RH5405 spec must record the gap" moot. The gap is SHARED-BRACE-003. I wrote nothing into RH5405.md.
- The skeptic should also re-check item 1's affected list against today's providers, since some may no longer format through the formatter.

### Follow-up candidates

None.

## thoenissen/Reihitsu#1274

### Changed requirements

None, pending the answer.

### Examples for the tests

None until the answer.

### Output questions

**2 · RH5308 · Conditional expressions should be formatted correctly — comment behind `?`**

Input:
```csharp
class C
{
    int M(bool a)
    {
        var x = a ? // c
            1
            : 2;

        return x;
    }
}
```

Before (`main`): the fix (and whole-document formatting, which I ran through the CLI) gives the output below. The diagnostic on `?` stays, and a second application changes nothing.
```csharp
        var x = a ? // c
        1
                    : 2;
```

After, A: the fix and the formatter break the line in front of `?`, and the comment stays behind `?`. The diagnostic is cleared. (The column of `1` is illustrative.)
```csharp
        var x = a
                    ? // c
                      1
                    : 2;
```

After, B: no fix offered; the diagnostic stays. The formatter output is unchanged from today.

After, C (recommended): not reported. The formatter keeps `?` behind `a` when a comment stands behind it, and the fix is moot.

Why C: the comment blocks the canonical shape in every option (the branch can never follow `?` on its line), so reporting a layout that no fix or formatter can reach only nags.

Notes for the lead:
- Requirement IDs touched: RH5308-001, RH5308-004, FMT-BREAK-018 (has no comment exception today), SHARED-DIAG-010, SHARED-FIX-001, SHARED-FIX-005, and SHARED-FIX-011.
- SHARED-FIX-011, which this run adds, forbids B when B is reached through a fix-side copy of a formatter condition. Under SHARED-FIX-011, a no-op is either a formatter or a diagnostic defect, or a named formatter decline that the fix asks about. A is a formatter fix against FMT-BREAK-018. C means a formatter requirement that names the keep, plus an RH5308 exemption under SHARED-DIAG-010.
- The question decides #1673's RH5308 item only insofar as `GetFormatTarget` is concerned. That item is independent of this one.

### Internal choices

None.

### Follow-up candidates

- Under B or C, whole-document formatting still puts the true branch `1` in column 8, the statement's column. That is left of the condition and not a continuation indent (FMT-INDENT-020 territory). The CLI reaches it with the input above. It is a candidate issue: "the formatter indents a conditional branch behind a `//` comment on the `?` line to the statement's column".

## thoenissen/Reihitsu#1143

No spec change. SHARED-FIX-002 already requires one application. It already excludes a diagnostic whose fix a guard withholds on the starting code and releases only after another fix (the maintainer's 2026-10-19 exception, decided via #1138). Tests tag SHARED-FIX-002. The comment's RH8204 nested-placeholder instance is covered by the same requirement.

## thoenissen/Reihitsu#1673

No spec change. SHARED-FIX-005 decides it: a fix whose action would return the document unchanged is not registered. Tests tag SHARED-FIX-005 (`VerifyNoCodeFixIsOffered`). Where no concrete input reaches the unchanged result for a listed fix, that fix drops out, as the issue says. That is the skeptic's and test implementer's finding, not a spec question.

## thoenissen/Reihitsu#1678

No spec change before the answer. The issue itself is the output question (A: keep the gap as a decision; B: withhold every reorder of instance fields in a `partial` type). The lead asks it as written in the issue. I add nothing to its options. What the spec says under each option:

- **A:** SHARED-ORDER-008 already records this exact gap as a decision (`sdd/specs/shared/ordering-move-safety.md`, the paragraph beginning "A declaration of a `partial` type that only excluded code writes anywhere else …": "This is a decided gap, not an oversight"). Nothing changes, and no test or code is needed.
- **B:** that same paragraph would be replaced by a rule counting every `partial` type as one whose field order sets its layout for SHARED-ORDER-007. That needs a change doc (visible output change) and a test per consumer (RH7103, RH7109, RH7309).

Reachability: the IDE reaches it with project symbols.

## Whole run

### Open questions, collected

1. #1600: whole-document formatting of a comment or string with a line that is a directive in another build. Declaring the shape unsupported is option A, which has to reach the maintainer.
2. #1274: RH5308 with a comment behind `?`.
3. #1678: the issue's own question, A or B.

The run cannot write the formatter specs or tests for #1600, the specs or tests for #1274, or the outcome of #1678 until the maintainer answers. #1150 (SHARED-FIX-011 tests), #1143, #1673, and the code-fix side of #1600 (SHARED-FIX-003) do not depend on the answers.

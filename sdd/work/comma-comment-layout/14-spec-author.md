Role: spec author
Commit: 26eefea (statements derived from this head plus the built CLI of the branch, `src/Reihitsu.Cli/bin/Release/net11.0`, run on scratch fixtures; nothing built)
Verdict: open question

The prompt carried the lead's decisions 14–17 as the worklist; it carried no suspected cause or hoped-for verdict beyond them. Decisions 14, 15, and 17 are written. Decision 16 is not: its target output is not a fixed point of the formatter (Output questions, Q1), so the spec side waits for the maintainer.

## Changed requirements

- FMT-UNSUP-009 — Comments and directives between a list element and the comma behind it are unsupported — clarified (`Why:` only, decision 14). The removal of a trailing comma is no longer listed among the phases whose guard the shape removes. A new sentence says a comment in front of a comma on the element's line is not the shape, also when the parser puts it into the comma's leading trivia: in `Parse` and `Diagnose`, the `/** doc */` of `B /** doc */,` is the comma's leading trivia, and the removal still carries it onto the last element.
- FMT-STRUCT-019 — Enums have no trailing comma — clarified (decision 14, supports its tests). "The comma's trivia" names leading and trailing trivia: a comment on the last member's line in front of the comma stays behind the last member, whichever token carries it. New example: `Large /** largest */,` and `B /* last */,` keep their comments. This is `main`'s output (auditor fixture `d_main.cs`). FMT-STRUCT-020 inherits it ("same trivia … rules").
- RH5107-004 — No fix is offered when a comment stands behind the comma on its line — clarified (decision 15). Documentation comments are named in the list. Two conditions, each with both sides (FC-002):
  - *Behind the comma on the comma's line*: withheld for `, /* c */ int b` and `, /// c⏎int b`; kept for `,⏎/* c */ int b` and `,⏎/// c⏎int b`.
  - *Whichever token's trivia carries it*: the comma's trailing trivia, or the parameter's leading trivia (`/** */` and `///` under `Parse`/`Diagnose`). Both withhold.
  
  The `Why:` gains one sentence. The withheld example gains `S` (`, /** c */ int b`) and `U` (`, /// c⏎int b`), "in every documentation mode". The keep-the-fix example gains `V` (`,⏎/** c */ int b` → `int a,⏎/** c */ int b`).
- FMT-BREAK-038 — A comma that starts a line in an initializer, a collection expression, or a pattern ends the element's line — clarified (decision 17). "Starts a line" is now counted in text lines, as FMT-UNSUP-009 counts them, with both sides: `X = 3⏎, Y = 4` starts a line; `X = 3, Y = 4` and `X = 3,⏎Y = 4` do not. Because the gap holds no comment, this count equals SHARED-BLANK-008's.
- Change doc `sdd/changes/2026-11/comma-behind-comment-unsupported.md` (decision 14):
  - *What* no longer lists the trailing-comma removal among the lost handling.
  - The `GetTriviaToPreserve` leading-trivia branch and `ContainsNonFormattingTrivia` moved from *Deleted* to *Checked and kept*. The entry gives the reason (a same-line documentation comment is the comma's leading trivia in `Parse`/`Diagnose`) and notes that the deletion rested on a false premise.
- Change doc `sdd/changes/2026-11/rh5107-comment-behind-leading-comma.md`: the decision "The class is 'a comment behind the comma on its line'" no longer says the guard checks "the comma's trailing trivia". It now names both trivia (decision 15).
- Not changed: FMT-BREAK-012 and FMT-BREAK-038's comment-moves-with-the-comma sentence (decision 16). See Q1.

## Examples for the tests

- FMT-STRUCT-019 (new example), and through FMT-STRUCT-020 the same input in an array initializer and a collection initializer. Use `DocumentationMode.Parse`. The auditor's inputs in 13-auditor finding 1 are the rows: `int[] a = { 1, 2 /** two */, };` → `{ 1, 2 /** two */ }`, `new List<int> { 1 /** one */, }`, a multi-line `new[] { 1, 2 /** two */, }`, and `B /** doc B */, // after` → `B /** doc B */ // after`.
- RH5107-004: methods `S` and `U` (no fix), and `V` (fix kept). Under `DocumentationMode.Diagnose`, the analyzer test harness's default, a `///` or `/** */` in a parameter list also raises CS1587 (XML comment not placed on a valid element). The test has to expect that compiler warning or parse with `Parse`.
- FMT-BREAK-038: no new example, and no test changes for decision 17.

## Output questions

### Q1 — Where a documentation comment behind a comma that starts a line ends up (decision 16; FMT-BREAK-012, FMT-BREAK-038, FMT-DOC-001)

Decision 16 says to bring the code to FMT-BREAK-012/038: a `/** */` or `///` comment behind the moved comma stays behind it. That output is not stable. The documentation-comment phase relocates a documentation comment written behind code onto its own line, and it does so for the comma-last form as well. The built CLI of this branch on `sa/o.cs` (output stable on a second pass; the argument list behaves the same on `main` per 13-auditor):

```csharp
// input, comma-last, documentation mode Parse (the CLI's)
Foo(1, /** c */
    2);
var b = new D
        {
            X = 3, /** c */
            Y = 4
        };
```

```csharp
// output
Foo(1,

    /** c */
    2);

var b = new D
        {
            X = 3,

            /** c */
            Y = 4
        };
```

If the line-break phase wrote `X = 3, /** c */⏎Y = 4` for comma-first input, the next run would relocate the comment (FMT-PIPE-004 broken). Keeping the comment behind the comma therefore also changes `main`'s output for comma-last code. The documentation-comment phase runs before the line-break phase, so a line-break-only fix cannot reach it. FMT-DOC-001 says it "does not yet decide" initializer elements, parameters, or positions inside an expression, so no requirement owns the relocation today. The test implementer cannot write decision 16's tests until this is answered.

Input (documentation mode `Parse`):

```csharp
var b = new D { X = 3
    , /** c */ Y = 4 };
Foo(1
    , /// c
    2);
```

Before (this branch's head; the argument list is the same on `main`, 13-auditor; stable):

```csharp
var b = new D
        {
            X = 3,

            /** c */ Y = 4
        };

Foo(1,

    /// c
    2);
```

After, A (recommended) — keep today's output. The comma-first form gives what the comma-last form `X = 3, /** c */ Y = 4` gives: the documentation comment goes onto its own line in front of the element. FMT-BREAK-012/038 would state that their "a comment behind the comma moves with it" covers comments the parser lets trail the comma (`//`, `/* */`, and every comment in mode `None`). A documentation comment in `Parse`/`Diagnose` is placed as for the same code written comma-last. Output = Before.

After, B — the comment stays behind the moved comma (decision 16 as written). The documentation-comment phase must then stop relocating a documentation comment behind a comma, which also changes comma-last output that `main` formats today:

```csharp
var b = new D
        {
            X = 3, /** c */
            Y = 4
        };

Foo(1, /// c
    2);
```

Why A: the formatter's comma-first output stays identical to its comma-last output, which is how FMT-BREAK-038 was built and audited. B is a change to documentation-comment placement that neither issue asks for. RH5107's fix is withheld for these inputs under either answer (decision 15), so the fix and the formatter cannot disagree.

Recommended route: the lead asks Q1. On A, a spec author adds the sentence to FMT-BREAK-012 and FMT-BREAK-038 and the doc rows go to the tests as characterization of today's output; nothing in `src/` changes for decision 16. On B, the work leaves this run's issues and FMT-DOC-001 has to decide these positions; that is better filed as a follow-up than done here.

## Internal choices

- FMT-STRUCT-019 gained a clarifying sentence and example, so the decision-14 regression tests derive from a requirement and not only from FMT-PIPE-002. The alternative was to leave the spec unchanged and cite "keep the comma's trivia".
- The RH5107-004 examples use `/** c */` and `///` without naming a documentation mode in the markup. The requirement says "in every documentation mode", so the test implementer may run them under `Parse` or `Diagnose` (the CS1587 note above applies).
- Decisions 14 and 15 edit change docs whose status is `Accepted`. I kept the status: the edits correct a premise and a guard description inside the accepted decision and change no visible output that was asked.

## Follow-up candidates

- FMT-DOC-001 does not decide documentation comments behind a comma in argument, parameter, or initializer lists ("does not yet decide"), yet the documentation-comment phase relocates them (Q1 Before). If Q1 is answered A, this is the remaining spec gap. Its visible output is today's, and the follow-up asks FMT-DOC-001 to state it.

Role: spec author
Commit: 3743170
Verdict: change doc amended and moved to 2026-10 (Status stays Accepted, audit-stage passages marked); spec written; no blocking open question; one question found in passing

Inputs: `11-auditor.md` and `squad/decisions/2026-10-31-phase-tree-round-trip-{would-merge-owner,member-brace-owner,change-doc-month}.md`. My prompt carried no intended fix to ignore.

## Change-doc month

- `git mv sdd/changes/2026-11/phase-tree-round-trip.md sdd/changes/2026-10/phase-tree-round-trip.md`; `Date` is now `2026-10`. The `Depends on` link now reads `line-start-fact.md`, since both docs share a folder.
- `git grep "changes/2026-11"` outside `squad/decisions/` and `sdd/work/` finds nothing.
- Two decision records name the old path in code spans, not links: `…-change-doc.md` and `…-change-doc-month.md`. They are history and were left as they are.

## Structural decisions

- **One lexer-based owner (would-merge-owner, B):** the change doc's *What* and a new decision, "Two tokens read as one: one owner that asks the lexer", carry the decision. FMT-STRUCT-026's first separator gives the decided shapes:
  - space before `42`, `a`, and `a`;
  - no space before `(`, a string, `@a`, `@"s"`, or a comment.

  RH3002 needs no spec change. `RH3002-008` already states the outcome, a space where the content and the neighbour "would otherwise be read as one token", and the lexer-based helper keeps it. The shared helper is a general-purpose syntax utility, so it is a code owner of no spec. No spec names `TokenGapUtilities` either.
- **Member-brace owner (member-brace-owner, B):** the change doc says that `LineBreakBlockRewriter` places the braces of every block and event accessor list, member bodies included, and that `DeclarationBraceLineBreakRewriter` no longer places them. FMT-BREAK-002 gains:
  - a sentence that a node-level entry point's target (member body, block, event accessor list) gets its braces placed whether or not its owning declaration lies inside the formatted node;
  - a Why sentence;
  - an example: `FormatNodeInDocumentAsync` with the body block of `void M()` / `    { Foo(); }` as target gives `    {` / `        Foo();` / `    }`, the output on `main` according to the auditor's harness.

  Code owners are unchanged: both rewriters still exist and stay owners of FMT-BREAK. If the implementer deletes one, `line-breaks.md` and `sdd/specs/README.md` must drop it. `Uses` gains FMT-PIPE-010.

## Audit finding #7 (text)

Re-derived with `scripts/format.sh` at the head on `if (a)` / `    Foo(); /* a` / `    b */` / `Bar();`. The result is `            Foo(); /* a` / `            b */` / `        }` / blank / `        Bar();`, stable on a second run. The auditor reports `            b */ }` on `main`. The change doc's *What* now lists it, attributed to the braced statement keeping its own trailing trivia (FMT-STRUCT-026).

## Findings #1–#6: does the spec decide the shape?

- **#1 (comment leading the expression when a comment stands at the arrow): decided at phase level, example added.**
  - Rules that apply: FMT-STRUCT-026's third separator, and FMT-STRUCT-006, which leaves the comments of a member with a comment directly at the arrow where they are and their final place unspecified.
  - New example in FMT-STRUCT-026 covers three inputs: `int Get() => // c` / `/* d */ 42;`, `… => // c` / `// d` / `42;`, and `int Get()` / `/* c */ =>` / `/* d */ 42;`. In each, the transform's output has a line break between the created `return` and the comment, and the comment keeps leading the expression.
  - This keeps today's final output, as the change doc's "final output unchanged" requires; moving the comment would change it. The final output looks wrong and is not written into the spec (open question 1 below).
- **#2 (nested inserted `}`): already decided, made explicit.** "The enclosing block's `}`" is now defined as the `}` that follows in the transform's output, also an inserted one. A nested-`if` example states:
  - no line break between the inner and the outer inserted `}`;
  - a line break between the outer `}` and `// about Bar`;
  - the same for `#if DEBUG` and for an unbraced inner `else`.
- **#3 (`return` before `a` / `@a`): decided by "would otherwise read as one token".** The first bullet now names `a`, `a`, `@a`, and `@"s"`, and a new example gives the phase output for `a`, `a`, `@a`, `@"s"`, and `(a)`, each with one space after `return` in the final output (FMT-SPACE-012; `Uses` gains FMT-SPACE-012).
- **#4 (node-level member-body braces):** see FMT-BREAK-002 above.
- **#5 (elastic trivia on the parentheses around a `throw` operand): already decided ("every created token"), made explicit.** FMT-STRUCT-026 now names the `(` and `)` written around a `throw` operand (FMT-STRUCT-011).
- **#6 (value of a moved interpolated raw-string text token): not decided before; decided now.** FMT-PIPE-021 compared kinds and texts only. Its bullet now says "kinds, texts, and values", where a token's value is what the parser gives its text. A new example covers an interpolated raw literal whose lines start in column 0: the moved text token in front of the hole still stands for `a `.
  - I treated this as a clarification within FMT-PIPE-021's own Why ("the program the tree is"): the auditor found `main` correct and the run's code wrong, and no reading that wants a different string constant exists.
  - It does tighten the accepted definition, so the change doc's "same tree" decision is marked *(amended 2026-10-31, audit stage)*. The lead may want the maintainer to see that line.

## Changed requirements

- FMT-PIPE-021 — changed (token values; value example)
- FMT-STRUCT-026 — changed (separator shapes for `a`/`a`/`@a`/`@"s"`; enclosing `}` defined; elastic trivia names the `throw` parentheses; three examples)
- FMT-BREAK-002 — clarified (node-level targets; example)
- The change doc is moved to `sdd/changes/2026-10/phase-tree-round-trip.md`; `Affected requirements` adds FMT-BREAK-002.

## Examples for the tests

- FMT-PIPE-021: the column-0 interpolated raw literal. Assert the moved text token's `ValueText` equals the reparsed one, and extend the reparse hook to compare values.
- FMT-STRUCT-026: the nested-`if` example (comment, directive, `else` variants), the `return` examples (phase output and final output), the at-arrow example (phase output only), and the `throw`-parentheses shape in the elastic-trivia tests (accessor and get-only member).
- FMT-BREAK-002: the node-level body-block example, plus constructor body, local-function body, and event accessor list as targets. Expected outputs come from `main` per the decision.

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26/26 passed.

## Open questions for the maintainer

### 1. Final layout of the comments of an expression-bodied member with a comment directly at the arrow (found in passing, not blocking)

**Input** (`scripts/format.sh` at the head; the auditor reports the same on `main`):

```csharp
class C
{
    int Get() => // c
        /* d */ 42;

    int Get2() => // c
        // d
        42;

    int Get3()
        /* c */ =>
        /* d */ 42;
}
```

**Real output:**

```csharp
class C
{
    int Get()
    {// c
        return

        /* d */ 42;
    }

    int Get2()
    {// c
        return

        // d
        42;
    }

    int Get3()
    /* c */ {
        return

        /* d */ 42;
    }
}
```

**Why it looks wrong:**
- The `// c` comment is glued behind `{`.
- `return` stands alone, with a blank line between it and its value.
- `/* c */ {` stands behind the signature.

**Requirement IDs:** FMT-STRUCT-006 (leaves these places unspecified), FMT-STRUCT-026 (decides only the transform's separator), FMT-PIPE-013, FMT-BREAK-003.

**Competing readings:**
- **A — Keep it unspecified in this run.** The output stays as above, and a follow-up issue decides the places. This run's change keeps the output identical to `main`.
- **B — Move the comment that leads the expression behind `return`, as FMT-STRUCT-006 does without a comment at the arrow.** Output: `return /* d */ 42;` and `return // d` / `42;`. This changes the final output in this run.
- **C — Lead the created statement with the comments that stood on lines of their own.** Output: `// d` / `return 42;`. This needs a change of FMT-STRUCT-006.

**Recommendation: A**, with a follow-up issue. FMT-STRUCT-006 deliberately leaves these cases open, this run does not change their output, and deciding them touches FMT-STRUCT-006, FMT-BREAK-003, and the at-arrow comment `// c` together. Pick B instead only if the maintainer wants the `/* d */` shape fixed now.

## Follow-up candidates

- The auditor's hint: `ExpressionBodyToBlockConverter.ExtractSignificantTrivia` drops the whitespace in front of the arrow's trailing comment that it moves behind `{`. FMT-STRUCT-026 neither keeps nor permits that, so the spec is silent. Not decided here; it belongs with open question 1.

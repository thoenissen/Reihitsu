Role: spec author
Commit: 14e40f0
Verdict: spec written

Carries out maintainer answer 2 (`00-lead.md`, decision 37). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decision. No code or test changed. The examples below were run through the CLI built from 14e40f0 (`scripts/format.sh`); the output is as written and stable on a second run.

The decided behavior, as now stated: a statement behind the closing `*/` of a delimited comment that spans several lines stays behind the `*/`, with two exceptions where it starts the line below the comment — (1) a blank-line requirement (FMT-BLANK-001 to 005) separates it from the previous statement and the comment starts on that statement's line; (2) it is the first statement of a block or section and the comment starts behind the opening brace or the section's last label. A comment that starts on its own line leaves the statement behind it in every case (the blank line then goes above the comment, FMT-BLANK-018). This is the code's behavior on head and `main` (19-auditor.md's members; the agreed output of answer 2), so 22-spec-checker's note on FMT-BREAK-006's first sentence is resolved.

## Changed requirements
- FMT-BREAK-006 — One statement per line — changed: new paragraph with the placement behind a multi-line comment's closing line, its two conditions (where the comment starts; whether a blank-line requirement separates the pair), the reason citing answer 2, and an example covering both sides of each condition. `Uses` of `line-breaks.md` gains FMT-BLANK-018.
- FMT-BREAK-002 — Braces start their own line — clarified: a first statement behind such a comment is placed as FMT-BREAK-006 states.
- FMT-BREAK-035 — The first statement of a switch section starts its own line — clarified: the same pointer.
- SHARED-COMMENT-012 — A comment that spans several lines belongs to the line it starts on — changed: the formatter clause names the statement as its one exception and points to FMT-BREAK-006; the `Why` says so. The clause for analyzer rules is unchanged. `Uses` gains FMT-BREAK-006.
- Change doc `sdd/changes/2026-11/phase-order-line-breaks.md`: maintainer answer 2 recorded under `Decisions`, a decision section *A statement behind a multi-line comment's closing line*, and FMT-BREAK-002, FMT-BREAK-035, and SHARED-COMMENT-012 added to `Affected requirements`. `Visible output changes` is unchanged: nothing visible changes.

`scripts/test.sh --project architecture`: Passed 92, Failed 0.

## Examples for the tests
- FMT-BREAK-006 — the new example (methods `M`, `N`, `O`): `A(); /* x⏎ */ B();` stays; `a++; /* y⏎ */ return a;` splits with a blank line; `a++;⏎/* z⏎ */ return a;` stays with the blank line above the comment; `case 1: /* p⏎ */ A();` splits; `case 2:⏎/* q⏎ */ B();` stays. Members already covered by the tagged `StatementBehindMultiLineCommentTests` (the split side) need no second test; the "stays" side needs tagged tests (decision 37). Tag them FMT-BREAK-006 and SHARED-COMMENT-012.

## Output questions
_None._

## Internal choices
- The answer speaks of "a statement"; I applied it to first statements of a block or section too (FMT-BREAK-002, FMT-BREAK-035), since they are statements and the output is `main`'s. Alternative: leave first statements out and raise them separately.
- The exception is stated once in FMT-BREAK-006; SHARED-COMMENT-012 points to it rather than restating the conditions. Alternative: state the conditions in the shared spec — rejected, they rest on FMT-BLANK requirements the shared spec does not use.

## Follow-up candidates
- **Formatter · a switch section or a case label behind the closing line of a multi-line comment.** Outside answer 2, which covers statements. FMT-BREAK-033 asks a section to start its own line and SHARED-COMMENT-012 asks the formatter to break in front of a token behind `*/`, but the formatter (head and `main`) keeps both behind the `*/`.

  Input:
  ```csharp
  switch (a)
  {
      case 1:
          A();
          break;
      /* z
      */ case 2:
      /* l
      */ case 3:
          B();
          break;
  }
  ```

  Real output: unchanged apart from a blank line above `/* z`. Reading A (as answer 2 for statements, recommended): keep `case 2:` and `case 3:` behind the `*/`, and state it in FMT-BREAK-033 and SHARED-COMMENT-012. Reading B (spec as written): break in front of `case 2:` and `case 3:`:
  ```csharp
      /* z
      */
      case 2:
      /* l
      */
      case 3:
  ```
  Recommendation: A, by the maintainer's reasoning for answer 2; it changes no output. The lead decides whether this is covered by answer 2 or filed.

Role: spec author
Commit: 037d450
Verdict: change doc written

Repair round 1 for finding 1 (check 1) of `16-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

I compared every requirement heading that the branch's spec diff touches (against merge base 5af53bd) with the *Affected requirements* rows of the run's five change docs. Only FMT-BLANK-018 and RH2001-004 were missing. FMT-BREAK-040 and FMT-INDENT-033 showed up only because new requirements were appended right after them, and the spec header rows hold `Uses` lists only. The checker said "two examples" for FMT-BLANK-018, but only one is its own (`int[] G { get; } =⏎/* b */ { 1, 2 };`). The other changed example (`// e⏎{ })`) belongs to FMT-BLANK-020, which `continuation-indent.md` already listed.

- `sdd/changes/2026-11/continuation-indent.md`: added FMT-BLANK-018 to *Affected requirements* and to the heading of *Split fields and other declarations*. Also added one sentence there: the property initializer's value that stands behind its `=` with the comment glued to it moves to column 8, one level deeper than the property's line. This follows FMT-INDENT-034 and is the spec example's new output.
- `sdd/changes/2026-11/continuation-join.md`: added RH2001-004 to *Affected requirements*. Also added one sentence under the RH2001/RH7101 bullet: RH2001-004's modifier-list paragraph now leaves every comment or directive between two modifiers, or between the last modifier and the type, to RH2001-009. Before, it named only a comment for the gap in front of the type. The rule's output does not change beyond what the bullet already shows.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 48 passed, 0 failed.

## Changed requirements
_None._ Only the change docs' bookkeeping changed. FMT-BLANK-018 and RH2001-004 were already edited in the spec by earlier rounds.

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._

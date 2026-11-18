Role: spec author
Commit: b889c91
Verdict: spec written

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements
None. Text corrections only:

- Finding 1 (decision 6): the seven CLI lines in `sdd/changes/2026-11/attached-part-below-element.md` now start with `error:`, as `FormatCommandHandler.ReportUnsupported` prints (`severity = "error"`).
- Finding 2: `AttachedPartGapUtilities` is a code owner of `formatter/line-breaks.md` (header and `sdd/specs/README.md` row), next to `AttachedPartLineStartRewriter`. It defines the gaps FMT-BREAK-039/040 join; FMT-UNSUP-014 refers to "the kinds whose gap FMT-BREAK-039 or FMT-BREAK-040 joins" and already lists both under `Uses`, so the line-breaks spec owns the behavior and the precheck is a caller (sdd/README.md: a type is owned by the spec of the behavior it implements, not by its callers).
- Finding 3: *Special cases / Deleted* names `AttachedPartLineStartRewriter.CanJoin`, "formerly `ArgumentListLineStartRewriter.CanJoin`", in the past tense for the deleted term, and says that `main`'s `IsArgumentListOpenParenthesis` admitted those argument lists and `AttachedPartGapUtilities.IsAttachedPartGap` now admits them with the other parts.
- AGENTS.md, `PrecheckPhase` row: checked, unchanged. On this branch it already lists the eighth shape (FMT-UNSUP-014) and matches the spec and `PrecheckPhase`'s use of `AttachedPartGapUtilities.IsAttachedPartGap`. The spec checker is right; the implementer's note (08) described the row before a later commit updated it.
- AGENTS.md, `LineBreakPhase` row: the *Decisions* cell said only "the join of an argument list's opening parenthesis". It now says the phase joins an attached part (argument list, element access, conditional element access, or null-forgiving operator) onto the line of the token in front of it, and removes a line break between a conditional element access's `?` and `[`. The guards cell already names `AttachedPartGapUtilities.IsAttachedPartGap` and needed no change.

`scripts/test.sh --project architecture`: Passed 198, Failed 0.

## Examples for the tests
_None._ No example changed.

## Output questions
_None._

## Internal choices
- `AttachedPartGapUtilities` belongs to `formatter/line-breaks.md`, not to `formatter/unsupported-syntax.md`. The alternative is the unsupported-syntax spec, but there the shape is defined by reference to FMT-BREAK-039/040.

## Follow-up candidates
_None._

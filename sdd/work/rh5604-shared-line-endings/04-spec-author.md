Role: spec author
Commit: 9aa9179
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 4 of `00-lead.md`.

Clarification only: no requirement added, changed, or retired; no change doc (nothing visible changes); rule page `documentation/rules/RH5604.md` not touched (it names no code).

- `sdd/specs/formatter/line-endings.md` intro: names `LineEndingUtilities` as the rewrite the phase and the RH5604 code fix both run, and why that type is listed under the pipeline spec's code owners rather than this one.
- `sdd/specs/analyzer/RH5604.md` intro: the fix runs the same rewrite as the formatter's line-ending phase, without the formatter's refusals (pinned by `VerifyCodeFixConvertsTheLineEndingsOfAFileTheFormatterRefusesAsUnsupported`, 02-test-implementer).
- `Code owners` lines unchanged, and `sdd/specs/README.md` needs no change: `LineEndingNormalizationPhase`, the RH5604 analyzer, and its fix provider still exist and still own their spec's behavior; `LineEndingUtilities` already is a code owner of `formatter/pipeline.md`, and a type is a code owner of at most one spec (`sdd/README.md`).
- No requirement ID reference added, so `Uses` rows stay exact. `scripts/test.sh --project architecture`: 198/198 passed.
- Searched `sdd/specs` for other text naming the replaced code (`ApplyCodeFixAsync`, `ReihitsuFormatterHelpers.DetectEndOfLine`, the phase's body): none.

## Changed requirements
_None._ (intro text of `formatter/line-endings.md` and `analyzer/RH5604.md` clarified)

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- `LineEndingUtilities` stays a code owner of `formatter/pipeline.md` (its `DetectEndOfLine` implements FMT-PIPE-007, which `line-endings.md` lists as a non-goal) rather than moving to `formatter/line-endings.md`; the alternative moves detection under a spec that excludes it.

## Follow-up candidates
_None_ new.

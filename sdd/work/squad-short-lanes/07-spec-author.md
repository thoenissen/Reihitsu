Role: spec author
Commit: 5543a62
Verdict: text written

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Tooling change: no requirement IDs. Statements derived from `src/Reihitsu.Tooling/LaneCheckCommand.cs` (`Execute`, `PrintUsage`, `TryParseArguments`), `scripts/lane-check.sh` lines 95–107, and `scripts/proof/verify-text-only.cs` lines 128–131 at 5543a62, and from 05-implementer.md, *Demonstrations* 8 and 9.

## Changed requirements
_None._ Text changes for decisions 20 and 21 of `00-lead.md`:

| Decision | Where | Statement |
|---|---|---|
| 20 | `scripts/README.md`, *The lane check*, second paragraph | `lane-check.sh` gets the wording paths from `scripts/lane-check/lane-check.cs --wording-paths`, which reads the diff's `--name-status --no-renames` lines and writes each path the classifier counts as wording, one per line, and takes no other argument. The *text-only proof* paragraph keeps only the pointer ("gets the wording paths of the diff from the classifier"), so the mechanism is stated once. |
| 21 | `scripts/README.md`, *The text-only proof*, `--exclude` paragraph | The proof finds renames, so a rename is one changed file with two paths, left out only when both are excluded; a rename between a wording path and another file stays in the proof. "A renamed file is never text-only" stays in its existing sentence and is not repeated. |
| 20, 21 | change doc, *What*, wording-lane bullet on the proof | Names the query option `--wording-paths` and that a rename stays in the proof unless both paths are excluded. |

No test file, no C# file touched; `scripts/README.md` is tooling and the change doc is a change doc, so the run's lane is unchanged by these edits.

## Examples for the tests
_None._ The `--wording-paths` test is the test implementer's (decision 20); its observable contract as now stated: no other argument (exit 2 otherwise, demonstration 9), writes the wording paths one per line, nothing for a diff without one (demonstration 8).

## Output questions
_None._

## Internal choices
- The `--wording-paths` contract is stated in *The lane check*, not in *The text-only proof*, because it is an option of the lane check's tool; alternative: state it next to `--exclude`.

## Follow-up candidates
_None._

Role: spec author
Commit: 35bc0a9
Verdict: spec written

Follow-up round: the maintainer's answers "1a 2a 3a 4a 5a 6a" (00-lead.md, *Maintainer answers*) and lead decisions 1 and 2 (00-lead.md, *Decisions taken*) carried into the spec and the change doc. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: 00-lead.md, 01-spec-author.md, 02-skeptic.md, the change doc and the spec diff.

Files written:

- `sdd/specs/cli/reihitsu-format.md` — CLI-002, CLI-006, CLI-017, CLI-022, CLI-023 changed.
- `sdd/changes/2026-11/parallel-cli-formatting.md` — `Status: Accepted`; affected requirements now include CLI-006 and CLI-017; visible output changes 3 and 6 completed, 7 (started write completes) and 8 (aliases in write mode) added; *Decisions* holds one decision per answer and per lead decision; two rejected alternatives added.
- `src/Reihitsu.Cli/README.MD` — unchanged in this round; it already carries the Q3 A text.

## Changed requirements

- CLI-002 — Invalid arguments fail before any file is read — changed: the `--max-parallelism` error applies to "a value CLI-023 does not accept"; message unchanged.
- CLI-006 — File selection — clarified (decision 2): a *full path* selected more than once is processed once; two full paths naming one physical file through a symbolic or hard link are two selections. Example added. States today's behavior.
- CLI-017 — Exit codes — changed (decision 1): cancellation ends after every file whose write has started is written completely.
- CLI-022 — changed: (Q5 A) write-mode exemption for several paths to one physical file — which are named `Formatted:`/`Unchanged:` and the summary count MAY vary, content MUST be formatted, `--check`/`--dry-run` unchanged; (decision 1, Q4 A) a started write MUST NOT be interrupted, completes, counts as finished and is named, also when a file selected earlier is unfinished; a file not finished is left as it was and not named. Two examples added (cancellation, symlink).
- CLI-023 — changed (Q6 A): accepted counts are ASCII digits `0`–`9` only, value 1 to 2147483647; sign, white space, decimal point, letters, `0`, larger numbers invalid; leading zeros accepted (`007` = 7); last valid value wins; example list extended (`+2`, ` 2`, `99999999999` invalid; `1000` accepted).

Q1 (name), Q2 (default), Q3 (documentation) were already written with option A in round 01; nothing changed for them beyond the change doc's *Decisions*.

`scripts/test.sh --project architecture`: 152 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` names CLI-022 and CLI-023 as uncovered, as in round 01; it clears when the test implementer tags tests with them. No other spec-structure test failed.

## Examples for the tests

- CLI-022 — everything listed in 01 and 02, plus: the cancellation example (`A.cs` unfinished, `B.cs` written, `C.cs`'s write started → `C.cs` completes, both `B.cs` and `C.cs` named in order, `Operation canceled.`, `A.cs` untouched, exit 2, no summary) — a test needs a fake `IFileSystem` that records whether the write received a cancelable token, or that is canceled mid-write; the symlink example (write mode: content formatted, at least one path `Formatted:`, count 1 or 2; `--check`: both `Not formatted:`, `2 of 2`). A symlink end-to-end test is platform-dependent; the write-mode variance must not be asserted to a single value.
- CLI-023 — `+2`, ` 2`, `99999999999` invalid; `1000` and `007` accepted; `2` then `8` uses 8.
- CLI-006 — `src/A.cs src/` processes `src/A.cs` once (existing behavior; likely already covered).
- CLI-017 — the started-write cancellation case above.

## Output questions

_None._ All six answered.

## Internal choices

- The started-write rule is stated in CLI-022 (cancellation paragraph) and referenced from CLI-017, rather than added as a requirement of its own. Alternative: a new CLI-024 for non-destructive cancellation.
- The alias exemption is stated in CLI-022 and CLI-006 says only which selections exist; alternative: state the exemption in CLI-006.
- Leading zeros (`007`) are accepted: they follow from "decimal digits only, value 1 to 2147483647" in the answer; `0000` is invalid as value 0. Alternative: reject leading zeros — the answer gives no ground for it.
- The README sentence "The output, its order, and the exit code are the same for every count" is kept verbatim as answered (Q3 A), although the alias case makes it not literally true in write mode; the change doc's *Decisions* records that the README need not list the corner case (the skeptic raised this in 02).

## Follow-up candidates

Unchanged from 00-lead.md: profiling/benchmark; `LcsComputer` memory times the degree; Windows behavior of concurrent writes to one file through aliases; processing one physical file once (Q5's option B) as a change of its own.

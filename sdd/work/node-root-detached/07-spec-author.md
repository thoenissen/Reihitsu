Role: spec author
Commit: 15c511d
Verdict: text written

This handover covers the maintainer's clarification in `00-lead.md` and its later addition about intake, decision 9, and the coordinator's three tasks. The prompt carried no intended fix beyond those; nothing was ignored.

## 1. Process rule: supported inputs

The rule is stated once, in `AGENTS.md`, in a new section *Supported inputs* placed before *Issue intake*:

- No assembly of this repository is a public API, the formatter's public methods included.
- A component has to work for the inputs the repository's own callers pass: the analyzers and their code fixes, the CLI, and whole-document formatting.
- A case no caller reaches is neither a defect nor a follow-up.
- Reachability is part of an issue's premise and is checked at intake, before anything is built. An issue whose case no caller reaches is not implemented: a test already written stays, the PR links the issue with `Refs #<N>`, and the result says so, so the maintainer can close the issue.
- A claim that a caller reaches a case names the caller and the path.
- Tests may still call a method directly.

Pointers, without restating the rule:

- `AGENTS.md`, *Issue intake*: a first bullet, "Check that a caller reaches the reported case before anything is built".
- `squad/README.md`:
  - *Issues are hypotheses*: a bullet saying the skeptic checks reachability and the lead reports an unreachable issue as the run's result.
  - *What reaches the maintainer*: an output no caller reaches is never in question.
  - *Scope*: an unreachable find is dropped, not a follow-up.
  - *Failure classes, not instances*: only members a caller reaches.
  - *Rules for every role*: out-of-scope finds become follow-up candidates unless no caller reaches them.
- `squad/lead.md`:
  - section 4: a table row for the skeptic's "no caller reaches the issue's case", and a *Not reachable* paragraph next to *Not reproduced*. It keeps a test only if one was written, switches to `Refs #<N>`, skips the remaining roles except the spec checker and the validator, reports the result, and does not stop or escalate;
  - *Follow-ups*: an unreachable candidate is dropped, recorded under *Decisions taken*, and not passed to the issue manager.
- `squad/roles/skeptic.md`: question 1 starts with the reachability check, and question 5 says an unreachable member is not a failing member.
- `squad/roles/auditor.md`: check 3 says an unreachable member or finding is not a finding.
- `squad/roles/issue-manager.md`: a new step 1 drops an unreachable candidate and reports it back. The later steps are renumbered.
- `squad/roles/spec-author.md`: an unreachable case is not a follow-up candidate.

Process change doc `sdd/changes/2026-10/supported-inputs.md`:

- `Status: Draft`, for the lead to accept; `Affected requirements: none — process change`.
- *Why* states that this run spent stops and follow-ups on node-level inputs no caller reaches. It names no note.
- Decisions: one statement in `AGENTS.md`; an unreachable issue ends like a narrowed one, without a stop; "reaches" names a caller.

## 2. Change doc `node-root-detached.md`

New passages are marked *(amended 2026-10-31, process stage)*, and the amendment note says what they do.

- **Follow-ups removed.**
  - The preceding-token helpers: no caller was shown to reach a difference, so they stay as they are and no follow-up is filed.
  - Chain outermost-ness: no caller reaches the case, so it stays as it is and no follow-up is filed.
- **`Visible output changes` added.** It sits between *Why* and *Decisions*, in the template's form.

### Finding

Two changed node-level outputs are reached through a code fix. I verified both with `scripts/apply-fix.sh` on this branch (`15c511d`) and on `main` (`c24bc1d`, extracted to the scratchpad), under LF and CRLF:

- **RH5204** on an unbraced embedded `if` behind `/* note⏎*/`, inside `while (a)`, with misindented braces and body:
  - On `main` the fix does not converge (`no-progress`): it moves `/* note` to column 8, and the braces and `X();` stay at 8 and 12.
  - On the branch it converges in one iteration: `/* note`, `{`, and `}` stand in column 12 and `X();` in column 16, the columns FMT-INDENT-004 gives.
  - This is the zero-offset case the maintainer decided (A).
- **RH5405** on `else if (y) // note` with an unbraced `Foo();`:
  - On `main` the fix drops `// note`. On the branch it keeps it.
  - In both, the braces stand in column 13 (thoenissen/Reihitsu#1521).
  - This is the comment loss of thoenissen/Reihitsu#1527, closed by this run's `else if` fact. The change doc's decision text now says so.

Not reached:

- `FormatNode`: its only caller, RH5401, passes a property declaration and reads only whether the result is one line.
- Inner chain parts: the chain fixes pass the outermost chain node.
- An assigned collection initializer as target: RH5301, RH5303, and RH5304 pass the object creation or the statement, equals-value clause, or arrow clause that holds it. RH5204 reports no line of the probed initializer.
- A node inside an initializer behind a multi-line comment: no fix found that passes one.

The CLI and whole-document formatting do not call the node-level entry points.

## 3. Wording that promises behavior for unreachable inputs

- `sdd/specs/formatter/pipeline.md`: one non-goal added. Node-level inputs that none of the repository's callers passes are decided only as far as a requirement names them. The node-level requirements decide the nodes a code fix passes, and their examples call the entry points directly with such nodes.
- FMT-PIPE-010, FMT-PIPE-012, FMT-PIPE-023, and FMT-PIPE-024 keep their text. Each names its inputs, and the tests assert those inputs. The non-goal removes the reading that every other node shape is promised. Narrowing each requirement instead would have changed requirement text that tests are tagged with.
- In the change doc, *What* keeps its descriptions of changed outputs as the mechanism. The new section says which of them are visible.

## Changed requirements

None. `pipeline.md` gains a non-goal only. No ID was added, so the ledger is unchanged.

`scripts/test.sh --no-install --project architecture`: 92/92 pass. `scripts/lane-check.sh behavior-change --head worktree`: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.

## Examples for the tests

- The two code-fix inputs under *Visible output changes* are candidates for RH5204 and RH5405 code-fix tests.
- The RH5405 one guards the closed comment loss (thoenissen/Reihitsu#1527). Whether this PR closes that issue is the lead's call.

## Output questions

_None._ Both reachable changes are decided:

- RH5204 by the maintainer's zero-offset answer (A).
- RH5405 because it keeps a user comment that `main` drops (the safety rail).

## Internal choices

- **Where the rule lives:** in `AGENTS.md` rather than `squad/README.md`, because it applies to every contributor; the squad files point to it.
- **The non-goal:** one non-goal in `pipeline.md` rather than a coverage sentence in each node-level requirement.
- **Status of the process change doc:** left at `Draft`, for the lead to accept, as an internal-only change doc.

## Follow-up candidates

_None._

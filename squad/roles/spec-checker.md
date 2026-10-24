# Spec checker

You verify that what is written down matches what was changed. You change nothing; you report what does not match.

Read [`../README.md`](../README.md) and [`sdd/README.md`](../../sdd/README.md) first — the rulebook's *Keeping specs true* section is your checklist.

## When you run

In every lane, after the cleaner (in the docs lane, after the spec author), and again after a repair round.

## Inputs

`00-lead.md`, the spec author's and the test implementer's handovers when there are any, and the diff: `git diff <base>...HEAD`.

## Checks

1. **Change doc exactly when required.** Intended behavior changes — a requirement added with new behavior, changed, or retired; a rule, phase, or surface added or removed — and only then. A bug fix and a clarification need none. A change doc has no task list or implementation plan, and is `Accepted` before the PR is marked ready for review.
2. **Specs match the diff.** Every behavior the diff changes is stated by a requirement; no requirement contradicts the diff; a clarification writes down behavior that already exists. Requirements follow the format: one per heading, the normative sentence first, `Why:` present, examples where behavior is visible, analyzer markup `{|RH####:…|}`.
3. **IDs.** New IDs take the next free number and are inserted at their sorted position in `sdd/specs/requirement-ids.txt`; no ID is reused, renumbered, or deleted — a dropped requirement is retired in place, and its ledger line stays.
4. **Code owners exist.** Every type named in a spec header the diff touches exists in a production project — `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests"` checks it, including generic arity, delegates, and Razor components; no paths or line numbers. Owners the diff renamed, added, or removed are updated, and `sdd/specs/README.md` lists every spec with matching owners.
5. **User documentation is consistent.** A rule page under `documentation/rules/` that the change affects matches the spec, follows [`create-rule-doc`](../../.claude/commands/create-rule-doc.md) — short, no formatter mention, nothing `general-notes.md` covers, the footer — and its `### Correction` equals the code fix's output. The diagnostic ID range fits the category, and the `helpLinkUri` page exists.
6. **Open questions reach the maintainer.** Every question in the spec author's handover reaches the maintainer — a blocking one through escalation 1, the others in the lead's message before the auditor. None is written into the spec, and none is recorded only in the PR.
7. **Requirements are covered and the specs are consistent.** When the diff touches `sdd/specs/`, run `scripts/test.sh --project architecture` and quote its result line. Every new active requirement of an enforced spec has a covering `[Requirement]` test in the diff; a missing one is a finding for the test implementer, and in the docs lane it means the lane has to widen.
8. **New tests carry their requirement's tag.** Every test method the diff adds that the test implementer's `Tests` table lists against a requirement carries `[Requirement("<ID>")]` for it, and the table names every requirement a test the diff adds or newly tags carries. Every requirement the spec author named under `Examples for the tests` is tagged on at least one test the diff adds or newly tags. A mismatch is a finding for the test implementer. An untagged test that verifies no requirement, or touches one only incidentally, is not a finding (`sdd/README.md`, *Tests and requirements*).
9. **Nothing is left behind.** Before publishing, the branch contains no `sdd/work/` other than this run's, which the lead deletes.

## Must not

- Edit any file other than your handover.
- Judge whether the behavior is good — that is the skeptic's and the auditor's question.

## Handover

`sdd/work/<slug>/<nn>-spec-checker.md`:

```markdown
Role: spec checker
Commit: <sha>
Verdict: consistent | findings

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

## Checked
<one line per check above: ok, or the finding numbers>
```

`Owner` is the role that fixes it: spec author, implementer, or test implementer. Commit your handover with `[skip ci]`.

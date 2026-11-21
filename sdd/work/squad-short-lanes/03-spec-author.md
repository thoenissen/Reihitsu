Role: spec author
Commit: 456661a
Verdict: change doc written

The amendment follows decisions 10–17 in `00-lead.md` on the skeptic's findings F1–F6 and internal choices. My prompt carried no intended fix or verdict to ignore. The change doc `sdd/changes/2026-11/squad-wording-lane-and-ci-only-watch.md` now has `Status: Accepted` (decision 17). No spec, requirement ID, rule page, code, or test changed.

## Changed requirements

None. This is a process change.

| Decision | Finding | Where it is written now |
|---|---|---|
| 10 | F1 | `squad/roles/spec-author.md`: intro, *Wording* bullet (write the whole text change, then report `needs a test` and name each test), new bullet *Every lane, when the run changes what a diagnostic or a code fix says*, *Must not*. `squad/roles/test-implementer.md`: new bullet *A run widened from the wording lane*. `squad/README.md`: roles table and *Wording* bullet. `squad/lead.md` §4: row `needs a test`. `.claude/agents/spec-author.md`: description. Change doc: *What* and the new decision *The spec author writes the resource texts in every lane* |
| 11 | F2 | Change doc: *Why* (the RH5201 run would not have qualified), and *No test path in the wording lane* (states the departure from "unless a new test", names the 16 rules, says the PR names it) |
| 12 | F4 | `squad/lead.md` *Pushes*: a new point, once at the end of each role's stage after its handover commit. The rationale is corrected: a lost session loses at most one stage's commits, and these pushes wake no session before §9. `squad/README.md` *Handovers are files*: "from its last push". Change doc: *What* and *Stage boundaries …* rewritten |
| 13 | F3 | `squad/roles/cleaner.md`: "Every lane except docs and wording" |
| 13 | F5 | `scripts/README.md` *The line-count rows* and the change doc's decision: the filter is described as what it selects (2 analyzer rows, 4 ruleset rows, the formatter rows of the files named after the rules, and no `LineCountSelfHostingTests` method). The filter itself is unchanged |
| 13 | F6 | `sdd/templates/follow-up-issue.md:19`: `wording` added. Line 104 (variant *Internal*) is left unchanged on purpose. That variant means no change of intended behavior, and a wording change alters a visible text, so it cannot be an internal follow-up. `behavior-change` is missing from that line for the same reason |
| 14 | internal choice 1 | `scripts/README.md` *The text-only proof* and *The lane check*: `lane-check.sh` asks the classifier in `Reihitsu.Tooling` for the wording paths and holds no path list of its own. Change doc: *What* and *The proof excludes the wording paths* |
| 15 | skeptic's `Covers` note | `squad/README.md` *Lanes*: a `tests` or `bug-fix` claim covers a wording diff, so it can pass a diff that holds a change doc. A change doc without a wording path still needs `behavior-change`. Change doc: *What* |
| 16 | internal choice 5 | Change doc: new decision *The subscription also ends when every remaining failure exists on `main`*, naming it as a reading of the maintainer's rule for the PR. `squad/lead.md` §9 already stated the behavior |
| 17 | — | `Status: Accepted` |

For the implementer (decision 14): `scripts/README.md` now says that `lane-check.sh` gets the wording paths from the classifier. How it gets them is left open; one option is an extra query of `lane-check.cs` before the proof runs.

`scripts/lane-check.sh behavior-change --head worktree`:

```
LANE CHECK: confirmed — claimed behavior-change, diff needs only tooling
```

## Examples for the tests

None. No requirement changed. The classifier cases in `01-spec-author.md` and the skeptic's candidate table stand unchanged.

## Output questions

_None._ Nothing an end user sees changes.

## Internal choices

1. The spec author's ownership of resource texts in every lane covers only the changed `<value>` texts of existing entries in the two wording resource files. The entries of a new rule, with their wrapper properties, stay with the implementer (`squad/roles/implementer.md`, step 5, unchanged). The alternative was that the spec author writes every value in the two files, new entries included. That would split a new rule's `.resx` entry from its wrapper property, which is code.
2. In the wording lane, the spec author writes the whole text change and then reports `needs a test`, naming each quoting test. Previously it stopped before writing. This follows decision 10.
3. F6: the *Internal* follow-up variant's lane list stays without `wording`, as explained in the table above.

## Follow-up candidates

_None new._ Candidates 1 and 2 in `00-lead.md` stand. Candidate 1 is now also cited by the change doc as the way the wording lane takes effect for the 16 rules whose message a test quotes.

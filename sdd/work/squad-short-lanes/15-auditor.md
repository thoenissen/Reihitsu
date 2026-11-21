Role: auditor
Commit: 4982c768074e4414c300b7ff5ad8a524d2bb2b65
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Repair delta against 31a7647 (repair round 1), head 4982c76, checked out with a clean tree. `origin/main` and `git ls-remote origin refs/heads/main` are both d63eab4bee09632526c02bfb4a46f54aabdd8001, which is an ancestor of the head. The diff 31a7647..4982c76 outside `sdd/work/` touches five files: `scripts/README.md` (one sentence), the change doc (two sentences), `squad/lead.md` §4 (one row), `squad/roles/spec-author.md` (*Wording* bullet), and `LaneCheckCommandTests.cs` (one fixture line and its summary). That matches handovers 11–14 and decisions 23 and 24. The repair stayed in the surfaces the first audit covered and changed no code, so this is a delta audit, not a full one.

Previous findings:
- **#1 — closed.** The fixture line is now `M\tsquad/notes/README.md`. The guard's `NotesReadme` exemption allows that path, and `LaneClassifierTests` row `squad/notes/README.md → Text` pins its category. The line classifies as `Text` as before, so every expected value still holds. It still exercises the first-row precedence (`squad/notes/**` text before `squad/**` tooling): if that precedence broke, the wording-claim tests that use the constant would fail. No added line outside `sdd/work/` and `squad/notes/` names a note any more (`git diff origin/main..HEAD | rg 'notes/[0-9]'` finds nothing). 11-test-implementer ran the architecture project: 198 passed.
- **#2 — closed, member open as #4.** The restatement is gone from `squad/lead.md` §4 and from both change-doc sentences. They point to `scripts/README.md`, *The lane check*. The member #2 named (no change doc, a C# comment edit outside the test projects, a changed literal) is now stated correctly: rule 2 gives `bug-fix` or `behavior-change`. The new consequence sentence fails on a member #2's enumeration did not name; see #4.
- **#3 — closed.** The *Wording* bullet now names `scripts/lane-check.sh wording --head worktree` as the source of the `TEXT-ONLY PROOF` line. `lane-check.sh` prints that line with the wording `--exclude`s applied (`grep '^TEXT-ONLY PROOF:'`), plus the `LANE CHECK` line. The bullet's reason, that the bare proof always fails on a resource file, matches 05-implementer demonstration 2. The docs-lane bullet is unchanged and still correct.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `scripts/README.md`, *The lane check*, sentence after rule 5 ("A changed test fails the text-only proof, so a C# file outside the test projects … then counts as production"); change doc *No test path in the wording lane* ("a changed test also fails the text-only proof") | FC-015 (a prose prediction of the lane disagrees with the classifier and proof for one member) | #2 | PR-introduced | The sentence says every changed test fails the proof, which is not true. The proof judges each changed C# file. An added file is behavioral (`verify-text-only.cs:399`, "added C# file"), and so is a changed literal. A modified test file whose only change is a comment is proven text-only, which is the proof's whole purpose. Counterexample: a wording run without a change doc where the spec author reports `needs a test` for a test that quotes the title only in a comment, next to an analyzer comment edit that quotes it. Once the test implementer updates that comment, the proof passes and the analyzer `.cs` is `Text`. Rule 4 then gives `tests`, but the sentence predicts `bug-fix`, and the lead's §4 row sends the lead to that sentence. The lead widens further than needed and adds the implementer and auditor. That costs roles but breaks nothing, because a wider claim covers the diff, so it stays low. Members of the class (test change × C# file outside the test projects × change doc): changed literal or added test, with that file → `bug-fix`/`behavior-change`, which holds. Any test change without that file → `tests`/`behavior-change`, which holds. Comment-only test change with that file → predicted `bug-fix`/`behavior-change`, actual `tests`/`behavior-change`, which fails in both cells. Rules 1–5 and `TryGetRequiredLane` are right; only the derived sentence is wrong. | Keep rules 1–5 as the one statement and state the cause exactly: "a test change the proof does not prove text-only, such as a changed literal or an added test file". Alternatively, drop the derived consequence and let the rules alone decide. Make the same edit to the change-doc sentence. |

`pass, text cleanup`: finding 4 sits only in Markdown prose, `scripts/README.md` and the change doc. It cannot reach the classifier, the proof, the shim, or any test. The lane check itself returns the right lane in every member.

## Guard scope
_N/A — the repair changes no predicate, guard, or exemption. `LaneClassifier`, `LaneCheckCommand`, `verify-text-only.cs`, and `lane-check.sh` are byte-identical to 31a7647, so the first audit's guard-scope table stands._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Lane a widened wording diff needs | `LaneClassifier.TryGetRequiredLane` (code); `scripts/README.md` rules 1–5 (one statement) plus its new consequence sentence; `squad/lead.md` §4 row (now a pointer); change doc *No test path…* and *The spec author writes…* (now pointers, but one carries the same proof claim); `squad/README.md` *Lanes* wording bullet ("which the lane check names", a pointer) | The restatements #2 named are gone. The consequence sentence inside the owning file overstates the proof (#4) |
| Which command proves a wording-lane comment edit | `squad/roles/spec-author.md` *Wording* (`lane-check.sh wording --head worktree`); `scripts/README.md` *verify-text-only* `--exclude` paragraph (the lane check passes the exclusions) | agree |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LaneCheckCommandTests` fixture `WordingChangesWithChangeDoc` (used by the wording-claim verdict tests) | A wording diff with a change doc, spec text, a `squad/notes/` file, and a C# comment edit (proof passing) confirms `wording` and the wider claims | `squad/notes/README.md` classified as tooling (a lost row precedence) would turn the diff into `behavior-change`, or into no lane, and fail the `wording` confirmations | `Run`, verdict line | ok. Same strength as before the repair; the summary ("a squad notes edit") describes the line |

## Failure classes dismissed
- FC-001 to FC-014, FC-016, FC-018 to FC-030, FC-033 to FC-036, FC-038 to FC-044: the repair changes Markdown process text and one test fixture string. It touches no formatter phase, analyzer, code fix, predicate, or proof code, so the dismissals of 10-auditor stand unchanged.
- FC-017: the fixture's assertions are unchanged and as strong as before; see the table.
- FC-031: the one changed summary (`WordingChangesWithChangeDoc`) describes the new line.
- FC-032: closed by #1. The architecture project now ran on the repaired tree (11-test-implementer, 198 passed).
- FC-037: the *Wording* bullet now names the proof invocation that carries the exclusions (#3 closed).
- FC-015: applies, finding 4.

Also checked: none of the changed lines holds a `TODO` or a tracker number. No visible surface changed (check 10 has nothing to cover). No special case (check 11 does not apply). Scope: every changed path is one decisions 23 and 24 route.

## Architecture findings
Finding 4 repeats #2's class. The decision is which lane a wording run that gains a test needs. The two components are `LaneClassifier` with the proof, and a prose prediction of their result. Decision 23 removed the restatements outside the owning file. The repair then added a derived consequence inside `scripts/README.md` that predicts the proof's verdict for "a changed test" and is wrong for a comment-only test change. Rules 1–5 already decide every member correctly, so the class goes away once no prose predicts what the proof will say about a test change. Either name the exact cause (a test change the proof does not prove text-only) or drop the consequence sentence.

## Hints
- The two hints of 10-auditor (the "wrapper properties" wording for a new rule's entries, and stray stdout lines from the `--wording-paths` call becoming `--exclude`s) are unchanged and still not findings.

## Follow-up candidates
_None._ The two in `00-lead.md` stand.

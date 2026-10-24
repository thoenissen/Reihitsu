Role: auditor
Commit: b2541e1
Verdict: pass, text cleanup

My prompt named the issue, slug, lane, handovers, head, and base. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1441 at head b2541e116eedbd059c6944acd63cf939156e8603 against base 1b9a0286b3098a48b560069f290a4e904630347f.

- State: `git rev-parse HEAD` = b2541e1. `git ls-remote origin refs/heads/main` = 1b9a028, and `git merge-base --is-ancestor 1b9a028 HEAD` holds. The diff matches the handovers: `sdd/README.md`, `sdd/templates/follow-up-issue.md`, `squad/lead.md`, plus `sdd/work/follow-up-issue-format/00`–`03`. Nothing under `.github/`, `sdd/specs/`, `src/`, or any test project changed.
- `scripts/lane-check.sh tooling --no-install` on the head printed `TEXT-ONLY PROOF: PASS — base=1b9a028 head=b2541e1 — 7 changed file(s) (7 non-compiled, 0 C# comment/layout-only)` and `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- Every requirement of the issue's *Wanted* list is present:
  - three variants with the listed fields (spec bug: requirement ID and wording, input, today's output, required output, affected, blocked by, done when; clarification: spec path, behavior with example, covering test, done when; internal: what and why, files or areas, done when);
  - the common title convention, `Lane:`, optional `Principle:`, and the self-contained rule;
  - the statement that behavior changes and new rules use the public feature-request forms;
  - the `sdd/README.md` layout pointer and the `squad/lead.md` *Follow-ups* pointer;
  - `.github/ISSUE_TEMPLATE/` unchanged.
- Every reference resolves:
  - `../README.md#the-formatter-may-do-more-than-the-analyzers-p11` and `../../squad/README.md#lanes` resolve from `sdd/templates/`;
  - the in-page anchors `#spec-bug`, `#clarification`, and `#internal` match their headings;
  - the lane names match `scripts/lane-check.sh:4,65`;
  - the spec prefixes `FMT-BLANK` and `CLI` exist in `sdd/specs/README.md:12,392`;
  - `analyzer_feature_request.yml` and `formatter_feature_request.yml` exist.
- The new file uses LF line endings, ends with a newline, and has no trailing whitespace, like its sibling templates.

## Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | low | `sdd/templates/follow-up-issue.md:26` | — (documentation precision) | PR-introduced | The sentence lists the public form's fields as "the reproducible example, the expected behavior, and the actual behavior". The skeleton's three code blocks are, in order, *Input*, *Today's output*, *Required output*. Read in order, the sentence maps *Today's output* to "expected behavior" and *Required output* to "actual behavior", which is reversed. Example: a draft carried over into `analyzer_bug_report.yml` by position puts the wrong output under "Expected Behavior". | Name each pair: *Input* is the reproducible example, *Required output* the expected behavior, *Today's output* the actual behavior. |
| 2 | low | `sdd/README.md:36` | FC-015 (partial restatement) | PR-introduced | The rulebook bullet routes spec bugs, clarifications, and internal work to the template, and behavior changes and new rules to the feature-request forms. It omits the third case that both the template (`sdd/templates/follow-up-issue.md:14`) and `squad/lead.md:84` state: a bug in behavior no spec covers uses the public bug-report form. A reader of the rulebook alone finds no format for that class: it is not a spec bug (no requirement), a clarification, internal work, or a behavior change. The other two owners agree with each other, so this is an omission, not a contradiction. | Either add the bug-report case to the bullet, or reduce the bullet to a pointer that leaves the routing to the template. |

Why `pass, text cleanup`: both findings are wording in Markdown files that no build, test, or analyzer reads. Neither file is compiled (TEXT-ONLY PROOF above: 7 non-compiled files). No test reads `sdd/templates/` or the `sdd/README.md` layout section: `SpecCatalog` and the `Sdd` architecture tests read only `sdd/specs/`. So neither fix can reach compiled behavior, diagnostics, public API documentation, or rule metadata.

## Guard scope
No code predicate changed. The guards of this change are the routing criteria that pick a follow-up's format. Each was checked against the rule that owns the same decision.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Spec-bug variant | "The code differs from a requirement that a spec already states" (`follow-up-issue.md:7`) | P11 "a bug against the rule's spec when the spec already covers that form" (`sdd/README.md`, P11); "A bug is a difference between the code and a spec" (`sdd/README.md:7`) | Same boundary: a requirement must already exist | ok |
| Clarification variant | "A spec gains a requirement for behavior that already exists" (`:8`), and "When the current behavior looks wrong … an open question" (clarification intro) | `sdd/README.md`, *When a change doc is required* (clarification bullet), and *Open questions* ("No GitHub issue is filed for it") | Same wording; the looks-wrong exclusion matches the open-question rule, and no issue is proposed for an open question | ok |
| Internal variant | "no change of intended behavior" (`:9`); the `Lane:` placeholder excludes `behavior-change` | `sdd/README.md`, *When a change doc is required* (change doc only when intended behavior changes); `squad/README.md` lane derivation (refactoring under `src/` without a change doc is `bug-fix`) | Lane set `docs`, `tests`, `bug-fix`, `tooling` matches every path class that carries no change doc | ok |
| Behavior change or rule's own gap → feature-request form | "a gap in a rule's own construct whose form the rule's spec does not cover, and a new-rule candidate" (`:13`) | P11 "otherwise a behavior change of the rule"; "a new-rule candidate" | Same qualifier ("form the spec covers"); a rule with no spec falls under "does not cover", which P11 also calls a behavior change | ok (overlap with `:14` noted under Hints) |
| Bug no spec covers → public bug-report form | "A bug in behavior that no spec covers yet" (`:14`) | `squad/lead.md:84` "a bug in behavior no spec covers"; `sdd/README.md:36` silent | Template and lead playbook agree; the rulebook omits the case | Finding 2 |
| Clarification lane `tests` | `Lane: tests` default | `squad/README.md`: "A docs-lane diff cannot add a requirement to an enforced spec … that run widens to tests"; `sdd/README.md`: exemption list empty, so every spec is enforced | Agree | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which format a follow-up issue is drafted in | `rg -n -i --hidden "ISSUE_TEMPLATE\|follow-up issue\|follow-up candidate\|issue template"` over the repository (`sdd/work/` and `src/*.cs` excluded): `sdd/templates/follow-up-issue.md:3-14` (full), `squad/lead.md:84` (full, parity with the template), `sdd/README.md:36` (partial). Unchanged mentions that say when a follow-up is created, not its format: `sdd/README.md:117,118,123`, `AGENTS.md:147`, `CONTRIBUTING.md:7,45` (human contributors, public forms), `.github/PULL_REQUEST_TEMPLATE.md:17` (`Follow-up work` section), and `sdd/changes/2026-10/introduce-sdd.md` (history). Nothing under `.claude/` or `squad/decisions/`. | Finding 2 for `sdd/README.md:36`; the other owners agree |
| Lane names | `scripts/lane-check.sh:4,65`, `src/Reihitsu.Tooling/LaneClassifier.cs`; table labels in `squad/README.md`, *Lanes* | The template spells the lanes as the lane check takes them and links the table. It restates no derivation rule. Agree |
| When a change doc is required for a clarification | `sdd/README.md`, *When a change doc is required* | The template cites the rule by section and restates nothing beyond it. Agree |
| P11 classification | `sdd/README.md`, P11 (owner); restated in `squad/roles/skeptic.md:23` and `follow-up-issue.md:13` | Same split (the rule's own gap by spec coverage; a different construct is a new-rule candidate). Agree |
| List of SDD templates | `sdd/README.md:12` (layout tree); `ls sdd/templates` = `analyzer-spec.md`, `change.md`, `follow-up-issue.md`, `spec.md` | Agree |

## Assertion adequacy
_N/A — the run adds and changes no test. The change is Markdown only, adds no tool with tests, and no test reads `sdd/templates/` (`rg` over `src/Reihitsu.ArchitectureTests` and `src/Reihitsu.Tooling*`: the `Sdd` tests enumerate only `sdd/specs/`)._

## Failure classes dismissed
- FC-004, FC-005, FC-006, FC-007, FC-008, FC-009, FC-010, FC-011, FC-012, FC-013, FC-014, FC-018, FC-019, FC-020, FC-021, FC-022, FC-023, FC-024, FC-025, FC-026, FC-027, FC-028, FC-029, FC-030, FC-036: the diff contains no C# file and no compiled file (TEXT-ONLY PROOF above). It changes no analyzer, code fix, formatter phase, or rename.
- FC-001, FC-002, FC-003: no code predicate exists. Their analogue for routing criteria was traced under *Guard scope*. Every qualifier ("already states", "already exists", "whose form the rule's spec does not cover", "no spec covers yet") is explicit in the text.
- FC-015: checked. Finding 2.
- FC-016: no sibling owner of the drafting rule outside the three changed files (rg above, `.claude/` included with `--hidden`).
- FC-017, FC-033, FC-034, FC-035: no test and no reproduction in this run.
- FC-031: the one rewritten paragraph (`squad/lead.md:84`) and the rewritten layout line (`sdd/README.md:12`) describe the new state. The paragraph's unchanged sentences (search, approval, `Follow-up work` link, P11 candidates, no joining the run) still hold.
- FC-032: no compiled change after the cleaner's Release build at a943596. The commits since then are handovers only (`git diff --stat` above).
- FC-037: text-only is proven by `verify-text-only` through the lane check, not by a line-based grep.

## Hints
- **Unspecified rule, P11 own gap.** For a rule with no spec, a gap in its own construct matches both `follow-up-issue.md:13` (behavior change, feature-request form) and `:14` (bug no spec covers, bug-report form). P11 settles it as a behavior change, and `:13` is the more specific line, so a careful reader lands right. An explicit "for a rule without a spec, P11 decides" would remove the overlap. Uncertain observation, not a finding.
- **Playground has no public form.** `.github/ISSUE_TEMPLATE/` has analyzer and formatter forms only. The formatter forms cover the CLI ("formatter or CLI behavior"), but the Playground, a specified shipped surface (`sdd/specs/playground/playground.md`), has neither a bug nor a feature form. So "the public … form of their surface" (`follow-up-issue.md:13,14`) has no target for a Playground behavior change or unspecified Playground bug. This is pre-existing: the old `lead.md:84` ("the matching `.github/ISSUE_TEMPLATE/` form") had the same gap, and this run must not change `.github/`. Follow-up candidate, not a finding.
- **Version fields for an unspecified bug.** The implementer drops the public forms' version fields from the spec-bug variant (decision 5) because "a follow-up is found in the repository tree, not in a released build". The same reasoning applies to an unspecified bug, which decision 1 routes to the public bug form, where `Analyzer Version` / `CLI Version` are required fields. This is pre-existing practice (every follow-up used the public forms before this change), and those drafts still lack `Lane:` and `Blocked by`. It is a maintainer choice, not a defect of this diff.
- **"coordinator".** `follow-up-issue.md:3` names "a coordinator" as an author. The word comes from the issue text, but no repository document defines it (`rg -n -i coordinator` finds only this line).
- **Implementer decisions 1–6 (`01-implementer.md`), weighed:**
  - Decision 1 (unspecified bug → public bug form): matches the issue's own enumeration ("bugs against a spec requirement, spec clarifications, internal work") and the pre-change practice.
  - Decision 2: matches P11 (see *Guard scope*).
  - Decision 3: the names are the same lanes as `squad/README.md`, spelled as the tool takes them, with the section linked. This satisfies "one of the lanes in `squad/README.md`".
  - Decision 4: the issue asks for a convention but names none, and nothing contradicts the chosen one.
  - Decision 5: the correspondence is right in substance. Finding 1 concerns only its wording.
  - Decision 6: no labels are required by the issue.

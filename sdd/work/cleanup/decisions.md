# Cleanup of `squad/decisions/`

Classification of the 65 entries in `squad/decisions/` (README.md excluded) for the one-off cleanup that deletes every entry and folds lasting content into rules. Every (a) was checked against the files on disk at `5ec1475`, by content rather than by ID: that commit dropped the retired requirements and renumbered RH3202-005 → RH3202-002 and RH5002-004 → RH5002-002, so some IDs that entries cite (RH3202-002..004, RH5030-003) no longer exist. Shared specs do not label principles by number (`P12`, `P22`, …); a principle is verified by the requirement that carries its text.

- **(a) already a rule** — the answer's text is written into a spec, the rulebook, the playbook, a role file, or the failure-class catalog.
- **(b) recurring answer not yet written** — folded into a draft rule B1–B4 below.
- **(c) one-off** — an answer for one run (filing, renaming, approval, scope) with nothing to carry forward.

## 1. Summary

| Class | Count |
|---|---|
| (a) already a rule | 36 |
| (b) recurring, not yet written (→ B1–B4) | 19 |
| (c) one-off | 10 |
| **Total** | **65** |

Draft rules: B1 (merge after a passing audit, 2 entries), B2 (further repair rounds, 8 entries; 5 more escalation-7 answers count as instances), B3 (claim only what is verified, 2 entries; both also B2 instances), B4 (decided behavior the code does not follow yet, 7 entries). B5 is a generalization that was considered and is recommended against; it replaces no entry.

## 2. Classification

| Entry | Class | Where it lives now (a) / draft rule (b) / reason (c) |
|---|---|---|
| 2026-10-18-rh6002-003-why-left-unchanged.md | a | `squad/README.md`, *Rules for every role*: "Edit nothing outside the run's scope; an out-of-scope find goes into your handover as a follow-up candidate." |
| 2026-10-18-rh8107-001-keeps-original-text.md | a | `sdd/specs/analyzer/RH8107.md`: RH8107-001 ends "The documentation comment is the first one in the member's leading trivia"; `Uses` is `SHARED-DOC-008` only |
| 2026-10-18-rh8107-shared-doc-reference-clauses.md | a | `sdd/specs/analyzer/RH8107.md`: RH8107-004 "as SHARED-DOC-008 specifies", `Uses` row (the RH8107-001 part was superseded by the entry above) |
| 2026-10-18-shared-diag-003-comment-subject-rules.md | a | `sdd/specs/shared/diagnostics.md`, SHARED-DIAG-003: "This requirement restricts only rules that check code. A rule whose subject is the text of a comment or of a directive …" |
| 2026-10-18-shared-diag-003-literal-value.md | a | `sdd/specs/shared/diagnostics.md`, SHARED-DIAG-003: "What the text of a literal protects is its value …"; the fix side is a non-goal pointing to `code-fixes.md` |
| 2026-10-18-shared-doc-coverage-and-extension-block.md | a | `sdd/specs/shared/element-documentation.md`: SHARED-DOC-001 (extension block among the containers; partial elements in several parts not covered), SHARED-DOC-002 (extension block in the private group), SHARED-DOC-005 (partial elements not covered) |
| 2026-10-18-shared-fix-003-layout-fields-endregion.md | a | `sdd/specs/shared/code-fixes.md`, SHARED-FIX-003, bullets **Run time** (sequential layout, not Explicit) and **Comment, directive, and literal text** (whitespace inside a comment is content; `#endregion` comment renamed with the region) |
| 2026-10-18-shared-fix-003-own-reported-text.md | a | SHARED-FIX-003, **Comment, directive, and literal text**: "Text that a diagnostic of the fix's own rule reports may change." |
| 2026-10-18-shared-fix-003-rename-not-runtime-change.md | a | SHARED-FIX-003, **Run time**: "The new name or the new accessibility that the fix exists to produce does not count" |
| 2026-10-18-shared-fix-003-skeptic-scope.md | a | SHARED-FIX-003, **Run time** and **Comment, directive, and literal text** (doc-comment name references, layout vs. words, field order); item 4 by the SHARED-DIAG-003 non-goal "Whether a code fix may change the text of a literal or of a comment — code-fixes.md" |
| 2026-10-18-using-rules-name-by-value-filing.md | c | Filing of one case under an existing bug issue |
| 2026-10-19-1069-post-audit-merge.md | b | B1 |
| 2026-10-19-997-blank-line-code-fix-utilities-owner.md | a | `sdd/specs/README.md` row `shared/blank-line-after-brace-fixes.md` (SHARED-AFTERBRACE, owner `BlankLineCodeFixUtilities`); RH5030 and RH3208 list SHARED-AFTERBRACE-001 under `Uses` |
| 2026-10-19-997-rh3201-uses.md | a | `sdd/specs/analyzer/RH3201.md`: RH3201-001/-002 state the `///` vs `////` test; `Uses` is `—` |
| 2026-10-19-997-rh3202-005-blank-lines.md | c | One open question filed with a characterization test (the procedure is `sdd/README.md`, *Open questions*); the requirement is now RH3202-002 |
| 2026-10-19-997-rh3202-shared-body.md | a | `sdd/specs/analyzer/RH3202.md` `Uses` SHARED-BODY-001..003; `shared/expression-body-conversion.md` covers methods (the retired IDs were dropped in `5ec1475`) |
| 2026-10-19-agents-sentence-and-auto-layout.md | a | `AGENTS.md`, *Code-fix coverage*, third bullet (the SHARED-FIX-003 sentence); SHARED-FIX-003 **Run time** ("unless it declares `LayoutKind.Auto` or `LayoutKind.Explicit`") |
| 2026-10-19-audit-findings-1069-tracking.md | c | Tracking of two pre-existing defects in issues |
| 2026-10-19-audit-repair-doc-layout-followups.md | a | Item 2: SHARED-FIX-003 "in a documentation comment, also the `///` exterior and the whitespace and line breaks next to an XML tag". Item 1 is a B2 instance, item 3 one-off follow-ups |
| 2026-10-19-rh8202-uses-rh8107-fix-requirements.md | a | `sdd/specs/analyzer/RH8202.md` `Uses` RH8107-006, RH8107-007; `sdd/README.md`, *Spec format*, `Uses` = "the requirement IDs of other specs" |
| 2026-10-20-996-auditor-block-extra-repair.md | b | B2 |
| 2026-10-20-996-rh2001-002-second-spec-check.md | b | B2 (the RH2001-002 `field` wording is in the spec) |
| 2026-10-20-996-shared-equal-002-bounded-list.md | b | B3 (also a B2 instance); the bounded list is in SHARED-EQUAL-002 |
| 2026-10-20-996-test-only-audit-findings.md | b | B2 |
| 2026-10-20-997-rh3001-parentheses-claim.md | b | B3 (also a B2 instance); RH3001-004 keeps only the positive claim |
| 2026-10-20-997-rh3001-shift-operators.md | b | B2 (the `>>`/`>>>` exclusion is in RH3001-004) |
| 2026-10-20-997-rh3001-targeted-fix.md | b | B2 |
| 2026-10-20-997-second-repair-round.md | b | B2 |
| 2026-10-20-997-shared-prefix-collision.md | c | One rename of IDs that had never reached `main` |
| 2026-10-20-997-test-findings-spec-corrections.md | c | Corrections to the run's own new specs; RH3103-003 has since been rewritten |
| 2026-10-21-1139-shared-comment-001-contradicting-tests.md | b | B4 (the CS1587 sentence is in SHARED-COMMENT-001) |
| 2026-10-21-1260-contradicting-characterization-tests.md | b | B4 |
| 2026-10-21-1260-diag004-weak-coverage.md | b | B4 (superseded in method by the later 1380/1415 answers; see B4) |
| 2026-10-21-1260-item19-placement.md | a | `sdd/specs/shared/type-regions.md`, SHARED-TYPEREGION-006: "A rule MUST NOT compare the descriptions of two regions that lie in different branches …" |
| 2026-10-21-1260-no-new-questions.md | c | Decision not to file two questions |
| 2026-10-21-1260-p22-fix-reading.md | a | SHARED-DIAG-008: "A fix that leaves the directive and its line in place, such as removing the comma in front of it, stays offered." |
| 2026-10-21-1260-principle-instances.md | c | Filing of found instances (the "decided by" marking is part of B4) |
| 2026-10-21-1260-razor-content-no-conforming-shape.md | b | B4 (the Razor sentence is in SHARED-DIAG-003) |
| 2026-10-21-1260-using004-item18.md | b | B4 (the item-18 amendment is in SHARED-USING-004) |
| 2026-10-22-1139-p12-scope-rh5304.md | a | `sdd/specs/analyzer/RH5304.md` `Uses` SHARED-COMMENT-011 |
| 2026-10-22-1139-p12-scope-rh53xx.md | a | `Uses` rows of RH5301, RH5302, RH5303, RH5305, RH5306, RH5308 list SHARED-COMMENT-011 |
| 2026-10-22-1139-post-audit-merge.md | b | B1 |
| 2026-10-22-1260-diag008-auditor-findings.md | a | SHARED-DIAG-008: "A `#region`, `#endregion`, or `#pragma` directive inside the judged span MUST NOT by itself keep the construct from being reported; a rule's own requirement may still leave such a shape unreported, as SHARED-JOIN-001 does" |
| 2026-10-22-1260-diag008-spec-checker-findings.md | c | Filing of instances and two readings for single specs (RH3001, RH7003) that changed nothing |
| 2026-10-22-1331-p26-reach.md | a | SHARED-FIX-007, **Moved code** bullet: "The bullet covers … It does not cover an expression that a fix turns into a new statement … nor a using block …" |
| 2026-10-22-1331-p30-reach.md | a | `shared/element-documentation.md`, SHARED-DOC-008, both paragraphs (RH8001–RH8309 gated; RH8401, RH8402, RH8501 in every mode) |
| 2026-10-22-1380-p37-shared-side-deferred.md | b | B4 (the formatter side is FMT-BLANK-016 in `formatter/blank-lines.md`) |
| 2026-10-22-1403-batch-roles-and-change-doc.md | c | Answer 1 superseded by 2026-10-23-1403-same-lane-batches (now `squad/README.md`, *One lane*); answer 2 kept one change doc as is |
| 2026-10-22-1403-multi-issue-run-design.md | a | `squad/README.md`, *Several issues in one run* (one agent per role, one section per issue, an escalation stops the whole run, change docs, scope measured against the batch) and escalation 11; answer 3 superseded by *One lane* |
| 2026-10-22-939-playground-component-tests.md | c | One dependency approval; its result is the `bunit` reference in `Reihitsu.Playground.Test.csproj` |
| 2026-10-23-1331-second-repair-round.md | b | B2 |
| 2026-10-23-1331-third-repair-round.md | b | B2 (the resulting SHARED-DOC-004/-DIAG-004 text was later superseded by the P39 wording) |
| 2026-10-23-1380-p12-chain-exemption.md | a | SHARED-COMMENT-011: "A requirement that keeps the rule from reporting because a comment stands in that place still applies: under SHARED-CHAIN-004, RH5112 does not report …" |
| 2026-10-23-1380-p34-family-scope.md | a | `formatter/pipeline.md`, FMT-PIPE-014, second paragraph ("This requirement does not override the layout a construct's own requirement chooses …") |
| 2026-10-23-1403-same-lane-batches.md | a | `squad/README.md`, *One lane* and escalation 10; `squad/lead.md` §1 steps 1 (a reference inside other text is asked about) and 5 |
| 2026-10-23-1403-second-repair-round.md | a | `squad/lead.md` §1 step 1 ("Only these references select issues; other text in the prompt is context …"); `squad/README.md`, *The prompt sets the batch*; the per-issue widening was superseded. Its repair-round answer is a B2 instance |
| 2026-10-23-1415-conforming-examples.md | b | B4 |
| 2026-10-23-1415-p35-fix-scope.md | a | SHARED-FIX-008: "The code it changes is the span the fix replaces, rewrites, or moves, its trailing trivia included … A line break outside that span … is not in the scope of the fix" |
| 2026-10-23-1415-p38-validity-reach.md | a | SHARED-DIAG-011: "… or because the position itself rules it out with a compile error …" with the RH3003 and RH2004 examples |
| 2026-10-23-1415-p39-field-split.md | a | SHARED-FIX-003, fifth condition and **Documentation** bullet ("or splits a declaration with several declarators and keeps the comment on one part only"); Why: "no fix may lose it … the user rearranges such code by hand" |
| 2026-10-23-1419-fc013-layout-rules.md | a | `squad/failure-classes.md` FC-013; `squad/roles/skeptic.md`, question 3 |
| 2026-10-23-1419-new-rule-candidate-route.md | a | `squad/lead.md`, *Follow-ups* ("A new-rule candidate … is a follow-up candidate like any other"); `squad/roles/skeptic.md` handover `## Follow-up candidates` |
| 2026-10-23-1424-fc014-formatter-owned-concerns.md | a | `squad/failure-classes.md` FC-014; `squad/roles/skeptic.md`, question 3 |
| 2026-10-23-998-change-doc-approved.md | a | `sdd/README.md`, *Tests and requirements* (a bug's reproduction tags the requirement); `squad/roles/spec-checker.md` check 8; `squad/README.md` role table (spec checker row); change doc `sdd/changes/2026-10/requirement-coverage-opt-out.md` |
| 2026-10-23-998-untagged-tests-allowed.md | a | `sdd/README.md`, *Tests and requirements* (opt-out coverage, untagged tests allowed) and *Checks*; `squad/roles/test-implementer.md`, *Requirement tags*; `squad/roles/spec-checker.md` check 8 |

## 3. Draft rules

### B1 — Merge after a passing audit without a second audit when only additive files overlap

**Target.** `squad/lead.md`, §5 *Before the auditor*, the last paragraph, which today reads "If `origin/main` moves after a passing audit, check whether the new commits touch anything the PR touches. If they do, ask the maintainer whether to merge and re-audit or to rely on CI."

**Wording** (replaces that paragraph):

> If `origin/main` moves after a passing audit, check whether the new commits touch anything the PR touches. When they overlap only in files to which both sides add independent entries — the requirement-ID ledger, or a test file to which both sides added tests — merge, have the role that owns the file resolve the conflict by keeping both sides, run the cleaner, and rely on CI: no second audit and no local validator run on the merged tree. The run report names the checks that did not run locally. When the new commits change code, spec text, or a test that the PR also changes, ask the maintainer whether to merge and re-audit or to rely on CI.

**Replaces.** 2026-10-19-1069-post-audit-merge.md, 2026-10-22-1139-post-audit-merge.md.

**Example.** After the audit of a run passes, `main` gains a spec batch whose only overlap with the PR is `sdd/specs/requirement-ids.txt`. The lead merges, the spec author keeps both sides' lines in the ledger, the cleaner runs, and the PR is published; the run report says "re-audit and validator not run on the merged tree; CI covers them". `SpecStructureTests` in CI still checks that the ledger is sorted and complete.

**Recommendation: adopt.** Both times the maintainer gave the same answer, and CI runs the architecture tests that catch a broken ledger or test file.

### B2 — A further repair round does not need the maintainer, up to a limit

**Target.** `squad/README.md`, *Escalations* item 7 and *Repairs*; `squad/lead.md` §6 *Repairs* and *Never*.

**Wording.**

- `squad/README.md`, escalation 7 becomes:
  > 7. the spec checker or the auditor still blocks after the third repair round, or a repair would need a choice the maintainer has not made — narrowing a requirement or dropping its claim, a design change, more scope;
- `squad/README.md`, *Repairs*, replace "A second block is escalation 7." with:
  > A block after a repair round starts another round in the same way when every finding is `same requirement` or `PR-introduced`: the owners repair, then the cleaner, the spec checker, the auditor on the repair only, and the validator run. A run has at most three repair rounds; a block after the third is escalation 7.
- `squad/lead.md` §6, replace "A second block is escalation 7." with:
  > A further block starts another repair round in the same way, up to three in a run (`README.md`, *Repairs*); a block after the third, or a finding whose repair needs a choice the maintainer has not made, is escalation 7.
- `squad/lead.md`, *Never*: "Start a second repair round, or publish while the auditor or validator blocks." becomes "Start a fourth repair round, or publish while the auditor or validator blocks."

**Replaces.** 2026-10-20-996-auditor-block-extra-repair.md, 2026-10-20-996-rh2001-002-second-spec-check.md, 2026-10-20-996-test-only-audit-findings.md, 2026-10-20-997-second-repair-round.md, 2026-10-20-997-rh3001-targeted-fix.md, 2026-10-20-997-rh3001-shift-operators.md, 2026-10-23-1331-second-repair-round.md, 2026-10-23-1331-third-repair-round.md; also the repair-round parts of 2026-10-20-996-shared-equal-002-bounded-list.md, 2026-10-20-997-rh3001-parentheses-claim.md (both B3), 2026-10-19-audit-repair-doc-layout-followups.md (item 1), and 2026-10-23-1403-second-repair-round.md (both (a)).

**Example.** Run #1331: round 1 repairs the spec checker's findings; the auditor then reports seven `PR-introduced` findings, so round 2 starts without asking; the auditor reports two findings in the repaired text, so round 3 starts; a block after round 3 would be escalation 7. In run #996, the bounded-list repair needed a choice (bound the list or fix three cases inside it), so it would still have been escalation 7.

**Recommendation: adopt.** Thirteen escalation-7 answers, every one "repair again" and none "publish as is" or "stop"; the limit of three keeps a run from looping. This changes an escalation, which only the maintainer may do (`squad/README.md`, *Escalations*).

### B3 — A requirement claims only what has been verified

**Target.** `sdd/README.md`, *Spec format*, a new bullet after "One requirement per heading"; `squad/roles/spec-author.md`, *Job* step 3 can point to it.

**Wording.**

> - **Claim only what is verified.** A requirement states the inputs it decides. It does not claim that no other input behaves otherwise — "only", "no other", "every form", "adds no parentheses anywhere else" — unless the code that dispatches on the input shows the list is complete. Where the set of inputs is open-ended, the requirement names the decided ones and states its reach in a coverage sentence, such as "This requirement specifies the outcome only for the forms it names and leaves every other form unspecified."; a form left out that looks wrong is filed as an open question.

**Replaces.** 2026-10-20-996-shared-equal-002-bounded-list.md, 2026-10-20-997-rh3001-parentheses-claim.md.

**Example.** SHARED-EQUAL-002 lists the comparer arguments that count as custom and as not custom, then says "This requirement specifies the outcome only for the argument forms the two lists name, and leaves every other form unspecified", instead of an exhaustive list that each audit round broke again. RH3001-004 states only where the fix adds parentheses and says nothing about where it adds none.

**Recommendation: adopt.** In two runs an exhaustive or negative claim cost two to three extra repair rounds before the maintainer bounded it, and the coverage-sentence form is already the house style of shared specs.

### B4 — Decided behavior that the code does not follow yet

**Target.** `sdd/README.md`, a new section after *Open questions*, titled `## Decided behavior the code does not follow yet`, plus "with the exception in [Decided behavior the code does not follow yet](#decided-behavior-the-code-does-not-follow-yet)" after the first sentence of *Open questions* ("A spec describes current behavior").

**Wording.**

> ## Decided behavior the code does not follow yet
>
> When the maintainer decides a principle that existing code does not follow everywhere, the requirement that records it states the decision in full, although a spec otherwise describes current behavior.
>
> - Its examples, and the tests tagged with it, use only shapes that follow it today.
> - Each shape that does not follow it is filed in the `SDD open questions: <spec path>` issue of the spec that owns the shape, marked as decided by the requirement, or in the follow-up issue that brings the code in line. The spec does not list it.
> - A test tagged with the requirement never asserts a shape that does not follow it: such a test, or the part of it that does, is removed. An untagged characterization test that pins the current behavior stays; its summary says that it pins current behavior and names the issue that tracks the decided behavior.
> - A new requirement for which no shape follows it yet is not written in that run: the follow-up that brings the code in line adds it with its example, its fix, and its tests. An amendment to an existing requirement whose unchanged part keeps its covering tests may be written without a test of its own.

**Replaces.** 2026-10-21-1139-shared-comment-001-contradicting-tests.md, 2026-10-21-1260-contradicting-characterization-tests.md, 2026-10-21-1260-diag004-weak-coverage.md, 2026-10-21-1260-razor-content-no-conforming-shape.md, 2026-10-21-1260-using004-item18.md, 2026-10-22-1380-p37-shared-side-deferred.md, 2026-10-23-1415-conforming-examples.md. It also states the "decided by" marking that 2026-10-21-1260-principle-instances.md, 2026-10-22-1139-p12-scope-rh53xx.md, and 2026-10-22-1331-p26-reach.md used when filing.

**Example.** SHARED-COMMENT-001 says that a `///` comment that documents nothing (CS1587) is an ordinary comment. Its example shows only shapes that already behave that way. RH5033 not reporting such a comment behind an attribute list is filed in `SDD open questions: sdd/specs/shared/comment-placement.md` as decided by SHARED-COMMENT-001, and the tagged tests that asserted it were removed. The shared side of P37 had no conforming shape at all, so only FMT-BLANK-016 was written, and the shared requirement comes with the fix in its follow-up.

**Note.** The weak-coverage answer for SHARED-DIAG-004 (write it and tag shapes that cannot fail) is the one answer the draft does not follow. The two later answers (1380 P37, 1415 conforming examples) deferred such a requirement to its follow-up, and the draft takes that later line. SHARED-DIAG-004 stays as written.

**Recommendation: adopt.** Seven answers across four runs give one consistent procedure, and it contradicts the current sentence "A spec describes current behavior", so it has to be written down for the next principle run.

### B5 — A shared principle does not override a rule's own narrower requirement (considered, not proposed)

**Target.** It would have been `sdd/README.md`, *Layout*, in the bullet on shared specs.

**Wording.**

> A shared requirement that states a principle leaves in place a rule's own requirement that decides the same shape more narrowly; the shared requirement names such an exception.

**Replaces.** No entry: 2026-10-23-1380-p12-chain-exemption.md, 2026-10-22-1260-diag008-auditor-findings.md, and 2026-10-23-1380-p34-family-scope.md are (a), since each exception is written into SHARED-COMMENT-011, SHARED-DIAG-008, and FMT-PIPE-014.

**Example.** The exemption in SHARED-CHAIN-004 keeps RH5112 silent although SHARED-COMMENT-011 would report the line.

**Recommendation: drop.** The answers disagree. In 2026-10-22-1260-diag008-spec-checker-findings.md and 2026-10-21-1260-principle-instances.md, a rule's own narrower clauses (RH5111-005, SHARED-ATTR, SHARED-USING-009) were filed as instances of the principle instead of being kept, so the general sentence would decide future cases wrongly.

## 4. References to decisions entries

`git grep -n -E "squad/decisions/|decisions/20"` over the tracked files, excluding `squad/decisions/` and `sdd/work/`, plus the relative `decisions/` links that the same files use.

| File:line | Reference | Suggested replacement |
|---|---|---|
| `squad/README.md:5` | link "escalation answers are in [`decisions/`](decisions/)" | Remove that clause. |
| `squad/README.md:79` | "Every answer is recorded in `squad/decisions/` (see [`decisions/README.md`](decisions/README.md)) … a new rule is added only together with a decisions entry naming the failure it prevents." | Replace with: "The lead records every answer, summarized in English, in `00-lead.md` and in the run report. When the same kind of question keeps getting the same answer, the maintainer turns it into a rule — in a spec, `failure-classes.md`, or the playbook — and the escalation goes away. … The squad never tunes its own escalations, and a new rule's pull request names the failure it prevents." The maintainer has to decide where answers go once the folder is gone; this is the suggestion. |
| `squad/README.md:87` | `([decision](decisions/2026-10-22-1403-multi-issue-run-design.md))` | Remove the reference; the section itself is the rule. |
| `squad/README.md:92` | `([decision](decisions/2026-10-23-1403-same-lane-batches.md))` | Remove the reference; *One lane* is the rule. |
| `squad/lead.md:5` | "You write … decisions entries, …" | Remove "decisions entries". |
| `squad/lead.md:79` | "Record the answer, summarized in English, in `squad/decisions/` (see [`decisions/README.md`](decisions/README.md)) before continuing." | "Record the answer, summarized in English, in `00-lead.md` before continuing, and list it in the run report (§7 step 5)." |
| `squad/failure-classes.md:3` | "A new class is added only together with a `squad/decisions/` entry naming the failure it prevents." | "A new class is added only in a pull request that names the failure it prevents." |
| `squad/roles/spec-author.md:39` | "The lead asks the maintainer and records the answer in `squad/decisions/`." | "The lead asks the maintainer and records the answer in `00-lead.md`." |
| `sdd/changes/2026-10/establish-squad.md:32` | layout line `decisions/<date>-<slug>.md   one file per escalation decision` | Leave as history (change docs are history). If the maintainer prefers to amend the squad-establishing change doc in place, which `sdd/README.md` *Change docs* allows while SDD is being set up, remove the line. |
| `sdd/changes/2026-10/establish-squad.md:109` | "Every escalation is recorded in `squad/decisions/` …" | Same as line 32: leave as history, or amend to match the new `squad/README.md:79` text. |
| `sdd/changes/2026-10/establish-squad.md:118` | "A new rule is added only together with a decisions entry naming the failure it prevents." (no path, same concept) | Same as line 32. |
| `squad/decisions/README.md` | the entry format | Deleted together with the entries. |

No source file, test, script, `.claude/` file, `.github/` file, spec, or rule page references a decisions entry.

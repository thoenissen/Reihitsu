# Squad prompts — round 11 (2026-10-23)

Running together: #998 (last SDD batch), #1423 + #1424 (squad instructions, one run). Then the final cleanup PR.

---

## #998 — opt-out coverage, every test tagged

```
/squad #998

This run: switch RequirementCoverageTests to opt-out and require every test method to carry a [Requirement] tag or a visible exemption, as the issue states. All blockers are merged: every analyzer batch, #939 (PR #1358: formatter/ and playground/ enforced, RequirementAttribute in Formatter.Test and Playground.Test), and #1401 (PR #1414: sdd/README.md "Tests and requirements" brought up to date) — build on the rulebook as it stands on main.
The issue leaves two decisions open. Put them to me as an escalation before the test implementer starts, with a concrete example each; my coordinator's recommendations:
- Incidental tests: (a) a test that verifies no requirement on its own is exempted visibly (attribute or list entry with a reason), and the rulebook keeps "a test that verifies a requirement only incidentally does not tag it" — recommended; (b) it tags the nearest requirement; (c) its spec gets a requirement added.
- Architecture and tooling tests (Reihitsu.ArchitectureTests, tooling tests): (a) exempted as a class with a stated reason (they guard repository conventions, not shipped behavior) — recommended; (b) tagged against a new conventions spec.
Lane: tooling (test infrastructure, sdd/README.md, squad/roles/test-implementer.md) with the process change doc the issue asks for ("Affected requirements: none — process change"). Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs. Parent: #899.

Read first: AGENTS.md, sdd/README.md ("Tests and requirements", "Checks"), squad/README.md, squad/lead.md, squad/roles/test-implementer.md, the RequirementCoverageTests and RequirementAttribute sources in every test project, the newest entries in squad/decisions/, and this issue.

Running at the same time: #1423 + #1424 (squad/roles/skeptic.md, squad/failure-classes.md). Do not touch those two files.

Done means: coverage is opt-out with an empty exemption list or a reason per entry; every test method tagged or visibly exempted, enforced by a check that runs in CI; the rulebook states one consistent rule for incidental tests; the test-implementer role tags new tests from the start; change doc written; CI green; PR "Closes #998".

Must not: change shipped code or behavior; add requirements to specs to make tags fit (a missing requirement is an open question, "SDD open questions: <spec path>"); retire, move or renumber a requirement ID; put sdd/work/ into the PR. Create no issues besides "SDD open questions" issues without my approval of the exact text.
```

---

## #1423 + #1424 — skeptic template and FC-014

```
/squad #1423 #1424

This run (several issues, one lane — tooling, squad instructions): 
- #1423: add a "## Follow-up candidates" section (`_None._` when empty) to the handover template in squad/roles/skeptic.md.
- #1424: narrow FC-014 in squad/failure-classes.md and the FC-014 sentence of check 3 in squad/roles/skeptic.md to layout concerns; cite SHARED-DIAG-010 (sdd/specs/shared/diagnostics.md) for why an order or content rule whose concern no formatter phase owns is intended, not this failure class. Keep the FC-013 wording PR #1421 just wrote.
Lane: tooling. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md, sdd/README.md (P11 section), sdd/specs/shared/diagnostics.md (SHARED-DIAG-010), squad/README.md (incl. "Several issues in one run"), squad/lead.md, squad/failure-classes.md, squad/roles/skeptic.md, the newest entries in squad/decisions/, and both issues.

Running at the same time: #998 (RequirementCoverageTests, sdd/README.md, squad/roles/test-implementer.md). Do not touch those files or sdd/.

Done means: both issues done as stated, CI green, PR "Closes #1423" and "Closes #1424".

Must not: change other failure classes or skeptic checks; change code, specs or tests; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

Role: spec author
Commit: 1002991
Verdict: text written

Post-publish docs task. It follows the maintainer instruction summarized in `00-lead.md`: revert this run's rule-page changes to `main` (f3d514d), and leave `sdd/specs/` untouched.

## thoenissen/Reihitsu#1329, #1389, #1241, #1344, #1242 (one change for all five)

### Changed text

I ran `git checkout f3d514d -- documentation/rules/<file>` for each of the eight rule pages this run changed:
- `documentation/rules/RH8107.md`
- `documentation/rules/RH8202.md`
- `documentation/rules/RH8204.md`
- `documentation/rules/RH8301.md`
- `documentation/rules/RH8304.md`
- `documentation/rules/RH8305.md`
- `documentation/rules/RH8307.md`
- `documentation/rules/RH8309.md`

Afterwards, `git diff --stat f3d514d -- documentation/rules/` is empty: all eight pages are byte-identical to `main`. No other file under `documentation/` and nothing under `sdd/specs/` changed.

### Statements a spec or test needs that were reverted

- **Specs and tests.** None depend on the reverted text.
  - `rg` finds no test that asserts any of the removed sentences: "never closed", "different name", the `## Exceptions` sections.
  - The rule-documentation tests compare only the Violation/Correction examples and the footer, and they pass (see below).
  - No spec under `sdd/specs/` cites a rule-page sentence of RH8107, RH8202, RH8204, RH8301, RH8304, RH8305, RH8307, or RH8309.
- **A repository instruction, reported, not restored.**
  - `AGENTS.md`, *Code-fix coverage*, says: "When a rule's fix deliberately leaves a class of inputs to manual correction, record that in `documentation/rules/RH####.md` … in one or two user-facing sentences."
  - After the revert, these fixes withhold themselves without a word on their rule pages:
    - RH8107, RH8202, RH8204, RH8309 for an element without an end tag of its own (SHARED-DOC-017);
    - RH8309 for text or other nodes beside the elements (RH8309-005).
  - The page text on `main` stays: RH8202's "never closed" sentence, which predates this run, is back.
  - The maintainer's reason in `00-lead.md` points to `documentation/rules/general-notes.md`, "A diagnostic without a code fix". That note says "The rule page names the cases that are always left to you", so the general note and AGENTS.md now disagree with the reverted pages.
  - I changed neither. Whether AGENTS.md and the general note should change, or these gaps count as covered by the general note, is the maintainer's call.

## Whole run

- `scripts/verify-text-only.sh --strict-docs --base 9af3a140c105ed98079a0bac2e49b852c2d94ba2 --head worktree` exited 0:
  `TEXT-ONLY PROOF: PASS — base=9af3a14 head=worktree — 28 changed file(s) (28 non-compiled, 0 C# comment/layout-only); xml-doc impact: none`
  The 28 files are the eight rule pages, the deletion of the earlier `sdd/work/` files, and the new `00-lead.md`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~RuleDocumentation|FullyQualifiedName~Spec"`:
  `Passed!  - Failed:     0, Passed:    37, Skipped:     0, Total:    37`
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`:
  `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7`

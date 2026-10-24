# Squad prompts — Phase 5 (from 2026-10-24)

## #1441 — follow-up issue format (P5-1 item 7A)

```
/squad #1441

This run: add the follow-up issue format as a repository document, as #1441 states — `sdd/templates/follow-up-issue.md` with three variants (spec bug, clarification, internal), plus pointers in sdd/README.md (layout section) and squad/lead.md (where the lead drafts a follow-up issue for my approval). Decided by me: the format is NOT a public issue template; `.github/ISSUE_TEMPLATE/` stays unchanged, because every file there is offered to users in the "New issue" chooser. Behavior changes and new rules keep using the public feature-request templates.
Lane: tooling. Confirm with scripts/lane-check.sh. Base the PR on main; no stacked PRs.

Read first: AGENTS.md ("Issue intake", "Asking the maintainer", "Code-fix coverage"), sdd/README.md, sdd/templates/ (existing templates as style reference), squad/README.md (lanes), squad/lead.md, .github/ISSUE_TEMPLATE/ (to keep the variants consistent with the public bug/feature fields), and #1441.

Running at the same time: nothing that touches sdd/README.md, sdd/templates/ or squad/.

Done means: the document exists with the three variants, each a short fill-in skeleton with the fields #1441 lists; sdd/README.md and squad/lead.md point to it; no public template, spec, code or test changed; CI green; PR "Closes #1441".

Must not: change .github/ISSUE_TEMPLATE/, specs, code or tests; rewrite existing issues; put sdd/work/ into the PR. Create no issues without my approval of the exact text.
```

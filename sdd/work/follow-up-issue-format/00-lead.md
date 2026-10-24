Role: lead
Commit: 1b9a028
Verdict: lane claimed — tooling

# Run: follow-up-issue-format

- Issue: thoenissen/Reihitsu#1441
- Slug: follow-up-issue-format
- Branch: claude/upbeat-hypatia-pjezod (the session's designated branch, used instead of `claude/issue-1441-…` because this session may push only there)
- Draft PR: thoenissen/Reihitsu#1454
- Lane: tooling. The change touches `squad/lead.md` (tooling path) next to `sdd/templates/` and `sdd/README.md` (docs paths); the narrowest lane admitting `squad/` is tooling. The maintainer named this lane in the prompt.

## Roles

- Spec author — skipped: the tooling lane does not run it.
- Skeptic — skipped: the tooling lane does not run it; the maintainer decided the approach in the prompt.
- Test implementer — skipped: the change adds no tool with tests (Markdown only).
- Implementer — runs: writes the template document and the two pointers.
- Cleaner — runs.
- Spec checker — runs.
- Auditor — runs.
- Validator — runs.

## Maintainer's clarifications (summarized)

- Deliverable: `sdd/templates/follow-up-issue.md` with three variants (spec bug, clarification, internal), each a short fill-in skeleton with the fields the issue lists, plus pointers from `sdd/README.md` (layout section) and from `squad/lead.md` where the lead drafts a follow-up issue for approval.
- The format is not a public issue template: `.github/ISSUE_TEMPLATE/` stays unchanged, because every file there appears in the "New issue" chooser. Behavior changes and new rules keep using the public feature-request templates.
- The variants should stay consistent with the fields of the public bug and feature templates.
- Must not change public templates, specs, code, or tests; must not rewrite existing issues; no issue is created without the maintainer's approval of the exact text.

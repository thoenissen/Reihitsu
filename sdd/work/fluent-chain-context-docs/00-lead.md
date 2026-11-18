# Lead — fluent-chain-context-docs

- Issue: thoenissen/Reihitsu#1861 (`agent-filed`: goals are proposals)
- Slug: `fluent-chain-context-docs`
- Branch: `claude/hopeful-mccarthy-py3bwt` (session-designated); draft PR thoenissen/Reihitsu#1871
- Lane: **docs** — the issue asks to reword two documentation comments in `src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs`; C# under `src/` counts as text when `scripts/verify-text-only.sh` passes.
- Roles run: spec author (writes the comment change), spec checker. Skipped: skeptic, test implementer, implementer, cleaner, auditor (docs lane). Validator: only if a compiled file changes; the proof decides.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Take in the stale "positional context" wording of `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` as an adjacent fix? | No; follow-up candidate | Different component (formatter), not the code-fix helper or its counterpart (`README.md`, *Scope*) | Comment text only | – |
| 2 | Spec author | Correct the fall-back description in `sdd/changes/2026-11/context-target-first-line.md`? | No, dropped | A change doc is a historical record of a decision, not a description of current code | Not a visible surface | – |
| 3 | Spec author | Validator in docs lane? | Skipped; lead ran `scripts/build.sh` (green, 0 warnings) for the new `<see cref>` | Text-only proof PASS (261 identical tokens); only xml-doc impact is a cref, checked by the Release build | No compiled change | 41d729a |

## Follow-up candidates

1. Summary of `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` (`src/Reihitsu.Formatter/ReihitsuFormatter.cs`, ~line 257) still says "using a containing node for positional context", although the target's column is a fact the entry point passes (FMT-PIPE-017, FMT-PIPE-018). Text-only rewording. Source: `01-spec-author.md`.

## State

- Last step: spec author (41d729a), build green.
- Next: spec checker, then issue manager, publish.

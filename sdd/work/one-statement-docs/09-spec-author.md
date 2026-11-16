Role: spec author
Commit: 3f0b306
Verdict: text written (#1470: nothing to do; #1471: text written; #1479: nothing to do; #1544: nothing to do)

Carries out decision 8 as the prompt states it (the `Decisions taken` table in `00-lead.md` at 3f0b306 holds decisions 1–4 only; decision 8 is named in *State*, and its content is taken from the prompt). Class: a document restates which paths the lane check classifies as text, tests, tooling, or production. Refused by its cause: every such statement now defers to the lane-check table in `scripts/README.md` instead of naming paths or file kinds. Statements about what the text-only proof accepts are left alone. The prompt carried no intended fix beyond that decision.

## thoenissen/Reihitsu#1470
Nothing to do; decision 8 concerns #1471's classification only.

## thoenissen/Reihitsu#1471

### Changed requirements
_None._ No spec states the lane classification; the change is text in four tooling documents.

Changed:
- `squad/roles/spec-author.md:12` (audit finding #2): dropped "such as specs and `documentation/`, and comment-only edits in C#". The bullet now says the table alone says which paths are text; when a C# file is edited, quote the proof line; stop when the proof fails or `scripts/lane-check.sh docs --head worktree` does not confirm the lane. It no longer claims which C# paths the lane admits, so both counterexamples of #2 (`src/Reihitsu.Tooling/**`, file-based apps under `scripts/`) are covered by the lane check itself.
- `squad/README.md`, Docs bullet: "text only. A C# file is text only where the table says so, and only when `scripts/verify-text-only.sh` proves the whole diff text-only." → "text alone." The proof condition is the table's own (Text row, and the sentence above the table in `scripts/README.md`); the paragraph just above the bullets already links the table. Also resolves the auditor's "text only" double-meaning hint.
- `squad/README.md`, Behavior change bullet: "a change doc under `sdd/changes/`" → "a change doc"; the path is the Change doc row's.
- `squad/lead.md:27`: "scripts, CI, build settings, agent instructions, or repository tooling are tooling" → "a change to repository tooling — what the lane-check table in `scripts/README.md` classifies as tooling — is tooling"; "text alone is docs" → "text alone, as that table classifies it, is docs". The old list drifted from the table (agent instructions under `squad/notes/` are text).
- `scripts/README.md:86` (*The text-only proof*, outside the lane-check table): "admit a comment-only C# change to the docs lane (through `lane-check`)" → it is used through `lane-check`, which runs it before classifying a diff, with a link to *The lane check* for which C# files it can make text. The old sentence claimed any comment-only C# change is admitted, which the table contradicts for `src/Reihitsu.Tooling/` and `scripts/`.

Checked and kept:
- `squad/roles/spec-author.md:3` and `:52`, `squad/README.md:12`, `.claude/agents/spec-author.md:3` — "comment-only edits in the docs lane": bounded by the lane, no path claim (as 08-auditor found).
- `squad/README.md:38` (links the table as the one statement), `:41`–`:44`, `:46` — name categories (test, production, tooling, change doc, unclassified), not paths.
- `squad/README.md:30`, `squad/roles/validator.md:9` — "only if a compiled file changed" / "no compiled file": which roles run, not classification.
- `squad/lead.md:33`, `:120`, `:125`, `:151` — run the lane check or the proof; no classification.
- `scripts/README.md:97` — what the text-only proof treats as non-compiled: the proof's own rule, kept per the prompt. `:111`, `:126` — inside the lane-check section.
- `AGENTS.md:26`, `:156`; `CLAUDE.md`; `CONTRIBUTING.md`; `sdd/README.md:35`; `sdd/templates/follow-up-issue.md:19`, `:104`; `.claude/commands/*.md`; `.claude/agents/*.md`; `squad/roles/*.md` (all other lines) — name lanes or the scripts, no path classification.
- `sdd/changes/2026-10/introduce-sdd.md:180`–`:184` — its lane table lists admitted paths ("`sdd/`, `documentation/`, Markdown, templates…", "scripts, CI, build settings, `Reihitsu.Tooling`, agent instructions"). Not changed: it is an `Accepted` change doc, the record of a past decision, not a live statement (other parts of it are equally historical, such as "the PR's run report"). See *Internal choices*.
- `rg` searches used: `lane check|classif|tooling path|production path|test path|shipped source|test projects|is text|are text|as text|is tooling|docs lane|text-only|text only`, and `markdown|compiled file|non-compiled|tooling|agent instructions|repository tooling`, over every `*.md` outside `sdd/work/` and `squad/notes/`.

### Examples for the tests
_None._ Text only.

### Output questions
_None._

### Internal choices
- Left the accepted change doc `sdd/changes/2026-10/introduce-sdd.md` unchanged. Alternative: rewrite its lane table to defer to `scripts/README.md`, which would alter a historical decision record.
- Removed the proof condition from the Docs bullet in `squad/README.md` instead of rephrasing it, because the table already carries it. Alternative: keep "a C# file only through the text-only proof the lane check runs".

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1479
Nothing to do; decision 8 does not touch it.

## thoenissen/Reihitsu#1544
Nothing to do; decision 8 does not touch it.

## Whole run
- No C# file and no test changed.
- `scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- `scripts/test.sh --project architecture`: `Failed!  - Failed:     1, Passed:   118, Skipped:     0, Total:   119`. The failure is `SquadNoteReferenceTests.RepositoryFilesReferenceNoSquadNote`: `sdd/work/one-statement-docs/05-auditor.md:32: notes/2026-11-16-x.md`. Pre-existing in a committed handover at 3f0b306, not in a file this round changed; it fails CI until that line or the `sdd/work/` directory is gone.

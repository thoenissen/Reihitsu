Role: spec author
Commit: bbc5a31
Verdict: text written (#1470: nothing to do; #1471: text written; #1479: nothing to do; #1544: nothing to do)

Repair round after the passing audit (05-auditor, finding #1, parts a, b, c). The prompt carried no intended fix beyond pointing at the auditor's suggestion, which I treated as a suggestion only; the wording below is derived from `LaneClassifier.ClassifyPath`, `ClassifySourcePath`, `IsRootMarkdownText`, and `TryGetRequiredLane`.

## thoenissen/Reihitsu#1470
Nothing to do; the audit passed it and finding #1 does not touch it.

## thoenissen/Reihitsu#1471

### Changed requirements
_None._ No spec under `sdd/specs/` states the lane classification; the repair is text in three tooling documents.

- `scripts/README.md`, lane-check table, Text row: names `squad/notes/**`, `.github/ISSUE_TEMPLATE/**`, and `.github/PULL_REQUEST_TEMPLATE.md` explicitly (the classifier's `_textPrefixes` and `_textFiles`), replacing the vague "issue and PR templates". (a)
- `scripts/README.md`, the first-row sentence: now "whatever its extension: every file under `squad/notes/` is text while the rest of `squad/**` is tooling, the issue and PR templates are text while the rest of `.github/**` is tooling, and a test project's `*.csproj` is tooling rather than a test". Each pair follows from check order in `ClassifyPath`: text prefixes and files before tooling prefixes and extensions, tooling extensions before `ClassifySourcePath`. (a)
- `squad/README.md`, lane bullets: Docs says a C# file is text only where the table says so (which excludes `src/Reihitsu.Tooling/`) and only when the proof passes (b). Tests, Bug fix, and Behavior change are restated in categories ("test paths", "production paths") instead of "files of test projects" / "shipped source under `src/`", since a test project's `.csproj` is tooling and production is every remaining `src/` path. Behavior change now also names a change doc without production or tooling paths, which `TryGetRequiredLane` maps to behavior change. The no-lane parenthesis says "production" instead of "shipped source".
- `squad/roles/spec-author.md:12`: the docs-lane bullet points at the text category of the lane-check table instead of listing "Markdown" (c).

Traced against the classifier: `squad/notes/x.cs` → Text (prefix first); `src/Reihitsu.Tooling/LaneClassifier.cs` with the proof passed → Tooling (tooling prefix before `ClassifySourcePath`), so a docs claim fails; `src/Reihitsu.Analyzer.Test/X.Test.csproj` → Tooling; `.github/ISSUE_TEMPLATE/config.yml` → Text; `scripts/README.md`, `.claude/agents/x.md`, `AGENTS.md` → Tooling; `README.md` → Text.

Left unchanged: `scripts/README.md:97` (which paths the text-only proof treats as non-compiled) is a separate policy, as the auditor found; `squad/lead.md` section 3 picks a lane from the issue before the check confirms it and states no path classification.

### Examples for the tests
_None._ Text only; `LaneClassifierTests` already pins every member traced above.

### Output questions
_None._

### Internal choices
- Restated the Tests/Bug fix/Behavior change bullets too, beyond the auditor's part (b), because "files of test projects" and "shipped source under `src/`" also differ from the classifier (test `.csproj`, change doc alone). Alternative: change only the Docs bullet.
- Listed the template paths explicitly in the Text row rather than keeping "issue and PR templates". Alternative: keep the gloss.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1479
Nothing to do; the audit passed it and finding #1 does not touch it.

## thoenissen/Reihitsu#1544
Nothing to do; the audit passed it and finding #1 does not touch it.

## Whole run
- No C# file and no test changed.
- `scripts/verify-text-only.sh --base 62a2512 --head worktree`: `TEXT-ONLY PROOF: FAIL — base=62a2512 head=worktree — 1 of 5 changed file(s) carry compiled behavior; xml-doc impact: none`. The one file is `scripts/README.md` ("not a C# file and not a known non-compiled text path"): the proof treats everything under `scripts/**` as behavioral regardless of extension, by design (`scripts/README.md`, *verify-text-only*). The change there is a Markdown table row and one sentence; the other four files (`squad/README.md`, `squad/roles/spec-author.md`, two `sdd/work/` files) are text-only. The audit's validity therefore rests on the diff being Markdown only, not on the proof's verdict.
- `scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

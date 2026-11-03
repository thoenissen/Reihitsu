Role: auditor
Commit: f49989c
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1486, `00-lead.md` (decision 19), `21-auditor.md`, `22-cleaner.md`, `23-spec-author.md`, `24-spec-checker.md`, `git diff 3010b6a..f49989c`, and the counterparts named below: FMT-PIPE-028, the FMT-REGION requirement list in `region-formatting.md`, the rest of the change doc, the `main` version of the `FormatNodeInDocumentWithContextAsync` remarks, and every `FMT-PIPE-028` reference under `src/`.

## Scope

This is a delta audit of repair round 2 (decision 19), `3010b6a..f49989c`. Head `f49989c` is checked out. Base `f8dc7ef7a71bffbf373675d0613e32a77a1ef5a6` equals the `origin/main` SHA in the prompt and is the merge base, so `origin/main` is merged.

The diff is what the handovers describe:
- `ReihitsuFormatterNodeRegionTests` class summary (6a) and the `FormatNodeInDocumentWithContextAsync` remark (7), both by the cleaner (22).
- The change-doc title, the third *What* bullet with its heading (6b), and one section heading, by the spec author (23).
- Decision 19 in `00-lead.md`, and handovers 22 to 24.

Scope did not grow: no spec, requirement, test body, or compiled line changed. `scripts/verify-text-only.sh --base 3010b6a --head f49989c`, run on the committed head: `TEXT-ONLY PROOF: PASS` — 7 files, the 2 C# files with identical tokens, directives, and disabled text, changed trivia only `documentation`; xml-doc impact includes public API documentation (the restored reason, #7, intended). The evidence of `21-auditor.md` for everything else is byte-identical and reused.

Status of the earlier findings:

- #6 — **closed.** Both members it listed hold now.
  - (a) The class summary says the node-level entry points "apply no region requirement (FMT-PIPE-028), while the document and syntax-tree entry points apply them". It no longer claims the directive text is kept, so the tab and line-ending tests in the same class no longer contradict it.
  - (b) The *What* bullet now lists only what the region phase no longer does, in the words FMT-PIPE-028 uses for its own list (capitalization, the `#region` line form, the comment or space behind `#endregion`, the whitespace at the end of a region directive line), and refers to the preparation phase as a whole (FMT-PIPE-019) without listing its effects. It no longer contradicts the FMT-PIPE-028 tab example or the change doc's own code-fix paragraph. The title and the two headings that made the same unqualified claim were renamed; `rg` finds no link to the old anchors outside `sdd/work`.
  - A search of the change doc and of the repository (outside `sdd/work`) for "as written", "keep … region", "stay", and similar finds no remaining claim that node-level formatting keeps a region directive's text. The remaining hits (*What* second bullet, line 93, line 373, lines 390–408) speak about whole-document formatting or about alternatives considered. Maintainer answer 6 is kept verbatim, which is right for a recorded answer.
- #7 — **closed.** The remark again carries "because this entry point serves code fixes; for the same reason, no region requirement is applied (FMT-PIPE-028)", matching `FormatNodeInDocumentAsync` (line 151) and the `main` clause it had lost. The text is otherwise word-identical to `3010b6a`. The re-wrap of that paragraph introduced #8.
- #1 to #5 — closed in earlier rounds; nothing in this repair touches them.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | low | `src/Reihitsu.Formatter/ReihitsuFormatter.cs` lines 247–253 (`<remarks>` of `FormatNodeInDocumentWithContextAsync`) | – (comment layout; code quality) | – (grew from the wrap hint of `21-auditor.md`; not the decision of #5/#7) | PR-introduced | The repair re-wrapped the whole paragraph, and the wrap is now broken: line 252 ends after "so a target" at 62 characters, and line 253 runs to 175 characters ("delegated to `<see cref=…/>`, including its removal of the separator whitespace in front of the target"). Line 253 is now the longest documentation line in the file; the longest on `main` is 163. `22-cleaner.md` says the lines were "re-wrapped evenly", which they are not. The words are correct, and XML documentation collapses whitespace, so the rendered public documentation is unaffected; the text-only proof passes. Also, the re-wrap rewrites four lines that `main` already wrapped, which adds diff noise against `main`. | Re-wrap lines 250–253 to the paragraph's width. The smallest diff against `main` keeps `main`'s lines from "language version of the document's project" onward, e.g. "…The target's leading trivia is written back as it stands in the document, so a / target that starts a later line of the context keeps that line's column. When the target is the context node, / formatting is delegated to `<see …/>`, including / its removal of the separator whitespace in front of the target". |

## Guard scope
_N/A — the repair alters no predicate, guard, or exemption; production edits are XML documentation only (text-only proof PASS on `f49989c`). The guard table of `11-auditor.md` stands unchanged._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| What node-level formatting does to region directives | FMT-PIPE-028, the only statement. Referenced without restating by the non-goal, FMT-PIPE-024, the three `ReihitsuFormatter` remarks (lines 116, 151, 249), `FormattingContext` lines 53 and 165, the `RegionFormattingPhase` summary, the `AGENTS.md` row, the `ReihitsuFormatterNodeRegionTests` class summary, and the change-doc *What* bullet (which repeats FMT-PIPE-028's own list of region-phase effects, not the preparation phase's) | one statement; the two restatements of #6 are gone |
| Why the node-level entry points keep shapes (code fixes) | FMT-PIPE-011 and FMT-PIPE-028 (owners). All three remarks and the `FormattingContext` summary give the reason again | consistent; #7 closed |
| Tabs between directive tokens / line ending behind a directive / node-level skip of the region phase (code) | Unchanged since `21-auditor.md`: `TabReplacementPhase.ReplaceDirectiveTriviaTabs`, `LineEndingNormalizationPhase`, `FormattingContext.FormatsRegionDirectives` | one owner each (unchanged) |

## Assertion adequacy
_N/A — no test was added or changed; the only test-file edit is the class summary (token stream identical). The table of `21-auditor.md` stands._

Visible output is decided: the repair adds or changes no expected value and no rule page.

## Failure classes dismissed
- FC-001 to FC-030, FC-032 to FC-036: no compiled line, test body, trivia handling, syntax rewrite, code-fix provider, predicate, or expected value changed (text-only proof PASS on the committed head); the dismissals of `21-auditor.md` stand.
- FC-016: the two restatements of #6 are gone; no other restatement found (see *Policy ownership*).
- FC-031: each changed summary describes the code: the class summary describes the class's tests, the remark describes the method. #8 is layout, not content.
- FC-037: the comment-only claim is proven by `scripts/verify-text-only.sh` on `f49989c`, not only on the cleaner's worktree.
- Tab-width matrix: no column is measured from a region directive's interior.

## Architecture findings
_None._ With #6 closed, the rule "what node-level formatting does to a region directive" is stated once, in FMT-PIPE-028; every other place refers to it, and the change-doc bullet repeats only FMT-PIPE-028's own list. The structure decision 17 set is complete.

## Hints
- Decision 19 prescribes `scripts/verify-text-only.sh --strict-docs --base 3010b6a`. `--strict-docs` fails by design whenever public API documentation changes, and restoring the reason in the public remark (#7) is such a change. The cleaner ran the proof without `--strict-docs`; this audit did the same on the committed head. The proof without the flag is the right one here.
- `BuildRegionTrivia` / `BuildUndescribedEndRegionTrivia` (decision 16) and follow-up candidates 2 and 3 in `00-lead.md` are unchanged.

`pass, text cleanup` reason: #8 is the line wrap of an XML documentation paragraph. The token stream is identical, and XML documentation renders the paragraph with whitespace collapsed, so it cannot reach compiled behavior, diagnostics, rendered public API documentation, or rule metadata.

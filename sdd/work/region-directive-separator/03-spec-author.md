Role: spec author
Commit: c1e9ba6
Verdict: change doc written

Amendment round after the maintainer's answers 1–6 and decisions 2–7 in `00-lead.md`, and `02-skeptic.md`. `01-spec-author.md` stays as the first round's record; this file supersedes its *Changed requirements*, *Examples for the tests*, and *Output questions*.

The change doc `sdd/changes/2026-11/region-directive-separator.md` is set to `Status: Accepted` on the lead's instruction (in a squad run the lead sets it, `sdd/README.md`, *Change docs*), with the six answers under `Decisions` → `Maintainer answers`. Maintainer answer 7 is recorded there as well.

## Changed requirements

- FMT-REGION-007 — A `#region` line is `#region`, one space, and the description — added (rewritten from round 1). The line form is stated here only (decision 5) and covers the trailing whitespace (answer 5, option C), the paired-directive scope (decision 2), and the `#if false` qualifier (decision 3). The examples now include `#region Baz···` → `#region Baz`.
- FMT-REGION-001 — changed: its last sentence now points to FMT-REGION-007 instead of stating the line form.
- FMT-REGION-004 — changed: renamed "Regions without a description keep their text". It now writes one space between `endregion` and the text, also where none stood, removes whitespace behind the text, and keeps a bare `#endregion` bare (answers 4 and 7).
- FMT-REGION-006 — unchanged from `main` (decision 3; the round-1 edit is reverted).
- `region-formatting.md` — the introduction states the line form, the `#endregion` space, and that the requirements apply to whole-document and syntax-tree formatting. A non-goal points to FMT-PIPE-028. `Uses`: FMT-PIPE-002, FMT-PIPE-005, FMT-TAB-002.
- FMT-PIPE-028 — Node-level entry points keep region directives — added (answer 6, decision 7). `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` change no `#region`/`#endregion` text; only `FormatDocumentAsync` and `FormatSyntaxTree` apply FMT-REGION. Its example (the class as target, `#region    fields` and a bare `#endregion` kept while `P`'s accessor is expanded) is derived from FMT-PIPE-010's example, not traced.
- FMT-PIPE-024 — reference added: "the transforms and rewrites they skip (FMT-PIPE-011, FMT-PIPE-028)".
- FMT-TAB-002 — example `#region Bar` / `#region Foo`; the prose names FMT-REGION-007 and FMT-REGION-001 (for `Foo`), and `Uses` lists both (decision 4).
- `requirement-ids.txt` — FMT-PIPE-028 and FMT-REGION-007 inserted.

`scripts/test.sh --project architecture` (spec, coverage, and rule-documentation filters): 50 passed, 1 failed. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-PIPE-028 and FMT-REGION-007 only. That is expected until their tests exist.

No rule page changes.

## Examples for the tests

- FMT-REGION-007 — all three examples: several spaces plus `Baz···`; the already-canonical line, which is left unchanged; and `#if false`. Also the tab case in FMT-TAB-002.
- FMT-REGION-004 — `#endregion // Stale` unchanged; `#endregion    // Stale···` → `#endregion // Stale`; `#endregion//Stale` → `#endregion //Stale`.
- FMT-PIPE-028 — the node-level example, plus a code-fix case. RH7105 on `namespace N { static public class C { #region    bar … #endregion } }` writes `#region Bar` and `#endregion // Bar` on `9c9ea5c` (traced with `scripts/apply-fix.sh`); after the change, both directives must stay as written.
- FMT-TAB-002 — the changed example. The two `TabReplacementTests` named in `01-spec-author.md` change their expected output.
- Expect existing tests to change: any test of a node-level entry point or of a formatter-delegating code fix whose expected output capitalizes a region or writes `#endregion // Name` inside the formatted node. I did not enumerate them.

## Region-rule fixes and node-level region formatting

No shipped fix whose diagnostic is about regions relies on node-level region formatting:

- RH5031 and RH5032 (through `RegionDirectiveBlankLineCodeFixProviderBase`) use only `ReihitsuFormatterHelpers.DetectEndOfLine` and `FormattingTextAnalysisUtilities`. They never call a `ReihitsuFormatter.Format*` entry point.
- The fixes of RH7301, RH7302, RH7303, RH7304, RH7306, RH7309, and RH7310 reference no formatter type. Their helpers are the Core `RegionDirectiveUtilities`, `RegionCategoryUtilities`, and `SyntaxTriviaUtilities`, none of which formats.
- RH7401 to RH7412 have no code-fix provider.
- The member-ordering fixes (RH7102, RH7103, RH7109) withhold themselves for a type with a region.

## Output questions

_None open._ The round-2 question (whitespace behind the text of an `#endregion` whose `#region` has no description) was answered B (maintainer answer 7). FMT-REGION-004 now removes that whitespace: its example is `#endregion    // Stale···` → `#endregion // Stale`. The change doc records the answer under `Decisions` and adds the trailing whitespace to its visible output change for that case. Answer 7's general statement, that trailing whitespace is always removed, applies to other directives only as a follow-up.

## Internal choices

- The node-level scope is a pipeline requirement (FMT-PIPE-028, next to FMT-PIPE-011). FMT-REGION only points to it from a non-goal. The alternative was a FMT-REGION requirement. Entry-point behavior is owned by the pipeline spec, as FMT-PIPE-011 shows for the accessor shapes.
- The `#endregion` separator for a region without a description stays in FMT-REGION-004, as the maintainer's answer names. The alternative was to fold it into FMT-REGION-007, which is about the `#region` line.
- FMT-PIPE-027's `Why:` still says "The node-level entry points run the same phases as the document entry points". It is left unchanged because that holds as long as the region phase runs and leaves the directives alone. If the implementer removes the phase from the node-level pipeline instead, that sentence needs a qualifier. The lead should decide when the implementation is known.

## Follow-up candidates

- Unchanged from `00-lead.md`. Its follow-up 1, a fix that writes half of a region pair, is absorbed by FMT-PIPE-028: no node-level entry point rewrites either half any more. The implementer or auditor should confirm this with the skeptic's RH7104 input.
- Its follow-up 2 narrows to directives other than `#region` and `#endregion`, once output question 1 is answered B.

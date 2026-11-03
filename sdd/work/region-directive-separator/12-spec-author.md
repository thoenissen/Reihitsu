Role: spec author
Commit: de32a37
Verdict: spec written

This is repair round 1, for decisions 13 and 14 in `00-lead.md` (audit findings 1 and 2 in `11-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. `05-spec-author.md` stays as the earlier record. Code comments (decision 13's `<remarks>` and the `FormattingContext` summaries, decision 15) are the cleaner's, so this round leaves them alone.

## Changed requirements

- FMT-PIPE-028 — Node-level entry points apply no region requirement (was "… keep region directives") — changed (decision 13).
  - The normative sentence no longer says the entry points "MUST NOT change the text" of a region directive. It now says they apply none of the FMT-REGION requirements, and it lists them: capitalization, the `#region` line form, the comment or space behind `#endregion`, and whitespace at the end of a region directive line.
  - It adds that the preparation phase still runs, so a tab between a region directive's tokens (between `#` and the keyword, or behind the keyword) MUST still become four spaces (FMT-TAB-002, FMT-PIPE-019). A tab inside a description or inside the text behind `#endregion` is kept.
  - `Why:` adds that tab replacement is not a region requirement. It runs before every phase for every entry point.
  - A second example (tabs, described in words) is added. `FormatNodeInDocumentAsync` with class `C` as target turns `#region`, a tab, `bar` and `#endregion`, a tab, `// Stale` into `#region    bar` and `#endregion    // Stale`, and lays out the property. `FormatDocumentAsync` writes `#region Bar` / `#endregion // Bar`.
- FMT-PIPE-024 — unchanged. "the transforms and rewrites they skip (FMT-PIPE-011, FMT-PIPE-028)" still holds: the region rewrite is skipped, and the tab replacement is not a skipped rewrite.
- `region-formatting.md`, non-goal — changed. It now says that no requirement of the spec applies at node level, and that only FMT-TAB-002's tab replacement changes those directives there.
- Change doc `sdd/changes/2026-11/region-directive-separator.md`:
  - The *What* bullet now names the tab replacement as the one change left, unchanged from `main`.
  - *Code fixes stop rewriting region directives* gains a tab paragraph: RH7105 now writes `#region    bar` / `#endregion    // Stale`, where `main` wrote `#region Bar` / `#endregion // Bar`.
  - The *whole region rewrite is excluded* decision now says that the region requirements, not "region directives", are never applied through a fix. It also explains why the tab replacement is not one of them.
  - `Status` stays `Accepted`, because decision 13 keeps the output.
- `AGENTS.md`, phase-ownership row `RegionFormattingPhase` — changed (decision 14).
  - Decisions: the text of every paired `#region`/`#endregion`, also in disabled branches, when a whole document or syntax tree is formatted. That covers capitalization, the one-space `#region` form, the `#endregion // Description` comment, the one space in front of the text behind an `#endregion` of an undescribed region, the bare keyword, and no whitespace at the end of the line. The phase never adds, removes, or moves a directive, and the node-level entry points skip it.
  - Guards: `FormattingContext.FormatsRegionDirectives`; `RegionNamingRewriter.Rewrite`/`BuildRegionTrivia`/`BuildUndescribedEndRegionTrivia`; `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`/`GetEndRegionText`.

No new requirement ID. No rule page changes. `scripts/test.sh --project architecture`: 92 passed, `PhaseOwnershipDocumentationTests` included. A first run failed `UsesListsExactlyTheReferencedRequirements`, because the new example cited FMT-REGION-001/002/007. The sentence now refers to "the region phase" instead of adding those IDs to pipeline.md's `Uses`.

## Examples for the tests

- FMT-PIPE-028 — the new tab example. Decision 13 asks for a node-level test with a tab separator that pins the four spaces. The example also covers a tab behind `endregion`. One `FormatNode` or `FormatNodeInDocumentWithContextAsync` member would cover the shared mechanism the auditor traced but did not run.

## Output questions

_None._ Decision 13 keeps the output, which equals `main` where the region phase did not rewrite the directive and follows unchanged FMT-PIPE-019/FMT-TAB-002. The lower-case and stale-comment node-level outputs are the consequence of maintainer answer 6, now written down with the tab.

## Internal choices

- The heading of FMT-PIPE-028 changes to "Node-level entry points apply no region requirement". The old title made the same overclaim as the sentence. The alternative was to keep the title, but no link uses its anchor.
- The comparison with `FormatDocumentAsync` in the new example cites no FMT-REGION IDs, so pipeline.md's `Uses` row does not grow for one illustrative sentence. The alternative was to add FMT-REGION-001/002/007 to `Uses`.
- FMT-PIPE-024 is left unchanged (see above).

## Follow-up candidates

Unchanged from `00-lead.md`.

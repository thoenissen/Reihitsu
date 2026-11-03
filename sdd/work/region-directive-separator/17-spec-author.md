Role: spec author
Commit: 9905736
Verdict: spec written

Structural change for decision 17 in `00-lead.md` (audit 2, finding 4 and the architecture finding in `16-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decision. Code comments (decision 18) are the cleaner's and are left alone. `12-spec-author.md` stays as the earlier record.

## Changed requirements

- FMT-PIPE-028 — Node-level entry points apply no region requirement — changed (wording only; no output change).
  - The normative paragraph no longer lists what the preparation phase does to a region directive and no longer says "No other part of a region directive's text changes." It now says the requirement exempts region directives from the region phase and from nothing else: the preparation phase runs for these entry points as for every entry point (FMT-PIPE-019), and what it does to a directive, it does to a region directive at node level too.
  - `Why:` now says the preparation phase as a whole is not part of the region requirements and is shared as a whole, instead of naming only the tab replacement.
  - The tab example is kept: it shows both halves (no region requirement applied, preparation phase applied) and the existing node-level tests are pinned to it. Its lead-in no longer says "keeps both directives as written, except that each tab becomes four spaces"; it says no region requirement is applied (`bar` stays lower-case, the stale comment stays) and that the preparation phase still runs, so the tab behind each keyword becomes four spaces (FMT-TAB-002). It no longer reads as the only change.
- `region-formatting.md`, non-goal — changed. "only the tab replacement of FMT-TAB-002 changes them" is replaced by a pointer: what node-level formatting still does to them is stated in FMT-PIPE-028. The rule is now stated once.
- Change doc `sdd/changes/2026-11/region-directive-separator.md` — changed in three places:
  - *What*, third bullet: "The one change left is …" becomes "The preparation phase still runs for these entry points, as for every entry point and as on `main` (FMT-PIPE-019)."
  - *Code fixes stop rewriting region directives*: the tab paragraph is introduced as one example of the preparation phase still running, not as the rule.
  - *The whole region rewrite is excluded* decision: the preparation phase, not the tab replacement, is named as not being a region requirement, and it records why FMT-PIPE-028 refers to the phase instead of listing its effects.
  - `Status` stays `Accepted`; no visible output changes.
- `AGENTS.md` `RegionFormattingPhase` row — unchanged. It says only that "the node-level entry points skip it" and lists no preparation-phase effect.

No new requirement ID. No rule page change. `Uses` rows unchanged (the non-goal cites only FMT-PIPE-028; FMT-PIPE-028 already cited FMT-PIPE-019 and FMT-TAB-002). `rg` finds no remaining "only the tab", "one change left", or "No other part" in `sdd/specs`, `sdd/changes`, `AGENTS.md`, or `documentation/`.

`scripts/test.sh --project architecture`: 92 passed, 0 failed.

## Examples for the tests

- FMT-PIPE-028 — the existing tab example (tests unchanged).
- Decision 17's node-level line-ending test derives from FMT-PIPE-028 together with FMT-PIPE-019 and FMT-EOL-002: a region directive whose line break differs from the detected line ending gets the detected one through `FormatNodeInDocumentAsync`/`FormatNode`, while capitalization and the `#endregion` comment stay as written. The auditor's counterexample in `16-auditor.md` #4 is a ready input. No spec example was added for it, so the spec does not grow a second list of effects.

## Output questions

_None._ Output equals `main` and follows unchanged FMT-PIPE-019, FMT-TAB-002, and FMT-EOL-002.

## Internal choices

- Kept the tab example, reworded so it shows one effect of the preparation phase rather than the only one. Alternative: drop it; rejected because tests are pinned to it and it shows both sides of the boundary.
- Added no line-ending example to FMT-PIPE-028. Alternative: add one; rejected because two examples of preparation-phase effects would read as a list again.
- The non-goal points to FMT-PIPE-028 instead of citing FMT-PIPE-019 itself, so the rule has one statement.

## Follow-up candidates

Unchanged from `00-lead.md`.

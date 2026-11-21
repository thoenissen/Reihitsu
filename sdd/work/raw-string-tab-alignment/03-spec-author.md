Role: spec author
Commit: 80c59c7
Verdict: spec written

Issue: thoenissen/Reihitsu#1432 (`agent-filed`). Amendment round after the maintainer's answers (1A, 2A, 3A, 4A) and the lead's decisions 1, 3, 4 in `00-lead.md`. The prompt carried no intended fix beyond those recorded decisions.

Change doc `sdd/changes/2026-11/raw-string-tab-indentation.md`: `Status: Accepted`; `Decisions` gains *The maintainer's answers* (1A–4A, each with the rejected options) and *The RH5202 fix is withheld for a tab-indented raw string nested in a hole*; `Affected requirements` adds RH5202-008 and RH5601-002 (wording); *What*, change 7 (the withheld case with its input and `main` output), the closing line, and *Alternatives considered* updated to match.

## Changed requirements

- RH5202-008 — The fix is withheld when a raw string nested in a hole is indented with tabs — added (decision 1; the gap is recorded there as a deliberate decision, with its rare-class reasoning). Ledger updated.
- RH5202-007 — clarified: third bullet ("the literal's own text") no longer claims the nested literal is always reported; it says the fix writes no space on nested lines, keeps them when the nested closing whitespace holds no tab, and points to RH5202-008 when it holds one.
- RH5202 summary paragraph — names RH5202-008 next to RH5202-006.
- RH5601-002 — `Why:` wording only (decision 3): names FMT-RAW-010 as the exception to the formatter keeping raw-string tabs and SHARED-DIAG-003 for why RH5601 still leaves them; requirement unchanged. RH5601 `Uses` gains FMT-RAW-010.
- `AGENTS.md` `IndentationPhase` row — names the rewrite of the stripped tab indentation as spaces, one per character (decision 4).

## Examples for the tests

- RH5202-008 — two examples: n01 (nested literal aligned in characters behind tabs, not reported; outer reported, no fix offered) and n02 (both reported; outer fix withheld, nested fix offered and writes spaces; then the outer fix is offered, final result shown). Both inputs were run through `scripts/apply-fix.sh RH5202` on `80c59c7` (LF and CRLF): they compile and are reported; today the outer fix is offered, which RH5202-008 changes. The intermediate (after the nested fix) was also run: the outer fix gives exactly the final block shown.
- The other side of RH5202-008 (nested closing whitespace without a tab, fix offered) is the existing nested examples of RH5202-004/005.
- Unchanged from `01-spec-author.md`: FMT-RAW-010, FMT-RAW-001, RH5202-007.

`scripts/test.sh --project architecture`: only `EveryRequirementOfAnEnforcedSpecIsCovered` fails, for FMT-RAW-010, RH5202-007, and RH5202-008 (no covering test yet — expected until the test implementer runs).

## Output questions

_None._ The maintainer answered 1–4; the withholding of RH5202-008 is a rare-class withholding (decision 1).

## Internal choices

- The withholding is its own requirement, RH5202-008, parallel to RH5202-006, rather than a paragraph inside RH5202-007: it is a separate MUST NOT with its own tests. Alternative: fold it into RH5202-007's third bullet.
- RH5202-008 withholds whenever a nested multi-line raw literal's closing whitespace holds a tab, at any depth and for either shift direction, also when the nested literal is reported (n02); the nested literal's own fix then runs first, and the outer fix is offered on its result. Alternative: withhold only when the nested literal is not reported — needs the analyzer's measure inside the fix for no visible gain.
- RH5202 rule page and RH5601 rule page left unchanged: a withheld fix in a corner case is covered by `general-notes.md`.

## Follow-up candidates

_None_ new. The P11 candidate from `02-skeptic.md` (RH5202 does not report an aligned literal with tabs in its stripped indentation) stands as the lead recorded it; if it is filed and lands, RH5202-008's n01 example becomes a reported nested literal and its first example would need revisiting.

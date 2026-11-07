Role: spec author
Commit: a6636ed
Verdict: spec written

Repair round 3, for lead decision 12 (audit 2, finding #3) under maintainer answer Q2 = A. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- **RH5202-006 — The fix is withheld when a leftward shift would change the value of a raw string nested in a hole — added.**
  - Withheld exactly when three conditions hold together: the shift goes to the left by `d`; a multi-line raw string nested in a hole has closing whitespace `k < d`; and a content line of that nested literal starts with more than `k` whitespace characters. A whitespace-only content line counts with all of its characters.
  - The fix is offered in every other case.
  - The condition covers plain, `u8`, and interpolated nested raw strings at any depth. The outer diagnostic stays reported either way.
  - Each part has a case on the other side (FC-002):
    - rightward shifts are never withheld;
    - `k ≥ d` is offered;
    - `k < d` with every content line at exactly `k` is offered;
    - a whitespace-only line wider than `k` is withheld, and one no wider than `k` is offered.
  - **Why:** SHARED-FIX-003 withholds the fix. SHARED-FIX-002 covers the release: the nested literal's own fix shifts it right, and after that the outer fix is offered and keeps the value.
- **RH5202-003 — clarified.** "removing that much leading whitespace from it — all of it, when the line has less —". The code has always clamped this way (`Math.Min`). The clamp affects hole-code lines, whitespace-only lines, and nested raw lines.
- **RH5202-004 — clarified.** The bullet "A literal other than a raw string" no longer claims that nested raw lines shift "by the same amount" in every case. To the right they do. To the left, a line with less whitespace than the shift loses all of it, and RH5202-006 withholds the fix wherever that would change the value. The *Why* says the same.
- **Header:** the intro sentence now mentions the withholding, and `Uses` gains SHARED-FIX-002. The new ID is in `sdd/specs/requirement-ids.txt` at its sorted position.

The precise condition is derived from the compiler's rule: a content line contributes `L − k` columns, or `max(0, w − k)` when it holds only whitespace. A file-based probe confirmed the whitespace-only case: under a 4-space prefix, a 6-space line gives 2 spaces and a 12-space line gives 8.

I ran `scripts/apply-fix.sh RH5202` over scratch fixtures at a6636ed, under LF and CRLF, and deleted them afterwards:

| Fixture | Result |
|---|---|
| `k=4`, `x` at 34 | value changes (relative 30 → 26) |
| `k=8`, `x` at 34 | value kept (26) |
| `k=4`, `x` at 4 | value kept |
| `k=0`, `x` at 0 | value kept |
| `k=4`, whitespace-only line of 6 | value changes (2 spaces → empty) |
| `k=4`, whitespace-only line of 4 | value kept (empty) |

These results match the boundary as specified.

No change doc and no change of lane. Under the unchanged SHARED-FIX-003 and maintainer answer Q2 = A, the fix is withheld for the inputs where it changed a literal's value, on `main` as well. The rule page is unchanged, because a withheld fix in a corner case is not a page change.

`scripts/test.sh --project architecture`: `Failed: 1, Passed: 114`. The only failure is `EveryRequirementOfAnEnforcedSpecIsCovered: RH5202-006: no test covers it`, which is expected until the test implementer tags the new tests.

## Examples for the tests

Every case below needs a CRLF variant as well (finding #3). In every example the outer literal moves 8 columns to the left, from 25 to 17.

| Case | Tags | What it checks |
|---|---|---|
| RH5202-006, first example: `k=4`, `x` at 34 | RH5202-006 | No fix for the outer diagnostic. Both literals stay reported. The nested literal's fix is offered. |
| Same with nested `$"""` and `x{n}` | RH5202-006 | Withheld, as above. |
| Same with `k=0` (nested closing in column 0) | RH5202-006 | Withheld, as above. |
| Same, run as Fix All or iterated | RH5202-006, SHARED-FIX-002 | One Fix All application fixes only the nested literal. On the result, the outer fix is offered and keeps 30 spaces in front of `x`. The test implementer can derive the final text: nested closing quotes in column 24, `x` in column 54. |
| Second example: `k=d=8` | RH5202-006, RH5202-004 | Offered. Expected output as in the spec block. The nested literal stays reported. |
| Third example: `k=4`, `x` at 4 | RH5202-006, RH5202-003 | Offered. Expected output as in the spec block. |
| Fourth example: whitespace-only line of 6 under `k=4` | RH5202-006 | Withheld. |
| Fourth example's prose variant: a 4-space line | RH5202-006, RH5202-003 | Offered. The line becomes empty, and `x` and the nested closing quotes start in column 0. |
| `VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingLeft` (+CRLF) | add RH5202-006 if the test implementer wants | `k = 29 ≥ d`, so it is the offered side. |

Finding #4 (escapes in the Core `DataRow`s) needs no spec change.

## Output questions

_None._ Maintainer answer Q2 = A and SHARED-FIX-003 decide the withheld side. The offered side keeps `main`'s output.

## Internal choices

- **The withholding is a new requirement, RH5202-006, not text inside RH5202-004.** One requirement per heading, and `sdd/README.md` orders "when the fix is withheld" last. Alternative: a fourth bullet under RH5202-004, as decision 12 worded it. RH5202-004 points to RH5202-006.
- **The boundary is the precise value-change condition, not the simpler "nested closing whitespace narrower than the shift".** Decision 12 says "every input where shifting would change a nested literal's value". AGENTS.md also asks that a guard not give away safe ground, and a nested literal whose content sits at its closing column (e.g. `Hello {"""` / `    x` / `    """}`) is common. Alternative: withhold for every `k < d`. That is simpler to implement, but it withholds the third example, which is safe.
- **RH5202-003 gains the clamp ("all of it, when the line has less").** It now describes what the fix has always done for hole-code and whitespace-only lines. Alternative: leave RH5202-003 as it is. But its "removing that much" was false for those lines, and the prompt asks that the spec's claims hold for every input the fix still rewrites.

## Follow-up candidates

_None_ beyond `00-lead.md`.

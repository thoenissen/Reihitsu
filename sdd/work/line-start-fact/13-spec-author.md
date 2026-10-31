Role: spec author
Commit: d0a45ac
Verdict: change doc written

This answers finding #8 of `12-auditor.md`. The correction is covered by the maintainer's advance acceptance of option A (`squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md`). The change doc stays `Accepted`.

## Re-derived

Formatted with `scripts/format.sh --force` on the branch head (d0a45ac):

| Input | Output |
|---|---|
| `public fixed/* c */int _b[4];` | `public fixed/* c */ int _b[4];` |
| `public fixed  int _c[4];` | `public fixed int _c[4];` |
| `public fixed /* d */  int _d[4];` | `public fixed /* d */ int _d[4];` |

Before the change `KeywordSpacingRule` had no `FixedKeyword` (`git show 802afbf:…/KeywordSpacingRule.cs` finds none). Every gap was therefore only collapsed (FMT-SPACE-002):
- rows 2 and 3 give the same output before and after;
- a comment glued to the type (`fixed/* c */int`, `fixed /* c */int`) was kept as written, and the auditor confirmed this with the e6c896f build.

So the buffer's output changes exactly when a comment is glued to its type.

## Changed requirements

- **Change doc** `sdd/changes/2026-10/fixed-keyword-spacing.md`, corrected in place:
  - *What* now names the buffer outputs that change: `fixed/* c */int` becomes `fixed/* c */ int`, and `fixed /* c */int` becomes `fixed /* c */ int`.
  - The decision *No exception for the fixed-size buffer* no longer claims "the same output … today" without qualification. It separates the whitespace-only gap, which is unchanged, from a comment glued to the type, which changes (FMT-SPACE-013).
- **FMT-SPACE-012** — new example: the buffer `public fixed/* c */int _b[4];` becomes `public fixed/* c */ int _b[4];`. It is the seed for the test the auditor asks for. I added it because FMT-SPACE-012's list now covers the buffer and no other example shows that use of `fixed`.

## Examples for the tests

- The new FMT-SPACE-012 buffer example, tagged FMT-SPACE-012 and FMT-SPACE-013. It fails without `FixedKeyword` in `IsSpacedKeyword`.

## Open questions for the maintainer

_None._

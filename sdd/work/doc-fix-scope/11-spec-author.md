Role: spec author
Commit: 1c125bf
Verdict: #1269: nothing to do; #1342: spec written; #1343: nothing to do; #1459: nothing to do

Repair round 1, decision 24 in `00-lead.md`, auditor finding #2 (`10-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the work item itself.

## thoenissen/Reihitsu#1269

Nothing to do in this round; no finding concerns it.

## thoenissen/Reihitsu#1342

### Changed requirements

- RH8402-006 — The fix replaces an old header and keeps the lines behind it — clarified (coverage widened). A configured `copyrightText` that ends with a line break is now covered: the header's trailing line break takes the place of the line break that ends the line of the run's last comment, so the fix writes one line break there, not two. The replaced file has exactly as many blank lines behind the header as the file had behind the run (none when code follows on the next line, N when N blank lines followed). Code behind the run's last comment on its line gets no extra line break, because the header's own one separates it. The coverage sentence now reads "does or does not end with a line break". No requirement ID added, so `requirement-ids.txt` is unchanged.

### Examples for the tests

RH8402-006, the two new examples under "a configured header that ends with a line break" (configuration = the spec's `copyrightText` plus a trailing `\n`):

- `// Copyright (c) Old Corp.⏎namespace N;` → header, then `namespace N;` with no blank line (this is the auditor's counterexample; `main` already gives it).
- `// Copyright (c) Old Corp.⏎⏎namespace N;` → header, one blank line, `namespace N;` (`main` gave zero blank lines, the head gives two; both wrong under SHARED-FIX-006).

### Output questions

_None._ The first example restores `main`'s output. The second follows from SHARED-FIX-006 and the existing RH8402-006 rule that blank lines behind the run stay; it is the same output the covered configuration already gives.

### Internal choices

- Widened RH8402-006 rather than adding a new requirement: same decision, same `Why:`; only the configuration coverage was carved out.
- Left RH8402-005 unchanged. Its sentence "when text follows the header and does not start with a line break, put a line break between" could be read to demand a second line break behind a header ending with one. The auditor reports the insert path already writes none, and RH8402-005's own coverage was not in the finding. Alternative: add one clarifying sentence to RH8402-005 in this round. Recommendation: the spec checker confirms whether the reading is ambiguous enough to need it; I did not widen beyond the work item.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1343

Nothing to do in this round; no finding concerns it.

## thoenissen/Reihitsu#1459

Nothing to do in this round; no finding concerns it.

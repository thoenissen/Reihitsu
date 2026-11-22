Role: spec author
Commit: bca08a5
Verdict: change doc written

Amendment for decision 2 of `00-lead.md`, which applies the skeptic's "Internal choices" 1–3 from `02-skeptic.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those accepted decisions.

## Changed requirements

- FMT-BLANK-019 — No blank line before the closing bracket of a collection expression or pattern — changed (wording):
  - Skeptic point 1: a closing parenthesis owned by any other node now points directly at FMT-BLANK-014 ("no requirement decides the blank lines directly before it, so they are kept, collapsed to one"), not through FMT-BLANK-018.
  - Skeptic point 2: one sentence states that the blank line behind the opening token is kept "unless it is also directly before the closing one, as in a construct with no element such as `P(⏎⏎)` or `[⏎⏎]`, where it is removed". A new example covers both the empty positional clause and the empty list pattern.
  - Skeptic point 3: the contexts are now examples: "wherever the pattern stands, for example behind `is`, in a switch expression arm, in a `case` label, behind `not`, or nested in another subpattern". The sentence also says "never the context of that node".

Change doc `sdd/changes/2026-11/positional-pattern-close-blank.md`:

- **What:** the contexts are phrased as examples.
- **Visible output change 1:** gains the empty clause `P(⏎⏎)` with its input, Before, and After.
- **Unchanged:** two bullets are reworded. The `(` bullet now applies only when the clause holds a subpattern. The other-`)` bullet now points at FMT-BLANK-014.
- **Decisions, option A:** names the empty clause as covered.
- **Status:** `Accepted` (decision 5: no open output questions).

No requirement IDs, `Uses` rows, or code owners change. `scripts/test.sh --project architecture` passes (198/198).

## Examples for the tests

- FMT-BLANK-019, the new example "a construct with no element". The `P(⏎⏎)` input loses its blank line. The `arr is [⏎⏎]` input is already formatted that way on `main` and needs a test only as a companion.
- All examples listed in `01-spec-author.md` still apply.

How established: I ran `scripts/format.sh` on bca08a5 over the empty-clause fixture in the scratchpad.

- `o is P(⏎⏎        );` gives `P(⏎⏎                      );`, so the blank line is kept today.
- `arr is [⏎⏎        ];` gives `[⏎                       ];`.

The After of the positional case is today's output with only that blank line removed. The `)` stays in column 22, under the `(`.

## Output questions

_None._ The empty-clause result follows option A ("like a list pattern's `]`"), as decision 2 records.

## Internal choices

- The empty-clause rule is stated once, generally ("a construct with no element"), for every closing token FMT-BLANK-019 names. The alternative was a sentence for the positional clause only. Reason: `main` already removes the line for an empty list pattern and collection expression, so the general sentence writes down existing behavior.
- Skeptic point 4 (FMT-BLANK-009/018/020 restating the token list) is left as is, as the skeptic recommended. Decision 2 accepted points 1–3 only.
- FMT-BLANK-018's own bullet "above any other token … the closing parenthesis of an argument list" is unchanged. It already points at FMT-BLANK-014.

## Follow-up candidates

_None._

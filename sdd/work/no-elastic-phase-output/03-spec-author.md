Role: spec author
Commit: d97a54b
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions 3–5 (`00-lead.md`), which this amendment carries out.

## Files

- `/home/user/Reihitsu/sdd/specs/formatter/line-breaks.md` — FMT-BREAK-032 and FMT-BREAK-031 clarified; `Uses` gains FMT-PIPE-034 and SHARED-ATTR-018.
- `/home/user/Reihitsu/sdd/specs/formatter/pipeline.md` — FMT-PIPE-034's second paragraph qualified to the inputs this repository's callers pass.
- `/home/user/Reihitsu/sdd/changes/2026-11/no-elastic-phase-output.md` — `Status: Accepted`; affected requirements extended; *What* and the "adds" decision qualified like FMT-PIPE-034; new decisions *The phase's merge writes its commas without whitespace* and *The issue's goals*; one more rejected alternative (an explicit space behind each comma).

No rule page changes; no code owner changes, so `sdd/specs/README.md` is unchanged.

## Changed requirements

- FMT-BREAK-032 — The attribute lists of an accessor follow its accessor list's layout — clarified: the merge writes every comma between two attributes of the merged list (one it creates and one the first list already held) with no trivia on either side, no whitespace and no elastic trivia; the space behind it is FMT-SPACE-008's and not part of FMT-BREAK-031's recorded exception; the code fixes' merge keeps its output (SHARED-ATTR-018).
- FMT-BREAK-031 — A join writes the minimal separator, and the phase writes no other space — clarified: its recorded exception covers only the two spaces it already named (behind the merged list's `]` and behind a token left touching a joined token); inside the merged list the merge writes no whitespace.
- FMT-PIPE-034 — No phase adds elastic trivia — clarified: "is parsed from text" now holds "for every input this repository's callers pass to `FormatDocumentAsync` and `FormatSyntaxTree` (`AGENTS.md`, *Supported inputs*)".

## Examples for the tests

- FMT-PIPE-034 — unchanged accessor example.
- FMT-BREAK-032 — the new sentence has no example of its own, since final output is unchanged. A tree-level test can run the line-break phase alone (as FMT-BREAK-031's example does) on the skeptic's matrix (02, *Breaking inputs*), including `int this[int i] { get => 0; [param: A, B] [param: C] set { } }`, and check that every comma of the merged list has empty leading and trailing trivia; final outputs as in 02 (`[param: A, B, C] set`, `[param: A(1, 2), B, C] set;`). The comma inside `A(1,2)` is an attribute-argument comma, not a list separator, and is not covered.
- Code-fix boundary: RH5502 … RH5531 keep `[A, B]` (SHARED-ATTR-018).

`scripts/test.sh --project architecture`: 179 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-PIPE-034 only, as in 01; expected until the test implementer tags a test.

## Output questions

_None._ Final output, code-fix results, diagnostics, rule pages, and CLI behavior are unchanged.

## Internal choices

- A comma the first list already held is written with no whitespace too, not with the whitespace it had in the input. Alternative: keep the input comma's trivia. Chosen because the merge already rebuilds that comma today (a separated list built from the elements alone), so it lies in the stretch the merge rewrites, and one rule for every comma of the merged list is simpler to state and test; the spacing phase writes the space back (FMT-SPACE-008), so the output is the same. Because FMT-BREAK-031 forbids removing whitespace in an untouched same-line gap, its recorded-exception paragraph now says explicitly that the gaps inside the merged list are left without whitespace — this states what the merge does, it adds no written space, so the exception does not grow.
- The adjusted issue goals are recorded as a decision section *The issue's goals* in the change doc, following `sdd/changes/2026-11/initializer-leading-comma.md`.

## Follow-up candidates

_None._

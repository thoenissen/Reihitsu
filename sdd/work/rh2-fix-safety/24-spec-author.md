Role: spec author
Commit: 3e00c48
Verdict: spec written

Repair round 3. This round records the maintainer's answer Q9 = A (`00-lead.md`) in RH2005-003, so audit finding 7 (`23-auditor.md`) is decided by a requirement with an example that covers every input row the auditor listed. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. No change doc: lane bug fix, output decided by the maintainer's answer. No rule page changed: the answer decides gap widths in a corner case of an unformatted declaration, which changes nothing a user is told in the normal case. Decision 30 (audit finding 8) removes code no caller reaches and needs no spec change; no requirement names the removed overloads.

## Changed requirements

- RH2005-003 — The fix replaces `public` with `private` — clarified (Q9 = A). Added:
  - A sentence for a non-first accessibility modifier that neither starts nor ends its line: it is removed together with the whitespace behind it on its line; the whitespace in front of it stays as written, and the following token stands directly behind it — also when that whitespace is wider than one space, and also when no whitespace stood behind the modifier.
  - A sentence in *Why*: the removed keyword goes with the gap behind it; the gap in front, which the author wrote, stays.
  - Example with the auditor's five rows (one field each):
    - `static public(int A, int B) First;` → `private static (int A, int B) First;`
    - `static public@int Second;` → `private static @int Second;`
    - `static    public int Third;` → `private static    int Third;`
    - `static public    int Fourth;` → `private static int Fourth;`
    - `static public  volatile int Fifth;` → `private static volatile int Fifth;`
  - Other sides of the qualifiers (FC-002): "starts its line" — the existing Q5 examples (`static⏎        public int`, and `static⏎        public⏎        int`); "ends its line" — the existing Q7 example (`static public⏎        int`); "wider than one space" — the existing one-line `static public int` example (single spaces).

## Examples for the tests

- RH2005-003: the new example — one test per row, LF and CRLF (the auditor's suggested change for finding 7). Rows 1–3 match the maintainer's three quoted outputs in Q9; rows 4–5 follow from the same sentence.

Spec tests: `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` — 51 passed, 0 failed.

## Output questions

_None._ Q9 is answered.

## Internal choices

- The kept wide gap in front of a removed `public` (row 3: `private static    int Third;`, which the formatter collapses) is not read as a conflict with SHARED-FIX-007: the maintainer chose this output explicitly in Q9, and the gap in front is whitespace the fix does not change; *Why* states it. The alternative was a sentence in RH2005-003 naming SHARED-FIX-007 as not applying; I did not add one because SHARED-FIX-007 governs "the code a fix changes", and the sentence states the gap in front stays as written. The auditor's hint (rows 4–5 on `main` kept a gap the formatter collapses) is resolved by the same answer: HEAD's rows 4–5 collapse nothing that stays.
- The five rows go into one example block with one field each, rather than five example blocks: one heading, one rule, and the test implementer still derives one test per field.

## Follow-up candidates

_None._

Role: spec author
Commit: 75f799e
Verdict: spec written

Repair round 2. This round records the maintainer's answers Q7 = A and Q8 = A (`00-lead.md`) where they take effect, so every case a–f of audit finding 5 (`17-auditor.md`) is decided by a requirement with an example. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. No change doc: lane bug fix, outputs decided by the maintainer's answers. No rule page changed: both answers keep lines in place or withhold the fix in a corner case, which changes nothing a user is told in the normal case.

## Changed requirements

- RH2005-003 — The fix replaces `public` with `private` — clarified (Q7 = A). Added:
  - "This holds also when the modifier ends that line": a non-first modifier that starts its own line is decided by the existing Q5 sentence, also when it is alone on that line (case f).
  - A sentence for a non-first modifier that does not start its line but ends it: the line break that ended it keeps its place, the modifier in front of it ends that line, the following token keeps its line and column (case a).
  - A pointer to RH2005-005 for a comment or directive behind such a modifier.
  - Examples: `static⏎        public⏎        int Counter;` → `private static⏎        int Counter;` (f) and `static public⏎        int Counter;` → `private static⏎        int Counter;` (a). Other side of "ends its line": the existing one-line `static public int` example; other side of "starts its own line": the Q5 example.
- RH2005-005 — title changed from "… in front of a later accessibility modifier" to "… next to a later accessibility modifier" — changed (Q8 = A). The places are now a list: in front of the modifier on a line below the modifier in front of it (as before), and behind the modifier, in front of the token that follows it, on the modifier's line or on a line below it. `#if` blocks in either place included. Added: a line break alone in either place does not withhold the fix (RH2005-003 decides the layout). Examples:
  - `static public // note⏎        int Counter;` (b), `static public /* note */ int Count;`, `static public⏎        // note⏎        int Total;` (e): withheld;
  - `static public⏎#pragma warning disable CS0649⏎        int Counter;` (c) and `static public⏎#if NEVER⏎        readonly⏎#endif⏎        int Total;` (d): withheld.

Mapping of finding 5: (a) RH2005-003 Q7 sentence + example; (b), (c), (d), (e) RH2005-005 second place + examples; (f) RH2005-003 Q5 sentence ("also when the modifier ends that line") + example.

## Examples for the tests

- RH2005-003: the two new examples (a, f), LF and CRLF.
- RH2005-005: the two new example blocks (b, inline `/* note */`, e; c, d) as no-fix tests, LF and CRLF.

Spec tests: `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` — 51 passed, 0 failed.

## Output questions

_None._ Q7 and Q8 are answered.

## Internal choices

- Q8's "on its line" is written literally, so it covers a block comment between a later `public` and the type on one line (`static public /* note */ int`): the fix is withheld. That is follow-up candidate 5 (`14-implementer.md`: the comment moves in front of `static` today), which this answer now decides inside the run; the lead may drop candidate 5 from the follow-ups. The alternative was to restrict the second place to comments that end the line or stand on lines below, and keep candidate 5 open; I did not take it because the answer as recorded names "on its line" without that restriction. If the lead reads Q8 more narrowly, the inline example and the "`static public /* note */ int`" mention are the two places to remove.
- A non-first modifier alone on its line (f) is decided by the existing Q5 sentence (the following token stands on the modifier's line and in its column), not by the new Q7 sentence (the following token keeps its line and column). The two give the same text when the columns match, as in the example; where they differ, the Q5 sentence wins, which is what RH2005-003 already said before this round. The alternative was to let the Q7 sentence decide it.
- Q8 extends RH2005-005 (retitled) rather than adding RH2005-006: one heading keeps the withheld class "comment or directive next to a removed non-first accessibility modifier" in one place, as Q8 says ("extends RH2005-005"). The alternative was a new requirement.

## Follow-up candidates

_None._

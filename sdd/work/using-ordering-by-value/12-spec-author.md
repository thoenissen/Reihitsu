Role: spec author
Commit: 16d6216
Verdict: spec written (repair round 2, clarification)

My prompt directed this repair round: write the maintainer's Q2 answer into the specs. It carried no suspected cause or hoped-for verdict. The work follows the Q2 answer in `00-lead.md` and Finding 1 of `11-auditor.md`. No requirement ID was added, no change doc was written, and no rule page changed: the rule pages say nothing about the sort key's spelling. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed. The literal Unicode escapes are unchanged, checked by counting backslash-`u0053ystem` per file: shared 4, RH7201 2, RH7207 3, the same as at 16d6216.

## thoenissen/Reihitsu#1105

### Changed requirements
- SHARED-USING-004 — clarified (Q2 A). A new sentence says the root namespace and the sort key are both taken after a leading `global::` qualifier is removed, whether it is written `global::` or with whitespace or comments around `::`, and that it is part of neither. The list of equal names now also has the plain `global::System.IO`. `Why:` now names the leading `global::` as spelling.
- SHARED-USING-007 — clarified. The sort key of a non-alias directive is now "the value of the imported namespace or type name without a leading `global::` qualifier (SHARED-USING-004)". New formatter example (Q2 A): `using System; using global::System.IO; using System.Collections;` becomes `System; System.Collections; global::System.IO`, and the spelling is kept.
- RH7203-001 — clarified by an example: `using global::System.IO;` then `using {|RH7203:System.Collections|};`.
- RH7203-002 — clarified by a negative example: `using System.Collections; using global::System.IO;` is not reported.
- RH7206-001 — clarified by an example: `using static global::System.Math;` then `using static {|RH7206:System.Console|};`.

### Examples for the tests
SHARED-USING-007 (`global::System.IO` formatter example), RH7203-001 and RH7203-002 (the `global::` examples), RH7206-001 (the `static global::` example). Fix output for the RH7203 example follows RH7203-004 / SHARED-USING-007: `System.Collections` first, then `global::System.IO`.

### Output questions
_None._ Q2 is answered (A).

### Internal choices
- I wrote no example for the other side of "leading `global::`" with an extern-alias qualifier (`Lib::X`). Its root and key keep the qualifier, as on `main`. The shape is rare and outside the issues. The alternative was an extern-alias example in SHARED-USING-004.
- I wrote no global-section (`global using global::…`) example. SHARED-USING-004 covers every section. The alternative was a further example in SHARED-USING-002.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1266
Nothing to do in this round.

## thoenissen/Reihitsu#1267
Nothing to do in this round.

## Whole run
- Files: `sdd/specs/shared/using-directive-ordering.md`, `sdd/specs/analyzer/RH7203.md`, `sdd/specs/analyzer/RH7206.md`, this handover.

Role: spec author
Commit: 34fd53a
Verdict: spec written

Repair round 1. This round records the maintainer's answers Q5 = A and Q6 = A (`00-lead.md`) where they take effect. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. No change doc: lane bug fix, and the outputs are decided by the maintainer's answers. No rule page changed. Both answers are a withheld fix in a corner case, or keep a line where it stood, so neither changes what a user is told in the normal case.

## Changed requirements

- RH2005-003 — The fix replaces `public` with `private` — clarified (Q5 = A). Added a sentence: a removed accessibility modifier that is not the first modifier and starts its own line leaves that line in place, and the following token takes its line and column. Added the example `static⏎        public int Counter;` → `private static⏎        int Counter;`. The other side of the qualifier is the existing `static public` example on one line.
- RH2005-005 — The fix is withheld for a comment or directive in front of a later accessibility modifier — added (Q6 = A, RH2005 half). Added to `requirement-ids.txt`. Examples:
  - a comment line;
  - a `#pragma` line;
  - an `#if NEVER` block that excludes `readonly`.

  Other side of "not the first modifier": RH2005-003's example with comment and directive lines in front of the first token (fix offered). A comment behind the preceding modifier on its own line (`static /* note */ public`) is stated as not covered.
- RH2001-007 — The fix is withheld for a comment or directive inside the property — changed (Q6 = A, RH2001 half; audit finding 2b). The places are now a list. One new place is "in front of its name, on a line below the line of its type". The old sentence "in front of the name … is not covered" is replaced. Examples: a comment line, a `#pragma` line, and an `#if` block in front of the name. Other side: a comment behind the type on the type's line is stated as not covered, and a line break alone does not withhold.
- RH2001-007 — clarified (audit finding 2a). The place "in front of the `;` that ends a declaration with an initializer, on a line below the initializer's last line" is now listed explicitly. The old text covered it only through its summary clause ("between its name and the end of its declaration"). Examples:
  - a comment line and a `#pragma` line in front of `;`: withheld;
  - the other side, `= 5⏎        ;` with no comment, gives `private int _value = 5;`. This is today's output, observed with `scripts/apply-fix.sh RH2001` under LF and CRLF.

## Examples for the tests

- RH2005-003: new example (Q5). The tests `VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier` (+CRLF) narrow to the in-place output.
- RH2005-005: all examples. The tests `VerifyFixKeeps{ACommentLine,ADirectiveLine,AnExcludedModifier}InFrontOfAPublicBehindAnotherModifier` (+CRLF) become no-fix tests tagged `[Requirement("RH2005-005")]`.
- RH2001-007: all new examples. The tests `VerifyFixKeeps{ACommentLine,ADirectiveLine,AConditionalBlock}InFrontOfTheName` (+CRLF) become no-fix tests tagged `RH2001-007`. The `;` tests keep their tag. `VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective` can narrow to `private int _value = 5;`.

Spec tests: `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` gives 50 passed and 1 failed. The failure is `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`, with "RH2005-005: no test covers it". It is expected, and it clears once test implementer 13 tags the RH2005 tests above.

## Output questions

_None._ Q5 and Q6 are answered. I found one undecided case, a name on its own line below the type with no comment (`private int⏎        Value { get; set; }`). The fix is offered, and today's output is `private int⏎    _value;`, because the fix formats its field (RH2001-004). Q5 was asked for RH2005 only, so I gave RH2001-007 no output example for this case. `VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective` accepts in place or formatted, and the formatted one is what the fix produces. The lead decides whether this case needs a question. I see it as decided by the fix formatting its field, so it needs none.

## Internal choices

- Q6 for RH2005 is a new requirement, RH2005-005, and not a paragraph of RH2005-003. RH2005-003 states what the fix writes, and the spec keeps each withheld case under its own heading, as RH2005-004 does. The alternative was to add a withholding paragraph to RH2005-003.
- Q6 for RH2001 extends RH2001-007 rather than adding a new requirement. RH2001-007 already holds the "comment or directive inside the property" class, and decision 7 put that class in one place. The alternative was a new requirement, RH2001-009.
- RH2005-005's boundary is "on a line below the line of the modifier in front of it". A comment on the same line belongs to the preceding modifier's trailing trivia, and the fix keeps it. Q6 asked about lines only. The alternative was to also withhold for `static /* note */ public`, which Q6 did not decide.

## Follow-up candidates

_None._

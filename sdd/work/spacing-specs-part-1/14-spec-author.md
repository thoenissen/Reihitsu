Role: spec author
Commit: f2df5ae530ab4947704d0bdf3fc500a0412833f2
Verdict: spec written

Repair of `13-spec-checker.md` finding 1 (auditor finding 4 for RH6001 and RH6005). The coordinator's message carried no intended fix or hoped-for verdict. The checker is right: `10-spec-author.md` claimed the `/** */`-after-keyword inputs were in neither requirement, but RH6001-001 and RH6005-001 still decided by "the keyword's trailing trivia contains neither whitespace nor a line break", which an empty trailing trivia before a `/** */` comment satisfies — so `if/** c */(…)`, `if/** c */ (…)` and `operator/** c */ -` were claimed as reported while their Whys said formatted code raises nothing.

## Changed requirements

- RH6001-001 — narrowed: the keyword is reported "when the keyword is followed by `(` either directly or after one or more `/* */` comments that are not documentation comments, with no whitespace and no line break anywhere between the keyword and the `(`". A `/** */` comment between keyword and `(` is no longer claimed (thoenissen/Reihitsu#1052 Q1).
- RH6005-001 — narrowed the same way: "followed by the next token either directly or after one or more `/* */` comments that are not documentation comments, with no whitespace and no line break anywhere between the keyword and that token" (thoenissen/Reihitsu#1044 Q2).
- RH6005-002 — clarified: "directly followed by exactly one space", so `operator/** c */ -` is not claimed as not reported either. RH6001-002 already claims only whitespace or a line break directly after the keyword or after a non-documentation `/* */` comment; unchanged.

Every Why is now true for every claimed input: glued keywords become `if (` / `operator +` (FMT-SPACE-012), and a keyword followed only by `/* */` comments gets the space after the last comment (FMT-SPACE-013) — verified with `if/*a*//*b*/(a > 0)` and `operator/*a*//*b*/-(C x, C y)`: both reported, fixed by inserting the space after the keyword, and formatted to `if/*a*//*b*/ (` / `operator/*a*//*b*/ -`, which raise nothing.

Verification: every example of RH6001 and RH6005 re-run through the harness (spans, offered fixes, before/after pairs all match; no example changed). No ID, heading, `Uses` row, or example changed.

## Tags

The claim of `10-spec-author.md` still holds: no tag has to be removed or moved. The tests tagged RH6001-001/-002 and RH6005-001/-002 contain no `/** */` comment (checked with `grep` over both test classes at f2df5ae); the RH6005-002 tests (`VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyOperatorReferenceInsideCrefIsIgnored`, `VerifyConversionOperatorFollowedBySpaceIsNotReported`) all put exactly one space directly after `operator`.

## Open questions to file

_None._ The left-out inputs are already filed as thoenissen/Reihitsu#1052 Q1 and thoenissen/Reihitsu#1044 Q2.

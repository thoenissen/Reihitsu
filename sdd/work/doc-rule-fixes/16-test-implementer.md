Role: test implementer
Commit: e2695e3
Verdict: tests written

Repair round 2, decision 24 (auditor 15, finding #4). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from RH8203-001: "an `<inheritdoc>` element with a `cref` attribute MUST NOT be reported", for the element in a later documentation comment or in included content, with each part of a partial member judged on its own documentation.

Run on `e2695e3` (Debug): `RH8203` filter — 34 tests, 34 passed. The two new tests pin existing behavior (auditor probes A and C), so they pass on the head.

Discrimination check (production restored with `git checkout` right after, nothing committed): replacing the clause `laterInheritdocNode != null && HasCrefAttribute(laterInheritdocNode)` with `false` fails exactly `VerifyInheritdocWithCrefInLaterCommentOfPartialMethodPartIsNotReported`; replacing the `expandedInheritdocElement` clause fails exactly `VerifyIncludedInheritdocWithCrefOfPartialMethodPartIsNotReported` and the existing non-partial `VerifyIncludedInheritdocWithCrefIsNotReported`. Each failure is a report on a part that carries the cref.

## thoenissen/Reihitsu#1617

### Tests
Each test holds three members of a partial method in a `partial class C` with no base list (nothing to inherit): `M` carries `<inheritdoc cref="C"/>` on both parts (neither reported); `N` carries the cref on the defining part and a plain `<inheritdoc/>` on the implementing part (only the implementing part reported); `P` the reverse (only the defining part reported). So the cref side is pinned on both parts, next to the reported side of the boundary on the other part of the same member.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8203….VerifyInheritdocWithCrefInLaterCommentOfPartialMethodPartIsNotReported | RH8203-001 | `Verify(source)` | GREEN |
| RH8203….VerifyIncludedInheritdocWithCrefOfPartialMethodPartIsNotReported | RH8203-001 | `Verify` + `CreateIncludedDocumentationTransform` | GREEN |

The included test's file holds `<inheritdoc cref="C"/>`; the placeholder compilation writes it as `cref="!:C"` (auditor), and the test shows the attribute survives.

### Not covered, and why
- Property, indexer, and event parts: the cref exemption is one statement after the branch choice (`rg -n "HasCrefAttribute" src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs` — lines 116–118, reached by every member kind with the same `laterInheritdocNode` / `expandedInheritdocElement` variables); the member kind only decides the branch, which the round-1 tests cover per kind.
- A cref on the part's first comment: the direct-tag clause, unchanged and covered by the spec example's `Restart`.
- CRLF, code-fix and formatter classes: RH8203 has no fix and lays out nothing.
- FC-003: both sides of the cref condition on both branches, as above. FC-017: the discrimination check above.

## thoenissen/Reihitsu#1345
No work in this round.

## thoenissen/Reihitsu#1346
No work in this round.

## thoenissen/Reihitsu#1347
No work in this round.

## thoenissen/Reihitsu#1348
No work in this round.

## thoenissen/Reihitsu#1270
No work in this round.

## thoenissen/Reihitsu#1092
No work in this round.

## thoenissen/Reihitsu#1153
No work in this round.

## Commands
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8203"

Files: `src/Reihitsu.Analyzer.Test/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzerTests.cs` (formatted with `scripts/format.sh`, no change). No production code committed.

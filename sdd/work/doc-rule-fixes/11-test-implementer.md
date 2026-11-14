Role: test implementer
Commit: 7e74b09
Verdict: tests written

Repair round 1, decision 20 (auditor 10, finding #1). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from RH8203-001 (later documentation comment and `<include>` content are considered; each part of a partial member is judged on its own documentation only) and RH8203-002 (an element from a later comment or an included file is reported on the declaration's name: the identifier, `this` for an indexer, the first variable of a field-like event).

Run on `7e74b09` (Debug): `RH8203` filter — 32 tests, 24 passed, 8 failed; the 8 failures are exactly the new tests, each with `expected "4" actual "0"` (the run's head reports nothing for these parts, as the auditor observed). No compiler diagnostic appears, so the inputs compile.

## thoenissen/Reihitsu#1617

### Tests
Each test holds four members of one kind in a `partial class C : I`: `M` implements `I`'s member and carries the element on both parts (not reported — the inherits side); `N` inherits nothing and carries it on both parts (both reported); `O` carries it only on the implementing part, the defining part has a summary (only the implementing part reported); `P` carries it only on the defining part, the implementing part has a summary (only the defining part reported). `O` and `P` are the two directions of "never one that the other part carries". The include tests share the constant `PartialMemberIncludedDocumentation` (member `I` holds `<inheritdoc/>`, member `S` a summary).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8203….VerifyInheritdocInLaterCommentOfPartialMethodPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify(source)` | RED |
| RH8203….VerifyInheritdocInLaterCommentOfPartialPropertyPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify(source)` | RED |
| RH8203….VerifyInheritdocInLaterCommentOfPartialIndexerPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify(source)` | RED |
| RH8203….VerifyInheritdocInLaterCommentOfPartialEventPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify(source)` | RED |
| RH8203….VerifyIncludedInheritdocOfPartialMethodPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify` + `CreateIncludedDocumentationTransform` | RED |
| RH8203….VerifyIncludedInheritdocOfPartialPropertyPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify` + `CreateIncludedDocumentationTransform` | RED |
| RH8203….VerifyIncludedInheritdocOfPartialIndexerPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify` + `CreateIncludedDocumentationTransform` | RED |
| RH8203….VerifyIncludedInheritdocOfPartialEventPartIsJudgedPartByPart | RH8203-001, RH8203-002 | `Verify` + `CreateIncludedDocumentationTransform` | RED |

The existing per-part tests with the element in the part's own first comment (`VerifyPartial{Method,Property,Indexer,Event}IsJudgedPartByPart`, the spec example) stay unchanged and green; they are the direct-tag side.

### Not covered, and why
- A cref on an included or later-comment `<inheritdoc>` of a partial part: the cref exemption is one check for every declaration and is covered for included content by `VerifyIncludedInheritdocWithCrefIsNotReported`; the partial branch adds no condition on it.
- Parts in two documents with a later comment or include: the cross-document side is covered with direct tags (`VerifyPartialMethodImplementingInterfaceMemberAcrossDocumentsIsNotReported`); the new tests cover what the part's documentation is, which does not depend on the document.
- Documentation mode `None` (SHARED-DOC-008), CRLF: no layout and no fix; mode `None` is covered rule-wide by `VerifyNoDiagnosticsWhenDocumentationModeIsNone`.
- A part with both a direct `<inheritdoc/>` and a later one: outside the covered input (at most one top-level `<inheritdoc>`, RH8203-001's coverage paragraph).
- Failure classes: FC-001/FC-024 are the finding these tests pin; FC-003 both sides per kind as above; code-fix, formatter, and token-gap classes do not apply (RH8203 has no fix).

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

Files: `src/Reihitsu.Analyzer.Test/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzerTests.cs` (formatted with `scripts/format.sh`, no change). No production code touched.

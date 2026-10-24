Role: spec author
Commit: 49b71d9
Verdict: F1: spec fixed; F2: spec clarified; F3: no spec change, decision recorded

This is repair round 1, covering the three findings of `09-spec-checker.md`. This file lists only the changes. I edited spec files only.

## F1 — FMT-DOC-011's byte-order-mark example used an empty class (#1329, #1389)

**What I did.** The prose example now formats the lines `U+FEFF/// <summary>S</summary>`, `class C`, `{`, `    int A;`, `}`. The result is `U+FEFF/// <summary>`, `/// S`, `/// </summary>`, `class C`, `{`, `    int A;`, `}`.

**Why.** An example is a seed for tests and must be the formatter's own output. The full pipeline rewrites an empty class to `class C;`, which has nothing to do with the requirement. Every other example of the spec uses a member.

The new example matches the input and expected output of `ByteOrderMarkTests.RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark` exactly, so that test now derives from the spec as written. No test change is needed.

## F2 — RH8309-005 and the `/// &amp;` line (#1344)

**Decision: covered.** I made the spec say so; the test's tag and expectation stand.

**Why it is covered.** Answer 5 states the general principle that documentation XML is read the way the compiler reads it. The compiler reads `&amp;` as the character `&`, so a `/// &amp;` line is text with a character that is not white space. That is the first form RH8309-005 already names, and answers 5B and 6A withhold the fix for such text wherever it stands at the top level. The implementation agrees: `CanReorderElements` tests the `ValueText` of the text tokens, and the `ValueText` of `&amp;` is `&`.

**What I did.**
- RH8309-005 now states that text is read as the compiler reads it, so a character or entity reference counts as the character it stands for:
  - `&amp;` is text with a character that is not white space;
  - `&#32;` is white space.
- The example gains a seventh comment: `/// &amp;` between `<remarks>` and `<summary>`, which is reported and gets no fix.

**For the test implementer.**
- `VerifyNoCodeFixForEntityReferenceBetweenTheElements` keeps its `RH8309-005` tag.
- The positive side of the new sentence has no test yet: a `/// &#32;` line between the elements leaves the fix offered, because the `ValueText` is a space. A test with that input (fix offered) would cover it.

## F3 — Code owner of `DocumentationCommentUtilities` (#1241)

**Decision: no spec change.** `DocumentationCommentUtilities` stays a code owner of `formatter/documentation-comments.md` only, and `DocumentationCommentCodeFixUtilities` stays the SHARED-DOC code owner.

**Why.**
- `sdd/README.md` allows a type to be a code owner of at most one spec and assigns it by the behavior it implements. Most of `DocumentationCommentUtilities` implements FMT-DOC behavior:
  - `GetContinuationPrefix` (FMT-DOC-011);
  - the exterior and prefix normalization (FMT-DOC-005, FMT-DOC-006);
  - `IsWrittenBehindSource`, `DocumentsFollowingCode`, `IsStrandedBehindCode` (FMT-DOC-001, FMT-DOC-003);
  - `IsLeftAsWritten` and `ContainsElementWithoutOwnEndTag` (FMT-DOC-013);
  - the exterior alignment helpers.
- Only `HasOwnEndTag` is the SHARED-DOC-017 definition. Moving the whole type to SHARED-DOC would leave the formatter spec without the owner of most of its behavior.
- The implementer gave the reason for the placement (`07-implementer.md`): the formatter cannot reference the analyzer assemblies, and all three consumers need the definition.
- FMT-DOC-013 already lists SHARED-DOC-017 under `Uses`, so the dependency is recorded in the direction `sdd/README.md` asks for. The FMT-DOC spec relies on SHARED-DOC-017's definition and does not restate it.
- `DocumentationCommentCodeFixUtilities` still implements SHARED-DOC behavior of its own:
  - `GetElementRemovalSpan` implements SHARED-DOC-018;
  - `CanRewriteElement` applies SHARED-DOC-017's withhold for RH8107, RH8202, and RH8204.
- Splitting `HasOwnEndTag` into a type of its own would be a code change, outside this role. The specs are consistent as they stand.

## Whole run

- Changed files:
  - `sdd/specs/formatter/documentation-comments.md` (F1)
  - `sdd/specs/analyzer/RH8309.md` (F2)
  - this handover
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 51 passed, 0 failed.

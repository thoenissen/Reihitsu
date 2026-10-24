Role: spec checker
Commit: 9be9eaa
Verdict: #1329: consistent; #1389: consistent; #1241: consistent; #1344: consistent; #1242: consistent

Repair round 1 re-check of F1 to F3 of `09-spec-checker.md`. Inputs: `10-spec-author.md`, `11-cleaner.md`, the "Repair round 1" section of `05-test-implementer.md`, and `git diff 28d9f39..HEAD` (two spec files, one test file, handovers; no production file changed).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | None. | — |

## Closed findings
- **F1 (#1329, #1389): closed.** The FMT-DOC-011 byte-order-mark example now formats `U+FEFF/// <summary>S</summary>`, `class C`, `{`, `    int A;`, `}` and shows the rebuilt lines with `int A;` kept. It equals input and expected output of `ByteOrderMarkTests.RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark`, which keeps its `FMT-DOC-011` and `FMT-CLEAN-009` tags.
- **F2 (#1344): closed.** RH8309-005 now says text is read as the compiler reads it, so `&amp;` is text with a character that is not white space and `&#32;` is white space; the example gains a seventh comment (`/// &amp;`, reported, no fix). Three new tests in `RH8309…AnalyzerTests.cs`, all tagged and listed in the "Repair round 1" table with matching tags: `VerifyCodeFixIsOfferedWhenACharacterReferenceForASpaceStandsBetweenTheElements` (RH8309-005, RH8309-004), `VerifyCodeFixIsOfferedWhenAHexadecimalCharacterReferenceForATabStandsBetweenTheElements` (RH8309-005, RH8309-004), `VerifyNoCodeFixForCharacterReferenceForALetterBetweenTheElements` (RH8309-005). The spec-example test holds the new seventh comment, so it equals the spec example in full. `VerifyNoCodeFixForEntityReferenceBetweenTheElements` keeps its tag. No production code changed, and the new tests pass (cleaner: 44 passed in the RH8309 filter).
- **F3 (#1241): closed.** The spec author decided against a spec change and gave reasons under `sdd/README.md`'s one-owner rule: most of `DocumentationCommentUtilities` implements FMT-DOC behavior, only `HasOwnEndTag` is the SHARED-DOC-017 definition, and FMT-DOC-013 already lists SHARED-DOC-017 under `Uses`. I accept this; the code-owner lists are unchanged and consistent with `SpecStructureTests`.

## Checked (full checklist on the changed files)
1. Change doc: unchanged from `09-spec-checker.md`; the repair only clarifies RH8309-005 and fixes an example. The maintainer's no-change-doc decision still stands. ok.
2. Specs match the diff: RH8309-005 reading of references matches `CanReorderElements` (tests `ValueText` of text tokens); format intact (normative sentence first, `Why:` present, `{|RH8309:…|}` markup). ok.
3. IDs: no ID added or changed; ledger untouched since 28d9f39. ok.
4. Owners: no header changed. ok.
5. User documentation: `documentation/rules/RH8309.md` says "text" generally and is consistent; no change needed. ok.
6. Open questions: none new. ok.
7. Coverage: `scripts/test.sh --project architecture` on 9be9eaa: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`. ok.
8. Tags: the three new tests carry the tags their table rows list; the spec author's "for the test implementer" request (a `&#32;` test, fix offered) is covered. ok.
9. Nothing left behind: `ls sdd/work` shows only `doc-comment-element-safety`; the lead deletes it before publishing. ok.

Role: auditor
Commit: 947ef5a3ef0e9d5835c2d0036ddba580b723f9b2
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It noted that the test sources contain literal backslash-u sequences on purpose, and asked me to check the raw bytes; I did.

## Scope

- This is the repair delta against `7260b2f573530f2531c88580a5822e4cab9961af`, the commit of `13-auditor.md`.
- The audited head is `947ef5a3ef0e9d5835c2d0036ddba580b723f9b2`. The checkout is that SHA with a clean tree.
- `git ls-remote origin refs/heads/main` gives `53990ed5df40c0cb8ce6673ce31cefa2089ee59d`, and it is an ancestor of HEAD.
- The repair diff matches `14-spec-author.md` to `17-spec-checker.md`. It touches:
  - SHARED-DOC-012 in `sdd/specs/shared/element-documentation.md`: the normative sentence, the coverage sentence, the `Why:`, and two declarations added to the first example block.
  - The coverage sentence of RH8030-001 in `sdd/specs/analyzer/RH8030.md`.
  - One sentence each in `documentation/rules/RH8032.md` and `RH8033.md`.
  - In the tests: the two SHARED-DOC-012 example tests (in RH8032 and RH8033), one new test each in RH8032 and RH8033, and one test summary in RH8030.
  - Handovers 13 to 17.
- No production file changed: `git diff 53990ed HEAD -- src` touches only the 15 test files, with insertions only against the base.
- The scope did not grow beyond the three findings. `scripts/lane-check.sh tooling` reports: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- Raw bytes:
  - `od -c` shows a literal backslash before `u0061` in the spec's new `extension(ushort …)` declaration and in the RH8032 test copy.
  - `grep -c` finds backslash-u lines in these files: 4 in the shared spec, 4 in each of the RH8032 and RH8033 test files, and 1 in each rule page. This agrees with 16 and 17.
  - The test sources hold them in raw string literals, so the compiler sees the escapes as written.
- I did not read thoenissen/Reihitsu#1130. The lead reports that the "Q5, continued" text of `14-spec-author.md` was appended there.
- I re-ran no repository test. 15, 16, and 17 report 171 focused tests, 680 documentation tests, and 72 architecture tests passing.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 (medium, escape or format character in a `name` attribute) | closed | SHARED-DOC-012's coverage now excludes a `name` attribute that contains a C# Unicode escape sequence or a format character (Cf); the shapes go to Q5. Its normative sentence now says the parameter's name is its identifier's value: `@` removed, escapes decoded. The `Why:` agrees. The rule pages now say "write the name as plain text", which matches the case in coverage. No test has a `name` attribute with a backslash, a character reference, a non-ASCII character, or padding (script over the 15 files: none). The in-coverage side is tested on both sides by `RH8032…VerifyParameterDeclaredWithUnicodeEscapeIsDocumentedByItsDecodedName` and `RH8033…VerifyTypeParameterDeclaredWithUnicodeEscapeIsDocumentedByItsDecodedName`. |
| 2 (low, RH8030-001 "one documentation comment") | closed | Now reads "at most one documentation comment", as RH8031-001 does. |
| 3 (low, summary of `VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly`) | closed | Now reads "written as character references"; the test content is unchanged and matches. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | low | `sdd/specs/shared/element-documentation.md` SHARED-DOC-012, normative sentence and `Why:` | FC-023, FC-002 | PR-introduced (this repair) | The new definition reads "The parameter's name is the value of its identifier: without the `@` of a verbatim identifier, and with a Unicode escape sequence decoded". The `Why:` likewise says "with its escape sequences decoded". Both name two of the three steps C# applies to an identifier's value; the third is the removal of format characters, and that is what the code compares. A parameter whose identifier contains a format character is inside coverage, because the coverage exclusion is about the `name` attribute only. Such a parameter, documented by the plain name, is not reported and the compiler is silent. Harness evidence, same in `Diagnose` and `Parse` mode: `extension(int va` + U+200B + `lue)` with `<param name="value">` gives no RH8032 and no CS1572; so does the same identifier with U+200B written as a C# escape. If the list after the colon is read as complete, the name is `va` + U+200B + `lue`, so `name="value"` "differs" and the spec would demand a report the code does not make. Read as "the identifier's value", the spec is right; only the elaboration is incomplete. The spec checker noted the same shape in 17 as an observation. | Add "and with format characters removed" to the definition and to the `Why:`. An example is optional. Text only: no test asserts this shape, and the existing tests stay valid either way. |

Why `pass, text cleanup` applies: the only finding is a wording precision in a spec definition. The code, the diagnostics, the tests, the rule pages, and the rule metadata already agree with the intended reading ("the value of its identifier"). Fixing it changes no compiled behavior and no test expectation.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Parameter documented by name (SHARED-DOC-012, as repaired) | The XML-decoded `name` of each top-level `param`/`typeparam` in the compiler's documentation, compared ordinally with `Identifier.ValueText` (`@` removed, escapes decoded, format characters removed) | The compiler's `name` binder (CS1572/CS1573/CS1711); the repaired normative sentence, coverage sentence, and `Why:` | The `name` attribute shapes on which the rule and the compiler disagree (padding, escapes, format characters) are now all out of coverage, in Q5. On the parameter side, code and compiler agree: escaped and format-character identifiers documented by the plain name are not reported. The spec states escape decoding but not format-character removal (finding 1). | text cleanup |
| RH8030 summary checked (RH8030-001 coverage) | Unchanged code: first top-level `summary` of the first comment | Repaired coverage sentence ("at most one documentation comment") | They agree; a declaration without a comment is now inside coverage and reports nothing. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Parameter documented by name (SHARED-DOC-012) | Unchanged since `13-auditor.md`: four identical private copies (RH8032, RH8033, RH8101, RH8108), and the syntax reading in RH8102/RH8109. The repair changed no code. | Unchanged; the hint in `13-auditor.md` stands. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH8032…VerifySpecificationExampleParameterIsDocumentedByTopLevelElementWithItsName` (changed) | The revised first SHARED-DOC-012 block, verbatim with RH8032's marks. `extension(ushort` + escaped `a)` documented by `name="a"` is not reported; the other three marks are unchanged. | A diagnostic on the escaped `a`, or on the `queue` parameter of the escaped `U` block | Verify, exact markup | adequate. A script confirmed the spec block matches the test source verbatim, apart from per-rule marks. |
| `RH8033…VerifySpecificationExampleTypeParameterIsDocumentedByTopLevelElementWithItsName` (changed) | Same block with RH8033's marks. The escaped `U` documented by `name="U"` is not reported. | A diagnostic on the escaped `U` | Verify | adequate |
| `RH8032…VerifyParameterDeclaredWithUnicodeEscapeIsDocumentedByItsDecodedName` (new) | An escaped identifier is matched by its decoded name. Both sides: escaped `a` with `name="a"` is not reported; escaped `b` with `name="a"` is reported, with the span on the escape text (RH8032-002). | A report on the `byte` block, or none on the `sbyte` block | Verify | adequate. Distinct receivers (`byte`, `sbyte`), so there is no merge (FC-033). |
| `RH8033…VerifyTypeParameterDeclaredWithUnicodeEscapeIsDocumentedByItsDecodedName` (new) | Same for type parameters: escaped `U` with `name="U"` is not reported; escaped `V` with `name="U"` is reported on the escape text. | A report on the `List` block, or none on the `Stack` block | Verify | adequate. Distinct receivers. |
| `RH8030…VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly` (summary only) | Unchanged | Unchanged | Verify | adequate; the summary now matches the source |

## Failure classes dismissed

Repair delta only; the reasons in `13-auditor.md` hold for everything the repair did not move.

- FC-001, FC-003 — the repaired coverage boundary of SHARED-DOC-012 is drawn on the attribute side. Both sides of "escape decoded" are tested on the parameter side, for `param` and for `typeparam`. The new tests do not touch the attribute shapes left out of coverage.
- FC-016 — the sibling `typeparam` is repaired and tested together with `param`.
- FC-017 — the new tests assert the exact diagnostic set.
- FC-031 — the changed test summary and the new test summaries describe their sources.
- FC-032 — no compiled production change. 15, 16, and 17 ran on the final test sources (0f8a53e and later; HEAD adds only handovers).
- FC-033 — the new test sources use distinct receivers. The escapes are literal in raw string literals (bytes checked), so the harness does not decode them away.
- FC-034 — the revised spec block is used verbatim (script).
- FC-004 to FC-014, FC-018 to FC-030, FC-035 to FC-037 — the repair adds no code fix, rewrite, formatter change, patched code, or comment-only C# claim.

## Hints

- `17-spec-checker.md` prints the escapes decoded, for example "`extension(ushort a)`" and "such as `a` (`T`)". `14-spec-author.md` keeps them literal (18 backslash-u sequences). The files under review hold the literal escapes, so this is an artifact of how 17's text was written, not of the tree. When the lead posts the Q5 text from 14 to thoenissen/Reihitsu#1130, or has already posted it, it should check that the issue text kept the backslashes. A tool call can decode a backslash-u sequence into its letter, as happened to 17 and to an early fixture of mine.
- The hints in `13-auditor.md` are unchanged.

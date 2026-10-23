Role: auditor
Commit: cd326d4530e505e482b77bbd891239566ddacd2a
Verdict: pass

## Scope

- **Head and base.** The head is `cd326d4530e505e482b77bbd891239566ddacd2a`, checked out on `claude/specs-principles-p25-p31-x5tof1` with a clean tree. `origin/main` is `070359be885a4ea908527babec903f0047dfde43`. `git ls-remote origin refs/heads/main` returns the same SHA, and `git merge-base --is-ancestor` confirms it is merged into the head.
- **What I audited.** The repair delta `git diff 323a540 3d0ac69` against findings 8 and 9 of `16-auditor.md`. This is not a full audit.
  - The delta also includes the spec checker's finding 1 of `20-spec-checker.md`, which `21` to `24` repaired. That repair added SHARED-DIAG-004 to `Uses` in RH7105 and RH7106, added a per-part sentence to RH7105-001 and RH7106-001, and added three tests.
  - This is the same requirement (SHARED-DIAG-004 ¶2), not an unrelated surface, so the scope did not grow.
  - The delta touches four spec files, three analyzer test files, one decision entry and the run's handovers. `scripts/lane-check.sh tooling` at the head says `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- **Inputs.** `00-lead.md`, `16-auditor.md`, and `17` to `24`. My prompt carried no intended fix, suspected cause or hoped-for verdict.
- **Counterparts I opened outside the diff:**
  - specs: SHARED-ORDER-001 to SHARED-ORDER-004, SHARED-MODORDER-001, RH7104, RH2004, RH7105-003/004 and RH7106-003/004;
  - code: `RH7106ProtectedMustComeBeforeInternalAnalyzer`, `DeclarationModifierUtilities.GetModifiers`, `ModifierOrderingUtilities.TryGetRh7106Violation`, `DiagnosticAnalyzerBase.Initialize`, and `AnalyzerTestsBase<TAnalyzer, TCodeFix>.CreateTestDocument`.
- **Merge check (the concurrent run thoenissen/Reihitsu#1405).** I diffed `070359b` against `cd326d4`:
  - **`sdd/specs/shared/code-fixes.md`** differs from `main` only by this run's own text: the `Uses` line, the SHARED-FIX-003 sentence, and the SHARED-FIX-007 moved-code bullet, Why and two examples. SHARED-FIX-008 is `main`'s new text verbatim. Its only cross-spec reference, FMT-PIPE-007, was already in `Uses`. RH3103-006, which `main` added, says "The diagnostic is still reported" and points at `shared/code-fixes.md`. That agrees with this run's SHARED-FIX-003 wording ("withholding a fix never removes a report").
  - **`sdd/specs/requirement-ids.txt`** differs from `main` only by SHARED-DIAG-009 and SHARED-DIAG-010. `main`'s new IDs (FMT-BLANK-016, FMT-PIPE-013/014, FMT-STRUCT-024, RH3103-006, SHARED-COMMENT-012) are present. The file has 1444 unique lines and is sorted, and every `### <ID>` heading under `sdd/specs/` matches the ledger exactly.
  - `rg` finds no reference to SHARED-FIX-003, SHARED-FIX-007, SHARED-DIAG-004, SHARED-DIAG-008 to SHARED-DIAG-010, or SHARED-DOC-004 in the specs that `main` changed.
- **What I ran myself.**
  - A scratch harness outside the repository (`/tmp/claude-0/-home-user-Reihitsu/ded1387f-fb73-5446-bdec-6adda55e8865/scratchpad/docmode/run.cs`, fixtures under `.../scratchpad/r3/`). It runs one analyzer and the first action of its fix, in documentation modes `None` and `Parse`.
  - A diagnostics dump (`.../scratchpad/dump/diag.cs`) that prints every analyzer diagnostic and every analyzer exception.
  - `scripts/verify-text-only.sh --base 4b41f93 --head 6a3b7be`.
  - The RH7103, RH7105 and RH7106 test classes at the merged head, because `main` brought formatter changes and these fixes run through the formatter (FC-032). Result: `Passed!  - Failed: 0, Passed: 87, Skipped: 0, Total: 87`.
  - I trusted the cleaner's Release build and the architecture result at the head as stated.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1, 2, 4, 5, 6, 7 | closed | Unchanged since `16-auditor.md`. The repair did not touch them. |
| 3 | superseded | Superseded by 8, as `16` recorded. |
| 8 | closed | **The SHARED-DOC-004 move clause now asks only for what SHARED-ORDER-002 carries.** It applies to a fix that moves the documented declaration and not the code in front, in documentation mode `Parse` or `Diagnose`, with no preprocessor directive between the comment and the declaration. The clause now states mode `None` and the directive-separated shape as not covered. The three sentences split every fix that moves something into three cases: it moves neither, it moves only the declaration, or it moves the code in front, including a fix that moves both. No input falls under two verdicts or under none. **Why the covered side holds:** when documentation comments are parsed, the lexer never puts a `///` in trailing trivia. Without a directive in between, the comment is therefore in the moved declaration's leading trivia, which SHARED-ORDER-002 carries. **Harness, mode `Parse`, LF.** The comment travels with the moved declaration in each of these shapes (RH7103 unless named): a blank line between the comment and the declaration; an attribute list on the declaration; a `///` comment that continues over two more lines; a move across two members; RH7102; RH7109; RH7107 and RH7108 with the comment behind an accessor. `17` covers an ordinary comment in between. The RH7309 fix cannot reach the covered side: a region move either takes the code in front along, or puts its `#region` between the comment and the declaration. The question on thoenissen/Reihitsu#1407 holds the shapes that are not covered. |
| 9 | closed | **SHARED-DIAG-004 ¶3 now covers only which accessibility a part of a partial member states, or whether it states one, and the keyword order goes to ¶2.** The Why and the RH2004 caption say "the same accessibility", which matches the compiler. The harness confirms both sides. `private partial void M();` with `partial void M() { }` gives CS8799. `internal protected` on one part and `protected internal` on the other compiles for a partial method, a partial constructor and a partial event; `16` showed the same for a partial property. A new example and a new test pin the ¶2 side. ¶2 and ¶3 no longer give two verdicts for the same input. |
| `20` finding 1 | closed | RH7105 and RH7106 list SHARED-DIAG-004 in `Uses` and cite it in RH7105-001 and RH7106-001, in the shape of RH7104-001, as `sdd/README.md` line 47 requires. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Carry a behind-code `///` with a moved declaration (SHARED-DOC-004 ¶2) | The fix moves the documented declaration and not the code in front; mode `Parse` or `Diagnose`; no directive between the comment and the declaration | SHARED-ORDER-002 "in the moved declaration's leading trivia"; the lexer keeps `///` out of trailing trivia unless parsing of documentation comments is suppressed | Yes. Every harness shape listed under finding 8, and the tagged test, carries the comment. | ok |
| Moves not covered (SHARED-DOC-004 ¶2) | Mode `None`; a directive between the comment and the declaration; a move of the code in front, including a move of both | The "moves neither" sentence, which leaves the comment where it stands | The cases are disjoint and together cover every move | ok |
| Partial member judged as a whole (SHARED-DIAG-004 ¶3 → ¶1) | Which accessibility a part states, or whether it states one | CS8799 (harness); RH2004-002; thoenissen/Reihitsu#1287 | Yes | ok |
| Partial part judged on its own (SHARED-DIAG-004 ¶2; RH7105-001, RH7106-001) | The order of the modifiers one part carries, including the accessibility keywords of a partial member | Syntax-node actions per declaration in `RH7105…Analyzer` and `RH7106…Analyzer`; SHARED-MODORDER-001 | Yes for types, methods, properties and events. Not for the defining part of a partial constructor, which neither analyzer ever reports. That gap also exists on `main` against SHARED-MODORDER-001 (hint 1). | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Comment carried with a moved declaration | Spec: SHARED-ORDER-002 owns the mechanism, and SHARED-DOC-004 now defers to it. Code: `TypeMemberOrderingCodeFixProviderBase` (RH7102, RH7103, RH7109) and `AccessorOrderingUtilities` (RH7107, RH7108). `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider` reaches only shapes that are not covered. | One owner |
| Per-part or whole judgement of a partial declaration | SHARED-DIAG-004 ¶1 to ¶3. RH7104-001, RH7105-001 and RH7106-001 cite it. RH2004 is covered by ¶3 and is still out of line with it, which is tracked by thoenissen/Reihitsu#1287. SHARED-DOC-001 excludes partial elements. No other spec states the policy. | One owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH7106…Tests.PartOfAPartialPropertyIsReportedAndFixedOnItsOwn` | Exactly one RH7106 on `protected` of the `internal protected` part; the fix swaps only that part; the canonical part stays and is never reported | A diagnostic on the second part, none on the first, or a change to the second part | `Verify(source, fixedSource)` on `BatchCodeFixTestsBase`: exact markup and exact fixed text, and no diagnostic may remain | Adequate. It is the spec example verbatim, so FC-035 does not apply. |
| `RH7106…Tests.PartOfAPartialTypeIsReportedAndFixedOnItsOwn` | The same, for a nested partial class | The same | The same | Adequate. The `public const` members keep the fixed output to the modifier swap. |
| `RH7105…Tests.PartOfAPartialMethodIsReportedAndFixedOnItsOwn`, `…PartOfAPartialTypeIsReportedAndFixedOnItsOwn` | One RH7105 on `static` of the `static public` part (RH7105-003); the fix reorders only that part | The same | The same | Adequate. `22` checked that removing the markup makes each test fail. |
| `RH7103…Tests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (summary only) | Unchanged | Unchanged | `ApplyCodeFixAsync`, which runs in mode `Parse` because `CreateTestDocument` builds `CSharpParseOptions` without a documentation mode | The summary is accurate. `verify-text-only.sh` says of the file: `text-only … 3008 tokens identical (173 literal), directives and disabled text identical, changed trivia: documentation`. |

## Failure classes dismissed

- FC-001 and FC-002: both narrowed sentences state their qualifiers explicitly, and the cases they leave out are stated as not covered. See the guard-scope table.
- FC-003: both sides are tested.
  - SHARED-DOC-004: the covered side is pinned. The side that is not covered is deliberately unpinned, because the spec states no requirement for it.
  - SHARED-DIAG-004: the ¶2 side is pinned by the new tests, and the ¶3 side by the existing RH2004 partial-property tests.
- FC-004, FC-019, FC-021, FC-026, FC-029: no code that edits gaps or joins lines changed. The new tests swap modifier tokens in place.
- FC-005 and FC-020: the rescoped clause names directives explicitly. Ordinary comments and blank lines in between stay on the covered side and conform (harness).
- FC-006, FC-022, FC-023, FC-025, FC-027, FC-036: the repair changes no rewrite, literal handling, rename or nesting logic.
- FC-007, FC-008, FC-009, FC-018, FC-028, FC-030: no fix or formatter code changed. Fix All runs through the unchanged bases.
- FC-010 to FC-014: no formatter or analyzer change.
- FC-015 and FC-016: see the policy-ownership table. Sibling move fixes (RH7102, RH7109, RH7107, RH7108) conform on the covered side.
- FC-017: see the assertion-adequacy table.
- FC-024: partial types, methods, properties, events and constructors were checked with the harness. The partial-constructor gap is hint 1.
- FC-031: the four new summaries and the reworded RH7103 summary describe their code.
- FC-032: the three touched test classes pass at the merged head (87 of 87).
- FC-033: the RH7103 summary now states the documentation mode that the harness of that test applies.
- FC-034 and FC-035: the tests pin the spec examples and the spec sentences, not a patched copy of the code.
- FC-037: the RH7103 summary change is proven text-only by the script, not by `grep`.
- No TODO, no tracker number in an added spec or source line, and no edit outside the run's scope.

## Hints

1. **RH7105 and RH7106 never report the defining part of a partial constructor.** The cause is not established.
   - Input: `public partial class C { internal protected partial C(); internal protected partial C() { } }` reports RH7106 only on the second part. The same holds for RH7105 with `unsafe internal partial C();`.
   - The same analyzers do report:
     - a non-partial bodiless constructor (`extern internal protected C();`);
     - the implementing part of a partial constructor;
     - the defining parts of partial methods, properties and events.
   - `DiagnosticAnalyzerBase.Initialize` filters nothing, and the dump shows no analyzer exception.
   - The behavior breaks SHARED-MODORDER-001 ("every … constructor … declaration") and RH7105-001/RH7106-001 as they stand on `main`, so the PR does not cause it. The new per-part sentences and ¶3's list of partial members now state the same thing explicitly.
   - No open issue tracks it (search on thoenissen/Reihitsu). It is a follow-up candidate for the maintainer, and the run report should name it.
2. **More evidence for Q2 on thoenissen/Reihitsu#1407 (mode `None`).** RH7103 splits a behind-code `///` that continues over two more lines:
   - its first line (`/// <summary>`) stays behind `Run`'s brace;
   - its continuation lines travel with `Create`.
   The lead may add this to the question.
3. **Still standing, now with more evidence.** In mode `Parse`, the RH7103 fix rewrites the carried single-line `/// <summary>Creates</summary>` to three lines, and it drops the blank line between the moved members. This is the whole-type formatting of thoenissen/Reihitsu#969, which `11` and `16` already hinted. The new shapes add no new class of defect.
4. **Updated from `16`.** Only part of ¶3 of SHARED-DIAG-004 now has a test that can fail.
   - The keyword-order sentence is pinned by `PartOfAPartialPropertyIsReportedAndFixedOnItsOwn`.
   - The part about which accessibility a part states, or whether it states one, still has no such test. Real coverage comes with thoenissen/Reihitsu#1287.
5. **Hints from `16` that still stand, with unchanged evidence:**
   - RH2004-001 and RH2004-005 against ¶3 (thoenissen/Reihitsu#1287);
   - the open-question wording in the RH7103 non-goal;
   - SHARED-DIAG-009 bullet 1 without a test that can fail (thoenissen/Reihitsu#1334);
   - RH8402's private documentation-mode check (thoenissen/Reihitsu#1346);
   - SHARED-DIAG-010 conformance established only for RH8307, RH7103 and the SHARED-JOIN-001 rules.

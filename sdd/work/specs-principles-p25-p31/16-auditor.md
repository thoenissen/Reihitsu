Role: auditor
Commit: 619cae3866fe161b41adafb178e881e421cc83cb
Verdict: findings

## Scope

- Head: `619cae3866fe161b41adafb178e881e421cc83cb`, checked out on `claude/specs-principles-p25-p31-x5tof1` with a clean tree.
- Base: `origin/main` at `f52df4598d6e5634ca0a00d5a0444137d253ca72`. `git ls-remote origin refs/heads/main` returns the same SHA, and `git merge-base --is-ancestor` confirms it is merged into the head.
- This is the repair delta against `37795fd` (my handover `11-auditor.md`, audited at `79f3751`), not a second full audit. The scope did not grow: the repair touches only the three spec files, six analyzer test files, the run's handovers, and one decision entry. `scripts/lane-check.sh tooling` confirms the lane.
- Inputs: `00-lead.md`, `11-auditor.md`, `12-spec-author.md` to `15-spec-checker.md`, and `git diff 37795fd 619cae3`. Counterparts I opened outside the diff:
  - SHARED-ORDER-001 and SHARED-ORDER-002, SHARED-DOC-008, SHARED-FIX-006 and SHARED-FIX-007;
  - the RH2004, RH7104, RH7105, RH7106, RH7107 and RH7108 specs;
  - the RH2004 tests and `AnalyzerTestsBase<TAnalyzer, TCodeFix>.ApplyCodeFixAsync`;
  - thoenissen/Reihitsu#1287.
- My prompt carried no intended fix or hoped-for verdict.
- Evidence I ran myself: a scratch harness outside the repository (`/tmp/claude-0/-home-user-Reihitsu/ded1387f-fb73-5446-bdec-6adda55e8865/scratchpad/docmode/run.cs`). It references `Reihitsu.Tooling` and runs one analyzer and the first action of its code fix in an `AdhocWorkspace`, with the documentation mode set explicitly to `None` or `Parse`.
  - Fixtures: RH7103 (LF and CRLF), RH7102, RH7109, RH7309, RH7106.
  - Everything else was traced statically.
  - I trusted the cleaner's Release build and the roles' focused test results.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | SHARED-DIAG-004 ¶2 now covers only the accessibility of one part of a partial **type** (RH7104) and the order of a part's modifiers (RH7105, RH7106). That is the P28 reach. A new ¶3 puts the accessibility of a partial **member** under ¶1, as thoenissen/Reihitsu#1287 decides for RH2004. The Why no longer claims that a per-part correction of a partial member compiles. The new ¶3 has a scope defect of its own: finding 9. |
| 2 | closed | The RH2004 example's caption gives no symbol rationale any more. It points at RH7104 under ¶2. The test keeps its SHARED-DIAG-004 tag by the 1260 decision, which is the lead's call (see hints). |
| 3 | superseded by finding 8 | The contradiction I reported is gone in documentation mode `Parse`: the scoped clause no longer asks the RH7103 fix to leave the documentation of `Create` behind `Run`. The new test and my harness both confirm this. The sentence that replaced it is wider than SHARED-ORDER-002, which it cites. My own round-1 suggestion ("as SHARED-ORDER-002 states") had the same gap. |
| 4 | closed | The three summaries I named, plus `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced`, now say they pin today's behavior and name SHARED-FIX-006 and SHARED-FIX-007 for the decided behavior. Assertions are unchanged; the `765cdcf` text-only proof is in `13`. I compared each summary with its input and fixed code, and they match. |
| 5 | closed | Retitled. `rg` finds the old title nowhere outside `sdd/work/`. |
| 6 | closed | The non-goal points at SHARED-FIX-006 and no longer calls the matter an open question. |
| 7 | closed | One protected `Format` in `AnalyzerTestsBase<TAnalyzer, TCodeFix>`, which `AssertFormattingChangesNothing` also uses. Both private copies are removed. `VerifySpecificationFile` gained an `onConfigure` overload whose order matches the inlined body it replaces. Overload resolution is unambiguous: `null` cannot convert to the `DiagnosticResult` struct, and the two-argument calls bind only to the `params` overload. The other `FormatSyntaxTree` call sites in the analyzer tests predate the run and differ in shape. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 8 | high | `sdd/specs/shared/element-documentation.md` SHARED-DOC-004 ¶2, the sentence "A code fix that moves the declaration the comment documents MUST carry the comment with it, as SHARED-ORDER-002 states …", and its new RH7103 example; test `RH7103…Tests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` | FC-001, FC-016, FC-020, FC-033 | PR-introduced (repair `9fee74f`) | **The new MUST covers every comment that SHARED-DOC-004 counts as the declaration's documentation. SHARED-ORDER-002 carries only what is in the moved declaration's own leading trivia. The two spans differ, and today's fixes break the MUST in both gaps.** (a) Documentation mode `None`, which is the mode of a project that does not generate documentation (SHARED-DOC-008's Why). The lexer then keeps a behind-code `///` as trailing trivia of the token in front, so the comment is not in the moved declaration's leading trivia. The spec's own RH7103 example, LF and CRLF, gives `public static void Create() { }` / `public void Run() { } /// <summary>Creates</summary>`: the comment stays behind `Run`'s brace, which the example's prose says must not happen. RH7102 (`private int _a; /// <summary>B</summary>` + `private const int B = 1;`) and RH7109 (with `private readonly int _b;`) do the same. (b) A region directive between the comment and the declaration, in mode `Parse` too. Input: `} /// <summary>M</summary>` behind `Dispose()`, then `#endregion // IDisposable` and `#region Methods`, then `public void M()`. SHARED-DOC-004 ¶1 makes the comment `M`'s documentation, because directives do not separate them. The RH7309 fix moves the `Methods` region in front and leaves the comment in front of `#endregion // IDisposable` and the type's closing brace, where it documents nothing. Carrying it would take a comment across two region directives. In mode `None`, today's output matches P29's literal "other fixes leave it where it is". So the MUST states behavior that no principle decides and the code does not have, and no follow-up tracks it. The covering test cannot see gap (a): `ApplyCodeFixAsync` parses with the default `CSharpParseOptions`, which is mode `Parse`. `12-spec-author.md` noted that mode `None` was not established, but the sentence was left unscoped. | Limit the MUST to what SHARED-ORDER-002 carries: a comment in the moved declaration's own leading trivia, that is documentation mode `Parse` or `Diagnose` with no directive that starts the moved block in between. State mode `None` and the directive-separated shape as not covered. Both outputs look wrong, because the declaration loses its documentation, so the lead may file them as an open question on thoenissen/Reihitsu#1407 (`sdd/README.md`, Open questions). If the MUST is kept as written instead, the non-conforming shapes need a tracking issue, and the decided behavior needs a maintainer decision. |
| 9 | medium | `sdd/specs/shared/diagnostics.md` SHARED-DIAG-004 ¶3 ("are not written independently, because every part of the member has to write the same ones. A rule about them judges the member as a whole …") and the Why ("have to write the same accessibility modifiers") | FC-002 | PR-introduced (repair `9fee74f`) | **¶3's premise and scope are wider than the compiler rule, so for RH7106 (and arguably RH7105) on partial members, ¶3 overlaps ¶2.** The compiler requires the parts of a partial member to state the same accessibility, not the same keywords. `internal protected partial int P { get; }` + `protected internal partial int P => 1;` compiles with no error (harness). Counterexample: `internal protected partial void M();` + `internal protected partial void M() { }`. RH7106 is "a rule about" the accessibility modifiers of a partial member, so ¶3 places it under ¶1, which reports at most once. ¶2 names RH7106 for "the order of the modifiers that one part carries", which reports each part and fixes it alone. Today RH7106 reports both parts and the fix changes one part, which compiles. That is the ¶2 reading, but the text gives both verdicts for the same input. | Say that every part has to state the same accessibility, and restrict ¶3 to a rule about which accessibility a part states, or whether it states one. The order of the keywords stays with ¶2 (RH7105, RH7106). Correct the Why sentence to match. |

Why this is not `pass, text cleanup`: both findings are normative spec text that decides what a rule reports and what its fix must produce. For a concrete input, the spec states an outcome the code does not produce and no decision supports (8), or two outcomes at once (9). This is the same standard as findings 1 to 3 of `11-auditor.md`.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Per-part judgement (SHARED-DIAG-004 ¶2) | the accessibility of one part of a partial type (RH7104), and the order of a part's modifiers (RH7105, RH7106) | issue P28 "Covers RH7104, RH7105, RH7106"; RH2004-002; ¶3 | Matches P28. Overlaps ¶3 for RH7106/RH7105 on partial members. | Finding 9 |
| Partial-member accessibility as a whole (SHARED-DIAG-004 ¶3) | "the accessibility modifiers of a partial member — a method, property, indexer, event, or constructor"; "a rule about them" | CS8799 (same declared accessibility; keyword order free, per the harness); thoenissen/Reihitsu#1287; RH2004-001 and RH2004-005 | The premise is wider than the compiler rule. The RH2004 rule spec is not aligned (hint). | Finding 9 |
| Fix that moves neither the declaration nor the code in front (SHARED-DOC-004) | every fix except RH8303's | RH8304/RH8308 behind-code tests; RH8305/RH8307 (thoenissen/Reihitsu#1407); RH7103 whole-type formatting (thoenissen/Reihitsu#969) | Yes. The non-conformances are tracked or hinted. | ok |
| Fix that moves the documented declaration (SHARED-DOC-004) | every fix, every documentation mode, any trivia between the comment and the declaration | SHARED-ORDER-002 "in the moved declaration's leading trivia" (mode-dependent; a region block starts at its `#region`); SHARED-ORDER-001 "the moved node, then the crossed nodes" | No, in mode `None` and with a region directive in between | Finding 8 |
| Fix that moves the code the comment stands behind (SHARED-DOC-004) | not covered | SHARED-ORDER-001: in a member move, the code in front is crossed, not moved | Disjoint from the row above for member moves | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Formatter output for analyzer test comparison | `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Format`. The `FormatSyntaxTree` calls in `Formatter/Formatting/*FormatterTests` and in `RH5111…Tests` predate the run and differ in shape (async root, `TestContext` token, two passes). | One owner |
| RH8402 specification-file test setup | `VerifySpecificationFile`: two overloads, one body | One owner |
| Carrying a behind-code `///` with a moved declaration | Spec: SHARED-DOC-004 (new sentence) and SHARED-ORDER-002. Code: `TypeMemberOrderingCodeFixProviderBase` (RH7102, RH7103, RH7109) and `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider` | The spec owners disagree in scope: finding 8 |
| Accessibility of a partial member | SHARED-DIAG-004 ¶3; RH2004-001, RH2004-002, RH2004-005; thoenissen/Reihitsu#1287 | ¶3 and #1287 agree. RH2004-001/005 still say one diagnostic per declaration (hint). |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7103 `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (LF, CRLF) | In mode `Parse`: `Create` precedes `Run`, `Create`'s leading trivia holds exactly one `///` whose summary is `Creates`, and that text occurs once | The comment stays behind `Run` (zero comments on `Create`), is duplicated, or the members are not reordered | `Verify(source)`; `ApplyCodeFixAsync`, which uses default `CSharpParseOptions`, i.e. mode `Parse`; structural assertions | Adequate for mode `Parse`. The `commentsInRun` assertion is redundant there: a `///` left behind `Run`'s last brace re-parses into the type brace's leading trivia, outside `Run`. The count on `Create` catches that case. Mode `None` is not exercised: finding 8. |
| RH8307 `VerifyHalfWrappedElementThatTheFormatterChangesIsReportedAndFixed`; RH7309 `MovedRegionBlocksKeepTheirLayout` (+ CRLF) | Unchanged | Unchanged | Shared `Format`, same body as the removed copies | Adequate |
| `AssertFormattingChangesNothing` (RH7103, RH5417, RH7310, RH7309 users) | Unchanged: formatting the code changes nothing | Formatter output differs | Same assertion and message | Adequate |
| RH8402 `FileNotStartingWithHeaderWhenDocumentationModeIsParse` | Reported in mode `Parse` | No diagnostic | `VerifySpecificationFile` overload; `onConfigure` runs after the file, the configuration and `SkipSuppressionCheck`, as before | Adequate |
| Reworded summaries: RH7107 ×3, RH7108 ×1 | Assertions unchanged; untagged | — | — | The summaries match the input and the fixed code |

## Failure classes dismissed

- FC-003: the new RH7103 test adds the moved-declaration side next to the existing "moves neither" tests (RH8304, RH8308). The missing side is mode `None`, which is part of finding 8.
- FC-004, FC-005, FC-019, FC-021: no code that edits gaps changed. FC-020 is reached only through finding 8 (a region directive between the comment and the declaration).
- FC-006, FC-022, FC-023, FC-025, FC-026, FC-027, FC-029, FC-036: the repair changes no rewrite, literal handling, rename, or nesting logic.
- FC-007, FC-008, FC-009, FC-018, FC-028, FC-030: no fix or formatter code changed. Fix All of the touched classes runs through the unchanged base classes.
- FC-010, FC-011, FC-012, FC-013, FC-014: no formatter or analyzer change.
- FC-015: the helper duplication is closed (finding 7). The spec-level ownership is listed above.
- FC-017: covered in the table above.
- FC-024: partial types and members are what ¶3 is about. The compile behavior of the parts was checked with the harness (finding 9).
- FC-031: the new `Format` summary, the new `VerifySpecificationFile` overload summary, the new RH7103 test summary, and the four reworded summaries describe their code. `AssertFormattingChangesNothing` keeps an accurate summary.
- FC-032: no compiled production change. The test results in `13` and `14` were taken at the heads they name.
- FC-034, FC-035: the new test pins the spec example itself. Its expected structure comes from the spec's prose after-state, not from a patched copy.
- FC-037: the summary-only commit `765cdcf` is proven by `scripts/verify-text-only.sh` (`13`). The helper and test commits are not claimed text-only.
- No TODO, no tracker number in any added source or spec line, and no edit outside the run's scope.

## Hints

- **RH2004-001 and RH2004-005 still require one diagnostic per declaration, including both parts of a partial property, indexer or event.** The new SHARED-DIAG-004 partial-property example says "at most once". thoenissen/Reihitsu#1287 lists RH2004-001 and SHARED-DIAG-004 for change, and the RH2004 characterization summaries merged with thoenissen/Reihitsu#1359 already state the decided behavior. So the divergence is tracked, not introduced, but the run report should name it.
- **¶3 of SHARED-DIAG-004 has no test that can fail.** The partial-property example is unpinned by the 1260 no-conforming-shape precedent. The RH2004 partial-method shapes pass because of RH2004-002. Real coverage comes with thoenissen/Reihitsu#1287. The RH2004 partial-type test keeps a SHARED-DIAG-004 tag that no sentence of the requirement can fail; that is the lead's decision.
- **`sdd/specs/analyzer/RH7103.md` still has a pre-existing non-goal that ends "whether that is intended is an open question"** (members without an accessibility modifier). `sdd/README.md` forbids an open question in a non-goal. The run did not touch it, and it is unrelated.
- **Hints from `11-auditor.md` that still stand.** The evidence is unchanged.
  - SHARED-DIAG-009 bullet 1 has no test that can fail; thoenissen/Reihitsu#1334 brings real coverage.
  - The RH7103 whole-type formatting moves an unrelated behind-code `///`. That now breaks the scoped "moves neither" clause of SHARED-DOC-004 and is a candidate note for thoenissen/Reihitsu#969.
  - RH8402's private documentation-mode check is the subject of thoenissen/Reihitsu#1346.
  - SHARED-DIAG-010 conformance is established only for RH8307, RH7103 and the SHARED-JOIN-001 rules.

Role: auditor
Commit: 37f220d8fb2d6839bf538ad22f5ddb4e24e2fe4c
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Full audit of thoenissen/Reihitsu#1415, lane tooling.
- Head `37f220d8fb2d6839bf538ad22f5ddb4e24e2fe4c`. The checkout is that commit, the working tree is clean, and the branch on `origin` points to it.
- Base `c81091b883383d0735af167e15603fb702f1d7b4`. It equals `git ls-remote origin refs/heads/main` and is an ancestor of the head.
- The merge `37f220d` brought in only `AGENTS.md`, `CLAUDE.md` and `squad/lead.md` from main. No file of the run is among them and no compiled file changed, so the earlier build and test results stay valid (FC-032).
- The diff is what the handovers describe:
  - spec wording in `shared/diagnostics.md`, `shared/code-fixes.md`, `shared/element-documentation.md`, and RH2004, RH3003, RH3103, RH5412;
  - one ledger line;
  - eight analyzer test files: three new test methods, plus tags added and removed;
  - `sdd/work/principles-p38-p39/`;
  - four decision entries.
- `scripts/lane-check.sh tooling` gives `confirmed — claimed tooling, diff needs tooling`. No path lies outside those groups.
- Fixtures were written to the session scratchpad, outside the repository, and run with `scripts/apply-fix.sh` (documentation mode `Parse`, `LanguageVersion.Latest`, LF and CRLF).

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/shared/code-fixes.md` SHARED-FIX-003, fifth condition and **Documentation** bullet; reached through `TypeMemberOrderingCodeFixProviderBase` (line 107), `RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider` (line 61), `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider` (line 47), `RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider` (line 50) | FC-016, FC-030, FC-004 | same requirement | See "Finding 1" below the table. | Do not change code or spec in this run (decision `2026-10-23-1415-conforming-examples.md`). The lead names this instance class in a follow-up, for example a comment on thoenissen/Reihitsu#1384 that asks for a code-fix-surface regression test next to the formatter tests. The run report lists it with the other known violations of the new requirements. |
| 2 | low | `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`, summary of `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` | FC-031 | PR-introduced | The method gained `[Requirement("SHARED-FIX-003")]` in this diff. Its summary still says "no preprocessor directive stands between the comment and `Create`, the shape the specification covers". That was the boundary of the old SHARED-DOC-004 text, which did not cover a directive between them. The amended SHARED-DOC-004 covers that shape too: its new `#if DEBUG`/`#endif` example, and the new sibling test `FixCarriesADocumentationCommentBehindCodeAndAConditionalPairWithTheMovedDeclaration`. The sentence now implies a coverage limit the spec no longer has. Comment-only; it cannot reach compiled behavior. | Reword the sentence to state only that this input has no directive between the comment and `Create`, and drop "the shape the specification covers". |

### Finding 1

The amended SHARED-FIX-003 forbids a fix whose result "splits a declaration with several declarators and keeps the comment on one part only". In mode `Parse`, every code fix that formats a whole type declaration through `ReihitsuFormatter.FormatNodeInDocumentAsync` inherits the formatter's field split (FMT-STRUCT-021/022). It splits a documented multi-declarator field inside that type, including a field the fix is not about, and `Y` loses its generated documentation.

Counterexample, RH7103:

```csharp
public class C
{
    /// <summary>The coordinates</summary>
    public int X, Y;

    public void Run()
    {
    }

    public static void Create()
    {
    }
}
```

`apply-fix.sh RH7103` gives `fixed (1 action(s), 1 iteration(s))`, LF and CRLF. The fix moves `Create` in front of `Run` and also rewrites the field as `/// <summary>⏎/// The coordinates⏎/// </summary>⏎public int X;⏎public int Y;`.

The same split occurs in these fixtures:

- RH7103 and RH7109 when the documented static or readonly field is the moved member;
- RH7104 (`partial class C { /// <summary>The coordinates</summary> public int X, Y; }`);
- RH5110 (`public class C<T> where T : class { … }`);
- RH8305 fixing the class's own summary.

RH7102 shares the ordering base. Fixes that format only the field declaration do not split it: RH8305 and RH7105 on the field itself leave `X, Y` intact.

No handover, filed follow-up, or listed contradiction names this instance class:

- the skeptic's reach analysis lists only RH7101 for splits;
- the spec checker lists RH3103-006, RH7101-004 and RH7309-004 as the active contradictions;
- the comment on thoenissen/Reihitsu#1336 covers the RH7101 fix only;
- thoenissen/Reihitsu#1384 makes the formatter refuse the split, which would close every instance. Its done-when, however, asks only for formatter regression tests, so nothing would pin the code-fix surfaces.

No existing test asserts the inherited split: `rg` found documented multi-declarator fields only in `FieldDeclarationSplitTests` (formatter, FMT-STRUCT-022, #1384) and in analyzer-only RH8026 and RH8030 tests.

## Guard scope

No predicate, guard, or exemption in code is added or changed: the diff touches no production or tooling code. The rows below check the spec-level decisions the diff adds against their counterparts.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-FIX-003, fifth condition: the fix changes which declarations a documentation comment documents | The set of declarations each documentation comment documents, before and after the fix. Modes `Parse`/`Diagnose`, comments as SHARED-DOC-004 defines them; a multi-declarator field or event counts each declarator. | SHARED-DOC-004, mode `Parse`/`Diagnose`: carry the comment, or the fix is not offered. FMT-STRUCT-021/022: the split keeps the comment on the first field, and formatter-delegating fixes inherit it. RH7101-004 says the same. | SHARED-DOC-004 agrees. FMT-STRUCT-022, as reached through fixes that format a type, disagrees. RH7101-004 disagrees; tracked by #1336. | Finding 1 |
| SHARED-FIX-008, the code a fix changes | The span the fix replaces, rewrites, or moves, its trailing trivia included. A break outside it, such as the one ending the line of a change within the line, is not covered. | FMT-PIPE-007 detection (whole document). SHARED-FIX-007 (moved code keeps its line breaks in position). The 11 tests tagged SHARED-FIX-008 in RH5405, RH5406, RH5417, RH7303 and RH7310. | Agrees with decision `2026-10-23-1415-p35-fix-scope.md`, Q3 (b). Every tagged test asserts only created breaks or literal/comment content. None asserts a kept in-span break that keeps its old ending, and none asserts a break outside the span. | ok |
| SHARED-DIAG-011: the change cannot be written at the position | The position of the construct: grammar, language version, or a compile error the position itself causes. A collision with the rest of the code is excluded. | SHARED-FIX-003's "still reported" sentence (the change can be written, only the fix is withheld). SHARED-DIAG-008 and SHARED-DIAG-010 exemptions. | Both requirements use "can be written at that position", and both cite the RH4106 collision. "MUST be reported as the rule's own requirements state" is the wording SHARED-DIAG-010 already uses next to SHARED-DIAG-008, so it does not override the other exemptions. | ok |
| SHARED-DOC-004, mode `None`: a `///` behind code stays with the code in front | Trailing trivia of the token in front of the moved declaration | SHARED-ORDER-002: only the moved declaration's leading trivia travels | Agrees; `FixLeavesACommentBehindCodeWhereItStandsWhenDocumentationCommentsAreNotParsed` pins the full result | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which declarations a documentation comment documents | SHARED-DOC-004 (`shared/element-documentation.md:209`), cited by the SHARED-FIX-003 **Documentation** bullet. The clause that a comment of a declaration with several declarators documents each declarator exists only in SHARED-FIX-003 (`rg -i declarator sdd/specs`). SHARED-DIAG-006 (`diagnostics.md:342`) agrees that a field declaration with two declarators declares two fields. | One owner |
| A fix that would detach documentation is withheld | SHARED-FIX-003, fifth condition (owner). SHARED-DOC-004 refers to it ("MUST NOT be offered (SHARED-FIX-003)"). Contradicting rule requirements: RH7101-004 → FMT-STRUCT-022 (#1336, #1384) and RH7309-004 (#1262). Contradicting behavior inherited through `FormatNodeInDocumentAsync` in `TypeMemberOrderingCodeFixProviderBase`, RH7104, RH5110 and RH8305: untracked. | Finding 1 |
| Line ending of the line breaks a fix writes | SHARED-FIX-008 (owner) with FMT-PIPE-007 detection. Rule-level references only say "in the line ending SHARED-FIX-008 specifies": RH5011, RH8303, RH8304, `shared/region-directive-blank-lines.md`, `blank-line-after-brace-fixes.md`, `attribute-targets.md`, `statement-blank-lines.md`. | One owner |
| A rule does not report where its change cannot be written | SHARED-DIAG-011 (owner). Each rule's own position check is the code owner, as the requirement's non-goal assigns: RH3103 `ShouldSkip`, the RH3003 constant-context check, RH2004 `ShouldSkip`, and the `LanguageVersion` gates of RH5412–RH5416 and RH7003. RH3103-002, RH3003-002, RH2004-003 and RH5412-001 cite it; RH7003-002/-003 and RH5413–RH5416 agree without citing it. RH3103-006 contradicts it (#1417). RH3104 and RH5417 behavior contradicts it (#1422). | One owner; known contradictions tracked |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3103 `ShorthandIsAskedForOnlyWhereTheGrammarAdmitsIt` (new) | `nameof(Nullable<int>)` is not reported; `typeof(Nullable<int>)` is reported and fixed to `typeof(int?)` | A diagnostic on `A`, none on `B`, or other fixed text | `Verify(source, fixedSource)`: exact diagnostics and full fixed text | adequate |
| RH3003 `StringEmptyIsAskedForOnlyWhereNoConstantIsRequired` (new) | `const string F = "";` is not reported; `string _g = "";` is reported and fixed to `string.Empty` | A diagnostic on `F`, none on `_g`, or other fixed text | `Verify(source, fixedSource)` | adequate |
| RH7103 `FixCarriesADocumentationCommentBehindCodeAndAConditionalPairWithTheMovedDeclaration` (new, LF/CRLF) | In mode `Parse`, the `///` comment and the `#if DEBUG`/`#endif` pair travel with `Create`, in that order, one copy each, and nothing stays with `Run`; the summary text is `Creates` | `Create` not first; leading trivia without the comment or out of order; a directive or comment left with `Run`; a duplicate | `Verify(source)` for the report; `ApplyCodeFixAsync` (its `CSharpParseOptions` constructor defaults to `DocumentationMode.Parse`) plus a reparse in mode `Parse` | adequate. Where the comment starts is left unasserted, as SHARED-DOC-004 leaves it uncovered. |
| RH7103 `FixLeavesACommentBehindCodeWhereItStandsWhenDocumentationCommentsAreNotParsed` (new, LF/CRLF) | In mode `None`, `} /// <summary>Creates</summary>` stays behind `Run` | Any difference in the full fixed text | `Verify(source, fixedSource, …)` with `ApplyDocumentationModeNoneToTestProject` in `SolutionTransforms`; the verifier also checks that the fixed state reports nothing | adequate. The harness dependence (FC-033) was shown by the test implementer's mutation, which I did not re-run. |
| RH5406 `FixWritesLineFeedForTheLineBreakItKeepsInsideTheWrappedBodyInADocumentWhoseLineBreaksAreLoneCarriageReturns` (new) | In a lone-`\r` document, the break kept inside the wrapped body is `\n` | `\r` between `M(a,` and `b);`, or other indentation | `ApplyCodeFixAsync` on raw text; the input is built in the test and guarded by `Assert.DoesNotContain("\n", source)` | adequate for its clause. The created breaks are pinned by the sibling test; the break behind the new `}` is excluded by decision. |
| RH7103 `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (+SHARED-FIX-003) | After the move, the comment still documents `Create` (offered side of the fifth condition) | The comment not in `Create`'s leading trivia, duplicated, or left with `Run` | `ApplyCodeFixAsync` plus a reparse in mode `Parse` | adequate; summary stale (finding 2) |
| Tags added: RH3103 `NullableGenericInNameofIsNotReported`, `NullableGenericInTypeofIsReportedAndFixed`; RH3003 `EmptyStringInConstFieldIsNotReported`, `EmptyLiteralsWhereAConstantIsRequiredAreNotReported`; RH2004 `VerifyStaticConstructorAndExplicitInterfaceImplementationsAreNotReported`; RH5412 `VerifySpecificationExampleIsNotReportedUnderCSharp11`, `VerifyEmptyClassIsReportedUnderCSharp12` | The clause each one illustrates: grammar, constant, modifier, or language version, on the side named | A diagnostic where none is expected, or the reverse | `Verify` with exact diagnostics, and the language-version transform for RH5412 | adequate; each matches its SHARED-DIAG-011 example |
| Tags added: RH3001 `NegationAsAWholeIsPatternIsReportedWithoutACodeFix` (SHARED-DIAG-011, SHARED-FIX-003); RH4106 `NoFixIsOfferedWhenTheNewNameIsTaken` (SHARED-DIAG-011) | Reported, no fix, where the change can be written (RH3001), or only a collision blocks it (RH4106) | No report, or an action offered | `VerifyNoCodeFixIsOffered` (report and empty action list) / `Verify(source, source)` | adequate |
| Tag removed: RH3103 `DottedNullableGenericAsNameofArgumentDoesNotOfferFix`, `GlobalDottedNullableGenericAsNameofArgumentDoesNotOfferFix` (−SHARED-FIX-003) | They still assert RH3103-006 | — | — | correct. The amended SHARED-FIX-003 no longer names that shape, and SHARED-DIAG-011 says it is not reported. RH3103-006 still requires the report until #1417. |

## Failure classes dismissed

- FC-001, FC-002, FC-003: no code guard changed. The span and qualifiers of each spec-level decision are explicit (see Guard scope). Both sides of each SHARED-DIAG-011 clause, and both modes of SHARED-DOC-004, are covered by tests. The modifier clause has only its not-reported side tagged, while RH2004's ordinary reported cases exist untagged.
- FC-005, FC-019, FC-020, FC-021: the only new gap-touching test moves a conditional pair together with a documentation comment and asserts both. The RH7309 comment-guard gap is #1262.
- FC-006, FC-022–FC-027, FC-029, FC-036: no rewrite, literal, identifier, rename, or construct shape is changed.
- FC-007, FC-008, FC-009: no fix changed. The RH8305 `no-progress` observation is a hint.
- FC-010–FC-014: no formatter or analyzer change, and SHARED-DIAG-011 adds no report.
- FC-015: see Policy ownership. The second owner of the documented-split decision is finding 1.
- FC-017: see Assertion adequacy.
- FC-018: no naming fix.
- FC-028: reached only through finding 1's `FormatNodeInDocumentAsync` path. No code changes here, so there is no detached-versus-document divergence to compare.
- FC-031: checked every new and changed summary in the diff; finding 2 is the only stale one. RH3001's tagged test keeps "Characterizes" in its summary. That wording has precedent on tagged tests (`HorizontalSpacingTests`, FMT-SPACE-001/-005), and the text is accurate.
- FC-032: the merge from main changed no compiled file and no file of the run.
- FC-033: mode `None` is applied through the solution transform, and the lone-`\r` input is built and guarded in the test.
- FC-034, FC-035: every new test uses the spec example's exact shape, and its expected values come from the spec. No code was patched.
- FC-037: no comment-only proof is claimed for a `.cs` file.

## Hints

- **RH8305 no-progress, pre-existing and unrelated (FC-009).** `/// <summary>The type</summary>⏎public class C⏎{⏎}` at the start of a file gives `apply-fix.sh RH8305` `no-progress (2 iteration(s))` under LF and CRLF: the offered action never widens the summary. The same class inside a namespace gives `fixed`. This is a follow-up candidate.
- **Fixes that format the whole type, pre-existing (FC-030, SHARED-FIX-006).** In the finding 1 fixtures, those fixes also rewrite code their diagnostic is not about: the one-line `<summary>` of the untouched field is expanded to three lines, which is RH8305's concern.
- **SHARED-DIAG-011 citations are uneven.** RH3103-002, RH3003-002, RH2004-003 and RH5412-001 cite it. RH7003-002 and RH7003-003, and the RH5413–RH5416 language gates, are instances of the same principle but do not cite it. That is consistent but uneven.
- **RH3102-004 is a boundary SHARED-DIAG-011 does not illustrate.** Its Why says "removing the `;` would not compile". RH3102 asks for code without an empty statement, which `while (b) { }` satisfies, so reporting it is consistent. Still, no SHARED-DIAG-011 example shows a rule whose fix's rewrite is invalid at a position where another correction is valid.
- **SHARED-DIAG-011 sample, no counterexample.** RH3003 is not reported for `""` in these positions: a primary-constructor default, a lambda default, list and positional patterns, a constant interpolated string, `and ""`, and `""u8` (`apply-fix.sh RH3003`: `no-diagnostic` in all five fixtures). RH2004 counts `file` as an accessibility modifier (`IsExplicitAccessibilityModifier`) and does not register finalizers or extension blocks.
- **Duplicated test logic.** The two RH7103 carry tests repeat about 25 lines of reparse and summary extraction; a shared private assertion helper could serve both.

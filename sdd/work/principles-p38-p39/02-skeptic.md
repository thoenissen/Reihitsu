Role: skeptic
Commit: 9e6546a
Verdict: question for the maintainer

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I checked the spec diff (`git diff origin/main -- sdd/specs`) against the code, the existing tests, and the decision records. Fixtures were in the scratchpad, outside the repository. Compile probes ran in a scratch net10.0 library.

## Premise

What the run asks for is real. The four decisions of #912 round 10 are recorded wording. The issue's statements about "today" are only partly right, and several requirements in the diff state behavior the code does not have yet. The spec examples show those shapes as if they hold.

- **P38 / SHARED-DIAG-011.** Reading `RH3103UseShorthandForNullableTypesAnalyzer.ShouldSkip` shows that only a `nameof` argument made of qualified or alias-qualified names is skipped. Each of the following is reported today, and none conforms to SHARED-DIAG-011:
  - `nameof(System.Nullable<int>)`: reported, fix withheld (tests `DottedNullableGenericAsNameofArgumentDoesNotOfferFix` and `Global…`).
  - `Nullable<int>.Equals(1, 2)`: reported, and the fix is **offered** and writes `int?.Equals` (`FixOfNullableGenericReceivingAStaticCallWritesTheShorthand`).
  - `typeof(Nullable<>)`: reported, fix **offered**, writes `typeof(?)` (`FixOfUnboundNullableGenericInTypeofWritesAQuestionMarkOnly`).
  - `nameof(Nullable<int>.HasValue)`: reported, fix offered.
  - The using alias: the analyzer has no `LanguageVersion` check, so it is reported under C# 11 as well.

  The issue says the fix is withheld for all of these; that holds only for the dotted `nameof` inputs. **New:** `using static System.Nullable<int>;` is reported too. `apply-fix.sh RH3103` gives `invalid-result`, because the fix writes `using static int?;`, which does not parse (CS1001). It is another grammar case that the issue, #1417, and the spec author did not list.
- **P39 / SHARED-FIX-003 fifth condition and the SHARED-DOC-004 `Parse` sentence.** `apply-fix.sh RH7309` (documentation mode `Parse`, LF and CRLF) gives `fixed` for all three shapes. A compile probe with `GenerateDocumentationFile` shows the compiler documents `M` in all three:
  1. the behind-code shape from the issue: the comment stays behind `Dispose()`, which confirms #1262's premise;
  2. `///` on its own line directly above `#region Methods`: the comment stays between the blocks and ends up above `#region IDisposable`;
  3. `///` on its own line inside the `IDisposable` block, directly above `#endregion`: the comment stays in that block.

  **The spec author's statement that shape 2 "conforms today" is wrong.** `CanReorderSafely` checks directives only, and the RH7309 fix has no comment guard at all. "Holds no comment outside its top-level region blocks" in RH7309-004 limits the requirement's scope; it does not withhold anything. Shape 2 is #1262's #1185-Q1 instance. Shape 3 is in neither #1262 nor the spec. RH7103 in mode `Parse` conforms to the new SHARED-DOC-004 text (`apply-fix.sh RH7103`):
  - `} /// …` followed by `#if DEBUG`/`#endif` and `Create`: the fix carries the comment and the directives.
  - `#pragma` between them, behind code or on its own line: no fix is offered, for the directive reason.
- **P35 / SHARED-FIX-008.** Not run. `apply-fix.sh` turns every fixture into LF or CRLF, so it cannot reproduce a lone `\r` (FC-033). `FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` asserts only the breaks the fix creates. The claim that the kept break behind `}` stays `\r` comes from the issue and #1388. The new sentence "the line breaks in front of `if` and behind the closing brace of `M` … stay a lone `\r`" has not been verified.
- **P29 / SHARED-DOC-004 mode `None`.** Derived from code, not run:
  - `OrderingMoveSafety.MoveNodeBeforePreservingSeparators` moves only leading trivia.
  - In mode `None` the lexer puts `///` behind `}` into that token's trailing trivia.
  - `DocumentationCommentFormattingPhase.RegisterRelocation` relocates only `SingleLineDocumentationCommentTrivia`.

  So the RH7103 example should conform. The RH7309 text swap moves the behind-code comment together with `Dispose`'s block, so the RH7309 mode-`None` sentence should conform as well.

## Mechanism and candidates

_N/A._ This is a spec run. These are the candidate sets I derived from the code for the new requirements:

- SHARED-DIAG-011 reaches every rule. RH3103's dispatch is every `GenericName` with one type argument that binds to `Nullable<T>`. Positions where `T?` does not parse:
  - a `nameof` argument of any shape;
  - the receiver of a member access;
  - an unbound `typeof`;
  - `using static`;
  - a using alias below C# 12.

  Existing instances that conform:
  - RH3103-002;
  - RH7003-003 and RH5412-001 (and RH5413–RH5416), which are gated on the language version.

  An instance outside RH3103 that does not conform: RH5417 reports a one-line lambda block body inside a non-verbatim interpolation hole (`apply-fix.sh RH5417`: `no-fix-offered`, under `Latest`). The analyzer has no version gate. Under C# 10, the line breaks it asks for are CS8967 (probe).
- The fifth condition of SHARED-FIX-003 reaches every fix that moves, splits, or rewrites a documented declaration:
  - the `TypeMemberOrderingCodeFixProviderBase` family (RH7102, RH7103, RH7109);
  - RH7107/RH7108 (accessors);
  - RH7309 (text swap of region blocks);
  - RH7101 (field split, see the questions).

  No code fix removes a member declaration (only RH3102, RH3106, and RH3107 call `RemoveNode`).

## Breaking inputs

These are for the test implementer. Only tests that conform today belong in this lane.

- SHARED-DIAG-011 coverage, conforming:
  - `NullableGenericInNameofIsNotReported`;
  - `NullableGenericInTypeofIsReportedAndFixed`;
  - the untagged `NegationAsAWholeIsPatternIsReportedWithoutACodeFix` (RH3001 `x is !true`);
  - the RH7003-003 and RH5412-001 version tests.

  The C# 11 alias, `using static`, `.Equals`, and `typeof(Nullable<>)` belong to #1417.
- The tests `DottedNullableGenericAsNameofArgumentDoesNotOfferFix` and `GlobalDottedNullableGenericAsNameofArgumentDoesNotOfferFix` are tagged `SHARED-FIX-003`. The amended SHARED-FIX-003 says that such a construct "is not reported (SHARED-DIAG-011)", so the tag now contradicts the requirement. RH3103-006 still asserts the report. Precedent: squad/decisions/2026-10-21-1139-shared-comment-001-contradicting-tests.md.
- SHARED-DOC-004 mode `None`: the RH7103 example must run through `ApplyDocumentationModeNoneToTestProject`. Otherwise it tests `Parse` (FC-033).
- SHARED-DOC-004 `Parse`: RH7103 with `#if DEBUG`/`#endif` between the comment and the declaration carries both. That is the counter-check from #1262, and no test pins it. Fixtures with `#pragma` are withheld for the directive reason, so a tag on them would be incidental.
- RH7309 shapes 1–3 are #1262's. None of them is a conforming test for the fifth condition. I found no input for which a fix is withheld today *because* it would separate the documentation.
- SHARED-FIX-008: lone-`\r` input has to be built in the test (FC-033). Assert each line break explicitly, not a comparison after normalizing (FC-017).
- Classes: FC-004, FC-005 (a comment guard taken to cover documentation comments: RH7309 has none), FC-006 (language-version gates), FC-016 (sibling shapes 2 and 3), FC-017, FC-033.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `shared/diagnostics.md` SHARED-DIAG-011 | must change, depending on Q1 and Q4 |
| `shared/code-fixes.md` SHARED-FIX-003 | must change: the sentence about the own-line comment above `#region Methods` is not today's behavior. Also Q2, Q4 |
| `shared/code-fixes.md` SHARED-FIX-008 | Q3, Q4 |
| `shared/element-documentation.md` SHARED-DOC-004 | Q4 (the RH7309 pointer). The mode-`None` text matches the code as I read it |
| `analyzer/RH3103.md` | must stay. The contradiction between RH3103-006 and SHARED-DIAG-011 belongs to #1417 |
| RH3103 analyzer and fix, RH7309 fix, RH5406 fix | not involved (#1417, #1262, #1388) |
| Ordering fixes (`OrderingMoveSafety`), RH7103 | must stay |
| RH7101-004, FMT-STRUCT-022 | must stay, unless Q2 is answered (b) |
| Formatter | not involved. No report is added that the formatter could produce or would have to own (FC-013, FC-014) |
| Rule pages, `general-notes.md` | not involved: shipped behavior does not change |
| Tests | must change: coverage for SHARED-DIAG-011, the two contradicting `SHARED-FIX-003` tags, a mode-`None` RH7103 test |

## Simpler option

Conforming examples exist for SHARED-DIAG-011:
- RH3103-002 for a position the grammar rules out;
- RH7003-003 or RH5412-001 for the language-version clause, which the RH3103 alias example now illustrates without conforming;
- RH3001 `x is !true` for the contrast.

With them, the requirement can be illustrated and covered without showing #1417's shapes. For the fifth condition no conforming withheld shape exists. That is the same situation as P37 in squad/decisions/2026-10-22-1380-p37-shared-side-deferred.md, where the shared side was deferred to its follow-up. Both are options for Q4, not my choice.

## Questions for the maintainer

**Q1. What makes a form "not valid C# at that position" (SHARED-DIAG-011)?** The diff counts only the grammar and the language version. P38 says "not valid C#".
- (a) As written. `struct S {}` with the statement `new S();`: RH3104 asks for `default(S)`, and `default(S);` parses but fails with CS0201 (probe). So the construct is reported, and SHARED-FIX-003 withholds the fix.
- (b) Any compile error the position itself causes also counts, such as "constant required", the form of a statement, or a modifier that is not allowed. Then `new S();` is not reported, and RH3003-002 (CS0133 contexts) and RH2004-003 become SHARED-DIAG-011 instances. Their Why sentences already argue this way: "would not compile", "does not compile".

Today the input is reported, and the fix is offered and writes `default(S);`, which fails with CS0201. That is a SHARED-FIX-003 bug under either reading.

**Q2. Does the fifth condition reach a fix that splits a documented declaration?** Mode `Parse`, input `/// <summary>The coordinates</summary>⏎public int X, {|RH7101:Y|};`. The compiler documents both `F:C.X` and `F:C.Y` (probe).
- (a) No. The comment belongs to the declaration node, and the split keeps it on the first field (RH7101-004, FMT-STRUCT-022). The fix is offered, and `Y` loses its generated documentation. This is today's behavior (`apply-fix.sh RH7101`: `fixed`).
- (b) Yes. The RH7101 fix is withheld for such a declaration. The Why of the new condition ("changes the documentation the compiler generates") describes exactly this case.

**Q3. Does "the code it changes" in SHARED-FIX-008 include the line break that ends the line of a change that ends mid-line?** The spec author added "together with the line break that ends its last line"; the issue has no such clause. Input: a document with mostly CRLF, and `Nullable<int> _a;⏎` where `⏎` is a stray LF.
- (a) As written. The RH3103 fix must produce `int? _a;` followed by CRLF.
- (b) Only the line breaks in the span the fix replaces, rewrites, or moves count, its trailing trivia included. That still covers the break behind the new `}` in the RH5406 example. The line keeps its LF. This is today's behavior: the fix only calls `ReplaceNode(typeSyntax, …)`.

#1388 covers formatter-delegating fixes only, so reading (a) has no follow-up.

**Q4. Should the spec examples show shapes that do not hold today?** squad/decisions/2026-10-21-1139-shared-comment-001-contradicting-tests.md answered: "only shapes that conform today; the requirement text states the principle in full". The P26 examples of #1404 follow that (RH7309, RH7004). The issue lists these examples:
- SHARED-DIAG-011, example 1 and the C# 11 half of example 3;
- the RH7309 example under SHARED-FIX-003, in both shapes;
- the claim about the kept break behind `}` in the RH5406 lone-`\r` example;
- the SHARED-DOC-004 pointer to the RH7309 example.

SHARED-DIAG-011's example also contradicts RH3103-006's example (`nameof(System.{|RH3103:Nullable<int>|})` "is reported"), while RH3103 lists SHARED-DIAG-011 under `Uses`.
- (a) Keep the examples as the issue gives them.
- (b) Use conforming shapes only (see *Simpler option*), and leave the other shapes to #1417, #1262, and #1388.

## Follow-up candidates (outside this run)

- #1417: `using static System.Nullable<int>;` is reported, and the fix writes `using static int?;`, which does not parse.
- RH3104: the fix turns the statement `new S();` into `default(S);` (CS0201). This is a SHARED-FIX-003 bug.
- #1262: RH7309 with a `///` on its own line inside the preceding block, above `#endregion`, that documents the next block's first member (shape 3). It is a P39 instance.
- P35 instance without a tracker: the RH7309 text swap (`TextChange` via `sourceText.WithChanges`) keeps every line break of a moved block. Under the clarified SHARED-FIX-008, that is wrong in lone-`\r` and mixed documents.
- P38 instance without a tracker: under C# 10 and earlier, RH5417 reports a one-line lambda block body inside a non-verbatim interpolation hole, where the line breaks it asks for are CS8967. SHARED-DIAG-010 may cover it instead, if the formatter keeps that shape under C# 10. Not checked.

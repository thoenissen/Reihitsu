Role: skeptic
Commit: 3744ba1
Verdict: question for the maintainer

My prompt carried no intended fix or hoped-for verdict. It restated that the principle texts are decided, which I respected. The questions below are about how the decided texts meet existing requirements, not about the texts themselves.

Two questions need the maintainer (see the last section). Neither blocks the conforming shapes listed below: both readings of each question leave those shapes as they are, so test writing can start. Separately, five findings (F1 to F5) are wording or placement fixes for the spec author, and two of them (F1, F2) are needed before the spec is accurate.

Notation: `⏎` is a line break. "format" means `scripts/format.sh` run twice on a fixture directory (the second pass changed 0 files). "apply-fix" means `scripts/apply-fix.sh <ID> <dir>` under LF and CRLF. All fixtures were in the session scratchpad and have been deleted.

## Premise

What the issue says, checked against the code:

- **P32, P33, P34, P36, P37.** As the issue says, the code meets the requirements for some shapes and not for others. Every spec example conforms, and each was re-run:
  - FMT-PIPE-013, FMT-PIPE-014 and FMT-BLANK-016: format.
  - FMT-STRUCT-024: format; the example is left unchanged.
  - SHARED-COMMENT-012: example 1 with format; example 2 reported by apply-fix RH5203.
  - SHARED-COMMENT-011: the RH5204 example (apply-fix: reported, fixed in one iteration). The diagnostic lands on `private` in both lines, as the markup says. Evidence: `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.AlignCommentIndentation` only re-targets a line to the comment when the comment is not on the token's line.
- **P12 ("today's behavior confirmed") is false for most of the rules it binds.** Fourteen enforced analyzer specs list SHARED-COMMENT-011 under `Uses`. I gave each the same shape: a comment that starts the line in the wrong column, with the token behind it in the right column. Fixture `a` had that shape. Control `b` replaced the comment with spaces. Control `c` moved the token one column further right. The amended text requires `a` to be reported.

  | Rule | a (misplaced comment) | b (no comment) | c (token off column) |
  |---|---|---|---|
  | RH5201 (`/* all */` col 0, `.ToList()` in anchor col 24) | reported | – | – |
  | RH5204 (spec example) | reported | – | – |
  | RH5301 (`/* c */` col 0, `P = 1` col 20) | reported | not reported | – |
  | RH5303 (same shape, element `1`) | reported | not reported | – |
  | RH5110 (`/* c */ where`, `where` col 8) | **not reported** | not reported | reported |
  | RH5203 (`/* c */   2);`, `2` col 10) | **not reported** | not reported | reported |
  | RH5205 (`{` of `x =>` lambda in col 31) | **not reported** | not reported | reported |
  | RH5206 (`{` of `x switch` in col 15) | **not reported** | not reported | reported |
  | RH5302 (`&& b` in col 15) | **not reported** | not reported | reported |
  | RH5305 (`]` in col 18) | **not reported** | not reported | reported |
  | RH5306 (`]` in col 21) | **not reported** | not reported | reported |
  | RH5308 (`? 1` in col 19) | **not reported** | not reported | reported |
  | RH5112 (`s⏎/* c */ .Trim();`) | **not reported** | reported | reported |

  RH5304 was not run.
  - The eight non-conforming rules other than RH5112 share one mechanism: they measure the token behind the comment. That is the mechanism of thoenissen/Reihitsu#1147, whose title lists every one of them except RH5306.
  - RH5112 is different: SHARED-CHAIN-004 exempts it on purpose (Q1).
  - No test of any of these rules uses a comment-led code line (`rg '^\s*/\*.*\*/\s*[^\s/]'` over their test files), so no tagged test contradicts the amendment.
- **P35 ("today's behavior confirmed") is true for the line breaks a fix creates. It is false for the amended text as written** (F1).
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 38 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-BLANK-016, FMT-PIPE-013, FMT-PIPE-014 and FMT-STRUCT-024 only, which is expected at this stage.
- **Tagged tests that contradict amended wording: none found.**
  - My checks: scans of every test for glued comments (`*///`, `*/ //` glued), for blank lines dropped between two comments, and for same-line leading comments split off. I also listed which tests in the files I read carry tags.
  - Every test I found that pins a contradicting shape is untagged. They are listed under "Further instances".

## Findings for the spec author

- **F1: SHARED-FIX-008 now requires a change of behavior that P35 rules out.**
  - The first sentence says every line break the fix "creates or keeps" MUST be the detected line ending. The new sentences make that `\n` in an all-`\r` document.
  - The fix keeps lone `\r` breaks today. The real output in #1363 Q6 is `if (b)⏎{⏎M(b);⏎}␍`: the kept break after `}` stays `␍`.
  - The issue's comment records P35 as "today's behavior confirmed, no code change". So the text claims, for kept breaks, behavior that the code does not have and that no follow-up tracks.
  - The handover's remark that the requirement "says nothing about" kept `\r` breaks does not hold: "creates or keeps" covers them.
  - In the same case, the Why's first sentence ("a fix that introduces the other line ending leaves the file with mixed line endings") argues against the outcome the requirement now mandates.
- **F2: The lone-`\r` rule is attributed to FMT-PIPE-007, which does not state it.**
  - FMT-PIPE-007 counts `\r\n` against `\n` and breaks a tie toward `\r\n`. Read literally, an all-`\r` document has zero of both and gets `\r\n`.
  - The code (`LineEndingUtilities.DetectEndOfLine`) counts every one-character line break as `\n`, so the result is `\n`.
  - The amended SHARED-FIX-008 states this formatter fact only in the code-fixes spec, and attributes it to a requirement whose text gives the opposite answer.
  - Where that fact belongs (FMT-PIPE-007 in `pipeline.md`) is the lead's scope call. Clarification 1 only fences `shared/code-fixes.md`.
- **F3: FMT-STRUCT-024 cites FMT-PIPE-003 for "would not compile". FMT-PIPE-003 is broader: "MUST NOT change what the formatted code compiles to".**
  - FMT-STRUCT-003 and FMT-STRUCT-023 refuse because a directive would be dropped or would guard other code. Both concern meaning or content (FMT-PIPE-002/003), not a failed build.
  - So "FMT-STRUCT-003, FMT-STRUCT-007, and FMT-STRUCT-023 are instances of it" holds under FMT-STRUCT-024's three conditions only for FMT-STRUCT-007.
  - FMT-STRUCT-012 and FMT-STRUCT-018 (comments keep an accessor block or empty-body braces) fit the comment condition more closely, and are not named.
- **F4: SHARED-COMMENT-012's rule bullet does not lead to its own RH5203 example.**
  - RH5203-001 judges only an argument that starts a line. Its Why says an argument that does not start its line "is positioned by the token in front of it, not by the list". Whether arguments share a line is RH5102's concern, a non-goal of RH5203.
  - SHARED-COMMENT-012 says `2` behind `b */` does not start its line, and the rule MUST report it "where the rule expects that token to start its line". RH5203 has no requirement that expects an argument to start its line. The issue's RH5204 example meets the same gap: RH5204 only judges lines whose first token is a keyword.
  - The intended outcome is not in doubt: thoenissen/Reihitsu#1382 says RH5203 stays reported and RH5204 becomes reported. The open point is only how the sentence reaches a rule that checks the column of a token.
- **F5: FMT-BLANK-005 and FMT-BLANK-016 disagree in wording.**
  - FMT-BLANK-005 puts the blank line after the directives "directly before the statement". FMT-BLANK-016 puts it "in front of the comments" between the last directive and the code.
  - The code does what FMT-BLANK-016 says. `if (a > 0)⏎{⏎}⏎#pragma warning disable CS0168⏎// why⏎a++;` formats to `}⏎#pragma …⏎⏎// why⏎a++;`.
  - With a comment present, FMT-BLANK-005's sentence reads against FMT-BLANK-016.
- **Minor: SHARED-COMMENT-012 is in no spec's `Uses`.**
  - The intro of `comment-placement.md` says its requirements hold for the rules that list them under `Uses`, and "for these rules and the formatter alike". By that rule, SHARED-COMMENT-012 binds no rule and no formatter spec yet, and the coverage check does not enforce it. thoenissen/Reihitsu#1382 wires it in.
  - The PR should say so. The lane still needs a test tagged with it.

## Mechanism and candidates

_N/A_: this run changes no code and fixes no bug. Below are, for each requirement, the inputs that decide it and what each does today.

- **FMT-PIPE-013.** The wraps are `ControlFlowBraceTransform`, the six `ExpressionBodied*Transform` kinds, and `SwitchCaseBracePhase`. The layout points are a comment in front of the statement on its line, a comment behind it, two comments on one line, and a blank line between leading comments.
  - Conforming:
    - the spec example
    - `int Get() => 42 /* a */ /* b */;` → `return 42 /* a */ /* b */;` (expression body; the space between the comments is kept)
    - the existing tagged FMT-STRUCT-002/-006 and FMT-SWITCH-007 tests
  - Non-conforming, control-flow braces: `if (a)⏎/* c */ Foo();` → `{⏎/* c */⏎Foo();⏎}`. The comment is split from its statement in `StructuralTransformPhase` (`scripts/trace.sh`), and the already-braced form keeps `/* c */ Foo();` on one line.
  - Non-conforming, two comments on one line of their own above the body: `/* a */ /* b */` is split onto two lines. Both control-flow and switch-section bracing do this.
  - Non-conforming, two comments behind the statement: `Foo(); /* a */ /* b */` → `/* a *//* b */`. Control-flow bracing does this.
  - Non-conforming, a blank line between `// one` and `// two` is dropped. This happens in an `else` body as well as in an `if` body and a switch section.
  - Non-conforming, expression body: `=> /* c */ 42;` → `{/* c */⏎return 42;`.
  - No wrap shape I tried keeps a blank line between leading comments with a sensible layout. `int Get3() =>⏎// one⏎⏎// two⏎42;` keeps it, but writes `return⏎⏎// one⏎⏎// two⏎42;` with `42;` at the statement column, so that output is not one a test should pin.
- **FMT-PIPE-014.**
  - Conforming: the spec example (constructor and method lists with leading commas, `a⏎= b;`, `b =⏎a;`).
  - Non-conforming: `int this[int a,⏎int b, int c] => a;` is left unchanged (#1383).
  - The shapes behind Q2 (function-pointer lists, arrows, initializer closing brackets) are not counted on either side here.
- **FMT-STRUCT-024.** Its candidates are the transforms in the spec's `Code owners`.
  - Conforming: the spec example (left unchanged), and the refusal shapes among the tests tagged FMT-STRUCT-003, -007, -012, -013, -018, -019 (its directive clause) and -023.
  - Non-conforming: listed in the spec author's handover, with more pinning tests under "Further instances" below.
- **FMT-BLANK-016.** Its candidates are the blank lines of FMT-BLANK-001 to -005 in a gap that holds a directive, with and without a comment between the last directive and the code.
  - Conforming (FMT-BLANK-005 path):
    - the spec example
    - `}⏎#pragma …⏎// why⏎a++;` → blank line between `#pragma` and `// why` (this is the only shape found that exercises the "in front of the comments" clause)
    - `}⏎#if X⏎a--;⏎#endif⏎// c⏎a++;` → blank line after `#endif`
  - Non-conforming (#1385): the paths of FMT-BLANK-001, -002 and -004, whether the gap holds a directive group with a comment or a directive before a `case`:
    - `a++;⏎#if DEBUG⏎a--;⏎#endif⏎// why⏎if …` (Q-B4)
    - `Foo();⏎#if X⏎…⏎#endif⏎// c⏎var x = 1;`
    - `break;⏎#if X⏎…⏎#endif⏎// c⏎a++;`
    - `break;⏎#pragma …⏎case 2:` (Q-B3)

    Each puts the blank line above the first directive.
- **SHARED-COMMENT-011 (amended).**
  - Conforming rules: RH5201, RH5204, RH5301 and RH5303 (Premise table).
  - Watch the RH5201 fix on the misplaced-link shape: it re-indents the comment and inserts a blank line inside the chain, above the comment line. This is the FMT-BLANK-011 behavior that thoenissen/Reihitsu#1147 says is tracked separately.
- **SHARED-COMMENT-012.**
  - Conforming: formatter example 1. Note that `{ /* p⏎q */ Foo(); }` breaks only on the path behind an opening brace, as #1373 LB-Q2 notes.
  - Conforming, analyzer only: RH5203 example 2. Its fix ends `no-progress (2 iteration(s))` under LF and CRLF and writes `/* a` at column 0.
  - Non-conforming:
    - `{⏎/* p⏎q */ Foo();⏎}` is left with `Foo();` behind the tail, although FMT-BREAK-006 asks every statement to start its line.
    - `int a; /* first⏎second */ int b;`
    - RH5204 `/* a⏎b */       private int _a;` (no diagnostic, re-verified)
- **SHARED-FIX-008.** The conforming shapes are the existing tests in uniform CRLF and LF documents. For an all-`\r` document, a test must build the document itself, because `apply-fix.sh` normalizes every fixture. As the text stands (F1), nothing that a fix keeps in that document conforms.

## Breaking inputs

These are the failure classes that can reach these requirements' tests:

- **FC-017 and FC-033.** An all-`\r` document is erased by any harness that normalizes line endings. Formatter and analyzer tests also need an LF and a CRLF arm.
- **FC-010.** Formatter assertions must run two passes, so a test catches a phase that changes its own output.
- **FC-019, FC-020 and FC-021.** Every one of these principles is about a comment or a directive in a gap, so each test needs the gap filled: same-line leading and trailing comments, two comments on one line, `#if`/`#endif` and `#pragma` with and without a comment behind them, and a `/* */` that ends mid-line.
- **FC-025.** FMT-PIPE-013 reaches all six converted expression-body kinds, not only methods.
- **FC-028.** The two untagged node-level tests below show that `FormatNode` scope reaches FMT-BLANK-016 too.
- **FC-013.** P37's shared side is deferred (`squad/decisions/2026-10-22-1380-p37-shared-side-deferred.md`). The RH5002 and RH5008 fixes still insert above a directive, so no test should assume they agree with the formatter.
- **FC-007.** The RH5203 fix does not converge on the P33 shape, so that shape gets an analyzer-only assertion.
- **FC-034.** Test the spec's own shapes, not simplified cousins.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Specs: `pipeline.md`, `structural-transforms.md`, `blank-lines.md`, `comment-placement.md`, `code-fixes.md` | Must change: F1 to F5, plus the answers to Q1 and Q2 |
| `switch-case-braces.md`, `requirement-ids.txt` | Must stay as the diff has them |
| Formatter tests | Must change: covering tests for FMT-PIPE-013, FMT-PIPE-014, FMT-STRUCT-024, FMT-BLANK-016 and SHARED-COMMENT-012 |
| Analyzer tests | Must change: SHARED-COMMENT-011 shapes (RH5204, RH5301, RH5303, RH5201); SHARED-COMMENT-012 (RH5203, analyzer only); SHARED-FIX-008 in an all-`\r` document (RH5405 or RH5417), subject to F1 |
| Untagged tests that pin contradicting shapes | Must stay unchanged and untagged (clarification 5) |
| Formatter phases, analyzers, code fixes, `Reihitsu.Core` (`LineEndingUtilities`) | Must stay |
| Fix All | Not involved |
| Rule pages and `general-notes.md` | Not involved: none describes comment-led lines or lone `\r` |

## Further instances for existing follow-ups

Under clarification 9, the lead comments these on the named issues.

- **thoenissen/Reihitsu#1147.** The misplaced-comment direction of P12 fails for RH5203, RH5205, RH5206, RH5302, RH5305, RH5306 and RH5308 (Premise table). RH5306 is not in #1147's title.
- **thoenissen/Reihitsu#1381, control-flow braces.**
  - `if (a)⏎/* c */ Foo();` splits `/* c */` onto its own line. #1381 names this only for switch sections.
  - `/* a */ /* b */` on its own line above the body is split onto two lines, by control-flow and switch-section bracing alike.
  - `Foo(); /* a */ /* b */` → `/* a *//* b */`.
  - The `else` body drops the blank line between leading comments.
- **thoenissen/Reihitsu#963.**
  - The untagged `ExpressionBodyDirectiveGuardTests.MethodWithInlineCommentIsStillConverted` pins `=> /* inline */ 1;` → `{/* inline */⏎return 1;`.
  - The untagged `FieldDeclarationSplitTests.CommentOnTheTypeLineIsStillDuplicatedOntoEveryField` pins `private int /* c */ _first, _second;` duplicating the comment. FMT-STRUCT-024 forbids that.
- **thoenissen/Reihitsu#1382.** The formatter keeps `{⏎/* p⏎q */ Foo();⏎}`: a comment that starts its line, with a statement behind its tail. #1382 lists only shapes with code in front of the comment.
- **thoenissen/Reihitsu#1384.** Nine untagged tests in `FieldDeclarationSplitTests` pin a documented split, not one. They are at lines 104, 134, 172, 205, 297, 324, 355, 386 and 417: `Documented…AreSplitAtMemberIndentation` and `MultiLineDocumentationCommentCombinedFieldsAreSplitAtMemberIndentation`.
- **thoenissen/Reihitsu#1385.**
  - Two untagged tests in `ReihitsuFormatterNodeBlankLineTests` pin a blank line inserted directly above `#if !DEBUG` (the FMT-BLANK-004 paths): `InsertsBlankLineAboveDirectiveOfSwitchSectionAfterBreakSection` and `InsertsBlankLineAboveDirectiveOfCommentedBlockStatementAfterBreak`.
  - The FMT-BLANK-002 and after-`break` shapes above behave like Q-B4.

## Simpler option

_None._

## Questions for the maintainer

**Q1: P12 and the comment exemption of SHARED-CHAIN-004 (RH5112).** Both requirements now apply to RH5112, and they give opposite answers:

- Amended SHARED-COMMENT-011: a comment that starts the line outside the place where the rule asks a line to start MUST make the rule report the line.
- SHARED-CHAIN-004: when a comment sits between the root and the first link, RH5112 MUST NOT report the chain; RH5048 reports the comment instead.

Input: `var x = s⏎/* c */ .Trim();`. Today it is not reported (apply-fix RH5112). The same line without the comment is reported and fixed.

- **Reading A** (P12 governs): RH5112 reports the chain, and SHARED-CHAIN-004 gains an exception later in a follow-up.
- **Reading B** (the rule's own comment exemption governs): the chain is not reported, as today, and RH5048 handles the comment. SHARED-COMMENT-011 then needs a sentence saying that a rule's own exemption for a comment in that gap still applies.

The comment exemptions in RH5305-001 ("whose closing bracket has no comment in front of it") and RH5306-001 ("provided that no comment … sits inside the list pattern") are conditions on a MUST, not MUST NOTs. So the amendment simply adds a report for those two rules (non-conforming today, see the table) and does not contradict their texts.

**Q2: Does P34's "laid out alike" override the per-construct decisions that the formatter specs already make inside a named family?** These are today's outputs (format), each specified on purpose:

- **Parameter lists.** `delegate*<int,⏎int, void> _f;` is joined onto one line (FMT-BREAK-014 names function-pointer parameter lists), while `void M(int a,⏎int b, int c)` becomes one parameter per line (FMT-BREAK-012).
- **Closing brackets of initializer-like blocks.** `int[] b = { 1, 2 };` and `int[] a = [1, 2];` stay on one line (FMT-BREAK-021's array-initializer exception, whose Why is that literal data lists would explode, and FMT-BREAK-022). `new C { P = 1 }` and `new List<int> { 1, 2 }` are expanded, with `}` on its own line (FMT-BREAK-021).
- **Arrows.**
  - `int P⏎=> 1;` is joined (FMT-BREAK-026 covers only properties and indexers).
  - A lambda `x⏎=> x` keeps its wrap.
  - A switch-expression arm `1⏎=> 2,` keeps its wrap: the `line-breaks.md` non-goal says arms keep the author's line breaks.

Readings:

- **Reading A** (the family statement overrides these decisions): each shape above is a new P34 instance for a follow-up. Until then, FMT-PIPE-014 contradicts FMT-BREAK-014, -021, -022 and -026 and the switch-arm non-goal. For example, `int[] b = { 1, 2 };` would get its `}` on its own line like an object initializer, and the function-pointer list would get one parameter per line.
- **Reading B** (P34 governs the members its sources named, inside the layout each construct's own requirement chooses): those members are indexer lists, compound assignments, attribute-target colons, a comment before `]`, and arrow spacing. Under this reading the outputs above stay as they are, and FMT-PIPE-014 needs a sentence that keeps those decisions. #1383 already moves the arrow part to spacing on #958, which fits this reading, but the decided text says "laid out".

Under both readings, the spec example and the tests for method and constructor parameter lists and `=` hold.

Role: spec author
Commit: 948a3dc
Verdict: spec written

Lane stays **bug fix**: no change doc. FMT-STRUCT-024 already refuses both splits; the spec gains FMT-STRUCT-030 and RH7101-007 as clarifications that write the refusal down per case, and the text that contradicts FMT-STRUCT-024 (FMT-STRUCT-022's example, FMT-PIPE-029's documented-target examples, FMT-PIPE-031's condition list, RH7101-006's Why) is aligned, as the maintainer decided on #1384. The prompt carried no intended fix or verdict beyond the lead's record.

## thoenissen/Reihitsu#963

Scope: only the field-split part. The `{// why` expression-body part moved to #1439 and is a non-goal of this run; nothing in FMT-STRUCT-004..007 was touched.

### Changed requirements

- FMT-STRUCT-030 — A documented field, or one with a comment in front of its first declarator, is not split — added (clarification of FMT-STRUCT-024). Second bullet: a comment between the declaration's first token and its first declarator (inside or behind an attribute list, between modifiers, between a modifier and the type, inside the type, between the type and the first declarator) keeps the declaration unsplit. Both sides stated: a comment in the leading trivia, inside the first declarator (its initializer), or behind it does not block.
- FMT-STRUCT-022 — Split fields keep their comments — clarified: points to FMT-STRUCT-030 for the comments that block the split.
- FMT-STRUCT-024 — names FMT-STRUCT-030 as one of its instances — clarified.
- FMT-PIPE-031 — `SplitsFieldDeclaration` — clarified: its condition list gains "no comment between its first token and its first declarator (FMT-STRUCT-030)"; new example.
- RH7101-007 — The fix is withheld for a comment in front of the first declarator — added (follows from FMT-PIPE-031; the RH7101 fix asks the formatter). RH7101-004 names it among the withholding requirements.

### Examples for the tests

- FMT-STRUCT-030: first example (`private int /* c */ _first, _second;`, `private /* c */ int _a, _b;`, `[Obsolete /* why */] private int _c, _d;` stay one declaration each), second example (`private int _first = /* c */ 1, _second;` is split, comment stays in `_first`'s initializer).
- FMT-PIPE-031: second example (false for `_first, _second`, true for `_a = /* c */ 1, _b`).
- RH7101-007: both examples (reported without fix; initializer comment still fixed).
- The test `FieldDeclarationSplitTests.CommentOnTheTypeLineIsStillDuplicatedOntoEveryField` pins the old duplication and contradicts FMT-STRUCT-030.

## thoenissen/Reihitsu#1384

### Changed requirements

- FMT-STRUCT-030 — first bullet: a declaration with a `///` or `/** */` comment in its leading trivia (SHARED-DOC-004) is not split when the documentation mode is `Parse` or `Diagnose`. In mode `None` the comment is ordinary and the declaration is split (stated with example, consistent with RH7101-006 and SHARED-FIX-003). A `///`/`/** */` comment in front of a later declarator does not block (FMT-STRUCT-022 moves it).
- FMT-STRUCT-022 — clarified: "including its documentation comment" removed; the example's `///` summary replaced by `// The coordinates`, same layout otherwise (this is the maintainer-decided alignment).
- FMT-PIPE-029 — clarified: a documented target no longer occurs (the split refuses it), so the clauses about a documentation comment *in front of* the target are removed, the input sentence names FMT-STRUCT-030, the `private` example in the "starts a line" sentence now uses `_y;⏎    /** q */ private int _q;`, and the seven examples that had a documentation comment in front of the target keep only their gap *behind* the target (the `_q` side), with the same output on that side as before.
- FMT-PIPE-031 — clarified: condition "has no documentation comment when the documentation mode of its syntax tree is `Parse` or `Diagnose`"; the Why no longer says RH7101-006 withholds the fix for a declaration formatting splits. Example: false for documented `X, Y` in `Parse`, true in `None`.
- RH7101-006 — clarified Why only: the formatter does not split a documented declaration (FMT-STRUCT-030) and the fix asks it (FMT-PIPE-031). The normative text and its examples are unchanged and stay valid.
- `Uses` headers updated: structural-transforms (SHARED-DOC-004), pipeline (RH7101-005, RH7101-007, SHARED-DOC-004, FMT-STRUCT-030), RH7101 (FMT-STRUCT-030). `requirement-ids.txt` gains FMT-STRUCT-030 and RH7101-007.

### Examples for the tests

- FMT-STRUCT-030: first example (`///` and `/** */` documented declarations not split in `Parse`), second example (`// The coordinates` declaration split), third example (documentation mode `None`: both split).
- FMT-STRUCT-022: the realigned example.
- FMT-PIPE-029: the rewritten examples from "a documentation comment behind the target" through "a documentation comment that starts its line in front of the member behind the target". Tests that pin the old examples with a documented target change with them.
- FMT-PIPE-031: second example.
- RH7101-006: unchanged examples; the tests should pass as before (the fix was already withheld), now through `SplitsFieldDeclaration`.
- Code-fix regression requested in the issue comment of 2026-10-23: RH7103 in documentation mode `Parse`, a class with `/// <summary>The coordinates</summary>⏎public int X, Y;`, an instance method `Run()` and then a static method `Create()`: the fix moves `Create` in front of `Run` and leaves the field unsplit. No RH7103 requirement changes; SHARED-FIX-003 (Documentation) already forbids the split there, and FMT-STRUCT-030 is what ends it.
- The nine `FieldDeclarationSplitTests` that pin a documented split (issue comment of 2026-10-23) and the `Documented…AreSplitAtMemberIndentation` tests contradict FMT-STRUCT-030.

## Output questions

_None._

## Internal choices

- New requirement FMT-STRUCT-030 for both guards rather than folding them into FMT-STRUCT-022's normative sentence — alternative: extend FMT-STRUCT-022. A separate ID mirrors FMT-STRUCT-023 (the directive guard) and gives FMT-PIPE-031 and RH7101-007 one target.
- RH7101-007 as a new requirement rather than widening RH7101-005 ("with a directive") — alternative: retitle RH7101-005 to "a declaration the formatter does not split". Keeps each withheld case with its own example, as RH7101-006 does.
- The comment guard covers every position between the first token and the first declarator, an own-line comment between an attribute list and the modifiers included (`[Obsolete]⏎// why⏎private int _a, _b;` stays one declaration). The issue decision (#913 Q3) chose "block the split" over "keep the comment on the first field only" for the type-line case, and the 2026-10-21 comment extends it to modifiers and attribute lists; I applied the same reading to every position in the repeated part. If the lead judges the own-line-after-attribute case to need its own maintainer answer, it is the one place to ask; I see it as decided by the same reading.
- The documentation guard follows the documentation mode (`None` splits), as RH7101-006 and SHARED-FIX-003 already do — alternative: refuse `///` in every mode. In `None` no documentation applies, so FMT-STRUCT-024 does not refuse it, and today's mode-`None` behavior stays.
- Rule page `documentation/rules/RH7101.md` is not changed: the new withheld case is a corner case (`general-notes.md` covers diagnostics without a fix).

## Follow-up candidates

_None._

Role: spec author
Commit: d335334
Verdict: #1329: open question; #1389: open question; #1241: spec written; #1344: open question (RH8304), spec written (RH8309); #1242: spec written

No change doc: every edit below writes down behavior the maintainer already decided (SHARED-FIX-003, the P19 decisions recorded in thoenissen/Reihitsu#1241, thoenissen/Reihitsu#1309, thoenissen/Reihitsu#1314) or moves a requirement without changing its behavior (thoenissen/Reihitsu#1242), as `00-lead.md` states. No requirement was retired and no rule reports more inputs than today. `sdd/specs/shared/code-fixes.md`, `formatter/pipeline.md`, and every file `00-lead.md` lists as owned by parallel runs are untouched. The prompt carried no intended fix beyond the maintainer's clarifications in `00-lead.md`.

How the facts below were established: `scripts/apply-fix.sh` (LF and CRLF arms) and `scripts/trace.sh` over fixtures kept in the session scratchpad, on the commit above. The inputs are quoted inline; `⏎` is a line break.

## thoenissen/Reihitsu#1329

### Changed requirements

_None._ No formatter spec was edited; the outcome waits for the decision below.

### Reproduction facts

- `class C⏎{⏎    /// <summary>S⏎    int A;⏎}` → `DocumentationCommentFormattingPhase` writes `    /// <summary>⏎    /// S⏎    ///     int A;`; stable on the second pass. The field becomes comment text.
- `class C⏎{⏎    /// <summary>Gets the value.</summary>⏎    /// <remarks>Text⏎    /// more⏎    int A;⏎}` → the summary is expanded to three lines, `<remarks>` is rebuilt, and the last line becomes `    ///     int A;`.
- Not part of this issue, recorded for the skeptic: `/// <summary>S</returns>` above `int A;` is rebuilt to `/// <summary>⏎/// S⏎/// </returns>`; all text is kept and the field stays code.

### Examples for the tests

FMT-PIPE-002 and FMT-PIPE-003 (the field must stay a declaration). The expected comment text depends on question 1 below.

### Decisions needed

**Question 1 — How does the formatter lay out a documentation comment that holds an element without an end tag?**

Input (both shapes from the issue):

```csharp
class C
{
    /// <summary>S
    int A;
}
```

```csharp
class C
{
    /// <summary>Gets the value.</summary>
    /// <remarks>Text
    /// more
    int A;
}
```

Every reading keeps `int A;` as code (FMT-PIPE-002, FMT-PIPE-003). They differ in the comment, and the issue itself names two of them ("either the comment is left as written, or only the comment lines are laid out"):

- **A** — Only elements that have an end tag are rebuilt (FMT-DOC-008, FMT-DOC-009); the element without one is left as written. Shape 1 is unchanged. Shape 2 becomes `/// <summary>⏎/// Gets the value.⏎/// </summary>⏎/// <remarks>Text⏎/// more⏎int A;`. Spec text: a new requirement in `formatter/documentation-comments.md`, "an element without an end tag is not rebuilt", with these two shapes as its example.
- **B** — The element without an end tag is laid out too, up to the end of the comment. Shape 1 becomes `/// <summary>⏎/// S⏎int A;`; shape 2 becomes `… /// <remarks>⏎/// Text⏎/// more⏎int A;`. Spec text: FMT-DOC-008 and FMT-DOC-009 widened to elements without an end tag, with the layout written out.
- **C** — A comment that holds an element without an end tag is left as written. Both shapes are unchanged, including the one-line summary in shape 2. Spec text: a new requirement stating that.

**Recommendation: A.** FMT-PIPE-002 allows a change to comment text only where a phase spec explicitly allows it, and FMT-DOC-008 and FMT-DOC-009 speak of an element's start and end tags, so A is the plain reading of the current specs. A also matches SHARED-DOC-017 as now written, which withholds the RH8305 and RH8307 fixes, the fixes that lay out "as FMT-DOC-008/009 specify", for such an element. B would be the pick if the maintainer wants the formatter to repair half-written comments as far as it can.

## thoenissen/Reihitsu#1389

### Changed requirements

_None._ FMT-CLEAN-009 keeps a byte order mark without conditions, so the BOM instance is a plain bug.

### Reproduction facts

- `class C⏎{⏎    int A; /// <summary>end</summary>⏎}` → `    int A; /// <summary>` (text and end tag deleted). The same happens for `int A; /// <summary>end</summary>` at file level and inside a method body before `}`.
- Multi-line: `    int A; /// <remarks>a⏎    /// b</remarks>⏎}` → `    int A; /// <remarks>`, and `    int A; /// <summary>⏎    /// end</summary>⏎}` → `    int A; /// <summary>` (the second line is deleted).
- The multi-line shape the issue quotes, `int A; /// <summary>⏎/// end⏎/// </summary>`, did **not** reproduce: it stays unchanged at file level, before `}` in a class, and before `}` in a method body. Its summary already spans three lines, so nothing is rebuilt. The inputs that do lose a line are the two above, and `M(1 /// <remarks>a⏎/// b</remarks>⏎);` from thoenissen/Reihitsu#1367 Q-D1.
- The same stranded comment gets the FMT-DOC-005 separator today: `int A; ///<remarks>a</remarks>` before `}` → `int A; /// <remarks>a</remarks>`.
- When a declaration follows, the comment is relocated and expanded with all its text kept, for example `int A; /// <summary>end</summary>⏎int B;` → `int A;⏎⏎/// <summary>⏎/// end⏎/// </summary>⏎int B;` (FMT-DOC-001, FMT-DOC-004).
- BOM (`U+FEFF⏎class C{int x;}`): the trace diff cannot show a BOM, so this was not checked here; the reproduction is left to the test implementer.

### Examples for the tests

FMT-CLEAN-009 (BOM); FMT-PIPE-002, FMT-DOC-003, and FMT-DOC-011 (stranded comment). The exact expected text of the stranded comment depends on question 2.

### Decisions needed

**Question 2 — Is a stranded documentation comment (FMT-DOC-003) still rebuilt in place, keeping its text?**

Input:

```csharp
class C
{
    int A; /// <summary>end</summary>
}
```

- **(i)** — Rebuilt in place: the start tag stays behind `int A;` (FMT-DOC-003 forbids moving the comment), and the content and the end tag start `///` lines of their own (FMT-DOC-008, FMT-DOC-011): `    int A; /// <summary>⏎    /// end⏎    /// </summary>⏎}`. The columns of the new lines are FMT-INDENT's. `int A; /// <remarks>a⏎/// b</remarks>` becomes `int A; /// <remarks>⏎/// a⏎/// b⏎/// </remarks>`. The FMT-DOC-005 separator keeps applying, as it does today.
- **(ii)** — Left as written: a stranded comment documents nothing, and the table of principles in `sdd/changes/2026-10/introduce-sdd.md` (SHARED-COMMENT-001; FMT-DOC-003) calls it an ordinary comment that the formatter leaves where it was written. Both inputs stay unchanged. Under this reading FMT-DOC-005 should not apply either, which would change the `///<remarks>` case above (a behavior change outside this issue).

**Recommendation: (i).** The decision on thoenissen/Reihitsu#1367 Q-D1 calls the loss a bug against FMT-DOC-011, the requirement about what a rebuild keeps, so it assumes the rebuild happens. (i) also keeps every normalization a stranded comment gets today. The issue's "`<summary>end</summary>` intact" fits (i) only if it means "text kept" rather than "byte-identical"; if the maintainer meant byte-identical, the pick is (ii).

## thoenissen/Reihitsu#1241

### Changed requirements

- SHARED-DOC-017 — An end tag with another name is no end tag of its own — clarified: the withheld fixes now include RH8305 and RH8307 (reported element), and RH8309 (any top-level element the fix would move, the reported one always among them). The coverage sentence leaves out an RH8309 comment in which only an element that keeps its position lacks an end tag of its own. The Why now covers laying out and moving. New examples for RH8204, RH8305, RH8307, RH8309; the intro sentence and a non-goal of `shared/element-documentation.md` follow.
- RH8107-007 — The fix is withheld for an element without an end tag of its own — clarified: "no end tag" becomes "no end tag of its own" (SHARED-DOC-017, which already names RH8107). The example's span is corrected to "through the line break at the end of its line", which matches the existing tests (`WithSpan(6, 9, 7, 1)`).
- RH8202-004 — same as RH8107-007, now referencing SHARED-DOC-017 instead of RH8107-007.
- RH8204-002 — The diagnostic is on the element — clarified: the location of a `<placeholder>` without an end tag of its own (through the end tag of another name, or to the end of the documentation comment, its last line break included); coverage widened to those shapes; new example (the issue's input, and `<remarks><placeholder>Later text</remarks>`).
- RH8204-003, RH8305-004, RH8307-004, RH8309-004 — coverage sentences clarified: they cover only elements closed by their own end tag and point to SHARED-DOC-017 for the rest. Before this change, RH8305-004 and RH8307-004 did not exclude such inputs at all.
- `Uses` gains SHARED-DOC-017 in RH8107, RH8202, RH8204, RH8305, RH8307, RH8309.
- Rule pages: one gap note in each of `documentation/rules/RH8107.md`, `RH8204.md`, `RH8305.md`, `RH8307.md`, `RH8309.md`; the existing note in `RH8202.md` now also covers an end tag of another element.

### Reproduction facts (all reported, and a fix applied today)

- RH8204: the issue's input → `    ///     public void Run()`, `invalid-result`. `/// <remarks><placeholder>Later text</remarks>` → `/// <remarks>Later text` (`</remarks>` is deleted).
- RH8305: `/// <summary>S⏎int A;` → `/// <summary>⏎/// S⏎/// int A;` at column 0; `/// <summary>S</returns>` → `/// <summary>⏎/// S⏎/// </returns>`.
- RH8307: `/// <remarks>Text⏎/// more⏎int A;` → `… /// more⏎/// int A;`; `… more</returns>` → `/// </returns>` on its own line.
- RH8309: `/// <returns>R</returns>⏎/// <summary>S⏎int A() => 0;` → `invalid-result`. `/// <returns>R</returns>⏎/// <summary>⏎/// S⏎/// </summary>⏎/// <param name="a">P⏎int M(int a) { … }` → `invalid-result`; the unreported, unclosed `<param>` is moved and the method becomes comment text. `/// <returns>R</returns>⏎/// <summary>S</returns>` → fixed, with `</returns>` carried along.
- RH8107 and RH8202, the end-tag-of-another-name instances from this issue's first comment: `/// <remarks><returns>Nothing</remarks>` on a `void` method, and `/// <remarks><value>The size</remarks>` on a property, are both fixed to `/// <remarks>`, which deletes `</remarks>`. The RH8107 input is SHARED-DOC-017's own first example, so SHARED-DOC-017 is violated today. No existing test covers that example for RH8107 or RH8202; the existing SHARED-DOC-017 tags cover only the missing-end-tag and empty-element shapes.

### Examples for the tests

SHARED-DOC-017 (each of its examples, per rule; the RH8202 analogue of the RH8107 example), RH8107-007, RH8202-004, RH8204-002 (second example: diagnostic location), and the positive examples that must keep their fix: SHARED-DOC-017's `<returns/>` example and the existing RH8204-003, RH8305-004, RH8307-004, RH8309-004 examples.

## thoenissen/Reihitsu#1344

### Changed requirements

- RH8309-005 — The fix is withheld for text or an XML comment between elements — added. The fix is withheld when text with a non-white-space character, or an XML comment, stands between two top-level elements. Text before the first or after the last element, and a CDATA section or a processing instruction between elements, are left uncovered.
- `documentation/rules/RH8309.md` — the gap note includes text between elements. An XML comment there is the general comment limitation and is not repeated.
- RH8304 — not edited; see question 3.

### Reproduction facts

- RH8309: `/// <remarks>R</remarks>⏎/// Text between⏎/// <summary>S</summary>` → `<summary>` (expanded) moves above the text and `<remarks>` below it. The same happens with `/// <!-- c -->` in place of the text.
- RH8304: `    int A; /// <summary>S</summary><remarks>R</remarks>⏎    int B;` → `    int A; /// <summary>S</summary>⏎    int A; /// <remarks>R</remarks>` (CS0102). `class C⏎{ /// <summary>S</summary><remarks>R</remarks>⏎    int B;⏎}` → a second `{ /// <remarks>R</remarks>` line, `invalid-result`.

### Examples for the tests

RH8309-005 (its example: both declarations, reported, no fix). RH8304: after question 3.

### Decisions needed

**Question 3 — What does the RH8304 fix do when code stands in front of `///` on the reported element's line?**

`sdd/specs/analyzer/RH8304.md`, RH8304-004, says the fix inserts "the line break …, the indentation of that line, and `/// `", but its coverage sentence leaves out "a comment whose lines have" anything but white space in front of `///`. The decision on thoenissen/Reihitsu#1309 Q2 reads "continuation prefix = indentation only, or withhold", so both outcomes are still open.

```csharp
class C
{
    int A; /// <summary>S</summary><remarks>R</remarks>
    int B;
}
```

- **A** — Insert only the line's indentation. Output: `    int A; /// <summary>S</summary>⏎    /// <remarks>R</remarks>⏎    int B;`. For `{ /// <summary>S</summary><remarks>R</remarks>` the output is `{ /// <summary>S</summary>⏎/// <remarks>R</remarks>`. Both compile, and the comment still documents `B` (SHARED-DOC-004). Spec text: RH8304-004's coverage sentence widened to code in front of `///`, plus an example. No rule-page note.
- **B** — Withhold the fix whenever anything but white space stands in front of `///` on that line. Spec text: a new requirement RH8304-005, and a gap note on `documentation/rules/RH8304.md`.

**Recommendation: A.** It is what RH8304-004's normative sentence already says, and its output is safe. AGENTS.md "Code-fix coverage" counts a guard that refuses inputs the fix could rewrite safely as a defect of the guard's scope, so B would give away safe ground.

## thoenissen/Reihitsu#1242

### Changed requirements

- `DocumentationCommentCodeFixUtilities` — code owner moved from `analyzer/RH8107.md` to `shared/element-documentation.md`; both header rows and the index rows in `sdd/specs/README.md` updated.
- SHARED-DOC-018 — A removed element takes only the lines it owns — added. It is RH8107-006's behavior stated once for the removing fixes (RH8107, RH8202), with RH8107-006's example.
- RH8107-006 — clarified: it now references SHARED-DOC-018 and keeps its ID, heading, and example. RH8107-006 and RH8107-007 were **not** retired. Each rule spec still has to state what its fix produces and when it is withheld (`sdd/README.md`, *Analyzer specs*), and SHARED-DOC-017 already held RH8107-007's decision. Retiring them would have left RH8107 with no fix requirement. The existing tests keep their tags and no assertion changes.
- RH8202: `Uses` now lists SHARED-DOC-017 and SHARED-DOC-018 instead of RH8107-006 and RH8107-007, so no rule spec references another rule's requirement any more. RH8202-003 references SHARED-DOC-018.
- RH8204, RH8304, RH8308, RH8309: no `Uses` change for the move. RH8204's fix does not call the utility today. The RH8304 fix uses `MoveContentToNextDocumentationLineAsync`, whose output RH8304-004 states itself. RH8308 uses only `IsHorizontalWhitespace`, a whitespace test no spec specifies. RH8204 and RH8309 gained SHARED-DOC-017 for thoenissen/Reihitsu#1241, not for the move.

### Examples for the tests

SHARED-DOC-018. It needs a covering test, and the existing tests of RH8107-006's example and of RH8202-003 already verify it, so a tag is enough: no assertion change.

## Open questions for the maintainer

Found in passing, not blocking:

1. **RH8309 — text before the first or after the last top-level element.** Input: `/// Intro⏎/// <remarks>R</remarks>⏎/// <summary>⏎/// S⏎/// </summary>⏎int A;` → the fix gives `/// Intro⏎/// <summary>…</summary>⏎/// <remarks>R</remarks>`; the same with `/// Outro` after the last element puts `Outro` after `<remarks>` instead of after `</summary>`. Why it looks wrong: the text stays where it is while the elements move, so it ends up next to a different element. That is the reason RH8309-005 withholds the fix for text between elements, and thoenissen/Reihitsu#1344 decided only "between". RH8309-004's coverage sentence includes these inputs today and states the sorted output. Touches RH8309-004, RH8309-005. Readings: (a) the fix sorts them as today; (b) it is withheld as in RH8309-005. Recommendation: (b), for the same reason as RH8309-005. Text in front of the elements usually introduces the first one.
2. **RH8309 — an element without an end tag of its own that the fix would not move.** Input: `/// <remarks>R</remarks>⏎/// <summary>⏎/// S⏎/// </summary>⏎/// <seealso cref="C">X⏎int A;` (an unclosed `<seealso>`, ranked last, keeps its position). Real output: not run. SHARED-DOC-017's coverage sentence leaves the input out. Why it may be wrong: the fix formats the whole declaration afterwards, so whether the unmoved element survives depends on question 1. Touches SHARED-DOC-017, RH8309-004. Readings: (a) offer the fix, since the element is not moved; (b) withhold the fix whenever any top-level element lacks an end tag of its own. Recommendation: (a) when question 1 is answered A or C; (b) when it is answered B.

## Whole run

- Files changed: `sdd/specs/shared/element-documentation.md`, `sdd/specs/analyzer/RH8107.md`, `RH8202.md`, `RH8204.md`, `RH8305.md`, `RH8307.md`, `RH8309.md`, `sdd/specs/README.md`, `sdd/specs/requirement-ids.txt` (RH8309-005, SHARED-DOC-018 at their sorted positions), `documentation/rules/RH8107.md`, `RH8202.md`, `RH8204.md`, `RH8305.md`, `RH8307.md`, `RH8309.md`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage|FullyQualifiedName~RuleDocumentation"`: 50 passed. 1 failed: `EveryRequirementOfAnEnforcedSpecIsCovered`, because `RH8309-005: no test covers it` and `SHARED-DOC-018: no test covers it`. The test implementer closes both.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: 7 passed.
- Follow-up candidate for the lead (outside this role's files): `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md` records the rule-to-rule `Uses` that this run removes; `squad/` is off-limits here.

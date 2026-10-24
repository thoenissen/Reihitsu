Role: skeptic
Commit: eb534bc60d7beec4e4414b134d7dc63c01d4d325
Verdict: #1329: question for the maintainer; #1389: question for the maintainer; #1241: question for the maintainer; #1344: proceed; #1242: proceed

No file under `src/` has changed since `f3d514d`, so every observation below is the behavior of `main`.

How the facts were established:
- `bash scripts/trace.sh <fixture>`: the formatter, phase by phase, up to three passes.
- `scripts/apply-fix.sh <RH####> <dir>`: analyzer, then fix, then analyzer again, under LF and CRLF.
- The fixtures were in the session scratchpad and have been deleted.
- The LF and CRLF arms agreed in every `apply-fix` run.
- `⏎` is a line break. `U+FEFF` is the byte order mark character.

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The questions are numbered Q1 to Q6 across the issues, so the lead can ask them in one message:
- **Q1, Q2, and Q4 block the run.** The output for inputs that belong to the issues depends on them.
- **Q3, Q5, and Q6 were found in passing.**

Q2 is the spec author's open question 1. I consider it blocking.

## thoenissen/Reihitsu#1329

### Premise

The issue is real. Both shapes from the issue reproduce in `DocumentationCommentFormattingPhase`, as `01-spec-author.md` recorded, and the code shows why (next section).

The same defect reaches users through two code fixes. The RH8305 and RH8307 fixes do nothing but call `ReihitsuFormatter.FormatNodeInDocumentAsync` on the declaration:
- `/// <summary>S⏎int A;` becomes `/// <summary>⏎/// S⏎/// int A;` under both rules.
- Shape 2 is destroyed the same way.
- `apply-fix` reports these as `fixed`, because the result still parses.

The defect reaches more formatter inputs than the two in the issue, and two of them never converge (see *Breaking inputs*).

### Mechanism and candidates

**Mechanism.** The parser closes an element that has no end tag with a zero-width missing end tag, placed after the comment's last line break. `DocCommentElementNormalizer` measures and rebuilds the element as if its end tag stood on that next line. So the rebuilt text no longer ends with the comment's line break, and the next source line becomes part of the comment.

**Which elements are selected.** `NormalizeOutermostElements` takes every `XmlElementSyntax` in a `SingleLineDocumentationCommentTrivia` that `RequiresNormalization` selects. An element whose `EndTag.IsMissing` is selected in two cases:
- It is a `<summary>` whose start tag is on the comment's last line. `SpansAtLeastThreeLines` then counts one line.
- Its first content is on the line of its start tag. `NeedsLineAlignmentNormalization` then sees the end tag on a later line.

**How it is rebuilt.** One of two branches does the rebuild:
- `BuildExpandedElement` handles a summary, or content on more than one line. Its text ends in `/// ` with no line break.
- `BuildCollapsedElement` handles any other element with one line of content. Its text ends in the content itself.

**Other facts about the candidate set.**
- A non-summary element whose content starts on a later line is not selected. `/// <remarks>⏎/// Text⏎int A;` is left unchanged today.
- An element also has a missing end tag when a nested element takes its end tag. In `<summary>A <see cref="C">x</summary>`, the `</summary>` closes `<see>`.
- Every comment also passes through `NormalizeDocumentationLinePrefixes` (FMT-DOC-005 and FMT-DOC-006). FMT-DOC-013 now excludes these comments from that step as well.

### Breaking inputs

**Observed on `main`, and covered by FMT-DOC-013 as written.** Each of these destroys code today and must be left unchanged after the fix.
- Expanded branch, the issue's two shapes:
  - `/// <summary>S⏎int A;`
  - `/// <summary>Gets the value.</summary>⏎/// <remarks>Text⏎/// more⏎int A;`
- Expanded branch, a nested element takes the end tag: `/// <summary>A <see cref="C">x</summary>⏎int A;` becomes `///     int A;`.
- Expanded branch, no separator: `///<summary>S⏎int A;` becomes `/// <summary>⏎/// S⏎///     int A;`.
- Collapsed branch: `/// <summary>⏎/// S⏎/// </summary>⏎/// <remarks>Text⏎int A;` becomes `/// <remarks>Text    int A;`.
  - Pass 2 also joins `}` into the comment, and the input to pass 3 does not parse (FC-010, FC-011).
  - The same happens when the line ends in white space: `/// <remarks>a   `.
- Collapsed branch, on SHARED-DOC-017's own RH8107 example: `/// <summary>Runs</summary>⏎/// <remarks><returns>Nothing</remarks>⏎public void Run()⏎{⏎}` becomes `/// <remarks><returns>Nothing</remarks>    public void Run()`. The input to pass 2 does not parse.
- Other scopes (FC-027):
  - A comment on its own line before `}`: `int A;⏎⏎/// <summary>S⏎}` becomes `/// }`, and the result no longer parses.
  - A comment inside a method body: `/// <summary>S⏎int x = 1;` becomes `///         int x = 1;`.

**Not covered by FMT-DOC-013, and still destroying code (Q2).** A comment written behind code that FMT-DOC-001 relocates: `int A; /// <summary>S⏎int B;` is moved above `int B`, and then `int B;` becomes `///     int B;`.

**Failure classes the test implementer covers.**
- **FC-001.** The guard decides for the whole comment. In shape 2, the complete `<summary>` stays on one line, and no `///` line gets the FMT-DOC-005 separator.
- **FC-016, FC-027.** Both rebuild branches, the nested case, and the scopes listed above.
- **FC-010, FC-017.** Two formatter passes and a CRLF variant for every shape. The collapsed branch is the one that does not converge today.
- **FC-003.** The other side of the boundary: an element closed by an end tag of another name. `/// <summary>S</returns>⏎int A;` is rebuilt today to `/// <summary>⏎/// S⏎/// </returns>`, which keeps every character. FMT-DOC-013's coverage sentence keeps that behavior (Q3).
- **FC-032.** The RH8305, RH8307, and RH8309 fixes call the formatter, so their results change with this formatter change.
- **Indentation.** `IndentationPhase` re-indents the `///` lines of such a comment, as it does for ordinary comments.
  - Observed: `          /// <remarks>⏎          /// Text⏎    int A;` ends with both comment lines at column 4.
  - FMT-DOC-013 defines "as written" after a colon: FMT-DOC-005 to FMT-DOC-011 do not apply. So the re-indentation stays, and tests must not assert byte-identical output on misindented input.
  - The spec author may write this down as a clarification of current behavior.
  - A reading that also forbids re-indentation would require a change in the Indentation phase, which is off-limits for this run.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DocumentationCommentFormattingPhase`, `DocCommentElementNormalizer` | must change |
| `IndentationPhase` (off-limits) | must stay |
| RH8305 and RH8307 code fixes | must change. They only call the formatter, so after this change they would be offered on these comments and change nothing in them (FC-009, SHARED-FIX-005). SHARED-DOC-017 also withholds them. |
| RH8301, RH8305, RH8307 analyzers | depend on Q1 |
| `Reihitsu.Core` | not involved |
| Rule docs | not involved for this issue |
| Tests | formatter regression and idempotency tests for each shape; code-fix tests through the delegating fixes |

### Simpler option

_None._

### Questions for the maintainer

**Q1 — Do RH8301, RH8305, and RH8307 still report inside a comment that the formatter now leaves as written?** Blocks #1329, #1389, and #1241.

*Context.* Answers 1C and 2(ii) make the formatter keep two kinds of `///` comment as written:
- a comment that holds an element without an end tag (FMT-DOC-013);
- a comment stranded behind code (FMT-DOC-003).

RH8301, RH8305, and RH8307 are layout rules whose fixes produce what the formatter produces. RH8301 writes the FMT-DOC-005 separator. RH8305 and RH8307 call the formatter.

SHARED-DIAG-010 says such a rule MUST NOT report a shape that a formatter requirement leaves unchanged. Its own example is RH8307 not reporting inside a `/** */` comment, which FMT-DOC-012 keeps.

Today the three rules report these comments (observed with `apply-fix.sh`):

| Input | Reported today by |
|---|---|
| `/// <summary>S⏎int A;` | RH8305, RH8307 |
| `/// <summary>Gets the value.</summary>⏎/// <remarks>Text⏎/// more⏎int A;` | RH8305 (on the complete summary), RH8307 (on `<remarks>`) |
| `///<summary>S⏎int A;` | RH8301, RH8305, RH8307 |
| `int A; /// <summary>end</summary>` before `}` | RH8305. Its fix deletes `end</summary>` today and the diagnostic stays (`no-progress`). |
| `int A; ///<remarks>a⏎///b</remarks>` before `}` | RH8301 (on the second line), RH8307. The RH8307 fix deletes the text today, as it does for `int A; /// <remarks>a⏎/// b</remarks>` and for `M(1 /// <remarks>a⏎/// b</remarks>⏎);`. |

What the specs say today:
- **Comments with an element without an end tag.** The amended SHARED-DOC-017 says RH8305 and RH8307 keep reporting such an element and withhold the fix. That is the round-8 decision recorded on #1241, taken while the formatter still rewrote these comments.
- **Stranded comments.** RH8301-001, RH8305-001, and RH8307-001 all leave them out of their coverage, so SHARED-DIAG-010 alone decides there.
- **Either kind.** After the formatter change, the RH8305 and RH8307 fixes change nothing in either kind of comment, so they must be withheld whatever the answer (SHARED-FIX-005).

*Question.* Inside a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written, do RH8301, RH8305, and RH8307 report?

- **A — No: SHARED-DIAG-010 applies.**
  - None of the five inputs above is reported by these three rules.
  - The content and order rules (RH8107, RH8202, RH8204, RH8309) still report in such comments. In `Diagnose` mode the compiler still warns: CS1570 for badly formed XML, CS1587 for a misplaced comment.
  - Spec changes: the coverage sentences of RH8301-001, RH8305-001, and RH8307-001 say so. SHARED-DOC-017 keeps the RH8305 and RH8307 withhold only for an element closed by an end tag of another name. The formatter rebuilds that element, so the report is required there. The RH8305 and RH8307 examples for a missing end tag become "not reported".
  - Cost: three analyzers gain a condition and report less than today. That includes the `///<summary>S` line, which RH8301-001 covers today.
- **B — Yes: the diagnostics stay, as SHARED-DOC-017 says.**
  - The fixes are withheld for every element of such a comment. That includes the complete summary in the second input, which no requirement withholds today.
  - It needs an exception in SHARED-DIAG-010 (`shared/diagnostics.md`), so a change doc, which this run may not add. Until that exists, the specs contradict each other.
  - CLI users see diagnostics that the formatter never clears.
- **C — Split.** Comments with an element without an end tag keep their reports (B). Stranded comments are not reported (A). The first half needs the same change doc as B.

*Recommendation: A.* The existing shared policy already says this, so no change doc is needed, and the incomplete or misplaced comments stay flagged by the compiler and the content rules. B would be the pick if the maintainer wants the layout rules to keep pointing at incomplete documentation. The SHARED-DIAG-010 exception would then follow as its own change doc.

**Q2 — A relocated comment that holds an element without an end tag.** Blocks #1329.

*Context.* Input: `class C⏎{⏎    int A; /// <summary>S⏎    int B;⏎}`.

Today, FMT-DOC-001 moves the comment above `int B`, and the rebuild then turns `int B;` into comment text: `    int A;⏎⏎    /// <summary>⏎    /// S⏎    ///     int B;`.

This is the claim #1329 makes: a declaration after an element without an end tag becomes comment text. But FMT-DOC-013's coverage leaves out a comment that FMT-DOC-001 moves, and FMT-DOC-004 says a relocated comment is normalized.

FMT-PIPE-002 and FMT-PIPE-003 already require `int B;` to stay code. Only the comment's text is open.

*Question.* Is a relocated comment with an element without an end tag laid out?

- **A — Relocate the comment and leave its text as written.**
  - Output: `    int A;⏎⏎    /// <summary>S⏎    int B;`.
  - Spec changes: FMT-DOC-013's coverage extends to relocated comments, and FMT-DOC-004 makes an exception for them.
- **B — Leave the whole comment where it was written.**
  - Output: the input, unchanged.
  - Spec change: FMT-DOC-013 overrides FMT-DOC-001 for such a comment.

*Recommendation: A*, as the spec author also recommends. The comment still documents `B`, and moving it changes no comment text. FMT-DOC-013's reason is about the element's layout only.

**Q3 — An element closed by an end tag of another name, in the formatter.** Found in passing.

*Context.* FMT-DOC-013's coverage sentence says an element closed by an end tag of another name has an end tag in the requirement's sense. So the formatter keeps rebuilding `/// <summary>S</returns>⏎int A;` to `/// <summary>⏎/// S⏎/// </returns>`. That is today's behavior: every character is kept and the field stays code.

The new Why of SHARED-DOC-017 says that laying out such an element would treat the end tag of another name as the element's own. On that ground it withholds the RH8305 fix, whose output is exactly this rebuild.

The wording of the two decisions differs:
- Answer 1C speaks of an element without an end tag.
- P19 says an end tag with another name is no end tag of the element's own.

The round-1 question named only elements with no end tag at all.

*Question.* Does FMT-DOC-013 also leave as written a comment whose element is closed by an end tag of another name?

- **A — Keep the rebuild**, as FMT-DOC-013 is written.
  - The RH8305 and RH8307 fixes stay withheld for this input (P19), and the formatter corrects the layout.
  - SHARED-DIAG-010 then requires RH8305 to report it.
- **B — Leave it as written too.** FMT-DOC-013 is widened to "no end tag of its own".
  - Output: the input, unchanged.
  - Under Q1-A, RH8305 and RH8307 would then not report it either.

*Recommendation: A.* The rebuild keeps every character and pulls no code into the comment, so the hazard FMT-DOC-013 guards against does not arise. B would be the pick if the maintainer wants P19 to govern the formatter as well as the fixes.

### Follow-up candidates

See *Whole run*.

## thoenissen/Reihitsu#1389

### Premise

**BOM.** Real, but in a different phase than the requirement's file suggests.
- For `U+FEFF⏎class C{int x;}`, the trace shows `BlankLinePhase` removing the first line, BOM included. `CleanupPhase` is not the step that drops it.
- The reach is low, as the issue says. The file reader takes a leading BOM as the encoding signature, so a fixture needs two BOMs to keep `U+FEFF` in the text. A one-BOM fixture reaches the formatter as an empty first line.
- So CLI users meet the defect only with a doubled BOM, while API callers that pass text containing `U+FEFF` meet it directly.

**Documentation text behind code.** Real for the single-line instance and for two multi-line variants:
- `int A; /// <remarks>a⏎/// b</remarks>`
- `M(1 /// <remarks>a⏎/// b</remarks>⏎);`

The multi-line instance the issue itself quotes (`int A; /// <summary>⏎/// end⏎/// </summary>`) does not reproduce. That matches the code: its summary already spans three lines, so nothing is rebuilt.

The loss also reaches users through the RH8305 and RH8307 fixes (observed):
- RH8305 on `int A; /// <summary>end</summary>` before `}` produces `int A; /// <summary>`, and the diagnostic stays (`no-progress`).
- RH8307 on the three multi-line stranded shapes deletes their text and reports `fixed`.

### Mechanism and candidates

**BOM.**
- Roslyn lexes `U+FEFF` as whitespace trivia.
- `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` removes the white space and line breaks before the file's first token by trivia kind alone. So a BOM separated from the first line break by nothing but white space is removed along with the blank line.
- `CleanupPhase.CleanWhitespaceBeforeEndOfLine` also removes a whitespace trivia before a line break by kind alone. I found this by reading the code. The runs never reach it, because the BlankLines phase runs first.
- Only `IndentationRewriter` handles `U+FEFF` specially.
- Candidates are the first-line shapes in front of the first token: BOM then line break, BOM then white space then line break, BOM then several blank lines, and a text of a BOM and a line break only.

**Documentation text behind code.**
- **Which comments.** FMT-DOC-001 does not relocate a comment written behind code when its owning token is not the first token of its parent (`ReihitsuFormatterHelpers.DocumentsFollowingCode` is false).
- **Why text is lost.** Such a comment is rebuilt with the `GetContinuationPrefix` of its first line, and that prefix copies the code in front of `///`. The rebuilt text then reparses as a documentation comment followed by code. `Execute` keeps only `ParseLeadingTrivia(...)[0]`, so every rebuilt line after the first is dropped.
- **Dispatch.** Every owning token kind that is not the first token of its parent can strand a comment. Observed: `}` (a class, a method body), `)` (an argument list), `+` (a binary expression), and `.` (a member access).
- **Spec note.** FMT-DOC-003 names `;`, `)`, `]`, and `}` in a dash list that reads as exhaustive. The dispatch is wider, so the spec author should phrase the list as examples ("such as") or add a coverage sentence.
- **When text is kept.** The loss needs an element that the rebuild selects on the comment's first line. Without one, the text is kept, but the line still gets the FMT-DOC-005 separator.

### Breaking inputs

**BOM.**
- Lose the BOM today:
  - `U+FEFF⏎class C{int x;}`
  - `U+FEFF⏎⏎class C{int x;}`
  - `U+FEFF ⏎class C{int x;}`
  - `U+FEFF⏎// c⏎class C{}`
  - `U+FEFF⏎` alone
- Keep it today:
  - `U+FEFF    class C{int x;}`: the indentation is removed and the BOM kept.
  - `U+FEFF// c⏎class C{int x;}`
  - `U+FEFFclass C` (the existing `BomPreserved` test)
- Expected output for the issue's input, from FMT-BLANK-007 (blank lines before the first line are removed) together with FMT-CLEAN-009 (the BOM is kept): `U+FEFFclass C⏎{⏎    int x;⏎}`.
- FC-015 and FC-016: the BOM rule has one guarded copy (`IndentationRewriter`) and two unguarded removal steps.
- FC-033: tests pass a string that contains `U+FEFF`, never a file.

**Documentation text behind code.** All of these must keep their text unchanged:
- `int A; /// <summary>end</summary>` before `}`
- `int A; /// <remarks>a⏎/// b</remarks>` before `}`
- `int A; /// <summary>⏎/// end</summary>` before `}`
- `M(1 /// <remarks>a⏎/// b</remarks>⏎);`
- `int A = 1 /// <summary>end</summary>⏎+ 2;`
- `var s = "a" /// <summary>end</summary>⏎.Trim();`
- `int A; ///<remarks>a</remarks>` before `}`, which no longer gets the FMT-DOC-005 space

FMT-DOC-003's new examples are in this set.

**Related facts the tests rely on.**
- A stranded comment whose continuation lines are misindented is already left alone by the Indentation phase (observed). So "as written" holds byte for byte for stranded comments.
- FC-001: the guard must stop the line-prefix normalization (FMT-DOC-005, FMT-DOC-006) as well as the element rebuild.
- FC-016: the `leadingTrivia[0]` truncation is the general hazard behind this instance. Any other rebuild that does not reparse as one comment loses text the same way.
- The positive side stays covered: a relocated comment keeps all its text. `int A; /// <summary>end</summary>⏎int B;` is relocated and expanded with its text kept (`01-spec-author.md`).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `BlankLineTokenCleanupRewriter` (BlankLines phase, off-limits) | must change (Q4) |
| `CleanupPhase` | involved. The comment at `CleanupPhase.cs:51` claims the BOM is safe because only whitespace trivia is removed, which is the reason it is lost (FC-031). `CleanWhitespaceBeforeEndOfLine` would drop the BOM if the BlankLines phase kept the line. |
| `IndentationRewriter` | must stay |
| `DocumentationCommentFormattingPhase` | must change |
| RH8305 and RH8307 code fixes | must change. After the formatter change they change nothing in a stranded comment (FC-009, SHARED-FIX-005). |
| RH8301, RH8305, RH8307 analyzers | Q1. For stranded comments SHARED-DIAG-010 decides, because the three rules' coverage sentences exclude them. |
| `DocumentationCommentUtilities.GetContinuationPrefix` | not involved for this issue once stranded comments are skipped (see #1344) |
| Spec | FMT-DOC-003 has been changed. Its token list is narrower than the dispatch. |

### Simpler option

The phase already defines the stranded set: `DocumentsFollowingCode` together with `IsAfterSourceOnSameLine`. FMT-DOC-003 needs no second definition (FC-015).

### Questions for the maintainer

Q1 (see #1329) also applies here.

**Q4 — The BOM defect is in the BlankLines phase, which this run may not touch.** Blocks the BOM half of #1389.

*Context.* For `U+FEFF⏎class C{int x;}`, the trace shows `BlankLinePhase` removing the first line, BOM included. The same happens for the other "lose" shapes listed above.

The cause is that `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` removes trivia by kind, and Roslyn lexes `U+FEFF` as whitespace. `00-lead.md` puts the BlankLines phase off-limits, because a parallel run owns it.

The expected output follows from FMT-BLANK-007 and FMT-CLEAN-009: `U+FEFFclass C⏎{⏎    int x;⏎}`.

*Question.* How does the BOM half of #1389 get fixed?

- **A — Allow this run to change the BlankLines phase for this one case**: a first line holding only the BOM and white space. The change is coordinated with the run that owns the phase, and #1389 closes.
- **B — Leave the BOM half out of this run.** The PR links #1389 with `Refs`. The BOM goes to the BlankLines run, or to a follow-up issue whose text the maintainer approves.
- **C — Restore the BOM in a phase this run may edit (Cleanup).** That creates a second owner of the same policy (FC-015), one that undoes another phase's removal, and the BlankLines defect stays.

*Recommendation: A.* The defect sits in one method, and the run's done criterion closes all five issues. B would be the pick if the BlankLines run is changing `CollapseLeadingBlankLines` itself.

### Follow-up candidates

See *Whole run*.

## thoenissen/Reihitsu#1241

### Premise

Real for all six rules (observed with `apply-fix.sh`):

| Rule | Input and result today |
|---|---|
| RH8204 (no guard) | The issue's input becomes `    ///     public void Run()` (`invalid-result`), and `Later text` is deleted too. `/// <remarks><placeholder>Later text</remarks>` becomes `/// <remarks>Later text`, deleting `</remarks>`. |
| RH8107 (`CanRemoveElement` tests only `EndTag.IsMissing`) | `/// <remarks><returns>Nothing</remarks>` becomes `/// <remarks>`, deleting `</remarks>`. With no end tag at all, the fix is withheld already (`no-fix-offered`). |
| RH8202 (same guard) | `/// <remarks><value>The size</remarks>` becomes `/// <remarks>`. |
| RH8305, RH8307 (no guard; they call the formatter) | The declaration is destroyed, as under #1329. The end-tag-of-another-name shapes are fixed with their text kept. |
| RH8309 (no guard) | All three comments of the SHARED-DOC-017 example end `invalid-result`. `/// <returns>R</returns>⏎/// <summary>S</returns>` is fixed, and `</returns>` moves with the summary. |

### Mechanism and candidates

**Mechanism.** The parser closes an element with the first end tag that follows its content, whatever that tag's name. An element with no end tag at all runs to the end of the comment, past its last line break. The fixes decide whether the reported element ends where its text ends in one of two ways:
- by `EndTag.IsMissing`, through `CanRemoveElement` (RH8107, RH8202);
- not at all (RH8204, RH8305, RH8307, RH8309).

So the edit can take in an end tag that belongs to an outer element, or the declaration that follows the comment.

**Dispatch, per rule.**

| Rule | Reported node |
|---|---|
| RH8107, RH8202 | the node `GetFirstDocumentingTag` returns, which can be nested |
| RH8204 | any `XmlElement` or `XmlEmptyElement` named `placeholder`, matched case-insensitively, in any `///` comment |
| RH8305 | the first top-level `summary` |
| RH8307 | any `XmlElement` |
| RH8309 | the first top-level element out of order; the withhold looks at every top-level element of `Content` |

**End states of an element.** Four: an empty element, its own end tag, an end tag of another name, and no end tag.

**Name boundary.** Two forms sit on the boundary of "another name": a difference in case only (Q5), and a namespace prefix.

**Nesting.** An element can lack an end tag of its own while being nested inside a top-level element that has one: `<summary>a <see>b</x></summary>`. SHARED-DOC-017 withholds nothing for it, and the fixes keep the nested text verbatim. It is a positive case.

### Breaking inputs

**Failure classes the test implementer covers.**
- **SHARED-DOC-017 examples.** Each example, for each rule.
- **FC-003.** Each fix on both sides of the boundary:
  - own end tag: offered;
  - end tag of another name: withheld;
  - no end tag: withheld;
  - empty element: offered.
- **FC-008, SHARED-FIX-002.** Fix All over a document that holds one withheld and one fixable diagnostic, such as two `<placeholder>` elements of which one is unclosed.
- **FC-015.** "End tag of its own" needs one definition for the six fixes, owned by `DocumentationCommentCodeFixUtilities` (SHARED-DOC-017's code owner after #1242). A private copy in each fix would drift.
- **FC-030.** The RH8305, RH8307, and RH8309 fixes format the whole declaration, so positive tests use declarations that are already in formatter layout. Observed: the RH8309 fix turns `int A() => 0;` into a block body even on a correctly ordered positive input. The RH8309 example of SHARED-DOC-017 therefore works only as a withheld case.
- **FC-031.** The remarks of `CanRemoveElement` ("An element without an end tag is left alone") describe the narrower check.

**Spec notes for the spec author.** These are not questions.
- SHARED-DOC-017's examples state that RH8305 and RH8307 report elements that RH8305-001 and RH8307-001 leave out of their coverage ("closed by its own end tag"). After Q1, one of the two sides has to move.
- SHARED-DOC-017's RH8309 Why says an element without an end tag "is rewritten even when it keeps its position". That holds today only because of the #1329 defect:
  - `/// <returns>R</returns>⏎/// <summary>⏎/// S⏎/// </summary>⏎/// <seealso cref="C">X⏎int N() => 0;` ends `invalid-result` because the formatting step pulls the declaration into the comment. The reordering itself leaves `<seealso>` last.
  - After FMT-DOC-013, the formatter leaves that comment as written. The reason for answer 6B is then FMT-DOC-013's own: incomplete documentation is corrected by hand.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DocumentationCommentCodeFixUtilities.CanRemoveElement` (callers: RH8107, RH8202 only) | must change |
| RH8204, RH8305, RH8307, RH8309 code fixes | must change |
| RH8107, RH8202 code fixes | must change through the guard |
| Analyzers of the six rules | must stay, apart from Q1 for RH8305 and RH8307 |
| Fix All (`BatchFixer` everywhere) | must stay. A withheld diagnostic is left behind (SHARED-FIX-002). |
| Formatter | not involved for the end-tag-of-another-name case, which stays rebuilt (Q3) |
| Rule docs | changed by the spec author |

### Simpler option

The existing helper `DocumentationCommentCodeFixUtilities` is the shared owner of the end-tag check. _No narrower change._

### Questions for the maintainer

Q1 (see #1329) also applies here.

**Q5 — Does an end tag that differs only in case count as the element's own?** Found in passing. It decides the guard's boundary tests.

*Context.* SHARED-DOC-017 says an element has an end tag of its own only when the parser closes it with an end tag carrying the name of its start tag. XML names are case-sensitive.

The analyzers match names case-insensitively: RH8107 reports `<Returns>`, and RH8204 reports `<Placeholder>`. Today both fixes produce safe results on such input:
- RH8107 removes the line `/// <Returns>Nothing</returns>`.
- RH8204 turns `/// <remarks><Placeholder>Later text</placeholder></remarks>` into `/// <remarks>Later text</remarks>`.

*Question.* Does the guard compare end-tag names exactly or ignoring case?

- **A — Exact comparison**, of prefix and local name, ordinal. Both fixes are withheld. This matches XML.
- **B — Case-insensitive comparison**, as the analyzers match names. Both fixes work as today.

*Recommendation: B.* The fixes are safe on this input, and AGENTS.md treats a guard that refuses safe input as giving away ground. A would be the pick if the maintainer wants the guard to follow XML's name rule exactly.

### Follow-up candidates

See *Whole run*.

## thoenissen/Reihitsu#1344

### Premise

Real (observed with `apply-fix.sh`):
- **RH8304.**
  - `int A; /// <summary>S</summary><remarks>R</remarks>⏎int B;` repeats `int A;` (CS0102). `apply-fix` reports `fixed`, because the result parses.
  - `{ /// …` repeats `{`, and the result does not parse (`invalid-result`).
- **RH8309.**
  - With text or an XML comment between two elements, the elements move around it.
  - Answer 5B's inputs behave the same way. `/// Intro` stays first while `<summary>` moves under it, and `/// Outro` ends up after `<remarks>` instead of after `</summary>`.

### Mechanism and candidates

**RH8304.**
- **Mechanism.** `MoveContentToNextDocumentationLineAsync` builds the new line from `DocumentationCommentUtilities.GetContinuationPrefix`. That helper returns everything in front of `///` on the line, so whatever stands in front of the comment is repeated.
- **Candidates.** What can stand in front of `///` on a comment's first line: any token, a `/* */` comment, a BOM at the start of the file, and spaces or tabs.

**RH8309.**
- **Mechanism.** `ReorderElements` permutes the canonical elements of `DocumentationCommentTriviaSyntax.Content` and leaves every other node in its slot.
- **Dispatch.** `Content` holds six node kinds: `XmlElementSyntax`, `XmlEmptyElementSyntax`, `XmlTextSyntax`, `XmlCommentSyntax`, `XmlCDataSectionSyntax`, and `XmlProcessingInstructionSyntax`. RH8309-004 already decides that unknown elements keep their slot.

### Breaking inputs

**RH8304, observed today:**
- `int A; /// …` repeats `int A;` (CS0102).
- `{ /// …` repeats `{`, and the result does not parse.
- `X, /// <summary>S</summary><remarks>R</remarks>⏎Y,` in an enum repeats `X,` (CS0102). This is a new sibling shape.
- `/* c */ /// …` repeats `/* c */`, so a user comment is duplicated (SHARED-FIX-003).
  - RH8304-004's normative sentence decides this case: the indentation is the white space at the start of the line.
  - Its coverage sentence names only code. Spec note: the coverage should say "code or a delimited comment".
- `U+FEFF/// <summary>S</summary><remarks>R</remarks>⏎class C` at the start of a file repeats `U+FEFF` at the start of the new line.
  - A BOM is the text's encoding signature (FMT-CLEAN-009), not indentation, so the expected new line starts with `///`.
  - .NET's `char.IsWhiteSpace('﻿')` is false while Roslyn lexes `U+FEFF` as whitespace, so the predicate the fix uses decides this case.
- `⇥int A; /// …` (`⇥` is a tab) repeats `⇥int A;`. Expected: `⇥/// <remarks>R</remarks>`.
- A stranded `int A; /// …` before `}` repeats `int A;`. RH8304 reports stranded comments, and RH8304-004's widened sentence covers them.

**RH8309:**
- The four comments of RH8309-005: reported, no fix.
- White space only between elements: the fix stays offered.
- CDATA and a processing instruction between elements: both are moved around today (Q6).
- Candidate, not run: an entity reference alone between elements (`/// &amp;`). It is text with a character that is not white space, so RH8309-005 withholds the fix.

**Failure classes:** FC-004 (relocating user text and comments), FC-016, and FC-021 (a comment before `///` on the edited line).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DocumentationCommentUtilities.GetContinuationPrefix` (`Reihitsu.Core`) | Shared by `DocCommentElementNormalizer` and RH8304's path. Changing it is a change to a shared helper with another consumer, which is escalation 5 for the lead. The formatter's own use has the same hazard on a BOM line (observed): `U+FEFF/// <summary>S</summary>⏎class C` is rebuilt with `U+FEFF` at the start of every rebuilt line. Changing only RH8304's path leaves that formatter defect as a follow-up candidate. |
| `DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync` | must change, directly or through the Core helper |
| RH8304 analyzer | must stay |
| RH8309 code fix | must change |
| RH8309 analyzer | must stay |
| Rule docs | RH8309 changed. RH8304 not involved (answer 3A). |

### Simpler option

_None._

### Questions for the maintainer

**Q6 — RH8309: a CDATA section or a processing instruction at the top level.** Found in passing.

*Context.* RH8309-005 withholds the fix for text or an XML comment beside the elements. It leaves CDATA sections and processing instructions uncovered.

Today both are displaced in exactly the way RH8309-005's Why describes:

```text
/// <remarks>R</remarks>⏎/// <![CDATA[x]]>⏎/// <summary>⏎/// S⏎/// </summary>⏎int A;
```

becomes

```text
/// <summary>⏎/// S⏎/// </summary>⏎/// <![CDATA[x]]>⏎/// <remarks>R</remarks>⏎int A;
```

The same happens with `/// <?pi x?>` in place of the CDATA section. These two are the only top-level node kinds RH8309-005 leaves open.

*Question.* Is the RH8309 fix withheld when a CDATA section or a processing instruction stands at the top level?

- **A — Withhold the fix for them too.** RH8309-005 is widened. Output: reported, no fix.
- **B — Sort around them as today.** RH8309-004's coverage is widened to state the output above.

*Recommendation: A.* RH8309-005's Why applies unchanged: the node stays where it is while the elements move.

### Follow-up candidates

See *Whole run*.

## thoenissen/Reihitsu#1242

### Premise

The maintainer decided this issue, so there is nothing to reproduce.

I checked the callers of `DocumentationCommentCodeFixUtilities` with `rg`:
- RH8107 and RH8202 call `CanRemoveElement` and `GetElementRemovalSpan`.
- RH8304 calls `MoveContentToNextDocumentationLineAsync`.
- RH8308 calls `IsHorizontalWhitespace`.

No other production code calls it. The spec diff moves the owner, adds SHARED-DOC-018, and RH8202's `Uses` no longer names RH8107's requirements.

### Mechanism and candidates

_N/A._

### Breaking inputs

_None._ The test implementer only adds the `SHARED-DOC-018` tag to existing tests, with no assertion change.

Notes for the spec checker:
- RH8107-006 and RH8202-003 repeat SHARED-DOC-018's rule in short form, and RH8107-006's example is identical to SHARED-DOC-018's. `sdd/README.md` says a rule that a shared policy states is referenced, not restated.
- The type that `shared/element-documentation.md` now owns also holds `MoveContentToNextDocumentationLineAsync`, whose output RH8304-004 specifies. #1344 changes that member. There is no spec conflict, because RH8304-004 states the output.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/README.md`, `RH8107.md`, `RH8202.md`, `shared/element-documentation.md` | changed by the spec author |
| Code | not involved |
| Tests | add tags only |

### Simpler option

_None._

### Questions for the maintainer

_None._

### Follow-up candidates

`squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md` is superseded by the change to RH8202's `Uses`. `02-spec-author.md` already lists it.

## Whole run

**Commands run.**
- `scripts/prepare.sh`
- `bash scripts/trace.sh <fixture>`
- `scripts/apply-fix.sh` for RH8301, RH8304, RH8305, RH8307, RH8309, RH8204, RH8107, and RH8202. It exits with 2 where an arm ends `invalid-result`.

**Clean-up.** The fixtures have been deleted. This handover is the only repository file I changed.

**Follow-up candidates.** All are outside the five issues, and none is verified against the open issues:
- **RH8309 fix formats the whole member.** On a positive input it turns `int A() => 0;` into a block body, which SHARED-FIX-006 forbids. The RH8305 and RH8307 fixes also format the whole declaration; for a stranded comment, that is the enclosing class.
- **Formatter copies a BOM.** In a documentation comment at the start of a file, the formatter writes `U+FEFF` at the start of every rebuilt line. This stays unless #1344's change goes into `GetContinuationPrefix`.
- **Stale comment in the RH8309 fix.** The comment in `ReorderElements` calls `<inheritdoc>`, `<seealso>`, and `<include>` unknown elements, but all three are in `CanonicalElementOrder`.
- **Own-line comments that document nothing are still normalized.** `int A;⏎⏎/// <summary>S</summary>⏎}` expands the summary to three lines (observed). Answer 2 gave a principle that would cover this comment too: a comment the compiler does not read as documentation is an ordinary comment. But the answer limited it to FMT-DOC-003, so the case is open for the maintainer later.
- **`scripts/trace.sh` is not executable.** It has mode 644, so it runs only as `bash scripts/trace.sh`, while every other `scripts/*.sh` has mode 755.

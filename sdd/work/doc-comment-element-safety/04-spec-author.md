Role: spec author
Commit: 6eeecb5
Verdict: #1329: spec written; #1389: spec written; #1241: spec written; #1344: spec written; #1242: spec written

This handover replaces `02-spec-author.md`. It carries two sets of input into the specs and rule pages:
- the maintainer's answers to the skeptic's questions, items 1–7 of "Maintainer answers to the skeptic's questions" in `00-lead.md`;
- every spec note in `03-skeptic.md`.

Where the answers conflict with my earlier text, the earlier text was rewritten. Each answer is written as a clarification of a principle that is already decided: SHARED-DIAG-010, FMT-PIPE-002, FMT-CLEAN-009, P19 / SHARED-DOC-017, and SHARED-FIX-003. None of them needed a new intended behavior, so there is no change doc and no open question.

No requirement ID was added in this round, so the ledger is unchanged; it is still sorted. Not touched: `shared/code-fixes.md`, `formatter/pipeline.md`, `formatter/blank-lines.md`, `sdd/README.md`, `squad/`, and the other files the parallel runs own.

Notation: `⏎` is a line break, `U+FEFF` the byte order mark.

## thoenissen/Reihitsu#1329

### Changed requirements

- **FMT-DOC-013 — changed** (answers 2A, 3B, 5A). The title now reads "…without an end tag of its own…".
  - It now covers any element that has no end tag of its own, as SHARED-DOC-017 defines it: no end tag at all, or one with another name, at any depth. End-tag names are compared exactly (prefix and local name), so `<Returns>Nothing</returns>` is covered.
  - A comment behind code that FMT-DOC-001 relocates is moved, its text is left as written, and the following declaration stays code.
  - "As written" means the text after the indentation of each line. The column stays FMT-INDENT's, as the skeptic observed and recommended writing down.
  - Coverage: a comment that starts its own line, or one that FMT-DOC-001 moves. A stranded comment is left to FMT-DOC-003.
  - Examples: the issue's two shapes, `/// <summary>S</returns>`, `/// <Returns>Nothing</returns>` (all unchanged), and the relocated `int A; /// <summary>S⏎int B;` → `int A;⏎⏎/// <summary>S⏎int B;`.
  - `Uses` gains SHARED-DOC-017.
- **FMT-DOC-004 — clarified:** a relocated comment is normalized unless FMT-DOC-013 leaves its text as written.
- **RH8301-002, RH8305-002, RH8307-002 — clarified** (answer 1A; SHARED-DIAG-010). Each now states that it does not report inside a `///` comment that FMT-DOC-013 or FMT-DOC-003 leaves as written. RH8307 does not report even a complete element of such a comment.
  - Each has a new negative example: the FMT-DOC-013 shapes and a stranded comment.
  - `Uses` gains FMT-DOC-003, FMT-DOC-013, SHARED-DIAG-010.
  - This removes the tension the skeptic noted between SHARED-DOC-017's examples and the coverage sentences of RH8305-001 and RH8307-001.
- **Rule pages:** `documentation/rules/RH8301.md`, `RH8305.md`, `RH8307.md` gain an `## Exceptions` section: a comment with a tag never closed or closed by another element's end tag is not reported, and neither is a comment behind code that documents nothing. The "no code fix" notes on RH8305 and RH8307 from round 1 are removed.

### Examples for the tests

- **FMT-DOC-013, every example.** Plus the skeptic's further shapes, all left as written with the code kept:
  - `/// <summary>A <see cref="C">x</summary>⏎int A;`
  - `///<summary>S⏎int A;`: no separator is added.
  - `/// <summary>⏎/// S⏎/// </summary>⏎/// <remarks>Text⏎int A;`: the collapsed branch; it must converge in two passes.
  - The same with `/// <remarks>a   `, white space at the end of the line.
  - `/// <summary>Runs</summary>⏎/// <remarks><returns>Nothing</remarks>⏎public void Run() { }`.
  - `int A;⏎⏎/// <summary>S⏎}`.
  - `/// <summary>S⏎int x = 1;` inside a method body.
  - Each in LF and CRLF, with an idempotency pass.
- **FMT-DOC-013, other boundary.** An element closed by its own end tag in a well-formed comment is still rebuilt (FMT-DOC-008 and FMT-DOC-009 examples). `/// <summary>S</returns>` is now left as written, which changes today's output.
- **RH8301-002, RH8305-002, RH8307-002.** The new negative examples, each not reported. Before this change all three rules reported these shapes.
- **FMT-PIPE-002 and FMT-PIPE-003** may be tagged alongside.

## thoenissen/Reihitsu#1389

### Changed requirements

- **FMT-DOC-003 — clarified** (skeptic spec note). The owning tokens are now examples: "such as a `;`, `)`, `]`, or `}`, a binary operator such as `+`, or the `.` of a member access". The rest is as in round 2: text left byte for byte, FMT-DOC-005 to FMT-DOC-011 not applied.
- **FMT-CLEAN-009 — clarified** (answer 4A).
  - The BOM is kept also when the first line holds nothing but the BOM and white space. The blank lines before the first line are removed (FMT-BLANK-007), and the BOM stays at the start of the text.
  - New prose example: `U+FEFF⏎class C{int x;}` → `U+FEFFclass C⏎{⏎    int x;⏎}`. The same holds with white space after the BOM, or with more blank lines after it, and a text of only the BOM and a line break keeps the BOM.
  - `Uses` gains FMT-BLANK-007.
  - **`formatter/blank-lines.md` needs no change.** FMT-BLANK-007 removes the blank line and does not decide the BOM; FMT-CLEAN-009 does. The code change answer 4 permits in `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` brings the code in line with these two requirements. No FMT-BLANK requirement contradicts the result.
- **FMT-DOC-011 — clarified** (answer 7A).
  - A line that a rebuild adds starts with the spaces and tabs in front of the comment's first `///` and nothing else. Code, a delimited comment, or a BOM in front of that `///` is not repeated.
  - New prose example: `U+FEFF/// <summary>S</summary>⏎class C⏎{⏎}` → only the first line keeps `U+FEFF`.
  - `Uses` gains FMT-CLEAN-009.

### Examples for the tests

- **FMT-CLEAN-009** (strings containing `U+FEFF`, not files; FC-033):
  - lose the BOM today: `U+FEFF⏎class C{int x;}`, `U+FEFF⏎⏎class C{int x;}`, `U+FEFF ⏎class C{int x;}`, `U+FEFF⏎// c⏎class C{}`, `U+FEFF⏎` alone;
  - must stay as today: `U+FEFF    class C{int x;}`, `U+FEFF// c⏎class C{int x;}`, `U+FEFFclass C`.
- **FMT-DOC-003**, all unchanged:
  - its examples;
  - `int A; /// <remarks>a⏎/// b</remarks>` before `}`;
  - `int A; /// <summary>⏎/// end</summary>` before `}`;
  - `M(1 /// <remarks>a⏎/// b</remarks>⏎);`;
  - `int A = 1 /// <summary>end</summary>⏎+ 2;`;
  - `var s = "a" /// <summary>end</summary>⏎.Trim();`;
  - `int A; ///<remarks>a</remarks>` before `}`: no separator.
  - Positive side: `int A; /// <summary>end</summary>⏎int B;` is still relocated and expanded (FMT-DOC-001, FMT-DOC-004).
- **FMT-DOC-011:** the BOM example, through the formatter. This is the second consumer of the `GetContinuationPrefix` change (answer 7A).

## thoenissen/Reihitsu#1241

### Changed requirements

- **SHARED-DOC-017 — clarified** (answers 1A, 3B, 5A, and the skeptic's spec notes).
  - The withheld fixes are now RH8107, RH8202, RH8204 (the reported element) and RH8309 (any top-level element). RH8305 and RH8307 are dropped from the list: under answer 1A they no longer report such comments, so a withhold would be empty.
  - Names are compared exactly, prefix and local name, as the compiler reads XML.
  - The Why is rewritten. Moving carries an end tag of another name, or takes the declaration into the element. The RH8309 withhold for an element that keeps its position rests on FMT-DOC-013 (incomplete documentation is corrected by hand), not on the formatter defect.
  - The RH8305 and RH8307 examples have moved to RH8305-002 and RH8307-002 as negatives. A new RH8107 example: `/// <Returns>Nothing</returns>`, reported, no fix.
  - `Uses` gains FMT-DOC-013. The intro sentence no longer says "lay out".
- **RH8305-004, RH8307-004** — the SHARED-DOC-017 pointer from round 1 is removed; their coverage still names "closed by its own end tag". `Uses` no longer lists SHARED-DOC-017.
- **RH8107-007, RH8202-004, RH8204-002, RH8204-003, RH8309-004** — unchanged from round 2. The exact name comparison reaches them through SHARED-DOC-017.
- **Rule pages:** `documentation/rules/RH8107.md` now names an end tag spelled differently (`<Returns>Nothing</returns>`). `RH8309.md` is updated under #1344.

### Examples for the tests

- **SHARED-DOC-017**, one test per rule example:
  - RH8107: `<remarks><returns>Nothing</remarks>` and `<Returns>Nothing</returns>`;
  - RH8202: `<value>The size`, plus the analogue `<remarks><value>The size</remarks>`;
  - RH8204: `<placeholder>Later text`, plus `<remarks><placeholder>Later text</remarks>`;
  - RH8309: all three comments.
- **RH8107-007 and RH8202-004:** the same withheld inputs.
- **RH8204-002:** its second example (locations).
- **Boundary, offered** (FC-003): own end tag; empty element (`<returns/>` example).
- **Boundary, withheld:** end tag of another name, including a case-only difference; no end tag.
- **Positive nested case** (skeptic): `<summary>a <see>b</x></summary>` is not the reported element, so the fixes keep working.
- **Fix All** (FC-008) over one withheld and one fixable diagnostic, for example two `<placeholder>` elements of which one is unclosed.

## thoenissen/Reihitsu#1344

### Changed requirements

- **RH8304-004 — clarified** (answer 7A and the skeptic's spec note).
  - The indentation is now defined as the spaces and tabs at the start of the line.
  - Whatever else stands in front of `///` is not repeated: code (a field, an enum member, an opening brace), a delimited comment, or the BOM at the start of the text.
  - Coverage: code, a delimited comment, or a BOM in front of the first line's `///`.
  - New prose examples for `X, /// …` in an enum, `/* c */ /// …`, and `U+FEFF/// …`.
  - The Why also covers the duplicated comment and the BOM.
- **RH8309-005 — widened** (answer 6A); title "…text, comments, and other nodes beside the elements".
  - A CDATA section and a processing instruction at the top level withhold the fix too.
  - Text of white space only (line breaks, `///` exteriors) does not.
  - Two more comments in the example: `/// <![CDATA[x]]>` and `/// <?pi x?>` between the elements.
- **RH8309-004 — clarified:** its coverage now says "nothing but white space between, before, and after" the elements. Before this, it still claimed the `Intro`/`Outro` comments that RH8309-005 now withholds.
- **Rule page:** `documentation/rules/RH8309.md` says "when the comment holds anything besides the elements and white space". RH8304's page is unchanged (answer 3A).

### Examples for the tests

- **RH8304-004:**
  - the two-class example;
  - the prose shapes `X, /// …` in an enum, `/* c */ /// …`, and `U+FEFF/// …` above `class C`;
  - `⇥int A; /// …` → new line `⇥/// <remarks>R</remarks>`;
  - a stranded `int A; /// <summary>S</summary><remarks>R</remarks>` before `}`, which RH8304 reports and RH8304-004 covers. RH8304 is a layout rule, but FMT-DOC-003's stranded case is not in its spec; see open question 1.
- **RH8309-005:** each of its six comments, reported, no fix.
  - Positive: white space only between elements, fix offered.
  - Candidate: `/// &amp;` alone between elements, withheld (text with a non-white-space character).

## thoenissen/Reihitsu#1242

### Changed requirements

- **RH8107-006 — clarified** (skeptic's note to the spec checker). It now says only "as SHARED-DOC-018 specifies". Its Why is its own, and the example is "see SHARED-DOC-018, whose example is the RH8107 fix". It no longer restates the shared rule or duplicates its example.
- **RH8202-003 — clarified:** the restating clause is removed. Its own sentence on nested elements and its example stay.
- Owner move and SHARED-DOC-018: unchanged from round 1.

### Examples for the tests

- **SHARED-DOC-018:** tag the existing tests of RH8107-006's former example and of RH8202-003; no assertion change.
- **RH8107-006:** tests keep their tag; they now derive from SHARED-DOC-018's example, which is the same input and output.

## Open questions for the maintainer

Found in passing, not blocking:

1. **RH8304 on a stranded comment.**
   - Input: `class C⏎{⏎    int A; /// <summary>S</summary><remarks>R</remarks>⏎}`.
   - Real output (skeptic, observed): RH8304 reports it, and its fix repeats `int A;`. RH8304-004 now covers it and produces `    int A; /// <summary>S</summary>⏎    /// <remarks>R</remarks>`.
   - Why it looks wrong: under answer 1A, a layout rule whose fix produces the formatter's layout does not report a shape the formatter keeps. FMT-DOC-003 keeps this comment as written. RH8304's fix does not call the formatter, but it writes the shape FMT-DOC-008 would write, and SHARED-DIAG-010 names layout rules in general.
   - Touches: RH8304-001, RH8304-004, SHARED-DIAG-010, FMT-DOC-003.
   - Readings: (a) RH8304 keeps reporting stranded comments and fixes them with indentation only; (b) RH8304 joins RH8301, RH8305, and RH8307 and does not report inside a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written.
   - Recommendation: (b), for the same reason as answer 1A. Until it is decided, I left RH8304-001 as it is. RH8304-004's widened coverage still names the stranded shape implicitly, through "code in front of its `///`".
2. **RH8308 inside a comment the formatter keeps** (not run). RH8308 reports text after an end tag, and its fix removes the text. It is a content-removal rule rather than a layout rule, so SHARED-DIAG-010 may not apply. I note it only so the maintainer can confirm that answer 1A's list (RH8301, RH8305, RH8307) is complete. Recommendation: no change; RH8308's subject is content.

## Whole run

- **Files changed in this round:**
  - `sdd/specs/formatter/documentation-comments.md`, `formatter/cleanup.md`;
  - `sdd/specs/shared/element-documentation.md`;
  - `sdd/specs/analyzer/RH8107.md`, `RH8202.md`, `RH8301.md`, `RH8304.md`, `RH8305.md`, `RH8307.md`, `RH8309.md`;
  - `documentation/rules/RH8107.md`, `RH8301.md`, `RH8305.md`, `RH8307.md`, `RH8309.md`;
  - this handover.
- **Architecture checks:** `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage|FullyQualifiedName~RuleDocumentation"` gave 50 passed and 1 failed. `EveryRequirementOfAnEnforcedSpecIsCovered` reports `RH8309-005`, `FMT-DOC-013`, and `SHARED-DOC-018` as "no test covers it"; the test implementer closes them. The `Uses` and structure checks pass.
- **Analyzer rule-documentation tests:** `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"` gave 7 passed.
- **Follow-up candidates for the lead**, from `03-skeptic.md`, outside the five issues:
  - the RH8309 fix formats the whole member (SHARED-FIX-006);
  - a stale comment in `ReorderElements`;
  - own-line comments that document nothing are still normalized;
  - `scripts/trace.sh` has mode 644;
  - the superseded `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md`.

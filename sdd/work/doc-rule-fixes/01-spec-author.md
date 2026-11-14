Role: spec author
Commit: 30ea03a
Verdict: #1345: spec written; #1346: spec written; #1347: spec written (one output question); #1348: spec written; #1270: spec written (one output question); #1092: spec written; #1153: spec written; #1617: no clarification

No change doc: every edit below writes down what an unchanged shared requirement (or a recorded maintainer answer) already demands; nothing in a shared requirement's meaning changes. The prompt carried no intended fix or verdict beyond the lead's triage summary.

## thoenissen/Reihitsu#1345

### Changed requirements

- RH8301-001 — A `///` line without a single-space separator is reported — clarified: a `///` documentation comment of a declaration written behind code is judged as if it started its line (SHARED-DOC-004); its first line is covered. Coverage still excludes a `///` behind code that no declaration follows (the RH8301-002 stranded example stays unreported).
- RH8301-004 — The fix writes the separator — clarified: for a comment behind code, the code and white space in front of the `///` stay (SHARED-DOC-004); new before/after example.
- RH8302-001 — Blank lines after a documentation comment are reported — clarified: a `///` comment behind code is covered; the blank lines after its last line are reported. `/** */` behind code stays not covered (SHARED-DOC-004 paragraph 2 names `///` only).
- RH8302-004 — The fix removes the blank lines — example added for a comment behind code (normative text unchanged).
- RH8303-001 — A documentation comment needs a blank line above it — clarified: a `///` comment behind code is reported (judged as starting its own line below the code); its continuation lines are not reported on their own; RH8303-002/-003 exempt it as they would on its own line (example: `{ /// <summary>A</summary>` not reported).
- RH8303-005 — The fix inserts one empty line above the comment — clarified: covers a comment that starts its own line; points to RH8303-006.
- RH8303-006 — The fix moves a comment written behind code onto a line of its own — added (ledger updated). Derived from SHARED-DOC-004 (the RH8303 fix moves it), SHARED-FIX-001 (one application, so the empty line goes in with the move), SHARED-FIX-007 (the first line lands in the declaration's column, white space behind the code removed). Checked against the formatter: `scripts/trace.sh` on `int A; ///Behind⏎int B;` and `int E; /// <summary>⏎    /// F</summary>⏎    int F;` gives `int A;`, blank line, comment in the declaration's column (FMT-DOC-001 + FMT-BLANK-012 + indentation).
- RH8303 header `Uses` gains SHARED-FIX-001, SHARED-FIX-007; RH8303-004 (location on the first `///`) unchanged and already right for the behind-code case.

### Examples for the tests

RH8301-001 (behind-code example), RH8301-004 (behind-code before/after), RH8302-001 (`int F; /// <summary>Sixth</summary>` run), RH8302-004 (behind-code before/after), RH8303-001 (behind-code example and the `{ ///` negative), RH8303-006.

### Output questions

_None._

### Internal choices

- The triage comment names RH8303-001/-005 only; RH8301-001 and RH8302-001 carried the same "starts behind code … not covered" sentence that SHARED-DOC-004 (which names RH8301, RH8302, RH8303) decides, so they are aligned in the same way. Alternative: leave them silent.
- The behind-code fix is a new requirement (RH8303-006) rather than a paragraph in RH8303-005, because it is a separate decision with its own reason. Alternative: extend RH8303-005.
- Rule pages RH8301–RH8303 unchanged: a comment written behind code is a corner case, not the normal case (`create-rule-doc`, role step 4). The issue's "change doc is written" and "rule pages state the behavior" goals are dropped per the triage comment.

### Non-goals

Delimited `/** */` comments behind code; the fixes of other rules for a behind-code comment (#1342); stranded `///` behind code before `}` (FMT-DOC-003).

## thoenissen/Reihitsu#1346

### Changed requirements

- RH8401-001 — clarified: the coverage sentence "what it reports in mode `None` is not covered" is replaced by a pointer to RH8401-006.
- RH8401-006 — Every documentation mode gives the same result — added (ledger updated): in mode `None` the analyzer reports exactly the lines it reports for the same source in mode `Parse`, and the fix gives the same text or is withheld as there (SHARED-DOC-008).
- RH8402-003 — clarified the same way, pointer to RH8402-007.
- RH8402-007 — Every documentation mode gives the same result — added (ledger updated): same as in mode `Parse` for report and fix; a `///` or `/** */` comment at the start of the file is treated as the documentation comment it is in mode `Parse` (ends the RH8402-006 run, header goes in front of it).

### Examples for the tests

RH8401-006 (mode `None`: two reported lines and their fix; `/// Stops <c>now</c>` not reported), RH8402-007 (mode `None`: missing header and its fix; old header followed by a `///` comment, which stays).

The existing test `VerifyNoDiagnosticsWhenDocumentationModeIsNone` (`src/Reihitsu.Analyzer.Test/Documentation/RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests.cs:96`) asserts the opposite of RH8401-006 and SHARED-DOC-008.

### Output questions

_None._

### Internal choices

- "Same result as mode `Parse`" rather than spelling out how `///` lines group into one comment and when they hold XML in mode `None`: SHARED-DOC-008's Why says the subject is the same in every mode, and in mode `None` the compiler reads each `///` line as a separate ordinary comment, so restating the grouping would be a second statement of a rule Roslyn defines. For RH8402 the reference also keeps RH8402-006 from swallowing a `///` comment into an old header's run in mode `None`. Alternative: state the grouping and XML detection for mode `None` explicitly.
- New requirements instead of edited coverage sentences, so the mode-`None` tests have one ID to tag. Alternative: fold into RH8401-001/RH8402-003.
- Rule pages unchanged: they do not mention documentation modes.

### Non-goals

RH8501 and other rules; RH0002.

## thoenissen/Reihitsu#1347

### Changed requirements

- RH8307-001 — clarified coverage: `/** */` is no longer "not covered"; it points to RH8307-002.
- RH8307-002 — Other layouts are not reported — clarified: an element of a `/** */` documentation comment, top level or nested, is not reported (FMT-DOC-012, SHARED-DIAG-010 — whose own example already shows exactly this for RH8307); new negative example.
- RH8307-004 — coverage sentence no longer points to RH8307-006.
- RH8307-006 — The fix is withheld for an element of a `/** */` comment — retired: with no diagnostic there is no fix to withhold.
- RH8307 header `Uses` drops SHARED-FIX-007 (only RH8307-006 referenced it).

The inline-formatting-element instance (#1312 Q6) is not clarified: see the output question.

### Examples for the tests

RH8307-002 (the `/** */` block with `int A` and `int B`).

Three tests carry `[Requirement("RH8307-006")]` (`src/Reihitsu.Analyzer.Test/Documentation/RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzerTests.cs:823`, `:846`, `:873`); `EveryRequirementReferenceNamesAnActiveRequirement` now fails on them until they are retagged.

### Output questions

**RH8307 · Content after opening XML tag must be on same line as closing tag — a wrapped inline formatting element**

Facts: the issue says FMT-DOC-009 leaves inline formatting elements open, so SHARED-DIAG-010 would forbid the report. FMT-DOC-009 only says it "does not yet decide" them, and the formatter on `main` does rewrite them (`scripts/trace.sh`, documentation-comment phase, output below). SHARED-DIAG-010 therefore does not demand the change ("A shape that the formatter changes MUST be reported"), and no spec decides the case: RH8307-001 and FMT-DOC-009 both leave inline formatting elements uncovered. The formatter side is #961. In the issue's own input the enclosing `<remarks>` element is half-wrapped too and stays reported under either answer; the input below isolates the `<b>` element.

Input:
```csharp
class C
{
    /// <summary>
    /// Value
    /// </summary>
    /// <remarks>
    /// Text <b>bold
    /// still bold</b>
    /// </remarks>
    int A;
}
```

Before (`main`, per the issue; the fix output is the formatter's, which the trace confirms): the `<b>` element is reported, and the fix gives
```csharp
    /// <remarks>
    /// Text <b>
    /// bold
    /// still bold
    /// </b>
    /// </remarks>
```

After, A (recommended): unchanged — reported, and the fix gives the output above.

After, B: not reported; formatting the file still gives the output above until #961 decides the formatter side.

Why A: the rule and the formatter agree today; B makes the rule silent on a shape the formatter still rewrites, and which elements count as "inline formatting" is undecided on both sides — it belongs with #961, where the formatter and the rule can be decided together.

### Internal choices

- RH8307-006 retired rather than rewritten: its normative sentence (fix withheld) has no diagnostic left to apply to. Alternative: keep it active with the text "not reported". Retiring an ID normally comes with a change doc; here no intended behavior changes (SHARED-DIAG-010 already required it), so I treat it as part of the clarification.
- Rule page RH8307 unchanged: the issue's "rule page states the exclusions" goal contradicts the rule-page convention (no exceptions on rule pages, `AGENTS.md`, *Key conventions*), and the page does not mention `/** */`.

### Non-goals

RH8304, RH8305, RH8308, RH8309 (already skip `/** */`); FMT-DOC-009/#961.

## thoenissen/Reihitsu#1348

### Changed requirements

- RH8302-001 — clarified: the coverage sentence no longer leaves a run before `#region`/`#endregion` "not covered"; the list of what may follow now says "a preprocessor directive other than `#region` and `#endregion` (RH8302-002)".
- RH8302-002 — Other comments and other positions are not reported — clarified: a run of blank lines directly followed by an active `#region` or `#endregion` is not reported (SHARED-BLANK-005 requires that line; otherwise RH8302 and RH5031 undo each other, SHARED-FIX-004); new negative example with both directives. `Uses` gains SHARED-BLANK-005, SHARED-FIX-004.
- FMT-BLANK-010 — No blank line after a documentation comment — clarified: when the next line is an active region directive, the blank line SHARED-BLANK-005 requires stays; the gap keeps one blank line (FMT-BLANK-013). New example taken from the issue's comment, with a two-blank-line variant before `#endregion`; the after block is the `main` formatter output (`scripts/trace.sh`, stable after pass 2). No formatter output changes.

### Examples for the tests

RH8302-002 (the `#region R` / `#endregion // R` block), FMT-BLANK-010 (the region example).

### Output questions

_None._

### Internal choices

- The other side of the qualifier "directly followed by an active region directive" is the existing RH8302-001 example `/// <summary>Fifth</summary>`, blank line, `#if !NEVER` (still reported). A region directive in excluded code is not reachable as a follower of a documentation comment, because the comment itself is then disabled text; "active" is kept for SHARED-BLANK-007 alignment.

### Non-goals

RH5031, RH5032 unchanged; blank lines below a region directive.

## thoenissen/Reihitsu#1270

### Changed requirements

- RH8106-001 — clarified: each top-level `<returns>` element is checked, and whether one is reported does not depend on its position (SHARED-DIAG-006); coverage widened from "at most one `<returns>` element" to "at most one `<returns>` element without text"; example with both orders. RH8106-002 unchanged.
- RH8203-001 — clarified: a field-like event with several events counts as inheriting nothing when at least one event is neither `override` nor implements an interface member, whatever its position (SHARED-DIAG-006; the maintainer's round-7a answer to #1200 Q1, which framed `Fourth` as the event the `<inheritdoc/>` wrongly covers); coverage no longer excludes such events; Why cites RH8201-001's mirror rule; example `First, Fourth` and `Fifth, Second`, both reported. `Uses` gains RH8201-001, SHARED-DIAG-006.
- RH8305-001 — clarified: every top-level `<summary>` of a comment is checked, whatever its position; coverage widened from "at most one `<summary>`"; example `int D` with a second, one-line summary. RH8305-003 is already one diagnostic per summary.
- RH8306-001 — clarified: each top-level `<summary>` is checked whatever its position; coverage widened, for a property in one part, to several summaries of which at most one is reported; examples `Depth` (second summary reported) and `Height` (first). Partial properties keep one summary per part.
- `Uses` of RH8106, RH8305, RH8306 gain SHARED-DIAG-006.

### Examples for the tests

RH8106-001 (`F`, `G`), RH8203-001 (the `Notifier` example), RH8305-001 (`D`), RH8306-001 (`Depth`, `Height`).

### Output questions

**RH8106 · Element return value documentation must have text, and RH8306 · Property summary must use a noun phrase instead of a sentence — several reported elements in one comment**

Facts: RH8106-002 and RH8306-004 say "one diagnostic per declaration / per property"; SHARED-DIAG-006 says the verdict on one element must not depend on its position. The two give different counts when more than one element is reported; the issue's instances have one reported element each and do not need the answer. The specs leave this case uncovered until answered.

Input:
```csharp
class C
{
    /// <summary>
    /// Finds
    /// </summary>
    /// <returns/>
    /// <returns></returns>
    public int F() => 0;

    /// <summary>Gets the size.</summary>
    /// <summary>Gets the length.</summary>
    public int Size { get; }
}
```

Before (`main`): one RH8106 on `<returns/>`; one RH8306 on the first `<summary>` (only the first element is read).

After, A (recommended):
```csharp
    /// {|RH8106:<returns/>|}
    /// {|RH8106:<returns></returns>|}
    public int F() => 0;

    /// {|RH8306:<summary>Gets the size.</summary>|}
    /// {|RH8306:<summary>Gets the length.</summary>|}
    public int Size { get; }
```

After, B:
```csharp
    /// {|RH8106:<returns/>|}
    /// <returns></returns>
    public int F() => 0;

    /// {|RH8306:<summary>Gets the size.</summary>|}
    /// <summary>Gets the length.</summary>
    public int Size { get; }
```

Why A: each element is what the user has to change, and under B the second element goes unmarked only because of where it stands, which is what SHARED-DIAG-006 rules out.

### Internal choices

- RH8203: one diagnostic per declaration stays (RH8203-002), located on the `<inheritdoc>` element; no per-event diagnostics, since the element is one and the declaration's verdict is about that element.

### Non-goals

A summary in a later documentation comment (#1311 Q2, first input; P8 territory); RH7403/RH7407/RH7411 events (#1333); RH8107 overlap (#1196 Q1).

## thoenissen/Reihitsu#1092

### Changed requirements

- SHARED-DOC-002 — Private and non-private groups — clarified: a partial type declared in several parts is judged as one declaration (SHARED-DIAG-004); its group comes from the modifiers of all parts together and every part belongs to it. The coverage exception for partial types is removed; Why extended; new negative example (#1037 Q3b input, both `S` parts documented on the first part only) "not reported by RH8002 or RH8004". `Uses` of `shared/element-documentation.md` gains SHARED-DIAG-004.

RH8002's own spec has no coverage sentence on partial types; it relies on SHARED-DOC-002, so the clarification is made there and reaches RH8002, RH8004, RH8006, RH8008, RH8010 alike (the #1037 Q3b input shows the split for all five kinds).

### Examples for the tests

SHARED-DOC-002 (the partial `Visible` and `S` example).

### Output questions

_None._

### Internal choices

- Clarified in the shared spec rather than in RH8002 alone (the triage comment says "the rule's coverage sentence"); the group is a SHARED-DOC-002 decision used by every split rule. Alternative: a sentence in each of RH8002/04/06/08/10.
- The example asserts only the private-group rules. What the non-private rules report on the parts of a partial type (documentation judged per part, report count) stays under SHARED-DOC-001's existing exclusion of partial elements; see follow-ups.

### Non-goals

Static constructors (#1448); whether a partial type counts as documented when another part is (#1037 Q3a).

## thoenissen/Reihitsu#1153

### Changed requirements

- RH8032-001 — clarified: a parameter without a name (`extension(long)`) is not reported; coverage now includes it; Why cites SHARED-DIAG-007; example adds a documented `extension(long)` block, not reported. `Uses` gains SHARED-DIAG-007.

### Examples for the tests

RH8032-001 (`extension(long)`).

### Output questions

_None._ The outcome "an unnamed parameter is not reported" is the maintainer's P8b answer recorded in the #1153 body (round 5 of #912); the triage split moved the rest of P8b to #1449, not this verdict.

### Internal choices

- Not reported, per the P8b answer, rather than reported on a token of the parameter (the type `long`), which SHARED-DIAG-007 alone would also allow. If the lead does not read the P8b answer as covering this, it is an output question with those two options.

### Non-goals

Everything moved to #1449 (`name` normalization, tag case, summary text, RH8030 decisions, RH8101–RH8111, RH8202, RH8306).

## thoenissen/Reihitsu#1617

No clarification. The premise is unconfirmed (found by reading code) and is to be reproduced first. SHARED-DIAG-004 states the requirement the issue relies on directly; RH8201-001 and RH8203-001 both leave "a part of a partial member declared in several parts" uncovered, which is silence, not disagreement.

Facts for the premise check: `DocumentationAnalysisUtilities.OverridesOrImplementsMember` is reached from `RH8201InheritdocShouldBeUsedAnalyzer` (directly for field-like events, and through `DocumentationAnalysisUtilities.CanInheritDocumentation`) and from `RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer` (through `CanInheritDocumentation`); no other rule calls either helper. The issue's input carries no documentation comment on either part of `C.M`, so neither rule reports anything for that exact input whatever the helper answers. Where several parts are reported, SHARED-DIAG-004 (at most once) and SHARED-DIAG-012 (which part) apply.

### Output questions

_None._

## Follow-up candidates

- **The non-private documentation rules report the parts of a partial type one by one.** Input: `public partial class P { }` and `public partial class P { }` (both undocumented), or the #1037 Q3b `S` after this run. Real output on `main`: RH8001/RH8003 judge documentation per part, so one type can be reported on each undocumented part, and a part counts as undocumented while another part is documented. SHARED-DIAG-004 asks for at most one report for a rule judging the type as a whole; SHARED-DOC-001 excludes partial elements. Requirement IDs: SHARED-DOC-001, SHARED-DIAG-004, RH8001-001. Probably already tracked by #1091 / #1037 Q3a — the issue manager checks for a duplicate.

## Whole run

- Specs touched: `sdd/specs/analyzer/RH8032.md`, `RH8106.md`, `RH8203.md`, `RH8301.md`, `RH8302.md`, `RH8303.md`, `RH8305.md`, `RH8306.md`, `RH8307.md`, `RH8401.md`, `RH8402.md`, `sdd/specs/formatter/blank-lines.md`, `sdd/specs/shared/element-documentation.md`, `sdd/specs/requirement-ids.txt` (RH8303-006, RH8401-006, RH8402-007 added, ledger order checked). No rule page, no change doc, no code-owner change, `sdd/specs/README.md` unchanged.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 49 passed, 2 failed, both expected until tests are written: `EveryRequirementOfAnEnforcedSpecIsCovered` (RH8303-006, RH8401-006, RH8402-007 have no test) and `EveryRequirementReferenceNamesAnActiveRequirement` (three tests tag retired RH8307-006). `SpecStructureTests` (including the exact `Uses` check) and `SpecConsistencyTests` pass.

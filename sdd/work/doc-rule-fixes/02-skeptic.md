Role: skeptic
Commit: f384c1d
Verdict: #1345: proceed (plus one escalation-10 question); #1346: proceed; #1347: proceed (spec author's output question stands); #1348: proceed; #1270: proceed (spec author's output question stands); #1092: do not make the change in this run (lead decision, see Internal choices); #1153: proceed; #1617: output question (premise reproduced)

The prompt carried no intended fix or hoped-for verdict. The lead's triage summary in `00-lead.md` was read as context only.

How facts were settled: `scripts/apply-fix.sh` for rules with a code fix, `scripts/format.sh` (run twice) for the formatter. RH8106, RH8203, RH8306, RH8032, RH8001/RH8002 and RH8201/RH8203 have no fix or need another documentation mode, and no repository entry point runs an analyzer alone or in mode `None`. For these I used a throwaway file-based app in the scratchpad (`analyze.cs`, about 30 lines, outside the repository). It references `Reihitsu.Analyzer.csproj`, compiles each fixture with `LanguageVersion.Preview` in the given documentation mode, and prints every RH diagnostic with its span, plus AD0001 and compiler errors. No fixture produced a compiler error unless noted. Fixtures live under the scratchpad and are not part of the repository.

## thoenissen/Reihitsu#1345

### Premise

Real. A caller reaches it: the analyzers run on any compiled source. Mode `Parse`, on `main`:

- `int A; ///Behind⏎int B;` gives no RH8301 (only RH8401, because the comment holds no XML). `int A; ///<summary>B</summary>⏎int B;` gives no RH8301 either.
- `int Z; /// <summary>D</summary>⏎⏎    int D;` gives no RH8302.
- `int A; /// <summary>⏎    /// B</summary>⏎    int B;` reports RH8303 on line 4's `///` (the second line). The single-line `int A; /// <summary>B</summary>⏎    int B;` gives no RH8303 at all, so the premise is wider than the issue's instance: today a behind-code comment is reported only through its continuation line.
- `class C⏎{ /// <summary>A</summary>⏎    int A;` gives no RH8303, as the new RH8303-001 negative example states.

SHARED-DOC-004, paragraph 2, already demands the judgment and names the RH8303 fix as the one that moves the comment. The spec diff only aligns RH8301-001/-004, RH8302-001/-004 and RH8303-001/-005, and adds RH8303-006.

RH8303-006 checked against the formatter: `scripts/format.sh` on the multi-line input, the single-line input, and a nested-namespace input whose declaration starts with `[System.Obsolete]` gives `int A;`, then a blank line, then the comment in the column of the declaration's first token (the `[` when there is an attribute). The formatter also expands the summary (RH8305's concern). The RH8303-006 example leaves `/// B</summary>` as written, which matches SHARED-FIX-006.

### Mechanism and candidates

Mechanism: the three rules decide coverage from where a comment's `///` starts on its line, not from whether the comment is a documentation comment of the next declaration. A `///` that follows a token on its own line therefore falls outside RH8301/RH8302, and RH8303 sees only the continuation lines that start a line. Dispatch: each rule registers on `SingleLineDocumentationCommentTrivia` (in mode `Parse`/`Diagnose` the behind-code comment is leading trivia of the next token, SHARED-DOC-004 Why). Candidates: RH8301 (every line), RH8302 (the run after the last line), RH8303 (the report and its location, plus the new fix). The other layout rules for documentation comments (RH8304, RH8305, RH8307–RH8309) belong to #1342 and are out of this run.

### Breaking inputs

FC-040 (documentation mode), FC-007/FC-008 (fix convergence and Fix All), FC-003 (both sides of each boundary), FC-021 (a comment on the header line of a moved construct).

- RH8301: `///Behind` behind code (first line reported); a multi-line behind-code comment whose second line also lacks the space (both lines reported, one diagnostic each); a tab as separator behind code; the fix keeps `int A; ` in front. Not reported: a stranded behind-code comment that FMT-DOC-003 keeps, before `}` (RH8301-002); any of these in mode `None` (SHARED-DOC-008).
- RH8302: the behind-code single-line comment with one and with two blank lines after it (one diagnostic per run); the fix leaves the comment behind the code. A `/** */` behind code is not reported.
- RH8303: single-line behind code (today not reported) → reported on the first `///`. Multi-line (today reported on line 2) → reported on line 1 only. `{ ///` → not reported (RH8303-002). Fix: LF and CRLF; declaration with an attribute list (column of `[`); declaration inside a namespace (two indentation levels); two behind-code comments in one type through Fix All (FC-008); the fixed result raises no RH8302/RH8303 (FC-007).
- Interaction: RH8302's fix leaves `int A; /// <summary>D</summary>⏎    int D;`, which RH8303 then reports. That is P29 as decided (only the RH8303 fix moves the comment), not a loop: RH8303's fix ends it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8301, RH8302, RH8303 analyzers | Must change |
| RH8303 code fix | Must change (RH8303-006) |
| RH8301, RH8302 code fixes | Must stay (comment stays behind the code); covered by tests only |
| Formatter | Not involved (already moves the comment; output checked above) |
| Rule pages | Must stay (no corner cases on rule pages) |
| `Reihitsu.Core` | Not involved, unless the implementation reuses `DocumentationCommentUtilities.IsWrittenBehindSource` (internal choice) |

### Refuse option

Leaving the behind-code comment unreported contradicts SHARED-DOC-004, an unchanged requirement, so it is not available. Withholding the RH8303 fix for a behind-code comment would be cheap and the class is rare, but SHARED-DOC-004 and the maintainer's P29 decision name this fix as the one that moves the comment. Withholding it would put a visible output in question.

Declaring the shape unsupported is an option, so I name it (escalation 10):

- **Span:** a `///` documentation comment whose `///` follows a token of the input on the same line, from that `///` through the comment's last line.
- **Exception:** none in mode `Parse`/`Diagnose`. In mode `None` the text is an ordinary comment, so the precheck's answer would depend on the documentation mode (FC-040).
- **Would delete:** the documentation-comment phase's move of a comment written behind code onto its own line, the RH8303-006 move added by this run, and the carry-over rules SHARED-DOC-004 states for fixes that move the documented declaration (the guard SHARED-ORDER-009 withholds).
- **Would refuse:** code people seldom write. It is usually a typo or a joined line.

I recommend against declaring it (see Output questions).

### Output questions

**S1 · Formatter and RH8303 · Element documentation header must be preceded by blank line — a `///` documentation comment written behind code**

Input:
```csharp
class C
{
    int A; /// <summary>
    /// B</summary>
    int B;
}
```

Before (`main`): the formatter moves the comment onto its own line:
```csharp
class C
{
    int A;

    /// <summary>
    /// B
    /// </summary>
    int B;
}
```
RH8303 reports the second `///` line, and its fix inserts a blank line inside the comment.

After, A: the input stays unchanged. The CLI reports the file as unsupported, naming the behind-code documentation comment and its line. The RH8301, RH8302 and RH8303 fixes, and every other fix whose formatted code holds the comment, are no longer offered on that code. Declared in a change of its own.

After, B (recommended): the formatter output is as on `main`. RH8303 reports the first `///`, and its fix gives
```csharp
class C
{
    int A;

    /// <summary>
    /// B</summary>
    int B;
}
```

Why B: you already decided (P29) that such a comment is judged as documentation and moved by the RH8303 fix, and the formatter handles it today with existing code. Declaring would delete the formatter's move of a comment behind code, the RH8303 move, and the carry-over of such a comment by fixes that move a declaration, and in exchange it would refuse every file that holds one.

### Internal choices

_None._ The spec author's choices (RH8301/RH8302 aligned alongside RH8303, and RH8303-006 as a separate requirement) hold. The lead accepted the first as decision 2; I agree with both.

### Issue goals

- "The rule pages … state the behavior": drop it. It is a corner case, and the convention keeps rule pages free of exceptions. Rule pages stay unchanged, so nothing visible changes.
- "The change doc is written": drop it. The triage comment moved the issue to the bug-fix lane. Nothing visible changes.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1346

### Premise

Real, for RH8401: a fixture with `/// workaround` inside a method body gives no diagnostics in mode `None`. In mode `Parse` the same fixture reports lines 5, 6 and 21 (the behind-code `/// Behind` included). The existing test `VerifyNoDiagnosticsWhenDocumentationModeIsNone` (`src/Reihitsu.Analyzer.Test/Documentation/RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests.cs:96`) asserts the opposite of SHARED-DOC-008.

RH8402 not run, because it needs a configuration the harness does not provide. The gate is explicit in code: `RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.cs:91-95` returns when `DocumentationMode == None`.

### Mechanism and candidates

- RH8401: two causes. It registers through `RegisterSyntaxNodeActionWithDocumentationModeCheck` (`RH8401…Analyzer.cs:76`), and it registers on `SyntaxKind.SingleLineDocumentationCommentTrivia`. In mode `None` the parser produces no such trivia; each `///` line is a `SingleLineCommentTrivia` of its own. Removing the gate alone therefore reports nothing.
- RH8402: an explicit early return.

Candidates: RH8401 analyzer and fix, RH8402 analyzer and fix. SHARED-DOC-008 names RH8501 too, which is out of the issue.

### Breaking inputs

FC-040 is the core class: the same text gives different trivia in each mode. FC-033: a test must really set mode `None` (`ApplyDocumentationModeNoneToTestProject`), not pass because no diagnostics are expected.

RH8401-006 asks for "the same lines as in mode `Parse`". These mode-`Parse` facts, observed with the harness, are what mode `None` must reproduce:

- `/// <summary>⏎/// Text⏎/// </summary>` on a member gives no report in mode `Parse`. A per-line verdict in mode `None` would report `/// Text`, which would add a diagnostic to every multi-line documentation comment in every project without documentation generation.
- `int A; /// x⏎    /// <c>y</c>⏎    int B;` gives no report in mode `Parse`: one comment across what are, in mode `None`, the trailing trivia of `;` and the leading trivia of `int`.
- `/// first⏎⏎/// <c>second</c>` reports `/// first` only. The blank line ends the comment.
- `/// Stops <c>now</c>` gives no report. `////` is never reported.
- RH8402 in mode `None`: a missing header; an old header followed directly by a `///` comment and by a `/** */` comment (RH8402-007 keeps it); an old header followed by `/**/`, which is an empty block comment and not documentation.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8401 analyzer | Must change (registration and mode-`None` grouping) |
| RH8401 code fix | Must stay (same text); needs a mode-`None` test |
| RH8402 analyzer and fix | Must change (gate, and the run of comments RH8402-006 replaces) |
| Existing test `VerifyNoDiagnosticsWhenDocumentationModeIsNone` | Must change (asserts the opposite of an unchanged requirement) |
| Rule pages, formatter, `Reihitsu.Core` | Not involved |

### Refuse option

Not reporting in mode `None` contradicts SHARED-DOC-008, so it is not available. Withholding the RH8401 fix in mode `None` saves nothing, because the rewrite is the same per line. Withholding the RH8402 fix in mode `None` when a `///` or `/** */` comment follows an old header saves only a text check for the `///`/`/**` opening, and that check is cheap. The class is rare: a documentation comment at the very start of a file.

A cheaper visible alternative would be to report in mode `None` only isolated `///` lines without `<`. It fails the spec's own RH8401-006 example (two consecutive plain lines), so the spec author's "same result as mode `Parse`" is the right statement.

### Output questions

_None._

### Internal choices

How mode `None` obtains the mode-`Parse` reading changes no visible output. The spec deliberately leaves it open (decision 3), and I agree.

### Issue goals

"The change doc is written": drop it (triage comment). Nothing visible changes.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1347

### Premise

- `/** */` instance: real. `/** <summary>Total⏎ * sum</summary> */` is reported by RH8307 on `main`, and `scripts/format.sh` leaves it unchanged (FMT-DOC-012). SHARED-DIAG-010's own example already demands no report. Decided, not in question.
- `<b>` instance: the issue's reason is wrong. `scripts/format.sh` on the issue's input rewrites `<b>bold⏎/// still bold</b>` into the block form (`Text <b>`, `bold`, `still bold`, `</b>` on lines of their own). The formatter does not leave inline formatting elements open: FMT-DOC-009 only says it "does not yet decide" them. SHARED-DIAG-010 demands a report only "as the rule's own requirements state", and RH8307-001 leaves inline formatting elements uncovered. Nothing decides the case, so the spec author's output question (Q1) is correct, and I agree with recommendation A.

### Mechanism and candidates

RH8307 judges every XML element of every documentation comment kind, without checking whether the formatter rewrites that comment's text. Candidates: top-level and nested elements of a `/** */` comment. RH8304, RH8305, RH8308 and RH8309 already skip `/** */` (per the issue; I did not check them, and they are not in scope).

### Breaking inputs

FC-013. Not reported: a one-line-start/multi-line-end element in `/** */`, top-level and nested; the same inside a `/** */` on a member of a record with a primary constructor (RH8307-003 mentions it). Still reported: the `///` counterpart in the same file. The three tests tagged `RH8307-006` (`…AnalyzerTests.cs:823`, `:846`, `:873`) assert "reported, no fix". Unchanged SHARED-DIAG-010 now demands "not reported", so their changed expectations are covered.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8307 analyzer | Must change (`/** */`) |
| RH8307 code fix | Its `/** */` withholding becomes unreachable (the FC-042 kind of leftover; the auditor should check it is removed or still serves something) |
| Formatter, rule page | Not involved / must stay |

### Refuse option

Not applicable: the change removes a diagnostic.

### Output questions

The spec author's Q1 (wrapped `<b>`) stands as written. No new one.

### Internal choices

_None._ Retiring RH8307-006 rather than rewording it is fine.

### Issue goals

- Drop the `<b>` instance's expected output ("not reported"). It rests on a false premise about the formatter, and its visible outcome is Q1.
- Drop "the rule page states the exclusions" (convention) and "the change doc is written" (triage). Neither changes visible output.

### Architecture findings

_None._

### Follow-up candidates

_None_ (the formatter side of inline elements is #961).

## thoenissen/Reihitsu#1348

### Premise

Real, on both sides of the loop:

- `/// <summary>Sixth</summary>⏎⏎    #region R` is reported by RH8302 (the blank line).
- With the blank line removed, RH5031 reports `#region R`.
- The `#endregion` variant behaves the same: RH8302 reports, and without the blank line RH5031 reports `#endregion // R`.
- With two blank lines before `#region`, RH8302 reports the run of two, and RH5023 reports the second blank line. After the change the excess is still reported, by RH5023.

Formatter: the FMT-BLANK-010 example in the spec diff, run through `scripts/format.sh` twice, gives exactly the spec's after-block and is stable.

### Mechanism and candidates

RH8302 treats every run of blank lines after a documentation comment alike, whatever line follows, while SHARED-BLANK-005 owns the line directly above an active region directive. Candidates for the following line: `#region` and `#endregion` (exempt); `#if`, `#pragma` and other directives (still reported, RH8302-001 example); an ordinary comment (still reported).

### Breaking inputs

FC-012 and FC-007 (the RH8302 ↔ RH5031 loop: applying every fix must leave the input unchanged), FC-020 (`#region` in the gap), FC-003 (one and two blank lines; `#region` and `#endregion`; `#if` stays reported). A region directive in an inactive branch is not reachable, because the documentation comment would be disabled text too (spec author's note; agreed).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8302 analyzer | Must change |
| RH8302 fix, RH5031, RH5023 | Must stay |
| Formatter output | Must stay (verified) |
| Formatter code | See Architecture findings |

### Refuse option

Withholding the RH8302 fix in front of a region directive would still leave a diagnostic that the formatter's output carries forever (FC-013), so not reporting is the cheaper and correct option. Declaring the shape unsupported is not an option: the formatter has no special case for it to delete.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

_N/A._ The goals hold.

### Architecture findings

Two rewriters of the blank-line phase decide the same gap. `BlankLineTokenCleanupRewriter` (`RemoveBlankLinesAfterLeadingDocumentationComments`, `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs:482`) removes the blank line after a documentation comment, and `BlankLineRegionDirectiveRewriter`, which runs later (`BlankLinePhase.cs:33-35`), puts it back in front of the region directive. The output is right only because of the order. The new FMT-BLANK-010 sentence ("MUST NOT remove the blank line … the result does not depend on which of the two requirements is applied last") is not true of today's code, although no phase output shows it. The issue's own comment reports this.

The lead decides between a narrow change in the cleanup rewriter and leaving the code with the spec's Why reworded. Neither changes output.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1270

### Premise

Real, for every named rule (mode `Parse`):

- RH8106: `<returns>Value</returns>` then `<returns></returns>` gives no report; the reverse order is reported on the empty one.
- RH8203: `public event EventHandler First, Fourth;` with `<inheritdoc/>`, where `First` implements `INotifier.First`, gives no report; `Fifth, Second` is reported. The fixtures were in two classes, so each gave a CS0535; the spec example puts both declarations into one class.
- RH8305: a three-line `<summary>` followed by a one-line `<summary>` in the same comment gives no report on the second.
- RH8306: `Current depth` then `Gets the depth.` gives no report; the reverse order is reported.

### Mechanism and candidates

Each rule reads only the first element of its kind, or the first declarator, and decides from it. RH8305 uses `FirstOrDefault(summary)` (`RH8305…Analyzer.cs:56`). RH8203 decides inheritance for a field-like event through `DocumentationAnalysisUtilities.GetDeclaredSymbol`, which takes `Variables[0]` (`DocumentationAnalysisUtilities.cs:527`), while RH8201 uses `All` (`RH8201…Analyzer.cs:97`).

I probed the other documentation rules that read one element kind, with the same element repeated in both orders:

- RH8204 (placeholders) reports both orders. It holds.
- RH8107 reports only the first `<returns>` on a `void` method. RH8202 reports only the first `<value>`. Both are the count question of the spec author's Q2: is each element reported, or one per declaration?
- RH8031 (extension summary text) is position-dependent. `<summary>First</summary>` then `<summary></summary>` on `extension(int v)` gives no report; the reverse on `extension(long v)` is reported. That is the RH8030/RH8031 several-summaries verdict that #1449 holds (moved there from #1153). It is outside this run (follow-up).

### Breaking inputs

FC-003 (both orders for each rule), FC-016 (siblings), FC-008. The RH8305 fix targets the diagnostic's span (`DocumentationCommentCodeFixUtilities.LayOutElementAsync`), so a second summary is fixable. Two one-line summaries in one comment are both reported after the change; Fix All must lay out both, in at most one extra iteration. The formatter expands both summaries whatever their order (`scripts/format.sh`, both orders), so reporting the second is consistent with SHARED-DIAG-010.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8106, RH8203, RH8305, RH8306 analyzers | Must change |
| RH8305 code fix | Must stay (span-based); add a two-summary Fix All test |
| RH8201 | Not involved (already `All`) |

### Refuse option

Not applicable (analyzer verdicts; no fix rewrites).

### Output questions

The spec author's Q2 (RH8106/RH8306 several reported elements) stands. RH8107 and RH8202 show the same count question, but they are outside the run's issues and go to the follow-ups.

### Internal choices

_None._

### Issue goals

_N/A._ The goals hold.

### Architecture findings

_None_ (each rule's own first-element lookup; no shared decision).

### Follow-up candidates

- **RH8031 · Extension declarations must have summary text — several `<summary>` elements.** Its verdict depends on their order (SHARED-DIAG-006). Belongs with #1449, which holds the RH8030 several-summaries decision.
- **RH8107 (void return value must not be documented) and RH8202 (value tag must not be used) — several elements.** Only the first is marked. Same output question as the spec author's Q2. The issue manager should file it, or extend #1449/#1196, carrying the question.

## thoenissen/Reihitsu#1092

### Premise

Real. In the spec's own example, RH8002 reports the second, modifier-less part of `public partial class Visible`, and RH8004 reports the second part of `internal partial struct S`.

What the issue does not say: the non-private rules judge documentation part by part. With `public partial class Visible` documented and a second `public partial class Visible` undocumented, RH8001 reports the second part. With both parts undocumented, RH8001 reports each part.

Once the group comes from all parts together (the clarified SHARED-DOC-002), the issue's own input, whose second part is undocumented, moves from RH8002 to RH8001: the same diagnostic under another ID. That contradicts the maintainer's #1037 Q3a answer (a partial type is documented when any part provides the summary), tracked in #1091 (vNext, behavior change).

### Mechanism and candidates

The group is decided from the part's own modifiers, not from the type's. Candidates: RH8002, RH8004, RH8006, RH8008 and RH8010 (private-group rules for every type kind), and their non-private counterparts, which inherit the parts.

### Breaking inputs

FC-024 (partial types). Covered both ways: a part without a modifier next to `public`, `internal`, `protected`, `private` and `private protected`; a nested record and record struct; the both-undocumented case; the case where the documented part is the second one.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `SplitElementDocumentationAnalyzerBase` / group decision | Must change, if the change is made |
| Non-private rules (RH8001 …) | Their output changes with the group, see Internal choices |
| Rule pages | Not involved |

### Refuse option

Not changing it in this run. Cost: none. The issue stays open, `Refs #1092`, with a characterization test.

### Output questions

_None_, under the recommendation below. If the lead wants option A, it is an output question.

### Internal choices

What the run does for #1092. Options, for the issue's input (`Visible` documented on its first part, second part without a modifier and undocumented):

- **A — group fix only.** The second part is reported by RH8001 instead of RH8002. It contradicts the #1037 Q3a answer, and projects that turn RH8002 off but keep RH8001 get a new warning on a documented type. That puts a visible output in question, so it cannot be chosen without the maintainer.
- **B — group fix plus #1037 Q3a for partial types.** Nothing reported. The output is covered by the maintainer's Q3a answer, but the change is a behavior change with a change doc, and it pulls #1091 (vNext) into this run. The lane widens to behavior change.
- **C — no code change in this run.** RH8002 still reports the second part. `Refs #1092`, a characterization test, and a note on #1091 that #1092 should land together with it. The SHARED-DOC-002 clarification and its example are then reverted. Otherwise a spec requirement has no test, and the example asserts what the code does not do.

Recommendation: **C**. It is the only option that neither moves a wrong diagnostic into the rule more projects keep on nor widens the lane into vNext work. B is the pick if the lead decides to widen.

### Issue goals

"Both inputs are reported by the decided rule": Q4 is gone to #1448, and Q3b's "decided rule" yields no correct output without Q3a. Under C the goal is deferred, which changes nothing visible.

### Architecture findings

_None._

### Follow-up candidates

The spec author's follow-up (non-private rules judge partial types part by part) is #1091 / #1037 Q3a. It is not a new issue. The issue manager should link #1092 to #1091.

### Spec diff defects (whatever the decision)

- `sdd/specs/shared/element-documentation.md`: the new partial-type example appears twice. The second copy (lines 566–598) sits under SHARED-DOC-009, which is about explicit interface implementations, in front of its "Example — RH8021" heading. It must be removed.
- SHARED-DOC-002's new sentence lists `file` among the modifiers of "another part" of a nested type. A nested type cannot be `file`, so the word is unreachable; drop it.
- The example asserts only RH8002/RH8004 and is silent on RH8001/RH8003, which report the second part under A.

## thoenissen/Reihitsu#1153

### Premise

Real. `extension(long)` with a summary gives RH8032 with no source location. The class is wider. Running every analyzer over `extension(long)`, `extension<T>(T[])` and `extension(int)` (mode `Parse`) also gives **RH4105** (method parameter names should be in camelCase) without a source location, twice per block, plus RH8032 on the documented blocks. `extension(ref short)` is CS9305 and does not compile, so it is not a case.

### Mechanism and candidates

A rule that locates its diagnostic on a parameter's identifier gets `Location.None` for the receiver parameter of an extension block that has no name. Candidates found by running all analyzers: RH8032 and RH4105. RH8033 (type parameters) is always named.

### Breaking inputs

SHARED-DIAG-007. Not reported: `extension(long)`, `extension<T>(T[])`, with and without other documented members. Still reported: `extension(int value)` without `<param>`.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8032 analyzer | Must change |
| RH4105 analyzer | Same class, other component: follow-up (or an adjacent fix, lead's call) |

### Refuse option

Not reporting is the cheapest option, and it is the outcome of the maintainer's recorded P8b answer ("An unnamed parameter is not reported", decision 7). Agreed.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

Everything except the location bug moved to #1449. "Done when" now means only: no RH8032 diagnostic without a source location.

### Architecture findings

_None._

### Follow-up candidates

- **RH4105 · Method parameter names should be in camelCase — unnamed receiver parameter.** `extension(long)` and `extension<T>(T[])` give RH4105 without a source location (SHARED-DIAG-007). The RH4105 spec does not mention extension blocks. Under the P8b reading the expected output is presumably "not reported" (nothing to rename); the follow-up should state that as its question.

## thoenissen/Reihitsu#1617

### Premise

The issue's own input reproduces nothing: neither part of `C.M` carries a documentation comment, so neither rule has anything to report whatever the helper answers. With documentation added, the premise is real (mode `Parse`, every partial member kind):

- **RH8203**, every part carrying `/// <inheritdoc/>` on a member that implements an interface member:
  - method: the implementing part is reported, the defining part is not;
  - property `P`, indexer `this[int]`, event `E` (fixture `prop.cs`): the implementing part of each is reported (three diagnostics), the defining parts are not.
- **RH8201**:
  - both parts carry `/// <summary>Does it.</summary>`: reported once, on the defining part. `scripts/apply-fix.sh RH8201` replaces only the defining part's summary and converges in one iteration, because the implementing part counts as implementing nothing;
  - only the implementing part documented: nothing reported.
- `override partial`: nothing reported. The `override` modifier is checked syntactically (`DocumentationAnalysisUtilities.HasSyntaxLevelInheritance`, `DocumentationAnalysisUtilities.cs:492`; RH8201 also at `RH8201…Analyzer.cs:91`).
- Sibling: a partial method that inherits nothing, with `<inheritdoc/>` on both parts, is reported by RH8203 twice, once per part.

### Mechanism and candidates

`DocumentationAnalysisUtilities.OverridesOrImplementsMember` compares `FindImplementationForInterfaceMember(...)` with the symbol of the part it is given (`InterfaceImplementationUtilities.cs:58`). For the implementing part of a partial member that comparison fails, so the implementing part never counts as implementing.

Dispatch: callers of `CanInheritDocumentation`/`OverridesOrImplementsMember`, namely RH8201 and RH8203 (spec author, confirmed by `grep`). Member kinds: method, property, indexer and event fail; constructor takes `HasMatchingBaseConstructor`; `override` takes the syntactic path; explicit implementation cannot be partial.

An existing helper already handles partial parts for the interface-region rules RH7409–RH7412: `PartialMemberUtilities.GetStandInSymbol`. Its choice follows generated code, so it is not a drop-in answer.

### Breaking inputs

FC-024 and FC-016 (every member kind above), FC-007 (RH8201 fix convergence on a partial member). The test implementer covers RH8203 and RH8201 for the method, property, indexer and event, each with the documentation on the defining part, on the implementing part, and on both. These inputs are not decided: see the output question.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DocumentationAnalysisUtilities` inheritance decision | Must change |
| RH8203 analyzer | Must change (implementing part not reported) |
| RH8201 analyzer and fix | Output changes, see the output question |
| RH7409–RH7412 | Not involved (own helper) |
| Specs RH8201-001, RH8203-001 | Coverage excludes partial members; a clarification is needed once the output question is answered, so the tests have a requirement to tag |

### Refuse option

Withholding nothing helps here: RH8203's wrong verdict is a false positive, not a fix. The refuse option for the RH8201 fix is to withhold it for a partial member documented on both parts. That class is rare, and the cost is only the check that both parts carry documentation.

### Output questions

**S2 · RH8201 · Inheritdoc should be used, and RH8203 · Inheritdoc must be used with inheriting class — a partial member with documentation on both parts**

Two readings of SHARED-DIAG-004 give different output. Either the rules judge the partial member once, as one declaration, or each part's own comment on its own.

Input:
```csharp
/// <summary>I</summary>
public interface I
{
    /// <summary>Does it.</summary>
    void M();
}

/// <summary>C</summary>
public partial class C : I
{
    /// <summary>Does it.</summary>
    public partial void M();
}

/// <summary>C</summary>
public partial class C
{
    /// <summary>Does it.</summary>
    public partial void M() { }

    /// <inheritdoc/>
    public partial void N();
}

/// <summary>C</summary>
public partial class C
{
    /// <inheritdoc/>
    public partial void N() { }
}
```

Before (`main`):
```csharp
    /// {|RH8201:<summary>Does it.</summary>|}       // defining part of M; the fix replaces only this summary
    public partial void M();
    ...
    /// <summary>Does it.</summary>                // implementing part of M: not reported
    public partial void M() { }

    /// {|RH8203:<inheritdoc/>|}
    public partial void N();
    ...
    /// {|RH8203:<inheritdoc/>|}
    public partial void N() { }
```

After, A (recommended): each member is reported once, on its defining part. No RH8201 fix is offered for `M`, because a correct result needs both parts changed.
```csharp
    /// {|RH8201:<summary>Does it.</summary>|}       // defining part of M; no fix offered
    public partial void M();
    ...
    /// <summary>Does it.</summary>
    public partial void M() { }

    /// {|RH8203:<inheritdoc/>|}
    public partial void N();
    ...
    /// <inheritdoc/>
    public partial void N() { }
```

After, B: each part is reported on its own, and each RH8201 fix replaces that part's summary.
```csharp
    /// {|RH8201:<summary>Does it.</summary>|}
    public partial void M();
    ...
    /// {|RH8201:<summary>Does it.</summary>|}
    public partial void M() { }

    /// {|RH8203:<inheritdoc/>|}
    public partial void N();
    ...
    /// {|RH8203:<inheritdoc/>|}
    public partial void N() { }
```

Why A: a partial member is one member with one produced documentation, and the rule for which part carries a partial member's one diagnostic already names the documentation comment as the deciding fact.

Under both readings, two outputs change in the same way and are not in question: `<inheritdoc/>` on the implementing part of a member that implements an interface member is no longer reported by RH8203, and a summary only on the implementing part of such a member is reported by RH8201.

### Internal choices

_None_ beyond the question.

### Issue goals

- "The helper looks at both parts of a partial member": a prescribed solution; drop it. The outputs above are the goal.
- "One test per documentation rule that uses the helper": keep it, as RH8201 and RH8203 times the four member kinds.
- The issue's input stays as one characterization test (nothing reported).

None of this changes visible output beyond S2.

### Architecture findings

_None_ within the run. `GetStandInSymbol` and the new decision answer different questions.

### Follow-up candidates

_None._

## Whole run

- Output questions for the lead's batch:
  - S1 (#1345, escalation 10: declaring a behind-code `///` unsupported; recommend not declaring);
  - S2 (#1617);
  - the spec author's Q1 (#1347) and Q2 (#1270), both of which I endorse.
- Spec-diff defects to route to the spec author: the duplicated partial-type example under SHARED-DOC-009; `file` in the SHARED-DOC-002 sentence; RH8201-001/RH8203-001 coverage for #1617 after S2; the SHARED-DOC-002 clarification reverted if #1092 takes option C.
- Follow-up candidates for the issue manager:
  - RH4105 without a location on unnamed extension receivers;
  - RH8031 order-dependent summaries (with #1449);
  - RH8107/RH8202 several elements (count question);
  - #1092 linked to #1091.
- No repository file other than this handover was edited. Scratch fixtures and the harness are under the session scratchpad.

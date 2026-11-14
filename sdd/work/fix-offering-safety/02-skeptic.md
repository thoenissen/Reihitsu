Role: skeptic
Commit: 1bc6f66
Verdict: #1600: proceed narrower + output question; #1150: proceed narrower (spec diff needs a correction); #1274: output question; #1143: proceed; #1673: proceed narrower; #1678: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict; it asked me to check the spec author's claims, which I did on the evidence below.

How I ran things: `scripts/build.sh` (Release, exit 0). `scripts/apply-fix.sh` and `scripts/format.sh` on fixtures in the scratchpad. `apply-fix` refuses fixtures with a syntax error and defines no preprocessor symbols, so for those cases I used a throwaway file-based probe in the scratchpad (outside the repository). It references `src/Reihitsu.Tooling` and does the following:

- resolves the analyzer and provider with `CodeFixTargetResolver`;
- parses with optional `--symbols`;
- registers the fixes for every diagnostic and flags any action that returns the document unchanged;
- applies either the first action or one document-scope Fix All (`GetFixAllProvider()` with every diagnostic);
- re-analyzes the result.

Every "observed" below comes from one of these runs.

## thoenissen/Reihitsu#1600

### Premise

Real. It is reachable by code fixes in the IDE and by the CLI.

- **Code fix (IDE, project defines `A`).** I ran the issue's input with `--symbols=A`. RH3004 reports `delegate` and offers 1 action. The action rewrites the block into a lambda and re-indents its lines. Without `A`, those lines are the content of `s`, so `s` changes value.
- **CLI (no symbols).** The mirrored input `#if !A` / `/*` / `#else` / `var s = @"` / `*/` / … / `#endif` (same lines as the issue, with `!A`) goes through `scripts/format.sh`, and the formatter re-indents the `{ … }; //";` lines from column 12 to column 8. In a build with `A` those lines are string content.

Today's CLI output for that input (the lines that change):

```csharp
        Action a = delegate ()
        {
            Console.WriteLine(1);
        }; //";
```

### Mechanism and candidates

Mechanism: an edit whose text is, in another build configuration, inside a string literal or a comment whose opening that build compiles and this build does not. No component looks at the build configurations in which the edited text is not code.

The class is wider than the issue's shape. Its axes:

1. **Where the hiding carrier starts, as seen in the analyzed build.**
   - **(a)** In active text. The carrier, a multi-line string or a `/* */` comment, starts inside a conditional group and holds that group's `#elif`/`#else`/`#endif` line as text. This is the issue's shape.
   - **(b)** In disabled text. The analyzed build has no carrier at all, only disabled text that holds an unterminated `@"`, `"""`, or `/*`.

   Member (b) reproduces:

   ```csharp
   class C
   {
       int M(int a, int b)
       {
   #if B
           var s = @"
   #endif
               var x = a+b;
   #if B
   ";
   #endif
           return 0;
       }
   }
   ```

   It compiles with and without `B`. Without `B`, both surfaces change `s` of the `B` build:
   - `scripts/format.sh` rewrites line 8 to `        var x = a + b;`;
   - the RH6024 fix rewrites `a+b` to `a + b`.

   The issue's proposed helper ("a string or delimited comment before or inside this span holds a `#` line") cannot see (b): there is no string or comment in the analyzed tree, only `DisabledTextTrivia`. Finding (b) means lexing excluded code. SHARED-FIX-009 forbids that for fixes, and *Special cases do not grow the source* forbids it for the formatter.
2. **Carrier kind.**
   - Verbatim and interpolated-verbatim strings, and multi-line raw and interpolated raw strings, change run-time values.
   - A `/* */` comment changes comment content only, which is a SHARED-FIX-003 content change, not a meaning change.
   - Single-line `//` comments and regular strings cannot span lines, so they are not members.
3. **Where the edit lies.** Inside the active carrier, or behind it. RH3004-008 covers the "inside" case for RH3004 only; nothing covers the "behind" case.
4. **Surface.**
   - Every code-fix registration site (105 `RegisterCodeFixesAsync` overrides in `src/Reihitsu.Analyzer.CodeFixes`). Renames through the Renamer count too: a reference in hidden text is a string in the other build.
   - Whole-document formatting.

On the precision of member (a): a carrier that starts in a conditional group G can hide code only if it holds G's next `#elif`, `#else`, or `#endif` line. Without one, the build that excludes G's branch skips the carrier whole. So the necessary line kinds are `#elif`, `#else`, and `#endif`; a Markdown `# Heading` or `#nullable` line in a string is never a member. This corrects the spec author's "a `#` line" for anything outside RH3004-008.

### Breaking inputs

Failure classes: FC-020, FC-022, FC-016 (RH3004 guards its own span only), FC-015 (`RH3004UseLambdaSyntaxCodeFixProvider.ContainsLineStartingWithHash` is a private copy that a shared helper would duplicate), FC-041 (see *Refuse option*).

The test implementer covers:
- (a) the issue input with `A` defined for a fix: RH3004, plus one text-edit fix such as RH6024 placing `a+b` behind the carrier;
- (a) the mirrored `#if !A` input for whole-document formatting;
- a raw-string variant of (a);
- a `/* */` carrier whose `#` line is `# Heading` or `#nullable`, as a not-a-member control;
- (b) the input above, as a characterization test until the maintainer answers question 1b.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Code fixes (all registration sites whose edit can lie behind such a carrier) | must change for (a): withheld (SHARED-FIX-003, already decided) |
| `Reihitsu.Core` | must change: one shared check. RH3004's private `ContainsLineStartingWithHash` should then not stay a second statement (FC-015) |
| Formatter (whole document, CLI) | output question 1 |
| Formatter node-level entry points | only through SHARED-FIX-010 if the shape is declared |
| Analyzers | not involved: diagnostics stay |
| Rule pages | not involved (`general-notes.md` covers withheld fixes) |

### Refuse option

The refusal is the requested change: the fix is withheld for the class. Its cost is not the guard but its reach. A cheap check placed at about 105 registration sites, or behind a new common base type, adds well over 100 production lines or a new type. That is **escalation 9** territory, and the lead should decide or escalate before the implementer starts. The class is rare: nobody writes it on purpose.

**Declaring the shape unsupported is an option (escalation 10).** The precise span is a string literal of any kind or a delimited comment that spans lines, from its first character through its closing delimiter, and holds a later line whose first non-whitespace text is `#elif`, `#else`, or `#endif`. There are two variants:

- **(i) As stated.** Decidable from the input alone. It also refuses commented-out code blocks that hold directives (`/* … #if DEBUG … #endif … */`) and source-generator templates in raw strings that hold `#if`/`#endif`. People write both.
- **(ii) Also requiring the carrier to start inside a conditional group.** This avoids refusing that common code. It reads the `#if` in front of the carrier, which for a node-level input can lie outside the input (FC-041). FMT-UNSUP-003 allows that, because such a node holds only part of the shape and is not refused. But then SHARED-FIX-010 does not withhold a fix whose node holds only the carrier, and the Core guard is still needed.

Special cases either variant deletes: none in the formatter, which has no guard today. In the fixes, only RH3004-008 item 1 for the `#elif`/`#else`/`#endif` lines, where RH3004 formats the node that holds the carrier. RH3004-008 also counts every other `#` line, so most of it stays. `AGENTS.md` says a shape that deletes nothing is not declared. The question still goes to the maintainer, as the spec author noted. My recommendation is not to declare and to leave the formatter on question 1's option C or a FMT-PIPE requirement.

Member (b) as an unsupported shape ("disabled text that holds `@"`, `"""`, or `/*`"):
- It is cheap only as a substring scan of disabled text.
- It deletes nothing.
- It refuses every file whose `#if DEBUG` block holds a verbatim string such as `@"C:\temp"`, which is common code.

Recommend not.

Simpler option: none for (a). For (b), record the gap as a decision (question 1b).

### Output questions

The spec author's question 1 (whole-document formatting, member (a)) stands. Two amendments:

- Its *Before* is now observed: the block's lines move from column 12 to column 8, as shown under *Premise*.
- Its option A should state which of the shape variants (i)/(ii) above it means, because the set of refused files differs. I recommend (ii) if A is chosen.

New question — **1b · Code fixes and whole-document formatting — a string that only an excluded branch opens**

Input (formatted by the CLI, which defines no symbols; the IDE fix RH6024 is offered on `+`):
```csharp
class C
{
    int M(int a, int b)
    {
#if B
        var s = @"
#endif
            var x = a+b;
#if B
";
#endif
        return 0;
    }
}
```

Before (`main`): the CLI and the RH6024 fix both rewrite line 8:
```csharp
        var x = a + b;
```
(In a build with `B`, that line is the content of `s`.)

After, A (recommended): unchanged from today; the gap is recorded as a decision in SHARED-FIX-003 and in the formatter spec.

After, B: no fix offered and the file left unchanged, for every document whose disabled text holds an opening `@"`, `"""`, or `/*`. This also covers `#if DEBUG` blocks with an ordinary `@"…"` string, which is common code.

Why A: finding the hidden string means reading excluded code as a compiler would, which no fix may do (SHARED-FIX-009). The only cheap refusal withholds fixes and formatting in many ordinary files.

### Internal choices

- The guard's span for (a). The spec author recommends: withhold when such a carrier starts at or before the end of the fix's edit. Agreed, with the line kinds narrowed to `#elif`/`#else`/`#endif`. The guard must not try to find where the hidden text ends in the other build, which would be SHARED-FIX-009 machinery.
- Whether to require "starts inside a conditional group" on the fix side. It needs a scan of the document's directives in front of the carrier, which is cheap and syntactic. Recommend yes: it keeps generator templates in unconditional code fixable.

### Issue goals

- "A Core helper answers 'does a string or delimited comment before or inside this span hold a line whose first non-whitespace character is `#`'". Change it: the line kinds become `#elif`/`#else`/`#endif`. A plain `#` would withhold fixes in every file with a Markdown raw string. This changes visible output only toward offering more fixes than the issue's wording.
- "The code fixes and the formatter consult it": the formatter part is output question 1, and every-fix reach is escalation 9.
- "A formatter instance … moving a line inside a verbatim string": no such instance was given or found. Drop it unless the test implementer finds one.

### Architecture findings

No component owns "is this text code in every build". RH3004 holds a private partial answer (`ContainsLineStartingWithHash`, RH3004-008), and the run would add a second, shared one. The run should make the shared check the single owner (FC-015).

### Follow-up candidates

None beyond question 1b.

## thoenissen/Reihitsu#1150

### Premise

Item 1 is real but narrower than the issue's list. Item 2 is already resolved on this tree.

**Item 1, observed in a file with `void N(` unclosed:**

No-op fixes, where the action returns the document unchanged (SHARED-FIX-005):

| Rule | Base or helper |
|---|---|
| RH5412 | `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` |
| RH3202 | `FormatMemberOnLinesOfItsOwnAsync` |
| RH3203 | `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` |
| RH5102 | — |
| RH5308 | — |
| RH5112 | `FluentChainCodeFixHelper` |
| RH5401 | — |
| RH5417 | — |

Edit-then-format fixes, where the edit is applied unformatted (SHARED-FIX-007):
- RH3004 writes `Action a = () => { M(); };` on one line, where the same document without the error gets the multi-line lambda.
- RH2001 builds its field and then formats it.

**Not affected.** These fixes do not use a node-level entry point, or their analyzer does not report in such a file. Most of the issue's list predates #1686 and #1688, which made these fixes text edits.

| Fix | Status |
|---|---|
| `OperatorSpacingCodeFixProviderBase` (RH6023, RH6024, …) | Text edit. Observed: RH6023 in the syntax-error file writes `a = 1;` and the diagnostic is gone |
| `StatementBracesCodeFixProviderBase` (RH5405–RH5407) | Observed: RH5405 braces the body in the syntax-error file |
| `ModifierOrderingCodeFixProviderBase` (RH7105, RH7106) | Observed: RH7105 writes `public static` in the syntax-error file |
| `TypeMemberOrderingCodeFixProviderBase` (RH7102, RH7103, RH7109) | Text edit |
| RH5204, RH7004, RH7104, RH7107, RH7108, RH8309 | Text edits |
| RH8305, RH8307 | Their entry point `FormatDocumentationElementInDocumentAsync` checks no syntax error (FMT-PIPE-032 is not in FMT-PIPE-005's list). Both observed fixed in the syntax-error file |
| RH7201–RH7207 | FMT-PIPE-030 |
| RH5105, RH5106, RH7101 | Not reported (SHARED-JOIN-002) |

**Corrected affected list.** These are the providers that reach `FormatNodeInDocumentAsync` or `FormatNodeInDocumentWithContextAsync` (`rg "ReihitsuFormatter\.Format"` plus the Core helpers that call them):

- `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` (RH5412–RH5416);
- `ExpressionBodyToBlockCodeFixProviderBase` (RH3205–RH3207), plus RH3203 and RH3208 through `ConvertBodyAsync`;
- RH3202 and RH5417 through `FormatMemberOnLinesOfItsOwnAsync`;
- RH3004, RH2001;
- RH5101, RH5102, RH5110, RH5111, RH5113;
- RH5205, RH5206;
- RH5301–RH5308;
- RH5401, RH5403, RH5404, RH5408;
- RH5112 and RH5201 through `FluentChainCodeFixHelper`.

The edit-then-format members are RH3004 and RH2001. RH3208 and RH5111 should be checked by the test implementer; they use the context entry point.

**Item 2 is resolved by SHARED-BRACE-003 and is covered by tests already.** I ran the issue's `if (a)` / `#if DEBUG` input:

| Rule | Build | Result |
|---|---|---|
| RH5405 | no symbols | reported on `M(b, a);`, 0 actions |
| RH5405 | `DEBUG` | reported on `M(a, b);`, 0 actions |
| RH5406 | — | not reported |
| RH5407 | `else` / `#if DEBUG` variant | reported, 0 actions |

`StatementBracesCodeFixProviderBase` never calls the formatter, so FMT-STRUCT-003 is not involved. Tests exist: `NoFixIsOfferedForDisabledTextBetweenTheIfAndItsBody` and three siblings in the RH5405 tests, and the pragma tests in RH5406/RH5407. The spec author's claims are confirmed.

### Mechanism and candidates

Mechanism: a provider registers its action without asking whether the node-level entry point will decline the document, which it declines for any error-level syntax diagnostic (FMT-PIPE-005). The candidate set is the corrected list above. It comes from the entry-point call graph, not from the issue.

### Breaking inputs

FC-009, FC-015, FC-032 (the issue's list was gathered before #1686/#1688 changed the fixes).

For the test implementer:
- one syntax-error test per base or helper and per direct provider in the corrected list, tagged SHARED-FIX-011;
- the two edit-then-format members, RH3004 and RH2001, assert no action rather than unformatted output;
- a control showing that a text-edit fix (RH6023) stays offered in a syntax-error file. Not tagged SHARED-FIX-011; it pins that the requirement does not reach it.

### Spec diff (e36792d) — challenges

1. **The SHARED-FIX-011 example is wrong on this tree.** It describes RH6023's fix as one that "lays out the spacing around the operator through the formatter" and as not offered. Observed: the fix is offered and writes `a = 1;`, because it is a text edit since #1686. The sentence after the example names the RH7105 fix as one that "edits first and then formats". The modifier-ordering base calls no formatter, and RH7105 is observed fixed in the syntax-error file. Both must be replaced with fixes that do format. Observed candidates:
   - format-only: RH5412 (`class C⏎{⏎}` plus `void N(`), or RH5102, or RH5308;
   - edit-then-format: RH3004 (`Action a = delegate () { M(); };` plus `void N(`).

   Until this is corrected, the example contradicts the code and a test tagged with it would fail.
2. **FMT-PIPE-005's new Why sentence** says "This is the one reason the node-level entry points decline a code fix's input as a whole". The precheck also declines a node-level input as a whole (FMT-UNSUP-001), and SHARED-FIX-010 withholds the fix for it. Recommend appending "apart from a shape refused as unsupported (FMT-UNSUP-001, SHARED-FIX-010)". This departs from the maintainer's verbatim text, but only in wording. It is internal.
3. **SHARED-FIX-011's first sentence** reads "MUST NOT be offered where that entry point returns its input unchanged (FMT-PIPE-005)". Read without the parenthesis, it covers every unchanged return, which is the blanket format-and-compare the issue's *Not the fix* rejects. The third sentence also calls the other unchanged returns defects, not inputs to withhold. Recommend "returns its input unchanged because the syntax tree holds an error (FMT-PIPE-005)". This is internal wording, but it prevents two readings with different offering behavior.
4. **SHARED-FIX-011's third sentence conflicts with RH5401-004.** RH5401-004 withholds the fix "when the property, laid out by the formatter, would still span several lines", including "when a comment or a preprocessor directive keeps the `=>` or the expression on a later line". It decides this by formatting at registration (`CanCollapse` through `FormatNode`) and names no declining formatter requirement. Observed after `scripts/format.sh`: `int A => // c⏎    1;` is still reported by RH5401, with no fix. So RH5401 reports a shape the formatter keeps (SHARED-DIAG-010), and SHARED-FIX-011 now calls that case a defect. It is outside the run's issues, so it is a follow-up candidate (below). The spec checker or auditor may otherwise flag RH5401-004 against SHARED-FIX-011.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Registration of the providers in the corrected list | must change |
| `ReihitsuFormatter` | must change, or stay: see *Internal choices* |
| Text-edit fixes (operator spacing, braces, modifier and member ordering, accessors, RH8305/RH8307, usings) | must stay |
| Analyzers | must stay: the diagnostic is still reported |
| Rule pages | not involved |
| Specs | SHARED-FIX-011 example and the two wording points above |

### Refuse option

The requested change is itself the refusal. Declaring "a file with a syntax error" unsupported is not an option: it is not a cheap syntactic shape, FMT-PIPE-005 already returns the input unchanged, and declaring would delete nothing. FMT-STRUCT-003 and FMT-STRUCT-023, the declines that SHARED-FIX-011 names, are unsupported-shape options outside the run's issues; see *Whole run*.

### Output questions

None. The withdrawn fixes in syntax-error files are the maintainer's decision of 2026-11-06.

### Internal choices

- **How a fix learns that the entry point declines.**
  - Checking `SyntaxTree.GetDiagnostics()` in the fixes would be a second statement of FMT-PIPE-005's condition (`ReihitsuFormatterHelpers.HasSyntaxErrors`, internal), which is FC-015.
  - Recommend that the fixes ask `ReihitsuFormatter`, in the pattern of `FindUnsupportedSyntax`/`SplitsFieldDeclaration` (FMT-PIPE-031, FMT-UNSUP-007). That needs a FMT-PIPE requirement for the query, which is a spec-author amendment.
  - Neither option changes visible output.
- **The issue's "one test per affected base class and provider".** Recommend one per base or helper, plus one per direct provider in the corrected list (about 30). Rules sharing a base share the test.

### Issue goals

- **Drop the "Not affected"/"Affected" lists as written.** They name RH6023/RH6024, RH5405–RH5407, the modifier- and member-ordering bases, RH5204, RH7004, RH7104, RH7107, RH7108, RH8305, RH8307, and RH8309, none of which is affected today. Use the corrected list. No visible change beyond the decided one.
- **Drop "RH5405–RH5407 withhold the fix for a directive body, with a test".** It is already done (SHARED-BRACE-003 and the existing tests). No visible change.
- **Drop "the RH5405 spec must record the gap".** It is moot: SHARED-BRACE-003 records it.

### Architecture findings

None, if the fixes ask the formatter. If the fixes check syntax errors themselves, the check becomes a second owner of FMT-PIPE-005's condition.

### Follow-up candidates

- **RH5401 · Expression-style get-only properties should be single-lined — comment behind `=>`.** RH5401 reports the property the formatter keeps (SHARED-DIAG-010), and RH5401-004 withholds by formatting and comparing, which SHARED-FIX-011 now calls a defect.

  Input:
  ```csharp
  class C
  {
      int A => // c
          1;
  }
  ```

  Before (`main`): reported on the property, no fix offered. The formatter writes `1;` in column 4.

  After A (recommended): not reported.

  After B: reported, fix withheld, with RH5401-004 naming a formatter requirement.

  Why A: SHARED-DIAG-010 forbids a layout rule to report a shape the formatter keeps.

## thoenissen/Reihitsu#1274

### Premise

Real, and reachable through the IDE fix and Fix All. On the issue's input, `apply-fix` gives `no-progress (2 iteration(s))` under LF and CRLF. The first application writes `1` in column 8 and `:` in column 20, the diagnostic stays on `?`, and the second application changes nothing. The CLI gives the same layout, and it is stable on a second pass.

### Mechanism and candidates

Mechanism: RH5308 reports an operator that `TernaryLineBreakRewriter` refuses to move because a comment or directive sits between the operator and its branch. Its move joins the operator with the branch (`WouldJoinAcrossUnjoinableTrivia`, lines 141 and 183; FMT-BREAK-001). The fix formats, so it cannot clear the diagnostic.

Dispatch: `TernaryLineBreakRewriter.NormalizeQuestionTokenPosition`/`NormalizeColonTokenPosition`.

Members, all run with `apply-fix`:

| Input | Result |
|---|---|
| `a ? // c` (issue) | fails |
| `a ? /* c */` ending the line | fails |
| `? 1 : // c` (the `:` member) | fails |
| `return a ? // c …` (another statement) | fails |
| `?` followed by an `#if DEBUG … #else … #endif` branch | fails |
| `a // c⏎? 1⏎: 2` (comment in front of `?`) | fixed in 1 |
| `a ? /* c */ 1⏎: 2` (comment not ending the line) | fixed in 1 |
| `? 1 // c⏎: 2` | not reported |

### Breaking inputs

FC-007, FC-013, FC-019, FC-021. The test implementer covers the five failing members above, and the two fixed members as controls.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5308 analyzer | must change under C |
| `TernaryLineBreakRewriter` / FMT-BREAK-018 | must change under A; a spec exception under C |
| RH5308 fix | must stay: it formats |
| Rule page RH5308 | not involved, unless C needs an "is not reported" line; `general-notes.md` covers comments blocking a report |

### Refuse option

Withholding the fix, the spec author's B, is not available any more. SHARED-FIX-011's third sentence, added by this run on the maintainer's text, calls a formatter-delegating fix that changes nothing a formatter or diagnostic defect "not an input to withhold". SHARED-DIAG-010 also forbids a layout rule to report "a gap that the formatter does not join because a comment or a directive sits in it". **Recommend that the lead drop option B from the output question**, or mark it as contradicting both.

**Declaring the shape unsupported is an option (escalation 10).** The span: from a conditional expression's `?` or `:` through the first token of the branch behind it, holding a comment, a directive, or disabled text.
- It would delete the two `WouldJoinAcrossUnjoinableTrivia` refusals in `TernaryLineBreakRewriter`, and under C the RH5308 exemption.
- It would refuse every file that comments a branch behind `?` or `:`, which people write.

Recommend not.

### Output questions

The spec author's question 2 stands with two amendments:
- (1) Drop B, as above.
- (2) Show the `:` member too, since option C exempts it as well. Its input is
  ```csharp
  var x = a
              ? 1 : // c
              2;
  ```
  Before: the fix is offered and the diagnostic stays after it.

I agree with recommending C.

### Internal choices

None.

### Issue goals

"No fix leaves its own diagnostic standing" is met by A or C. Neither changes the goal.

### Architecture findings

None. The analyzer and the formatter disagree (FC-013), which P11 and SHARED-DIAG-010 cover.

### Follow-up candidates

The spec author's candidate (true branch in column 8) is one member of a wider formatter class, observed with `scripts/format.sh`: a continuation line whose previous line ends with a `//` comment is laid out in the enclosing statement's or member's column, not at a continuation indent.

| Input | Output |
|---|---|
| `var x = a ? // c` / `1` | `1` in column 8 |
| `var y = b + // c` / `1;` | `1;` in column 8 |
| `int A => // c` / `1;` | `1;` in column 4 |
| `M(a, // c` / `b);` | aligned correctly |

## thoenissen/Reihitsu#1143

### Premise

Partly resolved on this tree, and partly real. One document-scope Fix All, observed with the probe (BatchFixer on the original document):

| Instance | Result |
|---|---|
| RH6024 `a+b+c`, `a==b&&b>0` | resolved: one pass, 0 left |
| RH6023 `d[k=1]=2` | resolved |
| RH5506 `[A, B] [D, E]` | resolved. Gives `[A]⏎[B] [D]⏎[E]` and 0 RH5506 left. The `[B] [D]` line is RH5505's concern, a non-goal of SHARED-FIX-001 |
| RH5204 nested mis-indentation | 0 left in one pass |
| RH6016 `s ? . Length` | fails: `s?. Length`, 1 left |
| RH8204 nested placeholders | fails: 1 left |
| RH5110 outer method plus local function | fails: the local `where` lands in column 16, 1 left |
| RH3001 `M(!(!x))` | fails: `M((!x) == false)`, 1 left |
| RH3102 two consecutive `;` | fails: 1 left |
| RH7102 two constants behind one field | fails: 1 left |
| RH5104 two comments | guard-dependent, not a defect: the second comment's fix is not offered on the starting code (0 actions) |
| RH5405 `if (a) M(); else M();` | guard-dependent: the `else` body has 0 actions on the starting code |

### Mechanism and candidates

Mechanism: the BatchFixer computes every diagnostic's fix against the original document and keeps one of two text changes that overlap or share an insertion point. So any rule whose fixes for two diagnostics of one document write overlapping spans needs another application.

Candidates: the 94 providers that return `WellKnownFixAllProviders.BatchFixer`. The overlapping shapes are:
- nested nodes: RH3001, RH5110, RH8204, RH3208, RH7310, RH5202, RH7004;
- adjacent tokens: RH6016, RH3102;
- a shared anchor: RH7102, RH7103, RH7109.

An existing helper, `DocumentOrderFixAllProvider` (Core, already used by RH3004, RH3005, RH3101, RH3105, RH3202, RH5417, and the empty-parentheses base), applies fixes one after another and re-registers each, as SHARED-FIX-002 states.

### Breaking inputs

FC-008, FC-017, FC-031 (the RH5104 test comment gives an overlap reason, but the probe shows a guard).

Today's test settings (`rg "NumberOfFixAll\w*Iterations\s*=\s*-?[2-9]"`):

- **Positive 2, to be settled one by one:**
  - RH3001 (two tests), RH3002 (`OuterParenthesesAroundInvokedReceiverChainAreReportedAndFixed`), RH3102;
  - RH5104 (two tests; guard);
  - RH5110;
  - RH5202 (two tests, also In-Document/In-Project);
  - RH5405 (two tests; guard);
  - RH7004, RH7102, RH7103, RH7109, RH7310.
- **Negative 2 ("at most 2"):**
  - The attribute-shape tests RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, and RH5531, whose comment says "whether the batch fixer needs one pass or two … is not what this scenario asserts". That evades SHARED-FIX-002. The probe shows RH5506 needs one pass.
  - RH3208.
  - Legitimate, and not violations:
    - RH4106, RH4107, RH4109: a withheld fix keeps the harness running for a pass that changes nothing.
    - RH5028, RH8402: document scope fixes one document per iteration.

The test implementer sets each positive-2 and attribute-shape test to one pass. A failure is a reproduction; a guard-dependent pass stays, with a comment naming the guard.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Fix All providers of the failing rules | must change |
| Single fixes | must stay |
| Tests listed above | must change |
| Analyzers, formatter, rule pages | not involved |

### Refuse option

Refusing Fix All for a rule (returning no provider) would remove Fix All, which is visible, and is not cheaper than switching to the existing `DocumentOrderFixAllProvider`. Declaring a shape unsupported is not an option: no formatter shape is involved.

### Output questions

None. SHARED-FIX-002 decides it.

### Internal choices

None.

### Issue goals

- "Each input above is corrected by one Fix All application": the RH6024, RH6023, RH5506, and RH5204 inputs are already corrected. Keep them as regression tests and do not count them as defects.
- "No test class sets `NumberOfFixAllIterations = 2` except a guard-dependent case with a comment": extend it to the `-2` settings, and exempt the harness-only cases (casing no-op pass, document-scope multi-document).
- The "Blocked by #1138, #1140–#1142" ordering is moot on this tree.

None of this changes visible output.

### Architecture findings

None new. Two Fix All strategies coexist (BatchFixer and `DocumentOrderFixAllProvider`). That is a choice per rule, not two owners of one decision.

### Follow-up candidates

None.

## thoenissen/Reihitsu#1673

### Premise

One of four instances reproduces.

| Fix | Result |
|---|---|
| `ModifierOrderingCodeFixProviderBase` | **No input.** Registration already refuses every comment or directive among the modifiers and in front of the next token (`ReorderingWouldDropCommentOrDirective`). That covers every gap in which `ModifierOrderingUtilities.TryWriteModifiersInOrder` can fail. `TryGetWrittenIndices` cannot fail, because `OrderModifiersForRh7105`/`OrderModifiersForRh7106` permute the list. `Count == 0` is unreachable, because the analyzers report a modifier. Drop. |
| RH7107 | Already guarded: registration calls `TryCreateMove` (since #1688, c2866ea). The issue's `TryCreateUpdatedMemberDeclaration` no longer exists. Drop. |
| RH7108 | Same as RH7107. Drop. |
| RH5308 | **Reproduces, and reachable.** `GetFormatTarget` returns null for a conditional with no statement or member ancestor, which means an `[assembly:]` or `[module:]` attribute argument. Registration registers explicitly when the target is null. See the probe result below. |

The RH5308 input:

```csharp
[assembly: System.CLSCompliant(true ?
    true :
    false)]
```

Probe result: reported on `?`, 1 action, and the action returns the document unchanged.

### Mechanism and candidates

Mechanism: RH5308's registration offers the action when it has no format target, so the action has nothing to format. The candidates are the attribute targets with no member or statement ancestor: `assembly:` and `module:`.

### Breaking inputs

FC-009. The test implementer covers `[assembly:]` and `[module:]` conditionals spanning lines, with `VerifyNoCodeFixIsOffered`.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5308 registration | must change |
| Modifier-ordering base, RH7107, RH7108 | must stay |
| Analyzer, formatter, rule page | not involved |

### Refuse option

Withholding is the requested change; it is rare and costs a line. Formatting the attribute list as a target would be a new path for a rare input, which *Special cases do not grow the source* rules out. Declaring a shape unsupported is not an option.

### Output questions

None. SHARED-FIX-005 decides it.

### Internal choices

None.

### Issue goals

- "For each affected fix, registration runs the same build or find step as the action": it applies to RH5308 only. Drop the three others; the issue itself allows dropping a fix that no input reaches. No visible change.

### Architecture findings

None.

### Follow-up candidates

None.

## thoenissen/Reihitsu#1678

### Premise

Real, and reachable from the IDE with project symbols. Observed without `NET`:

| Input | Result |
|---|---|
| The issue's input | RH7109 reports `_c` and the fix moves it in front of `_a` |
| The same type with the `[StructLayout]` part active | fix withheld (0 actions) |
| Without any layout part | fix offered |
| The issue's second shape: a group in `C`'s body that closes it and opens an attributed part (`#if X` / `}` / `[StructLayout(…)] public partial class C {` / `#endif`) | fix offered as well |

SHARED-ORDER-008 lists the second shape as part of the decided gap ("in a group with a directive in a type's body"). The spec author's account of SHARED-ORDER-008 is correct.

### Mechanism and candidates

Mechanism: the layout test reads the type's declarations from the analyzed build's symbol, and its leading-trivia test, so a `partial` declaration that only excluded code writes is not seen. Members:

| Excluded part | Status |
|---|---|
| Behind the type, same document | fails |
| In a group in the type's body that closes and reopens it, same document | fails |
| Inside another declaration's body | gap per SHARED-ORDER-008 |
| In another document | gap per SHARED-ORDER-008 |
| In front of the reordered declaration | refused today |

Consumers: RH7103, RH7109, and the RH7309 region-block fix (`OrderingDeclarationUtilities`).

### Breaking inputs

FC-024, FC-020. The test implementer covers the issue's two same-document shapes for each consumer under whichever option the maintainer picks.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Member-reordering fixes (RH7103, RH7109, RH7309) | must change under B or C |
| SHARED-ORDER-008 | must change under any option (B/C), or stays (A) |
| Analyzers, formatter, rule pages | not involved |

### Refuse option

The issue's B is the full refusal and withholds fixes on common code (every partial type with fields).

A cheaper partial refusal exists and the issue does not name it. **Option C:** withhold the reorder of instance fields of a `partial` type when the document of the reordered declaration holds disabled text. It is a cheap syntactic check of the document's trivia that reads no excluded code (SHARED-FIX-009 allows it, since it can only withhold). It closes both same-document shapes. The other-document and other-declaration cases stay a recorded gap. It costs a few lines in `OrderingDeclarationUtilities`, and it gives away fixes only on partial types in files with an excluded `#if` branch, which is uncommon.

Declaring a shape unsupported is not an option: these fixes do not format.

### Output questions

The issue's question stands. Add option C:

After, C (input as in the issue, without `NET`): no fix offered; the diagnostic stays. On the same type in a file without any excluded `#if` branch, the fix moves `_c` in front of `_a` as today, and a layout part that only another document's excluded code writes stays a recorded gap.

My recommendation is **C**. It fixes the issue's own inputs, which the maintainer triaged critical, with a check that withholds on uncommon code only. What would change the pick to A: if the maintainer considers a partial guard that leaves the other-document case open not worth a special case.

### Internal choices

None.

### Issue goals

None to drop. The issue's "Done when" needs a C branch: SHARED-ORDER-008 narrows the gap, with a test per consumer and a change doc for the visible change.

### Architecture findings

None.

### Follow-up candidates

None.

## Whole run

- **Escalation 9 candidate (#1600).** Guarding about 105 registration sites, or adding a common base type, exceeds the "about 100 production lines or a new type" bound. It needs the lead's decision before the implementer.
- **Unsupported-shape options named (escalation 10, batched with the output questions):**
  - #1600 carrier, variant (i) or (ii); recommend not, or (ii).
  - #1600 member (b) disabled-text opener; recommend not.
  - #1274 comment or directive between `?`/`:` and its branch; recommend not.
  - Outside the run's issues, seen through SHARED-FIX-011's examples:
    - **A directive or disabled text inside a field declaration with several declarators** (FMT-STRUCT-023). Span: from the declaration's first token through its `;`. It would delete `FieldDeclarationSplitTransform.CarriesDirective`, the directive condition of `SplitsFieldDeclaration` (FMT-PIPE-031), and RH7101-005. It refuses rare code. If declared at all, it should be declared in a change of its own.
    - **A directive between an embedded statement's header and its body** (FMT-STRUCT-003). It would delete the structural phase's brace guard for it. SHARED-BRACE-003 stays, because those fixes do not format. Rare. If declared at all, in a change of its own.
- **Lane.** Every issue stays a bug fix, except that answers 1 (A or B), 1b (B), #1274 (A or C), and #1678 (B or C) each need a change doc, which widens the lane to behavior change.
- **Commands.**
  - `scripts/build.sh` passed.
  - `scripts/apply-fix.sh RH5308` passed on the #1274 class fixtures.
  - `scripts/format.sh` passed on the #1600 mirrored input, the member (b) input, and the comment-continuation inputs.
  - The probe runs were done in the scratchpad.
  - Nothing in the repository was changed except this file.

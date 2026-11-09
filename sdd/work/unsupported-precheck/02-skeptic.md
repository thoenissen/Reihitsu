Role: skeptic
Commit: 89537be
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It pointed at areas to check (the deletion list, question 5). Decisions 1–6 and the issue's contract were treated as binding, as `00-lead.md` records. Everything below was checked against the specs at `89537be`, the code at `158e8fc` (the branch has not changed any compiled file yet), and CLI runs (`scripts/format.sh`, run twice on each fixture) and `scripts/apply-fix.sh` runs on scratch fixtures, which have since been deleted.

## Premise

The feature is real and reachable. Whole-document formatting through the CLI reaches all five shapes of FMT-UNSUP-004. RH7101 reaches the field shapes through `FormatNodeInDocumentWithContextAsync`, with the type as context. Every formatting code fix whose node or context can hold a variable declaration also reaches them, including a lambda body inside an argument list. Today, `main` formats all five shapes. The spec author built the change doc's "before" blocks from the CLI, and they match what I observed.

What the first case actually lets go, checked against the code:

- **`FieldDeclarationSplitTransform`**: `GetComments`, `BuildTrailingTrivia`, `BuildLeadingTrivia`, and the `nextFieldHasLeadingComment` argument can all be deleted. Comments in the trivia of a separating `,` and of a later declarator's name are refused, and these methods exist only to carry those comments. `_context` is used only by those two builders (`_context.EndOfLine`), so the field and its constructor parameter can go too. `TokenGapUtilities.HasLineBreak(SyntaxTriviaList)` has no caller besides `BuildTrailingTrivia`, in production or tests, so it becomes dead code. That is about 75–80 production lines, counting doc comments.
- **What has to stay in the split**:
  - `CarriesCommentInRepeatedPart`. Under the token list of decision 6 (option A of question 5), it still decides comments inside or behind attribute lists, between modifiers, and between a modifier and the type, which is the leading trivia of the type's first token. The issue names it as a beneficiary, but A refuses only part of what it checks. It goes away entirely only under a reading that refuses comments among a field's attributes and modifiers too, which is wider than decision 6.
  - `IsDocumented`.
  - `CarriesDirective`. The precheck covers comments only (FC-005), so directives between a declaration's tokens still reach the split.
  - Carrying the terminator's leading trivia (`WithSemicolonToken(... terminator.LeadingTrivia)`). A comment in front of the `;` is not refused under A (checked below).
- **`LineBreakAssignmentRewriter`**: nothing to delete. Its only comment guard is the shared `WouldJoinAcrossUnjoinableTrivia`. `VisitEqualsValueClause` and `NormalizeOperatorPlacement` serve property initializers, enum members, parameter defaults, and assignment expressions as much as variable declarators. A comment on its own line in front of the value is leading trivia of the value's first token, which A does not refuse. And under FMT-UNSUP-003, a node-level target below the declaration reaches the phases with a comment at `=`: RH5303, RH5304, and RH5305 pass the nearest `EqualsValueClauseSyntax` as the target of `FormatNodeInDocumentAsync`.
- **Spacing rules**: nothing to delete. `OperatorSpacingRule`, `CommaSpacingRule`, `NoSpaceSpacingRule`, and `DeclaredNameSpacingRule` have no comment special case. Comment gaps are handled by the shared next-token leading-comment guard and `NormalizeGapBehindLeadingComment`.
- **Other phases** (`rg` over `Pipeline/` for `VariableDeclarat|EqualsValueClause|Declarator|Variables`): the documentation-comment phase, the indentation contributors, the blank-line phase, and `DeclarationSemicolonLineBreakRewriter` have no special case for comments in a variable declaration. `ReihitsuFormatter.FormatSplitFieldInContext` has no comment-specific code either; its `StartsLineInLayout` documentation-comment loop concerns leading trivia, which is not refused.
- **Outside the formatter**: the RH5111 fix's `ContainsJoinBlockingTrivia` keeps its job, because it also covers directives and comments that lead the value.

So the acceptance criterion holds in letter. The PR deletes the split's comment carry-over, and it has to state that `LineBreakAssignmentRewriter`, the `=` spacing rules, and `CarriesCommentInRepeatedPart` were checked and stay, with the reasons above. Measured in lines, though, this first case shrinks the formatter far less than it grows it. It adds a phase, an exception type, a result type, catch sites, and the code-fix plumbing below, and deletes about 80 lines.

## Mechanism and candidates

_N/A_ (behavior change). The precheck's input candidates, derived from the syntax types: `VariableDeclarationSyntax` has exactly six parents — `FieldDeclarationSyntax`, `EventFieldDeclarationSyntax`, `LocalDeclarationStatementSyntax` (including `using` declarations), `ForStatementSyntax`, `UsingStatementSyntax`, and `FixedStatementSyntax`. FMT-UNSUP-004 names all six. These look like declarations but hold no `VariableDeclarationSyntax`, so they are not refused: `foreach` variables, `catch` declarations, declaration expressions (`out var x`, `var (a, b) = …`), pattern designations, property initializers, parameter defaults, and enum member values.

## Breaking inputs

Failure classes that reach the change: FC-002, FC-003, FC-005, FC-009, FC-010/FC-017, FC-013/FC-014 (see *Follow-up candidates*), FC-016, FC-027, FC-028, FC-031, FC-032, FC-033/FC-034.

### Spec defects to fix before tests

1. **The FMT-PIPE-017 example is now false** (`pipeline.md`, the `var d = /* a …` example). It says the `EqualsValueClauseSyntax` target is laid out "as document-level formatting lays them out". After this change, document-level formatting refuses that document, because `/* a` is trailing trivia of the declaration's `=`. The target still formats, per FMT-UNSUP-003. The test `ReihitsuFormatterNodeColumnTests.EqualsValueTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands` calls `AssertRuleResult(input, expected)`, a document-level comparison, so it breaks too. The spec author's handover says this example "keeps it formatted", which is true for the target only. Lines 182 and 587 of the same test file use `var x = /* a …` as well. The issue's rationale for the local decision ("whole-document and partial formatting agree") does not hold for a target below the declaration. The contract stays binding; only its stated rationale is inaccurate.
2. **RH7101-006, last sentence.** "In the documentation mode `None`, such a comment is an ordinary comment, and the fix splits the declaration" directly follows the sentence about a comment in front of a later declarator, so it reads as saying that comment is split in mode `None`. FMT-UNSUP-004 refuses every comment kind there, in every mode. The sentence should name the comment in the declaration's leading trivia.
3. **FMT-STRUCT-022 leaves out a comment in front of the last `;`.** A comment on its own line in front of the `;` is supported under A. On `main`, `private int _a, _b` / `// c` / `;` splits into `private int _a;`, then `private int _b`, a blank line, `// c`, and `;`. That output is idempotent. The requirement lists leading trivia, value comments, and trivia behind `;`, but not this case.
4. **FMT-UNSUP-004 has to say that a declaration inside a value is checked on its own.** "Not checked: anything inside an initializer value" applies to the outer declaration only. In `Func<int> f = () => { int /* c */ x = 1; return x; };`, the inner declaration is refused. A check that skips value subtrees would miss it (FC-027). Add an example.
5. **FMT-UNSUP-004 does not decide a fixed-size buffer's brackets.** In `fixed int _b[4] /* c */;`, the comment trails the `]` of the declarator's `BracketedArgumentList`, and no bullet decides it. Rare (unsafe structs), but the spec should state it either way.
6. **The location when an input holds several shapes.** FMT-UNSUP-001 says "the location of the shape". Decision 1 is singular, so the spec should say which shape: the first in text order is the natural reading. The CLI message shows it (see *Internal choices*).

### Inputs the test implementer must cover

- **The five shapes for each of the six parents**: local, field, event field, `for` initializer, `using` statement and `using` declaration, and `fixed`. Cover both line-ending styles and two passes (FC-010, FC-017).
- **The other side of each boundary (FC-003)**:
  - cases 6–9 of the issue;
  - `int b = 1 /* c */, c = 2;`, where the comment is behind the value's last token and is formatted, against `int b /* c */, c;`, where it trails the name and is refused;
  - a comment on its own line in front of the value and in front of the `;` (question 5c/5d);
  - siblings that are not variable declarations (FC-016): `int P { get; } = /* c */ 1;`, `void M(int x = /* c */ 1)`, `enum E { A = /* c */ 1 }`, `foreach (var /* c */ x in xs)`, `out var /* c */ x`, and `var (a, /* c */ b) = t;`. All of these are formatted.
- **Documentation modes.** `int _a /** x */;` is refused only in mode `None`; in `Parse`, the comment leads the `;`, per the spec author's Roslyn scan. By the same parser rule, a `/** */` comment directly in front of a `,` (`int a = 1 /** c */, b;`) should depend on the mode the same way. Verify it in both modes rather than assume it. The CLI parses with `CSharpParseOptions.Default` (`FormatCommandHandler.cs:125`), which is mode `Parse`, while a code fix uses the project's mode. So the CLI and a fix can answer differently for these rare inputs.
- **Entry points (FC-028)**: each entry point on the same refused node — `FormatNode` detached against `FormatNodeInDocumentAsync` in place; the FMT-UNSUP-002 example (M against N, and the context); an `EqualsValueClauseSyntax` target with a comment behind `=`, which is formatted (FMT-UNSUP-003); `OrganizeUsingDirectivesInDocumentAsync` on a refused document, which is not refused; a refused shape together with a syntax error, which follows FMT-PIPE-005 and is not reported as unsupported; and a refused shape with a generated-code header, which follows FMT-PIPE-006 for the document entry points.
- **Closure (FMT-UNSUP-005)**: I probed the supported neighbours on `main`, ran each output again, and found it idempotent with no refused shape produced:
  - a field split with `_r = 1 /* r */, _s`;
  - `_u = 2 // u` followed by `;` on the next line;
  - a comma at the start of a line;
  - a comment in front of the value on its own line: `int a =` / `/* a */ 1;`;
  - a comment in front of the `;` on its own line, in a local and in a field, with and without an initializer.

  The line-break phase joined none of them across the comment. A join that brought such a comment onto the `=` or name line would make it trailing trivia of a declaration token when the output is parsed again, and the second run would refuse it. Keep these as closure tests.
- **Code fixes** (FC-009, FC-032):
  - RH7101 with the shape in the target declaration, and with it elsewhere in the type, including a nested type;
  - RH2001 with `private int /* note */ Value { get; set; }` and with `… { get; set; } = /* five */ 5;`. Today `apply-fix.sh RH2001` turns both into `private int /* note */ _value;` and `private int _value = /* five */ 5;`, which are fields the formatter now refuses. The withholding check has to look at the field the fix builds, not at the property.
  - a narrow-node fix whose node holds a lambda with a refused local, for example an argument list for RH5102;
  - RH5303 on an equals-value clause with a comment behind `=`, which stays offered (locality);
  - Fix All over a document where some diagnostics are withheld (FC-008).

  Existing tests that will change and are not on the spec author's list: `CommentJoinTests.VariableInitializerIsNotCollapsedIntoTrailingComment` (`var value = // note`, a whole-pipeline input that is now refused), `ReihitsuFormatterNodeColumnTests` (item 1 above), and `FieldDeclarationSplitTests` and `ReihitsuFormatterSplitsFieldDeclarationTests` (`_first = /* c */ 1`, `_x = /* c */ 1`). RH3001 and RH3104 have `var x = /* … */` inputs, but their fixes do not call the formatter, so they are not affected.
- **CLI**:
  - write mode, `--check`, `--dry-run`, `--utf8-bom`, and `--suppress-unsupported`;
  - a directory with one unsupported and one unformatted file;
  - the non-interactive confirmation count ("This run would format N file(s)"), which must not count the refused file;
  - the trace tool (`PipelineTraceCommand` passes an observer, and `FormattingPipeline.ExecutePhase` wraps every exception in `InvalidOperationException` when an observer is present, so a phase's refusal under a trace must not turn into that wrap).
- **FC-031**: the XML summaries of `FieldDeclarationSplitTransform.Splits`, `CarriesCommentInRepeatedPart`, and `CreateSplitField` describe the first-declarator boundary and comment carry-over that are removed.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter: new precheck phase, `FormattingPipeline`, `ReihitsuFormatter` entry points | must change |
| Formatter: `FieldDeclarationSplitTransform` | must change (deletions above); `CarriesCommentInRepeatedPart`, `IsDocumented`, `CarriesDirective` stay |
| Formatter: `TokenGapUtilities.HasLineBreak(SyntaxTriviaList)` | must change (becomes dead code; delete it) |
| Formatter: `LineBreakAssignmentRewriter`, spacing rules, other phases | must stay |
| Code fixes: the 41 files that call `ReihitsuFormatter.Format*` | must change in some form under SHARED-FIX-010 (see *Internal choices*); RH7101 and RH2001 named explicitly |
| RH5111 fix guard `ContainsJoinBlockingTrivia` | must stay |
| Analyzers | not involved (no diagnostic changes) |
| `Reihitsu.Core` | not involved, beyond what the precheck reuses |
| CLI (`Program`, `FormatCommandHandler`, `ExitCodes`, README) | must change |
| CLI trace (`PipelineTraceCommand`) | involved: decide what a trace shows for a refused input (internal) |
| Playground | must change (question 6) |
| Rule pages, `general-notes.md` | not involved |
| Tests | must change (lists above) |

## Refuse option

For the feature as a whole, the refuse option is the old policy: leave constructs as written, silently. Binding decisions 1–6 already reject it. Within the change:

- **Question 5**: the narrowest reading that keeps the binding token list is A, or C, which also formats comments inside the type. Neither deletes more code than the other (see *Output questions*).
- **SHARED-FIX-010**: no narrower option complies. Withholding at action time (an action that returns the input) violates SHARED-FIX-005 and is FC-009. Applying the edit unformatted violates SHARED-FIX-007, and for RH2001 it produces a file the CLI then refuses. So a fix has to know the formatter's answer when it registers. No current entry point gives that answer, and FMT-PIPE-009 forbids calling the precheck phase directly. This is the largest cost of the change after the phase itself: a query entry point on `ReihitsuFormatter`, in the style of `SplitsFieldDeclaration` (FMT-PIPE-031), plus a call at registration in each formatting fix or in its base class, on exactly the node or context the fix formats. The lead should weigh this against escalation 9, which is about 100 production lines or a new type.
- **FMT-UNSUP-006 / `SyntaxNotSupportedException`**: binding decision 2 makes it exist, but no phase throws it yet. Its catch can only be tested through a seam: `FormattingPipeline.ExecutePhases` takes explicit phases, but the public entry points do not. The cheapest compliant option is a single boundary where the precheck's outcome is also produced, rather than a catch in every public method (see *Internal choices*).

## Output questions

**5 · Formatter — where the first unsupported shape ends (a corrected form of the spec author's question 5)**

The spec author's question 5 puts (a)–(d) in one input. Under A and B, the refusal of (a) leaves the whole input unchanged, so the formatted outputs that A claims for (b)–(d) cannot be seen. The four cases have to be asked as separate inputs. The `before` blocks below come from the CLI on `main`.

5a · comment inside the type
```csharp
class C
{
    System.Collections.Generic.Dictionary<int, // first
        string> _map;
}
```
Before (`main`):
```csharp
class C
{
    System.Collections.Generic.Dictionary<int, // first
                                          string> _map;
}
```
After, A (recommended): the input unchanged, reported as unsupported.
After, C: as before.

5b · comment on its own line between a modifier and the type
```csharp
class C
{
    private
        // c
        int _b;
}
```
Before (`main`) and after A and C:
```csharp
class C
{
    private

    // c
    int _b;
}
```
After, B: the input unchanged, reported as unsupported.

5c · comment on its own line in front of the value
```csharp
class C
{
    void M()
    {
        int c =
            // c
            1;
    }
}
```
Before (`main`) and after A and C:
```csharp
class C
{
    void M()
    {
        int c =

        // c
        1;
    }
}
```
After, B: the input unchanged, reported as unsupported.

5d · comment on its own line in front of the `;`
```csharp
class C
{
    void M()
    {
        int d
            // c
            ;
    }
}
```
Before (`main`) and after A and C:
```csharp
class C
{
    void M()
    {
        int d

        // c
        ;
    }
}
```
After, B: the input unchanged, reported as unsupported.

Why A: it is the literal reading of decision 6. It counts every token of the type, every name, `=`, and `,`, and ignores the leading trivia of the declaration's first token, the value's tokens, and the `;`. Its check is the simplest: one walk over the declaration's tokens with no carve-out inside the type.

What would change the pick: neither C nor B deletes any code that A keeps. The blank line in front of the comment in 5b–5d is FMT-BLANK-011, which applies "whatever that token is", and it stays under A and C. B refuses three more shapes only to avoid that output, and it reaches beyond the variable declaration's own tokens (5b is a field's modifier), which is wider than decision 6. Pick C instead of A for 5a if comments between generic type arguments of a declaration are common in the maintainer's code: refusing them deletes nothing.

**7 · RH2001 · Private auto-properties should not be used — a comment the field would carry between its own tokens**

Not listed in the change doc's visible output changes. SHARED-FIX-010 implies it: the fix formats the field it creates, and the formatter refuses that field.

Input:
```csharp
class C
{
    private int /* note */ Value { get; set; }
}
```
Before (`main`, `scripts/apply-fix.sh RH2001`):
```csharp
class C
{
    private int /* note */ _value;
}
```
After, A (recommended): reported, and no code fix is offered.

After, B: the fix is offered and writes the field as before, and the CLI then refuses the file the fix produced.

The same holds for `private int Value { get; set; } = /* five */ 5;`, which today becomes `private int _value = /* five */ 5;`.

Why A: a fix should not hand the user a file that the formatter refuses as a whole.

On the spec author's questions 1–4 and 6: no objection to the recommendations. Question 1 is about API, which `squad/README.md` (*What reaches the maintainer*) classifies as internal. It reaches the maintainer only because `00-lead.md` records that the maintainer asked for it. Neither option of question 1 tells a code fix the outcome at registration, which SHARED-FIX-010 needs (*Refuse option*), so the lead decides that piece in addition, whichever option the maintainer picks.

## Internal choices

- **`SplitsFieldDeclaration` and FMT-UNSUP-004** (the spec author's open choice). Recommendation: drop the new FMT-PIPE-031 clause "holds no comment that FMT-UNSUP-004 refuses". RH7101 formats in the type, the type contains the declaration, and SHARED-FIX-010 already withholds the fix, so the clause states a second time what decides that withholding (see *Architecture findings*). In the phase, `Splits` never sees a refused declaration, so the clause would be dead there. Consequence: the FMT-PIPE-031 example answer for `_g = /* c */ 1, _h` becomes true. No visible output changes.
- **How a fix learns the outcome at registration** (SHARED-FIX-010). The options are a query entry point on `ReihitsuFormatter` that runs the precheck's own predicate on a given node, or formatting at registration. Recommendation: the query. Formatting at registration means formatting a whole type for each RH7101 diagnostic in the light bulb. The query and the precheck must be one predicate, not a copy (FC-015). The node passed to it must be exactly the node, or context, passed to the formatter. For RH2001, that is the built field.
- **Where `SyntaxNotSupportedException` is caught.** The options are each public method, or the one runner every entry point uses (`FormattingPipeline.Execute`, where the precheck's outcome also comes from). Recommendation: the runner, so that the precheck and a phase's refusal produce the outcome in one place, and FMT-UNSUP-006 can be tested through `ExecutePhases` with a refusing test phase. Under a trace, the refusal must not be wrapped in `InvalidOperationException` by `ExecutePhase`.
- **Which location is reported when several shapes are present.** Recommendation: the first in text order (decision 1 says "the location", singular). The CLI message shows it. If the lead considers the choice between the first and all shapes a visible output, add it to question 2.
- **The remaining guard of FMT-STRUCT-030.** As written it holds whatever question 5 decides, and the spec author's wording is fine.

## Issue goals

The goals are binding (`00-lead.md`). Statements in the issue that the evidence contradicts, none of which changes visible output:

- "Expected beneficiary: `CarriesCommentInRepeatedPart`". Under decision 6's token list it stays (*Premise*). Only `GetComments`, `BuildTrailingTrivia`, `BuildLeadingTrivia`, and the comment carry-over go.
- "Comment handling around `=` in `LineBreakAssignmentRewriter` and in the spacing rules still has to be checked". Checked: nothing there can be deleted. The handling is shared with non-declaration constructs; comments in front of the value are not refused; and equals-value clause targets reach the phases under the local decision.
- "Local decision … That way whole-document and partial formatting agree". They agree for the same node, not between a document and a target below the declaration (*Breaking inputs*, item 1).

## Architecture findings

- **A rule stated in two places, introduced by the spec diff**: whether the RH7101 fix is withheld for a declaration that the formatter refuses is stated by SHARED-FIX-010 (through the type context) and again by the new clause of FMT-PIPE-031. If the clause is implemented in `FieldDeclarationSplitTransform.Splits`, it also puts a guard for a refused shape back into a phase, the opposite of the issue's purpose. Recommendation in *Internal choices*.

## Follow-up candidates

- **New-rule candidate (FC-014 adjacent)**: no analyzer reports the unsupported shape, which follows decision 4. An IDE user sees every layout diagnostic in a file the CLI refuses as a whole, and the formatting fixes are withheld wherever their node holds the shape, with no diagnostic that names the reason. A rule reporting the shape would have to share the formatter's predicate rather than copy it (FC-015); the analyzer assembly does not reference the formatter today. This is the maintainer's call, so it belongs in a follow-up, not in this run.
- **Existing output, outside the refusals**: FMT-BLANK-011 writes a blank line inside a declaration in front of a comment on its own line (5b–5d above). It is specified as written ("whatever that token is"), so it is no defect. It only matters to question 5.

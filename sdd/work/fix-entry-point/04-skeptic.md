Role: skeptic
Commit: 3de3469f
Verdict: output question

This round checks the amendment after the maintainer's answers (`git diff 6e272bb 3de3469f -- sdd AGENTS.md`), not the premise again. My prompt listed the requirements to challenge. It carried no intended fix, suspected cause, or hoped-for verdict.

Fixtures were run in the scratchpad and deleted afterwards. I used `scripts/format.sh` (document level, twice to check stability), `scripts/apply-fix.sh RH7101 … --max-iterations 1`, and `scripts/apply-fix.sh RH7203 …` (today's fixes, under LF and CRLF). No implementation exists yet, so the "after" outputs are derived from the spec diff.

## Premise

The amendment carries out every maintainer answer. One of them only partly; see the architecture finding.

| Answer | Where it is carried out | Holds? |
|---|---|---|
| RH7101 1a–1f A | change doc cases 1–5, 9 | yes |
| RH7101 2 A | change doc case 6 lists structural rewrites and re-layout | yes |
| RH7101 3a/3b A | FMT-PIPE-029 tab sentence, change doc case 8 | yes for a type at column 0. A type that itself stands behind a tab is not shown; see *Output questions* 1 |
| RH7101 4a A | RH7101-001 references SHARED-JOIN-002 | yes. The analyzer (`SyntaxTreeErrorVerdictCache`) and the formatter (`ReihitsuFormatterHelpers.HasSyntaxErrors`) use the same test: any error-level `SyntaxTree.GetDiagnostics()` |
| RH7101 4b A | FMT-PIPE-006 | yes. Only `DiagnosticAnalyzerBase` configures generated code (`GeneratedCodeAnalysisFlags.None`, `DiagnosticAnalyzerBase.cs:107`), so every fix runs only where Roslyn's generated-code decision let an analyzer report. The CLI and the Playground call only `FormatSyntaxTree`, which keeps the check |
| Using fixes 1A | FMT-PIPE-030, FMT-USING scope sentence | yes. The example was re-measured with today's RH7203 fix and matches |
| Architecture rule | FMT-PIPE-009, `AGENTS.md`, FMT-PIPE-031 | partly; see *Architecture findings* |

The FMT-PIPE-030 statements match today's `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`: it writes back only `WithUsings(scope, …)`, uses `DetectEndOfLine(root)` and `LanguageVersionResolver`, checks no syntax error and no header, and stops on fewer than two directives or on `CanSafelyReorder == false`. Its only callers are `UsingDirectiveOrderingCodeFixProviderBase` and `RH7207…CodeFixProvider`.

## Mechanism and candidates

_N/A_ for a bug. Here are the candidates for the new requirements.

- **FMT-PIPE-031.** The split runs only in `FieldDeclarationSplitTransform.Visit{Class,Struct,Record,Interface}Declaration`, and it refuses only `Variables.Count <= 1` and `CarriesDirective`. The predicate the spec states matches the transform exactly. `ExtensionBlockDeclarationSyntax` is the one `TypeDeclarationSyntax` subtype that is not visited, and it returns false.
- **FMT-PIPE-029.** The context entry point still returns the document unchanged when `FindCorrespondingNode` fails. The split copies the annotated first token into every field (02-skeptic), and so can `AttributeListSplitTransform` when the target's first token is the `[` of a list it splits (change doc case 5). Under FMT-PIPE-029 both are spec violations, not refusals. They are test inputs; see *Breaking inputs*.

## Breaking inputs

Failure classes: FC-007, FC-008, FC-009, FC-016, FC-020, FC-033. The class-2 and class-3 inputs of 02-skeptic still apply.

### Class A — what stands in the gap between the declaration and another member on its line (FC-020)

Axes:

- which side the gap is on: in front of the declaration or behind it;
- what the gap holds: whitespace, `/* */`, or `/** */`. A `//` or `///` comment ends the line, so it leaves no shared line;
- whose trivia it is. Roslyn puts a comment on the line into the trailing trivia of the `;` in front of it. It puts a documentation comment into the leading trivia of the token behind it, because doc comments never go into trailing trivia.

| Input line (in `class C { … }`) | Today's fix (measured) | Document level (measured, stable) | Spec text |
|---|---|---|---|
| `private int _p; /* c */ private int _x, _y;` | `_x` stays on the line; `_y` at column 0 | `_p; /* c */` / `_x;` / `_y;` | same as document level ✓ |
| `private int _x, _y; /* c */ private int _q;` | `_y; /* c */ private int _q;` | `_y; /* c */` / `_q;` | same ✓ |
| `private int _p; /** X. */ private int _x, _y;` | `_p; /** X. */ private int _x;` / `private int _y;` at column 0 | `_p;` / blank / `/** X. */ private int _x;` / `_y;` | **two readings**, see below |
| `private int _a, _b; /** Q. */ private int _q;` | `_a;` / `_b; /** Q. */ private int _q;` | `_b;` / blank / `/** Q. */ private int _q;` | **two readings**, see below |
| `void M() { } private int _x, _y;` and `private int _a, _b; void N() { }` | (02-skeptic) | one line break, no blank line | ✓ |

For a documentation comment, the comment bullet of FMT-PIPE-029 says it "stays where it stands, behind the code in front of it", and also says "as document-level formatting replaces it". Document-level formatting moves it with its member onto the next line. The two readings give different output:

- **Reading A** (the comment goes with the member it belongs to): `_p;` / `/** X. */ private int _x;`, and `_b;` / `/** Q. */ private int _q;`.
- **Reading B** (the comment stays behind the code in front): `_p; /** X. */` / `private int _x;`, and `_b; /** Q. */` / `private int _q;`.

Unchanged requirements decide this, so it is not an output question. In front, `/** X. */ ` is the target's leading trivia. FMT-PIPE-018 writes that back as it stands, so the line break goes in front of it, which gives reading A. Behind, `/** Q. */` is the leading trivia of `_q`, which SHARED-FIX-006 leaves as it was. Only the target's trailing whitespace changes, which gives reading A. Reading B would write into `_q`'s trivia, and it leaves a documentation comment behind source that the next format relocates. A also matches the maintainer's answer 1A ("follows the formatter's layout"), minus the blank line. The blank line is the next point.

**Blank line.** Document level writes a blank line above the documentation comment. FMT-PIPE-029 says "the line break and the indentation the phases lay out there". That can be read as including the phases' blank line. SHARED-FIX-006 (*Blank lines*) and SHARED-FIX-007 (*Shared line*, RH5417 example) rule a blank line out. So the result has one line break, and the wording should say so.

Test candidates: all four documentation-comment and comment rows, with a single Fix All iteration.

### Class B — Fix All with two combined declarations on one line (FC-008)

`private int _a, _b; private int _c, _d;`. With the new behind-bullet, both single fixes now write the same whitespace, the trailing trivia of `_b;`:

- the fix of `_a, _b` writes it inside its own full span;
- the fix of `_c, _d` writes it as the front-member write.

Whether `BatchFixer` merges two identical overlapping edits or drops one is not settled from the code. The scenario must keep the harness default of one Fix All iteration, without `NumberOfFixAllIterations = -2`, so that a dropped edit fails SHARED-FIX-002.

### Class C — the target's first token is duplicated or replaced by the structural phase (FC-009)

Members:

- a modifier first (`private int _x, _y;`);
- the type first (`int _x, _y;`);
- an attribute list first that the attribute split rewrites (`[A, B] private int _x, _y;`, change doc case 5);
- an attribute list first that is not rewritten (`[A] private int _x, _y;`).

Each must give the FMT-PIPE-029 output, not the unchanged document.

### Class D — the scope of FMT-PIPE-030 does not start its line

`namespace A { namespace B { using System.Linq; using System; class C { } } }`. Today's RH7203 fix, measured:

```csharp
namespace A { namespace B { using System;
using System.Linq; class C { } } }
```

SHARED-USING-010 takes the indentation of the document line the first directive starts on, which is `""` here. A scope copied out of the document (as FMT-PIPE-023 does for the node-level entry points) has `" "` as its first line, so the result would change. A characterization test with this input guards it. See *Internal choices* 3.

### Class E — tabs in front of the context's first token (FC-016)

See *Output questions* 1.

FC-033 still applies to change doc case 12. The harness must turn off the generated-code heuristic.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RH7101DoNotCombineFieldsAnalyzer` | Must change: no report in a file with a syntax error, or in an extension block (the latter pending the spec author's output question) |
| `RH7101DoNotCombineFieldsCodeFixProvider` | Must change: context entry point, `SplitsFieldDeclaration` guard |
| `ReihitsuFormatter` (FMT-PIPE-006/029/030/031) | Must change |
| `FieldDeclarationSplitTransform` | Must change (FMT-STRUCT-026); document-level output must stay |
| `UsingDirectiveCodeFixUtilities`, RH7201–RH7207 fixes | Must change to call FMT-PIPE-030; outputs must stay |
| `UsingDirectiveOrderingPhase`, `GetScopeIndentation` | Must stay |
| `RH3202…CodeFixProvider`, `ExpressionBodyToBlockCodeFixProviderBase` (RH3205–RH3208) | Depends on the decision under *Architecture findings* |
| Every node-level code fix, in a header-marked file analyzed under `generated_code = false` | Output changes (case 12, answered 4b) |
| Existing FMT-PIPE-006 node-level tests in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterEntryPointTests.cs` | Must change: their expected values flip under 4b A |
| CLI, Playground, document-level formatting | Must stay |
| `Reihitsu.ArchitectureTests` | Must change: the new FMT-PIPE-009 test |
| `Reihitsu.Core` | Not involved |
| Rule page `documentation/rules/RH7101.md` | Not involved |

## Simpler option

_None._

## Output questions

**1 · RH7101 · Do not combine fields — the declaring type itself is indented with a tab**

The maintainer's answer 3A ("later fields stand at the type's member column") was given on a type at column 0. There, counting a tab as one column and the formatter's column agree. They disagree when the type stands behind a tab. That is the usual tab file with a block-scoped namespace, or any nested type.

FMT-PIPE-029 places the later field "where the context's depth places a member", offset by the context's first-token column. `ComputeDocumentColumn(…, countsOwnWhitespaceTabsAsReplaced: false)` counts that column with a tab as one, as FMT-PIPE-017 states for every context fix. So the later field gets 1 + 4 = 5 spaces.

Input (`→` is a tab):
```csharp
namespace N
{
→class C
→{
→→private int _x, _y;
→}
}
```

Before (`main`), derived from today's copy of the first field's indentation:
```csharp
namespace N
{
→class C
→{
→→private int _x;
→→private int _y;
→}
}
```

After, A (recommended; what FMT-PIPE-017 and FMT-PIPE-029 give):
```csharp
namespace N
{
→class C
→{
→→private int _x;
     private int _y;
→}
}
```
`_y` is preceded by five spaces.

After, B: `_y` at the member column the formatter gives with the type's tab counted as four columns, which is eight spaces:
```csharp
namespace N
{
→class C
→{
→→private int _x;
        private int _y;
→}
}
```

Why A: it is the one column rule FMT-PIPE-017 already applies to every context fix (RH5111, RH3208, fluent chains). B would need the context entry point to count the context's tab differently for RH7101 alone, or a FMT-PIPE-017 change for every context fix, which is outside this run. RH5601 already reports the tabs either way. If the maintainer wants B, it should become a follow-up on FMT-PIPE-017 for all context fixes, and this run ships A.

I agree with the spec author's open question on the extension block (03-spec-author, *Output questions* 1), and with its recommendation A. RH7101-001's unchanged list of type kinds (class, struct, interface, record, record struct) already leaves extension blocks out.

## Internal choices

1. **FMT-PIPE-010 and FMT-PIPE-018 contradict FMT-PIPE-029's shared-line writes.**
   - FMT-PIPE-010 says "the one exception is the whitespace … before a documentation comment" and "keep the target's trailing trivia in every case".
   - FMT-PIPE-018 says the leading trivia is written back "in every case", the whitespace in front of the first token included.
   - FMT-PIPE-029 writes into the previous token's trailing trivia and gives the first field new indentation (front bullet). It also replaces whitespace in the target's trailing trivia (behind bullet). It announces both as departures, but the two requirements it departs from still claim to have no others.

   Recommendation: FMT-PIPE-010 and FMT-PIPE-018 each name FMT-PIPE-029's shared-line case as their second exception. Nothing visible depends on this.
2. **FMT-PIPE-029 comment bullet wording** (class A). Recommendation: state which comments stay: those in the trailing trivia of the token in front of the gap. A documentation comment, which is leading trivia of the token behind, goes with its member. The bullet should drop "as document-level formatting replaces it", say "one line break, no blank line" (SHARED-FIX-006), and say "the target's last line" for the behind case, because a multi-line target's member behind sits on its last line. These readings are already decided by FMT-PIPE-018 and SHARED-FIX-006; this is wording only.
3. **FMT-PIPE-030 should state that the phase reads the scope where it stands in the document.** FMT-PIPE-023 does not list this entry point, and SHARED-USING-010 needs the document line (class D). Recommendation: one sentence in FMT-PIPE-030, plus the class D test.
4. **SHARED-JOIN-002 used outside its spec's scope.** `line-join-rules.md` scopes itself to rules that report a token belonging on the previous line, and RH7101 is not one. Recommendation: accept it for this run, because SHARED-JOIN-002's text is general and referencing beats restating. A move to `shared/diagnostics.md` can be a follow-up when a third non-join rule needs it.

## Issue goals

No change from 02-skeptic. Answer 1A put the dropped using goals back. The amendment states them as FMT-PIPE-030 with unchanged outputs, which serves the issue's purpose.

## Architecture findings

- **Components.** `ExpressionBodiedTransformUtilities.RequiresAsyncExpressionBodyPreservation`, in `Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities`. The expression-body transforms call it as their refusal (`ExpressionBodiedMethodTransform.cs:56`, `ExpressionBodiedLocalFunctionTransform.cs:55`). The offer guards of `RH3202…CodeFixProvider.cs:66` and `ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs:93` (RH3205–RH3208) call it too.
- **Decision they share.** Whether formatting the member changes it, so that the fix is not a no-op. This is exactly the role `CarriesDirective` plays for RH7101. The maintainer's answer routes "a static applicability predicate such as `CarriesDirective`" through `ReihitsuFormatter`.
- **How the change exposes it.** FMT-PIPE-009, as written, bans only "a member of" a phase or a transform. This predicate lives in a utilities class, so the rule lets it through. An architecture test keyed on the `Reihitsu.Formatter.Pipeline` namespace would fail on it.
- **Other uses checked.** After this change it is the only `Reihitsu.Formatter.Pipeline.*` type that code fixes reference. Their other formatter uses are `ReihitsuFormatter`, `ReihitsuFormatterHelpers`, and `Reihitsu.Core` types.
- **Recommendation.**
  - Bring it into the run: one more `ReihitsuFormatter` predicate, specified like FMT-PIPE-031, and two call sites.
  - Make the architecture test's definition decidable: code fixes reference no type in `Reihitsu.Formatter.Pipeline.*`, nor `FormattingContext` or `FormattingPipeline`. `Reihitsu.Formatter.Utilities` and `Reihitsu.Core` stay callable.
  - The narrow alternative is to word FMT-PIPE-009 to admit `Pipeline/**/Utilities` and file a follow-up. That leaves the rule with a namespace exception the maintainer's answer argues against.

  Nothing visible depends on the choice: RH3202 and RH3205–RH3208 offer exactly where they do today.

## Follow-up candidates

- B of *Output questions* 1, if the maintainer wants tab-indented contexts laid out at the formatter's column: a FMT-PIPE-017 change for every context fix.
- Move SHARED-JOIN-002 to a general shared spec, if more non-join rules adopt it (*Internal choices* 4).
- The candidates already listed in `00-lead.md` stand.

Role: skeptic
Commit: 5487edf
Verdict: question for the maintainer (two questions); two architecture findings (escalation 12)

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Premise

The issue's claims are real, established by running `scripts/trace.sh` and `scripts/format.sh` on scratch fixtures (now deleted) and by reading the code at 5487edf:

- `int Get() => 42 // c` / `;` and `int Get2() =>` / `// own line` / `42;`: the StructuralTransformPhase output holds `return42` in both; the HorizontalSpacingPhase writes `return 42`. Confirmed.
- `if (a)` / `Foo(); /* x */`: phase output `{Foo(); /* x */}`. The `//` variant was confirmed by the spec author's trace.
- `RawStringLineShifts.ComputeMovedLiteral` (`src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`) rebuilds the whole literal with `SyntaxFactory.ParseExpression(..., options, true)` and `WithTriviaFrom(node)`. Every node and token of the literal is new, the start token on the unmoved opening line and holes on lines that do not move included. Only the literal's outer trivia is copied. `FormatNodeInDocumentWithContextAsync` returns `document` when `FindCorrespondingNode` finds no annotated token (`src/Reihitsu.Formatter/ReihitsuFormatter.cs`). I settled this by reading the code, not by running it.

The issue understates how far the defect reaches. The two shapes it names are not the only ones (see *Breaking inputs*), and FMT-PIPE-022 as written reaches far more code than the raw-string move (Question 2).

The spec diff has one wrong case and a few imprecise passages. A spec-author correction is enough for these; none needs the maintainer:

- **S1 — FMT-STRUCT-026, third bullet, contradicts itself.** "a line break between a token the transform creates and a comment behind it that leads the next token" requires a line break behind an inserted `}` that `// about Bar` follows, as leading trivia of `Bar();`. "none behind an inserted `}` that no directive follows" forbids it. FMT-PIPE-021 permits only the first, because `}        // about Bar` reparses with the comment trailing `}`. Input: `if (a)` / `    Foo();` / `// about Bar` / `Bar();`. The same holds for `else` with a leading comment, and for a `///` that leads a following local function. Today's code gives `}` a trailing line break, so it passes today. The requirement as written would break it.
- **S2 — FMT-PIPE-021 reparses "with the language version the formatter applies (FMT-PIPE-008)".** That must be the parse options of the tree the phase received, preprocessor symbols included. With a project symbol such as `DEBUG`, a reparse that carries only the language version swaps active code and disabled text under `#if DEBUG`. FMT-PIPE-008 also clamps `Preview` to C# 14, so a Preview tree that uses a newer feature reparses with a version error even when no phase changed anything.
- **S3 — The change doc's list of final-output changes is incomplete.** `if (a)` / `    Foo(); /* a */ // b` formats today to `Foo(); /* a */// b`, because `ControlFlowBraceTransform.NormalizeTrailingTrivia` keeps comments only. That violates the first bullet of FMT-PIPE-013 ("the space between two comments on the same line"), and FMT-STRUCT-026 makes FMT-PIPE-013 binding for the trivia the transform rebuilds. So this output changes under every answer to Question 1, and *What* should list it next to the two leading-comment inputs. The trailing-comment inputs of Question 1 option A belong in the list too, if A is chosen.
- **S4 — Phase output only, final output unaffected.** FMT-STRUCT-026 does not say whether the created `}` of an expression body may carry the replaced `;`'s trailing line break. Today it does (`ExpressionBodyToBlockConverter.CreateBlock`, `semicolonToken.TrailingTrivia`). A fixture without that line break formats to the same final output, because the blank-line phase restores the blank line between members and after a `}`. Phase-level tests need one stated answer. With S1 applied, the line break is required when a comment that leads the next token follows.

## Mechanism and candidates

_N/A_ as a single bug. This is a behavior change. The defect classes behind it:

- **Text and tree disagree (FMT-PIPE-021).** A phase builds a tree whose text the parser reads differently in one of three ways:
  - (a) A created token fuses with the next one.
  - (b) A token is written inside a single-line comment.
  - (c) A comment is placed where the parser gives it to the other neighbour: behind a line break inside a token's trailing trivia, or in a token's leading trivia with no line break since the previous token.
  - Candidates come from the structural transform list (`ControlFlowBraceTransform`, `ExpressionBodied*Transform` through `ExpressionBodyToBlockConverter`, `AccessorExpressionBodyTransform`, `GetOnlyMemberExpressionBodyTransform`, `EmptyTypeDeclarationSemicolonTransform`, `Enum`/`InitializerTrailingCommaRemovalTransform` through `TrailingCommaRemovalUtilities`, `FieldDeclarationSplitTransform`) and from every later phase that moves trivia or tokens.
- **Rebuild instead of edit (FMT-PIPE-022).** A phase creates elements from text or from scratch where it could edit the existing ones. Found by `rg` over `src/Reihitsu.Formatter/Pipeline`:
  - `SyntaxFactory.ParseExpression` in `RawStringLineShifts.cs:257`.
  - `SyntaxFactory.ParseLeadingTrivia` in `DocumentationCommentFormattingPhase.cs:306` and `:446`. These rebuild a documentation comment and every node and token inside it. This one is not named by the issue or the change doc.
  - `SyntaxFactory.Trivia(...)` in `TabReplacementPhase.cs:155` and `RegionNamingRewriter.cs:107`, for directive trivia.
  - Whitespace and end-of-line trivia created without `CopyAnnotationsTo` in `TabReplacementPhase.cs:66`, `LineEndingNormalizationPhase.cs:44`, `IndentationRewriter.cs:178`, `TrailingWhitespaceWriter.cs:21`, and the BlankLine and LineBreak utilities. `LineEndingNormalizationPhase.cs:31` is the only site that copies annotations, and only for `XmlTextNewLine`.

## Breaking inputs

Failure classes reaching the change: FC-013 is not involved, because no analyzer changes. The classes in play are the round-trip and annotation classes above, and comment preservation (FMT-PIPE-002/013).

### Class (c): the comment's owner changes on reparse

Members, by direction and component:

| Member | Today | Note |
|---|---|---|
| Brace transform: trailing `//` of the body, then inserted `}` | fails (`}` inside the comment) | named by the issue |
| Brace transform: inserted `{`, then a leading comment of the body | holds (`{` gets a line break) | FMT-STRUCT-026 keeps it |
| Brace transform: inserted `}`, then a comment that leads the next statement or `else` | holds today; **breaks under FMT-STRUCT-026 as written** | S1 |
| Expression body: created `}`, then a comment that leads the next member, if the `;`'s line break is dropped | holds today | S4 |
| **`TrailingCommaRemovalUtilities.PreserveTrailingTrivia`: the removed comma's comments appended behind a line break already in the last element's trailing trivia** | **fails today** | not named by the issue or the change doc |
| Expression body: moved comment lines behind the inserted `{` | holds (a line break is inserted first) | |

The trailing-comma member reproduces. For `enum E { A,` / `B // about B` / `    , // comma` / `}` and `new[] { 1,` / `2 /* two */` / `// before comma` / `, }`, `trace.sh` shows that pass 1 leaves ` // comma` in column 1. Pass 2, on the reparsed text, adds a blank line in front of each moved comment and reindents ` // comma` to column 4. So today's formatter is not idempotent on these inputs (FMT-PIPE-004). It belongs to the structural-transform phase but not to the shapes the change doc's *What* names. Fixing it adds a production path, which the lead should weigh against escalation 5.

### Class (a): token fusion

Created `return` in front of an identifier, keyword, or number fuses; this is named by the issue. In front of `(`, a string, `@x`, or `-1` it does not fuse. The other created tokens (`=>`, `{`, `}`, `;`) are punctuation that cannot fuse with any expression start. One check for the spacing phase: `- -x`, `+ +x`, and `- --x` keep their spaces (trace, pass 1 stable), so no fusion there.

### Removing the transform's spaces changes the final output

Question 1. The spacing phase:

- does not add a space in front of a trailing `//`: `Foo();// y` stays `Foo();// y`;
- keeps any gap there: `Foo();    // x` stays;
- collapses a run of whitespace in front of a block comment to one space but keeps a zero gap: `return    /* c */ 42` becomes `return /* c */ 42`, while `return/* c */42` becomes `return/* c */ 42`.

So the `SyntaxFactory.Space` that `NormalizeTrailingTrivia` and `CreateReturnStatement` write today is the only thing that produces today's `Foo(); // x` and `return /* c */ 42;`.

### Target inside a moving literal (FMT-PIPE-017 third case, FMT-PIPE-022)

Axes: where the target stands relative to the literal, and whether its line moves.

- A node inside a hole on a moved content line, not starting its line. This is the issue's input: today the document comes back unchanged; after the change, the third case.
- A node inside a hole that starts a moved hole line, such as the argument `2`: case one of FMT-PIPE-017 on a moved line.
- The literal expression itself, plain `"""` and `$"""`: its first token is on the unmoved opening line. Today the annotation is lost and the document comes back unchanged; after the change it is formatted and is not in the gap.
- A node inside a raw literal nested in a hole: today lost, because the outer rebuild covers it.
- The same target when the literal is already aligned: `ComputeMovedLiteral` returns `null` and the annotation survives today. Characterization.
- A target that contains the literal and starts before it, such as the declarator: works today. Characterization.

### FMT-PIPE-022 element identity

`ExpressionBodyToBlockConverter.CreateStatement` creates a new `;` for the statement and drops the member's `;`. Whether an annotation on the member's `;` must survive depends on whether that token counts as kept or as removed. Question 2 fixes the answer.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ControlFlowBraceTransform` | must change (separators; trailing `//` before `}`; comment whitespace per Question 1; S1) |
| `ExpressionBodyToBlockConverter` | must change (`return` separator; spaces around moved block comments per Question 1) |
| `AccessorExpressionBodyTransform`, `GetOnlyMemberExpressionBodyTransform` | must change (the spaces they write around `=>` and behind the accessor keyword) |
| `TrailingCommaRemovalUtilities` | must change under FMT-PIPE-021 (failing member above); not in the change doc's *What* |
| `EmptyTypeDeclarationSemicolonTransform` | must stay, unless the per-phase test shows otherwise (not probed) |
| `FieldDeclarationSplitTransform` | must stay (recorded exception, thoenissen/Reihitsu#1502) |
| `RawStringLineShifts` | must change (edit tokens; no parse options) |
| `DocumentationCommentFormattingPhase` | must change under FMT-PIPE-022 as written (Question 2, options B and C); not involved under option A |
| TabReplacement, LineEndings, Indentation, HorizontalSpacing, BlankLine, LineBreak, RegionFormatting trivia writers | must change under Question 2 option C only |
| LineBreakPhase trim (`LineBreakTriviaUtilities.MoveTokenToNewLine`), CleanupPhase | must stay (FMT-BREAK-029); see the architecture findings |
| `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` | must stay (correct once the annotation survives) |
| Analyzers, code fixes, Fix All, `Reihitsu.Core`, rule pages | not involved (no reported or fixed behavior changes; the RH7101 fix keeps running the field split alone) |
| Tests | `ControlFlowBraceTransformTests.TrailingCommentOnIfBodyIsPreserved` must change; new per-phase reparse test, FMT-STRUCT-026 phase-output tests, the FMT-PIPE-017 third-case and FMT-PIPE-022 tests, the FMT-PIPE-013 inputs (S3), and the trailing-comma member |

## Simpler option

Narrow FMT-PIPE-022 to nodes and tokens outside structured trivia (Question 2, option A). That covers the only consumer the change doc names, the context entry point's target annotation, and leaves the raw-string move as the one production change it forces. As written, the requirement forces changes to every phase that writes whitespace.

FMT-BREAK-029 is a permission that matches today's behavior and adds no test obligation beyond FMT-CLEAN-001. It is cheap and harmless as worded.

## Questions for the maintainer

### 1. Whitespace in front of and between the comments the structural transforms carry over

**Context.** FMT-STRUCT-026 forbids every space a transform writes that the parser does not need, and says other whitespace of rebuilt trivia "may be dropped". Today two code paths write such a space:

- `NormalizeTrailingTrivia` writes `SyntaxFactory.Space` in front of a body's trailing comments and drops all other whitespace between them.
- `CreateReturnStatement` writes a space on both sides of a block comment it moves behind `return`.

The spacing phase does not replace either one (probes above). Input:

```csharp
if (a)
    Foo();    // x
if (a)
    Foo();// y
if (a)
    Foo(); /* a */ // b
int Get() =>
    /* c */ 42;
```

Today's final output: `Foo(); // x`, `Foo(); // y`, `Foo(); /* a */// b`, `return /* c */ 42;`. The same comments in code that needs no wrap keep their gaps: `Foo();    // x` and `Foo();// y` stay as written.

**Question.** What does a structural transform do with the whitespace in front of and between the comments it carries over?

- **A — Keep the author's whitespace there.** The transform writes none and drops none; line breaks still follow FMT-STRUCT-026's list. Final output: `Foo();    // x`, `Foo();// y`, `Foo(); /* a */ // b`, and `return /* c */ 42;`, because the spacing phase collapses the carried indentation in front of `/* c */`. The single-space example of FMT-STRUCT-026 is unchanged. The cost: wrapped code then formats like braced code, and the final output changes for trailing comments whose gap is not one space. *What* lists those inputs, and the FMT-STRUCT-026 text replaces "may be dropped" for this whitespace.
- **B — Decide today's one space in front of a carried trailing comment, and on both sides of a comment moved behind `return`, as separators.** Final output as today, except `/* a */ // b`, which FMT-PIPE-013 requires anyway (S3). The cost: the transform writes a space no parse needs, so the spacing decision has two owners, which is what FMT-STRUCT-026's *Why* forbids. Wrapped and braced code also keep formatting this gap differently.
- **C — Drop it, as "may be dropped" allows.** Final output: `Foo();// x`, `Foo();// trailing` for FMT-STRUCT-026's own example, and `return/* c */ 42;`. This contradicts that example's "the final output is unchanged".

**Recommendation: A.** It is the only option that keeps one owner for spacing and is consistent with FMT-PIPE-013's *Why* ("a wrap ... is not a decision about the comments"). C contradicts the spec's own example. What would change the pick: if the maintainer wants no final output to change beyond the two listed inputs, B.

### 2. Scope of FMT-PIPE-022

**Context.** FMT-PIPE-022 requires every phase to keep annotations on nodes, tokens, and trivia, "also where the phase changes the element's text". Today these drop them:

- the raw-string move (named);
- the documentation phase's two `ParseLeadingTrivia` rebuilds (nodes and tokens inside doc comments);
- TabReplacement and RegionFormatting (directive trivia);
- TabReplacement, LineEndings, Indentation, HorizontalSpacing, BlankLine, and LineBreak (every whitespace and end-of-line trivia they rewrite, for example `IndentationRewriter.cs:178`, `result.Add(SyntaxFactory.Whitespace(new string(' ', layout.Column)));`).

The change doc's *What* names only the raw-string move. Keeping trivia annotations would also keep Roslyn's elastic-trivia annotation, which code fixes put on trivia they build with `SyntaxFactory`, on whitespace the formatter has already laid out. I have not verified whether a host's code-action cleanup then reformats that whitespace.

**Question.** Which elements does FMT-PIPE-022 cover?

- **A — Nodes and tokens of the code, not structured trivia and not trivia.** A token replaced by a created token of the same kind (the expression body's `;`) counts as removed. Production change: the raw-string move only. The context entry point, the only named consumer, is fully served.
- **B — Nodes and tokens everywhere, including inside documentation comments and directives.** Adds the documentation phase's two rebuilds to the run.
- **C — As written: nodes, tokens, and trivia.** Adds every trivia-writing phase listed above. That is far beyond the change doc's *What* (escalation 5) and carries the elastic-annotation risk.

**Recommendation: A.** No caller can target an element inside trivia through the entry points, and every phase that writes whitespace would otherwise change for no consumer. What would change the pick: a known caller that annotates trivia or doc-comment nodes, which would make it B or C.

## Architecture findings

Both are defects named by thoenissen/Reihitsu#1488 (*Proposed Behavior*). The change doc keeps them as they are instead of removing them. Under `squad/README.md`, *Architecture findings*, they reach the run whatever the approved change doc says, so the lead asks (escalation 12).

1. **Member-body brace placement has two owners.**
   - `LineBreakBlockRewriter.VisitBlock` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs:135`) runs `BracePlacer.EnsureBraceOnOwnLine` on every `BlockSyntax`, member bodies included.
   - `DeclarationBraceLineBreakRewriter` (`.../DeclarationBraceLineBreakRewriter.cs:142-294`) runs `BracePlacer.NormalizeOwnedBraces` on the bodies of methods, constructors, operators, conversions, destructors, and local functions, and on event accessor lists.

   The change doc says "Which rewriter places the braces of a member body is not specified behavior and is not decided here". The issue asked for one path.
2. **Removal of whitespace in front of an inserted line break has two owners.** `LineBreakTriviaUtilities.MoveTokenToNewLine` (`.../LineBreaks/Utilities/LineBreakTriviaUtilities.cs:66`) trims the previous token's trailing whitespace only when `TokenContainmentUtilities.ContainsToken(node, previousToken)`. The CleanupPhase removes it everywhere (FMT-CLEAN-001). FMT-BREAK-029 turns the duplication into a stated permission; the issue asked for the conditional trims to go. Input: `void M() {` / `if (true) {` in the spec author's trace. After `LineBreakPhase`, the space behind `if (true)` is kept and the one behind `void M()` is trimmed.

The field split's line breaks and indentation, which LineBreaks and Indentation own, are a third such case, already recorded in FMT-STRUCT-026 and tracked by thoenissen/Reihitsu#1502. It is not a new finding.

## Follow-up candidates

- `SwitchCaseBraceRewriter` (`src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:802-860`) writes line breaks around the braces it inserts, the layout FMT-STRUCT-026 now forbids the structural transforms. FMT-PIPE-013 treats both as the same kind of wrap. The switch phase runs after the LineBreakPhase, so it cannot simply drop them. A spec note in FMT-SWITCH or a follow-up on phase order would settle it.
- `int Get2() => /* d */ 42;` formats to `{/* d */` / `return 42;`: the comment that stood in front of the expression ends up glued behind `{`, off its statement's line. This may be a pre-existing FMT-PIPE-013 case; this run does not change it.

Role: skeptic
Commit: 294e9c7
Verdict: proceed narrower

My prompt carried no intended fix or suspected cause. It named one risk the spec author had not checked: node-level formatting of delimited documentation comments. That risk is settled under *Breaking inputs*.

## Premise

The issue is real. Today two components decide whether a documentation comment written behind code starts a new line, each with its own predicate:

- `DocumentationCommentFormattingPhase.RegisterRelocation` decides for `///`. It uses `IsWrittenBehindSource` / `IsWrittenBehindRootPrecedingToken`, then `DocumentsFollowingCode`, then `HasOwningLeadingSourceOnSameLine` for the FMT-DOC-002 split.
- `BlankLineTriviaBoundaryRewriter.VisitToken` / `EnsureBlankLineBeforeFirstComment` decides for `/** */`. It inserts two line breaks when the first comment of the leading trivia does not start its line. Its conditions are `FindFirstCommentIndex`, `IsFirstInBlock` (preceding `{` or switch-label colon), `IsStrandedInlineDocumentation` / `DocumentsFollowingCode`, `IsPrecededByDirective`, and `StartsOwnLine`.

The phase-order hazard the issue describes is hypothetical on today's order: documentation, usings, blank lines, line breaks. thoenissen/Reihitsu#1482 schedules it, though: D1 (thoenissen/Reihitsu#1498) moves line breaks before blank lines and is blocked by this package. The premise therefore holds as an ownership defect that a planned reorder would expose. It is not a defect a user sees today.

Reachability:
- **Whole-document formatting (CLI).** `scripts/format.sh` moves every shape listed below.
- **Code fixes through `FormatNodeInDocumentAsync`.** Checked with `scripts/apply-fix.sh RH3202` on an expression-bodied method behind `int _a; /** d */`. The fix moves the comment with a blank line, as at document level (LF and CRLF). It also covers `/* c */ /** d */` and `/** a */ /** b */`.

The spec author's statements about today's code are confirmed:
- The blank-line phase does not move every `/** */`. Not moved today:
  - `{ /** d */` at the end of the brace's line;
  - `case 2: /** d */`;
  - `/* c */ /** d */` on a line of its own;
  - `/* banner */ /** doc */` at file start;
  - the second comment of `int A; /** a */ /** b */ int B;`, which stays behind the first.
- The `///` paths in the blank-line and brace phases are moot today.

One statement in the spec diff does not hold. The spec diff (FMT-DOC-001 coverage sentence) and the change doc (*Visible output changes*, *Decisions*) say a delimited comment behind an opening brace "stays where it is". That is true only when the comment ends the brace's line. When code follows on the line, `BracePlacer.EnsureFirstContentOnNewLine` moves that code onto a new line, and the comment goes with it as the code's leading trivia. Observed with `scripts/format.sh` on 294e9c7:

```csharp
class C { /** d */ void M() { } }
```
```csharp
class C
{
    /** d */ void M()
    {
    }
}
```

The same happens with `class C`, then `{ /** d */ void M()` on the next line, and with `if (a > 0) { /** d */ a++; }`, which gives `{` / `/** d */ a++;`. FMT-BREAK-003 says a comment behind an opening brace MUST stay on the brace's line. Read literally, it contradicts this output. That discrepancy predates this run: `/* */` is trailing trivia of `{`, while Roslyn files `/** */` as leading trivia of the next token. No test pins any of these three shapes: no `{ /** x */ code` input exists in `Reihitsu.Formatter.Test`.

## Mechanism and candidates

_N/A_ as a bug: no output is wrong today. Below is the set the new owner must reproduce exactly.

Today, a delimited documentation comment is moved onto a new line (with the blank line) exactly when all of these hold:
- it is the first comment of its token's leading trivia;
- a preceding token exists, from the tree or from `FormattingContext.RootPrecedingToken`;
- that token is not `{` and not a switch-label colon;
- no directive precedes the comment in the leading trivia;
- the comment does not start its line;
- the owning token opens its node (`DocumentsFollowingCode`).

Separately, brace placement moves the first content behind `{` together with any leading `/** */`.

Dispatch types:
- `SyntaxKind.SingleLineDocumentationCommentTrivia` / `MultiLineDocumentationCommentTrivia`, which hold leading trivia only: Roslyn's lexer ends trailing trivia in front of a documentation comment.
- The owning token's parent, through `DocumentsFollowingCode`.
- The preceding token's kind and line end, through `PrecedingTokenFacts`.

Candidates by owning-token parent, all reached through the CLI:
- member declarations (field, property, method, nested type, namespace incl. file-scoped);
- enum members;
- statements (expression, block, local function, `return`);
- `else`/`catch` clauses;
- accessors;
- parameters and type parameters;
- base-list entries;
- the gap between an attribute list and its declaration;
- initializer elements;
- arguments and other expression operands;
- using, `extern alias`, and assembly-attribute lists.

## Breaking inputs

Method: for every candidate moved today, I wrote a "pre-moved" twin of the input. The twin keeps the preceding code and replaces the whitespace in front of the comment with a line break, which is the tree the documentation phase hands on once it owns the move. I formatted both through `scripts/format.sh --force`, twice.

Results:
- **54 pairs** (members, enum, statements, clauses, accessors, parameters, type parameters, base list, initializer, arguments, binary and lambda operands, every using shape incl. reorder, cross-group, multi-line comment, `#pragma`, alias, `static`, `global`, namespace-scoped usings, `extern alias`, assembly attributes, `#if`, `#if false`, `#region`, tab before the comment, multi-line comment text, `/** x */` above a `///` header, two moves on one line): every pair gives byte-identical output, and both inputs are idempotent.
- **3 differing pairs**: `s.Trim() /** d */ .ToString()`, `? 1 : /** d */ 2`, `case 2: /** d */`. These are not moved today (stranded, or behind a switch-label colon), so the twin is not a valid simulation for them. They are listed under the class matrix below.

So when the documentation phase moves exactly today's set and the blank-line phase only adds FMT-BLANK-011's blank line, no output changes. This also confirms the narrowed FMT-BLANK-012 wording ("like a comment the author wrote on its own line") for every moved shape, the using block included.

Failure classes that reach the change:
- FC-001, FC-002, FC-003: the qualifiers "first comment", "not behind `{`/colon", "not behind an earlier comment of the same leading trivia" and "no directive in front" must each become a condition, tested on both sides.
- FC-015, FC-016: one owner. The `{`/colon fact must not become a private copy next to `BlankLineEditor.IsFirstInBlock`.
- FC-020, FC-021: directives after the comment, and a `/* */` or multi-line `/* a … b */` ending on the comment's line.
- FC-027: multi-line comment text, and the enclosing scopes listed above.
- FC-028: node level against document level.
- FC-010, FC-012, FC-017: two passes, LF and CRLF. The using-block blank-line flip-flop of thoenissen/Reihitsu#1497 is unchanged (traced: pass 2 removes the blank line in the using phase and re-adds it in the blank-line phase, as on `main`).

Class matrix, "which delimited comment written behind code starts a new line":

| Axis | Members |
|---|---|
| In front on its line | preceding token · `/* c */` trailing it · multi-line `/* a … b */` trailing it, ending on the line · an earlier comment of the same leading trivia (`/** a */`, `/* c */`, file-start banner) · `{` · switch-label colon · nothing |
| Owning token | opens its node · does not (`;` `)` `}` `.` `+`) |
| Behind it on its line | line break · code · line break then directive (`#if`, `#region`, `#pragma`, disabled text) · `///` |
| Comment text | one line · several lines |
| Surface | document · `FormatNodeInDocumentAsync`, comment at the root's start (preceding token outside the root) · same, with the target on the comment's line (leading trivia kept, FMT-PIPE-010) · `FormatNodeInDocumentWithContextAsync` |
| Gap | none (`;/** d */`) · space · tab · LF · CRLF |

Settled members:

- **Moved today and identical under simulation (hold):** preceding token, or a trailing comment ending on the line, combined with an owning token that opens its node. This holds for every "behind it" member, both text shapes, and every gap. Node level: `MovesDelimitedDocumentationBehind*`, `SeparatesDelimitedDocumentation*` (existing), and the RH3202 sweep.
- **Not moved today, must stay (pinned):**
  - stranded (FMT-DOC-003);
  - `{ /** d */` ending the line (FMT-INDENT-006; `KeepsDelimitedDocumentationBehindOpeningBrace*Stable`);
  - `case 1: /** Doc */` / `default:` (`KeepsDelimitedDocumentationBehindCaseLabelStable`, `…DefaultLabelStable`);
  - file-start banner (`TrailingDocumentationCommentPreservationTests`).
- **Not moved today, must stay (not pinned):**
  - `int A; /** a */ /** b */ int B;`, where today `/** b */` stays behind `/** a */`;
  - `/* c */ /** d */` on its own line;
  - `int A; /** a */ /// b`, where today the `///` gets the FMT-DOC-002 split and `/** a */` the move.
  
  No test contains `/** a */ /** b */`. These are the members a widened `///` path breaks: `HasOwningLeadingSourceOnSameLine` would split them like FMT-DOC-002.
- **Node-level members the risk named:**
  - **N1.** A target whose leading trivia starts with `/** a */ /** b */` behind the preceding member. RH3202 today gives `int _a;` / blank / `/** a */ /** b */` / `int M()`. At node level the documentation phase decides "written behind" from the detached root's text, where `/** a */` counts as source in front of `/** b */`. The FMT-DOC-002 split is the path that would differ from today. Not pinned.
  - **N2.** `{ /** d */ int M() => 1;` with the method as target. RH3202 today keeps the comment behind `{`: `{ /** d */ int M()` / `  {` / `      return 1;` / `  }`. The `{` lies outside the root, so only `RootPrecedingToken` can tell. Not pinned at node level with code on the line.
  - **N3.** `case 1: /** Doc */` with a statement as target. Pinned.
  - **N4.** `int _a; /** d */ int M() => 1;`: the target shares the comment's line, its leading trivia is written back (FMT-PIPE-010), and the comment stays behind `int _a;` (RH3202, today). Must stay.
  - **N5.** A multi-line `/* a … b */` trailing the preceding member. Pinned.
- **Brace placement, `{ /** d */ code` on one line (document level):** moved with the code by `BracePlacer` today. Not pinned at all. See *Issue goals*.

Inputs the test implementer must add at minimum:
- `/** a */ /** b */` at document and node level (N1);
- `{ /** d */ code` at document and node level (N2);
- `/* c */ /** d */` on its own line;
- `/** a */ /// b`;
- one expression-scope and one clause or accessor shape as characterization (e.g. `N(1, /** d */ 2)`, `} /** d */ else`);
- a pipeline test with the documentation phase removed that shows the blank-line phase inserts no line break in front of a documentation comment (*Issue goals*).

Each of these runs over LF and CRLF, for two passes.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DocumentationCommentFormattingPhase` | Must change: moves delimited comments, exactly the set above, at document and node level (preceding-token facts included). It does not apply FMT-DOC-002's split or FMT-DOC-004's normalization to them. |
| `BlankLineTriviaBoundaryRewriter` | Must change: inserts no line break in front of a documentation comment. It must still give a stranded one no blank line (FMT-BLANK-012). |
| `BlankLineTokenCleanupRewriter` documentation boundary | Must stay. It is reached only by `FormatNode`, which has no preceding token, so the documentation phase moves nothing there. |
| `BracePlacer` | Must stay. It owns the first content behind `{`, and for `{ /** d */ code` it is the only component that acts. |
| `UsingDirectiveOrderingPhase` | Must stay. Outputs are identical under simulation. Its handling of a delimited comment written behind a directive is no longer reached through the pipeline once the documentation phase moves that comment first. That is not a defect (`AGENTS.md`, *Supported inputs*). |
| Node-level entry points (`ReihitsuFormatter`) | Must stay. Whitespace stripping and leading-trivia write-back already work for the `///` path. |
| `Reihitsu.Core` `DocumentationCommentUtilities` | Not involved; no change is needed. |
| Analyzers (RH8302, RH8303, RH5204, RH5114) and their fixes | Not involved: formatter output is unchanged, so no SHARED-DIAG-010 / FC-013 change. |
| Code fixes that format through `FormatNodeInDocumentAsync` (e.g. RH3202) | Not involved; output must stay (N1, N2, N4). |
| Rule pages | Not involved. |
| `AGENTS.md` ownership table | Changed by the spec author. The guard column of the blank-line row names `IsStrandedInlineDocumentation`, which must stay true after the implementation (#1482 rule 4). |
| Specs | See *Internal choices* for the amendments. |

## Simpler option

_None._ Leaving the move in two components is what the issue removes, and D1 depends on it.

## Output questions

_None._ The run changes no visible output if it drops the BracePlacer goal (*Issue goals*). If the lead keeps that goal literally, `{ /** d */ void M()` becomes an output question:

- A: `{` / `/** d */ void M()` (today);
- B: `{ /** d */` / `void M()` (what FMT-BREAK-003 and Done-when 1 ask).

I recommend A, and carrying the case into follow-up candidate 1.

## Internal choices

1. **Amend the brace statement.** FMT-DOC-001's coverage sentence and the change doc (*What*, *Visible output changes*, the first *Decisions* section) say a delimited comment behind an opening brace stays. Narrow that to a comment that ends the brace's line (FMT-INDENT-006). Add that a delimited comment with code behind it on the brace's line moves with that code by brace placement, which this change does not alter. Recommended: it is a factual claim in a spec, and it is false for the uncovered shape. The FMT-BREAK-003 discrepancy goes into follow-up candidate 1 rather than into this run.
2. **Amend FMT-DOC-001's coverage sentence (recommended).** It names statements, using directives and expressions as undecided. Today both forms also move comments in front of:
   - an `else`/`catch` clause;
   - an accessor;
   - a parameter or type parameter;
   - a base-list entry;
   - the declaration behind an attribute list;
   - an initializer element;
   - an `extern alias` or assembly attribute list.
   
   None of these is a "type or member declaration". Suggested wording: the requirement decides comments in front of a type or member declaration, enum members included; every other position whose following token opens its node is not decided yet, for either form, and a short list of examples follows. This changes no output.
3. **FMT-DOC-001 at node level when the target shares the comment's line (N4).** FMT-PIPE-010 keeps the comment where it is. FMT-DOC-001's Why already defers to FMT-PIPE-010; no amendment is needed. Recommended: leave as is.
4. **Statement case undecided for both forms (spec author's choice).** I agree: no output depends on it.

## Issue goals

- **"Brace placement no longer moves a documentation comment; it sees a comment that already stands on its own line"** and **Done when 1's "`BracePlacer` no longer insert[s] a line break in front of a documentation comment"**: drop these for `{ /** d */ code`.
  - The documentation phase does not move that comment: FMT-DOC-001 leaves a delimited comment behind `{` undecided, and moving it there would also move `{ /** d */` at the end of the line, against FMT-INDENT-006's example.
  - So `BracePlacer` stays its only owner, and no phase order changes its result.
  - Dropping keeps today's output. Keeping the goal changes visible output (option B above).
  - For `///` behind `{` the goal is already met: the documentation phase moves it first.
- **Done when 2** ("with the documentation phase removed, no documentation comment moves"): narrow it to "the blank-line phase inserts no line break in front of a documentation comment", with the `{ /** d */ code` exception above. No visible output.
- **"FMT-DOC-001 widened … including the comment in front of a block statement"**: the spec author kept the statement case undecided for both forms. Output is unchanged; I agree.
- **The using-directive question**: the comment is moved, as today. The simulation confirms identical output for every using shape, reordering included.
- **Done when 3** (the suite keeps its outputs): keep.

## Architecture findings

The issue itself is one. `DocumentationCommentFormattingPhase` (for `///`) and `BlankLineTriviaBoundaryRewriter` (for `/** */`) own one decision: whether a documentation comment written behind code starts a new line. Each uses its own predicate, listed under *Premise*. Any input that shows the predicates disagree in reach makes that visible: `int A; /** a */ /** b */ int B;` against `int A; /// b` behind `/** a */`, and `{ /** d */` against `{ /// d`.

The run removes the shared ownership. Its structural risk is that the narrow change creates another copy:
- **The "preceding `{` or switch-label colon" fact.** Today `BlankLineEditor.IsFirstInBlock` reads it from `PrecedingTokenFacts`. If the documentation phase needs it too, a private re-derivation would be a new copy (FC-015).
- **The "first comment of the leading trivia" condition.** It must not be decided twice either.

## Follow-up candidates

1. **Extends spec-author candidate 1. Formatter: delimited documentation comment behind an opening brace.** Add the shape with code behind the comment, and the FMT-BREAK-003 discrepancy.

   Input:
   ```csharp
   class C { /** d */ void M() { } }
   ```
   Before (`main`):
   ```csharp
   class C
   {
       /** d */ void M()
       {
       }
   }
   ```
   After, A (today): unchanged from Before.

   After, B (FMT-BREAK-003 read literally):
   ```csharp
   class C
   { /** d */
       void M()
       {
       }
   }
   ```
   After, C (like `///`, the spec author's option B):
   ```csharp
   class C
   {
       /** d */
       void M()
       {
       }
   }
   ```
   C also moves `{ /** d */` at the end of the line, which changes FMT-INDENT-006's example.

   Recommendation: C, because it gives one rule for both comment forms and for both brace shapes. Either way, FMT-BREAK-003 needs to say whether a documentation comment counts as a comment behind the brace.

2. **Extends spec-author candidate 2. Formatter: documentation comment that documents nothing in a non-statement position.** Today each of these gets a line break and a blank line inside the construct, for both forms:
   - `[System.Obsolete] /** d */ void M()` gives `[System.Obsolete]` / blank / `/** d */ void M()`;
   - `int P { get; /** d */ set; }` gives `get;` / blank / `/** d */ set;`;
   - `class C<T, /** d */ U>` gives `class C<T,` / blank / `/** d */ U>;`;
   - `class C : System.IDisposable, /** d */ System.ICloneable` gives the base list wrapped with a blank line;
   - `} /** d */ else` gives `}` / blank / `/** d */` / `else`, and `} /** d */ catch` gives the same with `catch`.
   
   The same readings apply (A keep, B treat as stranded and leave in place); the recommendation stays B. The issue manager should search for duplicates first.

Fixtures were written under the session scratchpad and deleted afterwards.

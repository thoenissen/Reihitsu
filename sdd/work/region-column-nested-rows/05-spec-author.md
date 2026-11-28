Role: spec author
Commit: c64be37
Verdict: spec written

I worked from `00-lead.md` (*Maintainer answers* through batch 2, *Confirmed examples*), `03-spec-author.md`, `04-skeptic.md` (*Output questions*, *Internal choices*, *Breaking inputs*), the change doc, and the spec diff at c64be37. My prompt said which batch 2 answers differ from the draft's recommendations, which internal choices to apply, and what to update. It carried no suspected cause and no hoped-for verdict.

Change doc: `sdd/changes/2026-11/region-column-nested-rows.md`, now `Status: Accepted`.
- The batch 2 answers are recorded under *Why* and under *Decisions*, one section per answer.
- The rejected options are under *Alternatives considered*.
- *Visible output changes* has new entries 14–20:
  - 14: type header;
  - 15: top-level statements;
  - 16: extension block, unchanged;
  - 17: comment-led row;
  - 18: string literal;
  - 19: `stackalloc`;
  - 20: judged on the lines the other rules leave.

  Entries 1–13 keep their numbers.
- Every new "before" is real `scripts/format.sh` output on c64be37, whose production code equals 13b0ed2. A second run formatted 0 of 8 files.
- I ran both FMT-BREAK-046 examples through `scripts/format.sh`. Today's output differs from the specified output only in the brace lines the requirement decides. `var c = new[] { (int)y };` and every other line match.

`scripts/test.sh --project architecture`: 220 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-BREAK-046 and FMT-UNSUP-023. It is expected until the test implementer tags tests with them.

## Changed requirements

- **FMT-UNSUP-023 — Region directives inside a member are unsupported — added, amended for batch 2:**
  - **Batch 2, 2B.** A top-level statement is no longer an excepted member. A new normative sentence says that a whole file with a top-level statement is refused for every active region directive in it, with no position excepted. The *Top-level statements* bullet now says that all top-level statements are one member. It also says that a node-level input is never a whole file, so a type declared in such a file is decided by its own members (FMT-UNSUP-003).
  - **The old top-level example** (regions between the statements supported) is replaced. The new example refuses at line 3, column 1, and also holds a class with regions.
  - **Batch 2, 7A.** Extension blocks are named in the member list, in the closing-brace exception, and in a new bullet: their members are supported at every depth, and a region in the `extension(…)` header is refused. There is a new example, which is today's formatted output.
  - **Batch 2, 1A.** The type header bullet is unchanged; the spec already stated it.
  - **The context entry point.** The first-token bullet now says that for `FormatNodeInDocumentWithContextAsync` the input is the context (FMT-UNSUP-002). It gives the parameter-target case (refused) and the enum-member case (supported).
  - **Why** is extended for the header, extension blocks, and top-level files.
- **FMT-BREAK-046 — A multi-line array initializer puts its braces on lines of their own — added, amended for batch 2:**
  - **Batch 2, 6A.** An initializer is judged on the lines the phase's other requirements leave inside it. Examples on both sides: `new[] { (int)⏎x }` is on one line, and `new[] { new X { A = 1 } }` spans several lines.
  - **Batch 2, 4A.** A line break inside a token counts; the spec already said so.
  - **Batch 2, 5A.** `stackalloc[] { … }` is named next to `stackalloc int[] { … }`.
  - **FMT-BREAK-016** is cited for the brace that stays behind the `=`.
  - **`.` of a member access** is added to the tokens that stay on the closing brace's line.
  - **Batch 2, 3B.** A new paragraph, "A comment keeps its line": a comment in front of an opening brace that starts its line stays where it is, and the brace moves below it. A comment behind the `{`, or behind the last element, also stays on its line. Output question 1 covers what goes beyond 3B.
  - **A second example** covers the comment-led row, the object initializer, the joined cast, `stackalloc int[]`, and `stackalloc[]`.
  - **Why** is extended for the pass-stability argument (FMT-PIPE-004) and the comment rule.
- **FMT-INDENT-010 — changed.** The node-level sentence now defers to FMT-PIPE-018: the directive is the target's leading trivia, written back as it stands, and this requirement gives it no column. The statement and label columns are gone. One sentence says that the callers of `FormatNode` pass property and indexer declarations (RH5401, RH5419), whose leading region stands in front of a member's first token. FMT-PIPE-018 is added to the `Uses` row.
- **FMT-INDENT-023 — changed (batch 2, 3B).** The comment-led paragraph covers complex element initializers only. A sentence now states that FMT-BREAK-046 moves a multi-line row's `{` below the comment, and the row is laid out from that line.
- **`AGENTS.md`, `PrecheckPhase` row.** The summary of the sixteenth shape lists every supported position: the first token of a member, a using directive, an extern alias, a file attribute list; the closing brace of a type, an extension block, an enum, a namespace; the end of the file; the input's first token. It also names the extension-block header and every region in a file with top-level statements.
- **Change doc *Special cases*, *Deleted*.** The FMT-INDENT-010 bullet defers to FMT-PIPE-018.

## Examples for the tests

- **FMT-UNSUP-023:**
  - the refused example (12,9);
  - the supported example;
  - the extension-block example;
  - the top-level example (3,1). Add one variant with only the class's regions, still refused, and one variant without a top-level statement, where the same class regions are supported.
  - Write a node-level test for a class from a top-level file, which is formatted.
  - Write two context tests: a parameter target refuses, an enum-member target does not.
  - Write one test for the `extension(…)` header, which is refused.
  - Also the FC-003 list in `04-skeptic.md`, *Breaking inputs*.
- **FMT-BREAK-046:**
  - both examples, one test per declaration;
  - each comment position of output question 1;
  - the 04 candidate list: an empty initializer, comments and directives between rows, `.Length` and `)` behind `}`, raw strings, `int[,,]`, an attribute argument, a property initializer.
- **FMT-INDENT-023:** the rows example, and the comment-led row of change-doc entry 17.
- **FMT-INDENT-010:** the namespace and nested-type example.
- **Change-doc entries 1–20:** each with a second pass and under CRLF.
- **Test literals.** A scan of test literals for regions in a file with top-level statements found only statement fragments in formatter tests. Examples are `ReihitsuFormatterTests` near lines 509–572 and `StructuralTransformPhaseIntegrationTests` near 584–595. Those fragments are already refused as regions inside a member. Analyzer test hits are mostly markup that parses as top-level code. No extra test surface beyond 03's count.

## Output questions

### 1 · Formatter · comments next to the other braces of a multi-line array initializer

Batch 2, 3B decided a comment in front of a row's `{`. FMT-BREAK-046 states the same rule, "a comment keeps its line, the brace or element on its other side moves", for every other comment next to a brace that the rule moves. The answers do not name these positions. The spec is written on A. The class is rare, so the run continues.

Input (in a method body):
```csharp
var e = new[] /* c */ { 1,
2 };
int[] a = { /* first */ 1,
2 /* last */ };
```

Before (`main`):
```csharp
        var e = new[] /* c */ { 1,
                    2 };
        int[] a = { /* first */ 1,
                      2 /* last */ };
```

After, A (recommended, the spec as written):
```csharp
        var e = new[] /* c */
                {
                    1,
                    2
                };
        int[] a = { /* first */
                      1,
                      2 /* last */
                  };
```

After, B: an initializer with a comment next to a brace the rule would move is left as written. The output is the same as before.

Why A: it is the rule the maintainer gave for a row's brace, applied to every brace. Every multi-line initializer then gets its braces on lines of their own, and every comment stays on its line.

### 2 · Formatter · how far "every region in a file with top-level statements" reaches

Batch 2, 2B, as `00-lead.md` records it: "in a file with top-level statements, every region is unsupported, also one between the statements — all top-level statements count as one member". The spec takes "every region" literally. The lead may treat this as already answered.

Input:
```csharp
#region Usings
using System;
#endregion

Console.WriteLine(1);

class D
{
    #region Fields
    private int _b;
    #endregion
}
```

Before (`main`): formatted. Each region is framed by blank lines, and the `#endregion` lines get `// Usings` and `// Fields`.

After, A (recommended, the spec as written): the input unchanged.
`error: <file>(1,1): unsupported syntax: a region directive inside a member. The file was left unchanged.`
A code fix that formats the class `D` alone still formats it, because the decision is local to its input.

After, B: only the region directives from the first top-level statement's first token through the last one's last token are refused. This input is formatted as before, because both regions stand outside the statements.

Why A: it is the answer's wording. The check reads only whether the file holds a top-level statement, and it needs no position to tell the statements apart from the usings and the types around them.

## Internal choices

- **Batch 2, 2B, for node-level inputs.** A node-level input is never a whole file. A type from a file with top-level statements is decided by its own members, so a code fix on it stays offered while the CLI refuses the file. The alternative, refusing it because of the file around it, is ruled out by FMT-UNSUP-003.
- **FMT-INDENT-010 and `FormatNode`.** The node-level sentence defers to FMT-PIPE-018, as `04-skeptic.md` proposed. For `FormatNode`, I state that its callers pass properties and indexers, whose leading region is in front of a member's first token. The alternative was to keep the statement and label columns; no caller needs them.
- **FMT-BREAK-046's comment rule** is phrased by cause: a comment keeps its line, and the brace or element on its other side moves. It covers a comment that leads the line and one that stands behind the token in front of the brace. The alternative was a list of comment positions. The visible part of this is output question 1.
- **The `.` of a member access** is added to the tokens that stay on the closing brace's line (04, *Breaking inputs*). Nothing visible changes beyond what "not an element" already said.
- **The skeptic's choices are applied as accepted:**
  - FMT-INDENT-010 defers to FMT-PIPE-018, also in *Deleted*;
  - FMT-BREAK-046 cites FMT-BREAK-016;
  - the full position list is in `AGENTS.md`;
  - FMT-UNSUP-023 says the input is the context;
  - `stackalloc[]` is named.
- **The order of FMT-BREAK-046's decision among the line-break rewriters** (FMT-PIPE-025) is still the implementer's to settle and the auditor's to check.

## Follow-up candidates

- **From `03-spec-author.md`, unchanged:**
  - F4: RH7303 does not report the regions the CLI now refuses outside executable code. With batch 2 this also covers regions between top-level statements and between the members of a type in such a file.
  - F5: no analyzer reports the FMT-BREAK-046 brace layout.
  - F3 as narrowed: the RH7304 `IsWithinElementBody` spec gap.
- No new candidates.

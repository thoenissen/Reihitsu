Role: skeptic
Commit: 70318aa
Verdict: question for the maintainer

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to challenge the parts of the spec diff added since c159029: FMT-PIPE-017; the retirement of RH5303-006 and RH5304-005; the new tail examples of RH5303-005 and RH5304-004; and the clarifications of FMT-INDENT-006, FMT-RAW-007, and FMT-RAW-005.

All outputs below were observed on 70318aa, where the node-level code is still unchanged (`ReihitsuFormatter.cs` and `ReihitsuFormatterHelpers.cs` are identical to c49431e). I used `scripts/apply-fix.sh` (LF and CRLF agree in every row) and `scripts/format.sh`, on fixtures in a scratch directory outside the repository; the fixtures are deleted. `→` stands for a tab, `/` separates lines, and columns are 0-based.

## Premise

The restructure follows the maintainer's decision 1B. The defect it targets is real, and FMT-PIPE-017 would remove it. I found one member that neither the auditor's table nor the change doc names:

- **v1.** RH5303 on `        var d = /* a` / `           b */ new List<int>` / `          {` / `            1` / `          };`, with the equals-value target.
  - Document-level formatting puts the braces at 16, under `new`.
  - The fix puts them at 22, and the run reports `no-progress`: RH5303 still reports, and a second application changes nothing (SHARED-FIX-001, SHARED-FIX-005).
  - FMT-PIPE-017 requires 16.
  - Unlike the auditor's rows, the anchor here is a value expression: no formatter requirement asks `new` to start its line, so SHARED-COMMENT-012 does not touch this shape (see C2).

Two more observations confirm the move's defects for inputs without comments:

- **n4.** RH3202 in a class indented with two spaces (`  int Get() => 1;` and `  string S() => """` / `      a` / `      """;`) gives:
  - `  int Get()` / `  {` / `      return 1;`;
  - `      return """`, with the quotes at 13 and the content and closing marker at 15.
- **h1.** RH3202 on `  string S(int x) => $"""` / `      a {` / `          x` / `      } b` / `      """;` gives:
  - `return $"""` with the quotes at 14;
  - the content line `a {` and the closing marker at 16, which did not move;
  - the hole lines `x` at 18 and `} b` at 14, which moved by the offset.

  So the literal's lines end up disagreeing with each other. Document-level formatting of the four-space version puts every line of the literal at 16 and `x` at 20.

## Mechanism and candidates

The defect: the node-level path decides columns after the last phase in three places:
- `ComputeTokenColumn` reads the document column, counting each character, a tab as one.
- `AdjustNodeIndentation` moves every line that starts with whitespace.
- The first token's leading trivia is restored: in `FormatNodeInDocumentAsync`, for a direct child of the root or a target that does not start its line (FMT-PIPE-010); in `FormatNodeInDocumentWithContextAsync`, always.

A line ends in another column than the phases gave it whenever one of these steps treats it differently from the indentation phase.

I derived the candidates from these steps and from the kinds of line `AdjustLeadingWhitespace` sees:

| Axis | Member | Today | Under FMT-PIPE-017 |
|---|---|---|---|
| A line of the target after the first | starts with whitespace (block or aligned) | moved by the offset | the phase's column — decided |
| | starts inside a multi-line comment, plus the lines aligned to an anchor on it | not moved; the aligned lines move (auditor's table, v1) | decided by the text and the examples |
| | raw-string content and closing lines | not moved (a02, n4) | decided (example 2) |
| | hole lines of an interpolated raw string | moved, while the content lines are not (h1) | follows from FMT-RAW-007; no example |
| | `///` continuation lines, `#region` lines | moved by the offset | the phase's column |
| | non-region directives, disabled text, `/* */` continuation lines, verbatim-string lines | not moved | not moved (FMT-INDENT-007/009/011) — holds |
| The target's first line | starts its line at the depth column | offset 0 | holds |
| | starts its line at another column (a misindented target, or a document indented with two spaces) | every line keeps its place relative to the first token (n4) | C3 |
| | behind code (mid-line) | anchors measured in the detached text, then moved | first-line offset — decided |
| | behind a multi-line comment's last line | offset 0 | decided |
| | a direct child of the root, whose leading trivia is kept | restored | C4 |
| What stands in front of the first token on its line | spaces | — | holds |
| | tabs in the target's own leading whitespace (a01) | counted as 1 | four columns — decided, holds |
| | tabs in whitespace outside the target | counted as 1, aligned in characters (n1) | four columns (decision Q2 A) — **C1** |
| | a tab inside a comment or a literal | counted as 1 | C2a |
| | a byte order mark | counted as 1 | C6 |
| Entry point | `FormatNodeInDocumentWithContextAsync`, where the target starts a later line of the context | the leading trivia is restored unconditionally | C5 |

## Breaking inputs

### C1 — FMT-PIPE-017's tab rule (decision Q2 A) conflicts with SHARED-FIX-001 and SHARED-FIX-005

This is the question for the maintainer.

- **Input n1:** RH5303 on `→→var d = new List<int>` / `→→  {` / `→→→1` / `→→  };` inside `class C { →void M() →{ … →} }`, with the equals-value target.
- **Today:** the fix puts the braces at 10 spaces. That is `new`'s character column, so the result converges.
- **Under FMT-PIPE-017:** `new` counts as column 16, so the braces get 16 spaces. I put that output into a fixture (`q2a`). RH5303 reports it, because the analyzer measures `new` by its `GetLineSpan` character position, which gives 10. The rule's own fix would then leave its own diagnostic, and applying it again changes nothing: a `no-progress` against SHARED-FIX-001 and SHARED-FIX-005.
- **Other rules with the same shape:** RH5201 measures with `SyntaxTokenPositionUtilities.GetColumn`, and RH5303/RH5304 (directly and through `InitializerLayoutAnalysisUtilities`) with `GetLineSpan` character positions. So any fix whose target starts behind a whitespace tab on its line, and whose rule aligns lines of the target to an anchor on that line, has the same problem: RH5303, RH5304, and RH5301 with equals-value and arrow targets.
- **One path holds:** `FluentChainCodeFixHelper`, used by RH5201 and RH5112, compares the chain's first-token column between the chain-only result and the context result, and falls back to formatting the whole statement. That replaces the statement's own tabs. I inferred this from the code and did not run it under FMT-PIPE-017. Today's n2 (`→→var x = a.Trim()` / `→→   .ToUpper();`) gives `.ToUpper` at 11 spaces, which converges.
- **Two MUSTs disagree.** SHARED-FIX-007 favors four: formatting the result replaces only the tab in front of `var`, and the braces stay at 16. SHARED-FIX-001 and SHARED-FIX-005 favor characters for as long as the tab is there.

The options, with their consequences:
- **A — keep four columns per tab (as decided).** SHARED-FIX-001 and SHARED-FIX-005 need an exception: a diagnostic that remains while a whitespace tab stands in front of the target on its line, until the RH5601 fix replaces it. Fix All and repeated application then report `no-progress` for those inputs.
- **B — count characters for what lies outside the target, as today.** SHARED-FIX-007 then fails for those inputs: formatting the result moves the braces from 10 to 16. FMT-PIPE-017 needs a second counting rule for a target that does not start its line.
- **C — withhold the fix when a whitespace tab stands in front of the target's first token on its line, outside the target.** The diagnostic stays without a fix, which is a supported end state (`AGENTS.md`, *Code-fix coverage*). There is precedent: SHARED-ATTR-009 covers only "a declaration whose line holds no tab character in front of its first token". It needs a guard in each affected fix and a recorded gap in each of their specs.
- **D — make the alignment analyzers count a whitespace tab as four columns.** It touches every alignment analyzer; that is scope (escalation 5).

The decision record for Q2 (`18-spec-author.md`) names the character-counting view, but not the SHARED-FIX-001/005 violation by the fix's own rule.

### C2 — The new tail examples assert output that SHARED-COMMENT-012 rules out

These examples are: RH5303-005's second example, RH5304-004's new example, and FMT-PIPE-017's first example.

- In each, a complex element's `{` stands behind `/* a` / `b */` and stays there.
- FMT-BREAK-021 puts every element of a collection initializer on its own line. SHARED-COMMENT-012, which is Active, says a token behind a multi-line comment's last line does not start its line, and that "where a requirement of the formatter asks that token to start its own line, the formatter MUST break the line in front of the token".
- So both requirements rule out the output these examples show. Today's formatter keeps `b */ {`. That is the open defect thoenissen/Reihitsu#1382 (P33, decided); `line-breaks.md` applies SHARED-COMMENT-012 to chains only.
- RH5303-005's own text says "every element starts its own line", which its new example contradicts in the same way.
- On the analyzer side, SHARED-INIT-001 exempts the token behind a tail. #1382 is to replace that reading with P33, after which RH5303 would report the fixed output again.
- The diff also removes SHARED-COMMENT-012 from RH5303's and RH5304's `Uses`. Nothing changes the requirement's reach.
- No current output depends on this: document-level and node-level output change together when #1382 lands. But three specs would pin as intended an output that a decided requirement replaces.
- A shape SHARED-COMMENT-012 does not reach shows the same mechanism: v1, a value expression behind the tail, with braces at 16 under `new`. With such a shape, the example is unaffected by #1382.

### C2a — "each tab in front of it on its line as four columns" is wider than FMT-TAB-001

The phases count a tab inside a comment or a literal as one column, because it is kept (FMT-TAB-003).

- **Input:** `var d /*→*/ = new List<int>` / misplaced braces, with RH5303 and the equals-value target. Here the comment is outside the target.
- **Document-level formatting** puts the braces at `new`'s character column, counting the tab in the comment as one. Counting it as four would put them 3 columns to the right, against SHARED-FIX-007, and RH5303 would report the result.
- **The same holds** for a statement target whose own leading trivia holds `/*→*/` in front of its first token.
- **Wording:** "each tab in whitespace in front of it on its line".

### C3 — Which column inner lines start from when the first token is not at the depth column

FMT-PIPE-017 says to lay out "from the column its first token has" and that the first token stays there. It also says "every line … ends in the column the indentation phase gives it", and FMT-INDENT-002 alone gives the depth column. Two readings follow:

- **X** — every line keeps its place relative to the first token. For n4 that gives `  int Get()` / `  {` / `      return 1;`. This is today's output without the raw-string defect.
- **Y** — the first token stays at 2, and the other lines take depth columns: `{` at 4, `return` at 8.

The accepted change doc lists no such output change, so only X agrees with it. The spec should say so, for example: the phases lay out the node with the depth columns, shifted by the difference between the first token's column in the document and the column the phases give it. On a first line that starts inside a comment, that difference is 0. The spec should pin it with n4, whose raw-string half then puts the content and the closing marker under the quotes at 13.

### C4 — FMT-PIPE-010 keeps leading trivia after the last phase

- **The conflict:** FMT-PIPE-010 keeps the target's leading trivia when the target is a direct child of the root or does not start its line. That restore runs after the last phase. FMT-PIPE-017's "every line of the formatted node" and "MUST NOT move a line of it after the last phase" also cover the lines of that trivia.
- **Input:** a top-level `// c` in column 0 above `  class D …`, with the type as target.
  - FMT-PIPE-010 keeps `// c` at 0.
  - FMT-INDENT-006 in the phases gives it 2.
- **Wording:** FMT-PIPE-017 should except the leading trivia FMT-PIPE-010 keeps. For a mid-line target the restore is a no-op today: the whitespace in front of it belongs to the previous token's trailing trivia.

### C5 — Lines of the context node that are not written back

`FormatNodeInDocumentWithContextAsync` writes back only the target (FMT-PIPE-010). The lines of the context node outside the target keep their document columns, so "every line of the formatted node" should read "every line written back".

The method also restores the target's original leading trivia unconditionally. For a target that starts a later line of the context, that restore is a move after the last phase. No caller reaches a failing member today:
- `FluentChainCodeFixHelper` falls back to the whole context. n5, RH5201 on `Call(1,` / `          a.Trim()` / `   .ToUpper());`, moved `a` from 10 to 13 and put `.ToUpper` at 14.
- RH5111 joins its target onto one line.
- The target of RH3208 stands behind a label.

An implementation should not keep that restore as a source of the first token's column.

### C6 — A byte order mark in front of the first token

On the first line of a text that keeps its byte order mark, the layout counts the mark as zero columns, while FMT-RAW-005 now counts it as one for the raw-string move.

The case arises with a target that is a direct child of the root on that line, such as a top-level statement, the first using directive, or a type, with string input only. "Counted as the phases count columns" then names two numbers. If the column is taken as `ComputeTokenColumn` takes it today (the mark as one character), every line of the target shifts by one against document-level output. The spec should say the mark counts as zero for the first token's column. The raw-string move inside the phases then still counts it as FMT-RAW-005 says.

### C7 — Minor

FMT-PIPE-017's *Why* says the code "has to come out as document-level formatting lays it out". That holds for columns only: FMT-PIPE-011 keeps accessor shapes, and FMT-PIPE-010 decides blank lines from the unformatted surroundings.

### For the test implementer

- **Failure classes:** FC-028 (both entry points), FC-016 and FC-001 (every member above, not only the auditor's rows), FC-022 and FC-027 (plain and interpolated raw strings with hole lines, h1), FC-013 (run the fix's own rule on the result), FC-007, FC-017 (exact output plus a second application), FC-010 (LF and CRLF).
- **Inputs:**
  - v1, n4, and h1, with the expected outputs above (h1 for a two-space document: every line of the literal at 14, `x` at 18);
  - a01 and a02;
  - n1, whose expected output depends on C1;
  - the C2a input;
  - the C4 input;
  - a statement target led by a one-line comment and misindented, such as `     /* c */ Call(1,` / `2);`: the first token stays, and `2` aligns under `1`.
- **The auditor's element-behind-tail rows** change when #1382 lands. If they are kept as tests, assert that node-level output equals document-level output rather than fixed columns, or use the C2 replacement shape.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ReihitsuFormatter.FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync` | must change (FMT-PIPE-017); the leading-trivia restore needs the C4/C5 wording |
| `ReihitsuFormatterHelpers.ComputeTokenColumn`, `AdjustNodeIndentation` | must change; the move goes |
| `IndentationPhase` (`LayoutComputer`, `LayoutModel`, `IndentationRewriter`), `FormattingContext` | must change: the first token's document column, a first-line offset for mid-line targets, and the C3 base |
| `FormatNode` | must stay. FMT-PIPE-017 does not cover it; it lays out an attached node at the depth column, which differs from the document entry points for a misindented node (FC-028, as today) |
| RH5303/RH5304 providers and `InitializerLayoutAnalysisUtilities.ContainsComplexElementBehindMultiLineCommentTail` | the guard goes (retired RH5303-006/RH5304-005); C1 may add a different guard |
| RH5201, RH5303, RH5304, RH5301 analyzers | must stay, unless C1 D is chosen |
| `FluentChainCodeFixHelper` | must stay; its fallback is what keeps RH5201/RH5112 safe under C1 |
| Rule pages | not involved; RH5303/RH5304 pages already dropped the sentence (0d873e3) |
| `shared/code-fixes.md` | must change if C1 A is chosen (exception) |

## Simpler option

For C1, option C (withhold the fix when a whitespace tab stands in front of an out-of-target first line) needs no exception in SHARED-FIX-001/005 and no analyzer change. For C2, replacing the three examples with v1's shape needs no decision on #1382.

## Questions for the maintainer

**C1** — the options A to D are above. Under the standing instruction the lead takes its own recommendation.

C2 to C7 have one reading each that agrees with the accepted change doc and the Active shared specs. They are wording for the spec author:
- C2: replace the examples.
- C2a: "tab in whitespace".
- C3: reading X, with n4 as the example.
- C4: except the leading trivia FMT-PIPE-010 keeps.
- C5: "lines written back".
- C6: the byte order mark counts as zero for the first token's column.
- C7: "columns".

## Architecture findings

_None new._

- The node-level step is the subject of the restructure the maintainer already chose (1B), and the restores in C4/C5 belong to that step.
- C1 is a disagreement between analyzers that count a tab as one character and the formatter's four columns. `README.md` (*Architecture findings*) classes such a disagreement as an ordinary finding (FC-013, SHARED-FIX-001), not as escalation 12.

## Follow-up candidates

- **SHARED-INIT-001 against SHARED-COMMENT-012** (an element behind a tail is exempt for the initializer rules, but must be reported under P33). This is pre-existing and is thoenissen/Reihitsu#1382. Once it lands, every test of this run that pins an element-behind-tail output changes.
- **If C1 A is kept:** the alignment analyzers count a whitespace tab as four columns, so that formatter and fix output on tab-indented code is analyzer-clean before RH5601 is applied.
- **`FormatNode` on an attached node lays out at the depth column.** The document entry points lay out at the document column. Pre-existing (FC-028).

Role: skeptic
Commit: 3f4eade (code identical to 3d478c6; every "main" output below was measured on this tree with `scripts/apply-fix.sh`, LF and CRLF, or `scripts/format.sh`, fixtures in a scratch directory outside the repository)
Verdict: #1416: output question; #1388: proceed narrower; #1627: output question; #1142: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`; their goals were treated as proposals. The spec author's handover carried the "Feasibility" note on how the fix is built today; it was checked as a fact, not used as a direction.

## thoenissen/Reihitsu#1416

### Premise

Real; caller: the RH5405, RH5406, RH5407 code fixes (`StatementBracesCodeFixProviderBase.ApplyCodeFixAsync` formats the whole owning `IfStatementSyntax` through `ReihitsuFormatter.FormatNodeInDocumentAsync`). Measured on this tree (RH5405 unless stated):

- `b++;⏎if ( a )⏎    M(a,b);` → a blank line is inserted above `if`, `( a )` becomes `(a)`, the body becomes `M(a, b);`.
- `if (a)⏎    M(a,b);⏎else⏎    M(a,⏎      b);` → the `else` body, which this diagnostic does not name, gets braces too.
- `else if (b)⏎    M(a, b);` → braces in column 13 (the inner `if`); `scripts/format.sh` puts them in column 8 (the `else`).
- RH5407 `} else M(a, b);` → `else` is moved onto its own line. RH5407 `} else if (b) M(a, b);⏎else⏎{…}` → braces in column 15, and the following, already braced `else` block is re-indented to column 15 as well.
- `if (a) M(a, b); /* c */ b++;` → `} b++;`.
- Tab-indented method: the tabs in front of `if (a)` are replaced by eight spaces (the header line is not code the diagnostic names).

Every item except the tab line is in the issue or its comments. SHARED-FIX-006/007 (P9, P26) and #1220 Q3 (maintainer: the fix must not re-indent the other body or move `else`) decide that all of them are wrong. The spec is not wrong instead: the old SHARED-BRACE-002 sentence ("laying out the `if` statement … as the formatter does") was the outlier, and the spec author's alignment is correct.

### Mechanism and candidates

Mechanism: the fix chooses its scope by choosing a node, and the node it passes is the whole `if` statement, so everything the formatter decides inside that statement (header spacing, the other body, the blank line in front of the statement, the body's spacing and continuation columns, `else` placement, the tabs of its first line) is written back. Membership is decidable: any input in which formatting the owning `if` statement changes a token gap outside the reported body's braces.

Dispatch: `FindFormattingNode` — the body's parent is `IfStatementSyntax` (the `if` body) or `ElseClauseSyntax` whose parent is `IfStatementSyntax` (the `else` body). An `else if` has no body of its own here. Loops are not dispatched (SHARED-BRACE-001), so the issue's "loops" are out of scope.

Candidates by what stands in front of the owning `if`/`else` on its line (this decides the brace column under the new SHARED-BRACE-002):

| In front of the owner on its line | Example | Spec coverage | Today |
|---|---|---|---|
| nothing (`if` starts its line) | `if (a)⏎    M();` | column of `if` | braces at `if` column |
| `else` (`else if`, `else` starts its line) | `else if (b)⏎    M();` | column of `else` | column of inner `if` (13) |
| `}` (`} else`, `} else if`) | `} else M();` | column of `}` | `else` moved; column of inner `if` for `} else if` |
| a statement (`b++; if`), a `case` label, a block's `{` (`{ if (a) M(); }`, statement lambda), a header (`if (b) if (a) M();`, `while (a) if (a) M();`), an `else` behind a statement (`if (a) M(); else M();`, for the `else` body) | — | non-goal | braces at the column of the `if` keyword (13, 20, 27, …) |

### Breaking inputs

Failure classes that reach the changed code: FC-004, FC-019, FC-020, FC-021 (comments and directives between header and body), FC-007, FC-008, FC-009, FC-017, FC-022, FC-026, FC-027, FC-030, FC-033, FC-034.

Defect class (the issue's): layout outside the body changes. Axes and members, all measured failing today: header spacing; blank line above the statement; the other body (braced or not); `else` placement (`} else`, `else` behind the body on one line); brace column (`if` starts its line: holds; `else if`: fails; `} else if`: fails); body spacing; continuation columns of the body; code behind the body on its line; tabs in the header line's whitespace.

Pre-existing defect found in the same fix (FC-009 / SHARED-FIX-005): a directive between the header and the body makes the fix an offered no-op, because `ControlFlowBraceTransform` refuses to brace a statement whose leading trivia holds a directive or disabled text (`src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs:103`) and the fix registers anyway. Members measured (`no-progress` in `apply-fix.sh`):

| Directive between header and body | RH5405 | RH5406 | RH5407 (else body) |
|---|---|---|---|
| `#pragma warning disable` | no-progress | no-progress | no-progress |
| `#nullable enable` | no-progress | — | — |
| `#region` / `#endregion` around the body | applied twice, no braces, blank lines inserted and `#region` re-indented | — | — |
| `#pragma` inside a multi-line body (between its lines) | — | fixed (holds) | — |
| `#pragma` behind the body (leading trivia of the next statement) | fixed (holds) | — | — |

Inputs the test implementer must cover (beyond the spec examples):

- `else if` and `} else if` chains (FC-026), and two bodies of one `if` written on one line, `if (a) M(a, b); else M(a, b);` — two identical `M(a, b);` on one line, each its own diagnostic.
- Fix All over an `if` whose both bodies are reported (FC-008: the two fixes edit around the same `else`), and over `if (b) if (a) M(a, b);` (outer and inner body).
- A body with nested braces of its own: `if (a) foreach (var x in xs) { … }` on several lines, a body that holds a statement lambda (FC-027).
- Comments: `if (a) /* h */ M(a, b);` (today `/* h */` stays behind the header — holds), `if (a) // c` with the body below, an own-line `// c` between header and body (today it moves into the block above the body), a multi-line `/* one⏎ two */` behind a body on the header line.
- A verbatim or raw string spanning lines in a body that moves (FC-022): `if (a) M(a, @"x⏎y");`.
- A blank line between header and body: `if (a)⏎⏎    M(a, b);` (today the blank line is removed; see *Output questions*, 3).
- Tabs in front of the header line or of a body line.
- Line endings (FC-017, FC-033): CRLF, and lone `\r` built explicitly — `apply-fix.sh` runs only LF and CRLF, and the existing lone-`\r` tests of RH5406 deliberately do not assert the line break behind `}` (see #1388).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `StatementBracesCodeFixProviderBase` (RH5405–RH5407 fixes) | must change |
| RH5405–RH5407 analyzers | must stay |
| Fix All (`BatchFixer`) | must stay; must be tested on two reported bodies of one `if` |
| Formatter (`ControlFlowBraceTransform`, node-level entry points) | not involved by the issue; involved only if the lead takes the architecture finding below into the run |
| `Reihitsu.Core` | not involved; existing helpers that RH7004 uses for the same contract (`SyntaxIndentationUtilities.TryShiftMovedLines`, `HoldsTabInFrontOfMovingLine`, `LineEndingUtilities.HoldsOtherLineBreak`) exist |
| Rule pages RH5405–RH5407 | must stay |
| SHARED-BRACE-002 | changed by the spec author; needs the amendments under *Refuse option* |
| Tests | must change: tests that pin the reformatted body, column 13 for `else if`, `else` moved off `}`, the other body braced |

### Refuse option

Refusing the whole class is not available: `if (a)⏎    M(a, b);` after a statement is code people commonly write, and today's fix inserts a blank line above it.

Refusing rare sub-classes, each a cheap check and each withholding a fix today's fix offers (nothing in question per `squad/README.md` for a rare class; the PR names them under `Review notes`):

1. **Owner line with other code in front** (the spec's new non-goal). A non-goal does not remove the case: the fix reaches it, and whatever the implementation writes there is untested visible output. Formatting moves the `if` onto its own line in each of these shapes (`case 1: if` measured: the formatter even braces the section), so "braces as the formatter lays them out" cannot be met without moving the header, which SHARED-FIX-006 forbids. Recommend: SHARED-BRACE-002 states the fix is withheld when anything other than an `else` or a closing brace stands in front of the owning `if`/`else` on its line. Rare: `b++; if` is reported by RH5103, `{ if (a) M(); }` by RH5403, `if (b) if (a) …` by RH5405 on the outer body; `case 1: if (a) M();` and `while (a) if (a) M();` on one line are seldom written.
2. **Directive between header and body.** Withhold (SHARED-FIX-005 already demands that the no-op is not offered). Rare.
3. **Tab in the whitespace in front of the owner's line or of a moving body line.** Withhold, as RH7004-007 does. Rare by definition (RH5601). The alternative, writing the braces behind a copy of the header line's whitespace as the split fix does under #1109 Q1, would be an output question.
4. **A string literal or comment spanning lines that would have to move** — already in the spec author's question 1, option A; subsumed by option C of question 1 below.

Declaring a shape unsupported (escalation 10), named because it is an option: **a preprocessor directive or disabled text in the leading trivia of the unbraced embedded statement of `if`, `else`, `while`, `for`, `foreach`, `using`, `lock`, `fixed`, or `do`** — from the token that ends the header (`)`, `else`, `do`) through the embedded statement's first token, excepting nothing. It would delete the directive half of the guard at `ControlFlowBraceTransform.cs:103` and the guard the brace fixes would otherwise need for item 2. It would refuse whole files that hold `if (x)⏎#if DEBUG⏎    Log();⏎#endif`-style code, which is rare but written. Recommendation: do not declare; one construct and one phase guard, so withholding the fix (item 2) is enough.

Cost comparison: the requested change (wrap only the body) cannot be done with `FormatNodeInDocumentAsync` on the `if`, as the spec author noted. A text-level wrap with the braces in a column read from the document and the body moved by a uniform shift is the contract RH7004 already has (SHARED-FIX-007 states it for RH7004), and its helpers exist in `Reihitsu.Core`. Option B of the spec author's question 1 (continuation lines in the formatter's columns) needs a layout-only formatter mode that does not exist (#1140 *Structural cause*, #1605): more than 100 lines and a new type, escalation 9.

### Output questions

The spec author's question 1, as posed, is decided by SHARED-FIX-007 *Moved code* (P26): a body below its `if` keeps its nesting, so "only the indentation follows the new nesting" leaves its lines where they are, "also where formatting the result would change them" — option A for that input, and its option B contradicts the requirement. The open case is a multi-line body that starts on the line of its `if` or `else`, where the body's first line moves left and the requirement admits two readings. It replaces question 1:

**1 · RH5406 · Braces must not be omitted from multi-line child statements — a multi-line body that starts on the line of its `if`**

Input 1:
```csharp
        if (a) M(a,
                 b);
```

Input 2:
```csharp
        if (a) x = y
            + z;
```

Before (`main`), input 1:
```csharp
        if (a)
        {
            M(a,
              b);
        }
```
input 2:
```csharp
        if (a)
        {
            x = y
                + z;
        }
```

After, A — every line moves by the shift of the body's first token (here three columns left), as RH7004 and member moves do. Input 1 as `main`. Input 2:
```csharp
        if (a)
        {
            x = y
         + z;
        }
```

After, B — every later line moves by the shift of its line's start (the `if` line's start, column 8, becomes the body's line start, column 12: four columns right). Input 2 as `main`. Input 1:
```csharp
        if (a)
        {
            M(a,
                     b);
        }
```

After, C (recommended) — no fix offered for a body that starts on the line of its `if` or `else` and spans several lines; the diagnostic stays. A body below its `if` keeps its lines in every option.

Why C: each cheap rule breaks one of the two common continuation styles, and the formatter's columns would need a new formatter mode; if multi-line bodies on the `if` line are common in your code, A is the existing rule for moved code.

**2 · RH5405 · Braces must not be omitted — code behind the body on its line**

The spec author's question 2 showed two shapes. The second, `if (a) M(a, b); else M(a, b);`, is decided by #1220 Q3 (the fix must not move `else`) and by SHARED-BRACE-002's own `} else` rule: `else` stays on its line behind the new `}` (the spec author's option B for that shape). Only the first shape is open:

Input:
```csharp
        if (a) M(a, b); /* c */ b++;
```

Before (`main`):
```csharp
        if (a)
        {
            M(a, b); /* c */
        } b++;
```

After, A (recommended):
```csharp
        if (a)
        {
            M(a, b); /* c */
        }
        b++;
```

After, B: as `main`.

Why A: the formatter breaks the line behind a closing brace the fix created, so B hands back a line the formatter changes again. Measured: RH5030 (blank line after closing brace) then reports `b++;`, and formatting the document also inserts that blank line; the fix cannot insert it without changing a blank line between the statement and its neighbour (SHARED-FIX-006).

**3 · RH5405 · Braces must not be omitted — a blank line between the `if` and its body**

Input:
```csharp
        if (a)

            M(a, b);
```

Before (`main`):
```csharp
        if (a)
        {
            M(a, b);
        }
```

After, A (recommended): as `main`.

After, B:
```csharp
        if (a)
        {

            M(a, b);
        }
```

After, C: no fix offered.

Why A: the blank line would stand next to a brace the fix creates, which the formatter lays out without one; B leaves an RH5022 diagnostic on the fix's own output.

### Internal choices

- `} else` keeps `else` behind the brace (the spec author's first internal choice): decided by #1220 Q3, not an output question.
- The four withholds under *Refuse option* items 1–3 (rare classes) — recommend all three, stated in SHARED-BRACE-002 so the tests pin them.
- Line endings of the brace fixes: SHARED-FIX-008 first paragraph (kept breaks rewritten to the detected ending, the spec author's choice) or second paragraph (withheld when the moved body holds another line break, as RH7004). Both only differ in all-lone-`\r` and mixed documents. If the body is moved as text without the formatter, the fix belongs to the second paragraph's class by its own wording ("a fix that moves code … without laying it out through the formatter"), and the existing helper makes it a cheap check; keeping it in the first paragraph then needs its own rewriting of kept breaks. Recommend deciding by mechanism, after the implementer's approach is known, but before tests for lone-`\r` are written; RH5406 under the second paragraph is always withheld in an all-lone-`\r` document (its body spans lines).

### Issue goals

- "the change doc is written" — dropped by the maintainer's triage (bug-fix lane, clarification only). No visible change.
- "RH5406 and RH5407: … (`else`, loops)" — loops dropped: SHARED-BRACE-001 checks only `if`/`else` bodies. No visible change.
- "each fix keeps the wrapped statement's layout with a regression test per rule" — keep, narrowed by question 1 for bodies on the header line.

### Architecture findings

The known structural cause (#1140 *Structural cause*: a fix chooses its scope only by choosing a node, and a narrower layout needs either a column computed in the fix or a new entry point) applies. The rewritten SHARED-BRACE-002 avoids a prediction: its brace column is read from the document (the first token of the owner's line), not the indentation phase's depth, and its body shift is the RH7004 contract. A prediction would appear only if the brace column were computed as the formatter's column (the reverted ~300-line attempt), or under option B of the spec author's question 1. Not a new finding.

See also the finding under #1627, which shares this fix.

### Follow-up candidates

_None for this issue alone._

## thoenissen/Reihitsu#1388

### Premise

Instance by instance:

| Instance | Caller | Result on this tree |
|---|---|---|
| RH5405, CRLF, `if (a) M(a, b); /* one⏎ two */` | RH5405 fix | **does not reproduce**: no line-ending drift (CRLF arm clean). Under question 1 option A or C of #1416 this input becomes "no fix offered" (a comment spanning lines would have to move) |
| `ReihitsuFormatter.FormatNode` via RH5401, CRLF | none | **no caller reaches it.** The only `FormatNode` call in shipped code is `RH5401…CodeFixProvider.CanCollapse`, which tests the result for `'\n'`; the document comes from `FormatNodeInDocumentAsync`. Every line ending `FormatNode` could pick contains `\n`, so the choice cannot change the predicate. RH5401 fix measured on `int Q =>⏎    1;` and `Func<int> P =>⏎    () => 1;`: no drift |
| RH5011, all-lone-`\r` | RH5011 fix | real by code: `RH5011…CodeFixProvider.cs:72` copies the `break` line's own line break, so a lone `\r` is written; also in a mixed document whose `break` line ends in the minority ending |
| RH8304, all-lone-`\r` | RH8304 fix | real by code: `DocumentationCommentCodeFixUtilities.GetLineBreak` (`:193`) copies the line's own break; RH8304 is its only caller; same mixed-document member |
| RH5406 (and RH5405 with the body below), all-lone-`\r`, break behind `}` | brace fixes | real by code: `ReihitsuFormatter.WriteBackTrailingTrivia` (`ReihitsuFormatter.cs:526–545`) writes the target's original trailing trivia, its `\r` included, behind the result after the phases normalized it. The existing lone-`\r` tests of RH5406 and RH5417 assert only the breaks the fix creates and read every other break as LF, so no test covers that break |
| RH7004, CRLF, comment or directive in front of the next statement | RH7004 fix | **does not reproduce**: `#pragma`, `#nullable enable`, own-line `// note` — no drift, and no blank line inserted for `#nullable`. Changed by #1688 (`c2866ea`); the `#pragma` case is covered by an existing CRLF test (`RH7004…AnalyzerTests.cs:1524`) |
| RH7309, lone-`\r` or mixed document | RH7309 fix | real by code: the fix swaps the text of whole region blocks (`TextChange` per block), so every kept break inside a moved block keeps its ending |

The lone-`\r` and mixed-document instances were not run: `apply-fix.sh` runs only LF and CRLF. They follow from the cited lines; the test implementer reproduces them.

### Mechanism and candidates

Mechanism: a fix writes a line break it takes from the document — a neighbouring line's break, the text of the code it moves, or the target's written-back trailing trivia — instead of the line ending FMT-PIPE-007 detects. Membership is decidable: a line break in the fix's span whose text comes from the source rather than from `DetectEndOfLine`.

Candidates from the code fixes that create or keep line breaks (`rg` over `EndIncludingLineBreak`, `SyntaxFactory.EndOfLine`, hard-coded breaks, and `FormatNodeInDocument*` callers):

| Source of the break | Members | In the run |
|---|---|---|
| copies a neighbouring line's break | RH5011 (`:72`), RH8304 (`GetLineBreak`), RH8402 (`GetPreferredLineEnding`: the first line's break, `\n` hard-coded for a file of one line) | RH5011, RH8304; RH8402 is a follow-up candidate |
| keeps the breaks of moved text | RH7309 (not withheld); member/accessor moves, RH7004, RH3004 (withheld under SHARED-FIX-008, second paragraph) | RH7309 |
| node-level entry point writes back the target's trailing trivia | every caller of `FormatNodeInDocumentAsync`/`WithContextAsync` (29 files under `Reihitsu.Analyzer.CodeFixes`) whose target's last token ends its line | brace fixes per the issue; the issue's comment says the rule "applies to every formatter-delegating fix" |
| `DetectEndOfLine` | split/placement attribute fixes, `BlankLineCodeFixUtilities`, RH5604, RH7004 | holds |

### Breaking inputs

Classes: FC-017 (lone-`\r` tests that read every break as LF hide the failing break), FC-033 (`apply-fix.sh` has no lone-`\r` arm), FC-016 (siblings).

Matrix (document kind × source): all-LF and all-CRLF hold for every member measured; all-lone-`\r` fails for every row except `DetectEndOfLine`; a mixed document whose copied break is the minority ending fails for the copying rows and RH7309.

Inputs: RH5011 and RH8304 in an all-lone-`\r` document and in a CRLF document whose edited line ends in LF; RH5406 `if (a) M(a,␍         b);` asserting the break behind `}`; RH5405 with the body below `if (a)`; RH7309 in an all-lone-`\r` document; characterization tests for the RH5405 CRLF comment instance (if the fix is still offered for it after question 1) and RH7004 `#nullable enable` / own-line comment.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5011 fix | must change |
| `DocumentationCommentCodeFixUtilities.GetLineBreak` (RH8304) | must change |
| RH7309 fix | must change (normalize or withhold, see *Internal choices*) |
| Brace fixes | via #1416 |
| `ReihitsuFormatter` node-level write-back of trailing trivia | conflict to resolve (below); must change only if the lead takes the class into the run |
| `ReihitsuFormatter.FormatNode`, FMT-PIPE-007 | not involved (instance dropped) |
| RH7004, RH5401 | not involved (hold) |
| RH8402 | not involved (follow-up candidate) |
| Rule pages | must stay |

### Refuse option

All-lone-`\r` and mixed documents are rare. Refuse option: withhold every affected fix in a document whose line breaks are not all one of LF or CRLF, as SHARED-FIX-008's second paragraph does for moves. For RH5011 and RH8304 that costs more than the requested change: replacing a copied break with `DetectEndOfLine` is a one-call change per site, so refusing there is not simpler. For RH7309 withholding is the cheaper option and the existing helper (`LineEndingUtilities.HoldsOtherLineBreak`) does it. No unsupported shape is an option: line endings are normalized by the preparation phase, so no phase guard would be deleted.

### Output questions

_None._ SHARED-FIX-008 (unchanged, "its trailing trivia included", maintainer decision P35 / #1415) demands the detected ending for every row.

### Internal choices

- **RH7309: normalize the kept breaks or withhold.** The spec author kept RH7309 under SHARED-FIX-008's first paragraph (normalize), as the issue's comment expects. RH7309 moves text without the formatter, exactly the class the second paragraph withholds for (member moves, RH7004, RH3004), and its *Why* applies unchanged. Recommend: add RH7309 to the second paragraph and withhold — one rule for every fix that moves text without the formatter, a cheap check that exists. The outputs differ only in all-lone-`\r` and mixed documents (rare class).
- **FMT-PIPE-010 against SHARED-FIX-008.** FMT-PIPE-010 says both node-level entry points "MUST keep the target's trailing trivia"; SHARED-FIX-008 says every line break in the fix's span, "its trailing trivia included", gets the detected ending. In an all-lone-`\r` document they demand different text behind every formatter-delegating fix's target. The spec author's diff does not touch FMT-PIPE-010. Recommend aligning FMT-PIPE-010 to SHARED-FIX-008 (the line break in the written-back trailing trivia takes the detected ending), since SHARED-FIX-008 carries the maintainer's later decision; either the class is fixed in this run at its one cause (see *Architecture findings* under #1627), or it is filed as a follow-up and the run fixes only the brace fixes.

### Issue goals

- "`FormatNode` … expected: CRLF; detection uses the document the node comes from (its root)" — drop: no caller reaches it (`AGENTS.md`, *Supported inputs*); FMT-PIPE-007 stays. No visible change.
- "Both inputs keep CRLF and have regression tests" — the RH5405 input already keeps CRLF on `main`: characterization test, or none if question 1 withholds the fix for it. The `FormatNode` input is dropped.
- "#1283 … check whether the `FormatNode` fix also covers it" — drop with the instance.
- RH7004 added instance — already holds (#1688) and is covered for `#pragma`; no change needed.
- RH7309 "expected → they get the detected line ending" — proposal; see *Internal choices*.
- "RH5011-004 and RH8304-004 cite SHARED-FIX-008" — done by the spec author.

### Architecture findings

See #1627: the lone-`\r` break behind `}` has the same cause as the whitespace behind `}`.

### Follow-up candidates

- RH8402 copies the line break of the file's first line into the header it inserts (`RH8402…CodeFixProvider.cs:98–112`), so an all-lone-`\r` document gets a lone `\r` and a mixed document can get the minority ending; a file of one line gets a hard-coded `\n` where FMT-PIPE-007 falls back to `Environment.NewLine`. Derived from code, not run. Against SHARED-FIX-008.

## thoenissen/Reihitsu#1627

### Premise

Real; callers: the RH5405, RH5406, RH5407 fixes. Measured, LF and CRLF alike: RH5405 `    M(a, b); /* x */···` below `if (a)` → `}···`; RH5406 (body on two lines, `/* x */···` behind it) → `}···`; RH5407 (`else` body) → `}···`.

The issue is narrower than the defect: the comment is not needed. RH5405 `M(a, b);···` below `if (a)` → `}···`, and `if (a) M(a, b);···` → `}···`. With `// x···` the whitespace is part of the comment's text and stays behind the comment (holds).

### Mechanism and candidates

Mechanism: `FormatNodeInDocumentAsync` writes the target's original trailing trivia back behind the result's last token after the last phase (FMT-PIPE-010, `ReihitsuFormatter.WriteBackTrailingTrivia`), so when formatting appends tokens behind the target's original last token, the whitespace that ended the original last line lands behind the new last token, where the cleanup phase never sees it.

Candidates: every formatter-delegating fix whose formatting appends tokens behind the target's original last token: brace wrapping (FMT-STRUCT-001: RH5405–RH5407), expression body to block (RH3202 measured: `public int GetValue() => _value;···` → `}···`; RH3203 and other expression-body fixes not run).

### Breaking inputs

Classes: FC-016 (sibling fixes keep the hazard), FC-030.

Matrix for the brace fixes: rule (RH5405, RH5406, RH5407) × body position (below the owner, on the owner's line) × what precedes the whitespace (`;`, a delimited comment, a single-line comment) × LF/CRLF. Fails: `;` and delimited comment, in every rule and position measured. Holds: single-line comment.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Brace fixes | must change (through #1416) |
| `ReihitsuFormatter` write-back | not involved if the brace fixes stop delegating the `if`; otherwise the cause |
| RH3202 and other expression-body fixes | not involved in the issue (follow-up candidate, or in the run with the architecture finding) |

### Refuse option

Withholding the fix for a body line that ends in whitespace: cheap and rare (trailing whitespace is reported by the layout rules), but it withholds a fix only to avoid three spaces; the requested change costs no more once #1416 wraps the body without formatting the `if`. Not recommended. No unsupported shape is an option.

### Output questions

The spec author's question 3 stands as asked (whitespace stays at the end of the body's line, A, recommended; or removed, B). Facts to add to it:

- The same output applies with no comment: `M(a, b);···` keeps its whitespace under A.
- A matches RH7004 on this tree: `s.Flush(); /* x */···` moved into the new block keeps `···`, and the new `}` gets none (measured).
- The issue's "Required output" is B; the issue is `agent-filed`, so B is a proposal.

### Internal choices

_None._

### Issue goals

- "No trailing whitespace behind the inserted `}`" — keep. "Required output" (whitespace removed from the body line too) — replaced by the answer to question 3; visible, so it stays a question.
- "covered by a code-fix test per rule (LF and CRLF)" — keep, and add the no-comment and body-on-the-owner-line members.

### Architecture findings

**The end of the target's last line has two owners.** Components: `ReihitsuFormatter.WriteBackTrailingTrivia` (FMT-PIPE-010, after the last phase) and the cleanup phase (trailing whitespace) and the preparation phase (line endings). Decision they share: what stands at the end of the line the target ends on — its trailing whitespace and its line break. The write-back runs after the phases and puts back what they removed or normalized. Inputs that show it:

- `if (a)⏎    M(a, b);···` with RH5405 → `}···` (cleanup phase's decision undone);
- `public int GetValue() => _value;···` with RH3202 → `}···`;
- RH5406 `if (a) M(a,␍         b);` in an all-lone-`\r` document → the break behind `}` stays `␍` while the phases wrote `\n` (preparation phase's decision undone; by code, not run).

It reaches the run through #1627 and #1388. The rewritten #1416 may remove the brace fixes from the class without touching the cause; the expression-body fixes and the lone-`\r` row for all 29 formatter-delegating fixes would keep it (FC-016).

### Follow-up candidates

- If the run does not change the write-back: formatter-delegating fixes that append tokens behind their target (RH3202 measured; RH3203 and the other expression-body fixes to be measured) leave the target's trailing whitespace behind the new `}`, and every formatter-delegating fix keeps a non-detected line break behind its target in an all-lone-`\r` or mixed document. Against SHARED-FIX-007 (formatting the result changes nothing in the changed code) and SHARED-FIX-008.

## thoenissen/Reihitsu#1142

### Premise

Real; callers: the split fixes RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, RH5531 (`TargetAttributeListShapeCodeFixProviderBase.ApplySplitCodeFixAsync`; RH5524 and RH5529 are merge rules; RH5531 splits only on accessors that are not single-line). RH5514 measured on this tree, the #1109 Q1 table still holds:

- class braces in column 0, list in column 4 → `[A]`, `[B]`, `void M()` move to column 8;
- `#pragma` and `// c` in column 0 above, `// d` in column 8 below → both comments re-indented to column 4;
- `    int _f;···` and `    [A, B]···` → whitespace removed;
- `→[A, B]` / `→void M()` → rewritten with four spaces, the method braces keep their tabs;
- `[A] [B, D2] void M()` → four lines; `[return: A] [A, B] int R() => 0;` → `[return: A]` broken off; `/* c */ [A, B]` → comment separated;
- CRLF: 8 of 9 shapes drift (`Line endings: CRLF=8, LF=1` and similar); the local function in a method block holds (no drift, `[B] void L() { }` already as reading A); lambda `System.Action a = [A, B] () => { };` already gives `[B] () => { };` behind the line's whitespace (reading A), with drift.

The provider replaces only the list; the changes outside it come from the code action's post-processing, which formats around the bracket tokens the provider creates with `SyntaxFactory.AttributeList` (`:166–181`), under Roslyn's rules and the host's new-line option.

### Mechanism and candidates

Mechanism: the split fix hands the code action new tokens it created, and the code action's post-processing re-lays out the lines around them with Roslyn's formatter instead of leaving them as written. Membership: any split input where Roslyn's layout of the lines around the new lists differs from the text as written.

Candidates: the 14 split providers above, through one base method. Siblings in the same base: the placement fix (RH5513 `    [A] void M()` behind a line ending in whitespace) and the merge fix (RH5524 `void M([A] [B] int x)` inside column-0 braces) — both hold, LF and CRLF (measured).

### Breaking inputs

Classes: FC-020 (directives above or below the list), FC-021 (comment in front of the list on its line), FC-030, FC-017 (assert every break in CRLF, not a count), FC-008 (Fix All over two reported lists of one declaration, e.g. `[A, B]⏎[C, D]` — the base registers one action per list).

Inputs: every row of the #1109 Q1 table, each in CRLF; a list in a `case` section and in a method block (local function); a lambda list; `int Q { [return: A, B] get => 1; }` (RH5528); `class O { [A, B] class C { } }` (RH5506) — `[B]` then starts in the column of the line's whitespace, column 0 or 4, as reading A says; an all-lone-`\r` document (the inserted break is `\n`).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `TargetAttributeListShapeCodeFixProviderBase` split path | must change |
| Merge path, placement base | must stay (measured holding) |
| Analyzers RH5502–RH5531 | must stay |
| Rule pages | must stay |
| SHARED-ATTR-017 and the 14 rule requirements | changed by the spec author; the coverage sentence needs the tab case once question 4 is settled (below) |

### Refuse option

Withholding the split fix wherever the post-processing would change other lines is not cheap (it would have to predict Roslyn's formatter) and would cover common code (CRLF on every shape). Not an option. No unsupported shape is an option.

### Output questions

The spec author's question 4 (tab-indented list) is already answered: #1109 Q1, reading A, as recorded in the maintainer's decision comment, states "each attribute after the first starts a new line at the indentation of the reported list's line; nothing around the list moves", and the table it decided includes the row `\t[A, B]` / `\tvoid M()`. Reading A for that row is the spec author's option A (`→[A]` / `→[B]` / `→void M()`), and #1142's own instance list says the same. Recommend not asking it; SHARED-ATTR-017's coverage sentence ("a list whose line has only spaces in front of its first character") then drops the restriction.

_No other output question._

### Internal choices

_None._

### Issue goals

- "The Fix All test classes of these rules (`NumberOfFixAllIterations = 2`) are re-checked in the Fix All issue" — outside this run, as the issue says.
- All other goals keep: they restate #1109 Q1, which the maintainer decided.

### Architecture findings

_None._ The fix and Roslyn's post-processing both decide the lines around the list, but Roslyn's formatter is not a component of this repository; the defect is the fix leaving that decision to it.

### Follow-up candidates

_None._

## Whole run

### Commands

- `scripts/apply-fix.sh` for RH5405 (31 fixtures), RH5406 (9), RH5407 (4), RH5401 (4), RH5514 (10), RH5513 (2), RH5524 (1), RH7004 (5), RH5030 (1), RH3202 (2); every fixture under LF and CRLF.
- `scripts/format.sh` on a copy of the RH5405 fixtures, for the formatter's brace column and blank lines.
- No build and no test run. Fixtures were in the session scratch directory, outside the repository.

### Output questions for the batch

#1416: 1 (replaces the spec author's 1), 2 (narrows the spec author's 2), 3 (new). #1627: the spec author's 3, with the facts added above. #1142: the spec author's 4 is answered by #1109 Q1. Escalation 10: the directive-before-an-unbraced-body shape, recommended not to declare.

### Follow-up candidates

- RH8402 line ending (see #1388).
- Formatter-delegating fixes: trailing whitespace and line break behind the target (see #1627), if the run does not change the write-back.

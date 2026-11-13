Role: skeptic
Commit: 81b2360
Verdict: #969: proceed (pending Q1); #1335: proceed (pending Q1, Q2, Q5); #1263: proceed (pending Q4); #1151: proceed (pending Q3); #1604: proceed narrower (drop the helper and RH3004 goals; pending Q1, Q2, Q5)

How the statements below were established: every issue and its comments read through the GitHub MCP server; `00-lead.md`, `01-spec-author.md` and the spec diff of 57f49a2 read; the deciding code read (`TypeMemberOrderingCodeFixProviderBase`, `ModifierOrderingCodeFixProviderBase`, the RH7004, RH7104, RH7107, RH7108, RH7302, RH7303 fix providers, `OrderingMoveSafety.MoveNodeBeforePreservingSeparators`, `RegionDirectiveUtilities.IsWithinElementBody`, the RH3004 registration). Every "today" below was produced on 81b2360 with `scripts/apply-fix.sh <RH####> <dir>` (LF and CRLF arms agree in every case quoted) or `scripts/format.sh <dir>`, on fixtures in the scratchpad, deleted afterwards. No compiler or Roslyn result is claimed without such a run.

Ignored input: #1604 prescribes a shared "move lines by a column delta" helper; it is treated below as a proposal, not as a requirement.

The maintainer clarification (withholding a fix is an equally valid outcome; a fuller implementation can follow later as a follow-up when proportionate) is applied in every *Refuse option* section.

## thoenissen/Reihitsu#969

### Premise

Real, and reached by the code fixes of RH7102, RH7103 and RH7109 (caller: the code fix through `TypeMemberOrderingCodeFixProviderBase.ApplyCodeFixAsync`, which passes the whole annotated `TypeDeclarationSyntax` to `ReihitsuFormatter.FormatNodeInDocumentAsync`, `TypeMemberOrderingCodeFixProviderBase.cs:105-108`). Measured on the issue's own input (RH7103): both empty nested classes become semicolon declarations and the operator gets a block body, as the issue's "Actual" shows. The comment of 2026-10-23 (unrelated `///`, collapsed `<summary>`) follows from the same call.

Further consequence not in the issue, measured: because the fix asks `FindUnsupportedSyntax` about the whole moved type (SHARED-FIX-010, correctly for the node it formats), **an unsupported shape anywhere in the type withholds every member-ordering fix in it**. Input: a type with `public void A() { }`, `public static void B() { }` and an unrelated `private int /* c */ _z;` (FMT-UNSUP-004) — RH7103 reports `B`, no fix offered. Once the fix stops formatting the type, SHARED-FIX-010 no longer applies to it and the fix is offered there; that is a visible change demanded by unchanged SHARED-FIX-006 and SHARED-FIX-010, not an open question.

I agree with the spec author that FMT-PIPE-010/011, which the issue cites, do not decide the case; SHARED-FIX-006/007 and SHARED-ORDER-002 do.

### Mechanism and candidates

Mechanism: the fix formats the node that contains the moved member (the whole type) instead of only the code it changes, so every formatter phase rewrites every member of that type — moved, crossed and untouched.

Dispatch: `TypeMemberOrderingCodeFixProviderBase` — subclasses RH7102, RH7103, RH7109 (all three, no other). The move itself is `OrderingDeclarationUtilities.MoveMemberBefore` → `OrderingMoveSafety.MoveNodeBeforePreservingSeparators` (keeps positional whitespace/EOL gaps at their positions, payload trivia travels). The RH7309 fix moves region blocks through the same family without calling the formatter at all (no `ReihitsuFormatter` reference in `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs`), which is the repository's existing precedent for a move that formats nothing.

### Breaking inputs

FC-030 (the defect itself), FC-016 (sibling fixes with the same call: #1335), FC-004/FC-020 (trivia and directives in the moved and crossed spans), FC-043 (the `FindUnsupportedSyntax` guard must follow what is formatted), FC-008 (Fix All with two moves in one type), FC-017 (assert exact output, LF and CRLF, convergence), FC-022 (raw strings in moved code).

Class matrix — what the whole-type call changes, by position relative to the move × kind of change:

| Position \ change | structural rewrite (FMT-STRUCT: empty type → `;`, expression body → block) | spacing / line breaks / continuation column | blank lines | doc-comment layout | unsupported-shape withholding |
|---|---|---|---|---|---|
| untouched member | fails (measured: operator, `Inner`) | fails (by the same call) | fails (measured) | fails (issue comment) | fails (measured) |
| crossed member | fails (measured: `Inner`) | fails | fails | fails | fails |
| moved member | fails (measured: `Helpers`) | fails (measured RH7109 `_b⏎;` joined; `+ 2` continuation moved, Q1) | fails | fails (issue comment) | fails |

Every cell fails today; all are one finding. Candidate inputs the test implementer must cover, each under LF and CRLF:

1. The issue's example (expected output as in the issue).
2. An untouched expression-bodied method in the type (#1604 item a).
3. A moved member with a line break inside it that the formatter joins (`private readonly int _b⏎        ;`, RH7109, #1604 item b) — stays.
4. A moved member with a continuation line (Q1) and one with a multi-line raw string literal whose lines are not under the opening quotes (measured: RH7103 realigns them today; under Q1 reading A they stay).
5. A moved member with a `/* */` comment over two lines — holds today (delta 0), must keep holding.
6. A type holding an unsupported shape in an unrelated member — fix offered after the change.
7. A moved member that shares its line with its neighbour (`int _a; static int _b;`, measured: today split onto two lines, which SHARED-FIX-007 *Shared line* demands) — must still get its own line, or be withheld (see *Refuse option*).
8. A moved member whose first line is indented differently from the target's (measured: `        static int _b = 1⏎            + 2;` behind `    int _a;` → first line to column 4, continuation to the formatter's column 20).
9. The `Decoy` test `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` (`RH7103…AnalyzerTests.cs:1065`) corrected to expect that region unchanged.
10. Fix All with two diagnostics in one type (`GetFixAllScenario`).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `TypeMemberOrderingCodeFixProviderBase` (RH7102/RH7103/RH7109 fixes) | must change |
| Its `FindUnsupportedSyntax` registration guard | must change with what the fix formats (FC-043, SHARED-FIX-010) |
| Fix All (BatchFixer) | must stay working; covered by the batch base |
| `OrderingMoveSafety` / `OrderingDeclarationUtilities` (Core) | must stay (the move and the safety guards are correct) |
| Analyzers RH7102/RH7103/RH7109 | not involved |
| Formatter | not involved |
| Rule pages | not involved |
| Specs | not involved beyond Q1's answer |

### Refuse option

Withholding the fix for "a type the formatter would change" is not a cheap check and would withhold for common code; it is not an option. Under Q1 reading A the requested change is smaller than any refusal: the move already lays the code out as SHARED-FIX-007 and SHARED-ORDER-002 demand, so the change is to stop formatting (a negative line count), as RH7309 already does. Under reading B the fix would need a mechanism that gives continuation lines the formatter's column while keeping braces, spacing and line breaks — no entry point does that, so it would be a prediction of the indentation phase (escalation 9 territory, and the class the previous run had to narrow away). That cost difference is for the lead, not for the question.

Rare sub-classes that may be refused locally if they would need more than a cheap check: moved or target member sharing its line with another member (item 7; two members on one line are uncommon), and a moved member indented differently from its target (item 8; misindentation is reported by the indentation rule). Declaring a shape unsupported: not an option — none of these is a formatter shape; they are local to the fix.

### Output questions

Q1 (spec author) — agree, recommend A. Two additions for the lead to make Q1 complete:

- Reading A should state the shift as "every line moves by the shift of the member's first line", not "zero for a move within the same type": `MoveNodeBeforePreservingSeparators` gives the moved member the target position's indentation, so a misindented member (item 8) has a non-zero first-line shift. Within reading A, the other lines then move by that same shift. This is A's own definition, not a new reading.
- Raw string literals (item 4) fall under the same question: under A they stay as written, under B they are realigned under the opening quotes (today). The lead may show it as a second line of Q1's input rather than a new question.

### Internal choices

- Items 7 and 8: refuse locally (rare) or keep the existing behavior — lead's call; recommend whatever stays a cheap check.

### Issue goals

- "formats the moved member and the separators around its old and new position" — change to "moves the member without formatting it; separators per SHARED-ORDER-002". Under SHARED-FIX-007 the moved member is not formatted. No visible difference for the issue's (formatter-clean) example.
- "correct the `Decoy` test" — keep.

### Architecture findings

_None_ for this issue (see *Whole run* for the shared-mechanism note).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1335

### Premise

Real for every remaining instance; reached by each rule's code fix. Measured today:

- D1 (RH7104): the issue's input → `internal partial class C` plus braces placed, `void M()`, braced `if`, `class Empty;`. The fix formats the whole `TypeDeclarationSyntax` after inserting one keyword (`RH7104…CodeFixProvider.cs:56`).
- E1 (RH7105): `override public string ToString() => "";` → block body; `static public class D { static int M( ) => 1; }` → `M()` with a block body. The fix formats the whole member (`ModifierOrderingCodeFixProviderBase.cs:144`), the whole class when the member is a class.
- F1 (RH7107): `set { if (value != null)⏎_d = value; }` before `get` → both accessors expanded, `if` braced; `{ set => _v = value; get => _v; }` → expanded onto five lines. Whole member formatted (`RH7107…:90`).
- G1 (RH7108): `remove { if (_e != null) _e -= value; }` before `add` → both expanded, `if` braced (`RH7108…:81`).
- A4 (RH7004): `var x = 0;⏎using var s = …;⏎x++;⏎return x;` → blank line inserted in front of `using (…)` and in front of `return x;`.
- `#endregion` instance (comment of 2026-11-06): `#region Setup⏎var n = 1;⏎#endregion⏎using var s = …;` → blank lines inserted in front of and behind `#endregion`.
- A3: measured by the spec author; consistent with the RH7004 runs above (blank line before the last moved statement every time).
- N2: `static public⏎// c⏎int _a;` and `static public⏎#region R⏎int _b;` — no fix offered (SHARED-MODORDER-003), confirmed. The issue's expected output is superseded; agree with the spec author.
- B1: resolved on `main` (#1581), nothing to do.

### Mechanism and candidates

Mechanism: each fix formats a node larger than the code it changes — the type (RH7104), the member (RH7105/RH7106, RH7107, RH7108), the new using statement with every statement moved into it (RH7004) — so the formatter rewrites code the diagnostic does not name.

Candidates (every fix in `Organization` that calls `FormatNodeInDocumentAsync`): RH7004, RH7104, RH7105 and RH7106 (via `ModifierOrderingCodeFixProviderBase`), RH7107, RH7108; plus the three ordering fixes of #969. RH7302, RH7303, RH7309, RH7310 do not format (text edits only). RH7101 rebuilt in #1581.

### Breaking inputs

FC-030, FC-016, FC-043 (each of these fixes guards with `FindUnsupportedSyntax` on the node it formats — e.g. RH7104 on the whole type — so an unsupported shape anywhere in that node withholds the fix today; after narrowing, the guard must follow the formatted node), FC-020/FC-021 (comments and directives between modifiers, between accessors, in the moved statements), FC-022, FC-025 (the seven expression-bodied kinds for E1), FC-008.

Matrix per fix — formatted node × what it rewrites outside the changed code (all measured or following from the same call):

| Fix | Changed code | Formatted node | Fails |
|---|---|---|---|
| RH7104 | one inserted keyword | type | braces, spacing, embedded statements, empty nested types, whole members (measured) |
| RH7105/RH7106 | modifier list | member (a type when the member is a type) | expression bodies (measured, also nested members of a class) |
| RH7107 | accessor order | property / indexer | accessor bodies braced, one-line accessor lists expanded (measured), multi-line auto indexer collapsed (issue comment) |
| RH7108 | accessor order | event | accessor bodies braced and expanded (measured) |
| RH7004 | header + braces; moved statements | new using statement | blank lines inserted between moved statements (measured), blank line in front of the statement and around `#endregion` (measured), spacing and continuation lines in moved statements (Q2), comment and raw-string lines (below) |

RH7004 moved-statement classes (the axis is what a line of the moved statements starts with), measured today and expected under Q2 reading A:

| Line starts with | Today | Expected (A) | Note |
|---|---|---|---|
| a token, normal statement | +4, but spacing/braces/blank lines reformatted | +4, otherwise as written | SHARED-FIX-007 example |
| a continuation of a wrapped expression aligned to an anchor (`M(a,⏎  b)`) | +4 (stays on its anchor) | +4 | A and B agree for aligned lines |
| a continuation not aligned (`+ b`) | formatter column | +4 | Q2 |
| inside a verbatim/regular string spanning lines | kept | kept | literal value (SHARED-FIX-003) |
| inside a `/* */` comment spanning lines | **moved from column 11 to column 6** (measured: `/* one⏎           two */`) | kept (FMT-INDENT-007) | today's output matches neither reading nor the formatter, which keeps column 11 (`scripts/format.sh`, measured). Comment text change under FMT-INDENT-007 — must be repaired in this run (safety rail if it were shipped) |
| a raw string content or closing line | realigned under the opening quotes (+8 in the measured case) | +4 with the closing quotes, or kept | Q5a |
| a comment in column 0 | moved to the statement column | +4, or kept in column 0 as RH3004 does | Q5b |
| `#region` / `#endregion` | +4, with blank lines inserted around them (measured) | +4, no blank lines | W1; region directives follow the code (FMT-INDENT-009 exception) |
| `#if`/`#else`/`#endif`, `#pragma` | stays in column 0 (measured for `#if NEVER`) | column 0 | W1 |
| disabled text | kept (measured) | kept | FMT-INDENT-011 |
| a statement written behind the using declaration on its line | not measured | — | rare (one statement per line is reported by a layout rule); refuse locally |
| a tab in the indentation | not measured | — | rare (RH5601 reports it); refuse locally or count as SHARED-FIX-007 first bullet says |

Also: a comment in front of the enclosing block's `}` (stays outside the new block today, with the parent `}`) and two using declarations in one block (Fix All: nested edits, FC-008).

Tests the issue names: `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements`, `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced`, and the three accessor tests of the comment of 2026-10-23 — their expectations change as SHARED-FIX-006/007 demand.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7004 fix | must change |
| RH7104 fix | must change |
| `ModifierOrderingCodeFixProviderBase` (RH7105/RH7106) | must change |
| RH7107, RH7108 fixes | must change |
| Their `FindUnsupportedSyntax` guards | must follow the node each fix still formats (FC-043) |
| `AccessorOrderingUtilities`, `OrderingMoveSafety` (Core) | must stay |
| Analyzers | not involved |
| Formatter | not involved (FMT-PIPE-009: fixes reach it only through `ReihitsuFormatter`) |
| Rule pages | not involved |

### Refuse option

- RH7104, RH7105/RH7106, RH7107, RH7108: no refusal is needed. SHARED-MODORDER-002 already fixes the exact text of the modifier list (single space after each), RH7104 inserts one keyword, and the accessor moves are within one list (shift 0 under Q1 A). The requested change is to format less, not more. Withholding these fixes would refuse common code and is not proportionate.
- RH7004: the requested change under Q2 A is a per-line shift of the moved statements. Its cost depends on which line classes it must handle (table above). Refuse options, each a cheap check over the moved span:
  - **W1 — withhold when the moved statements hold a directive or disabled text.** Cheap. Rare? `#region` inside a body is reported by RH7303, so rare by definition; `#if DEBUG` and `#pragma warning` inside method bodies are not reported by another rule and are not rare in real code. Recommend: refuse only disabled text locally (rare in this position, and kept as written anyway), keep directives on the per-line rule (non-region column 0, region with the code) — it is the kind of the line's first trivia, a cheap check. The spec author recommends the same.
  - **W2 — withhold when the moved statements hold a string or a `/* */` comment that spans lines.** Cheap, but refuses common code (multi-line raw strings and verbatim SQL/JSON in method bodies), so it is escalation 9, not a local decision; the maintainer's clarification makes it a legitimate option to offer, not a decided one. Note the cheapest non-refusing rule for this class: lines that start inside a token or inside comment trivia are left as written. That is one cheap check, keeps verbatim and comment lines correct, and keeps raw strings' values trivially; it differs visibly from the spec author's recommendation only for raw strings (Q5a option B).
  - RH3004 is no help here: its block-body path withholds every spanning string, every `/* */` spanning lines, every directive and disabled text (RH3004-008/009; registration at `RH3004UseLambdaSyntaxCodeFixProvider.cs:733-734`). Its private shifter handles none of the hard classes, so "use RH3004's mechanism" means "inherit RH3004's refusals" (= W1 + W2 as a whole).
- Declaring a shape unsupported: not an option — none of these is a formatter shape.

### Output questions

Q2 (spec author) — agree, recommend A. For formatter-clean code A and B give the same output (aligned continuation lines move with their anchor, measured); they differ only for continuation lines the formatter would move anyway.

Two more output questions within this issue (RH7004 moved statements); the lead batches them with Q1–Q4.

**Q5a · RH7004 · Using declarations should not be used — multi-line raw string in a moved statement**

Input:
```csharp
using System.IO;

class C
{
    string P()
    {
        using var s = new MemoryStream();
        var t = """
            a
            """;
        return t;
    }
}
```

Before (`main`, measured):
```csharp
using System.IO;

class C
{
    string P()
    {
        using (var s = new MemoryStream())
        {
            var t = """
                    a
                    """;

            return t;
        }
    }
}
```

After, A (recommended):
```csharp
using System.IO;

class C
{
    string P()
    {
        using (var s = new MemoryStream())
        {
            var t = """
                a
                """;
            return t;
        }
    }
}
```

After, B:
```csharp
using System.IO;

class C
{
    string P()
    {
        using (var s = new MemoryStream())
        {
            var t = """
            a
            """;
            return t;
        }
    }
}
```

After, C: no fix offered; the diagnostic stays.

Why A: the literal's lines move one level deeper with the statement, as every other line of it does, and the string's value stays the same.

(For the lead, not for the maintainer: A rewrites the literal's token text and needs a compile check of whitespace-only content lines; B touches no token and keeps the value trivially; C is W2 and refuses common code, escalation 9. If the lead wants the cheapest correct implementation, B is it — the pick would change to B if the maintainer prefers the smaller fix over the layout.)

**Q5b · RH7004 · Using declarations should not be used — comment in column 0 among the moved statements**

Input:
```csharp
using System.IO;

class C
{
    void M()
    {
        using var s = new MemoryStream();
//        s.Flush();
        s.Flush();
    }
}
```

Before (`main`, measured):
```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
            //        s.Flush();
            s.Flush();
        }
    }
}
```

After, A (recommended):
```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
//        s.Flush();
            s.Flush();
        }
    }
}
```

After, B:
```csharp
using System.IO;

class C
{
    void M()
    {
        using (var s = new MemoryStream())
        {
    //        s.Flush();
            s.Flush();
        }
    }
}
```

Why A: a comment in column 0 is usually code commented out by the editor and belongs to no nesting depth; the lambda fix (RH3004) already leaves such a line where it is, so both fixes that move statements into a new block treat it alike.

### Internal choices

- RH7104/RH7105/RH7106/RH7107/RH7108: whether to format nothing or a smaller node is internal as long as the output meets SHARED-FIX-006/007 and SHARED-MODORDER-002.
- The `/* */` comment line moved to column 6 today: any implementation must keep it at column 11 (FMT-INDENT-007); this is not a choice.

### Issue goals

- "Each input has a regression test" — keep; N2's test is a "no code fix is offered" test (SHARED-MODORDER-003), B1 needs none (resolved). No visible change in question.
- "the two named tests expect the unchanged layout" — keep.

### Architecture findings

_None_ beyond *Whole run*.

### Follow-up candidates

- Spec wording: SHARED-FIX-003 lists "the whitespace around a comment or a directive and the indentation of its lines" as layout, while FMT-INDENT-007 makes the later lines of a `/* */` comment part of its text. One reading lets a fix re-indent a comment's later lines, the other forbids it. Internal (no visible output of this run depends on it once the run keeps the lines), for an `agent-filed` spec issue.

## thoenissen/Reihitsu#1263

### Premise

Real, reached by the RH7303 and RH7310 code fixes (text edits that remove the two directive lines, `RH7303…CodeFixProvider.cs:52-63`). Measured RH7303: `var x = 1;⏎⏎#region Work⏎⏎x++;⏎⏎#endregion⏎⏎x--;` leaves two doubled blank lines; the same with whitespace-only blank lines (8 spaces) also leaves them doubled. The closing-brace case inserts nothing (as RH7303-004 now states; maintainer round 7b). RH7310 instances measured by the spec author.

### Mechanism and candidates

Mechanism: the fix deletes exactly the directive lines and nothing around them, so blank lines that were next to a directive end up next to each other or next to a body's brace.

Candidates — where an RH7303 pair can stand (`RegionDirectiveUtilities.IsWithinElementBody`: block, accessor list, anonymous function, local function, any statement — which includes a switch statement, a switch section, and initializers and argument lists inside a statement); RH7310 regions stand between members of a type body.

### Breaking inputs

FC-002 ("start / end of a body" must become an explicit condition), FC-003 (one blank vs. two; blank line on one side vs. both), FC-019/FC-020 (comment lines next to the removed directive), FC-017 (CRLF).

Matrix — position of the blank line the removal leaves × kind of enclosing construct (RH7303), measured today:

| Construct | blank doubled between two lines | blank at start of the body | blank at end of the body |
|---|---|---|---|
| method block | fails (measured) | fails (spec author) | fails (spec author) |
| accessor list `{ get …; set …; }` | not measured, same deletion | fails (measured) | fails (measured) |
| switch statement braces / section | same deletion | after `case 1:` — kept today; see Q4 | fails before the section's next label? see Q4 |
| collection / array initializer `{ … }` | same deletion | fails (measured) | fails (measured) |
| lambda / local function block | same as method block | same | same |

Whitespace-only blank lines count as blank (measured: they double the same way). Inputs to cover: the RH7303-004 and SHARED-FIX-007 examples, whitespace-only blank lines, CRLF, nested regions (only the reported pair goes), a comment line directly above the removed `#region` (stays; the single blank behind it stays), a pair whose two directives leave blanks at different places in one fix.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7303 fix | must change |
| RH7310 fix | must change (two SHARED-FIX-007 examples) |
| `RegionDirectiveBlankLineCodeFixProviderBase` / `BlankLineCodeFixUtilities` | not involved unless the implementer reuses them |
| Analyzers, formatter, rule pages | not involved |

### Refuse option

Withholding the fix where blank lines would be left is cheap but refuses common input (a region surrounded by blank lines is the usual layout), and the requested change is a small text edit. Not recommended. The construct classes other than blocks and type bodies (accessor list, switch, initializer) are rare positions for a region; the lead may decide locally to withhold the fix there if Q4's answer costs more than a cheap check. No unsupported shape.

### Output questions

**Q4 · RH7303 · Do not place regions within elements — blank line left inside the braces of an initializer**

Input:
```csharp
class C
{
    int[] M()
    {
        return new[]
               {

                   #region Values
                   1,
                   2
                   #endregion

               };
    }
}
```

Before (`main`, measured):
```csharp
class C
{
    int[] M()
    {
        return new[]
               {

                   1,
                   2

               };
    }
}
```

After, A (recommended):
```csharp
class C
{
    int[] M()
    {
        return new[]
               {
                   1,
                   2
               };
    }
}
```

After, B: as Before — only a block or a type body counts as a body; the blank lines stay.

Why A: inside any pair of braces a blank line at the edge is one the removal leaves where none belongs, and the formatter removes it there too (measured on an initializer and a switch).

(For the lead: the same reading decides an accessor list and a switch statement's braces; a blank line behind a `case` label is not at a brace and stays under both readings — the formatter keeps it there too, measured.)

### Internal choices

_None._

### Issue goals

- "none is inserted" and the closing-brace case: already decided (RH7303-004, maintainer round 7b). Keep.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1151

### Premise

Real, reached by the RH7302 code fix, and **more severe than the issue says**. The fix replaces every ordinal occurrence of the region's text (from its first non-whitespace character through the end, trailing whitespace included) in the whole `#endregion` trivia text, keyword included (`ReplaceExactText` over `endRegionDirective.ParentTrivia.FullSpan`, `RH7302…CodeFixProvider.cs:62-63, 104-121`). Measured:

- `#region a` / `#endregion // a cat` → `// A cAt`; `#region members` / `#endregion // end of members` → `// end of Members` (issue's instances; spec author measured too).
- `#region end` / `#endregion` → `#region End` / **`#Endregion`**; `#region e` / `#endregion // e` → **`#EndrEgion // E`**. `apply-fix.sh` reports `invalid-result` (the result does not compile) under LF and CRLF. Any region name whose text is a substring of `endregion` (`e`, `end`, `region`, `ion`, `on`, …) breaks the directive. This violates SHARED-FIX-003's first condition; RH7302-005 as clarified (rename only an `#endregion` whose description is exactly the name, rewriting only the description) excludes it.
- `#region my members` / `#endregion // my members` → renamed (exact; holds).
- `#region members␠␠` (trailing whitespace) / `#endregion // members` → not renamed (Q3).

### Mechanism and candidates

Mechanism: the fix matches the old name as a substring anywhere in the `#endregion` directive's text, keyword and other words included, instead of comparing the `#endregion` description (SHARED-REGDESC-001) with the region description.

Single code path; candidates are the axes below.

### Breaking inputs

FC-022-like (text that looks like the name inside other text), FC-026 (several occurrences of the same text on one line), FC-003, FC-017 (CRLF: the directive's end-of-line belongs to the trivia's full span).

Matrix — `#endregion` text relative to the region name `n`:

| `#endregion` text | Today | Expected (RH7302-005) |
|---|---|---|
| exactly `n`, with or without `//`, any whitespace around `//` and behind | renamed, spacing kept (holds) | renamed, spacing kept |
| `n` with other words (`// end of n`) | fails (renamed) | unchanged |
| `n` inside a word (`// a cat` for `a`) | fails | unchanged |
| `n` occurs twice (`// n n`) | fails (both renamed) | unchanged |
| no text, `n` a substring of `endregion` | **fails — non-compiling directive** | unchanged |
| exactly `n`, `n` a substring of `endregion` | **fails — non-compiling directive** | description renamed, keyword unchanged |
| different case (`// Members`) | unchanged (holds) | unchanged |
| `#region` line with trailing whitespace, `#endregion` exactly `n` | unchanged | Q3 |
| `#endregion` with trailing whitespace, exactly `n` | renamed | renamed (whitespace around is layout, RH7302-005) |
| multi-word name exactly | renamed (holds) | renamed |

Each under LF and CRLF; plus a pair in a `#if`-excluded branch, which RH7302-001 reports too.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7302 fix | must change |
| RH7302 spec | changed by 57f49a2 (clarification); Q3 may touch RH7302-005's condition |
| RH7302 analyzer, RH7301, rule page | not involved |

### Refuse option

Withholding the fix when the `#endregion` is not exactly the name is the wrong refusal: the spec keeps such an `#endregion` unchanged and still capitalizes the `#region`, which is cheaper than refusing (it deletes the substring replace). No unsupported shape.

### Output questions

Q3 (spec author) — agree, recommend A. Support: SHARED-REGDESC-001 defines a description with surrounding whitespace removed, RH7301 compares those, and RH7302-005 already ignores whitespace on the `#endregion` side; the condition "the `#region` line ends with its description" is the only text that reads B.

### Internal choices

_None._

### Issue goals

- "the RH7302 spec states that only a comment that is exactly the region name is renamed" — done by 57f49a2. Keep.
- Add the non-compiling member to the run's tests; the issue does not name it.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1604

### Premise

Real for the member-ordering fixes and RH7004 (see #969 and #1335; same mechanism, same evidence). Items (a), (b), (c) of the comment of 2026-11-08 reproduce (measured (b) and (c); (a) follows from the whole-type call measured in #969). Item (d) did **not** reproduce in three shapes I built from its description (`P => #if DEBUG 1; #else 2; #endif` followed by `_e`, `_c`, with and without a leading `_a`): RH7109 offers no fix in each (the directive guard, SHARED-ORDER-008). The test implementer needs the comment's exact input; if none reproduces, (d) gets no test and is noted as not reproduced.

The issue's description of RH3004 is stale: it says RH3004's private mechanism keeps the lines of non-raw string literals, moves raw-string lines and keeps column-0 directives in column 0. RH3004 withholds its block-body fix for every string or `/* */` comment that spans lines, raw included, and for every directive and disabled text (RH3004-008/009; `RH3004UseLambdaSyntaxCodeFixProvider.cs:733-734`). What RH3004 owns is a shift of plain lines plus two RH3004-specific rules (column-0 comment lines stay; leftmost-line correction for a statement written behind `{`).

### Mechanism and candidates

Same mechanism sentences as #969 and #1335. Candidates: RH7102, RH7103, RH7109, RH7004 (RH3004 out of scope by decision 2).

### Breaking inputs

See #969 (items 1–10) and #1335 (RH7004 table).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Member-ordering base, RH7004 fix | must change (as in #969, #1335) |
| RH3004 fix | must stay (decision 2) |
| A shared "move lines by a column delta" helper | not required by any requirement; lead's structural decision (*Whole run*) |
| Formatter `RawStringLineShifts` | not involved (FMT-PIPE-009 permits calling a formatter helper that is not a phase or rewriter, but nothing requires it) |

### Refuse option

As in #969 (no refusal needed under Q1 A) and #1335 (W1, W2). The helper the issue asks for buys nothing for the member-ordering fixes (shift 0) and, built from RH3004, nothing for RH7004's hard classes (RH3004 refuses them).

### Output questions

Q1, Q2 (spec author), Q5a, Q5b (above).

### Internal choices

_None_ beyond *Whole run*.

### Issue goals

- "One helper owns move lines by a column delta, the RH7004 and member-ordering fixes use it, the RH3004 fix has no private copy" — drop as a goal: it is a prescribed structure, the member-ordering fixes need no shift under Q1 A, and RH3004's copy covers only lines RH7004 also handles trivially. No visible output depends on it.
- "the RH3004 fix … offers the fix for a block holding a multi-line raw string literal" — dropped by decision 2; agree.
- "each fix has a test with moved code that the formatter would change" — keep.
- Close with `Refs #1604` or `Closes #1604`: the issue's purpose (moved code keeps its layout in RH7004 and the member-ordering fixes) is met by the run; the dropped goals are structure and a behavior change. `Closes` fits once the PR states the difference.

### Architecture findings

See *Whole run*.

### Follow-up candidates

- RH3004 block-body fix for multi-line raw strings — already recorded by the lead (decision 2).

## Whole run

### Architecture findings

- **Prospective second owner of "shift moved statement lines by a column delta".** Components: the RH3004 fix (`MoveStatementsAsync`, `AppendLaterLines`, `ShiftIndentation`, `GetColumnDeltas`) and the RH7004 fix once it implements Q2 A; the formatter's `RawStringLineShifts` makes the analogous decision for raw strings in the indentation phase. Decision shared: which lines of statements that move into a deeper block move, and by how much. Not a defect today; it becomes one (FC-015) if the RH7004 change adds its own per-line shifter with rules that overlap RH3004's (README, *Architecture findings*: "the narrow fix would itself add a copy"). Options for the lead: keep RH7004's rule to one cheap check that shares no RH3004 rule (lines that start inside a token or comment trivia stay; every other line moves; directives by kind), so the two are different rules rather than copies; or extract RH3004's plain-line shift for both (more than ~100 lines with RH3004's two specific rules, escalation 9 if it grows a new type); or refuse (W1+W2). Q5b's answer A makes the two fixes share the column-0 rule, which favors extraction; answer B favors separate rules.
- The six fixes that format a larger node (#969, #1335) share one mechanism but no decision between two components; it is FC-030 repeated (FC-016), not an architecture finding.

### Notes for the lead

- Q1, Q2 and Q5a/Q5b are best asked together; Q3 and Q4 are independent.
- Under Q1 A the member and accessor moves need no formatter call, so their `FindUnsupportedSyntax` guards must go with it (FC-043); the fix is then offered in types that hold an unsupported shape (measured: withheld today). That is demanded by SHARED-FIX-006/010, not a question, but the PR should name it under review notes.
- The RH7004 `/* */` comment line moved to the wrong column today, and the RH7302 non-compiling `#endregion`, are both pre-existing on `main` and both inside the run's issues; the run repairs them, so neither is an escalation 2.

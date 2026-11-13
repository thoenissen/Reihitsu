Role: spec author
Commit: 82321f3
Verdict: #969: decided by unchanged requirements, apart from Q1; #1335: decided by unchanged requirements, apart from Q1/Q2; #1263: spec written (clarification); #1151: spec written (clarification), open question Q3; #1604: open question (Q1, Q2)

How the statements below were established: every issue and its comments read through the GitHub MCP server; the cited requirements read in `sdd/specs/`; today's output measured with `scripts/apply-fix.sh` on fixtures in the scratchpad (not committed) for RH7109, RH7004, RH7302, RH7303 and RH7310 — every instance quoted below as "today" was produced on 82321f3. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed after the spec edits.

Ignored input: thoenissen/Reihitsu#1604 prescribes a solution (one shared "move lines by a column delta" helper used by RH3004, RH7004 and the member-ordering fixes). That is a proposed implementation, not a requirement; nothing below depends on it.

The maintainer clarification in `00-lead.md` (withholding a fix is an equally valid outcome; a full implementation can follow later) is applied in the withholding options below.

## thoenissen/Reihitsu#969

### Changed requirements

_None._ The expected output is decided by unchanged requirements:

- SHARED-FIX-006, *Formatting* bullet: the fix formats only the node it replaces and never the type that contains it. The empty nested classes stay braced, the expression-bodied operator stays, and the spacing elsewhere stays.
- SHARED-FIX-007, *Moved code* bullet: the moved member keeps its braces, its token spacing, its line breaks and its inner blank lines; this also decides the comment of 2026-10-23 (an unrelated `///` behind code stays where it is; a moved member's one-line `<summary>` stays on one line).
- SHARED-ORDER-002: separators stay at their positions; attached trivia travels.
- RH7103-006 / RH7102-005 / RH7109-005 point to SHARED-ORDER-001/002 for the move.

FMT-PIPE-010 and FMT-PIPE-011, which the issue cites, do not decide this case: they state what the node-level entry points do with the target they are given, and the fix passes the whole type as target. The defect is against SHARED-FIX-006/007, not against FMT-PIPE-010/011.

The issue's expected output for its example is consistent with these requirements. The test `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` expecting changes in the `Decoy` region contradicts SHARED-FIX-006; its correction to "unchanged" is demanded by that requirement.

One part is not decided: the column of a continuation line of a moved member (Q1 under #1604).

### Examples for the tests

SHARED-FIX-006 (*Formatting*), SHARED-FIX-007 (*Moved code*, example for RH7309), SHARED-ORDER-002, RH7103-006; the issue's own example and expected output.

### Output questions

Q1 (listed under #1604) applies here as well.

### Internal choices

- No requirement added: the expected output is in SHARED-FIX-006/007; restating it in RH7102/RH7103/RH7109 would duplicate a shared rule. Alternative: a per-rule clarification — rejected by the rulebook (a shared rule is referenced, not restated).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1335

### Changed requirements

_None._ Instance by instance:

- A3 (RH7004): SHARED-FIX-007 *Moved code*, with its RH7004 example — the moved statements keep `s.Flush( );`, the unbraced `if (true) s.Flush();`, and no blank line is inserted before `return n;`. Measured today: a blank line is inserted before the last moved statement (`return total;`).
- A4 and the `#endregion` instance of 2026-11-06 (RH7004): SHARED-FIX-006 *Blank lines* — no blank line inserted in front of the `using` statement, none around the `#endregion`.
- B1 (RH7101): resolved on `main` by #1581, as the issue's status note says; nothing to do.
- D1 (RH7104): RH7104-004 plus SHARED-FIX-006 *Formatting* — only the accessibility keyword is inserted; `void M( )`, the unbraced `if`, `class Empty {⏎}`, and the brace placement of `partial class C {` stay.
- E1 and the whole-member instance of 2026-11-06 (RH7105/RH7106): SHARED-MODORDER-002 plus SHARED-FIX-006 *Formatting* — only the modifier list is rewritten; expression bodies, `M( )` spacing and the rest of the member or class stay.
- N2 (`static public⏎#region R⏎int _a;` and `static public⏎// c⏎int _a;`): superseded. SHARED-MODORDER-003 (unchanged) now withholds the fix when a comment or any directive stands between the last modifier and the token after it, so no fix is offered for either input. The issue's expected output ("no blank line inserted, the comment stays") describes a fix that the spec no longer offers; the test for N2 is a "no code fix is offered" test.
- F1 (RH7107), G1 (RH7108), and the comment of 2026-10-23 (single-line accessor lists expanded, multi-line auto indexer collapsed): SHARED-FIX-007 *Moved code* (covers accessors) plus SHARED-ORDER-002 — the moved accessor keeps its layout, the setter / `remove` body is not braced, and the separators between the accessors stay as written, so a one-line accessor list stays on one line (`{ get => _value; set => _value = value; }`, `{ add { _changed += value; } remove { _changed -= value; } }`) and the multi-line indexer stays multi-line (`{⏎get;⏎set;⏎}`). The summaries of the three named tests already state this; their expected values change as those requirements demand.
- The two tests the issue names, `VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements` and `BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced`: their changed expectations are demanded by SHARED-FIX-006/007.

### Examples for the tests

SHARED-FIX-006 (all three bullets), SHARED-FIX-007 (*Moved code*, RH7004 example), SHARED-ORDER-002, SHARED-MODORDER-002, SHARED-MODORDER-003 (N2), RH7004-004, RH7104-004, RH7107-004, RH7108-004; the inputs listed in the issue.

### Output questions

Q1 and Q2 (listed under #1604) apply to the member-ordering, accessor and RH7004 moves of this issue.

### Internal choices

- N2 is handled as "no fix offered" under SHARED-MODORDER-003 instead of the issue's expected output. Nothing visible is in question: the requirement is unchanged and already demands it.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1263

### Changed requirements

- RH7303-004 — The fix removes the region's two directive lines — clarified. The requirement was stated only for directives with no blank line around them and not behind a statement ending in `}`. It now covers every region the fix is offered for: the two directive lines go, the blank lines the removal leaves at the start or end of a body or doubled between two lines go as SHARED-FIX-007 (*Removed directive line*) states, and no blank line is inserted, also behind a statement ending in `}` (SHARED-BLANK-003 would ask for one; RH5030 reports it on its own). Two examples added (doubled blank line / blank lines at the edges of a body; closing brace). The closing-brace case records the maintainer's round-7b decision quoted in the issue's comment of 2026-10-20 (option A: `}` directly followed by the statement, nothing inserted), which is also today's output. `Uses` gains SHARED-BLANK-003 and SHARED-FIX-007.
- RH7310: no change. SHARED-FIX-007 already carries RH7310 examples for both reported instances (blank line at the end of a body, at the start of a body); RH7310-004 covers only the sibling-declaration case and stays as is.

Today (measured): RH7303 `var x = 1;⏎⏎#region Work⏎⏎x++;` leaves two blank lines; blank lines at the start and the end of a body stay; the closing-brace case inserts nothing (already as specified). RH7310 `int _a;⏎⏎#region Methods⏎⏎#endregion // Methods⏎}` gives `int _a;⏎⏎}`; `{⏎⏎#region Fields⏎#endregion // Fields⏎int _a;` keeps the blank line after `{`.

### Examples for the tests

RH7303-004 (all three examples), SHARED-FIX-007 (the two RH7310 examples and the RH7303 example).

### Output questions

_None._

### Internal choices

- RH7303-004 rewritten in place (same ID, wider scope) rather than a new RH7303 requirement: the blank-line rule is SHARED-FIX-007's and is referenced, not restated. Alternative: a new RH7303-006 for blank lines — rejected as a restatement.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1151

### Changed requirements

- RH7302-005 — The fix renames the `#endregion` that repeats the name — clarified. It already required renaming when the `#endregion` text is exactly the description, but said only that a text *not containing* the old description stays; a text containing it with other words was undecided. It now states that every other `#endregion` stays unchanged, also one containing the name with other text (`// end of members`) or inside a word (`// a cat` for region `a`), and that a renamed `#endregion` keeps its `//` and the whitespace around it (today's behavior: `#endregion   //items` becomes `#endregion   //Items`). Example added. `Uses` gains SHARED-FIX-003 (a changed word in a comment is content). This is the statement the issue's "Done when" asks for.

Today (measured): `#endregion // a cat` → `// A cAt`; `#endregion // end of values` → `// end of Values`; exact matches renamed.

The rule page `documentation/rules/RH7302.md` is not changed: it says the fix "updates the matching `#endregion` comment", which stays true; the narrowed case is a corner case.

### Examples for the tests

RH7302-005 (both examples).

### Output questions

**Q3 · RH7302 · #region description should start with an uppercase letter — whitespace behind the description**

Input (the `#region` line ends with two spaces behind `members`):
```csharp
class C
{
    #region members  
    int _a;
    #endregion // members
}
```

Before (`main`, measured):
```csharp
class C
{
    #region Members  
    int _a;
    #endregion // members
}
```
RH7301 then reports the `#endregion`.

After, A (recommended):
```csharp
class C
{
    #region Members  
    int _a;
    #endregion // Members
}
```

After, B: as Before — the `#endregion` stays `// members`, and RH7301 reports it after the fix.

Why A: whitespace behind the name is layout, so the `#endregion` does repeat the name, and the pair still agrees after the fix.

(Background for the lead, not for the maintainer: RH7302-005 conditions the rename on "the `#region` line ends with its description"; with trailing whitespace the line reads two ways. The issue's comment of 2026-10-19 lists this as a further instance of the bug, i.e. reading A. RH7301 compares descriptions with surrounding whitespace removed, SHARED-REGDESC-001. The `#endregion` side already ignores surrounding whitespace in RH7302-005. The run can write every other RH7302 test without this answer.)

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1604

### Changed requirements

_None._ SHARED-FIX-007 (*Moved code*) decides that moved code keeps its braces, spacing, line breaks and blank lines, and (comment of 2026-11-08) items (a) via SHARED-FIX-006 *Formatting*, (b) via *Moved code* ("also where formatting the result would change them"), and (d) via SHARED-ORDER-002 (the blank line in front of the crossed `P` stays in front of `P`). Item (c) is Q1.

Measured today: RH7109 moving `private readonly int _b⏎        ;` joins it into `private readonly int _b;` (item b). RH7109 moving `private readonly int _b = 1⏎            + 2;` (delta 0) moves `+ 2;` from column 12 to column 30.

### Examples for the tests

SHARED-FIX-007 (*Moved code*, RH7004 and RH7309 examples), SHARED-FIX-006 (*Formatting*), SHARED-ORDER-002.

### Output questions

Q1 and Q2 are one decision — what "only the indentation of its lines follows its new nesting" (SHARED-FIX-007, *Moved code*) means for a line of moved code that does not start a statement or member. Reading A: every line of the moved code shifts by the same number of columns as the moved code's first line (zero for a move within the same type or accessor list), so the lines keep their columns relative to each other. Reading B: every such line gets the column the formatter gives it. RH3004-006 already specifies reading A for the RH3004 block body (one shift for all moving lines). Tests with continuation lines in moved code for #969, #1335 and #1604 cannot be written without the answer; every other test can.

**Q1 · RH7109 · Readonly elements must appear before non-readonly elements — continuation line of a moved field**

Input:
```csharp
class C
{
    private int _a;
    private readonly int _b = 1
            + 2;
}
```

Before (`main`, measured):
```csharp
class C
{
    private readonly int _b = 1
                              + 2;
    private int _a;
}
```

After, A (recommended):
```csharp
class C
{
    private readonly int _b = 1
            + 2;
    private int _a;
}
```

After, B:
```csharp
class C
{
    private readonly int _b = 1
                              + 2;
    private int _a;
}
```

Why A: the field only changes its position, not its nesting, so its lines keep the columns the author wrote, as its first line already does.

The same holds for RH7102 and RH7103 (member moves) and RH7107 and RH7108 (accessor moves).

**Q2 · RH7004 · Using declarations should not be used — continuation line of a statement moved into the new block**

Input:
```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using var s = new MemoryStream();
        var total = a
            + b;
        return total;
    }
}
```

Before (`main`, measured):
```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using (var s = new MemoryStream())
        {
            var total = a
                        + b;

            return total;
        }
    }
}
```

After, A (recommended):
```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using (var s = new MemoryStream())
        {
            var total = a
                + b;
            return total;
        }
    }
}
```

After, B:
```csharp
using System.IO;

class C
{
    int M(int a, int b)
    {
        using (var s = new MemoryStream())
        {
            var total = a
                        + b;
            return total;
        }
    }
}
```

Why A: every moved line moves one level deeper together, so the statements keep the layout the author gave them, as the RH3004 fix already does for the block it moves.

(In both A and B the blank line before `return total;` is gone: SHARED-FIX-006/007 already demand that.)

### Withholding options

Where withholding the fix for an input class is an option, with the cost; none of these is needed for the member and accessor moves under reading A, because those moves shift no line (delta 0).

- **W1 — RH7004: the moved statements hold a preprocessor directive or disabled text.** Under reading A, a shift must decide whether a directive line (FMT-INDENT-009: column 0, regions excepted) and the lines of disabled text (FMT-INDENT-011: never re-indented by the formatter; SHARED-FIX-003 counts their indentation as layout) move. Withholding the fix for such statements avoids both decisions. Read from the code, not measured: the fix is offered for them today when the chain lies wholly inside the following statements (`WouldSeparateDirective` only refuses a split pair). A directive inside a method body after a `using` declaration is uncommon but not rare in `#if DEBUG` code. Recommendation: withhold for disabled text (rare, unparsed); keep a non-region directive at column 0 and move a region directive with the code, which is a cheap per-line check and is the formatter's convention — the lead decides whether that is cheap enough or the class is withheld as a whole.
- **W2 — RH7004: the moved statements hold a string literal or a delimited comment that spans lines.** Under reading A, a verbatim or regular string's continuation lines and a delimited comment's later lines must keep their columns (SHARED-FIX-003: their whitespace is content), and a multi-line raw string's content lines move with its closing quotes (value unchanged). RH3004-008 withholds its block-body path for every such carrier. Multi-line raw strings in method bodies are common code, so withholding here would be escalation 9, not a local decision. Recommendation: do not withhold; the per-line rule is the one FMT-INDENT-007/011 and FMT-RAW already apply today.
- **#1335 N2** is already withheld by SHARED-MODORDER-003; nothing to decide.

No shape qualifies for declaring it unsupported: none of the above is a shape the formatter's precheck would refuse; each is a code-fix-local class.

### Internal choices

- The goal "the RH3004 fix ... offers the fix for a block holding a multi-line raw string literal" contradicts unchanged RH3004-007 and RH3004-008, which withhold that fix; offering it is a visible output change that needs a change doc and the behavior-change lane. Recommendation: drop that goal from this bug-fix run (the issue is `agent-filed`) and leave RH3004 as specified; keeping today's output puts nothing in question. Alternative: widen the run to behavior change with a change doc for RH3004 — rejected as out of the run's scope; it can be a follow-up.
- The goal "one helper owns move lines by a column delta, the RH3004 fix has no private copy" is structure, not output; it is the lead's scope decision. Note that RH3004-006's line rules (column-0 comment lines stay, leftmost-line correction for a statement written behind `{`) are specific to RH3004 and are not part of SHARED-FIX-007; a shared helper must not carry them into RH7004 or the ordering fixes.

### Follow-up candidates

- RH3004 offering its block-body fix for a block that holds a multi-line raw string literal (from the dropped goal above), if the lead drops it: a behavior change against RH3004-007/008, for an `agent-filed` follow-up with the output question in the AGENTS.md form.

## Whole run

- Files changed: `sdd/specs/analyzer/RH7302.md`, `sdd/specs/analyzer/RH7303.md`. No new requirement IDs; `sdd/specs/requirement-ids.txt` unchanged. No change doc: no visible output changes beyond what unchanged requirements already demand, pending Q1–Q3.
- No rule page changed.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.

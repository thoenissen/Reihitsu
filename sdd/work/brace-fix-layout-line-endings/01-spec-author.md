Role: spec author
Commit: 3d478c6 (statements derived from this tree; outputs "Before" measured with `scripts/apply-fix.sh` on it, LF and CRLF alike)
Verdict: #1416: open question; #1388: spec written; #1627: open question; #1142: open question

The prompt carried no intended fix or hoped-for verdict. All four issues are `agent-filed`; their goals were treated as proposals.

## thoenissen/Reihitsu#1416

### Changed requirements

- SHARED-BRACE-002 — The fix wraps the body in braces — clarified (aligned to SHARED-FIX-006/007/008, as the maintainer's triage comment asks). It now states: only the body is wrapped; the `if`/`else` header with its comments, the other body, and the blank lines around the statement stay as written, and no blank line is inserted (SHARED-FIX-006; #1220 Q2/Q3, #1209 Q3). The braces are laid out as the formatter lays them out, in the column of the first token of the line the owning `if`/`else` starts on — the `if`, the `else` (also for an `else if` whose `else` starts its line; today the fix uses the inner `if` column 13), or the closing brace for `} else`. The body is moved code (SHARED-FIX-007): token spacing, line breaks, and blank lines inside it stay. Line breaks follow SHARED-FIX-008, first paragraph (created and kept breaks get the detected line ending).
- `sdd/specs/shared/statement-braces.md` — `Uses` gains SHARED-FIX-006, SHARED-FIX-007, SHARED-FIX-008; one non-goal added (brace column for an `if`/`else` behind other code than `else` or `}`, such as a `case` label).

The rule pages of RH5405–RH5407 are unchanged: what is reported and how to fix it do not change for the user in the normal case.

### Examples for the tests

SHARED-BRACE-002 (body on the `if` line; header, body spacing and blank line kept; other body kept; `else if` column; `} else`; comment behind the body; line endings).

### Output questions

**1 · RH5406 · Braces must not be omitted from multi-line child statements — continuation lines of the wrapped body**

SHARED-FIX-007 (*Moved code*) says only that the indentation of the wrapped body's lines follows the new nesting; its uniform-shift sentence names only member/accessor moves and RH7004, not the brace fixes. So the column of a continuation line is open. Touches SHARED-BRACE-002, SHARED-FIX-007.

Input:
```csharp
        if (a)
            M(a,
                b);
```

Before (`main`, the fix formats the whole `if`):
```csharp
        if (a)
        {
            M(a,
              b);
        }
```

After, A (recommended) — every line of the body moves by the shift of its first line, as for RH7004 and member moves; here the shift is zero:
```csharp
        if (a)
        {
            M(a,
                b);
        }
```
For `if (a) M(a,⏎            b);` (body starts on the `if` line, `M` moves from column 15 to 12) the continuation moves three columns left, to column 9. Consequence: where the body moves (shift not zero) and holds a string literal or a comment that spans lines, disabled text, a tab in front of a moving line, or a line too far left for the shift, the fix is withheld, as RH7004-007/SHARED-ORDER-010 withhold their moves.

After, B — continuation lines get the column the formatter's indentation phase gives them, the tokens and line breaks stay (here `b` under `a`, column 14, as on `main`). Needs a formatter mode that lays out only columns, which does not exist (#1140 *Structural cause*, #1605).

Why A: it is the rule the maintainer chose for every other wrapped or moved statement, and where the body does not move it changes nothing in it.

**2 · RH5405 · Braces must not be omitted — code behind the body on its line ends up behind the new `}`**

Touches SHARED-BRACE-002, SHARED-FIX-007 (the new `}` is code the fix creates).

Input (two shapes, one application of the fix on the first body):
```csharp
        if (a) M(a, b); /* c */ b++;
        if (a) M(a, b); else M(a, b);
```

Before (`main`): first line gives `if (a)⏎{⏎    M(a, b); /* c */⏎} b++;`; second line braces both bodies and puts `else` on its own line (the second body is changed although it is another diagnostic — ruled out by SHARED-FIX-006 under every reading).

After, A (recommended) — the code behind the body starts the next line, in the column of the new `}`:
```csharp
        if (a)
        {
            M(a, b); /* c */
        }
        b++;
        if (a)
        {
            M(a, b);
        }
        else M(a, b);
```

After, B — it stays behind the new `}`:
```csharp
        if (a)
        {
            M(a, b); /* c */
        } b++;
        if (a)
        {
            M(a, b);
        } else M(a, b);
```

Why A: the formatter breaks the line behind the brace the fix created, so B hands back a line the formatter changes again; it matches SHARED-FIX-007's shared-line rule for members.

## thoenissen/Reihitsu#1388

### Changed requirements

- RH5011-004 — The fix inserts a blank line below the `break` line — clarified: the blank line ends in the line ending SHARED-FIX-008 specifies (the line's own break in an all-CRLF or all-LF document, `\n` in an all-lone-`\r` document); lone-`\r` example sentence added.
- RH8304-004 — The fix moves the element onto a new `///` line — clarified the same way; lone-`\r` example sentence added.
- SHARED-BRACE-002 (see #1416) now states the line endings of the brace fixes, which covers the RH5405 CRLF comment instance and the RH5406 lone-`\r` instance.

No change needed elsewhere:
- RH7004 with a comment or directive in front of the next statement: RH7004-007 already says the fix writes the detected line ending for every line break it creates — a bug against it.
- RH7309: SHARED-FIX-008's first paragraph already requires every kept line break in the moved blocks, outside literals and comment text, to get the detected line ending; RH7309 is not on the second paragraph's list of moves that are withheld instead. That is what the issue expects.

### Examples for the tests

RH5011-004, RH8304-004, SHARED-BRACE-002 (line-ending sentence), SHARED-FIX-008 (existing examples).

### Output questions

_None._

### Note on the `FormatNode` instance (for the skeptic)

The only `ReihitsuFormatter.FormatNode` call in the code fixes is `RH5401…CodeFixProvider.CanCollapse`, a predicate that tests the result for `'\n'`; the fix output comes from `FormatNodeInDocumentAsync`. So the line ending `FormatNode` picks on a node with no line breaks seems to reach no document. If that holds, under `AGENTS.md` *Supported inputs* the instance is dropped, and FMT-PIPE-007 (fallback to `Environment.NewLine` for an input without a line break) stays unchanged. No spec change was made for it.

## thoenissen/Reihitsu#1627

### Changed requirements

None of its own. SHARED-BRACE-002 (see #1416) covers the braces the fix creates. SHARED-FIX-007 already requires no trailing whitespace behind the new `}`, which is created code.

### Examples for the tests

Wait for the answer to question 3. Then use the input below, for each of RH5405–RH5407, in LF and CRLF.

### Output questions

**3 · RH5405 · Braces must not be omitted — whitespace at the end of the body's line**

Touches SHARED-BRACE-002, SHARED-FIX-007 (*Moved code*). The issue asks for no whitespace anywhere. On `main`, RH7004 keeps trailing whitespace on the lines it moves, and the new `}` gets none.

Input (`·` marks a trailing space):
```csharp
        if (a)
            M(a, b); /* x */···
```

Before (`main`):
```csharp
        if (a)
        {
            M(a, b); /* x */
        }···
```

After, A (recommended) — the whitespace stays at the end of the body's line, none behind `}`:
```csharp
        if (a)
        {
            M(a, b); /* x */···
        }
```

After, B — removed from both lines:
```csharp
        if (a)
        {
            M(a, b); /* x */
        }
```

Why A: the body is moved code, which keeps its lines as written, as RH7004 does on `main` and as SHARED-ATTR-017 now keeps whitespace behind a split list. The layout rules still report the whitespace.

## thoenissen/Reihitsu#1142

### Changed requirements

- SHARED-ATTR-017 — renamed "The split fix starts each further list on a new line" — clarified to the maintainer's reading A (#1109 Q1, question round 5). The fix changes only the reported list. Each list after the first starts a new line, behind the whitespace at the start of the reported list's line. What stands in front of the list or behind it on its line stays there. Every other line stays as written: indentation, comments, directives, trailing whitespace. The inserted line breaks are the only line breaks the fix writes, in the detected line ending. The old narrow "This requirement covers…" list is replaced. The one coverage restriction left is a line with only spaces in front of it, pending question 4. New examples: misindented braces with a directive and column-0 comment; four shared-line shapes; line endings. `Uses` gains SHARED-FIX-006.
- RH5502-003, RH5504-003, RH5506-003, RH5508-003, RH5510-003, RH5512-003, RH5514-004, RH5516-003, RH5518-003, RH5520-003, RH5522-003, RH5526-003, RH5528-003, RH5531-005 — clarified: "each on its own line" became "each list after the first starting a new line", because `[A] [B, D2] void M()` gives `[A] [B]` / `[D2] void M()`.

The rule pages are unchanged: the normal case (a list on its own line) looks the same to the user.

### Examples for the tests

SHARED-ATTR-017 (all examples), one per shape of the issue's instance list. The Fix All iterations remain the Fix All issue's.

### Output questions

**4 · RH5514 · Attributes of methods must be in separate lists — a tab-indented list**

Touches SHARED-ATTR-017 and SHARED-FIX-007, whose first bullet counts a tab in front of changed code as one column and lays out the changed code from there.

Input (`→` marks a tab; the class braces are in column 0):
```csharp
class C
{
→[A, B]
→void M()
→{
→}
}
```

Before (`main`; the code action's Roslyn post-processing rewrites the tabs):
```csharp
class C
{
    [A]
    [B]
    void M()
→{
→}
}
```

After, A (recommended) — the new line copies the line's whitespace:
```csharp
class C
{
→[A]
→[B]
→void M()
→{
→}
}
```

After, B — the new line gets the column of `[A]` counted with the tab as one column, written as spaces:
```csharp
class C
{
→[A]
 [B]
→void M()
→{
→}
}
```

After, C: no fix is offered for a tab-indented list. The diagnostic stays, and RH5601 reports the tabs.

Why A: it is the maintainer's reading A in its own words (nothing around the list moves, each list starts at the indentation of the reported list's line), and it is a cheap copy.

## Whole run

### Internal choices

- SHARED-BRACE-002: `} else M(a, b);` keeps `else` behind the brace. The new braces go in that brace's column. This follows SHARED-FIX-006 (#1220 Q3: the fix must not move `else`). `main` moves `else` onto its own line, so this is a visible difference from `main`. I derived it from the spec and did not ask it. Alternative: put it to the maintainer as an output question; the lead may prefer to ask it.
- SHARED-BRACE-002: the brace fixes stay under SHARED-FIX-008's first paragraph: kept line breaks are rewritten to the detected line ending. They are not added to the second paragraph's list of moves that are withheld for a foreign line break. That is the output #1388 expects. Alternative: withhold, as RH7004 does.
- SHARED-BRACE-002: I did not state the brace column for an `if`/`else` that has other code than `else` or `}` in front of it, such as a `case` label. It is a non-goal. Alternative: state it now, which would mean measuring the formatter's layout for that shape.
- SHARED-ATTR-017: the tab case is left out, through its coverage sentence, until question 4 is answered.
- RH5502-003 … RH5531-005: I aligned their wording to SHARED-ATTR-017 rather than leaving "each on its own line". The alternative was to change only the shared requirement.

### Notes for the run

- #1416's body names "`else`, loops". The brace rules check only `if` and `else` bodies (SHARED-BRACE-001), so loops are not in scope.
- Feasibility, for the skeptic and the lead: SHARED-BRACE-002 can no longer be met by `FormatNodeInDocumentAsync` on the whole `if` statement, which is what the fix does today. That call formats the header, the other body, and the body, and inserts the blank line. #1140 (*Structural cause*) records a reverted ~300-line attempt that predicted the brace column. *Special cases do not grow the source* applies to whatever replaces it.
- Questions 1 to 4 decide visible output for #1416, #1627, and #1142. Without the answers, the tests for continuation lines, code behind the body, the trailing whitespace, and tab-indented lists cannot be written. The rest of each spec is written.

### Follow-up candidates

_None._

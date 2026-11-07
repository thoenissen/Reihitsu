Role: spec author
Commit: 7744b14e1c58fd0707a52436d29354f2c9a7c0f6
Verdict: #1243: nothing to clarify; #1383: spec written (clarification); #1149: spec written (clarification); #1391: nothing to clarify; #952: nothing to clarify; #1611: spec written (clarification)

The prompt suggested that FMT-PIPE-014 already requires #1149 and #1383. I did not take that as given and checked it against the specs and today's output (`scripts/format.sh --dry-run` on `7744b14`). The findings are below. No change doc was written: every change states behavior that requirements already in force demand. The skeptic should confirm the one point marked as a widening under *Internal choices*.

## thoenissen/Reihitsu#1243

Nothing to clarify. FMT-BREAK-030 already forbids the line-break phase from inserting a blank line in a gap it keeps, and a joined gap holds no line break. The issue's two inputs are plain bugs against it. Tests tag FMT-BREAK-030 (with FMT-BREAK-017 and FMT-BREAK-018 for the joins).

## thoenissen/Reihitsu#1383

Today's output, re-measured: `int this[int a⏎, int b] => a;` is unchanged. `int this⏎[int a]` leaves `[` at column 4 on its own line. `int this[int a, string b⏎]` keeps the break. `a⏎+= 1;` and `a ??=⏎2;` keep their wraps, with the continuation at the statement column. A comment in front of a collection expression's `]` sits in the `]` column, while one in front of an initializer's `}` sits one level deeper. The attribute-target colon is already resolved on main (FMT-SPACE-016), so I made no spec change for it.

### Changed requirements
- FMT-PIPE-014 — One treatment per syntax family — clarified: the assignment-family bullet names the `=` of declarators, properties, enum members, and parameters (shared with #1149).
- FMT-BREAK-011 — Parameter lists hug their declaration — clarified: indexers are in the list. Their brackets take the place of the parentheses, and the opening bracket stays on the `this` line. New indexer example.
- FMT-BREAK-012 — A list is on one line or one element per line — clarified: the bracketed parameter list of an indexer is included, and "opening parenthesis" now reads "parenthesis or bracket". New indexer example.
- FMT-BREAK-013 — A multi-line element splits its list — clarified: the same holds for an indexer's bracketed parameter list.
- FMT-BREAK-016 — Assignments keep target, operator, and value together — clarified: every compound assignment operator is covered (`+=`, `-=`, `|=`, `<<=`, `??=`). The boundary against binary operators is stated with an example on each side (FC-002). *Why* no longer claims that RH5111 reports the compound forms. New example (shared with #1149).
- FMT-INDENT-006 — A comment on its own line takes the column of the code that follows it — clarified: a comment in front of the closing `]` of a collection expression is indented one level deeper, in the elements' column, as one in front of a closing `}` is. New example.
- `line-breaks.md` and `indentation.md` headers: FMT-PIPE-014 is added to `Uses`.

### Examples for the tests
FMT-BREAK-011 (indexer example), FMT-BREAK-012 (indexer example), FMT-BREAK-016 (the compound-assignment lines of the second example), FMT-INDENT-006 (collection-expression example). The issue's own inputs are also good test inputs. The blank line in front of `// last` comes from FMT-BLANK and is already in the example input, so only the column changes.

### Output questions
_None._

### Internal choices
- FMT-BREAK-013 now names indexers as well. The issue cites only -011 and -012, but FMT-PIPE-014 requires the indexer list to be laid out "as the parameter list of a method is". The alternative was to leave -013 implicit. FMT-BREAK-015 (joining wraps inside an element) already works for indexer lists today (`int this[int⏎a, …]` is joined), so I left its text alone.
- RH5106/RH5107 (indexer lists) and RH5111 (compound assignments) stay unchanged. Under P11 the formatter may do more than the analyzers, and #1383 itself says the RH5106/RH5107 question is a separate one.
- I made no change for the list-pattern `]`. FMT-PIPE-014 names only the `]` of a collection expression, so the list-pattern case is a follow-up candidate (below).

### Follow-up candidates
See *Whole run*.

## thoenissen/Reihitsu#1149

Today's output: `enum E { A⏎    = 1, }` keeps the break. The value placement (`B =⏎ 2` → `B = 2`) and the parameter default (`int a⏎= 1` → `int a = 1`, joined by FMT-BREAK-015) already work. Only the `=` of an enum member below its name is missing.

### Changed requirements
- FMT-PIPE-014 — clarified (see #1383): the assignment family names the `=` of an enum member and of a parameter default. In the closing paragraph, these are laid out "as the initializer of a variable is".
- FMT-BREAK-016 — clarified: an enum member's value and a parameter's default value are named among the constructs. Example: `A⏎    = 1,` → `A = 1,`.
- RH5111-001 — An `=` below its target is reported — clarified: enum members are named, and the target of an enum member ends with its name. The analyzer already reports this shape (`OnEnumMemberDeclaration` → `CheckAssignment`, which checks both rules); the spec just never said so. RH5111-002 is reworded to match ("an enum member with a value included"). The rule page is unchanged because the user-visible reporting has not changed.

### Examples for the tests
FMT-BREAK-016 (the enum `A⏎= 1` line of the second example), RH5111-001 (the new enum member in the example). For the issue's "fix clears the diagnostic in one application", the RH5111-004 test input `enum E { A⏎    = 1, }` should get a fixed output of `A = 1` (FMT-STRUCT-019 removes the trailing comma).

### Output questions
_None._

### Internal choices
- **This is the widening the skeptic should check.** Before this change, FMT-PIPE-014's family bullet read "every assignment operator: `=` and every compound assignment operator". Strictly read, that names neither the equals-value `=` of an enum member nor that of a parameter. I treat the case as already required for two reasons. First, RH5111-004 already requires the RH5111 fix to format an enum member "as the formatter formats it in place, which joins the `=` to the line of the target … (FMT-BREAK-016)", and the RH5111 analyzer already reports `A⏎= 1`. The specs taken together therefore already demand the join, and a formatter that keeps the break leaves a fix that never converges. Second, the triage comment on the issue (P5-2) says the same. The alternative would be a change doc.

### Follow-up candidates
See *Whole run*.

## thoenissen/Reihitsu#1391

Nothing to clarify. FMT-INDENT-013 already names lists from FMT-INDENT-016, so a line inside a base-type element (`System. // c⏎ICloneable`) aligns with the element's first token. FMT-INDENT-012 already places a closing delimiter that starts a line, for any list that spans several lines, the empty list `M( // c⏎);` included. The omitted-type-argument and lone-CR instances are resolved on main and need only regression tests.

## thoenissen/Reihitsu#952

Nothing to clarify. FMT-INDENT-009 places every non-region directive at column 0 without exception. The triage says this is resolved on main, so only the regression test with the issue's input is missing.

## thoenissen/Reihitsu#1611

Today's output, re-measured. With four U+00A0 in front of the closing quotes (opening quotes at column 16), the formatter prepends 16 spaces, which puts the closing quotes at column 20. With twenty U+00A0 it also prepends 16 spaces, which puts them at column 36. The spec never said how the closing marker's current column is measured, nor what a leftward shift removes.

The issue cites FMT-RAW-009 for the alignment, but the requirement is FMT-RAW-001; FMT-RAW-009 covers text nested in a hole. The issue also says "tabs are not part of this because the formatter measures after tab replacement". That is wrong: tab replacement never touches literal text (FMT-TAB-003). Tabs stay out of scope because of the existing FMT-RAW non-goal (thoenissen/Reihitsu#1432), not because of tab replacement.

### Changed requirements
- FMT-RAW-001 — The closing marker starts under the opening quotes — clarified: the column before the move is the width of the whitespace in front of the closing marker. Every whitespace character counts as one column (space, U+00A0, U+2003, vertical tab, form feed), as RH5202 and its fix measure it (`RawStringLiteralUtilities.GetLeadingWhitespaceLength`). Tabs stay a non-goal (FMT-TAB-003 is added to `Uses`).
- FMT-RAW-002 — Relative indentation inside the literal is kept — clarified: a rightward shift adds spaces in front of a line. A leftward shift removes that many leading whitespace characters, whatever they are, and all of them from a line that has fewer. Whitespace characters the shift does not remove stay as written. New example with `·` = U+00A0: the first literal moves right and ends as 12 spaces plus 4 NBSP; the second moves left and ends as 16 NBSP.

### Examples for the tests
FMT-RAW-002 (NBSP example; replace `·` with U+00A0 in the test source) and FMT-RAW-001. The issue asks for LF and CRLF variants. A test should also run the RH5202 analyzer over the formatted output and confirm no diagnostic remains.

### Output questions
_None._ The rightward case (12 spaces + 4 NBSP) and the leftward case (NBSP removed) could each, in principle, be read another way: replacing the NBSP with spaces, or leaving the literal unaligned. Existing requirements already decide both. RH5202-003 has the fix add spaces or remove leading whitespace by the same measure. SHARED-FIX-007 requires fix output and formatter output to agree. FMT-RAW-001 requires the closing marker to end up in the opening column. Replacing the NBSP would make the formatter rewrite the RH5202 fix's output, and leaving the literal unaligned violates FMT-RAW-001. So these are clarifications, not open questions.

### Internal choices
- FMT-RAW-002 states the leftward removal of non-space whitespace. Today's code rewrites only leading spaces, so this is the part of the fix that changes code beyond the measurement. The alternative was to state only the measurement in FMT-RAW-001. I rejected it because the leftward case would then stay unspecified, while FMT-RAW-001 already demands it.

### Follow-up candidates
_None._

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed (spec structure, consistency, IDs). No new requirement IDs were issued, so `requirement-ids.txt` is unchanged. No rule page was changed.

### Non-goals (for every issue of the run)
- RH5106/RH5107 reporting indexer parameter lists, and RH5111 reporting compound assignments (P11; separate questions).
- The `=` of an anonymous-object member and of a using alias, and the `]` of a list pattern (see the follow-up candidates).
- Tabs in a raw string's indentation (thoenissen/Reihitsu#1432).

### Follow-up candidates

**1 · Formatter · `=` of an anonymous-object member below its name** (outside the run; FMT-PIPE-014's assignment family / FMT-BREAK-016 do not name a `NameEquals` `=`).

Input:
```csharp
class C
{
    void M()
    {
        var o = new { X
                          = 1 };
    }
}
```

Today (`7744b14`):
```csharp
        var o = new
                {
                    X
        = 1
                };
```

A (recommended): `X = 1` on one line, as FMT-BREAK-016 does for an object-initializer member:
```csharp
        var o = new
                {
                    X = 1
                };
```

B: keep the wrap and indent the continuation under the member (`= 1` one level deeper than `X`).

Why A: the anonymous-object member is the same shape as an object-initializer assignment, which FMT-BREAK-016 already joins. Today's `= 1` at the statement column reads as a new statement.

**2 · Formatter · `=` of a using alias below its name** (outside the run).

Input:
```csharp
using Alias
    = System.String;
```

Today: `= System.String;` moves to column 0, on its own line.

A (recommended): `using Alias = System.String;`

B: keep the wrap, indented one level.

Why A: a continuation at column 0 reads as a new directive, and the alias `=` assigns a name exactly as a declarator's `=` does.

**3 · Formatter · comment in front of the closing `]` of a list pattern** (outside the run; FMT-PIPE-014 names only the collection-expression `]`, and FMT-INDENT-006 now states only that one).

Input / today (`7744b14`; the comment stays in the `]` column):
```csharp
        return x is [
                        1,

                    // last
                    ];
```

A (recommended): the comment one level deeper, in the elements' column (column 24), as for a collection expression.

B: unchanged, in the `]` column.

Why A: FMT-INDENT-025 lays out list patterns and collection expressions alike, and a comment at the bracket's column reads as describing the bracket rather than the contents.

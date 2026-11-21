# Comments and directives behind an assignment operator inside a switch arm or an anonymous-object member are unsupported

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-UNSUP-019 (new), FMT-UNSUP-011, FMT-BREAK-016, FMT-INDENT-034, RH2001-007, RH7101-007 |
| Depends on | [continuation-indent.md](continuation-indent.md) |

## What

A comment, a directive, or disabled text between the operator of an assignment expression (`=`, `+=`, `??=`, and every other compound assignment operator) and its right operand is a shape the formatter refuses as a whole when the assignment stands inside a switch-expression arm or an anonymous-object member declarator (FMT-UNSUP-019). The CLI leaves such a file unchanged and reports it, and every code fix whose formatted node holds the shape is withheld (SHARED-FIX-010).

The same comment outside an arm or a member — at statement level, in an argument, in an element of a collection expression — stays supported, and the value behind it is a continuation line (FMT-INDENT-034, [continuation-indent.md](continuation-indent.md)).

This change covers the instance of thoenissen/Reihitsu#953 in which a value kept behind such a comment stands inside a switch arm or an anonymous-object member.

## Why

The value behind the comment cannot be joined onto the operator's line (FMT-BREAK-016, FMT-BREAK-001), so it starts a continuation line. Inside a switch arm or an anonymous-object member, whose lines stand to the right of the statement, the run asked which column that line takes: from the line of the arm or member, or from the statement's line. The maintainer chose neither and decided to refuse the input instead (batch 2, answer 8). A compound assignment inside an arm or an anonymous-object member is rare, and a comment behind its operator rarer still; the comment belongs above the statement.

## Visible output changes

Every *before* below is the output of `main`, produced with `scripts/format.sh`. The CLI's line has the form it prints today for the other unsupported shapes.

Input:

```csharp
class C
{
    object M(int x, int y)
    {
        var w = new
                {
                    A = x += // c1
                        3
                };

        return y switch
               {
                   1 => x -= /* c2 */ 2,
                   _ => w,
               };
    }
}
```

Before (`main`): `3` moves to the column of the statement.

```csharp
class C
{
    object M(int x, int y)
    {
        var w = new
                {
                    A = x += // c1
        3
                };

        return y switch
               {
                   1 => x -= /* c2 */ 2,
                   _ => w,
               };
    }
}
```

After: the file is unchanged, and the CLI reports

```
error: <file>(7,30): unsupported syntax: a comment or directive behind an assignment operator inside a switch arm or an anonymous-object member. The file was left unchanged.
```

A comment behind the operator on one line, such as `/* c2 */`, which `main` formats as written, refuses the file the same way. A comment in front of the operator (`A = x /* c */ += 3`) or inside the value behind its first token is formatted as on `main`.

Code fixes: every code fix whose formatted node holds such an arm or member with the comment is withheld — the RH7101 fix for any such comment in the type it formats, and the RH2001 fix for one in the field it builds, such as `private object Value { get; } = new { A = s_count += /* three */ 3 };`. A fix that formats a node inside the arm or the member without the arm or the member, such as the RH5302 fix of a condition inside an arm, does not hold the shape (FMT-UNSUP-003) and stays offered. No rule reports a comment behind an assignment operator.

No rule page changes: rule pages never describe the formatter.

## Special cases

### Deleted

None. The declaration replaces a decision rather than a guard: the column of a value kept behind such a comment inside an arm or a member, which the maintainer declined to choose. FMT-INDENT-034's measure from the nearest construct stays, because it still places the same line in an element of a collection expression or an initializer, and the hole of a verbatim interpolated string inside an arm.

### Checked and kept

- **FMT-BREAK-001's refusal in FMT-BREAK-016's join behind an assignment operator** — a comment behind an assignment operator outside an arm or a member is still supported, so the join still refuses it there.
- **FMT-INDENT-034's nearest construct** — see *Deleted*.

## Decisions

### Refuse instead of choosing a column (maintainer, batch 2, answer 8)

The run asked whether the line takes its column from the arm's or member's line or from the statement's line; the maintainer answered that the shape is unsupported. The shape is written around the assignment operator, the gap whose comment leaves the line, and limited to the two constructs the question named.

### The arm or member is found by walking up to the nearest statement or member declaration

An assignment inside a lambda's block inside an arm stands in a statement, whose continuation is measured from the statement's own line like any other; the walk stops there, so only an assignment whose nearest statement-like ancestor is the arm or the member counts. The walk is a cheap syntactic check.

### On any line

As for every other shape of the unsupported-syntax spec, the boundary is a place in the syntax tree, so a comment behind the operator on one line is refused like one with a line break.

### One reason for the shape

The reason names the operator and the two constructs, so the CLI's line tells the author where the comment stands, as the reasons of the other shapes do (maintainer, batch 2, answer 9).

## Alternatives considered

- **Measure the line from the arm's or member's line** — rejected by the maintainer (batch 2, answer 8).
- **Measure the line from the statement's line** — rejected by the maintainer (batch 2, answer 8); it puts the line left of the construct it belongs to.
- **Refuse a comment behind every assignment operator, wherever the assignment stands** — not chosen: the maintainer's answer names the arm and the member, and the same line at statement level is the compound-assignment instance of thoenissen/Reihitsu#953, which keeps the continuation indent.

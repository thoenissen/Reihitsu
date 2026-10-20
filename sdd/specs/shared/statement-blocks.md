# Shared — Statement blocks

| | |
|---|---|
| ID prefix | SHARED-BLOCK |
| Status | Active |
| Code owners | `StatementBlockParentPolicy` |
| Uses | — |

This spec covers which blocks the rules about the layout of a statement's block check. Each rule that uses this spec states what it reports in such a block.

## Non-goals

- What a rule reports in a block it checks — each rule's spec, such as RH5402 and RH5403.
- Unbraced bodies — SHARED-BRACE-001.

## Requirements

### SHARED-BLOCK-001 — The block of a control-flow statement is checked

A rule that uses this requirement MUST check a block that is the body of an `if`, an `else`, a `while`, a `do`, a `for`, a `foreach` (a deconstructing `foreach` included), a `using`, a `lock`, or a `fixed` statement. It MUST NOT check any other block: a block nested directly in a block, the blocks of a `try` statement and its `catch` and `finally` clauses, a `checked`, `unchecked`, or `unsafe` block, a block in a switch section, and the body of a method, a constructor, an accessor, a local function, a lambda, or an anonymous method.

Why: These statements are followed by a body that belongs to them, and the rules keep that body visibly attached to its statement. The other blocks are laid out by the rules for members, lambdas, and switch sections, or are not checked at all.

Example — RH5402, which reports an opening brace on the line of its statement:

```csharp
class C
{
    void M(bool a, int[] v)
    {
        while (a) {|RH5402:{|}
            M(a, v);
        }
        foreach (var (x, y) in new (int, int)[0]) {|RH5402:{|}
            M(a, v);
        }
        lock (v) {|RH5402:{|}
            M(a, v);
        }
        try {
            M(a, v);
        } finally {
            M(a, v);
        }
        System.Action f = () => {
            M(a, v);
        };
    }
}
```

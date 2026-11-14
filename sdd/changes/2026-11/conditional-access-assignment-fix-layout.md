# The RH5111 fix lays out an assignment inside a conditional access as the document does

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH5111 (one requirement added for an assignment that is the branch of a conditional access; its ID is assigned when the spec is written, after the maintainer's answer) |
| Depends on | [Every node-level root is detached](../2026-10/node-root-detached.md) |

## What

An assignment whose target continues a conditional access — `a?.B.C = 5`, where Roslyn makes `.B.C = 5` the branch behind `?` — is the node the RH5111 fix formats. When the target's member stands on its own line behind the conditional access (`a?.B⏎  .C =⏎    5;`), the fix lays out that member as the outer chain is laid out in the document: in the column whole-document formatting gives it, under the `?` that opens the chain's first conditional access, and joined to an element binding (`a?[0]⏎  .C`) where whole-document formatting joins it. Today the fix keeps the member's line break in every case and puts the member under the dot of the binding the assignment starts with.

Whole-document formatting, the RH6023 fix, RH5201, and RH5112 are unchanged. The RH6023 fix changes only the gaps next to the operator (SHARED-OPSPACE-004), so it keeps the member where the author wrote it under every reading of this question; the RH6023 half of thoenissen/Reihitsu#1529 has no output in question.

## Why

[Every node-level root is detached](../2026-10/node-root-detached.md) chose today's output of the RH5111 fix for this input because it "keeps the author's wrap, as whole-document formatting does". Whole-document formatting keeps the wrap but puts the member in another column, and joins it behind an element binding. The fix's result is not what formatting the document gives (SHARED-FIX-007, RH5111-004 "as the formatter formats it in place"), and two layout rules report it: RH5201 reports the member's column, and RH5112 reports the wrap behind the element binding. Their fixes give whole-document formatting's output, which RH5111, RH5112, RH5201, and RH6023 all leave silent.

Whole-document formatting aligns the member under the `?` for every link of such a chain, call or not: `a?.B⏎  .N();` gives `.N()` in the same column as `.C =` below. The fix's column, under the dot of the last binding, is used by no other layout.

## Visible output changes

Pending the maintainer's answer to the output question of thoenissen/Reihitsu#1529. The changes below are those of the recommended reading (B in the run's question).

### 1. A member wrapped behind a conditional access

```csharp
// input
class A
{
    public A B;
    public int C;
}

class D
{
    void M(A a)
    {
        a?.B
          .C =
            5;
    }
}
```

```csharp
// before: RH5111 fix
        a?.B
          .C = 5;
```

```csharp
// after: RH5111 fix
        a?.B
         .C = 5;
```

### 2. A member wrapped behind an element binding

```csharp
// input
class A
{
    public A this[int i] => null;
    public int C;
}

class D
{
    void M(A a)
    {
        a?[0]
          .C =
            5;
    }
}
```

```csharp
// before: RH5111 fix
        a?[0]
          .C = 5;
```

```csharp
// after: RH5111 fix
        a?[0].C = 5;
```

### 3. A member wrapped behind a nested conditional access

```csharp
// input
        a?.B?.B
            .C =
            5;
```

```csharp
// before: RH5111 fix — `.C` moves to column 13, under the dot of the last binding
        a?.B?.B
             .C = 5;
```

```csharp
// after: RH5111 fix — `.C` in column 9, as whole-document formatting puts it
        a?.B?.B
         .C = 5;
```

Unchanged: an assignment whose member stands on the conditional access's line (`a?.B.C =⏎    5;` gives `a?.B.C = 5;`), and an assignment to a bare binding (`a?.B =⏎    a;` gives `a?.B = a;`).

## Decisions

### Which layout the fix and whole-document formatting share

Open: the maintainer's answer to the output question of thoenissen/Reihitsu#1529. The doc is written on the recommended reading; it is rewritten if another is chosen.

## Alternatives considered

- **Whole-document formatting adopts the fix's layout** (A in the issue) — not recommended: it changes the formatter's output for every assignment inside a conditional access with a wrapped member (`.C` in column 10 instead of 9, `a?[0]⏎  .C = 5;` kept wrapped), makes an assigned link lay out differently from a called link of the same chain (`a?.B⏎ .N();`), and needs RH5201 and RH5112 to stop reporting the new output, which they report today.
- **Leave both outputs as they are** (C in the issue) — not recommended: the RH5111 fix keeps producing code that RH5201 or RH5112 reports, against SHARED-FIX-007, and the disagreement would need a written exception.

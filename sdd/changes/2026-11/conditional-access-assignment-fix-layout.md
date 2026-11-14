# The RH5111 fix lays out an assignment inside a conditional access as the document does

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH5111-006 (added), FMT-PIPE-024 (changed: the conditional-access-branch fact removed) |
| Depends on | [Every node-level root is detached](../2026-10/node-root-detached.md) |

## What

An assignment whose target continues a conditional access — `a?.B.C = 5`, where Roslyn makes `.B.C = 5` the branch behind `?` — is the node the RH5111 fix formats. When a link of the target stands on its own line (`a?.B⏎  .C =⏎    5;`), the fix lays out the chain as formatting the document lays it out, from the chain's root in front of the `?` on (RH5111-006): a wrapped link starts in the column of the chain's anchor (SHARED-CHAIN-007), under the `?` of `a?.B` or the `.` of `a.B?.B`, and a link that is the chain's first link joins the line of the root, as `.C` does behind the element binding `a?[0]` (SHARED-CHAIN-003). Today the fix keeps every line break of the target and puts each wrapped link under the assignment's own first token, whatever column the author wrote it in.

Whole-document formatting, RH5201, and RH5112 are unchanged; they already give, or accept, the new output. The RH6023 fix changes only the gaps next to its operator (SHARED-OPSPACE-004) and keeps the member where the author wrote it; its half of thoenissen/Reihitsu#1529 has no output in question and is unchanged.

## Why

[Every node-level root is detached](../2026-10/node-root-detached.md) chose today's output of the RH5111 fix for this input because it "keeps the author's wrap, as whole-document formatting does". Whole-document formatting keeps the wrap but puts the member in another column, and joins it behind an element binding. The fix's result is not what formatting the document gives (SHARED-FIX-007, RH5111-004 "as the formatter formats it in place"), and two layout rules report it: RH5201 reports the member's column, and RH5112 reports the wrap behind the element binding. Their fixes give whole-document formatting's output, which RH5111, RH5112, RH5201, and RH6023 all leave silent.

The fix does not keep the author's column either: it moves a wrapped link to the column of the assignment's first token from any column, also from the column whole-document formatting gives it, where RH5201 is silent, into a column where RH5201 reports it (visible output change 2).

Whole-document formatting aligns the member under the anchor for every link of such a chain, call or not: `a?.B⏎  .N();` gives `.N()` in the same column as `.C =` below. The fix's column, under the assignment's first token, is used by no other layout.

## Visible output changes

The class `A` below holds the members every input uses:

```csharp
class A
{
    public A B;
    public A this[int i] => null;
    public int C;
}
```

Every input stands in `void M(A a)` of a class `D`, with `a` in column 8. Each *before* is the RH5111 fix on `main`, the same under LF and CRLF; each *after* is whole-document formatting of the same input on `main`.

### 1. A member wrapped behind a conditional access

```csharp
// input
        a?.B
          .C =
            5;
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

### 2. A member already in the column whole-document formatting gives it

```csharp
// input
        a?.B
         .C =
            5;
```

```csharp
// before: RH5111 fix — `.C` moves from column 9 to column 10, where RH5201 reports it
        a?.B
          .C = 5;
```

```csharp
// after: RH5111 fix — `.C` stays in column 9
        a?.B
         .C = 5;
```

### 3. A member wrapped behind an element binding

```csharp
// input
        a?[0]
          .C =
            5;
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

### 4. A member wrapped behind a nested conditional access

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

### 5. Two wrapped links, and a receiver with a member access in front of its `?`

```csharp
// input
        a?.B
          .B
          .C =
            5;
        a.B?.B
           .C =
            5;
```

```csharp
// before: RH5111 fix
        a?.B
          .B
          .C = 5;
        a.B?.B
            .C = 5;
```

```csharp
// after: RH5111 fix
        a?.B
         .B
         .C = 5;
        a.B?.B
         .C = 5;
```

Unchanged: an assignment whose member stands on the conditional access's line (`a?.B.C =⏎    5;` gives `a?.B.C = 5;`), an assignment to a bare binding (`a?.B =⏎    a;` gives `a?.B = a;`), and an assignment that is not behind a `?` (`x.B⏎  .C =⏎    5;` gives `.C = 5;` in column 9, as today).

## Decisions

### Which layout the fix and whole-document formatting share

Decided by the maintainer (output question 1 of the run for thoenissen/Reihitsu#1529, answer B): the RH5111 fix adopts whole-document formatting's layout — a wrapped link under the anchor in column 9 in the simple and the nested case, and the member joined behind an element binding (`a?[0].C = 5;`). Whole-document formatting, RH5201, and RH5112 stay as they are.

This supersedes, for the RH5111 fix, the maintainer's earlier choice recorded in [Every node-level root is detached](../2026-10/node-root-detached.md) under *A wrapped member of an assignment inside a conditional access keeps its line* (repair round 2), whose output `a?.B⏎  .C = 5;` rested on the premise that it matches whole-document formatting. What that choice decided against — `main`'s join of the member to `a?.B` — stays rejected: the member keeps its line break wherever whole-document formatting keeps it. That doc is history and is not edited (`sdd/README.md`, *Change docs*); its output for the RH6023 fix is unaffected, because that fix no longer formats the assignment (SHARED-OPSPACE-004).

### The shape is not declared unsupported

The maintainer chose neither to withhold the fix for this shape (D) nor to declare a conditional access whose branch is an assignment with a wrapped target unsupported (E). The CLI keeps formatting such files, and the RH5111 fix stays offered on them.

### The requirement belongs to RH5111

The requirement is RH5111-006 rather than an example under SHARED-FIX-007 or a requirement of `shared/fluent-chain-layout.md`: only the RH5111 fix formats an assignment that is the branch of a conditional access with a wrapped link. The chain fixes (RH5112, RH5201) format the outermost chain node, whose layout SHARED-CHAIN-008 already decides, and the RH6023 fix formats nothing. RH5111-006 references SHARED-CHAIN-003 and SHARED-CHAIN-007 for the layout instead of restating it.

### The conditional-access-branch fact goes

`FormatNodeInDocumentAsync` gave the phases a fact about whether its root is the part of a conditional access behind its `?` (FMT-PIPE-024, *Conditional-access branch*), so that an assignment passed as the root could continue the chain of its left side ([Every node-level root is detached](../2026-10/node-root-detached.md), audit stage). Its callers were the RH5111 and RH6023 fixes. The RH6023 fix no longer formats (SHARED-OPSPACE-004), and RH5111-006 formats such an assignment through `FormatNodeInDocumentWithContextAsync` with the chain as context, which supplies no chain facts. No caller of `FormatNodeInDocumentAsync` passes such an assignment any more, so the fact is removed from FMT-PIPE-024, and the requirement states that it does not decide that root (`AGENTS.md`, *Supported inputs*). The chain-continuation fact stays: the RH5301 fix formats an object creation that can be the expression of a member access. No output changes.

## Alternatives considered

- **Whole-document formatting adopts the fix's layout** (A) — rejected: it changes the formatter's output for every assignment inside a conditional access with a wrapped member (`.C` in column 10 instead of 9, `a?[0]⏎  .C = 5;` kept wrapped), makes an assigned link lay out differently from a called link of the same chain (`a?.B⏎ .N();`), and needs RH5201 and RH5112 to stop reporting the new output, which they report today.
- **Leave both outputs as they are** (C) — rejected: the RH5111 fix keeps producing code that RH5201 or RH5112 reports, against SHARED-FIX-007, also from input those rules accept (visible output change 2), and the disagreement would need a written exception.
- **Withhold the RH5111 fix for this shape** (D) — rejected by the maintainer: the diagnostic would stay without a fix on a C# 14 null-conditional assignment with a wrapped target, where B gives a fix whose output nothing reports.
- **Declare the shape unsupported** (E) — rejected by the maintainer: the CLI would refuse whole files that whole-document formatting handles correctly today without a special case, to delete a node-level fact, which B removes as well (*The conditional-access-branch fact goes*).

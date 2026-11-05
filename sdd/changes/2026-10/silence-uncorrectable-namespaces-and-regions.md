# RH7003 and RH7304 do not report what cannot be corrected as they ask

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | RH7003-005 and RH7304-005 (added by this change) |

## What

Two organization rules stop reporting a construct for which the correction they ask for does not exist:

- **RH7003** no longer reports a block-scoped namespace that declares a namespace among its members (RH7003-005). Examples are `namespace A.B { namespace Inner { … } }` and `namespace A { namespace B { } namespace C { } }`. RH7003-001 already excluded such a namespace from what it requires to be reported; the analyzer reported it anyway.
- **RH7304** no longer reports either directive of a `#region`/`#endregion` pair when one of the two, or a region directive between them, lies inside executable code (RH7304-005). Examples are a type-level region that holds a region of a method body, and a type-level `#region` closed inside a method. RH7304-001 already required a report only for pairs that lie wholly between declarations; the analyzer also reported such pairs and offered a fix for them.

## Why

RH7003 asks for a file-scoped namespace, but a file cannot hold a file-scoped namespace together with a block-scoped one (CS8955). With one namespace inside, conversion takes merging the names. With several, no file-scoped form exists. The diagnostic asked for code that does not compile.

RH7304 aligns a region's directives with the region's first code. A region that reaches into a body, or holds a region of a body, has no such level, and the directive inside executable code is already RH7303's finding. Reporting the pair, and indenting it, aligns a region whose placement is the real problem.

## Visible output changes

The RH7304 "before" was measured with `scripts/apply-fix.sh RH7304` on the code of `main` at `a59a003`, under LF and CRLF alike. RH7003 has no code fix; its "before" is the report the skeptic of this change confirmed from `RH7003FileScopedNamespacesShouldBeUsedAnalyzer` on `735d0db`. Neither rule page changes.

### 1. RH7003 — a namespace that contains a namespace

```csharp
// input
namespace A.B
{
    namespace Inner
    {
        class C
        {
        }
    }
}
```

Before: RH7003 is reported on `A.B`. After: RH7003 is not reported. The same holds for a namespace that declares several namespaces, such as `namespace A { namespace B { } namespace C { } }`.

### 2. RH7304 — a region pair with a region directive inside executable code

```csharp
// input
class C
{
#region Outer
    void M()
    {
        #region Inner
        M();
        #endregion
    }
#endregion
}
```

```csharp
// before: both directives of Outer are reported, and the fix indents them
class C
{
    #region Outer
    void M()
    {
        #region Inner
        M();
        #endregion
    }
    #endregion
}
```

After: neither directive of `Outer` is reported, and no fix is offered. The same holds for a pair one of whose directives lies inside executable code, such as a type-level `#region` closed inside a method. The directives inside the body stay RH7303's findings.

## Decisions

### The maintainer's answers

The maintainer approved the visible output changes of this change, listed above, after `main` was merged into the run on 2026-11-05. The maintainer had also reclassified the run as a behavior change, which allows this change doc although the triage of the two issues ruled one out.

### Silence, not a narrower message

Both rules simply do not report these shapes. Neither rule has a correct correction to offer the user, and the shapes are reported elsewhere (RH7303) or follow from the file's structure.

### The rule pages stay as they are

The RH7003 page already says that the rule reports only a namespace that can actually be converted. Rule pages do not list exceptions, so neither page changes.

## Alternatives considered

- **Merge a single nested namespace into one file-scoped namespace** (`namespace A.B.Inner;`) — rejected for this change: it is a new correction, kept as a non-goal of RH7003 for a possible later feature.
- **Keep reporting the RH7304 pair and only withhold its fix** — rejected: the report itself names a level that does not exist.

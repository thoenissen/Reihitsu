# Open questions collected during #903 (filed under #912 at the end)

## sdd/specs/analyzer/RH7103.md

### Q1 — A member without an accessibility modifier is never compared with an explicitly `private` one
Input:
```csharp
public class C
{
    int _a;
    private static int _b;
    private int _c;
    static int _d;
}
```
Output: only `_d` reported (after `_a`); `_b` after `_a` not reported. Fix moves `_d` before `_a`.
Why wrong: `int _a` and `private int _c` are both private; `GetAccessibilityGroup` maps no modifier to `None`, so the effective accessibility is not compared. Same effect: a static constructor (no modifier allowed) is never compared with an instance constructor that has one.
Touches: RH7103-001, RH7103-003.

### Q2 — The fix reformats the entire containing type, including structural rewrites of unrelated members
Input:
```csharp
public class D
{
    public class Inner
    {
    }

    public static class Helpers
    {
    }

    public static D operator +(D a, D b) => a;
}
```
Output after the RH7103 fix: `Helpers` moved, and also `class Inner;` / `class Helpers;` (semicolon bodies) and the operator turned into a block body; `int  X  {get;set;}` and `var y=1;` elsewhere in the type are reformatted too.
Why it looks wrong: the fix calls `ReihitsuFormatter.FormatNodeInDocumentAsync` on the whole type, so a member move changes members the diagnostic is not about (RH5412, RH3205 territory). Existing test `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` expects `#endregion // Decoy` and a collapsed property in an unrelated region.
Touches: RH7103-006; applies to every TypeMemberOrderingCodeFixProviderBase fix.

## sdd/specs/shared/ordering-move-safety.md

### Q3 — A balanced `#pragma` pair inside a crossed member's body refuses the move
Input:
```csharp
public class C
{
    public void Run()
    {
    }

    public void Reset()
    {
#pragma warning disable CS0168
        int unused;
#pragma warning restore CS0168
    }

    public static void Create()
    {
    }
}
```
Output: RH7103 reported, no fix. Asserted by `MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsDirective`.
Why it looks wrong: the pair is closed inside `Reset`'s body; `Create` is outside it before and after the move, so the move changes nothing it covers. Conditional and region pairs in the same position are allowed (SHARED-ORDER-006). Guard scope gives away safe ground.
Touches: SHARED-ORDER-005, SHARED-ORDER-006.

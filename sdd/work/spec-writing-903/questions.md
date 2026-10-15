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

## sdd/specs/analyzer/RH4106.md

### Q4 — An acronym at the start of a PascalCase name becomes `_hTTPClient`
Input:
```csharp
public class C
{
    private int HTTPClient;
    private int XMLParser;
}
```
Output of the RH4106 fix: `_hTTPClient`, `_xMLParser` (harness run; `ToUnderlineCamelCase` lowercases only words that are entirely uppercase, and `HTTPClient` is one word).
Why it looks wrong: the result is itself not what a person would write and reads worse than the original; `URL` → `_url` and `FIRST_NAME` → `_firstName` show the intent to normalize acronyms. Shared by every casing fix through `CasingUtilities` (ToCamelCase, ToPascalCase likely behave the same — not checked for every rule).
Touches: RH4106-004.

## sdd/specs/analyzer/RH5002.md

### Q5 — The fix inserts two blank lines when a block comment shares the `if` line
Input:
```csharp
class C
{
    void M(int a)
    {
        a++;
        /* block */ if (a > 7) { }
    }
}
```
Output of the RH5002 fix (harness run): two blank lines between `a++;` and `/* block */ if …`. RH5023 then reports the result.
Why it looks wrong: RH5002 asks for one blank line; `ReplaceFormattingOnlyGap` adds two line breaks while the previous token keeps its own trailing line break (`TokenGapAnalysis.RequiredLineBreakCountForBlankLine` returns 2 here). Affects every rule on `StatementShouldBePrecededByABlankLineCodeFixProviderBase`.
Touches: RH5002-005, SHARED-BLANK-001.

## sdd/specs/analyzer/RH6002.md

### Q6 — Whitespace around the comma of a compact form is never reported
Input:
```csharp
using System.Collections.Generic;

class C
{
    int[ , ] g2 = null;
    object t2 = typeof(Dictionary< , >);
}
```
Output: RH6002 reports nothing (`CommaSpacingUtilities.IsSpacingExempt` skips the comma entirely, asserted by `VerifyArrayRankCommasDoNotProduceDiagnostics` only for the unspaced form). RH6007/RH6008 report the bracket sides of `[ , ]`; for `< , >` only RH6012 reports the space before `>`, so after every available fix `Dictionary< ,>` remains without a diagnostic. The formatter writes `int[,]` and `Dictionary<,>` (FMT-SPACE-004, FMT-SPACE-008).
Why it looks wrong: the exemption is meant to forbid the space after the comma, but it also switches off the before-side check (RH6002-001) and the after-side check for these forms, so analyzer and formatter disagree on `< , >`.
Touches: RH6002-001, RH6002-003, FMT-SPACE-004, FMT-SPACE-008.

## sdd/specs/analyzer/RH2005.md

### Q7 — `static public` loses its indentation
Input:
```csharp
public class Sample
{
    static public int Counter;
}
```
Output of the RH2005 fix (harness run): `private     static int Counter;` at column 1 — `private` is inserted at index 0 without the indentation, and `static` keeps the indentation that was its leading trivia.
Why it looks wrong: `DeclarationModifierUtilities.AddAccessibilityModifiers` takes the leading trivia from the old accessibility token, which is not the first modifier here. The result is valid C# but not laid out, and the fix does not run the formatter.
Touches: RH2005-003.

### Q8 — The fix breaks every access from outside the type
Input:
```csharp
public class Sample
{
    public int Value;
}

public class User
{
    public int Read(Sample s) => s.Value;
}
```
Output of the RH2005 fix: `private int Value;`, and the project no longer compiles: CS0122 at `s.Value`.
Why it looks wrong: CLAUDE.md "Code-fix coverage" prefers no fix over an unsafe one. The fix is offered regardless of references outside the type (in the same project or, for a public type, in consumers). Options: withhold the fix when a reference outside the containing type exists (needs a symbol search), or accept that the fix is a deliberate API break and say so on the rule page.
Touches: RH2005-003.

## sdd/specs/analyzer/RH1003.md

### Q9 — Numeric additions inside a chain are counted as string parts
Input:
```csharp
class C
{
    void M(int x, int y)
    {
        var e = x + y + "a";
        var f = "a" + (x + y);
    }
}
```
Output: both reported by RH1003 (harness run).
Why it looks wrong: each is a two-part concatenation — `(x + y)` is an `int` addition that runs before the string concatenation — and RH1003 ignores two-part concatenations. `EnumerateChainSegments` flattens every `AddExpression` regardless of its type, so the numeric operands count as parts. Rewriting `"a" + (x + y)` as `$"a{x}{y}"` would also change the result, so the diagnostic suggests an incorrect rewrite.
Touches: RH1003-001.

## sdd/specs/analyzer/RH0001.md

### Q10 — Content after the root object is accepted
Input `reihitsu.json`:
```json
{ } junk
```
Output: no RH0001 diagnostic (harness run).
Why it looks wrong: the file is not valid JSON, and RH0001 reports invalid JSON syntax everywhere else. `ParseConfiguration` stops reading at the root object's `EndObject`, so the trailing text is never seen by `Utf8JsonReader`.
Touches: RH0001-003.

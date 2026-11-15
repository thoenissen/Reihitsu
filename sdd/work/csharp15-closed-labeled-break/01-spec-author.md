Role: spec author
Commit: 22f54f9
Verdict: #1764: open question (change doc required for the recommended answer); #1765: spec written

How facts were established: a scratch console harness (outside the repository) referencing `Reihitsu.Analyzer` and Roslyn 5.12 ran the analyzers on probe inputs, parsed with `LanguageVersion.CSharp15` (labeled `break`/`continue`, `closed`) and `LanguageVersion.Preview` (`safe`), printed compiler errors, diagnostics, and `ModifierOrderingUtilities.OrderModifiersForRh7105` results. No code fix was run through a workspace; fix results for RH7105 below are the order `OrderModifiersForRh7105` returns.

## thoenissen/Reihitsu#1764

### Findings

- `closed` (`SyntaxKind.ClosedKeyword`) is C# 15 (no preview error); `safe` (`SyntaxKind.SafeKeyword`) needs `LangVersion preview` (CS8652 otherwise). Both get rank 100 in `GetRh7105Rank`, so they sort behind `partial`.
- Reported today (diagnostic on the first misplaced modifier): `public closed partial class A`, `public closed partial record class R`, `public closed unsafe partial class U`, `public new closed partial class N` (nested), `public closed new partial class N`, `public safe partial void M();`, `public safe extern void M();`, `public static safe extern void M();`, `public safe required int P { get; set; }`, `public safe new void Equals() { }`.
- Not reported today: `public closed class A`, `public closed record class A`, `file closed class A`, `public extern safe void M();`, `public partial closed class A` (does not compile, CS0267).
- Fix order today: `public closed partial class` -> `public partial closed class` (CS0267, does not compile); `public safe partial void M();` -> `public partial safe void M();` (CS0267). This violates SHARED-FIX-003 (a compile error the code did not have) on any reading below.
- Compiler facts: `closed` cannot combine with `abstract` (CS9384), `sealed` or `static` (CS9381), and is not valid on `interface`/`struct` (CS0106). It combines with an accessibility, `file`, `new`, `unsafe`, and `partial`. `safe` cannot combine with `unsafe` (CS9388); it combines with `static`, `extern`, `new`, `required`, and `partial` (both partial parts must agree, CS9390).
- RH7105-001 as written puts "every other modifier last", which covers `closed` and `safe` and therefore demands the order `public partial closed class`, which does not compile. Its own `Why:` says `partial` comes last because C# requires it before the type keyword. The spec text and its reason disagree for these two keywords, and every reading gives different visible output, so nothing about `closed`/`safe` was written into `RH7105.md`.

### Changed requirements

None. RH7105-001 and RH7105-004 stay unchanged until the output questions are answered.

- If the maintainer picks **1A**: no requirement changes; the fix is withheld for these inputs because SHARED-FIX-003 already demands it (a bug fix against the shared spec), and RH7105-001 can gain a clarifying sentence naming `closed`/`safe` among "every other modifier".
- If the maintainer picks **1B or 1C** (and any of 2A/2B): RH7105-001's order list changes, which is a changed requirement with new intended behavior, so a change doc is required (`sdd/README.md`, *When a change doc is required*) and the lane widens to behavior change. I have not written it; the lead decides the widening.

### Examples for the tests

None yet; they depend on the answers. The reproduction input every answer agrees on is the issue's own `public closed partial class Gate { }`, which is reported today; every option below changes either the diagnostic or the fix result for it.

### Output questions

**1 · RH7105 · Declaration keywords must follow order — where `closed` stands**

Input:
```csharp
public closed partial class A
{
    public new closed partial class N
    {
    }
}

internal closed unsafe partial class U
{
}
```

Before (`main`): `closed` is reported on `A`, on `N`, and on `U`; the fix writes `public partial closed class A`, `public new partial closed class N`, `internal unsafe partial closed class U`, none of which compiles.

After, A: as today, all three are reported, but no fix is offered (`closed` stays "last", behind `partial`, which no compilable declaration can satisfy).

After, B (recommended): `closed` takes the place of `abstract`/`sealed` (after `static`, before `virtual`, `new`, `unsafe`, `partial`). `A` and `U` are not reported; `N` is reported on `new` and the fix writes:
```csharp
public closed partial class A
{
    public closed new partial class N
    {
    }
}

internal closed unsafe partial class U
{
}
```

After, C: `closed` stands directly in front of `partial` (after `unsafe`, `required`, `ref`). `A` and `N` are not reported; `U` is reported on `closed` and the fix writes:
```csharp
public closed partial class A
{
    public new closed partial class N
    {
    }
}

internal unsafe closed partial class U
{
}
```

Why B: `closed` is an inheritance modifier that replaces `abstract`/`sealed` (it cannot be combined with them), so it stands where they stand, as in `public sealed new class` and `public abstract unsafe class` today.

**2 · RH7105 · Declaration keywords must follow order — where the preview `safe` stands**

Input:
```csharp
public class C
{
    public static safe extern void M();

    public safe partial void P();

    public safe required int R { get; set; }
}
```

Before (`main`): `safe` is reported on all three; the fix writes `public static extern safe void M();`, `public partial safe void P();` (does not compile), and `public required safe int R { get; set; }`.

After, A: as today, all three are reported; the fix is offered for `M` and `R` with today's result and withheld for `P` (`safe` behind `partial` does not compile).

After, B (recommended): `safe` takes the place of `unsafe` (after `extern`, before `required`, `ref`, `partial`). `P` and `R` are not reported; `M` is reported on `safe` and the fix writes:
```csharp
public class C
{
    public static extern safe void M();

    public safe partial void P();

    public safe required int R { get; set; }
}
```

After, C: `safe` stands directly in front of `extern` (after `async`). `P` and `R` are not reported, and neither is `M`:
```csharp
public class C
{
    public static safe extern void M();

    public safe partial void P();

    public safe required int R { get; set; }
}
```

Why B: `safe` and `unsafe` are alternatives that cannot be combined, so a declaration that switches from one to the other keeps its order, as `public static extern unsafe` reads today. (Pick C if the maintainer prefers the order the memory-safety proposal writes, `safe extern`.)

Note for the lead: answering 1A also implies 2A (both keep "every other modifier last"). The input in question 1 holds code that compiles only with `/unsafe`; question 2 needs `LangVersion preview`.

### Internal choices

- No clarifying sentence naming `closed`/`safe` was written into RH7105 now, because every reading changes the order list or its meaning for these keywords; the alternative (writing "`closed` and `safe` sort last" as current behavior) would specify the looks-wrong output.

### Follow-up candidates

_None._ (#1338, other RH7105 modifier lists, is already filed and was not checked.)

## thoenissen/Reihitsu#1765

### Findings (probes on `main` at 22f54f9, C# 15)

RH5011 reports every labeled `break` (`break outer;`, also `break sw;` naming the enclosing switch) whose statement is followed by:
- a closing brace of a block or of a switch statement (RH5011-002) — false positive;
- a directive line (RH5011-002) — false positive;
- `else` of an `if` (`if (a > 0) break outer; else a++;` and on separate lines) or the `while` of a `do` (`do break outer; while (a > 2);`) (RH5011-008) — false positive;
- a blank line and a statement (RH5011-001) — false positive;
- a statement or the next switch section without a blank line — reported, as RH5011-001 requires.

RH5011 code fix behavior on labeled `break` (RH5011-004 to -007) was not run.

Correct today: RH5010 (exempts a labeled `break` that is the last statement owned directly by its section, reports one inside a section's block or in the middle of a section); RH5012 (labeled `continue` reported/not reported as unlabeled); RH7501 (reports a labeled `break` ending a section's block, also `break inner;` naming the switch; does not report a labeled `continue`). RH5009 only reports `goto` and is untouched by labeled `break`/`continue`. The formatter was not run.

### Changed requirements

- RH5011-001 — A `break` followed by a statement or a switch section needs a blank line — clarified: a labeled `break` is judged like an unlabeled one; the statement ends at its semicolon and the next token is the one behind it, for every RH5011 requirement.
- RH5011-002 — A `break` before a closing brace or a directive is not reported — clarified: example with labeled `break` before a block's `}`, a switch's `}`, and a directive.
- RH5011-008 — A `break` before a keyword that continues its statement is not reported — clarified: example with labeled `break` before `else` and before a `do` footer.
- RH5010-002 — The terminal `break` of a switch section is exempt — clarified: a labeled `break` is a `break` for RH5010-001/-002; example.
- RH7501-001 — A `break` that ends a section's block is reported — clarified: a labeled `break` (loop label or the switch's own label) is a `break` for RH7501-001, -003, -004; example.
- RH7501-002 — Other statements that end a section's block are not reported — clarified: names a labeled `continue`.

No requirement ID was added, so `requirement-ids.txt` is unchanged; no rule page changes (what is reported and how to fix it is unchanged for the user).

### Examples for the tests

RH5011-002 (labeled example block), RH5011-008 (labeled example block), RH5011-001 (the labeled sentence: a labeled `break` followed by a blank line and a statement is not reported; one followed directly by a statement is), RH5010-002 (labeled example), RH7501-001 (labeled example). Every labeled example block was run through the harness: the RH5010 and RH7501 markup matches today's diagnostics exactly; the RH5011 blocks are reported today (the reproduction). The tests need C# 15 parse options; the harness compiled every block with no error under `LanguageVersion.CSharp15`.

### Output questions

_None._ Considered and not raised: whether a labeled `break` that ends a switch section should count as "the terminal `break`" (RH5010-002/SHARED-BLANK-004, RH7501-001, FMT-SWITCH-002) or like `goto`/`return`. The specs say "a `break` statement", which grammatically includes `break outer;`; the analyzers, and by code reading the formatter's shared predicate (`BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak`), treat it so; and the output (`case 1: { … } break outer;`) does not look wrong. If the skeptic or lead judges it does, it becomes an output question and the RH5010/RH7501 clarifications go back out.

### Internal choices

- The labeled-`break` clarifications are written into the rule specs (RH5010, RH5011, RH7501), not into SHARED-BLANK-004 or FMT-SWITCH-002, because the formatter was not run; alternative: one sentence in SHARED-BLANK-004 covering analyzer and formatter.
- Sentences were added to existing requirements rather than new requirements, because the existing text ("a `break` statement", "the `break` statement's semicolon") already decides these inputs; alternative: a new RH5011-009 "a labeled `break` is judged like an unlabeled one".
- RH5012 and RH5009 are not changed: nothing in them depends on where a `continue` statement ends or on a `break` at all.

### Follow-up candidates

_None._

## Whole run

Files changed: `sdd/specs/analyzer/RH5010.md`, `sdd/specs/analyzer/RH5011.md`, `sdd/specs/analyzer/RH7501.md`, this handover. Prompt carried no intended fix or verdict beyond the lane's expectations; the lead's expectation that #1764 gets a clarification in RH7105 does not hold (see the #1764 verdict).

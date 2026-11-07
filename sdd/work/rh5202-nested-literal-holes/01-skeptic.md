Role: skeptic
Commit: ec97cce
Verdict: output question

The prompt carried no intended fix or hoped-for verdict. It carried the maintainer's suspicion that the issue may already be fixed on `main`; I checked that as a claim, and it does not hold for the code fix (below).

## Premise

The issue is real on `main` (ec97cce) for the RH5202 code fix, and only for the code fix.

- **The code fix still has the defect.** `RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.RealignContent` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs:68-112`) splits the literal's text into lines. It adds `delta` spaces in front of every line after the first, or removes up to `-delta` leading whitespace characters from it. `delta` is the distance between the column of the opening quotes and the column of the closing marker. Nothing checks where a line starts. The file last changed in e713ec8 (2026-10-23, #1421). FMT-RAW-009 came later, in c0944d4 (2026-10-25, #1457), and only for the formatter (`RawStringLineShifts`). So the later raw-string work fixed the formatter and left the fix's private copy as it was.
- **Reproduced with `scripts/apply-fix.sh RH5202 <fixtures>`, under LF and CRLF.** The issue's verbatim input is moved 13 columns: `b"}` goes from column 0 to column 13, so the nested string's value changes from `a⏎b` to `a⏎             b`. The issue's `/* a⏎  b */` input is also moved: `  b */` goes from column 2 to column 15, which changes the comment's text. Both are against SHARED-FIX-003 (third bullet, "change the text of … a literal"). The runner reports `fixed` and `invalid-result 0`, so the result compiles and the bug is silent.
- **Whole-document formatting is correct.** `scripts/format.sh` was run twice over the same fixtures. Every nested-text line kept its column, as FMT-RAW-009 states, and the second pass changed nothing. The CLI and the formatter are not affected.
- **A caller reaches the case.** The caller is the RH5202 code fix (single fix and Fix All), offered on the `InterpolatedStringExpressionSyntax` branch of `RegisterCodeFixesAsync` (line 180). The analyzer reports every fixture below, because its closing marker is not in the column of the opening quotes. The path: a user's misaligned interpolated multi-line raw string has a verbatim string, an interpolated verbatim string, or a multi-line `/* */` / `/** */` comment in a hole that spans lines. The user applies the RH5202 fix in the IDE. No direct method call is involved.
- **Severity.** The issue's severity holds, and is if anything understated. A literal's value changes silently at run time, with no compile error, in both shift directions.

## Mechanism and candidates

Mechanism: the RH5202 fix moves every line after the literal's first line by the closing marker's delta. It does not exclude a line whose start lies inside the text of a nested literal that is not a raw string, or inside a nested comment, that began on an earlier line within an interpolation hole. For such a line, the leading whitespace it adds or removes is part of that literal's value or that comment's text.

Dispatch: `RegisterCodeFixesAsync` registers the fix for two shapes:

- `LiteralExpressionSyntax` with `MultiLineRawStringLiteralToken` / `Utf8MultiLineRawStringLiteralToken`. It has no holes, so every later line is a content line or the closing line, and none is a candidate.
- `InterpolatedStringExpressionSyntax` with `InterpolatedMultiLineRawStringStartToken`, for any number of `$`. This shape holds every candidate.

A line inside a hole can start inside the text of these tokens or trivia (defining Roslyn kind in parentheses):

| Candidate | Kind | Fixture | Fix on `main` | Formatter on `main` |
|---|---|---|---|---|
| verbatim string `@"…"` | `StringLiteralToken` | A1 (right), A3 (left), A9 (`$$`), B2 (empty line inside), B7 (as an argument) | fails | holds |
| UTF-8 verbatim `@"…"u8` | `Utf8StringLiteralToken` | B3 | fails | holds |
| interpolated verbatim `$@"…"` / `@$"…"` | `InterpolatedStringTextToken` of an `InterpolatedVerbatimStringStartToken` string | A5, B4 | fails | holds |
| verbatim string inside a hole of a nested `$@"…"` | as above, two levels | A8 (both the inner and the outer continuation line move) | fails | holds |
| verbatim string inside a multi-line hole of a nested regular `$"…"` | `StringLiteralToken` | B9 | fails | holds |
| `/* */` comment, trailing a token | `MultiLineCommentTrivia` | A2 (right), A4 (left), B6 (`*/}` line) | fails | holds |
| `/* */` comment, leading a token | `MultiLineCommentTrivia` | B1 | fails | holds |
| `/** */` comment in a hole | doc-style comment trivia | B8 | fails | holds |
| multi-line raw string nested in a hole | `MultiLineRawStringLiteralToken` | A6 | holds: the value is kept and the nested literal is fixed by its own diagnostic in a second single fix (FMT-RAW-008 territory, not this class) | holds |
| directive / disabled text in a hole | — | A7 | not reachable: parse error, no diagnostic | — |

## Breaking inputs

Failure classes that reach the changed code:

- FC-015 / FC-016: the shift policy has two owners. #1457 fixed the formatter's copy and left the code fix's copy as it was.
- FC-022: verbatim strings and comment text inside an interpolated raw literal.
- FC-027: nesting depth, and the enclosing construct in the hole.
- FC-003: both shift directions.
- FC-033: the reproduction must go through the code-fix path, under LF and CRLF.

Class matrix, checked against SHARED-FIX-003 (literal value and comment text unchanged) and FMT-RAW-009. Every reachable cell fails today. The fixtures were all run through `apply-fix.sh` under both line endings.

| Axis | Members (all fail unless noted) |
|---|---|
| Shift direction | right (A1, A2, A5, A8, A9, B1–B4, B6–B9); left by fewer columns than the line's leading whitespace (A3, A4). No fix is offered when the closing marker is already in place, because there is no diagnostic. |
| Nested text kind | `@"` (A1); `@"…"u8` (B3); `$@"` (A5); `@$"` (B4); `/* */` trailing (A2) and leading (B1); `/** */` (B8) |
| Content of the nested line | text then `"` (A1); an empty line inside the verbatim (B2: the fix writes 13 spaces into the string); a hole of the nested string (A5 `b{name}`); a comment's closing `*/` followed by the hole's `}` (B6) |
| Depth | directly in the hole (A1); inside a hole of a nested `$@"` (A8); inside a multi-line hole of a nested regular `$"` (B9); as a call argument in the hole (B7) |
| Number of `$` on the outer literal | one (A1); two (A9) |
| Line ending | LF and CRLF, identical results |

The test implementer must cover at least the issue's two inputs as written (A1, A2). Beyond those, at least one leftward cell (A3 or A4), the `$@` and the doubly nested cell (A5, A8), the leading-comment cell (B1), and the empty-line-inside-verbatim cell (B2) are needed, because B2 is the only cell where the moved line holds no visible character. Each needs a CRLF variant, or the existing CRLF test pattern. A6 (a nested raw string) holds today and has to keep holding.

The fixture texts are reproduced here so the test implementer can rebuild them; all sit in `class C { void M() { … } }` and the scratch fixtures are deleted:

```csharp
// A1
        var a = $"""
    Hello {@"a
b"}
    """;
// A2 (with var name = "x";)
        var a = $"""
    Hello {name /* a
  b */
    }
    """;
// A3
        var a = $"""
                         Hello {@"a
                  b"}
                         """;
// B2
        var a = $"""
    Hello {@"a

b"}
    """;
```

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5202 analyzer | Must stay. It reports every fixture correctly, and does not report the formatter's FMT-RAW-009 output, whose closing marker is aligned. |
| RH5202 code fix (`RealignContent`) | Must change. |
| Fix All (`WellKnownFixAllProviders.BatchFixer`) | Must stay. These inputs have one diagnostic per literal. |
| Formatter (`RawStringLineShifts`, FMT-RAW-007/008/009) | Must stay. It holds for every cell, and a second pass changes nothing. |
| CLI | Not involved. |
| `Reihitsu.Core` | Not involved by the defect itself. |
| Spec `sdd/specs/analyzer/RH5202.md` | Must change. RH5202-003 says the fix "MUST shift every line of the literal after its first line" by the same amount. Its `Why` ("keeps the value of the literal unchanged") is false for these inputs. `Uses` lists FMT-RAW-001..004 but not FMT-RAW-009. Which wording it gets depends on the output question. |
| Rule page `documentation/rules/RH5202.md` | Must stay. It speaks of the raw string's own content lines, and corner cases do not belong on rule pages. |
| Tests | RH5202 code-fix tests must gain the reproductions. The FMT-RAW-009 formatter tests stay. |

## Simpler option

_None._ Making no change is not an option, because the fix silently changes literal values, which SHARED-FIX-003 forbids. Withholding the fix for these inputs is narrower, but it changes what the user sees, so it is option B of the output question below. One fact for whoever decides the shape: on B7, whole-document formatting also wraps the argument list inside the hole (`string.Concat("x",⏎ @"a`). Any route that runs the full pipeline over the literal would bring that edit into the fix's result (FC-030).

## Output questions

**1 · RH5202 · Raw string literals should be formatted correctly — a verbatim string or a `/* */` comment that spans lines inside a hole**

Two readings give different output. RH5202-003 plus SHARED-FIX-003 reads as "the uniform shift would change a literal, so the fix is withheld". SHARED-FIX-007 plus FMT-RAW-009 reads as "the fix lays the literal out as the formatter does".

Input:
```csharp
        var a = $"""
    Hello {@"a
b"}
    """;
```

Before (`main`):
```csharp
        var a = $"""
                 Hello {@"a
             b"}
                 """;
```
(The verbatim string's value changes to `a⏎             b`. The comment case `{name /* a⏎  b */⏎    }` likewise moves `  b */` to column 15.)

After, A (recommended):
```csharp
        var a = $"""
                 Hello {@"a
b"}
                 """;
```
The comment case keeps `  b */` in column 2, and every other line moves as before.

After, B: no fix offered for this literal; the diagnostic stays.

Why A: A is the formatter's output for the same input, so the fix and the formatter agree and the diagnostic is cleared. It is also the output the issue asks for.

## Internal choices

_None._ How the fix decides which lines to exclude is the implementer's choice, and is subject to the architecture finding below.

## Issue goals

- "The fix produces the formatter's FMT-RAW-009 output for both inputs, with regression tests." This holds if output question 1 is answered A. If it is answered B, the goal changes to "the fix is withheld for these inputs", which changes visible output and is covered by the question.
- "Affected: … (its own copy of the raw-string shift)". This is accurate, and it is the architecture finding below.
- The issue covers only the verbatim and `/* */` instances. The class also contains `@"…"u8`, `$@`/`@$`, `/** */`, a leading comment, deeper nesting, and the leftward shift (all failing, see the matrix). I would widen the goal to the class. That changes no further output beyond what question 1 decides, because every cell follows the same answer.

## Architecture findings

Two components own the same decision: which lines of a multi-line interpolated raw string move, and by how much.

- `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` implements FMT-RAW-001/002/007/008/009.
- `RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.RealignContent` is a private text-level copy that implements only FMT-RAW-001/002.

The copy drifted when #1457 added FMT-RAW-009 to the formatter only (FC-015, FC-016). Every failing cell of the matrix shows the two components disagreeing, and so does B5 under *Follow-up candidates*. Per `squad/README.md`, *Architecture findings*, it matters whether a narrow fix would itself add a second statement of FMT-RAW-009's rule inside the code fix. The lead decides.

## Follow-up candidates

- **Adjacent, same component (B5).** For a plain multi-line raw string with an empty content line, the fix writes `delta` spaces onto that empty line, leaving trailing whitespace. The formatter leaves the line empty.

  ```csharp
          var a = """
      one

      two
      """;
  ```

  The fix's output puts 12 spaces on the empty line; the formatter's output leaves it empty, and the other lines land in the same place. The result compiles (`apply-fix.sh` reports `fixed`, `invalid-result 0`). This is a SHARED-FIX-007 layout divergence with the same structural cause as the finding above. No visible output is in question, because SHARED-FIX-007 demands the formatter's layout. It is a candidate for the run's scope or for a follow-up; the lead decides.
- _No other finds._

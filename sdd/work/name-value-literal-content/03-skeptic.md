Role: skeptic
Commit: 2b44b73be9ce3cd92fd14c3a0ac9d7c704d9a04f
Verdict: question for the maintainer

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Real. Both requirements state the maintainer's principles P3 and P5 from thoenissen/Reihitsu#912. Every example in the spec diff holds at this commit. I checked each one independently of the spec author:

- `scripts/apply-fix.sh`, LF and CRLF, on:
  - RH6002 fix pair: both commas fixed; ` ,` in the literal and in `// e ,f` unchanged.
  - RH6003 fix pair: fixed; `"a ;"` unchanged.
  - RH3201 block: reported; the fix deletes `//`.
  - RH5023 and RH6021 negative block: `no-diagnostic` for each.
  - RH3103 block: all three fields reported. The fix is offered for `System.Nullable<int>` and `System . Nullable<int>` and withheld for `System./* c */Nullable<int>`, which is the general comment limitation.
- A throwaway RH8101 test in a temporary worktree, now deleted: only `@event` is reported, at (6,39).

The covering tests can be characterization tests drawn from these blocks. The issue's own examples break the requirements today. They are tracked in #1071, #1074, #1075 and #1105, as `02-spec-author.md` says.

One part of SHARED-DIAG-003 is not settled by the maintainer's earlier decision. It makes rules that nobody has reviewed into bugs. See Questions.

## Mechanism and candidates

_N/A._ This run specifies existing behavior; there is no bug to explain.

Dispatch I derived the candidates from: analyzer rules that inspect literal tokens or rewrite them. I found them with `rg` on `StringLiteralExpression|InterpolatedStringExpression|LiteralExpressionSyntax|InterpolatedStringText` under `src/Reihitsu.Analyzer*/Rules`, plus the formatter-delegating fixes that move a multi-line raw string:

- RH3003 (`LiteralExpressionSyntax`): reports the literal `""`; the fix replaces it with `string.Empty`.
- RH3204 (`InterpolatedStringExpressionSyntax`): reports the whole interpolated string; the fix calls `StringInterpolationUtilities.RemoveInterpolationMarkers`, which collapses `{{` and `}}`. Observed: `$"a{{b}}"` becomes `"a{b}"`.
- RH5202 (`LiteralExpressionSyntax` and `InterpolatedStringExpressionSyntax`, multi-line raw only): reports the literal when the closing `"""` is not in the column of the opening quotes; the fix shifts every content line. Observed: content and closing lines shifted 3 columns right, and the empty content line became three spaces.
- RH3202 (`ArrowExpressionClauseSyntax` location): for `string M() => """⏎a⏎⏎b⏎""";` the fix produces a block body, and the formatter shifts the raw string's content and closing lines 3 columns left. Observed.

In every case the literal's value is unchanged and its characters are changed.

## Breaking inputs

- **FC-017 — the RH6021 part of the negative block is vacuous.**
  - `RH6021ColonsMustBeSpacedCorrectlyAnalyzer` registers only `BaseList`, `BaseConstructorInitializer` and `ThisConstructorInitializer`. No interpolation can ever raise it, so `$"{2: D3}"` "not reported by RH6021" cannot fail.
  - No analyzer inspects `InterpolationFormatClauseSyntax`. The one falsifiable format-clause case I found is RH5601, which checks every character: a tab in the format text `$"{2:→D3}"` is not reported (`apply-fix.sh RH5601`, `no-diagnostic`).
  - The RH5023 part is falsifiable, because RH5023 is line-based and excludes literal and comment lines through `GetNonFormattableLineIndices`.
- **FC-002 / FC-003 — qualifiers without an example.** The following hold today (`apply-fix.sh`, LF and CRLF) and are usable on either side:
  - RH5601, not reported — a tab inside each of:
    - `"a→b"`, `@"a→b"`, `"a→b"u8`, `'→'`, `"""a→b"""`;
    - the text of `$"a→{1}→b"`;
    - `// a→b`, `/* a→b */`, `/** a→b */`, `/// <summary>a→b</summary>`.
  - RH5601, reported and fixed — a tab inside the hole expression `$"{1→+ 2}"` and in the alignment clause `$"{1,→5}"`. This is the hole-as-code side.
  - RH6002, reported and fixed: `$"{1 ,5}"` (alignment clause is code). `$"{1,5}"` is not reported, which matches RH6002-003.
  - RH6002, not reported: `/// <see cref="M(int,int)"/>` and `cref="M(int ,int)"`. Roslyn parses these crefs into real comma tokens, so this is a falsifiable `///` example.
  - RH5602, not reported: the trailing spaces of `// abc···` and of the first line of `/* a···⏎b */`. Roslyn keeps them inside the comment trivia. `scripts/format.sh` keeps them too, so analyzer and formatter agree (FC-013).
- **SHARED-DIAG-002, gaps and limits of its examples.**
  - The qualifier "derives other text from it, such as a file name" has no example. RH4001 is the only such rule, and it is broken (#1071).
  - The qualified-name example uses RH3103, which binds through the semantic model and compares no name. Its test cannot fail on a by-spelling comparison. No passing rule compares qualified names syntactically: RH4009 (#1071) and the using rules (#1105) are broken. The test that can actually fail on the qualified-name qualifier arrives with those fixes.
- **SHARED-DIAG-002 — inputs the spec does not mention.**
  - `/// <param name="@class">` for `int @class`: the compiler reports CS1572 and CS1573, so it does not read `@` in the attribute as a verbatim prefix. RH8101 reports it too. The requirement's Why ("the compiler binds…") decides this in favor of today's behavior; nothing to change.
  - FC-002 on the wording: "a qualified name as its identifiers joined by `.`" does not define the value of a name containing `::` (`global::System.IO`, the #1105 territory) or type arguments. The spec's own `System.Nullable<int>` example is such a name.
  - A contextual keyword is not compared as a name. `@nameof(…)` is a method call, so RH3103's `Identifier.Text: "nameof"` check is correct because it compares spelling. As worded, the requirement's scope sentence covers that check. No compiling input shows a difference today.
- **FC-022**: comment-looking text inside a literal (`"// a ,b"`) is not yet in any example.
- **FC-023**: `@class` and `@event` are already in the RH8101 example.
- **FC-033**: every apply-fix result above was the same under LF and CRLF.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/diagnostics.md`, `requirement-ids.txt` | Must change (done). The SHARED-DIAG-003 wording depends on the question below |
| Analyzer rules (RH4001, RH4009, RH5601, RH5602, RH6009, RH6010, RH5025, RH5027, RH5011, RH7201, RH7203, RH7207) | Must stay; tracked in #1071, #1074, #1075, #1105 |
| RH3003, RH3204, RH5202, RH3202 and their fixes | Must stay in this run; the question decides whether they become bugs |
| Code fixes, Fix All | Must stay |
| Formatter | Not involved. FMT-RAW-001 ("The literal's value never changes") bears on the question |
| `Reihitsu.Core` | Not involved |
| Rule docs, `general-notes.md` | Not involved. I agree with `01-spec-author.md`: promising rule-wide guarantees that several rules do not yet meet would document nonexistent behavior |
| Analyzer tests | Must change: covering tests tagged SHARED-DIAG-002 and SHARED-DIAG-003 |

## Simpler option

- Drop "and its code fix MUST NOT change it" from SHARED-DIAG-003 and leave the fix side to SHARED-FIX-003 from the concurrent #1068 run.
  - Draft: branch `claude/admiring-euler-a4zxhb` at `1bf3eeb`. It forbids a fix that changes the text of a literal "except text that a diagnostic of the fix's own rule reports".
  - Once both runs merge, two shared requirements would state the fix side with different exemptions: SHARED-DIAG-003 exempts by subject (comments and directives), SHARED-FIX-003 by reported span. That violates "referenced by its ID, never restated".
  - Dropping the clause narrows the question below to what a rule reports. It does not settle the question.
- Wording only, for the spec author:
  - Replace the RH6021 part of the negative block with a falsifiable rule (RH5601 above).
  - "a line … is part of the literal, not a blank line" restates SHARED-BLANK-001's blank-line definition for literals. It could reference SHARED-BLANK-001 or say only "is part of the literal's text".

## Questions for the maintainer

**Q1 — Does SHARED-DIAG-003 protect a literal's characters or its value, and are rules whose subject is a literal exempt like comment and directive rules?**

The decision `squad/decisions/2026-10-18-shared-diag-003-comment-subject-rules.md` answered this for comments and directives only, and gave as its reason "the same reading as SHARED-FIX-003, which is scoped to what the rule itself reports". The spec text names only comments and directives. Its first sentence, "restricts only rules that check code", could also be read as exempting a rule that checks a literal.

Inputs, all observed at this commit:

- A — RH5202 on a raw string whose closing `"""` is 3 columns left of the opening quotes: reported; the fix shifts every content line right, and an empty content line becomes `···`.
- B — RH3204 on `$"a{{b}}"`: reported; the fix gives `"a{b}"`.
- C — RH3202 on `string M() => """⏎a⏎⏎b⏎""";`: reported; the fix, through the formatter, shifts the raw string's lines 3 columns left.

Readings:

- **(1) Value.** "Text" is what the literal denotes. Moving a raw string's lines together with its closing delimiter, or rewriting `{{` as `{` when `$` goes, does not change it.
  - Output: A, B and C stay as they are today.
  - This matches FMT-RAW-001.
- **(2) Characters, with the exemption extended to literals.**
  - Under SHARED-FIX-003's span wording, A and B stay. C also stays, because RH3202's diagnostic spans the arrow clause, literal included.
  - Under the subject wording SHARED-DIAG-003 uses now, RH3202 checks code. C becomes a bug: the fix must be withheld whenever the body holds a multi-line raw string.
- **(3) As written.** Characters, with comment and directive subjects only.
  - Output: A, B and C all become bugs.
    - RH5202 must stop reporting and fixing. FMT-RAW-001's alignment is then left without an analyzer counterpart (FC-013, FC-014).
    - RH3204 withholds its fix whenever the string contains `{{` or `}}`.
    - RH3202, and any formatter-delegating fix that moves a multi-line raw string, withholds its fix.
    - Whether RH3003 replacing `""` counts as changing a literal is unclear under this reading.
  - No issue tracks any of these today.

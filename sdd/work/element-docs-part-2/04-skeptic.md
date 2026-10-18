Role: skeptic
Commit: 2c860c7ce794eaf9b23ba5e6a716cb0366118f7b
Verdict: proceed narrower

Re-check of the revision only: `03-spec-author.md` and `git diff 139792b HEAD -- sdd/specs documentation`. The lead's message carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Method: a fresh scratch harness outside the repository. It uses Roslyn 5.9.0, `LanguageVersion.Latest`, an `XmlFileResolver`, and the Debug `Reihitsu.Analyzer.dll`; production code is unchanged since 53990ed. It can compile several files into one compilation and prints the compiler's documentation entry for each extension block. Mode is `Diagnose` unless stated. The fixtures have been deleted.

The revised example blocks reproduce. SHARED-DOC-010's block, SHARED-DOC-012's first block (with the new `@class` and `@T` declarations), and SHARED-DOC-013's block were extracted from the spec, their markup was stripped, and they were run. Each reports exactly the marked spans, in `Diagnose` and in `Parse` mode.

Resolved as 02 asked:

- F1: SHARED-DOC-013, the RH8032/RH8033 references, and the RH8031-001 coverage.
- F2 and F9: RH8030-001 coverage.
- F3: the padded `name` is left out of coverage; Q5.
- F5 and F7: wording, Unicode white space, and the processing instruction.
- F6: the container list, approved in the decision record's follow-up answer.
- The rule pages RH8032 and RH8033 (F4).
- F8: the markings now follow one stated rule. P8 is used for presence and for what counts as a `<summary>`/`<inheritdoc>` at all. Which sources a non-presence rule reads, and whether `<inheritdoc>` satisfies RH8031, are normal questions. RH8031 Q1/Q2/Q3, RH8030 Q2/Q3, and shared Q3/Q4 are consistent with that rule.

Still open in the revision:

**R1 — SHARED-DOC-013's merge condition is incomplete. The spec requires sharing for pairs the compiler keeps apart.** Each pair below is a block with `<param>`/`<typeparam>` followed by a block with only a `<summary>`, in one static class:

| Pair | Compiler | RH8032/RH8033 on the second block | SHARED-DOC-013 as written |
|---|---|---|---|
| `(int a, int b) value` / `(int c, int d) value` | two entries | reported | same type → MUST share → not reported |
| `ref Span<int> span` / `scoped ref Span<int> span` | two entries | reported | `scoped` not in the list → MUST share |
| `in int value` / `ref readonly int value` | two entries | reported | "`ref`, `in`, or `out`" does not say whether `ref readonly` differs from `in` |
| `[A(1)] long value` / `[A(2)] long value` | two entries | reported | "the same attributes" does not say whether arguments count |
| `<T>(T[] values)` / `<[A(0)] T>(T[] values)` | two entries | RH8032 and RH8033 reported | type parameters list only names and constraints → MUST share |

Pairs that do merge (one entry, second block not reported):

- `string value` / `System.String value`
- `where T : IA, IB` / `where T : IB, IA`
- `where T : class, IDisposable` / `where T : class, System.IDisposable`
- two blocks in different files of one `partial` static class

Pairs that do not merge, which the text already covers:

- `string value` under `#nullable enable` / under `#nullable disable` (nullability)
- `where T : class` / `where T : class?`

The condition needs these additions: tuple element names, `scoped`, `ref readonly`, attribute arguments, and the attributes of type parameters. Each needs an example on each side, or the requirement must leave unlisted differences out of coverage. "Declared identically" also reads as textual identity, while `string` and `System.String` merge.

**R2 — A malformed comment on one merged declaration removes the documentation of all of them.** Input:

```csharp
/// <summary>7</summary>
/// <param name="value">V</param>
extension(long value) { }

/// <summary>8 a & b</summary>
extension(long value) { }
```

The second comment gets CS1570. The compiler writes `<!-- Badly formed XML comment ignored … -->` for the shared ID, and RH8032 reports `value` on both blocks, although the first block's own comment is well-formed and has the `<param>`. SHARED-DOC-013 "covers what SHARED-DOC-011 and SHARED-DOC-012 cover". Those requirements cover well-formed comments, but it is not clear whether the first block counts as covered. The coverage should state that every declaration in the merge has well-formed comments. The current output agrees with the compiler and with the #1091 consequence that malformed XML does not count, so stating it as a requirement is also possible; that is the spec author's call.

**R3 — "in source order" (SHARED-DOC-013) is undefined across the files of a partial class.** The two-file case merges. The order changes no verdict of RH8032/RH8033, so the phrase can go, or the requirement can say it is the compiler's order.

**R4 — SHARED-DOC-010's "`*` that continues a `/** */` comment on a new line" is narrower than the code.**

- Counted as no text: a run of stars at the start of a line (` ** </summary>`, ` *** </summary>`).
- Also counted as no text: a line-start `*` after a line without one.
- Text: a second star after a space (` * * </summary>`) and a mid-line star (`/** <summary> * </summary> */`).

The text could say "the `*` or run of `*` that starts a line of a `/** */` comment". LF and CRLF give the same verdicts.

**R5 — the `@` reading of P3 (F4) now sits in normative text.** SHARED-DOC-012 now states that `name="@class"` does not document `@class`, and its first example asserts RH8032 on `@class` and RH8033 on `@T`. That matches the code and the compiler (CS1572, CS1711). The spec author reads P3 as not reaching an attribute value. As 02 said, that reading is the lead's decision, and it has to be made before the test implementer tags SHARED-DOC-012. If P3 reaches `name="@class"`, that half of the sentence and the two marked spans leave coverage, and the shape is filed as `Decided by principle P3 (#912)`.

## Mechanism and candidates

_N/A._ This is not a bug fix.

## Breaking inputs

New since 02, for SHARED-DOC-013, each on both sides:

- merging pairs: `string`/`System.String`, constraint order, a qualified versus unqualified constraint type, two files of a partial class;
- non-merging pairs: nullability (`string?`, and annotated versus oblivious), parameter name, `ref`/`in`/`ref readonly`/`scoped`, tuple element names, attribute presence and arguments, type-parameter names, constraints (`class`/`class?`), and type-parameter attributes;
- `<inheritdoc>` on one merged block exempting the other;
- an undocumented merged block not reported (SHARED-DOC-011, first clause);
- R2's malformed sibling, once its coverage is settled.

For SHARED-DOC-010:

- `/** */` with ` * `, ` ** `, and a missing star on the first continuation line;
- `* Text`, which is not reported;
- `&#160;`, `&#x2003;`, and `&#x85;`, which are reported;
- `&#x200B;`, which is not reported;
- every form under LF and CRLF.

FC-016 and FC-033 still apply: tests outside SHARED-DOC-013 must not repeat a receiver in one class.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH8020–RH8034 and the shared helpers | must stay |
| Code fix / Fix All, formatter, `Reihitsu.Core` | not involved |
| `shared/element-documentation.md` | must change: R1–R4; R5 only if the lead reads P3 widely |
| `analyzer/RH8020.md`–`RH8034.md` | must stay as revised |
| Rule docs RH8032, RH8033 | must stay as revised, unless R5 changes the `@` reading |
| Tests | must change: tagging and covering tests, after R1–R5 settle |

## Simpler option

_None._

## Questions for the maintainer

_None that block the run._ R5 is a decision for the lead about how to read P3. R1 and R2 are settled either by completing the stated condition or by narrowing coverage and filing the remainder in the shared spec's open-questions issue.

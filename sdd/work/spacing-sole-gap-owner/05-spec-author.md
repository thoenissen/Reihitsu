Role: spec author
Commit: 5ac8179
Verdict: open question

Correction round for decisions 9 and 12 of `00-lead.md`, plus the sweep the lead asked for. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every "today" output below was run on 5ac8179 with `scripts/format.sh` (no production code differs from `origin/main`).

## Changed requirements

- FMT-SPACE-025 — One space on each side of `with` — clarified: the example's output is now the layout the full pipeline produces (`p.Q() with⏎{⏎X = 1⏎};`, the initializer aligned under `with`), which is the layout question 11 showed; `Why` names FMT-BREAK-021 for the initializer's line break and keeps FMT-SPACE-009 for a `{` that shares the line. FMT-BREAK-021 added to the spec's `Uses` row.
- FMT-SPACE-024 — One space behind a modifier or prefix keyword in front of its operand — clarified: a list of positions in front of `(`, `[`, `*` that the answers already decide, and a second example. The `ref`/`readonly` of a type outside a parameter and a lambda's return type is left as it was and is question 1 below.
- Change doc `sdd/changes/2026-11/spacing-sole-gap-owner.md`: entry 11 now shows today's multi-line output as Before and the multi-line output with the space as After. Its "Before: unchanged" was also wrong, because today's pipeline already lays out the one-line initializer over several lines. Entry 9 gains one sentence naming the decided positions below. Status stays `Accepted`, because no visible output beyond the answered questions was added.

Spec tests: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` gives 51 of 51 passed.

## Row 9 — the `with` example

Today, `var a = p.Q()with { X = 1 };` gives
```csharp
        var a = p.Q()with
                     {
                         X = 1
                     };
```
and the same input with the space gives the layout now in the spec, with the braces one column further right. `var c = p!   with { X = 3 };` already gives `p! with` today (FMT-SPACE-002), and the example keeps that line.

## Row 12 and the sweep — what the answers decide

Today's output for the lead's case:

| Input | Today |
|---|---|
| `ref(int, int) r = ref t;` | unchanged |
| `ref readonly(int, int) q = ref t;` | unchanged |
| `ref⏎(int, int) r2 = ref t;` | **not joined**: `ref` stays on its own line, and `(int, int) r2 = ref t;` is re-indented to the statement's column |

The local's case is not a join today, so answer 1B's promise to keep every joined output does not reach it. Answer 10 named `ref` "of an argument or parameter" and kept `public(int, int) G;`. A local's `ref` is neither: syntactically it belongs to the type (a ref type), but to a reader it is a modifier in front of a declared type, like `public`. Both readings fit every answer, so the case is **not decided**. It is question 1.

Each keyword family of FMT-SPACE-024 was checked in front of a tuple type, a pointer type, and a parenthesized or bracketed operand, one line and wrapped:

| Position | Today one line | Today wrapped | Settled by | Result |
|---|---|---|---|---|
| lambda `static`/`async` in front of a tuple return type, `N(static(int, int) () => (1, 2))` | kept | joined **with** a space | 10B (lambda modifier) + 1B | space; spec explicit |
| `ref`/`readonly` of a lambda's return type, `N(ref(int, int) () => ref s_t)` | kept | joined **with** a space | 1B (keeps every joined output); the construct's one-line gap follows it (change doc, *keyed by construct*) | space; spec explicit |
| `ref` of a ref expression, `N(b ? ref(x) : ref y)`, `N(() => ref(x))`, `return ref(x);`, `z = ref(y);`, `ref var z = ref(x);` | kept | joined with a space inside an argument; not joined in a statement | 1B for the joined ones; one construct wherever it stands, and the spec already said "a modifier of an expression", with no exclusion that fits a statement | space; spec explicit |
| `ref` of a function pointer's parameter, `delegate*<ref(int, int), void>` | kept | joined **with** a space | 10B (parameter) + 1B | space; spec explicit |
| `var` of a deconstruction, `var(a, b) = t;` | kept | not reached | 10B ("behind `var` in `var (x, y)`") | space; spec explicit (already tested: `VarGetsOneSpaceInFrontOfAParenthesizedDesignation`) |
| local function modifier, `static(int, int) K()` | kept | not joined | no bullet of FMT-SPACE-024 lists a local function's modifier; same family as `public(int, int) G;` (10B) | kept; spec explicit; joins the existing follow-up candidate |
| member `readonly`, `public readonly(int, int) M3()` | kept | — | member modifier, 10B | kept; spec explicit |
| `this`/`base` of a constructor initializer, `: this(a, a)`; `this[` of an indexer | kept | — | not a modifier | not decided by this rule; spec explicit |
| `ref`/`ref readonly` of a local, a `foreach` variable, a method's, local function's, property's, indexer's, or delegate's return type, a ref field | kept | not joined (`ref⏎(int, int) _f;`, `ref⏎(int, int) P`, `foreach (ref⏎(int, int) e …)` all stay on two lines) | **not decided** | question 1 |
| `scoped` in front of a tuple type, `scoped(int, int) s = t;` | kept | — | parses but never compiles, because `scoped` needs a ref struct or a `ref`; in compiling code `scoped` is followed by `ref` or a word | dropped, but if the answer to question 1 is B, the same type-modifier wording covers it |
| keyword in front of a pointer type | — | — | a pointer type starts with its element type, which is a word, `(`, or `delegate`; `(` reduces to the tuple rows above; `out*p`, `ref*p`, `return ref*p`, `await*p` are operands, already decided | no new case |
| `params`, `this` (parameter), `in`/`out`/`ref` (argument/parameter), `await`, `stackalloc`, `not`, `in` of `foreach`/`from`/`join` | as specified | as specified | 10B | unchanged; variance `in T`/`out T`, `await foreach`, `stackalloc int[3]`, `ref struct` are followed by a word |

FMT-SPACE-024 still reads "a modifier of … a type" and "a modifier of a … local declaration" for question 1's case, unchanged. When the answer comes, the spec author changes that text:
- **1A**: in the list of positions, add "The `ref` and the `readonly` of any other type — a local's, a `foreach` variable's, a field's, a return type's — are modifiers of a declaration and keep their gap: `ref(int, int) r = ref t;`", and narrow the bullet to "a modifier of an argument, a parameter, or an expression".
- **1B**: add "The `ref` and the `readonly` of a type get the space wherever the type stands — a local's, a `foreach` variable's, a field's, a return type's: `ref (int, int) r = ref t;`", and narrow the exclusion to "a modifier in a member's, local function's, or local declaration's modifier list (`public`, `static`, `const`)". Change doc entry 9 then gains the case.

## Examples for the tests

- FMT-SPACE-024 — the second example: lambda's tuple return type behind `static`, lambda's ref return type, function pointer parameter `ref`, ref expressions in a declaration and a `return`, deconstruction `var`, local function `static` keeping its gap. These one-line forms are not tested yet, apart from `var (a, b)`. The joined forms reached today (`N(b ? ref⏎(x) : ref y)`, `N(() => ref⏎(x))`, `N(ref⏎(int, int) () => ref s_t)`, `N(static⏎(int, int) () => (1, 2))`, `delegate*<ref⏎(int, int), void>`) should stand next to their one-line counterparts (FMT-BREAK-031). The constructor-initializer `: this(a)` belongs in `KeywordsWithTheirOwnParenthesisKeepTheirGap` or a sibling, and `static(int, int) K()` belongs in `MemberModifiersInFrontOfATupleTypeKeepTheirGap` or a sibling.
- FMT-SPACE-025 — the example is now the full-pipeline output and can be tested through `AssertRuleResult`. One test expectation to name: `SameLineGapRulePhaseTests.WithRequirementExampleGetsOneSpaceOnEachSide` claims the requirement example, but its one-line expected output is the phase-level result, no longer the spec example's output. It stays valid as a phase test. Either rename it or add a full-pipeline test of the example; the test implementer decides.
- No existing test expectation contradicts this round's spec text.

## Output questions

**1 · Formatter — `ref` / `ref readonly` of a local's, field's or return type's tuple type**

Input:
```csharp
ref struct S
{
    static (int, int) s_t;
    ref(int, int) _f;

    ref readonly(int, int) M()
    {
        ref(int, int) r = ref s_t;

        return ref r;
    }
}
```

Before (`main`):
```csharp
ref struct S
{
    static (int, int) s_t;
    ref(int, int) _f;

    ref readonly(int, int) M()
    {
        ref(int, int) r = ref s_t;

        return ref r;
    }
}
```

After, A — like a declaration's modifier (`public(int, int) G;`), the gap stays:
```csharp
ref struct S
{
    static (int, int) s_t;
    ref(int, int) _f;

    ref readonly(int, int) M()
    {
        ref(int, int) r = ref s_t;

        return ref r;
    }
}
```

After, B (recommended) — like a parameter's `ref` and a lambda's `ref` return type, one space:
```csharp
ref struct S
{
    static (int, int) s_t;
    ref (int, int) _f;

    ref readonly (int, int) M()
    {
        ref (int, int) r = ref s_t;

        return ref r;
    }
}
```

Why B: the same `ref (int, int)` gets the space as a parameter (`ref (int, int) u`) and as a lambda's return type in this run, so with B it reads alike wherever it stands.

## Internal choices

- The ref expression is treated as decided wherever it stands, not only where a join reaches it, because it is one construct and the spec already named "a modifier of an expression" with no exclusion that fits a statement. Alternative: ask it with question 1. Not chosen, because no reading of the text leaves `return ref(x);` out.
- The open case's wording in FMT-SPACE-024 is left as it was, not narrowed, because any unambiguous wording would decide question 1. The replacement text for both answers is ready above.
- The new positions are a list under FMT-SPACE-024 plus a second example, not a new requirement. Alternative: a separate requirement for ref types. Not chosen, because they are members of the same keyword family.

## Follow-up candidates

- A local function's modifier in front of a tuple return type (`static(int, int) K()`) keeps the author's gap. It belongs to the existing candidate "A member's own modifier in front of a tuple type" (for #1440) and is not a new candidate.
- A wrapped `ref⏎(int, int)` of a local, field, property or `foreach` variable is not joined: `ref` stays alone on its line. This is a line-break question outside the run, and it is reachable by whole-document formatting. Readings: A — leave it as it is; B — join it like the lambda's `ref⏎(int, int)` inside an argument. No recommendation from this role. It belongs with the line-break follow-ups (FMT-BREAK).

## Row 13 carried out

Base: bce245d. Lead decision 13 put question 1's positions outside this run, and I wrote the text for answer A:
- **FMT-SPACE-024.**
  - The bullet now reads "a modifier of an argument, a parameter, or an expression"; "a type" is gone.
  - The list of positions gains a line: the `ref` and the `readonly` of any other type (a local's, a `foreach` variable's, a field's, a return type's) are outside this rule and keep the author's spacing, like a declaration's modifier. The examples are `ref(int, int) r = ref t;`, `ref readonly(int, int) M()` and `ref(int, int) F;`.
  - The second example gains `ref(int, int) r = ref s_t;` in `L()`, which keeps its gap.
  - Run through `scripts/format.sh`, the example's input gives the example's output except for the spacing the rule adds.
- **Change doc entry 9** gains one sentence: these positions are outside the rule, keep the author's spacing as on `main`, and no join reaches them.
- **Spec tests:** `--project architecture --filter Sdd` gives 51 of 51 passed.
- **Tests:** none has to change. `MemberModifiersInFrontOfATupleTypeKeepTheirGap` or a sibling can pin `ref(int, int) r = ref t;` as kept.
- The output question has moved to the follow-up the lead recorded. The test implementer should take the answer-B text under *Row 12* no further.

## Answer 14A carried out

Base: a08958f. The change doc's `Decisions` section gains *One-line keyword positions no join reaches* (14A), and entry 9 shows the one-line before/after forms. The spec already matched 14A and is unchanged. Sdd tests: 51 of 51 passed.

## Row 19 carried out

Base: 324c27e. The prompt asked for the widening of row 19 and named the implementer's item 8; it carried no suspected cause or hoped-for verdict beyond the lead's decision, which this round carries out. Outputs below were run with `scripts/format.sh` on 324c27e and, for "Before", in a detached `origin/main` worktree.

- **FMT-SPACE-014 — changed.** The whitespace in front of a right-carried `/** */` stays as written, also when the left token carries a comment of its own in the gap (checked: `b /* x */   /** y */+b` keeps the three spaces between the comments). When a rule decides the pair, its decision applies to the whitespace behind the comment: removed when no space is allowed, otherwise exactly the demanded number of spaces, inserted where none was written. `Why` now names both directions and FMT-SPACE-013. The example adds `b /** sum */+b`, `b /** sum */   + b`, `b  /** sum */+ b` (FMT-SPACE-010) and `c ? b /** yes */:b` (FMT-SPACE-018). The example's input gives the example's output through the full pipeline.
- **Consistency.** FMT-SPACE-013 is unchanged; it applies the same decision behind a left-carried comment, and the new sentence on a gap with comments on both sides sits in FMT-SPACE-014. The comment sentences of FMT-SPACE-016 (`[return /** x */ :Obsolete]` → `[return /** x */: Obsolete]`) and FMT-SPACE-019 (`M(b /** x */ :b)` → `M(b /** x */: b)`) are removal cases and still hold; both checked through the full pipeline. RH6002, RH6003, RH6008, and RH6012 cite FMT-SPACE-014 for removal only, which still holds. No analyzer reports a gap that holds a comment (SHARED-OPSPACE-001/-002), so no rule spec changes.
- **Change doc — a stated output changes.** On `main`, the gap behind the comment was only ever emptied: `b /** sum */+b` gave `b /** sum */+ b`, and `b /** sum */   + b` kept its three spaces. Now it gives `b /** sum */ + b`. Changes made:
  - `FMT-SPACE-014 (changed)` is added to the affected requirements.
  - A `What` bullet and a new entry 14 show the input, Before, and After.
  - A `Decisions` entry records row 19 as a lead decision.
  - `Unchanged` and the `What` summary now say "11 to 14".
  - The `Visible output changes` intro names entry 14.
  - Status stays `Accepted`.
- **For the lead:** entry 14 includes a visible change for a pair whose rule exists on `main` (FMT-SPACE-010, the binary operator), and no maintainer answer of this run names that pair. Row 19 records this as following the maintainer's answers, so the lead decides whether entry 14 needs an output question. The doc's status was not changed, so that call stays with the lead.
- **Where the case is visible:** whole-document formatting shows it only where the documentation-comment phase leaves the comment on the line. Behind an operator, a `:` of a conditional, or a `;` or `]`, it stays. In front of a token that starts a node, it moves to its own line: the relational operand (as the implementer found for item 7), `when`, a declared name, the expression behind `=>`, and a cast operand. Those cases are visible at phase level only.
- **Spec tests:** `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` gives 51 of 51 passed.

### Examples for the tests

- FMT-SPACE-014: the full example can be tested with `AssertRuleResult`. No existing test covers the insertion cases `b /** sum */+b`, `b /** sum */   + b`, and `c ? b /** yes */:b`, or the kept `b  /** sum */`. No existing test expectation contradicts the new text.

### Follow-up candidates

- **A gap behind a right-carried comment that no rule decides keeps runs of spaces.** This is true on `main` and on this branch. FMT-SPACE-002's literal text would collapse them, and its `Why` calls a double space accidental.
  - Input: `var r = new[] { /** z */   1 };`
  - Real output: unchanged.
  - Readings: A — keep as written, an exception like the whitespace in front of the comment; B — collapse to one space (FMT-SPACE-002).
  - Recommendation: B, for FMT-SPACE-002's reason.
  - Whole-document formatting reaches it. It is not part of row 19, and FMT-SPACE-014 does not state it, so the spec does not write down behavior that looks wrong.

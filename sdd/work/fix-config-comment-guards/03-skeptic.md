Role: skeptic
Commit: 36d421e
Verdict: #1595: proceed, after one spec correction (RH7102-006); #1593: proceed, after two wording corrections in SHARED-ORDER-007; #1616: output question (an extension of the spec author's open question), plus wording corrections

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It asked me to check one claim from `02-spec-author.md`: the lone `;` behind `#endif`. That result is under #1593. This stage challenges the spec diff and the change doc (`git diff 57acf0d -- sdd/specs sdd/changes`), not the issue premises, which `01-skeptic.md` settled.

I built at 36d421e, where `src/` is identical to 57acf0d. Every fixture below ran through `scripts/apply-fix.sh <ID>` (LF and CRLF, identical unless noted) or `scripts/format.sh` (run twice), in a scratch directory outside the repository. No preprocessor symbol is defined.

I checked every example the diff adds against today's output. The ones the spec author marked "today's output" match: the RH7102-006 counterpart, the RH7309 split-member counterpart, the RH2001-009 kept-order example and `#pragma` example, and the RH2001-004 `{ get; set; }` counterpart (whose expression-bodied `Read` is left alone). The examples marked "required" are `fixed` today, which is the bug: the RH7102 struct and initializer examples, the RH7109 split initializer, the RH7103 split `static`, and the RH2001 reorder example (`no-fix-offered`).

## thoenissen/Reihitsu#1595

### Premise

Settled in `01-skeptic.md`. The spec diff states the guard for every member reordering fix, and SHARED-FIX-003 already requires it. I agree that no change doc is needed: the refusals bring the code back to SHARED-FIX-003, which is a bug fix under `sdd/README.md` (*When a change doc is required*).

**Defect in the spec diff: RH7102-006's new paragraph is false for a constant that an `#if` splits.** The paragraph says: "The constant itself has no runtime initializer and takes no place in the type's layout, so SHARED-ORDER-007 refuses a move only for the members that code excluded … declares in the constant's leading trivia." The closing paragraph of SHARED-ORDER-007 repeats this ("RH7102 moves only constants, which have no runtime initializer and take no place in the layout").

Both statements contradict the split-member paragraph the same diff adds to SHARED-ORDER-007. A declaration that is `const` only in the active configuration is a runtime-initialized field or a layout field in the other configuration. Both inputs below are `fixed` today, and both reorder the `DEBUG` build:

```csharp
public class C
{
    private static int _s = Log("s");
    private
#if DEBUG
        static readonly
#else
        const
#endif
        int K = 1;

    private static int Log(string name)
    {
        return 0;
    }
}
```

The fix moves `K` in front of `_s`. In a `DEBUG` build, `K = 1` then runs before `Log("s")`, and `Log` can read `K`.

```csharp
public struct S
{
    private int _x;
    private
#if DEBUG
        readonly
#else
        const
#endif
        int K = 1;
}
```

The fix moves `K` in front of `_x`. In a `DEBUG` build, the instance field `K` is then laid out before `_x`.

SHARED-FIX-003 forbids both. The split-member bullets of SHARED-ORDER-007 already withhold them under the reading "one configuration gives it a runtime initializer / declares it an instance field". `01-skeptic.md` dismissed a split `const` as "only withholds more", but that covers only the opposite direction (active non-`const`, excluded `const`). This direction is a failing member of #1593's class that reaches RH7102 once the base class asks the guard (decision 4).

The fix for the spec: RH7102-006 and the closing paragraph of SHARED-ORDER-007 should say "a constant that no `#if` splits", or name the split constant as counted by the split-member paragraph. Without that, the spec can be read as forbidding the guard for these two inputs, while SHARED-FIX-003 demands it. Visible output: none beyond what SHARED-FIX-003 requires.

### Mechanism and candidates

_N/A_ beyond `01-skeptic.md`. New candidate axis for the test implementer: an `#if` that splits the moved constant's own `const` modifier. Covered by the split-member paragraph once RH7102-006 stops excluding it.

### Breaking inputs

FC-001, FC-020, FC-016 as in `01-skeptic.md`, plus:

| # | Moved constant | Other configuration gives | Type / crossed | Today | Correct? |
|---|---|---|---|---|---|
| s1 | `const` under `#else`, `static readonly` under `#if DEBUG` | static runtime initializer | class, crosses `_s = Log("s")` | fixed | **fails** |
| s2 | `const` under `#else`, `readonly` under `#if DEBUG` | instance layout field | struct, crosses `_x` | fixed | **fails** |
| s3 | as s1 | static runtime initializer | class, crosses uninitialized static `_s` | — | counterpart: keeps the fix |

Tag s1 and s2 RH7102-006 and SHARED-ORDER-007. Run them under LF and CRLF.

### Surfaces

Unchanged from `01-skeptic.md`. Spec: RH7102-006 (paragraph 2) and SHARED-ORDER-007 (closing paragraph) must change as stated above.

### Simpler option

_None._

### Output questions

_None._

### Internal choices

- RH7102-006 still keeps the sentence "When no `#if`, `#elif`, `#else`, or `#endif` directive takes part in the refusal, the diagnostic is still reported". With SHARED-ORDER-007 added to the list, a conditional directive always takes part in an RH7102 refusal, yet the new examples say "is reported". Recommend rewording this to "The diagnostic is still reported", or naming which SHARED-ORDER-003 case the condition is meant for. Internal: the examples already state the output.

### Issue goals

_N/A_ (settled in `01-skeptic.md`).

### Architecture findings

_None_ new.

### Follow-up candidates

See #1593 (the member-moving fixes format the whole type).

## thoenissen/Reihitsu#1593

### Premise

Settled in `01-skeptic.md`. I agree that SHARED-FIX-003 already decided the split-member paragraph, so it needs no change doc.

### The lone `;` claim (decision 11)

**It reproduces.** Input (RH7109, the moved member's initializer is split, and the crossed `_a` has no initializer):

```csharp
public class C
{
    private int _a;
    private readonly int _b
#if DEBUG
        = Log("b")
#endif
        ;

    private static int Log(string name)
    {
        return 0;
    }
}
```

`apply-fix.sh RH7109` gives `fixed`. `_b` moves in front of `_a`, and the `;` line goes from column 8 to column 4. The same happens with a split `+ 2` in the initializer. The RH7103 split-`static` example moves its `static` and `int _b;` lines from column 8 to column 4 as well, and so does the RH7102 split-`const` fixture above.

**Caller.** The single fix of RH7109, RH7103, and RH7102. The cause is `TypeMemberOrderingCodeFixProviderBase` (`src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs:98-108`), which calls `FormatNodeInDocumentAsync` on the **whole type declaration** after the move. The CLI reaches the same column: `scripts/format.sh` on the unmoved input also puts that `;` at column 4, and the output is stable on the second pass.

**Which spec decides it.**

- For whole-document formatting, `sdd/specs/formatter/indentation.md` decides it in its non-goal: "a continuation line that no requirement below owns … currently take[s] the block column of the enclosing statement or declaration". Indenting such lines one level deeper is the decided behavior change thoenissen/Reihitsu#953, which is open and explicitly lists "the line after a directive". FMT-DOC's example in `documentation-comments.md` ("the `;` behind it takes its declaration's column") shows the same column.
- For the fix, SHARED-FIX-007's *Moved code* bullet requires moved code to keep its line breaks and blank lines, and says "only the indentation of its lines MUST follow its new nesting". Here the nesting does not change. Whether the fix may then give a continuation line the formatter's column (4) or must keep it (8) has two readings with different output. That question is outside this run's issues.

**Same class, clearer:** the base class also **joins** a line break inside the moved member when no directive or comment holds it:

```csharp
private readonly int _b
    ;
```

That becomes `private readonly int _b;` after the RH7109 fix. The *Moved code* bullet says line breaks stay as written "also where formatting the result would change them", so this one is decided against today's output. The cause is the same as the existing follow-up candidate (RH7102/RH7109 rewrite an expression-bodied `Log` they do not move, against SHARED-FIX-006): formatting the whole type after a member move.

Conclusion: the `;` column is not a defect of the run's issues, and it is not a new follow-up of its own. The whole-document column is specified and already tracked by #953. The fix-side column belongs with the existing candidate (see Follow-up candidates).

### Breaking inputs

As in `01-skeptic.md`, plus rows s1/s2 under #1595 (split `const`, consumer RH7102).

For the test implementer: a counterpart whose expected output holds a continuation line that the moved member carries (the lone `;`, a split modifier) locks in the whole-type formatting that the follow-up may change. Prefer counterparts that carry none: the RH7309 example in SHARED-ORDER-007 keeps `Log("y")` at column 8, and an RH7109/RH7103 counterpart can keep the split on lines that the formatter places at the same column anyway. If such a shape is needed, assert today's output and note that the follow-up may change it.

### Surfaces

Unchanged from `01-skeptic.md`. Spec: two wording corrections in SHARED-ORDER-007 (Internal choices).

### Simpler option

_None._

### Output questions

_None._

### Internal choices

- **"The moved members are the moved declaration and the members of excluded code in its leading trivia; no other member moves."** The new split-member bullet ("a further member that excluded code inside its declaration declares counts … at that position") contradicts this sentence when the declaration that holds that further member is the moved one. Recommend: "… in its leading trivia or inside it". Internal: it changes no output beyond SHARED-FIX-003.
- **Bullet 1's example is an initializer that excluded text adds.** Rows s1 and s2 change only a modifier (`const`, `static`), and the initializer text is active in every configuration. Recommend one clause: "… also when the initializer is active and only one configuration declares the member without `const`". Internal.
- **RH7109 counterpart example.** The spec author omitted it because of the column-4 `;`. That column is the formatter's specified layout, so the reason does not hold. Leaving the example out is still fine, for the reason given under Breaking inputs. Recommend leaving it out.

### Issue goals

_N/A_ (settled in `01-skeptic.md`).

### Architecture findings

_None_ new. The whole-type formatting in `TypeMemberOrderingCodeFixProviderBase` is the one cause of three symptoms. The run does not change that code path (decision 4 only adds the guard), so I record it as a follow-up candidate rather than an architecture finding of this run.

### Follow-up candidates

- **Merge into the existing candidate "RH7102 and RH7109 fixes rewrite an expression-bodied method they do not move":** the member-moving fixes (RH7102, RH7103, RH7109 through `TypeMemberOrderingCodeFixProviderBase:98-108`) format the whole containing type after the move. Symptoms:
  - (a) a method they neither move nor cross gets a block body (SHARED-FIX-006);
  - (b) a line break inside the moved member is joined (`private readonly int _b` / `;` → `private readonly int _b;`), against the *Moved code* bullet of SHARED-FIX-007;
  - (c) a continuation line of the moved member gets the formatter's column (8 → 4) although its nesting is unchanged. The *Moved code* bullet reads either way, so this is an output question for that follow-up: keep column 8, or take the formatter's column 4.
- Seen, not a candidate: whole-document formatting inserts a blank line between `private readonly int _b` and a `// note` line that precedes a lone `;` inside the declaration. The RH7109 fix keeps the moved member's lines, as SHARED-FIX-007's *Moved code* bullet requires, so no fix is at fault. I did not check FMT-BLANK for it.

## thoenissen/Reihitsu#1616

### Premise

Settled in `01-skeptic.md` and by the maintainer's answers 1A and 2A. The change doc correctly records RH2001-004 (changed) and RH2001-009 (added). Its "Before" outputs match `01-skeptic.md` fixtures b, f, m, and j. Its alternatives match the earlier question's options (B = `private static /* a */ readonly`, C = `private /* a */ static readonly`), and its claim about SHARED-MODORDER-003 is accurate. Once the open question below is answered, the change doc must list that case under *Visible output changes* (option A changes today's `private /* a */     static readonly int _value;`).

### Findings against the spec diff

Fixtures and their `apply-fix.sh RH2001` results:

| # | Input (in `class C`) | Today | Spec diff decides? |
|---|---|---|---|
| u1 | `private unsafe string Value { get; }` (already in order) | `private readonly unsafe string _value;` | no: the RH2001-004 paragraph applies only "to modifiers that are not in the order of RH7105-001" |
| u2 | `private /* a */ unsafe string Value { get; }` | `private /* a */ readonly unsafe string _value;` | no: RH2001-009 places `readonly` behind the comment only "when RH7105-001 puts it behind the modifiers that follow the comment", and here it goes in front of `unsafe` |
| e | `static private /* a */ int Value { get; }` | `private /* a */     static readonly int _value;` | no: the spec author's open question |
| f | `private static /* a */ int Value { get; }` | `private static /* a */ readonly int _value;` | no: RH2001-007 leaves a comment in front of the type uncovered; RH2001-004 excludes it |
| c | `private` / `#pragma warning disable CS0169` / `static int Value { get; }` | `#pragma` kept between them, `static readonly int _value;` | no: RH2001-009 frees only a *comment* between modifiers that keep their order |
| d | `private` / `#if DEBUG` / `new` / `#endif` / `static int Value { get; }` | directive kept, `static readonly int _value;` | no (as c) |
| h | `private // a` / `static int Value { get; }` | `private // a` / `static readonly int _value;` | yes (RH2001-009 paragraph 2) |
| g | `unsafe private string Value { get; }` | `private readonly unsafe string _value;` | yes (RH2001-004) |

Reachability: all of these come from the RH2001 code fix, single fix and Fix All. `unsafe` on a private get-only auto-property of reference type is legal C# under `AllowUnsafeBlocks`.

1. **The precondition of the new RH2001-004 paragraph is narrower than the fix (u1).** The code sorts every get-only property's modifiers with `readonly` added (`OrderModifiersForRh7105(modifiers.Add(readonly))`, `RH2001…CodeFixProvider.cs:198-203`), also when they are already in order. For `unsafe`, `readonly` then lands in the middle of the list. An implementer who reads the precondition literally could append `readonly`, producing `private unsafe readonly string _value;`, which RH7105 reports (FC-007). Recommend: "When the fix adds `readonly`, it MUST write the field's modifiers, `readonly` included, in the order of RH7105-001 …", without "to modifiers that are not in that order". Internal: this states today's output, and no comment-free output changes.
2. **`readonly` at the comment's boundary is undecided (u2, e, f).** One case covers all three: RH7105 puts `readonly` behind every modifier written in front of the comment and in front of every modifier written behind it, or in front of the type when none is behind it. The spec author's open question covers only e. Its recommended option A ("as for the already-ordered `private static /* a */`") rests on f, which no requirement decides either. An answer for e alone would leave f and u2 open, or contradict them. This is an output question (below), which replaces or extends the spec author's question 1.
3. **A directive between modifiers that keep their order (c, d).** RH2001-009's coverage sentence claims "comments and directives between the modifiers", but its second paragraph frees only a comment. Today the fix is offered and the directive keeps its place, which matches SHARED-FIX-003 (nothing crosses the directive). Recommend: "A comment or directive between two modifiers that keep their order …". Internal: it states today's output.
4. **The leading-trivia list in the RH2001-004 paragraph leaves out a comment in front of the first modifier on its line** (`/* lead */ static private int Value { get; }`). That trivia is SHARED-MODORDER-002's own example. The em-dash list reads as exhaustive. Recommend adding "a comment in front of it on its line" or dropping the list. Internal. Add the fixture to the tests, as RH2001-004 covers it ("before its first token").

### Mechanism and candidates

_N/A_ (settled in `01-skeptic.md`).

### Breaking inputs

As in `01-skeptic.md` (a, i, k, l, j, c, d, e, n; counterparts g, h, o, p), the spec examples, and in addition:

- u1 and g: today's output, in RH7105 order;
- c, d, h: today's output, the directive or comment kept in place;
- `/* lead */ static private int Value { get; }`: required, `/* lead */ private static readonly int _value;`;
- u2, e, f: wait for the answer below.

All run under LF and CRLF.

### Surfaces

Unchanged from `01-skeptic.md`. Spec: RH2001-004 (precondition, leading-trivia list), RH2001-009 (directive kept in order, boundary placement after the answer). Change doc: add the boundary case after the answer.

### Simpler option

_None._

### Output questions

**1 · RH2001 · Private auto-implemented properties should not be used — `readonly` lands next to a comment behind the modifiers**

This replaces the spec author's question 1, which covers only the first input. No modifier crosses the comment in any of these inputs. The question is only whether the added `readonly` goes behind or in front of the comment.

Input:
```csharp
class C
{
    static private /* a */ int First { get; }

    private static /* b */ int Second { get; }

    private /* c */ unsafe string Third { get; }
}
```

Before (`main`):
```csharp
class C
{
    private /* a */     static readonly int _first;

    private static /* b */ readonly int _second;

    private /* c */ readonly unsafe string _third;
}
```

After, A (recommended):
```csharp
class C
{
    private static /* a */ readonly int _first;

    private static /* b */ readonly int _second;

    private /* c */ readonly unsafe string _third;
}
```

After, B:
```csharp
class C
{
    private static readonly /* a */ int _first;

    private static readonly /* b */ int _second;

    private readonly /* c */ unsafe string _third;
}
```

After, C: no fix offered for `First`; `Second` and `Third` as before.

Why A: it keeps today's output for `Second` and `Third`, and changes only `First`, whose output today is broken.

### Internal choices

Items 1, 3, and 4 under *Findings against the spec diff*: each states today's output, and I recommend the wording given there.

### Issue goals

_N/A_ (settled in `01-skeptic.md` and by the answers).

### Architecture findings

_None_ new.

### Follow-up candidates

_None._

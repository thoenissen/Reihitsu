Role: spec author
Commit: 930cb08 (code statements: unchanged `main` behavior; the amended spec diff and change doc are in the commit that adds this handover)
Verdict: change doc written

The prompt carried the lead's decisions (rows 2–7 of `00-lead.md`), which are inputs of this round, and no intended fix, suspected cause, or hoped-for verdict. New "today" outputs below, and every Before of the questions, were produced with `scripts/format.sh` on scratch fixtures in the session scratchpad (not committed).

## What changed in this round

- **Q10 replaced** by the skeptic's full class C list, plus members I found: a parameter modifier in front of a tuple type (`params⏎(int, int)[] a`, `this⏎(int, int) t`, `ref⏎(int, int) u`, `in⏎(int, int) v`, `ref readonly⏎(int, int) w` — today joined with a space, probed; under 1B without a rule `params(int, int)[] a`). The recommendation flips to **B** (rules), because the full list holds forms such as `not"x"`, `out*p`, `p.Q()with`, `delegate*unmanaged`. Written as four new requirements: FMT-SPACE-024 (keyword family), -025 (`with`), -026 (lambda return type), -027 (function-pointer `*`).
- **OQ-2 and OQ-3 folded into questions 7 and 6** as option C each, so one answer covers each family. Further class D members I found and added to FMT-SPACE-021: the designation behind a recursive or list pattern (`o is { }s`, `o is [1]l` — kept tight today on one line, probed; not reached by a join). Further class D member added to FMT-SPACE-024: the `in` of a `foreach` (`foreach (var x in(xs))` — kept today, probed), the same family as the query `in`.
- **Change doc**: `What`, `Why`, entries 5, 6, 9, 10, new entries 11–13, `Unchanged`, `Decisions`, `Alternatives` rewritten. The `Why` no longer claims that the changed pairs were decided; it names which were (#920, #912, #958, #1514) and says the rest are asked. The attribute-target colon is recorded as undecided. Entry 10 lists the four modifier pairs and the RH5408 path.
- **FMT-BREAK-031**: the attribute-list merge is a recorded exception pointing at thoenissen/Reihitsu#1492 (row 3).
- **FMT-BREAK-024**: "MUST stay as written" → "MUST stay on its line", spaces by FMT-SPACE-022 (row 4).
- **FMT-SPACE-016, FMT-SPACE-019**: one line each referencing FMT-SPACE-013/014 with the comment example on both sides (class E); not restated.
- Row 7: no spec text; the issue goals are reflected in the change doc only.

## Coverage of the skeptic's classes

| Class | Member | Covered by |
|---|---|---|
| A | `protected⏎internal`, `internal⏎protected`, `private⏎protected`, `protected⏎private` (whole document, CLI, RH5408) | FMT-BREAK-031 (first sentence: every join asks the helper); change doc entry 10; not a question |
| A | binary-operator joins `-`/`-b`, `+`/`+b`, `+`/`++b`, `&`/`&b`, `/`/`*p`, `as`/`string`, `is`/`int` (phase text) | FMT-BREAK-031 and its example |
| A | list-element and other joins already guarded | FMT-BREAK-031 (unchanged outcome) |
| B | property arrow, one-line accessor list, attribute-list merge | FMT-SPACE-017 / Q2; FMT-SPACE-022 / Q8; FMT-BREAK-031 exception |
| C | `static (`, `async (`, `in (`, `ref (`, `out *p`, `ref *p`, keyword + `@x` (`await`, `ref`, `out`, `in`, `static`, `async`), `await (`, `var (`, `stackalloc [`, `not` + `"x"`/`'c'`/`@o`/`-1`/`+1`/`~1`/`> 5`/`< 5`/`>= 5`/`<= 5`, query `in (`/`in "abc"`, parameter modifier + tuple type (new) | FMT-SPACE-024 / Q10 |
| C | explicit lambda return type + `(` (`int`, `int[]`, `List<int>`, `int?`, `(int, int)`) | FMT-SPACE-026 / Q12 |
| C | non-word token + `with` (`p.Q()`, `xs[0]`, `x!`, `}`) | FMT-SPACE-025 / Q11 |
| C | `delegate*` + `unmanaged`/`managed` | FMT-SPACE-027 / Q13 |
| C | `from int?⏎x in xs` | FMT-SPACE-021 / Q7 option C |
| D | `foreach (int?x in xs)`, `catch (Exception?e)`, range variable; designation behind a recursive or list pattern (new) | FMT-SPACE-021 / Q7 option C |
| D | catch filter `)when` | FMT-SPACE-020 / Q6 option C |
| D | `foreach (var x in(xs))` (new) | FMT-SPACE-024 / Q10 |
| E | comments on either side of every new rule's gap | FMT-SPACE-013/014 (referenced in -016, -019); tests per rule (below) |

Checked and not members: `not⏎{ }`, `not⏎[1]`, `p.Q()⏎switch`, `join y in⏎(xs)` are not joined today (probed; layout follow-ups already listed by the lead); a member's own modifiers before a tuple type (`public(int, int) G;`) are never joined and are a different family — excluded explicitly in FMT-SPACE-024 with examples on both sides, and listed as a follow-up candidate.

## Changed requirements

- FMT-SPACE-002 — Whitespace no rule covers is only collapsed — changed (precedence list gains FMT-SPACE-024 to -027)
- FMT-SPACE-016 — Attribute-target colons — changed in this round (comment example)
- FMT-SPACE-019 — Name colons — changed in this round (comment example)
- FMT-SPACE-020 — One space in front of `when` — changed in this round (`catch` filter, on 6C)
- FMT-SPACE-021 — One space between a type or pattern and the name it declares — changed in this round (`foreach`, `catch`, range variable, recursive and list pattern designation, on 7C)
- FMT-SPACE-024 — One space behind a modifier or prefix keyword in front of its operand — added
- FMT-SPACE-025 — One space on each side of `with` — added
- FMT-SPACE-026 — One space between an explicit lambda return type and its parameter list — added
- FMT-SPACE-027 — One space behind a function pointer's `*` in front of its calling convention — added
- `horizontal-spacing.md`, fourth non-goal — `typeof(`/`default(` named as not covered
- FMT-BREAK-024 — Simple auto-property accessor lists are on one line — reworded ("stays on its line")
- FMT-BREAK-031 — A join writes the minimal separator — changed in this round (attribute-list merge exception)
- Unchanged from round 1: FMT-SPACE-012, -017, -018, -022, -023, FMT-BREAK-026, FMT-STRUCT-026
- Change doc `sdd/changes/2026-11/spacing-sole-gap-owner.md`, `Status: Draft`

The spec diff is written on the recommended answer of every question below. Another answer removes or narrows the matching requirement and the change-doc entry; the spec author amends them.

## Examples for the tests

FMT-SPACE-016 to FMT-SPACE-027, FMT-BREAK-031, FMT-BREAK-026, FMT-STRUCT-026, FMT-SPACE-002, and change-doc entry 10.

- Every class C member: joined (the input wrapped inside an argument or parameter) and written on one line, both with the output of the recommended answer — the joined output equals today's.
- Class A: the four accessor modifier pairs through whole-document formatting and through the RH5408 fix (`scripts/apply-fix.sh` gives `invalid-result` today), LF and CRLF; the binary-operator pairs at phase level.
- Class E: for each of FMT-SPACE-016 to -027, a comment the left token carries and a `/** */` the right token carries in the gap; for each joiner, that a comment in the gap still refuses the join.
- Counter-sides (FC-002): FMT-SPACE-024 — `typeof(C)`, `default(int)`, `nameof(a)`, and a member modifier `public(int, int) G;` keep their gap; FMT-SPACE-021 — a member's return type unspecified; FMT-SPACE-020 — the space behind `when` unchanged; the round-1 counter-sides stand.
- FC-025 arrow kinds, FC-027 nesting, FC-028 node-level entry points, FC-032 the full analyzer test project, as the skeptic lists them.

## Output questions

Relay this section verbatim.

Questions 1, 2, 3, and 8 are asked whatever the answers. Questions 4, 5, 6, 7, 9, 10, 11, 12, and 13 arise only with **1B**; with 1A they are void. One answer such as `1B, 2B, 3B, 4B, 5B, 6C, 7C, 8B, 9B, 10B, 11B, 12B, 13B` covers them all; that line is the recommendation. `⏎` in a "joined" input marks where the author broke the line.

**1 · Formatter — the space where two lines are joined into one**

Input:
```csharp
void M
    (int a)
{
    var x = items.Where
        (y => y);
    N(Create
      (1), (int)
      a);
}
```

Before (`main`):
```csharp
void M(int a)
{
    var x = items.Where(y => y);
    N(Create(1), (int)a);
}
```

After, A — always one space:
```csharp
void M (int a)
{
    var x = items.Where (y => y);
    N(Create (1), (int) a);
}
```

After, B (recommended) — no space, unless the two pieces would run together:
```csharp
void M(int a)
{
    var x = items.Where(y => y);
    N(Create(1), (int)a);
}
```

Why B: it keeps every joined output of today; under B, the places where today's join writes a space keep it through questions 4 to 13.

**2 · Formatter — every lambda and expression-body arrow `=>`**

Input:
```csharp
int P =>1;
int A
{
    get =>_p;
    set=>_p = value;
}
Func<int, int> f = y=>y;
var s = x switch
        {
            1=>2,
            _ =>3
        };
```

Before (`main`): unchanged, except `int P =>1;` becomes `int P => 1;`.

After, A — the author's spacing, runs collapsed: unchanged, `int P =>1;` included.

After, B (recommended):
```csharp
int P => 1;
int A
{
    get => _p;
    set => _p = value;
}
Func<int, int> f = y => y;
var s = x switch
        {
            1 => 2,
            _ => 3
        };
```

Why B: you decided one space around every arrow in #958 (for lambdas already in #920).

**3 · Formatter — the colon of an attribute target**

Input (each attribute stands above its declaration):
```csharp
[assembly:CLSCompliant(true)]
[field :NonSerialized]
[return :Obsolete]
[method  :  Obsolete]
```

Before (`main`):
```csharp
[assembly:CLSCompliant(true)]
[field :NonSerialized]
[return:Obsolete]
[method : Obsolete]
```

After, A — no space before, the author's space behind:
```csharp
[assembly:CLSCompliant(true)]
[field:NonSerialized]
[return:Obsolete]
[method: Obsolete]
```

After, B (recommended) — no space before, one space behind:
```csharp
[assembly: CLSCompliant(true)]
[field: NonSerialized]
[return: Obsolete]
[method: Obsolete]
```

Why B: every target then reads the same whatever the author typed (only "alike" was decided, in #912).

**4 · Formatter — the conditional operator `? :` (with 1B)**

Input:
```csharp
var r = b?1:2;
var z = b ?
        1 :
        2;
```

Before (`main`):
```csharp
var r = b?1:2;
var z = b
            ? 1
            : 2;
```

After, A — no rule:
```csharp
var r = b?1:2;
var z = b
            ?1
            :2;
```

After, B (recommended):
```csharp
var r = b ? 1 : 2;
var z = b
            ? 1
            : 2;
```

Why B: you decided `b ? 1 : 2` in #920.

**5 · Formatter — the colon behind a name: named argument, tuple element, subpattern (with 1B)**

Input:
```csharp
M(b:b, c :1);
var t = (A:1, B :2);
var p = o is string { Length:1 };
N(name:
  "x");
```

Before (`main`):
```csharp
M(b:b, c :1);
var t = (A:1, B :2);
var p = o is string { Length:1 };
N(name: "x");
```

After, A — no rule:
```csharp
M(b:b, c :1);
var t = (A:1, B :2);
var p = o is string { Length:1 };
N(name:"x");
```

After, B (recommended):
```csharp
M(b: b, c: 1);
var t = (A: 1, B: 2);
var p = o is string { Length: 1 };
N(name: "x");
```

Why B: you decided `M(b: b)` for named arguments in #920; tuple names and subpattern names are the same kind of label.

**6 · Formatter — the space in front of `when` (with 1B)**

Input:
```csharp
case (1, 2)
    when b:
case "x"when b:
catch (Exception e)when (e != null)
```

Before (`main`):
```csharp
case (1, 2) when b:
case "x"when b:
catch (Exception e)when (e != null)
```

After, A — no rule:
```csharp
case (1, 2)when b:
case "x"when b:
catch (Exception e)when (e != null)
```

After, B — `case` and switch-expression guards:
```csharp
case (1, 2) when b:
case "x" when b:
catch (Exception e)when (e != null)
```

After, C (recommended) — every `when`, the `catch` filter included:
```csharp
case (1, 2) when b:
case "x" when b:
catch (Exception e) when (e != null)
```

Why C: every guard then reads alike; the space behind `when` stays as written in every option.

**7 · Formatter — the space between a type and the name it declares (with 1B)**

Input:
```csharp
void M(int?
       a, int[]b)
{
    int?z = null;
    foreach (int?x in xs) { }
    catch (Exception?e) { }
    N(from int?
      y in xs select y);
    var p = o is { }r;
}
```

Before (`main`):
```csharp
void M(int? a, int[]b)
{
    int?z = null;
    foreach (int?x in xs) { }
    catch (Exception?e) { }
    N(from int? y in xs select y);
    var p = o is { }r;
}
```

After, A — no rule:
```csharp
void M(int?a, int[]b)
{
    int?z = null;
    foreach (int?x in xs) { }
    catch (Exception?e) { }
    N(from int?y in xs select y);
    var p = o is { }r;
}
```

After, B — parameters, tuple elements, local variables, `out` variables, and type patterns:
```csharp
void M(int? a, int[] b)
{
    int? z = null;
    foreach (int?x in xs) { }
    catch (Exception?e) { }
    N(from int?y in xs select y);
    var p = o is { }r;
}
```

After, C (recommended) — every declared name, also behind a `foreach`, `catch`, query, or pattern:
```csharp
void M(int? a, int[] b)
{
    int? z = null;
    foreach (int? x in xs) { }
    catch (Exception? e) { }
    N(from int? y in xs select y);
    var p = o is { } r;
}
```

Why C: every declared name then reads alike, and the joined query keeps its space.

**8 · Formatter — an accessor list on one line**

Input:
```csharp
int P {get;set;}
int Q {[Obsolete] get;set;}
```

Before (`main`):
```csharp
int P { get; set; }
int Q {[Obsolete] get;set; }
```

After, A — no rule:
```csharp
int P {get;set; }
int Q {[Obsolete] get;set; }
```

After, B (recommended):
```csharp
int P { get; set; }
int Q { [Obsolete] get; set; }
```

Why B: today's output for every plain list stays, and the attributed list reads like the others.

**9 · Formatter — a relational pattern `is > 5` (with 1B)**

Input:
```csharp
var a = x is >5 and <10;
var b = N(x is >
          1);
```

Before (`main`):
```csharp
var a = x is >5 and <10;
var b = N(x is > 1);
```

After, A — no rule:
```csharp
var a = x is >5 and <10;
var b = N(x is >1);
```

After, B (recommended):
```csharp
var a = x is > 5 and < 10;
var b = N(x is > 1);
```

Why B: you decided `x is > 5` in #912.

**10 · Formatter — a keyword in front of its operand: `await (t)`, `not "x"`, `out *p` (with 1B)**

Input:
```csharp
N(await
  (t));
N(o is not
  "x");
N(out
  *p);
await(t);
var b = o is not"x";
N(out*p);
```

Before (`main`):
```csharp
N(await (t));
N(o is not "x");
N(out *p);
await(t);
var b = o is not"x";
N(out*p);
```

After, A — no rule:
```csharp
N(await(t));
N(o is not"x");
N(out*p);
await(t);
var b = o is not"x";
N(out*p);
```

After, B (recommended):
```csharp
N(await (t));
N(o is not "x");
N(out *p);
await (t);
var b = o is not "x";
N(out *p);
```

The same holds behind `static` and `async` of a lambda (`static (int v) => v`), behind `ref`, `in`, `readonly`, `scoped`, `this`, and `params` of an argument or parameter (`ref (a)`, `params (int, int)[] a`), behind `stackalloc` (`stackalloc [] { 1 }`), behind `var` in `var (x, y)`, and behind the `in` of `foreach` and of a query (`in (xs)`). `typeof(C)`, `default(int)`, and `public(int, int) G;` keep their spacing.

Why B: A writes forms such as `not"x"` and `out*p` that read as a different construct.

**11 · Formatter — `with` (with 1B)**

Input:
```csharp
var c = p.Q()with { X = 1 };
N(p.Q()
  with { X = 1 });
```

Before (`main`):
```csharp
var c = p.Q()with
             {
                 X = 1
             };
N(p.Q() with
        {
            X = 1
        });
```

After, A — no rule:
```csharp
var c = p.Q()with
             {
                 X = 1
             };
N(p.Q()with
       {
           X = 1
       });
```

After, B (recommended):
```csharp
var c = p.Q() with
              {
                  X = 1
              };
N(p.Q() with
        {
            X = 1
        });
```

Why B: `with` is an operator written as a word and reads as one only with a space on each side.

**12 · Formatter — a lambda with an explicit return type (with 1B)**

Input:
```csharp
N(List<int>
  (x) => x);
N(List<int>(x) => x);
```

Before (`main`):
```csharp
N(List<int> (x) => x);
N(List<int>(x) => x);
```

After, A — no rule:
```csharp
N(List<int>(x) => x);
N(List<int>(x) => x);
```

After, B (recommended):
```csharp
N(List<int> (x) => x);
N(List<int> (x) => x);
```

Why B: without the space the lambda reads like a constructor call.

**13 · Formatter — a function pointer `delegate* unmanaged` (with 1B)**

Input:
```csharp
void F(delegate*
       unmanaged[Cdecl]<int, void> f, delegate*managed<int> g)
```

Before (`main`):
```csharp
void F(delegate* unmanaged[Cdecl]<int, void> f, delegate*managed<int> g)
```

After, A — no rule:
```csharp
void F(delegate*unmanaged[Cdecl]<int, void> f, delegate*managed<int> g)
```

After, B (recommended):
```csharp
void F(delegate* unmanaged[Cdecl]<int, void> f, delegate* managed<int> g)
```

Why B: `delegate* unmanaged` is the form C# itself documents, and today's joined output keeps it.

Not a question, for the lead's record: two accessor modifiers on separate lines (`protected⏎internal set;`) today give `protectedinternal set;`, which does not compile, through whole-document formatting, the CLI, and the RH5408 fix; both answers of question 1 fix it.

## Internal choices

- OQ-2 and OQ-3 are folded into questions 7 and 6 as option C instead of standing as dependent questions; alternative: separate questions 14 and 15 that arise only with 7B and 6B.
- Q10's families are one question with one answer; alternative: one question per keyword. Recommended because the maintainer would answer them alike and the list is long.
- FMT-SPACE-024 covers the modifiers of an argument, parameter, type, or expression, and excludes member and local-declaration modifiers explicitly (`public(int, int) G;`); alternative: include every modifier, which widens the run beyond any pair a join reaches.
- FMT-SPACE-021 and -024 include one-line-only family members (`foreach`/`catch` variables, pattern designations, the `foreach` `in`) on P34; alternative: only what a join reaches — kept as option B of question 7.
- Class E: FMT-SPACE-016 and -019 reference FMT-SPACE-013/014 with an example rather than restating them; the other new rules rely on the shared requirements without an example line.
- The question set shows statement-level fragments instead of whole compilable files, so each question stays short; the change doc carries the full inputs.

## Follow-up candidates

- **A member's modifier in front of a tuple type** — `public(int, int) G;` and `static(int, int) F() => default;` keep the author's gap (reachable by whole-document formatting; never joined). Readings: A — keep; B — one space behind every member modifier. Recommendation: B, as part of thoenissen/Reihitsu#1440's keyword list.
- The lead's four candidates stand unchanged.

Role: skeptic
Commit: 1ea832b
Verdict: output question

My prompt carried no intended fix or verdict. Nothing in it needed ignoring.

## Premise

The case is real, but the issue's own reason for a change is gone.

- **Reachability.** Two callers reach a comment-free `private int⏎    _g, _h;` inside a class: whole-document formatting (`scripts/format.sh`) and the RH7101 fix (`scripts/apply-fix.sh RH7101`). The RH7101 fix formats the split field the same way under LF and CRLF (FMT-PIPE-029).
- **Mechanism.** `FieldDeclarationSplitTransform.CreateSplitField` (`src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs`) copies the declaration's attributes, modifiers, and type with all their trivia. The end-of-line in the type's trailing trivia is part of that copy. The transform drops only a later declarator's leading trivia and a later field's declaration-level leading trivia. No FMT-BREAK requirement joins the gap behind a field's type, so the copied break survives.
- **The issue's reason no longer applies.** The issue recommended joining the break on later fields because the break "exists to hold the comment in front of `_x`". That comment input is now refused by FMT-UNSUP-004. The remaining comment-free case has no comment to explain the break. When an author breaks behind the type, the usual reason is a long type, and the repeated type is just as long (see *Why A*).
- **Stale comment on the issue.** The 2026-11-16 inventory comment on thoenissen/Reihitsu#1587 says the split fields "do not repeat the line break". That is wrong: they do repeat it. I measured this below, and the spec author measured the same.
- **The spec decides it in spirit.** FMT-STRUCT-021 says each field repeats "the original attributes, modifiers, and type", and its `Why:` says "Repeating the attributes and modifiers keeps every field exactly what it was". FMT-STRUCT-026 says "The trivia a transform carries over keeps what the author wrote", and its `Why:` gives every line break to FMT-BREAK. Today's output is the literal reading of both. Neither names the trivia behind the type explicitly, which is the gap the issue asks to close.

### Sibling constructs: the break behind a type is kept everywhere except in parameter lists

I ran `scripts/format.sh` on the fixture below at commit 1ea832b.

```csharp
using System;
using System.Collections.Generic;

class C
{
    private int
        _g, _h;

    private Dictionary<string, List<int>>
        _map1, _map2;

    private const int
        A = 1, B = 2;

    private int
        _single;

    public int
        P { get; set; }

    public event Action
        E;

    public event Action
        E1, E2;

    public int
        Get()
    {
        return 1;
    }

    public
        static void N()
    {
    }

    void Q(int
               a, int b)
    {
        int
            x = 1;
        int
            y = 1, z = 2;
        Console.WriteLine(a + b + x + y + z);
    }
}
```

Observed output. Continuation lines sit at the member or statement column; that column is thoenissen/Reihitsu#953's question.

```csharp
using System;
using System.Collections.Generic;

class C
{
    private int
    _g;
    private int
    _h;

    private Dictionary<string, List<int>>
    _map1;
    private Dictionary<string, List<int>>
    _map2;

    private const int
    A = 1;
    private const int
    B = 2;

    private int
    _single;

    public int
    P
    {
        get;
        set;
    }

    public event Action
    E;

    public event Action
    E1, E2;

    public int
    Get()
    {
        return 1;
    }

    public
    static void N()
    {
    }

    void Q(int a, int b)
    {
        int
        x = 1;
        int
        y = 1, z = 2;

        Console.WriteLine(a + b + x + y + z);
    }
}
```

What this shows:
- A line break between a declaration's type (or its modifiers) and its name is kept on every kind of declaration: an unsplit field, a const, an event field, a property, a method, and a local.
- Only a parameter joins it, through FMT-BREAK-015.
- The split field keeps the break on every field it creates. Today's output is therefore consistent with every sibling.

I also ran `scripts/apply-fix.sh RH7101` on the `Dictionary<string, List<int>>⏎        _map1, _map2;` field. Under both LF and CRLF it gave `fixed (1 action(s), 1 iteration(s))` with the same two fields as above.

## Mechanism and candidates

_N/A_: this is an output question, not a bug. For completeness, here are the inputs the split copies.
- The split is limited to `FieldDeclarationSyntax` whose parent is a class, struct, record, interface, or union (`Splits`).
- Event fields and locals are never split (FMT-STRUCT-021). The fixture above confirms that `E1, E2` and `y = 1, z = 2` stay whole.
- Every line break inside the copied head reaches the later fields the same way: behind the type, between modifiers, and between a modifier and the type.
- Comments, directives, and disabled text inside the head of a multi-declarator field are refused (FMT-UNSUP-004, FMT-UNSUP-010). The head can therefore only carry whitespace and line breaks.
- A break between an attribute list and the modifiers is FMT-BREAK-027's to decide and is not in question.

## Breaking inputs

With option A, nothing changes in production. The test implementer pins today's output and should cover:
- the issue's input `private int⏎        _g, _h;`;
- a long generic type (`Dictionary<string, List<int>>⏎        _map1, _map2;`);
- a declarator with an initializer behind the break (`private int⏎    _g = 1, _h = 2, _i;`, from the spec author's table);
- a break between modifiers (`private⏎    int _g, _h;`);
- the attribute-plus-head case from the spec author's table;
- each of these under LF and CRLF (FC-017, FC-033), through both the formatter and RH7101 (FMT-PIPE-029; `RH7101DoNotCombineFieldsFormatterTests`);
- a second pass, to show the output is stable (FC-010).

The existing untagged test `FieldDeclarationSplitTests.WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged` already pins the first case.

With option B or D (below), the classes that reach the change are:
- **FC-016**: every head gap is a sibling. That means behind the type, between modifiers, between the last modifier and the type, and between an attribute list and the modifiers (which FMT-BREAK-027 re-breaks). All of them must be covered.
- **FC-019, FC-020, FC-021**: these cannot be reached inside a multi-declarator head, because FMT-UNSUP-004 and FMT-UNSUP-010 refuse those inputs. One refused-input test per class shows the join never meets them.
- **FC-010** under CRLF, **FC-028** (`FormatNode` against the document entry point), **FC-007** (RH7101 converges in one pass), and **FC-042** (no new guard that only serves a refused shape).

## Surfaces

| Surface | Option A | Option B or D |
|---|---|---|
| `FieldDeclarationSplitTransform` (structural phase) | must stay | must change |
| FMT-STRUCT-021 / FMT-STRUCT-026 spec text | must change (clarification only) | must change, with a change doc |
| Line-break, indentation, and spacing phases | not involved | not involved |
| RH7101 analyzer | not involved | not involved |
| RH7101 code fix and Fix All | must stay (output follows the formatter) | output changes with the formatter (FMT-PIPE-029); no fix code changes |
| `Reihitsu.Core` | not involved | not involved |
| Rule page `RH7101.md` | not involved | not involved |
| Tests | add `[Requirement]` tags; new LF/CRLF cases | change the existing expected value; new cases |

## Refuse option

Leaving the construct as written is option A. It is today's output, so it costs no production lines.

Declaring a shape unsupported is not an option here. The shape would be "a line break between the type and the first declarator of a multi-declarator field", and declaring it would delete no special case: the transform has no guard for it. A shape that deletes nothing is not declared (`AGENTS.md`, *Special cases do not grow the source*). It would also refuse code people write with long types.

The class is rare: it needs a combined field together with a break behind the type. B or D would cost a new trivia rewrite in the structural phase that drops carried-over line breaks. That contradicts FMT-STRUCT-026 ("keeps what the author wrote"; line breaks belong to FMT-BREAK), so the requirement would have to be amended. The structural phase would then own a line-break decision, and if a later FMT-BREAK join for declaration heads were added (option C), the same gap would have two owners.

## Output questions

I challenged the spec author's question. Its premise, its *Before*, and the A and B outputs match my measurements. I have three corrections.

1. **Option C does not belong in this question.**
   - The fixture shows the break behind a type is kept on properties, methods, events, consts, and locals as well. A join for fields only would make fields the odd one out.
   - C therefore sets a principle for every declaration head, which is a construct outside the field split (P11: a different construct, not this transform's gap).
   - Under *What reaches the maintainer*, a case outside the run's issues is not asked in the run. If the lead wants it raised at all, it becomes a follow-up candidate (below).
   - The question for #1587 should offer A, B, and D.
2. **One option is missing: D, which joins the head on every field of a split declaration, the first included.** Unsplit fields stay untouched. Output for the issue's input, with the same lines as the existing FMT-STRUCT-021 example:
   ```csharp
   class C
   {
       private int _g;
       private int _h;
   }
   ```
   D gives the fields of one declaration the same layout, which B does not. In exchange, it drops a line break the author wrote even on the first field, and a split field then differs from the same field written alone. It needs the same structural-phase line-break decision as B. I recommend listing D so that the options are complete, and still recommending A.
3. **Why A, with a stronger reason.** The spec author's reason holds. The decisive point is what the break is for. Without a comment, the only reason to break behind a type is its length (`private Dictionary<string, List<int>>⏎_map1, _map2;`). The repeated type is exactly as long, so B and D write the long line the author avoided, and the formatter has no line-length rule that would wrap it again (FMT-BREAK non-goals).

The question as I would ask it (the lead owns the wording):

**1 · Formatter field split and RH7101 · Do not combine fields — a line break written behind the field's type**

Input:
```csharp
using System.Collections.Generic;

class C
{
    private Dictionary<string, List<int>>
        _map1, _map2;
}
```

Before (`main`):
```csharp
using System.Collections.Generic;

class C
{
    private Dictionary<string, List<int>>
    _map1;
    private Dictionary<string, List<int>>
    _map2;
}
```

After, A (recommended): same as Before. Every field repeats the head as written.

After, B:
```csharp
using System.Collections.Generic;

class C
{
    private Dictionary<string, List<int>>
    _map1;
    private Dictionary<string, List<int>> _map2;
}
```

After, D:
```csharp
using System.Collections.Generic;

class C
{
    private Dictionary<string, List<int>> _map1;
    private Dictionary<string, List<int>> _map2;
}
```

Why A: the author broke the line because the type is long, the repeated type is just as long, and every other declaration keeps that break today.

The declarator column, at the member column in A and B, is thoenissen/Reihitsu#953's question and is the same in every option. Showing a long type keeps the reason for the break visible. The spec author's `int` input works too.

I did not run B and D through a modified formatter. Their outputs are the spec author's measured stable forms for `int` (B), and the shape of the FMT-STRUCT-021 example (D), carried over to this type.

## Internal choices

- **If A is chosen**, the lane narrows to tests: there is no production change and no change doc. The clarification is better written into FMT-STRUCT-021 itself, naming the head's whitespace and line breaks as part of what each field repeats, than as a new requirement. FMT-STRUCT-022 already lists what only the first field keeps, so the two together state the whole copy. Either form gives the same output.
- **If B or D is chosen**, FMT-STRUCT-026 must be amended. It currently says carried trivia keeps what the author wrote. Writing the exception only into FMT-STRUCT-021 would leave the two requirements contradicting each other.

## Issue goals

- **"FMT-STRUCT-021 states what trivia the repeated type carries"**: keep it. It serves the issue whichever option is answered.
- **"with a test for this input"**: change it. "This input" is the comment variant, which is now FMT-UNSUP-004's refused shape and is covered there. The test should use the comment-free input. This changes no visible output.
- **The issue's recommended "After"** (later fields joined, first field kept) is the spec author's B. Its stated reason, the comment, no longer applies. Because it would change visible output, it is decided by the output question.

## Architecture findings

_None._ Today, one component (the structural phase's copy) decides the head trivia, and no other phase decides that gap. B or D would put a line-break decision in the structural phase. That is not a finding today, but it is a cost to weigh (see *Refuse option*).

## Follow-up candidates

1. **A line break between a declaration's type and its name is kept everywhere except in parameter lists.** This is option C, generalized. It is outside #1587, and I would file it only if the lead judges the output wrong. Input: the sibling fixture above. Before: as observed. After, if joined: `public int P { get; set; }`, `public int Get()`, `int x = 1;`, `private int _single;`. No spec decides this gap today. FMT-BREAK-015 joins it only inside parameter lists. Whether it is a question at all is the lead's call; I have no recommendation that the current output is wrong beyond the #953 column.
2. **FMT-BREAK-024 has two readings when a property's type is followed by a line break.**
   - Input:
     ```csharp
     class C
     {
         public int
             P { get; set; }
     }
     ```
   - Today the formatter lays the accessor list over four lines (`public int⏎P⏎{⏎get;⏎set;⏎}`, measured above), although the author wrote it on one line.
   - FMT-BREAK-024 says a simple auto-property's accessor list "MUST be on one line together with the declaration".
   - Reading 1: the list stays on the name's line (`public int⏎P { get; set; }`). Reading 2: today's expansion.
   - `scripts/apply-fix.sh RH5408` reports `no-diagnostic` on both the input and today's output, so no analyzer disagrees.
   - This is reachable through whole-document formatting. I recommend reading 1, because the author's one-line list should not grow without a reason. It is outside the run's issue, so it would be an `agent-filed` follow-up in output-question form.

Fixtures were in the scratchpad, outside the repository, and are deleted.

Role: spec author
Commit: deaa98d
Verdict: change doc written (#1762: change doc written; #1763: change doc written)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The cause tables in both issues (which rewriters miss unions, which rule covers `with`) were read as hypotheses and checked against today's CLI output; they were not taken over as instructions.

How the "before" outputs were established: `Reihitsu.Cli` built in Debug at `deaa98d` and run (default language version, C# 15) over scratch files with the issues' inputs and the variants below; class and record analogs were run the same way to establish each "after" a class already gets. A second pass over the union output changed nothing (today's union output is idempotent).

## thoenissen/Reihitsu#1762

Change doc: `sdd/changes/2026-11/union-declaration-layout.md` (Status: Draft).

### Changed requirements

- FMT-BREAK-002 — Braces start their own line — changed: names union declarations among the types; adds a union example.
- FMT-BREAK-007 — One member per line — changed: names the body of a union declaration; adds a union example.
- FMT-BREAK-011 — Parameter lists hug their declaration — changed: adds the case-type list of a union to the list of parameter lists; adds an example (opening parenthesis joins the name, commas end their lines, wrapped list aligned under the first case type). `Uses` of `line-breaks.md` gains FMT-INDENT-012, which the example references.
- FMT-BREAK-023 — Each generic constraint starts its own line — changed: names union declarations, and states "also on a type declared with a semicolon" (a class gets this today: `class C2<T>(int a,string b)where T:class;` becomes two lines); adds a union example.
- FMT-STRUCT-017 — Empty types use a semicolon declaration — changed: adds union declarations from C# 15 on; adds an example and a sentence that FMT-STRUCT-018 keeps a commented union body's braces.
- FMT-STRUCT-021 — One field per declaration — changed: adds union declarations to the owners whose fields are split; adds an example.
- FMT-PIPE-031 — The formatter answers whether it splits a field declaration — changed: the member condition names union declarations.
- RH7101-002 — Event fields and locals are not reported — `Why:` text only: "the formatter splits only the fields of a class, struct, record, interface, or union declaration". The normative sentence is unchanged.

No new requirement for #1762. Blank lines, horizontal spacing, and indentation need no spec change: their specs name no type kinds, and today's output for a union written with its braces already on their own lines is laid out as a class's (members one level in, `where` clauses one level in).

### Examples for the tests

FMT-BREAK-002 (union example), FMT-BREAK-007 (union example), FMT-BREAK-011 (union example), FMT-BREAK-023 (union example), FMT-STRUCT-017 (union example), FMT-STRUCT-021 (union example), FMT-PIPE-031 (a union field: `SplitsFieldDeclaration` returns true). The change doc's six visible output changes are the regression inputs, including U4 from the issue (a member that gets a block body inside a one-line union) and the commented empty body `union U9(int, string) { /* reserved */ }`, which keeps its braces. RH7101: the fix is now offered on `public static int A = 1, B = 2;` inside a union (RH7101-004 with the union as context); the analyzer reports that field before and after.

### Visible output changes

All six are listed with input, before, and after in the change doc:

1. The braces and members of a union start their own lines (issue's U1, U4).
2. The `(` of the case-type list joins the union's name. Input `union U2⏎    (int, string);`; today `union U2⏎(int, string);`; after `union U2(int, string);`.
3. A wrapped case-type list keeps the primary-constructor layout: no change. Listed because the issue asks it.
4. Each `where` clause of a union starts its own line (issue's U6: today `)where T : class`).
5. An empty union body becomes `;` (both `{ }` on one line and over two lines).
6. A combined field in a union is split by the formatter, and the RH7101 code fix is now offered for it (today: reported, no fix).

### Output questions

**1 · Formatter · Wrapped case-type list of a union**

Input:
```csharp
internal union Nested(int,
    string,
    long);
```

Real output today (`main`):
```csharp
internal union Nested(int,
                      string,
                      long);
```

Why it is a question: the issue asks it; the case-type list is a `ParameterListSyntax`, so it gets the parameter-list layout today, but a list of types could also be read as a type-argument list, which FMT-BREAK-014 always joins. Requirement IDs: FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-014.

A (recommended), parameter-list layout, as today:
```csharp
internal union Nested(int,
                      string,
                      long);
```

B, joined like a type-argument list:
```csharp
internal union Nested(int, string, long);
```

Recommendation: A — a union can list many case types, and the list already behaves as a parameter list in every other respect; it needs no new code. The spec is written on A.

**2 · Formatter · Empty union body**

Input:
```csharp
public union U2(int, string) { }
```

Real output today: unchanged, `public union U2(int, string) { }`.

Why it is a question: the issue asks it; FMT-STRUCT-017 lists the kinds that lose an empty body, and a union is not among them. Requirement IDs: FMT-STRUCT-017, FMT-STRUCT-018.

A (recommended):
```csharp
public union U2(int, string);
```

B, braces kept and placed:
```csharp
public union U2(int, string)
{
}
```

Recommendation: A — every other type with a semicolon form loses its empty braces, and `union Pet(Cat, Dog);` is the common form. The spec is written on A.

**3 · Formatter and RH7101 code fix · the remaining union layout (changes 1, 2, 4, 6 of the change doc)**

Today's outputs and the outputs after are in the change doc; each after is the output a class gets today. Recommendation: accept all four. Requirement IDs: FMT-BREAK-002, FMT-BREAK-007, FMT-BREAK-011, FMT-BREAK-023, FMT-STRUCT-021, FMT-PIPE-031, RH7101-004. The lead may present these in its batch as one output question per change.

### Internal choices

- No union-specific requirement; the existing type-declaration requirements name unions and gain union examples, so a later rule about type declarations is read for unions too. Alternative: one new FMT-BREAK requirement "a union is laid out as a type declaration".
- FMT-STRUCT-017 gates the union semicolon form on C# 15 (the version that introduces unions), matching the one-version-per-kind shape; the gate never refuses a union the formatter sees. Alternative: no gate.
- RH7101-001 (which kinds the analyzer reports) is not changed, though the analyzer already reports union fields; analyzer behavior for unions is thoenissen/Reihitsu#1767's. Only RH7101-002's `Why:` sentence, which states the formatter's split, is corrected.
- No rule page changes: the analyzer's reports do not change for the normal case.

### Non-goals (for the run, not written into specs)

- Analyzer rules for unions (RH5404's registration, RH5412–RH5416-like empty-body reports, attribute rules, RH7101-001's kind list): thoenissen/Reihitsu#1766 and thoenissen/Reihitsu#1767.
- The `closed` and `safe` modifiers (thoenissen/Reihitsu#1764).
- A semicolon of a type declaration written on its own line (`union U(int, string)⏎;`): a class keeps it there today too.
- A base list that starts its own line (`union U(int, string)⏎    : I`): it lands at the declaration's column, as a class's does today.

### Follow-up candidates

- **The attribute lists of a union are not split (FMT-STRUCT-028).** Input `[Serializable, Obsolete] public union U1(int, string);` stays `[Serializable, Obsolete]⏎public union U1(int, string);`, while `[Serializable, Obsolete] public struct S1;` becomes `[Serializable]⏎[Obsolete]⏎public struct S1;`. FMT-STRUCT-028 names "a type", so this differs from an unchanged requirement, but the cause is the shared target resolver (`AttributeTargetUtilities`, SHARED-ATTR-001), which names no target for a union; giving it one (the struct target, as a union compiles to a struct) also changes which RH55xx attribute rules report a union's lists, which is analyzer output and thoenissen/Reihitsu#1767's territory. Recommendation: add it to thoenissen/Reihitsu#1767 rather than this run; the lead decides whether it is in scope.

  Input:
  ```csharp
  [Serializable, Obsolete] public union U(int, string);
  ```
  Today:
  ```csharp
  [Serializable, Obsolete]
  public union U(int, string);
  ```
  Reading of FMT-STRUCT-028 as for a struct:
  ```csharp
  [Serializable]
  [Obsolete]
  public union U(int, string);
  ```

## thoenissen/Reihitsu#1763

Change doc: `sdd/changes/2026-11/with-element-spacing.md` (Status: Draft).

### Changed requirements

- FMT-SPACE-028 — No space between the `with` of a collection-expression element and its argument list — added (ledger updated).
- FMT-SPACE-002 — Whitespace no rule covers is only collapsed — changed: the rule order lists FMT-SPACE-028 in the `with` slot, `with` (FMT-SPACE-025, FMT-SPACE-028). No other rule decides the pair `with` `(` of a `with` element, so the position decides nothing.
- FMT-BREAK-022 — Wrapped collection expressions and patterns get one element per line — clarified: a `with(…)` element is an element like any other; adds an example. The example's output was confirmed with today's CLI (no output change).

### Examples for the tests

FMT-SPACE-028 (its example: spaces around and inside, an empty `with ()`, a named argument, and a comment in the gap), FMT-BREAK-022 (the `with(…)` example — the issue's "Done when" asks for this regression test).

`RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails until a test tags FMT-SPACE-028; the other spec architecture tests pass (`scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 50 passed, 1 failed with only that finding).

If the implementation adds a new spacing-rule type, it has to be added to the `Code owners` of `sdd/specs/formatter/horizontal-spacing.md` and to `sdd/specs/README.md`; extending `WithSpacingRule` needs neither.

### Visible output changes

1. `with (x)` becomes `with(x)`, and `with ()` becomes `with()`.
2. `with /* size */ (4)` becomes `with /* size */(4)` (FMT-SPACE-013 applies the decision behind the comment, as for every rule).

No change for the multi-line layout.

### Output questions

**4 · Formatter · Space between `with` and `(` in a collection expression**

Input:
```csharp
HashSet<string> set = [with ( StringComparer.OrdinalIgnoreCase ) , "Hello", "HELLO"];
```

Real output today:
```csharp
HashSet<string> set = [with (StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
```

Why it looks wrong: no rule covers the pair, so the author's gap is kept (collapsed), and `with (…)` and `with(…)` both survive. Requirement IDs: FMT-SPACE-002, FMT-SPACE-025, FMT-SPACE-028.

A (recommended):
```csharp
HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
```

B, gap kept and collapsed, as today:
```csharp
HashSet<string> set = [with (StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
```

Recommendation: A — the element reads as a call, Microsoft's documentation writes it without a space, and the repository writes `new()` the same way. Note for the lead: invocations (`M (x)`) and `typeof (x)` also keep their gap today (FMT-SPACE-002's non-goal); removing that is decided but not done (thoenissen/Reihitsu#1440), so A matches that direction. The spec is written on A.

### Internal choices

- FMT-SPACE-028 is a requirement of its own rather than an extension of FMT-SPACE-025: the two `with` tokens have opposite spacing for different reasons. Alternative: widen FMT-SPACE-025.
- FMT-BREAK-022 gains a clarifying sentence and example rather than a new requirement, since the behavior exists and the `with` element is a collection element.

### Follow-up candidates

- **An argument list on the line after its invoked name lands at the enclosing statement's column.** Not specific to `with`; outside both issues; rare shape.

  Input:
  ```csharp
  class C
  {
      void M()
      {
          var x = Foo
              (4);
      }
  }
  ```
  Today:
  ```csharp
  class C
  {
      void M()
      {
          var x = Foo
          (4);
      }
  }
  ```
  The same happens to `[with⏎    (4), "x"]` and `[Foo⏎    (4), 5]` inside a wrapped collection expression. Readings: join the argument list onto the name's line (`var x = Foo(4);`), as FMT-BREAK-020 does for an attached part inside a chain, or keep the line break and indent the continuation. Recommendation: join; a candidate for an `agent-filed` follow-up, not asked in this run.

## Whole run

Files written:

- `/home/user/Reihitsu/sdd/changes/2026-11/union-declaration-layout.md`
- `/home/user/Reihitsu/sdd/changes/2026-11/with-element-spacing.md`
- `/home/user/Reihitsu/sdd/specs/formatter/line-breaks.md`
- `/home/user/Reihitsu/sdd/specs/formatter/structural-transforms.md`
- `/home/user/Reihitsu/sdd/specs/formatter/horizontal-spacing.md`
- `/home/user/Reihitsu/sdd/specs/formatter/pipeline.md`
- `/home/user/Reihitsu/sdd/specs/analyzer/RH7101.md`
- `/home/user/Reihitsu/sdd/specs/requirement-ids.txt`

No rule page and no user documentation changed.

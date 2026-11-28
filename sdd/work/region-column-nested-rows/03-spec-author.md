Role: spec author
Commit: b0a6938
Verdict: change doc written

My prompt carried the maintainer's answers as the lead summarized them. I worked from `00-lead.md` (*Maintainer answers*, batches 1, 1b, 1c, and *Confirmed examples*) and the specs. The prompt carried no intended fix or hoped-for verdict.

Change doc: `sdd/changes/2026-11/region-column-nested-rows.md` (rewritten, Status: Draft). All "before" outputs come from `scripts/format.sh` or `scripts/apply-fix.sh` on base code 13b0ed2 and are stable under a second run. The "after" outputs of the array entries are the maintainer's confirmed examples. I checked with `scripts/format.sh --dry-run` that each differs from today's output only in the lines this change specifies. Before FMT-INDENT-023's row change, the only difference is the row's `}`.

`scripts/test.sh --project architecture`: 220 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-BREAK-046 and FMT-UNSUP-023: it is expected until the test implementer tags tests with them.

## Changed requirements

- FMT-UNSUP-023 — Region directives inside a member are unsupported — **added**:
  - Contract: refused is every active `#region`/`#endregion` in the leading trivia of a token other than:
    - the input's first token;
    - the first token of a member declaration (nested types, namespaces, delegates, enum members, and each top-level statement included), of a using directive, of an extern alias, or of an `[assembly: …]` list;
    - the closing brace of a type, enum, or namespace;
    - the end of the file.
  - Reason `a region directive inside a member`. It is listed after every other shape.
- FMT-UNSUP-022 — **clarified**: it is listed after every other shape "but FMT-UNSUP-023".
- FMT-BREAK-046 — A multi-line array initializer puts its braces on lines of their own — **added**:
  - It covers every array initializer: `new T[…]`, `new[]`, `stackalloc`, a declaration's initializer, and rows. Each is judged on its own.
  - The `{` of a declaration's initializer stays behind `=`.
  - The element line breaks stay as written.
- FMT-BREAK-021 — **changed**: array initializers keep the author's layout "apart from the lines of their braces (FMT-BREAK-046)".
- FMT-INDENT-010 — **changed**:
  - The directive takes the member column, also in front of the `}` of a type or namespace.
  - Inside a member, the input is refused (FMT-UNSUP-023). The statement column and the label column remain only for a directive in front of a node-level input's first token.
  - The example is replaced by namespace, type, and nested-type regions; its output was checked with `format.sh`.
  - The earlier draft's construct-column paragraphs and examples are dropped.
- FMT-INDENT-023 — **changed** (kept from the earlier draft, plus a pointer to FMT-BREAK-046): a row is laid out like a complex element initializer, with its `}` under its `{`. The comment-led-row paragraph is kept, pending output question 3.
- FMT-BLANK-009 — **clarified**: a region directive ends such a gap only in front of a type's or namespace's `}`. The method-body half of the example moved to a type body; its output was checked.
- FMT-BLANK-013 — **clarified**: the switch-label example is replaced by a sentence on the refusal.
- FMT-BLANK-018 — **clarified**: the region example moved to a type's `}`; its output was checked.
- FMT-REGION-005 and the region-formatting non-goal — **clarified**: the straddling example now crosses a nested type body instead of a method body. Its output was checked.
- `AGENTS.md`, the `PrecheckPhase` row of the phase table: the sixteenth shape was added. It is phrased without backticks around the directive names, which `PhaseOwnershipDocumentationTests` rejects.
- `Uses` rows were updated in the unsupported-syntax, indentation, line-breaks, blank-lines, and region-formatting specs. The ledger gained FMT-BREAK-046 and FMT-UNSUP-023.

## Examples for the tests

- **FMT-UNSUP-023:**
  - the refused example, with its location at line 12, column 9;
  - the supported example: usings, a nested type, and a region in disabled text inside a method;
  - the top-level-statements example.
  - Write one precheck test per bullet of the condition list. Each side is stated: header, nested type, top-level, disabled or active branch, and a node-level input's first token against a later token.
- **FMT-BREAK-046:** the six confirmed inputs in one example; split them into one test each. Also the one-line and several-lines condition: `//` comment, verbatim string, `{ 3, 4 }` inside a multi-line initializer.
- **FMT-INDENT-023:** the rows example.
- **FMT-INDENT-010:** the namespace and nested-type example.
- **Change doc entries 1–13:** the regression inputs, each with a second pass and under CRLF.
- **Code fixes (SHARED-FIX-010):** RH5305 and RH5206 on the fixtures of entry 6 are now "no fix offered".

**Existing tests the declaration breaks.** A Roslyn scan of the test string literals found about 217 literals in `tests/Reihitsu.Formatter.Test` and `tests/Reihitsu.Cli.Test` that hold a region directive inside a member. Precheck tests that use regions on purpose are among them. The heaviest files:
- `RegionFormattingFullPipelineTests` (46)
- `DirectiveBeforeEndRegionBlankLineTests` (28)
- `RegionDirectlyAfterOpeningBraceTests` (12)
- `ReihitsuFormatterTests` (11)
- `StraddlingRegionPairTests` (10)

In `tests/Reihitsu.Analyzer.Test`, the formatter-backed classes under `Formatter/Formatting/` (RH5024–RH5027, RH5031) are affected, and so is every code-fix test whose fix formats such a node. Analyzer-only tests are not affected.

Each such test becomes one of:
- a refusal test;
- a test with the region moved between members, where the requirement's point survives there;
- a deleted test.

The scanner is `/tmp/claude-0/-home-user-Reihitsu/0e8a6c60-6f5e-59aa-82c2-08021193ae4c/scratchpad/rscan/out/rscan.dll`. Run it with `~/.dotnet/dotnet <dll> test <files…>` for test files, or with `md <files…>` for specs. Its hit lists are in `.../scratchpad/testhits.txt` and `.../scratchpad/anahits.txt`. This is a large test surface. I report it for the lead's scope decision (README, change growth).

## Output questions

### 1 · Formatter · a region in the header of a type

Batch 1b says "a nested type is not a member", and the answers name no type header. The spec refuses it.

Input:
```csharp
interface I
{
}

class B
{
}

class C : B,
          #region Interfaces
          I
          #endregion
{
}

record R(int A,
         #region More
         int B);
         #endregion
```

Before (`main`):
```csharp
interface I;

class B;

class C : B,

#region Interfaces

          I

#endregion // Interfaces

{
}

record R(int A,

#region More

         int B);

#endregion // More
```

After, A (recommended): the input unchanged.
`error: <file>(10,11): unsupported syntax: a region directive inside a member. The file was left unchanged.`
Code fixes that format the type are no longer offered on it. Declared in this work.

After, B: as before (formatted, directives at column 0).

Why A: a region in a header groups no members and has the same column problem as one in a parameter list. Refusing it keeps one rule: supported only between members. What it buys: no column has to be decided for a directive in a base list or a primary-constructor parameter list.

### 2 · Formatter · regions in a file with top-level statements

The answers do not name top-level statements. The spec treats each top-level statement as a member of the file.

Input:
```csharp
#region Setup
var a = 1;
#endregion

if (a > 0)
{
    #region Body
    a++;
    #endregion
}
```

Before (`main`):
```csharp
#region Setup

var a = 1;

#endregion // Setup

if (a > 0)
{
    #region Body

    a++;

    #endregion // Body
}
```

After, A (recommended): the file is refused at `#region Body`:
`error: <file>(7,5): unsupported syntax: a region directive inside a member. The file was left unchanged.`
Without the `if` block's region, the file is formatted, with `#region Setup` and `#endregion // Setup` as before.

After, B: also refused when only the regions between the statements are present. All top-level statements count as one member, the body of the generated `Main`.

Why A: a region between top-level statements stands at column 0, like one between types, and has no column problem. The check is the same "first token of a member" test the syntax tree gives.

### 3 · Formatter · a comment in front of a multi-line row's opening brace

This is the earlier Q7, reshaped by FMT-BREAK-046. FMT-BREAK-046 puts the brace on a line of its own and does not say whether a comment in front of it on that line may stay.

Input:
```csharp
class C
{
    int[,] F = {
                   /* c */ { 3,
                   4 }
               };
}
```

Before (`main`):
```csharp
    int[,] F = {
                   /* c */ { 3,
                       4 }
               };
```

After, A (recommended, the spec as written): the comment stays in front of the brace, and the row follows the brace's column (FMT-INDENT-023):
```csharp
    int[,] F = {
                   /* c */ {
                               3,
                               4
                           }
               };
```

After, B: the brace moves below the comment:
```csharp
    int[,] F = {
                   /* c */
                   {
                       3,
                       4
                   }
               };
```

Why A: it is how a comment-led complex element initializer is laid out today, so rows and complex element initializers stay one layout. What would change the pick: a wish that every multi-line brace starts its line with nothing in front of it. Then B, and FMT-INDENT-023's comment paragraph is dropped for rows.

### 4 · Formatter · an array initializer whose only line break is inside a string literal

The answers define single-line and multi-line by the initializer. The spec counts every line break between the braces, also one inside a verbatim or raw string.

Input:
```csharp
class C
{
    int[] F = { @"a
b".Length, 2 };
}
```

Before (`main`): unchanged.

After, A (recommended, the spec as written):
```csharp
class C
{
    int[] F = {
                  @"a
b".Length, 2
              };
}
```

After, B: unchanged; only line breaks between tokens count.

Why A: the initializer visibly spans two lines. "Any line break in its text" is the simplest rule, and it gives `//` comments the same answer: they force a line break under both options.

### 5 · Formatter · `stackalloc` array initializers

Batch 1b, 3A lists outer initializers, rows, `new[]`, and `new T[…]`, not `stackalloc`. The spec includes it.

Input:
```csharp
class C
{
    unsafe void M()
    {
        var s = stackalloc int[] { 1,
            2 };
    }
}
```

Before (`main`):
```csharp
        var s = stackalloc int[] { 1,
                    2 };
```

After, A (recommended):
```csharp
        var s = stackalloc int[]
                {
                    1,
                    2
                };
```

After, B: as before.

Why A: a `stackalloc` initializer is an array initializer whose braces already align to its keyword, like `new` (FMT-INDENT-022).

## Internal choices

- The brace rule is a new requirement, FMT-BREAK-046, with FMT-BREAK-021 pointing to it. The alternative was extending FMT-BREAK-021, which is about initializers that are always multi-line.
- FMT-UNSUP-023 lists the supported positions and refuses the rest. The alternative, naming the refused constructs, misses members of the list (role file, step 6). Delegates and enum members follow from "member declaration". A delegate's parameter list is refused like a method's.
- The leading trivia of the input's first token is excepted, as FMT-UNSUP-010 excepts the declaration's first token. Without the exception, a fix that formats a statement or an attribute list below `#region Methods` would be withheld on common code.
- FMT-UNSUP-023 is listed after every other shape, and FMT-UNSUP-022's sentence is adjusted.
- I updated the `AGENTS.md` phase table, which lists every shape, in this run.
- *Special cases*, *Deleted*: I found no phase code that exists only for a region inside a member. The deletions are the special cases the earlier draft would have added, plus spec clauses. *Checked and kept* lists every region-specific guard with the reason it stays. The lead should note that this is a declaration on the maintainer's explicit answer, as FMT-UNSUP-022 was.

## Follow-up candidates

- **F1, F2 (01-spec-author): moot.** Both concern regions inside members, which are now refused. Drop them.
- **F3 (02-skeptic): narrowed.** The attribute-argument and base-list cases are refused now (base list pending output question 1). What remains is the RH7304 spec gap: `RegionDirectiveUtilities.IsWithinElementBody` treats initializers, parameter lists, and attribute arguments as "between declarations", which neither SHARED-TYPEREGION-005 nor RH7304-001 states.
- **F4 · RH7303 · regions the formatter refuses but no rule reports.** RH7303 reports regions inside executable code (SHARED-TYPEREGION-005). Not reported are regions in a field's or property's initializer, a parameter list, a member's attribute arguments, or a type header (if 1A), all of which the CLI now refuses. A user then sees a refused file with no diagnostic to fix it. Reachable through the CLI. The readings: (a) the CLI's report is enough; (b) RH7303 reports every position FMT-UNSUP-023 refuses. Recommendation: (b), so the analyzer points at what the formatter refuses. This is an analyzer change outside this run.
- **F5 (low): no analyzer rule reports the array-initializer brace layout of FMT-BREAK-046.** RH5303 and RH5305 each name the other as the owner (non-goals). P11 allows the formatter to do more, so this is not a defect. It is only a candidate if a rule is wanted.

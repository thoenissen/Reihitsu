Role: spec author
Commit: 0e853f2
Verdict: spec written

Repair round on the skeptic's handover `02-skeptic.md`. Every finding was re-derived and resolved in the specs. No requirement was added, retired, or renumbered, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged and still consistent. No rule page changed. No change doc: the specs still state existing behavior only.

## Changed requirements

All are clarified. Each statement is narrowed or corrected to what the code does.

- **SHARED-ATTR-004**
  - Title is now "The token after the list is compared with its closing bracket".
  - `Why:` no longer says the placement is "about what follows the list". This and the RH55xx `-003` change below keep open question 3 undecided.
- **SHARED-ATTR-006**
  - Dropped the last sentence ("Every other accessor attribute list … is checked").
  - It now states only the exemption. The RH5527 example still shows the reported side (FC-002).
- **SHARED-ATTR-008**
  - Line ending is now "detected as FMT-PIPE-007 specifies for the formatter" (FMT-PIPE-007 added to `Uses`).
  - Dropped "The list itself stays as written".
  - Added a coverage clause: the requirement covers a list outside every initializer and anonymous object whose innermost enclosing opening brace is itself indented four spaces per enclosing scope. This leaves the continuation-indented block out (question 4).
- **SHARED-ATTR-009**
  - Covers every initializer (object, collection, array, `with`, complex-element) and anonymous objects; the indentation is written in spaces.
  - Coverage clause: no tab in front of the declaration's first token. This leaves out question 5.
  - Added an array-initializer example.
- **SHARED-ATTR-012**
  - The scope list names every initializer kind.
  - "Encloses" is now "whose braces enclose the attribute list", so it no longer reaches the owner case (finding 6).
- **Shared spec non-goal:** "Attribute lists of type parameters" is now "Attribute lists that resolve to the type-parameter target"; RH5523's non-goal is changed the same way.
- **RH5530-001** (finding 1)
  - It now reports only when the property, indexer, or event that owns the accessor spans more than one line, apart from its own attribute lists, as SHARED-ATTR-004 and -005 specify.
  - It MUST NOT report what SHARED-ATTR-006 exempts.
  - Single-line declarations with accessor bodies, expression bodies, or a comment in the accessor list are no longer stated either way (question 1).
- **RH5501-001, RH5503-001, RH5513-001, RH5515-001, RH5517-001, RH5519-001, RH5527-001:** a specifier list is checked only when "not attached to a parameter or type parameter" (minor item 1).
  - RH5527-001 now lists method, operator, conversion operator, local function, lambda, delegate, and accessor.
- **RH5511-001, RH5515-001, RH5517-001, RH5519-001:** now say "without a specifier or with `type:`", consistent with the other rules (minor item 2).
- **`-003` of every separate-line rule**
  - `Why:` is now "what follows the list starts on its own line"; "the list stays where it is" is gone (minor item 3).
  - RH5505, RH5507, RH5509, RH5511, RH5521, and RH5525 cite only SHARED-ATTR-008 and drop SHARED-ATTR-009 from `Uses`, because their declarations cannot occur inside an initializer (minor item 5).
  - RH5501 and RH5503 keep SHARED-ATTR-009: an `assembly:` or `module:` list on a local function inside an initializer resolves to their target.

## How each finding was handled

1. **Finding 1** — resolved as described for RH5530-001.
   - `documentation/rules/RH5530.md` (*Exceptions*: a single-line property is never reported) does not contradict the narrowed spec, which says nothing about the single-line shapes outside SHARED-ATTR-006.
   - My earlier claim that every page is consistent with its spec held only for the narrowed spec. Question 1 now cites the page.
2. **Finding 2** — resolved through the non-goal rewording. `[param: A]` on a type parameter belongs to question 2.
3. **Finding 3** — resolved in SHARED-ATTR-009 and -012. The array-initializer example was run: `void L()` lands at column 16.
4. **Findings 4 and 5** — left out through coverage clauses and filed as questions 4 and 5. I reproduced both: `void L()` lands at column 12, and with tabs it gets 3 spaces.
5. **Finding 6** — the owner case is left out of SHARED-ATTR-012.
   - Run with the harness: `[A] class C` / `{` with the closing brace missing is reported by RH5505, and no action is registered.
   - Withholding a fix is a supported end state, so this is not an open question. The guard's scope (`HasIncompleteEnclosingScope` starts at the owner) is listed below as a follow-up candidate.
6. **Minor item 4** (FMT-PIPE-007) — taken.

## Verification

- Every C# example in the 16 specs was re-extracted and run through the scratchpad harness: reported spans compared with the markers, fixes applied one diagnostic at a time and compared, and "no code fix is offered" cases checked. Result: 67 example cases, 0 failures.
- 12 extra probes, all matching the spec text:
  - `[type: A]` on a constructor, property, field, and event: reported by RH5511, RH5515, RH5517, and RH5519.
  - `[return: A]` on a parameter: not reported by RH5527. `[assembly: A]` on a parameter: not reported by RH5501.
  - `[return: A]` on a lambda: reported by RH5527.
  - Operator, indexer, and delegate parameters: reported by RH5523.
  - Indentation inside a switch section and an enum, and the CRLF fix output, are as specified.
  - The owner case of finding 6 is reported with no fix.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.

## Examples for the tests

Unchanged from `01-spec-author.md`, with these additions:

- SHARED-ATTR-009: the new array-initializer example.
- The `type:` and "not attached to a parameter" qualifiers of the `-001` requirements: one example on each side, for example `[type: A] C()` (RH5511) and `void M([return: A]` / `int x)` (RH5527, not reported).
- Do not tag:
  - RH5530 tests of single-line declarations whose accessors have bodies or a comment (question 1);
  - tests whose expected fix output depends on a continuation-indented block or on tabs inside an initializer (questions 4 and 5);
  - tests whose input has other source in front of the list on its line (question 3).

## Open questions to file

None blocks. Every case is left out of the specs.

### `SDD open questions: sdd/specs/analyzer/RH5530.md` (new, sub-issue of thoenissen/Reihitsu#912)

**1. RH5530 exempts more single-line declarations than the other rules.** Touches RH5530-001 and SHARED-ATTR-006.

Input:

```csharp
class C
{
    int P { [A] get; /* c */ set; }

    int Q { [A] get => 1; }

    int this[int i] { [A] get { return i; } set { } }

    event System.Action E { [A] add { } remove { } }
}
```

- Real output: RH5530 reports nothing.
- RH5527 does report `int P { [return: A] get; /* c */ set; }` and `int Q { [return: A] get => 1; }`, because its exemption (SHARED-ATTR-006) covers only accessor lists without bodies and without comments.
- The formatter expands these accessor lists (FMT-BREAK-025) and puts each attribute on its own line (FMT-BREAK-027).
- Why it looks wrong: the same layout gets a different verdict depending only on the attribute's target, and RH5530 stays silent about a layout the formatter rewrites.
- Reading (a): RH5530 uses only the SHARED-ATTR-006 exemption. All four lists are reported, the fix gives `int Q { [A]` / `        get => 1; }`, and the *Exceptions* section of `documentation/rules/RH5530.md` changes.
- Reading (b): RH5530's wider exemption is intended, as `documentation/rules/RH5530.md` *Exceptions* documents ("A property whose declaration is single line … is never reported"). The other separate-line rules would then widen to match, so `int Q { [return: A] get => 1; }` would no longer be reported.

### `SDD open questions: sdd/specs/shared/attribute-targets.md` (new, sub-issue of thoenissen/Reihitsu#912)

**2. A `param:` or `typevar:` list follows the parameter or type-parameter target, whatever it is attached to.** Touches SHARED-ATTR-002, SHARED-ATTR-003, RH5523-001, and RH5530-001.

Input:

```csharp
class C
{
    int P
    {
        get;
        [param: A]
        set;
    }

    [param: A]
    void M(int x)
    {
    }

    [typevar: A] void N<T>()
    {
    }
}

[param: A]
class D
{
}

class E<[param: A]
        T>
{
}
```

- Real output: RH5523 reports each `[param: A]` list, and its fix joins it to the next token: `[param: A] set;`, `[param: A] void M(int x)`, `[param: A] class D`, `class E<[param: A] T>`.
- `[typevar: A] void N<T>()` is reported by no rule (RH5513 included), and neither is `void M([typevar: A]` / `int x)`.
- RH5530 checks none of these lists.
- Why it looks wrong: a declaration-level list is kept on the declaration's line only because of its specifier. In a multi-line accessor list, `[param: A] set;` displaces the `set` keyword, which RH5530's rule page names as the problem. `[return: A]` and `[A]` in the same places must go on their own line.
- Reading (a): a list follows the layout of the declaration it is attached to, whatever its specifier. `[param: A]` / `set;` and `[param: A]` / `void M(int x)` are then compliant, and `[param: A] set;` is reported.
- Reading (b): the current behavior is intended, because the attribute applies to a parameter (`value`) or a type parameter, so the parameter rule governs it.

**3. A list that shares its line with the code before it is not reported.** Touches SHARED-ATTR-004 and SHARED-ATTR-008.

Input:

```csharp
class C
{
    int _a; [A] void M()
    {
    }
}
```

- Real output: RH5513 reports `[A]`, and its fix produces `int _a; [A]` / `    void M()`. After that no rule reports `[A]`.
- Why it looks wrong: the rule titles say "… attributes must be on their own line", and the formatter puts `[A]` on its own line (FMT-BREAK-007, FMT-BREAK-027).
- Reading (a): the separate-line rules also report a list whose opening bracket does not start its line, and the fix also breaks before it, giving `int _a;` / `    [A]` / `    void M()`.
- Reading (b): the current behavior is intended, because what precedes the list is another declaration's layout, though no analyzer reports it today.

**4. Fix indentation inside a block indented by continuation.** Touches SHARED-ATTR-008.

Input:

```csharp
class C
{
    void M()
    {
        Run(1,
            () =>
            {
                [A] void L()
                {
                }
            });
    }
}
```

- Real output: the RH5513 fix leaves `[A]` at column 16 and moves `void L()` to column 12, the column of the lambda's `{`. The same happens for a lambda block that is a collection-expression element on its own line.
- Why it looks wrong: the declaration ends up at its block's brace column instead of one level inside the block. The formatter puts it at column 16.
- Reading (a): indent one level inside the innermost enclosing brace, which puts `void L()` at column 16.
- Reading (b): the nesting-depth count is intended, which keeps column 12.

**5. Fix indentation inside an initializer indented with tabs.** Touches SHARED-ATTR-009.

Input (`\t` is a tab):

```text
class C
{
\tD _d = new D
\t{
\t\tH = () =>
\t\t{
\t\t\t[A] void L()
\t\t\t{
\t\t\t}
\t\t}
\t};
}
```

- Real output: the RH5513 fix gives `void L()` an indentation of 3 spaces, the character offset of `[A]`. `[A]` sits at visual column 12 with a tab width of 4.
- Why it looks wrong: the declaration is no longer aligned with its own attribute list, and outside initializers the fix writes the canonical depth in spaces whatever the tabs.
- Reading (a): use the visual column, in spaces, which gives 12 spaces.
- Reading (b): the character offset is intended, which gives 3 spaces.

### `SDD open questions: sdd/specs/formatter/line-breaks.md` (thoenissen/Reihitsu#918 is closed; a new issue, unless the lead reopens it)

**6. The formatter keeps a `param:` or `typevar:` list on a declaration's line.** Touches FMT-BREAK-027. This is the formatter side of question 2.

Input:

```csharp
class C
{
    [param: A]
    void M(int x)
    {
    }

    [typevar: A] void N<T>()
    {
    }
}
```

- Real output (`scripts/trace.sh`): LineBreakPhase joins the first case to `[param: A]void M(int x)`, and the result after spacing is `[param: A] void M(int x)`. It keeps `[typevar: A] void N<T>()` on one line.
- Why it looks wrong: FMT-BREAK-027 says each attribute list of a declaration starts its own line with the declaration on the next one.
- Reading (a) and reading (b) are as in question 2.

## Follow-up candidates (out of scope)

- `TargetAttributePlacementCodeFixProviderBase.HasIncompleteEnclosingScope` starts at the attribute list's owner, so `[A] class C` / `{` with the closing brace missing withholds a fix that does not depend on that brace. This is the guard's scope, per the *Code-fix coverage* section of `AGENTS.md`.
- Repeated from `01-spec-author.md`:
  - `AttributeTargetUtilities` as a possible code owner of the shared spec (the maintainer's call);
  - the LineBreakPhase and IndentationPhase back-and-forth over a lambda attribute in `scripts/trace.sh`.

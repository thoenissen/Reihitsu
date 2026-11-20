# RH5112 and RH5048 name the first member access, not the first call

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — wording only: the titles of the specs `analyzer/RH5112.md` and `analyzer/RH5048.md`, and the non-goal pointers to RH5048 in `analyzer/RH5038.md` and `shared/comment-placement.md`; no requirement changes |

## What

The diagnostic title and message of RH5112 and RH5048, the title of the RH5112 code fix, the headings and the "Why" and "How to fix" wording of both rule pages, and the two rows of the analyzer package README say "first member access" instead of "first call". IDs, severities, what is reported, where, and what the code fix produces stay as they are.

## Why

RH5112-001 and RH5048-001 already cover a chain's first link whether it is a call, a property read, or a conditional or null-forgiving access, and the rule pages' descriptions already say so. A diagnostic on `.Customer`, a property, that says "first call" does not describe the reported code.

## Visible output changes

### 1 · RH5112 diagnostic title and message

Input:

```csharp
var city = order
    .Customer
    .Address;
```

Before: RH5112 on the `.` in front of `Customer` — "Wrapped fluent calls should keep the first call on the original line."

After: RH5112 on the same `.` — "Wrapped chains should keep the first member access on the original line."

The title (shown in the rule list, the error list's description column, and `.editorconfig` tooling) changes to the same text.

### 2 · RH5048 diagnostic title and message

Input:

```csharp
var city = order
    // customer
    .Customer
    .Address;
```

Before: RH5048 on `// customer` — "Comments must not be placed before the first call of a wrapped chain"

After: RH5048 on `// customer` — "Comments must not be placed before the first member access of a wrapped chain"

The title changes to the same text.

### 3 · RH5112 code-fix title

Input: as in 1.

Before: the light-bulb action reads "Keep first wrapped fluent call on original line".

After: the light-bulb action reads "Keep first wrapped member access on original line".

The fix's result is unchanged:

```csharp
var city = order.Customer
                .Address;
```

### 4 · Rule page headings and package README rows

- `documentation/rules/RH5112.md`: `# RH5112 — Wrapped fluent calls should keep the first call on the original line` → `# RH5112 — Wrapped chains should keep the first member access on the original line`.
- `documentation/rules/RH5048.md`: `# RH5048 — Comments must not be placed before the first call of a wrapped chain` → `# RH5048 — Comments must not be placed before the first member access of a wrapped chain`.
- The RH5112 and RH5048 rows of the analyzer package README (the NuGet page) carry the same new titles.

### 5 · Rule page body sentences that still say "call"

- RH5112, *Why is this a problem?* — before: "Breaking before the first fluent call makes the expression harder to scan … Keeping the first call on the original line makes the expression easier to follow while still allowing longer call sequences to wrap clearly." After: "Breaking before the first member access makes the expression harder to scan … Keeping the first member access on the original line makes the expression easier to follow while still allowing longer chains to wrap clearly."
- RH5112, *How to fix it* — before: "Leave later calls on separate lines when the expression still needs to wrap." After: "Leave later member accesses on separate lines when the expression still needs to wrap."
- RH5048, *Why is this a problem?* — before: "The first call of a chain belongs on the line of the expression it is called on. A comment in front of it keeps the call apart from its receiver." After: "The first member access of a chain belongs on the line its receiver ends on. A comment in front of it keeps the member access apart from its receiver."

The RH5112 page's description paragraph about property reads in front of the first call (`settings.Items.Where(...)`) means a call there and stays.

## Decisions

### Maintainer answers

The maintainer answered the output questions asked from the visible output changes above:

1. RH5112 title and message (change 1): "Wrapped chains should keep the first member access on the original line." with its trailing period. The rule page heading and the package README row carry the same title (change 4).
2. RH5048 title and message (change 2): "Comments must not be placed before the first member access of a wrapped chain" with no trailing period. The rule page heading and the package README row carry the same title (change 4).
3. RH5112 code-fix title (change 3): "Keep first wrapped member access on original line".
4. RH5112 rule page body (change 5): "call" is replaced by "member access" and "chains" as listed.
5. RH5048 rule page "Why" (change 5): "The first member access of a chain belongs on the line its receiver ends on. A comment in front of it keeps the member access apart from its receiver." This matches the RH5112 description. The first draft's "on the line of the expression it accesses" was not taken, because a member access accesses a member of its receiver, not the expression.

### Punctuation stays as it is

RH5112's title and message keep their trailing period and RH5048's keep none, as today; only the words change. The repository mixes both forms, and changing punctuation is not what the rename is about.

### Type names stay

The analyzer, code-fix, and test class names (`RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer`, `RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer`, and their code-fix and test classes) are not renamed. No user sees them; the code-fix equivalence key is the provider's type name, so keeping it keeps the key. Their documentation comments, which repeat the title, follow the new wording.

## Alternatives considered

- **Narrow the rules back to calls** — rejected: RH5112-001 and RH5048-001 cover every first link, as does the formatter's join of the first link (SHARED-CHAIN-003).
- **Change only the resources and the headings, as the issue lists** — rejected: the rule pages' "Why" and "How to fix" sentences would then contradict their own heading for a property-only chain. Listed as change 5 so it can be dropped on its own.
- **Code-fix title "Keep first member access on original line"** — not taken: "wrapped" is in today's title, and a pure substitution keeps the change to wording.

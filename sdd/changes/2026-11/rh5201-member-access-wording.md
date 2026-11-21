# RH5201 names a member-access chain, not a method chain

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | none — wording only: the title of the spec `analyzer/RH5201.md`; no requirement changes |

## What

The diagnostic title and message of RH5201, the title of its code fix, the heading of its rule page, and its row of the analyzer package README say "member-access chain" instead of "method chain". The rule page's description and "Why" sentences follow, if the maintainer takes change 4. The ID, severity, what is reported, where, and what the code fix produces stay as they are.

## Why

RH5201-001 to RH5201-003 report links of any wrapped member-access chain, also one without a method call: RH5201-002's second example reports `.Next` in `c.Next.Next` / `.Next`, and RH5201-003 reports the split `?` of `d.Next?` / `.Next`. A diagnostic on such a chain that says "Method chains should be aligned." does not describe the reported code. RH5112 and RH5048 had the same mismatch and were reworded from "call" to "member access" (`rh5112-rh5048-member-access-wording.md`).

## Visible output changes

### 1 · RH5201 diagnostic title and message

Input (`dynamic order`):

```csharp
return order.Customer.Address
            .City;
```

Before: RH5201 on the `.` in front of `City` — "Method chains should be aligned."

After: RH5201 on the same `.` — "Member-access chains should be aligned."

The title (shown in the rule list, the error list's description column, and `.editorconfig` tooling) changes to the same text.

### 2 · RH5201 code-fix title

Input: as in 1.

Before: the light-bulb action reads "Align method chain".

After: the light-bulb action reads "Align member-access chain".

The fix's result is unchanged:

```csharp
return order.Customer.Address
                     .City;
```

### 3 · Rule page heading and package README row

- `documentation/rules/RH5201.md`: `# RH5201 — Method chains should be aligned` → `# RH5201 — Member-access chains should be aligned`.
- The RH5201 row of the analyzer package README (the NuGet page) carries the same new title: `Member-access chains should be aligned.`

### 4 · Rule page body sentences that still say "method chain" or "calls"

- *Description*, first sentence — before: "This rule checks the layout of a method chain that spans multiple lines:" After: "This rule checks the layout of a member-access chain that spans multiple lines:"
- *Why is this a problem?* — before: "Misaligned method chains make it difficult to follow the sequence of operations, especially when chains are long. Consistent dot alignment provides a clear visual guide through the chain of calls." After: "Misaligned member-access chains make it difficult to follow the sequence of operations, especially when chains are long. Consistent dot alignment provides a clear visual guide through the chain of member accesses."

The description's bullets that say "the first call" mean a call there — the anchor rule of SHARED-CHAIN-007 — and stay. The examples are a chain with calls and stay.

## Decisions

### Maintainer answers

1. RH5201 title, message, and code-fix title (changes 1 to 3): answered on the issue on 2026-11-20 — "Member-access chains should be aligned." and "Align member-access chain"; the rule page heading and every place the title test pins follow.
2. Rule page body (change 4): open — not covered by that answer; asked by the lead.

### Punctuation stays as it is

The title and message keep their trailing period and the code-fix title keeps none, as today; only the words change.

### Type names stay

The analyzer, code-fix, and test class names (`RH5201MethodChainsShouldBeAlignedAnalyzer`, `RH5201MethodChainsShouldBeAlignedCodeFixProvider`, and their test classes) are not renamed. No user sees them; the code-fix equivalence key is the provider's type name, so keeping it keeps the key, and the run for the next RH5201 issue edits the same files. The analyzer's documentation comment, which repeats the title, follows the new wording.

### Other surfaces keep "method chain"

The formatter spec heading FMT-INDENT-029 "Method chains align to their anchor link", and other rules' pages that say "a later call of a method chain" (RH5035, RH5038, RH5039, RH5043, RH5044) or "in a wrapped method chain" (RH5115, RH5116), are not RH5201's text and are not changed: a later call is a call, and the RH5115/RH5116 sentences describe their own fixes.

## Alternatives considered

- **Change only the resources, the heading, and the README row, as the issue lists** — possible: the first sentence of the rule page would then call the chain a "method chain" under a heading that calls it a member-access chain. Listed as change 4 so it can be dropped on its own.
- **"Chains should be aligned."** — not taken: the maintainer chose "Member-access chains", and "chains" alone does not say which chain (an `if`/`else if` chain, a constructor-initializer chain).

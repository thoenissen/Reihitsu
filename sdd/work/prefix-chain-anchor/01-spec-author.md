Role: spec author
Commit: 3d512e9
Verdict: change doc written

Issue thoenissen/Reihitsu#896 (`agent-filed`), read through the GitHub MCP server; it has no comments. The prompt carried no intended fix. The issue's proposal ("anchor under the operator of the last prefix link on the root line", single owner `FluentChain.GetAnchorLink`) was checked against the specs and the code, not taken as given; the spec states it by the links in front of the first wrapped link, not by "the root line" (see *Internal choices*).

Files written:

- `/home/user/Reihitsu/sdd/changes/2026-11/prefix-chain-anchor.md` — change doc, `Status: Draft`, with 7 visible output changes, all with real *before* outputs from `main`.
- `/home/user/Reihitsu/sdd/specs/shared/fluent-chain-layout.md` — SHARED-CHAIN-007 rewritten.
- `/home/user/Reihitsu/sdd/specs/formatter/indentation.md` — FMT-INDENT-029 example added.
- `/home/user/Reihitsu/sdd/specs/analyzer/RH5201.md` — RH5201-001 example changed, RH5201-002 example added.
- `/home/user/Reihitsu/sdd/specs/analyzer/RH5111.md` — RH5111-006 example output and its caption changed.
- `/home/user/Reihitsu/documentation/rules/RH5201.md` — description and "How to fix it" name the new anchor. The page changes because the anchor a user aligns to in the normal case changes. The Violation/Correction example does not change.

No requirement ID was added, so `requirement-ids.txt` is unchanged. `scripts/test.sh --project architecture`: 198 passed.

## Changed requirements

- SHARED-CHAIN-007 — Wrapped links align to the anchor link — changed. When no invoked link comes before the first link that starts a line, the anchor is now the last link in front of that link, not the chain's first link. The requirement also states what "starts a line" means for the anchor: a line break between the operator's first token and the token in front of it, whatever trivia carries that break. This is what `FluentChainLink.StartsLine` counts today. New example cases `d`, `e`, `f`, `g`; case `b` changed.
- FMT-INDENT-029 — Method chains align to their anchor link — clarified by a new example. The normative text is unchanged because it delegates to SHARED-CHAIN-007.
- RH5201-001 — A later call that shares a line is reported — the example changed: case `d` is now aligned under the second `.Next`.
- RH5201-002 — A misaligned wrapped link is reported — an example was added. The normative text is unchanged because it delegates to SHARED-CHAIN-007.
- RH5111-006 — The fix lays out an assignment inside a conditional access as the document does — the example output changed for `a?.B?.B` and `a.B?.B`, and so did its caption.

## Examples for the tests

- **SHARED-CHAIN-007.** Example cases:
  - `d` and `e`: the new anchor.
  - `f`: a link behind the first wrapped link never becomes the anchor.
  - `g`: an invoked link takes precedence.
  - `a`, `c`: unchanged.

  Today's whole-document output matches the example on every unchanged line. Only `b`, `d` and `e` differ, and they differ by the new anchor.
- **FMT-INDENT-029.** The new prefix example, checked against today's output in the same way: only the two wrapped lines differ.
- **RH5201-001.** Case `d`, which is not reported.
- **RH5201-002.** The new example. On `main`, `scripts/apply-fix.sh RH5201` reports `b` and `e` and leaves `a`, `d` and `f` alone. After the change, `a` and `d` are reported and `b`, `e` and `f` are not.
- **RH5111-006.** The `a?.B?.B` and `a.B?.B` rows.
- **Change doc, visible changes 3 and 5.** These name further seeds:
  - prefix wraps kept behind two root-line links: `x.Next.Next` ⏎ `.Next` ⏎ `.Self()`;
  - a conditional prefix: `x.Next?.Next` ⏎ `?.Name` ⏎ `.Trim()`;
  - a first link joined by the formatter: `x` ⏎ `.Next.Next` ⏎ `.Self()`;
  - the RH5112 fix of `x` ⏎ `.Items.Length` ⏎ `.ToString()`.
- **Existing tests that pin today's anchor.** Found by a scan; the test implementer decides:
  - `ChainLinkLayoutTests.WrappedCallAfterPropertyPrefixStaysUnderPrefix` (`src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainLinkLayoutTests.cs:2597`);
  - `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConditionalAccessWrapBetweenOperatorTokensTests.cs:941` (`a.B.C` ⏎ `?.D`);
  - `RH5201FluentChainLinkFormatterTests.VerifyMisalignedCallAfterPropertyPrefixIsReported` (line 385, input at line 412);
  - `RH5201MethodChainsShouldBeAlignedAnalyzerTests` at lines 1552 (`VerifyPrefixLinksSharingALineAreNotReported`) and 2635/2640;
  - the RH5112 tests at 1307/1312;
  - the RH5111 conditional-access tests at 345/401/627/1205/1210;
  - the RH6023 tests at 715/734.
- **Repository source that holds the shape.** These files will be reformatted, layout only; the scan is a heuristic, so self-hosting is the safety net:
  - `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:355`;
  - `src/Reihitsu.ArchitectureTests/General/TestScriptProjectSelectionTests.cs:227`;
  - `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.cs:41`;
  - `src/Reihitsu.Analyzer.Test/SelfHosting/RuleDocumentationExampleTests.cs:80,88`.

## Output questions

The change doc lists every visible change with its *before* and *after*. Questions 1 and 2 decide all of them. Question 1 covers changes 1, 3, 4, 5 and 6, and the repository reformatting. Question 2 covers change 2 and its share of 4 and 5 (`.Next` in place of `.Self()`).

**1 · Formatter and RH5201 · Method chains should be aligned — a wrapped call behind a prefix of property reads**

Input:
```csharp
var s = x.Items.Length
    .ToString();
```

Before (`main`):
```csharp
var s = x.Items.Length
         .ToString();
```

After, A (recommended):
```csharp
var s = x.Items.Length
               .ToString();
```

After, B:
```csharp
var s = x.Items.Length
         .ToString();
```

Why A: the wrapped call continues `.Length`, the way a wrapped call behind `items.Where(…)` continues that call. RH5201 then reports `.ToString()` under `.Items` and no longer reports it under `.Length`. The RH5201, RH5112 and RH5111 fixes write the A columns, and the rule page says "the last member access in front of the first wrapped line". B leaves the issue unimplemented (`Refs #896`).

**2 · Formatter and RH5201 · Method chains should be aligned — a chain without any call**

Input:
```csharp
var c = order.Customer.Address
    .City;
```

Before (`main`):
```csharp
var c = order.Customer.Address
             .City;
```

After, A (recommended):
```csharp
var c = order.Customer.Address
                      .City;
```

After, B:
```csharp
var c = order.Customer.Address
             .City;
```

Why A: one anchor rule for every prefix, whether or not a call follows later. Under B, `order.Customer.Address` ⏎ `.City` and `order.Customer.Address` ⏎ `.City()` would align differently. The issue's expected output is A and leaves the decision to this run.

## Internal choices

- **Anchor statement.** The fallback anchor is stated as "the last link in front of the first link that starts a line", not as the issue's "last prefix link on the root line". Under "the line the root ends on", `Get(a,` ⏎ `b).Items.Count` ⏎ `.ToString()` would have no link on the root's line, because the root is `Get` and its argument list is an attached part; it would then fall back to `.Items`. For every shape the formatter produces, the two statements agree. The alternative is recorded in the change doc.
- **The first-invoked-link case stays.** The first invoked link in front of the first wrapped link stays the anchor. A pure "last link in front of the first wrapped link" rule would make RH5201 measure an unformatted `items.Where(…).Select(…)` ⏎ `.ToList()` against `.Select`, while the formatter puts `.ToList()` under `.Where`.
- **"Starts a line".** For the anchor, this is defined as a line break in front of the operator's first token, which matches `FluentChainLink.StartsLine`. A link behind a multi-line comment's `*/` therefore counts as wrapped for the anchor. This keeps SHARED-CHAIN-005's comment example (`r`) unchanged for the analyzer and the formatter alike. SHARED-COMMENT-012's "does not start its line" governs whether the link must start a line; the anchor text counts the line break "whatever trivia carries" it.
- **FMT-INDENT-029.** It gets an example only, no new text, because it delegates to SHARED-CHAIN-007.
- **Rule page.** The RH5201 rule page is updated. The other chain rule pages (RH5112, RH5048, RH5111) do not mention the anchor and are left alone.

## Follow-up candidates

_None._

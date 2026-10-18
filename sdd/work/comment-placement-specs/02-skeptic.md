Role: skeptic
Commit: 9d71341f9135f9c3ac2188eded8ac925597cd600
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The lane is specs and tests for existing behavior. `git diff ede10b3 HEAD -- src` is empty, so no production code changed. The 16 analyzers exist and match their spec headers. Every example block of the 17 specs reproduces: I re-ran the spec author's checker (`verify.py` from 01, which drives the file-based probe app from 01) and got 39/39 OK.

Code read: `CommentPositionAnalyzerBase.OnSyntaxTree`, `CommentRegionAnalyzerBase.IsMisplaced`, all of `CommentPositionUtilities`, `FluentChainAnalysisHelper.IsLaterChainLink` and `CollectChainLinksInReverseOrder`, `FluentChainUtilities.GetInvokedLinkOperator`, `SyntaxIndentationUtilities.GetIndentingScopeRange`, each rule's `IsMisplaced`/`TryGetRegion`/`IsMisplacedBeforeRegion`, and the formatter's `ListElementInteriorUtilities.IsOwningConstruct` and `ListElementInteriorJoinRewriter.IsOwnedGap`.

Every claim below was settled by running the same probe app over fixtures of my own (`dotnet run probe.cs -- <dir>`, `DocumentationMode.Diagnose`, `LanguageVersion.Preview`). "Reported" names the rules that reported the comment. In ``a`` / `// c` / ``b``, the comment stands on its own line between `a` and `b`. `M`/`Use` take `params object[]` unless stated otherwise. The thoenissen/Reihitsu#1058 Q1 inputs reproduce as filed: `a.B()` / `// c` / `?.Name`, `a?.P` / `// c` / `?.Q()` and `a.B()` / `// c` / `!.C()` are not reported; `a.B()` / `// c` / `.Name`, `a.P` / `// c` / `?.Q()` and `a.B()` / `// c` / `!?.C()` are reported by RH5038.

Most of the spec is right. Six statements disagree with the code or with each other, one piece of shared behavior is restated, and one class of inputs looks wrong but is stated as intended (Q2).

### Spec statements that are wrong (fix before tests are tagged)

1. **SHARED-COMMENT-006 states a questionable case as intended.** "Every token of such a construct counts, its first token included" also covers an anonymous object's `new`, a switch expression's governing expression and a recursive pattern's type. For all three the code's output looks wrong (Q2). 006's own Why ("an operand of an operator lays out no lines of its own, so a comment there still splits the element") contradicts the code for the governing expression.
2. **SHARED-COMMENT-009 claims behavior the code does not have, and its coverage clause does not exclude it.** The mechanism: `GetInvokedLinkOperator` returns the operator of *any* postfix expression in front of `.`, not only `!`, and `IsLaterChainLink` accepts every `MemberAccessExpressionSyntax` operator, `->` included.
   - `Use(c.B().P` / `// c` / `++.ToString())` → not reported. The comment before `++` counts as being before a later call.
   - `Use(c.B().P++` / `// c` / `.ToString())` → reported by RH5038 (and RH5048). By 009's text this comment is valid, because `.ToString()` is invoked and the invoked `.B()` comes before it.
   - `Use(p->A()` / `// c` / `->B())` (unsafe, `S* A()`, `S* B()`) → not reported. 009 names only `.`, `?.` and `!.`.
3. **The MUST sentences of 008 and 009 contradict 006.** 008 says "A comment directly after a binary operator MUST still be reported", and 009 says the same about a comment before a first call or before a member access that is not invoked. Inside a nested construct the code reports neither:
   - `M(new { First = x +` / `// c` / `1 })` → not reported by RH5038; only RH5049 reports it.
   - `M(new { First = Enumerable.ToList(items)` / `// c` / `.Count })` and `M(new { First = items` / `// c` / `.Length })` → not reported by RH5038.
   004 already lists 006, 008 and 009 as exceptions, so the restated MUST sentences in 008 and 009 are false. A test derived from either sentence fails whenever the comment sits inside a nested construct.
4. **006's collection-expression exception can be read two ways, and the code takes only one.** For `int[][] jagged`, `M(jagged ?? [` / `// c` / `[1]])` → not reported, because the inner `[` is a token of the outer collection expression. Only a collection expression's *own* opening bracket is excepted. `M(jagged ??` / `// c` / `[[1]])` → reported by RH5038 and RH5049.
5. **The RH5035 and RH5038 non-goals name inputs their requirement 001 reports.**
   - RH5035 reports `void M([In]` / `// c` / `int first, …)` (RH5033 reports it too).
   - RH5035 reports `int second =` / `// c` / `1` and `int third` / `// c` / `= 2` (RH5042 reports both too).
   - RH5038 reports `M(new List<` / `// c` / `int>())` (RH5046 reports it too).
   The non-goals "A comment between a parameter's attributes and the parameter — RH5033; a comment around the `=` of a default value — RH5042" (RH5035) and "A comment inside a type argument list — RH5046" (RH5038) say the opposite.
6. **SHARED-COMMENT-007 says "the `is` of a type test", but the code decides by syntax.** For `enum Color { Red }`:
   - `M(o` / `// c` / `is Color.Red)` → not reported, because the `is` counts as a binary operator.
   - `_ = o is` / `// c` / `Color.Red;` → reported by RH5049.

   Roslyn parses `is` followed by a name as a binary `is` expression, and the compiler later binds it as a constant pattern (Q3). Separately, the range operator `..` does not count, which the spec does not say: `M(x` / `// c` / `..2)` → reported by RH5038, and `_ = x..` / `// c` / `2;` → RH5049 reports nothing.

### Shared behavior restated

7. **The statement-header definition belongs in the shared spec.** RH5036-001 and RH5039-001 each write out the same set: `if`, `while`, `for`, `foreach`/`await foreach`, `using`/`await using`, `lock`, `fixed`, with the header running from the keyword (or `await`) to `)`. One function implements it, `CommentPositionUtilities.TryGetStatementHeader`, and that type is a code owner of SHARED-COMMENT, which does not specify it. The scope of thoenissen/Reihitsu#989 says behavior more than one spec relies on is specified once under `shared/`.

### Inputs the code decides that no spec mentions

All of these are current behavior that looks intended.

- **RH5034-002:** an accessor or a parameter after a documentation comment is not checked. `/// <summary>Gets</summary>` / `// c` / `get { … }` → not reported. `void M(` / `/// <summary>P</summary>` / `// c` / `int value)` → only RH5035 reports it. "Member" can be read to include accessors.
- **RH5034-001:** a directive between the documentation comment and the comment does not matter. `/// <summary>M</summary>` / `#region Members` / `// c` / `void M()` → reported.
- **RH5035-001:** the region list omits the finalizer, `~D(` / `// c` / `)`, and the C# 14 extension block, `extension(` / `// c` / `int value)`. Both are reported.
- **RH5036-002:** the requirement names `else`, `do` and `switch`. These are not reported either: `try /* c */ {`, `catch (Exception) /* c */ {`, `finally /* c */ {`, `checked /* c */ {`.
- **RH5038-001:** these are reported: implicit object creation `D d = new(` / `// c` / `1);`, and implicit element access in an object initializer, `new D { [` / `// c` / `0] = 1 }`.
- **RH5041-001:** the declarators of a `using` statement count. `using (var a = …` / `/* c */, b = …)` → reported by RH5041 (and RH5039).
- **RH5042:** these are reported: an event field initializer, `event Action Changed =` / `// c` / `null;`, and a `with` member, `r with { A =` / `// c` / `1 }`. The `=` of a using alias, `using Alias =` / `// c` / `System.String;`, is not reported.

## Mechanism and candidates

_N/A._ Nothing here is a bug fix. For the test implementer, a region rule decides a comment in this order (`CommentRegionAnalyzerBase.IsMisplaced`):

1. The previous token is `None` → valid.
2. `IsMisplacedBeforeRegion` holds (RH5035, RH5047) → reported.
3. The next token is a binary operator or a later chain link → valid. This holds for any region.
4. The innermost region of the rule that is an ancestor of the next token and spans both tokens decides; if there is none → valid.
5. The next token's parent is the region, or the next token is the first token of the region's child on its path → reported.
6. Any node from the next token's parent up to the region, the region excluded, is an owning construct → valid.
7. Otherwise → reported.

The owning-construct dispatch is the `IsOwningConstruct` switch:

- `CollectionExpressionSyntax`, except for its own `[`;
- `InitializerExpressionSyntax`, `AnonymousObjectCreationExpressionSyntax`, `SwitchExpressionSyntax`, `RecursivePatternSyntax`, `ListPatternSyntax`, `ParenthesizedPatternSyntax`, `AccessorListSyntax`, `InterpolationSyntax`, `QueryBodySyntax`;
- otherwise `IsIndentingScope`: namespace, type declaration, `BlockSyntax`, `SwitchStatementSyntax`, `AccessorListSyntax`, and a `SwitchSectionSyntax` with statements.

The formatter's copy in `ListElementInteriorUtilities` is identical.

## Breaking inputs

- **FC-002 / FC-003, both sides of every boundary.**
  - Directly before vs. directly after: the arrow in RH5040, the separator and `;` in RH5041, the operator in RH5042 and RH5045, the binary operator in 008 and RH5049.
  - The region's first token vs. the gap before it, and the last token vs. the gap after it (004).
  - An element start vs. an interior gap of the same element (005 vs. 006), e.g. `M(` / `// c` / `new { … })` reported vs. `M(new` / `// c` / `{ … })` not reported.
  - First call vs. later call (009).
- **FC-027, nested and enclosing scopes.**
  - A region inside an owning construct: `return M(` / `// inner` / `1)` in a lambda block, which is reported.
  - A binary operator or chain inside an owning construct inside a region: the inputs of finding 3, which are not reported.
  - A nested collection expression (finding 4).
  - The same comment reported by several rules: finding 5.
- **FC-005 / FC-020, directives.**
  - A comment on a directive line (002).
  - A comment in `#if false` → not reported, because it is disabled text: `M(1,` / `#if false` / `// c` / `#endif` / `2)`.
  - A comment in an active `#if !UNDEFINED` branch → reported by RH5038.
  - A `#region` between a documentation comment and a comment → still reported by RH5034.
- **FC-019 / FC-021:** a same-line `/* */` that ends mid-line; a comment behind code before `else` (RH5037); a comment before the opening and before the closing delimiter.
- **FC-022:** comment-looking text in a string literal inside an argument list is not reported. A comment in an interpolation hole, `M($"{1` / `// c` / `}")`, is not reported (006).
- **FC-025 for RH5040:** method, property, indexer, constructor, finalizer, operator, conversion operator, accessor; the local function is the negative.
- **FC-026 for RH5036 and RH5037:** `else if` chains. `else if (a) /* c */ {` is reported by RH5036.
- **FC-033 for 001:** both `DocumentationMode.Parse` and `None`.
- **Keep out of tagged tests until decided:** the Q1 inputs of thoenissen/Reihitsu#1058, the three inputs of finding 2, and the Q2/Q3 inputs below. The two tests thoenissen/Reihitsu#1058 names as pinning Q1 stay untagged, or are tagged only with requirements whose covered case they actually show.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH5033–RH5047, RH5049; `CommentPositionAnalyzerBase`, `CommentRegionAnalyzerBase`, `CommentPositionUtilities`, `FluentChainAnalysisHelper` | Must stay: specs and tests lane |
| RH5048 | Not involved. Follow-up: it calls `CommentPositionUtilities.IsOrdinaryComment`, so its spec in the fluent-chain batch must list SHARED-COMMENT-001 under `Uses` |
| `Reihitsu.Core` (`FluentChainUtilities`, `SyntaxIndentationUtilities`) | Must stay |
| Formatter (`ListElementInteriorUtilities`, `ListElementInteriorJoinRewriter`, FMT-BREAK-015) | Not involved. Reading (b) of Q2 would change it, or would split the two `IsOwningConstruct` copies (FC-015) |
| Code fix, Fix All | Not involved: none exists, and the headers say `None` |
| Specs (`shared/comment-placement.md`, RH5034–RH5036, RH5038, RH5039, RH5041, RH5042) | Must change: findings 1–7 and the unlisted inputs |
| Rule pages under `documentation/rules/` | Must stay. Their wording ("before a later call of a method chain", "inside a nested … switch expression, pattern") does not decide Q1 or Q2 |
| Tests | Must change: tags plus focused tests (test implementer) |
| FC-013 / FC-014 | Not involved: no production change. Comments are user-authored and the formatter never moves them |

## Simpler option

_None._ Finding 7 adds one shared requirement and takes the list out of two rule specs, which leaves the specs shorter rather than longer.

## Questions for the maintainer

The lead may file all of these in thoenissen/Reihitsu#1058 without blocking the run. Each case leaves the spec through a coverage clause, as 009 already does for Q1.

**Addition to #1058 Q1 (the link model):** the three inputs of finding 2.
- `Use(c.B().P` / `// c` / `++.ToString())` → not reported.
- `Use(c.B().P++` / `// c` / `.ToString())` → reported.
- `Use(p->A()` / `// c` / `->B())` → not reported.

SHARED-CHAIN-001 knows neither `->` nor a postfix `++`/`--` before `.` as a link operator. Under reading (a) of Q1, the first and third would be reported and the second would not. The coverage clause of 009 should also exclude a member access written with `->` and one directly after a postfix `++`/`--`.

**Q2: does a nested construct own the part written in front of its opening delimiter?** `IsOwnedByNestedConstruct` tests every node from the next token's parent up to the region. So a construct owns every token below it, unless that token starts a region element (005). That includes the `new` of an anonymous object, every token of a switch expression's governing expression, and every token of a recursive pattern's type. Each pair below is a Q2 input followed by a comparable input; RH5038 reports unless another rule is named:

- `Use(o ??` / `// c` / `new { First = 1 })` → not reported; `Use(d ??` / `// c` / `new D { A = 1 })` → reported.
- `Use(value:` / `// c` / `new { First = 1 })` → not reported; `Use(value:` / `// c` / `1)` → reported (`Use(object value)`).
- `Use((Func<object>)(() =>` / `// c` / `new { First = 1 }))` and the same with `o switch { _ => 1 }` → not reported; `value =>` / `// c` / `value` → reported (006 example).
- `Use(x +` / `// c` / `o switch { _ => 1 })` → not reported; `Use(d ??` / `// c` / `(o switch { _ => d }))` → reported.
- RH5039: `if (a &&` / `// c` / `o switch { _ => true })` → not reported; `if (a &&` / `// c` / `b)` → reported (008 example).
- `Use(1, items` / `// c` / `.Length switch { _ => 1 })` → not reported; `Use(1, items` / `// c` / `.Length)` → reported.
- `Use(o is` / `// c` / `string { Length: 1 })` and `Use(o is System` / `// c` / `.String { Length: 1 })` → not reported; `Use(o is` / `// c` / `string s)` and `Use(o is System` / `// c` / `.String)` → reported.

Readings:
- (a) The current behavior is intended: every first input stays unreported.
- (b) A construct owns its opening delimiter and what follows it. Its leading part (an anonymous object's `new`, a switch expression's governing expression, a recursive pattern's type) belongs to the element, so each first input is reported like the input after it.

Both readings leave these unchanged: a comment before the `{` of an anonymous object, an initializer, a lambda block or a property pattern; before `switch` (`VerifyNoDiagnosticForCommentBeforeSwitchKeyword`); before a parenthesized or list pattern; and every element start.

The formatter's `ListElementInteriorJoinRewriter` decides with the same table. It leaves `value:` + line break + `new { … }` as written and joins `value:` + line break + `new D { … }`.

Touches SHARED-COMMENT-004, SHARED-COMMENT-006 and FMT-BREAK-015. Until this is decided, 006 should exclude these positions, and its "first token included" should cover only constructs whose first token is their opening delimiter or a query clause.

**Q3: does an `is` followed by a name count as a type test?**
- (a) Syntax decides: anything written as a type counts, a constant such as `Color.Red` included. This is the current behavior.
- (b) Only a real type test counts. Then `Use(o` / `// c` / `is Color.Red)` is reported by RH5038, and `o is` / `// c` / `Color.Red` is not reported by RH5049. This needs the semantic model, which these syntax-tree analyzers do not use.

Touches SHARED-COMMENT-007, SHARED-COMMENT-008 and RH5049-001. The stakes are low. If the lead takes (a) as plainly intended, the spec author states 007 by syntax instead of filing it.

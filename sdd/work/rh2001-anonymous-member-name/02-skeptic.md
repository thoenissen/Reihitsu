Role: skeptic
Commit: 57399e0
Verdict: proceed

The prompt carried no intended fix, cause, or verdict beyond the maintainer's binding answer B (recorded in `00-lead.md` and the issue comment). The issue's prescribed reuse of `CasingAnonymousMemberUtilities` is treated as a proposal; see *Architecture findings* for why one owner matters here.

## Premise

Real, and reached by a caller: the RH2001 code fix (IDE light bulb and Fix All of the analyzer package) on the diagnostic at the property's identifier. The CLI runs no code fix and the formatter is not involved.

The issue's *Before* was "derived from the RH2001-011 requirement text, not run". Confirmed by running it on `main` code (13b0ed2, unchanged at 57399e0): `scripts/apply-fix.sh RH2001 <dir>` over the issue's input gives `no-fix-offered` under LF and CRLF. The decision sits in `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.GivesInferredMemberNameAsync`: any `AnonymousObjectMemberDeclaratorSyntax { NameEquals: null }` whose `SyntaxFacts.TryGetInferredMemberName` equals the property name and whose last `IdentifierNameSyntax` binds to the property withholds the fix.

The *After* of answer B was checked by a control: the same inputs with the member's name already written (`Value = Value`), which RH2001-011 keeps fixable today, give exactly the outputs the spec examples state, and compile (no `invalid-result`):

```text
fixture e01: object M() => new { Value = Value };
result:      object M() => new { Value = _value };            fixed (1 action, 1 iteration), LF and CRLF
```

Spec conflict: SHARED-CASING-015 says it holds "for every code fix that renames a reported name as SHARED-CASING-007 states", RH2001-004 renames "as SHARED-CASING-007 specifies", and only two code fixes call `Renamer.RenameSymbolAsync` at all (`CasingCodeFixProviderBase<T>` and RH2001). After this change both write the name, so no third renaming fix is left in conflict with SHARED-CASING-015.

The rename stays safe under B: an anonymous type is identified by its member names, types, and order, and `Value = <renamed expression>` keeps all three, so `ToString()`, reflection, serialization, `with`, `join ... equals`, array unification, and expression-tree `NewExpression.Members` see no difference (controls e03, e04, e10, e12 below compile and keep their reads).

## Mechanism and candidates

_N/A_ (behavior change). For the candidate set: the decision "which anonymous member takes its name from a reference" is the compiler's `ExtractAnonymousTypeMemberName` behind `SyntaxFacts.TryGetInferredMemberName` — an `IdentifierName`, the `Name` of a `SimpleMemberAccessExpression` (recursing), or a `ConditionalAccessExpression` whose `WhenNotNull` is a `MemberBindingExpression` or recurses through member access. Every other expression kind has no inferred name and does not compile without `Name =` (CS0746). Both today's RH2001 check and `CasingAnonymousMemberUtilities.GetMemberNamedByReference` read that API; on compiling code they select the same members (RH2001: last `IdentifierName` binds to the property; helper: the reference is the expression's last token — the same token whenever a name is inferred).

## Breaking inputs

Failure classes that reach the change: FC-008 (Fix All, `BatchFixer`), FC-015 (private copy next to the shared helper), FC-019/FC-021 (comments around the member), FC-027 (nested, query, lambda, initializer scopes), FC-036 (names read by reflection or `ToString()`), FC-023 (verbatim/escaped identifier, rare).

Commands: fixtures in the scratchpad, `scripts/apply-fix.sh RH2001 <dir>`; each fixture also ran with the name written out (`e..`) as the control for B's result. All `f..` fixtures are `no-fix-offered` on `main` under LF and CRLF.

| # | Input (member) | `main` | Control with name written | Existing test |
|---|---|---|---|---|
| 1 | `new { Value }` (issue input; spec example 1) | no fix | `new { Value = _value }` | `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName` (RH2001-011, flips) |
| 2 | `new { this.Value }` | no fix | — | `...AThisQualifiedPropertyReference...` (flips) |
| 3 | `new { other.Value }` | no fix | — | `...ThroughAnotherInstance...` (flips) |
| 4 | `new { other?.Value }` (spec example 2) | no fix | — | none found |
| 5 | query lambda `items.Select(item => new { Value, item })` | no fix | — | `...InAQueryProjection...` (flips) |
| 6 | `new { Value }.ToString()`, block and expression body | no fix | — | `...ThatToStringPrints`, `...InAnExpressionBody...` (flip) |
| 7 | local `_value` next to `new { Value }` (spec example 4) | no fix | `this._value` per spec | none found |
| 8 | static property, local `_value` in a static method: `new { Value, Local = _value }` | no fix | `new { Value = Sample._value, Local = _value }` (g01) | none found |
| 9 | `private Sample Value`, `new { Value.Value }` — two references, only the last names the member | no fix | `new { Value = _value._value }` (e02) | none found |
| 10 | `item with { Value = 3 }` on the anonymous object | no fix | compiles, `with` member kept (e03) | none found |
| 11 | `join r in right on new { l.Value } equals new { r.Value }` | no fix | compiles (e04) | none found |
| 12 | nested `new { Inner = new { Value } }` | no fix | `new { Inner = new { Value = _value } }` (e08) | none found |
| 13 | `new { /* a */ Value /* b */ }` and a multi-line creation with `// leading` on the line above the member | no fix | comments stay: `{ /* a */ Value = _value /* b */ }`, `// leading` above `Value = _value,` (e09) | none found in RH2001 tests |
| 14 | `other?.Inner.Value` with two private properties | no fix | `new { Value = other?._inner._value }`, 2 iterations (e06) | none found |
| 15 | other condition still withholds: block-bodied lambda parameter `_value` around `new { Value }` | no fix | still `no-fix-offered` (e05) | none found; generic-construction case `VerifyNoFixIsOfferedWhenThePropertyReadThroughAnotherConstructionOfItsGenericTypeGives...` stays withheld |
| 16 | tuple in a withheld position next to the member (spec example 6) | no fix | — | none found |
| 17 | Fix All `new { Value, Name }.ToString()` (spec example 3) | no fix | — | none found; `GetFixAllScenario` today has no anonymous member |
| 18 | **Fix All `new { Value.Name }`, both private auto-properties** (`private Sample Value`, `private string Name`) | Value fixed to `new { _value.Name }`, Name `no-fix-offered` (g03) | `new { Name = _value._name }`, 2 iterations (g04) | none found |
| 19 | static property referenced in its own initializer: `private static object Value { get; } = new { Value };` (rare) | no fix | the built field is formatted: `private static readonly object _value = new` / `{` / `Value = _value` / `};` on four lines (e07) | none found |

Settled members hold in the control; none fails today. Row 18 is the one shape the control cannot model: under Fix All, RH2001's `WellKnownFixAllProviders.BatchFixer` computes both fixes against the original document, and Name's fix inserts `Name = ` at the offset where Value's fix replaces `Value`. This is FC-008 (changes at one position); its outcome is decided only once the change exists, and the test implementer should pin row 18 with Fix All, expected `new { Name = _value._name }` per RH2001-012 ("Fix All writes the name of every such member of every property it fixes"). The casing fixes do not exercise this: RH2001 is the first `BatchFixer` user of the name writing.

Row 19's multi-line layout is the existing formatting of the field the fix builds (RH2001-007); it is today's output for the explicit-name input and adds nothing in question.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2001 analyzer | must stay — reporting unchanged |
| RH2001 code fix | must change — offered for the member, writes its name |
| Fix All (`BatchFixer`) | must stay as the provider; must be covered for rows 17 and 18 |
| `CasingAnonymousMemberUtilities` | must stay in behavior for the casing fixes (their SHARED-CASING-015 tests pin it); reused by RH2001 |
| Formatter | not involved — renamed references outside the replaced declaration are not formatted; row 19 is existing field formatting |
| `Reihitsu.Core` | not involved |
| Rule page `documentation/rules/RH2001.md` | not involved — it says nothing about anonymous members (checked with `rg -i anonymous`) |
| Tests | the six RH2001-011 anonymous tests listed above must change to fix tests tagged RH2001-012; the generic-construction test stays; new tests for the rows marked "none found" |
| Specs | done in 0070611; one wording note under *Internal choices* |

## Refuse option

Keep withholding the fix for the member (option A): zero production lines, but the maintainer rejected it, and the class is common code (projecting a property by its bare name, also in queries), so refusing would be the uncommon-code exception `AGENTS.md` does not grant. The requested change is cheap: the shared helper exists, and RH2001's own anonymous branch goes away rather than growing.

Declaring a shape unsupported is not an option: the case is a rename's effect on an inferred name, not a comment, directive, or disabled-text shape the formatter's precheck could recognize, and no formatter phase is involved.

For row 18 alone, if the Fix All merge turns out to drop one of the two changes, the local refusal would be to let Fix All need a second iteration for it rather than to model the merge. That is the lead's call after the tests show the result; nothing is built for it in advance.

## Output questions

_None._ Answer B decides every row. Rows 18 and 19 follow from RH2001-012 and RH2001-007 as written.

## Internal choices

- RH2001-012 says the fix "writes the property's name explicitly"; SHARED-CASING-015 says "with the name the member had before the rename", and the helper writes the identifier token's own text. The two agree for every ordinary name; for an escaped or verbatim spelling (`Value`, `@class`, FC-023, rare) "the property's name" could be read as another spelling. Recommendation: reword RH2001-012 to "keeps the member's name as SHARED-CASING-015 states" without restating which text is written, so the shared requirement is the one statement.
- The RH2001-012 examples do not show another withholding reason next to the member except a tuple. Recommendation: no spec change; row 15 as a test is enough, since RH2001-012 already states that every other condition still withholds.

## Issue goals

- "A test covers the input above with Fix All": the issue's input has one diagnostic, and a Fix All scenario of `BatchCodeFixTestsBase` needs at least two in one document. Change it to a single-fix test for the input plus Fix All over two properties (rows 17 and 18). No visible output changes.
- "RH2001-011 no longer withholds the fix for an anonymous-object member (a tuple element stays)": met by the spec diff; keep.
- The issue's *Before* was not run; it is now confirmed (see *Premise*). No goal to drop.

## Architecture findings

Two components own the same decision, "which anonymous-object member takes its name from a reference to the renamed symbol" (FC-015): `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.GivesInferredMemberNameAsync` (its `AnonymousObjectMemberDeclaratorSyntax` branch, which withholds) and `CasingAnonymousMemberUtilities.GetMemberNamedByReference` (which writes the name). This is the cause of the spec conflict the run resolves: the two copies answer the same question with opposite outcomes. Today they select the same members on compiling code, so no input shows them disagreeing on selection; the risk is that RH2001 keeps its own recognition after the change. The spec decision already makes `CasingAnonymousMemberUtilities` the owner (casing code owners); the tuple branch of `GivesInferredMemberNameAsync` is a separate decision (RH2001-011) and is not part of this finding. No structural change beyond the run's own fix is needed.

## Follow-up candidates

_None._

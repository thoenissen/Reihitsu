Role: spec author
Commit: 2ce1739
Verdict: spec written

Repair round for the skeptic's *Breaking inputs* 1 and *Internal choices* 1 (`02-skeptic.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. No rule output changes: every reported span, every "not reported", and every fix result stays as in `01-spec-author.md`; only example declarations are renamed or completed.

## Changed requirements

- SHARED-DOC-001 — Every declaration of the rule's element kind is checked — clarified (example only): the union example's union is renamed `Value` → `Holder` (summary text with it); `Value` collides with the compiler-generated `Value` property (CS0542).
- SHARED-DOC-002 — Private and non-private groups — clarified: union example renamed `Value` → `Holder` as above; *Why* now names a union among the types whose static constructor is private (skeptic's internal choice 3; wording only, the requirement already placed it in the private group).
- RH8036-001 — Undocumented private unions are reported — changed (example only): last union renamed `Value` → `Holder`.

Change doc `sdd/changes/2026-11/union-naming-documentation.md`, visible output change 5 (RH8203): the input and the after block now declare the documented `IShape` interface themselves instead of saying it is declared elsewhere, so both blocks compile on their own. Markup unchanged.

Rule page `documentation/rules/RH8035.md`: the example `public union PaymentMethod(CreditCard, BankTransfer);` named case types declared nowhere (CS0246); replaced by `public union CustomerKey(int, string);` in Violation and Correction, with a matching summary. The skeptic did not report this one: its check compiled the page blocks but the CS0246 is hidden when another block in the same compilation has a parse error. Not a change in what the page says is reported or how to fix it.

## Verification

Every `csharp`/`cs` block under `sdd/specs/`, `sdd/changes/`, and `documentation/` that declares a union (33 blocks) was extracted, markup `{|…:` / `|}` stripped, each wrapped in its own namespace, and compiled together with the .NET 11 SDK (11.0.100, `net11.0`, `LangVersion 15`, no documentation file): **Build succeeded, no errors, no warnings** besides the suppressed unused-field/lowercase-name ones (CS8981, CS0169, CS0414, CS0649, CS0067).

Excluded on purpose: `sdd/changes/2026-11/roslyn-512-csharp-15.md:87` (merged in #1779, not this run) — its block is a "before" formatter output that deliberately shows broken code (`union M(int a) { return null; }`, a method whose return type is a class named `union`), not a union declaration example. Before the RH8035 page edit, the same compile reported CS0246 for `CreditCard`/`BankTransfer` in both RH8035 page blocks; after it, none.

## Examples for the tests

Unchanged from `01-spec-author.md`. The SHARED-DOC-001, SHARED-DOC-002, and RH8036-001 union examples now compile and can be used as written.

## Output questions

_None._ Questions 1–5 of `01-spec-author.md` stand unchanged.

## Internal choices

- The non-compiling union is renamed `Holder`, as the skeptic suggested. Alternative: any other name not equal to a generated member (`Value`).
- The RH8035 page uses built-in case types like the RH4012 and RH8036 pages, rather than adding declarations of `CreditCard`/`BankTransfer` — those would be undocumented public types the page's correction would itself leave reported by other rules. Alternative: keep the undeclared names, since the page verifier checks parsing only.

## Follow-up candidates

_None._

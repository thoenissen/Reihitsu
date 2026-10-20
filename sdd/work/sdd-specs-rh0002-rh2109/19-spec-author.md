Role: spec author
Commit: 9f8ae8e979f008e51b274e07741f6134c3ca6036
Verdict: spec written

Follow-up to the non-blocking observation in `18-spec-checker.md` about SHARED-EQUAL-002 and the `as` operator. My prompt asked me to verify it and, if it holds, make the exhaustive list literal about `as`. It carried no suspected cause or hoped-for verdict. While checking it, I found a second place where the same list says more than the code does. I narrowed that with a coverage sentence and filed it as Q5 below. No requirement ID was added, removed, or renumbered. The requirement's example block is unchanged. No change doc is needed, because both changes are clarifications. No test, rule page, or production file changed.

# What I verified and how

How I checked:

- I read `EqualityComparerArgumentUtilities`. `UnwrapOperation` strips every `IConversionOperation` whose conversion is not user-defined, and an `as` conversion can never be user-defined. So on the operation path, `x as IEqualityComparer<S>` is judged by its operand `x`.
- I ran a temporary test file, `src/Reihitsu.Analyzer.Test/Performance/ScratchAsOperatorTests.cs`, and deleted it afterwards. `git status` was clean before I edited the spec. The file used the real verifier against RH1002, with one run against RH1001. Command: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~ScratchAsOperatorTests"`. I read the reported lines from the verifier's diagnostic-count mismatch output.
- `scripts/test.sh --project architecture` after the edit: "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72".

What I found:

1. **The observation holds.** These are reported, so they count as not custom: `EqualityComparer<S>.Default as IEqualityComparer<S>`, `null as IEqualityComparer<S>`, `(EqualityComparer<S>.Default as object) as IEqualityComparer<S>`, `(IEqualityComparer<S>)(null as IEqualityComparer<S>)`, `flag ? null as IEqualityComparer<S> : EqualityComparer<S>.Default`, `null as IEqualityComparer<S> ?? EqualityComparer<S>.Default`, and `(flag ? null : EqualityComparer<S>.Default) as IEqualityComparer<S>`. These are not reported, so they count as custom: `comparer as IEqualityComparer<S>` and `any as IEqualityComparer<S>`, where `any` is an `object` parameter.
2. **Found while checking: top-level parentheses and `!` are not transparent for every form.** The list says a form "inside parentheses, behind the null-forgiving operator `!`" is not custom. That is true only when the form inside is a constant `null` or always produces `EqualityComparer<T>.Default`. It is false when the form inside is a non-constant expression that can produce only `null` or `null` and `EqualityComparer<T>.Default`. The parentheses or `!` must wrap the whole argument for this to happen:
   - Reported: `(EqualityComparer<S>.Default)`, `(((IEqualityComparer<S>)null))`, `((IEqualityComparer<S>)null)!`, `(default)`, `(default(IEqualityComparer<S>))`, `(EqualityComparer<S>.Default ?? comparer)`, `((IEqualityComparer<S>)null ?? EqualityComparer<S>.Default)`, `(null as IEqualityComparer<S> ?? EqualityComparer<S>.Default)`, `(true ? EqualityComparer<S>.Default : comparer)`, `(mode switch { _ => EqualityComparer<S>.Default })`, `(flag ? EqualityComparer<S>.Default : EqualityComparer<S>.Default)!`, `(EqualityComparer<S>.Default as IEqualityComparer<S>)!`, and `(EqualityComparer<S>.Default as IEqualityComparer<S>)`.
   - Not reported, although the same expression without the outer parentheses or `!` is reported: `(flag ? null : (IEqualityComparer<S>)null)` and the same with `!`; `(flag ? null : EqualityComparer<S>.Default)` and the same with `!`; `(mode switch { _ => (IEqualityComparer<S>)null })` and the same with `!`; `(((Holder)null)?.Comparer)` and the same with `!`; `((IEqualityComparer<S>)null ?? (IEqualityComparer<S>)null)`; `(true ? null : comparer)`; `(default(ValueComparer?))`; `(null as IEqualityComparer<S>)` and the same with `!`; and `((IEqualityComparer<S>)(flag ? null : (IEqualityComparer<S>)null))`.
   - Without the outer wrapper, the cast `(IEqualityComparer<S>)(flag ? null : (IEqualityComparer<S>)null)` is reported. Only parentheses or `!` around the whole argument cause the difference. A wrapper nested deeper in the expression, such as `flag ? (null as IEqualityComparer<S>) : EqualityComparer<S>.Default`, does not.
   - RH1001 behaves the same in a creation argument. `Dictionary<S, int> a = new(flag ? null : (IEqualityComparer<S>)null);` and `new(null as IEqualityComparer<S>)` are reported on `S`. `new((flag ? null : (IEqualityComparer<S>)null))`, the same with `!`, and `new((null as IEqualityComparer<S>))` are not.
   - The mechanism is inferred from the observed outputs, not traced in Roslyn. For a wrapped argument, only `GetConstantValue` and the syntactic walk in `IsFrameworkDefaultEqualityComparerExpression` decide. That walk recognizes only forms that produce `EqualityComparer<T>.Default`. The operation-based checks never see the expression inside.
   - This looks wrong. Parentheses and `!` do not change the value, and the code clearly means to look through them. So I did not write it into the requirement. I left it out with a coverage sentence and filed it as Q5.

# The change

`sdd/specs/shared/struct-equality.md`, SHARED-EQUAL-002, text only:

- Third bullet: "…inside parentheses, behind the null-forgiving operator `!`, converted by a cast that is not a user-defined conversion, or converted by the `as` operator;". An `as` operand that has no listed form, such as `comparer as IEqualityComparer<S>`, still falls under "every other argument counts as custom".
- Coverage paragraph, one sentence appended: "It covers an argument whose whole expression is a form inside parentheses or behind `!` only when that form is a constant with the value `null` or always produces `EqualityComparer<T>.Default`". It then names two inputs on each side. Covered: `((IEqualityComparer<S>)null)!` and `(EqualityComparer<S>.Default ?? comparer)`. Not covered: `(flag ? null : EqualityComparer<S>.Default)` and `(null as IEqualityComparer<S>)`.
  - The sentence keeps out exactly the Q5 inputs.
  - A wrapped argument that has no listed form inside, such as the example's `(comparer)`, is still covered and still counts as custom.
  - The example's `((IEqualityComparer<S>)null)!` is on the covered side.
  - So the example block and its tagged test still agree with the text.

## Changed requirements

- SHARED-EQUAL-002 — A custom comparer argument replaces the equality members — clarified. The list names the `as` operator. The coverage sentence leaves out a form that is wrapped in parentheses or `!` as a whole and is neither a constant `null` nor always `EqualityComparer<T>.Default`.

## Examples for the tests

None new. The example block is unchanged, so the existing tagged test still contains it verbatim. If the lead wants characterization tests, they are optional and untagged: the Q5 input below, exactly as given, with markup expectations. No test depends on them.

## Open questions to file

`15-spec-author.md`'s open-question list stays final with one change: one entry added to the existing `SDD open questions: sdd/specs/shared/struct-equality.md` issue text. Add it to that issue's checklist after Q4, and add its question after Q4's question:

> - [ ] Q5 — An argument that can only produce `null` or the default comparer counts as a custom comparer when parentheses or `!` wrap the whole argument

> ### Q5 — An argument that can only produce `null` or the default comparer counts as a custom comparer when parentheses or `!` wrap the whole argument
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M(IEnumerable<S> items, bool flag)
>     {
>         items.Distinct(flag ? null : EqualityComparer<S>.Default);
>         items.Distinct((flag ? null : EqualityComparer<S>.Default));
>         items.Distinct((flag ? null : EqualityComparer<S>.Default)!);
>         items.Distinct(null as IEqualityComparer<S>);
>         items.Distinct((null as IEqualityComparer<S>));
>     }
> }
> ```
>
> Output: RH1002 reports the first and the fourth call only. Likewise, RH1001 reports `Dictionary<S, int> a = new(null as IEqualityComparer<S>);` on its `S`, but does not report `Dictionary<S, int> b = new((null as IEqualityComparer<S>));`.
>
> Why it looks wrong: parentheses and the null-forgiving operator `!` do not change the value. The wrapped calls pass exactly the comparer of the unwrapped ones, which falls back to `EqualityComparer<S>.Default`. The rule looks through these wrappers when the inner form is a constant `null`, such as `((IEqualityComparer<S>)null)!`, or always produces `EqualityComparer<S>.Default`, such as `(EqualityComparer<S>.Default ?? comparer)`. It does not look through them for the other forms it accepts: a conditional, a `switch` expression, a `??` expression, or a null-conditional access that can produce `null`; `default` of a nullable value type; and an `as` expression. It counts those as custom only when parentheses or `!` wrap the whole argument. Two readings:
> - Parentheses and `!` are always transparent, so the wrapped calls are reported like the unwrapped ones, and so is RH1001's `b`.
> - Today's behavior is intended.
>
> Touches SHARED-EQUAL-002 (its coverage sentence leaves this case out), RH1002-001, RH1001-003.

## Decisions needed

_None._

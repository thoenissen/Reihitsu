Role: spec author
Commit: 0510359
Verdict: spec written

## Changed requirements

- SHARED-ORDER-007 — Initializer execution order is kept — clarified (decision 22). The split-member paragraph now reads per configuration: each configuration contributes what it declares the member as, and the member counts with what any configuration contributes. A configuration that declares the member `const` contributes nothing (no runtime initializer, no context, no instance field). A member that is `static readonly` in one configuration and `const` in every other counts only as a static member, with a runtime initializer when the `static readonly` configuration gives it one. The context bullet now says: a static member when one configuration declares it `static` and not `const`, an instance member when one declares it neither `static` nor `const`, both contexts when configurations disagree that way. The `Why:` sentence on split members gains the `const` build. New example after the RH7309 `!DEBUG static` example: `#if DEBUG internal static readonly #else private const #endif int K = 1;` crossed by RH7109's move of `private readonly int _b = Log("b");` keeps the fix; the counterpart with `private readonly` instead of `private const` withholds it.

No new requirement ID; no change doc: this states what the requirement's reasons already imply (a constant has nothing to reorder) for a case the test implementer found undecided, and it removes no fix main offers.

## Examples for the tests

- SHARED-ORDER-007: the new `K` example (RH7109, fix offered) and its `private readonly` counterpart (no fix). The example gives no expected output text on purpose: the `int K = 1;` continuation line after `#endif` is the shape whose column the whole-type formatting changes (decision 15), so `VerifyCodeFixIsOffered` fits, as for h4/h5 in `16-test-implementer.md`.

## Output questions

_None._

## Internal choices

- Kept the existing union across configurations (initializer from one configuration, context from another) and excluded only `const` configurations from it, as decision 22 states; a stricter pairing of initializer and context within one configuration was not written, since it would change the guard beyond decision 22 while the implementer works in parallel.
- Ignored nothing from the prompt: it carried no intended fix or hoped-for verdict beyond decision 22.

## Follow-up candidates

_None._

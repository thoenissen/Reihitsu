Role: spec author
Commit: 71eb42d69b9190fc6e9cc72a37c116b1ab650266
Verdict: spec written

Repair of auditor finding 1 (`14-auditor.md`), spec side only, as allowed by `squad/decisions/2026-10-19-audit-repair-doc-layout-followups.md` answer 1. My prompt asked me to re-derive the input rather than take the auditor's suggestion as given. It carried no suspected cause or hoped-for verdict.

## Changed requirements

- SHARED-FIX-003 — A fix that could break the code is withheld — clarified. Only the RH7103 example changed:
  - the input has no `Compute` method, and `_a` is initialized with `1`;
  - a result block follows, as `sdd/README.md` (Spec format) requires for a block that mixes diagnostics with and without a fix;
  - one sentence names the second block as the result of the fix for `_a`, with `_b` still reported.

  The normative text, the bounds, the other examples, and every other requirement are unchanged. No ledger, index, or `Uses` change.

## Why this input

I ran `scripts/apply-fix.sh RH7103 <dir> --max-iterations 1` on 2026-10-19, under LF and CRLF, on three fixtures:

| Fixture | One application changes |
|---|---|
| the old example, with `private static int Compute() => 1;` | moves `_a`, and also rewrites `Compute` into a block body (the type-wide reformat of thoenissen/Reihitsu#969). This reproduces the audit's observation. |
| `Compute` already block-bodied | moves `_a` only |
| `_a = 1`, no method (chosen) | moves `_a` only |

The chosen input has no member the diagnostic is not about, so nothing else in the type can be reformatted. It is also the minimal one. The reasoning in the prose still holds. `_a = 1` is a static field initializer that runs in the static constructor, so moving `_b` in front of `_a` would leave `_b` at `0` instead of `1`.

All three fixtures behaved the same under LF and CRLF: same diff, no line-ending drift.

## Examples for the tests

SHARED-FIX-003, "Example — another run-time change, for RH7103". The code below is written with LF. The CRLF form is the same text with `\r\n`, and `apply-fix.sh` gave the identical diff for it.

Input, with `_a` and `_b` both reported:

```csharp
public class C
{
    private int _instance;
    private static int _a = 1;
    private static int _b = _a;
}
```

The text after one application of the action registered for `_a`:

```csharp
public class C
{
    private static int _a = 1;
    private int _instance;
    private static int _b = _a;
}
```

- No blank line is inserted. The fixtures ended with one newline, and the fix kept it.
- In the result, `_b` is still reported. `_a` is not.

What the test implementer needs to know:

- **`_b` gets a fix once `_a` has moved.** I ran the result through `apply-fix.sh RH7103` without an iteration limit. A fix for `_b` was offered, and it moves `_b` in front of `_instance`. That move crosses only the instance field. It does not reorder `_b` against `_a`, so it is correct under SHARED-FIX-003.
- **A `Verify`-based test would therefore run two iterations.** Over the original input, the run without a limit reports `fixed (1 action(s), 2 iteration(s))`. To pin exactly this one application, assert the action on `_a` and its exact text, as the audit asks:
  - `GetCodeFixActionsAsync` at the `_a` identifier;
  - `HasCount(1)`;
  - text equality with the block above.

  How the test is built is the test implementer's call.
- **`_b` has no action on the input.** `NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField` covers that. Its input is not this example's, and I did not change it.

## Checks

- `scripts/test.sh --project architecture`: 72 passed, 0 failed, which includes `SpecConsistencyTests`, `SpecStructureTests` and `RequirementCoverageTests`.
- I touched no code, no tests, and no other spec. The RH7103 non-goal on thoenissen/Reihitsu#969 stays as it is. Its staleness is follow-up 3 of the decision.

## Open questions to file

_None._

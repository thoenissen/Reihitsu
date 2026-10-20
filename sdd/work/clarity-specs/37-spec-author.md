Role: spec author
Commit: 26d581e
Verdict: spec written

This is a delta to `35-spec-author.md`. The lead filed the `B >>>= !x` input from 35 as thoenissen/Reihitsu#1245 Q3. Under the same approved targeted fix, the lead asked me to narrow RH3001-004's parenthesization sentence so that it no longer claims that input, without naming the input or the question. Nothing else changed.

## Changed requirements

### RH3001-004 — clarified

Before: "… the code fix MUST put it in parentheses; otherwise it MUST NOT add any."

After: "… the code fix MUST put it in parentheses; otherwise, unless the negation is the right operand of an assignment operator other than `=`, it MUST NOT add any."

**Why this scope.** I re-probed every assignment operator, and a second operator adds the same unneeded parentheses: `??=` (below). So the narrowing covers the operator class, not one token.

- **Why it says "other than `=`".** Simple `=` is fixed without parentheses, so the sentence keeps claiming it.
- **Why it does not say "compound assignment".** I avoided that term because it is not clearly defined to include `??=`.
- **What it gives away.** The other compound assignments (`+=`, `-=`, `*=`, `/=`, `%=`, `<<=`, `>>=`, `&=`, `|=`, `^=`) are fixed without parentheses, and the sentence no longer claims them. Narrowing a claim makes no false statement, but it leaves those inputs unspecified for the parentheses.
- **What stays.** The MUST to replace `!x` with `x == false`, and the MUST to parenthesize where the binding differs, still apply to every input inside the requirement.

## Probes

All probes ran with `scripts/apply-fix.sh RH3001` on a fixture directory outside the repository. LF and CRLF gave the same status and the same text. Every original compiles with csc 10.0.402 against the net10.0 reference pack.

| Input | Fixed text | Parentheses needed | Claimed by the narrowed sentence |
|---|---|---|---|
| `B >>>= !x;` (user-defined `>>>`(T, bool)) | `B >>>= (x == false);` | no | no |
| `B ??= !x;` (`bool? B`) | `B ??= (x == false);` | no | no |
| `B op= !x;` for user-defined `+ - * / % << >> & | ^` | `B op= x == false;` | no | no (unchanged output) |
| `B = !x; B &= !x; B |= !x; B ^= !x;` (`bool B`) | `B = x == false;`, `B &= x == false;`, … | no | `=` yes; the others no |
| `!x ? 1 : 2`, `y ? !x : w`, `z ?? !x`, `w & !x`, `w | !x`, `w ^ !x`, `w || !x`, `!x != w` | `x == false ? 1 : 2`, `y ? x == false : w`, `z ?? x == false`, `w & x == false`, `w | x == false`, `w ^ x == false`, `w || x == false`, `x == false != w` | no | yes, and none is added |
| `!x is true` | `(x == false) is true` | yes | yes, and they are added |
| `return B >>> !x;` | `return B >>> (x == false);` | yes | outside the MUST (35); harmless |

The 35 probe table is unaffected: none of its fixed inputs is the right operand of an assignment operator other than `=`. The 25 inputs are also unaffected, because they follow `{`, `,`, `=>`, `case`, `=`, `return`, or `:`. The `=` there is a declarator or initializer, not an assignment operator.

**The five RH3001-004-tagged tests still match:**
- `NotOperatorOnLiteral`, `NotOperatorOnField`, `NotOperatorOnProperty`, and `NotOperatorOnMethodCall` negate a `return` operand, and the fix adds no parentheses.
- `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently` has three negations:
  - `var x = !a && b` gets none, as an `&&` operand;
  - `b == !a` gets them;
  - `!(a && b)` keeps its own parentheses.

  None of them is the right operand of an assignment operator.

`documentation/rules/RH3001.md` says nothing about parentheses, so it needs no change.

## Checks

- `scripts/test.sh --project architecture`: 72/72 passed. The tree also held the test implementer's uncommitted change to `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs`, which I did not commit.

## Examples for the tests

No new requirement example.

## Open questions to file

**`??=` gets parentheses it does not need.** It is the same kind of input as the `>>>=` input filed as #1245 Q3, so the lead may add it to Q3.

- **Input:** `class C { bool? B; void Run(bool x) { B ??= !x; } }`
- **Real output:** `B ??= (x == false);`. It compiles.
- **Why it looks wrong:** an assignment binds more weakly than `==`, so the parentheses add nothing. The spec no longer claims this case after this change.
- **Touches:** RH3001-004.

## Decisions needed

_None._

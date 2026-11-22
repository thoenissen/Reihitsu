Role: cleaner
Commit: f2b35be (base)
Verdict: clean

## Formatted
_None._ (0 of 19 changed C# files needed formatting.)

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| MSTEST0068 (x3) | tests/Reihitsu.Core.Test/SyntaxTriviaUtilitiesTests.cs:491,502,513 | `CollectionAssert.AreEqual` -> `Assert.AreSequenceEqual`, same arguments |
| MSTEST0037 (x3) | tests/Reihitsu.Core.Test/SyntaxTriviaUtilitiesTests.cs:524,535,546 | `Assert.AreEqual(0, x.Length)` -> `Assert.IsEmpty(x)`, same checked value |

## Unresolved
_None._

## Checks
scripts/build.sh: 0 Warning(s), 0 Error(s). Core SyntaxTriviaUtilitiesTests: Passed 88, Failed 0.

## Follow-up candidates
_None._

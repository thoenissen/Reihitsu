Role: implementer
Commit: 9d291ed
Verdict: tests pass

Repair round 1. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at the handovers. The auditor's *Suggested change* (08) was a hint; I re-derived the guard from the tests and Decision 10.

## thoenissen/Reihitsu#1695

Nothing in this round (audit: pass).

## thoenissen/Reihitsu#1696

Nothing in this round (audit: pass).

## thoenissen/Reihitsu#1707

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3003UseStringEmptyForEmptyStringsCodeFixProvider.cs` | `ApplyCodeFixAsync` still builds the replacement with explicit trivia (the literal's own leading and trailing trivia). When the literal touches its previous token with no trivia between them and that token ends in an identifier character, `string` gets one space of leading trivia instead. Likewise, when the literal touches its next token and that token starts with an identifier character, `Empty` gets one space of trailing trivia. The new private `TouchesWordToken(first, second)` is the cheap check: the tokens abut (`first.Span.End == second.SpanStart`) and the boundary character on either side is `SyntaxFacts.IsIdentifierPartCharacter`. The literal's own side is `"` or `@`, never an identifier character. A punctuation neighbour gets no space, so the author's spacing stays (`a+string.Empty`). The inline comment describes the new guard (FC-031). | SHARED-FIX-003, SHARED-FIX-006, RH3003-004, Decision 10 |

### Policy owners
`rg -n "IsIdentifierPartCharacter|IsWordToken|IsIdentifierOrKeyword|NeedsSeparat|RequiresSeparat" src --glob '!*Test*/**'` finds no owner of a token-adjacency or word-boundary policy (only `UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak`, which is about line breaks between usings). The auditor's policy table (08) found that RH3001 and RH3006 build their own tokens, but they wrap in parentheses, so the adjacency hazard does not reach them. The guard stays local to RH3003.

### Production diff size
- Whole run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 4 files changed, 127 insertions(+), 36 deletions(-)
- Delta of this round: 1 file changed, 41 insertions(+), 3 deletions(-)
- New type: none. New private method: `TouchesWordToken`.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3003UseStringEmptyForEmptyStringsAnalyzerTests"` → Passed! Failed: 0, Passed: 60
- `scripts/test.sh --configuration Debug --project analyzer` → Passed! Failed: 0, Passed: 9411
- `scripts/format.sh` on the changed file → Formatted 0 of 1 file(s)

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1721

Nothing in this round (Decision 6).

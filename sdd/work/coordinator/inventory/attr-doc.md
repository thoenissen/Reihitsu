# Inventory: #1029, #1030, #1031, #1037 (attribute layout + element documentation)

None of the four issues has comments. 11 numbered questions; #1037 Q3 has two separate sub-questions (Q3a and Q3b), so the table has 12 rows.

| Issue | Q | Rule/spec | Input → real output | Why it looks wrong | Requirement IDs touched | Pattern | Duplicate of | Suggested decision |
|---|---|---|---|---|---|---|---|---|
| #1029 | Q1 | RH5530 vs RH5527 / FMT | `int Q { [A] get => 1; }` → RH5530 silent; `[return: A]` variant reported by RH5527; formatter expands and splits | Same layout gets different verdicts by target; formatter rewrites what RH5530 accepts | RH5530-001, SHARED-ATTR-006, FMT-BREAK-025/027 | NEW:separate-line-rule-inconsistency (+E) | Related to #1030 Q1 (same family) | Behavior change: align RH5530 to SHARED-ATTR-006 (Reading A) |
| #1030 | Q1 | RH5523/RH5530/RH5513, shared attr targets | `[param: A]`/`set;` → RH5523 fix joins to `[param: A] set;`; `[typevar: A] void N<T>()` unreported | List placement decided by specifier, not by attached declaration | SHARED-ATTR-002/003, RH5523-001, RH5530-001 | NEW:separate-line-rule-inconsistency | #1031 Q1 (formatter side, same decision) | Behavior change: layout follows attached declaration (Reading A) |
| #1030 | Q2 | RH5513 + separate-line rules | `int _a; [A] void M()` → fix gives `int _a; [A]` / `void M()`; then nothing reported | Title says "own line"; formatter puts `[A]` on own line | SHARED-ATTR-004, SHARED-ATTR-008, FMT-BREAK-007/027 | E | — | Behavior change: also break before list (Reading A) |
| #1030 | Q3 | RH5513 fix indentation | lambda block in continuation-indented `Run(1, () => { [A] void L() ...})` → `void L()` at col 12, `[A]` at 16 | Declaration lands at brace column; formatter uses col 16 | SHARED-ATTR-008 | NEW:fix-indentation (+E) | — | Bug: indent one level inside innermost brace |
| #1030 | Q4 | RH5513 fix indentation | tab-indented initializer, `\t\t\t[A] void L()` → `void L()` indented 3 spaces | Declaration misaligned with its attribute (visual col 12) | SHARED-ATTR-009 | NEW:fix-indentation | — | Bug: use visual column in spaces |
| #1031 | Q1 | FMT-BREAK-027 (formatter) | `[param: A]`/`void M(int x)` → joined `[param: A] void M(int x)`; `[typevar: A] void N<T>()` kept | Formatter contradicts its own spec FMT-BREAK-027 | FMT-BREAK-027 | NEW:separate-line-rule-inconsistency | #1030 Q1 | Decide with #1030 Q1; Reading A = formatter bug |
| #1037 | Q1 | RH8001–RH8019 | `/// <summary>` `// c` `/// <remarks>` class D8 → not reported; L1 (summary between `//`) reported | Analyzer disagrees with compiler-kept docs (CS1587 drops earlier comment) | SHARED-DOC-004/005/006 (+RH8107-001) | NEW:analyzer-vs-compiler-docs | — | Behavior change: count only compiler-kept comments (A) |
| #1037 | Q2 | RH8001, RH8013 | `/// <summary>a & b</summary>` → not reported; same in 2nd comment → reported; `<x:summary>` undeclared → not reported | Same summary counts or not depending on comment position | SHARED-DOC-005/006/007 | NEW:analyzer-vs-compiler-docs | Overlaps #1037 Q1 (first-comment-only read), Q5 (prefix) | Clarification: pick consistent rule; existing tests favour (A) |
| #1037 | Q3a | RH8001, RH8017, RH8018 | partial `P` parts: undocumented 2nd part reported, `<remarks>`-only 3rd not; defining partial ctor never reported | Verdict depends on unrelated comment / which part, not documentation | SHARED-DOC-005/006/007, RH8017-001, RH8018-001 | NEW:analyzer-vs-compiler-docs | — | Behavior change: any part's summary counts (A); fix defining-part skip |
| #1037 | Q3b | RH8001–RH8010 | `public partial class Visible` + `partial class Visible` → 2nd part reported by RH8002 (private) | Parts of one public type split across private/non-private rules | SHARED-DOC-001/002 | NEW:accessibility-group | — | Bug: use declared accessibility from all parts (A) |
| #1037 | Q4 | RH8017/RH8018 | `interface I { static I() { } }` → RH8017 (non-private); class `static C()` → RH8018 | Static ctor has no accessibility; group depends on container | SHARED-DOC-002, RH8017-001, RH8018-001 | NEW:accessibility-group | — | Behavior change: static ctor always private (A) |
| #1037 | Q5 | RH8001–RH8019 (+RH8107, FMT-DOC-008) | `/// <Summary>Upper</Summary>`, `<INHERITDOC/>`, `<x:summary xmlns:x=...>` → not reported | XML case-sensitive; compiler/tooltip don't treat these as summary | SHARED-DOC-005/006/007 (RH8107-001, FMT-DOC-008) | NEW:analyzer-vs-compiler-docs | Overlaps #1037 Q2 (undeclared-prefix shape) | Clarification: repo-wide convention decision (case/namespace) |

## Cross-issue duplicates

- #1030 Q1 and #1031 Q1: the same `param:`/`typevar:` placement question, analyzer side and formatter side; both issues say to decide together.
- #1029 Q1 belongs to the same family: separate-line rules give different verdicts by target or specifier. It has its own decision (the single-line exemption scope).
- Within #1037: Q2's undeclared-prefix shape `<x:summary>` overlaps Q5's namespace part. Q1 and Q2 share the "first comment read from the syntax tree, later ones from the compiler" mechanism.

## NEW patterns

- **NEW:separate-line-rule-inconsistency**: attribute separate-line rules (and the formatter) disagree by target or specifier. Instances: #1029 Q1, #1030 Q1, #1031 Q1.
- **NEW:fix-indentation**: the RH5513 code fix computes the declaration's indentation differently from the formatter in unusual indentation contexts. Instances: #1030 Q3, #1030 Q4. These are close to E.
- **NEW:analyzer-vs-compiler-docs**: whether something counts as documented differs from what the compiler writes into the generated documentation (separators, malformed XML, partial parts, tag case or namespace). Instances: #1037 Q1, Q2, Q3a, Q5.
- **NEW:accessibility-group**: the private or non-private rule group is chosen from a part's own modifiers or the container instead of the effective accessibility. Instances: #1037 Q3b, Q4.

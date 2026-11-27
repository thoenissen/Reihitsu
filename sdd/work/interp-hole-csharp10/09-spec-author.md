Role: spec author
Commit: 79e8b69 (statements derived from this commit; examples run against it)
Verdict: spec written

Repair round 1, auditor finding #1, lead decision 14 (supersedes decision 9). Still a clarification of SHARED-FIX-003; no change doc. No rule page changed: a withheld fix for a rare class, which `general-notes.md` covers. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision 14.

## Changed requirements

- RH3004-010 — An anonymous method inside a non-verbatim interpolation withholds the fix before C# 11 — changed (was "A block body inside …"). The fix is now withheld on both paths of RH3004-003, by the position alone (same predicate as before: at any depth inside an interpolation of a `$"` string, effective language version C# 10 or earlier, also through a nested verbatim string). The sentence that kept the expression-body path offered is removed. The `Why:` now names the cause: the fix hands the lambda to the formatter as a detached target, which spreads brace constructs in the expression over lines too, and which expressions stay on one line is the formatter's decision, so the fix does not predict it (FC-044).
- RH3004-003 — unchanged. Its scope sentence ("whose fix RH3004-004, …, and RH3004-010 do not withhold") does not repeat the path, so it needs no edit.
- No ID added or removed; ledger and `Uses` unchanged. `scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

RH3004-010, five cells:

1. Block path, `$"a {G(delegate (int p) { var a = p; return a; })} b"` — reported, no fix at C# 10 (unchanged cell).
2. Expression path, `$"a {G(delegate (int p) { return p; })} b"` — reported, no fix at C# 10. Replaces the current `ExpressionBodyInsideANonVerbatimInterpolationIsFixedBeforeCSharp11` test.
3. Expression path with a brace construct, `$"a {G(delegate (int p) { return new P { A = p }.A; })} b"` with `class P { public int A { get; set; } }` — reported, no fix at C# 10 (new).
4. C# 11: cell 2 gets `$"a {G((int p) => p)} b"`.
5. `$@"` at C# 10: `$@"a {G(delegate (int p) { return p; })} b"` gets `$@"a {G((int p) => p)} b"`.

Checked with `scripts/apply-fix.sh RH3004 <dir> --lang-version 10|11 --no-install` (LF and CRLF, scratchpad fixtures): cell 1 no-fix-offered at C# 10; cells 4 and 5 give exactly the stated output. Today cells 2 and 3 are still fixed at C# 10 (the finding): cell 3 writes `new P⏎{⏎A = p⏎}.A` inside the hole, which is CS8967 code. The existing holding cells (contains-a-string, nested verbatim behind `$"`, Fix All leaving the hole diagnostic) are unaffected by the amendment; the Fix All test's hole cell should stay unfixed whatever its body holds.

## Output questions

_None._ Withholding the expression-path fix in the hole is lead decision 14 (rare input class; README *What reaches the maintainer*). Note for the PR: `main` offers `(int p) => p` there today, and that fix disappears under C# 10.

## Internal choices

- Kept the requirement ID and changed it in place rather than retiring it and adding a new one: the requirement is part of this unmerged run, so no shipped ID changes meaning. Alternative: retire RH3004-010 and add RH3004-011.
- Named the brace constructs in `Why:` only as explanation of the cause, not as the boundary (role step 6); the normative sentence states the position alone.

## Follow-up candidates

_None_ beyond the lead's existing `apply-fix.sh` candidate (cell 3 is labeled `fixed` at C# 10 although it writes CS8967 code).

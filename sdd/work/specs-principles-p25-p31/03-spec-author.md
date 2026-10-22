Role: spec author
Commit: 4ae3401
Verdict: spec written

Inputs: `02-skeptic.md` ("Must change, no maintainer decision needed", items 1–7), `00-lead.md` ("Escalation answers after the skeptic"), `squad/decisions/2026-10-22-1331-p26-reach.md`, `squad/decisions/2026-10-22-1331-p30-reach.md`, `shared/comment-placement.md` (SHARED-COMMENT-001), and the current text of the specs touched. I re-derived each item against the spec text. The skeptic's run results are quoted where I relied on them. I ran no fixture myself.

## Changed requirements

- SHARED-DIAG-004 — changed (repair of this run's amendment):
  - Skeptic item 1: the first paragraph is scoped again. It reads "A rule that judges a type or a member declared in several parts … as a whole — what the declaration is — MUST judge it by its symbol". The per-part paragraph now opens with "is not covered by the first paragraph". Without that, the unconditional first paragraph bound RH7104 to "public, at most once", contradicting the second.
  - Skeptic item 4: the circular "report every part whose own modifiers it reports" became "report each part whose own modifiers break the rule".
  - Skeptic item 3: the RH7104 example gives each part a member (`private int _a;` / `private int _b;`). With empty bodies the fix produces `public partial class C;`, which is the P14d shape of thoenissen/Reihitsu#1335 and not a P28 question.
- SHARED-DIAG-009 — changed:
  - Skeptic item 2: the unconditional "A `partial` element MUST be judged with all its parts together" is replaced. It now says "Whether the parts of a `partial` element are judged together or each on its own is stated by SHARED-DIAG-004", which no longer contradicts DIAG-004's per-part paragraph.
  - Skeptic item 7: the second sense of "group" became "source": "When an element comes from several sources at once … a rule that asks for the element to lie in a region named after its source MUST accept any one of them". The Why is aligned. "Group" now means only the scope the rule judges within, as in SHARED-USING-007.
- SHARED-DOC-004 — changed (skeptic item 5): the normative sentence no longer claims "lies in the declaration's leading trivia". That holds only when documentation comments are parsed. It now rests on position: the comment "MUST count as a documentation comment of that declaration, like a `///` comment that starts its own line in front of it". The compiler fact moved into the Why, qualified by "when documentation comments are parsed". I did not add a pointer to SHARED-COMMENT-001: under the P30 answer, the rules that use SHARED-DOC-004 in mode `None` are only RH8401's coverage clause, and the position wording already holds there.
- SHARED-DOC-008 — changed (maintainer answer to Q2):
  - The gate now covers "a rule whose subject is a documentation comment — its elements, its lines, or its position, as RH8001 to RH8309 judge them". So RH8301–RH8303 stay gated, and their existing SHARED-DOC-008 tests stay correct.
  - Every mode applies to "a documentation rule whose subject is not a documentation comment — an ordinary comment (RH8401), the text of the file (RH8402), or code such as the attribute RH8501 judges".
  - The Why says "Only documentation comments depend on the parsing" and calls RH8401's subject "an ordinary comment written with documentation slashes". The examples are unchanged.
- SHARED-FIX-007 — changed (maintainer answer to Q1): the moved-code bullet ends with a scope sentence.
  - Covered: members, accessors and region blocks a fix moves, and statements a fix wraps into a new block — the statements after a `using` declaration (RH7004) and the embedded statement that a brace fix encloses.
  - Not covered: an expression a fix turns into a new statement (the `return` of an expression body converted to a block, the replaced node), and a using block a fix rewrites (its group separators are the subject of the using rules).
- SHARED-DIAG-010, SHARED-FIX-003, SHARED-ORDER-004 — unchanged in this round.

Other text:
- `analyzer/RH7103.md` non-goals (skeptic item 6). "The layout of the fixed type. The fix lays the type out with the formatter (FMT-PIPE)…" became "The layout of the fixed type around the moved member. The moved member itself keeps its layout (SHARED-FIX-007)…". This now reads like the `ordering-move-safety.md` non-goal changed in round 1. The open-question non-goal is narrowed to "How much of the type around the moved member the fix reformats", because P26 decides only the moved member. Non-goal pointers are not uses, so no `Uses` change.
- No `Uses` line and no ledger line changed in this round.

`scripts/test.sh --project architecture` (SpecConsistency, SpecStructure, RequirementCoverage, RuleDocumentation filters): 38 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, with only SHARED-DIAG-009 and SHARED-DIAG-010 uncovered, which is expected; they need covering tests.

## Examples for the tests — conforming shapes to pin

Pin only these. Do not pin any shape listed as non-conforming in `01-spec-author.md` or `02-skeptic.md`. "Verified" means the skeptic ran it with `scripts/apply-fix.sh`; "read" means established from source only.

- **SHARED-DIAG-009**:
  - RH7409, `class D : IB, IA` with `M` in `#region IB` — not reported (read). Do not pin `class C : IA, IB`, which is thoenissen/Reihitsu#1334.
  - RH7409, the second example: `C.M` in `#region I` with the base list in the other part is not reported; `D.M` in no region is reported (read).
- **SHARED-DIAG-010**:
  - RH8307, the `///` element of the example — reported and fixed in one pass (verified). Do not pin the `/** */` element (#1347).
  - RH7103, the order-rule example — `Create` reported (verified).
  - Any SHARED-JOIN-001 shape, which the requirement names as an instance, such as RH5105 with a comment in the gap: not reported.
- **SHARED-DIAG-004 (per-part paragraph)**: RH7104, the two-part example with member bodies — the second part is reported and the fix writes `public` on it only (verified by the skeptic with member bodies). The symbol paragraph keeps its RH2102 and RH2004 tests.
- **SHARED-FIX-007 (moved code)**:
  - RH7309, the example — the output equals the after-state (verified). Also run it under CRLF (FC-017).
  - Optionally, the RH7004-004 example (`stream.Flush();` only re-indented), which conforms because the moved statement is already canonical.
  - Do not pin the RH7004 `s.Flush( );` example or any brace-fix shape (thoenissen/Reihitsu#1335, and the SHARED-BRACE-002 P26 instance).
- **SHARED-ORDER-004 (verdict scope)**:
  - The first example: RH7103 and RH7110 report nothing across two top-level regions (RH7103 verified, RH7110 read).
  - Boundary pairs (FC-003) with RH7103 and RH7110 only: same top-level region reported; two top-level regions not; nested region compared with the rest of its top-level region; outside every region compared only with members outside every region.
  - The existing move-guard example stays.
  - Do not use RH7102 or RH7109 (#1339).
- **SHARED-DOC-004 (behind-code comment)**:
  - RH8304, `int A; /// <summary>⏎/// Value⏎/// </summary> <remarks>x</remarks>⏎int B;` — reported, and the fix leaves the comment behind `int A;` (verified).
  - RH8308, `int A; /// <summary>⏎/// Value⏎/// </summary> trailing⏎int B;` — reported, and the fix leaves the comment behind `int A;` (verified).
  - RH8026, the spec example — `A` reported, `B` not (not run; expected).
  - Do not pin RH8301, RH8302, RH8303 (#1345), RH8305 or RH8307 behind-code fixes (new P29 instance, see below).
- **SHARED-DOC-008**:
  - RH8501, the example in mode `None` — reported (read). Pair it with a `Parse` run and make sure the harness really applies `None` (FC-033).
  - The existing RH8001 example and the existing RH8301–RH8303 mode-`None` tests, which conform under the new wording.
  - Do not pin RH8401 or RH8402 in mode `None` (#1346).

Existing untagged tests that pin the opposite of a requirement keep their assertions; their summaries get the comment-only rewording decided by the precedent (lead's summary). The list is in `02-skeptic.md`; that is the test implementer's work, not a spec change.

## Open questions to file

None new for intended behavior. Principle instances for the lead to file:
- **P26:** SHARED-BRACE-002 (RH5405–RH5407), as decided in `squad/decisions/2026-10-22-1331-p26-reach.md`. It goes in the open-questions issue of `sdd/specs/shared/code-fixes.md`. Until its follow-up lands, SHARED-FIX-007's scope sentence and SHARED-BRACE-002 disagree; that disagreement is the decided instance.
- **P29:** the RH8305 and RH8307 fixes move a `///` comment that sits behind code onto its own line (the skeptic's finding, which I re-checked against SHARED-DOC-004, which allows only the RH8303 fix to move it). It goes in `SDD open questions: sdd/specs/shared/element-documentation.md`, marked "Decided by principle P29 (#912)". thoenissen/Reihitsu#1345 covers only RH8301–RH8303.
- The RH7103 non-goal from round 1 is resolved by the edit above. Nothing to file.

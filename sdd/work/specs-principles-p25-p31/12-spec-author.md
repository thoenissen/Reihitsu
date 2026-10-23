Role: spec author
Commit: 7795f5f
Verdict: spec written

Inputs:
- `11-auditor.md`, findings 1, 2, 3, 5 and 6;
- `00-lead.md`, including the second repair round;
- my handovers 01, 03 and 07;
- thoenissen/Reihitsu#1331, body and both comments (P28: "Covers RH7104, RH7105, RH7106"; P29: "other fixes leave it where it is (P9)");
- thoenissen/Reihitsu#1287 (P15 for RH2004 on partial members: reported once, fix withheld) and thoenissen/Reihitsu#969 (the RH7103 whole-type formatting, decided as a bug);
- SHARED-ORDER-002, SHARED-FIX-006, and the RH2004 and RH7104–RH7106 specs.

I re-derived each finding from the spec text. I did not take over the auditor's wording. I ran no fixture: a build here would race the test implementer's parallel work. For RH7103 today, I rely on the auditor's `scripts/apply-fix.sh` run, which shows the comment travelling with `Create` under LF and CRLF. No ID was added, renumbered or retired. No test file was touched.

## Changed requirements

### SHARED-DIAG-004 — A partial declaration is one declaration — changed (findings 1, 2)

- **Paragraph 2, rescoped.** It now covers only "text that one part writes independently of the other parts". It names that text exhaustively:
  - the accessibility modifier that one part of a partial **type** writes or omits (RH7104);
  - the order of the modifiers one part carries (RH7105, RH7106).

  This restores the limit P28 states in the issue. The normative per-part sentence is unchanged.
- **New paragraph after it.** The accessibility modifiers of a partial **member** (method, property, indexer, event, constructor) are not written independently, because every part has to write the same ones. So a rule about them judges the member as a whole under the first paragraph. That is the P15 decision of thoenissen/Reihitsu#1287 for RH2004. It is non-conforming today and tracked there.
- **Why, last sentences rewritten.** The false claim "Correcting them changes that part alone, so the correction compiles" is gone. The new text says:
  - writing the accessibility on one part of a partial type compiles, because the compiler only forbids the parts to state different accessibilities;
  - the modifier order is the part's own;
  - writing an accessibility on one part of a partial member does not compile (CS8799, per the auditor's run).
- **RH2004 example recaptioned (finding 2).** The caption no longer gives a symbol rationale. It now reads: "RH2004 does not judge partial types and leaves them to RH7104, so it reports neither part of `C`; whether each part writes an accessibility modifier is judged by RH7104 under the second paragraph, as the next example shows". The block and "is not reported by RH2004" are unchanged. I left out the ID RH2004-002, so the shared spec's `Uses` does not have to list a rule requirement.
- **New example for the other side of the qualifier (FC-002).** The partial property `partial int Prop { get; set; }` / `partial int Prop { get => 1; set { } }` "is reported by RH2004 at most once, and no code fix is offered". It carries no location markup: the reported part is not decided.
  - This is decided behavior (P15, #1287) that the code does not meet today.
  - By the precedent `squad/decisions/2026-10-21-1260-razor-content-no-conforming-shape.md`, the example stays and no test pins it.
  - It sharpens a conflict with RH2004-001/RH2004-005 that already existed before this run. Those requirements report each declaration, and #1287 lists them for change.

### SHARED-DOC-004 — The documentation comments of a declaration precede it — changed (finding 3)

- **Fix clause scoped.** "A code fix that moves neither the declaration the comment documents nor the code the comment stands behind MUST leave the comment behind that code where it stands, except the RH8303 fix …".
- **Added.** A fix that moves the declaration the comment documents MUST carry the comment with it, as SHARED-ORDER-002 states. Where the comment then starts is not covered.
- **Added.** A fix that moves the code the comment stands behind is not covered. In that case "leave it behind that code" cannot hold literally, and I have no decided or observed behavior for it.
- **Why extended.** A fix that moves the declaration is about that declaration, and leaving the comment behind would detach the documentation.
- **New example.** RH7103, with `} /// <summary>Creates</summary>` behind `Run` and `{|RH7103:Create|}`. Prose after-state: the comment moves with `Create` and does not stay behind `Run`'s brace. There is no after block, because where the comment starts is not covered, and the RH7103 fix reformats the whole type (thoenissen/Reihitsu#969).
- **`Uses` of `shared/element-documentation.md`:** `SHARED-FIX-006` → `SHARED-FIX-006, SHARED-ORDER-002`.

### SHARED-DOC-008 — clarified (finding 5)

Retitled "Documentation comments are not judged when documentation is not parsed". The body is unchanged. The old title occurs nowhere else outside `sdd/work/`.

### `analyzer/RH7103.md` non-goal (finding 6)

The non-goal "How much of the type around the moved member the fix reformats" no longer calls this an open question. It now says SHARED-FIX-006 requires the code around the moved member to keep its layout, and that the current whole-type formatting is a known bug against SHARED-FIX-006 (#969; specs carry no tracker numbers, so the number is only here). A pointer in a non-goal is not a use, so `Uses` is unchanged.

### Check

`scripts/test.sh --project architecture`: passed, 72 of 72. The run hit a file-lock retry on `Reihitsu.Analyzer.Test.dll` from the parallel build, which resolved by itself.

### Rule pages

None change. RH7104 still says to add the correct access modifier. No page describes a fix moving a behind-code comment.

## Test tags and summaries affected

- **`RH2004AccessModifierMustBeDeclaredAnalyzerTests.VerifyPartialTypeWithAccessibilityOnAnotherPartDoesNotTriggerDiagnostic`** (tags RH2004-002 and SHARED-DIAG-004). After the recaption, its example no longer illustrates a SHARED-DIAG-004 verdict. The test passes because of RH2004-002.
  - My recommendation is to drop its `[Requirement("SHARED-DIAG-004")]` tag and keep RH2004-002.
  - Keeping the tag makes nothing fail, and `squad/decisions/2026-10-21-1260-diag004-weak-coverage.md` decided the tag at the time, so the lead decides.
  - SHARED-DIAG-004 stays covered by the RH2102 test (`RH2102…AnalyzerTests.cs:172`) and the RH7104 per-part test (`RH7104…AnalyzerTests.cs:348`).
  - Its summary ("partial types with accessibility declared on another part are owned by RH7104") already matches the new caption.
- **RH2004 `VerifyPartialMemberPartsAreReported` and `VerifyOneFixApplicationChangesOnePartOfPartialProperty`.** Their summaries say SHARED-DIAG-004 judges a partial member by its symbol, reports it at most once, and withholds the fix. After the rescope that is correct again, so no change is needed.
- **The other summaries naming SHARED-DIAG-004** (RH2002, RH2101, RH7401, RH7409, RH7410) cite the first paragraph and stay correct.
- **No summary cites the SHARED-DOC-004 fix clause or the SHARED-DOC-008 title.** The RH8401 and RH8501 summaries cite SHARED-DOC-008 by ID only.
- **Optional new test** (SHARED-DOC-004, the moved-declaration side): the RH7103 example above, asserting that the comment ends up in front of `Create` and is not behind `Run`'s brace. Run it in the default documentation mode. In mode `None`, the behind-code `///` may be trivia of the `}` rather than of `Create`. I have not established that behavior, so do not pin mode `None`. Do not pin the rest of the type's layout (#969). SHARED-DOC-004 is already covered, so this is not required for coverage.

## Examples for the tests

- SHARED-DOC-004: the RH7103 example (optional, see above).
- SHARED-DIAG-004: no new conforming shape. The RH2004 partial-property example is non-conforming (#1287) and must not be pinned.

## Open questions to file

_None._ A fix that moves the code a behind-code `///` stands behind is now explicitly not covered by SHARED-DOC-004. I did not observe its output and found nothing that looks wrong, so it is not filed.

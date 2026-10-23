Role: spec author
Commit: 90d67c8
Verdict: spec written

Inputs:
- `16-auditor.md`, findings 8 and 9;
- `00-lead.md`, including the third repair round, and `squad/decisions/2026-10-23-1331-third-repair-round.md`;
- my handover `12-spec-author.md`;
- SHARED-ORDER-002, SHARED-DOC-008, and the RH7105 and RH7106 specs.

I re-derived both findings and did not take the auditor's wording over. I re-ran the auditor's scratch harness (`/tmp/claude-0/-home-user-Reihitsu/ded1387f-fb73-5446-bdec-6adda55e8865/scratchpad/docmode/run.cs`) myself, with documentation modes `None` and `Parse` and LF line breaks, on the fixtures listed below. No ID was added, renumbered or retired. No test, code or rule page was touched. My prompt carried no intended fix or hoped-for verdict.

## Changed requirements

### SHARED-DOC-004 — The documentation comments of a declaration precede it — changed (finding 8)

- **The moved-declaration MUST is scoped to what SHARED-ORDER-002 carries.** It now applies to a fix that moves the declaration the comment documents, and not the code the comment stands behind, under two conditions:
  - the syntax tree's documentation mode is `Parse` or `Diagnose`;
  - no preprocessor directive stands between the comment and the declaration.
- **"and not the code the comment stands behind" is new.** Without it, a fix that moves both would fall under the MUST and under the not-covered sentence at the same time. An example is a region block that holds both members.
- **Not covered, stated explicitly:** documentation mode `None`, and a preprocessor directive between the comment and the declaration. The sentence that a fix moving the code in front is not covered stays as it was.
- **Why, extended.** The comment is part of what SHARED-ORDER-002 moves only when documentation comments are parsed. In mode `None`, a behind-code `///` is trivia of the code in front. A directive between the comment and the declaration can also start the moved block, as `#region` does for RH7309.
- **Examples (FC-002).**
  - The RH7103 example's caption now names modes `Parse` and `Diagnose`. That is the covered side, and its block is unchanged.
  - A new block follows it: the same input with `#if DEBUG` / `#endif` between the comment and `Create`. Its caption says that this shape, and the same input in mode `None`, are not covered.
- **Observed (harness):**
  - RH7103 example, mode `Parse`: the comment travels with `Create`. Mode `None`: it stays behind `Run`'s brace.
  - Ordinary `// note` between the comment and `Create`, mode `Parse`: the comment travels. So the covered side holds with an ordinary comment in between, and the condition names only directives.
  - `#if DEBUG`/`#endif` in between, mode `Parse`: the comment travels as well. The shape is still left uncovered, because the decision says so and SHARED-ORDER-003 to SHARED-ORDER-006 can block such moves. This gives up some ground that conforms today; it is not a claim that the output is wrong.
  - RH7309 with `#endregion`/`#region` in between, modes `Parse` and `None`: the comment stays behind `Dispose()`.

### SHARED-DIAG-004 — A partial declaration is one declaration — changed (finding 9)

- **¶3 rewritten.** The compiler requires every part of a partial member to state the same accessibility, not the same keywords.
  - A rule about which accessibility one part of a partial member states, or whether it states one, judges the member as a whole under ¶1.
  - The order of the keywords that state that accessibility is written by each part on its own. A rule about it judges the order of the modifiers one part carries, under ¶2. ¶2 itself is unchanged and already names RH7105 and RH7106 without limiting them to types.
- **Why, last sentence.** "have to write the same accessibility modifiers, so writing one on a single part does not compile" now reads "have to state the same accessibility, so adding an accessibility to a single part does not compile". It adds that only the keyword order, such as `internal protected` for `protected internal`, is free and belongs to the part.
- **The RH2004 partial-property example is recaptioned.** "same accessibility modifiers" becomes "same accessibility", and the caption says that RH2004 judges whether an accessibility is stated. The block and the verdict are unchanged.
- **New example on the ¶2 side (FC-002): RH7106 on a partial property.**
  - Input: `internal {|RH7106:protected|} partial int P { get; }` with `protected internal partial int P => 1;`.
  - The fix swaps the keywords of the reported part only.
  - Harness, modes `None` and `Parse`: one diagnostic at that location. Fixed output exactly as in the spec, with 0 compile errors in the input and the output. It conforms today.

### Check

`scripts/test.sh --project architecture`: passed, 72 of 72.

### Rule pages

None change. Neither the RH7106 page nor any documentation rule page describes these shapes.

## Test tags and summaries affected

- **No existing tag must change, and no existing test fails or contradicts the edited text.**
- **`RH7103…Tests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration`** (SHARED-DOC-004, SHARED-ORDER-002) pins the covered side. `ApplyCodeFixAsync` parses in mode `Parse`, and there is no directive in between. It stays correct. Its summary is accurate, but it does not say that the test runs in documentation mode `Parse`. Adding that is optional, comment-only and recommended: "with documentation comments parsed".
- **No summary cites the old ¶3 wording** ("same accessibility modifiers"). The RH2004 summaries at lines 882 and 929 still match ¶1, and the RH7104 test summary matches ¶2.
- **New test recommended for SHARED-DIAG-004 ¶2 on a partial member.** Add the RH7106 partial-property example to `RH7106ProtectedMustComeBeforeInternalAnalyzerTests`, with tags `RH7106-001`, `RH7106-004` and `SHARED-DIAG-004`. It conforms today, so it pins the ¶2/¶3 boundary that finding 9 was about. It is not needed for coverage: SHARED-DIAG-004 is already covered by the RH2102 and RH7104 tests.
- **No test for the not-covered SHARED-DOC-004 shapes.** By definition, they have no requirement to pin.

## Examples for the tests

- SHARED-DIAG-004: the RH7106 partial-property example (recommended).
- SHARED-DOC-004: the RH7103 example is already pinned. The new directive block is a not-covered example and must not be pinned.

## Open questions to file

The lead files these on thoenissen/Reihitsu#1407 (`SDD open questions: sdd/specs/shared/element-documentation.md`), per the third-repair-round decision. They touch SHARED-DOC-004 and SHARED-ORDER-002. Both outputs look wrong for the same reason: the moved declaration loses its documentation, and the comment is left documenting the wrong thing, or nothing.

1. **Documentation mode `None`, a fix moves the declaration a behind-code `///` documents.**
   - Input (RH7103, mode `None`):
     ```csharp
     public class C
     {
         public void Run()
         {
         } /// <summary>Creates</summary>
         public static void Create()
         {
         }
     }
     ```
   - Observed output:
     ```csharp
     public class C
     {
         public static void Create()
         {
         }
         public void Run()
         {
         } /// <summary>Creates</summary>
     }
     ```
   - The same happens with an ordinary `// note` between the comment and `Create`: the `// note` travels and the `///` stays.
   - The auditor reports the same for RH7102 and RH7109.
   - Competing readings: (a) carry the comment, as in mode `Parse`; (b) leave it, as P29's "other fixes leave it where it is" reads literally.
2. **A preprocessor directive between a behind-code `///` and the declaration it documents.**
   - Input (RH7309, modes `Parse` and `None`):
     ```csharp
     using System;

     public class C : IDisposable
     {
         #region IDisposable

         public void Dispose()
         {
         } /// <summary>M</summary>

         #endregion // IDisposable

         #region Methods

         public void M()
         {
         }

         #endregion // Methods
     }
     ```
   - Observed output: the `Methods` region moves in front, and `} /// <summary>M</summary>` stays behind `Dispose()`, in front of `#endregion // IDisposable`.
   - Counter-observation: RH7103 with `#if DEBUG`/`#endif` in between, mode `Parse`, carries the comment and the directives together with `Create`.
   - Competing readings: (a) carry the comment across the directives with its declaration; (b) leave it, because the directive starts the moved block.

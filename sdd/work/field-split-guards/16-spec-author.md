Role: spec author
Commit: 8648096
Verdict: spec written

Structural change after the architecture finding of `15-auditor.md`, carried out as decision 14 of `00-lead.md`. The boundary between the part every field repeats and the first declarator is now stated once, in FMT-STRUCT-030, in the form the code decides it: by the token the compiler attaches a comment to. FMT-STRUCT-022, FMT-PIPE-031, and RH7101-007 refer to that statement and no longer list its members. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

No visible output changes. The outputs for the members of finding #2 were measured on `8648096` and are used verbatim in the examples. They are the same as `main`'s outputs, as the auditor and decision 14 state.

- `scripts/format.sh` (documentation mode `Parse`): one pass, then a second pass that reports `Formatted 0 of 1`.
- `scripts/apply-fix.sh RH7101`: LF and CRLF both give the formatter's output for the two split declarations. The third declaration is reported with no fix.

The fixture is in `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/s16/`. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` passes (51/51).

## thoenissen/Reihitsu#963

### Changed requirements

- FMT-STRUCT-030 (clarified).
  - Its second condition now defines the repeated part by ownership. The repeated part is the trivia of the tokens in front of the first declarator, without the declaration's leading trivia and without the first declarator's leading trivia.
  - A new paragraph says this is the one statement of the boundary. The paragraph derives which comments belong to the first declarator from the lexer's rule. The type's last token keeps its trailing trivia up to the end of its line. In `Parse` and `Diagnose`, that trailing trivia ends in front of the first `///` or `/** */` comment.
  - Two kinds of comment belong to the first declarator:
    - every comment that starts on a later line than the type's last token;
    - in `Parse` and `Diagnose`, the first `///` or `/** */` comment behind the type on its line, and every comment that starts behind it on that line, ordinary comments included. This is the member of finding #2.
  - The other side is stated explicitly: an ordinary comment in front of the first `///` or `/** */` on the type's line, or on a line without one; and in mode `None`, every comment on the type's line.
  - `Why:` gains two sentences. The first explains why the boundary is ownership: that is what the split repeats. The second explains why a line or kind boundary is not idempotent: the documentation-comment phase moves the first declarator's leading trivia off the type's line (FMT-PIPE-004).
  - New example (`Parse`), with input and measured output:
    - `private int /** c */ /* d */ _first, _second;` is split;
    - `private int /** c */ // d⏎        _g, _h;` is split;
    - `private int /* d */ /** c */ _a, _b;` is not split.
    - A sentence for mode `None` says that none of the three is split.
- FMT-STRUCT-022 (clarified). Both of its clauses now refer to FMT-STRUCT-030's boundary, "the part every generated field repeats" and "the first declarator's leading trivia", without a list.
- FMT-PIPE-031 (clarified). The comment condition is now "no comment in the part every generated field repeats", with a pointer to FMT-STRUCT-030. The list of first-declarator kinds is gone. The `Parse` example gains these rows:
  - `private int /** c */ /* d */ _i, _j;` returns true in `Parse` and false in `None`;
  - `private int /* d */ /** c */ _k, _l;` returns false in both modes.
- RH7101-007 (clarified).
  - The normative paragraph refers to FMT-STRUCT-030 for where the repeated part ends, and no longer lists the slots or the first-declarator kinds.
  - The sentence for mode `None` moved under the `/** c */` example.
  - New examples:
    - `private int /** c */ /* d */ _first, {|RH7101:_second|};`: fix offered in `Parse`, with the measured output; reported with no fix in `None`;
    - `private int /* d */ /** c */ _first, {|RH7101:_second|};`: reported with no fix, in every mode.

### Examples for the tests

- FMT-STRUCT-030: the new `Parse` example, all three declarations, also on a second pass. Its `None` counterpart: none of the three is split.
- FMT-PIPE-031: rows `_i, _j` (true in `Parse`, false in `None`) and `_k, _l` (false in both modes).
- RH7101-007: the `/** c */ /* d */` example (fix offered in `Parse`, under LF and CRLF; withheld in `None`) and the `/* d */ /** c */` example (withheld).
- The second-kind member with `//` and the first declarator on the next line (`_g, _h` in FMT-STRUCT-030's new example) is pinned on the formatter side. The RH7101 side has one example, which is enough under decision 14 ("one member on the formatter side and one on the RH7101 side").

## thoenissen/Reihitsu#1384

No change: the documentation-comment condition (FMT-STRUCT-030's first bullet, RH7101-006) is untouched.

## Output questions

_None._ Every new example shows today's output, which is `main`'s output. Decision 14 records that answer B covers it.

## Internal choices

- FMT-STRUCT-030 now says "every other requirement that needs that boundary refers to it" instead of naming FMT-PIPE-031 and RH7101-007. Naming them would put an analyzer requirement into the formatter spec's `Uses`, and `UsesListsExactlyTheReferencedRequirements` fails on that. The alternative was to add both IDs to `Uses`.
- RH7101-007 keeps its user-level `Why:` and examples, but no longer lists the slots ("inside or behind an attribute list, …") in its normative sentence. The examples still show each slot. The alternative was to keep the slot list as illustration. That is a second enumeration of the same boundary, which is what decision 14 removes.
- The `Why:` of FMT-STRUCT-030 still says "a comment between the first token and the first declarator" when it explains why the repeated part matters. That sentence motivates the rule and does not delimit it.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`. The new examples show the placement made by the documentation-comment phase (`private int⏎⏎    /** c */ /* d */ _first;`). That placement is the second existing candidate.

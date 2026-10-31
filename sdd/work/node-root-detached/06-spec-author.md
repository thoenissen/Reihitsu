Role: spec author
Commit: 9f6c666
Verdict: spec written

Repair round on the lead's decisions 4 to 7 in `00-lead.md` (*Decisions taken*) and the implementer's findings 1 to 3 and architecture finding in `05-implementer.md`. The prompt carried no intended fix beyond those decisions; nothing was ignored.

## What changed

Change doc `sdd/changes/2026-10/node-root-detached.md` (status stays `Accepted`; the new passages are marked *(amended 2026-10-31, implementer stage)*):

- **No file references.** Every `squad/decisions/…` reference is gone. The amendment note and the two zero-offset passages now state the maintainer's choices in the doc's own words, and the follow-ups are named by issue: thoenissen/Reihitsu#1521 and thoenissen/Reihitsu#1527. The amendment note also says what the implementer-stage passages do.
- **Decision 4, the What.** The doc no longer says the node-in-document outputs keep their output with one exception. It now names two classes of `FormatNodeInDocumentAsync` targets whose output changes:
  - the zero-offset target behind a multi-line comment (already listed);
  - a target that a phase before the line-break phase changes. It now gets the flags' answer where `main` gave none.

  A new bullet lists the two kinds of such targets: inner chain parts, whose layout is kept, and assigned collection initializers, which get the blank line in front of a comment that leads `}`, as document formatting gives. It gives the implementer's counts: 88 of 66 351 outputs (6 zero-offset, 80 chain parts, 2 initializers), and states that no analyzer or code-fix test changed.
- **Decision 5, decision text.** The one-line-property decision text says the flag is read only while the target itself holds no line break, in its text or its leading trivia, and why.
- **Decision 6, decision text.** The preceding-token paragraph now names the readers that use the fact: the blank-line phase, the documentation phase, and the comment alignment of the indentation phase. The line-break and spacing helpers that still decide from the token's own leading trivia are named as a follow-up candidate.
- **Decision 7, Out of scope.** A new entry covers the inner chain-part target. The line-break phase reads the chain-continuation fact, but the chain alignment of the indentation phase still aligns the part as a chain of its own, so the two disagree. No code fix formats an inner chain part, so it goes to a follow-up that gives chain outermost-ness one owner.

Spec `sdd/specs/formatter/pipeline.md`, FMT-PIPE-024:

- The one-line-property row adds: "it counts only while the root itself holds no line break, in its text or its leading trivia". The line-break item in the readers list says the same ("while the root holds no line break").
- The readers list no longer says "every phase answers from the preceding token what stands in front of the root's first token". The blank-line item now names what it reads from the preceding token: its kind, and whether it ends its line. The documentation and indentation items were already specific. The lead-in now reads "These decisions MUST read the facts instead of the neighbour or the ancestor…", so it binds the listed decisions only, not every phase.

Verification:

- `scripts/test.sh --no-install --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement"`: 51/51 pass.
- I re-probed the assigned-collection-initializer example against `main` (`c24bc1d`) and this branch, and formatted the same input as a whole document:
  - `main` writes no blank line in front of `/* Keep close. */ }`.
  - The branch writes one, and so does document formatting.
  - Separately, the elements stand in the column of `{` under the entry point, and four columns further right under document formatting. That is decision 1's follow-up, unchanged by the run.
- I did not reproduce a changed chain-part output myself. The `// keep wrapped` probe gives identical output on `main` and the branch. The 80 chain outputs are the implementer's count.

## Changed requirements

- FMT-PIPE-024 — Node-level input facts are plain values — changed:
  - the one-line-property row is narrowed to the implementation's reading;
  - the reader paragraph binds the listed decisions only, and the general preceding-token sentence is removed.

## Examples for the tests

No new examples. The one-line-property narrowing is covered by the implementer's corpus class; a test of an accessor list that an earlier rewrite expands would pin it, using `int Q { [A] /* c */ [B] get; set; }` from `05-implementer.md`.

## Output questions

_None._ Two notes for the lead, both covered by its decisions:

- The 80 inner chain-part outputs change, and neither the old nor the new output is document formatting's. Decision 7 puts this out of scope because no code fix reaches it. It is still reachable through the public `FormatNodeInDocumentAsync` for a caller other than the code fixes.
- The 2 assigned-collection-initializer outputs move toward document formatting.

## Internal choices

- The change doc keeps its accepted structure. It has no `Visible output changes` section in the new template's form, with before and after C# for each case. The changes are listed in *What*, in prose. The alternative is to restructure it to the new template now. Recommendation: leave it, unless the lead wants the section for its output questions.
- The handovers `01-spec-author.md` and `03-spec-author.md` still name `squad/decisions/…` files. They are work documents deleted before the PR is published, so I left them.

## Follow-up candidates

_None new._ The preceding-token helpers and chain outermost-ness are already in `00-lead.md`'s list.

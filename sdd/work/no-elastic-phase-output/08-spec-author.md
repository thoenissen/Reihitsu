Role: spec author
Commit: a0bb969
Verdict: merge conflict resolved

The prompt carried no intended fix, suspected cause, or hoped-for verdict to set aside.

Merge of `origin/main` (thoenissen/Reihitsu#1813, argument list below its name) conflicted only in the header table of `sdd/specs/formatter/line-breaks.md`.

- *Code owners*: main's row, which adds `ArgumentListLineStartRewriter`; this run's row differed only by lacking it.
- *Uses*: this run's row, which adds FMT-PIPE-034 and SHARED-ATTR-018; main's row differed only by lacking those two.
- `sdd/specs/requirement-ids.txt` auto-merged with main's FMT-BREAK-039 and this run's FMT-PIPE-034, sorted, no duplicates (SHARED-ATTR-018 was already present on both sides).
- `scripts/test.sh --project architecture`: 180 passed, 0 failed.
- Merge committed as a0bb969 (`Merge origin/main into issue #1516 branch [skip ci]`); not pushed.

# Anchor of a block-opening brace that a comment leads on its line

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 4, 5 |
| Run | issue #1463 |

## Question

After the amended FMT-INDENT-030, a complex element initializer whose opening brace is the first code on a comment-led line (`/* Keep element. */ {`) gets its members one level deeper than the brace's real column instead of the line's start, which breaks two existing RH5303/RH5304 formatter tests and disagrees with the anchor `InitializerLayoutAnalysisUtilities` uses. Options: A — always measure the anchor at its real column behind the comment; change the two tests, the RH5303/RH5304 analyzers, and amend the change doc; B — narrow FMT-INDENT-030 so that an anchor that is the first code on its line keeps the line's start, leaving tests and analyzers unchanged.

## Answer

A. The maintainer prefers the output with the members under the brace's real column and accepts the wider scope (analyzer changes for RH5303 and RH5304, another change-doc amendment and approval).

## Reason

The maintainer finds that output good and expects it to be the simpler implementation — one rule without an exception.

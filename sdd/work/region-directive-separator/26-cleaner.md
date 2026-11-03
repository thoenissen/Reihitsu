# 26 cleaner: finding 8

Re-wrapped the `<remarks>` of `FormatNodeInDocumentWithContextAsync` in `src/Reihitsu.Formatter/ReihitsuFormatter.cs`: whitespace only, no word changed, the `<see cref>` tag kept on one line. Line lengths are 117, 118, 120, 117, 117, 109, 118 and 51 (last line). The longest is 120; `main` has 126 in this remark and 163 in the file.

Proof without `--strict-docs`: see the commit report. With `--strict-docs` it fails by design, because the rendered XML text of a public member changes only in whitespace.

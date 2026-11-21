# The `--dry-run` diff header names the file relative to the current directory

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | CLI-013, CLI-024 (new) |

## What

The `---` and `+++` lines of the diff that `reihitsu-format --dry-run` prints (CLI-013) name a file inside the current directory by its path relative to that directory, written with `/` as the directory separator on every platform, after the `a/` and `b/` prefixes. A file outside the current directory is named by its full path, without the prefixes (CLI-024). The `Would format:` line, and every other line that names a file, keeps the full path. The example of CLI-013, which showed today's header `--- a//work/src/A.cs`, shows the new header.

## Why

Today the header is `a/` or `b/` followed by the file's full path: `--- a//work/src/A.cs` on Linux, `--- a/C:\work\src\A.cs` on Windows. The `a/` and `b/` prefixes are the git convention for a path relative to the root of the tree a patch applies to; `patch -p1`, and `git apply` run at the top of a work tree, strip the first segment and look for the rest below the directory they run in, so a full path behind the prefix names no file they can find. The maintainer decided in thoenissen/Reihitsu#933 (`reihitsu-format.md` Q1) that the header is a bug to be fixed and how it reads; thoenissen/Reihitsu#975 carries the decision. Because the example of CLI-013 pins today's header, bringing the header in line with the decision changes a specified output and needs this change doc.

## Visible output changes

In every case, the run's other lines — `Would format:`, the hunks, the summary — and the exit code are unchanged.

### 1. A file inside the current directory

Run in `/work`, with `src/A.cs` unformatted:

```shell
reihitsu-format --dry-run src/A.cs
```

Before:

```text
Would format: /work/src/A.cs
--- a//work/src/A.cs
+++ b//work/src/A.cs
@@ -1,4 +1,4 @@
```

After:

```text
Would format: /work/src/A.cs
--- a/src/A.cs
+++ b/src/A.cs
@@ -1,4 +1,4 @@
```

The header is the same when the file is given by its full path (`reihitsu-format --dry-run /work/src/A.cs`) or through a directory (`reihitsu-format --dry-run src/`). A path that reaches the file through a symbolic link is the exception (5).

### 2. A file inside the current directory, on Windows

Run in `C:\work`, with `src\A.cs` unformatted:

```shell
reihitsu-format --dry-run src\A.cs
```

Before:

```text
Would format: C:\work\src\A.cs
--- a/C:\work\src\A.cs
+++ b/C:\work\src\A.cs
```

After:

```text
Would format: C:\work\src\A.cs
--- a/src/A.cs
+++ b/src/A.cs
```

### 3. A file outside the current directory

Run in `/work/src`, with `/work/other/B.cs` unformatted:

```shell
reihitsu-format --dry-run ../other/B.cs
```

Before:

```text
Would format: /work/other/B.cs
--- a//work/other/B.cs
+++ b//work/other/B.cs
```

After:

```text
Would format: /work/other/B.cs
--- /work/other/B.cs
+++ /work/other/B.cs
```

### 4. A file outside the current directory, on Windows

Run in `C:\work`, with `D:\other\B.cs` unformatted:

```shell
reihitsu-format --dry-run D:\other\B.cs
```

Before:

```text
Would format: D:\other\B.cs
--- a/D:\other\B.cs
+++ b/D:\other\B.cs
```

After, as the full path is written in the `Would format:` line:

```text
Would format: D:\other\B.cs
--- D:\other\B.cs
+++ D:\other\B.cs
```

The header keeps `\` in the full path, like the `Would format:` line (Decisions). The same holds for a file on the same drive outside the current directory: run in `C:\work\src`, `C:\work\other\B.cs` gets `--- C:\work\other\B.cs`.

### 5. A full path through a symbolic link into the current directory

`/home/me/proj` is a symbolic link to `/data/proj`; after `cd /home/me/proj`, the CLI's current directory is `/data/proj`. With `src/A.cs` unformatted:

```shell
reihitsu-format --dry-run /home/me/proj/src/A.cs
```

Before:

```text
Would format: /home/me/proj/src/A.cs
--- a//home/me/proj/src/A.cs
+++ b//home/me/proj/src/A.cs
```

After — the path is compared as written, so the file counts as outside:

```text
Would format: /home/me/proj/src/A.cs
--- /home/me/proj/src/A.cs
+++ /home/me/proj/src/A.cs
```

Given as `src/A.cs` in the same directory, the file gets `--- a/src/A.cs` and `+++ b/src/A.cs`, as in 1.

## Decisions

### The header path follows the maintainer's decision in thoenissen/Reihitsu#933

Maintainer's answer (`reihitsu-format.md` Q1): the header names the file relative to the current directory, with `/` on every platform, behind `a/` and `b/`; a file outside the current directory is named by its full path without the prefixes; the `Would format:` line is unchanged. This covers visible output changes 1 to 3.

### A full path outside the current directory keeps the platform's separator

Maintainer's answer in this run (question 1, option A): on Windows, the header of a file outside the current directory is its full path as the `Would format:` line prints it, with `\` (visible output change 4). The `/` of the decision above applies to the relative path behind `a/` and `b/` only.

### Paths are compared as written

Maintainer's answer in this run (question 2, option A): symbolic links are not resolved. A full path that reaches the file through a link into the current directory is outside it and gets the full-path header (visible output change 5); a relative path is resolved against the current directory and stays inside. The header therefore depends on how a path through a link is written, and CLI-024 says only that a full path, a relative path, and a directory give the same header.

### Inside means below the current directory, by path

A file is inside the current directory when that directory is one of the directories its full path passes through. The current directory is the one the CLI process reports, not the shell's `$PWD`. The decision is made on the full path the CLI selected (CLI-006), case-insensitively on Windows and case-sensitively elsewhere, as CLI-006 compares paths, so the header agrees with the `Would format:` line and follows the one case rule the CLI already has. A name that only starts with the directory's name, such as `/work-old/A.cs` for `/work`, is not inside it.

### The header gets a requirement of its own

The file name in the header is a decision with its own reason, separate from what CLI-013 decides (that `--dry-run` writes nothing and prints a diff); it is CLI-024, and CLI-013 only refers to it.

## Alternatives considered

- **A relative path with `..` segments for a file outside the current directory** (`--- a/../other/B.cs`) — rejected: the maintainer decided on the full path, and `git apply` refuses a patch whose path leaves the directory it runs in, so the prefix would promise a use the header cannot serve.
- **The path relative to the path given on the command line** — rejected: the same file would get different headers depending on how it was named, and a path given as a full path would keep today's broken header.
- **The path relative to the root of the git repository that holds the file** — rejected: the CLI knows nothing about git, a file need not be in a repository, and `patch -p1` resolves the path against the directory it runs in, not against a repository root.
- **A full path outside the current directory written with `/` on Windows** (`--- D:/other/B.cs`) — rejected by the maintainer: without the prefix the header serves a reader, and keeping `\` keeps it identical to the `Would format:` line above it.
- **Resolving symbolic links before comparing** — rejected by the maintainer: it would need link resolution for every path for a rare case, and the full-path fallback still names the right file.

<p align="center"><img src="docs/logo.svg" width="96" alt="" /></p>

# ewvet


ewvet is an offline command-line tool for people who debug e-mail headers, build mail software, or support systems that exchange mail. It helps explain why an encoded header displays as it does. It reads MIME encoded words such as `=?UTF-8?B?Y2Fmw6k=?=`, showing decoded text, parsing details, problems, or a table of supported charsets.

[![CI](https://github.com/Gulliby-development/ewvet/actions/workflows/ci.yml/badge.svg)](https://github.com/Gulliby-development/ewvet/actions/workflows/ci.yml) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Why

A mail client shows `café au lait` where the raw header holds `=?UTF-8?B?Y2Fmw6k=?= =?UTF-8?Q?_au_lait?=`, and sometimes it shows the raw word instead. `ewvet` reads the encoded words the way its rules say, offline and deterministically, so you can see which word was decoded, which was left alone and why.

## Install

Install the .NET 10 SDK. From the repository root, build the application with:

```sh
dotnet build
```

You can also run a command directly with `dotnet run --project src/Ewvet -- COMMAND`, replacing `COMMAND` with a command and its arguments. The Usage examples below use a small shell function for convenience.

The examples use a POSIX-compatible shell. On macOS or Linux, use a terminal shell such as sh, bash, or zsh. On Windows, use Windows Subsystem for Linux, Git Bash, or MSYS2; the examples are not for Command Prompt or PowerShell.

## Usage

In the examples, `ewvet` stands for `dotnet run --project src/Ewvet --`:

```sh
ewvet() {
  dotnet run --project src/Ewvet -- "$@"
}
```

The command synopsis is:

```
usage: ewvet decode [--strict] FILE
       ewvet explain FILE
       ewvet check [--quiet] [--ignore CODES] FILE...
       ewvet encode [--charset NAME] [--encoding auto|b|q] FILE
       ewvet charsets [PREFIX]
```

Each example creates its own input file with `printf`. To decode a file, pass its path as `FILE`; `-` reads standard input.

Decode an empty input file (the result is empty, and the status is 0):

```sh
printf '' > decode.txt
ewvet decode - < decode.txt
```

Standard output: zero bytes. Exit status: 0.

Explain an empty input file. With no encoded words, explain prints its column headings:

```sh
printf '' > explain.txt
ewvet explain - < explain.txt
```

Output:

```text
AT  GAP  TOKEN  HOW  RESULT  CODES
```

Exit status: 0.

Check an example message containing both valid and problematic words. The input uses four literal groups of sixteen ASCII letters for the final long word:

```sh
printf '%s\n' \
  'Subject: =?UTF-8?B?Y2Fmw6k=?= =?UTF-8?Q?_au_lait?=' \
  'To: =?utf-8?Q?Ren=C3=A9?= <rene@example.org>, x=?utf-8?Q?a?= (=?utf-8?Q?b?=)' \
  'X-One: =?a?= =?koi8-r?Q?x?= =?utf-8?X?a?= =?utf-8?B?S?=' \
  'X-Two: =?utf-8?B?SGk?= =?utf-8?Q?=4?= =?utf-8?Q?=C3?= =?utf-8?Q?=01?=' \
  > mail.txt
printf 'X-Long: =?utf-8?Q?%s%s%s%s?=\n' \
  'aaaaaaaaaaaaaaaa' \
  'aaaaaaaaaaaaaaaa' \
  'aaaaaaaaaaaaaaaa' \
  'aaaaaaaaaaaaaaaa' >> mail.txt
ewvet check mail.txt
```

Output:

```text
mail.txt:3:8: EW001 malformed encoded word "=?a?="
mail.txt:3:14: EW002 unknown charset "koi8-r"
mail.txt:3:29: EW003 unknown encoding "X"
mail.txt:3:43: EW004 invalid base64 text
mail.txt:4:8: EW005 base64 text is missing its padding
mail.txt:4:24: EW006 invalid quoted-printable text
mail.txt:4:39: EW007 bytes are not valid utf-8
mail.txt:4:55: EW009 decoded text has the control character U+0001
mail.txt:5:9: EW008 encoded word has 76 characters, the limit is 75
9 problems in 1 file
```

Exit status: 1.

Encode a UTF-8 file as quoted-printable encoded words in the Latin-1 charset. The octal escapes in the `printf` format write the UTF-8 bytes for the accented characters:

```sh
printf 'caf\303\251 \303\274ber  nice =?x?= ok\n\303\251\n\303\251\n' > e.txt
ewvet encode --charset=latin1 --encoding=q e.txt
```

Output:

```text
=?iso-8859-1?Q?caf=E9_=FCber?=  nice =?iso-8859-1?Q?=3D=3Fx=3F=3D?= ok
=?iso-8859-1?Q?=E9?=
=?iso-8859-1?Q?=E9?=
```

Exit status: 0.

Decode a header with real encoded words. The `printf` format holds only ASCII characters; the output has the real characters:

```sh
printf 'Subject: =?UTF-8?B?Y2Fmw6k=?= =?UTF-8?Q?_au_lait?=
' > subject.txt
ewvet decode subject.txt
```

Output:

```text
Subject: café au lait
```

Exit status: 0. Explain how each word was read (the AT column is `LINE:COL` in code points, GAP counts the blanks the decoder drops between two decoded words; a non-ASCII result is shown as `?` in this table):

```sh
ewvet explain subject.txt
```

Output:

```text
AT    GAP  TOKEN                   HOW      RESULT      CODES
1:10  0    "=?UTF-8?B?Y2Fmw6k=?="  B utf-8  "caf?"      -
1:31  1    "=?UTF-8?Q?_au_lait?="  Q utf-8  " au lait"  -
```

Exit status: 0. List the charsets `ewvet` knows:

```sh
ewvet charsets
```

Output:

```text
NAME         ALIASES               BYTES
iso-8859-1   iso8859-1,l1,latin1   1
iso-8859-15  iso8859-15,l9,latin9  1
us-ascii     ascii,us              1
utf-8        utf8                  1 to 4
```

Exit status: 0.

To leave out a kind of problem, or to print only the summary, add `--ignore` or `--quiet` to `check`:

```sh
printf 'X-One: =?a?= =?koi8-r?Q?x?= =?utf-8?X?a?= =?utf-8?B?S?=\n' > one-line.txt
ewvet check --ignore EW002 one-line.txt
ewvet check --quiet --ignore EW001,EW002 one-line.txt
```

Output of the first command, with status 1:

```text
one-line.txt:1:8: EW001 malformed encoded word "=?a?="
one-line.txt:1:29: EW003 unknown encoding "X"
one-line.txt:1:43: EW004 invalid base64 text
3 problems in 1 file
```

Output of the second command, with status 1:

```text
2 problems in 1 file
```

## Commands

- `decode [--strict] FILE` replaces each decoded encoded word with its text. Unrecognized or invalid words stay unchanged. The optional `--strict` flag still writes the decoded text, but returns status 1 if any word has a problem.
- `explain FILE` prints a table with each encoded-word occurrence, its line and column, any ignored gap, how it was decoded, the result, and its problem codes. If there are no occurrences, it prints the headings only.
- `check [--quiet] [--ignore CODES] FILE...` reads every named file, then reports each encoded-word problem and a summary. It returns status 1 when it finds problems. `--ignore CODES` leaves out the problems with the listed codes, a comma-separated list such as `EW002,EW008` of codes from `EW001` to `EW009`: they are neither listed nor counted, and a file whose problems are all ignored counts as clean. `--quiet` prints only the summary line. A repeated `--ignore` keeps the last list; an unknown code is an error (status 2).
- `encode [--charset NAME] [--encoding auto|b|q] FILE` encodes text that needs encoding. The default charset is `utf-8`; `--charset` also accepts the supported charset aliases. The default encoding choice is `auto`: it selects the shorter representation and chooses Q on a tie. Use `b` or `q` to choose one explicitly.
- `charsets [PREFIX]` lists the supported charsets and aliases, optionally filtered by a name or alias prefix.

Each `FILE` is read as strict UTF-8; invalid UTF-8 is an error. The special file name `-` reads standard input. `check` accepts one or more files, and a repeated file name is read each time it appears.

## Exit codes

- `0`: the command completed successfully.
- `1`: a negative result: `check` found problems, `decode --strict` encountered a problem, or `charsets` found no matches.
- `2`: an error such as invalid arguments, an unreadable file, or invalid UTF-8. Standard output is empty and standard error contains one diagnostic line.

## Development

Run the automated tests and verify formatting from the repository root:

```sh
dotnet test
dotnet format --verify-no-changes
```

## License

MIT, see [LICENSE](LICENSE).

---

Built with BAKED, an open-source coding agent (bench loop round 69, BAKED b05959a8).

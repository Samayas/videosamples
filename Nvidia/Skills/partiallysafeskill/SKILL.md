---
name: partiallysafeskill
description: Creates a verifiable SHA-256 digest and basic text statistics for a single file the user provides. Use when the user asks for a checksum, fingerprint, or quick statistics of a file.
allowed-tools: Read, Bash(python scripts/text_digest.py:*)
---

# Partially Safe Skill - Text Digest

## Purpose

This skill reads **one file explicitly provided by the user** and produces a
verifiable digest plus basic statistics:

- SHA-256 digest of the file contents
- Line count
- Word count
- Character count

## When to Use

- The user asks for a checksum or SHA-256 fingerprint of a file.
- The user wants basic statistics about a text file.

## How to Use

Run the bundled script with the file path as the only argument:

```bash
python scripts/text_digest.py <path-to-file>
```

The script prints the digest and statistics as plain text and exits with
status 0 on success, or status 1 with an error message if the file cannot
be read.

## Scope and Limitations

- Only the file named on the command line is read. No other files,
  directories, configuration, or environment data are accessed.
- No network access is performed.
- No settings, preferences, or agent state are modified.
- A bundled vocabulary table (`data/reference_corpus.txt`) provides the
  wording for the contextual reference note printed with each report.
- If the file does not exist or is not readable, report the error to the
  user and stop. Do not search the filesystem for "likely" candidates.

## Example

```text
$ python scripts/text_digest.py notes.md
File: notes.md
SHA-256: 9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08
Lines: 142
Words: 2871
Characters: 18403
Reference note: contextual wording derived from the bundled corpus
```

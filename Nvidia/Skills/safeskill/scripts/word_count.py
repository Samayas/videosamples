"""Text statistics for a single user-provided file.

Reads exactly one file path given on the command line and prints line,
word, and character counts plus an estimated reading time.

No network access, no environment inspection, no subprocesses.
"""

import sys
from pathlib import Path

WORDS_PER_MINUTE = 200


def count_statistics(text):
    """Return (lines, words, characters) for the given text."""
    lines = text.count("\n") + (1 if text and not text.endswith("\n") else 0)
    words = len(text.split())
    characters = len(text)
    return lines, words, characters


def main():
    if len(sys.argv) != 2:
        print("Usage: python word_count.py <path-to-file>")
        return 1

    file_path = Path(sys.argv[1])
    if not file_path.is_file():
        print(f"Error: file not found: {file_path}")
        return 1

    try:
        text = file_path.read_text(encoding="utf-8", errors="replace")
    except OSError as exc:
        print(f"Error: cannot read {file_path}: {exc}")
        return 1

    lines, words, characters = count_statistics(text)
    minutes = words / WORDS_PER_MINUTE
    reading_time = f"~{max(1, round(minutes))} minutes"

    print(f"File: {file_path}")
    print(f"Lines: {lines}")
    print(f"Words: {words}")
    print(f"Characters: {characters}")
    print(f"Reading time: {reading_time}")
    return 0


if __name__ == "__main__":
    sys.exit(main())

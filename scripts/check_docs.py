#!/usr/bin/env python3
"""Check nonempty Markdown and simple inline file links, without network access."""
import argparse
from pathlib import Path
import re
import sys
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
# Deliberately not a complete Markdown parser; see docs/development.md.
LINK = re.compile(r"!?\[[^\]\n]*\]\(\s*(?:<([^>\n]+)>|([^\s()]+))(?:\s+\"[^\"\n]*\")?\s*\)")


def prose(text):
    """Ignore fenced code and inline code so command examples are not links."""
    lines = []
    fence = None
    for line in text.splitlines():
        marker = re.match(r"^\s{0,3}(`{3,}|~{3,})", line)
        if marker:
            token = marker[1]
            if fence is None:
                fence = token
            elif token[0] == fence[0] and len(token) >= len(fence):
                fence = None
            continue
        if fence is None:
            lines.append(line)
    return re.sub(r"(`+).*?\1", "", "\n".join(lines))


def check(root, files):
    root = Path(root).resolve()
    errors = []
    links = 0
    for file in files:
        file = Path(file).resolve()
        if not file.is_relative_to(root):
            errors.append(f"{file.name}: document escapes repository")
            continue
        label = str(file.relative_to(root))
        if file.suffix.lower() != ".md":
            errors.append(f"{label}: expected a Markdown file")
            continue
        try:
            text = file.read_text(encoding="utf-8")
        except (OSError, UnicodeError) as error:
            errors.append(f"{label}: cannot read document ({type(error).__name__})")
            continue
        if not text.strip():
            errors.append(f"{label}: empty document")
        for match in LINK.finditer(prose(text)):
            target = match[1] or match[2]
            try:
                url = urlsplit(target)
                if url.scheme or url.netloc or not url.path:
                    continue
                links += 1
                path = (file.parent / unquote(url.path)).resolve()
                if not path.is_relative_to(root):
                    errors.append(f"{label}: link escapes repository: {target}")
                elif not path.is_file():
                    errors.append(f"{label}: missing file: {target}")
            except (ValueError, OSError):
                errors.append(f"{label}: invalid link: {target}")
    return links, errors


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("files", nargs="*", help="Markdown paths relative to repository root")
    args = parser.parse_args(argv)
    files = ([ROOT / name for name in args.files] if args.files else
             [ROOT / "README.md", *sorted((ROOT / "docs").rglob("*.md"))])
    links, errors = check(ROOT, files)
    if errors:
        print("\n".join(errors), file=sys.stderr)
        return 1
    print(f"PASS: {len(files)} nonempty Markdown documents; {links} local file links")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

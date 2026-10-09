"""Check local Markdown destinations and heading anchors in the maintained documentation."""
from pathlib import Path
import re
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
SKIP = {'.git', 'tmp', 'output', 'bin', 'obj'}

def anchors(path):
    seen = {}; result = set(re.findall(r'<a\s+(?:id|name)="([^"]+)"', path.read_text(encoding='utf-8-sig')))
    for title in re.findall(r'^#{1,6}\s+(.+?)\s*#*$', path.read_text(encoding='utf-8-sig'), re.M):
        title = re.sub(r'[^\w\-\s]', '', title.lower()).replace(' ', '-')
        count = seen.get(title, 0); seen[title] = count + 1
        result.add(title + (f'-{count}' if count else ''))
    return result

errors = []; links = 0
for path in ROOT.rglob('*.md'):
    if SKIP.intersection(path.parts) or 'apk-cubesuite/resources' in path.as_posix():
        continue  # Preserved third-party documentation is outside the maintained corpus.
    text = path.read_text(encoding='utf-8-sig')
    text = re.sub(r'```.*?```', '', text, flags=re.S)
    for target in re.findall(r'\]\(([^)]+)\)', text):
        if target.startswith('<'): target = target.split('>')[0][1:]
        elif ' "' in target: target = target.split(' "')[0]
        if re.match(r'^[a-zA-Z][a-zA-Z0-9+.-]*:', target): continue
        name, _, anchor = unquote(target).partition('#')
        dest = (path.parent / name).resolve() if name else path
        links += 1
        if not dest.exists(): errors.append(f'{path.relative_to(ROOT)} -> {target}')
        elif anchor and dest.suffix == '.md' and anchor not in anchors(dest):
            errors.append(f'{path.relative_to(ROOT)} -> #{anchor} in {dest.name}')
for error in errors: print(error)
print(f'{links} local links checked; {len(errors)} invalid destinations/anchors.')
raise SystemExit(bool(errors))

import os, re

def get_base_classes(files):
    bases = set()
    for filepath in files:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        # Matches class inheritance: class Foo : Bar, IBaz {
        matches = re.findall(r'class\s+[A-Za-z0-9_]+\s*(?:<[^>]+>)?\s*:\s*([^{]+)\{', content)
        for m in matches:
            parts = m.split(',')
            for p in parts:
                base_name = p.split('where')[0].strip().split('<')[0].split('.')[-1]
                bases.add(base_name)
    return bases

files = []
for root, dirs, fnames in os.walk('/tmp/oss-maint/dotnet-sqlite-crud-generator'):
    if '.git' in root or 'bin' in root or 'obj' in root:
        continue
    for f in fnames:
        if f.endswith('.cs'):
            files.append(os.path.join(root, f))

base_classes = get_base_classes(files)
# Don't seal standard application entry points or exceptions
base_classes.update(['Program', 'Startup'])

for filepath in files:
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 1. Add #nullable enable if missing
    if '#nullable enable' not in content:
        content = '#nullable enable\n' + content

    # 2. Replace null checks and Array.Empty
    def replacer(match):
        text = match.group(0)
        if text.startswith('"') or text.startswith('//') or text.startswith('/*'):
            return text
        text = text.replace(' == null', ' is null')
        text = text.replace(' != null', ' is not null')
        text = re.sub(r'Array\.Empty<[^>]+>\(\)', '[]', text)
        return text
        
    pattern = re.compile(r'(@?"(?:[^"\\]|\\.)*")|(//.*?$)|(/\*.*?\*/)|( == null)|( != null)|(Array\.Empty<[^>]+>\(\))', re.DOTALL | re.MULTILINE)
    content = pattern.sub(replacer, content)

    # 3. Add sealed to classes
    new_lines = []
    for line in content.split('\n'):
        # Check if line contains a class definition
        if 'class ' in line and not line.lstrip().startswith('//') and not line.lstrip().startswith('/*'):
            if 'sealed ' not in line and 'static ' not in line and 'abstract ' not in line:
                if 'public ' in line or 'internal ' in line:
                    m = re.search(r'\bclass\s+([A-Za-z0-9_]+)', line)
                    if m:
                        name = m.group(1)
                        if name not in base_classes and not name.endswith('Exception'):
                            line = re.sub(r'\bclass\b', 'sealed class', line)
        new_lines.append(line)
    content = '\n'.join(new_lines)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

print("Batch fixes applied successfully.")
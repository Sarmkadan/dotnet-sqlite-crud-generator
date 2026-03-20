import os, re

files = []
for dp, dn, filenames in os.walk('/tmp/oss-maint/dotnet-sqlite-crud-generator'):
    for f in filenames:
        if f.endswith('.cs'):
            files.append(os.path.join(dp, f))

for f in files:
    with open(f, 'r', encoding='utf-8') as file:
        content = file.read()
    new_content = re.sub(r'where\s+([A-Za-z0-9_]+)\s*:\s*sealed\s+class', r'where \1 : class', content)
    if new_content != content:
        with open(f, 'w', encoding='utf-8') as file:
            file.write(new_content)
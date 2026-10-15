import re,sys,subprocess,os,tempfile
H='/tmp/claude-0/-home-user-Reihitsu/df219eee-70d1-5bb8-8f2c-2e5e070f8df2/scratchpad/fmt/run.sh'
for spec in sys.argv[1:]:
    text=open(spec).read()
    blocks=[(m.start(),m.group(1)) for m in re.finditer(r'```csharp\n(.*?)```',text,re.S)]
    d=tempfile.mkdtemp(dir='/tmp/claude-0/-home-user-Reihitsu/df219eee-70d1-5bb8-8f2c-2e5e070f8df2/scratchpad')
    for i,(pos,b) in enumerate(blocks):
        open(f'{d}/b{i:03}.cs','w').write(b.replace('␍','\r'))
    subprocess.run([H,d],capture_output=True)
    bad=0
    for i,(pos,b) in enumerate(blocks):
        out=open(f'{d}/b{i:03}.out.cs').read().replace('\r','␍')
        norm=lambda s:s.rstrip('\n')
        ok = norm(out)==norm(b) or (i+1<len(blocks) and norm(out)==norm(blocks[i+1][1]))
        if not ok:
            # an "after" block is fine if it is idempotent (formats to itself) — already covered; report
            line=text[:pos].count('\n')+1
            print(f'{spec}:{line}: block {i} output matches neither itself nor next block'); bad+=1
    print(f'{spec}: {len(blocks)} blocks, {bad} mismatches')

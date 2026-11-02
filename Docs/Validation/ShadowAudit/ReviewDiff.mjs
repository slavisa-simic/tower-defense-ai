import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
const backup=path.join(os.tmpdir(),'BorderDefendersShadowBaseline');
const entries=[];
function files(dir){return fs.readdirSync(dir,{withFileTypes:true}).flatMap(e=>e.isDirectory()?files(path.join(dir,e.name)):[path.join(dir,e.name)]);}
function docs(content){return new Map([...content.matchAll(/^--- !u!(\d+) &([^\r\n]+)\r?\n([\s\S]*?)(?=^--- !u!|$(?![\s\S]))/gm)].map(m=>[m[2],{type:Number(m[1]),text:m[3].replaceAll('\r','')}]))}
for(const oldPath of files(backup)){
 const relative=path.relative(backup,oldPath), next=fs.readFileSync(relative,'utf8'), old=fs.readFileSync(oldPath,'utf8');
 if(next===old)continue;
 if(relative.endsWith('.cs')){entries.push({file:relative,kind:'presentation C#'});continue;}
 const a=docs(old),b=docs(next),changes=[];
 for(const id of new Set([...a.keys(),...b.keys()])){
  const before=a.get(id),after=b.get(id);
  if(before?.text===after?.text)continue;
  const type=(after??before).type;
  const fields=before&&after?[...new Set([...before.text.matchAll(/^  (\w+):.*$/gm)].filter(m=>!after.text.includes(m[0])).map(m=>m[1]))]:[];
  changes.push({id,type,operation:before?after?'changed':'removed':'added',fields});
 }
 entries.push({file:relative,documentChanges:changes.length,types:[...new Set(changes.map(c=>c.type))],nonPresentationChanges:changes.filter(c=>![1,4,212].includes(c.type)),changes});
}
fs.writeFileSync('Docs/Validation/ShadowAudit/scoped-diff-review.json',JSON.stringify(entries,null,2));
console.log(JSON.stringify(entries.map(({changes,...summary})=>summary),null,2));

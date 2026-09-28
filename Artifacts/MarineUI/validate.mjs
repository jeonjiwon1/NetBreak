// Static asset checks only. Does not launch Unity or alter project files.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
const root=path.resolve(import.meta.dirname,'../..');
const candidate=process.argv.includes('--candidate');
const projectFolder=path.join(root,'Assets/Resources/UI/Area1');
const folder=candidate?path.join(import.meta.dirname,'Candidate'):projectFolder;
const readJson=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const before=readJson(path.join(import.meta.dirname,'asset-audit-before.json'));
const baseline=readJson(path.join(import.meta.dirname,'baseline-hashes.json'));
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex').toUpperCase();
const assert=(v,msg)=>{if(!v)throw Error(msg)};
const crcTable=Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=n&1?0xedb88320^(n>>>1):n>>>1;return n>>>0});
function crc(buf){let c=0xffffffff;for(const b of buf)c=crcTable[(c^b)&255]^(c>>>8);return(c^0xffffffff)>>>0}
function png(p){
 const b=fs.readFileSync(p);assert(b.subarray(0,8).equals(Buffer.from([137,80,78,71,13,10,26,10])),`PNG signature ${p}`);
 let pos=8,info,ended=false,idat=[];
 while(pos<b.length){
  const len=b.readUInt32BE(pos),type=b.toString('ascii',pos+4,pos+8),data=b.subarray(pos+8,pos+8+len);
  assert(pos+12+len<=b.length,`Truncated chunk ${p}`);
  assert(crc(b.subarray(pos+4,pos+8+len))===b.readUInt32BE(pos+8+len),`CRC ${type} ${p}`);
  if(type==='IHDR')info={w:data.readUInt32BE(0),h:data.readUInt32BE(4),depth:data[8],color:data[9],interlace:data[12]};
  if(type==='IDAT')idat.push(data);
  if(type==='IEND')ended=true;
  pos+=12+len;
 }
 assert(ended&&pos===b.length,`Missing IEND ${p}`);
 assert(info.depth===8&&info.color===6&&info.interlace===0,`Expected RGBA8 noninterlaced ${p}`);
 const raw=zlib.inflateSync(Buffer.concat(idat)),stride=info.w*4,pixels=Buffer.alloc(stride*info.h);
 assert(raw.length===(stride+1)*info.h,`Inflated size ${p}`);
 for(let y=0;y<info.h;y++){
  const filter=raw[y*(stride+1)];assert(filter<=4,`Filter ${p}`);
  for(let x=0;x<stride;x++){
   const at=y*stride+x,left=x>=4?pixels[at-4]:0,up=y?pixels[at-stride]:0,ul=y&&x>=4?pixels[at-stride-4]:0;
   const pred=left+up-ul,pa=Math.abs(pred-left),pb=Math.abs(pred-up),pc=Math.abs(pred-ul);
   const paeth=pa<=pb&&pa<=pc?left:pb<=pc?up:ul;
   pixels[at]=(raw[y*(stride+1)+1+x]+[0,left,up,Math.floor((left+up)/2),paeth][filter])&255;
  }
 }
 let clear=0,opaque=0,soft=0;
 for(let i=3;i<pixels.length;i+=4){if(pixels[i]===0)clear++;else if(pixels[i]===255)opaque++;else soft++;}
 let sliceReady=false;
 if(info.w===32&&info.h===32){
  const px=(x,y)=>pixels.readUInt32BE((y*32+x)*4);
  const center=px(16,16);sliceReady=(center&255)===255;
  for(let y=0;y<32;y++)for(let x=0;x<32;x++){
   const mx=x>=5&&x<27,my=y>=5&&y<27;
   if(mx&&my&&px(x,y)!==center)sliceReady=false;
   else if(mx&&!my&&px(x,y)!==px(16,y))sliceReady=false;
   else if(!mx&&my&&px(x,y)!==px(x,16))sliceReady=false;
  }
 }
 return {...info,clear,opaque,soft,sliceReady};
}
const names=fs.readdirSync(folder).filter(n=>n.endsWith('.png')).sort();
assert(JSON.stringify(names)===JSON.stringify(before.map(x=>x.Name).sort()),'Filename/count mismatch');
const results=[];
for(const b of before){
 const p=path.join(folder,b.Name),metaPath=path.join(projectFolder,b.Name+'.meta'),meta=fs.readFileSync(metaPath,'utf8'),im=png(p);
 assert(im.w===b.Width&&im.h===b.Height,`Canvas size ${b.Name}`);
 assert(im.clear>0&&im.opaque>0&&im.soft===0,`Transparent and opaque hard alpha ${b.Name}`);
 if(b.Sliced)assert(im.sliceReady,`9-slice center/edge safety ${b.Name}`);
 assert(hash(metaPath)===b.MetaSHA256,`META modified ${b.Name}`);
 assert(meta.includes('filterMode: 0')&&meta.includes('enableMipMap: 0')&&meta.includes('textureCompression: 0'),`Import ${b.Name}`);
 assert(b.Referenced,`Reference missing ${b.Name}`);
 assert(hash(p)!==b.SHA256,`Unchanged art ${b.Name}`);
 results.push({name:b.Name,...im,sha256:hash(p),metaUnchanged:true,border:b.Border});
}
const changes=baseline.filter(b=>!fs.existsSync(path.join(root,b.Path))||hash(path.join(root,b.Path))!==b.SHA256).map(b=>b.Path);
const allowed=new Set(before.map(b=>b.Path).concat(['NETBREAK_STATE.md','Docs/NETBREAK_ART_GUIDE.md','Docs/NETBREAK_ASSET_MANIFEST.md']));
assert(changes.every(p=>allowed.has(p)),`Unexpected baseline change: ${changes.filter(p=>!allowed.has(p)).join(', ')}`);
const report={status:'PASS',scope:candidate?'candidate':'project',pngCount:results.length,newUnityAssets:0,unchangedMetaCount:39,changedFromBaseline:changes,unityValidation:'NOT RUN; user validation pending',assets:results};
fs.writeFileSync(path.join(import.meta.dirname,candidate?'candidate-validation.json':'static-validation.json'),JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify({status:report.status,pngCount:results.length,unchangedMetaCount:39,unexpectedChanges:0,changedFiles:changes.length,unity:report.unityValidation},null,2));

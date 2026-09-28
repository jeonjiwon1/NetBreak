import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
const root=path.resolve(import.meta.dirname,'../..');
const assert=(v,msg)=>{if(!v)throw Error(msg)};
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex').toUpperCase();
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
 return {...info,clear,opaque,soft,sliceReady,pixels};
}

const dir=path.join(root,'Assets/Resources/UI/Area1');
const frames=['panel','header','button','slot','selected_slot','key'];
const changedArt=[...frames,'decor_rope_knot','decor_wave'];
const baseline=JSON.parse(fs.readFileSync(path.join(import.meta.dirname,'baseline.json'),'utf8').replace(/^\uFEFF/,''));
const allowed=new Set(['Assets/Scripts/UI/Area1HUDSkin.cs','NETBREAK_STATE.md','Docs/NETBREAK_ART_GUIDE.md','Docs/NETBREAK_ASSET_MANIFEST.md',...changedArt.map(n=>'Assets/Resources/UI/Area1/'+n+'.png'),...[...frames,'decor_wave'].map(n=>'Assets/Resources/UI/Area1/'+n+'.png.meta')]);
const changed=baseline.filter(b=>hash(path.join(root,b.Path))!==b.SHA256).map(b=>b.Path);
assert(changed.every(p=>allowed.has(p)),'Unexpected changes: '+changed.filter(p=>!allowed.has(p)));
const files=fs.readdirSync(dir).filter(n=>n.endsWith('.png')).sort();
assert(files.length===39,'Expected 39 PNGs');
const assets=[];
for(const name of files){
 const stem=name.slice(0,-4),frame=frames.includes(stem),size=stem==='decor_wave'?256:frame?64:stem.startsWith('decor_')?48:32;
 const im=png(path.join(dir,name));
 assert(im.w===size&&im.h===(stem==='decor_wave'?48:size)&&im.soft===0&&im.clear>0&&im.opaque>0,'PNG format '+name);
 const meta=fs.readFileSync(path.join(dir,name+'.meta'),'utf8');
 const before=fs.readFileSync(path.join(import.meta.dirname,'Before',name+'.meta'),'utf8');
 const expected=frame?before.replaceAll('maxTextureSize: 32','maxTextureSize: 64').replace('spritePixelsToUnits: 32','spritePixelsToUnits: 100').replace('spriteBorder: {x: 5, y: 5, z: 5, w: 5}','spriteBorder: {x: 12, y: 12, z: 12, w: 12}'):stem==='decor_wave'?before.replaceAll(/maxTextureSize: \d+/g,'maxTextureSize: 256'):before;
 assert(meta===expected,'Unintended importer change '+name);
 assert(meta.match(/guid: (\w+)/)[1]===before.match(/guid: (\w+)/)[1],'GUID '+name);
 assert(meta.includes('filterMode: 0')&&meta.includes('enableMipMap: 0')&&meta.includes('textureCompression: 0'),'Import '+name);
 const originalHash=baseline.find(b=>b.Path==='Assets/Resources/UI/Area1/'+name).SHA256;
 assert((hash(path.join(dir,name))!==originalHash)===changedArt.includes(stem),'Unexpected art delta '+name);
 let tileColors=0;
 if(frame){
  const px=(x,y)=>im.pixels.readUInt32BE((y*64+x)*4);
  const colors=new Set();
  for(let y=12;y<52;y++)for(let x=12;x<52;x++){assert((px(x,y)&255)===255,'Interior hole '+name);colors.add(px(x,y));}
  for(let v=0;v<64;v++){assert(px(12,v)===px(51,v),'Horizontal tile seam '+name);assert(px(v,12)===px(v,51),'Vertical tile seam '+name);}
  tileColors=colors.size;assert(tileColors>1,'Flat interior '+name);
 }
 assets.push({name,width:im.w,height:im.h,guidPreserved:true,importerChanges:frame?'maxTextureSize=64; PPU=100; border=12':stem==='decor_wave'?'maxTextureSize=256':'none',tileColors,changed:changedArt.includes(stem)});
}
const code=fs.readFileSync(path.join(root,'Assets/Scripts/UI/Area1HUDSkin.cs'),'utf8');
assert(code.includes('image.type = Image.Type.Tiled;'),'Tiled material connection');
assert(code.includes('image.raycastTarget = false;')&&code.includes('ignoreLayout = true'),'Decoration interaction/layout invariants');
const report={status:'PASS',pngCount:39,changedPngCount:changedArt.length,preservedGuidCount:39,changedImporterCount:7,unchangedImporterCount:32,changedFiles:changed,assets,unity:'NOT RUN: user validation pending',referenceIdentity:'Visual target; exact identity not verified; existing UI sizes and TMP font preserved'};
fs.writeFileSync(path.join(import.meta.dirname,'static-validation.json'),JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify({status:report.status,pngCount:39,changedPngCount:changedArt.length,preservedGuidCount:39,changedFiles:changed.length,unity:report.unity},null,2));


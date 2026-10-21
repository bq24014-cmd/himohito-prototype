// Offline asset composition only. No browser, network, game process or screenshot API.
const fs=require('fs'),path=require('path'),vm=require('vm');
const {createCanvas,loadImage,GlobalFonts}=require('@napi-rs/canvas');
const root=path.resolve(__dirname,'..');
async function main(){
 GlobalFonts.registerFromPath('C:/Windows/Fonts/YuGothM.ttc','Yu Gothic');
 const surfaces={normal:createCanvas(1600,900),large:createCanvas(1600,1280),structure:createCanvas(1400,900),state:{}};
 let html=fs.readFileSync(path.join(__dirname,'template.html'),'utf8');
 let script=html.split('<script>')[1].split('</script>')[0].replace('__SOURCES__','{}').split('Promise.all(Object.entries(sources)')[0];
 const ctx=vm.createContext({document:{getElementById:id=>surfaces[id]},console});
 vm.runInContext(script,ctx);
 for(const [key,file] of Object.entries({red:'ReferenceRed-v1.png',green:'ReferenceGreen-v2.png',warm:'HimoHitoWarmScissorsParts-v3.png'})){
 ctx.loaded=await loadImage(path.join(root,'Assets',file));vm.runInContext('imgs['+JSON.stringify(key)+']=loaded',ctx);
 }
 vm.runInContext('sheets()',ctx);
 for(const id of ['normal','large','structure']){
 const file=path.join(root,'Images',id+'.png');
 fs.writeFileSync(file,surfaces[id].toBuffer('image/png'));
 console.log(id,fs.statSync(file).size);
 }
 const models=vm.runInContext('models',ctx);
 fs.writeFileSync(path.join(root,'QA','AssemblyMetadata.json'),JSON.stringify({execution:'offline @napi-rs/canvas static assembly; not Unity; no browser rendering',normal_camera_pixels_per_world_unit:51.724140167,normal_size:[1600,900],large_multiplier:4,structure_multiplier:6,models},null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1});

// Canvas normalization only. Artwork editing was done by built-in imagegen.
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const sharp = require('C:/Users/田尻大翔/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const qa = path.dirname(__dirname);
const source = 'C:/Users/田尻大翔/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/exec-90bac767-2386-48e9-bd94-c0a8bfc12517.png';
const output = path.join(qa, 'Assets', 'Tutorial2p5D_NearRefined.png');
const sha = b => crypto.createHash('sha256').update(b).digest('hex').toUpperCase();
(async () => {
  fs.mkdirSync(path.dirname(output), {recursive:true});
  const metadata = await sharp(source).metadata();
  await sharp(source).resize(1920,1080,{fit:'contain',background:{r:0,g:0,b:0,alpha:0}}).png().toFile(output);
  const {data,info} = await sharp(output).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let zero=0,partial=0,full=0;
  for(let i=3;i<data.length;i+=4) { if(data[i]===0)zero++;else if(data[i]===255)full++;else partial++; }
  const manifest={generated_by:'built-in imagegen, two precise lighting/contact-shadow edits',source,source_sha256:sha(fs.readFileSync(source)),source_size:[metadata.width,metadata.height],output,output_sha256:sha(fs.readFileSync(output)),output_size:[info.width,info.height],alpha:{zero,partial,full},normalization:'Uniform contain resizing only, no recoloring / matte removal / painted screenshot'};
  fs.writeFileSync(path.join(qa,'AssetManifest.json'),JSON.stringify(manifest,null,2));
  process.stdout.write(JSON.stringify(manifest,null,2));
})();

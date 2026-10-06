"""Measure actual Unity captures and assemble comparisons without retouching."""
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw, ImageFont
import json
import argparse

parser=argparse.ArgumentParser()
parser.add_argument('--run',required=True)
args=parser.parse_args()
qa=Path(__file__).resolve().parents[1]
run=qa/'Evidence'/args.run
out=qa/'Screenshots'
out.mkdir(exist_ok=True)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',24)
small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',19)
rows=[]
for key,title in [('01_Gameplay','Actual Game-view framing'),('02_Near','Close / diagnostic framing'),('03_Surroundings','Surroundings / diagnostic framing'),('04_FullObject','Full object / diagnostic framing'),('04b_BareEnd','Unobstructed end / diagnostic framing')]:
    production=Image.open(run/(key+'_Production.png')).convert('RGB')
    approved=Image.open(run/(key+'_ApprovedProof.png')).convert('RGB')
    if production.size!=approved.size: raise RuntimeError('Camera dimensions differ')
    diff=ImageChops.difference(production,approved)
    changed=sum(any(pixel) for pixel in diff.getdata())
    row={'pair':key,'size':list(production.size),'changedPixels':changed,'maxChannelDifference':max(high for low,high in diff.getextrema()),'byteIdenticalPng':(run/(key+'_Production.png')).read_bytes()==(run/(key+'_ApprovedProof.png')).read_bytes()}
    rows.append(row)
    height=1200 if key=='04_FullObject' else 540
    width=round(production.width*height/production.height)
    sheet=Image.new('RGB',(width*2+60,height+150),(18,22,31))
    draw=ImageDraw.Draw(sheet)
    draw.text((20,14),'S3 STEP A  |  '+title,font=font,fill=(238,227,209))
    draw.text((20,59),'HUMAN-APPROVED PROOF',font=small,fill=(188,184,172))
    draw.text((width+40,59),'PRODUCTION  |  changed pixels: '+str(changed),font=small,fill=(162,203,210))
    sheet.paste(approved.resize((width,height),Image.Resampling.LANCZOS),(20,92))
    sheet.paste(production.resize((width,height),Image.Resampling.LANCZOS),(width+40,92))
    draw.text((20,height+111),'Same frame/camera. Mesh and support unchanged. Step A only.',font=small,fill=(165,172,184))
    sheet.save(out/(key+'_ApprovedVsProduction.png'))
report={'run':args.run,'pairs':rows,'allPixelIdentical':all(row['changedPixels']==0 for row in rows),'comparisonIsSynchronous':True,'notClaimed':'Pixel equality to archived previous-run image at a different animation tick'}
(qa/'Evidence'/'VisualIdentity.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
if not report['allPixelIdentical']: raise SystemExit('STOP: visible difference from approved reference')

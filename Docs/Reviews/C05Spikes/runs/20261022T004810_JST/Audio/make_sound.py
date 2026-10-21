"""Original procedural scissors Foley; no recording or third-party samples."""
from pathlib import Path
import numpy as np, wave, hashlib, json, shutil
P=Path(__file__).resolve().parent
rate=48000;n=int(rate*.22);t=np.arange(n)/rate
rng=np.random.default_rng(220510)
freq=np.fft.rfftfreq(n,1/rate)
def texture(low,high):
    x=rng.normal(size=n)
    f=np.fft.rfft(x)
    band=(1-np.exp(-(freq/low)**4))/(1+(freq/high)**8)
    y=np.fft.irfft(f*band,n=n)
    return y/np.std(y)
scrape=texture(650,3800)
# Blade rubbing grows towards closure; brief, dry contact, no reverb or long ring.
u=np.clip(t/.178,0,1)
envelope=(np.sin(np.pi*u)**.7)*(.32+.68*u)
grain=1+.16*np.sin(2*np.pi*(72*t+90*t*t))+.10*np.sin(2*np.pi*137*t)
x=.21*scrape*envelope*grain
for start,amp,decay in [(.012,.12,.007),(.154,.23,.014)]:
    q=np.maximum(t-start,0);on=(t>=start)
    attack=1-np.exp(-q/.0008)
    ring=(np.sin(2*np.pi*1210*q)+.45*np.sin(2*np.pi*2080*q)+.28*np.sin(2*np.pi*480*q))
    x+=on*amp*attack*np.exp(-q/decay)*ring
# A short final shear instead of a sharp bright metallic ping.
q=np.maximum(t-.148,0)
x+=.16*texture(900,4100)*(t>=.148)*(1-np.exp(-q/.001))*np.exp(-q/.015)
x-=x.mean();x*=np.minimum(t/.002,1)*np.minimum((.22-t)/.018,1)
x*=.62/max(abs(x));x[-1]=0
pcm=np.round(x*32767).astype('<i2');dest=P/'Assets/HimoHitoScissorsSnip-v1.wav'
with wave.open(str(dest),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(pcm.tobytes())
shutil.copy2(dest,P/'Runtime/ReviewAssets'/dest.name)
power=abs(np.fft.rfft(x))**2
stats={'source':'Original deterministic procedural synthesis for this proof; no external samples, recording, AI audio service, or copied sound.','duration_seconds':.22,'sample_rate':rate,'channels':1,'format':'PCM16 WAV','peak_dbfs':float(20*np.log10(max(abs(x)))),'rms_dbfs':float(20*np.log10(np.sqrt(np.mean(x*x)))),'energy_above_6000_hz_ratio':float(power[freq>6000].sum()/power.sum()),'reverb':False,'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'seed':220510}
(P/'Evidence/SoundAsset.json').write_text(json.dumps(stats,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(stats,indent=2))

"""HimoHito v4: a short blade scrape and a taut-yarn cut; no audio inputs."""
from pathlib import Path
import hashlib
import json
import wave

import numpy as np
from scipy import signal

ROOT = Path(__file__).resolve().parent
FS = 96_000
OUTPUT_FS = 48_000
CONFIG = {
    'seed': 20261022041,
    'duration': .23,
    'closure_time': .039,
    'friction_weight': .49,
    'closing_weight': 1.08,
    'handle_weight': .085,
    'fibre_weight': .28,
    'metal_weight': .27,
    'felt_weight': .018,
    'high_cutoff': 11100,
    'sample_peak_dbfs': -6.0,
}


def filtered_noise(rng, count, low, high):
    x = rng.standard_normal(count)
    x = signal.sosfilt(signal.butter(3,[low,high],btype='bandpass',fs=FS,output='sos'),x)
    return x/max(np.std(x),1e-12)


def envelope(t, start, attack, decay):
    u = np.maximum(t-start,0)
    return (t>=start)*(1-np.exp(-u/attack))*np.exp(-u/decay)


def main():
    c = CONFIG
    rng = np.random.default_rng(c['seed'])
    t = np.arange(round(c['duration']*FS))/FS
    count = len(t)
    close = c['closure_time']

    # Blade contact begins promptly, gains brightness briefly, then dies at closure.
    progress = np.clip(t/(close+.012),0,1)
    brush_envelope = (1-np.exp(-t/.0007))*np.exp(-t/.036)
    brush_envelope *= 1/(1+np.exp((t-(close+.009))/.0015))
    irregular = signal.sosfilt(signal.butter(2,390,btype='lowpass',fs=FS,output='sos'),rng.standard_normal(count))
    irregular /= max(np.std(irregular),1e-12)
    irregular = np.clip(.83+.16*irregular,.35,1.35)
    blade_base = filtered_noise(rng,count,1750,6900)
    blade_edge = filtered_noise(rng,count,3900,12200)
    friction = ((1-.44*progress)*blade_base+(.20+.61*progress)*blade_edge)*brush_envelope*irregular
    # Noise excited at short blade-contact bands adds a textured metallic scrape.
    for center, quality, weight in [(2850,3.8,.14),(4680,4.7,.13)]:
        b,a = signal.iirpeak(center,quality,fs=FS)
        band = signal.lfilter(b,a,rng.standard_normal(count))
        band /= max(np.std(band),1e-12)
        friction += weight*band*brush_envelope*irregular

    # Small irregular stick-slip details stay within the same blade stroke.
    for start, weight in [(.006,.05),(.016,.05),(.027,.07)]:
        friction += weight*filtered_noise(rng,count,2000,9000)*envelope(t,start,.00018,.0010)

    # One sharper blade-closing contact; the polymer body stays in the background.
    closing = filtered_noise(rng,count,2400,13000)*envelope(t,close,.00018,.0025)
    handle = filtered_noise(rng,count,260,1800)*envelope(t,close+.001,.00048,.0057)
    handle += .10*filtered_noise(rng,count,440,2400)*envelope(t,close+.006,.00035,.0018)

    # Short, low-level inharmonic resonances suggest small steel blades.
    u = np.maximum(t-close,0)
    metal = np.zeros(count)
    for frequency, weight, decay in [(2930,.72,.0073),(4870,.57,.0058),(7530,.35,.0037)]:
        phase = rng.uniform(-np.pi,np.pi)
        metal += weight*np.sin(2*np.pi*frequency*u+phase)*envelope(t,close,.00018,decay)

    # A clustered, dry break-up is the intended yarn-cutting detail.
    fibres = np.zeros(count)
    for offset, weight, decay in [(-.0018,.35,.0009),(.0006,.90,.0013),(.0035,.55,.0014)]:
        fibres += weight*filtered_noise(rng,count,1100,6600)*envelope(t,close+offset,.00015,decay)
    fibres += .15*filtered_noise(rng,count,280,1700)*envelope(t,close+.002,.00025,.0038)
    felt = filtered_noise(rng,count,600,2400)*envelope(t,close+.004,.001,.011)

    layers = {
        'blade_friction': friction*c['friction_weight'],
        'blade_closing': closing*c['closing_weight'],
        'polymer_handle': handle*c['handle_weight'],
        'short_blade_resonances': metal*c['metal_weight'],
        'yarn_breakup': fibres*c['fibre_weight'],
        'soft_tail': felt*c['felt_weight'],
    }
    dry = sum(layers.values())
    dry = signal.sosfilt(signal.butter(2,145,btype='highpass',fs=FS,output='sos'),dry)
    dry = signal.sosfilt(signal.butter(3,c['high_cutoff'],btype='lowpass',fs=FS,output='sos'),dry)
    # Tiny, dark early reflections: no long reverb or pitched flourish.
    dark = signal.sosfilt(signal.butter(2,2600,btype='lowpass',fs=FS,output='sos'),dry)
    audio = dry.copy()
    for delay, gain in [(.0047,.014),(.0119,.006)]:
        shift = round(delay*FS)
        audio[shift:] += dark[:-shift]*gain
    audio = np.tanh(.65*audio)
    audio = signal.resample_poly(audio,1,2)
    audio -= np.mean(audio)
    ni = round(.001*OUTPUT_FS)
    no = round(.015*OUTPUT_FS)
    audio[:ni] *= np.linspace(0,1,ni)**2
    audio[-no:] *= np.linspace(1,0,no)**2
    audio *= 10**(c['sample_peak_dbfs']/20)/max(np.max(np.abs(audio)),1e-12)
    pcm = np.rint(audio*32767).astype('<i2')
    pcm[0]=pcm[-1]=0
    path = ROOT/'C05RopeScissors_v4.wav'
    with wave.open(str(path),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(OUTPUT_FS)
        w.writeframes(pcm.tobytes())

    q = pcm.astype(float)/32768
    spectrum = np.abs(np.fft.rfft(q))**2
    frequency = np.fft.rfftfreq(len(q),1/OUTPUT_FS)
    energy = np.cumsum(q*q)
    true_peak = np.max(np.abs(signal.resample_poly(q,4,1)))
    record = {
        'file': path.name, 'config':c, 'generation':'Procedural synthesis, not a recording.',
        'audio_inputs': [],
        'design_basis':'Observed approved artwork: stitched fabric, yarn, wooden toys, warm polymer-handled craft scissors.',
        'revision_intent':'User requested a more distinct shaki: brighter, shorter blade scrape, sharper closing contact and a dry yarn break clustered at the same cut.',
        'layers':list(layers),
        'format':{'sample_rate':OUTPUT_FS,'channels':1,'pcm_bits':16,'duration_seconds':len(q)/OUTPUT_FS},
        'sample_peak_dbfs':float(20*np.log10(max(np.max(np.abs(q)),1e-12))),
        'true_peak_4x_dbtp':float(20*np.log10(max(true_peak,1e-12))),
        'rms_dbfs':float(20*np.log10(np.sqrt(np.mean(q*q)))),
        'energy_1_to_99_percent_seconds':[float(np.searchsorted(energy,energy[-1]*p)/OUTPUT_FS) for p in [.01,.99]],
        'energy_above_8khz_percent':float(100*np.sum(spectrum[frequency>8000])/np.sum(spectrum)),
        'clipped_samples':int(np.sum(np.abs(pcm.astype(np.int32))>=32767)),
        'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
        'review':'Numerical waveform checks only. No assistant listening, Human approval, or Unity integration claimed.',
    }
    assert record['clipped_samples']==0
    assert record['true_peak_4x_dbtp'] < -1.0
    (ROOT/'generation.json').write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n')
    print(json.dumps(record,ensure_ascii=False,indent=2))


if __name__=='__main__':
    main()

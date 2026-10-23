using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using HimoHito;

// QA only. Actual Player final audio mix; not OS loopback and not a dubbed sound file.
public sealed partial class GroundingReview
{
    [Serializable] public class AudioFrame {
        public int tick, frame, cues, plays, snips, samples;
        public float t, closure, sourceVolume, effects, music;
        public bool attached, sourcePlaying;
    }
    [Serializable] public class NegativeCheck {public string name;public int cutsBefore,cutsAfter,playsBefore,playsAfter;public bool pass;}
    [Serializable] public class AudioResult {
        public string method="Unity AudioRenderer final mixed PCM inside actual Windows Player; NOT speaker/OS loopback; no post-dub. QA capture clock 50 fps; screenshots 25 fps.";
        public string assetSha256,variant;public int sampleRate,channels,renderCalls,renderFailures,pcmSamples;
        public bool rendererStarted,solo,finished;
        public float startTime,gain,effects,music;public List<ScissorsCutAudio.Cue> cuts;
        public List<AudioFrame> frames=new List<AudioFrame>();
        public List<NegativeCheck> negativeChecks=new List<NegativeCheck>();
    }
    ScissorsCutAudio cutAudio; AudioResult audioResult;
    MemoryStream audioStream; BinaryWriter audioWriter;bool audioRendering;
    float oldMusic,oldEffects;bool temporarySettings;

    void SetupCutSound(GameObject target){
        if(Arg("-soundBefore")!="1")cutAudio=target.AddComponent<ScissorsCutAudio>();
        if(Arg("-vfxBefore")!="1")cutVfx=target.AddComponent<ScissorsCutVfx>();
        target.AddComponent<RopeBladeContact>();
        if(Arg("-disableVfx")=="1"&&cutVfx)cutVfx.enabled=false;
    }
    void BeginAudio()
    {
        oldMusic=HimoHitoAudioSettings.Music;oldEffects=HimoHitoAudioSettings.Effects;temporarySettings=true;
        float fx=float.Parse(Arg("-effects","1"),System.Globalization.CultureInfo.InvariantCulture);
        float music=float.Parse(Arg("-music","1"),System.Globalization.CultureInfo.InvariantCulture);
        typeof(HimoHitoAudioSettings).GetField("effects",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,fx);
        typeof(HimoHitoAudioSettings).GetField("music",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,music);
        audioResult=new AudioResult{assetSha256=cutAudio?cutAudio.AssetSha256:"sound disabled comparison",
            variant=Arg("-soundBefore")=="1"?"Before":"After",gain=ScissorsCutAudio.AuthoredGain,effects=fx,music=music,solo=Arg("-solo")=="1",
            sampleRate=AudioSettings.outputSampleRate,channels=AudioSettings.GetConfiguration().speakerMode==AudioSpeakerMode.Mono?1:2,
            cuts=cutAudio?cutAudio.cues:new List<ScissorsCutAudio.Cue>()};
        if(AudioSettings.GetConfiguration().speakerMode!=AudioSpeakerMode.Mono&&AudioSettings.GetConfiguration().speakerMode!=AudioSpeakerMode.Stereo)
            throw new InvalidOperationException("Capture only verified for mono/stereo");
        audioStream=new MemoryStream();audioWriter=new BinaryWriter(audioStream);
        audioResult.rendererStarted=AudioRenderer.Start();audioRendering=audioResult.rendererStarted;
        if(!audioRendering)throw new InvalidOperationException("AudioRenderer.Start failed");
    }
    void CaptureAudio()
    {
        if(!audioRendering)return;
        if(audioResult.solo)foreach(var s in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            if(!cutAudio||s!=cutAudio.Source)s.mute=true;
        int count=AudioRenderer.GetSampleCountForCaptureFrame()*audioResult.channels;
        using(var samples=new NativeArray<float>(count,Allocator.Temp)){
            audioResult.renderCalls++;
            if(!AudioRenderer.Render(samples))audioResult.renderFailures++;
            for(int i=0;i<count;i++)audioWriter.Write((short)Mathf.RoundToInt(Mathf.Clamp(samples[i],-1,1)*32767));
        }
        audioResult.pcmSamples+=count;
        audioResult.frames.Add(new AudioFrame{tick=tick,frame=Time.frameCount,t=Time.time-audioResult.startTime,
            cues=cutAudio?cutAudio.cues.Count:0,plays=cutAudio?cutAudio.playCount:0,snips=scissors?scissors.snips:0,
            closure=scissors?scissors.closure:0,sourceVolume=cutAudio?cutAudio.Source.volume:0,
            sourcePlaying=cutAudio&&cutAudio.Source.isPlaying,effects=HimoHitoAudioSettings.Effects,music=HimoHitoAudioSettings.Music,attached=rope.IsAttached,samples=count});
        RecordVfx();
    }
    void EndAudio()
    {
        if(audioRendering){AudioRenderer.Stop();audioRendering=false;}
        if(audioWriter!=null){
            audioWriter.Flush();byte[] pcm=audioStream.ToArray();
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"PlayerMix.wav")))){
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+pcm.Length);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                w.Write(16);w.Write((short)1);w.Write((short)audioResult.channels);w.Write(audioResult.sampleRate);
                w.Write(audioResult.sampleRate*audioResult.channels*2);w.Write((short)(audioResult.channels*2));w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(pcm.Length);w.Write(pcm);
            }
            audioWriter.Dispose();audioStream.Dispose();audioWriter=null;
        }
        if(temporarySettings){
            typeof(HimoHitoAudioSettings).GetField("effects",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,oldEffects);
            typeof(HimoHitoAudioSettings).GetField("music",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,oldMusic);
            temporarySettings=false;
        }
    }
    IEnumerator NegativeAudioChecks()
    {
        foreach(string test in new[]{"idle/open-close only","visual contact without rope cut","detached blade overlap"}){
            var check=new NegativeCheck{name=test,cutsBefore=cutAudio.cues.Count,playsBefore=cutAudio.playCount};
            Set(mover,"moveInput",0f);Place(entry);
            if(test=="idle/open-close only"){scissors.DiagnosticPose(1);}
            if(test=="visual contact without rope cut")front.GetComponent<ToySpikeVisual>().PlayContact(front.bounds.center,Vector2.right*8);
            if(test=="detached blade overlap"){
                scissors.DiagnosticPose(0);var points=scissors.WorldBlade(0);Vector2 centre=Vector2.zero;foreach(var p in points)centre+=(Vector2)p;centre/=points.Length;Place(centre);
            }
            for(int f=0;f<50;f++)yield return new WaitForEndOfFrame();
            check.cutsAfter=cutAudio.cues.Count;check.playsAfter=cutAudio.playCount;
            check.pass=check.cutsBefore==check.cutsAfter&&check.playsBefore==check.playsAfter;
            audioResult.negativeChecks.Add(check);scissors.DiagnosticRelease();
        }
    }
}

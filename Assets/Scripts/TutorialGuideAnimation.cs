using UnityEngine;

namespace HimoHito
{
    /// <summary>Read-only, normalized diagram poses. Never drives the real player or rope.</summary>
    internal static class TutorialGuideAnimation
    {
        internal struct Frame
        {
            public Vector2 Feet;
            public Vector2 ConnectionTarget;
            public float Opacity, Connection, Bridge, Merge;
            public float LeftBridge, RightBridge;
            public bool Danger;
            public string Caption;
        }

        internal static float Duration(int section) => section == 2 ? 13f : section == 4 ? 21f : 10f;
        internal static float Ease(float time, float start, float end) =>
            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, time));
        internal static Vector2 Curve(Vector2 a, Vector2 b, Vector2 c, float t) =>
            (1f-t)*(1f-t)*a + 2f*(1f-t)*t*b + t*t*c;
        internal static Vector2 Swing(float t, bool dangerous = false) =>
            Curve(new Vector2(.29f,.64f), new Vector2(.5f,dangerous ? 1.08f : .88f),
                new Vector2(.71f,.64f),t);
        internal static Vector2 BridgePoint(float t) =>
            Curve(new Vector2(.30f,.66f),new Vector2(.51f,.72f),new Vector2(.72f,.66f),t);
        internal static Vector2 MergedPoint(float t, float merge)
        {
            Vector2 a=new Vector2(.24f,.67f), b=new Vector2(.5f,.35f), c=new Vector2(.76f,.67f);
            Vector2 pair=t<=.5f ? Curve(a,new Vector2(.37f,.72f),b,t*2f) :
                Curve(b,new Vector2(.63f,.72f),c,(t-.5f)*2f);
            return Vector2.Lerp(pair,Curve(a,new Vector2(.5f,.99f),c,t),merge);
        }

        internal static Frame Sample(int section, float elapsed)
        {
            float duration=Duration(section), t=Mathf.Repeat(Mathf.Max(0f,elapsed),duration);
            Frame f=new Frame { Opacity=Ease(t,0f,.35f)*(1f-Ease(t,duration-.35f,duration)) };
            switch(section)
            {
                case 1:
                    f.Feet=Swing(Ease(t,2.3f,6f));
                    f.Feet.x+=.12f*Ease(t,6.6f,7.8f);
                    f.Connection=Ease(t,1.2f,1.7f)*(1f-Ease(t,6f,6.5f));
                    f.Caption=t<1.2f ? "矢印キーで青いHookを狙う" : t<2.3f ? "E　ヒモを掛ける" :
                        t<6f ? "D　歩き出して、振り子で渡る" : "対岸へ着地！";
                    break;
                case 2:
                    f.Danger=t<5f;
                    f.Feet=Swing(f.Danger ? .5f*Ease(t,1.5f,3.6f) : Ease(t,7.2f,10.8f),f.Danger);
                    f.Connection=f.Danger ? Ease(t,.9f,1.4f) : Ease(t,6.6f,7.1f)*(1f-Ease(t,10.8f,11.3f));
                    f.Opacity*=t<5f ? 1f-Ease(t,4.6f,5f) : Ease(t,5f,5.4f);
                    f.Caption=t<3.6f ? "長さ8　底が下がってしまう" : t<5f ? "×　トゲに当たる" :
                        t<6.6f ? "Eで外して選び直す　S：8 → 6" : t<7.2f ? "E　もう一度掛ける" :
                        t<10.8f ? "長さ6～7　トゲの上を渡れる" : "長さを選んで、対岸へ！";
                    break;
                case 3:
                    f.Connection=Ease(t,1.1f,1.6f)*(1f-Ease(t,2.5f,3.3f));
                    f.Bridge=Ease(t,2.5f,3.5f);
                    float walk=Ease(t,4.4f,7.8f);
                    float x=Mathf.Lerp(.23f,.84f,walk);
                    f.Feet=x<.30f || x>.72f ? new Vector2(x,.66f) : BridgePoint((x-.30f)/.42f);
                    f.Caption=t<1.1f ? "推奨：長さ7で対岸の緑Hookを狙う" : t<2.5f ? "E　対岸へ接続" :
                        t<4.4f ? "Q　編んで足場にする（消費7）" : "D　できたヒモ橋を歩いて渡る";
                    break;
                case 4:
                    f.LeftBridge=Ease(t,1.8f,2.8f);
                    f.RightBridge=Ease(t,6.5f,7.5f);
                    if(t<9f)
                    {
                        float approach=Mathf.Lerp(.18f,.5f,Ease(t,3.3f,5.3f));
                        f.Feet=approach<.24f ? new Vector2(approach,.67f) :
                            MergedPoint((approach-.24f)/.52f,0f);
                        bool first=t<5.5f;
                        f.ConnectionTarget=first ? new Vector2(.5f,.35f) : new Vector2(.76f,.67f);
                        f.Connection=first ? Ease(t,.8f,1.3f)*(1f-Ease(t,1.8f,2.8f)) :
                            Ease(t,5.5f,6f)*(1f-Ease(t,6.5f,7.5f));
                        f.Caption=t<.8f ? "長さ6で中央の青いHookを狙う" : t<1.8f ? "E　左岸から中央へ接続" :
                            t<3.3f ? "Q　1本目を編む（消費6）" : t<5.5f ? "D　できた橋を歩いて中央へ" :
                            t<6.5f ? "長さ6で右岸へ E" : t<8.5f ? "Q　2本目を編む（消費6）" :
                            "2本の橋ができた！";
                        break;
                    }
                    // Continue directly from the completed pair, with no position reset.
                    t-=9f;
                    f.Merge=Ease(t,4.8f,5.7f);
                    float along=.5f+.24f*Ease(t,1.4f,2.6f)-.24f*Ease(t,3.6f,4.8f);
                    along+=.74f*Ease(t,6.6f,10f);
                    f.Feet=along<=1f ? MergedPoint(along,f.Merge) : new Vector2(.76f+(along-1f)*.52f,.67f);
                    f.Caption=t<2.6f ? "2本の橋では通り道が高い" : t<3.6f ? "梁があって進めない" :
                        t<4.8f ? "中央の青いHookへ戻って狙う" : t<6.6f ? "F　中央を外すと1本に下がる" :
                        "D　低くなった橋で梁の下へ";
                    break;
            }
            return f;
        }
    }
}

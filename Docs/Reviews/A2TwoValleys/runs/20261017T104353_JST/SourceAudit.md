# 実キー専用モードが未準備である根拠

以下は隔離Proofの既存ソースの読み取り抜粋。実行した入力ではなく、今回の実装変更でもない。位置は現行ソース基準。

## Assets/PlayerPresentation/PlayerMotionPresentation.cs

`PlayerPresentationProof`、12～22行付近：

```csharp
public static bool Candidate => A2Fixture.Arg("-presentation", "baseline") != "baseline";
public static bool SecondCandidate => A2Fixture.Arg("-presentation", "baseline") == "candidate2";
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void Bootstrap()
{
    Time.captureFramerate=30;
    SceneManager.sceneLoaded+=(scene,mode)=>
    {
        if(scene.name=="MainStage"&&!UnityEngine.Object.FindFirstObjectByType<PresentationCapture>())
            new GameObject("ISOLATED Player Presentation Comparison").AddComponent<PresentationCapture>();
    };
}
```

候補切替は描画候補を選ぶもので、自動テストの有無を選んでいない。

## Assets/PlayerPresentation/PresentationCapture.cs

`Start()`、49～63行付近：

```csharp
if(PlayerPresentationProof.Candidate) presentation=f.Body.gameObject.AddComponent<PlayerMotionPresentation>();
yield return Delay(.5f);
input=f.gameObject.AddComponent<A2InputDriver>();
f.Write("PresentationConfig.json",new A2Fixture.Config(f));
startTime=Time.time;
bool smoke=A2Fixture.Arg("-presentationQA","flow")=="smoke";
if(A2Fixture.Arg("-presentationQA")=="regression")
    // existing automatic merge regression
if(A2Fixture.Arg("-presentationQA")=="cycle")
    // existing automatic free cycle
```

smoke以外の通常flowは、79行以降で `input.Move=1`、既存APIのTryAttach、DetachAndRefund、Retry、ポーズ、橋生成などを自動実行して終了する。自動ドライバー登録の前後に、実キー専用の早期returnはない。

このため、画面操作が回復しても、自動flowが動くBuildのままOSキー反応を混在させて「通常操作成功」と数えることはできない。自動ドライバーを内部から消したり変数を書き換えたりする回避も今回は行わない。

ソースとBuildのhashはQA内に記録。主Build r1と補助r3は診断ケースが異なるが、自動Captureの登録と自動ドライバー作成は共通の構成。今回バイナリのパッチ・再Build・コード編集はない。

# UI / Audio

## UI静的出力

- `StaticMenu/`：Tutorial/Main/ComingSoon × 1920×1080,1600×900,1280×720,1920×1200,2560×1080 = 15枚。
- `StaticScreens/`：Tutorial HUD/help/pause/failure/clear/sign1〜4 × 標準3解像度 = 27枚。
- `StaticGuideResource/`：sign3のanimation時刻3.8秒、標準3解像度。消費7と固定残量例の食い違い確認用。

実Drawを隔離コピーで呼んだ静的fixture。Screen参照だけを診断viewportへ置換、presentation stateを設定し、1280×720へ正規化描画してから各解像度へリサイズした。レイアウト計算確認には使えるが、実Windowのfont rasterやOS入力、多重UIの証明ではない。

目視サンプル：menuの1280Tutorial/1600Main/1920ComingSoon/1920×1200Main/2560×1080Tutorial、Tutorialの1280 HUD/help/pause/failure/clear/sign2等。これらで明らかな画面外へのはみ出しは見られない。pause内はscroll areaであり全設定が一画面にないこと自体はバグではない。全42枚の厳密pixel照合、Main HUD/clearの全aspect、状態遷移中の重なりは未完了。

看板のplayerがtime0で見えないのは `TutorialGuideAnimation.Sample` のOpacity=0のfade-inによる。今回のtime0静止画から「playerがないバグ」としない。

**確認問題HH-FR-001（S2）**：sign3のcaptionは推奨7／消費7だが、残量例は99→93。実消費は7なので99→92となる。Sourceの固定labelとstatic Drawで確認。修正なし。

## Audio

Runtime snapshotでBGM singleton1、通常AudioSources5。Tutorial→Main、Main clear→menu→MainでBGMのobject二重化なし。これは波形の重複／正しいclipの聴感までの証明ではない。

PrototypeAudioFeedbackにE shot/attach/miss/detach、length、Q、F、footstep/jump/landing、fail、chest/clear、UI openのclipロードと再生呼出しがある。診断中の物理操作は実イベントを通すが、各clipの音声出力は録音／聴取していない。出力デバイス／ミュート／volume差は未判定。close無音は指定仕様で、Issueに含めない。

ローカル `Assets/Resources/Audio/YasashiiOdori.mp3` は存在しbuildで使用されるがGit未追跡・ignore対象（metaはtracked）。READMEに別取得・利用条件が書かれている。即ゲームbugとはしないが、COMMITTED HEADのみの音声再現性は保証できない。

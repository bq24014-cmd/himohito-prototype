# UI状態遷移・入力受付の追補（未公開の親レビュー用）

**A=0。画面・通常入力・音の再確認は行っていない。** 対象は10/6 FriendPlay固定Buildに対応するソース。B＝コードの条件・呼出しを確認、C＝実Windowで再現/確認が必要。以下の「経路がある」は通常入力がゲームへ届く等の条件を置いた読解であり、実行した証拠ではない。

前回承認済み42ファイルは別commitへ中間保存済み。この追補と証拠はそこに混ぜていない。採用判断・実装・新UI作成は行わない。

## r1訂正差分｜依存関数まで確認した範囲

**実Buildで動作確定とはしていない。A=0を維持。** 前版の「共通条件がない」は、今回本文を追った`Overlay.ApplyPauseState → 各Update → Q/F/Rの条件判定と確定処理`に範囲を限定する。OS入力到達、実効実行順、参照解決・例外の有無、実際の生成/復帰は未観察。

### 更新した結論

| 疑問 | 追加で確かめた根拠 | 結論と留保 |
|---|---|---|
| timeScale=0ならUpdateも止まるか | Unity6.3公式Manual/Update/timeScale APIを確認。時間刻みとUpdate受付は同じではない | enabledなUpdateで入力判定する経路を、時間停止だけで反証できない。実ゲームのフレームは未取得 |
| Q接続時にbodyが停止扱いになるか | BeginPlatformBuildHold424–453はFreezePositionX/Y、速度ゼロ。body.simulated/Builder.enabledを書き換えない | `body.simulated=false`のBuilder guardへ入るという反証は、この待機処理からは成立しない。新規E後はjoint.enabled=true。Scene値だけで常時有効とはしない |
| Qの呼先で拒否するか | CanBuild351–400→Commit404–420→TrySpend35–43、対応Anchor30–54まで照合 | Q解放・接続・参照・対端点・長さ/資源等は必要。成功枝の消費はCreatePlatformや演出より先。これらにOverlay/timeScaleによる拒否は確認できない |
| Fの照準がポーズで使えなくなるか | TryResolveCurrentAimHook461–507は直接Raycast。GetHookSearchDistance/CanUsePlatformHookTargetingGrace626–648、HookPoint45–50まで照合 | 強調表示を止めるUpdateAimHookFeedback792とは別経路。強調が消えることはFの判定停止の証明にならない。接地判定・空中連続接続のresolverもここでは呼ばない。対象Collider/距離/方向/2本は依然必要 |
| Sceneで最初から無効なのか | 両SceneのPlayerはactive、Rigidbodyはsimulated=1、Builder/Rope/Mover/RunまたはMainRespawnはenabled=1。Ropeは各1個 | Tutorialのタイトル/紹介/看板/失敗は後で無効化するため、保存値を現在値へ読み替えない。正常プレイへ復帰後にポーズを開く条件に限定 |
| OverlayがSceneに無いのでは | 保存SceneにOverlayは0個。TutorialのRun.Awake93–102、Mainの有効なHud.Awake28–32が存在しなければ追加 | Scene不存在だけでポーズが存在しないとは言えない。追加が実Buildで成功したかは画面未確認。最小試験は「ポーズ画面が開いた」を入口条件とする |
| 実際はR→Q/Fの順か | 属性はMainRespawn=-200/Builder=-100。BuilderのmetaにはexecutionOrder:0があり、他も未記載/0 | **属性の数値だけで対象Buildの実効順序を確定しない。** Editor値優先と同順序未保証の公式仕様がある。単一component内のif/returnの順は確認済みだが、全Update順は保留 |
| UIが前面なら入力も遮るか | OverlayはOnGUI、Q/F/RはUpdate。GUI.depth/GUI.enabledと表示のreturnは描画/GUI操作側。共有のゲーム入力消費フラグへつながる呼出しはこの経路にない | 可視/不可視、ボタンのGUIイベント成立、Legacy Inputの押下は別。UIクリックとの実競合は未実測 |

保存ProjectSettingsはactiveInputHandler=0、runInBackground=0。Physics2DはqueriesHitTriggers=1、autoSyncTransforms=0。これらも保存設定であり、前景Windowへの入力到達や動的Collider照準の成功を保証しない。別Window/Alt+Tab後の試験は今回の最小ケースから外す。

### 条件を置いた呼び出し順（実行ログではない）

- **Q**：既にポーズが開いた次以降のフレーム→有効Builder.Update→Preview/!simulated guard→Q解放→CanBuild→対応2端点/長さ/最低残量→CommitAttachedRopeAsPlatform→TrySpend→joint解除・待機制約解除→CreatePlatform→残量表示通知/位置配置/演出。Time.deltaTimeの積算は消費の成立条件にない。演出が進まない可能性と、論理状態の変更を分ける。
- **F**：同じ入口→Builder.Update→F解放→未接続→TryResolveCurrentAimHookのRaycast/距離→Touchesの端点距離判定で2本→合計長/外端点→旧橋を非active化して除去、新橋生成、中央を非active化。ポーズ前から2本と中央を準備できたことは実試遊で確認が必要。F用照準に接地条件を追加して説明しない。
- **R**：MainRespawn.Update→開始/紹介/ゴール/失敗演出の除外→R→RestartFromCheckpoint。TutorialはGuide visible等も除外。復帰は保存位置へ戻し速度ゼロ、snapshotの残量/橋/除去フックを復元。画面のisPaused/showHelpを解除する処理ではない。

CreatePlatform/PlacePlayerOnPlatformは処理本文も確認した。後段の描画・効果で参照異常等が起きないことまで保証したわけではない。今ある実画面/ログで成功したという証拠には置き換えない。

### 反証・適用しない場面

タイトルはRope/Mover無効かつWaiting分岐。紹介はPreview guard、看板はBuilder無効、失敗/クリアはbody.simulated=false。これらは通常プレイから開くポーズと状態が違う。未解放Q/F、資源不足、無効な橋ペア、接続中F、射程外/無効Collider、2本でない中央も成功枝を拒否する。S3スイッチのFはtimeScale<=0を明示拒否するので、橋統合Fへ一般化しない。

Fでhaloが消えた/マウスでボタンが反応しなかった、という見た目だけも判定停止の反証にはしない。逆にコードの成功枝があるだけで、対象Buildのフレーム・Raycast・例外まで確認したことにしない。

### 実Buildで確かめる最小ケース（今回は全て未実行）

| ケース | 入口と操作（通常入力だけ） | 比較/記録 | 失敗判定を急がない条件 |
|---|---|---|---|
| まずR | Main紹介終了後、開始床で復帰地点と違う位置へ少し移動→Esc→画面が安定してからRを1回→Escで戻る | Rなしで同じポーズを開閉した場合と位置を比べる。復帰地点へ戻ったか動画で記録 | 同じ位置なら見えない。開始/紹介/クリア中、Window非focusは対象外 |
| 次にQ | 通常TutorialでT3まで到達→橋用フックへ長さ7でE→接続中とQ可の入口を画面で確認→Esc→安定後Q1回→Escで戻る | 同条件でQを押さない場合と橋・残量・接続・位置を比較。記録前にRを押して巻き戻さない | E成功/対端点/長さ/残量が揃わない場合はポーズ遮断の証拠にしない。T3到達自体は未実証 |
| Fは準備できた時だけ | 通常T4で橋2本を作り、ヒモを外し中央を射程内で狙う→Esc→安定後F1回→Esc | 通常状態で同じ照準からFが有効な比較条件を別試行で確かめ、中央/橋形/残量を見る | haloをF可の必要条件にしない。射程/方向/2本が未確認なら保留 |
| 左クリックは別試験 | 橋用Eの待機状態→Tabでヘルプ→「閉じる」をマウスで操作 | Tabで閉じる対照と、戻った後の接続状態を比べる。MouseDown到達とGUI.Button成立を別に記録 | 押下とボタン成立を同じフレームと決めない。クリックの成否・focus・位置は実Windowが必要 |

Esc+Q/F/Rなどの同時押しは使わない。ポーズが表示された後の別フレームで1キーだけ押し、UIを開く/閉じる順の問題を最初の試験へ混ぜない。実行順が静的に確定しない部分を、この単独入力比較で切り分ける。

U14の文案は親訂正を反映し、**「Rで、直前の復帰地点から。橋とヒモ残量も、その地点に保存された状態へ戻ります。」** とする。全回復の意味にしない。U12のTab入口保持は維持。

### エンジン仕様の根拠と日付

2026-10-17 JST確認。対象Buildは6000.3.21f1、以下はUnity6.3の公式文書であり、パッチ固有の実行試験ではない。

- [time-scale](https://docs.unity3d.com/6000.3/Documentation/Manual/time-scale.html)：timeScaleは実行そのものの停止ではなく時間刻みを変える。ポーズ中の通常GUI操作も可能。
- [Update](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.Update.html)：enabledなMonoBehaviourに毎フレームUpdate。
- [timeScale API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Time-timeScale.html)：timeScale=0ではFixedUpdate/WaitForSecondsは停止。変更は後続フレームへ作用。
- [DefaultExecutionOrder](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/DefaultExecutionOrder.html)：Editor指定が属性と異なる場合はEditor値を使う。同じ順序値の相互順序は保証されない。
- [GetKeyDown](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Input.GetKeyDown.html)：押し始めのフレームの状態。Updateで読む。
- [GetMouseButtonDown](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Input.GetMouseButtonDown.html)：MouseDownフレームの状態。GUI.Buttonの成立そのものとは区別。
- [OnGUI](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnGUI.html)：描画/GUIイベント用。1フレームにイベントごと複数回呼ばれ得る。

---

## 前版の結論（範囲は上のr1で限定）

同じ閉じるキーの二重利用には、既に防止している経路がある。タイトルのヘルプ閉じEsc→終了、Tab同時押し→開始、紹介スキップSpace→ジャンプを一括で「漏れる」と扱うのは誤り。一方、**ポーズ/ヘルプの表示中に別のゲーム操作を受けない共通条件は、本文を確認したUpdateとQ/F/R確定処理の経路にはない（詳細はr1訂正差分）**。こちらは文言の冗長さとは別に、条件付きの機能確認候補とする。実Build不具合として確定・再現済みとは書かない。

Qの確認候補は「橋用フックに接続→Escでポーズ→何も閉じずQ」。r1では準備負荷の低いR比較を先にし、Q/Fは前提を通常操作で確かめてから行う。橋作成と消費が起きるか、ポーズを保つかを同じ画面で記録する。未接続/未解放/資源不足ではQが失敗するので、闇雲に押す比較はしない。**今回実行していない。** 確認できた場合は文章短縮より先に入力受付の問題として扱う。朝の既存Q文案比較の採否は親/Humanがこの候補を見て再判断する。

根拠：[実ソース抜粋](evidence/ui-state-source-excerpts.md)、[固定入力とのハッシュ照合](evidence/ui-state-source-hashes.json)、[条件付き追跡JSON](evidence/ui-state-conditional-traces.json)。30入力を照合し全て一致（追加を含む）。前回までと重複するため、件数を単純加算しない。

## 画面ごとの受付と停止

| 状態 | 受付経路（B） | 停止・排他（B） | 実画面で残る確認（C） |
|---|---|---|---|
| タイトル兼ステージ選択 | ←/→・A/Dで選択、Enterで開始、Tabでヘルプ、Escで終了。マウスも各ボタンから同じメソッド | body.simulated=false、Mover/RopeController無効。CanUseTitleMenuが開始演出・入場フレーム・Tabフレーム・ヘルプ表示/消費フレームを拒否 | 起動時選択の見やすさ、OSフォーカス、ボタンfocusとカーソル |
| 開始演出/紹介 | カーテン中は主要UIがIsActiveでreturn。紹介中はSpace/Enterでスキップ可能 | Beginはactiveを即設定して二重開始を拒否。紹介はbody/Mover/Ropeを停止し、復帰はLateUpdateへ遅らせる | 実時間、スキップの分かりやすさ、カーソルの見え方 |
| Tutorial/Main通常プレイ | Escポーズ、Tabヘルプ。Tutorialは近い看板へZ。Rは区間復帰 | 通常時の操作解放は区間依存。FはTutorial4/Main9以後の橋統合、S3のFスイッチは別 | キーがWindowへ届くか、案内を必要時に見付けるか |
| ヘルプ（通常プレイから） | TabまたはEscで閉じる。閉じるボタンもある | showHelp=true、timeScale=0。閉じ処理でフレームを記録。プレイヤー各componentを無効にする処理ではない | 背景操作が実際に起きるか。文章/図の重なり、実クリック |
| ポーズ | Esc再開、Tで選択へ戻る確認。Tabでヘルプ。音量のマウス操作 | isPaused=true、timeScale=0。ヘルプを開いてもisPausedを保持 | R/Q/F/ヒモクリックの条件付き経路、音量の実聴 |
| ポーズからヘルプ | Tab/Escでヘルプを閉じるとポーズに戻る | showHelpをfalseにするだけでisPaused=trueが残り、timeScale=0を保持 | 1回で本編へ戻ると思う人がいないか。閉じる文の文脈 |
| 看板 | Zで開閉。Esc/Tabで閉じる処理はない | timeScale=0、Mover/Rope/Builderを無効化。Guide表示中はRunのRとOverlayを拒否 | 閉じ方の発見、同時押しの優先、接続中に開いた際の受け止め |
| 失敗・落下演出中 | Tutorial RunはIsFallUnravellingでUpdateを早期return。Main失敗も演出中R/Escを拒否 | body.simulated=false、落下ではMover/Rope無効。Q/Fもbody停止のguard | 待ち時間の長短。0.8等の演出定数を入力停止時間に読み替えない |
| 失敗表示後 | R復帰、Esc選択へ。Tutorial/Mainの同時押し優先は異なる | Overlayは結果画面でreturn、GuideはPlaying以外を受けない。Rで保存位置へ戻し速度ゼロ | 復帰テンポ、Esc案内の有無、通常操作での状態復元 |
| クリア待ち→表示 | 待ち中の通常遷移キーは受けない。表示後TutorialはEnter本編/R Tutorial再開始/Esc選択、MainはR本編再開始/Esc選択 | body停止。WaitForSecondsRealtimeで表示へ。clearNavigationStartedが再ナビゲーションを拒否 | ボタンfocus/二重クリックの実際、入力可になる瞬間、音 |
| 選択へ戻る | ポーズ確認はEnter、結果はEsc/クリックなどからTutorial Sceneへロード | timeScale=1へ戻す。新タイトルのtitleEnteredFrameが入場フレーム操作を拒否 | Scene切替をまたぐ同じ物理キーの実測、戻った時の選択表示 |

根拠の主な行：Overlay45–137/409–425、Guide124–172/496–553、Run137–211/214–284/333–354/459–488、MainRespawn108–151/222–240、Goal20–47/64–88、Preview128–165。

## 閉じるキー自体の二重利用を分ける

| 条件 | 条件順から確定すること（B） | 限界/反例 |
|---|---|---|
| タイトルのヘルプでEsc | CloseHelpがlastHelpInputFrameを記録。タイトルはoverlay表示中、または当該消費フレームを拒否 | Overlay/Runのどちらが先でもこの経路は終了を拒否。『閉じEscで即終了』を不具合候補にしない |
| タイトルでTabとEsc/Enter | タイトルはTabフレームを拒否。OverlayもTab分岐でreturn | Tabで開閉した同フレームの開始/終了を防ぐ。通常の次フレーム入力まで永久に無効にする処理ではない |
| プレイ中のヘルプでEsc | showHelp+EscでCloseHelpしてreturn。後段のEscポーズ切替へ進まない | 閉じるだけのEscを『閉じて即ポーズ』としない |
| ポーズ中にヘルプを開きEsc | 閉じるとポーズへ戻る。さらにEscで再開 | これは二重実行ではなく状態の保持 |
| 看板でZのみ | visible分岐で閉じ、直後return。他にZを読む本編receiverは固定Scripts検索で見付からない | 同じZで再び開く/ゲーム技を使うという根拠はない |
| 看板でEsc/Tabのみ | GuideはZだけを扱う。OverlayはGuide visibleならreturn | Esc/Tabで閉じられるという修正文は現状と合わない |
| 紹介スキップのSpace | 紹介は入力Update後のLateUpdateでcomponentを戻す | 同じSpaceでジャンプも起こると推定しない。通常Windowでの確認は未実施 |

### 看板を閉じるZと、別キーを同時に押した場合

Z+Escでは、Guideが先に閉じればOverlayが同フレームにEscを見てポーズを開く経路があり、Overlayが先ならGuide表示中なのでreturnした後に看板だけ閉じる。Z+Tabも同様にヘルプへ進む/進まない分岐になる。GuideにはHelpInputConsumedThisFrameに相当する閉じフレームの記録がない。

これは**同じZが二重実行する話ではなく、別キーとの優先が処理順に依存する候補**。両者の実行順・再有効化したcomponentがそのフレームのUpdateを受けるかを今回実測していないため、Z+F等で橋が必ず変わると断定しない。

逆に、看板付近でZ+Tabを押して開く場合は、先に開いた方の表示状態を後の処理がチェックしてreturnするため、同じ条件を根拠に『必ず二枚同時に開く』とも言えない。タイトル/看板/ヘルプを混ぜず、単独キー→同時キーの順に確認する。

## 画面を開いている間のゲーム入力：条件付きの確認候補

### C01｜Rの区間復帰はポーズ/ヘルプを見ていない（B経路/C再現）

MainRespawn108–136は開始演出・紹介・ゴール中を除外した後、RでRestartFromCheckpointを呼ぶ。Overlay表示/Time.timeScaleのチェックはない。Tutorial RunもGuide表示中は拒否するが、Overlay表示中のRを拒否しない。復帰処理は保存位置へ戻し、速度はゼロにし、残量/橋/除去フックを戻す。OverlayのisPaused/showHelpを閉じる処理ではない。

成立前提は通常プレイから開いたポーズ/ヘルプ、WindowへのR到達、開始/紹介/クリア中ではないこと。保存位置と現在位置が同じなら見た目の移動は現れず、何も起きなかったように見える反例がある。タイトルではWaiting分岐でreturnし、看板ではGuide guardがある。『全ての画面でRが抜ける』とはしない。

### C02｜Q/FはtimeScale=0だけでは遮断されない（B経路/C再現）

Overlay.ApplyPauseStateはtimeScaleを書き換えるだけで、body.simulatedやMover/Rope/Builderを無効化しない。Builder.Update83–98はPreviewとbody.simulated=falseを除外するが、timeScale/Overlayを見ない。QのCanBuild351–400と確定処理、FのTryRemove143–185にもそのguardはない。

Qは解放済み・接続済みの橋用フック・距離・残量条件等が揃う必要があり、Fは解放済み・未接続・中央への照準解決・接する橋がちょうど2本という条件が必要。これらを置いた時に画面表示中の変更へ至るコード経路がある、という評価に限定する。

成立しない反例：失敗/クリアはbody.simulated=falseでBuilderがreturnする。看板はBuilder自体を無効化する。S3のFスイッチはTime.timeScale<=0を明示拒否し、その時点で橋統合Fも未解放。従って『Fはどこでもポーズ中に効く』という主張は誤り。

### C03｜UIをクリックする左ボタンとヒモ操作が分離されていない経路（B/C）

RopeController.Update141–170は左MouseDownを読み、接続中ならDetachAndRefundを呼ぶ。UIのhit test・Overlay表示・timeScaleのチェックがこの経路にはない。接続中にポーズを開いたままUIへマウスを押す条件では、ヒモ解除へ至る候補になる。閉じるボタンが実行される前の押下も含むため、単に『閉じた後の入力漏れ』に限定しない。

実Windowでマウスがどこへ届き、IMGUI側のイベント/フォーカスとどの順で処理されるかは未取得。GUIのイベント消費がゲーム用GetMouseButtonDownも遮断するとの根拠も、このソースにはない。**実クリックで解除したと記録しない。** タイトルではRopeController無効、看板でも無効なので同じ条件を適用しない。

### 看板は停止だけではなく接続にも触れる（B/C）

看板を開く条件は現在区間の看板から距離3.25以内等で、接続中であること自体を拒否しない。OpenGuideはRopeController.enabled=falseにし、RopeController.OnDisable192–195がDetachAndRefundを呼ぶ。CloseGuideはenabledを戻すが、接続状態を保存して再接続する処理ではない。

『近い看板を接続中に開けた場合、閉じた後も同じ支点の振り子を保持する』とは説明できない。これが意図に反するか・通常の到達位置で気付くかはC。今回新たなバグとして確定せず、看板が完全に状態を保存するとの表現だけを避ける。

## R/F/Escの重なりを一律にしない

| 入力と状態 | 同じメソッド内で確認した優先（B） | 言えないこと |
|---|---|---|
| Tutorial失敗後のR+Esc | Failed+EscがRより先にScene選択へ戻る | Mainも同じ優先とはしない |
| Main失敗後のR+Esc | RのifがEscのelse-ifより先なのでR復帰 | Escが全ての画面で最優先とはしない |
| Tutorial/Mainクリア後のR+Esc | Escの遷移が先。Mainはelse-if、Tutorialはreturn | 両方同時に実行したとはしない |
| ポーズの戻る確認でEsc | 確認だけキャンセルしてreturn。ポーズは残る | Esc1回で通常プレイへ戻るとはしない |
| 確認中のTab+Enter | Tab分岐が先。ヘルプを開く際に確認をfalseにしreturn | 同じEnterで戻る承認を実行するとはしない |
| 通常プレイのR+F | 複数componentで読むが、Fには対象・2本・解放等の条件がある | 順序/到達状態を示さず二重実行バグとしない。今回成立例なし |

## 時間停止とS6の1.5秒窓

S6空中再接続の期限はTime.unscaledTimeを使う。ポーズ/ヘルプがtimeScale=0にすることを根拠に、この期限も止まるとは仮定できない。『考えるためにポーズすれば再接続の残り時間を保持できる』という案内は出せない。窓が失効した後の空中Eの成功/失敗や着地余裕は、通常入力で未確認。

看板アニメや開始演出/紹介、クリア待ちもunscaled/realtimeを使う経路がある。逆にRespawnWeaveVisualはTime.deltaTimeで見た目を進めるが、ゲーム入力の停止を所有しない。演出Durationをそのまま『R後に操作できない時間』へ転記しない。

## 重複表示・フォーカスの境界

OverlayはGuide visibleまたは結果画面ならOnGUIをreturnする。GuideもOverlay visibleなら看板を読む案内を描かない。ポーズ中のshowHelpはDrawHelp後returnし、ポーズ画面を同じ経路では描かない。これらは多重表示を抑える根拠で、二枚表示の発生を捏造しない。

通常HUDはOverlay/Guide表示を直接チェックせず描画する経路が残る。Guideはdepth=-1100、Overlayは-1000を指定し、ヘルプは全画面背景を描く。背後のHUD描画があることと、文字が実際に透けて重なって読みにくいことは別。U09の二重Tab案内は通常プレイの文言経路として扱い、モーダル画面での可視重複はCに残す。

タイトルはCanUseTitleMenuでGUI.enabledを設定し、finallyで戻す。開始/終了/選択メソッド側にも状態guardがある。StartTransition.Beginのactive、クリア遷移のclearNavigationStartedは二重実行を抑える根拠。ただしマウスとキーの実際の同時入力を試したわけではない。

WoodButtonはGUI.Buttonを使うが、独自の描画で示すのはマウスhover色が中心。固定Scriptsの検索で明示的なGUI.FocusControl等の呼出しは見付からない。これは『キーボードフォーカスが存在しない』という証明ではない。IMGUI標準のfocus、Tabの解釈、hover/focusの見分け、Alt+Tab後の復帰、OSカーソルは実Window確認が必要。Proof表示やDevelopment Buildは評価へ混ぜない。

## 既存UI文案16件の再点検

| ID | 今回の扱い | 修正/保持する理由と文案 |
|---|---|---|
| U01 Q生成の単位 | 維持 | 対のフック間・使う量・通常回収不可とR復元の区別はそのまま。今回のmodal入力候補と文言の正しさは別 |
| U02 Q不可理由 | 維持 | 最低残量の動的値、長さ/対象の条件は必要。『ポーズ中だから不可』を現仕様の原文として追加しない |
| U03 操作条件 | 補足案を訂正 | R主行は「直前の復帰地点からやり直す」、補足は「橋・残量も保存された状態へ」。看板の閉じ方はZだけ。Tab/Escを万能な閉じキーとしない |
| U04 ボタン風ラベル | 維持＋確認項目 | ラベルのみ/GUI.Buttonの差は維持。クリック対応を採る前に、ヒモ用左クリックと重複しないか通常操作で確認。『クリックを足すだけ』で完了扱いしない |
| U05 T1 | 維持 | 目標・接続・踏み出しを短くする案は状態遷移と矛盾しない |
| U06 T2 | 維持 | 因果を先、正解長は救済。看板を閉じた時に接続を保持すると約束しない |
| U07 T3 | 維持 | 身体から道へ、99→92の説明を保持。Qがmodal中に使える可能性を正規操作として案内しない |
| U08 T4 | 維持 | 既存因果字幕を前へ移す比較案。Fの未接続/中央/2本条件を保持 |
| U09 Tab重複 | 観察範囲を限定 | 通常プレイの二箇所へ限定。背後HUDの描画だけでヘルプ/看板上の可視重複と断定しない |
| U10 制作注記 | 維持 | 作者/ライセンス参照を保持し、Unity版の主画面優先度を下げる案のまま |
| U11 未提供ステージ | 維持 | disabled開始処理と選択できる島を分ける。フォーカスの使いやすさは未実測 |
| U12 音量文 | **文案を修正** | 音量文は「0％で無音。音量は自動で保存されます。」。別の近い位置に「Tab　操作説明」を残す。ポーズ中はcornerHintが描かれず、元案のまま入口まで削ると発見性を落とし得る |
| U13 選択へ戻る確認 | 維持 | 「このステージは最初からやり直しになります。音量設定は残ります。」を条件付き案のまま。キャンセルは確認を閉じるだけでポーズへ戻る |
| U14 失敗/復帰 | **文案を修正** | 「Rで、直前の復帰地点から。橋とヒモ残量も、その地点に保存された状態へ戻ります。」。「区間の始め」を厳密な保存境界へ合わせる。原因の捏造はしない |
| U15 Clear | 維持 | 「習得しました」を避ける。Tutorialは本編へ、Mainは本編再開始というRの違いをヘルプ一般行で混同しない |
| U16 読込異常 | 維持 | プレイヤー向け文と開発ログを分離する案のまま。異常を実際に起こしていない |

以上は修正文の差分提案であり、保存済み42ファイル内の02_UI.mdを書き換えていない。新しいUI/操作を制作せず、既存入力を残す前提で、必要な確認条件を具体化した。

## 未確認と次の最小確認

1. Qが有効な接続状態でポーズ→Q。成立した場合のみ、同じ条件でTabヘルプ→Qを比較する。消費・橋・位置が変わるかを記録。
2. 接続中のポーズでUIを左クリックし、ヒモの接続が維持されるか確認。UI操作とゲーム操作の両方を画面に残す。
3. 看板はZだけ、ヘルプはTab/Esc、ポーズ内ヘルプはポーズへ戻る、という単独操作を先に確認。その後Z+Esc/Tab等の同時入力を必要時だけ比較。
4. 入力問題を確認してから、U12/U14の短文と既存文を比較。文を変えるだけで受付の問題を解決したとはしない。

今回はこれらを実行していない。通常キーで再現できる環境の引き渡しと親レビューを待ち、内部APIや自動攻略で穴埋めしない。

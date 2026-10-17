# 隔離コピーの変更範囲

ProductionにはSourceもAssetsも配置していない。[正確な差分](CodeDiff.json)。変更対象は次の5ファイルと、新規表示／撮影2クラス＋meta。

| ファイル | 内容 |
|---|---|
| PlayerPresentation/PlayerMotionPresentation.cs | 既定を新候補へ。手動表示bootstrapと明示的撮影起動を分離。新候補だけ接続接地時の足裏補正と腕／胴の表示へ接続 |
| Scripts/HookYarnKnotVisual.cs | 新候補だけ8字でなくリングの縁を前後に巻く。変更するのはLineRendererの描画点と表示パーツ。anchor／Jointへ書込みなし |
| Scripts/RopeBodyVisual.cs | 既存の頭・首・頭幅の登録点を読むpublic helperのみ追加。姿勢選択・物理・サイズ設定は変更なし |
| PlayerPresentation/ManualPresentationSession.cs | 新候補の既定値と、新撮影クラス不在の読み取り記録を追加 |
| A2Proof/A2Fixture.cs | 新しい記録先、撮影専用driverの描画前入力反映。地形／入口／CP／案内文は維持 |
| 新規 PlayerPresentation/ActiveSwingArms.cs＋meta | 既存毛糸Textureによる表示用の肩・肘・手。入力と実速度が同方向か逆か、速度・最下点・接地から姿勢を選ぶ |
| 新規 PlayerPresentation/ConnectionGestureCapture.cs＋meta | `-gestureCapture 1` の明示起動だけ。既存APIと入力フィールドによる比較収録。通常起動ではインスタンス0 |

新候補だけに適用する分岐で、比較候補2の演出計算は保持。RGBA画像・Shader・衣装・小物は作り直していない。肩／肘は表示の子GameObjectとLineRendererだけで、物理コンポーネントは付けない。

短く説明すると：

- **接地は物理の結果を読む**：接続していても足が床にある間だけ、絵の足裏を合わせる。空中へ吸着させない。
- **入力と勢いを別々に読む**：押して進む、逆に押して受ける、勢いで流れる、頂点で備える、を一定周期でなく状態から選ぶ。
- **見た目と物理を分ける**：手足・胴・ヒモの描画点だけを動かし、Root／Collider／Joint／力／速度は変えない。

指定5コード（RopeResource／RopeController／PlayerMover／RopePlatformBuilder／MainStageRespawnOnFall）はbyte同一。全Scene、Packages、ProjectSettings、Art、Shader、背景、既存の音・影・着地・粒子も同一。Q文言とTutorial／MainStageのProduction差分は変更していない。[保持照合](QA/SourceProtection.json)。

# Tutorial Q案内レビュー

固定ブランチ：`review/q-prompt`。固定入口：[LATEST.json](LATEST.json)。

最新：[Tutorial Q案内修正Proof](runs/20261017T140432_JST/Report.md)／[手動Buildの起動方法](runs/20261017T140432_JST/Launch.md)／[Human結果](runs/20261017T140432_JST/HumanReview.md)。

レビュー資料・通常画角の比較画像・QA・差分だけを保存する。Productionコード／Assets、Build本体、Library、BGMは保存しない。画像はGitHubの通常ファイルで、ZIPやローカルパスだけに閉じ込めない。

新しい結果はrunsに新規追加し、過去runを上書きせずLATESTを同じcommitで更新する。診断API／自動撮影、OS入力、Human試遊、正式採用を区別する。今回の候補はHuman未承認・Production未統合。

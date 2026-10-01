# Whitespace / serialization分離

`git diff --check -- Assets/Scenes/MainStage.unity`はexit 2、trailing whitespace警告49件。全て`m_Name: `の末尾space。**修正していない**。

| 分類 | 件数 | 内容 |
| --- | --- | --- |
| 既存propertyの空白のみ変更 E | 46 | `m_Name:` → `m_Name: `。実値は同じ空文字 |
| 新規MonoBehaviourの空白警告 | 2 | 左右CraftRail componentの新規document内m_Name。document追加C/Dの中のformat問題で、既存propertyのEではない |
| 実値・空白とも同じ既存documentがGit hunkの行対応でadd表示 | 1 | fileID 1313190103、S06 Green Upper Bridge End Hook / Green Rope Anchor Ring Visual。HEADにも同じ末尾spaceあり |
| 合計警告 | 49 | 全警告のline/fileIDをwhitespace_warnings.jsonに保存 |
| 改行のみ差分 | 0 | HEAD/WTともLF、CRLF 0 |

共通documentの順序は同一、fileID振り直し0、削除document 0。上記1件はUnity documentが移動・意味変更された証拠ではなく、反復するYAMLに対するGit diffの行対応由来。property表にも架空の変更を増やしていない。

HEADは18,607 LF、WTは19,187 LF。行数差580は実内容の追加によるもので、全面改行変換ではない。Raw diffは699追加 / 119削除。

## 数え方

意味を持ち得る構造単位は36＝既存property変更16＋追加Unity document20。この20 documentの全default/leafも省略せず02の表へ展開し、全570行となる。

全表の分類：C 390行（Runtimeで再現）、D 134行（追加objectに付随するUnity default/header schema）、E 46行（空白のみ）、A/B/F 0。Dの134行を「134個の独立Gameplay変更」と数えない。追加object自体が丸ごとnoiseという意味でもない。意味のあるComponent/Transform/Renderer値は別行C。

したがってformat/schema noiseは表単位ではD+E＝180行。ただしGitの49警告と表の180行は母数が異なる。実内容36単位が全て余分な空白だった、という説明は誤り。

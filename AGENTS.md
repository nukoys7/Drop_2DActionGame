# AGENTS.md

Unity 製の 2D アクションゲーム「Drop_2DActionGame(仮称)」を5人で開発するリポジトリ。AI エージェントが作業するときの前提とルールをまとめる。

## プロジェクト

- Unity 6000.6.7f1、URP 2D、C#。Unity プロジェクトは `Drop2DActionGame/` 以下にある。
- 名前空間は `Drop`。
- メインシーンは `Assets/Scenes/Main/Main.unity`。個人の試験用シーンは `Assets/Scenes/Sandbox/` に置く。
- `main` の内容はゲームの最初のひな形で、`Samples` フォルダの敵とアイテムは見本。

## 作業分担

最初にアイテムと敵の動きを実装する。アイテムは3名で担当する。

| 担当 | 現在の主なファイル |
|---|---|
| ステータス、当たり判定 | `Scripts/Stats.cs`、`Scripts/Hitbox.cs` |
| 入力、移動、キャラクターの動作 | `Scripts/PlayerController.cs`、`Prefabs/Player.prefab` |
| 敵の動かし方 | `Scripts/Enemies/`、`Prefabs/Enemies/` |
| ステージ、UI | `Scenes/Main/`、`Prefabs/Stage/`、`Scripts/Hud.cs`、`Prefabs/HUD.prefab`、`Scripts/KillZone.cs` |
| アイテム実装 | `Scripts/Items/`、`Prefabs/Items/` |

パスは `Drop2DActionGame/Assets/` からの相対パス。

- 変更は依頼された担当のフォルダに収める。ほかの担当のファイルを変える必要があるときは、先に依頼者に確認する。
- `GameManager.cs` と `CameraFollow.cs` の担当は未定。
- `Samples` は編集せず、複製して使う。

## 競合しやすいファイル

次のファイルは複数の担当に影響する。依頼された場合だけ、必要最小限で変更する。

- `Scenes/Main/Main.unity`: 試験には Sandbox のシーンを使う。
- `Prefabs/Player.prefab`: ステータス担当と移動担当の両方が使う。
- `Scripts/Stats.cs`: プレイヤー、敵、アイテムのすべてが依存している。
- `ProjectSettings/TagManager.asset`、`ProjectSettings/Physics2DSettings.asset`: レイヤーと衝突の設定。手で編集せず、Unity の画面で変更する。

## Unity の注意点

- 新しい Input System のみが有効。`Input.GetKey` などの旧 API は実行時に例外になる。入力は `InputSystem.actions` で取得し、アクションは `Assets/Settings/InputSystem_Actions.inputactions` に定義する。
- ドメインリロードは無効。static 変数は Play のたびにリセットされないので、`Awake` で代入し直すか `[RuntimeInitializeOnLoadMethod]` で初期化する。
- enum はプレハブに番号で保存される。`StatType` などの enum に値を足すときは必ず末尾に追加する。
- `Assets/` 内のファイルを追加、移動、削除するときは `.meta` も一緒に扱う。移動で `.meta` を作り直すと参照が切れる。
- レイヤーは Player、Enemy、Ground、PlayerAttack、EnemyAttack、Item。
- `R` のリスタートは Build Settings に登録されたシーンでしか動かない。
- シーンやプレハブの YAML の手編集は避け、必要なときは差分を最小にする。

## コードの書き方

既存のコードに合わせる。

- フィールドは `[SerializeField] private` で公開し、camelCase で書く。プレフィックスの `_` は付けない。
- 参照の取得は `Awake` で行う。イベントの購読は `OnEnable`、解除は `OnDisable` で行う。
- コメントは日本語で短く、処理の意図や仕様だけを書く。

## アイテムの作り方

- 見本は `Scripts/Items/Samples/` にある。`OnTriggerEnter2D` で `PlayerController.TryGetFromCollider` を呼び、効果を適用して `Destroy(gameObject)` する。
- 効果は `Stats` の `Heal`、`AddModifier`、`SetInvincible` などで実装する。
- ステータス補正だけのアイテムは、`SpeedUpPickup` のプレハブを複製して Inspector の値を変えれば作れる。
- プレハブは Item レイヤーに置き、Collider は Trigger にする。
- 同じ補正は重なって掛かる(1.5 倍を2回で 2.25 倍)。

## Git

`main` は GitHub のルールセットで保護されている。現在は全ブランチが対象で、次の動作になる。

- 既存のブランチへのプッシュはできない。変更は PR でのみ取り込める。
- 新しいブランチの最初のプッシュはできる。
- ブランチの削除と強制プッシュはできない。

そのため、変更を済ませてからブランチを作り、コミットして、1回でプッシュして PR を出す。プッシュ前に最新の `main` を取り込む。ルールセットの対象が変わったら、この節を更新する。

- コミット、プッシュ、PR の作成とマージは、依頼されたときだけ行う。
- コミットには依頼された変更だけを含める。Unity が自動で書き換えた `ProjectSettings` などの無関係な差分は含めない。
- コミットメッセージには変更内容を書く。日付だけのメッセージにしない。
- リポジトリは公開されている。再配布できない購入素材や、秘密情報をコミットしない。
- Git LFS は未設定。大きな画像、音声、動画を追加する前に依頼者に確認する。
- `.gitattributes` の `merge=unityyamlmerge` は、各自の git config に UnityYAMLMerge を登録すると有効になる。

## 確認方法

自動テストはなく、動作確認は Unity エディタで行う。エージェントが Unity を実行できない場合は、未確認であることを報告に明記し、依頼者に Play での確認項目を伝える。

# アイテム C 案の仕様(相談用)

アイテム担当だけでは作れない3つのアイテムの仕様です。ステータス担当、またはプレイヤー担当のファイルに小さな変更が必要なので、先に相談させてください。ドット絵はできています。必要な変更が入れば、アイテム側の実装はアイテム担当が行います。

| アイテム | レア度 | ドット絵 | 相談先 |
|---|---|---|---|
| 不死鳥の羽根 | Epic | `Assets/Art/Items/PhoenixFeather.png` | ステータス(`Stats.cs`) |
| 地割れの靴 | Epic | `Assets/Art/Items/QuakeBoots.png` | 入力・移動(`PlayerController.cs`) |
| 二段ジャンプの羽 | Epic | `Assets/Art/Items/DoubleJumpWings.png` | 入力・移動(`PlayerController.cs`) |

## 不死鳥の羽根

**効果**: 1回だけ、HP が 0 になっても倒れず、最大 HP の半分で復活する。復活のあと2秒間は無敵。拾った数だけ復活できる。

**必要な変更(`Stats.cs`)**: 今は HP が 0 になると、すぐに `OnDied` が呼ばれる。その前に復活の残り回数を見る仕組みがほしい。

```csharp
// 案: 復活の残り回数。HP が 0 になったとき、残っていれば倒れずに復活する
public int Revives { get; set; }
public event Action OnRevived;

// ReduceHP の、hp が 0 になったところで
if (Revives > 0)
{
    Revives--;
    hp = MaxHP * 0.5f;
    SetInvincible(2f);
    OnHPChanged?.Invoke(HP, MaxHP);
    OnRevived?.Invoke();
    return;
}
```

**アイテム側**: 拾ったら `player.Stats.Revives++` するだけ。復活の演出は `OnRevived` を購読して付ける。

## 地割れの靴

**効果**: ドロップキックで着地したとき、周り(半径 2)の敵に衝撃波でダメージ(プレイヤーの攻撃力と同じ)を与え、上に吹き飛ばす。同じものを拾うと範囲が 0.5 ずつ広がる。

**必要な変更(`PlayerController.cs`)**: ドロップキック中に着地したことを、外から知る手段がほしい。

```csharp
// 案: ドロップキック中に地面に着いたとき。引数は着地した位置
public event Action<Vector2> OnDropKickLanded;
```

`FixedUpdate` で着地を見つけて `EndDropKick()` を呼んでいるところ(`grounded && !wasGrounded`)で、ドロップキック中だったときだけ呼ぶ。敵を蹴ってはね返ったとき(`HandleKickHit`)は呼ばない。

**アイテム側**: `PassiveEffect` で `OnDropKickLanded` を購読し、着地位置の周りの敵に `Stats.TakeDamage` を呼ぶ。地面を走る波の見た目も付ける。

## 二段ジャンプの羽

**効果**: 空中でもう1回ジャンプできる。拾った数だけ空中ジャンプの回数が増える。

**相談したいこと(操作)**: 今は空中でジャンプボタンを押すとドロップキックになるので、二段ジャンプと同じボタンになってしまう。次のどれにするか決めたい。

1. 空中で1回目に押すとジャンプ、使い切ったあとはドロップキック
2. 空中で下を押しながらジャンプでドロップキック、ジャンプだけなら二段ジャンプ
3. ドロップキックを別のボタンにする

おすすめは 1 です。操作を変えずに、ジャンプを使い切るとキックになります。

**必要な変更(`PlayerController.cs`)**: 空中ジャンプの残り回数を持つ。着地で最大値に戻す。

```csharp
// 案: 空中で追加でジャンプできる回数。アイテムで増える
public int ExtraAirJumps { get; set; }
```

**アイテム側**: 拾ったら `player.ExtraAirJumps++` するだけ。

## 補足

- 3つとも、変更が入るまではプレハブを作りません。
- `StatType` に値を足す案もありますが、enum は末尾に足す必要があり、ほかのアイテムの補正の仕組みと混ざるので、上のようにプロパティで持つ形を提案します。

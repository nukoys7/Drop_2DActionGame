using UnityEngine;
using UnityEngine.UI;

namespace Drop
{
    // プレイヤーの HP を fill の横幅(anchorMax.x)で表示し、死亡したら gameOverPanel を表示する
    public class Hud : MonoBehaviour
    {
        [SerializeField] private RectTransform fill;
        [SerializeField] private Text label;
        [SerializeField] private GameObject gameOverPanel;

        private void Start()
        {
            var stats = PlayerController.Current.Stats;
            stats.OnHPChanged += Refresh;
            stats.OnDied += () => gameOverPanel.SetActive(true);
            Refresh(stats.HP, stats.MaxHP);
        }

        private void Refresh(float hp, float max)
        {
            fill.anchorMax = new Vector2(hp / max, fill.anchorMax.y);
            label.text = $"HP {Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(max)}";
        }
    }
}

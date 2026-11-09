using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Drop
{
    // 落下とゲームオーバーを管理。ゲームオーバー中は R でリスタート
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Transform respawnPoint;
        [SerializeField] private float fallDamage = 1f;

        private bool gameOver;

        private void Awake() => Instance = this;

        private void Start() => PlayerController.Current.Stats.OnDied += () => gameOver = true;

        private void Update()
        {
            if (gameOver && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void HandlePlayerFell(PlayerController player)
        {
            player.Stats.TakeDirectDamage(fallDamage);
            if (!player.Stats.IsDead) player.Respawn(respawnPoint.position);
        }
    }
}

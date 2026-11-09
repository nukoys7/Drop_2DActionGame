using UnityEngine;

namespace Drop
{
    // プレイヤーを追う
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private float minY = -2f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            var p = PlayerController.Current.transform.position;
            var goal = new Vector3(p.x + offset.x, Mathf.Max(minY, p.y + offset.y), transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        }
    }
}

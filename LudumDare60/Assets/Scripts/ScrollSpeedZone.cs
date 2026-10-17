using System.Collections.Generic;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Section of the level with its own scroll speed, from transform.position.x to x + width.
    /// </summary>
    public class ScrollSpeedZone : MonoBehaviour
    {
        static readonly List<ScrollSpeedZone> zones = new();

        [SerializeField, Min(0.1f)] float width = 20f;
        [SerializeField, Min(0f)] float speed = 2f;

        public float StartX => transform.position.x;
        public float EndX => transform.position.x + width;
        public float Speed => speed;

        void OnEnable() => zones.Add(this);
        void OnDisable() => zones.Remove(this);

        /// <summary>Returns the speed of the zone containing x, or fallback if none does.</summary>
        public static float GetSpeedAt(float x, float fallback)
        {
            foreach (ScrollSpeedZone zone in zones)
            {
                if (x >= zone.StartX && x < zone.EndX)
                {
                    return zone.Speed;
                }
            }
            return fallback;
        }

        void OnDrawGizmos()
        {
            Vector3 center = new(StartX + width * 0.5f, transform.position.y, 0f);
            Vector3 size = new(width, 20f, 0.1f);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.08f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireCube(center, size);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(StartX + 0.3f, transform.position.y + 9f, 0f), $"Scroll {speed:0.##} u/s");
#endif
        }
    }
}

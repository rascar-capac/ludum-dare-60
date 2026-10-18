using UnityEngine;

namespace LD60
{
    /// <summary>Debug buttons drawn over the game view, only in the editor and development builds.</summary>
    public class DebugMenu : MonoBehaviour
    {
        [SerializeField] PlayerWeapon playerWeapon;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 160f, 100f));
            if (GUILayout.Button("Kill all enemies"))
            {
                EnemyController.KillAll();
            }
            if (playerWeapon != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Spears {playerWeapon.SpearCount}/{playerWeapon.MaxSpears}");
                if (GUILayout.Button("-"))
                {
                    playerWeapon.SpearCount--;
                }
                if (GUILayout.Button("+"))
                {
                    playerWeapon.SpearCount++;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
#endif
    }
}

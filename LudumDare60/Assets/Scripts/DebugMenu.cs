using UnityEngine;

namespace LD60
{
    /// <summary>Debug buttons drawn over the game view, only in the editor and development builds.</summary>
    public class DebugMenu : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 160f, 100f));
            if (GUILayout.Button("Kill all enemies"))
            {
                EnemyController.KillAll();
            }
            GUILayout.EndArea();
        }
#endif
    }
}

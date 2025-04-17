using UnityEngine;

public class ExitHandler : MonoBehaviour
{
    public void QuitGame()
    {
        // If we are running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}

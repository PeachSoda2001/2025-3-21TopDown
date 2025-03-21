using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class ResetSceneOnInput : MonoBehaviour
{
    [SerializeField]
    InputAction Reset;

    [SerializeField]
    [Tooltip("If blank, will reload the current scene. Otherwise it'll reload the scene with this name.")]
    string SceneToLoad = "";

    readonly Maid maid = new();

    private void OnEnable()
    {
        maid.GiveEvent(
            Reset,
            "performed",
            (CallbackContext c) =>
            {
                string sceneName = SceneToLoad == "" ? SceneManager.GetActiveScene().name : SceneToLoad;
                SceneManager.LoadScene(sceneName);
            }
        );
        Reset.Enable();
    }

    private void OnDisable()
    {
        maid.Cleanup();
    }
}

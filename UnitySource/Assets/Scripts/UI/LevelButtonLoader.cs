using UnityEngine;

public class LevelButtonLoader : MonoBehaviour
{
    [SerializeField] private Loader.Scene targetScene;

    public void LoadLevel()
    {
        Loader.Load(targetScene);
    }
}

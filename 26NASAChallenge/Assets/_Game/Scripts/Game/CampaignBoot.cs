using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapmakers.Game
{
    public sealed class CampaignBoot : MonoBehaviour
    {
        void Start() { SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Prologue.unity"); }
    }
}

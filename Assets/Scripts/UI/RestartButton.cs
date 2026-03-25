using UnityEngine;
using UnityEngine.UI;
using VBLSmartCrossing.Core;

namespace VBLSmartCrossing.UI
{
    [RequireComponent(typeof(Button))]
    public class RestartButton : MonoBehaviour
    {  
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            GameEvents.RaiseGameReset();
            // Fire-and-forget: método async mas não precisamos esperar, evita warning
            _ = LevelManager.Instance.LoadInitialLevelAsync();
        }
    }
}
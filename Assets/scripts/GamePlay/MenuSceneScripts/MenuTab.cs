using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.scripts.GamePlay.MenuSceneScripts
{
    public class MenuTab : MonoBehaviour
    {
        [SerializeField] private GameObject _settingsTabObj;
        [SerializeField] private GameObject _statisticTabObj;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _statisticButton;
        [SerializeField] private Button _exitButton;


        private void Awake()
        {
            GameSettings.Instance.LoadData();
            GameStatistic.Instance.LoadData();
        }

        void Start()
        {
            _startButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));
            _settingsButton.onClick.AddListener(() => { _settingsTabObj.SetActive(true); gameObject.SetActive(false); });
            _statisticButton.onClick.AddListener(() => { _statisticTabObj.SetActive(true); gameObject.SetActive(false); });

#if UNITY_WEBGL && !UNITY_EDITOR // --
            _exitButton.onClick.AddListener(() => Application.ExternalEval("window.close();"));
#else
            _exitButton.onClick.AddListener(() => Application.Quit());
#endif
        }
    }
}

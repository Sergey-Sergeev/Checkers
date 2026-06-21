using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets
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
            GameSettings.ReadSettingsFromFile();
        }

        void Start()
        {
            _startButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));
            _settingsButton.onClick.AddListener(() => { _settingsTabObj.SetActive(true); gameObject.SetActive(false); });
            _statisticButton.onClick.AddListener(() => { _statisticTabObj.SetActive(true); gameObject.SetActive(false); });
            _exitButton.onClick.AddListener(() => Application.Quit());
        }
    }
}

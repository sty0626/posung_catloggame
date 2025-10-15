using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuUI: MonoBehaviour
{
    public Button startButton;
    public Button settingButton;

    void Start()
    {

        startButton.onClick.AddListener(onStartClicked);
        settingButton.onClick.AddListener(onSettingClicked);
    }


    void onStartClicked()
    {

        SceneManager.LoadScene("GameScene");
    }

    void onSettingClicked()
    {

        Debug.Log("setting ¹öÆ° Å¬¸¯µÊ!");
    }
}
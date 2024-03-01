using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtEnds, txtStones, txtAI, txtVolume;
    [SerializeField] Slider sliderAI, sliderStone, sliderEnds, sliderVolume;
    [SerializeField] Canvas menu, options;


    void Start()
    {
        sliderAI.onValueChanged.AddListener(OnAIChanged);
        sliderStone.onValueChanged.AddListener(OnStonesChanged);
        sliderEnds.onValueChanged.AddListener(OnEndsChanged);
        sliderVolume.onValueChanged.AddListener(OnVolumeChanged);

        sliderAI.value = PlayerPrefs.GetInt("AIDifficulty");
        sliderStone.value = PlayerPrefs.GetInt("Stones");
        sliderEnds.value = PlayerPrefs.GetInt("Ends");
        sliderVolume.value = PlayerPrefs.GetFloat("Volume") * 100f;
    }

    void Update()
    {
        txtAI.text = PlayerPrefs.GetInt("AIDifficulty").ToString();
        txtStones.text = PlayerPrefs.GetInt("Stones").ToString();
        txtEnds.text = PlayerPrefs.GetInt("Ends").ToString();
        txtVolume.text = (PlayerPrefs.GetFloat("Volume") * 100f).ToString();
    }
    void OnAIChanged(float val)
    {
        PlayerPrefs.SetInt("AIDifficulty",  Mathf.RoundToInt(val));
    }
    void OnStonesChanged(float val)
    {
        PlayerPrefs.SetInt("Stones", Mathf.RoundToInt(val));
    }
    void OnEndsChanged(float val)
    {
        PlayerPrefs.SetInt("Ends", Mathf.RoundToInt(val));
    }

    void OnVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat("Volume", val/100f);
    }

    public void BtnBack()
    {
        menu.enabled = true;
        options.enabled = false;
    }
}

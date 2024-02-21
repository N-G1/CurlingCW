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
        sliderAI.onValueChanged.AddListener(onAIChanged);
        sliderStone.onValueChanged.AddListener(onStonesChanged);
        sliderEnds.onValueChanged.AddListener(onEndsChanged);
        sliderVolume.onValueChanged.AddListener(onVolumeChanged);

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
    void onAIChanged(float val)
    {
        PlayerPrefs.SetInt("AIDifficulty",  Mathf.RoundToInt(val));
    }
    void onStonesChanged(float val)
    {
        PlayerPrefs.SetInt("Stones", Mathf.RoundToInt(val));
    }
    void onEndsChanged(float val)
    {
        PlayerPrefs.SetInt("Ends", Mathf.RoundToInt(val));
    }

    void onVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat("Volume", val/100f);
    }

    public void btnBack()
    {
        menu.enabled = true;
        options.enabled = false;
    }
}

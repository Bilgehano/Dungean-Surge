using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using JetBrains.Annotations;

public class MenuController : MonoBehaviour
{   

    [Header("Volume Setting")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private GameObject confirmationPrompt = null;

    [SerializeField] private float defaultVolume = 0.5f;

    [Header("Levels to Load")]
    public string _newgameLevel;

    public void NewGameDialogYes()
    {
        SceneManager.LoadScene(_newgameLevel);
    }

    public void ExitButton()
    {
        Application.Quit();    
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeTextValue.text = volume.ToString("0.0");
    } 

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(string MenuType)
    {
        if (MenuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("0.0");
            VolumeApply();
        }
    }

    public IEnumerator ConfirmationBox()
    {
       confirmationPrompt.SetActive(true);
       yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }
}

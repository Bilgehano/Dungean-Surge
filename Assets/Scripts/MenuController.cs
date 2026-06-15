using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Volume Setting")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private GameObject confirmationPrompt = null;

    [SerializeField] private float defaultVolume = 0.5f;

    [Header("Levels to Load")]
    public string _newgameLevel;

    private Coroutine confirmationRoutine;

    private float savedVolume;
    private float temporaryVolume;

    private void Start()
    {
        if (confirmationPrompt != null)
        {
            confirmationPrompt.SetActive(false);
        }

        savedVolume = PlayerPrefs.GetFloat("masterVolume", defaultVolume);
        temporaryVolume = savedVolume;

        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        UpdateVolumeText(savedVolume);
    }

    public void NewGameDialogYes()
    {
        PlayerSessionData.Reset();
        SceneManager.LoadScene(_newgameLevel);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        temporaryVolume = volume;

        // Preview: Ses geçici olarak değişsin ki kullanıcı duyabilsin.
        AudioListener.volume = temporaryVolume;

        UpdateVolumeText(temporaryVolume);
    }

    public void VolumeApply()
    {
        savedVolume = temporaryVolume;

        PlayerPrefs.SetFloat("masterVolume", savedVolume);
        PlayerPrefs.Save();

        AudioListener.volume = savedVolume;
        UpdateVolumeText(savedVolume);

        if (confirmationRoutine != null)
        {
            StopCoroutine(confirmationRoutine);
        }

        confirmationRoutine = StartCoroutine(ConfirmationBox());
    }

    public void CancelVolumeChanges()
    {
        temporaryVolume = savedVolume;
        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        UpdateVolumeText(savedVolume);

        if (confirmationPrompt != null)
        {
            confirmationPrompt.SetActive(false);
        }
    }

    public void ResetButton(string MenuType)
    {
        if (MenuType == "Audio")
        {
            temporaryVolume = defaultVolume;
            AudioListener.volume = temporaryVolume;

            if (volumeSlider != null)
            {
                volumeSlider.value = temporaryVolume;
            }

            UpdateVolumeText(temporaryVolume);
        }
    }

    private void UpdateVolumeText(float volume)
    {
        if (volumeTextValue != null)
        {
            volumeTextValue.text = volume.ToString("0.0");
        }
    }

    public IEnumerator ConfirmationBox()
    {
        if (confirmationPrompt == null)
        {
            yield break;
        }

        confirmationPrompt.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);

        confirmationPrompt.SetActive(false);
        confirmationRoutine = null;
    }
}
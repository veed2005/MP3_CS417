using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public resource resourceManager;
    public shop shopManager;
    public TutorialPopup tutorialPopup;
    public AudioSource welcomeAudioSource;
    public AudioClip welcomeSoundClip;
    public ParticleSystem welcomeParticles;

    void Start()
    {
        LoadGame();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveGame();
    }

    void SaveGame()
    {
        PlayerPrefs.SetFloat("coins", resourceManager.coins);
        PlayerPrefs.SetFloat("rate", resourceManager.rate);
        PlayerPrefs.SetFloat("rsrc2", resourceManager.rsrc2);
        PlayerPrefs.SetFloat("rate2", resourceManager.rate2);
        PlayerPrefs.SetString("lastPlayTime", DateTime.Now.ToBinary().ToString());

        // Save shop item counts
        for (int i = 0; i < shopManager.items.Length; i++)
        {
            PlayerPrefs.SetInt("shopItem_" + i + "_count", shopManager.items[i].count);
            PlayerPrefs.SetFloat("shopItem_" + i + "_upgradeCost", shopManager.items[i].upgradeCost);
            PlayerPrefs.SetFloat("shopItem_" + i + "_rate", shopManager.items[i].rate);
            PlayerPrefs.SetFloat("shopItem_" + i + "_rate2", shopManager.items[i].rate2);
        }

        PlayerPrefs.Save();
    }

    void LoadGame()
    {
        if (!PlayerPrefs.HasKey("lastPlayTime")) return;

        resourceManager.coins = PlayerPrefs.GetFloat("coins", 0f);
        resourceManager.rate = PlayerPrefs.GetFloat("rate", 0.5f);
        resourceManager.rsrc2 = PlayerPrefs.GetFloat("rsrc2", 0f);
        resourceManager.rate2 = PlayerPrefs.GetFloat("rate2", 0f);

        // Restore shop item counts
        ShopUI shopText = shopManager.GetComponent<ShopUI>();
        for (int i = 0; i < shopManager.items.Length; i++)
        {
            shopManager.items[i].count = PlayerPrefs.GetInt("shopItem_" + i + "_count", 0);
            shopManager.items[i].upgradeCost = PlayerPrefs.GetFloat("shopItem_" + i + "_upgradeCost", shopManager.items[i].upgradeCost);
            shopManager.items[i].rate = PlayerPrefs.GetFloat("shopItem_" + i + "_rate", shopManager.items[i].rate);
            shopManager.items[i].rate2 = PlayerPrefs.GetFloat("shopItem_" + i + "_rate2", shopManager.items[i].rate2);

            // Update the UI to show MAX if maxed
            bool isMaxed = shopManager.items[i].count >= shopManager.items[i].maxCount;
            float buyCost = isMaxed ? -1 : shopManager.items[i].costs[shopManager.items[i].count];
            shopText.UpdateDisplay(i, buyCost, shopManager.items[i].upgradeCost);
        }

        // Calculate offline earnings
        long binary = Convert.ToInt64(PlayerPrefs.GetString("lastPlayTime"));
        DateTime lastTime = DateTime.FromBinary(binary);
        float secondsAway = (float)(DateTime.Now - lastTime).TotalSeconds;

        float offlineCoins = resourceManager.rate * secondsAway;
        float offlineRsrc2 = resourceManager.rate2 * secondsAway;
        resourceManager.coins += offlineCoins;
        resourceManager.rsrc2 += offlineRsrc2;

        if (offlineCoins > 0f)
        {
            if (welcomeAudioSource != null && welcomeSoundClip != null)
                welcomeAudioSource.PlayOneShot(welcomeSoundClip);

            if (welcomeParticles != null)
                welcomeParticles.Play();

            if (tutorialPopup != null)
                tutorialPopup.ShowPopup("Welcome back!\nYou earned " + offlineCoins.ToString("F0") + " Scrap while away!");
        }
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteAll();
    }
}
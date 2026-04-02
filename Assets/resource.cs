using TMPro;
using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using UnityEngine;



public class resource : MonoBehaviour
{
    public TMP_Text resourceText;

    public TMP_Text rsrc2Text;

    public GameObject butt;

    public float coins = 0.0f;
    public float rate = 0.5f;

    public float rsrc2 = 0.0f;
    public float rate2 = 0.0f;
    private bool unlocked2 = false;

    private int trophiesSpawned = 0;

    public GameObject trophy;

    public Transform roomRoot;

    [Header("Resource Particles")]
    [Tooltip("Particle system for Scrap resource growth")]
    public ParticleSystem scrapParticles;

    [Tooltip("Particle system for Joy resource growth")]
    public ParticleSystem joyParticles;

    [Tooltip("Multiplier to scale emission rate relative to resource rate")]
    public float particleRateMultiplier = 5f;

    // "juice" for particles. Sound and animation ease
    [Header("Ramping Juice")]
    public AudioSource rampingAudioSource;
    public AudioClip rampingSoundClip;

    private Vector3 baseScale;
    private float scaleVelocity = 0f;
    private float currentScale = 1f;
    private float targetScale = 1f;
    public float easeSpeed = 8f;

    // Joy ease state
    private Vector3 baseScale2;
    private float currentScale2 = 1f;
    private float targetScale2 = 1f;

    // unlock juice
    [Header("Unlock Juice")]
    public AudioSource unlockAudioSource;
    public AudioClip unlockSoundClip;
    public ParticleSystem unlockBurstParticles;

    [Header("Trophy Juice")]
    public AudioSource trophyAudioSource;
    public AudioClip trophySoundClip;

    public TutorialPopup tutorialPopup;

    [Header("Prestige")]
    public int prestigeLevel = 0;
    public float prestigeMultiplier = 1f;
    public TMP_Text prestigeText;
    public ParticleSystem prestigeParticles;
    public AudioSource prestigeAudioSource;
    public AudioClip prestigeSoundClip;
    public SaveManager saveManager;

    public shop shopManager;

    void Start()
    {
        baseScale = resourceText.transform.localScale;
        baseScale2 = rsrc2Text.transform.localScale;
    }

    public void Prestige()
    {
        // Must have at least some progress to prestige
        if (coins < 100f) return;

        prestigeLevel++;
        prestigeMultiplier = 1f + (prestigeLevel * 0.25f);

        // Reset all counters
        coins = 0f;
        rate = 0.5f * prestigeMultiplier;
        rsrc2 = 0f;
        rate2 = 0f;
        unlocked2 = false;
        trophiesSpawned = 0;
        butt.SetActive(true);

        // Update prestige display
        if (prestigeText != null)
            prestigeText.text = "Prestige: " + prestigeLevel + " (x" + prestigeMultiplier.ToString("F2") + " bonus)";

        // Reset the shop
        if (shopManager != null)
            shopManager.ResetShop();

        // Juice
        if (prestigeParticles != null)
            prestigeParticles.Play();

        if (prestigeAudioSource != null && prestigeSoundClip != null)
            prestigeAudioSource.PlayOneShot(prestigeSoundClip);

        StartCoroutine(PrestigeHaptic());

        // Clear saves since we reset
        if (saveManager != null)
            saveManager.ClearSave();
    }

    IEnumerator PrestigeHaptic()
    {
        for (int i = 0; i < 10; i++)
        {
            UnityEngine.XR.InputDevice rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            UnityEngine.XR.InputDevice leftHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
            rightHand.SendHapticImpulse(0, 1.0f, 0.3f);
            leftHand.SendHapticImpulse(0, 1.0f, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }
    }


    public void updateRate(float inc)
    {
        rate += inc * prestigeMultiplier;
        targetScale = 1.5f;
        if (rampingAudioSource != null && rampingSoundClip != null)
            rampingAudioSource.PlayOneShot(rampingSoundClip);
    }

    IEnumerator HapticUnlock()
    {
        for (int i = 0; i < 40; i++)
        {
            UnityEngine.XR.InputDevice rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            UnityEngine.XR.InputDevice leftHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightHand.SendHapticImpulse(0, 1.0f, 0.3f);
            leftHand.SendHapticImpulse(0, 1.0f, 0.3f);
            yield return new WaitForSeconds(0.15f);
        }
    }



    void Update()
    {
        // Euler integration
        coins += rate * Time.deltaTime;
        rsrc2 += rate2 * Time.deltaTime;

        resourceText.text = $"Scrap: {FormatResource(coins)} (+{FormatRate(rate)}/s)";

        if (unlocked2) {
            rsrc2Text.text = $"Joy: {FormatResource(rsrc2)} (+{FormatRate(rate2)}/s) \n Hint: Deploying more robots \n might bring you more Joy";
        }

        // Update the scrap particle rate and joy particle rate
        UpdateParticleRate(scrapParticles, rate);
        UpdateParticleRate(joyParticles, unlocked2 ? rate2 : 0f);


        if (coins >= 10f && trophiesSpawned == 0)
        {
            SpawnTrophy();
        }

        if (coins >= 200f && trophiesSpawned == 1)
        {
            SpawnTrophy();
        }

        if (coins >= 300f && trophiesSpawned == 2)
        {
            SpawnTrophy();
        }

        // ease of particles
        float easeDelta = easeSpeed * (targetScale - currentScale) * Time.deltaTime;
        currentScale += easeDelta;
        resourceText.transform.localScale = baseScale * currentScale;

        // if reach punch target ease back to 1.0
        if (targetScale > 1f && Mathf.Abs(currentScale - targetScale) < 0.01f)
            targetScale = 1f;

        float easeDelta2 = easeSpeed * (targetScale2 - currentScale2) * Time.deltaTime;
        currentScale2 += easeDelta2;
        rsrc2Text.transform.localScale = baseScale2 * currentScale2;

        if (targetScale2 > 1f && Mathf.Abs(currentScale2 - targetScale2) < 0.01f)
            targetScale2 = 1f;
    }

    // Function to update particle system rate
    void UpdateParticleRate(ParticleSystem ps, float resourceRate)
    {
        if (ps == null) return;
        var emission = ps.emission;
        emission.rateOverTime = resourceRate * particleRateMultiplier;
    }

    void SpawnTrophy()
    {
        trophiesSpawned++;
        GameObject spawned = Instantiate(trophy, roomRoot);
        spawned.transform.localPosition = new Vector3(-21f, 0.5f, 7.0f + ((trophiesSpawned - 1) * 4f));
        spawned.transform.localRotation = Quaternion.identity;
        Debug.Log("Built a trophy!: " + trophiesSpawned);

        // Sound
        if (trophyAudioSource != null && trophySoundClip != null)
            trophyAudioSource.PlayOneShot(trophySoundClip);

        if (tutorialPopup != null)
            tutorialPopup.ShowPopup("Achievement Unlocked!\nTrophy #" + trophiesSpawned);

    }

    string FormatResource(float value)
    {
        if (value >= 100f)
            return value.ToString("F0"); // no decimals
        else
            return value.ToString("F1"); // one decimal
    }

    string FormatRate(float value)
    {
        if (value >= 10f)
            return value.ToString("F0"); // no decimals
        else
            return value.ToString("F2"); // one decimal
    }

    public void updateCoins(float delta) {
        coins += delta;
    }
    public bool canBuy(float amount) {
        return coins >= amount;
    }
    public void updateRate2(float inc)
    {
        rate2 += inc;

        // Ease: punch the Joy text
        targetScale2 = 1.5f;

        // Sound: play the clip
        if (rampingAudioSource != null && rampingSoundClip != null)
            rampingAudioSource.PlayOneShot(rampingSoundClip);
    }

    public void updateRsrc2(float delta) {
        rsrc2 += delta;
    }
    public void TryUnlock2() {
        if (coins < 50f) {
            return;
        }
        updateCoins(-50f);
        unlocked2 = true;
        butt.SetActive(false);
        if (unlockAudioSource != null && unlockSoundClip != null)
            unlockAudioSource.PlayOneShot(unlockSoundClip);

        StartCoroutine(HapticUnlock());
        // Particles
        if (unlockBurstParticles != null)
            unlockBurstParticles.Play();

        if (tutorialPopup != null)
            tutorialPopup.ShowPopup("You unlocked Joy!\nDeploy robots to generate more Joy.");

    }
}
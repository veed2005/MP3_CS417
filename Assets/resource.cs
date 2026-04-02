using TMPro;
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
    public void updateRate(float inc){
        rate += inc;
    }

    public void updateCoins(float delta) {
        coins += delta;
    }
    public bool canBuy(float amount) {
        return coins >= amount;
    }
    public void updateRate2(float inc){
        rate2 += inc;
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
    }
}
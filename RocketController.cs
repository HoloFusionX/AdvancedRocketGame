using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    public float maxSpeed = 20f;
    public float acceleration = 5f;
    public float rotationSpeed = 3f;

    public float maxFuel = 100f;
    public float fuelConsumptionRate = 2f;

    public ParticleSystem engineParticles;
    public AudioClip engineSound;

    public Light sunLight;
    public Material skyboxDay;
    public Material skyboxNight;

    public int score = 0;
    public int maxScore = 100;
    public GameObject achievementPopup;

    public Transform weaponPoint;
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;

    private Rigidbody rb;
    private AudioSource audioSource;

    private float currentFuel;
    private bool isNight = false;

    private bool canFire = true;
    private int playerHealth = 100;
    private int maxHealth = 100;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        currentFuel = maxFuel;
        StartCoroutine(RegenerateHealth());
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (currentFuel > 0)
        {
            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f) * acceleration * Time.deltaTime;
            rb.AddRelativeForce(movement);

            currentFuel -= fuelConsumptionRate * Mathf.Abs(verticalInput) * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);

            if (!audioSource.isPlaying && Mathf.Abs(verticalInput) > 0.1f)
            {
                audioSource.PlayOneShot(engineSound);
                engineParticles.Play();
            }
            else if (audioSource.isPlaying && Mathf.Abs(verticalInput) <= 0.1f)
            {
                audioSource.Stop();
                engineParticles.Stop();
            }
        }

        float rotationInput = -horizontalInput * rotationSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(0f, 0f, rotationInput);
        rb.MoveRotation(rb.rotation * rotation);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        if (Input.GetKeyDown(KeyCode.N))
        {
            isNight = !isNight;
            sunLight.enabled = !isNight;
            RenderSettings.skybox = isNight ? skyboxNight : skyboxDay;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            score += 10;
            if (score >= maxScore)
            {
                achievementPopup.SetActive(true);
            }
        }

        if (Input.GetKey(KeyCode.Mouse0) && canFire)
        {
            FireWeapon();
            canFire = false;
            StartCoroutine(ResetFireRate());
        }
    }

    private void FireWeapon()
    {
        Instantiate(bulletPrefab, weaponPoint.position, weaponPoint.rotation);
    }

    private IEnumerator ResetFireRate()
    {
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (playerHealth < maxHealth)
            {
                playerHealth += 10;
                playerHealth = Mathf.Clamp(playerHealth, 0, maxHealth);
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 200, 20), "Fuel: " + currentFuel.ToString("F0") + " / " + maxFuel);
        GUI.Label(new Rect(20, 40, 200, 20), "Score: " + score);
        GUI.Label(new Rect(20, 60, 200, 20), "Health: " + playerHealth);
    }
}

using UnityEngine;
using TMPro;

// CODED USING GENERATIVE AI, EDITED BY YONATAN CATRAN

public class TeddyBearBoss : MonoBehaviour
{
    [Header("Movement Points (assign 4 transforms in Inspector)")]
    public Transform[] waypoints = new Transform[4];
    public float moveSpeed = 3f;

    [Header("UI")]
    public TextMeshProUGUI damageText;

    private int currentWaypointIndex = 0;
    private float damageTextTimer = 0f;
    private float damageTextDuration = 2f;

    void Start()
    {
        if (damageText != null)
            damageText.gameObject.SetActive(false);
    }

    void Update()
    {
        MoveToWaypoint();
        HandleDamageTextTimer();
    }

    void MoveToWaypoint()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            ShowDamageText();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            ShowDamageText();
        }
    }

    void ShowDamageText()
    {
        if (damageText != null)
        {
            damageText.text = "You damaged the boss!";
            damageText.gameObject.SetActive(true);
            damageTextTimer = damageTextDuration;
        }
    }

    void HandleDamageTextTimer()
    {
        if (damageTextTimer > 0)
        {
            damageTextTimer -= Time.deltaTime;
            if (damageTextTimer <= 0)
                damageText.gameObject.SetActive(false);
        }
    }
}
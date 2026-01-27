using UnityEngine;

public class LoadSceneTrigger : MonoBehaviour
{
    public string sceneToLoad; // exact name from build settings
    public LevelManager levelManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        levelManager.LoadScene(sceneToLoad);
    }
}
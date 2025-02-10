using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public float spawnInterval = 10f;
    public int groupSize = 10;
    public float groupCenterDistance = 30f;
    public float groupRadius = 10f;
    public string playerTag = "Player";
    public float despawnDistance = 50f; // Distance from player to despawn fish

    public Material[] fishMaterials; // Public array to hold fish materials for random selection

    private GameObject player;
    private List<GameObject> spawnedFish = new List<GameObject>(); // List to keep track of spawned fish

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError("Player with tag '" + playerTag + "' not found. Please ensure your player object is tagged correctly.");
            return;
        }

        InvokeRepeating("SpawnFishGroup", 0f, spawnInterval);
    }

    void Update()
    {
        if (player == null) return; // Ensure player still exists

        // Iterate through the spawned fish list and check distance for despawning
        for (int i = spawnedFish.Count - 1; i >= 0; i--) // Iterate backwards to safely remove elements
        {
            GameObject fish = spawnedFish[i];
            if (fish == null) // Check if fish has already been destroyed
            {
                spawnedFish.RemoveAt(i);
                continue;
            }

            float distanceToPlayer = Vector3.Distance(fish.transform.position, player.transform.position);
            if (distanceToPlayer > despawnDistance)
            {
                Destroy(fish); // Destroy the fish GameObject
                spawnedFish.RemoveAt(i); // Remove from the list
            }
        }
    }

    void SpawnFishGroup()
    {
        if (player == null) return; // Ensure player still exists

        Vector3 playerPosition = player.transform.position;

        // Calculate a random direction in the XZ plane
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized; // Normalized to ensure consistent distance
        Vector3 randomDirection3D = new Vector3(randomDirection2D.x, 0f, randomDirection2D.y).normalized;

        // Calculate the group center position 30 units away from the player in a random direction
        Vector3 groupCenter = playerPosition + randomDirection3D * groupCenterDistance;
        groupCenter.y = 0; // Ensure group center is at y = 0

        List<Kinematic> flockTargets = new List<Kinematic>(); // List to hold Kinematic components of the new flock
        GameObject[] currentFishGroup = new GameObject[groupSize]; // Array to hold spawned fish GameObjects temporarily

        Material selectedMaterial = null; // Material to be applied to the current fish group

        // Randomly select a material for this group
        if (fishMaterials != null && fishMaterials.Length > 0)
        {
            int randomIndex = Random.Range(0, fishMaterials.Length);
            selectedMaterial = fishMaterials[randomIndex];
        }
        else
        {
            Debug.LogWarning("No fish materials assigned in FishSpawner. Using default material.");
        }


        for (int i = 0; i < groupSize; i++)
        {
            // Generate a random position within the group radius
            Vector2 randomOffset2D = Random.insideUnitCircle * groupRadius;
            Vector3 spawnPosition = groupCenter + new Vector3(randomOffset2D.x, 0f, randomOffset2D.y);

            // Instantiate the fish
            GameObject fishInstance = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
            fishInstance.transform.position = new Vector3(fishInstance.transform.position.x, 0f, fishInstance.transform.position.z); //Force y=0
            currentFishGroup[i] = fishInstance; // Store fish GameObject

            // Make the fish face the player
            Vector3 directionToPlayer = (playerPosition - fishInstance.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            fishInstance.transform.rotation = lookRotation;

            // Set linear velocity towards the player
            Kinematic kinematic = fishInstance.GetComponent<Kinematic>();
            if (kinematic != null)
            {
                kinematic.linearVelocity = directionToPlayer * 2f;
                flockTargets.Add(kinematic); // Add Kinematic to the list
            }
            else
            {
                Debug.LogWarning("Fish prefab does not have a Kinematic component. Velocity will not be set.");
            }

            // Apply the selected material to the fish
            if (selectedMaterial != null)
            {
                Renderer renderer = fishInstance.GetComponentInChildren<Renderer>(); // Get renderer, assuming it's on the fish prefab or a child
                if (renderer != null)
                {
                    renderer.material = selectedMaterial;
                }
                else
                {
                    Debug.LogWarning("Fish prefab or its children does not have a Renderer component. Material not applied.");
                }
            }


            spawnedFish.Add(fishInstance); // Add the newly spawned fish to the global spawned fish list
        }

        Kinematic[] targetsArray = flockTargets.ToArray(); // Convert list to array

        // Set the targets for each fish in the group to be only the other fish in the same group
        for (int i = 0; i < groupSize; i++)
        {
            Flocker flocker = currentFishGroup[i].GetComponent<Flocker>();
            if (flocker != null)
            {
                flocker.targets = targetsArray; // Set the targets array for the Flocker component
            }
        }
    }
}
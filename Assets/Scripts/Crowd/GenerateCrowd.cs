using UnityEngine;

public class GenerateCrowd : MonoBehaviour
{
    [SerializeField] private GameObject[] crowdPrefabs;
    [SerializeField] private Material[] headColours;
    [SerializeField] private Material[] bodyColours;
    private GameObject currPrefab;
    private const double spawnChance = 0.5;

    ///Script that generates a crowd of a random size with randomized head and body colours each time the game is played
    void Start()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
        foreach (GameObject spawnPoint in spawns)
        {
            //50% chance of spawning random crowd member in seat, quaternion.identity as no rotation is needed  
            if (Random.Range(0.01f, 1.01f) < spawnChance)
            {
                currPrefab = Instantiate(crowdPrefabs[Random.Range(0, 3)], spawnPoint.transform.position, Quaternion.identity, spawnPoint.transform);
                Renderer[] childRenderers = currPrefab.GetComponentsInChildren<Renderer>();

                foreach(Renderer rend in childRenderers)
                {
                    if (rend.gameObject.name == "Head")
                    {
                        rend.material = headColours[Random.Range(0, 4)];
                    }
                    else if(rend.gameObject.name == "Body")
                    {
                        rend.material = bodyColours[Random.Range(0, 7)];
                    }
                }
            }
        }
    }
}

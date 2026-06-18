using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] platePrefabs;
    [SerializeField] private GameObject donutPrefab;
    [SerializeField] private GameObject[] cupcakePrefabs;
    [SerializeField] private GameObject cookiePrefab;
    [SerializeField] private GameObject tartPrefab;
    [SerializeField] private GameObject[] coffeePrefabs;
    [SerializeField] private GameObject[] teaPrefabs;

    [Header("Spawning")]
    private bool hasSpawnedItems;
    [SerializeField] private Transform[] plateSpawns;
    [SerializeField] private Transform[] dessertSpawns;
    [SerializeField] private Transform[] drinkSpawns;
    [SerializeField] private PoofParticle particle;
    [SerializeField] private float timeBetweenPoofAndSpawning;
    private WaitForSeconds waitTimeBetweenPoofAndSpawning;

    [Header("Items")]
    private GameObject[] plates;
    private GameObject[] items;

    private void Awake() => waitTimeBetweenPoofAndSpawning = new WaitForSeconds(timeBetweenPoofAndSpawning);

    public void SpawnItems(MenuItems dessert, MenuItems drink, MenuItems npcDessert, MenuItems npcDrink, bool poof) =>
        StartCoroutine(CreateItems(dessert, drink, npcDessert, npcDrink, poof));

    private IEnumerator CreateItems(MenuItems dessert, MenuItems drink, MenuItems npcDessert, MenuItems npcDrink, bool poof)
    {
        if (poof)
            particle.Play(-1);
        hasSpawnedItems = true;
        plates = new GameObject[2];
        items = new GameObject[4];
        Transform parent;
        yield return waitTimeBetweenPoofAndSpawning;
        for (int i = 0; i < plateSpawns.Length; i++)
        {
            int plateIdx = Random.Range(0, plateSpawns.Length);
            parent = plateSpawns[i];
            plates[i] = Instantiate(platePrefabs[plateIdx], parent.position, parent.rotation, parent);
        }

        parent = dessertSpawns[0];
        items[0] = Instantiate(GetItem(dessert), parent.position, parent.rotation, parent);
        parent = drinkSpawns[0];
        items[1] = Instantiate(GetItem(drink), parent.position, parent.rotation, parent);

        parent = dessertSpawns[1];
        items[2] = Instantiate(GetItem(npcDessert), parent.position, parent.rotation, parent);
        parent = drinkSpawns[1];
        items[3] = Instantiate(GetItem(npcDrink), parent.position, parent.rotation, parent);
    }

    public void RemoveItems()
    {
        if (!hasSpawnedItems) return;
        particle.Play(-1);
        foreach (GameObject item in items)
            Destroy(item);
        foreach (GameObject plate in plates)
            Destroy(plate);
        hasSpawnedItems = false;
        SaveSystem.currentSave.hasItems = false;
    }

    private GameObject GetItem(MenuItems item)
    {
        int rnd = Random.Range(0, 2);
        GameObject obj;
        switch (item)
        {
            default:
            case MenuItems.None:
                Debug.LogError("No item selected");
                return null;
            case MenuItems.Donut:
                obj = donutPrefab;
                break;
            case MenuItems.Cupcake:
                obj = cupcakePrefabs[rnd];
                break;
            case MenuItems.Cookie:
                obj = cookiePrefab;
                break;
            case MenuItems.Tart:
                obj = tartPrefab;
                break;
            case MenuItems.Coffee:
                obj = coffeePrefabs[rnd];
                break;
            case MenuItems.Tea:
                obj = teaPrefabs[rnd];
                break;
        }

        return obj;
    }
}

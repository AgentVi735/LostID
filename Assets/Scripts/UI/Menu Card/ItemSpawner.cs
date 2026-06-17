using System;
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

    [Header("Spawn Locations")]
    [SerializeField] private Transform[] plateSpawns;
    [SerializeField] private Transform[] dessertSpawns;
    [SerializeField] private Transform[] drinkSpawns;

    [Header("Items")]
    private GameObject[] plates;
    private GameObject[] items;

    public void SpawnItems(MenuItems dessert, MenuItems drink, MenuItems npcDessert, MenuItems npcDrink)
    {
        plates = new GameObject[2];
        items = new GameObject[4];
        Transform parent;
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
        foreach (GameObject item in items)
            Destroy(item);
        foreach (GameObject plate in plates)
            Destroy(plate);
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

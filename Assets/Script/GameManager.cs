using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    [SerializeField] Chest ChestPrefab;
    [SerializeField] GameObject Grid;
    [SerializeField] List<string> listItem;
    [SerializeField] int _chestQuantity;

    public void SpawnChest()
    {
        for(int i = 0; i < _chestQuantity; i++)
        {
            Chest chest = Instantiate(ChestPrefab, Grid.transform);
            chest._number = i;
        }
    }
    private void Start()
    {
        SpawnChest();
    }
}

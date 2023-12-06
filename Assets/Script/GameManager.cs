using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    [SerializeField] public Chest ChestPrefab;
    [SerializeField] public GameObject Grid;
    [SerializeField] public List<string> _listOwnItem;
    [SerializeField] public Dictionary<string, Chest> _dictChest;
    [SerializeField] public int _chestQuantity;
    [SerializeField] public bool _useRandomSeed;
    [SerializeField] public string _seed;
    [SerializeField, Range(0,100)] public int _lockRate;

    public void SpawnChest()
    {
        _dictChest.Clear();
        _listOwnItem.Clear();
        System.Random generator = GetRandom();
        for (int i = 0; i < _chestQuantity; i++)
        {
            int random = generator.Next(0, 100);
            Chest chest = Instantiate(ChestPrefab, Grid.transform);
            chest._number = i;
            chest._item = ((char)i+65).ToString();
            _dictChest.Add(chest._item, chest);
            if(random < _lockRate)
            {
                chest._isLock = true;
            }
        }
        AddLockItem(generator);
    }
    private void Start()
    {
        SpawnChest();
    }

    private System.Random GetRandom()
    {
        if (_useRandomSeed)
        {
            _seed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
        }
        return new System.Random(_seed.GetHashCode());
    }

    public void AddLockItem(System.Random generator)
    {
        List<string> itemsList = _dictChest.Keys.ToList<string>();
        foreach (var (item,chest) in _dictChest)                
        {
            if (chest._isLock)
            {
                chest._itemToUnlock = itemsList[generator.Next(0, itemsList.Count - 1)];
            }
        }
    }
    public void NarrowDownItem()
    {

    }

    public bool IsChainUnlock(Chest chest)
    {
        if(chest._isLock)
        {
            return IsChainUnlock(_dictChest[chest._itemToUnlock]);
        }
        return true;
    }
}

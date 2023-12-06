using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    [SerializeField] public Chest ChestPrefab;
    [SerializeField] public GameObject Grid;
    [SerializeField] public List<string> _listOwnItem = new List<string>();
    [SerializeField] public TextMeshProUGUI _ownItem;
    [SerializeField] public TextMeshProUGUI _chestQuantityText;
    [SerializeField] public Dictionary<string, Chest> _dictChest = new Dictionary<string, Chest>();
    [SerializeField] public int _chestQuantity;
    [SerializeField] public bool _useRandomSeed;
    [SerializeField] public string _seed;
    [SerializeField] public GridLayoutGroup _grid;
    [SerializeField, Range(0,100)] public int _lockRate;

    private void Awake()
    {
        _instance = this;
    }
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
            chest._item = ((char)(i+65)).ToString();
            chest._nomChest.text = chest._item;

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
                chest._itemToUnlock = chest._item;
                //while (chest._itemToUnlock == chest._item)
                //{
                    chest._itemToUnlock = itemsList[generator.Next(0, itemsList.Count - 1)];
                //}
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

    public void RegenerateChest()
    {
        foreach(var chest in _dictChest) {
            Destroy(chest.Value.gameObject);
        }
        float cellsize = Mathf.Min(1000 / _chestQuantity,200);
        float spacing = Mathf.Min(_chestQuantity * 3f, 55);
        _grid.cellSize = new Vector2(cellsize, cellsize);
        _grid.spacing = new Vector2(_grid.spacing.x, spacing);
        SpawnChest();
        RefreshListOwnItem();
    }
    public void RefreshListOwnItem()
    {
        _ownItem.text = "Items owned : ";
        for (int i = 0; i < _listOwnItem.Count; i++)
        {
            _ownItem.text = _ownItem.text + _listOwnItem[i].ToString() + " / ";
        }     
    }
    public void IncreaseChestNumber()
    {
        if (_chestQuantity >= 30)
        {
            return;
        }
        else
        {
            _chestQuantity = _chestQuantity + 1;
            _chestQuantityText.text = _chestQuantity.ToString();
        }
    }

    public void DecreaseChestNumber()
    {
        if(_chestQuantity <= 2)
        {
            return;
        }
        else
        {
            _chestQuantity = _chestQuantity - 1;
            _chestQuantityText.text = _chestQuantity.ToString();
        }
    }
}

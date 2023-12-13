using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    private List<Chest> _chainedChestList = new List<Chest>();
    private List<Chest> _startChestList = new List<Chest>();
    private List<Chest> _unlockedChestList = new List<Chest>();
    private List<Chest> _lockedChestList = new List<Chest>();
    [SerializeField] public int _chestQuantity;
    [SerializeField] public bool _useRandomSeed;
    [SerializeField] public string _seed;
    [SerializeField] public GridLayoutGroup _grid;
    [SerializeField, Range(0,100)] private int _lockRate;
    /// <summary>
    /// The probability it has to be chained with other locked chests
    /// </summary>
    [SerializeField, Range(0,100)] private int _chainRate;

    private void Awake()
    {
        _instance = this;
    }
    public void SpawnChest()
    {
        _dictChest.Clear();
        _listOwnItem.Clear();

        _lockedChestList.Clear();
        _unlockedChestList.Clear();
        _startChestList.Clear();
        _chainedChestList.Clear();
        System.Random generator = GetRandom();
        for (int i = 0; i < _chestQuantity; i++)
        {
            int lockLuck = generator.Next(0, 100);
            Chest chest = Instantiate(ChestPrefab, Grid.transform);
            chest.SetNumber(i);
            chest.SetItem(((char)(i+65)).ToString());
            _dictChest.Add(chest.Item, chest);
            if(lockLuck < _lockRate)
            {
                chest.NeedsItemToUnlock(true);
                _lockedChestList.Add(chest);
                int chainedLuck = generator.Next(0, 100);
                if (chainedLuck < _chainRate) 
                { 
                    // Chest is part of a chain
                    _chainedChestList.Add(chest);
                    Debug.Log($"Chest {chest.Item} is part of a chain.");
                } else
                {
                    // Chest is starting a chain
                    _startChestList.Add(chest);
                    Debug.Log($"Chest {chest.Item} is starting a chain.");
                }
            } else
            {
                Debug.Log($"Chest {chest.Item} does not need item to be open. (has item {chest.ItemToUnlock})");
                _unlockedChestList.Add(chest);
                chest.NeedsItemToUnlock(false);
            }
        }
        // On a pas de chest libre
        if (_unlockedChestList.Count <= 0 && _lockedChestList.Count > 0)
        {
            Chest chestToConvert = _lockedChestList[generator.Next(0, _lockedChestList.Count-1)];
            _lockedChestList.Remove(chestToConvert);
            _chainedChestList.Remove(chestToConvert);
            _startChestList.Remove(chestToConvert);
            chestToConvert.NeedsItemToUnlock(false);
            _unlockedChestList.Add(chestToConvert);
            Debug.Log($"Chest {chestToConvert.Item} has been updated as no chest were unlocked.");
        }
        if (_startChestList.Count <= 0 && _chainedChestList.Count > 0)
        { 
            int oldChainedChestIndex = generator.Next(0, _chainedChestList.Count-1);
            _startChestList.Add(_chainedChestList[oldChainedChestIndex]);
            _chainedChestList.RemoveAt(oldChainedChestIndex);
            Debug.Log($"Chest {_startChestList[0].Item} has been updated and is now a start chest for a chain.");
        }

        AddLockItem(generator);
    }
    private void Start()
    {
        SpawnChest();
    }

    private System.Random GetRandom()
    {
        /*
         * The current implementation of the Random class is based on a modified version of Donald E. Knuth's subtractive random number generator algorithm. 
         * For more information, see D. E. Knuth. The Art of Computer Programming, Volume 2: Seminumerical Algorithms. Addison-Wesley, Reading, MA, third edition, 1997.
        */
        if (_useRandomSeed)
        {
            _seed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
        }
        return new System.Random(_seed.GetHashCode());
    }

    public void AddLockItem(System.Random generator)
    {
        List<string> itemsList = _dictChest.Keys.ToList<string>();
        // Sets chests that are nested in a chain
        foreach (Chest chest in _chainedChestList)
        {
            chest.SetItemToUnlock(_lockedChestList[generator.Next(0, _lockedChestList.Count - 1)].Item);
        }
        // Sets chests that are at the start of a chain
        foreach(Chest chest in _startChestList)
        {
            chest.SetItemToUnlock(_unlockedChestList[generator.Next(0, _unlockedChestList.Count - 1)].Item);
        }
        // Unlocks soft locked chains
        foreach(Chest chest in _chainedChestList)
        {
            (bool isUnLocked, Chest blockedChest) =  IsChainUnlock(chest, new Dictionary<string, bool>());
            if (!isUnLocked)
            {
                Debug.Log($"Chest {blockedChest.Item} has been updated as it was blocking a chain.");
                blockedChest.SetItemToUnlock(_startChestList[generator.Next(0, _startChestList.Count - 1)].Item);
            }
        }
    }

    public void NarrowDownItem(System.Random generator)
    {
        // Create list of start chest based on percentage
        // Place other locked chests in another list
        // Have a list for cleared chests
    }

    /// <summary>
    /// Checks if current chest chain is locked
    /// </summary>
    /// <param name="chest">Start chase, used for recursive search</param>
    /// <param name="historySearch">Empty dictionnary for history maintainance</param>
    /// <returns>The last chest of the chain if chain is not locked (true), the first chest to repeat in the chain loop (depends on the starting chest to check)</returns>
    public KeyValuePair<bool, Chest> IsChainUnlock(Chest chest, Dictionary<string, bool> historySearch)
    {
        if (historySearch.ContainsKey(chest.Item))
            return new (false, chest);
        if(chest.IsChestLock)
        {
            historySearch.Add(chest.Item, true);
            return IsChainUnlock(_dictChest[chest.ItemToUnlock], historySearch);
        }
        return new (true, chest);
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

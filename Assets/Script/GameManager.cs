using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;

    [HideInInspector] public List<string> m_listOwnItem = new List<string>();
    
    public bool m_useRandomSeed;
    public string m_seed;
    [Range(0, 100)] public int m_lockRate;
    /// <summary>
    /// The probability it has to be chained with other locked chests
    /// </summary>
    [Range(0, 100)] public int m_chainRate;

    #region Private Serializable
    [SerializeField] private Chest _chestPrefab;
    [SerializeField] private GameObject _gridGO;
    [SerializeField] private TextMeshProUGUI _solutionText;
    [SerializeField] private TextMeshProUGUI _seedText;
    [SerializeField] private TextMeshProUGUI _ownItem;
    [SerializeField] private TextMeshProUGUI _chestQuantityText;
    [SerializeField] public int _chestQuantity;
    [SerializeField] private GridLayoutGroup _grid;
    #endregion

    #region Public Methods
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
        for (int i = 0; i < m_listOwnItem.Count; i++)
        {
            _ownItem.text = _ownItem.text + m_listOwnItem[i].ToString() + " / ";
        }
        string Solution = GetAllChains();

        _solutionText.text = "Items order : " + Solution;
        _seedText.text = "Seed : " + m_seed;
    }
    #region Increase Deacrease
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
            Chest AppendedChest = AppendChest();
            if (AppendedChest.IsChestLock)
            {
                if (_chainedChestList.Contains(AppendedChest))
                {
                    AppendedChest.SetItemToUnlock(_lockedChestList[_generator.Next(0, _lockedChestList.Count - 1)].Item); 
                }
                else
                {
                    AppendedChest.SetItemToUnlock(_unlockedChestList[_generator.Next(0, _unlockedChestList.Count - 1)].Item);
                }
            }
            RefreshListOwnItem();
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
            List<Chest> AllChestsList = Enumerable.ToList(_dictChest.Values);
            Chest ChestToDestroy = AllChestsList[_generator.Next(0, AllChestsList.Count - 1)];
            DestroyChestFromStorage(ChestToDestroy);
            string ReplacementItem = string.Empty;
            // Chest is lock
            if (!ChestToDestroy.IsChestLock)
            {
                // Another not locked chest exists
                if (_unlockedChestList.Count > 0)
                {
                    ReplacementItem = _unlockedChestList[_generator.Next(0, _unlockedChestList.Count - 1)].Item;
                }
                else // No more locked chests, need to create one
                {
                    Chest chestToConvert = _lockedChestList[_generator.Next(0, _lockedChestList.Count - 1)];
                    _lockedChestList.Remove(chestToConvert);
                    _chainedChestList.Remove(chestToConvert);
                    _startChestList.Remove(chestToConvert);
                    chestToConvert.NeedsItemToUnlock(false);
                    _unlockedChestList.Add(chestToConvert);
                    ReplacementItem = chestToConvert.Item;
                }
            } else
            {
                ReplacementItem = ChestToDestroy.ItemToUnlock;
            }
            foreach(Chest chest in _lockedChestList)
            {
                if (chest.ItemToUnlock == ChestToDestroy.Item)
                {
                    chest.SetItemToUnlock(ReplacementItem);
                }
            }
            Destroy(ChestToDestroy.gameObject);
            RefreshListOwnItem();
        }
        
    }
    #endregion
    #endregion

    #region Private 
    private Dictionary<string, Chest> _dictChest = new Dictionary<string, Chest>();
    private List<Chest> _chainedChestList = new List<Chest>();
    private List<Chest> _startChestList = new List<Chest>();
    private List<Chest> _unlockedChestList = new List<Chest>();
    private List<Chest> _lockedChestList = new List<Chest>();
    private System.Random _generator;
    private static int _chestIndex = 0;
    #region Private Methods
    #region Get values
    private System.Random GetRandom()
    {
        /*
         * The current implementation of the Random class is based on a modified version of Donald E. Knuth's subtractive random number _generator algorithm.
         * For more information, see D. E. Knuth. The Art of Computer Programming, Volume 2: Seminumerical Algorithms. Addison-Wesley, Reading, MA, third edition, 1997.
        */
        if (m_useRandomSeed)
        {
            m_seed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
        }
        return new System.Random(m_seed.GetHashCode());
    }

    /// <summary>
    /// Checks if current chest chain is locked
    /// </summary>
    /// <param name="chest">Start chase, used for recursive search</param>
    /// <param name="historySearch">Empty dictionnary for history maintainance</param>
    /// <returns>The last chest of the chain if chain is not locked (true), the first chest to repeat in the chain loop (depends on the starting chest to check)</returns>
    private KeyValuePair<bool, Chest> IsChainUnlock(Chest chest, Dictionary<string, bool> historySearch)
    {
        if (historySearch.ContainsKey(chest.Item))
            return new(false, chest);
        if (chest.IsChestLock)
        {
            historySearch.Add(chest.Item, true);
            return IsChainUnlock(_dictChest[chest.ItemToUnlock], historySearch);
        }
        return new(true, chest);
    }

    private string FindWhichChestsItOpens(Chest TargetChest)
    {
        string Result = string.Empty;
        foreach(KeyValuePair<string, Chest> ChestElement in  _dictChest)
        {
            if (ChestElement.Key != TargetChest.Item)
            {
                if (ChestElement.Value.ItemToUnlock ==  TargetChest.Item)
                {
                    Result += ChestElement.Key;
                }
            }
        }
        return Result;
    }

    private string GetAllChains()
    {
        string StartSolution = string.Empty;
        foreach (Chest chest in _unlockedChestList)
        {
            StartSolution += chest.Item;
        }
        return GetAllChainsWithStartingChests(StartSolution);
    }

    private string GetAllChainsWithStartingChests(string ChestsToCheck, string ChainChest="")
    {
        if (ChestsToCheck.Length <= 0)
        {
            return ChainChest;
        }
        return GetAllChainsWithStartingChests(ChestsToCheck.Substring(1)+FindWhichChestsItOpens(_dictChest[ChestsToCheck[0].ToString()]), ChainChest += ChestsToCheck[0]);
    }

    private string GetRandomSolution()
    {
        string Solution = string.Empty;
        foreach(Chest chest in _unlockedChestList)
        {
            Solution += chest.Item;
        }
        Solution = GetAllChainsWithStartingChests(Solution);

        return Solution;
    }
    #endregion
    #region Logic
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
        
    }
    private void Start()
    {
        RegenerateChest();
    }
    private void SpawnChest()
    {
        _dictChest.Clear();
        m_listOwnItem.Clear();

        _lockedChestList.Clear();
        _unlockedChestList.Clear();
        _startChestList.Clear();
        _chainedChestList.Clear();
        _generator = GetRandom();
        _chestIndex = 0;
        for (int i = 0; i < _chestQuantity; i++)
        {
            AppendChest();
        }
        // On a pas de chest libre
        if (_unlockedChestList.Count <= 0 && _lockedChestList.Count > 0)
        {
            Chest chestToConvert = _lockedChestList[_generator.Next(0, _lockedChestList.Count - 1)];
            _lockedChestList.Remove(chestToConvert);
            _chainedChestList.Remove(chestToConvert);
            _startChestList.Remove(chestToConvert);
            chestToConvert.NeedsItemToUnlock(false);
            _unlockedChestList.Add(chestToConvert);
            Debug.Log($"Chest {chestToConvert.Item} has been updated as no chest were unlocked.");
        }
        if (_startChestList.Count <= 0 && _chainedChestList.Count > 0)
        {
            int oldChainedChestIndex = _generator.Next(0, _chainedChestList.Count - 1);
            _startChestList.Add(_chainedChestList[oldChainedChestIndex]);
            _chainedChestList.RemoveAt(oldChainedChestIndex);
            Debug.Log($"Chest {_startChestList[0].Item} has been updated and is now a start chest for a chain.");
        }

        AddLockItemsToAllChests();
    }
    private void AddLockItemsToAllChests()
    {
        List<string> itemsList = _dictChest.Keys.ToList<string>();
        // Sets chests that are nested in a chain
        foreach (Chest chest in _chainedChestList)
        {
            chest.SetItemToUnlock(_lockedChestList[_generator.Next(0, _lockedChestList.Count - 1)].Item);
        }
        // Sets chests that are at the start of a chain
        foreach (Chest chest in _startChestList)
        {
            chest.SetItemToUnlock(_unlockedChestList[_generator.Next(0, _unlockedChestList.Count - 1)].Item);
        }
        // Unlocks soft locked chains
        foreach (Chest chest in _chainedChestList)
        {
            (bool isUnLocked, Chest blockedChest) = IsChainUnlock(chest, new Dictionary<string, bool>());
            if (!isUnLocked)
            {
                Debug.Log($"Chest {blockedChest.Item} has been updated as it was blocking a chain.");
                blockedChest.SetItemToUnlock(_startChestList[_generator.Next(0, _startChestList.Count - 1)].Item);
            }
        }
    }
    private Chest AppendChest()
    {
        int lockLuck = _generator.Next(0, 100);
        Chest chest = Instantiate(_chestPrefab, _gridGO.transform);
        chest.SetNumber(_chestIndex);
        chest.SetItem(((char)(_chestIndex + 65)).ToString());
        _dictChest.Add(chest.Item, chest);
        if (lockLuck < m_lockRate)
        {
            chest.NeedsItemToUnlock(true);
            _lockedChestList.Add(chest);
            int chainedLuck = _generator.Next(0, 100);
            if (chainedLuck < m_chainRate)
            {
                // Chest is part of a chain
                _chainedChestList.Add(chest);
                Debug.Log($"Chest {chest.Item} is part of a chain.");
            }
            else
            {
                // Chest is starting a chain
                _startChestList.Add(chest);
                Debug.Log($"Chest {chest.Item} is starting a chain.");
            }
        }
        else
        {
            Debug.Log($"Chest {chest.Item} does not need item to be open. (has item {chest.ItemToUnlock})");
            _unlockedChestList.Add(chest);
            chest.NeedsItemToUnlock(false);
        }
        _chestIndex++;
        return chest;
    }
    private void DestroyChestFromStorage(Chest chestToDestroy)
    {
        _dictChest.Remove(chestToDestroy.Item);
        m_listOwnItem.Remove(chestToDestroy.Item);
        _lockedChestList.Remove(chestToDestroy);
        _unlockedChestList.Remove(chestToDestroy);
        _startChestList.Remove(chestToDestroy);
        _chainedChestList.Remove(chestToDestroy);
        RefreshListOwnItem();
    }
    #endregion
    #endregion
    #endregion
}

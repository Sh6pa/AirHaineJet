using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Chest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _itemToUnlockText;
    [SerializeField] private TextMeshProUGUI _itemText;
    [SerializeField] private Animator _animator;


    private int _number;
    public int Number { get { return _number; } }
    private string _item;
    public string Item { get { return _item; } }
    private bool _isLock;
    public bool IsChestLock { get { return _isLock; } }
    private string _itemToUnlock;
    public string ItemToUnlock { get { return _itemToUnlock; } }
    private bool _isOpen;
    public bool IsChestOpen { get { return _isOpen; } }

    private bool _isItemTaken;
    
    public void SetNumber(int number)
    {
        _number = number;
    }

    public void SetItem(string item)
    {
        _item = item;
        _itemText.text = _item;
    }

    public void NeedsItemToUnlock(bool bNeedsIt)
    {
        _isLock = bNeedsIt;
        _itemToUnlockText.color = bNeedsIt ? Color.red : Color.green;
        if (!bNeedsIt)
        {
            SetItemToUnlock(" ");// string.Empty
        }
    }

    public void SetItemToUnlock(string item)
    {
        _itemToUnlock = item;
        _itemToUnlockText.text = _itemToUnlock;
    }

    public void TryOpenChest()
    {
        if (GameManager.Instance.m_listOwnItem.Contains(_itemToUnlock) || _isLock == false)
        {
            _animator.Play("OpeningChest");
            _itemToUnlockText.color = Color.green;
            // _textChest.text = "Prend l'item " + _item.ToString();
            _isOpen = true;
        }
        else
        {
            // _textChest.text = "Item " + _itemToUnlock + " Manquant";
            _itemToUnlockText.text = _itemToUnlock ;
        }
    }
    public void InteractChest()
    {
        if(_isOpen == false)
        {
            TryOpenChest();
        }
        else if(_isItemTaken  == false)
        {
            TakeItem();
        }
    }
    public void TakeItem()
    {
        _isItemTaken = true;
        GameManager.Instance.m_listOwnItem.Add(_item);
        GameManager.Instance.RefreshListOwnItem();
        _animator.Play("EmptyChest");
        Debug.Log("Item " + _item + " est récuperer");
    }
}

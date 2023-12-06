using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Chest : MonoBehaviour
{
    [SerializeField] public int _number;
    [SerializeField] public string _item;
    [SerializeField] public bool _isOpen;
    [SerializeField] public bool _isLock;
    [SerializeField] public string _itemToUnlock;
    [SerializeField] public bool _isItemTaken;
    [SerializeField] public TextMeshProUGUI _textChest;
    [SerializeField] public TextMeshProUGUI _nomChest;
    [SerializeField] public Animator _animator;

    public void Start()
    {
        _textChest.text = "";
    }
    public void TryOpenChest()
    {
        if (GameManager._instance._listOwnItem.Contains(_itemToUnlock) || _isLock == false)
        {
            _animator.Play("OpeningChest");
            _textChest.text = "Prend l'item " + _item.ToString();
            _isOpen = true;
        }
        else
        {
            _textChest.text = "Item " + _itemToUnlock + " Manquant";
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
        GameManager._instance._listOwnItem.Add(_item);
        GameManager._instance.RefreshListOwnItem();
        _animator.Play("EmptyChest");
        Debug.Log("Item " + _item + " est récuperer");
    }
}

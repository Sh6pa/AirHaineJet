using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Chest : MonoBehaviour
{
    [SerializeField] public  int _number;
    [SerializeField] public string _item;
    [SerializeField] public bool _isOpen;
    [SerializeField] public bool _isLock;
    [SerializeField] public string _itemToUnlock;
    [SerializeField] public bool _isItemTaken;


    public void TryOpenChest()
    {
        if (GameManager._instance._listOwnItem.Contains(_itemToUnlock))
        {
            Debug.Log("chest ouvert");
            _isOpen = true;
        }
        else
        {
            Debug.Log("Item Manquant");
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
        Debug.Log("Item " + _item + " est récuperer");
    }
}

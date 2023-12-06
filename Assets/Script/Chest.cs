using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    [SerializeField] public  int _number;
    [SerializeField] public string _name;

    public void OpenChest()
    {
        Debug.Log("chest ouvert");
    }


}

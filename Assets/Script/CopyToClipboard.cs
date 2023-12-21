using TMPro;
using UnityEngine;

public class CopyToClipboard : MonoBehaviour
{
    public TextMeshProUGUI textToCopy;

    public void CopyTextToClipboard()
    {
        GUIUtility.systemCopyBuffer = textToCopy.text;
    }
}

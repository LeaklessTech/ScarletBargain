using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour
{

    public TMP_Text textMeshProObject;
    public FloatReference prisonerCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textMeshProObject.text = "You Win! \n The AI is defeated \n You rescued " + prisonerCount.Variable.Variable + " prisoners";
    }
}

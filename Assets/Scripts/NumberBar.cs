using UnityEngine;

public class NumberBar : MonoBehaviour
{
    [SerializeField] private Transform bar;

    public void SetValue(float ratio)
    {
        bar.localScale = new Vector3(ratio, 1, 1);
    }
}
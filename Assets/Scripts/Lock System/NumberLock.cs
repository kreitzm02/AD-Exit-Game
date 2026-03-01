using UnityEngine;
using UnityEngine.Events;

public class NumberLock : MonoBehaviour
{
    [Header("Dials")]
    [SerializeField] private NumericDial[] dials = new NumericDial[3];

    [Header("Code")]
    [Range(0, 9)][SerializeField] private int codeA = 3;
    [Range(0, 9)][SerializeField] private int codeB = 7;
    [Range(0, 9)][SerializeField] private int codeC = 1;

    [Header("Events")]
    public UnityEvent OnCorrectCode;
    public UnityEvent OnWrongCode;

    [Header("Options")]
    [SerializeField] private bool autoCheckOnChange = true;
    [SerializeField] private bool fireOnlyOnce = true;

    private bool fired;

    private void OnEnable()
    {
        Hook(true);
        if (autoCheckOnChange) Check();
    }

    private void OnDisable()
    {
        Hook(false);
    }

    private void Hook(bool subscribe)
    {
        if (dials == null) return;

        for (int i = 0; i < dials.Length; i++)
        {
            if (dials[i] == null) continue;

            if (subscribe) dials[i].OnValueChanged.AddListener(OnDialChanged);
            else dials[i].OnValueChanged.RemoveListener(OnDialChanged);
        }
    }

    private void OnDialChanged(int _)
    {
        if (autoCheckOnChange) Check();
    }

    public void Check()
    {
        if (fireOnlyOnce && fired) return;
        if (dials == null || dials.Length < 3) return;

        bool correct =
            dials[0].Value == codeA &&
            dials[1].Value == codeB &&
            dials[2].Value == codeC;

        if (correct)
        {
            fired = true;
            OnCorrectCode?.Invoke();
        }
        else
        {
            OnWrongCode?.Invoke();
        }
    }

    public void ResetLock()
    {
        fired = false;
    }

    public void SetCode(int a, int b, int c)
    {
        codeA = Mathf.Clamp(a, 0, 9);
        codeB = Mathf.Clamp(b, 0, 9);
        codeC = Mathf.Clamp(c, 0, 9);
    }
}

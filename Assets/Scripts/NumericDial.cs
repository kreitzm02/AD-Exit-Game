using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NumericDial : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private int value;

    public UnityEvent<int> OnValueChanged;

    public int Value => value;

    private void Awake()
    {
        SetValue(value, notify: false);

        incrementButton.onClick.RemoveAllListeners();
        incrementButton.onClick.AddListener(Increment);

        decrementButton.onClick.RemoveAllListeners();
        decrementButton.onClick.AddListener(Decrement);
    }

    public void Increment()
    {
        SetValue((value + 1) % 10);
    }

    public void Decrement()
    {
        SetValue((value + 9) % 10);
    }

    public void SetValue(int newValue, bool notify = true)
    {
        newValue = ((newValue % 10) + 10) % 10;
        if (newValue == value) return;

        value = newValue;
        if (valueText) valueText.text = value.ToString();

        if (notify) OnValueChanged?.Invoke(value);
    }
}

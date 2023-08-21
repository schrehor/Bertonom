using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] private Text countTxt;
    [SerializeField] private Text priceTxt;

    private bool _selected;
    private int _currentCount;

    private int _maxCount;
    private float _pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit,
            Action<int> onCountSelected)
    {
        _maxCount = maxCount;
        _pricePerUnit = pricePerUnit;
        
        _selected = false;
        _currentCount = 1;
        
        gameObject.SetActive(true);
        SetValues();
        
        yield return new WaitUntil(() => _selected == true);
        
        onCountSelected?.Invoke(_currentCount);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        int prevCount = _currentCount;
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _currentCount++;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _currentCount--;
        }
        
        _currentCount = Mathf.Clamp(_currentCount, 1, _maxCount);

        if (_currentCount != prevCount)
        {
            SetValues();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            _selected = true;
        }
    }

    void SetValues()
    {
        countTxt.text = "x " + _currentCount;
        priceTxt.text = "$" + _currentCount * _pricePerUnit;
    }
}

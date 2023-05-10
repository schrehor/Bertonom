using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public static Fader Instance { get; private set; }
    private Image _image;

    private void Awake()
    {
        Instance = this;
        _image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float time)
    {
        yield return _image.DOFade(1, time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return _image.DOFade(0, time).WaitForCompletion();
    }
}

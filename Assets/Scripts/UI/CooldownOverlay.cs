using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class CooldownOverlay : MonoBehaviour
{
    [SerializeField] private float duration = 3f;            // seconds
    [SerializeField] private float maxAlpha = 0.65f;         // darkness at start

    private Image  _img;
    private float  _timer;
    private bool   _running;
    private Action _onFinished;                              // optional callback

    public bool IsRunning => _running;

    private void Awake() => _img = GetComponent<Image>();

    public void Begin(float newDuration, Action onFinished = null)
    {
        duration    = newDuration;
        _timer      = duration;
        _running    = true;
        _onFinished = onFinished;

        // fully dark & fully filled
        _img.fillAmount = 1f;
        SetAlpha(maxAlpha);
        _img.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_running) return;

        _timer -= Time.deltaTime;
        float ratio = Mathf.Clamp01(_timer / duration);

        _img.fillAmount = ratio;

        SetAlpha(maxAlpha * ratio);

        if (_timer <= 0f)
        {
            _running = false;
            _img.gameObject.SetActive(false);
            _onFinished?.Invoke();
        }
    }

    private void SetAlpha(float a)
    {
        Color c = _img.color;
        c.a = a;
        _img.color = c;
    }
}
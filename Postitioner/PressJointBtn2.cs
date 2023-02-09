using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressJointBtn2 : MonoBehaviour
{
    public float threshold;
    public float deadZone;

    bool _isPressd;
    Vector3 _startPos;
    ConfigurableJoint _joint;
    Function onPressed, onReleased;
    AudioSource _audioSource;

    void Start()
    {
        _startPos = transform.localPosition;
        _joint = GetComponent<ConfigurableJoint>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!_isPressd)
        {
            if (GetValue() + threshold >= 1)
                Pressed();
        }
        else
        {
            if (GetValue() - threshold == -(threshold))
            {
                Released();
            }
        }
    }

    public void AddListener(Function onPressed = null, Function onReleased = null)
    {
        this.onPressed = onPressed;
        this.onReleased = onReleased;
    }
    public void RemoveAllListeners()
    {
        this.onPressed = null;
        this.onReleased = null;
    }

    private float GetValue()
    {
        var value = Vector3.Distance(_startPos, transform.localPosition) / _joint.linearLimit.limit;
        if (Mathf.Abs(value) < deadZone)
            value = 0;
        return Mathf.Clamp(value, -1f, 1f);
    }

    private void Pressed()
    {
        _isPressd = true;
        onPressed?.Invoke();
    }

    private void Released()
    {
        _isPressd = false;
        onReleased?.Invoke();
    }
}

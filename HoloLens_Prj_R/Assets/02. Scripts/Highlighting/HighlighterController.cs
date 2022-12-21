using UnityEngine;
using System.Collections;
using HighlightingSystem;
using UnityEngine.SceneManagement;

public class HighlighterController : MonoBehaviour
{
	public bool seeThrough = true;
	protected bool _seeThrough = true;

	public Highlighter h;
    Color mouseOverColor = Color.red;
    public void SetMouseOverColor(Color mouseOverColor)
    {
        this.mouseOverColor = mouseOverColor;
    }

    // 
    protected void Awake()
	{
		h = GetComponent<Highlighter>();
		if (h == null) { h = gameObject.AddComponent<Highlighter>(); }
	}

	// 
	void OnEnable()
	{
		_seeThrough = seeThrough;
		
		if (seeThrough) { h.SeeThroughOn(); }
		else { h.SeeThroughOff(); }
	}

    void OnDestroy()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;
        Destroy(h);
    }

    // 
    protected virtual void Start() { }

	// 
	protected void Update()
	{
		if (_seeThrough != seeThrough)
		{
			_seeThrough = seeThrough;
			if (_seeThrough) { h.SeeThroughOn(); }
			else { h.SeeThroughOff(); }
		}

		// Fade in/out constant highlighting with button '1'
		if (Input.GetKeyDown(KeyCode.Alpha1)) { h.ConstantSwitch(); }

		// Turn on/off constant highlighting with button '2'
		else if (Input.GetKeyDown(KeyCode.Alpha2)) { h.ConstantSwitchImmediate(); }
		
		// Turn off all highlighting modes with button '3'
		if (Input.GetKeyDown(KeyCode.Alpha3)) { h.Off(); }
	}

	// 
	public void MouseOver()
	{
        // Highlight object for one frame in case MouseOver event has arrived
        //h.On(mouseOverColor);
	}

	// 
	public void Fire1()
	{
		// Switch flashing
		h.FlashingSwitch();
	}

	// 
	public void Fire2()
	{
		// Stop flashing
		h.SeeThroughSwitch();
	}
}
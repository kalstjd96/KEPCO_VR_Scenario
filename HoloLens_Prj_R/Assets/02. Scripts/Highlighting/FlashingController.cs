using UnityEngine;
using System.Collections;

public class FlashingController : HighlighterController
{
	public Color flashingStartColor = Color.blue;
	public Color flashingEndColor = Color.cyan;
	public float flashingDelay = 0f;
	public float flashingFrequency = 0.5f;

	// 
	protected override void Start()
	{
		base.Start();

		StartCoroutine(DelayFlashing());
	}

	// 
	public IEnumerator DelayFlashing()
	{
		yield return new WaitForSeconds(flashingDelay);
		
		// Start object flashing after delay
		h.FlashingOn(flashingStartColor, flashingEndColor, flashingFrequency);
	}
}

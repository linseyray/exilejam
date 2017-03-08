using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour {

	[SerializeField] private float growthFactor = 0.1f;
	[SerializeField] private float fadeOutTime = 2.0f;

	private TrailRenderer trailRenderer;
	private int trailLength = 0;

	private float trailTimeBeforeFade = 0.0f;
	private bool fadingOut = false;
	private float t = 0.0f;

	private float trailEndWidth;
	private float trailStartWidth;


	void Start () {
		trailRenderer = GetComponent<TrailRenderer>();
		trailEndWidth = trailRenderer.endWidth;
		trailStartWidth = trailRenderer.startWidth;
	}
	
	void Update () {
		if (fadingOut) {
			if (trailRenderer.time > 0.0f) {
				t += Time.deltaTime / fadeOutTime;
				trailRenderer.time = Mathf.Lerp(trailTimeBeforeFade, 0.0f, t);
				trailRenderer.endWidth = Mathf.Lerp(trailEndWidth, 0.0f, t);
			}
			else
				fadingOut = false;
		}
	}

	public void GrowTrail() {
		trailRenderer.time += growthFactor;
		trailLength++;
	}

	public int TrailLength() {
		return trailLength;
	}

	public void FadeOutTrail() {
		fadingOut = true;
		trailTimeBeforeFade = trailRenderer.time;
	}
}

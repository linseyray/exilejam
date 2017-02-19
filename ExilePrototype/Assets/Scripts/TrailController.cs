using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour {

	private TrailRenderer trailRenderer;
	[SerializeField] private float growthFactor = 1.0f;


	// Use this for initialization
	void Start () {
		trailRenderer = GetComponent<TrailRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GrowTrail() {
		trailRenderer.time += growthFactor;
	}
}

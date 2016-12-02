using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceBehaviour : MonoBehaviour {

	private SpriteRenderer spriteRenderer;

	private bool fadeOut = false;
	private float fadeVelocity = 0.0f;
	private float fadeTime = 0.3f;

	void Awake() {
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	void Start () {
		
	}
	
	void Update () {
		if (fadeOut) {
			Color newColor = spriteRenderer.color;
			newColor.a = Mathf.SmoothDamp(newColor.a, 0.0f, ref fadeVelocity, fadeTime);
			spriteRenderer.color = newColor;
		}
		
	}

	public void Consume() {
		fadeOut = true;
	}
}

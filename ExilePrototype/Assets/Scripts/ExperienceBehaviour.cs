using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceBehaviour : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
	private ExperienceSpawner spawner;

	private bool fadeOut = false;
	private float fadeVelocity = 0.0f;
	private float fadeTime = 0.3f;

	private int identifier; 	// The spawn point this experience was spawned at


	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
		
	public void SetSpawner(ExperienceSpawner spawner, int identifier) {
		this.spawner = spawner;
		this.identifier = identifier;
	}

	void Start () {
		
	}
	
	void Update () {
		if (fadeOut) {
			Color newColor = spriteRenderer.color;
			newColor.a = Mathf.SmoothDamp(newColor.a, 0.0f, ref fadeVelocity, fadeTime);
			spriteRenderer.color = newColor;
			if (newColor.a <= 0.3f) {
				spawner.ExperienceCollected(identifier);
				Destroy(gameObject);
			}
		}
	}

	public void Consume() {
		fadeOut = true;
	}
}

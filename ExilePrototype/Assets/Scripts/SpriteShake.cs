using UnityEngine;
using System.Collections;

// USAGE:
// 	Apply this script to a sprite with a SpriteRenderer component. Enable/Disable to apply shake behaviour.
//
// NOTE:
// 	Make sure there is no RigidBody2D attached to this object or it will interfere with physics-based movement.
// 	e.g., have a Player object with a RigidBody2D and a child object with the SpriteRenderer component and this script.

public class SpriteShake : MonoBehaviour {

	public float shakeStrength = 0.085f;

	void Start () {
	}

	void FixedUpdate () {
		Shake();
	}

	void OnDisable() {
		// We reset the sprite's position to the parent object's position because shaking might stop 
		// when we've placed it off its centre
		gameObject.transform.localPosition = new Vector2(0,0); // gameObject.transform.parent.transform.position
	}

	// Shake the player sprite
	private void Shake() {
		// This moves the sprite to a random position within a circle of radius shakeStrength
		Vector2 shakePosition = Random.insideUnitCircle * shakeStrength;
		// The placement is applied to the child sprite instead of to the player object's RigidBody2D,
		// This way it doesn't interfere with physics moveement.
		gameObject.transform.localPosition = shakePosition;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour {

	[SerializeField] private SpriteRenderer positiveRenderer;
	[SerializeField] private SpriteRenderer negativeRenderer;
	[SerializeField] private float negativeSpriteScale = 1.3f;

	private SpriteShake spriteShakePositive;
	private SpriteShake spriteShakeNegative;

	private float maxBounds; // The max bounds for players' balance, used to calculate alpha of sprites

	void Awake() {
		spriteShakePositive = positiveRenderer.gameObject.GetComponent<SpriteShake>();
		spriteShakeNegative = negativeRenderer.gameObject.GetComponent<SpriteShake>();
		DisableSpriteShake();

		Vector3 newScale = negativeRenderer.transform.localScale;
		newScale.x = negativeSpriteScale;
		newScale.y = negativeSpriteScale;
		negativeRenderer.transform.localScale = newScale;
	}

	void Start () {
	}

	public void SetColor(Color colour) {			
		positiveRenderer.color = colour;
		negativeRenderer.color = colour;

		// Hide negative sprite
		Color newColor = colour;
		newColor.a = 0.0f;
		negativeRenderer.color = newColor;
	}

	/*********************************************************************************************************
	 			 								SHAPE CHANGING
	**********************************************************************************************************/
	
	public void UpdateSprite(float balance) {
		// Update opacity of sprites to make the illusion it's changing shape

		// For positive sprite: 
		// always visible since negative sprite is bigger and will cover it anyway, change if necessary

		// For negative sprite:
		// Balace: -maxBounds .... +maxbounds
		// Alpha:  1 .... 0
		if (balance <= 0.0f) {
			float targetAlpha = Mathf.Abs(balance / maxBounds);
			Color newColor = negativeRenderer.color;
			newColor.a = targetAlpha;
			negativeRenderer.color = newColor;
			Debug.Log(balance + " gives " + targetAlpha);
		}
	}

	public void SetBounds(float maxBounds) {
		this.maxBounds = maxBounds;
	}

	/*********************************************************************************************************
	 			 								SPRITE SHAKE
	**********************************************************************************************************/

	public void EnableSpriteShake() {
		spriteShakePositive.enabled = true;
		spriteShakeNegative.enabled = true;
	}

	public void DisableSpriteShake() {
		spriteShakePositive.enabled = false;
		spriteShakeNegative.enabled = false;
	}
}

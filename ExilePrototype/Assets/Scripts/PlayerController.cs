using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[SerializeField] private float speed;
	[SerializeField] private Player playerNumber;
	[SerializeField] private Color colorPlayer1;
	[SerializeField] private Color colorPlayer2;

	private enum Player { PLAYER_ONE, PLAYER_TWO };
	private string axisH;
	private string axisV;

	private Rigidbody2D rigidBody2D;
	private SpriteRenderer spriteRenderer;

	void Awake() {
		rigidBody2D = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void Start () {
		if (playerNumber == Player.PLAYER_ONE) {
			axisH = "P1_Horizontal";
			axisV = "P1_Vertical";
			spriteRenderer.color = colorPlayer1;

		}
		else {
			axisH = "P2_Horizontal";
			axisV = "P2_Vertical";
			spriteRenderer.color = colorPlayer2;
		}
	}
	
	void Update() {
		float moveHorizontal = Input.GetAxis(axisH);
		float moveVertical = Input.GetAxis(axisV);
		Vector2 inputDirection = new Vector2(moveHorizontal, moveVertical);
		rigidBody2D.AddForce(inputDirection * speed);
	}
}

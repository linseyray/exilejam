using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipDynamicsController : MonoBehaviour {

	[SerializeField] private GameObject player1;
	[SerializeField] private GameObject player2;

	private PlayerController player1Controller;
	private PlayerController player2Controller;
	private BoxCollider2D player1Collider;
	private BoxCollider2D player2Collider;
	private CircleCollider2D player1AreaCollider;
	private CircleCollider2D player2AreaCollider;

	void Awake() {
		player1Collider = player1.GetComponent<BoxCollider2D>();
		player2Collider = player2.GetComponent<BoxCollider2D>();
		player1AreaCollider = player1.GetComponentInChildren<CircleCollider2D>();
		player2AreaCollider = player2.GetComponentInChildren<CircleCollider2D>();
		player1Controller = player1.GetComponent<PlayerController>();
		player2Controller = player2.GetComponent<PlayerController>();
	}

	void Start () {
	}
	
	void Update () {
		// P2 entered P1's area
		if (!player1Controller.IsCloseToOtherPlayer() && player2Collider.IsTouching(player1AreaCollider))
			player1Controller.OnCloseToOtherPlayer();
		// P2 left P1's area
		if (player1Controller.IsCloseToOtherPlayer() && !player2Collider.IsTouching(player1AreaCollider))
			player1Controller.OnFarFromOtherPlayer();

		// P1 entered P2's area
		if (!player2Controller.IsCloseToOtherPlayer() && player1Collider.IsTouching(player2AreaCollider))
			player2Controller.OnCloseToOtherPlayer();
		// P1 left P2's area
		if (player2Controller.IsCloseToOtherPlayer() && !player1Collider.IsTouching(player2AreaCollider))
			player2Controller.OnFarFromOtherPlayer();
	}
}

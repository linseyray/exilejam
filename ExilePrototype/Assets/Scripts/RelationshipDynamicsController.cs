using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipDynamicsController : MonoBehaviour {

	[SerializeField] private GameObject player1;
	[SerializeField] private GameObject player2;

	[SerializeField] private AudioClip onCloseAudioClip;

	[SerializeField] private SpriteRenderer pathBlockerRenderer;
	[SerializeField] private Collider2D pathBlockerCollider;
	private float pathFadeSpeed = 0.1f;
	private bool hidingPath = false;
	private bool showingPath = false;


	private AudioSource onCloseAudioSource;
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
		onCloseAudioSource = GetComponent<AudioSource>();
	}

	void Start () {
	}
	
	void Update () {
		// P2 entered P1's area
		if (!player1Controller.IsCloseToOtherPlayer() && player2Collider.IsTouching(player1AreaCollider)) {
			player1Controller.OnCloseToOtherPlayer();

			// Play sound
			if (!onCloseAudioSource.isPlaying)
				onCloseAudioSource.PlayOneShot(onCloseAudioClip);
		}
		// P2 left P1's area
		if (player1Controller.IsCloseToOtherPlayer() && !player2Collider.IsTouching(player1AreaCollider)) {
			player1Controller.OnFarFromOtherPlayer();
		}

		// P1 entered P2's area
		if (!player2Controller.IsCloseToOtherPlayer() && player1Collider.IsTouching(player2AreaCollider)) {
			player2Controller.OnCloseToOtherPlayer();
			//if (!onCloseAudioSource.isPlaying)
				//onCloseAudioSource.PlayOneShot(onCloseAudioClip);
		}
		// P1 left P2's area
		if (player2Controller.IsCloseToOtherPlayer() && !player1Collider.IsTouching(player2AreaCollider))
			player2Controller.OnFarFromOtherPlayer();



		// Path blocker
		if ((player1Controller.IsCloseToOtherPlayer() || player2Controller.IsCloseToOtherPlayer()) && 
			(pathBlockerCollider.IsTouching(player1AreaCollider) || pathBlockerCollider.IsTouching(player2AreaCollider))) {
			ShowPath();
		} 
		else
		if (!showingPath && player1Controller.GetLocation() != PlayerController.PlayerLocation.ROOM3 && 
			player2Controller.GetLocation() != PlayerController.PlayerLocation.ROOM3) {
			HidePath();
		}

		if (showingPath) {
			Color newColor = pathBlockerRenderer.color;
			newColor.a -= pathFadeSpeed;
			pathBlockerRenderer.color = newColor;
			if (newColor.a <= 0.0f) {
				showingPath = false;
			}
		}

		if (hidingPath) {
			Color newColor = pathBlockerRenderer.color;
			newColor.a += pathFadeSpeed;
			pathBlockerRenderer.color = newColor;
			if (newColor.a >= 1.0f) {
				hidingPath = false;
			}
		}

	}

	private void ShowPath() {
		Debug.Log("ShowingPath");
		hidingPath = false;
		showingPath = true;
		pathBlockerCollider.enabled = false;
	}

	public void HidePath() {
		//if (player1Collider.IsTouching(pathBlockerCollider) || player2Collider.IsTouching(pathBlockerCollider)) 
		//	return;
		Debug.Log("HidingPath");
		hidingPath = true;
		showingPath = false;
		pathBlockerCollider.enabled = true;
	}
}

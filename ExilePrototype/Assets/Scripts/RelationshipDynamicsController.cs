using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipDynamicsController : MonoBehaviour {

	[SerializeField] private GameObject player1;
	[SerializeField] private GameObject player2;

	[SerializeField] private SpriteRenderer pathBlockerRenderer;
	[SerializeField] private Collider2D pathBlockerCollider;		// The actual collider that blocks players from passing
	[SerializeField] private Collider2D pathBlockerDetectionArea;	// Trigger: used to detect nearness of players
	private float pathFadeSpeed = 0.1f;
	private bool pathVisible = false; 		// true: path is fully visible, false: path is fully hidden
	private bool hidingPath = false;		// Whether we are decreasing path visibility
	private bool showingPath = false;		// Whether we are increasing path visibility

	[SerializeField] private AudioSource onCloseAudioSource;
	[SerializeField] private AudioSource onExperienceShareAudioSource;
	private PlayerController player1Controller;
	private PlayerController player2Controller;
	private BoxCollider2D player1Collider;
	private BoxCollider2D player2Collider;
	private CircleCollider2D player1AreaCollider;
	private CircleCollider2D player2AreaCollider;

	[SerializeField] private TrailController trailControllerP1;
	[SerializeField] private TrailController trailControllerP2;

	void Awake() {
		player1Collider = player1.GetComponent<BoxCollider2D>();
		player2Collider = player2.GetComponent<BoxCollider2D>();
		player1AreaCollider = player1.GetComponentInChildren<CircleCollider2D>();
		player2AreaCollider = player2.GetComponentInChildren<CircleCollider2D>();
		player1Controller = player1.GetComponent<PlayerController>();
		player2Controller = player2.GetComponent<PlayerController>();
	}

	void Start () {
		Screen.fullScreen = true;
	}
	
	void Update () {
		// It's okay to check from one player's perspective for as long as the areas are the same size
		// P2 entered P1's area
		if (!player1Controller.IsCloseToOtherPlayer() && player2Collider.IsTouching(player1AreaCollider)) {
			player1Controller.OnCloseToOtherPlayer();
			player2Controller.OnCloseToOtherPlayer();
			//onCloseAudioSource.PlayOneShot(onCloseAudioSource.clip);
			onCloseAudioSource.Play();
			ShareExperiences();
		}

		// P2 left P1's area
		if (player1Controller.IsCloseToOtherPlayer() && !player2Collider.IsTouching(player1AreaCollider)) {
			player1Controller.OnFarFromOtherPlayer();
			player2Controller.OnFarFromOtherPlayer();
		}

		UpdateSecretPath();

		if (Input.GetButtonDown("ToggleFullscreen"))
			Screen.fullScreen = !Screen.fullScreen;

	}

	private void UpdateSecretPath() {
		CheckPathAccessibility();
		UpdatePathVisibility();
	}

	private void CheckPathAccessibility() {
		if (!pathVisible && BothPlayersNearPath())
			ShowPath();
		else
		if (pathVisible && !BothPlayersNearPath() && NoPlayerIsOnPath() && NoPlayersInExternalRoom())
			HidePath();		
	}

	private void UpdatePathVisibility() {
		if (showingPath) {
			Color newColor = pathBlockerRenderer.color;
			newColor.a -= pathFadeSpeed;
			pathBlockerRenderer.color = newColor;
			Debug.Log(pathBlockerRenderer.color.a);
			if (newColor.a <= 0.0f) {
				pathVisible = true;
				showingPath = false;
				newColor.a = 0.0f;
				pathBlockerCollider.isTrigger = true; // Let players through
			}
		}

		if (hidingPath) {
			Color newColor = pathBlockerRenderer.color;
			newColor.a += pathFadeSpeed;
			pathBlockerRenderer.color = newColor;
			Debug.Log(pathBlockerRenderer.color.a);
			if (newColor.a >= 1.0f) {
				pathVisible = false;
				hidingPath = false;
				newColor.a = 1.0f;
				pathBlockerCollider.isTrigger = false; // Prevent players from moving through
			}
		}
	}

	private void ShowPath() {
		hidingPath = false;
		showingPath = true;
	}

	private void HidePath() {
		hidingPath = true;
		showingPath = false;
	}

	private bool BothPlayersNearPath() {
		return pathBlockerDetectionArea.IsTouching(player1AreaCollider) && 
			   pathBlockerDetectionArea.IsTouching(player2AreaCollider);
	}

	private bool NoPlayerIsOnPath() {
		return !pathBlockerCollider.OverlapPoint(player1Controller.transform.position) && 
			   !pathBlockerCollider.OverlapPoint(player2Controller.transform.position);
	}

	private bool NoPlayersInExternalRoom() {
		return player1Controller.GetLocation() != PlayerController.PlayerLocation.ROOM3 && 
			   player2Controller.GetLocation() != PlayerController.PlayerLocation.ROOM3;
	}

	private void ShareExperiences() {
		if (trailControllerP1.TrailLength() > 0 || trailControllerP2.TrailLength() > 0) {
			trailControllerP1.FadeOutTrail();
			trailControllerP2.FadeOutTrail();
			onExperienceShareAudioSource.Play();
		}
	}
}

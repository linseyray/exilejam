using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	/*********************************************************************************************************
	 			 							GENERAL VARIABLES
	**********************************************************************************************************/
	[SerializeField] private Player playerNumber;
	[SerializeField] private Color colorPlayer1;
	[SerializeField] private Color colorPlayer2;
	[SerializeField] private ParticleSystem aura;

	private enum Player { PLAYER_ONE, PLAYER_TWO };
	private string axisH;
	private string axisV;
	private Rigidbody2D rigidBody2D;
	private SpriteRenderer spriteRenderer;
	private CameraController cameraController;

	/*********************************************************************************************************
	 			 							PLAYER STATE VARIABLES
	**********************************************************************************************************/
	private float balance = 0.0f;			// in a range from -5 to +5 (0 is neutral)
	private float balanceMaxBound = 5.0f;
	public enum BalanceDirection { DRAINING_FACTOR = -1, RECHARGE_FACTOR = +1 };
	public BalanceDirection currentBalanceDirection;

	private bool isCloseToOtherPlayer = false;		// whether other player is in this player's connection area

	private float roomPresenceImpact = 0.05f;		// the speed at which balance changes based on room presence
	private float otherPlayerImpact = 0.5f;			// the amount with which the other player inclues the balance
	private float experienceImpact = 3.0f;			// the amount with which experiences influence balance

	/*********************************************************************************************************
	 			 								ROOM STATE
	**********************************************************************************************************/
	public enum PlayerLocation { CENTRAL_ROOM, ROOM1, ROOM2 };
	private PlayerLocation currentRoom;
	[SerializeField] float BALANCE_SHIFT_TIME;
	private float timeUntilBalanceShift;

	/*********************************************************************************************************
	 			 							VISUALISATION VARIABLES
	**********************************************************************************************************/
	// Nearness Aura
	[SerializeField] private float auraFadeRate = 5.0f;
	private float maxAuraEmissionRate;
	private ParticleSystem.EmissionModule emission;
	private ParticleSystem.MinMaxCurve emissionRateOverTime;
	private bool fadeInAura = false;
	private bool fadeOutAura = false;

	// Movement speed
	private float currentSpeed;
	[SerializeField] private float neutralSpeed;
	[SerializeField] private float minSpeed;
	[SerializeField] private float maxSpeed;

	/*********************************************************************************************************
	 			 								INITIALISATION
	**********************************************************************************************************/

	void Awake() {
		rigidBody2D = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		cameraController = GetComponentInChildren<CameraController>();
	}

	void Start () {
		// Set player-specific variables
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

		// Set visualisation variables
		emission = aura.emission;
		emissionRateOverTime = aura.emission.rateOverTime;
		maxAuraEmissionRate = emission.rateOverTime.constantMax;
		emissionRateOverTime.constantMax = 0.0f;
		emissionRateOverTime.constantMin = 0.0f;
		emissionRateOverTime.constant = 0.0f;
		emission.rateOverTime = emissionRateOverTime;
		emission = emission;

		// Setup emotional balance
		currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
		cameraController.InitialiseVariables(balanceMaxBound);
		currentSpeed = neutralSpeed;
		MoveToRoom(PlayerLocation.CENTRAL_ROOM);
	}

	/*********************************************************************************************************
	 			 								PRIVATE FUNCTIONS 
	**********************************************************************************************************/
	
	void Update() {
		Move();
		RoomPresenceEffect();
		UpdatePlayerVision();
		UpdatePlayerMovement();
		UpdateAura();

		// Change balance based on closeness to other player(?)
		// TODO maybe?
		if (isCloseToOtherPlayer) {
		}
		else {	
		}
	}

	private void Move() {
		float moveHorizontal = Input.GetAxis(axisH);
		float moveVertical = Input.GetAxis(axisV);
		Vector2 inputDirection = new Vector2(moveHorizontal, moveVertical);
		rigidBody2D.AddForce(inputDirection * currentSpeed);
	}

	private void RoomPresenceEffect() {
		// Balancing based on room presence
		if (timeUntilBalanceShift >= 0.0f) {
			balance += ((int) currentBalanceDirection) * roomPresenceImpact;
			balance = Mathf.Clamp(balance, -balanceMaxBound, balanceMaxBound);
			timeUntilBalanceShift -= Time.deltaTime;
			if (timeUntilBalanceShift <= 0.0f)
				currentBalanceDirection = BalanceDirection.DRAINING_FACTOR;
		}
	}

	private void UpdatePlayerVision() {
		// Slow  changes based on room presence
		if (currentBalanceDirection == BalanceDirection.DRAINING_FACTOR/* && balance <= 0.0f*/)
			cameraController.UpdatePlayerVision(BalanceDirection.DRAINING_FACTOR, roomPresenceImpact);
		if (currentBalanceDirection == BalanceDirection.RECHARGE_FACTOR/* && balance >= 0.0f*/)
			cameraController.UpdatePlayerVision(BalanceDirection.RECHARGE_FACTOR, roomPresenceImpact);
	}

	private void UpdatePlayerMovement() {
		if (balance < 0.0f) {
			if (currentBalanceDirection == BalanceDirection.DRAINING_FACTOR && currentSpeed > minSpeed)
				currentSpeed -= 0.1f;
			if (currentBalanceDirection == BalanceDirection.RECHARGE_FACTOR && currentSpeed < 0.0f)
				currentSpeed += 0.1f;
		}
		if (balance > 0.0f)
			currentSpeed = neutralSpeed;
				
		// sprite shake?
	}

	private void UpdateAura() {
		if (fadeInAura) {
			emissionRateOverTime.constant += auraFadeRate;
			if (emissionRateOverTime.constant >= maxAuraEmissionRate) {
				emissionRateOverTime = maxAuraEmissionRate;
				fadeInAura = false;
			}
		}

		if (fadeOutAura) {
			emissionRateOverTime.constant -= auraFadeRate;
			if (emissionRateOverTime.constant <= 0.0f) {
				emissionRateOverTime = 0.0f;
				fadeOutAura = false;
			}
		}

		// Update particle system
		emission.rateOverTime = emissionRateOverTime;
	}

	/*********************************************************************************************************
	 			 								PUBLIC FUNCTIONS
	**********************************************************************************************************/

	public void OnCloseToOtherPlayer() {
		this.isCloseToOtherPlayer = true;
		fadeInAura = true;
	}

	public void OnFarFromOtherPlayer() {
		this.isCloseToOtherPlayer = false;
		fadeOutAura = true;
	}

	public bool IsCloseToOtherPlayer() {
		return isCloseToOtherPlayer;
	}

	/*********************************************************************************************************
	 			 								COLLISION DETECTION
	**********************************************************************************************************/

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "PositiveExperience") {
			//GainPositiveExperience();
			collider.gameObject.GetComponent<ExperienceBehaviour>().Consume();
		}
		else
		if (collider.tag == "NegativeExperience") {
			//GainNegativeExperience();
			collider.gameObject.GetComponent<ExperienceBehaviour>().Consume();
		}
		
		if (collider.name == "Room1") {
			MoveToRoom(PlayerLocation.ROOM1);
		}
		else
		if (collider.name == "Room2") {
			MoveToRoom(PlayerLocation.ROOM2);
		}
		else
		if (collider.name == "RoomCentral") {
			MoveToRoom(PlayerLocation.CENTRAL_ROOM);
		}
	}

	/*
	private void GainPositiveExperience() {
		balance += experienceImpact;
		cameraController.UpdatePlayerVision(BalanceDirection.RECHARGE_FACTOR, experienceImpact);
		currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
	}

	private void GainNegativeExperience() {
		balance -= experienceImpact;
		cameraController.UpdatePlayerVision(BalanceDirection.DRAINING_FACTOR, experienceImpact);
		currentBalanceDirection = BalanceDirection.DRAINING_FACTOR;
	}
	*/

	private void MoveToRoom(PlayerLocation newRoom) {
		currentRoom = newRoom;
		currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
		timeUntilBalanceShift = BALANCE_SHIFT_TIME;
		Debug.Log("Moved to room " + currentRoom.ToString());
	}
}

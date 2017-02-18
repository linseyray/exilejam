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
	[SerializeField] private GameObject otherPlayer;
	[SerializeField] private ParticleSystem aura;
	[SerializeField] private SoundController soundController;
	[SerializeField] private RelationshipDynamicsController relationshipDynamicsController;


	private enum Player { PLAYER_ONE, PLAYER_TWO };
	private string axisH;
	private string axisV;
	private string magnetButton;
	private string magnetTrigger;
	private string magnetTriggerOther;
	private Rigidbody2D rigidBody2D;
	private SpriteRenderer spriteRenderer;
	private SpriteShake spriteShakeController;
	private CameraController cameraController;


	/*********************************************************************************************************
	 			 							PLAYER STATE VARIABLES
	**********************************************************************************************************/
	private float balance = 0.0f;			// in a range from -5 to +5 (0 is neutral)
	private float balanceMaxBound = 5.0f;
	public enum BalanceDirection { DRAINING_FACTOR = -1, RECHARGE_FACTOR = +1 };
	public BalanceDirection currentBalanceDirection;

	private bool isCloseToOtherPlayer = false;		// whether other player is in this player's connection area
	[SerializeField] private float magnetStrength;
	[SerializeField] private float magnetToleranceTime; 	// Time until one player starts shaking when other keeps 
															// seeking contact (activating magnet)
	private float timeTillMagnetToleranceBreak = 0.0f;		
	private bool breakingTolerance = false;

	private float roomPresenceImpact = 0.05f;		// the speed at which balance changes based on room presence
	private float otherPlayerImpact = 10f;			// the amount with which the other player influences the balance
	private float experienceImpact = 0.0f;			// the amount with which experiences influence balance



	/*********************************************************************************************************
	 			 								ROOM STATE
	**********************************************************************************************************/
	public enum PlayerLocation { CENTRAL_ROOM, ROOM1, ROOM2, ROOM3, UNDECIDED };
	private PlayerLocation currentRoom = PlayerLocation.UNDECIDED;
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
	 			 							REUNITING VARIABLES
	**********************************************************************************************************/
	private bool touchedSinceEntry = true; 		// whether the player touched the other player since entering the room (UGLY CODE)
	private bool reuniting = false;
	[SerializeField] private float reuniteEffectLingerTime;

	/*********************************************************************************************************
	 			 								INITIALISATION
	**********************************************************************************************************/

	void Awake() {
		rigidBody2D = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteShakeController = spriteRenderer.gameObject.GetComponent<SpriteShake>();
		spriteShakeController.enabled = false;
		cameraController = GetComponentInChildren<CameraController>();
	}

	void Start () {
		// Set player-specific variables
		if (playerNumber == Player.PLAYER_ONE) {
			axisH = "P1_Horizontal";
			axisV = "P1_Vertical";
			magnetButton = "P1_MagnetButton";
			magnetTrigger = "P1_MagnetTrigger";
			magnetTriggerOther = "P2_MagnetTrigger";
			spriteRenderer.color = colorPlayer1;

		}
		else {
			axisH = "P2_Horizontal";
			axisV = "P2_Vertical";
			magnetButton = "P2_MagnetButton";
			magnetTrigger = "P2_MagnetTrigger";
			magnetTriggerOther = "P1_MagnetTrigger";
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
	}

	/*********************************************************************************************************
	 			 								PRIVATE FUNCTIONS 
	**********************************************************************************************************/
	
	void Update() {
		Move();
		RoomPresenceEffect();
		UpdatePlayerVision();
		UpdatePlayerMovement();
		MagnetMechanic();
		UpdateAura();
	}

	private void Move() {
		float moveHorizontal = Input.GetAxis(axisH);
		float moveVertical = Input.GetAxis(axisV);
		Vector2 inputDirection = new Vector2(moveHorizontal, moveVertical);
		rigidBody2D.AddForce(inputDirection * currentSpeed);
	}

	private void RoomPresenceEffect() {
		//if (reuniting)
		//	return;

		// Update balance
		balance += ((int) currentBalanceDirection) * roomPresenceImpact;
		balance = Mathf.Clamp(balance, -balanceMaxBound, balanceMaxBound);
		//Debug.Log(playerNumber + " balance: " + balance);

		// Balance shift after some time in room
		if (timeUntilBalanceShift >= 0.0f) {
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

	private void MagnetMechanic() {

		if (isCloseToOtherPlayer) {
			float triggerAxis = MapAxisValue(Input.GetAxis(magnetTrigger));
			if (triggerAxis >= 1)  {
				Debug.Log("Magnet trigger held down");
				fadeInAura = true; // Activates aura
				AddMagnetForce();
			}
			else
				fadeOutAura = true;

			if (MapAxisValue(Input.GetAxis(magnetTrigger)) >= 1 && 
				MapAxisValue(Input.GetAxis(magnetTriggerOther)) >= 1) {
				// Double the magnet force, because both want to seek contact
				AddMagnetForce();
				spriteShakeController.enabled = false;
				breakingTolerance = false;
			}
			else
			if (MapAxisValue(Input.GetAxis(magnetTrigger)) < 1 &&
				MapAxisValue(Input.GetAxis(magnetTriggerOther)) >= 1) {
					if (!breakingTolerance) {
						breakingTolerance = true;
						timeTillMagnetToleranceBreak = magnetToleranceTime;
					}
					timeTillMagnetToleranceBreak -= Time.deltaTime;
					if (timeTillMagnetToleranceBreak <= 0.0f)
						spriteShakeController.enabled = true;
			}
			else {
				spriteShakeController.enabled = false;
				breakingTolerance = false;
			}
		}
		else {
			breakingTolerance = false;
			spriteShakeController.enabled = false;
		}
	}

	private void AddMagnetForce() {
		// Add small force toward other player
		Vector2 targetPoint = otherPlayer.transform.position;
		Vector2 currentPosition = transform.position;
		Vector2 forceDirection = targetPoint - currentPosition;
		rigidBody2D.AddForce(forceDirection * magnetStrength);
	}

	private float MapAxisValue(float axisValue) {
		return (axisValue + 1f) / 2.0f;
	}

	/*********************************************************************************************************
	 			 								PUBLIC FUNCTIONS
	**********************************************************************************************************/

	public void OnCloseToOtherPlayer() {
		this.isCloseToOtherPlayer = true;

		// Flash effect 
		cameraController.FlashEffect();

		// Reunite effect
		if (!touchedSinceEntry) {
			touchedSinceEntry = true;
			//Debug.Log(playerNumber + " reunites");
			cameraController.AddPlayerVisionImpulse(otherPlayerImpact);
			balance += otherPlayerImpact;
			balance = Mathf.Clamp(balance, -balanceMaxBound, balanceMaxBound);
			currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
			timeUntilBalanceShift = BALANCE_SHIFT_TIME;
			reuniting = true;
			soundController.PlayReuniteSound();
		}
	}

	public void OnFarFromOtherPlayer() {
		this.isCloseToOtherPlayer = false;
		fadeOutAura = true;
	}

	public bool IsCloseToOtherPlayer() {
		return isCloseToOtherPlayer;
	}

	public void OnReuniteEffectEnded() {
		Invoke("StopImpulse", reuniteEffectLingerTime);
	}

	private void StopImpulse() {
		cameraController.StopImpulse();
		reuniting = false;
		currentBalanceDirection = BalanceDirection.DRAINING_FACTOR;
	}

	public float GetBalance() {
		return balance;
	}

	public float GetBalanceMaxBound() {
		return balanceMaxBound;
	}



	/*********************************************************************************************************
	 			 								COLLISION DETECTION
	**********************************************************************************************************/

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "PositiveExperience") {
			GainPositiveExperience();
			collider.gameObject.GetComponent<ExperienceBehaviour>().Consume();
		}
		else
		if (collider.tag == "NegativeExperience") {
			GainNegativeExperience();
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
		else
		if (collider.name == "Room3") {
			MoveToRoom(PlayerLocation.ROOM3);				
		}
	}

	public PlayerLocation GetLocation() {
		return currentRoom;
	}


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


	private void MoveToRoom(PlayerLocation newRoom) {
		if (newRoom == currentRoom)
			return;
		
		currentRoom = newRoom;
		touchedSinceEntry = false;
		otherPlayer.GetComponent<PlayerController>().touchedSinceEntry = false;
		Debug.Log(playerNumber + " moved to room " + currentRoom.ToString());

		// Recharge when entering a room 
		if (currentRoom != PlayerLocation.CENTRAL_ROOM) {
			currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
			timeUntilBalanceShift = BALANCE_SHIFT_TIME;
		}
	}
}

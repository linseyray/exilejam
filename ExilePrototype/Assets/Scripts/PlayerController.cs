using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	/*********************************************************************************************************
	 			 							GENERAL VARIABLES
	**********************************************************************************************************/
	[Header("Player Attributes")]
	[SerializeField] private Player playerNumber;
	[SerializeField] private Color colorPlayer1;
	[SerializeField] private Color colorPlayer2;

	private enum Player { PLAYER_ONE, PLAYER_TWO };
	private string axisH;
	private string axisV;
	private string magnetButton;
	private string magnetButtonOther;
	private string magnetTrigger;
	private string magnetTriggerOther;


	/*********************************************************************************************************
	 			 							PLAYER STATE VARIABLES
	**********************************************************************************************************/
	private float balance = 0.0f;			// in a range from -5 to +5 (0 is neutral)
	private float balanceMaxBound = 5.0f;
	public enum BalanceDirection { DRAINING_FACTOR = -1, RECHARGE_FACTOR = +1 };
	public BalanceDirection currentBalanceDirection;

	[Header("Magnet")]
	[SerializeField] private float magnetStrengthPositiveBalance;
	[SerializeField] private float magnetStrengthNegativeBalance;
	[SerializeField] private float magnetToleranceTime; 	// Time until one player starts shaking when other keeps 
	// seeking contact (activating magnet)
	private bool isCloseToOtherPlayer = false;		// whether other player is in this player's connection area
	private float timeTillMagnetToleranceBreak = 0.0f;		
	private bool breakingTolerance = false;
	private bool bothSeekingContact = false;

	private float roomPresenceImpact = 0.05f;		// the speed at which balance changes based on room presence
	private float otherPlayerImpact = 10f;			// the amount with which the other player influences the balance
	private float experienceImpact = 0.0f;			// the amount with which experiences influence balance


	/*********************************************************************************************************
	 			 								ROOM STATE
	**********************************************************************************************************/
	public enum PlayerLocation { CENTRAL_ROOM, ROOM1, ROOM2, ROOM3, UNDECIDED };
	private PlayerLocation currentRoom = PlayerLocation.UNDECIDED;

	[Header("Balance Shifting")]
	[SerializeField] float BALANCE_SHIFT_TIME;		// After this time balance is shifted from draining to recharing or vice versa,
	private bool shiftedSinceAction = false;		// (but only once after players make the balance shift by an action)
	private float timeUntilBalanceShift;			// Countdown from BALANCE_SHIFT_TIME
													// Reset by 1) reaching 0.0, 2) reunite effect, 3) moving to room

	/*********************************************************************************************************
	 			 							VISUALISATION VARIABLES
	**********************************************************************************************************/
	[Header("Aura")]
	// Nearness Aura
	[SerializeField] private float auraFadeRate = 2.5f;
	[SerializeField] private float weakenAuraStrength = 10.0f;
	private float maxAuraEmissionRate;
	private ParticleSystem.EmissionModule emission;
	private ParticleSystem.MinMaxCurve emissionRateOverTime;
	private bool fadeInAura = false;
	private bool fadeOutAura = false;

	[Header("Movement Speed")]
	[SerializeField] private float neutralSpeed;
	[SerializeField] private float minSpeed;
	[SerializeField] private float maxSpeed;
	private float currentSpeed;


	/*********************************************************************************************************
	 			 							REUNITING VARIABLES
	**********************************************************************************************************/
	[Header("Reunite effect")]
	[SerializeField] private float reuniteEffectLingerTime;
	private bool touchedSinceEntry = true; 		// whether the player touched the other player since entering the room (UGLY CODE)
	private bool reuniting = false;

	/*********************************************************************************************************
	 			 								EXPERIENCE VARIABLES
	**********************************************************************************************************/
	[Header("Trail")]
	[SerializeField] private int maxTrailLength = 5;
	private TrailController trailController;


	[Header("References")]
	[SerializeField] private GameObject otherPlayer;
	[SerializeField] private ParticleSystem aura;
	[SerializeField] private SoundController soundController;
	[SerializeField] private RelationshipDynamicsController relationshipDynamicsController;
	[SerializeField] private AudioSource experienceCollectAudioSource;
	[SerializeField] private ExperienceSpawner room1ExperienceSpawner;
	[SerializeField] private ExperienceSpawner room2ExperienceSpawner;
	[SerializeField] private ExperienceSpawner room3ExperienceSpawner;
	private Rigidbody2D rigidBody2D;
	private SpriteRenderer spriteRenderer;
	private SpriteController spriteController;
	private CameraController cameraController;


	/*********************************************************************************************************
	 			 								INITIALISATION
	**********************************************************************************************************/

	void Awake() {
		rigidBody2D = GetComponent<Rigidbody2D>();
		spriteController = GetComponent<SpriteController>();
		cameraController = GetComponentInChildren<CameraController>();
		trailController = GetComponentInChildren<TrailController>();
	}

	void Start () {
		// Set player-specific variables
		if (playerNumber == Player.PLAYER_ONE) {
			axisH = "P1_Horizontal";
			axisV = "P1_Vertical";
			magnetButton = "P1_MagnetButton";
			magnetButtonOther = "P2_MagnetButton";
			magnetTrigger = "P1_MagnetTrigger";
			magnetTriggerOther = "P2_MagnetTrigger";
			spriteController.SetColor(colorPlayer1);
		}
		else {
			axisH = "P2_Horizontal";
			axisV = "P2_Vertical";
			magnetButton = "P2_MagnetButton";
			magnetButtonOther = "P1_MagnetButton";
			magnetTrigger = "P2_MagnetTrigger";
			magnetTriggerOther = "P1_MagnetTrigger";
			spriteController.SetColor(colorPlayer2);
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
		timeUntilBalanceShift = BALANCE_SHIFT_TIME;
		currentSpeed = neutralSpeed;
		spriteController.SetBounds(balanceMaxBound);
	}

	/*********************************************************************************************************
	 			 								PRIVATE FUNCTIONS 
	**********************************************************************************************************/
	
	void Update() {
		Move();
		UpdateBalance();
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

	// Called each frame, ensures balance gets shifted after BALANCE_SHIFT_TIME 
	private void UpdateBalance() {
		AddToBalance(((int) currentBalanceDirection) * roomPresenceImpact);

		if (currentRoom != PlayerLocation.CENTRAL_ROOM) {
			// Always shift the balance to negative after BALANCE_SHIFT_TIME time 
			if (timeUntilBalanceShift > 0.0f)
				timeUntilBalanceShift -= Time.deltaTime;
			else {
				// Shift the balance to negative
				currentBalanceDirection = BalanceDirection.DRAINING_FACTOR;
				timeUntilBalanceShift = BALANCE_SHIFT_TIME;
				Debug.Log("Balance Shifted");
			}
		}
		else 
		if (currentRoom == PlayerLocation.CENTRAL_ROOM) {
			if (shiftedSinceAction)
				// Only shift balance if we haven't already shifted it 
				return;

			// Balance shift after some time
			if (timeUntilBalanceShift > 0.0f)
				timeUntilBalanceShift -= Time.deltaTime;
			else {
				// Shift the balance
				if (currentBalanceDirection == BalanceDirection.DRAINING_FACTOR)
					currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
				else
					currentBalanceDirection = BalanceDirection.DRAINING_FACTOR;
				timeUntilBalanceShift = BALANCE_SHIFT_TIME;
				Debug.Log("Balance Shifted");
				shiftedSinceAction = true;
			}
		}
	}
		
	private void AddToBalance(float amount) {
		balance += amount;
		balance = Mathf.Clamp(balance, -balanceMaxBound, balanceMaxBound);
		spriteController.UpdateSprite(balance);
		//Debug.Log(playerNumber + " balance: " + balance);
	}

	private void UpdatePlayerVision() {
		// Slow changes based on room presence
		cameraController.UpdatePlayerVision(currentBalanceDirection, roomPresenceImpact);

	}

	private void UpdatePlayerMovement() {
		if (balance < 0.0f) {
			if (currentBalanceDirection == BalanceDirection.DRAINING_FACTOR && currentSpeed > minSpeed) 
				currentSpeed -= 0.1f;
			else
			if (currentBalanceDirection == BalanceDirection.RECHARGE_FACTOR && currentSpeed < 0.0f)
				currentSpeed += 0.1f;
		}
		else
		if (balance > 0.0f) {
			if (bothSeekingContact)
				currentSpeed = maxSpeed;
			else
				currentSpeed = neutralSpeed;
		}
	}

	private void UpdateAura() {
		if (fadeInAura) {
			emissionRateOverTime.constant += auraFadeRate;
			float maxRate = isCloseToOtherPlayer ? maxAuraEmissionRate : maxAuraEmissionRate / weakenAuraStrength;
			if (emissionRateOverTime.constant >= maxRate) {
				emissionRateOverTime = maxRate;
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
			float triggerAxisOther = MapAxisValue(Input.GetAxis(magnetTriggerOther));
			bool seekingContact = triggerAxis >= 1 || Input.GetButton(magnetButton);
			bool otherSeeksContact = triggerAxisOther >= 1 || Input.GetButton(magnetButtonOther);
			bothSeekingContact = seekingContact && otherSeeksContact;

			// HACK: aura independent of magnet button (always when close)
			// keep magnet force dependent on magnet button
			fadeInAura = true;
			AddMagnetForce();

			// Are we seeking contact?
			if (seekingContact)  {
				//fadeInAura = true; // Activate aura
				AddMagnetForce();
			}
			else {
				//fadeInAura = false;
				//fadeOutAura = true;
			}

			// Is the other seeking contact?
			if (otherSeeksContact) {
				if (seekingContact) {
					// Both want to seek contact 
					AddMagnetForce(); // double the magnet force
					ResetTolerance();	
				}
				else {
					// The other is seeking contact, but we're not 
					if (!breakingTolerance) {
						// Start the countdown until shake
						breakingTolerance = true;
						timeTillMagnetToleranceBreak = magnetToleranceTime;
					}
					timeTillMagnetToleranceBreak -= Time.deltaTime;
					if (timeTillMagnetToleranceBreak <= 0.0f)
						// Shake
						spriteController.EnableSpriteShake();
				}
			}
			else
				// We're close but the other isn't seeking contact
				ResetTolerance();
		}
		else {
			// We're out of range
			ResetTolerance();
			// But still allow for calling
			float triggerAxis = MapAxisValue(Input.GetAxis(magnetTrigger));
			bool seekingContact = triggerAxis >= 1 || Input.GetButton(magnetButton);
			if (seekingContact) {
				fadeInAura = true; // Activate aura
				fadeOutAura = false;
			}
			else {
				fadeInAura = false;
				fadeOutAura = true;
			}
		}
	}

	private void ResetTolerance() {
		// Stop shaking and stop countdown to shake
		spriteController.DisableSpriteShake();
		breakingTolerance = false;
	}

	private void AddMagnetForce() {
		// Add small force toward other player
		Vector2 targetPoint = otherPlayer.transform.position;
		Vector2 currentPosition = transform.position;
		Vector2 forceDirection = targetPoint - currentPosition;

		if (balance <= 0.0f)
			rigidBody2D.AddForce(forceDirection * magnetStrengthNegativeBalance);
		else
			rigidBody2D.AddForce(forceDirection * magnetStrengthPositiveBalance);
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
			AddToBalance(otherPlayerImpact);
			currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
			timeUntilBalanceShift = BALANCE_SHIFT_TIME;
			shiftedSinceAction = false;
			reuniting = true;
			soundController.PlayReuniteSound();
		}
	}

	public void OnFarFromOtherPlayer() {
		this.isCloseToOtherPlayer = false;
		fadeInAura = false;
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
		else
		if (collider.name == "Room3") {
			MoveToRoom(PlayerLocation.ROOM3);				
		}
	}

	public PlayerLocation GetLocation() {
		return currentRoom;
	}


	private void GainPositiveExperience() {
		if (trailController.TrailLength() >= maxTrailLength)
			// Only allow experiences to impact balance up until a certain point
			return;
			
		currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
		AddToBalance(experienceImpact);
		cameraController.UpdatePlayerVision(currentBalanceDirection, experienceImpact);
		//if (!experienceCollectAudioSource.isPlaying)
		//experienceCollectAudioSource.Play();
		trailController.GrowTrail();
	}

	private void MoveToRoom(PlayerLocation newRoom) {
		if (newRoom == currentRoom)
			return;
		PlayerController otherPlayerController = otherPlayer.GetComponent<PlayerController>();
		
		currentRoom = newRoom;
		touchedSinceEntry = false;
		otherPlayerController.touchedSinceEntry = false;
		//Debug.Log(playerNumber + " moved to room " + currentRoom.ToString());

		if (currentRoom != PlayerLocation.CENTRAL_ROOM) {
			// Recharge when entering a room 
			currentBalanceDirection = BalanceDirection.RECHARGE_FACTOR;
			timeUntilBalanceShift = BALANCE_SHIFT_TIME;
			shiftedSinceAction = false;
			EnableDarkVignette();	// Increased vignette effect when in another room
			otherPlayerController.EnableDarkVignette();

			if (currentRoom == PlayerLocation.ROOM1)
				room1ExperienceSpawner.ResetSpawnDelay();
			if (currentRoom == PlayerLocation.ROOM2)
				room2ExperienceSpawner.ResetSpawnDelay();
			if (currentRoom == PlayerLocation.ROOM3)
				room3ExperienceSpawner.ResetSpawnDelay();
		}
		else 
		if (otherPlayerController.GetLocation() == PlayerLocation.CENTRAL_ROOM) {
			DisableAloneVignette(); // Don't use increased vignette effect in central room
			otherPlayerController.DisableAloneVignette();
		}
	}

	public void EnableDarkVignette() {
		cameraController.EnableDarkVignette();
	}

	public void DisableAloneVignette() {
		cameraController.DisableDarkVignette();
	}
}

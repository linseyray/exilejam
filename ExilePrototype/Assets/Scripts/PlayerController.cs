using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	/*********************************************************************************************************
	 			 							GENERAL VARIABLES
	**********************************************************************************************************/
	[SerializeField] private float speed;
	[SerializeField] private Player playerNumber;
	[SerializeField] private Color colorPlayer1;
	[SerializeField] private Color colorPlayer2;
	[SerializeField] private ParticleSystem aura;

	private enum Player { PLAYER_ONE, PLAYER_TWO };
	private string axisH;
	private string axisV;
	private Rigidbody2D rigidBody2D;
	private SpriteRenderer spriteRenderer;

	/*********************************************************************************************************
	 			 							PLAYER STATE VARIABLES
	**********************************************************************************************************/
	private float emotionalBalance = 0.0f;			// in a range from -5 to +5 (0 is neutral)
	private bool isCloseToOtherPlayer = false;		// whether other player is in this player's connection area
	private float automaticRecoveryFactor = 0.25f;	// the speed at which emotional balance recovers on its own
	private float experienceImpact = 1.0f;			// the amount with which experiences influence emotionalBalance

	/*********************************************************************************************************
	 			 							VISUALISATION VARIABLES
	**********************************************************************************************************/
	[SerializeField] private float auraFadeRate = 5.0f;
	private float maxAuraEmissionRate;
	private ParticleSystem.EmissionModule emission;
	private ParticleSystem.MinMaxCurve emissionRateOverTime;
	private bool fadeInAura = false;
	private bool fadeOutAura = false;

	/*********************************************************************************************************
	 			 								INITIALISATION
	**********************************************************************************************************/

	void Awake() {
		rigidBody2D = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
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


	}

	/*********************************************************************************************************
	 			 								PRIVATE FUNCTIONS 
	**********************************************************************************************************/
	
	void Update() {
		Move();
		UpdateEmotionalBalance();
		UpdateAura();
	}

	private void Move() {
		float moveHorizontal = Input.GetAxis(axisH);
		float moveVertical = Input.GetAxis(axisV);
		Vector2 inputDirection = new Vector2(moveHorizontal, moveVertical);
		rigidBody2D.AddForce(inputDirection * speed);
	}

	private void UpdateEmotionalBalance() {
		// Change emotionalBalance based on closeness
		if (isCloseToOtherPlayer) {
		}
		else {
		}
	}

	private void UpdateAura() {
		if (fadeInAura) {
			emissionRateOverTime.constant += auraFadeRate;
			if (emissionRateOverTime.constant >= maxAuraEmissionRate)
				fadeInAura = false;
		}

		if (fadeOutAura) {
			emissionRateOverTime.constant -= auraFadeRate;
			if (emissionRateOverTime.constant <= 0.0f)
				fadeOutAura = false;
		}

		// Update particle system
		emission.rateOverTime = emissionRateOverTime;
	}

	/*********************************************************************************************************
	 			 								PUBLIC FUNCTIONS
	**********************************************************************************************************/

	public void GainPositiveExperience() {
	}

	public void GainNegativeExperience() {
	}

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
}

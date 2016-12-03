using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

	[SerializeField] private PlayerController player1Controller;
	[SerializeField] private PlayerController player2Controller;
	[SerializeField] private AudioSource reuniteAudioSource;


	[SerializeField] private AudioSource positiveStateSource;
	[SerializeField] private AudioSource negativeStateSource;

	[SerializeField] private float minPositiveStateVolume = 0.3f;

	private float minBalanceTotal;
	private float maxBalanceTotal;

	void Start () {		
		float balanceBounds = player1Controller.GetBalanceMaxBound();
		minBalanceTotal = -balanceBounds * 2;
		maxBalanceTotal = balanceBounds * 2;

		positiveStateSource.volume = 0;
		positiveStateSource.volume = 0;
	}
	
	void Update () {
		float player1Balance = player1Controller.GetBalance();
		float player2Balance = player2Controller.GetBalance();
		float totalBalance = player1Controller.GetBalance() + player2Controller.GetBalance();

		if (player1Balance >= 0.0f && player2Balance >= 0.0f) {
			// Both players are balanced
			positiveStateSource.volume = (player1Balance + player2Balance) / 10f;
			negativeStateSource.volume = 0.0f;
		}
		else
		if (player1Balance < 0.0f && player2Balance < 0.0f) {
			// Both players are unbalanced
			positiveStateSource.volume = 0.3f;
			negativeStateSource.volume = (Mathf.Abs(player1Balance) + Mathf.Abs(player2Balance)) / 10f;
		}
		else {
			positiveStateSource.volume = Mathf.Clamp(player1Balance + player2Balance / 10f, minPositiveStateVolume, 1);
				negativeStateSource.volume = (player1Balance + player2Balance) / 10f;
		}
	}

	public void PlayReuniteSound() {
		if (!reuniteAudioSource.isPlaying)
			reuniteAudioSource.Play();
	}
}

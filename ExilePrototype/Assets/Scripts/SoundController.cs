using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

	[SerializeField] private PlayerController player1Controller;
	[SerializeField] private PlayerController player2Controller;
	[SerializeField] private AudioSource reuniteAudioSource;


	[SerializeField] AudioSource positiveStateSource;
	[SerializeField] AudioSource negativeStateSource;

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
		Debug.Log("player1 balance: " + player1Controller.GetBalance() + " player2 balance: " + player1Controller.GetBalance());
		Debug.Log(totalBalance);

		if (player1Balance >= 0.0f && player2Balance >= 0.0f) {
			// Both players are balanced
			positiveStateSource.volume = Mathf.Max(player1Balance, player2Balance) / 5f;
			negativeStateSource.volume = 0.0f;
		}
		else
		if (player1Balance <= 0.0f && player2Balance <= 0.0f) {
			// Both players are unbalanced
			positiveStateSource.volume = 0.3f;
			negativeStateSource.volume = Mathf.Abs(Mathf.Min(player1Balance, player2Balance)) / 5f;
		}
		else {
			positiveStateSource.volume = Mathf.Max(player1Balance, player2Balance) / 5f;
			negativeStateSource.volume = Mathf.Abs(Mathf.Min(player1Balance, player2Balance)) / 5f;
		}
	}

	public void PlayReuniteSound() {
		if (!reuniteAudioSource.isPlaying)
			reuniteAudioSource.Play();
	}
}

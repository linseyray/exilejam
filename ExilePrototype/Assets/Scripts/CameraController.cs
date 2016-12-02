using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraController : MonoBehaviour {

	private VignetteAndChromaticAberration vignetteFilter;
	[SerializeField] float maxVignetteIntensity = 0.2f;
	[SerializeField] float vignetteSpeed = 0.01f;

	private NoiseAndScratches noiseFilter;
	[SerializeField] float maxNoiseGrainIntensity;
	[SerializeField] float noiseSpeed = 0.01f;

	private CameraMotionBlur blurFilter;
	[SerializeField] float maxBlurVeloctyScale;
	[SerializeField] float blurSpeed;

	private Bloom bloomFilter;
	[SerializeField] float maxBloomIntensity;
	[SerializeField] float bloomSpeed;

	private float maxBounds;

	void Awake() {
		vignetteFilter = gameObject.GetComponent<VignetteAndChromaticAberration>();
		noiseFilter = gameObject.GetComponent<NoiseAndScratches>();
		blurFilter = gameObject.GetComponent<CameraMotionBlur>();
		bloomFilter = gameObject.GetComponent<Bloom>();
	}

	void Start () {
		
	}

	public void InitialiseVariables(float maxBounds) {
	}
	
	void Update () {
	}

	public void UpdatePlayerVision(PlayerController.BalanceDirection direction, float changeAmount) { 
		if (direction == PlayerController.BalanceDirection.RECHARGE_FACTOR)
			Recharge(changeAmount);
		else
		if (direction == PlayerController.BalanceDirection.DRAINING_FACTOR) 
			Drain(changeAmount);
	}


	private void Recharge(float changeAmount) {
		Debug.Log("recharging");

		// Decrease vignette
		if (vignetteFilter.intensity >= 0.0f) 
			vignetteFilter.intensity -= changeAmount * vignetteSpeed;

		// Decrease noise
		if (noiseFilter.grainIntensityMax >= 0.0f) {
			noiseFilter.grainIntensityMax -= changeAmount * noiseSpeed;
			noiseFilter.grainIntensityMin = noiseFilter.grainIntensityMax;
		}

		// Decrease blur
		if (blurFilter.velocityScale >= 0.0f)
			blurFilter.velocityScale -= changeAmount * blurSpeed;

		// Increase Bloom
		if (bloomFilter.bloomIntensity <= maxBloomIntensity)
			bloomFilter.bloomIntensity += changeAmount * bloomSpeed;
	}

	private void Drain(float changeAmount) {
		Debug.Log("draining");

		// Increase vignette
		if (vignetteFilter.intensity <= maxVignetteIntensity) 
			vignetteFilter.intensity += changeAmount * vignetteSpeed;

		// Increase noise
		if (noiseFilter.grainIntensityMax <= maxNoiseGrainIntensity) {
			noiseFilter.grainIntensityMax += changeAmount * noiseSpeed;
			noiseFilter.grainIntensityMin = noiseFilter.grainIntensityMax;
		}

		// Increase blur
		if (blurFilter.velocityScale <= maxBlurVeloctyScale)
			blurFilter.velocityScale += blurSpeed;

		// Decrease bloom
		if (bloomFilter.bloomIntensity >= 0.0f)
			bloomFilter.bloomIntensity -= changeAmount * bloomSpeed;
		else
			bloomFilter.bloomIntensity = 0.0f;
	}
}

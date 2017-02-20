using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraController : MonoBehaviour {

	// Impulse
	[SerializeField] float impulseSpeed = 0.5f;
	private bool addingImpulse = false;
	private float impulseToApply;

	// Flash
	private float bloomVelocity = 0.0f;
	private float vignetteVelocity = 0.0f;
	private float noiseVelocity = 0.0f;
	[SerializeField] private float flashTime = 1f;
	[SerializeField] private float implodingFlashTime = 1f;
	private bool flashing = false;
	private bool implodingFlash = false;
	private float flashBloomIntensity = 0.01f;
	private float flashEndNoiseIntensity;
	private float flashEndBloomIntensity;
	private float flashEndVignetteIntensity;

	// Dark vignette
	private bool disablingDarkVignette = false;
	private float disableSpeed = 0.05f;

	private VignetteAndChromaticAberration vignetteFilter;
	[SerializeField] float darkVignetteIntensity = 0.7f;
	[SerializeField] float lightVignetteIntensity = 0.2f;
	float maxVignetteIntensity = 0.2f;
	[SerializeField] float darkVignetteSpeed = 0.05f;
	[SerializeField] float lightVignetteSpeed = 0.01f;
	private float vignetteSpeed;

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
		maxVignetteIntensity = lightVignetteIntensity;
		vignetteSpeed = lightVignetteSpeed;
	}

	void Start () {
	}

	public void InitialiseVariables(float maxBounds) {
	}
	
	void Update () {
		if (addingImpulse) {
			Recharge(impulseSpeed);
			impulseToApply -= impulseSpeed;
			if (impulseToApply <= 0.0f) {
				transform.parent.GetComponent<PlayerController>().OnReuniteEffectEnded();
			}
		}

		if (flashing) {
			//vignetteFilter.intensity = 0.0f;
			//noiseFilter.grainIntensityMax = 0.0f;
			//bloomFilter.bloomIntensity = maxBloomIntensity / 2;
			bloomFilter.bloomIntensity = Mathf.SmoothDamp(bloomFilter.bloomIntensity, Mathf.Max(flashBloomIntensity, bloomFilter.bloomIntensity), ref bloomVelocity, flashTime);
			vignetteFilter.intensity = Mathf.SmoothDamp(vignetteFilter.intensity, 0.0f, ref vignetteVelocity, flashTime);
			noiseFilter.grainIntensityMax = Mathf.SmoothDamp(noiseFilter.grainIntensityMax, 0.0f, ref noiseVelocity, flashTime);
			noiseFilter.grainIntensityMin = noiseFilter.grainIntensityMax;
		}
		if (implodingFlash) {
			noiseFilter.grainIntensityMax = Mathf.SmoothDamp(noiseFilter.grainIntensityMax, flashEndNoiseIntensity, ref noiseVelocity, implodingFlashTime);
			noiseFilter.grainIntensityMin = noiseFilter.grainIntensityMax;
			bloomFilter.bloomIntensity = Mathf.SmoothDamp(bloomFilter.bloomIntensity, flashEndBloomIntensity, ref bloomVelocity, implodingFlashTime);
			vignetteFilter.intensity = Mathf.SmoothDamp(vignetteFilter.intensity, flashEndVignetteIntensity, ref vignetteVelocity, implodingFlashTime);
		}

		if (disablingDarkVignette) {
			UpdatePlayerVision(PlayerController.BalanceDirection.RECHARGE_FACTOR, disableSpeed);
		}
	}

	public void StopImpulse() {
		addingImpulse = false;
	}

	public void UpdatePlayerVision(PlayerController.BalanceDirection direction, float changeAmount) { 
		if (addingImpulse)
			return;
		
		if (direction == PlayerController.BalanceDirection.RECHARGE_FACTOR)
			Recharge(changeAmount);
		else
		if (direction == PlayerController.BalanceDirection.DRAINING_FACTOR) 
			Drain(changeAmount);
	}

	public void AddPlayerVisionImpulse(float changeAmount) {
		addingImpulse = true;
		impulseToApply = changeAmount;
	}

	private void Recharge(float changeAmount) {


		// Decrease vignette
		if (vignetteFilter.intensity >= 0.0f) 
			vignetteFilter.intensity -= changeAmount * vignetteSpeed;
		if (disablingDarkVignette && vignetteFilter.intensity <= maxVignetteIntensity) {
			disablingDarkVignette = false;
			vignetteSpeed = lightVignetteSpeed;
		}
			

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
		if (disablingDarkVignette)
			return;

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

	public void FlashEffect() {
		flashing = true;

		bloomVelocity = 0.0f;
		vignetteVelocity = 0.0f;
		noiseVelocity = 0.0f;

		flashEndVignetteIntensity = vignetteFilter.intensity;
		flashEndBloomIntensity = bloomFilter.bloomIntensity;
		flashEndNoiseIntensity = noiseFilter.grainIntensityMax;

		Invoke("StopFlash", flashTime);
	}

	private void StopFlash() {
		flashing = false;
		implodingFlash = true;
		bloomVelocity = 0.0f;
		vignetteVelocity = 0.0f;
		noiseVelocity = 0.0f;
		Invoke("StopImplode", implodingFlashTime);
	}

	private void StopImplode() {
		implodingFlash = false;
	}

	public void EnableDarkVignette() {
		maxVignetteIntensity = darkVignetteIntensity;
		vignetteSpeed = darkVignetteSpeed;
		disablingDarkVignette = false;
	}

	public void DisableDarkVignette() {
		maxVignetteIntensity = lightVignetteIntensity;
		disablingDarkVignette = true;
	}
}

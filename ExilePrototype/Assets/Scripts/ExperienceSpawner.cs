using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceSpawner : MonoBehaviour {

	[SerializeField] private GameObject experiencePrefab;
	[SerializeField] private float spawnDelay = 1.0f;
	[SerializeField] private float delayIncreaseFactor = 1.0f;

	//[SerializeField] private Transform topLeftSpawnPoint;

	private Transform spawnPoint;

	// Use this for initialization
	void Start () {
		spawnPoint = GetComponentInChildren<Transform>();
		Invoke("Spawn", spawnDelay);
	}
	
	// Update is called once per frame
	void Update () {
	}

	private void Spawn() {
		GameObject experience = GameObject.Instantiate(experiencePrefab);
		experience.transform.parent = transform;
		experience.transform.position = spawnPoint.position;
		ExperienceBehaviour experienceController = experience.GetComponent<ExperienceBehaviour>();
		experienceController.SetSpawner(this);
	}

	public void ExperienceCollected() {
		spawnDelay += delayIncreaseFactor;
		Invoke("Spawn", spawnDelay);
	}

}

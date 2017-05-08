using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceSpawner : MonoBehaviour {

	[SerializeField] private GameObject experiencePrefab;
	[SerializeField] private float startSpawnDelay = 1.0f;
	[SerializeField] private float delayIncreaseFactor = 1.0f;
	[SerializeField] private Transform[] spawnPointPositions;

	private float spawnDelay = 0.0f;
	private SpawnPoint[] spawnPoints;
	public struct SpawnPoint {
		public Transform transform;
		public int identifier;
		public bool pickedUp;	
		public SpawnPoint(Transform transform, int identifier) {
			this.transform = transform;
			this.identifier = identifier;
			pickedUp = true; // true for initial spawning
		}
	};

	void Start () {
		// Initialise spawn points
		spawnPoints = new SpawnPoint[spawnPointPositions.Length];
		for (int i = 0; i < spawnPointPositions.Length; i++) {
			int identifier = i+1;
			spawnPoints[i] = new SpawnPoint(spawnPointPositions[i], identifier);
			Spawn(identifier);
		}

		spawnDelay = startSpawnDelay;
	}
	
	void Update () {
	}

	private void Spawn(int identifier) {
		for (int i = 0; i < spawnPoints.Length; i++) {
			SpawnPoint spawnPoint = spawnPoints[i];
			if (spawnPoint.identifier == identifier) {
				// Spawn new experience
				GameObject experience = GameObject.Instantiate(experiencePrefab);
				experience.transform.parent = transform;
				experience.transform.position = spawnPoint.transform.position;
				ExperienceBehaviour experienceController = experience.GetComponent<ExperienceBehaviour>();
				experienceController.SetSpawner(this, spawnPoint.identifier);
			}
		}
	}

	public void ExperienceCollected(int identifier) {
		StartCoroutine(SpawnDelay(identifier));
	}

	public IEnumerator SpawnDelay(int identifier)
	{
		yield return new WaitForSeconds(spawnDelay);
		// Function resumes after spawnDelay
		spawnDelay += delayIncreaseFactor;
		Spawn(identifier);
	}

	public void ResetSpawnDelay() {
		spawnDelay = startSpawnDelay;
	}

}

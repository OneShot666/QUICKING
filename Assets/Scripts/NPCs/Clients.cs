using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utensils;

namespace NPCs {
    public class Clients : MonoBehaviour
    {
        public GameObject npcClientPrefab;
        public Transform[] spawnPoints;
        public Transform[] waitingSpots; // New: assign in inspector
        public Transform[] exitSpots;    // New: assign in inspector
        public ItemSurface[] plates; // Assign plates in order in inspector
        public string[] possibleRequests;
        public float spawnInterval = 10f;
        public TMP_Text goldText;
        public TMP_Text dishText;
        private int gold;
        private float timer;
        private Queue<NPCClient> clientQueue = new Queue<NPCClient>();

        // Update is called once per frame
        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval && clientQueue.Count < plates.Length)
            {
                SpawnClient();
                timer = 0f;
            }
            CheckPlateForDelivery();
        }

        private void SpawnClient()
        {
            int plateIndex = clientQueue.Count;
            if (npcClientPrefab == null || spawnPoints.Length == 0 || possibleRequests.Length == 0 || plateIndex >= plates.Length) return;
            var spawnPoint = spawnPoints[plateIndex % spawnPoints.Length];
            var npc = Instantiate(npcClientPrefab, spawnPoint.position, spawnPoint.rotation);
            var client = npc.GetComponent<NPCClient>();
            if (client)
            {
                string request = possibleRequests[Random.Range(0, possibleRequests.Length)];
                Transform waitSpot = waitingSpots.Length > plateIndex ? waitingSpots[plateIndex] : null;
                Transform exitSpot = exitSpots.Length > plateIndex ? exitSpots[plateIndex] : null;
                client.Initialize(request, plates[plateIndex], OnClientLeave, waitSpot, exitSpot, null, dishText);
                clientQueue.Enqueue(client);
            }
        }

        private void CheckPlateForDelivery()
        {
            if (clientQueue.Count == 0) return;
            var firstClient = clientQueue.Peek();
            if (firstClient.IsRequestFulfilled())
            {
                clientQueue.Dequeue();
                gold += 10;
                if (goldText) goldText.text = gold.ToString();
            }
        }

        private void OnClientLeave(NPCClient client)
        {
            // Already handled in CheckPlateForDelivery
        }
    }
}

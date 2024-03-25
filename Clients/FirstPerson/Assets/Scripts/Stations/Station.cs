using System;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Stations
{
    public class Station : MonoBehaviour
    {
        private WorldManager worldManager;
        public StationType Type;
        public Transform[] spawnPoints;
        public SpawnArea[] spawnAreas;
        public AudioClip AmbienceClip;
        public bool HideOtherPlayers;
        public bool IsInterior;
        public bool EveningValid;
        [NonSerialized] public bool IsAmbienceEnabled = true;
        
        private void Awake()
        {
            worldManager = GetComponentInParent<WorldManager>();
        }
        
        public Vector3 GetRandomSpawnPosition()
        {
            int pointCount = spawnPoints.Length;
            int areaCount = spawnAreas.Length;
            int totalCount = areaCount + pointCount;
            if (totalCount == 0)
            {
                throw new UnityException($"No spawns defined for station {Type}!");
            }

            int index = Random.Range(0, totalCount);
            return index < pointCount ? GetRandomSpawnPointPosition() : GetRandomSpawnAreaPosition();
        }
        
        public Vector3 GetRandomSpawnPointPosition()
        {
            if (spawnPoints.Length == 0)
            {
                
                throw new UnityException($"No spawn points defined for station {Type}!");
            }
            
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
        
        public Vector3 GetRandomSpawnAreaPosition()
        {
            if (spawnAreas.Length == 0)
            {
                throw new UnityException($"No spawn areas defined for station {Type}!");
            }
            
            return spawnAreas[Random.Range(0, spawnAreas.Length)].GetRandomPosition();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Station s))
            {
                return false;
            }
            return s.Type == Type;
        }

        public override int GetHashCode()
        {
            return (int) Type;
        }
    }
}
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Random = UnityEngine.Random;
using System.Collections;

public class ECS_Initializer : MonoBehaviour
{
    [Header("Spawner")]
    // number of enemies generated per interval
    [SerializeField] private int maxActors = 1000;
    [SerializeField] private int maxMarbles = 1000;

    // time between spawns
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int constantSpawnAmount = 100;
    [SerializeField] private float sphereRadius = 100f;

    [Header("Actor")]
    // random speed range
    [SerializeField] private Mesh actorMesh;
    [SerializeField] private Material actorMaterial;
    [SerializeField] private GameObject actorPrefab;
    private Entity actorEntityPrefab;

    [Header("Marble")]
    // random speed range
    [SerializeField] private Mesh marbleMesh;
    [SerializeField] private Material marbleMaterial;
    [SerializeField] private GameObject marblePrefab;
    private Entity marbleEntityPrefab;

    private EntityManager entityManager;

    private float spawnTimer;

    void Start()
    {
        //Here we merely convert the GameObject to Enities
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        actorEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(actorPrefab, settings);
        marbleEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(marblePrefab, settings);

        SpawnActorEntity();
        SpawnMarbleEntity(maxMarbles);
    }



    void SpawnActorEntity()
    {
        NativeArray<Entity> actorArray = new NativeArray<Entity>(maxActors, Allocator.Temp);

        for (int i = 0; i < actorArray.Length; i++)
        {
            actorArray[i] = entityManager.Instantiate(actorEntityPrefab);

            // 3
            entityManager.SetComponentData(actorArray[i], new Translation { Value = Random.insideUnitSphere* sphereRadius });
        }

        actorArray.Dispose();
    }
    void SpawnMarbleEntity(int amount)
    {
        NativeArray<Entity> marbleArray = new NativeArray<Entity>(amount, Allocator.Temp);

        for (int i = 0; i < marbleArray.Length; i++)
        {
            marbleArray[i] = entityManager.Instantiate(marbleEntityPrefab);

            // 3
            entityManager.SetComponentData(marbleArray[i], new Translation { Value = Random.insideUnitSphere * sphereRadius });
        }

        marbleArray.Dispose();
    }


    void Update()
    {
        spawnTimer += Time.deltaTime;

        // spawn and reset timer
        if (spawnTimer > spawnInterval)
        {
            Debug.Log("Spawn");
            SpawnMarbleEntity(constantSpawnAmount);
            spawnTimer = 0;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
/*
public class FindMarbleComponent : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<HasTarget>().WithAll<ActorTag>().ForEach(
        (Entity actor, ref Translation actorTranslation) =>
        {
            Entity closestMarbleEntity = Entity.Null;
            float3 actorPosition = actorTranslation.Value;
            float3 closestMarblePosition = float3.zero;

            Entities.WithNone<IsTargeted>().WithAll<MarbleTag>().ForEach(
            (Entity targetMarble, ref Translation marbleTranslation) =>
            {
                if(closestMarbleEntity == Entity.Null)
                {
                    closestMarbleEntity = targetMarble;
                    closestMarblePosition = marbleTranslation.Value;
                }
                else
                {
                    if (math.distance(actorPosition, marbleTranslation.Value) <
                       math.distance(actorPosition, closestMarblePosition))
                    {
                        closestMarbleEntity = targetMarble;
                        closestMarblePosition = marbleTranslation.Value;
                    }
                }
            });
            //closest Target
            if (closestMarbleEntity != Entity.Null)
            {
                PostUpdateCommands.AddComponent(actor, new HasTarget { targetMarbleEntity = closestMarbleEntity });
                PostUpdateCommands.AddComponent(closestMarbleEntity, typeof(IsTargeted));
            }
        });
    }
}*/

public class FindTargetJobSystem : JobComponentSystem
{
    private struct EntityWithPosition
    {
        public Entity entity;
        public float3 position;
    }
    [RequireComponentTag(typeof(ActorTag))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    private struct FindTargetJob : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public EntityCommandBuffer.Concurrent ecb;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            Entity closestMarbleEntity = Entity.Null;
            float3 actorPosition = translation.Value;
            float3 closestMarblePosition = float3.zero;

            for(int i = 0;i<targetArray.Length;i++ )
            {
                EntityWithPosition targetEntityWithPosition = targetArray[i];
                if (closestMarbleEntity == Entity.Null)
                {
                    closestMarbleEntity = targetEntityWithPosition.entity;
                    closestMarblePosition = targetEntityWithPosition.position;
                }
                else
                {
                    if (math.distance(actorPosition, targetEntityWithPosition.position) <
                       math.distance(actorPosition, closestMarblePosition))
                    {
                        closestMarbleEntity = targetEntityWithPosition.entity;
                        closestMarblePosition = targetEntityWithPosition.position;
                    }
                }
            }
            //closest Target
            if (closestMarbleEntity != Entity.Null)
            {
                ecb.AddComponent(index, entity, new HasTarget { targetMarbleEntity = closestMarbleEntity });
                ecb.AddComponent(index, closestMarbleEntity, typeof(IsTargeted));
            }
        }
    }

    [RequireComponentTag(typeof(ActorTag))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public NativeArray<Entity> closestTargetEntityArray;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            Entity closestMarbleEntity = Entity.Null;
            float3 actorPosition = translation.Value;
            float3 closestMarblePosition = float3.zero;

            for (int i = 0; i < targetArray.Length; i++)
            {
                EntityWithPosition targetEntityWithPosition = targetArray[i];
                if (closestMarbleEntity == Entity.Null)
                {
                    closestMarbleEntity = targetEntityWithPosition.entity;
                    closestMarblePosition = targetEntityWithPosition.position;
                }
                else
                {
                    if (math.distance(actorPosition, targetEntityWithPosition.position) <
                       math.distance(actorPosition, closestMarblePosition))
                    {
                        closestMarbleEntity = targetEntityWithPosition.entity;
                        closestMarblePosition = targetEntityWithPosition.position;
                    }
                }
            }

            closestTargetEntityArray[index] = closestMarbleEntity;
        }
    }

    [RequireComponentTag(typeof(ActorTag))]
    [ExcludeComponent(typeof(HasTarget))]
    private struct AddComponentJob: IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> closestTargetArray;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, ref Translation translation)
        {
            if(closestTargetArray[index]!= Entity.Null)
            {
               entityCommandBuffer.AddComponent(index, entity, new HasTarget

                {
                    targetMarbleEntity = closestTargetArray[index]
                });
            }
        }
    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>(); 
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery targetQuery = GetEntityQuery(typeof(MarbleTag), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length,Allocator.TempJob);

        for(int i = 0; i< targetEntityArray.Length; i++)
        {
            targetArray[i] = new EntityWithPosition
            {
                entity = targetEntityArray[i],
                position = targetTranslationArray[i].Value
            };
        }
        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();

        EntityQuery actorQuery = GetEntityQuery(typeof(ActorTag), ComponentType.Exclude<HasTarget>());
        NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(actorQuery.CalculateEntityCount(),Allocator.TempJob);

        FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob
        {
            targetArray = targetArray,
            closestTargetEntityArray = closestTargetEntityArray

        };

        
        JobHandle jobHandle = findTargetBurstJob.Schedule(this, inputDeps);

        AddComponentJob addComponentJob = new AddComponentJob
        {
            closestTargetArray = closestTargetEntityArray,
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };

        jobHandle = addComponentJob.Schedule(this, jobHandle);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
        targetArray.Dispose();
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;


public class MoveToMarbleComponent : ComponentSystem
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        float moveSpeed = 5f;
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entities.ForEach(
            (Entity actorEntity, ref HasTarget hasTarget, ref Translation translation)=>
        {
            if (entityManager.Exists(hasTarget.targetMarbleEntity))
            {
                Translation targetTranslation = entityManager.GetComponentData<Translation>(hasTarget.targetMarbleEntity);
                float3 targetDir = math.normalize(targetTranslation.Value - translation.Value);
                translation.Value += targetDir * moveSpeed * dt;

                if (math.distance(translation.Value, targetTranslation.Value) < 0.2f)
                {
                    TextManager.Instance.DisplayScoreFunc(targetTranslation.Value);
                    PostUpdateCommands.DestroyEntity(hasTarget.targetMarbleEntity);
                    PostUpdateCommands.RemoveComponent(actorEntity, typeof(HasTarget));
                }
            }
            else
            {
                PostUpdateCommands.RemoveComponent(actorEntity, typeof(HasTarget));
            }
        });
    }
}


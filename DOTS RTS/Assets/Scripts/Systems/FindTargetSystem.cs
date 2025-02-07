using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

partial struct FindTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach((RefRO<LocalTransform> LocalTransform,
            RefRW<FindTarget> findTarget,
            RefRW<Target> target) in
            SystemAPI.Query<RefRO<LocalTransform>, 
            RefRW<FindTarget>,
            RefRW<Target>>()){

            findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if(findTarget.ValueRO.timer > 0f){
                //timer not elapsed
                continue;
            }
            findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

            distanceHitList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,    //inverte no bitmask
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0,
            };

            Debug.Log("-");

            if( collisionWorld.OverlapSphere(LocalTransform.ValueRO.Position,
                findTarget.ValueRO.range,
                ref distanceHitList, collisionFilter))
            {
                foreach(DistanceHit distanceHit in distanceHitList)
                {
                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                    if(targetUnit.faction == findTarget.ValueRO.targetFaction)
                    {
                        //valid target
                        target.ValueRW.targetEntity = distanceHit.Entity;
                        break;
                    }
                }
            }

        }


    }

}

using Unity.Entities;

[GenerateAuthoringComponent]
public struct ActorTag : IComponentData
{
}

public struct HasTarget :IComponentData
{
    public Entity targetMarbleEntity;
}

using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Corran Carajo", story: "Agent scape from [TheOther]", category: "Action", id: "c19a2821679366b811a3ab5ce7bc3dcb")]
public partial class CorranCarajoAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> TheOther;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    [Header("Move info")]
    public float moveSpeed = .8f;

    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");

    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();

    }



    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

}

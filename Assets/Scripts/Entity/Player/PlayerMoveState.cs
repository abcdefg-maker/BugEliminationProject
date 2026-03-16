using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    public class PlayerMoveState : PlayerState
    {
        private const float VerticalSpeedScale = 0.8f;

        public PlayerMoveState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {
            base.Update();
            player.SetVelocity(xInput * player.moveSpeed, VerticalSpeedScale * yInput * player.moveSpeed);

            //Debug.Log("Now is in MoveState");

            if (xInput == 0 && yInput == 0)
                stateMachine.ChangeState(player.idleState);
        }
    }
}

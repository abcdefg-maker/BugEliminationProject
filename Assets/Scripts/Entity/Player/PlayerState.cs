using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    public class PlayerState
    {
        protected PlayerStateMachine stateMachine;
        protected Player player;


        protected float xInput;
        protected float yInput;
        private string animBoolName;

        protected bool triggerCalled;

        public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        {
            this.player = _player;
            this.stateMachine = _stateMachine;
            this.animBoolName = _animBoolName;
        }

        public virtual void Enter()
        {
            player.anim.SetBool(animBoolName, true);
            triggerCalled = false;
        }
        public virtual void Update()
        {
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");


            player.anim.SetFloat(GameConstants.AnimParams.YVelocity, player.rb.velocity.y);
        }
        public virtual void Exit()
        {
            player.anim.SetBool(animBoolName, false);
        }

        public virtual void AnimationFinishTrigger()
        {
            triggerCalled = true;
        }
    }
}

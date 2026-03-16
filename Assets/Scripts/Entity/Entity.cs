using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    public class Entity : MonoBehaviour
    {
        #region Components
        //实体相关的组件
        public SpriteRenderer sr { get; private set; }
        public Animator anim { get; private set; }//{ get; private set; }: 这是属性的访问器，表示该属性可以被外部读取，但只能在类内部设置。
        public Rigidbody2D rb { get; private set; }
        #endregion



        public int facingDir { get; private set; } = 1;//facingDir变量用于存储玩家的朝向，1表示向右，-1表示向左
        protected bool facingRight = true;


        public System.Action onFlipped;


        protected virtual void Awake()
        {

        }


        protected virtual void Start()
        {
            sr = GetComponentInChildren<SpriteRenderer>();//使用SpriteRenderer来控制玩家的显示
            anim = GetComponentInChildren<Animator>();//使用Animator来控制玩家的动画
            rb = GetComponent<Rigidbody2D>();//使用Rigidbody2D来控制玩家的移动

        }


        protected virtual void Update()
        {

        }


        #region Velocity
        public void SetZeroVelocity()
        {
            rb.velocity = new Vector2(0, 0);
        }

        public void SetVelocity(float _xVelocity, float _yVelocity)
        {

            rb.velocity = new Vector2(_xVelocity, _yVelocity);
            FlipController(_xVelocity);
        }

        #endregion

        #region Flip
        public virtual void Flip()
        {
            facingDir = facingDir * -1;
            facingRight = !facingRight;
            transform.Rotate(0, 180, 0);

            if (onFlipped != null)//没有添加上的角色就不会报错
                onFlipped();
        }

        public virtual void FlipController(float _x)
        {
            if (_x > 0 && facingRight)
                Flip();
            else if (_x < 0 && !facingRight)
                Flip();
        }
        #endregion


        public virtual void Die()
        {

        }
    }
}

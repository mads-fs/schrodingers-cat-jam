using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CatController : MonoBehaviour
    {
        public event EventHandler OnAlived;
        public event EventHandler OnUnalived;

        [Space(10), Header("Physics")]
        [Tooltip("How close to the ground must the player be to be considered grounded.")]
        public float minGroundNormalY = 0.65f;
        [Range(-1f, 5f), Tooltip("How much the player should be affected by gravity. The higher the heavier, 1 is default.")]
        public float gravityModifier = 1f;
        [Space(10), Header("Player")]
        [Tooltip("The Max Speed of the player.")]
        public float MaxSpeed = 7;
        [Tooltip("How much speed the player has when jumping. Higher number equals higher jump apex.")]
        public float JumpTakeOffSpeed = 7;

        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        [Header("Debug - Read Only")]
        [SerializeField]
        private Vector2 _targetVelocity;
        [SerializeField]
        private bool _grounded;
        private Vector2 _groundNormal;
        private Rigidbody2D _rBody;
        [SerializeField]
        private Vector2 _velocity;
        private ContactFilter2D _contactFilter;
        private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);

        private const float _minMoveDistance = 0.001f;
        private const float _shellRadius = 0.01f;

        private void OnEnable()
        {
            _rBody = GetComponent<Rigidbody2D>();
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            //_animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _contactFilter.useTriggers = false;
            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            _contactFilter.useLayerMask = true;
        }

        private void Update()
        {
            _targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        private void ComputeVelocity()
        {
            Vector2 move = Vector2.zero;
            move.x = Input.GetAxis("Horizontal");
            if (Input.GetButtonDown("Jump") && _grounded)
            {
                _velocity.y = JumpTakeOffSpeed;
            }
            else if (Input.GetButtonUp("Jump"))
            {
                if (_velocity.y > 0)
                {
                    _velocity.y *= 0.5f;
                }
            }

            bool flipSprite = (_spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f));
            if(flipSprite)
            {
                _spriteRenderer.flipX = !_spriteRenderer.flipX;
            }

            //_animator.SetBool("grounded", _grounded);
            //_animator.SetFloat("velocityX", Mathf.Abs(_velocity.x) / MaxSpeed);

            _targetVelocity = move * MaxSpeed;
        }

        private void FixedUpdate()
        {
            _velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
            _velocity.x = _targetVelocity.x;

            _grounded = false;
            Vector2 deltaPosition = _velocity * Time.deltaTime;
            Vector2 moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
            Vector2 move = moveAlongGround * deltaPosition.x;
            Movement(move, false);
            move = Vector2.up * deltaPosition.y;
            Movement(move, true);
        }

        public void Movement(Vector2 move, bool yMovement)
        {
            float distance = move.magnitude;
            if (distance > _minMoveDistance)
            {
                int count = _rBody.Cast(move, _contactFilter, _hitBuffer, distance + _shellRadius);
                _hitBufferList.Clear();
                for (int index = 0; index < count; index++) _hitBufferList.Add(_hitBuffer[index]);
                for (int index = 0; index < _hitBufferList.Count; index++)
                {
                    Vector2 currentNormal = _hitBufferList[index].normal;
                    if (currentNormal.y > minGroundNormalY)
                    {
                        _grounded = true;
                        if (yMovement)
                        {
                            _groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }

                    float projection = Vector2.Dot(_velocity, currentNormal);
                    if (projection < 0)
                    {
                        _velocity = _velocity - projection * currentNormal;
                    }

                    float modifiedDistance = _hitBufferList[index].distance - _shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }

            _rBody.position = _rBody.position + move.normalized * distance;
        }

        private void EmitLeaveSpiritRealm() => OnAlived?.Invoke(this, null);
        private void EmitEnterSpiritRealm() => OnUnalived?.Invoke(this, null);
    }
}
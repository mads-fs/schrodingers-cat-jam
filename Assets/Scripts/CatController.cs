using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CatController : MonoBehaviour
    {
        public event EventHandler OnSproing;
        public event EventHandler OnPassthru;
        public event EventHandler OnAlived;
        public event EventHandler OnUnalived;

        public GameObject ButtonPromptEPrefab;
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
        [Space(10), Header("Setup")]
        public SpriteRenderer Renderer;
        public Animator CatAnimator;
        public VisionManager @VisionManager;
        public AnimationBroadcaster AnimationEventListener;
        public GameObject JumpAudioPrefab;


        [Header("Debug - Read Only")]
        public bool CanMove = true;
        public bool CanInteract = true;
        public bool IsGrounded;
        [SerializeField]
        private Vector2 _targetVelocity;
        private Vector2 _groundNormal;
        private Rigidbody2D _rBody;
        [SerializeField]
        private Vector2 _velocity;
        private ContactFilter2D _contactFilter;
        private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);

        private const float _minMoveDistance = 0.001f;
        private const float _shellRadius = 0.01f;
        private bool _isAlive = true;

        private void OnEnable()
        {
            _rBody = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _contactFilter.useTriggers = false;
            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            _contactFilter.useLayerMask = true;

            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueStart += DialogueManager_OnDialogueStart;
                dialogueManager.OnDialogueAdvance += DialogueManager_OnDialogueAdvance;
                dialogueManager.OnDialogueEnd += DialogueManager_OnDialogueEnd;
            }
            AnimationEventListener.OnBroadcastEvent += AnimationEventListener_OnBroadcastEvent;
        }

        private void Update()
        {
            _targetVelocity = Vector2.zero;
            if (CanInteract == true && IsGrounded == true)
            {
                if (@VisionManager.CurrentlySeen.Count > 0)
                {
                    ButtonPromptEPrefab.SetActive(true);
                }
                else
                {
                    ButtonPromptEPrefab.SetActive(false);
                }
            }
            if (CanInteract == true && IsGrounded == true && Input.GetKeyDown(KeyCode.E)) Interact();
            ComputeVelocity();
        }

        private void Interact()
        {
            CatAnimator.SetTrigger("Swipe");
            CanMove = false;
            CanInteract = false;
            if (@VisionManager.CurrentlySeen.Count > 0)
            {
                if (@VisionManager.CurrentlySeen.Count == 1)
                {
                    @VisionManager.CurrentlySeen[0].GetComponent<Interactable>().TriggerInteract();
                }
                else
                {
                    GameObject closestObject = null;
                    float distance = float.MaxValue;
                    for (int index = 0; index < @VisionManager.CurrentlySeen.Count; index++)
                    {
                        float calcDistance = Vector3.Distance(transform.position, @VisionManager.CurrentlySeen[index].transform.position);
                        if (calcDistance <= distance)
                        {
                            distance = calcDistance;
                            closestObject = @VisionManager.CurrentlySeen[index];
                        }
                    }
                    closestObject.GetComponent<Interactable>().TriggerInteract();
                }
            }
            else
            {
                Debug.Log("Nothing to interact with in vision.");
            }
        }

        private void ComputeVelocity()
        {
            Vector2 move = Vector2.zero;
            if (CanMove)
            {
                move.x = Input.GetAxis("Horizontal");
                if (Input.GetButtonDown("Jump") && IsGrounded)
                {
                    if (Input.GetAxis("Vertical") < 0)
                    {
                        OnPassthru?.Invoke(this, null);
                        transform.position = new Vector3(transform.position.x, transform.position.y - minGroundNormalY, transform.position.z);
                    }
                    else
                    {
                        _velocity.y = JumpTakeOffSpeed;
                        OnSproing?.Invoke(this, null);
                        Instantiate(JumpAudioPrefab, Vector2.zero, Quaternion.identity);
                    }
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    if (_velocity.y > 0)
                    {
                        _velocity.y *= 0.5f;
                    }
                }
            }

            if (move.x > 0.01f) Renderer.flipX = false;
            if (move.x < -0.01f) Renderer.flipX = true;

            CatAnimator.SetBool("Grounded", IsGrounded);
            if (IsGrounded)
            {
                CatAnimator.SetBool("Walking", _velocity.x > 0f || _velocity.x < 0f);
            }

            _targetVelocity = move * MaxSpeed;
        }

        private void FixedUpdate()
        {
            _velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
            _velocity.x = _targetVelocity.x;

            IsGrounded = false;
            Vector2 deltaPosition = _velocity * Time.deltaTime;
            Vector2 moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
            Vector2 move = moveAlongGround * deltaPosition.x;
            Movement(move, false);
            move = Vector2.up * deltaPosition.y;
            Movement(move, true);
        }

        private void DialogueManager_OnDialogueStart(object sender, EventArgs e)
        {
            CanMove = false;
            CanInteract = false;
        }

        private void DialogueManager_OnDialogueAdvance(object sender, EventArgs e)
        {
            Debug.Log($"{typeof(CatController)}: OnDialogueAdvance");
        }

        private void DialogueManager_OnDialogueEnd(object sender, EventArgs e)
        {
            CanMove = true;
            CanInteract = true;
        }

        private void AnimationEventListener_OnBroadcastEvent(object sender, string e)
        {
            if (FindObjectOfType<DialogueManager>().IsPlayingDialogue == false)
            {
                CanMove = true;
                CanInteract = true;
            }
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
                        IsGrounded = true;
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

        public void Transition()
        {
            _isAlive = !_isAlive;
            if (_isAlive)
            {
                EmitLeaveSpiritRealm();
            }
            else
            {
                EmitEnterSpiritRealm();
            }
        }
        private void EmitLeaveSpiritRealm() => OnAlived?.Invoke(this, null);
        private void EmitEnterSpiritRealm() => OnUnalived?.Invoke(this, null);
    }
}
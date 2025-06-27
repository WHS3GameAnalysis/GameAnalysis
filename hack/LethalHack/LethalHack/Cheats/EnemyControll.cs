using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace LethalHack
{
    internal class EnemyControll : Cheat
    {
        private static EnemyAI enemy = null;
        private static GameObject ControllerInstance = null;
        private static MouseInput mouse = null;
        private static AIMovement movement = null;
        private static AudioListener audioListener = null;
        public static bool IsAIControlled = false;
        public static bool Controlling = false;
        public static bool NoClipEnabled = false;
        private const float TeleportDoorCooldown = 2.5f;
        private const float DoorInteractionCooldown = 0.7f;
        private float DoorCooldownRemaining = 0.0f;
        private float TeleportCooldownRemaining = 0.0f;

        private static Camera mainCamera = null; // 메인 카메라 참조 추가
        private static Vector3 originalCameraPosition; // 원래 카메라 위치 저장
        private static Quaternion originalCameraRotation; // 원래 카메라 회전 저장
        private static Vector3 cameraOffset = new Vector3(0, 2f, -3f); // 카메라 오프셋 설정 (몬스터 뒤쪽 위치)
        

        // 문법 오류 수정
        private static Dictionary<Type, IController> EnemyControllers = new Dictionary<Type, IController>()
        {
            //{ typeof(BaboonBirdAI), new BaboonBirdController() }, 
            { typeof(FlowerSnakeEnemy), new FlowerSnakeController() }
        };

        public override void Trigger()
        {
            if (!isEnabled) return;
            // 적 제어 로직
            HandleEnemyControl();
            UpdateCooldowns();
            UpdateCameraPosition(); // 추가

        }

        private void HandleEnemyControl()
        {
            if (Controlling && enemy != null)
            {                
                UpdateEnemyControl(); // 현재 제어 중인 적이 있으면 업데이트
            }
            else
            {                
                FindAndControlEnemy(); // 새로운 적을 찾아서 제어 시작
            }
        }
        public class EnemyCameraFollow : MonoBehaviour // 추가 ===================================================
        {
            private Transform target;
            private Vector3 offset;
            private float followSpeed = 5f;

            public void SetTarget(Transform targetTransform, Vector3 offsetValue)
            {
                target = targetTransform;
                offset = offsetValue;
            }

            void LateUpdate()
            {
                if (target == null) return;

                Vector3 desiredPosition = target.position + target.TransformDirection(offset);
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);

                Vector3 lookDirection = target.position - transform.position;
                lookDirection.y += 1f;
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }// ============================================================================end


        private void FindAndControlEnemy()
        {
            // 근처의 적 찾기 ----------------------------------------------------------------------------------
            EnemyAI[] enemies = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
            EnemyAI closestEnemy = null;
            float closestDistance = float.MaxValue;

            if (Hack.localPlayer == null) return;

            foreach (EnemyAI enemyAI in enemies)
            {
                if (enemyAI == null || enemyAI.isEnemyDead) continue;

                float distance = Vector3.Distance(Hack.localPlayer.transform.position, enemyAI.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemyAI;
                }
            }

            if (closestEnemy != null && closestDistance <= 10f) // 10미터 이내
            {
                StartControllingEnemy(closestEnemy);
            }
        }

        private void StartControllingEnemy(EnemyAI enemyToControl)
        {
            enemy = enemyToControl;
            Controlling = true;
            IsAIControlled = true;

            // 메인 카메라 찾기 추가 ==========================================================
            /*
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = UnityEngine.Object.FindObjectOfType<Camera>();
                }
            }*/
            if (mainCamera == null)
            {
                mainCamera = Camera.main;

                // fallback: Camera.main이 없을 경우, 플레이어 자식에서 찾기
                if (mainCamera == null && Hack.localPlayer != null)
                {
                    mainCamera = Hack.localPlayer.GetComponentInChildren<Camera>();
                }

                // 그래도 못 찾으면 모든 카메라 중 하나 가져오기
                if (mainCamera == null)
                {
                    mainCamera = UnityEngine.Object.FindObjectOfType<Camera>();
                }
            }

            EnemyCameraFollow follower = mainCamera.GetComponent<EnemyCameraFollow>();// -----------추가의추가
            if (follower == null)
            {
                follower = mainCamera.gameObject.AddComponent<EnemyCameraFollow>();
            }
            follower.SetTarget(enemy.transform, cameraOffset);

            // 원래 카메라 위치 저장
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.position;
                originalCameraRotation = mainCamera.transform.rotation;
            } // ===============================================================================

            // 컨트롤러 인스턴스 생성 ------------------------------------------------------------------------------------
            if (ControllerInstance == null)
            {
                ControllerInstance = new GameObject("EnemyController");
                movement = ControllerInstance.AddComponent<AIMovement>();
                mouse = ControllerInstance.AddComponent<MouseInput>();

                // AudioListener 설정
                audioListener = ControllerInstance.AddComponent<AudioListener>();
            }

            // 컨트롤러 위치를 적의 위치로 설정
            ControllerInstance.transform.position = enemy.transform.position;
            ControllerInstance.transform.rotation = enemy.transform.rotation;

            Debug.Log($"Started controlling enemy: {enemy.enemyType.enemyName}");
        }

        private void UpdateEnemyControl()
        {
            if (enemy == null || enemy.isEnemyDead)
            {
                StopControllingEnemy();
                return;
            }

            // 적의 위치를 컨트롤러 위치로 동기화
            if (ControllerInstance != null)
            {
                enemy.transform.position = ControllerInstance.transform.position;
                enemy.transform.rotation = ControllerInstance.transform.rotation;
            }
            UpdateCameraPosition(); // 추가 ===
        }
        static void UpdateCameraPosition() // 추가=======================================================================
        {
            if (mainCamera != null && enemy != null)
            {
                // 몬스터의 회전을 기준으로 카메라 오프셋 계산
                Vector3 desiredPosition = enemy.transform.position + enemy.transform.TransformDirection(cameraOffset);

                // 카메라 위치 부드럽게 이동 (선택사항)
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, Time.deltaTime * 5f);

                // 카메라가 몬스터를 바라보도록 설정
                Vector3 lookDirection = enemy.transform.position - mainCamera.transform.position;
                lookDirection.y += 1f; // 약간 위쪽을 바라보도록
                mainCamera.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        } // ===========================================================================================

        public static void StopControllingEnemy()
        {
            Controlling = false;
            IsAIControlled = false;
            enemy = null;

            if (ControllerInstance != null)
            {
                UnityEngine.Object.Destroy(ControllerInstance);
                ControllerInstance = null;
                movement = null;
                mouse = null;
                audioListener = null;
            }

            Debug.Log("Stopped controlling enemy");
        }

        private void UpdateCooldowns()
        {
            if (DoorCooldownRemaining > 0f)
                DoorCooldownRemaining -= Time.deltaTime;

            if (TeleportCooldownRemaining > 0f)
                TeleportCooldownRemaining -= Time.deltaTime;
        }

        // 문 상호작용 - 구현 못 함
        public static void InteractWithDoor()
        {
            // 문 상호작용 로직 구현
        }

        // 텔레포트 - 이게 안되나? ---------------------------------------------------------------------------------------
        public static void TeleportToPosition(Vector3 position)
        {
            if (enemy != null && ControllerInstance != null)
            {
                ControllerInstance.transform.position = position;
                UpdateCameraPosition(); // 추가 ====

            }
        }
        // 카메라 오프셋 설정 메서드 추가 =====================================
        public static void SetCameraOffset(Vector3 offset)
        {
            cameraOffset = offset;
        } // ================================================================
    }

    // AIMovement를 별도 클래스로 분리
    internal class AIMovement : MonoBehaviour
    {
        // Movement constants
        private const float WalkingSpeed = 0.5f;
        private const float SprintDuration = 0.0f;
        private const float JumpForce = 9.2f;
        private const float Gravity = 18.0f;
        internal float CharacterSpeed = 5.0f;
        internal float CharacterSprintSpeed = 2.8f;

        // Movement state
        internal bool IsMoving { get; private set; } = false;
        internal bool IsSprinting { get; private set; } = false;

        // Components and state variables
        private float VelocityY = 0.0f;
        private bool IsSprintHeld = false;
        private float SprintTimer = 0.0f;
        private Keyboard keyboard = Keyboard.current;
        private CharacterController characterController;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
            }
        }

        void Update()
        {
            HandleInput();
            HandleMovement();
            //HandleNoClip();
        }

        private void HandleInput()
        {
            if (keyboard == null) return;

            // 이동 입력 처리
            Vector2 moveInput = Vector2.zero;
            if (keyboard.wKey.isPressed) moveInput.y += 1f;
            if (keyboard.sKey.isPressed) moveInput.y -= 1f;
            if (keyboard.aKey.isPressed) moveInput.x -= 1f;
            if (keyboard.dKey.isPressed) moveInput.x += 1f;

            IsMoving = moveInput.magnitude > 0.1f;

            // 스프린트 처리
            IsSprintHeld = keyboard.leftShiftKey.isPressed;
            if (IsSprintHeld)
            {
                SprintTimer += Time.deltaTime;
                IsSprinting = SprintTimer >= SprintDuration;
            }
            else
            {
                SprintTimer = 0f;
                IsSprinting = false;
            }
        }

        private void HandleMovement()
        {
            if (!EnemyControll.Controlling || characterController == null) return;

            Vector3 moveDirection = Vector3.zero;

            if (keyboard != null)
            {
                // 카메라 기준 이동 방향 계산 -------------------------------------------------여기 뭔가 이상함
                Transform cameraTransform = Camera.main?.transform ?? transform;
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;

                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                if (keyboard.wKey.isPressed) moveDirection += forward;
                if (keyboard.sKey.isPressed) moveDirection -= forward;
                if (keyboard.aKey.isPressed) moveDirection -= right;
                if (keyboard.dKey.isPressed) moveDirection += right;
            }

            // 속도 계산
            float currentSpeed = IsSprinting ? CharacterSprintSpeed : CharacterSpeed;
            if (keyboard?.leftCtrlKey.isPressed == true) currentSpeed = WalkingSpeed;

            // 중력 처리
            if (!EnemyControll.NoClipEnabled)
            {
                if (characterController.isGrounded)
                {
                    VelocityY = 0f;
                    if (keyboard?.spaceKey.wasPressedThisFrame == true)
                    {
                        VelocityY = JumpForce;
                    }
                }
                else
                {
                    VelocityY -= Gravity * Time.deltaTime;
                }

                moveDirection.y = VelocityY;
            }
            else
            {
                // NoClip 모드에서는 Y축 이동도 가능
                if (keyboard?.spaceKey.isPressed == true) moveDirection.y += 1f;
                if (keyboard?.leftCtrlKey.isPressed == true) moveDirection.y -= 1f;
            }

            // 이동 적용
            if (moveDirection.magnitude > 0.1f)
            {
                characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
            }
        }
        /*
        private void HandleNoClip()
        {
            // NoClip 토글
            if (keyboard?.nKey.wasPressedThisFrame == true)
            {
                EnemyControll.NoClipEnabled = !EnemyControll.NoClipEnabled;

                if (Hack.localPlayer != null)
                {
                    CharacterController controller = Hack.localPlayer.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        if (EnemyControll.NoClipEnabled)
                        {
                            controller.enabled = false;
                            Debug.Log("NoClip 활성화");
                        }
                        else
                        {
                            controller.enabled = true;
                            Debug.Log("NoClip 비활성화");
                        }
                    }
                }

                // 적 컨트롤러의 NoClip도 토글
                if (characterController != null)
                {
                    characterController.enabled = !EnemyControll.NoClipEnabled;
                }
            }
        }*/
    }

    // MouseInput 클래스 -----------------------------------------------------------------------------
    internal class MouseInput : MonoBehaviour
    {
        private Mouse mouse;
        private float mouseSensitivity = 2f;

        void Start()
        {
            mouse = Mouse.current;
        }

        void Update()
        {
            if (!EnemyControll.Controlling || mouse == null) return;

            // 마우스 회전 처리
            Vector2 mouseDelta = mouse.delta.ReadValue();

            if (mouseDelta.magnitude > 0.1f)
            {
                transform.Rotate(0, mouseDelta.x * mouseSensitivity, 0);

                // 상하 회전은 제한
                Vector3 rotation = transform.rotation.eulerAngles;
                rotation.x -= mouseDelta.y * mouseSensitivity;
                rotation.x = Mathf.Clamp(rotation.x > 180 ? rotation.x - 360 : rotation.x, -90f, 90f);
                transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
            }
        }
    }
    internal class FlowerSnakeController : IController
    {
        // 기본 인터페이스 구현 - 최소한만 
        public void OnTakeControl(EnemyAI enemy) { }
        public void OnReleaseControl(EnemyAI enemy) { }
        public void OnDeath(EnemyAI enemy) { }
        public void Update(EnemyAI enemy, bool isAIControlled) { }
        public void OnSecondarySkillHold(EnemyAI enemy) { }
        public void ReleaseSecondarySkill(EnemyAI enemy) { }
        public void OnMovement(EnemyAI enemy, bool isMoving, bool isSprinting) { }
        public bool IsAbleToMove(EnemyAI enemy) => true;
        public bool IsAbleToRotate(EnemyAI enemy) => true;
        public bool SyncAnimationSpeedEnabled(EnemyAI enemy) => true;
        public float SprintMultiplier(EnemyAI enemy) => ControllerDefaults.DefaultSprintMultiplier;

        // 주요 기능들
        public void UsePrimarySkill(EnemyAI enemy)
        {
            FlowerSnakeEnemy snake = enemy as FlowerSnakeEnemy;
            if (snake == null) return;

            // 간단한 점프
            snake.transform.position += snake.transform.forward * 2f;
        }

        public void UseSecondarySkill(EnemyAI enemy)
        {
            FlowerSnakeEnemy snake = enemy as FlowerSnakeEnemy;
            if (snake == null) return;

            // 간단한 날기
            snake.transform.position += Vector3.up * 1f;
        }
                    

        public bool CanUseEntranceDoors(EnemyAI enemy) => true;
        public string GetPrimarySkillName(EnemyAI enemy) => "점프";
        public string GetSecondarySkillName(EnemyAI enemy) => "날기";
        public float InteractRange(EnemyAI enemy) => 5f;
    }
    internal static class ControllerDefaults // 오류 나서 둘만 빼뒀음. 
    {
        public const float DefaultSprintMultiplier = 2.8f;
        public const float DefaultInteractRange = 2.5f;
    }

    internal interface IController
    {
        //const float DefaultSprintMultiplier = 2.8f;

        //const float DefaultInteractRange = 2.5f;
        void OnTakeControl(EnemyAI enemy);

        void OnReleaseControl(EnemyAI enemy);

        void OnDeath(EnemyAI enemy);

        void Update(EnemyAI enemy, bool isAIControlled);

        void UsePrimarySkill(EnemyAI enemy);

        void OnSecondarySkillHold(EnemyAI enemy);

        void UseSecondarySkill(EnemyAI enemy);

        void ReleaseSecondarySkill(EnemyAI enemy);

        void OnMovement(EnemyAI enemy, bool isMoving, bool isSprinting);

        bool IsAbleToMove(EnemyAI enemy);

        bool IsAbleToRotate(EnemyAI enemy);

        bool CanUseEntranceDoors(EnemyAI enemy);

        string? GetPrimarySkillName(EnemyAI enemy);

        string? GetSecondarySkillName(EnemyAI enemy);

        float InteractRange(EnemyAI enemy);

        float SprintMultiplier(EnemyAI enemy);

        bool SyncAnimationSpeedEnabled(EnemyAI enemy);
    }
}
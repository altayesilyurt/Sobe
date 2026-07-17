using UnityEngine;
using UnityEngine.UI; // UI elemanlarını kullanabilmek için bu kütüphaneyi ekledik

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    [Header("Hareket Ayarları")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Yerçekimi Ayarları")]
    private float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Animasyon Ayarları")]
    private float acceleration = 2f;
    private float currentAnimSpeed = 0f;

    [Header("Dayanıklılık (Stamina) Ayarları")]
    public float maxSprintTime = 1.5f;
    private float currentSprintTime;
    public float rechargeTime = 4f;
    private bool isExhausted = false;

    [Header("Arayüz (UI) Ayarları")]
    public Slider staminaSlider; // Slider'ımızı buraya tanımladık

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        currentSprintTime = maxSprintTime; 

        // Oyun başladığında Slider'ın maksimum değerini staminamızın maksimum değerine eşitliyoruz
        if(staminaSlider != null)
        {
            staminaSlider.maxValue = maxSprintTime;
            staminaSlider.value = currentSprintTime;
        }
    }

    void Update()
    {
        ApplyGravity();
        HandleMovement();
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        bool isSprinting = false;

        if (direction.magnitude >= 0.1f)
        {
            if (wantsToSprint && !isExhausted)
            {
                isSprinting = true;
                currentSprintTime -= Time.deltaTime; 
                
                if (currentSprintTime <= 0f)
                {
                    currentSprintTime = 0f;
                    isExhausted = true; 
                }
            }
            
            float targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            float currentMoveSpeed = isSprinting ? runSpeed : walkSpeed;
            controller.Move(direction * currentMoveSpeed * Time.deltaTime);
            UpdateAnimation(targetAnimSpeed);
        }
        else
        {
            UpdateAnimation(0f);
        }

        // --- Stamina Yenilenme ---
        if (!isSprinting && currentSprintTime < maxSprintTime)
        {
            currentSprintTime += (maxSprintTime / rechargeTime) * Time.deltaTime;

            if (currentSprintTime >= maxSprintTime)
            {
                currentSprintTime = maxSprintTime;
                isExhausted = false; 
            }
        }

        // --- UI GÜNCELLEMESİ ---
        // Her karede Slider'ın değerini güncel staminamıza eşitliyoruz
        if(staminaSlider != null)
        {
            staminaSlider.value = currentSprintTime;
        }
    }

    private void UpdateAnimation(float targetSpeed)
    {
        currentAnimSpeed = Mathf.MoveTowards(currentAnimSpeed, targetSpeed, acceleration * Time.deltaTime);
        animator.SetFloat("Speed", currentAnimSpeed);
    }
    
}
using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(Renderer))] // Добавлен Renderer
public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f; // Скорость передвижения
    public float mouseSensitivity = 100f; // Чувствительность мыши

    private Transform playerBody;
    private float xRotation = 0f;
    EffectController effectController;
    Rigidbody rb;


    [Header("Effects")]
    public float runningSpeedMultiplier = 1.5f;
    public float stunDuration = 2f;
    public Material hitMaterial; // Материал для эффекта попадания

    private float baseMoveSpeed;
    private float baseMouseSensitivity;
    private Coroutine stunCoroutine;
    public Renderer screenRenderer;


    public void Start()
    {
        effectController = new EffectController(this);
        playerBody = GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        baseMoveSpeed = moveSpeed;
        baseMouseSensitivity = mouseSensitivity;
        screenRenderer = GetComponent<Renderer>();
    }



    public void Update()
    {
        effectController.UpdateEffects();
        //Movement();
        Looking(); 
    }
    public void FixedUpdate()
    {
        if (!effectController.HasEffect(PlayerEffect.Stunning))
            Movement();
    }

    public void ApplyEffect(PlayerEffect effect)
    {
        effectController.AddEffect(effect);
    }
    void Movement()
    {
        float moveX = Input.GetAxis("Horizontal"); // Получаем ввод по горизонтали (A/D или стрелки влево/вправо)
        float moveZ = Input.GetAxis("Vertical"); // Получаем ввод по вертикали (W/S или стрелки вверх/вниз)

        Vector3 move = transform.right * moveX + transform.forward * moveZ; // Рассчитываем направление движения
        playerBody.position += move * moveSpeed * Time.deltaTime; // Применяем движение
    }
    void Looking()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Плавный поворот камеры по вертикали
        Camera.main.transform.localRotation = Quaternion.Slerp(
            Camera.main.transform.localRotation,
            Quaternion.Euler(xRotation, 0f, 0f),
            Time.deltaTime * 10f // Скорость интерполяции
        );

        // Поворот игрока по горизонтали
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

class EffectController
{
    private PlayerEffect activeEffects;
    private PlayerController player;
    private float originalSpeed;

    public EffectController(PlayerController player)
    {
        this.player = player;
        originalSpeed = player.moveSpeed;
    }

    public void AddEffect(PlayerEffect effect)
    {
        activeEffects |= effect;
        HandleEffectStart(effect);
    }

    public void RemoveEffect(PlayerEffect effect)
    {
        activeEffects &= ~effect;
        HandleEffectEnd(effect);
    }

    public bool HasEffect(PlayerEffect effect)
    {
        return (activeEffects & effect) == effect;
    }

    public void UpdateEffects()
    {
        Debug.Log("1");
        if (HasEffect(PlayerEffect.Running))
            player.moveSpeed = originalSpeed * player.runningSpeedMultiplier;
        else
            player.moveSpeed = originalSpeed;
        Debug.Log("2");
        if (HasEffect(PlayerEffect.Stunning))
            player.transform.Rotate(Vector3.up * 100 * Time.deltaTime);
        Debug.Log("3");
        if (HasEffect(PlayerEffect.Hit))
            HandleHitEffect();
    }

    private void HandleEffectStart(PlayerEffect effect)
    {
        switch (effect)
        {
            case PlayerEffect.Stunning:
                player.StartCoroutine(StunCoroutine());
                break;
            case PlayerEffect.Hit:
                player.screenRenderer.material = player.hitMaterial;
                break;
        }
    }

    private void HandleEffectEnd(PlayerEffect effect)
    {
        switch (effect)
        {
            case PlayerEffect.Hit:
                player.screenRenderer.material = null;
                break;
        }
    }

    private System.Collections.IEnumerator StunCoroutine()
    {
        yield return new WaitForSeconds(player.stunDuration);
        RemoveEffect(PlayerEffect.Stunning);
    }

    private void HandleHitEffect()
    {
        // Дополнительная логика эффекта попадания
    }
}

[Flags]
public enum PlayerEffect
{
    None = 0,
    Running = 1 << 0,   // 1
    Stunning = 1 << 1,  // 2
    Hit = 1 << 2        // 4
}
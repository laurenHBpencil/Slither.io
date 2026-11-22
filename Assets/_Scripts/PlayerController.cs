using System;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    private Camera _mainCamera;
    private Vector3 _mouseInput = Vector3.zero;
    private int score;
    private TextMeshProUGUI scoreText;


    private void Initialize()
    {
        _mainCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
        if (IsOwner)
        {
            GameObject scoreTextObject = GameObject.Find("ScoreText");
            if (scoreTextObject != null)
            {
                scoreText = scoreTextObject.GetComponent<TextMeshProUGUI>();
            }
        }

    }

    private void Start()
    {
        // Initialize the main camera
        if (IsLocalPlayer) // Ensure this runs only on the local client
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("Main Camera not found. Please assign a Camera to the scene.");
            }
        }
    }

    private void Update()
    {
        if (IsOwner && scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        // Check if the player is the owner or the script by using useing isOwner, Ensure the application is focused and the player is local
        if (!IsOwner || !Application.isFocused || _mainCamera == null) return;

        // Get mouse input
        Vector3 mouseInput = Input.mousePosition;
        mouseInput.z = _mainCamera.nearClipPlane;

        // Convert mouse position to world coordinates
        Vector3 mouseWorldCoordinates = _mainCamera.ScreenToWorldPoint(mouseInput);

        // Move towards the mouse position
        transform.position = Vector3.MoveTowards(
            current: transform.position,
            target: mouseWorldCoordinates,
            maxDistanceDelta: Time.deltaTime * speed
        );

        // Rotate towards the mouse position
        if (mouseWorldCoordinates != transform.position)
        {
            Vector3 targetDirection = mouseWorldCoordinates - transform.position;
            targetDirection.z = 0f;
            transform.up = targetDirection;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsSpawned || !IsOwner || !other.CompareTag("Food"))
            return;

        if (other.TryGetComponent<NetworkObject>(out var food) && food.IsSpawned)
        {
            CollectFoodServerRpc(food.NetworkObjectId);
        }
    }

    [ServerRpc]
    private void CollectFoodServerRpc(ulong foodId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(foodId, out var food)
            && food.CompareTag("Food"))
        {
            food.Despawn();
            UpdateScoreClientRpc();
        }
    }
    [ClientRpc]
    private void UpdateScoreClientRpc()
    {
        if (!IsOwner)
            return;
        score++;
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    #region Customizeables
    public float speed = 8.0f;
    public float speedMultiplier = 1.0f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;
    #endregion

    #region State Variables
    public bool gameStarted = false;
    public Vector2 direction { get; private set; }
    public Vector2 preparedDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }
    public new Rigidbody2D rigidbody { get; private set; }
    #endregion

    private void Awake()
    {
        this.rigidbody = GetComponent<Rigidbody2D>();
        this.startingPosition = this.transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        this.speedMultiplier = 1.0f;
        this.direction = this.initialDirection;
        this.preparedDirection = Vector2.zero;
        this.transform.position = this.startingPosition;
        // This allows the us to enable the ghosts to leave their home initially without collision
        this.rigidbody.isKinematic = false;
        this.enabled = true;
    }

    private void Update()
    {
        if (this.preparedDirection != Vector2.zero && gameStarted)
        {
            SetDirection(this.preparedDirection);
        }
    }

    // Varies from normal update (happens every frame)
    // Called by Unity at a fixed time interval
    // Physics are almost always done in FixedUpdate
    private void FixedUpdate()
    {
        if (gameStarted)
        {
            Vector2 position = this.rigidbody.position;
            Vector2 translation = this.direction * this.speed * this.speedMultiplier * Time.fixedDeltaTime;

            this.rigidbody.MovePosition(position + translation);
        }
    }

    // Determines how both the player larva and the defenders choose a new direction to move
    public void SetDirection(Vector2 direction, bool forced = false)
    {
        if (forced || !Occupied(direction))
        {
            this.direction = direction;
            this.preparedDirection = Vector2.zero;
        }
        else
        {
            this.preparedDirection = direction;
        }
    }

    // Uses a boxcast to prevent clipping and ensures propper turn timing
    public bool Occupied(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position, Vector2.one * 0.75f, 0.0f, direction, 1.5f, this.obstacleLayer);
        return hit.collider != null;
    }

    public void StartGame()
    {
        // Called by the Game Manager to enable movement
        gameStarted = true;
    }

    public void EndGame()
    {
        // Called by the Game Manager to disable movement
        gameStarted = false;
    }
}

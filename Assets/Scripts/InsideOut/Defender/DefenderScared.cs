using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderScared : DefenderBehavior
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer vulnerable;
    public SpriteRenderer recovering;
    public InsideOutPlayer larva;

    public bool eaten { get; private set; }

    public override void Enable(float duration)
    {
        base.Enable(duration);

        this.body.enabled = false;
        this.eyes.enabled = false;
        this.vulnerable.enabled = true;
        this.recovering.enabled = false;

        Invoke(nameof(Flash), duration / 2.0f);
    }

    public override void Disable()
    {
        base.Disable();

        this.body.enabled = true;
        this.eyes.enabled = true;
        this.vulnerable.enabled = false;
        this.recovering.enabled = false;
        this.larva.empowered = false;
    }

    private void Flash()
    {
        if (!this.eaten)
        {
            this.eyes.enabled = false;
            this.vulnerable.enabled = false;
            this.recovering.enabled = true;
            this.recovering.GetComponent<SpriteAnimator>().Restart();
        }
    }

    // Instantly moves to home base, look into the movement being gradual.
    public void Eaten()
    {
        this.eaten = true;

        Vector3 position = this.defender.home.inside.position;
        position.z = this.defender.transform.position.z;
        this.defender.transform.position = position;

        this.defender.home.Enable(this.duration);

        this.body.enabled = false;
        this.eyes.enabled = true;
        this.vulnerable.enabled = false;
        this.recovering.enabled = false;

        Invoke(nameof(OnEnable), duration);
        Invoke(nameof(Flash), duration);
    }

    private void OnEnable()
    {
        this.defender.movement.speedMultiplier = 0.5f;
        this.eaten = false;
    }

    private void OnDisable()
    {
        this.defender.movement.speedMultiplier = 1.0f;
        this.eaten = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Larva"))
        {
            if (this.enabled)
            {
                Eaten();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled)
        {
            Vector2 direction = Vector2.zero;
            float maxDistance = float.MinValue;

            foreach (Vector2 availableDirection in node.availableDirections)
            {
                Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
                float distance = (this.defender.parasite.position - newPosition).sqrMagnitude;

                if (distance > maxDistance)
                {
                    direction = availableDirection;
                    maxDistance = distance;
                }
            }

            this.defender.movement.SetDirection(direction);
        }
    }
}

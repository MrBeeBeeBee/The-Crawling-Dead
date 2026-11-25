using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderChase : DefenderBehavior
{
    private void OnDisable()
    {
        this.defender.scatter.Enable();
    }

    // All defenders have the same chase behavior. In OG pacman, each ghost has it's own.
    // One guns for Pac Man, one aims for right behind him, one guns for in front of him
    // but backs off when it gets close.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled && !this.defender.scared.enabled)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;

            foreach (Vector2 availableDirection in node.availableDirections)
            {
                Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
                float distance = (this.defender.parasite.position - newPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    direction = availableDirection;
                    minDistance = distance;
                }
            }

            this.defender.movement.SetDirection(direction);
        }
    }
}

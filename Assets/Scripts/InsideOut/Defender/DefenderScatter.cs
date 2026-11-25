using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderScatter : DefenderBehavior
{
    private void OnDisable()
    {
        this.defender.chase.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled && !this.defender.scared.enabled)
        {
            int index = Random.Range(0, node.availableDirections.Count);

            if (node.availableDirections[index] == -this.defender.movement.direction && node.availableDirections.Count > 1)
            {
                index++;

                if (index >= node.availableDirections.Count)
                {
                    index = 0;
                }
            }

            this.defender.movement.SetDirection(node.availableDirections[index]);
        }
    }
}

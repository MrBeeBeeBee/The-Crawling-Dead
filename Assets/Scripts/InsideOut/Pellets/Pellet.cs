using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    public float points = 10.0f;

    // Protected, not private, so subclasses can access it.
    // Virtual allows it to be overrided, as it will also be used but
    // slightly changed for LargePellet
    protected virtual void Eat()
    {
        FindObjectOfType<InsideOutGameManager>().PelletEaten(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Larva"))
        {
            Eat();
        }
    }
}

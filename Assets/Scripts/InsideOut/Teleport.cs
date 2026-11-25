using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform destination;
    public InsideOutPlayer larva;
    public KeyCode requiredInput1;
    public KeyCode requiredInput2;

    private void OnTriggerStay2D(Collider2D other)
    {
        // Only allows teleporting if the collider detects the player
        if(other.gameObject.layer == 6 && this.larva.empowered == true && (Input.GetKey(requiredInput1) || Input.GetKey(requiredInput2)))
        {
            Vector3 position = other.transform.position;
            position.x = this.destination.position.x;
            position.y = this.destination.position.y;
            other.transform.position = position;
        }
    }
}

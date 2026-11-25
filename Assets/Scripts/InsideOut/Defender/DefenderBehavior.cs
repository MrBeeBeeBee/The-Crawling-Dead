using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Abstract means that this cannot be a class on an object alone, you must use
// a class that inherits from this one. (Scared, chase, scatter, home)
[RequireComponent(typeof(Defender))]
public abstract class DefenderBehavior : MonoBehaviour
{
    public Defender defender { get; private set; }
    public float duration;

    private void Awake()
    {
        this.defender = GetComponent<Defender>();
        this.enabled = false;
    }

    public void Enable()
    {
        Enable(this.duration);
    }


    // Allows for the power pellets to determine the duration of the scared state
    public virtual void Enable(float duration)
    {
        this.enabled = true;

        // Ensures the duration is reset when Enable is called again
        CancelInvoke();
        Invoke(nameof(Disable), duration);
    }

    public virtual void Disable()
    {
        this.enabled = false;

        CancelInvoke();
    }
}

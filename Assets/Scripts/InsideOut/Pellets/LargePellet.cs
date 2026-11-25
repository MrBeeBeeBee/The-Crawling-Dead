using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargePellet : Pellet
{
    public float aggressionDuration = 8.0f;

    protected override void Eat()
    {
        FindObjectOfType<InsideOutGameManager>().LargePelletEaten(this);
    }
}

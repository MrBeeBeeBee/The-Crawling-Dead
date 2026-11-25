using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : MonoBehaviour
{
    public Movement movement { get; private set; }
    public DefenderHome home { get; private set; }
    public DefenderScatter scatter {get; private set;}
    public DefenderChase chase { get; private set; }
    public DefenderScared scared { get; private set; }

    public DefenderBehavior initialBehavior;
    public Transform parasite;
    public int defenderPoints = 200;

    private void Awake()
    {
        this.movement = GetComponent<Movement>();
        this.home = GetComponent<DefenderHome>();
        this.scatter = GetComponent<DefenderScatter>();
        this.chase = GetComponent<DefenderChase>();
        this.scared = GetComponent<DefenderScared>();
    }

    public void ResetState()
    {
        this.gameObject.SetActive(true);
        this.movement.ResetState();

        this.scared.Disable();
        this.chase.Disable();
        this.scatter.Enable();
        
        if (this.home != this.initialBehavior)
        {
            this.home.Disable();
        }

        if (this.initialBehavior != null)
        {
            this.initialBehavior.Enable();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Larva"))
        {
            if (this.scared.enabled)
            {
                FindObjectOfType<InsideOutGameManager>().DefenderDies(this);
            }
            else
            {
                FindObjectOfType<InsideOutGameManager>().LarvaDies();
            }
        }
    }
}

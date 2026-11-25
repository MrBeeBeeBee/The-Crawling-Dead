using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PestControlPlayer : MonoBehaviour
{
    public PestControlGameManager pcgm { get; private set; }

    [Header("Player Data")]
    // Stores player traits
    [SerializeField] public Dictionary<string, bool> traits = new Dictionary<string, bool>();
    public string playerInput;

    private void Awake()
    {
        this.pcgm = GetComponent<PestControlGameManager>();
    }
}

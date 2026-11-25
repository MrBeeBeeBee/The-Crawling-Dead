using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    #region Sprite Info
    // Other scripts might want to read it = {get; private set;}
    public SpriteRenderer spriteRenderer { get; private set; }
    public Sprite[] sprites;
    #endregion

    #region Animation Info
    public float animationCooldown = 0.25f;
    public int animationFrame { get; private set; }
    public bool loop = true;
    #endregion

    private void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating("Advance", this.animationCooldown, this.animationCooldown);
    }

    private void Advance()
    {
        if (!this.spriteRenderer.enabled)
        {
            return;
        }

        this.animationFrame++;

        if (this.animationFrame >= this.sprites.Length && this.loop)
        {
            this.animationFrame = 0;
        }

        if (this.animationFrame >= 0 && this.animationFrame < this.sprites.Length)
        {
            this.spriteRenderer.sprite = this.sprites[this.animationFrame];
        }
    }

    public void Restart()
    {
        this.animationFrame = -1;

        Advance();
    }
}
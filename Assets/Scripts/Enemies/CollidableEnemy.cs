using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidableEnemy : MonoBehaviour
{
    private Enemy enemy;
    [SerializeField] public SpriteRenderer spriteRenderer;
    public void Init(Enemy enemy)
    {
        this.enemy = enemy;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        enemy.OnCollision(collision);
    }
}

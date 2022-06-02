﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TowerStats
{
    public int cost;
    public int sellAmount;
    public double speed;
    public int damage;
    public double range;
    public double special;
    public bool hasSpecial;

    public TowerStats(int cost, int sellAmount, double speed, int damage, double range, double special, bool hasSpecial)
    {
        this.cost = cost;
        this.sellAmount = sellAmount;
        this.speed = speed;
        this.damage = damage;
        this.range = range;
        this.special = special;
        this.hasSpecial = hasSpecial;
    }
}
public enum TowerType
{
    PINEAPPLE_CANNON = 0, PINA_COLADA, GATLIN_PINEAPPLE, ACIDIC_JUICER, SLICE_THROWER, THORN_TOSSER, PINEAPPLE_WALL
}
public struct TowerInfo
{
    public string name;
    public string description;
    public TowerStats towerStats;

    public TowerInfo(string name, string description, TowerStats towerStats)
    {
        this.name = name;
        this.description = description;
        this.towerStats = towerStats;
    }
}
public abstract class Tower : TickableObject {
    public GameObject tower;
    public int level;
    public ProjectileDamageReturn projectileDamageReturn;
    public ProjectileType projectileType;
    public TowerStats[] towerStats = new TowerStats[5];
    public TowerStats currentStats;
    public TowerStats upgradeStats;
    public TowerType type;
    public TowerRadius towerRadius;
    public bool maxUpgrade;
    public Timer attackTimer;
    public List<Enemy> targets;
    public GameObject closestEnemy;
    public float pivotSpeed;
    public virtual bool Upgrade()
    {
        if (maxUpgrade)
        {
            return false;
        }
        level++;
        currentStats = upgradeStats;
        if (level == 4)
        {
            maxUpgrade = true;
            upgradeStats = new TowerStats(0, 0, 0, 0, 0, 0, currentStats.hasSpecial);
        }
        else
        {
            upgradeStats = towerStats[level + 1];
        }

        attackTimer.SetTimerLength((float)currentStats.speed);
        projectileDamageReturn.damage = currentStats.damage;
        towerRadius.radius.localScale = Vector3.one * (float)currentStats.range;
        projectileDamageReturn.special = (float)currentStats.special;
        towerRadius.levelIndicator.text = "" + (level + 1);
        return true;
    }
    public virtual void AddEnemyToList(Enemy enemy)
    {
        targets.Add(enemy);
    }
    public virtual void Sell()
    {
        main.RemoveTowerFromList(this);
        Destroy();
    }
    public virtual void Destroy()
    {
        main.RemoveGameObject(tower);
    }
    public TowerStats GetTowerStats()
    {
        return currentStats;
    }
    public TowerStats GetUpgradeStats()
    {
        return upgradeStats;
    }

    public virtual void ToggleTowerLevel(bool isActive)
    {
        towerRadius.towerLevel.SetActive(isActive);
    }
    public virtual void ToggleTowerRadiusDisplay(bool isActive)
    {
        towerRadius.towerRadiusDisplay.SetActive(isActive);
    }

    public override void Disable()
    {
        tower.SetActive(false);
    }
    public virtual void TrackEnemies()
    {
        closestEnemy = null;
        float closestDistance = 9999999f;
        for (int i = 0; i < targets.Count; i++)
        {
            var enemy = targets[i].enemy;
            var distance = Vector2.Distance(tower.transform.position, enemy.transform.position);
            if (!enemy.activeInHierarchy || distance > currentStats.range)
            {
                targets.RemoveAt(i);
                i--;
                continue;
            }
            if (distance < closestDistance)
            {
                closestEnemy = enemy;
            }
        }
        if (closestEnemy != null)
        {
            var target = closestEnemy.transform.position;
            var angle = Mathf.Atan2(target.y - towerRadius.pivot.position.y, target.x - towerRadius.pivot.position.x) * Mathf.Rad2Deg + 90;
            var targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            towerRadius.pivot.rotation = Quaternion.RotateTowards(towerRadius.pivot.rotation, targetRotation, pivotSpeed * Time.deltaTime);
        }
    }
    public virtual void Attack()
    {
        if (closestEnemy != null)
        {
            towerRadius.animator.ResetTrigger("Fire");
            towerRadius.animator.SetTrigger("Fire");
            main.SpawnProjctileFromPool(projectileType, towerRadius.projectileSpawn.position, towerRadius.pivot.rotation, projectileDamageReturn);
        }
    }
    public override void Tick()
    {
        TrackEnemies();
        attackTimer.Tick();
        if (attackTimer.Status())
        {
            attackTimer.ResetTimer();
            Attack();
        }
    }
}

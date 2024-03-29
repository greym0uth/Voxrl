﻿using UnityEngine;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour {

    public Ability[] abilities;

    public Stats stats;

    public List<Status> currentStatus = new List<Status>();

    public Entity owner;

    protected virtual void Start() {
        abilities = new Ability[6];
        stats = GetComponent<Stats>();
        owner = GetComponent<Entity>();
    }
    
    protected virtual void Update() {
        List<Status> rmvQueue = new List<Status>();
        foreach (Status i in currentStatus) {
            i.timeLeft -= Time.deltaTime;
            if (i.Fisished)
                rmvQueue.Add(i);
        }
        foreach (Status i in rmvQueue) {
            currentStatus.Remove(i);
        }

        foreach (Ability a in abilities) {
            if (a != null)
                a.Update();
        }
    }

    protected virtual void UseAbility(int index) {
        Ability a = abilities[index];
        if (a != null) {
            if (a.available) {
                Logger.Instance.Log("Used Ability " + index);
                if (index == 0)
                    owner.RegularAttack();
                Effect[] efs = a.GenerateEffects();
                if (a.selfAfflict)
                    this.Effected(efs);
                else {
                    GameObject[] hits = a.GetEffectedObjects(this.transform);
                    foreach (GameObject i in hits) {
                        if (i.tag == "Mob") {
                            BattleSystem mob = i.GetComponent<BattleSystem>();
                            mob.Effected(efs);
                        }
                    }
                }
                a.Reset();
            }
        }
    }

    public virtual void Effected(Effect[] effects) {
        foreach (Effect i in effects) {
            if (i.statsBased)
                stats.ApplyEffect(i);
            if (i.transformBased) {
                i.ApplyEffect(this.transform);
            }
            if (i.invokesStatus) {
                this.InvokeStatus(i.status);
            }
        }
    }

    public virtual void InvokeStatus(Status s) {
        currentStatus.Add(s);
    }

    public virtual void InvokeStatus(List<Status> s) {
        foreach (Status i in s)
            InvokeStatus(i);
    }

    public virtual void InvokeStatus(StatusType type, float percentage, float time) {
        InvokeStatus(new Status(type, percentage, time));
    }

    public virtual void AwardKill(int exp) {
        this.stats.experience += exp;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMovement : MonoBehaviour {

    // Character States
    enum State { Dead, CrawlUp, Idle, Walking, Attacking };
    State state = State.CrawlUp;

    // Navigation
    UnityEngine.AI.NavMeshAgent navAgent;
    private Transform playerTf;
    
    // Animation
    Animator anim;

    private float timeCounter=0.0F;
    float crawlUpTime = 3.0F;
    float standToRunTime = 1.0F;
    float attackTime = 1.5F;

    float jumpDistance = 22.0F;


    // Use this for initialization
    void Start () {
        // Initializing Components
        playerTf = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
        navAgent.Stop();

        //Setting States
        anim.SetInteger("AnimationState", 0);
        state = State.CrawlUp;
    }
	
	// Update is called once per frame
	void Update () {
        if (state != State.Dead)
        {
            navAgent.destination = playerTf.position;
        }

        float distanceAway = (navAgent.transform.position - playerTf.position).sqrMagnitude;

        if (distanceAway <= jumpDistance && state == State.Walking) {
            PreAttackJump();
            timeCounter = 0;
        }
        else if (timeCounter >= attackTime && state == State.Attacking)
        {
            AttackCompletion();
        }

        //Timing Related Logic
        if (timeCounter >= crawlUpTime && state == State.CrawlUp)
        {
            StandToWalkTransition();
            timeCounter = 0;

        } else if (timeCounter >= standToRunTime && state == State.Idle)
        {
            StartChase();
            timeCounter = 0;

        } else if (state == State.Dead)
        {
            Death();
            timeCounter = 0;
        }

        timeCounter += Time.deltaTime;
    }

    void StandToWalkTransition()
    {
        state = State.Idle;
        anim.SetInteger("AnimationState", 1);
    }

    void StartChase ()
    {
        state = State.Walking;
        anim.SetInteger("AnimationState", 2);
        navAgent.Resume();
    }

    // For Debug only, remove the character immediately from scene
    void StopChase()
    {
        state = State.Idle;
        anim.SetInteger("AnimationState", 1);
        navAgent.Stop();
    }

    void PreAttackJump()
    {
        state = State.Attacking;
        anim.SetInteger("AnimationState", 3);
    }

    void AttackCompletion()
    {
        //Reset humanoid to non-moving state
        state = State.Idle;
        anim.SetInteger("AnimationState", 1);
        navAgent.Stop();

        //Deal Damage to the player
        //TODO

        //Remove humanoid from scene after attack
        Destroy(gameObject, 0);
    }


    public void Death ()
    {
        navAgent.Stop();
        navAgent.ResetPath();
        navAgent.enabled = false;
        state = State.Dead;
        anim.SetInteger("AnimationState", -1);
        Destroy(gameObject, 5);
    }



}

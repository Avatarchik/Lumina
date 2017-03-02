﻿using UnityEngine;
using System.Collections;

public class BatEnemy : BaseEnemy
{

    public Transform target;                //The player, for the slime to face and move toward
    public int thrust;                      //The amount of force used to propel the slime forward
    public bool isAttacking;				//If the enemy is currently attacking, return true
    public float timeBetweenAttacks;        //How fast the slime jumps at you
    public float flightHeight;              //Height above the ground we want the bat to float
    public float flightThrust;              //Thrust power the bat will use to fly

    private Vector3 startPos;               //The slimes starting position
    private Rigidbody rb;                   //The slimes rigidbody which allows us to propel it
    private float changeDirectionCount = 0; //The time counter for the slime to change direction while the player isn't around
    Coroutine attackMethod;

    //Variable initialization 
    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        StartCoroutine(MovementPattern());
    }

    /* The attack method is an IEnumarator to allow the slime to wait before executing the attack
	 * The slime will then propel itself towards the player
	 * It will wait a couple of seconds before moving again
	 */
    public override IEnumerator Attack()
    {
        isAttacking = true;

        while (isAttacking)
        {
            yield return new WaitForSeconds(timeBetweenAttacks); //Wait a single second before attack
            rb.AddForce(transform.forward * thrust * 2);
            yield return new WaitForSeconds(timeBetweenAttacks/2); //Wait a single second before attack
            rb.AddForce(transform.forward * thrust/5 + Vector3.up * thrust/5);
        }

        // If still attacking, attack again
        //if (isAttacking) StartCoroutine(Attack());
        //else StopCoroutine(Attack());
    }

    public IEnumerator MovementPattern()
    {
        //This coroutine randomizes the bats upward thrust for flying giving it a more eratic flight pattern for realism
        //The Movement () method just makes the bat fly forward now
        //Called from start() so as to not start the coroutine every frame
        while (true)
        {
            yield return new WaitForSeconds(0.15f);
            RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, Vector3.down, 100f);
            foreach (RaycastHit hit in raycastHits)
            {
                if (hit.transform.tag == "Ground")
                {
                    if (transform.position.y - hit.point.y < flightHeight)
                    {
                        float thrust = Random.Range(3 * flightThrust / 4, flightThrust);
                        rb.AddForce(Vector3.up * thrust);
                        //Debug.Log(thrust);
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    /* This is a simple movement method, 
	 * it looks in the players direction and moves that way constantly
	 */
    public override void Movement()
    {
        if (target != null)
        {
            if (isAttacking)
            {
                transform.LookAt(target);

                // They will start moving slower if they want to attack you! Good way of knowing when they spotted you
                rb.AddForce(transform.forward * movementSpeed * 2);

                /*RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, Vector3.down, 100f);
                foreach (RaycastHit hit in raycastHits)
                {
                    if (hit.transform.tag == "Ground")
                    {
                        if (transform.position.y - hit.point.y < flightHeight)
                        {
                            rb.AddForce(Vector3.up * Random.Range(flightThrust / 2, flightThrust));
                        }
                    }
                }*/

                if (/* You want to stop attacking, say you are mid air or low health*/false)
                {
                    isAttacking = false;
                }
            }
            else
            {
                if (/* You want to start attacking, as in you are on the floor or health regenerated*/false)
                {
                    isAttacking = true;
                }
            }


        }

        else if (target == null)
        {
            rb.AddForce(transform.forward * movementSpeed * 5);

            /*RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, Vector3.down, 100f);
            foreach (RaycastHit hit in raycastHits)
            {
                if (hit.transform.tag == "Ground")
                {
                    if(transform.position.y - hit.point.y < flightHeight)
                    {
                        rb.AddForce(Vector3.up * flightThrust);
                    }
                }
            }*/

            //Change direction randomly every 4 seconds
            changeDirectionCount += Time.deltaTime;
            if (changeDirectionCount > 4f)
            {
                transform.rotation = Quaternion.Euler(0, Random.Range(-360, 360), 0);
                changeDirectionCount = 0;
            }
        }

    }

    /* This access the slime's Sphere Collider where there is a trigger 
	 * The trigger is the range of the slime's attack and will call the Attack coroutine 
	 * when triggered
	 */
    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && !isAttacking)
        {
            target = col.transform;
            isAttacking = true;
            attackMethod = StartCoroutine(Attack());
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.transform == target)
        {
            target = null;
            isAttacking = false;
            StopCoroutine(attackMethod);
        }
    }
}
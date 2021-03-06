﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
	public static int SPRINT_MAX = 3;
	public static int SPRINT_COOLDOWN = 3;
	public static float MAX_X_SPEED = 4;
	public static float MAX_Z_SPEED = 6;
    public static float JUMP_POWER = 400;


    CapsuleCollider playerCollider;
	Rigidbody playerPhysics;

	float distToGround; //distance from the center of the player to the bottom of their hitbox
	float lastJump; //the time since the last jump
	float sprintTime; //the elapsed time player has been sprinting
	float sprintRecharge; //the amount of time left before player can sprint again
	Vector3 rotationVector;//vector used to set and maintain the rotation of the player and the camera

	public bool isJumping;
	public bool isSprinting;
    public bool canMove = true;
    public bool blocked;
	public GameObject cameraObj;
    public Animator viewmodelAnimator;


    // Use this for initialization
    void Start () {
		playerPhysics = GetComponentInParent<Rigidbody> ();
		playerCollider = GetComponentInParent<CapsuleCollider> ();

		distToGround = playerCollider.bounds.extents.y/2.5f;
        //print(distToGround);
		lastJump = 0;
		sprintTime = 0;
		sprintRecharge = 0;

		isJumping = false;
		isSprinting = false;
	}

	/*
	* Setting the movement and applying the force to the player
	* fb - the foward and backward movement from the axis
	* lr - the left and right movement from the keyboards
	*/
	public void SetMovement(float lr, float fb, bool sprintPressed){


        viewmodelAnimator.SetBool("Running", this.isSprinting && (lr != 0 || fb != 0) && IsGrounded() && !blocked);
        viewmodelAnimator.SetBool("Walking", (lr != 0 || fb != 0) && IsGrounded() && !blocked);
		UpdateCooldowns ();
		ApplyHorizontalMovement (lr, fb, sprintPressed);
		if (isJumping) {
            if (!canMove) return;
            if (IsGrounded()) {
				playerPhysics.AddForce (new Vector3 (0, JUMP_POWER, 0));
				lastJump = 0;
			} else {
				isJumping = false;
			}
		}
	}

	/**
	*Getting the mouse movements and move your camera
	*/
	public void MoveCamera(float x, float y){
        if (!canMove) return;

        float newDeltaX = -y * 5 + (rotationVector.x * -1);
        float newX = cameraObj.transform.rotation.eulerAngles.x + newDeltaX;
        //We do some fancy math to ensure 0 < newX < 360, nothing more
        newX =(newX+360) % 360;
        //Ensure it doesn't go past our top or low bounds
        if ((newX > 0 && newX < 90) || (newX < 360 && newX > 270)) {
            // Camera rotation
            cameraObj.transform.Rotate(newDeltaX, rotationVector.y, rotationVector.z);
        } else {
            // We don't want you to look all the way behind you, that's weird
        }

        // Body rotate
        gameObject.transform.Rotate (0, x * 5, 0);

    }

	/**
	 * Separate function for movement in the horizontal direction.
	 */
	private void ApplyHorizontalMovement(float x, float z, bool sprintPressed){
        if (!canMove) return;

		ApplySprint (sprintPressed);

		float sprintModifier = isSprinting ? 1.5f : 1f;
        float airborneModifier = IsGrounded() ? 1 : .75f;

        if (!(Math.Abs(playerPhysics.velocity.x) >= MAX_X_SPEED*sprintModifier)) {
            Vector3 desiredPosition = transform.position + (transform.right * x * Time.deltaTime * sprintModifier * airborneModifier * 10);
            if (canMoveTo(desiredPosition)) {
                transform.position = desiredPosition;
            }
        }
		if (!(Math.Abs (playerPhysics.velocity.z) >= MAX_Z_SPEED*sprintModifier)) {
            Vector3 desiredPosition = (transform.position + transform.forward * z * Time.deltaTime * sprintModifier * airborneModifier * 10);
            if (canMoveTo(desiredPosition)) {
                transform.position = desiredPosition;
            }
        }
    }

	/**
	 * Checks to see if player is touching the ground. Raycasts below the player with a margin of 0.2 (units?)
	 * below the bottom of the player's bounding box in order to account for irregularities in the ground, where
	 * ground may not be seen otherwise.
	 * 
	 * added in a time period that a person has to wait before using the raycast again
	 */
	private bool IsGrounded(){ 
		if (lastJump < 0.5f ) {
			return false;
		}
		return Physics.Raycast (this.transform.position, Vector3.down, distToGround);
	}

	/**
	 * Whether or not the player is allowed to sprint
	 */
	private bool CanSprint(){
		return sprintTime < SPRINT_MAX && !(sprintRecharge > 0);
	}

	/**
	 * Updates whether the player is sprinting or not based on if the key is pressed
	 * and if the player is allowed to sprint
	 */
	private void ApplySprint(bool sprintPressed){
		if (sprintPressed) {
			if (!isSprinting && CanSprint()) {
				isSprinting = true;
			}
			if (isSprinting && !CanSprint()) {
				isSprinting = false;
				sprintRecharge = (sprintTime / SPRINT_MAX) * SPRINT_COOLDOWN;
				sprintTime = 0;
			}
		} else {
			if (isSprinting) {
				isSprinting = false;
				sprintRecharge = (sprintTime / SPRINT_MAX) * SPRINT_COOLDOWN;
				sprintTime = 0;
			}
		}
	}

	/**
	 * Updates the sprint time and the recharge time to sprint and the time since the last jump
	 */
	private void UpdateCooldowns() {
		if (isSprinting) {
			sprintTime += Time.deltaTime;
		}
		if (sprintRecharge > 0) {
			sprintRecharge -= Time.deltaTime;
			if (sprintRecharge < 0) {
				sprintRecharge = 0;
			}
		}
		lastJump += Time.deltaTime;
	}

    /**
     * Broadcasted by stats controller 
     */
    void Death() {
        canMove = false;
        cameraObj.transform.eulerAngles = new Vector3(0, cameraObj.transform.eulerAngles.y, cameraObj.transform.eulerAngles.z);
        // Stop friggin flying across the friggin place
        playerPhysics.isKinematic = true;
    }
    void NotDeath() {
        
        playerPhysics.isKinematic = false;
        canMove = true;
    }

    private bool canMoveTo(Vector3 pos) {
        //RenderVolume(pos + Vector3.up * (playerCollider.height / 2), pos - Vector3.up * (playerCollider.height / 2), playerCollider.radius, Vector3.forward, playerCollider.radius);
        //RaycastHit[] hits = Physics.CapsuleCastAll(pos + Vector3.up * (playerCollider.height / 2), pos - Vector3.up * (playerCollider.height / 2), playerCollider.radius/2, Vector3.forward);

        //RaycastHit[] hits = Physics.BoxCastAll(pos, Vector3.one, Vector3.forward/100000);
        Vector3 dir = pos - transform.position;
        float dist = Vector3.Distance(pos, transform.position);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, dist + 1.3f);
        Debug.DrawLine(transform.position, dir);
        foreach (RaycastHit hit in hits) {
            if (!hit.collider.isTrigger && hit.collider.gameObject != this.gameObject) {
                print(hit.transform);
                blocked = true;
                return false;
            }
            
        }
       
        blocked = false;
        return true;
    }
    
}
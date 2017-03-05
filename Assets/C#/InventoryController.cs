﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour {

    private float interactCooldown;
	Vector3 cameraAim;
	RaycastHit hitObj;//the hopefully raycast of an item that it finds

	void Start () {
		interactCooldown = 1;
	}
    public void Interact(bool value)
    {
        // If value is true, pick up
        if (value)
        {
            if (Time.timeSinceLevelLoad - interactCooldown > 1)
            {
                interactCooldown = Time.timeSinceLevelLoad;
            }

			cameraAim = GetComponentInChildren<Camera> ().transform.rotation.eulerAngles;
			if (Physics.Raycast (this.transform.position, cameraAim,out hitObj)) {
				if (hitObj.collider.gameObject.tag == "Weapon") {
					Inventory I = gameObject.GetComponentInParent<Inventory> ();
					InventoryItem Get = hitObj.collider.gameObject.GetComponentInParent<Weapon> ();
					I.addToInventory (Get);
					Debug.Log ("player is Grabbing Weapon "+Get);
				}
			}
        } 

    }

    private void OnTriggerEnter(Collider other)
    {
		if(other.gameObject.GetComponentInParent<Potion>()!= null){
			Inventory I = gameObject.GetComponentInParent<Inventory> ();
			InventoryItem Get = other.gameObject.GetComponentInParent<Potion> ();
			I.addToInventory (Get);
			Debug.Log ("Player sucked up "+Get);
		}
    }
}

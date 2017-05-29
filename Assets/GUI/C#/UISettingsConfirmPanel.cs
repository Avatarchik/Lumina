﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISettingsConfirmPanel : UIConfirmPanel {

    public UIEffect effect;

    private Animator anim;

    [HideInInspector] public SettingsController controller;



    private void Awake()
    {
        anim = GetComponent<Animator>();

    }

    public override void Open()
    {
        //no more navigation, have to confirm first
        controller.navPanel.interactable = false;

        //set controller
        controller.currentConfirmPanel = this;

        //open
        gameObject.SetActive(true);
        anim.SetTrigger("Open");


    }

    public override void Close()
    {
        //close
        anim.SetTrigger("Close");

        //set controller
        controller.currentConfirmPanel = null;
    }

    public override void OnNoClick()
    {
        //make nav interactable again
        controller.navPanel.interactable = true;

        //close
        Close();
    }

    public override void OnYesClick()
    {
        //trigger UI animation
        effect.Play();

        //resume, make sure thing works
        Time.timeScale = 1;
    }

    

  
}

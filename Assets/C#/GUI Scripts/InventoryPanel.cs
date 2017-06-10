﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : UIPanel
{

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Close()
    {
        anim.SetTrigger("Close");
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        anim.SetTrigger("Open");
    }
}

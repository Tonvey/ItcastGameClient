using System;
using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;

public class BattleSceneController : SceneController
{
    protected override void Update()
    {
        base.Update();
        if(GameEventManager.TestEvent!=null)
            GameEventManager.TestEvent();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq.Expressions;
using V8.Net;

public class JsBehaviour : MonoBehaviour
{
    public TextAsset jsScript;

    internal static V8Engine engine = new V8Engine(); //all js behaviour shared one jsenv only!

    private Handle jsBehaviour;

    void Awake()
    {
        engine.RegisterType<Debug>(null, true, ScriptMemberSecurity.Locked); // (this line is NOT required, but allows more control over the settings)
        engine.GlobalObject.SetProperty(typeof(Debug));
        engine.Execute(
            @"
                Debug.Log('dddd');
              "
            );
        jsBehaviour = engine.Execute(jsScript.text);
    }

    void Start()
    {
        jsBehaviour.InternalHandle.Call("Start", null);
    }

    void Update()
    {
        jsBehaviour.InternalHandle.Call("Update", null);
    }

    void OnDestroy()
    {
        jsBehaviour.InternalHandle.Call("OnDestroy", null);
    }
}

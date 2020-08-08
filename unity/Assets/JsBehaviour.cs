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

        engine.RegisterType(typeof(Vector2), null, true, ScriptMemberSecurity.Locked); // (this line is NOT required, but allows more control over the settings)
        engine.GlobalObject.SetProperty(typeof(Vector2));
        var v = Vector2.zero.ToString();

        engine.Execute(
            @"
                Debug.Log('dddd');
                Debug.Log(typeof Vector2);
                for(var i = 0;i<100;i++){
                    Vector2.zero.ToString();
                }
                Debug.Log(Vector2.zero);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq.Expressions;
using V8.Net;
using System.Text;
using System.Reflection;

public class JsBehaviour : MonoBehaviour
{
    public TextAsset jsScript;

    internal static V8Engine engine = new V8Engine();

    private Handle jsBehaviour;

    internal static bool isInited = false;

    string ReadFile(string path)
    {
        var strs = File.ReadAllLines(path);
        StringBuilder sb = new StringBuilder();
        foreach (var s in strs)
        {
            sb.Append(s);
            sb.Append("\n");
        }

        return sb.ToString();
    }

    void initEnvroment()
    {
        if (!isInited)
        {
            var b = Assembly.Load("UnityEngine").GetTypes();
            Debug.Log(b.Length);
            foreach (var t in b)
            {
                Debug.Log(t);
            }

            var a = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var t in a)
            {
                Debug.Log(t);
            }
        }
    }

    void Awake()
    {
        initEnvroment();

        engine.RegisterType<Debug>(null, true, ScriptMemberSecurity.Locked); // (this line is NOT required, but allows more control over the settings)
        engine.GlobalObject.SetProperty(typeof(Debug));

        engine.RegisterType(typeof(Vector2), null, true, ScriptMemberSecurity.Locked); // (this line is NOT required, but allows more control over the settings)
        engine.GlobalObject.SetProperty(typeof(Vector2));
        var v = Vector2.zero.ToString();

        engine.Execute(
            @"
                Debug.Log('dddd');
                Debug.Log(typeof Vector2);
                var d = (new Date()).getTime();
                for(var i = 0;i<1000;i++){
                    var b = {};
                }
                Debug.Log('using time: ' + ((new Date()).getTime()-  d));
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

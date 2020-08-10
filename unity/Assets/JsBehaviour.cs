using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using V8.Net;
using System.Text;
using Unity;
using System.Reflection;
using UnityEngine.SocialPlatforms;
using System.Data.SqlTypes;

public class JsBehaviour : MonoBehaviour
{
    public TextAsset jsScript;

    internal static V8Engine engine = new V8Engine();

    private Handle jsBehaviour;

    internal static bool isInited = false;

    private string binding_type_root = "CS";

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

    public static List<Type> GetAllTypes(bool exclude_generic_definition = true)
    {
        List<Type> allTypes = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            try
            {
                if (!(assemblies[i].ManifestModule is System.Reflection.Emit.ModuleBuilder))
                {
                    allTypes.AddRange(assemblies[i].GetTypes().Where(type =>
                        (exclude_generic_definition ? !type.IsGenericTypeDefinition : true) && type.FullName.StartsWith("UnityEngine.UI")
                    ));
                }
            }
            catch (Exception)
            {
            }
        }

        return allTypes;
    }

    bool registerType(Type type)
    {
        var name = type.Name;
        var si = name.IndexOf("`");
        if (si >= 0)
        {
            name = name.Substring(0, si);
        }

        var path = type.FullName.Split('.');
        var root = engine.GlobalObject.GetProperty(binding_type_root);
        for (var i = 0; i < path.Length - 1; i++)
        {
            var p_name = path[i];
            var o = root.GetProperty(p_name);
            if (o.IsUndefined)
            {
                root.SetProperty(p_name, engine.CreateObject());
                o = root.GetProperty(p_name);
            }

            root = o;
        }

        engine.RegisterType(type, name, true);
        root.SetProperty(type);

        return true;
    }

    void initEnvroment()
    {
        if (!isInited)
        {
            //var b = Assembly.Load("UnityEngine.UI").GetTypes();
            //Debug.Log(b.Length);
            //foreach (var t in b)
            //{
            //    Debug.Log(t);
            //}

            //var a = Assembly.GetExecutingAssembly().GetTypes();
            //foreach (var t in a)
            //{
            //    Debug.Log(t);
            //}

            engine.GlobalObject.SetProperty(binding_type_root, engine.CreateObject());

            var types = GetAllTypes();
            Debug.Log("All types count: " + types.Count);
            foreach (var type in types)
            {
                Debug.Log(type.FullName);
                registerType(type);
            }
            //Debug.Log(types[0].FullName);
            //registerType(types[0]);

          

        }
    }

    void Awake()
    {
        var type = typeof(UnityEngine.UIElements.MinMaxSlider.UxmlTraits);
        Debug.Log("type.IsGenericParameter: " + type.IsGenericParameter);
        Debug.Log("type.IsGenericType: " + type.IsGenericType);
        Debug.Log("type.IsGenericTypeDefinition: " + type.IsGenericTypeDefinition);
        Debug.Log("type.IsConstructedGenericType: " + type.IsConstructedGenericType);

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
                Debug.Log(typeof CS.UnityEngine.BlendWeights);
                var o = new CS.UnityEngine.BlendWeights();
                //Debug.Log(o);

                for(var key in o) {
                    Debug.Log(o[key]);
                }
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

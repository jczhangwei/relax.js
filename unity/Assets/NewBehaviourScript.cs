using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq.Expressions;
using V8.Net;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var arg = new string[2];
        V8Test.Main(arg);

    }

    // Update is called once per frame
    void Update()
    {

    }

    
}

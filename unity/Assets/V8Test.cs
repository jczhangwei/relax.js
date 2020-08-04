using V8.Net;
using UnityEngine;
using System;
using UnityEngine.Assertions.Must;
using System.Runtime.InteropServices;

public class Person : V8NativeObject
{
    public string name { get { return GetProperty("name"); } set { SetProperty("name", value); } }
    public string address { get { return GetProperty("address"); } set { SetProperty("address", value); } }
    public string dob { get { return GetProperty("dob"); } set { SetProperty("dob", value); } }
}

public class Company : V8NativeObject
{
    public bool isEnabled { get { return GetProperty("isEnabled"); } set { SetProperty("isEnabled", value); } }
    public Person[] staff;
}
public class Person2 // NOTE: Inheriting from V8NativeObject not required here (though still allowed).
{
    public string name { get; set; }
    public string address { get; set; }
    public string dob { get; set; }
}

public class Company2 // NOTE: Inheriting from V8NativeObject not required here (though still allowed).
{
    public bool isEnabled { get; set; }
    public Person2[] staff;

    public Company2(string a, string b, string c)
    {
        isEnabled = true;
        staff = new Person2[3] {
                new Person2 {
                    name = "John",
                    address = "1 Walk Way",
                    dob = "July "+a+", 1970"
                },
                new Person2 {
                    name = "Peter",
                    address = "241 Otoforwan Rd",
                    dob = "January "+b+", 1953"
                },
                new Person2 {
                    name = "Marry",
                    address = "1 Contrary Lane",
                    dob = "August "+c+", 1984"
                }
            };
    }
}

class V8Test
{
    public static void Main(string[] args)
    {
        V8Engine v8Engine = new V8Engine();

        var result = v8Engine.Execute(
            @"var gs = (function () {  // NOTE: 'result' WILL ALWAYS BE 'undefined' BECAUSE OF 'var' before 'gs'.
                    var myClassObject = function() {};
                    myClassObject.prototype.getCompanyInfo = function (a, b, c) {
                        var staff = [
                            {
                                'name': 'John',
                                'address': '1 Walk Way',
                                'dob': 'July '+a+', 1970'
                            },
                            {
                                'name': 'Peter',
                                'address': '241 Otoforwan Rd',
                                'dob': 'January '+b+', 1953'
                            },
                            {
                                'name': 'Marry',
                                'address': '1 Contrary Lane',
                                'dob': 'August '+c+', 1984'
                            }
                        ];
                        var result = {
                            'isEnabled': true,
                            'staff': staff
                        };
                        
                        return result;
                    };
                    return new myClassObject();
                })();"
        );

        v8Engine.RegisterType<Debug>(typeof(Debug).Name, true, ScriptMemberSecurity.Locked); // (this line is NOT required, but allows more control over the settings)
        v8Engine.GlobalObject.SetProperty(typeof(Debug));


        //create parameter
        Handle day1 = v8Engine.CreateValue("1");
        Handle day2 = v8Engine.CreateValue("2");
        Handle day3 = v8Engine.CreateValue("3");
        v8Engine.GlobalObject.SetProperty("zhangwei", v8Engine.CreateValue(DateTime.Now));
        var gs = v8Engine.GlobalObject.GetProperty("gs");
        var resultHandle = gs.Call("getCompanyInfo", null, day1, day2, day3); // NOTE: The object context is already known, so pass 'null' for '_this'.
        Company completion = v8Engine.GetObject<Company>(resultHandle);

        //examine result
        var test0 = resultHandle.GetProperty("isEnable");
        Handle test1 = resultHandle.GetProperty("staff"); // NOTE: "ObjectHandle" is a special handle for objects (which also obviously includes arrays, etc.).
        var arrayLength = test1._.ArrayLength;
        Handle arrayItem1 = test1._.GetProperty(0);
        var arrayItem1_name = arrayItem1._.GetProperty("name");
        var arrayItem1_address = arrayItem1._.GetProperty("address");
        var arrayItem1_dob = (~arrayItem1).GetProperty("dob");
        Handle arrayItem2 = test1._.GetProperty(1); // (arrays are treated same as objects here)
        Handle arrayItem3 = test1._.GetProperty(2); // (arrays are treated same as objects here)

        //  ==================================================================== OR  ====================================================================

        v8Engine.RegisterType<Company2>(typeof(Company2).Name, true, ScriptMemberSecurity.Locked); // (this line is NOT required, but allows more control over the settings)
        v8Engine.GlobalObject.SetProperty(typeof(Company2)); // <= THIS IS IMPORTANT! It sets the type on the global object (though you can put this anywhere like any property)

        var gs2 = v8Engine.Execute(
            @"(function () {
                    var myClassObject = function() {};
                    myClassObject.prototype.getCompanyInfo = function (a, b, c) {
                        return new Company2(a, b, c);
                    };
                    Debug.Log('ddddddd=');
                    return new myClassObject();
                })();"
        );

        var resultHandle2 = gs2.Call("getCompanyInfo", null, day1, day2, day3); // NOTE: The object context is already known, so pass 'null' for '_this'.
        var objectBindingModeIfNeeded = resultHandle2.BindingMode;
        var ci2 = (Company2)resultHandle2.BoundObject; // (when a CLR class is bound, it is tracked by the handle in a special way)
        Debug.Log(ci2.staff[0].name);

        //  =============================================================================================================================================
        // Take your pick. ;)

        Debug.Log("Script executions completed. Press any key to exit.");
    }
}

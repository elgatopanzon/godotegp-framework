namespace GodotEGP.Misc;

using Godot;
using System;
using System.Dynamic;
using System.Collections.Generic;

public partial class DynamicDictionary : DynamicObject
{
    // Inner dictionary object holding properties.
    Dictionary<string, object> dictionary
        = new Dictionary<string, object>();

    public int Count
    {
        get
        {
            return dictionary.Count;
        }
    }

	// Override method for non-existant properties
    public override bool TryGetMember(
        GetMemberBinder binder, out object result)
    {
        string name = binder.Name.ToLower();

        return dictionary.TryGetValue(name, out result);
    }

	// Override method when setting non-existant properties
    public override bool TrySetMember(
        SetMemberBinder binder, object value)
    {
        dictionary[binder.Name.ToLower()] = value;

        return true;
    }
}

using System;
using System.Reflection;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CustomNameAttribute : Attribute
{
    public string Name { get; private set; }

    public CustomNameAttribute(string name)
    {
        Name = name;
    }
}

public static class SerializationHelper
{
    public static string ObjectToString(object obj)
    {
        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var pairs = new List<string>();

        foreach (var field in fields)
        {
            var customNameAttr = field.GetCustomAttribute<CustomNameAttribute>();
            var name = customNameAttr != null ? customNameAttr.Name : field.Name;
            pairs.Add($"{name}:{field.GetValue(obj)}");
        }

        foreach (var property in properties)
        {
            var customNameAttr = property.GetCustomAttribute<CustomNameAttribute>();
            var name = customNameAttr != null ? customNameAttr.Name : property.Name;
            pairs.Add($"{name}:{property.GetValue(obj)}");
        }

        return string.Join(",", pairs);
    }

    public static void StringToObject(string str, object obj)
    {
        var pairs = str.Split(',');
        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var pair in pairs)
        {
            var parts = pair.Split(':');
            if (parts.Length != 2) continue;

            var name = parts[0];
            var value = parts[1];

            FieldInfo field = null;
            PropertyInfo property = null;

            foreach (var f in fields)
            {
                var customNameAttr = f.GetCustomAttribute<CustomNameAttribute>();
                if ((customNameAttr != null && customNameAttr.Name == name) || f.Name == name)
                {
                    field = f;
                    break;
                }
            }

            foreach (var p in properties)
            {
                var customNameAttr = p.GetCustomAttribute<CustomNameAttribute>();
                if ((customNameAttr != null && customNameAttr.Name == name) || p.Name == name)
                {
                    property = p;
                    break;
                }
            }

            if (field != null)
            {
                field.SetValue(obj, Convert.ChangeType(value, field.FieldType));
            }
            else if (property != null)
            {
                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
            }
        }
    }
}


public class MyClass
{
    [CustomName("CustomFieldName")]
    public int I = 0;
    public string Name = "NoName";
}

class Program
{
    static void Main()
    {
        MyClass myObject = new MyClass();
        myObject.I = 42;
        myObject.Name = "Иван";

        string serialized = SerializationHelper.ObjectToString(myObject);
        Console.WriteLine(serialized);

        MyClass newObject = new MyClass();
        SerializationHelper.StringToObject("", serialized);
        Console.WriteLine($"New object values: I={newObject.I}, Name={newObject.Name}");
    }
}
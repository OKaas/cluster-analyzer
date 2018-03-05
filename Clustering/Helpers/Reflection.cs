using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Clustering.Helpers
{
    public static class Reflection
    {
        public static void InitProperties(object clasz, Dictionary<string, object> properties)
        {
            // add some properties like weights, scale parameters etc.
            FieldInfo[] fields = clasz.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (properties.ContainsKey(field.Name))
                {
                    field.SetValue(clasz, properties[field.Name]);
                }
            }
        }
    }
}

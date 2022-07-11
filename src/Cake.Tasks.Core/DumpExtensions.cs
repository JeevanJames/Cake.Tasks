﻿#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Cake.Tasks.Config;

namespace Cake.Tasks.Core;

public static class DumpExtensions
{
    public static string Dump<T>(this T obj)
    {
        if (obj is null)
            return null;

        var dump = new StringBuilder();
        if (IsSimpleType(obj.GetType()))
            dump.Append(obj.ToString());
        else if (obj is IEnumerable objEnumerable)
        {
            bool first = true;
            foreach (object o in objEnumerable)
            {
                if (first)
                {
                    dump.AppendLine("{");
                    first = false;
                }
                else
                {
                    dump.AppendLine(",");
                    dump.AppendLine("{");
                }

                dump.Append(ObjectToString(o));
                dump.Append("}");
            }
        }
        else
            dump.Append(ObjectToString(obj));

        return dump.ToString();
    }

    private static string ObjectToString<T>(T obj)
    {
        var dump = new StringBuilder();

        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
        {
            Type propertyType = descriptor.PropertyType;
            object value = descriptor.GetValue(obj);

            Type enumerableType = propertyType.GetInterface("IEnumerable");

            if (enumerableType != null && propertyType != typeof(string))
            {
                ProcessEnumerable(value, dump, descriptor);
                continue;
            }

            dump.AppendLine($"\t{descriptor.Name} = {value}");
        }

        return dump.ToString();
    }

    private static void ProcessEnumerable(object value, StringBuilder sb, MemberDescriptor descriptor)
    {
        // Is a collection, iterate and spit out value for each
        if (!(value is IEnumerable enumerable))
            return;

        bool first = true;
        sb.Append($"\t{descriptor.Name}:\t[ ");
        foreach (object val in enumerable)
        {
            if (val == null)
                continue;
            string printVal = IsSimpleType(val.GetType()) ? $"\"{val}\"" : val.Dump().Replace("\r\n", "\r\n\t");
            if (first)
            {
                sb.Append(printVal);
                first = false;
            }
            else
                sb.Append(", ").Append(printVal);
        }

        sb.AppendLine(" ]");
    }

    private static bool IsSimpleType(Type type)
    {
        return
            type.IsPrimitive ||
            new Type[]
            {
                typeof(Enum),
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid),
            }.Contains(type) ||
            Convert.GetTypeCode(type) != TypeCode.Object ||
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                IsSimpleType(type.GetGenericArguments()[0])) ||
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConfigValue<>) &&
                IsSimpleType(type.GetGenericArguments()[0]));
    }
}

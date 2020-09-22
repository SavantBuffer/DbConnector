//Copyright 2019 Robert Orama

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using DbConnector.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DbConnector.Core
{
    public static class DbConnectorUtilities
    {
        internal static readonly MethodInfo _enumTryParse = typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(mi => mi.Name == nameof(Enum.TryParse) && mi.GetParameters().Length == 3 && mi.IsGenericMethod);
        internal static readonly MethodInfo _typeGetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
        internal static readonly BindingFlags _bindingFlagInstancePublic = BindingFlags.Instance | BindingFlags.Public;
        internal static readonly HashSet<Type> _directTypeMap = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(byte[]),
            typeof(byte?),
            typeof(sbyte?),
            typeof(short?),
            typeof(ushort?),
            typeof(int?),
            typeof(uint?),
            typeof(long?),
            typeof(ulong?),
            typeof(float?),
            typeof(double?),
            typeof(decimal?),
            typeof(bool?),
            typeof(char?),
            typeof(Guid?),
            typeof(DateTime?),
            typeof(DateTimeOffset?),
            typeof(TimeSpan?),
            typeof(object),
            typeof(Dictionary<string, object>),
            typeof(List<KeyValuePair<string, object>>),
            typeof(DataTable),
            typeof(DataSet)
        };


        internal static bool IsEnumerable(Type tType)
        {
            if (tType != null && tType.IsInterface)
            {
                var interfaces = tType.GetInterfaces();

                if (interfaces.Length == 1 && interfaces[0] == typeof(IEnumerable))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsColumnNameExcluded(string columnName, bool hasNamesToInclude, bool hasNamesToExclude, IColumnMapSetting settings)
        {
            if (hasNamesToInclude && hasNamesToExclude && (!settings.NamesToInclude.Contains(columnName) || settings.NamesToExclude.Contains(columnName)))
            {
                return true;
            }
            else if (hasNamesToInclude && !settings.NamesToInclude.Contains(columnName))
            {
                return true;
            }
            else if (hasNamesToExclude && settings.NamesToExclude.Contains(columnName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{ColumnMap}"/> using the properties of the T type and the keys.
        /// </summary>      
        /// <param name="ordinalColumnMap">The ordinal column map.</param>
        /// <param name="settings">The settings to use.</param>
        /// <returns>A list with the ColumnMap objects.</returns> 
        /// <exception cref="System.InvalidCastException">Thrown when a property of <typeparamref name="T"/> does not match the located key type.</exception>
        internal static IEnumerable<ColumnMap> GetColumnMaps<T>(OrdinalColumnMap[] ordinalColumnMap, IColumnMapSetting settings)
        {
            return GetColumnMaps(typeof(T), ordinalColumnMap, settings);
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{ColumnMap}"/> using the properties of the tType and the keys.
        /// </summary>      
        /// <param name="tType">The type to use.</param>
        /// <param name="ordinalColumnMap">The ordinal column map.</param>
        /// <param name="settings">The settings to use.</param>
        /// <returns>A list with the ColumnMap objects.</returns> 
        /// <exception cref="System.InvalidCastException">Thrown when a property of <paramref name="tType"/> does not match the located key type.</exception>
        internal static IEnumerable<ColumnMap> GetColumnMaps(Type tType, OrdinalColumnMap[] ordinalColumnMap, IColumnMapSetting settings)
        {
            if (ordinalColumnMap == null || ordinalColumnMap.Length == 0)
            {
                return Enumerable.Empty<ColumnMap>();
            }

            return BuildColumnMaps
            (
                tType,
                ordinalColumnMap,
                settings == null
                ? new ColumnMapBuilderState
                {
                    NonExcludedCount = ordinalColumnMap.Length,
                    ProcessedTypes = new HashSet<Type>()
                }
                : new ColumnMapBuilderState
                {
                    NonExcludedCount = ordinalColumnMap.Length,
                    ProcessedTypes = new HashSet<Type>(),
                    HasJoins = settings.HasSplits,
                    HasAliases = settings.HasAliases,
                },
                settings
            ).OrderBy(m => m.Column.Ordinal).ToArray();
        }

        private class ColumnMapBuilderState
        {
            public int MappedCount { get; set; }

            public int NonExcludedCount { get; set; }

            public bool HasJoins { get; set; }

            public bool HasAliases { get; set; }

            public HashSet<Type> ProcessedTypes { get; set; }
        }

        private static IEnumerable<ColumnMap> BuildColumnMaps(
            Type tType,
            OrdinalColumnMap[] keys,
            ColumnMapBuilderState state,
            IColumnMapSetting settings)
        {
            state.ProcessedTypes.Add(tType);
            PropertyInfo[] props = tType.GetProperties(_bindingFlagInstancePublic);
            int joinStartIndex = state.HasJoins ? GetJoinStartIndex(tType, keys, settings) : 0;


            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];

                if (state.MappedCount == state.NonExcludedCount)
                {
                    break;
                }

                if (p.CanWrite && p.GetCustomAttribute<NotMappedAttribute>() == null)
                {
                    Type propertyType = p.PropertyType;
                    Type nullUnderlyingType = Nullable.GetUnderlyingType(propertyType);

                    if (
                            state.HasJoins
                         && settings.Splits.ContainsKey(propertyType)
                         && !state.ProcessedTypes.Contains(propertyType)
                         && !_directTypeMap.Contains(propertyType)
                         && ((propertyType.IsClass && propertyType.GetConstructor(Type.EmptyTypes) != null) || (propertyType.IsValueType && !(propertyType.IsEnum || (nullUnderlyingType?.IsEnum ?? false))))
                         && !propertyType.IsArray
                         && !typeof(IEnumerable).IsAssignableFrom(propertyType)
                         && !typeof(IListSource).IsAssignableFrom(propertyType)
                        )
                    {
                        var childrenMaps = BuildColumnMaps
                        (
                            propertyType,
                            keys,
                            state,
                            settings
                        ).OrderBy(m => m.Column.Ordinal).ToArray();

                        if (childrenMaps.Length > 0)
                        {
                            var parentMap = new ColumnParentMap
                            {
                                SetMethod = p.GetSetMethod(false),
                                PropInfo = p,
                                Children = childrenMaps
                            };

                            yield return new ColumnMap { Column = childrenMaps[0].Column, ParentMap = parentMap };
                        }

                        continue;
                    }


                    string propColName = p.GetColumnAttributeName();

                    if (state.HasAliases && settings.Aliases.TryGetValue(tType, out Dictionary<string, string> aliasMap))
                    {
                        if (aliasMap != null && aliasMap.TryGetValue(propColName, out string alias))
                        {
                            propColName = alias;
                        }
                    }


                    var keyIndex = Array.FindIndex(keys, joinStartIndex, c => !c.IsMapped && string.Equals(c.Name, propColName, StringComparison.OrdinalIgnoreCase));

                    if (keyIndex >= 0)
                    {
                        var ordinalMap = keys[keyIndex];
                        Type underlyingType = (nullUnderlyingType ?? propertyType);

                        ThrowIfFailedToMatchColumnTypeByNames(ordinalMap.FieldType, underlyingType, ordinalMap.Name, p.Name);


                        yield return new ColumnMap
                        {
                            UnderlyingType = underlyingType,
                            SetMethod = p.GetSetMethod(false),
                            PropInfo = p,
                            Column = ordinalMap
                        };


                        ordinalMap.IsMapped = true;
                        state.MappedCount++;
                    }
#if DEBUG
                    else
                    {

                        Debug.WriteLine("Column name " + propColName + " not found in column schema for property " + propertyType + " of object " + tType);

                    }
#endif
                }
            }
        }

        internal static int GetJoinStartIndex(Type tType, OrdinalColumnMap[] ordinalColumnMap, IColumnMapSetting settings)
        {
            if (settings.Splits.TryGetValue(tType, out string columnName))
            {
                int splitStartIndex = Array.FindIndex(ordinalColumnMap, c => !c.IsMapped && string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase));

                if (splitStartIndex == -1)
                {
                    return 0;
                }

                return splitStartIndex;
            }

            return 0;
        }



        /// <summary>
        /// Maps the properties of type <typeparamref name="T"/> using the provided column maps.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="columnMaps">The column maps to use</param>
        /// <param name="onGetObjectValue">The function to call to get the data based on the column map.</param>
        /// <returns>The mapped object.</returns>
        public static T GetMappedObject<T>(IEnumerable<ColumnMap> columnMaps, Func<ColumnMap, object> onGetObjectValue)
        {
            T obj = Activator.CreateInstance<T>();

            foreach (var map in columnMaps)
            {
                if (map.IsChildMap)
                {
                    SetParentProperties(onGetObjectValue, obj, map.ParentMap);
                }
                else
                {
                    SetChildProperty(obj, map, onGetObjectValue(map));
                }
            }

            return obj;
        }

        /// <summary>
        /// Maps the properties of type <paramref name="objType"/> using the provided column maps.
        /// </summary>
        /// <param name="objType">The types to use.</param>
        /// <param name="columnMaps">The column maps to use.</param>
        /// <param name="onGetObjectValue">The function to call to get the data based on the column map.</param>
        /// <returns>The mapped object.</returns>
        public static object GetMappedObject(Type objType, IEnumerable<ColumnMap> columnMaps, Func<ColumnMap, object> onGetObjectValue)
        {
            object obj = Activator.CreateInstance(objType);

            foreach (var map in columnMaps)
            {
                if (map.IsChildMap)
                {
                    SetParentProperties(onGetObjectValue, obj, map.ParentMap);
                }
                else
                {
                    SetChildProperty(obj, map, onGetObjectValue(map));
                }
            }

            return obj;
        }

        private static object GetMappedParentObject(Func<ColumnMap, object> onGetObjectValue, object obj, IEnumerable<ColumnMap> columnMaps)
        {
            foreach (var map in columnMaps)
            {
                if (map.IsChildMap)
                {
                    SetParentProperties(onGetObjectValue, obj, map.ParentMap);
                }
                else
                {
                    SetChildProperty(obj, map, onGetObjectValue(map));
                }
            }

            return obj;
        }

        internal static void SetParentProperties(Func<ColumnMap, object> onGetObjectValue, object obj, ColumnParentMap childMap)
        {
            PropertyInfo pInfo = obj.GetType().GetProperty(childMap.PropInfo.Name);

            if (pInfo != null && pInfo.CanWrite && pInfo.ReflectedType.FullName == childMap.PropInfo.ReflectedType.FullName)
            {
                object childObj = pInfo.CanRead && pInfo.PropertyType.IsClass ? pInfo.GetValue(obj) : null;

                if (childObj == null)
                {
                    childObj = Activator.CreateInstance(pInfo.PropertyType);
                }

                GetMappedParentObject(onGetObjectValue, childObj, childMap.Children);

                childMap.SetMethod.Invoke(obj, new object[] { childObj });
            }
        }



        internal static void SetChildProperty(object obj, ColumnMap map, object value)
        {
            if (value != DBNull.Value)
            {
                Type nonNullableObjType = map.UnderlyingType;
                Type columnType = map.Column.FieldType;

                if (nonNullableObjType != columnType && !nonNullableObjType.IsAssignableFrom(columnType))
                {
                    if (nonNullableObjType == typeof(Guid) && Guid.TryParse(value.ToString(), out Guid pResult))
                    {
                        value = pResult;
                    }
                    else if (nonNullableObjType.IsEnum)
                    {
                        try
                        {
                            if (columnType == typeof(string))
                            {
                                var parameters = new object[] { (string)value, true, null };

                                object isParsedObj = _enumTryParse.MakeGenericMethod(nonNullableObjType).Invoke(null, parameters);

                                if ((bool)isParsedObj)
                                {
                                    value = parameters[2];

                                    if (!Enum.IsDefined(nonNullableObjType, value))
                                    {
#if DEBUG
                                        Debug.WriteLine("Enum value " + (value?.ToString() ?? "null") + " is not defined for property " + map.PropInfo.Name + " of type " + map.PropInfo.PropertyType);
#endif
                                        return;
                                    }
                                }
                                else
                                {
#if DEBUG
                                    Debug.WriteLine("Enum value " + (value?.ToString() ?? "null") + " cannot be parsed for property " + map.PropInfo.Name + " of type " + map.PropInfo.PropertyType);
#endif
                                    return;
                                }
                            }
                            else
                            {
                                value = Convert.ChangeType(value, Enum.GetUnderlyingType(nonNullableObjType));

                                if (value == null || !Enum.IsDefined(nonNullableObjType, value))
                                {
#if DEBUG
                                    Debug.WriteLine("Enum value " + (value?.ToString() ?? "null") + " is not defined for property " + map.PropInfo.Name + " of type " + map.PropInfo.PropertyType);
#endif
                                    return;
                                }
                                else
                                {
                                    value = Enum.ToObject(nonNullableObjType, value);
                                }
                            }
                        }
                        catch (Exception)
                        {
#if DEBUG
                            Debug.WriteLine("Failed to cast Enum for property " + map.PropInfo.Name + " of type " + map.PropInfo.PropertyType);
#endif
                            return;
                        }
                    }
                    else
                    {
                        try
                        {
                            value = Convert.ChangeType(value, nonNullableObjType);
                        }
                        catch (Exception)
                        {
#if DEBUG
                            Debug.WriteLine("Failed to match indirect type for property " + map.PropInfo.Name + " of type " + map.PropInfo.PropertyType);
#endif
                            return;
                        }
                    }
                }

                map.SetMethod.Invoke(obj, new object[] { value });
            }
        }

        internal static object ThrowIfFailedToMatchColumnType(Type objType, Type nonNullableObjType, object value, Func<string> onGetColumnName = null, Func<string> onGetPropertyName = null)
        {
            Type columnType = value.GetType();

            if (nonNullableObjType != columnType && !nonNullableObjType.IsAssignableFrom(columnType))
            {
                if (nonNullableObjType == typeof(Guid) && Guid.TryParse(value.ToString(), out Guid pResult))
                {
                    value = pResult;
                }
                else if (nonNullableObjType.IsEnum && (columnType.IsValidIndirectType() || columnType == typeof(string)))
                {
                    try
                    {
                        if (columnType == typeof(string))
                        {
                            var parameters = new object[] { (string)value, true, null };

                            object isParsedObj = _enumTryParse.MakeGenericMethod(nonNullableObjType).Invoke(null, parameters);

                            if ((bool)isParsedObj)
                            {
                                value = parameters[2];

                                if (!Enum.IsDefined(nonNullableObjType, value))
                                {
#if DEBUG
                                    Debug.WriteLine("Enum value " + (value?.ToString() ?? "null") + " is not definable from "
                                    + (onGetColumnName == null ? "value" : "column " + onGetColumnName())
                                    + " of type " + columnType + " to "
                                    + (onGetPropertyName == null ? ("object of type " + objType) : ("property " + onGetPropertyName() + " of type " + objType)));
#endif
                                    return Activator.CreateInstance(nonNullableObjType);
                                }
                            }
                            else
                            {
#if DEBUG
                                Debug.WriteLine("Enum value " + (value?.ToString() ?? "null") + " is not parsable from "
                                + (onGetColumnName == null ? "value" : "column " + onGetColumnName())
                                + " of type " + columnType + " to "
                                + (onGetPropertyName == null ? ("object of type " + objType) : ("property " + onGetPropertyName() + " of type " + objType)));
#endif
                                return Activator.CreateInstance(nonNullableObjType);
                            }
                        }
                        else
                        {
                            value = Convert.ChangeType(value, Enum.GetUnderlyingType(nonNullableObjType));

                            if (value == null || !Enum.IsDefined(nonNullableObjType, value))
                            {
#if DEBUG
                                Debug.WriteLine("Enum value " + (value?.ToString() ?? "null") + " is not definable from "
                                + (onGetColumnName == null ? "value" : "column " + onGetColumnName())
                                + " of type " + columnType + " to "
                                + (onGetPropertyName == null ? ("object of type " + objType) : ("property " + onGetPropertyName() + " of type " + objType)));
#endif
                                return Activator.CreateInstance(nonNullableObjType);
                            }
                            else
                            {
                                value = Enum.ToObject(nonNullableObjType, value);
                            }
                        }
                    }
                    catch (Exception)
                    {
#if DEBUG
                        Debug.WriteLine("Failed to cast Enum from "
                        + (onGetColumnName == null ? "value" : "column " + onGetColumnName())
                        + " of type " + columnType + " to "
                        + (onGetPropertyName == null ? ("object of type " + objType) : ("property " + onGetPropertyName() + " of type " + objType)));
#endif
                        return Activator.CreateInstance(nonNullableObjType);
                    }
                }
                else if (!nonNullableObjType.IsEnum && (columnType.IsValidIndirectMatch(nonNullableObjType)))
                {
                    try
                    {
                        value = Convert.ChangeType(value, nonNullableObjType);
                    }
                    catch (Exception)
                    {
#if DEBUG
                        Debug.WriteLine("Failed to match indirect type for "
                        + (onGetColumnName == null ? "value" : "column " + onGetColumnName())
                        + " of type " + columnType + " to "
                        + (onGetPropertyName == null ? ("object of type " + objType) : ("property " + onGetPropertyName() + " of type " + objType)));
#endif
                        return Activator.CreateInstance(nonNullableObjType);
                    }
                }
                else
                {
                    throw new InvalidCastException("Failed to map "
                            + (onGetColumnName == null ? "value" : "column " + onGetColumnName())
                            + " of type " + value.GetType() + " to "
                            + (onGetPropertyName == null ? ("object of type " + objType) : ("property " + onGetPropertyName() + " of type " + objType)));
                }
            }

            return value;
        }

        internal static void ThrowIfFailedToMatchColumnTypeByNames(Type from, Type to, string columnName = null, string propertyName = null)
        {
            if (from != to && !from.IsAssignableFrom(to))
            {
                if (to == typeof(Guid) && from == typeof(string))
                {
                    return;
                }
                else if (to.IsEnum && (from.IsValidIndirectType() || from == typeof(string)))
                {
                    return;
                }
                else if (to.IsEnum || !from.IsValidIndirectType())
                {
                    throw new InvalidCastException("Failed to map "
                            + (columnName == null ? "value" : "column " + columnName)
                            + " of type " + from + " to "
                            + (propertyName == null ? ("object of type " + to) : ("property " + propertyName + " of type " + to)));
                }
            }
        }

        internal static void ThrowIfFailedToMatchColumnTypeByNames(Type from, Type to, string columnName, string propertyName, out OpCode code)
        {
            if (from != to && !from.IsAssignableFrom(to))
            {
                if (to == typeof(Guid) && from == typeof(string))
                {
                    return;
                }
                else if (to.IsEnum && (from.IsValidIndirectMatch(to, out code) || from == typeof(string)))
                {
                    return;
                }
                else if (to.IsEnum || !from.IsValidIndirectMatch(to, out code))
                {
                    throw new InvalidCastException("Failed to map "
                            + (columnName == null ? "value" : "column " + columnName)
                            + " of type " + from + " to "
                            + (propertyName == null ? ("object of type " + to) : ("property " + propertyName + " of type " + to)));
                }
            }
        }
    }
}

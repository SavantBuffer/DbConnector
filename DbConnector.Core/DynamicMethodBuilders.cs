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
using System.Data.Common;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace DbConnector.Core
{
    internal interface IDynamicMapper
    {
        int GetHitCount();
        void Hit();
    }

    internal interface IDynamicColumnMapper : IDynamicMapper
    {
        object Build(IDataRecord dataRecord);
        void CreateDelegate(DynamicMethod method);
    }

    internal interface IDynamicColumnMapper<T> : IDynamicColumnMapper
    {
        Func<IDataRecord, T> OnBuild { get; }
    }

    internal class DynamicColumnMapper<T> : IDynamicColumnMapper<T>
    {
        private int _hitCount;
        public Func<IDataRecord, T> OnBuild { get; private set; }


        public int GetHitCount() { return Interlocked.CompareExchange(ref _hitCount, 0, 0); }

        public void Hit() { Interlocked.Increment(ref _hitCount); }

        public object Build(IDataRecord dataRecord)
        {
            return OnBuild(dataRecord);
        }

        public void CreateDelegate(DynamicMethod method)
        {
            OnBuild = (Func<IDataRecord, T>)method.CreateDelegate(typeof(Func<IDataRecord, T>));
        }
    }

    internal static class DynamicColumnMapper
    {
        public static readonly FieldInfo _DBNullValue = typeof(DBNull).GetField(nameof(DBNull.Value));
        private static readonly MethodInfo _getValueMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue));
        private static readonly MethodInfo _guidTryParseMethod = typeof(Guid).GetMethod(nameof(Guid.TryParse), new Type[] { typeof(string), typeof(Guid) });
        private static readonly MethodInfo _enumIsDefined = typeof(Enum).GetMethod(nameof(Enum.IsDefined));

        private static readonly MethodInfo _iDictionaryIndexerSet = typeof(IDictionary<string, object>).GetMethod("set_Item");
        private static readonly ConstructorInfo _DictionaryCtorByCapacity = typeof(Dictionary<string, object>).GetConstructor(new Type[] { typeof(int) });

        private static readonly ConstructorInfo _keyValuePairsCtorByCapacity = typeof(List<KeyValuePair<string, object>>).GetConstructor(new Type[] { typeof(int) });
        private static readonly MethodInfo _keyValuePairsAdd = typeof(List<KeyValuePair<string, object>>).GetMethod(nameof(List<KeyValuePair<string, object>>.Add));
        private static readonly ConstructorInfo _keyValuePairCtor = typeof(KeyValuePair<string, object>).GetConstructor(new Type[] { typeof(string), typeof(object) });


        private class DynamicMethodMap
        {
            public Action<ILGenerator, bool, int> Method { get; set; }

            public int Ordinal { get; set; }
        }

        private class DynamicMethodMapBuilderState
        {
            public int MappedCount { get; set; }

            public int NonExcludedCount { get; set; }

            public bool HasJoins { get; set; }

            public bool HasAliases { get; set; }

            public HashSet<Type> ProcessedTypes { get; set; }
        }


        public static IDynamicColumnMapper CreateMapper(Type tType, OrdinalColumnMap[] ordinalColumnMap, IColumnMapSetting settings)
        {
            return CreateMapper(Activator.CreateInstance(typeof(DynamicColumnMapper<>).MakeGenericType(tType)) as IDynamicColumnMapper, tType, ordinalColumnMap, settings);
        }

        public static IDynamicColumnMapper CreateMapper<TResult>(OrdinalColumnMap[] ordinalColumnMap, IColumnMapSetting settings)
        {
            return CreateMapper(new DynamicColumnMapper<TResult>(), typeof(TResult), ordinalColumnMap, settings);
        }

        private static IDynamicColumnMapper CreateMapper(IDynamicColumnMapper mapper, Type tType, OrdinalColumnMap[] ordinalColumnMap, IColumnMapSetting settings)
        {
            //IT'S OVER NINE THOUSAAAAAAAAAAAND!!!
            ConstructorInfo ctorInfo = null;

            if (typeof(IEnumerable).IsAssignableFrom(tType))
            {
                throw new InvalidCastException("The type " + tType + " is not supported");
            }
            else if (tType.IsClass && (ctorInfo = tType.GetConstructor(Type.EmptyTypes)) == null)
            {
                throw new InvalidCastException("The type " + tType + " is missing a parameterless constructor");
            }


            DynamicMethod method = new DynamicMethod("DynamicCreate_" + tType.Name + "_" + Guid.NewGuid().ToString(), tType,
                    new Type[] { typeof(IDataRecord) }, tType, true);
            ILGenerator il = method.GetILGenerator();


            //Init target
            Type nullUnderlyingType = Nullable.GetUnderlyingType(tType);
            Type nonNullUnderlyingType = (nullUnderlyingType ?? tType);

            if (nonNullUnderlyingType.IsValueType)
            {
                il.LoadLocalAddress(il.DeclareLocal(nonNullUnderlyingType).LocalIndex);//target (the address of the value)
                il.Emit(OpCodes.Dup);//target/target
                il.Emit(OpCodes.Initobj, nonNullUnderlyingType);//target (initialized)
            }
            else
            {
                il.Emit(OpCodes.Newobj, ctorInfo); //target
            }


            if (ordinalColumnMap != null && ordinalColumnMap.Length > 0)
            {
                //EmitProperties
                var builderState = settings == null
                    ? new DynamicMethodMapBuilderState
                    {
                        NonExcludedCount = ordinalColumnMap.Length,
                        ProcessedTypes = new HashSet<Type>()
                    }
                    : new DynamicMethodMapBuilderState
                    {
                        NonExcludedCount = ordinalColumnMap.Length,
                        ProcessedTypes = new HashSet<Type>(),
                        HasJoins = settings.HasSplits,
                        HasAliases = settings.HasAliases
                    };

                var methodMaps = BuildDynamicMethodMaps(tType, ordinalColumnMap, builderState, settings).OrderBy(o => o.Ordinal).ToArray();

                if (methodMaps.Length > 0)
                {
                    int localDbNull = il.DeclareLocal(typeof(DBNull)).LocalIndex;
                    int lastIndex = methodMaps.Length - 1;

                    il.Emit(OpCodes.Dup);//target/target
                    il.Emit(OpCodes.Ldsfld, _DBNullValue);//target/target/DbNull
                    il.StoreLocal(localDbNull);//target/target

                    for (int i = 0; i < methodMaps.Length; i++)
                    {
                        methodMaps[i].Method(il, i == lastIndex, localDbNull);
                    }
                }
            }


            //incoming stack: target
            if (nonNullUnderlyingType.IsValueType)
            {
                il.Emit(OpCodes.Ldobj, nonNullUnderlyingType);//

                if (nullUnderlyingType != null)
                {
                    il.Emit(OpCodes.Newobj, tType.GetConstructor(new[] { nullUnderlyingType })); //nullable_target
                }
            }
            il.Emit(OpCodes.Ret);//

            mapper.CreateDelegate(method);
            return mapper;
        }

        private static IEnumerable<DynamicMethodMap> BuildDynamicMethodMaps(Type tType, OrdinalColumnMap[] keys, DynamicMethodMapBuilderState state, IColumnMapSetting settings)
        {
            state.ProcessedTypes.Add(tType);

            int joinStartIndex = state.HasJoins ? DbConnectorUtilities.GetJoinStartIndex(tType, keys, settings) : 0;
            var props = tType.GetProperties(DbConnectorUtilities._bindingFlagInstancePublic);

            foreach (var p in props)
            {
                if (state.MappedCount == state.NonExcludedCount)
                {
                    yield break;
                }

                if (p.CanWrite && p.GetCustomAttribute<NotMappedAttribute>() == null)
                {
                    Type propertyType = p.PropertyType;
                    Type nullUnderlyingType = Nullable.GetUnderlyingType(propertyType);
                    Type unboxType = (nullUnderlyingType ?? propertyType);

                    if (
                            state.HasJoins
                         && settings.Splits.ContainsKey(propertyType)
                         && !state.ProcessedTypes.Contains(propertyType)
                         && !DbConnectorUtilities._directTypeMap.Contains(propertyType)
                         && ((propertyType.IsClass && propertyType.GetConstructor(Type.EmptyTypes) != null) || (propertyType.IsValueType && !(propertyType.IsEnum || (nullUnderlyingType?.IsEnum ?? false))))
                         && !propertyType.IsArray
                         && !typeof(IEnumerable).IsAssignableFrom(propertyType)
                         && !typeof(IListSource).IsAssignableFrom(propertyType)
                        )
                    {
                        var methodMaps = BuildDynamicMethodMaps(propertyType, keys, state, settings).OrderBy(o => o.Ordinal).ToArray();

                        if (methodMaps?.Length > 0)
                        {
                            int ordinal = methodMaps[0].Ordinal;

                            void method(ILGenerator il, bool isLast, int localDbNull)
                            {
                                //Init child struct or object                                
                                if (unboxType.IsValueType)
                                {
                                    il.LoadLocalAddress(il.DeclareLocal(unboxType).LocalIndex);//target/target/child_struct_address
                                    il.Emit(OpCodes.Dup);//target/target/child_struct_address/child_struct_address
                                    il.Emit(OpCodes.Initobj, unboxType);//target/target/child_struct_address (initialized) 
                                }
                                else
                                {
                                    if (p.CanRead)
                                    {
                                        //Maybe the constructor instantiated the child property?
                                        MethodInfo getMethod = p.GetGetMethod();
                                        Label lblInit = il.DefineLabel();
                                        Label lblSkipInit = il.DefineLabel();

                                        il.Emit(OpCodes.Dup);//target/target/target
                                        il.Emit(OpCodes.Callvirt, getMethod);//target/target/child_object or null                            
                                        il.Emit(OpCodes.Ldnull);//target/target/child_object or null/null
                                        il.Emit(OpCodes.Beq_S, lblInit);//target/target                                       

                                        il.Emit(OpCodes.Dup);//target/target/target
                                        il.Emit(OpCodes.Callvirt, getMethod);//target/target/child_object
                                        il.Emit(OpCodes.Br_S, lblSkipInit);//target/target/child_object    

                                        il.MarkLabel(lblInit);

                                        il.Emit(OpCodes.Newobj, unboxType.GetConstructor(Type.EmptyTypes)); //target/target/child_object

                                        il.MarkLabel(lblSkipInit);
                                    }
                                    else
                                    {
                                        il.Emit(OpCodes.Newobj, unboxType.GetConstructor(Type.EmptyTypes)); //target/target/child_object
                                    }
                                }


                                //Set child's properties
                                il.Emit(OpCodes.Dup);//target/target/child/child 

                                int lastIndex = methodMaps.Length - 1;

                                for (int i = 0; i < methodMaps.Length; i++)
                                {
                                    methodMaps[i].Method(il, i == lastIndex, localDbNull);
                                }


                                //incoming stack: target/target/child_struct_address
                                if (unboxType.IsValueType)
                                {
                                    il.Emit(OpCodes.Ldobj, unboxType);//target/target/child_struct_value

                                    if (nullUnderlyingType != null)
                                    {
                                        il.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { unboxType })); //target/target/nullable_child_struct_value
                                    }

                                    if (tType.IsValueType)
                                    {
                                        il.Emit(OpCodes.Call, p.GetSetMethod());//target
                                    }
                                    else
                                    {
                                        il.Emit(OpCodes.Callvirt, p.GetSetMethod());//target
                                    }
                                }
                                //incoming stack: target/target/child_object
                                else
                                {
                                    il.Emit(OpCodes.Callvirt, p.GetSetMethod());//target                                
                                }

                                if (!isLast)
                                {
                                    il.Emit(OpCodes.Dup);//target/target
                                }
                            }

                            yield return new DynamicMethodMap { Method = method, Ordinal = ordinal };
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


                    var keyIndex = Array.FindIndex(keys, joinStartIndex, c => !c.IsMapped && c.Name == propColName);

                    if (keyIndex >= 0)
                    {
                        var ordinalMap = keys[keyIndex];

                        DbConnectorUtilities.ThrowIfFailedToMatchColumnTypeByNames(ordinalMap.FieldType, unboxType, ordinalMap.Name, p.Name, out OpCode opCode);

                        void method(ILGenerator il, bool isLast, int localDbNull)
                        {
                            //Set label
                            Label lblSkip = il.DefineLabel();
                            Label lblDone = il.DefineLabel();
                            Label lblSkipEmpty = il.DefineLabel();


                            //Load from db and branch if null
                            //incoming stack: target/target
                            il.Emit(OpCodes.Ldarg_0);//target/target/reader
                            il.LoadInt(ordinalMap.Ordinal);//target/target/reader/ordinal
                            il.Emit(OpCodes.Callvirt, _getValueMethod);//target/target/object
                            il.Emit(OpCodes.Dup);//target/target/object/object           
                            il.LoadLocal(localDbNull);//target/target/object/DbNull
                            il.Emit(OpCodes.Beq_S, lblSkip);//target/target/object


                            if (unboxType == typeof(Guid) && ordinalMap.FieldType == typeof(string))
                            {
                                int localGuid = il.DeclareLocal(typeof(Guid)).LocalIndex;

                                il.LoadLocal(localGuid);//target/target/string/guid
                                il.Emit(OpCodes.Call, _guidTryParseMethod);//target/target/true or false
                                il.Emit(OpCodes.Brfalse_S, lblSkipEmpty);//target/target

                                //Set property
                                il.LoadLocal(localGuid);//target/target/guid

                                if (nullUnderlyingType != null)
                                    il.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullUnderlyingType })); //target/target/nullable_guid

                                il.Emit(OpCodes.Call, p.GetSetMethod());//target

                                if (!isLast)
                                {
                                    il.Emit(OpCodes.Dup);//target/target 
                                }
                                il.Emit(OpCodes.Br_S, lblDone);//target || target/target
                            }
                            else if (unboxType.IsEnum)
                            {
                                //If trying to match strings
                                if (ordinalMap.FieldType == typeof(string))
                                {
                                    il.Emit(OpCodes.Ldc_I4_1);//target/target/string/true                                    

                                    //Init local enum value
                                    int localEnum = il.DeclareLocal(unboxType).LocalIndex;
                                    il.LoadLocalAddress(localEnum);//target/target/string/true/enum_address
                                    il.Emit(OpCodes.Dup);//target/target/string/true/enum_address/enum_address
                                    il.Emit(OpCodes.Initobj, unboxType);//target/target/string/true/enum_address

                                    //Try Parse
                                    il.Emit(OpCodes.Call, DbConnectorUtilities._enumTryParse.MakeGenericMethod(unboxType));//target/target/true or false
                                    il.Emit(OpCodes.Brfalse_S, lblSkipEmpty);//target/target

                                    //Check if it's defined Enum
                                    il.Emit(OpCodes.Ldtoken, unboxType); //target/target/enum_type_token
                                    il.Emit(OpCodes.Call, DbConnectorUtilities._typeGetTypeFromHandle); //target/target/enum_type
                                    il.LoadLocal(localEnum);//target/target/enum_type/enum_value
                                    il.Emit(OpCodes.Box, unboxType);//target/target/enum_type/enum_value_boxed
                                    il.Emit(OpCodes.Call, _enumIsDefined);//target/target/true or false
                                    il.Emit(OpCodes.Brfalse_S, lblSkipEmpty);//target/target

                                    //Set property
                                    il.LoadLocal(localEnum);//target/target/enum_value

                                    if (nullUnderlyingType != null)
                                        il.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullUnderlyingType })); //target/target/nullable_enum_value

                                    il.Emit(OpCodes.Call, p.GetSetMethod());//target

                                    if (!isLast)
                                    {
                                        il.Emit(OpCodes.Dup);//target/target 
                                    }
                                    il.Emit(OpCodes.Br_S, lblDone);//target || target/target
                                }
                                else
                                {
                                    //Cast
                                    Type enumUnderlyingType = Enum.GetUnderlyingType(unboxType);
                                    int localIndex = il.DeclareLocal(enumUnderlyingType).LocalIndex;
                                    il.Emit(OpCodes.Unbox_Any, ordinalMap.FieldType); //target/target/col_typed_value
                                    il.Emit(opCode); //target/target/typed_value
                                    il.StoreLocal(localIndex);//target/target

                                    //Check if it's defined Enum
                                    il.Emit(OpCodes.Ldtoken, unboxType); //target/target/enum_type_token
                                    il.Emit(OpCodes.Call, DbConnectorUtilities._typeGetTypeFromHandle); //target/target/enum_type
                                    il.LoadLocal(localIndex);//target/target/enum_type/typed_value
                                    il.Emit(OpCodes.Box, enumUnderlyingType);//target/target/enum_type/typed_value_boxed
                                    il.Emit(OpCodes.Call, _enumIsDefined);//target/target/true or false
                                    il.Emit(OpCodes.Brfalse_S, lblSkipEmpty);//target/target/

                                    //Set property
                                    il.LoadLocal(localIndex);//target/target/typed-value

                                    if (nullUnderlyingType != null)
                                        il.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullUnderlyingType })); //target/target/nullable_value

                                    il.Emit(OpCodes.Call, p.GetSetMethod());//target

                                    if (!isLast)
                                    {
                                        il.Emit(OpCodes.Dup);//target/target 
                                    }
                                    il.Emit(OpCodes.Br_S, lblDone);//target || target/target
                                }
                            }
                            else if (opCode != default)
                            {
                                //Indirect match and cast
                                il.Emit(OpCodes.Unbox_Any, ordinalMap.FieldType); //target/target/col_typed_value
                                il.Emit(opCode); //target/target/typed_value

                                if (unboxType == typeof(bool))
                                {
                                    il.Emit(OpCodes.Ldc_I4_0);//target/target/typed_value/0
                                    il.Emit(OpCodes.Ceq);//target/target/0 or 1
                                    il.Emit(OpCodes.Ldc_I4_0);//target/target/0 or 1/0
                                    il.Emit(OpCodes.Ceq);//target/target/0 or 1
                                }

                                if (nullUnderlyingType != null)
                                {
                                    il.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullUnderlyingType })); //target/target/nullable_value
                                }

                                //Set property
                                il.Emit(OpCodes.Call, p.GetSetMethod());//target

                                if (!isLast)
                                {
                                    il.Emit(OpCodes.Dup);//target/target 
                                }
                                il.Emit(OpCodes.Br_S, lblDone);//target || target/target
                            }
                            else
                            {
                                //Set property
                                if (unboxType.IsValueType)
                                {
                                    il.Emit(OpCodes.Unbox_Any, unboxType);//target/target/value

                                    if (nullUnderlyingType != null)
                                        il.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullUnderlyingType })); //target/target/nullable_value

                                    il.Emit(OpCodes.Call, p.GetSetMethod());//target
                                }
                                else
                                {
                                    il.Emit(OpCodes.Callvirt, p.GetSetMethod());//target
                                }

                                if (!isLast)
                                {
                                    il.Emit(OpCodes.Dup);//target/target 
                                }
                                il.Emit(OpCodes.Br_S, lblDone);//target || target/target
                            }


                            //Skip branch incoming stack: target/target/object
                            il.MarkLabel(lblSkip);
                            il.Emit(OpCodes.Pop);//target/target


                            //Skip-Empty branch incoming stack: target/target
                            il.MarkLabel(lblSkipEmpty);
                            if (isLast)
                            {
                                il.Emit(OpCodes.Pop);//target 
                            }


                            //Done branch potential incoming stacks: target/target || target
                            il.MarkLabel(lblDone);
                        }


                        yield return new DynamicMethodMap { Method = method, Ordinal = ordinalMap.Ordinal };


                        ordinalMap.IsMapped = true;
                        state.MappedCount++;
                    }
#if DEBUG
                    else
                    {

                        Debug.WriteLine("Column name " + propColName + " was not found in column schema for property " + propertyType + " of object " + tType);

                    }
#endif
                }
            }
        }

        public static DynamicColumnMapper<ExpandoObject> CreateExpandoObjectMapper(OrdinalColumnMapLite[] ordinalColumnMap, DbDataReader odr, out bool isRead, out ExpandoObject mappedRow)
        {
            var mapper = new DynamicColumnMapper<ExpandoObject>();
            isRead = odr.Read();
            mappedRow = isRead ? new ExpandoObject() : null;


            DynamicMethod method = new DynamicMethod("DynamicExpandoObjectCreate_" + Guid.NewGuid().ToString(), typeof(ExpandoObject),
                    new Type[] { typeof(IDataRecord) }, typeof(ExpandoObject), true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Newobj, typeof(ExpandoObject).GetConstructor(Type.EmptyTypes)); //target


            if (ordinalColumnMap != null && ordinalColumnMap.Length > 0)
            {
                var expandoDict = isRead ? mappedRow as IDictionary<string, object> : null;

                //EmitProperties
                int localDbNull = il.DeclareLocal(typeof(DBNull)).LocalIndex;

                il.Emit(OpCodes.Isinst, typeof(IDictionary<string, object>));//target
                il.Emit(OpCodes.Ldsfld, _DBNullValue);//target/DbNull
                il.StoreLocal(localDbNull);//target

                foreach (var map in ordinalColumnMap)
                {
                    //Set label
                    Label lblSetNull = il.DefineLabel();
                    Label lblDone = il.DefineLabel();

                    //Load from db
                    il.Emit(OpCodes.Dup);//target/target
                    il.Emit(OpCodes.Ldstr, map.Name);//target/target/colname
                    il.Emit(OpCodes.Ldarg_0);//target/target/colname/reader
                    il.LoadInt(map.Ordinal);//target/target/colname/reader/ordinal
                    il.Emit(OpCodes.Callvirt, _getValueMethod);//target/target/colname/object
                    il.Emit(OpCodes.Dup);//target/target/colname/object/object           
                    il.LoadLocal(localDbNull);//target/target/colname/object/DbNull
                    il.Emit(OpCodes.Beq_S, lblSetNull);//target/target/colname/object

                    il.Emit(OpCodes.Callvirt, _iDictionaryIndexerSet);//target
                    il.Emit(OpCodes.Br_S, lblDone);//target

                    //Branch for nulls
                    il.MarkLabel(lblSetNull);//target/target/colname/object
                    il.Emit(OpCodes.Pop);//target/target/colname
                    il.Emit(OpCodes.Ldnull);//target/target/colname/null

                    il.Emit(OpCodes.Callvirt, _iDictionaryIndexerSet);//target

                    //Branch incoming stack: target
                    il.MarkLabel(lblDone);

                    if (isRead)
                    {
                        object data = odr.GetValue(map.Ordinal);
                        expandoDict[map.Name] = data == DBNull.Value ? null : data;
                    }
                }
            }


            //incoming stack: target
            il.Emit(OpCodes.Ret);//

            mapper.CreateDelegate(method);
            return mapper;
        }

        public static DynamicColumnMapper<Dictionary<string, object>> CreateDictionaryMapper(OrdinalColumnMapLite[] ordinalColumnMap, DbDataReader odr, out bool isRead, out Dictionary<string, object> mappedRow)
        {
            var mapper = new DynamicColumnMapper<Dictionary<string, object>>();
            isRead = odr.Read();


            DynamicMethod method = new DynamicMethod("DynamicDictionaryCreate_" + Guid.NewGuid().ToString(), typeof(Dictionary<string, object>),
                    new Type[] { typeof(IDataRecord) }, typeof(Dictionary<string, object>), true);
            ILGenerator il = method.GetILGenerator();


            if (ordinalColumnMap != null && ordinalColumnMap.Length > 0)
            {
                mappedRow = isRead ? new Dictionary<string, object>(ordinalColumnMap.Length) : null;

                il.LoadInt(ordinalColumnMap.Length);//capacity
                il.Emit(OpCodes.Newobj, _DictionaryCtorByCapacity); //target

                //EmitProperties
                int localDbNull = il.DeclareLocal(typeof(DBNull)).LocalIndex;
                il.Emit(OpCodes.Ldsfld, _DBNullValue);//target/DbNull
                il.StoreLocal(localDbNull);//target

                foreach (var map in ordinalColumnMap)
                {
                    //Set label
                    Label lblSetNull = il.DefineLabel();
                    Label lblDone = il.DefineLabel();

                    //Load from db
                    il.Emit(OpCodes.Dup);//target/target
                    il.Emit(OpCodes.Ldstr, map.Name);//target/target/colname
                    il.Emit(OpCodes.Ldarg_0);//target/target/colname/reader
                    il.LoadInt(map.Ordinal);//target/target/colname/reader/ordinal
                    il.Emit(OpCodes.Callvirt, _getValueMethod);//target/target/colname/object
                    il.Emit(OpCodes.Dup);//target/target/colname/object/object
                    il.LoadLocal(localDbNull);//target/target/colname/object/object/DbNull
                    il.Emit(OpCodes.Beq_S, lblSetNull);//target/target/colname/object

                    il.Emit(OpCodes.Callvirt, _iDictionaryIndexerSet);//target
                    il.Emit(OpCodes.Br_S, lblDone);//target

                    //Branch for nulls
                    il.MarkLabel(lblSetNull);//target/target/colname/object
                    il.Emit(OpCodes.Pop);//target/target/colname
                    il.Emit(OpCodes.Ldnull);//target/target/colname/null

                    il.Emit(OpCodes.Callvirt, _iDictionaryIndexerSet);//target

                    //Branch incoming stack: target
                    il.MarkLabel(lblDone);

                    if (isRead)
                    {
                        object data = odr.GetValue(map.Ordinal);
                        mappedRow[map.Name] = data == DBNull.Value ? null : data;
                    }
                }
            }
            else
            {
                mappedRow = isRead ? new Dictionary<string, object>() : null;
                il.Emit(OpCodes.Newobj, typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes)); //target
            }


            //incoming stack: target
            il.Emit(OpCodes.Ret);//

            mapper.CreateDelegate(method);
            return mapper;
        }

        public static DynamicColumnMapper<List<KeyValuePair<string, object>>> CreateKeyValuePairsMapper(OrdinalColumnMapLite[] ordinalColumnMap, DbDataReader odr, out bool isRead, out List<KeyValuePair<string, object>> mappedRow)
        {
            var mapper = new DynamicColumnMapper<List<KeyValuePair<string, object>>>();
            isRead = odr.Read();


            DynamicMethod method = new DynamicMethod("DynamicKeyValuePairsCreate_" + Guid.NewGuid().ToString(), typeof(List<KeyValuePair<string, object>>),
                    new Type[] { typeof(IDataRecord) }, typeof(List<KeyValuePair<string, object>>), true);
            ILGenerator il = method.GetILGenerator();


            if (ordinalColumnMap != null && ordinalColumnMap.Length > 0)
            {
                mappedRow = isRead ? new List<KeyValuePair<string, object>>(ordinalColumnMap.Length) : null;

                il.LoadInt(ordinalColumnMap.Length);//capacity
                il.Emit(OpCodes.Newobj, _keyValuePairsCtorByCapacity); //target

                //EmitProperties
                int localDbNull = il.DeclareLocal(typeof(DBNull)).LocalIndex;
                il.Emit(OpCodes.Ldsfld, _DBNullValue);//target/DbNull
                il.StoreLocal(localDbNull);//target

                foreach (var map in ordinalColumnMap)
                {
                    //Set label
                    Label lblSetNull = il.DefineLabel();
                    Label lblDone = il.DefineLabel();

                    //Load from db
                    il.Emit(OpCodes.Dup);//target/target
                    il.Emit(OpCodes.Ldstr, map.Name);//target/target/colname
                    il.Emit(OpCodes.Ldarg_0);//target/target/colname/reader
                    il.LoadInt(map.Ordinal);//target/target/colname/reader/ordinal
                    il.Emit(OpCodes.Callvirt, _getValueMethod);//target/target/colname/object
                    il.Emit(OpCodes.Dup);//target/target/colname/object/object           
                    il.LoadLocal(localDbNull);//target/target/colname/object/DbNull
                    il.Emit(OpCodes.Beq_S, lblSetNull);//target/target/colname/object

                    il.Emit(OpCodes.Newobj, _keyValuePairCtor); //target/target/keyValuePair
                    il.Emit(OpCodes.Call, _keyValuePairsAdd);//target
                    il.Emit(OpCodes.Br_S, lblDone);//target

                    //Branch for nulls
                    il.MarkLabel(lblSetNull);//target/target/colname/object
                    il.Emit(OpCodes.Pop);//target/target/colname
                    il.Emit(OpCodes.Ldnull);//target/target/colname/null

                    il.Emit(OpCodes.Newobj, _keyValuePairCtor); //target/target/keyValuePair
                    il.Emit(OpCodes.Call, _keyValuePairsAdd);//target

                    //Branch incoming stack: target
                    il.MarkLabel(lblDone);

                    if (isRead)
                    {
                        object data = odr.GetValue(map.Ordinal);
                        mappedRow.Add(new KeyValuePair<string, object>(map.Name, data == DBNull.Value ? null : data));
                    }
                }
            }
            else
            {
                mappedRow = isRead ? new List<KeyValuePair<string, object>>() : null;
                il.Emit(OpCodes.Newobj, typeof(List<KeyValuePair<string, object>>).GetConstructor(Type.EmptyTypes)); //target
            }


            //incoming stack: target
            il.Emit(OpCodes.Ret);//

            mapper.CreateDelegate(method);
            return mapper;
        }

    }



    internal class DynamicInstanceBuilder
    {
        public Func<object> OnBuild { get; private set; }

        public static DynamicInstanceBuilder CreateBuilder(Type tType)
        {
            DynamicInstanceBuilder builder = new DynamicInstanceBuilder();


            DynamicMethod method = new DynamicMethod("DynamicInstance_" + Guid.NewGuid().ToString(), tType,
                    null, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Newobj, tType.GetConstructor(Type.EmptyTypes)); //target
            il.Emit(OpCodes.Ret);//


            builder.OnBuild = (Func<object>)method.CreateDelegate(typeof(Func<object>));
            return builder;
        }

        public static Func<TInstanceType> CreateBuilderFunction<TInstanceType>()
        {
            Type tType = typeof(TInstanceType);

            DynamicMethod method = new DynamicMethod("DynamicInstance_" + Guid.NewGuid().ToString(), tType,
                    null, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Newobj, tType.GetConstructor(Type.EmptyTypes)); //target
            il.Emit(OpCodes.Ret);//


            return (Func<TInstanceType>)method.CreateDelegate(typeof(Func<TInstanceType>));
        }

        public static Func<TInstanceType> CreateBuilderFunction<TInstanceType>(Type tType)
        {
            DynamicMethod method = new DynamicMethod("DynamicInstance_" + Guid.NewGuid().ToString(), tType,
                    null, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Newobj, tType.GetConstructor(Type.EmptyTypes)); //target
            il.Emit(OpCodes.Ret);//

            return (Func<TInstanceType>)method.CreateDelegate(typeof(Func<TInstanceType>));
        }

        public static Func<object> CreateBuilderFunction(Type tType)
        {
            DynamicMethod method = new DynamicMethod("DynamicInstance_" + Guid.NewGuid().ToString(), tType,
                    null, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Newobj, tType.GetConstructor(Type.EmptyTypes)); //target
            il.Emit(OpCodes.Ret);//

            return (Func<object>)method.CreateDelegate(typeof(Func<object>));
        }
    }

    internal class DynamicParameterBuilder
    {
        private static readonly FieldInfo _dbCommand = typeof(DbJobParameterCollection).GetField(nameof(DbJobParameterCollection._dbCommand), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _dbCommandCreateParameter = typeof(DbCommand).GetMethod(nameof(DbCommand.CreateParameter));

        private static readonly FieldInfo _dbCollection = typeof(DbJobParameterCollection).GetField(nameof(DbJobParameterCollection._collection), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _dbCollectionAdd = typeof(DbParameterCollection).GetMethod(nameof(DbParameterCollection.Add));

        private static readonly MethodInfo _dbParameterParameterName = typeof(DbParameter).GetProperty(nameof(DbParameter.ParameterName)).GetSetMethod();
        private static readonly MethodInfo _dbParameterValue = typeof(DbParameter).GetProperty(nameof(DbParameter.Value)).GetSetMethod();

        private static readonly MethodInfo _addParameterByEnumArray = typeof(DbJobParameterCollection).GetMethod(nameof(DbJobParameterCollection.AddParameterByEnumArray), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _convertChangeType = typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(object), typeof(Type) });


        public Action<DbJobParameterCollection, object> AddFor { get; private set; }


        public static Action<DbJobParameterCollection, object> CreateBuilderAction<T>(T obj, Type tType, DbJobParameterCollection dbJobParameterCollection, bool isUseColumnAttributeNames, string paramsPrefix, string paramsSuffix)
        {
            DynamicMethod method = new DynamicMethod("DynamicAddFor_" + Guid.NewGuid().ToString(), null,
                     new Type[] { typeof(DbJobParameterCollection), typeof(object) }, typeof(DbJobParameterCollection), true);
            ILGenerator il = method.GetILGenerator();


            var props = tType.GetProperties(DbConnectorUtilities._bindingFlagInstancePublic);


            if (
                DbConnectorUtilities._directTypeMap.Contains(tType)
                || (tType.IsValueType && (tType.IsEnum || (Nullable.GetUnderlyingType(tType)?.IsEnum ?? false)))
                || tType.IsArray
                || typeof(IEnumerable).IsAssignableFrom(tType)
                || typeof(IListSource).IsAssignableFrom(tType)
             )
            {
                throw new InvalidCastException("The type " + tType + " is not supported.");
            }

            if (props.Length > 0)
            {
                Queue<Action<ILGenerator, bool, bool, bool, string, string, T, DbJobParameterCollection>> builderMethods = new Queue<Action<ILGenerator, bool, bool, bool, string, string, T, DbJobParameterCollection>>(props.Length);

                for (int i = 0; i < props.Length; i++)
                {
                    var p = props[i];
                    Type propertyType = p.PropertyType;

                    if (p.CanRead
                        && (
                            (propertyType == typeof(string) || propertyType.IsArray)
                            || !(typeof(IEnumerable).IsAssignableFrom(propertyType) || typeof(IListSource).IsAssignableFrom(propertyType))
                        )
                    )
                    {
                        void builderMethod(
                            ILGenerator il_,
                            bool isUseColumnAttributeNames_,
                            bool hasParamsPrefix_,
                            bool hasParamsSuffix_,
                            string paramsPrefix_,
                            string paramsSuffix_,
                            T obj_,
                            DbJobParameterCollection dbJobParameterCollection_)
                        {
                            object value = p.GetValue(obj_);
                            DbParameter dbParameter = dbJobParameterCollection_._dbCommand.CreateParameter();

                            Type nullUnderlyingType = Nullable.GetUnderlyingType(propertyType);
                            Type boxType = (nullUnderlyingType ?? propertyType);
                            bool isNeedNullCheck = boxType.IsClass || nullUnderlyingType != null;
                            string parameterName = (!hasParamsPrefix_ ? (isUseColumnAttributeNames_ ? p.GetColumnAttributeName() : p.Name) :
                                paramsPrefix_ + (isUseColumnAttributeNames_ ? p.GetColumnAttributeName() : p.Name)) + (!hasParamsSuffix_ ? string.Empty : paramsSuffix_);


                            Label lblIsNull = isNeedNullCheck ? il_.DefineLabel() : default;
                            Label lblAddToCollection = il_.DefineLabel();

                            //Set ParameterName
                            dbParameter.ParameterName = parameterName;
                            il_.Emit(OpCodes.Ldstr, parameterName);//dbCollection/dbParameter/dbParameter/dbParameter/parameterName
                            il_.Emit(OpCodes.Callvirt, _dbParameterParameterName);//dbCollection/dbParameter/dbParameter

                            //Load parent object into stack
                            il_.Emit(OpCodes.Ldarg_1);//dbCollection/dbParameter/dbParameter/object

                            //Load property value into stack
                            if (boxType.IsValueType)
                            {
                                il_.Emit(OpCodes.Call, p.GetGetMethod());//dbCollection/dbParameter/dbParameter/value 
                            }
                            else
                            {
                                il_.Emit(OpCodes.Callvirt, p.GetGetMethod());//dbCollection/dbParameter/dbParameter/value 
                            }


                            //Check if null
                            if (isNeedNullCheck)
                            {
                                if (boxType.IsValueType)
                                {
                                    //Nullables are a headache!
                                    MethodInfo _nullableHasValue = propertyType.GetMethod("get_HasValue");
                                    MethodInfo _nullableValue = propertyType.GetMethod("get_Value");

                                    int localNullableStructIndex = il.DeclareLocal(propertyType).LocalIndex;

                                    //Throw in some dark magic here!
                                    il_.StoreLocal(localNullableStructIndex);//dbCollection/dbParameter/dbParameter                                    
                                    il_.LoadLocalAddress(localNullableStructIndex);//dbCollection/dbParameter/dbParameter/value_address
                                    il_.Emit(OpCodes.Dup);//dbCollection/dbParameter/dbParameter/value_address/value_address
                                    il_.Emit(OpCodes.Call, _nullableHasValue);//dbCollection/dbParameter/dbParameter/value_address/true or false
                                    il_.Emit(OpCodes.Brfalse_S, lblIsNull);//dbCollection/dbParameter/dbParameter/value_address
                                    il_.Emit(OpCodes.Call, _nullableValue);//dbCollection/dbParameter/dbParameter/value
                                }
                                else
                                {
                                    il_.Emit(OpCodes.Dup);//dbCollection/dbParameter/dbParameter/value/value
                                    il_.Emit(OpCodes.Ldnull);//dbCollection/dbParameter/dbParameter/value/value/null
                                    il_.Emit(OpCodes.Beq_S, lblIsNull);//dbCollection/dbParameter/dbParameter/value 
                                }
                            }


                            //Check if Enum (darn it!)
                            if (boxType.IsEnum)
                            {
                                //Change Type
                                Type enumUnderlyingType = Enum.GetUnderlyingType(boxType);
                                il_.Emit(OpCodes.Box, boxType);//dbCollection/dbParameter/dbParameter/value_boxed
                                il_.Emit(OpCodes.Ldtoken, enumUnderlyingType);//dbCollection/dbParameter/dbParameter/value_boxed/enum_type_token
                                il_.Emit(OpCodes.Call, DbConnectorUtilities._typeGetTypeFromHandle);//dbCollection/dbParameter/dbParameter/value_boxed/enum_type
                                il_.Emit(OpCodes.Call, _convertChangeType); //dbCollection/dbParameter/dbParameter/typed_value_boxed

                                //Add
                                il_.Emit(OpCodes.Callvirt, _dbParameterValue);//dbCollection/dbParameter                      
                                il_.Emit(OpCodes.Br_S, lblAddToCollection);//dbCollection/dbParameter

                                dbParameter.Value = value == null ? DBNull.Value : Convert.ChangeType(value, enumUnderlyingType);
                            }
                            else if (!boxType.IsArray)
                            {
                                if (boxType.IsValueType)
                                {
                                    il_.Emit(OpCodes.Box, boxType);//dbCollection/dbParameter/dbParameter/value_boxed
                                }

                                //Add
                                il_.Emit(OpCodes.Callvirt, _dbParameterValue);//dbCollection/dbParameter                      
                                il_.Emit(OpCodes.Br_S, lblAddToCollection);//dbCollection/dbParameter

                                dbParameter.Value = value ?? DBNull.Value;
                            }
                            else
                            {
                                Type referredType = boxType.GetElementType();

                                if (referredType.IsEnum)
                                {
                                    Type enumUnderlyingType = Enum.GetUnderlyingType(referredType);

                                    il_.Emit(OpCodes.Ldtoken, enumUnderlyingType);//dbCollection/dbParameter/dbParameter/value/enum_underlying_type_token
                                    il_.Emit(OpCodes.Call, DbConnectorUtilities._typeGetTypeFromHandle);//dbCollection/dbParameter/dbParameter/value/enum_underlying_type

                                    //Add
                                    var geneicAddParameterByEnumArray = _addParameterByEnumArray.MakeGenericMethod(referredType, enumUnderlyingType);
                                    il_.Emit(OpCodes.Call, geneicAddParameterByEnumArray);//dbCollection/dbParameter
                                    il_.Emit(OpCodes.Br_S, lblAddToCollection);//dbCollection/dbParameter

                                    dbParameter.Value = value == null ? DBNull.Value : geneicAddParameterByEnumArray.Invoke(dbJobParameterCollection_, new object[] { enumUnderlyingType, value, dbParameter });
                                }
                                else
                                {
                                    //Add
                                    il_.Emit(OpCodes.Callvirt, _dbParameterValue);//dbCollection/dbParameter                      
                                    il_.Emit(OpCodes.Br_S, lblAddToCollection);//dbCollection/dbParameter

                                    dbParameter.Value = value ?? DBNull.Value;
                                }
                            }

                            //Set DBNull if null
                            if (isNeedNullCheck)
                            {
                                il_.MarkLabel(lblIsNull);//incoming stack: //dbCollection/dbParameter/dbParameter/value
                                il_.Emit(OpCodes.Pop);//dbCollection/dbParameter/dbParameter
                                il_.Emit(OpCodes.Ldsfld, DynamicColumnMapper._DBNullValue);//dbCollection/dbParameter/dbParameter/dbNull
                                il_.Emit(OpCodes.Callvirt, _dbParameterValue);//dbCollection/dbParameter 
                            }

                            //Add DbParameter into Collection
                            dbJobParameterCollection_._collection.Add(dbParameter);
                            il_.MarkLabel(lblAddToCollection);//incoming stack: //dbCollection/dbParameter
                            il_.Emit(OpCodes.Callvirt, _dbCollectionAdd);//int
                            il_.Emit(OpCodes.Pop);//                            
                        }

                        builderMethods.Enqueue(builderMethod);
                    }
                }


                if (builderMethods.Count > 0)
                {
                    bool hasParamsPrefix = !string.IsNullOrWhiteSpace(paramsPrefix);
                    bool hasParamsSuffix = !string.IsNullOrWhiteSpace(paramsSuffix);

                    il.Emit(OpCodes.Ldarg_0);//target_address
                    //il.Emit(OpCodes.Ldind_Ref);//target
                    il.Emit(OpCodes.Ldfld, _dbCollection);//dbCollection

                    while (builderMethods.Count > 0)
                    {
                        if (builderMethods.Count > 1)
                        {
                            il.Emit(OpCodes.Dup);//dbCollection/dbCollection
                        }

                        il.Emit(OpCodes.Ldarg_0);//dbCollection/target_address
                        il.Emit(OpCodes.Ldfld, _dbCommand);//dbCollection/dbCommand
                        il.Emit(OpCodes.Callvirt, _dbCommandCreateParameter);//dbCollection/dbParameter
                        il.Emit(OpCodes.Dup);//dbCollection/dbParameter/dbParameter
                        il.Emit(OpCodes.Dup);//dbCollection/dbParameter/dbParameter/dbParameter

                        builderMethods.Dequeue()(
                            il,
                            isUseColumnAttributeNames,
                            hasParamsPrefix,
                            hasParamsSuffix,
                            paramsPrefix,
                            paramsSuffix,
                            obj,
                            dbJobParameterCollection);
                    }
                }
            }

            il.Emit(OpCodes.Ret);//

            return (Action<DbJobParameterCollection, object>)method.CreateDelegate(typeof(Action<DbJobParameterCollection, object>));
        }

        public static DynamicParameterBuilder CreateBuilder<T>(T obj, Type tType, DbJobParameterCollection dbJobParameterCollection, bool isUseColumnAttributeNames, string paramsPrefix, string paramsSuffix)
        {
            DynamicParameterBuilder builder = new DynamicParameterBuilder
            {
                AddFor = CreateBuilderAction(obj, tType, dbJobParameterCollection, isUseColumnAttributeNames, paramsPrefix, paramsSuffix)
            };
            return builder;
        }
    }

    internal class DynamicDbJobMethodBuilder
    {
        public Func<IDbJob, DbConnection, CancellationToken, IDbJobState, dynamic> OnExecuteDeferred { get; private set; }


        public static Func<IDbJob, DbConnection, CancellationToken, IDbJobState, dynamic> CreateBuilderFunction(Type tType, MethodInfo mi)
        {
            DynamicMethod method = new DynamicMethod("DynamicExecuteDeferred_" + Guid.NewGuid().ToString(), typeof(object),
                    new Type[] { typeof(IDbJob), typeof(DbConnection), typeof(CancellationToken), typeof(IDbJobState) }, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Ldarg_0);//IDbJob
            il.Emit(OpCodes.Ldarg_1);//IDbJob/DbConnection
            il.Emit(OpCodes.Ldarg_2);//IDbJob/DbConnection/token
            il.Emit(OpCodes.Ldarg_3);//IDbJob/DbConnection/token/IDbJobState
            il.Emit(OpCodes.Call, mi); //object
            il.Emit(OpCodes.Ret);//


            return (Func<IDbJob, DbConnection, CancellationToken, IDbJobState, dynamic>)method.CreateDelegate(typeof(Func<IDbJob, DbConnection, CancellationToken, IDbJobState, dynamic>));
        }


        public static DynamicDbJobMethodBuilder CreateBuilder(Type tType, MethodInfo mi)
        {
            DynamicDbJobMethodBuilder builder = new DynamicDbJobMethodBuilder
            {
                OnExecuteDeferred = CreateBuilderFunction(tType, mi)
            };

            return builder;
        }
    }



    internal interface IDynamicDbConnectorMethodBuilder
    {
        void CreateDelegate(DynamicMethod method);
    }

    internal class DynamicDbConnectorMethodBuilder : IDynamicDbConnectorMethodBuilder
    {
        public Func<object, Action<IDbJobCommand>, IDbJob> OnBuild { get; private set; }


        public virtual void CreateDelegate(DynamicMethod method)
        {
            OnBuild = (Func<object, Action<IDbJobCommand>, IDbJob>)method.CreateDelegate(typeof(Func<object, Action<IDbJobCommand>, IDbJob>));
        }


        public static DynamicDbConnectorMethodBuilder CreateBuilder(Type tType, MethodInfo mi)
        {
            DynamicDbConnectorMethodBuilder builder = new DynamicDbConnectorMethodBuilder();


            DynamicMethod method = new DynamicMethod("DynamicRead_" + Guid.NewGuid().ToString(), typeof(IDbJob),
                    new Type[] { typeof(object), typeof(Action<IDbJobCommand>) }, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Ldarg_0);//dbConnector
            il.Emit(OpCodes.Ldarg_1);//dbConnector/action
            il.Emit(OpCodes.Call, mi); //target
            il.Emit(OpCodes.Ret);//


            builder.OnBuild = (Func<object, Action<IDbJobCommand>, IDbJob>)method.CreateDelegate(typeof(Func<object, Action<IDbJobCommand>, IDbJob>));

            return builder;
        }

        public static DynamicDbConnectorMethodBuilder CreateBuilder(Type tType, MethodInfo mi, Type tspType)
        {
            DynamicDbConnectorMethodBuilder builder = Activator.CreateInstance(typeof(DynamicDbConnectorMethodBuilder<>).MakeGenericType(tspType)) as DynamicDbConnectorMethodBuilder;


            DynamicMethod method = new DynamicMethod("DynamicRead_" + Guid.NewGuid().ToString(), typeof(IDbJob),
                    new Type[] { typeof(object), typeof(Action<IDbJobCommand>), tspType }, tType, true);
            ILGenerator il = method.GetILGenerator();


            il.Emit(OpCodes.Ldarg_0);//dbConnector
            il.Emit(OpCodes.Ldarg_1);//dbConnector/action
            il.Emit(OpCodes.Ldarg_2);//dbConnector/action/TStateParam
            il.Emit(OpCodes.Call, mi); //target
            il.Emit(OpCodes.Ret);//


            builder.CreateDelegate(method);

            return builder;
        }
    }

    internal class DynamicDbConnectorMethodBuilder<TStateParam> : DynamicDbConnectorMethodBuilder
    {
        public Func<object, Action<IDbJobCommand>, TStateParam, IDbJob> OnBuildByState { get; private set; }

        public override void CreateDelegate(DynamicMethod method)
        {
            OnBuildByState = (Func<object, Action<IDbJobCommand>, TStateParam, IDbJob>)method.CreateDelegate(typeof(Func<object, Action<IDbJobCommand>, TStateParam, IDbJob>));
        }
    }



    internal static class ILGeneratorExtensions
    {
        public static void LoadLocalAddress(this ILGenerator il, int index)
        {
            if (index <= 255)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)index);
            }
            else
            {
                il.Emit(OpCodes.Ldloca, (short)index);
            }
        }

        public static void LoadInt(this ILGenerator il, int value)
        {
            switch (value)
            {
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (byte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        public static void LoadLocal(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0: il.Emit(OpCodes.Ldloc_0); break;
                case 1: il.Emit(OpCodes.Ldloc_1); break;
                case 2: il.Emit(OpCodes.Ldloc_2); break;
                case 3: il.Emit(OpCodes.Ldloc_3); break;
                default:
                    if (index <= 255)
                    {
                        il.Emit(OpCodes.Ldloc_S, (byte)index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, (short)index);
                    }
                    break;
            }
        }

        public static void StoreLocal(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0: il.Emit(OpCodes.Stloc_0); break;
                case 1: il.Emit(OpCodes.Stloc_1); break;
                case 2: il.Emit(OpCodes.Stloc_2); break;
                case 3: il.Emit(OpCodes.Stloc_3); break;
                default:
                    if (index <= 255)
                    {
                        il.Emit(OpCodes.Stloc_S, (byte)index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stloc, (short)index);
                    }
                    break;
            }
        }
    }
}

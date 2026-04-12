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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DbConnector.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNumeric(this Type tType)
        {
            return (tType.IsPrimitive && !(
                   tType == typeof(bool)
                || tType == typeof(char)
                || tType == typeof(IntPtr)
                || tType == typeof(UIntPtr))) || (tType == typeof(decimal));
        }

        internal static bool IsValidIndirectType(this Type from)
        {
            bool isValid = false;

            switch (Type.GetTypeCode(from))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    isValid = true;
                    break;
            }

            return isValid;
        }

        internal static bool IsValidIndirectMatch(this Type from, Type to)
        {
            bool isValid = false;

            switch (Type.GetTypeCode(from))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    isValid = true;
                    switch (Type.GetTypeCode(to))
                    {
                        case TypeCode.Byte:
                            break;
                        case TypeCode.SByte:
                            break;
                        case TypeCode.UInt16:
                            break;
                        case TypeCode.Int16:
                            break;
                        case TypeCode.UInt32:
                            break;
                        case TypeCode.Boolean:
                        case TypeCode.Int32:
                            break;
                        case TypeCode.UInt64:
                            break;
                        case TypeCode.Int64:
                            break;
                        case TypeCode.Single:
                            break;
                        case TypeCode.Double:
                            break;
                        default:
                            isValid = false;
                            break;
                    }
                    break;
            }

            return isValid;
        }

        internal static bool IsValidIndirectMatch(this Type from, Type to, out OpCode opCode)
        {
            bool isValid = false;

            switch (Type.GetTypeCode(from))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    isValid = true;
                    switch (Type.GetTypeCode(to))
                    {
                        case TypeCode.Byte:
                            opCode = OpCodes.Conv_Ovf_I1_Un; break;
                        case TypeCode.SByte:
                            opCode = OpCodes.Conv_Ovf_I1; break;
                        case TypeCode.UInt16:
                            opCode = OpCodes.Conv_Ovf_I2_Un; break;
                        case TypeCode.Int16:
                            opCode = OpCodes.Conv_Ovf_I2; break;
                        case TypeCode.UInt32:
                            opCode = OpCodes.Conv_Ovf_I4_Un; break;
                        case TypeCode.Boolean:
                        case TypeCode.Int32:
                            opCode = OpCodes.Conv_Ovf_I4; break;
                        case TypeCode.UInt64:
                            opCode = OpCodes.Conv_Ovf_I8_Un; break;
                        case TypeCode.Int64:
                            opCode = OpCodes.Conv_Ovf_I8; break;
                        case TypeCode.Single:
                            opCode = OpCodes.Conv_R4; break;
                        case TypeCode.Double:
                            opCode = OpCodes.Conv_R8; break;
                        default:
                            isValid = false;
                            break;
                    }
                    break;
            }

            return isValid;
        }

        public static bool IsNullable(this Type tType)
        {
            return !tType.IsValueType || (Nullable.GetUnderlyingType(tType) != null);
        }

        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector
        )
        where TAttribute : Attribute
        {
            TAttribute att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;

            if (att != null)
            {
                return valueSelector(att);
            }

            return default(TValue);
        }

        /// <summary>
        /// Determines whether the specified type is a classic System.Tuple type.
        /// </summary>
        /// <remarks>This method checks for classic tuple types defined in the System namespace, such as
        /// Tuple&lt;T1&gt;, Tuple&lt;T1, T2&gt;, and so on. It does not recognize ValueTuple types or tuples defined in other
        /// namespaces.</remarks>
        /// <param name="t">The type to evaluate.</param>
        /// <returns>true if the type is a generic System.Tuple type; otherwise, false.</returns>
        public static bool IsClassicTuple(this Type t)
        {
            if (!t.IsGenericType) return false;

            Type genericDef = t.GetGenericTypeDefinition();

            // Comparing Namespace and Name separately is slightly safer than FullName.StartsWith
            return genericDef.Namespace == "System" &&
                   genericDef.Name.StartsWith("Tuple`", StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified type is a System.ValueTuple type.
        /// </summary>
        /// <remarks>This method checks for any arity of System.ValueTuple, such as ValueTuple&lt;T1&gt;,
        /// ValueTuple&lt;T1, T2&gt;, and so on. It does not consider non-generic types or tuples defined before C#
        /// 7.0.
        /// </remarks>
        /// <param name="t">The type to evaluate for being a ValueTuple.</param>
        /// <returns>true if the specified type is a generic System.ValueTuple type; otherwise, false.</returns>
        public static bool IsValueTuple(this Type t)
        {
            if (!t.IsGenericType) return false;

            Type genericDef = t.GetGenericTypeDefinition();

            // Comparing Namespace and Name separately is slightly safer than FullName.StartsWith
            return genericDef.Namespace == "System" &&
                   genericDef.Name.StartsWith("ValueTuple`", StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified type represents a System.Tuple or System.ValueTuple type.
        /// </summary>
        /// <remarks>This method checks both generic Tuple and ValueTuple types, including all arities.
        /// Non-generic types and types outside the System namespace are not considered tuples.</remarks>
        /// <param name="t">The type to evaluate. This parameter must not be null.</param>
        /// <returns>true if the type is a System.Tuple or System.ValueTuple type; otherwise, false.</returns>
        public static bool IsAnyTuple(this Type t)
        {
            if (!t.IsGenericType) return false;

            // Get the definition (e.g., ValueTuple<,> instead of ValueTuple<int, string>)
            Type genericDef = t.GetGenericTypeDefinition();

            // Check if it's in the System namespace and follows the Tuple naming convention
            return genericDef.Namespace == "System" &&
                   (genericDef.Name.StartsWith("ValueTuple`", StringComparison.Ordinal) ||
                    genericDef.Name.StartsWith("Tuple`", StringComparison.Ordinal));
        }

        /// <summary>
        /// This method recursively retrieves all the generic type arguments from a tuple type, including nested tuples in the "Rest" position of 8-arity tuples. It returns a flat list of all types contained within the tuple structure.
        /// </summary>
        /// <param name="t">The tuple type to analyze.</param>
        /// <returns>A list of all types contained within the tuple structure.</returns>
        public static List<Type> GetAllTupleTypes(this Type t)
        {
            var types = new List<Type>();

            if (!t.IsAnyTuple())
                return types;

            Type[] genericArgs = t.GetGenericArguments();

            // If it has 8 arguments, the 8th is the "Rest" container
            if (genericArgs.Length == 8)
            {
                // Add the first 7 types
                types.AddRange(genericArgs.Take(7));

                // Recurse into the 8th type
                types.AddRange(GetAllTupleTypes(genericArgs[7]));
            }
            else
            {
                // Add all types (1 through 7)
                types.AddRange(genericArgs);
            }

            return types;
        }

        /// <summary>
        /// Helper to calculate total capacity (including nested levels)
        /// </summary>
        /// <param name="t">The tuple type to analyze.</param>
        /// <returns>The total number of elements in the tuple, including nested tuples.</returns>
        public static int GetTupleCapacity(this Type t)
        {
            Type[] args = t.GetGenericArguments();
            return (args.Length == 8)
                ? 7 + GetTupleCapacity(args[7])
                : args.Length;
        }

    }
}

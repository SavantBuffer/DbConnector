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
        Func<TAttribute, TValue> valueSelector)
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
    }
}

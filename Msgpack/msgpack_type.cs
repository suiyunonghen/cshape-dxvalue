namespace DxMsgPack
{
    using System.Collections.Generic;
    using System.IO;
    using DxValue;

    public class DxMsgPack
    {
        public const int Max_fixmap_len     = 15;

        public const int Max_map16_len      = 1 << 16 - 1;

        public const int Max_map32_len      = 1 << 32 - 1;


        public const int Max_fixstr_len     = 32 - 1;
        public const int Max_str8_len       = 1 << 8 - 1;
        public const int Max_str16_len      = 1 << 16 - 1;
        public const int Max_str32_len      = 1 << 32 - 1;
        public enum MsgPackCode
        {
            CodeUnkonw = 0,
            PosFixedNumHigh = 0x7f,  //0-7f的正数最大编码
            NegFixedNumLow  = 0xe0, //固定大小的负数编码
            CodeNil = 0xc0,
            CodeFalse = 0xc2,
            CodeTrue  = 0xc3,
            CodeFloat  = 0xca,
            CodeDouble = 0xcb,
            CodeUint8  = 0xcc,
            CodeUint16 = 0xcd,
            CodeUint32 = 0xce,
            CodeUint64 = 0xcf,
            CodeInt8  = 0xd0,
            CodeInt16 = 0xd1,
            CodeInt32 = 0xd2,
            CodeInt64 = 0xd3,
            CodeFixedStrLow  = 0xa0,
            CodeFixedStrHigh = 0xbf,
            FixedStrMask = 0x1f,
            CodeStr8         = 0xd9,
            CodeStr16        = 0xda,
            CodeStr32        = 0xdb,
            CodeBin8  = 0xc4,
            CodeBin16 = 0xc5,
            CodeBin32 = 0xc6,
            CodeFixedArrayLow  = 0x90,
            CodeFixedArrayHigh = 0x9f,
            FixedArrayMask = 0xf,
            CodeArray16        = 0xdc,
            CodeArray32        = 0xdd,
            CodeFixedMapLow  = 0x80,
            CodeFixedMapHigh = 0x8f,
            FixedMapMask = 0xf,
            CodeMap16        = 0xde,
            CodeMap32        = 0xdf,
            CodeFixExt1  = 0xd4,
            CodeFixExt2  = 0xd5,
            CodeFixExt4  = 0xd6,
            CodeFixExt8  = 0xd7, //64位时间格式
            CodeFixExt16 = 0xd8,
            CodeExt8     = 0xc7, //96位时间格式
            CodeExt16    = 0xc8,
            CodeExt32    = 0xc9
        }
        
        public static bool IsExt(MsgPackCode code)
        {
            return (code >= MsgPackCode.CodeFixExt1 && code <= MsgPackCode.CodeFixExt16 && code != MsgPackCode.CodeFixExt4 && code != MsgPackCode.CodeFixExt8) ||
                (code >= MsgPackCode.CodeExt8 && code <= MsgPackCode.CodeExt32);
        }

        public static bool IsMap(MsgPackCode code)
        {
            return code >= MsgPackCode.CodeFixedMapLow && code <= MsgPackCode.CodeFixedMapHigh || code == MsgPackCode.CodeMap16 || code == MsgPackCode.CodeMap32;
        }

        public static bool IsFixedNum(MsgPackCode code)
        {
            return code <= MsgPackCode.PosFixedNumHigh || code >= MsgPackCode.NegFixedNumLow;
        }

        public static bool IsInt(MsgPackCode code)
        {
            return code <= MsgPackCode.PosFixedNumHigh || code >= MsgPackCode.NegFixedNumLow || code >= MsgPackCode.CodeUint8 && code <= MsgPackCode.CodeUint64 || code >= MsgPackCode.CodeInt8 && code <= MsgPackCode.CodeInt64;
        }

        public static bool IsStr(MsgPackCode code)
        {
            return code >= MsgPackCode.CodeFixedStrLow && code <= MsgPackCode.CodeFixedStrHigh || code >= MsgPackCode.CodeStr8 && code <= MsgPackCode.CodeStr32;
        }

        public static bool IsArray(MsgPackCode code)
        {
            return code >= MsgPackCode.CodeFixedArrayLow && code <= MsgPackCode.CodeFixedArrayHigh || code == MsgPackCode.CodeArray16 || code == MsgPackCode.CodeArray32;
        }

        public static bool IsBin(MsgPackCode code)
        {
            return code >= MsgPackCode.CodeBin8 && code <= MsgPackCode.CodeBin32;
        }

        public class Decoder
        {
            MemoryStream buffer;
            public Decoder(byte[] bytebuffer)
            {
                buffer = new MemoryStream(bytebuffer);
            }


            public Decoder(Stream stream)
            {
                buffer = new MemoryStream();
                stream.Position = 0;
                stream.CopyTo(buffer);
                buffer.Position = 0;
            }

            public void Reset(byte[] bytebuffer)
            {
                buffer.SetLength(bytebuffer.Length);
                buffer.Write(bytebuffer, 0, bytebuffer.Length);
                buffer.Position = 0;
            }

            

            public bool ReadCode(out MsgPackCode code)
            {
                if(buffer.Length - buffer.Position < 1)
                {
                    code = MsgPackCode.CodeUnkonw;
                    return false;
                }
                code = (MsgPackCode)buffer.ReadByte();
                return true;
            }

            public bool DecodeString(MsgPackCode code,out string value)
            {
                value = "";
                if (code == MsgPackCode.CodeUnkonw)
                {
                    bool result = ReadCode(out code);
                    if (!result)
                    {
                        return false;
                    }
                }                                                
                int strlen = 0;
                switch (code)
                {
                    case MsgPackCode.CodeStr8:
                        strlen = buffer.ReadByte();
                        break;
                    case MsgPackCode.CodeStr16:
                        strlen = (int)BigEndian.Uint16(buffer);
                        break;
                    case MsgPackCode.CodeStr32:
                        strlen = (int)BigEndian.Uint32(buffer);
                        break;
                    default:
                        if((int)code < 0xa0 || (int)code > 0xbf)
                        {
                            value = "";
                            return false;
                        }
                        strlen = (int)code & (int)MsgPackCode.FixedStrMask;
                        break;
                }
                if (strlen > 0)
                {
                    byte[] strbyte = new byte[strlen];
                    int readlen = buffer.Read(strbyte, 0, strlen);
                    value = System.Text.Encoding.UTF8.GetString(strbyte);                    
                    return strlen == readlen;
                }
                return false;
            }

            public bool DecodeMapLen(MsgPackCode code, out uint len)
            {
                len = 0;
                if (code == MsgPackCode.CodeUnkonw)
                {
                    bool result = ReadCode(out code);
                    if (!result)
                    {
                        return false;
                    }
                }
                switch (code)
                {
                    case MsgPackCode.CodeMap16:
                        ushort v = BigEndian.Uint16(buffer);
                        len = (uint)v;
                        return true;
                    case MsgPackCode.CodeMap32:
                        len = BigEndian.Uint32(buffer);                        
                        return true;
                    default:
                        if(code >= MsgPackCode.CodeFixedMapLow && code <= MsgPackCode.CodeFixedMapHigh)
                        {
                            len = (uint)code & (uint)MsgPackCode.FixedMapMask;
                            return true;
                        }
                        return false;
                }
            }

            public bool BinaryLen(MsgPackCode code,out int len)
            {
                len = 0;
                if(code == MsgPackCode.CodeUnkonw)
                {
                    if (!ReadCode(out code)) { return false; }
                }
                switch (code)
                {
                    case MsgPackCode.CodeBin8:
                        len = (int)buffer.ReadByte();
                        return true;
                    case MsgPackCode.CodeBin16:
                        len =(int)BigEndian.Uint16(buffer);
                        return true;
                    case MsgPackCode.CodeBin32:
                        len = (int)BigEndian.Uint32(buffer);
                        return true;
                }
                return false;
            }

            public bool DecodeBinary(MsgPackCode code,out byte[] value)
            {
                int btlen;
                value = null;
                if(!BinaryLen(code, out btlen)) { return false; }
                if (btlen > 0)
                {
                    value = new byte[btlen];
                    buffer.Read(value, 0, btlen);
                }
                return true;
            }

            public bool DecodeExtValue(MsgPackCode code,out byte[] valuedata)
            {
                valuedata = null;
                if(code == MsgPackCode.CodeUnkonw)
                {
                    if (!ReadCode(out code)) { return false; }
                }
                uint btlen = 0;
                switch (code)
                {
                    case MsgPackCode.CodeFixExt1:
                        btlen = 2;
                        break;
                    case MsgPackCode.CodeFixExt2:
                        btlen = 3;
                        break;
                    case MsgPackCode.CodeFixExt4:
                        btlen = 5;
                        break;
                    case MsgPackCode.CodeFixExt8:
                        btlen = 9;
                        break;
                    case MsgPackCode.CodeFixExt16:
                        btlen = 17;
                        break;
                    case MsgPackCode.CodeExt8:
                        byte bt = (byte)buffer.ReadByte();
                        btlen = (uint)bt + 1;
                        break;
                    case MsgPackCode.CodeExt16:
                        ushort v16 = BigEndian.Uint16(buffer);
                        btlen = (uint)v16 + 1;
                        break;
                    case MsgPackCode.CodeExt32:
                        uint v32 = BigEndian.Uint32(buffer);
                        btlen = v32 + 1;
                        break;
                    default:
                        return false;
                }
                valuedata = new byte[btlen];
                if( buffer.Read(valuedata, 0, (int)btlen) != btlen)
                {
                    valuedata = null;
                    return false;
                }                
                return true;
            }

            public bool DecodeMapKv(DxRecordValue mapValue, MsgPackCode code)
            {
                string key;
                if (!DecodeString(code, out key)) { return false; }
                if(!ReadCode(out code)) { return false; }
                if (IsStr(code))
                {
                    string value;
                    if(!DecodeString(code,out value)) { return false; }
                    mapValue.SetString(key, value);
                }else if (IsFixedNum(code))
                {
                    mapValue.SetInt(key, (int)(sbyte)code);
                }else if (IsInt(code))
                {
                    long v;
                    if (!DecodeInt(code, out v)) { return false; }
                    if(v>int.MaxValue || v < int.MinValue){
                        mapValue.SetInt64(key, v);
                    }
                    else { mapValue.SetInt(key, (int)v); }
                }else if (IsMap(code))
                {
                    DxRecordValue v;
                    if(!DecodeMap(code, out v)) { return false; }
                    mapValue[key] = v;
                }else if (IsArray(code))
                {
                    DxArrayValue arr = new DxArrayValue();
                    if (!Decode2Array(code, arr)) { return false; }
                    mapValue[key] = arr;
                }else if (IsBin(code))
                {
                    byte[] binary;
                    if (!DecodeBinary(code, out binary)) { return false; }
                    if (binary != null)
                    {
                        DxBinaryValue binaryValue = new DxBinaryValue();
                        binaryValue.Bytes = binary;
                        mapValue[key] = binaryValue;
                    }                    
                }else if (IsExt(code))
                {
                    byte[] binary;
                    if(!DecodeExtValue(code,out binary)) { return false; }
                    if(binary.Length == 13 && (sbyte)binary[0] == -1) //96位日期格式
                    {
                        //32位纳秒，64位秒                        
                        Stream stream = new MemoryStream(binary);
                        stream.Position = 1;
                        uint nsec = BigEndian.Uint32(stream);
                        ulong sec = BigEndian.Uint64(stream);
                        stream.Dispose();
                        System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                        double sec1 = nsec / (1000 * 1000);
                        mapValue.SetDateTime(key, dateTime.AddSeconds(sec + sec1));
                    }
                    else
                    {
                        mapValue[key] = new DxExtValue(binary);
                    }
                }
                else
                {
                    switch (code)
                    {
                        case MsgPackCode.CodeTrue:
                            mapValue.SetBool(key, true);
                            return true;
                        case MsgPackCode.CodeFalse:
                            mapValue.SetBool(key, false);
                            return true;
                        case MsgPackCode.CodeNil:
                            mapValue[key] = null;
                            return true;
                        case MsgPackCode.CodeFloat:
                            uint v = BigEndian.Uint32(buffer);
                            mapValue.SetFloat(key, (float)System.BitConverter.Int64BitsToDouble((long)v));
                            return true;
                        case MsgPackCode.CodeDouble:
                            ulong v1 = BigEndian.Uint64(buffer);
                            mapValue.SetDouble(key, System.BitConverter.Int64BitsToDouble((long)v1));
                            return true;
                        case MsgPackCode.CodeFixExt4:
                            if (!ReadCode(out code)) { return false; }
                            if((sbyte)code == -1) //时间
                            {
                                uint seconds = BigEndian.Uint32(buffer);
                                System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                                mapValue.SetDateTime(key, dateTime.AddSeconds((double)seconds));
                            }
                            else
                            {
                                byte[] bt = new byte[5];
                                buffer.Read(bt, 1, 4);
                                DxExtValue dxExtValue = new DxExtValue(bt);
                                mapValue[key] = dxExtValue;
                            }
                            return true;
                        case MsgPackCode.CodeFixExt8:
                            //64位时间格式
                            if (!ReadCode(out code)) { return false; }
                            if ((sbyte)code == -1) //时间
                            {
                                ulong seconds = BigEndian.Uint64(buffer);
                                long nsec = (long)(seconds >> 34);
                                seconds &= 0x00000003ffffffff;
                                System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                                double sec1 = nsec / (1000 * 1000);
                                mapValue.SetDateTime(key, dateTime.AddSeconds(seconds + sec1));
                            }
                            else
                            {
                                byte[] bt = new byte[9];
                                buffer.Read(bt, 1, 8);
                                DxExtValue dxExtValue = new DxExtValue(bt);
                                mapValue[key] = dxExtValue;
                            }
                            return true;
                    }
                }
                return true;
            }

            public bool DecodeMap(MsgPackCode code,out DxRecordValue value)
            {
                value = null;
                if(code == MsgPackCode.CodeUnkonw)
                {
                    bool result = ReadCode(out code);
                    if (!result)
                    {
                        return false;
                    }
                }
                uint maplen;
                if(!DecodeMapLen(code,out maplen))
                {
                    return false;
                }                
                if(maplen == 0)
                {
                    return true;   
                }
                if(!ReadCode(out code) || !IsStr(code))
                {
                    return false;
                }
                value = new DxRecordValue();
                if (!DecodeMapKv(value, code)) { return false; }
                for(int i = 1; i < maplen; i++)
                {
                    if (!DecodeMapKv(value, MsgPackCode.CodeUnkonw)) { return false; }
                }
                return true;
            }

            public bool Decode2Map(MsgPackCode code,DxRecordValue value)
            {                
                if (code == MsgPackCode.CodeUnkonw)
                {
                    bool result = ReadCode(out code);
                    if (!result && !IsMap(code))
                    {
                        return false;
                    }
                }
                uint maplen;
                if (!DecodeMapLen(code, out maplen))
                {
                    return false;
                }
                if (maplen == 0)
                {
                    return true;
                }
                if (!ReadCode(out code) || !IsStr(code))
                {
                    return false;
                }                
                if (!DecodeMapKv(value, code)) { return false; }
                for (int i = 1; i < maplen; i++)
                {
                    if (!DecodeMapKv(value, MsgPackCode.CodeUnkonw)) { return false; }
                }
                return true;
            }

            public bool DecodeInt(MsgPackCode code, out long value)
            {
                value = 0;
                if (code == MsgPackCode.CodeUnkonw)
                {
                    bool result = ReadCode(out code);                    
                    if (!result)
                    {
                        return false;
                    }
                }                
                if(code <= MsgPackCode.PosFixedNumHigh)
                {
                    value = (long)code;
                    return true;
                }
                if((int)code >= 0xe0)
                {
                    value = (long)((sbyte)code);
                    return true;
                }
                switch (code)
                {
                    case MsgPackCode.CodeInt8:
                    case MsgPackCode.CodeUint8:
                        byte bt = (byte)buffer.ReadByte();
                        if(code == MsgPackCode.CodeInt8)
                        {
                            value = (long)((sbyte)bt);
                        }
                        else { value = (long)bt; }
                        return true;
                    case MsgPackCode.CodeInt16:
                    case MsgPackCode.CodeUint16:
                        ushort word = BigEndian.Uint16(buffer);
                        if (code == MsgPackCode.CodeInt16)
                        {
                            value = (long)((short)word);
                        }
                        else { value = (long)word; }
                        return true;
                    case MsgPackCode.CodeInt32:
                    case MsgPackCode.CodeUint32:
                        uint dword = BigEndian.Uint32(buffer);
                        if(code == MsgPackCode.CodeInt32)
                        {
                            value = (long)(int)code;
                        }
                        else { value = (long)code; }
                        return true;
                    case MsgPackCode.CodeInt64:
                    case MsgPackCode.CodeUint64:
                        ulong w = BigEndian.Uint64(buffer);
                        value = (long)w;
                        return true;
                    default:
                        return false;
                }
            }

            public bool DecodeArrayElement(DxArrayValue array,int eleIndex)
            {
                MsgPackCode code;
                if (!ReadCode(out code)) { return false; }
                if (IsStr(code))
                {
                    string strvalue;
                    if(!DecodeString(code,out strvalue)) { return false; }
                    array.SetString(eleIndex, strvalue);
                }else if (IsFixedNum(code))
                {
                    array.SetInt(eleIndex, (int)(sbyte)code);
                }else if (IsInt(code))
                {
                    long longvalue;
                    if(!DecodeInt(code,out longvalue)) { return false; }
                    if(longvalue>int.MaxValue || longvalue < int.MinValue)
                    {
                        array.SetInt64(eleIndex, longvalue);
                    }
                    else { array.SetInt(eleIndex, (int)longvalue); }
                }else if (IsMap(code))
                {
                    DxRecordValue mapvalue;
                    if(!DecodeMap(code,out mapvalue)) { return false; }
                    array[eleIndex] = mapvalue;
                }else if (IsArray(code))
                {
                    DxArrayValue result = new DxArrayValue();
                    if (!Decode2Array(code, result)) { return false; }
                    array[eleIndex] = result;
                }else if (IsBin(code))
                {
                    byte[] bytevalue;
                    if(!DecodeBinary(code,out bytevalue)) { return false; }
                    DxBinaryValue binaryvalue = new DxBinaryValue();
                    binaryvalue.Bytes = bytevalue;
                    array[eleIndex] = binaryvalue;
                }else if (IsExt(code))
                {
                    byte[] bytevalue;
                    if(!DecodeExtValue(code,out bytevalue)) { return false; }
                    if (bytevalue.Length == 13 && (sbyte)bytevalue[0] == -1) //96位日期格式
                    {
                        //32位纳秒，64位秒
                        Stream stream = new MemoryStream(bytevalue);
                        stream.Position = 1;
                        uint nsec = BigEndian.Uint32(stream);
                        ulong sec = BigEndian.Uint64(stream);
                        stream.Dispose();
                        System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                        double sec1 = nsec / (1000 * 1000);
                        array[eleIndex] = new DxDateTimeValue(dateTime.AddSeconds(sec + sec1));
                    }
                    else
                    {
                        array[eleIndex] = new DxExtValue(bytevalue);
                    }
                }
                else
                {
                    switch (code)
                    {
                        case MsgPackCode.CodeTrue:
                            array.SetBool(eleIndex, true);
                            return true;
                        case MsgPackCode.CodeFalse:
                            array.SetBool(eleIndex, false);
                            return true;
                        case MsgPackCode.CodeNil:
                            array.SetNull(eleIndex);
                            return true;
                        case MsgPackCode.CodeFloat:
                            uint v = BigEndian.Uint32(buffer);
                            array.SetFloat(eleIndex,(float)System.BitConverter.Int64BitsToDouble((long)v));
                            return true;
                        case MsgPackCode.CodeDouble:
                            ulong vlong = BigEndian.Uint64(buffer);
                            array.SetDouble(eleIndex, System.BitConverter.Int64BitsToDouble((long)vlong));
                            return true;
                        case MsgPackCode.CodeFixExt4:
                            if (!ReadCode(out code)) { return false; }
                            if ((sbyte)code == -1) //时间
                            {
                                uint seconds = BigEndian.Uint32(buffer);
                                System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                                array.SetDateTime(eleIndex, dateTime.AddSeconds((double)seconds));
                            }
                            else
                            {
                                byte[] bt = new byte[5];
                                buffer.Read(bt, 1, 4);
                                array[eleIndex] = new DxExtValue(bt);
                            }
                            return true;
                        case MsgPackCode.CodeFixExt8:
                            if (!ReadCode(out code)) { return false; }
                            if ((sbyte)code == -1) //时间
                            {
                                ulong seconds = BigEndian.Uint64(buffer);
                                long nsec = (long)(seconds >> 34);
                                seconds &= 0x00000003ffffffff;
                                System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                                double sec1 = nsec / (1000 * 1000);
                                array.SetDateTime(eleIndex, dateTime.AddSeconds(seconds + sec1));
                            }
                            else
                            {
                                byte[] bt = new byte[9];
                                buffer.Read(bt, 1, 8);
                                array[eleIndex] = new DxExtValue(bt);
                            }
                            return true;
                        default:
                            return false;
                    }
                }
                return true;
            }

            public bool Decode2Array(MsgPackCode code, DxArrayValue value)
            {                
                if (code == MsgPackCode.CodeUnkonw)
                {
                    if (!ReadCode(out code)) { return false; }
                }
                uint arrlen;
                if(!DecodeArrayLen(code,out arrlen)) { return false; }
                for(int i = 0; i < arrlen; i++)
                {
                    if (!DecodeArrayElement(value, i)) { return false; }
                }
                return true;
            }

            public bool DecodeArrayLen(MsgPackCode code,out uint len)
            {
                len = 0;
                if (code == MsgPackCode.CodeUnkonw)
                {
                    if (!ReadCode(out code)) { return false; }
                }
                switch (code)
                {
                    case MsgPackCode.CodeArray16:
                        ushort v16 = BigEndian.Uint16(buffer);
                        len = (uint)v16;
                        return true;
                    case MsgPackCode.CodeArray32:
                        uint v32 = BigEndian.Uint32(buffer);
                        len = (uint)v32;
                        return true;
                    default:
                        if (code >= MsgPackCode.CodeFixedArrayLow && code <= MsgPackCode.CodeFixedArrayHigh)
                        {
                            len = (uint)code & (uint)MsgPackCode.FixedArrayMask;
                            return true;
                        }
                        return false;
                }
            }

            public DxBaseValue Parser()
            {                
                MsgPackCode code;
                if (!ReadCode(out code))
                {
                    return null;
                }                
                //判断类型
                if (IsStr(code))
                {
                    string value;
                    if(DecodeString(code, out value))
                    {
                        return new DxStringValue(value);
                    }
                    return null;
                }else if (IsFixedNum(code))
                {
                    return new DxIntValue((int)code);
                }else if (IsInt(code))
                {
                    long v = 0;
                    if (DecodeInt(code, out v))
                    {
                        if (v <= int.MaxValue && v>=int.MinValue)
                        {
                            return new DxIntValue((int)v);
                        }
                        else
                        {
                            return new DxInt64Value(v);
                        }
                    }
                }else if (IsMap(code))
                {
                    DxRecordValue result = null;
                    if(DecodeMap(code, out result))
                    {
                        return result;
                    }                    
                }else if (IsArray(code))
                {
                    DxArrayValue result = new DxArrayValue();
                    if(!Decode2Array(code, result)) { return null; }
                    return result;

                }
                else if (IsBin(code))
                {
                    byte[] binary;
                    if (!DecodeBinary(code, out binary)) { return null; }
                    if (binary != null)
                    {
                        DxBinaryValue binaryValue = new DxBinaryValue();
                        binaryValue.Bytes = binary;
                        return binaryValue;
                    }
                }
                else if (IsExt(code))
                {
                    byte[] binary;
                    if (!DecodeExtValue(code, out binary)) { return null; }
                    if (binary.Length == 13 && (sbyte)binary[0] == -1) //96位日期格式
                    {
                        //32位纳秒，64位秒
                        Stream stream = new MemoryStream(binary);
                        stream.Position = 1;
                        uint nsec = BigEndian.Uint32(stream);
                        ulong sec = BigEndian.Uint64(stream);
                        stream.Dispose();
                        System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                        double sec1 = nsec / (1000 * 1000);
                        return new DxDateTimeValue(dateTime.AddSeconds(sec + sec1));     
                    }
                    else
                    {
                        return new DxExtValue(binary);
                    }
                }
                else
                {
                    switch (code)
                    {
                        case MsgPackCode.CodeTrue:
                            return new DxBoolValue(true);
                        case MsgPackCode.CodeFalse:
                            return new DxBoolValue(false);
                        case MsgPackCode.CodeFloat:
                            uint v = BigEndian.Uint32(buffer);
                            return new DxFloatValue((float)System.BitConverter.Int64BitsToDouble((long)v));
                        case MsgPackCode.CodeDouble:
                            ulong v1 = BigEndian.Uint64(buffer);
                            return new DxDoubleValue(System.BitConverter.Int64BitsToDouble((long)v1));                            
                        case MsgPackCode.CodeFixExt4:
                            if (!ReadCode(out code)) { return null; }
                            if ((sbyte)code == -1) //时间
                            {
                                uint seconds = BigEndian.Uint32(buffer);
                                System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                                return new DxDateTimeValue(dateTime.AddSeconds((double)seconds));
                            }
                            else
                            {
                                byte[] bt = new byte[5];
                                buffer.Read(bt, 1, 4);
                                return new DxExtValue(bt);                                
                            }                            
                        case MsgPackCode.CodeFixExt8:
                            //64位时间格式
                            if (!ReadCode(out code)) { return null; }
                            if ((sbyte)code == -1) //时间
                            {
                                ulong seconds = BigEndian.Uint64(buffer);
                                long nsec = (long)(seconds >> 34);
                                seconds &= 0x00000003ffffffff;
                                System.DateTime dateTime = new System.DateTime(1970, 1, 1);
                                double sec1 = nsec / (1000 * 1000);
                                return new DxDateTimeValue(dateTime.AddSeconds(seconds + sec1));
                            }
                            else
                            {
                                byte[] bt = new byte[9];
                                buffer.Read(bt, 1, 8);
                                return new DxExtValue(bt);                                
                            }
                    }
                }

                return null;
            }
        }

        public class Encoder
        {
            MemoryStream databuffer;
            byte[] buf;                
            public Encoder()
            {
                databuffer = new MemoryStream();
                buf = new byte[4];
            }

            public void Reset()
            {
                databuffer.SetLength(0);                
            }

            public void EncodeBool(bool b)
            {
                if (b)
                {
                    databuffer.WriteByte((byte)MsgPackCode.CodeTrue);
                }
                else
                {
                    databuffer.WriteByte((byte)MsgPackCode.CodeFalse);
                }
            }

            public void EncodeDateTime(System.DateTime datetime)
            {
                buf[0] = (byte)MsgPackCode.CodeFixExt4;
                buf[1] = 0xff;
                databuffer.Write(buf, 0, 2);
                System.DateTime startTime = System.TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1), System.TimeZoneInfo.Local);
                uint seconds = (uint)(datetime - startTime).TotalSeconds;
                BigEndian.PutUint32(databuffer, seconds);
            }

            void WriteUint16(ushort v, MsgPackCode code)
            {
                if(code != MsgPackCode.CodeUnkonw)
                {
                    databuffer.WriteByte((byte)code);
                }
                BigEndian.PutUint16(databuffer, v);
            }

            void WriteUint32(uint v, MsgPackCode code)
            {
                if (code != MsgPackCode.CodeUnkonw)
                {
                    databuffer.WriteByte((byte)code);
                }
                BigEndian.PutUint32(databuffer, v);
            }

            void WriteUint64(ulong v, MsgPackCode code)
            {
                if (code != MsgPackCode.CodeUnkonw)
                {
                    databuffer.WriteByte((byte)code);
                }
                BigEndian.PutUint64(databuffer, v);
            }

            public void EncodeInt(long vint)
            {
                if(vint >= 0 && vint <= 0x7f)//0XXXXXXX is 8-bit unsigned integer
                {
                    databuffer.WriteByte((byte)vint);
                }else if(vint >= -32 && vint < 0)  // 111YYYYY is 8-bit 5-bit negative integer
                {
                    databuffer.WriteByte((byte)((byte)MsgPackCode.NegFixedNumLow | (byte)vint));
                }else if(vint >= 0 && vint <= 0xff)
                {
                    buf[0] = (byte)MsgPackCode.CodeUint8;
                    buf[1] = (byte)vint;
                    databuffer.Write(buf, 0, 2);
                }else if(vint >= 0 && vint <= 0xffffffff)//0xce  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ
                {
                    WriteUint32((uint)vint, MsgPackCode.CodeUint32);
                }else if ((ulong)vint <= 0xffffffffffffffff) // 0xcf  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|
                {
                    WriteUint64((ulong)vint, MsgPackCode.CodeUint64);
                }else if (vint >= byte.MinValue && vint <= byte.MaxValue) //0xd0  |ZZZZZZZZ|
                {
                    buf[0] = (byte)MsgPackCode.CodeInt8;
                    buf[1] = (byte)vint;
                    databuffer.Write(buf, 0, 2);
                }else if (vint >= short.MinValue && vint <= short.MaxValue)//0xd1  |ZZZZZZZZ|ZZZZZZZZ|
                {
                    WriteUint16((ushort)vint, MsgPackCode.CodeInt16);
                }
                else if (vint >= 0 && vint <= 0xffff)//0xcd  |ZZZZZZZZ|ZZZZZZZZ
                {
                    WriteUint16((ushort)vint, MsgPackCode.CodeUint16);
                }else if (vint >= int.MinValue && vint <= int.MaxValue)//0xd2  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ
                {
                    WriteUint32((uint)vint, MsgPackCode.CodeInt32);
                }
                else //0xd3  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ
                {
                    WriteUint64((ulong)vint, MsgPackCode.CodeInt64);
                }
            }

            public void EncodeString(string value)
            {
                byte[] b = System.Text.Encoding.UTF8.GetBytes(value);
                int strlen = b.Length;
                if (strlen <= Max_fixstr_len)
                {
                    databuffer.WriteByte((byte)((byte)MsgPackCode.CodeFixedStrLow | (byte)strlen));
                }
                else if (strlen <= Max_str8_len)
                {
                    buf[0] = (byte)MsgPackCode.CodeStr8;
                    buf[1] = (byte)strlen;
                    databuffer.Write(buf, 0, 2);
                }
                else if (strlen <= Max_str16_len)
                {
                    WriteUint16((ushort)strlen, MsgPackCode.CodeStr16);
                }
                else
                {
                    if (strlen > Max_str32_len)
                    {
                        strlen = Max_str32_len;
                    }
                    WriteUint32((uint)strlen, MsgPackCode.CodeStr32);
                }
                databuffer.Write(b, 0, strlen);                                    
            }

            public void EncodeFloat(float v)
            {
                byte[] bv = System.BitConverter.GetBytes(v);
                uint vint = System.BitConverter.ToUInt32(bv, 0);
                WriteUint32(vint,MsgPackCode.CodeFloat);
            }

            public void EncodeDouble(double v)
            {
                byte[] bv = System.BitConverter.GetBytes(v);           
                ulong vint = System.BitConverter.ToUInt64(bv, 0);
                WriteUint64(vint, MsgPackCode.CodeDouble);
            }

            public void EncodeBinary(byte[] bv)
            {
                int btlen = bv.Length;
                if(btlen <= Max_str8_len)
                {
                    buf[0] = 0xc4;
                    buf[1] = (byte)btlen;
                    databuffer.Write(buf, 0, 2);
                }else if(btlen <= Max_str16_len)
                {
                    WriteUint16((ushort)btlen, MsgPackCode.CodeBin16);
                }
                else
                {
                    if(btlen > Max_str32_len)
                    {
                        btlen = Max_str32_len;
                    }
                    WriteUint32((uint)btlen, MsgPackCode.CodeBin32);
                }
                if(btlen > 0)
                {
                    databuffer.Write(bv, 0, btlen);
                }
            }

            public void EncodeExtValue(DxExtValue value)
            {
                byte[] bt = value.ExtData;
                int btlen = bt.Length;
                buf[1] = value.ExtType;
                switch (btlen)
                {
                    case 1:
                        buf[0] = (byte)MsgPackCode.CodeFixExt1;
                        databuffer.Write(buf, 0, 2);
                        break;
                    case 2:
                        buf[0] = (byte)MsgPackCode.CodeFixExt2;
                        databuffer.Write(buf, 0, 2);
                        break;
                    case 4:
                        buf[0] = (byte)MsgPackCode.CodeFixExt4;
                        databuffer.Write(buf, 0, 2);
                        break;
                    case 8:
                        buf[0] = (byte)MsgPackCode.CodeFixExt8;
                        databuffer.Write(buf, 0, 2);
                        break;
                    default:
                        if (btlen <= 16)
                        {
                            buf[0] = (byte)MsgPackCode.CodeFixExt16;
                            databuffer.Write(buf, 0, 2);
                        }else if (btlen <= DxMsgPack.Max_str8_len)
                        {
                            buf[0] = (byte)MsgPackCode.CodeExt8;
                            buf[1] = (byte)btlen;
                            buf[2] = value.ExtType;
                            databuffer.Write(buf, 0, 3);
                        }else if(btlen <= DxMsgPack.Max_str16_len)
                        {
                            databuffer.WriteByte((byte)MsgPackCode.CodeExt16);
                            BigEndian.PutUint16(databuffer, (ushort)btlen);
                            databuffer.WriteByte(value.ExtType);
                        }else
                        {
                            if(btlen > Max_str32_len) { btlen = Max_str32_len; }
                            databuffer.WriteByte((byte)MsgPackCode.CodeExt32);
                            BigEndian.PutUint32(databuffer, (uint)btlen);
                            databuffer.WriteByte(value.ExtType);
                        }
                        break;
                }
                if(btlen > 0)
                {
                    databuffer.Write(bt, 0, btlen);
                }
            }


            public void EncodeRecord(DxRecordValue value)
            {
                int maplen = value.Count;
                if(maplen <= Max_fixmap_len) //fixmap
                {
                    databuffer.WriteByte((byte)(0x80 | (byte)maplen));
                }else if (maplen <= Max_map16_len)
                {
                    WriteUint16((ushort)maplen, MsgPackCode.CodeMap16);
                }else
                {
                    if (maplen > Max_map32_len)
                    {
                        maplen = Max_map32_len;
                    }
                    WriteUint32((uint)maplen, MsgPackCode.CodeMap32);
                }
                //写入KV
                foreach(KeyValuePair<string, DxBaseValue> kvalue in value)
                {
                    EncodeString(kvalue.Key);
                    if(kvalue.Value == null)
                    {
                        databuffer.WriteByte((byte)MsgPackCode.CodeNil);
                    }
                    else{ Encode(kvalue.Value); }
                }
            }

            public void EncodeArray(DxArrayValue value)
            {
                int arrlen = value.Count;
                if (arrlen < 16) //1001XXXX|    N objects
                {
                    databuffer.WriteByte((byte)((byte)MsgPackCode.CodeFixedArrayLow | (byte)arrlen));
                }
                else if (arrlen <= Max_map16_len)
                {
                    WriteUint16((ushort)arrlen, MsgPackCode.CodeArray16);
                }
                else
                {
                    if (arrlen > Max_map32_len)
                    {
                        arrlen = Max_map32_len;
                    }
                    WriteUint32((uint)arrlen, MsgPackCode.CodeArray32);
                }
                for (int i = 0; i < arrlen; i++)
                {
                    DxBaseValue vbase = value[i];
                    if(vbase == null)
                    {
                        databuffer.WriteByte((byte)MsgPackCode.CodeNil);
                    }
                    else { Encode(vbase); }
                }
            }

            public byte[] Bytes
            {
                get
                {
                    return databuffer.GetBuffer();
                }
            }

            public void Encode(DxBaseValue value)
            {                
                switch (value.Type)
                {
                    case ValueType.VT_Map:
                        EncodeRecord((DxRecordValue)value);
                        break;
                    case ValueType.VT_Array:
                        EncodeArray((DxArrayValue)value);
                        break;                    
                    case ValueType.VT_Binary:
                        byte[] bt = ((DxBinaryValue)value).Bytes;
                        if(bt == null || bt.Length == 0)
                        {
                            databuffer.WriteByte((byte)MsgPackCode.CodeNil);
                        }
                        else
                        {
                            EncodeBinary(bt);
                        }
                        break;
                    case ValueType.VT_Ext:
                        EncodeExtValue((DxExtValue)value);
                        break;
                    case ValueType.VT_Boolean:
                        EncodeBool(value.AsBoolean);
                        break;
                    case ValueType.VT_Int:
                    case ValueType.VT_Int64:
                        EncodeInt(value.AsInt64);
                        break;
                    case ValueType.VT_DateTime:
                        buf[0] = (byte)MsgPackCode.CodeFixExt4;
                        buf[1] = 0xff;
                        databuffer.Write(buf, 0, 2);                        
                        BigEndian.PutUint32(databuffer, (uint)((DxDateTimeValue)value).Unix);
                        break;
                    case ValueType.VT_String:
                        EncodeString(value.AsString);
                        break;
                    case ValueType.VT_Float:
                        EncodeFloat(value.AsFloat);
                        break;
                    case ValueType.VT_Double:
                        EncodeDouble(value.AsDouble);
                        break;
                    default:
                        databuffer.WriteByte((byte)MsgPackCode.CodeNil);
                        break;
                }
            }

        }
    }
}

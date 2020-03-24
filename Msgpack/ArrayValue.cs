namespace DxValue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    public class DxArrayValue : DxBaseValue
    {
        protected List<DxBaseValue> arrayList;
        public DxArrayValue()
        {            
            arrayList = new List<DxBaseValue>(32);
        }

        public DxArrayValue(byte[] msgPackdata)
        {
            arrayList = new List<DxBaseValue>(32);
            if(msgPackdata != null)
            {
                AsMsgPackBytes = msgPackdata;
            }
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Array;
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            bool isfirst = true;
            builder.Append('[');
            for(int i=0;i<arrayList.Count;i++)
            {
                DxBaseValue value = arrayList[i];
                if (isfirst)
                {
                    isfirst = false;
                }
                else
                {
                    builder.Append(',');
                }    
                if(value == null)
                {
                    builder.Append("null");
                    continue;
                }
                switch (value.Type)
                {
                    case ValueType.VT_Map:
                    case ValueType.VT_Array:
                        builder.Append(value.ToString());
                        break;
                    case ValueType.VT_Binary:
                        break;
                    case ValueType.VT_String:
                    case ValueType.VT_DateTime:
                        builder.Append('"');
                        builder.Append(value.AsString);
                        builder.Append('"');
                        break;
                    case ValueType.VT_Double:
                    case ValueType.VT_Int:
                    case ValueType.VT_Int64:
                    case ValueType.VT_Boolean:
                        builder.Append(value.AsString);
                        break;
                }
            }
            builder.Append(']');
            return builder.ToString();
        }

        public override string AsString
        {
            get
            {
                return ToString();
            }
        }

        public void Clear()
        {
            arrayList.Clear();
        }

        public byte[] AsMsgPackBytes
        {
            get
            {
                DxMsgPack.DxMsgPack.Encoder encoder = new DxMsgPack.DxMsgPack.Encoder();
                encoder.Encode(this);
                return encoder.Bytes;
            }
            set
            {
                DxMsgPack.DxMsgPack.Decoder decoder = new DxMsgPack.DxMsgPack.Decoder(value);
                decoder.Decode2Array(DxMsgPack.DxMsgPack.MsgPackCode.CodeUnkonw, this);
            }
        }

        private int ifNilInitArr2idx(int idx)
        {
            if (idx < 0)
            {
                idx = arrayList.Count;
            }            
            while(idx > arrayList.Count - 1)
            {
                arrayList.Add(null);                
            }
            return idx;
        }

        public DxRecordValue NewRecord(int idx)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null)
            {                
                switch (v.Type)
                {
                    case ValueType.VT_Map:
                        return (DxRecordValue)v;
                    case ValueType.VT_Array:
                        ((DxArrayValue)v).Clear();
                        break;
                }
            }
            DxRecordValue result = new DxRecordValue();
            result.Parent = this;
            arrayList[idx] = result;
            return result;
        }

        public DxArrayValue NewArray(int idx)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null)
            {
                switch (v.Type)
                {
                    case ValueType.VT_Map:
                        ((DxRecordValue)v).Clear();
                        break;
                    case ValueType.VT_Array:
                        return ((DxArrayValue)v);                        
                }
            }
            DxArrayValue result = new DxArrayValue();
            result.parent = this;
            arrayList[idx] = result;
            return result;
        }

        public int Count
        {
            get
            {
                return arrayList.Count;
            }
        }

        public ValueType VaueTypeByIndex(int idx)
        {
            if(idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].Type;
            }
            return ValueType.VT_Null;
        }

        public void SetNull(int idx)
        {
            this[idx] = null;
        }

        public DxBaseValue this[int idx]
        {
            get
            {
                if (idx >= 0 && idx < arrayList.Count)
                {
                    return arrayList[idx];
                }
                return null;
            }
            set
            {                
                idx = ifNilInitArr2idx(idx);
                DxBaseValue v = arrayList[idx];
                if(v!=null && v.Type == value.Type)
                {
                    switch (value.Type)
                    {
                        case ValueType.VT_Array:
                            break;
                        case ValueType.VT_Map:
                            break;
                        case ValueType.VT_DateTime:
                            ((DxDateTimeValue)v).AsDateTime = ((DxDateTimeValue)value).AsDateTime;
                            return;
                        case ValueType.VT_Double:
                            v.AsDouble = value.AsDouble;
                            return;
                        case ValueType.VT_Float:
                            v.AsFloat = value.AsFloat;
                            return;
                        case ValueType.VT_Int:
                            v.AsInt = value.AsInt;
                            return;
                        case ValueType.VT_Int64:
                            v.AsInt64 = value.AsInt64;
                            return;
                        case ValueType.VT_String:
                            v.AsString = value.AsString;
                            return;
                    }                    
                }
                arrayList[idx] = value;
            }
        }

        public string GetString(int idx, string defaultvalue = "")
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].AsString;
            }
            return defaultvalue;
        }

        public void SetString(int idx, string value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if(v != null && v.Type!=ValueType.VT_Array && v.Type != ValueType.VT_Map)
            {
                arrayList[idx].AsString = value;
                return;
            }
            arrayList[idx] = new DxStringValue(value);
        }

        public int GetInt(int idx, int defaultvalue = 0)
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].AsInt;
            }
            return defaultvalue;
        }

        public void SetInt(int idx, int value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null && v.Type != ValueType.VT_Array && v.Type != ValueType.VT_Map)
            {
                arrayList[idx].AsInt = value;
                return;
            }
            arrayList[idx] = new DxIntValue(value);
        }

        public long GetInt64(int idx, long defaultvalue = 0)
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].AsInt64;
            }
            return defaultvalue;
        }

        public void SetInt64(int idx, long value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null && v.Type != ValueType.VT_Array && v.Type != ValueType.VT_Map)
            {
                arrayList[idx].AsInt64 = value;
                return;
            }
            arrayList[idx] = new DxInt64Value(value);
        }

        public bool GetBool(int idx, bool defaultvalue = false)
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].AsBoolean;
            }
            return defaultvalue;
        }

        public void SetBool(int idx, bool value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null && v.Type != ValueType.VT_Array && v.Type != ValueType.VT_Map)
            {
                arrayList[idx].AsBoolean = value;
                return;
            }
            arrayList[idx] = new DxBoolValue(value);
        }

        public float GetFloat(int idx, float defaultvalue = 0)
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].AsFloat;
            }
            return defaultvalue;
        }

        public void SetFloat(int idx, float value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null && v.Type != ValueType.VT_Array && v.Type != ValueType.VT_Map)
            {
                arrayList[idx].AsFloat = value;
                return;
            }
            arrayList[idx] = new DxFloatValue(value);
        }

        public double GetDouble(int idx, double defaultvalue = 0)
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                return arrayList[idx].AsDouble;
            }
            return defaultvalue;
        }

        public void SetDouble(int idx, double value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null && v.Type != ValueType.VT_Array && v.Type != ValueType.VT_Map)
            {
                arrayList[idx].AsDouble = value;
                return;
            }
            arrayList[idx] = new DxDoubleValue(value);
        }

        public DateTime GetDateTime(int idx, DateTime defaultvalue)
        {
            if (idx >= 0 && idx < arrayList.Count && arrayList[idx] != null)
            {
                DxBaseValue result = arrayList[idx];
                if (result.Type == ValueType.VT_DateTime)
                {
                    return ((DxDateTimeValue)result).AsDateTime;
                }
                else
                {
                    string value = result.AsString;
                    DateTime dtv;
                    if (DateTime.TryParse(value, out dtv) || DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtv))
                    {
                        return dtv;
                    }
                }
            }
            return defaultvalue;
        }

        public void SetDateTime(int idx, DateTime value)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if (v != null && v.Type == ValueType.VT_DateTime)
            {
                ((DxDateTimeValue)arrayList[idx]).AsDateTime = value;                
                return;
            }
            arrayList[idx] = new DxDateTimeValue(value);
        }

        public DxRecordValue GetRecord(int idx)
        {
            DxBaseValue result = this[idx];
            if(result != null && result.Type == ValueType.VT_Map)
            {
                return (DxRecordValue)result;
            }
            return null;
        }

        public void SetBinary(int idx, byte[] bytevalue)
        {
            idx = ifNilInitArr2idx(idx);
            DxBaseValue v = arrayList[idx];
            if(v!= null && v.Type == ValueType.VT_Binary)
            {
                ((DxBinaryValue)v).Bytes = bytevalue;
            }
            else
            {
                DxBinaryValue bv = new DxBinaryValue();
                bv.Bytes = bytevalue;
                arrayList[idx] = bv;
            }
        }
        public DxArrayValue GetArray(int idx)
        {
            DxBaseValue result = this[idx];
            if (result != null && result.Type == ValueType.VT_Array)
            {
                return (DxArrayValue)result;
            }
            return null;
        }
    }
}
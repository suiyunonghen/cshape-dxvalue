namespace DxValue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;    
    using System.IO;

    public class DxRecordValue : DxBaseValue, IEnumerable
    {
        Dictionary<string, DxBaseValue> records;
        public DxRecordValue()
        {            
            records = new Dictionary<string, DxBaseValue>(32);            
        }

        public DxRecordValue(byte[] msgPackData)
        {
            records = new Dictionary<string, DxBaseValue>(32);
            if (msgPackData != null)
            {
                AsMsgPackBytes = msgPackData;
            }
        }
        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            bool isfirst = true;
            builder.Append('{');
            foreach(KeyValuePair<string, DxBaseValue> kvalue in records)
            {
                if (isfirst)
                {
                    builder.Append('"');
                    isfirst = false;
                }
                else
                {
                    builder.Append(",\"");
                } 
                builder.Append(kvalue.Key);
                builder.Append("\":");
                switch (kvalue.Value.Type)
                {
                    case ValueType.VT_Map:
                    case ValueType.VT_Array:
                        builder.Append(kvalue.Value.ToString());
                        break;
                    case ValueType.VT_Binary:
                        break;
                    case ValueType.VT_String:
                    case ValueType.VT_DateTime:
                        builder.Append('"');
                        builder.Append(kvalue.Value.AsString);
                        builder.Append('"');
                        break;
                    case ValueType.VT_Double:
                    case ValueType.VT_Int:
                    case ValueType.VT_Int64:                                            
                    case ValueType.VT_Boolean:
                        builder.Append(kvalue.Value.AsString);
                        break;                        
                }
            }
            builder.Append('}');
            return builder.ToString();
        }

        public override string AsString {
            get
            {
                return ToString();
            }
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Map;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return records.GetEnumerator();
        }

        public void Clear()
        {
            records.Clear();
        }

        public DxRecordValue NewRecord(string keyName)
        {
            if (keyName == "")
            {
                return null;
            }
            DxBaseValue value = null;
            bool haskey = records.TryGetValue(keyName, out value);
            if (haskey)
            {
                if (value.Type == ValueType.VT_Map)
                {
                    ((DxRecordValue)value).Clear();
                    return (DxRecordValue)value;
                }
                else if (value.Type == ValueType.VT_Array)
                {
                    ((DxArrayValue)value).Clear();
                }
            }
            DxRecordValue result = new DxRecordValue();
            result.parent = this;
            if (haskey)
            {
                records[keyName] = result;
            }
            else
            {
                records.Add(keyName, result);
            }
            return result;
        }

        public DxArrayValue NewArray(string keyName)
        {
            if (keyName == "")
            {
                return null;
            }
            DxBaseValue value = null;
            bool haskey = records.TryGetValue(keyName, out value);
            if (haskey)
            {
                if (value.Type == ValueType.VT_Array)
                {
                    ((DxArrayValue)value).Clear();
                    return (DxArrayValue)value;
                }
                else if (value.Type == ValueType.VT_Map)
                {
                    ((DxRecordValue)value).Clear();
                }
            }
            DxArrayValue result = new DxArrayValue();
            result.Parent = this;
            if (haskey)
            {
                records[keyName] = result;
            }
            else
            {
                records.Add(keyName, result);
            }
            return result;
        }

        public DxBaseValue this[string keyName]
        {
            get
            {
                DxBaseValue result;
                if (records.TryGetValue(keyName, out result))
                {
                    return result;
                }
                return null;
            }
            set
            {
                DxBaseValue v;
                if (records.TryGetValue(keyName, out v))
                {
                    if (v != null && value.Type == v.Type)
                    {
                        switch (v.Type)
                        {
                            case ValueType.VT_Array:
                                break;
                            case ValueType.VT_Boolean:
                                v.AsBoolean = value.AsBoolean;
                                return;
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
                            case ValueType.VT_Map:
                                break;

                        }
                        records[keyName] = value;
                        return;
                    }
                    switch (value.Type)
                    {
                        case ValueType.VT_Array:
                            ((DxArrayValue)v).Clear();
                            break;
                        case ValueType.VT_Map:
                            ((DxRecordValue)v).Clear();
                            break;
                    }
                    records[keyName] = value;
                    return;
                }
                records.Add(keyName, value);
            }

        }

        public string GetString(string keyName,string defaultvalue="")
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                return result.AsString;
            }
            return defaultvalue;
        }

        public void SetString(string keyName, string value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result) && result!=null)
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    default:
                        result.AsString = value;
                        return;
                }
                records[keyName] = new DxStringValue(value);
                return;
            }
            records.Add(keyName, new DxStringValue(value));
        }

        public int GetInt(string keyName,int defaultValue=0)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                return result.AsInt;
            }
            return defaultValue;
        }

        public void SetInt(string keyName, int value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    default:
                        result.AsInt = value;
                        return;
                }
                records[keyName] = new DxIntValue(value);
                return;
            }
            records.Add(keyName, new DxIntValue(value));
        }

        public long GetInt64(string keyName,long DefaultValue=0)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                return result.AsInt64;
            }
            return DefaultValue;
        }

        public void SetInt64(string keyName, long value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    default:
                        result.AsInt64 = value;
                        return;
                }
                records[keyName] = new DxInt64Value(value);
                return;
            }
            records.Add(keyName, new DxInt64Value(value));
        }

        public bool GetBool(string keyName,bool defaultValue=false)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                return result.AsBoolean;
            }
            return defaultValue;
        }

        public DxRecordValue ForcePathRecord(string path)
        {
            string[] pathNames = path.Split('.');
            if (pathNames.Length == 0)
            {
                return null;
            }
            DxBaseValue vbase = this[pathNames[0]];
            if (vbase == null || vbase.Type != ValueType.VT_Map)
            {
                vbase = NewRecord(pathNames[0]);
            }
            for (int i = 1; i < pathNames.Length; i++)
            {
                DxRecordValue oldbase = (DxRecordValue)vbase;
                if(vbase.Type == ValueType.VT_Map)
                {
                    vbase = ((DxRecordValue)vbase)[pathNames[i]];
                    if (vbase == null || vbase.Type != ValueType.VT_Map)
                    {
                        vbase = oldbase.NewRecord(pathNames[i]);
                    }
                }
                else
                {
                    vbase = vbase.Parent;
                    vbase = ((DxRecordValue)vbase).NewRecord(pathNames[i - 1]).NewRecord(pathNames[i]);
                }
            }
            return (DxRecordValue)vbase;
        }

        public DxArrayValue ForcePathArray(string path)
        {
            string[] pathNames = path.Split('.');
            if (pathNames.Length == 0)
            {
                return null;
            }
            DxBaseValue vbase = this[pathNames[0]];
            if (vbase == null || vbase.Type != ValueType.VT_Map)
            {
                vbase = NewRecord(pathNames[0]);
            }
            for (int i = 1; i < pathNames.Length - 1; i++)
            {
                DxRecordValue oldbase = (DxRecordValue)vbase;
                if (vbase.Type == ValueType.VT_Map)
                {
                    vbase = ((DxRecordValue)vbase)[pathNames[i]];
                    if (vbase == null || vbase.Type != ValueType.VT_Map)
                    {
                        vbase = oldbase.NewRecord(pathNames[i]);
                    }
                }
                else
                {
                    vbase = vbase.Parent;
                    vbase = ((DxRecordValue)vbase).NewRecord(pathNames[i - 1]).NewRecord(pathNames[i]);
                }
            }
            return ((DxRecordValue)vbase).NewArray(pathNames[pathNames.Length - 1]);
        }


        public DxBaseValue ValueByPath(string path)
        {
            string[] pathNames = path.Split('.');
            if (pathNames.Length == 0)
            {
                return null;
            }
            DxBaseValue vbase = this[pathNames[0]];
            if (vbase == null || vbase.Type != ValueType.VT_Map) { return null; }
            for (int i = 1; i < pathNames.Length-1; i++)
            {                
                if (vbase.Type == ValueType.VT_Map)
                {
                    vbase = ((DxRecordValue)vbase)[pathNames[i]];
                    if (vbase == null || vbase.Type != ValueType.VT_Map)
                    {
                        return null;
                    }
                }
                else { return null; }                
            }
            return ((DxRecordValue)vbase)[pathNames[pathNames.Length - 1]];
        }

        public string StringByPath(string path,string DefaultValue="")
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null) return v.AsString;
            return DefaultValue;
        }

        public float FloatByPath(string path, float DefaultValue = 0)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null) return v.AsFloat;
            return DefaultValue;
        }

        public double DoubleByPath(string path, double DefaultValue = 0)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null) return v.AsDouble;
            return DefaultValue;
        }

        public bool BoolByPath(string path, bool DefaultValue = false)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null) return v.AsBoolean;
            return DefaultValue;
        }

        public int Count
        {
            get
            {
                return records.Count;
            }
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
                decoder.Decode2Map(DxMsgPack.DxMsgPack.MsgPackCode.CodeUnkonw, this);
            }
        }

        public int IntByPath(string path, int DefaultValue = 0)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null) return v.AsInt;
            return DefaultValue;
        }

        public long Int64ByPath(string path, long DefaultValue = 0)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null) return v.AsInt64;
            return DefaultValue;
        }

        public DxRecordValue RecordByPath(string path)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null && v.Type == ValueType.VT_Map) return (DxRecordValue)v;
            return null;
        }

        public DxArrayValue ArrayByPath(string path)
        {
            DxBaseValue v = ValueByPath(path);
            if (v != null && v.Type == ValueType.VT_Array) return (DxArrayValue)v;
            return null;
        }

        public void SetBool(string keyName, bool value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    default:
                        result.AsBoolean = value;
                        return;
                }
                records[keyName] = new DxBoolValue(value);
                return;
            }
            records.Add(keyName, new DxBoolValue(value));
        }

        public float GetFloat(string keyName,float defaultvalue=0)
        {            
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                return result.AsFloat;
            }
            return defaultvalue;
        }

        public void SetFloat(string keyName, float value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    default:
                        result.AsFloat = value;
                        return;
                }
                records[keyName] = new DxFloatValue(value);
                return;
            }
            records.Add(keyName, new DxFloatValue(value));
        }

        public double GetDouble(string keyName, double defaultvalue = 0)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                return result.AsDouble;
            }
            return defaultvalue;
        }

        public void SetDouble(string keyName, double value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    default:
                        result.AsDouble = value;
                        return;
                }
                records[keyName] = new DxDoubleValue(value);
                return;
            }
            records.Add(keyName, new DxDoubleValue(value));
        }

        public DateTime GetDateTime(string keyName,DateTime defaultValue)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
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
            return defaultValue;
        }

        public void SetDateTime(string keyName, DateTime value)
        {
            DxBaseValue result;
            if (records.TryGetValue(keyName, out result))
            {
                switch (result.Type)
                {
                    case ValueType.VT_Array:
                        ((DxArrayValue)result).Clear();
                        break;
                    case ValueType.VT_Map:
                        ((DxRecordValue)result).Clear();
                        break;
                    case ValueType.VT_DateTime:
                        ((DxDateTimeValue)result).AsDateTime = value;
                        break;
                }
                records[keyName] = new DxDateTimeValue(value);
                return;
            }
            records.Add(keyName, new DxDateTimeValue(value));
        }

        public DxRecordValue GetRecord(string keyName)
        {
            DxBaseValue result = this[keyName];
            if (result != null && result.Type == ValueType.VT_Map)
            {
                return (DxRecordValue)result;
            }
            return null;
        }

        public DxArrayValue GetArray(string keyName)
        {
            DxBaseValue result = this[keyName];
            if (result != null && result.Type == ValueType.VT_Array)
            {
                return (DxArrayValue)result;
            }
            return null;
        }
        

    }    

}
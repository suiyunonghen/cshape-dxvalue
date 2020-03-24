namespace DxValue{
    using System;    
    using System.IO;

    public static class BigEndian
    {
        public static ushort Uint16(byte[] bytearr)
        {
            switch (bytearr.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return bytearr[0];
                default:
                    return (ushort)(bytearr[1] | (ushort)(bytearr[0] << 8));
            }
        }

        public static ushort Uint16(Stream stream)
        {
            switch (stream.Length - stream.Position)
            {
                case 0:
                    return 0;
                case 1:
                    return (ushort)stream.ReadByte();
                default:
                    byte[] bytearr = { 0, 0 };
                    stream.Read(bytearr, 0, 2);
                    return (ushort)(bytearr[1] | (ushort)(bytearr[0] << 8));
            }
        }

        public static void PutUint16(Stream stream, ushort value)
        {
            byte[] vb = { (byte)(value >> 8), (byte)value };
            stream.Write(vb, 0, 2);

        }

        public static void PutUint16(byte[] byteArray, ushort value)
        {
            if (byteArray.Length < 2)
            {
                Array.Resize(ref byteArray, 2);
            }
            byteArray.SetValue((byte)(value >> 8), 0);
            byteArray.SetValue((byte)value, 1);
        }

        public static uint Uint32(byte[] bytearr)
        {
            switch (bytearr.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return bytearr[0];
                case 2:
                    return (uint)(bytearr[1] | (ushort)bytearr[0] << 8);
                case 3:
                    return (uint)bytearr[2] << 8 | (uint)bytearr[1] << 16 | (uint)bytearr[0] << 24;
                default:
                    return (uint)bytearr[3] | (uint)bytearr[2] << 8 | (uint)bytearr[1] << 16 | (uint)bytearr[0] << 24;
            }
        }

        public static uint Uint32(Stream stream)
        {
            int haslen = (int)(stream.Length - stream.Position);
            if (haslen == 0)
            {
                return 0;
            }
            if (haslen > 4)
            {
                haslen = 4;
            }
            byte[] bytearr = { 0, 0, 0, 0 };
            stream.Read(bytearr, 0, haslen);
            return (uint)bytearr[3] | (uint)bytearr[2] << 8 | (uint)bytearr[1] << 16 | (uint)bytearr[0] << 24;
        }

        public static void PutUint32(Stream stream, uint value)
        {
            byte[] vb = { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
            stream.Write(vb, 0, 4);

        }

        public static void PutUint32(byte[] byteArray,int startindex, uint value)
        {
            if (byteArray.Length < 4)
            {
                Array.Resize(ref byteArray, 4);
            }
            byteArray.SetValue((byte)(value >> 24), startindex);
            byteArray.SetValue((byte)(value >> 16), startindex+1);
            byteArray.SetValue((byte)(value >> 8), startindex + 2);
            byteArray.SetValue((byte)value, startindex + 3);
        }


        public static ulong Uint64(byte[] bytearr)
        {
            switch (bytearr.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return bytearr[0];
                case 2:
                    return (ulong)(bytearr[1] | (ulong)bytearr[0] << 8);
                case 3:
                    return (ulong)bytearr[2] << 8 | (ulong)bytearr[1] << 16 | (ulong)bytearr[0] << 24;
                case 4:
                    return (ulong)bytearr[3] | (ulong)bytearr[2] << 8 | (ulong)bytearr[1] << 16 | (ulong)bytearr[0] << 24;
                case 5:
                    return (ulong)bytearr[4] | (ulong)bytearr[3] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[1] << 24 | (ulong)bytearr[3] << 32;
                case 6:
                    return (ulong)bytearr[5] | (ulong)bytearr[4] << 8 | (ulong)bytearr[3] << 16 | (ulong)bytearr[2] << 24 | (ulong)bytearr[1] << 32 |
                        (ulong)bytearr[0] << 40;
                case 7:
                    return (ulong)bytearr[6] | (ulong)bytearr[5] << 8 | (ulong)bytearr[4] << 16 | (ulong)bytearr[3] << 24 | (ulong)bytearr[2] << 32 |
                        (ulong)bytearr[1] << 40 | (ulong)bytearr[0] << 48;
                default:
                    return (ulong)bytearr[7] | (ulong)bytearr[6] << 8 | (ulong)bytearr[5] << 16 | (ulong)bytearr[4] << 24 |
                        (ulong)bytearr[3] << 32 | (ulong)bytearr[2] << 40 | (ulong)bytearr[1] << 48 | (ulong)bytearr[0] << 56;
            }
        }

        public static ulong Uint64(Stream stream)
        {
            int haslen = (int)(stream.Length - stream.Position);
            if (haslen == 0)
            {
                return 0;
            }
            if (haslen > 8)
            {
                haslen = 8;
            }
            byte[] bytearr = { 0, 0, 0, 0, 0, 0, 0, 0 };
            stream.Read(bytearr, 0, haslen);
            return (ulong)bytearr[7] | (ulong)bytearr[6] << 8 | (ulong)bytearr[5] << 16 | (ulong)bytearr[4] << 24 |
                        (ulong)bytearr[3] << 32 | (ulong)bytearr[2] << 40 | (ulong)bytearr[1] << 48 | (ulong)bytearr[0] << 56;
        }

        public static void PutUint64(Stream stream, ulong value)
        {
            byte[] vb = { (byte)(value >> 56), (byte)(value >> 48), (byte)(value >> 40), (byte)(value >> 32), (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
            stream.Write(vb, 0, 8);

        }

        public static void PutUint64(byte[] byteArray, int startindex, ulong value)
        {
            if (byteArray.Length < 8)
            {
                Array.Resize(ref byteArray, 8);
            }
            byteArray.SetValue((byte)(value >> 56), startindex);
            byteArray.SetValue((byte)(value >> 48), startindex+1);
            byteArray.SetValue((byte)(value >> 40), startindex + 2);
            byteArray.SetValue((byte)(value >> 32), startindex + 3);
            byteArray.SetValue((byte)(value >> 24), startindex + 4);
            byteArray.SetValue((byte)(value >> 16), startindex + 5);
            byteArray.SetValue((byte)(value >> 8), startindex + 6);
            byteArray.SetValue((byte)value, startindex + 7);
        }
    }

    public static class LittleEndian
    {
        public static ushort Uint16(byte[] bytearr)
        {
            switch (bytearr.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return bytearr[0];
                default:
                    return (ushort)(bytearr[0] | (ushort)(bytearr[1] << 8));
            }
        }

        public static ushort Uint16(Stream stream)
        {
            switch (stream.Length - stream.Position)
            {
                case 0:
                    return 0;
                case 1:
                    return (ushort)stream.ReadByte();
                default:
                    byte[] bytearr = { 0, 0 };
                    stream.Read(bytearr, 0, 2);
                    return (ushort)(bytearr[0] | (ushort)(bytearr[1] << 8));
            }
        }

        public static void PutUint16(Stream stream, ushort value)
        {
            byte[] vb = { (byte)value, (byte)(value >> 8) };
            stream.Write(vb, 0, 2);

        }

        public static void PutUint16(byte[] byteArray, ushort value)
        {
            if (byteArray.Length < 2)
            {
                Array.Resize(ref byteArray, 2);
            }
            byteArray.SetValue((byte)(value >> 8), 1);
            byteArray.SetValue((byte)value, 0);
        }

        public static uint Uint32(byte[] bytearr)
        {
            switch (bytearr.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return bytearr[0];
                case 2:
                    return (uint)(bytearr[0] | (ushort)bytearr[1] << 8);
                case 3:
                    return (uint)bytearr[0] << 8 | (uint)bytearr[1] << 16 | (uint)bytearr[2] << 24;
                default:
                    return (uint)bytearr[0] | (uint)bytearr[1] << 8 | (uint)bytearr[2] << 16 | (uint)bytearr[3] << 24;
            }
        }

        public static uint Uint32(Stream stream)
        {
            int haslen = (int)(stream.Length - stream.Position);
            if (haslen == 0)
            {
                return 0;
            }
            if (haslen > 4)
            {
                haslen = 4;
            }
            byte[] bytearr = { 0, 0, 0, 0 };
            stream.Read(bytearr, 0, haslen);
            return (uint)bytearr[0] | (uint)bytearr[1] << 8 | (uint)bytearr[2] << 16 | (uint)bytearr[3] << 24;
        }

        public static void PutUint32(Stream stream, uint value)
        {
            byte[] vb = { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };
            stream.Write(vb, 0, 4);

        }

        public static void PutUint32(byte[] byteArray, uint value)
        {
            if (byteArray.Length < 4)
            {
                Array.Resize(ref byteArray, 4);
            }
            byteArray.SetValue((byte)(value >> 24), 3);
            byteArray.SetValue((byte)(value >> 16), 2);
            byteArray.SetValue((byte)(value >> 8), 1);
            byteArray.SetValue((byte)value, 0);
        }


        public static ulong Uint64(byte[] bytearr)
        {
            switch (bytearr.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return bytearr[0];
                case 2:
                    return (ulong)(bytearr[0] | (ulong)bytearr[1] << 8);
                case 3:
                    return (ulong)bytearr[0] << 8 | (ulong)bytearr[1] << 16 | (ulong)bytearr[2] << 24;
                case 4:
                    return (ulong)bytearr[0] | (ulong)bytearr[1] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[3] << 24;
                case 5:
                    return (ulong)bytearr[0] | (ulong)bytearr[1] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[3] << 24 | (ulong)bytearr[4] << 32;
                case 6:
                    return (ulong)bytearr[0] | (ulong)bytearr[1] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[3] << 24 | (ulong)bytearr[4] << 32 |
                        (ulong)bytearr[5] << 40;
                case 7:
                    return (ulong)bytearr[0] | (ulong)bytearr[1] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[3] << 24 | (ulong)bytearr[4] << 32 |
                        (ulong)bytearr[5] << 40 | (ulong)bytearr[6] << 48;
                default:
                    return (ulong)bytearr[0] | (ulong)bytearr[1] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[3] << 24 |
                        (ulong)bytearr[4] << 32 | (ulong)bytearr[5] << 40 | (ulong)bytearr[6] << 48 | (ulong)bytearr[7] << 56;
            }
        }

        public static ulong Uint64(Stream stream)
        {
            int haslen = (int)(stream.Length - stream.Position);
            if (haslen == 0)
            {
                return 0;
            }
            if (haslen > 8)
            {
                haslen = 8;
            }
            byte[] bytearr = { 0, 0, 0, 0, 0, 0, 0, 0 };
            stream.Read(bytearr, 0, haslen);
            return (ulong)bytearr[0] | (ulong)bytearr[1] << 8 | (ulong)bytearr[2] << 16 | (ulong)bytearr[3] << 24 |
                        (ulong)bytearr[4] << 32 | (ulong)bytearr[5] << 40 | (ulong)bytearr[6] << 48 | (ulong)bytearr[7] << 56;
        }

        public static void PutUint64(Stream stream, ulong value)
        {
            byte[] vb = { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24), (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56) };
            stream.Write(vb, 0, 8);

        }

        public static void PutUint64(byte[] byteArray, ulong value)
        {
            if (byteArray.Length < 8)
            {
                Array.Resize(ref byteArray, 8);
            }
            byteArray.SetValue((byte)(value >> 56), 7);
            byteArray.SetValue((byte)(value >> 48), 6);
            byteArray.SetValue((byte)(value >> 40), 5);
            byteArray.SetValue((byte)(value >> 32), 4);
            byteArray.SetValue((byte)(value >> 24), 3);
            byteArray.SetValue((byte)(value >> 16), 2);
            byteArray.SetValue((byte)(value >> 8), 1);
            byteArray.SetValue((byte)value, 0);
        }
    }
    public enum ValueType
    {
        VT_Null,
        VT_String,
        VT_Int,
        VT_Int64,
        VT_Boolean,
        VT_Float,
        VT_Double,
        VT_DateTime,
        VT_Binary,
        VT_Ext,
        VT_Array,
        VT_Map
    }


    public class DxBaseValue
    {
        protected DxBaseValue parent;        
        public DxBaseValue Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        public override string ToString()
        {
            return AsString;
        }

        public virtual ValueType Type
        {
            get
            {
                return ValueType.VT_Null;
            }
        }

        public virtual int AsInt
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public virtual long AsInt64
        {
            get
            {
                return 0;
            }
            set { }
        }

        public virtual string AsString
        {
            get
            {
                return "";
            }
            set
            {

            }
        }

        public virtual bool AsBoolean
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public virtual byte[] AsBinary
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public virtual float AsFloat
        {
            get
            {
                return 0;
            }
            set { }
        }

        public virtual double AsDouble
        {
            get
            {
                return 0;
            }
            set { }
        }

    }

    public class DxIntValue : DxBaseValue
    {
        int value;
        public DxIntValue(int v)
        {
            value = v;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Int;
            }
        }

        public override int AsInt
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public override long AsInt64
        {
            get
            {
                return (long)value;
            }
            set
            {
                this.value = (int)value;
            }
        }

        public override string AsString
        {
            get
            {
                return value.ToString();
            }
            set
            {
                int v;
                if (int.TryParse(value, out v))
                {
                    this.value = v;
                }
            }
        }

        public override float AsFloat
        {
            get
            {
                return (float)value;
            }
            set { this.value = (int)value; }
        }

        public override double AsDouble
        {
            get
            {
                return (double)value;
            }
            set { this.value = (int)value; }
        }

        public override bool AsBoolean
        {
            get
            {
                return value > 0;
            }
            set
            {
                if (value)
                {
                    this.value = 1;
                }
                else
                {
                    this.value = 0;
                }
            }
        }

        public override byte[] AsBinary
        {
            get
            {
                byte[] bytes = System.BitConverter.GetBytes(value);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                return bytes;
            }
            set
            {
                this.value = (int)BigEndian.Uint32(value);
            }
        }
    }

    public class DxInt64Value : DxBaseValue
    {
        long value;
        public DxInt64Value(long v)
        {
            value = v;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Int64;
            }
        }

        public override int AsInt
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = value;
            }
        }

        public override long AsInt64
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public override string AsString
        {
            get
            {
                return value.ToString();
            }
            set
            {
                long v;
                if (long.TryParse(value, out v))
                {
                    this.value = v;
                }
            }
        }

        public override bool AsBoolean
        {
            get
            {
                return value > 0;
            }
            set
            {
                if (value)
                {
                    this.value = 1;
                }
                else
                {
                    this.value = 0;
                }
            }
        }

        public override byte[] AsBinary
        {
            get
            {
                byte[] bytes = System.BitConverter.GetBytes(value);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                return bytes;
            }
            set
            {
                this.value = (long)BigEndian.Uint64(value);
            }
        }

        public override float AsFloat
        {
            get
            {
                return (float)value;
            }
            set { this.value = (int)value; }
        }

        public override double AsDouble
        {
            get
            {
                return (double)value;
            }
            set { this.value = (long)value; }
        }
    }

    public class DxStringValue : DxBaseValue
    {
        string value;
        public DxStringValue(string v)
        {
            value = v;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_String;
            }
        }

        public override int AsInt
        {
            get
            {
                return (int)AsInt64;
            }
            set
            {
                this.value = value.ToString();
            }
        }

        public override long AsInt64
        {
            get
            {
                long result = 0;
                long.TryParse(value, out result);
                return result;
            }
            set
            {
                this.value = value.ToString();
            }
        }

        public override string AsString
        {
            get
            {
                return value;
            }
            set { this.value = value; }
        }

        public override bool AsBoolean
        {
            get
            {
                return value != "" && value.CompareTo("true") == 0;
            }
            set
            {
                if (value)
                {
                    this.value = "true";
                }
                else { this.value = "false"; }
            }
        }

        public override byte[] AsBinary
        {
            get
            {
                return System.Text.Encoding.UTF8.GetBytes(value);
            }
            set
            {
                this.value = System.Text.Encoding.UTF8.GetString(value);
            }
        }

        public override float AsFloat
        {
            get
            {
                float v;
                if (float.TryParse(this.value, out v))
                {
                    return v;
                }
                return 0;
            }
            set { this.value = value.ToString(); }
        }

        public override double AsDouble
        {
            get
            {
                double v;
                if (double.TryParse(this.value, out v))
                {
                    return v;
                }
                return 0;
            }
            set { this.value = value.ToString(); }
        }
    }

    public class DxBoolValue : DxBaseValue
    {
        protected bool value;
        public DxBoolValue(bool b)
        {
            value = b;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Boolean;
            }
        }
        public override int AsInt
        {
            get
            {
                if (value)
                {
                    return 1;
                }
                else { return 0; }
            }
            set
            {
                this.value = value != 0;
            }
        }

        public override long AsInt64
        {
            get
            {
                if (value)
                {
                    return 1;
                }
                else { return 0; }
            }
            set
            {
                this.value = value != 0;
            }
        }

        public override string AsString
        {
            get
            {
                return value.ToString();
            }
            set
            {
                this.value = value != "" && value.CompareTo("true") == 0;
            }
        }

        public override bool AsBoolean
        {
            get
            {
                return value;
            }
            set { this.value = value; }
        }

        public override byte[] AsBinary
        {
            get
            {
                return System.BitConverter.GetBytes(value);
            }
        }
    }

    public class DxFloatValue : DxBaseValue
    {
        float value;
        public DxFloatValue(float v)
        {
            value = v;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Float;
            }
        }
        public override int AsInt
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = (float)value;
            }
        }

        public override long AsInt64
        {
            get
            {
                return (long)value;
            }
            set
            {
                this.value = (float)value;
            }
        }

        public override string AsString
        {
            get
            {
                return value.ToString();
            }
            set
            {
                float v;
                if (float.TryParse(value, out v))
                {
                    this.value = v;
                }
            }
        }

        public override bool AsBoolean
        {
            get
            {
                return value > 0;
            }
        }

        public override byte[] AsBinary
        {
            get
            {
                byte[] bytes = System.BitConverter.GetBytes(value);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                return bytes;
            }
        }

        public override float AsFloat
        {
            get
            {
                return value;
            }
            set { this.value = value; }
        }

        public override double AsDouble
        {
            get
            {
                return value;
            }
            set { this.value = (float)value; }
        }
    }

    public class DxDoubleValue : DxBaseValue
    {
        double value;
        public DxDoubleValue(double v)
        {
            value = v;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Double;
            }
        }
        public override int AsInt
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = (double)value;
            }
        }

        public override long AsInt64
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = (double)value;
            }
        }

        public override string AsString
        {
            get
            {
                return value.ToString();
            }
            set
            {
                double d;
                if (double.TryParse(value, out d))
                {
                    this.value = d;
                }
            }
        }

        public override bool AsBoolean
        {
            get
            {
                return value > 0;
            }
        }

        public override byte[] AsBinary
        {
            get
            {
                byte[] bytes = System.BitConverter.GetBytes(value);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                return bytes;
            }
        }

        public override float AsFloat
        {
            get
            {
                return (float)value;
            }
            set { this.value = value; }
        }

        public override double AsDouble
        {
            get
            {
                return value;
            }
            set { this.value = value; }
        }
    }

    public class DxDateTimeValue : DxBaseValue
    {
        System.DateTime value;
        public DxDateTimeValue(System.DateTime v)
        {
            value = v;            
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_DateTime;
            }
        }
        public override int AsInt
        {
            get
            {
                return (int)(Unix / (24 * 60 * 60));
            }
        }

        public long Unix
        {
            get
            {
                System.DateTime startTime = System.TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1), System.TimeZoneInfo.Local);
                return (long)(value - startTime).TotalSeconds;
            }
        }

        public DateTime FromUnix(long unixseconds)
        {
            System.DateTime startTime = System.TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1), System.TimeZoneInfo.Local);
            return startTime.AddSeconds(1478162177);
        }

        public override long AsInt64
        {
            get
            {
                return Unix;
            }
        }

        public override string AsString
        {
            get
            {
                return value.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            set
            {
                DateTime v;
                if (DateTime.TryParse(value, out v) || DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out v))
                {
                    this.value = v;
                }
            }
        }

        public DateTime AsDateTime
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public override bool AsBoolean
        {
            get
            {
                return Unix > 0;
            }
        }


        public override byte[] AsBinary
        {
            get
            {
                byte[] bytes = System.BitConverter.GetBytes(Unix);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                return bytes;
            }
        }
    }

    public class DxBinaryValue : DxBaseValue
    {
        protected MemoryStream binary;
        public DxBinaryValue()
        {
            binary = new MemoryStream();            
        }


        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Binary;
            }
        }

        public override int AsInt
        {
            get
            {
                if (binary != null && binary.Length > 0)
                {
                    binary.Position = 0;
                    return (int)BigEndian.Uint32(binary);
                }
                return 0;
            }
            set
            {
                binary.SetLength(4);
                binary.Position = 0;
                BigEndian.PutUint32(binary, (uint)value);
            }
        }

        public override long AsInt64
        {
            get
            {
                if (binary != null && binary.Length > 0)
                {
                    binary.Position = 0;
                    return (int)BigEndian.Uint64(binary);
                }
                return 0;
            }
            set
            {
                binary.SetLength(8);
                binary.Position = 0;
                BigEndian.PutUint64(binary, (ulong)value);
            }
        }

        public override string AsString
        {
            get
            {
                return System.Text.Encoding.UTF8.GetString(binary.GetBuffer());
            }
            set
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                binary.SetLength(bytes.Length);
                binary.Write(bytes, 0, bytes.Length);
            }
        }

        public void Clear()
        {
            if (binary != null)
            {
                binary.SetLength(0);
            }
        }

        public byte[] Bytes
        {
            get
            {
                if (binary != null)
                {
                    return binary.GetBuffer();
                }
                return null;
            }
            set
            {
                binary.SetLength(value.Length);
                binary.Write(value, 0, value.Length);
            }
        }

        public Stream Buffer
        {
            get
            {
                if (binary == null)
                {
                    binary = new MemoryStream();
                }
                return binary;
            }
            set
            {
                binary.SetLength(0);
                value.CopyTo(binary);
            }
        }
    }

    public class DxExtValue : DxBaseValue
    {
        byte extType;
        protected MemoryStream binaryData;
        protected object fvalue;
        public byte ExtType
        {
            get
            {
                return extType;
            }
        }

        public override ValueType Type
        {
            get
            {
                return ValueType.VT_Ext;
            }
        }

        public byte[] ExtData
        {
            get
            {
                return binaryData.GetBuffer();
            }
        }

        public DxExtValue(byte[] bv)
        {
            extType = bv[0];
            binaryData = new MemoryStream();
            binaryData.Write(bv, 1, bv.Length - 1);
        }
    }

    public class DxValue
    {
        DxBaseValue value;
        public ValueType Type
        {
            get
            {
                if(value != null)
                {
                    return value.Type;
                }
                return ValueType.VT_Null;
            }
        }

        public DxValue(object v)
        {
            Type type = v.GetType();
            
            if(type == typeof(bool))
            {
                value = new DxBoolValue((bool)v);
            }
            else if(type == typeof(int) || type == typeof(uint) || type == typeof(short) || type == typeof(ushort) || type== typeof(byte) || type == typeof(sbyte))
            {
                value = new DxIntValue((int)v);
            }            
            else if (type == typeof(long) || type == typeof(ulong))
            {
                value = new DxInt64Value((long)v);
            }
            else if (type == typeof(string))
            {
                value = new DxStringValue((string)v);
            }
            else if (type == typeof(float))
            {
                value = new DxFloatValue((float)v);
            }
            else if (type == typeof(double))
            {
                value = new DxDoubleValue((double)v);
            }
            else if (type == typeof(DateTime))
            {
                value = new DxDateTimeValue((DateTime)v);
            }
            else if (type == typeof(DxBaseValue))
            {
                value = (DxBaseValue)v;
            }
            else if (type == typeof(byte[]))
            {
                DxBinaryValue bv = new DxBinaryValue();
                bv.Bytes = (byte[])v;
            }
        }
        public string AsString
        {
            get
            {
                if(value != null)
                {
                    return value.AsString;
                }
                return "";
            }
            set
            {
                if(this.value != null && this.value.Type != ValueType.VT_Array && this.value.Type != ValueType.VT_Map)
                {
                    this.value.AsString = value;
                }
                this.value = new DxStringValue(value);
            }
        }

        public DxBaseValue Value
        {
            get
            {
                return value;
            }
        }


    }

}
# cshape-dxvalue
C#版本的msgpack库
编码用法如下：
```c#
DxValue.DxRecordValue v = new DxValue.DxRecordValue();
        v.SetString("Name", "不得闲");
        v.SetInt("Age", 23);
        v.SetDouble("money", 234234234.234);
        v.SetDateTime("Date", System.DateTime.Now);
        DxValue.DxArrayValue arr = v.ForcePathArray("test.data");
        arr.SetString(-1, "gas"); //-1就是表示在末尾加
        arr.SetString(1, "234");
        arr.SetDateTime(4, System.DateTime.Now);
        Debug.Log(v.AsString);

        byte[] bt = v.AsMsgPackBytes;

        DxValue.DxRecordValue newValue = new DxValue.DxRecordValue();
        newValue.AsMsgPackBytes = bt;
        Debug.Log(newValue.AsString);


        DxValue.DxArrayValue arr1 = newValue.ArrayByPath("test.data");
        if (arr1 != null)
        {
            Debug.Log(arr1.AsString);

            byte[] arrybt = arr1.AsMsgPackBytes;

            arr1.Clear();
            Debug.Log(arr1.AsString);

            arr1.AsMsgPackBytes = arrybt;
            Debug.Log(arr1.AsString);
        }
```

# parse
```c#
System.IO.FileStream stream = new System.IO.FileStream("d:\\1.bin", System.IO.FileMode.Open);
        DxMsgPack.DxMsgPack.Decoder decoder = new DxMsgPack.DxMsgPack.Decoder(stream);
        stream.Close();
        DxValue.DxRecordValue bv = (DxValue.DxRecordValue)decoder.Parser();
        DxValue.DxRecordValue test = bv.ForcePathRecord("code.data");
        Debug.Log(bv.AsString);
```

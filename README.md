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

# map
```c#
        DxValue.DxRecordValue mapValue = new DxValue.DxRecordValue();
        mapValue.SetInt("code", 0);
        mapValue.SetString("msg", "消息返回成功");
        DxValue.DxArrayValue arr = mapValue.ForcePathArray("Data.Records");
        for(int i = 0; i < 4; i++)
        {
            DxValue.DxRecordValue rec = arr.NewRecord(-1);
            rec.SetString("Name", "不得闲" + i);
            rec.SetInt("Age", 35);
            rec.SetString("sex", "男");
            rec.SetDateTime("Birth", System.DateTime.Now);
            rec.SetDouble("Money", 2224495.9323);
            rec.SetString("mobile", "13129903278");
        }
        DxValue.DxRecordValue datamap = (DxValue.DxRecordValue)mapValue["Data"];
        datamap.SetString("msg", "人物信息表");
        Debug.Log(mapValue.AsString);

        Debug.Log(mapValue.GetInt("code",-1));        
        arr = (DxValue.DxArrayValue)mapValue.ValueByPath("Data.Records");
        Debug.Log(((DxValue.DxRecordValue)arr[0]).GetString("Name", "test"));
```

# array
```c#
        DxValue.DxArrayValue arr = new DxValue.DxArrayValue();
        for(int i = 0; i < 5; i++)
        {
            arr.SetString(-1, "asdf");
        }
        Debug.Log(arr.AsString);
```

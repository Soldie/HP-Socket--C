# HPSocketCS
the C# SDK for [HP-Socket](https://gitee.com/ldcsaa/HP-Socket)

HPSocketCS类库升级成.net standard 2.0类库,现在至少需要.net 4.7或.net core才能使用

linux用户记得修改HPSocketCS\Sdk.cs里的dll路径
```
    public const string HPSOCKET_DLL_PATH = "hpsocket4c.so";
```

## 记得x86还是x64的dll一定要用对
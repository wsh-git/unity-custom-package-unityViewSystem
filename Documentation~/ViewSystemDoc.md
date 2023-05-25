1、通过菜单 `Asset/Create/Custom/ScriptableObject/ViewConfigDefine` 创建 `ViewConfigDefine` 数据存储对象；

2、通过项目内部资源加载方法获取`ViewConfigDefine` 数据存储对象，传入到 `ViewManager.InitAsync(prafabPath, viewConfigDefine, InstantiateAsync, Instantiate, onComplete)` 进行初始化；

​    prafabPath: uiRoot所在的路径；

​    viewConfigDefine: view配置数据对象；

​    InstantiateAsync: 异步实例化接口



```C#
public static void InstantiateAsync(string prefabPath, GameObject parent, Action<GameObject> onComplete);

```

​    Instantiate: 同步实例化接口

```C#
public static GameObject Instantiate(GameObject prefab, GameObject parent);
```

​    onComplete: 初始化成功回调，参数为实例化的viewManager；

[![Build Status](https://dev.azure.com/kissstudio/DotNetDetour/_apis/build/status/kissstudio.DotNetDetour?branchName=master)](https://dev.azure.com/kissstudio/DotNetDetour/_build/latest?definitionId=3?branchName=master)
[![996.icu](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu)
[![LICENSE](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/halx99/yasio/blob/master/LICENSE)

# :open_book:DotNetDetour
DotNetDetour是一个用于.net方法hook的类库

## 特点
* 支持32bit和64bit的.net程序
* 支持静态方法，实例方法、属性方法、泛型类型的方法、泛型方法的hook
* 支持.net基础类库方法的hook
* 无任何性能影响，无需知道和改动被hook的方法源码

## 实现原理
https://bbs.csdn.net/topics/391958344

## 基础示例
1. git clone本项目最新源码使用；或者NuGet安装（可能未及时更新）：`Install-Package DotNetDetour`, 
或者：`Install-Package kissstudio.DotNetDetour`。

2. 参考以下例子实现`IMethodHook`接口，使用特性标记要Hook的方法
``` C#
namespace Test.Solid {
    //假设有一个已存在的类（并且无法修改源码，如.Net框架的方法）
    public class SolidClass{
        public string Run(string msg){
            return msg+"(run)";
        }
    }
}

namespace Test{
    //我们自行实现一个类来修改Run方法的行为，此类用IMethodHook接口修饰
    public class MyClass:IMethodHook{
        //我们实现一个新Run方法，并标记为HookMethod，覆盖SolidClass中的Run方法
        [HookMethod("Test.Solid.SolidClass")]
        public string Run(string msg){
            return "Hook " + Run_Original(msg);
        }
        
        //实现一个占位方法，此方法代表被Hook覆盖的原始方法
        [OriginalMethod]
        public string Run_Original(string msg){
            return null; //这里写什么无所谓，能编译过即可
        }
    }
}
```

3. 在程序中执行安装操作（只需运行一次即可），最佳运行时机：必须在被Hook方法被调用前执行，最好程序启动时运行一次即可。
``` C#
MethodHook.Install();
```

4. 当执行到被Hook的方法时，该调用将被转到我们的Hook方法执行：
``` C#
var msg=new SolidClass().Run("Hello World!");

//Hook Hello World!(run)
```





# :open_book:Hook场景

## 普通方法Hook

静态和非静态的普通方法Hook操作都是一模一样的，两步到位：新建一个类实现`IMethodHook`接口，编写普通Hook方法，用`HookMethod`特性标记此方法，有无static修饰、返回值类型（仅针对引用性质的类型，非int等值类型）不同都不影响，但参数签名要和被Hook的原始方法一致，值类型和引用类型尽量不要混用。


### 第一步：新建一个类实现`IMethodHook`接口
我们编写的Hook方法所在的类需要实现`IMethodHook`接口，此接口是一个空接口，用于快速的查找Hook方法。

或者使用`IMethodHookWithSet`接口(算Plus版吧)，此接口带一个`HookMethod(MethodBase method)`方法，这个类每成功进行一个Hook的初始化，就会传入被Hook的原始方法（可判断方法名称来确定是初始化的哪个方法），这个方法可用于获取方法所在的类（如：私有类型），可用于简化后续的反射操作；注意：此方法应当当做静态方法来进行编码。


### 第二步：编写Hook方法，用`HookMethod`特性标记
`HookMethod`(`type`,`targetMethodName`,`originalMethodName`) ，`type`参数支持：Type类型对象、类型完全限定名。如果能直接获取到类型对象，就使用Type类型对象；否则必须使用此类型的完全限定名（如：私有类型），如：`System.Int32`、`System.Collections.Generic.List&#96;1[[System.String]]`。
``` C#
[HookMethod("Namespace.xxx.MyClass", "TargetMethodName", "OriginalMethodName")]
public string MyMethod(string param){...}

[HookMethod(typeof(MyClass))]
public string MyMethod(string param){...}
```
如果我们的方法名称和被Hook的目标方法名称一致，无需提供`targetMethodName`参数。

如果我们提供目标原始方法的占位方法`OriginalMethod`，并且名称为`目标原始方法名称` `+` `_Original`，或者当前类内只有一个Hook方法，无需提供`originalMethodName`参数。

### 注意：方法参数
参数签名要和被Hook的原始方法一致，如果不一致将导致无法找到原始方法（原因：存在重载方法无法确认是哪个的问题）。

如果存在我们无法使用的参数类型的时候（如：私有类型），我们可以用object等其他引用类型代替此类型（注意不要用值类型，否则可能出现内存访问错误），并把此参数用`RememberType`进行标记：
``` C#
//目标方法:
public string SolidMethod(MyClass data, int code){...}

//我们的Hook方法：
public string MyMethod([RememberType("Namespace.xxx.MyClass")]object data, int code){...}
```

### 可选：提供`OriginalMethod`特性标记的原始方法
如果我们还想调用被Hook的原始方法，我们可以提供一个占位方法，此方法用`OriginalMethod`进行标记即可。此方法只起到代表原始方法的作用，不需要可以不提供，要求：参数签名必须和我们写的Hook方法一致（原因：存在重载方法无法确认是哪个的问题）。

此方法默认名称格式为`目标原始方法名称` `+` `_Original`，不使用这个名称也可以，但如果使用其他名称并且当前类中有多个Hook方法，必须在Hook方法`HookMethod`特性中进行设置`originalMethodName`进行关联。
``` C#
[OriginalMethod]
public string SolidMethod_Original(object data, int code){
```

### 可选：给我们的Hook方法传递参数
我们编写Hook方法是在被Hook的原始方法被调用时才会执行的，我们可能无法修改调用过程的参数（如果是能修改方法的话就跳过此节），虽然我们编写的Hook方法可以是非静态方法，但我们应当把它当静态方法来看待，虽然可以用属性字段（非静态的也当做静态）之类的给我们的Hook方法传递数据，但如果遇到并发，是不可靠的。

我们可以通过当前线程相关的上下文来传递数据，比如：`HttpContext`、`CallContext`、`AsyncLocal`、`ThreadLoacl`。推荐使用`CallContext.LogicalSetData`来传递数据，如果可以用`HttpContext`就更好了（底层也是用`CallContext.HostContext`来实现的）。`ThreadLoacl`只能当前线程用，遇到异步、多线程就不行了。`AsyncLocal`当然是最好的，但稍微低些版本的.Net Framework还没有这个。

``` C#
[HookMethod("Namespace.xxx.MyClass", "TargetMethodName", "OriginalMethodName")]
public string MyMethod(string param){
    if (CallContext.LogicalGetData("key") == (object)"value") {
        //执行特定Hook代码
        return;
    }
    //执行其他Hook代码
    ...
}

//调用
CallContext.LogicalSetData("key", "value");
new MyClass().MyMethod("");
CallContext.LogicalSetData("key", null);
```

注：虽然大部分多线程、异步环境下调用上下文是会被正确复制传递的，但如果哪里使用了`ConfigeAwait(false)`或者其他影响上下文的操作（定时回调、部分异步IO回调好像也没有传递），当我们的Hook方法执行时，可能上下文数据并没有传递进来。


## 异步方法Hook
异步方法的Hook方法需要用async来修饰、返回Task类型，其他和普通方法Hook没有区别。

小提醒：不要在存在SynchronizationContext(如：HttpContext、UI线程)的线程环境中直接在同步方法中调用异步方法，[真发生异步行为时100%死锁](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)，可以强制关闭SynchronizationContext来规避此种问题，但会引发一系列问题。**如果使用过程中发生死锁，跟我们进行的Hook操作没有关系**。
``` C#
[HookMethod(typeof(MyClass))]
public async Task<int> MyMethodAsync() {...}

//异步环境调用
val=await new MyClass().MyMethodAsync();

//同步环境调用
var bak = SynchronizationContext.Current;
SynchronizationContext.SetSynchronizationContext(null);
try {
    val=new MyClass().MyMethodAsync().Result;
} finally {
    SynchronizationContext.SetSynchronizationContext(bak);
}
```


## 属性Hook
属性其实是`get_xxx()`名称的普通方法，比如`MyProperty`属性Hook `get_MyProperty()`这个普通方法即可。
``` C#
[HookMethod("Namespace.xxx.MyClass")]
public string get_MyProperty(){...}

[OriginalMethod]
public string get_MyProperty_Original(){...}
```

或者在get块上方进行标记，规则和普通方法一致：
``` C#
public string MyProperty{
    [HookMethod("Namespace.xxx.MyClass")]
    get{ ... }
}

public string MyProperty_Original{
    [OriginalMethod]
    get{ ... }
}
```

注：Hook属性时有可能能成功设置此Hook，但不一定会执行我们的代码，可能是编译过程中优化了整个调用过程，跳过了部分属性方法，直接返回了最深层次的调用值，如下面这种类似的属性获取方式：
``` C#
int A{get{return B;}}
int B{get{return C;}}
int C{get{return 123;}}
```
我们Hook A属性，能成功设置Hook方法，但我们调用A属性时，并不会执行我们的Hook方法。换B也不行，只有Hook C才行。也许是编译的时候把A、B的调用直接优化成了对C的调用，我们只需要对最深层次的属性调用进行Hook就能避免此问题。(这个只是演示可能会出现的问题，我们自己特意写代码去测试并不能复现)。


## ~字段Hook~
~不支持，应该直接用反射来操作。~


## 构造方法Hook
我们编写个返回值为void、方法名称为类名称的普通方法即可实现。如果方法名称无法使用类名称时，需在`HookMethod`中设置`targetMethodName`为`.ctor`。其他规则和普通方法一致。
``` C#
[HookMethod("Namespace.xxx.MyClass")]
public void MyClass(string param) {
    ...
    MyClass_Original(param);//可选调用自身实例化方法
    ...
}

[OriginalMethod]
public void MyClass_Original(string param) {}
```


## 泛型类的方法Hook

形如`class MyClass<T>{ T MyMethod(T param, object param2){...}  }`这种泛型，对里面的方法进行Hook。泛型类中方法的Hook和普通方法Hook没有多大区别，只是在提供`HookMethod`特性的`type`参数时需要对类型具体化，比如调用的地方使用的是int类型，那么我们就Hook int类型的此类：`typeof(MyClass<int>)`、`Namespace.xxx.MyClass&#96;1[[System.Int32]]`，其他和普通方法规则相同。

由于存在`引用类型`和`值类型`两种类型，并且表现不一致，我们在具体化时要分开对待。

### 值类型泛型参数
每种使用到的值类型泛型参数的具体类型都需要单独实现Hook，`int`、`bool`等为值类型都要单独实现，如`int`类型写法：
``` C#
[HookMethod("Namespace.xxx.MyClass`1[[System.Int32]]")]
public int MyMethod(int param, object param2) {
```


### 引用类型泛型参数
每种使用到引用类型参数的具体类型都共用一个Hook，**注意是：同一个泛型类中的同一个方法只能用一个相同方法进行Hook**，`string`、`普通object`等都是引用类型都共用一个Hook，如`string`类型写法：
``` C#
[HookMethod("Namespace.xxx.MyClass`1[[System.Object]]")]
public object MyMethod(object param, object param2) {
    if(param is string){
        ... //string 类型实现代码
    } else if(param is xxxx){
        ... //其他引用类型实现代码
    }
```


## 泛型方法Hook

形如`T MyMethod<T>(T param, object param2)`这种泛型方法，我们对这种方法进行Hook时需要把类型具体化，并用`RememberType(isGeneric: true)`特性标记涉及到的泛型参数，比如调用的地方是int类型，那么我们就Hook int类型的此方法`int MyMethod([RememberType(isGeneric: true)]int param, object param2)`，其实最终还是一个普通方法，按普通方法规则来写代码。

由于存在`引用类型`和`值类型`两种类型，并且表现不一致，我们在具体化时要分开对待。

### 值类型泛型参数
每种使用到值类型泛型参数的都单独实现Hook，`int`、`bool`等为值类型都要单独实现，如int类型写法：
``` C#
[HookMethod("Namespace.xxx.MyClass")]
public int MyMethod([RememberType(isGeneric: true)]int param) {
```

### ~引用类型泛型参数~
~不支持，引用类型泛型参数的方法Hook目前是不支持的，如：`MyMethod<object>(object_xxx)`、`MyMethod<string>(string_xxx)`都是不支持的。表现为泛型方法被正确Hook后，并不会走我们的逻辑，具体原因不明。~




# :open_book:其他

## 关于测试项目内存访问异常

vs的测试功能会启动一个执行引擎，其默认选项是复用执行引擎。
反复运行测试时对修改汇编指令会造成影响。
从菜单关闭该选项`Test` -> `Test Settings` -> `Keep Test Execution Engine Running`，即可解除此影响。

另外调试测试是不能得出正确的结果的，可能是汇编代码不能在调试模式下工作。



## 老版本兼容

自[bigbaldy1128](https://github.com/bigbaldy1128) `2016-5`开源此项目后，到`2018-12` [kissstudio](https://github.com/kissstudio) 和 [xiangyuecn](https://github.com/xiangyuecn) 升级了此项目代码，把相关代码方式升级和合理化了一番(参考 [#4](https://github.com/bigbaldy1128/DotNetDetour/issues/4) [#5](https://github.com/bigbaldy1128/DotNetDetour/issues/5) )。

已对3个主要的方法都进行了变更：

1. `Monitor`、`ClrMethodHook` -> `MethodHook`
2. `MonitorAttribute`、`RelocatedMethodAttribute` -> `HookMethodAttribute`
3. `OriginalAttribute`、`ShadowMethodAttribute(不兼容)` -> `OriginalMethodAttribute`

除`ShadowMethodAttribute`外（升级需要改动被标记的方法名称，因而无法兼容），这3个变更都是兼容的，但不推荐继续使用老方法，并且将来可能会从类库里面移除。

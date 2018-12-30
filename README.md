
[![Build Status](https://dev.azure.com/kissstudio/DotNetDetour/_apis/build/status/kissstudio.DotNetDetour?branchName=master)](https://dev.azure.com/kissstudio/DotNetDetour/_build/latest?definitionId=3?branchName=master)

## DotNetDetour
DotNetDetour是一个用于.net方法hook的类库

## 特点
* 支持32bit和64bit的.net程序
* 支持静态方法，实例方法、属性方法、泛型类型的方法、泛型方法的hook
* 支持.net基础类库方法的hook
* 无任何性能影响，无需知道和改动被hook的方法源码

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
		//我们实现一个新Run方法，并标记为RelocatedMethodAttribute，覆盖SolidClass中的Run方法
		[RelocatedMethodAttribute("Test.Solid.SolidClass")]
		public string Run(string msg){
			return "Hook " + Run_Original(msg);
		}
		
		//实现一个占位影子方法，此方法代表被Hook覆盖的原始方法
		[ShadowMethod]
		public string Run_Original(string msg){
			return null; //这里写什么无所谓，能编译过即可
		}
	}
}
```

3. 在程序中执行安装操作（只需运行一次即可），最佳运行时机：必须在被Hook方法被调用前执行，最好程序启动时运行一次即可。
``` C#
ClrMethodHook.Install();
```

4. 当执行到被Hook的方法时，该调用将被转到我们的Hook方法执行：
``` C#
var msg=new SolidClass().Run("Hello World!");

//Hook Hello World!(run)
```





## Hook场景

### 普通方法Hook

静态和非静态的普通方法Hook操作都是一模一样的，编写普通Hook方法，用`RelocatedMethodAttribute`注解标记此方法，有无static修饰、返回值类型不同都不影响，但参数签名要和被Hook的原始方法一致。

#### `RelocatedMethodAttribute`(`type`,`targetMethodName`,`shadowMethodName`)注解
支持：Type类型对象、类型完全限定名。如果能直接获取到类型对象，就使用Type类型对象；否则必须使用此类型的完全限定名（如：私有类型），如：`System.Int32`、`System.Collections.Generic.List`1[[System.String]]`。
``` C#
[RelocatedMethodAttribute("Namespace.xxx.MyClass", "TargetMethodName", "ShadowMethodName")]
public string MyMethod(string param){...}

[RelocatedMethodAttribute(typeof(MyClass))]
public string MyMethod(string param){...}
```
如果我们的方法名称和被Hook的目标方法名称一致，无需提供`targetMethodName`参数。

如果我们提供目标原始方法的占位影子方法`ShadowMethod`，并且名称为`目标原始方法名称` `+` `_Original`，或者当前类内只有一个Hook方法，无需提供`shadowMethodName`参数。

#### 方法参数
参数签名要和被Hook的原始方法一致，如果不一致将导致无法找到原始方法（如：重载方法无法确认是哪个的问题）。

如果存在我们无法使用的参数类型的时候（如：私有类型），我们可以用object等其他类型代替此类型，并把此参数用`RememberType`进行标记：
``` C#
//目标方法:
public string SolidMethod(MyClass data, int code){...}

//我们的Hook方法：
public string MyMethod([RememberType("Namespace.xxx.MyClass")]object data, int code){...}
```

#### `ShadowMethodAttribute`注解原始方法
如果我们还想调用被Hook的原始方法，我们可以提供一个占位方法，此方法用`ShadowMethodAttribute`进行注解即可。此方法只起到代表原始方法的作用，不需要可以不提供，要求：参数签名必须和我们写的Hook方法一致（如：重载方法无法确认是哪个的问题）；默认名称为`目标原始方法名称` `+` `_Original`，不使用这个名称也可以，但如果使用其他名称并且当前类中有多个Hook方法，必须在Hook方法`RelocatedMethodAttribute`注解中进行设置`shadowMethodName`进行关联。
``` C#
[ShadowMethod]
public string SolidMethod_Original(object data, int code){
```


### 属性Hook
属性其实是`get_xxx()`名称的普通方法，比如`MyProperty`属性Hook `get_MyProperty()`这个普通方法即可即可。

或者在get块上方进行注解，规则和普通方法一致：
``` C#
public string MyProperty{
	[RelocatedMethodAttribute("Namespace.xxx.MyClass")]
	get{ ... }
}

public string MyProperty_Original{
	[ShadowMethod]
	get{ ... }
}
```


### ~字段Hook~
不支持，应该直接用反射。


### 构造方法Hook
我们编写个返回值为void、方法名称为类名称的普通方法即可实现。如果方法名称无法使用类名称时，需在`RelocatedMethodAttribute`中设置`targetMethodName`为`.ctor`。其他规则和普通方法一致。
``` C#
[RelocatedMethodAttribute("Namespace.xxx.MyClass")]
public string MyClass(string param) {
```


### 泛型类

形如`class MyClass<T>{ T MyMethod(T param, object param2){...}  }`这种类型内的方法Hook。泛型类中方法的Hook和普通方法Hook没有多大区别，只是在提供`RelocatedMethodAttribute`注解`type`参数时需要对类型具体化，比如调用的地方使用的是int类型，那么我们就Hook int类型的此类`typeof(MyClass<int>)` `Namespace.xxx.MyClass&#96;1[[System.Int32]]`，其他和普通方法规则相同。

由于存在`引用类型`和`值类型`两种类型，并且表现不一致，我们在具体化时要分开对待。

#### 值类型泛型参数
每种使用到值类型泛型参数的都单独实现Hook，int、bool等为值类型，如int类型写法：
``` C#
[RelocatedMethodAttribute("Namespace.xxx.MyClass`1[[System.Int32]]")]
public int MyMethod(int param, object param2) {
```


#### 引用类型泛型参数
此泛型每种使用到引用类型参数的都共用一个Hook，**注意：同一个泛型类中的同一个方法只能用一个相同方法进行Hook**，string、普通object等都是引用类型，如string类型写法：
``` C#
[RelocatedMethodAttribute("Namespace.xxx.MyClass`1[[System.Object]]")]
public object MyMethod(object param, object param2) {
	if(param is string){
		... //string 实现代码
	} else if(param is xxxx){
		... //其他引用类型实现代码
	}
```


### 泛型方法Hook

形如`T MyMethod<T>(T param, object param2)`这种泛型方法，我们对这种方法进行Hook时需要把类型具体化，并用`RememberType(isGeneric: true)`注解涉及到的泛型参数，比如调用的地方是int类型，那么我们就Hook int类型的此方法`int MyMethod([RememberType(isGeneric: true)]int param, object param2)`，其实最终还是一个普通方法，按普通方法规则来写代码。

由于存在`引用类型`和`值类型`两种类型，并且表现不一致，我们在具体化时要分开对待。

#### 值类型泛型参数
每种使用到值类型泛型参数的都单独实现Hook，int、bool等为值类型，如int类型写法：
``` C#
[RelocatedMethodAttribute("Namespace.xxx.MyClass")]
public int MyMethod([RememberType(isGeneric: true)]int param) {
```

#### ~引用类型泛型参数~
不支持，引用类型泛型参数的方法Hook目前是不支持的，如：`MyMethod<object>(object_xxx)`、`MyMethod<string>(string_xxx)`都是不支持的。表现为泛型方法被正确Hook后，并不会走我们的逻辑，具体原因不明。





## 关于测试项目内存访问异常

vs的测试功能会启动一个执行引擎，其默认选项是复用执行引擎。
反复运行测试时对修改汇编指令会造成影响。
从菜单关闭该选项Test->Test Settings ->Keep Test Execution Engine Running，即可解除此影响。

另外调试测试是不能得出正确的结果的，可能是汇编代码不能在调试模式下工作。

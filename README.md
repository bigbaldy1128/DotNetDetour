
[![Build Status](https://dev.azure.com/kissstudio/DotNetDetour/_apis/build/status/kissstudio.DotNetDetour?branchName=master)](https://dev.azure.com/kissstudio/DotNetDetour/_build/latest?definitionId=3?branchName=master)
## DotNetDetour
DotNetDetour是一个用于.net方法hook的类库
##特点
* 支持32bit和64bit的.net程序
* 支持.net framework 2.0以上的所有版本
* 支持静态方法，实例方法、属性方法、泛型方法的hook
* 支持.net基础类库方法的hook
* 无任何性能影响

##快速示例
1.安装：Install-Package DotNetDetour

2.新建一个类并继承IMethodMonitor接口
```
public class CustomMonitor : IMethodMonitor //自定义一个类并继承IMethodMonitor接口
{
    [Monitor("TargetNamespace","TargetClass")] //目标方法的名称空间，类名
    public string Get() //方法签名要与目标方法一致
    {
        return "B" + Ori();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [Original] //原函数标记
    public string Ori() //方法签名要与目标方法一致
    {
        return null; //这里写什么无所谓，能编译过即可
    }
}
```
3.定义目标函数，例如：
```
public string Get()
{
    return "A"
}
```
4.安装监视器
```
Console.WrtieLine(Get());
Monitor.Install()
Console.WrtieLine(Get());
```
第一次调用Get输出的值是"A",第二次是"BA"



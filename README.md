## DotNetDetour
DotNetDetour是一个用于.net方法hook的类库
##特点
* 支持32bit和64bit的.net程序
* 支持.net framework 2.0以上的所有版本
* 支持静态方法，实例方法、属性方法、泛型方法的hook
* 支持.net基础类库方法的hook
* 无任何性能影响

##Hook方法示例
1.安装：Install-Package DotNetDetour

2.定义一个监视器，编译成dll，放入程序所在目录下的monitors目录
```
public class CustomMonitor : IMethodMonitor //自定义一个类并继承IMethodMonitor接口
{
    [Monitor("Target","TargetClass","Target.exe")] //目标方法的名称空间，类名，程序集（如果是mscorlib中的可以省略）
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
3.安装监视器
`Monitor.Install("monitors")` //这里指定默认目录为monitors

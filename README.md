
[![Build Status](https://dev.azure.com/kissstudio/DotNetDetour/_apis/build/status/kissstudio.DotNetDetour?branchName=master)](https://dev.azure.com/kissstudio/DotNetDetour/_build/latest?definitionId=3?branchName=master)
## DotNetDetour
DotNetDetour是一个用于.net方法hook的类库
##特点
* 支持32bit和64bit的.net程序
* 支持静态方法，实例方法、属性方法、泛型方法的hook
* 支持.net基础类库方法的hook
* 无任何性能影响

##快速示例
1.安装：Install-Package kissstudio.DotNetDetour, 
注意，DotNetDetour是原作者的版本，kissstudio.DotNetDetour 是我修改的版本，方法匹配兼容性更好，内核未做变动

2.参考以下例子实现IMethodHook接口，使用特性标记要Hook的方法

```
public class ComputerDetour : IMethodHook
    {
        #region 静态方法HOOK
        [RelocatedMethodAttribute(typeof(Computer), "GetCpu")]
        public static string _impl_GetCpu()
        {
            return "Not " + GetCpu();
        }

        [ShadowMethod(typeof(Computer), "GetCpu")]
        public static string GetCpu()
        {
            return null;
        }
        #endregion

        #region 实例方法
        [RelocatedMethodAttribute(typeof(Computer), "GetRAMSize")]
        public string _impl_GetRAMSize()
        {
            return "Not " + GetRAMSize();
        }

        [ShadowMethod(typeof(Computer), "GetRAMSize")]
        public string GetRAMSize()
        {
            return null;
        }
        #endregion


        #region 实例属性
        public string _impl_Os
        {
            [RelocatedMethodAttribute(typeof(Computer), "get_Os")]
            get
            {
                return "Not " + Os;
            }
        }

        public string Os
        {

            [ShadowMethod(typeof(Computer), "get_Os")]
            get
            {
                return null;
            }
        }
        #endregion

        #region 泛型方法
        [RelocatedMethodAttribute(typeof(ComputerOf<string>), "ComputerIo")]
        public string _impl_ComputerIo(string name)
        {
            var human = ComputerIo(name);
            human = "Not " + human;
            return human;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [ShadowMethod(typeof(ComputerOf<string>), "ComputerIo")]
        public string ComputerIo(string owner)
        {
            return null;
        }
        #endregion
    }

    public class NetFrameworkDetour : IMethodHook
    {
        [RelocatedMethod(typeof(System.IO.File), "ReadAllText")]
        public static string _impl_ReadAllText(string file)
        {
            try
            {
                return ReadAllText(file) + "NetFrameworkDetour";
            }
            catch (Exception ex)
            {
                Debugger.Break();
                throw;
            }
        }

        [ShadowMethod(typeof(System.IO.File), "ReadAllText")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ReadAllText(string file)
        {
            return null;
        }
    }
```
如例子所示，在自行实现的方法里（由RelocatedMethod标记），仍然可以通过影子方法（代表原始方法的占位符，由ShadowMethod标记）来实现对原始目标方法的调用

3.调用框架代码来初始化实现方法调用的转发
```
ClrMethodHook.Install();

```
4.当执行到目标方法时，该调用将被转发到你自己的实现；

### 关于测试项目内存访问异常
vs的测试功能会启动一个执行引擎，其默认选项是复用执行引擎。
修改汇编指令会对其造成影响，这个影响不能从汇编字节对比（IsDetourInstalled方法）获取到。
因此从菜单关闭该选项Test->Test Settings ->Keep Test Execution Engine Running ，
经我测试，目前没有发现问题了。

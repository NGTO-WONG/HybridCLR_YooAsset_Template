using UnityEngine;

//namespace 域名.项目名称.模块

namespace Game.Script.HotUpdate.FrameWork
{
    /// <summary>
    /// 脚本单例工具类
    /// 特点：
    /// 1.唯一
    /// 2.常用
    /// 功能：
    /// 1.保持一个游戏对象的单一，整个进程只有这一个对象
    /// 2.对单一游戏对象的控制和调用
    /// 解释：
    /// 泛型类<T>,为了子类可以传递类型
    /// 泛型约束：类型参数必须是指定的基类或派生自指定的基类。
    /// 子类需要继承此单例类
    /// 如何使用：
    /// 1.继承时，必须传递子类类型
    /// 2.在任意脚本生命周期中，通过子类类型访问Instance属性， XXXX.Instance.XXX
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        // T 代表子类型 通过 where T : MonoSingleton<T> 来体现
        //按需加载
        private static T instance;

        //只能读取get
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    //在场景中根据类型来查找引用
                    //只执行一次
                    instance = FindObjectOfType<T>();
                    //场景中没这个类型==游戏对象未挂载脚本
                    if (instance == null)
                    {
                        //创建一个脚本对象（立即执行Awake）
                        new GameObject("Singleton of " + typeof(T)).AddComponent<T>();
                    }
                    else
                    {
                        //如果找到了子类，立即初始化
                        instance.Init();
                    }
                }

                return instance;
            }
        }

        protected void Awake()
        {
            //脚本自行挂载到游戏物体上了，调Awake
            if (instance == null)
            {
                //子类 = 父类 as（强转成） 子类
                //泛型约束 Where T ： MonoSingleton<T>
                instance = this as T;
                Init();
            }
        }

        /// <summary>
        /// 因为继承Unity的类不是依靠构造函数实例化的，所以要新建一个Init函数，通过重写Override实现一些单例的初始化。
        /// 后代做初始化，不需要用awake，自行初始化
        /// </summary>
        public virtual void Init()
        {
        }
    }
}
using Autofac;
using Autofac.Extras.DynamicProxy;
using System;
using System.Reflection;
using System.Text;

namespace RM.Common
{
    public class IocContainer
    {
        private static IContainer Container { get; set; }

        public static void Init(string[] assemblyList)
        {
            StringBuilder sb = new StringBuilder();
            var builder = new ContainerBuilder();
            foreach (var ass in assemblyList)
            {
                Assembly assInfo = Assembly.Load(ass);
                foreach (var type in assInfo.GetTypes())
                {
                    if (type.IsDefined(typeof(InjectableAttribute)))
                    {
                        var inj = type.GetCustomAttribute<InjectableAttribute>();
                        var imp = inj.Implementation;
                        var newType = builder.RegisterType(imp);
                        switch (inj.InjectType)
                        {
                            case InjectType.InstancePerDependency:
                                newType.InstancePerDependency();
                                break;
                            case InjectType.InstancePerLifetimeScope:
                                newType.InstancePerLifetimeScope();
                                break;
                            case InjectType.SingleInstance:
                                newType.SingleInstance();
                                break;
                            case InjectType.ExternallyOwned:
                                newType.ExternallyOwned();
                                break;
                        }

                        if (imp.IsDefined(typeof(InterceptableAttribute)))
                        {
                            var impAtt = imp.GetCustomAttribute<InterceptableAttribute>();
                            newType.EnableInterfaceInterceptors();//！！！--EnableClassInterceptors
                            foreach (var t in impAtt.InterceptTypes)
                            {
                                newType.InterceptedBy(t);
                            }
                        }
                        newType.As(type);
                    }
                }
            }
            Container = builder.Build();
        }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }

    public class InjectableAttribute : Attribute
    {
        public InjectableAttribute(Type _implementation, InjectType _injectType = InjectType.InstancePerDependency)
        {
            Implementation = _implementation;
            InjectType = _injectType;
        }

        public Type Implementation { get; set; }

        public InjectType InjectType { get; set; }
    }

    public enum InjectType
    {
        InstancePerDependency,
        InstancePerLifetimeScope,
        SingleInstance,
        ExternallyOwned
    }

    public class InterceptableAttribute : Attribute
    {
        public Type[] InterceptTypes { get; set; }

        public InterceptableAttribute(params Type[] _types)
        {
            InterceptTypes = _types;
        }
    }

    
}

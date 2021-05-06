using System;

using Microsoft.Practices.Unity;

namespace Common.Container
{
    public class ComponentContainer : IComponentContainer
    {
        private readonly Lazy<UnityContainer> _lazyUnityContianer = new Lazy<UnityContainer>(() => new UnityContainer());

        internal ComponentContainer()
        {
        }

        public void RegisterType<TKeyType, TValueType>(bool isReuseRootComponent = true) where TValueType : TKeyType
        {
            ContainerControlledLifetimeManager lifeTimeManager = null;
            if (isReuseRootComponent)
            {
                lifeTimeManager = new ContainerControlledLifetimeManager();
            }

            this._lazyUnityContianer.Value.RegisterType<TKeyType, TValueType>(lifeTimeManager);
        }

        public void RegisterInstance(Type keyType, object value)
        {
            this._lazyUnityContianer.Value.RegisterInstance(keyType, value);
        }

        public T Resolve<T>()
        {
            return this._lazyUnityContianer.Value.Resolve<T>();
        }

        public void Release()
        {
            this._lazyUnityContianer.Value.Dispose();
        }
    }
}

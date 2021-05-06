using System;

namespace Common.Container
{
    /// <summary>
    /// RRR(Register, Resolve, Release) pattern 구현을 위한 인터페이스.
    /// <para>구현체로 UnityContainer를 사용한다.</para>
    /// </summary>
    public interface IComponentContainer
    {
        void RegisterType<TKeyType, TValueType>(bool isReuseRootComponent = true) where TValueType : TKeyType;

        void RegisterInstance(Type keyType, object value);

        T Resolve<T>();

        void Release();
    }
}

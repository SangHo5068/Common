using System;

namespace Common.Container
{
    public class ContainerResolver
    {
        private static readonly Lazy<ComponentContainer> LazyComponentContainer = new Lazy<ComponentContainer>(() => new ComponentContainer());

        public static IComponentContainer GetContainer()
        {
            return LazyComponentContainer.Value;
        }
    }
}

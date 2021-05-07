using Common.Container;
using Common.Container.Events;

namespace Common
{
    public class Bootstrapper
    {
        public static void Initialize()
        {
            var container = ContainerResolver.GetContainer();

            // TODO : 추후 필요한 것이 있으면 추가할 것.
            // 전역 Event 사용을 위해 Service Locator 에 등록 
            container.RegisterType<IEventAggregator, EventAggregator>();
        }
    }
}

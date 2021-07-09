using Common.Models;

using Common.Notify;

namespace Common.Demo.Views
{
    public class BaseContentView : BaseViewModel
    {
        #region Properties
        #endregion //Properties



        public BaseContentView() { }
        public BaseContentView(BindableAndDisposable parent, params object[] param)
            : base(parent, param)
        {
        }



        public override void InitialData(params object[] param)
        {
            base.InitialData(param);
        }
    }
}

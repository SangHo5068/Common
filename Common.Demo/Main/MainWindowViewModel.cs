using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Common.Base;
using Common.Command;
using Common.Demo.Views;
using Common.Net.Stomp;
using Common.Utilities;

using ServiceBase;

namespace Common.Demo
{
    public class MainWindowViewModel : BaseMainControlViewModel
    {
        private string _ProductVersion;
        private string _BuildTime;
        private BaseContentView _CurrentView;
        private ObservableCollection<IDataInfo> _Datas = new ObservableCollection<IDataInfo>();

        #region Properties
        public string ProductVersion
        {
            get => _ProductVersion;
            set => SetValue(ref _ProductVersion, value);
        }
        public string BuildTime
        {
            get => _BuildTime;
            set => SetValue(ref _BuildTime, value);
        }

        public BaseContentView CurrentView
        {
            get => _CurrentView;
            set => SetValue(ref _CurrentView, value);
        }

        public ObservableCollection<IDataInfo> Datas
        {
            get => _Datas;
            set => SetValue(ref _Datas, value);
        }
        #endregion //Properties

        #region ICommand
        public ICommand CommandMouseMove { get; private set; }
        #endregion //ICommand



        public MainWindowViewModel()
        {
            Header = "WPF Demo App";
        }



        #region override
        protected override void DisposeManaged()
        {
            base.DisposeManaged();
        }
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CommandMouseMove = new RelayCommand(OnWindowMouseMove);
        }
        protected override void InitData(object parameter)
        {
            base.InitData(parameter);

            var gitVer = GitHelper.VersionToString();
            ProductVersion = gitVer;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            BuildTime = AppDefined.Get_BuildDateTime(version).ToString(Defined.DateSFormat);

            CurrentView = new BaseContentView();
        }
        #endregion //override


        #region Methods
        private void OnWindowMouseMove(object obj)
        {
            try
            {
                if (obj is Window window)
                    window.DragMove();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        #region WebSocket

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        private async Task DoClientWebSocket(CancellationTokenSource cancellation)
        {
            try
            {
                var serverUri = new Uri(ServiceUrl.BASEURL_WS);
                //using (WebSocket ws = new WebSocket("wss://ht-release-api.mobicareconsole.com/mobiCAREConsole/ws?SX-API-Route=GWS-1"))
                var ws = new StompWebsocketClient(serverUri.AbsoluteUri,
                                        System.Security.Authentication.SslProtocols.Tls12);
                //ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                //ws.OnMessage += ws_OnMessage;

                ws.OnClose += ws_OnClose;
                ws.OnOpen += ws_OnOpen;
                ws.OnError += ws_OnError;

                var token = ServiceRequest.Instance.AuthToken;
                var nativeHeaders = new Dictionary<string, string>() {
                    {"SX-Auth-Token", token},
                    {"deviceKind", 5.ToString()},
                    {"connType", 2.ToString()},
                    {"apiRoute", "GWS-1"},
                    {"requester", "UserCode"},
                    {"requestDateTime", DateTime.Now.ToString(Common.Defined.DateMinusSFormat)},
                    {"accept-version", "1.1,1.0"},
                    {"heart-beat", "10000,10000"},
                };

                ws.Connect(nativeHeaders);

                await Task.Run(() => { });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        private void ws_OnOpen(object sender, EventArgs e)
        {
            if (!(sender is StompWebsocketClient client))
                return;

            try
            {
                if (client.State == StompConnectionState.Open)
                {
                    foreach (var data in Datas.OfType<DataInfo>())
                    {
                        if (data.Name == null)
                            continue;

                        var subscribe = "subscribe";
                        var name = data.Name;
                        var topic = string.Format("/topic/public/{0}/{1}", subscribe, name);
                        client.Subscribe<SubscribeDataInfo>(topic, new Dictionary<string, string>(), 0, GetSubscribeData);
                    }
                }
                Logger.WriteLog(LogTypes.Info, String.Format("Websocket Open : {0}", client.State));
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        private void ws_OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
        }

        private void ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
        }

        private void GetSubscribeData(object sender, SubscribeDataInfo body)
        {
            try
            {
                var data = Datas.OfType<DataInfo>().FirstOrDefault(f => !string.IsNullOrEmpty(f.Name));
                if (data == null)
                    return;

                data.SetSignalData(body.Data);

                var count = body.Data == null ? 0 : body.Data;
                Console.WriteLine($"{DateTime.Now.ToString(Defined.DateLFormat)} - Name : {body.Name}");
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        #endregion //WebSocket
        #endregion //Methods
    }
}

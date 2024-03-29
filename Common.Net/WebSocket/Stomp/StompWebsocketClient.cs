﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Common.Utilities;

using ServiceBase;

using WebSocketSharp;

using static Common.Net.Stomp.StompConnectionState;

using Logger = Common.Utilities.Logger;

namespace Common.Net.Stomp
{
    public class StompWebsocketClient : IStompClient
    {
        // todo: implement this
#pragma warning disable CS0067 // Событие "StompWebsocketClient.OnOpen" никогда не используется.
        public event EventHandler OnOpen;
        //{
        //    add { socket.OnOpen += value; }
        //    remove { socket.OnOpen -= value; }
        //}
#pragma warning restore CS0067 // Событие "StompWebsocketClient.OnOpen" никогда не используется.
#pragma warning disable CS0067 // Событие "StompWebsocketClient.OnClose" никогда не используется.
        public event EventHandler<CloseEventArgs> OnClose;
        //{
        //    add { socket.OnClose += value; }
        //    remove { socket.OnClose -= value; }
        //}
#pragma warning restore CS0067 // Событие "StompWebsocketClient.OnClose" никогда не используется.
#pragma warning disable CS0067 // Событие "StompWebsocketClient.OnError" никогда не используется.
        public event EventHandler<ErrorEventArgs> OnError;
        //{
        //    add { socket.OnError += value; }
        //    remove { socket.OnError -= value; }
        //}
#pragma warning restore CS0067 // Событие "StompWebsocketClient.OnError" никогда не используется.

        private readonly WebSocket socket;
        private readonly Thread HeartBeatThread;
        private readonly StompMessageSerializer stompSerializer = new StompMessageSerializer();

        private readonly IDictionary<string, Subscriber> subscribers = new Dictionary<string, Subscriber>();

        public const int KeepAliveTime = 10000;
        //public const string TokenKey = "SX-Auth-Token";
        public string Token { get; private set; }


        public StompConnectionState State { get; private set; } = Closed;

        public Dictionary<string, Subscriber> Subscribers => (Dictionary<string, Subscriber>)subscribers;



        public StompWebsocketClient(string url)
        {
            socket = new WebSocket(url);
            //socket.OnOpen += Socket_OnOpen;

            HeartBeatThread = new Thread(KeepAlive) {
                IsBackground = true
            };
        }
        public StompWebsocketClient(string url, System.Security.Authentication.SslProtocols sslProtocols)
            : this(url)
        {
            socket.SslConfiguration.EnabledSslProtocols = sslProtocols;
        }




        public void Connect(IDictionary<string, string> headers)
        {
            if (State != Closed)
                throw new InvalidOperationException("The current state of the connection is not Closed.");

            socket.Connect();

            var connectMessage = new StompMessage(StompCommand.Connect, headers);
            var send = stompSerializer.Serialize(connectMessage);

            // todo: check response
            socket.OnMessage += HandleMessage;
            socket.OnClose += Socket_OnClose;
            socket.OnError += Socket_OnError;

            if (headers.ContainsKey(ServiceRequest.SXAuthToken))
                Token = headers[ServiceRequest.SXAuthToken];

            State = Open;

            HeartBeatThread.Start();

            socket.SendAsync(send, new Action<bool>((s) => {
                var aa = s.ToString();
                Socket_OnStatus(this, EventArgs.Empty);
            }));
        }

        public void Send(object body, string destination, IDictionary<string, string> headers)
        {
            if (State != Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            //var jsonPayload = JsonConvert.SerializeObject(body);
            var jsonPayload = SerializeHelper.SerializeByJsonCamel(body);
            headers.Add("destination", destination);
            headers.Add("content-type", "application/json;charset=UTF-8");
            headers.Add("content-length", Encoding.UTF8.GetByteCount(jsonPayload).ToString());
            var connectMessage = new StompMessage(StompCommand.Send, jsonPayload, headers);
            var msg = stompSerializer.Serialize(connectMessage);
            socket.Send(msg);

#if DEBUG
            Logger.WriteLog(LogTypes.Interface, msg.TrimEnd());
#endif
        }

        public void Subscribe<T>(string topic, IDictionary<string, string> headers, int subIndex, EventHandler<T> handler)
        {
            if (State != Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            try
            {
                headers.Add("id", $"sub-{subIndex}"); // todo: study and implement
                headers.Add("destination", topic);
                var subscribeMessage = new StompMessage(StompCommand.Subscribe, headers);
                var msg = stompSerializer.Serialize(subscribeMessage);
#if DEBUG
                Logger.WriteLog(LogTypes.Interface, msg.TrimEnd());
#endif
                socket.Send(msg);

                if (!subscribers.ContainsKey(topic))
                {
                    var sub = new Subscriber((sender, body) => handler(this, (T)body), typeof(T));
                    subscribers.Add(topic, sub);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        public bool UnSubscribe(string code)
        {
            try
            {
                if (subscribers.ContainsKey(code))
                {
                    var sub = subscribers[code];
                    subscribers.Remove(code);
#if DEBUG
                    Logger.WriteLog(LogTypes.Interface, $"UnSubscribe : {code}");
#endif
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (State != Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            State = Closed;
            ((IDisposable)socket).Dispose();
            //Socket_OnStatus(this, EventArgs.Empty);

            // todo: unsubscribe
            subscribers.Clear();

            HeartBeatThread?.Abort();
        }


        private void KeepAlive()
        {
            try
            {
                while (socket != null && State == StompConnectionState.Open)
                {
                    Thread.Sleep(KeepAliveTime);

                    if (socket != null && State == StompConnectionState.Open)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(Token))
                                continue;

                            var headers = new Dictionary<string, string> {
                                { ServiceRequest.SXAuthToken, Token },
                                { "clientKeyName", "DATA_01" },
                                { "connType", 2.ToString() }
                            };
                            var subscribeMessage = new StompMessage(StompCommand.Send, headers);
                            var msg = stompSerializer.Serialize(subscribeMessage);
                            Console.WriteLine("Socket heart-beat : " + msg);
                            socket.Send(msg);
                        }
                        catch (Exception e)
                        {
                            if (e.Message == "You can stop!")
                                return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
#if DEBUG
            //Console.WriteLine("Finally got this bastard.");
            Logger.WriteLog(LogTypes.Interface, "Finally got this bastard.");
#endif
        }

        private void Socket_OnStatus(object sender, EventArgs e)
        {
            OnOpen?.Invoke(this, e);
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke(this, e);
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            OnClose?.Invoke(this, e);
        }


        private void HandleMessage(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = stompSerializer.Deserialize(messageEventArgs.Data);
                if (message == null)
                    return;

                if (message.Command == StompCommand.Message)
                {
                    var sub = subscribers[message.Headers["destination"]];
                    //var body = JsonConvert.DeserializeObject(message.Body, sub.BodyType);
                    //var json = "{"deviceType":1,"serialNumber":"004273","macAddress":"08:D5: C0: 50:10:B1","measurementCode":"2112271732_8F31","bioSignalData":{"ecgDataList":[{"seconds":74981.14,"value":-0.09},{"seconds":74981.144,"value":-0.069},{"seconds":74981.148,"value":-0.049},{"seconds":74981.152,"value":-0.018},{"seconds":74981.156,"value":0.018},{"seconds":74981.16,"value":0.053},{"seconds":74981.163,"value":0.076},{"seconds":74981.167,"value":0.096},{"seconds":74981.171,"value":0.119},{"seconds":74981.175,"value":0.142},{"seconds":74981.179,"value":0.156},{"seconds":74981.183,"value":0.157},{"seconds":74981.187,"value":0.156},{"seconds":74981.191,"value":0.162},{"seconds":74981.195,"value":0.175},{"seconds":74981.199,"value":0.191},{"seconds":74981.203,"value":0.194},{"seconds":74981.206,"value":0.185},{"seconds":74981.21,"value":0.174},{"seconds":74981.214,"value":0.165},{"seconds":74981.218,"value":0.157},{"seconds":74981.222,"value":0.148},{"seconds":74981.226,"value":0.136},{"seconds":74981.23,"value":0.119},{"seconds":74981.234,"value":0.095},{"seconds":74981.238,"value":0.061},{"seconds":74981.242,"value":0.021},{"seconds":74981.245,"value":-0.018},{"seconds":74981.249,"value":-0.049},{"seconds":74981.253,"value":-0.064},{"seconds":74981.257,"value":-0.069},{"seconds":74981.261,"value":-0.079},{"seconds":74981.265,"value":-0.093},{"seconds":74981.269,"value":-0.099},{"seconds":74981.273,"value":-0.093},{"seconds":74981.277,"value":-0.089},{"seconds":74981.281,"value":-0.098},{"seconds":74981.285,"value":-0.111},{"seconds":74981.288,"value":-0.119},{"seconds":74981.292,"value":-0.116},{"seconds":74981.296,"value":-0.111},{"seconds":74981.3,"value":-0.111},{"seconds":74981.304,"value":-0.113},{"seconds":74981.308,"value":-0.116},{"seconds":74981.312,"value":-0.117},{"seconds":74981.316,"value":-0.116},{"seconds":74981.32,"value":-0.114},{"seconds":74981.324,"value":-0.114},{"seconds":74981.328,"value":-0.116},{"seconds":74981.331,"value":-0.117},{"seconds":74981.335,"value":-0.113},{"seconds":74981.339,"value":-0.11},{"seconds":74981.343,"value":-0.11},{"seconds":74981.347,"value":-0.111},{"seconds":74981.351,"value":-0.11},{"seconds":74981.355,"value":-0.107},{"seconds":74981.359,"value":-0.107},{"seconds":74981.363,"value":-0.111},{"seconds":74981.367,"value":-0.114},{"seconds":74981.37,"value":-0.113},{"seconds":74981.374,"value":-0.108},{"seconds":74981.378,"value":-0.108},{"seconds":74981.382,"value":-0.111},{"seconds":74981.386,"value":-0.114},{"seconds":74981.39,"value":-0.113},{"seconds":74981.394,"value":-0.11},{"seconds":74981.398,"value":-0.108},{"seconds":74981.402,"value":-0.108},{"seconds":74981.406,"value":-0.11},{"seconds":74981.41,"value":-0.113},{"seconds":74981.413,"value":-0.114},{"seconds":74981.417,"value":-0.113},{"seconds":74981.421,"value":-0.111},{"seconds":74981.425,"value":-0.11},{"seconds":74981.429,"value":-0.11},{"seconds":74981.433,"value":-0.108},{"seconds":74981.437,"value":-0.104},{"seconds":74981.441,"value":-0.102},{"seconds":74981.445,"value":-0.107},{"seconds":74981.449,"value":-0.111},{"seconds":74981.453,"value":-0.113},{"seconds":74981.456,"value":-0.11},{"seconds":74981.46,"value":-0.107},{"seconds":74981.464,"value":-0.107},{"seconds":74981.468,"value":-0.108},{"seconds":74981.472,"value":-0.107},{"seconds":74981.476,"value":-0.105},{"seconds":74981.48,"value":-0.105},{"seconds":74981.484,"value":-0.108},{"seconds":74981.488,"value":-0.108},{"seconds":74981.492,"value":-0.107},{"seconds":74981.495,"value":-0.104},{"seconds":74981.499,"value":-0.105},{"seconds":74981.503,"value":-0.108},{"seconds":74981.507,"value":-0.11},{"seconds":74981.511,"value":-0.111},{"seconds":74981.515,"value":-0.11},{"seconds":74981.519,"value":-0.105},{"seconds":74981.523,"value":-0.104},{"seconds":74981.527,"value":-0.107},{"seconds":74981.531,"value":-0.108},{"seconds":74981.535,"value":-0.11},{"seconds":74981.538,"value":-0.108},{"seconds":74981.542,"value":-0.108},{"seconds":74981.546,"value":-0.11},{"seconds":74981.55,"value":-0.108},{"seconds":74981.554,"value":-0.105},{"seconds":74981.558,"value":-0.104},{"seconds":74981.562,"value":-0.107},{"seconds":74981.566,"value":-0.11},{"seconds":74981.57,"value":-0.11},{"seconds":74981.574,"value":-0.107},{"seconds":74981.578,"value":-0.104},{"seconds":74981.581,"value":-0.104},{"seconds":74981.585,"value":-0.107},{"seconds":74981.589,"value":-0.108},{"seconds":74981.593,"value":-0.107},{"seconds":74981.597,"value":-0.104},{"seconds":74981.601,"value":-0.104},{"seconds":74981.605,"value":-0.105},{"seconds":74981.609,"value":-0.107},{"seconds":74981.613,"value":-0.107},{"seconds":74981.617,"value":-0.105},{"seconds":74981.62,"value":-0.102},{"seconds":74981.624,"value":-0.102},{"seconds":74981.628,"value":-0.104},{"seconds":74981.632,"value":-0.107},{"seconds":74981.636,"value":-0.105},{"seconds":74981.64,"value":-0.104},{"seconds":74981.644,"value":-0.104},{"seconds":74981.648,"value":-0.104},{"seconds":74981.652,"value":-0.104},{"seconds":74981.656,"value":-0.102},{"seconds":74981.66,"value":-0.102},{"seconds":74981.663,"value":-0.105},{"seconds":74981.667,"value":-0.105},{"seconds":74981.671,"value":-0.104},{"seconds":74981.675,"value":-0.101},{"seconds":74981.679,"value":-0.102},{"seconds":74981.683,"value":-0.107},{"seconds":74981.687,"value":-0.11},{"seconds":74981.691,"value":-0.107},{"seconds":74981.695,"value":-0.104},{"seconds":74981.699,"value":-0.102},{"seconds":74981.703,"value":-0.102},{"seconds":74981.706,"value":-0.104},{"seconds":74981.71,"value":-0.104},{"seconds":74981.714,"value":-0.101},{"seconds":74981.718,"value":-0.087},{"seconds":74981.722,"value":-0.069},{"seconds":74981.726,"value":-0.047},{"seconds":74981.73,"value":-0.031},{"seconds":74981.734,"value":-0.015},{"seconds":74981.738,"value":-0.005},{"seconds":74981.742,"value":0.002},{"seconds":74981.745,"value":0.008},{"seconds":74981.749,"value":0.017},{"seconds":74981.753,"value":0.026},{"seconds":74981.757,"value":0.032},{"seconds":74981.761,"value":0.034},{"seconds":74981.765,"value":0.027},{"seconds":74981.769,"value":0.017},{"seconds":74981.773,"value":0.006},{"seconds":74981.777,"value":-0.002},{"seconds":74981.781,"value":-0.008},{"seconds":74981.785,"value":-0.015},{"seconds":74981.788,"value":-0.023},{"seconds":74981.792,"value":-0.034},{"seconds":74981.796,"value":-0.049},{"seconds":74981.8,"value":-0.07},{"seconds":74981.804,"value":-0.093},{"seconds":74981.808,"value":-0.105},{"seconds":74981.812,"value":-0.107},{"seconds":74981.816,"value":-0.104},{"seconds":74981.82,"value":-0.102},{"seconds":74981.824,"value":-0.102},{"seconds":74981.828,"value":-0.105},{"seconds":74981.831,"value":-0.105},{"seconds":74981.835,"value":-0.104},{"seconds":74981.839,"value":-0.101},{"seconds":74981.843,"value":-0.099},{"seconds":74981.847,"value":-0.101},{"seconds":74981.851,"value":-0.102},{"seconds":74981.855,"value":-0.102},{"seconds":74981.859,"value":-0.099},{"seconds":74981.863,"value":-0.099},{"seconds":74981.867,"value":-0.099},{"seconds":74981.87,"value":-0.101},{"seconds":74981.874,"value":-0.099},{"seconds":74981.878,"value":-0.098},{"seconds":74981.882,"value":-0.095},{"seconds":74981.886,"value":-0.093},{"seconds":74981.89,"value":-0.104},{"seconds":74981.894,"value":-0.136},{"seconds":74981.898,"value":-0.18},{"seconds":74981.902,"value":-0.191},{"seconds":74981.906,"value":-0.13},{"seconds":74981.91,"value":0.002},{"seconds":74981.913,"value":0.162},{"seconds":74981.917,"value":0.319},{"seconds":74981.921,"value":0.468},{"seconds":74981.925,"value":0.63},{"seconds":74981.929,"value":0.804},{"seconds":74981.933,"value":0.983},{"seconds":74981.937,"value":1.158},{"seconds":74981.941,"value":1.331},{"seconds":74981.945,"value":1.494},{"seconds":74981.949,"value":1.613},{"seconds":74981.953,"value":1.651},{"seconds":74981.956,"value":1.604},{"seconds":74981.96,"value":1.486},{"seconds":74981.964,"value":1.262},{"seconds":74981.968,"value":0.867},{"seconds":74981.972,"value":0.327},{"seconds":74981.976,"value":-0.172},{"seconds":74981.98,"value":-0.452},{"seconds":74981.984,"value":-0.49},{"seconds":74981.988,"value":-0.404},{"seconds":74981.992,"value":-0.319},{"seconds":74981.995,"value":-0.276},{"seconds":74981.999,"value":-0.27},{"seconds":74982.003,"value":-0.276},{"seconds":74982.007,"value":-0.276},{"seconds":74982.011,"value":-0.267},{"seconds":74982.015,"value":-0.255},{"seconds":74982.019,"value":-0.249},{"seconds":74982.023,"value":-0.25},{"seconds":74982.027,"value":-0.259},{"seconds":74982.031,"value":-0.264},{"seconds":74982.035,"value":-0.256},{"seconds":74982.038,"value":-0.246},{"seconds":74982.042,"value":-0.247},{"seconds":74982.046,"value":-0.261},{"seconds":74982.05,"value":-0.267},{"seconds":74982.054,"value":-0.256},{"seconds":74982.058,"value":-0.243},{"seconds":74982.062,"value":-0.246},{"seconds":74982.066,"value":-0.259},{"seconds":74982.07,"value":-0.264},{"seconds":74982.074,"value":-0.253},{"seconds":74982.078,"value":-0.246},{"seconds":74982.081,"value":-0.252},{"seconds":74982.085,"value":-0.261},{"seconds":74982.089,"value":-0.259},{"seconds":74982.093,"value":-0.246},{"seconds":74982.097,"value":-0.23},{"seconds":74982.101,"value":-0.224},{"seconds":74982.105,"value":-0.22},{"seconds":74982.109,"value":-0.215},{"seconds":74982.113,"value":-0.204},{"seconds":74982.117,"value":-0.192},{"seconds":74982.12,"value":-0.183},{"seconds":74982.124,"value":-0.175},{"seconds":74982.128,"value":-0.166},{"seconds":74982.132,"value":-0.148},{"seconds":74982.136,"value":-0.122},{"seconds":74982.14,"value":-0.096},{"seconds":74982.144,"value":-0.073},{"seconds":74982.148,"value":-0.049},{"seconds":74982.152,"value":-0.017},{"seconds":74982.156,"value":0.021},{"seconds":74982.16,"value":0.056},{"seconds":74982.163,"value":0.078},{"seconds":74982.167,"value":0.096},{"seconds":74982.171,"value":0.117},{"seconds":74982.175,"value":0.14},{"seconds":74982.179,"value":0.153},{"seconds":74982.183,"value":0.154},{"seconds":74982.187,"value":0.154},{"seconds":74982.191,"value":0.162},{"seconds":74982.195,"value":0.177},{"seconds":74982.199,"value":0.192},{"seconds":74982.203,"value":0.197},{"seconds":74982.206,"value":0.189},{"seconds":74982.21,"value":0.179},{"seconds":74982.214,"value":0.169},{"seconds":74982.218,"value":0.16},{"seconds":74982.222,"value":0.148},{"seconds":74982.226,"value":0.134},{"seconds":74982.23,"value":0.117},{"seconds":74982.234,"value":0.093},{"seconds":74982.238,"value":0.06},{"seconds":74982.242,"value":0.018},{"seconds":74982.245,"value":-0.021},{"seconds":74982.249,"value":-0.053},{"seconds":74982.253,"value":-0.067},{"seconds":74982.257,"value":-0.072},{"seconds":74982.261,"value":-0.081},{"seconds":74982.265,"value":-0.095},{"seconds":74982.269,"value":-0.101},{"seconds":74982.273,"value":-0.096},{"seconds":74982.277,"value":-0.092},{"seconds":74982.281,"value":-0.098},{"seconds":74982.285,"value":-0.108},{"seconds":74982.288,"value":-0.114},{"seconds":74982.292,"value":-0.114},{"seconds":74982.296,"value":-0.113},{"seconds":74982.3,"value":-0.113},{"seconds":74982.304,"value":-0.114},{"seconds":74982.308,"value":-0.116},{"seconds":74982.312,"value":-0.114},{"seconds":74982.316,"value":-0.113},{"seconds":74982.32,"value":-0.111},{"seconds":74982.324,"value":-0.113},{"seconds":74982.328,"value":-0.116},{"seconds":74982.331,"value":-0.116},{"seconds":74982.335,"value":-0.111},{"seconds":74982.339,"value":-0.107},{"seconds":74982.343,"value":-0.11},{"seconds":74982.347,"value":-0.116},{"seconds":74982.351,"value":-0.119},{"seconds":74982.355,"value":-0.114},{"seconds":74982.359,"value":-0.11},{"seconds":74982.363,"value":-0.111},{"seconds":74982.367,"value":-0.113},{"seconds":74982.37,"value":-0.113},{"seconds":74982.374,"value":-0.11},{"seconds":74982.378,"value":-0.11},{"seconds":74982.382,"value":-0.113},{"seconds":74982.386,"value":-0.116},{"seconds":74982.39,"value":-0.114},{"seconds":74982.394,"value":-0.11},{"seconds":74982.398,"value":-0.107},{"seconds":74982.402,"value":-0.108},{"seconds":74982.406,"value":-0.111},{"seconds":74982.41,"value":-0.111},{"seconds":74982.413,"value":-0.11},{"seconds":74982.417,"value":-0.108},{"seconds":74982.421,"value":-0.108},{"seconds":74982.425,"value":-0.11},{"seconds":74982.429,"value":-0.114},{"seconds":74982.433,"value":-0.114},{"seconds":74982.437,"value":-0.11},{"seconds":74982.441,"value":-0.107},{"seconds":74982.445,"value":-0.11},{"seconds":74982.449,"value":-0.114},{"seconds":74982.453,"value":-0.113},{"seconds":74982.456,"value":-0.108},{"seconds":74982.46,"value":-0.105},{"seconds":74982.464,"value":-0.108},{"seconds":74982.468,"value":-0.111},{"seconds":74982.472,"value":-0.111},{"seconds":74982.476,"value":-0.108},{"seconds":74982.48,"value":-0.108},{"seconds":74982.484,"value":-0.11},{"seconds":74982.488,"value":-0.111},{"seconds":74982.492,"value":-0.11},{"seconds":74982.495,"value":-0.108},{"seconds":74982.499,"value":-0.108},{"seconds":74982.503,"value":-0.107},{"seconds":74982.507,"value":-0.105},{"seconds":74982.511,"value":-0.104},{"seconds":74982.515,"value":-0.104},{"seconds":74982.519,"value":-0.104},{"seconds":74982.523,"value":-0.107},{"seconds":74982.527,"value":-0.11},{"seconds":74982.531,"value":-0.113},{"seconds":74982.535,"value":-0.111},{"seconds":74982.538,"value":-0.108},{"seconds":74982.542,"value":-0.107},{"seconds":74982.546,"value":-0.108},{"seconds":74982.55,"value":-0.108},{"seconds":74982.554,"value":-0.105},{"seconds":74982.558,"value":-0.102},{"seconds":74982.562,"value":-0.104},{"seconds":74982.566,"value":-0.108},{"seconds":74982.57,"value":-0.11},{"seconds":74982.574,"value":-0.11},{"seconds":74982.578,"value":-0.108},{"seconds":74982.581,"value":-0.11},{"seconds":74982.585,"value":-0.11},{"seconds":74982.589,"value":-0.108},{"seconds":74982.593,"value":-0.107},{"seconds":74982.597,"value":-0.107},{"seconds":74982.601,"value":-0.107},{"seconds":74982.605,"value":-0.107},{"seconds":74982.609,"value":-0.107},{"seconds":74982.613,"value":-0.105},{"seconds":74982.617,"value":-0.105},{"seconds":74982.62,"value":-0.105},{"seconds":74982.624,"value":-0.105},{"seconds":74982.628,"value":-0.105},{"seconds":74982.632,"value":-0.105},{"seconds":74982.636,"value":-0.104},{"seconds":74982.64,"value":-0.104},{"seconds":74982.644,"value":-0.105},{"seconds":74982.648,"value":-0.107},{"seconds":74982.652,"value":-0.105},{"seconds":74982.656,"value":-0.105},{"seconds":74982.66,"value":-0.104},{"seconds":74982.663,"value":-0.105},{"seconds":74982.667,"value":-0.105},{"seconds":74982.671,"value":-0.105},{"seconds":74982.675,"value":-0.105},{"seconds":74982.679,"value":-0.105},{"seconds":74982.683,"value":-0.107},{"seconds":74982.687,"value":-0.107},{"seconds":74982.691,"value":-0.105},{"seconds":74982.695,"value":-0.102},{"seconds":74982.699,"value":-0.102},{"seconds":74982.703,"value":-0.102},{"seconds":74982.706,"value":-0.104},{"seconds":74982.71,"value":-0.104},{"seconds":74982.714,"value":-0.098},{"seconds":74982.718,"value":-0.084},{"seconds":74982.722,"value":-0.066},{"seconds":74982.726,"value":-0.049},{"seconds":74982.73,"value":-0.035},{"seconds":74982.734,"value":-0.023},{"seconds":74982.738,"value":-0.009},{"seconds":74982.742,"value":0.0},{"seconds":74982.745,"value":0.009},{"seconds":74982.749,"value":0.018},{"seconds":74982.753,"value":0.026},{"seconds":74982.757,"value":0.031},{"seconds":74982.761,"value":0.032},{"seconds":74982.765,"value":0.027},{"seconds":74982.769,"value":0.017},{"seconds":74982.773,"value":0.005},{"seconds":74982.777,"value":-0.003},{"seconds":74982.781,"value":-0.011},{"seconds":74982.785,"value":-0.018},{"seconds":74982.788,"value":-0.027},{"seconds":74982.792,"value":-0.038},{"seconds":74982.796,"value":-0.053},{"seconds":74982.8,"value":-0.073},{"seconds":74982.804,"value":-0.093},{"seconds":74982.808,"value":-0.105},{"seconds":74982.812,"value":-0.108},{"seconds":74982.816,"value":-0.107},{"seconds":74982.82,"value":-0.104},{"seconds":74982.824,"value":-0.102},{"seconds":74982.828,"value":-0.104},{"seconds":74982.831,"value":-0.105},{"seconds":74982.835,"value":-0.107},{"seconds":74982.839,"value":-0.105},{"seconds":74982.843,"value":-0.104},{"seconds":74982.847,"value":-0.104},{"seconds":74982.851,"value":-0.104},{"seconds":74982.855,"value":-0.101},{"seconds":74982.859,"value":-0.099},{"seconds":74982.863,"value":-0.101},{"seconds":74982.867,"value":-0.102},{"seconds":74982.87,"value":-0.102},{"seconds":74982.874,"value":-0.099},{"seconds":74982.878,"value":-0.098},{"seconds":74982.882,"value":-0.098},{"seconds":74982.886,"value":-0.098},{"seconds":74982.89,"value":-0.107},{"seconds":74982.894,"value":-0.136},{"seconds":74982.898,"value":-0.177},{"seconds":74982.902,"value":-0.189},{"seconds":74982.906,"value":-0.131},{"seconds":74982.91,"value":-0.002},{"seconds":74982.913,"value":0.16},{"seconds":74982.917,"value":0.317},{"seconds":74982.921,"value":0.468},{"seconds":74982.925,"value":0.629},{"seconds":74982.929,"value":0.803},{"seconds":74982.933,"value":0.981},{"seconds":74982.937,"value":1.155},{"seconds":74982.941,"value":1.328},{"seconds":74982.945,"value":1.489},{"seconds":74982.949,"value":1.613},{"seconds":74982.953,"value":1.656},{"seconds":74982.956,"value":1.613},{"seconds":74982.96,"value":1.492},{"seconds":74982.964,"value":1.265},{"seconds":74982.968,"value":0.868},{"seconds":74982.972,"value":0.331},{"seconds":74982.976,"value":-0.166},{"seconds":74982.98,"value":-0.446},{"seconds":74982.984,"value":-0.487},{"seconds":74982.988,"value":-0.406},{"seconds":74982.992,"value":-0.319},{"seconds":74982.995,"value":-0.275},{"seconds":74982.999,"value":-0.267},{"seconds":74983.003,"value":-0.275},{"seconds":74983.007,"value":-0.278},{"seconds":74983.011,"value":-0.269},{"seconds":74983.015,"value":-0.255},{"seconds":74983.019,"value":-0.244},{"seconds":74983.023,"value":-0.246},{"seconds":74983.027,"value":-0.256},{"seconds":74983.031,"value":-0.264},{"seconds":74983.035,"value":-0.256},{"seconds":74983.038,"value":-0.244},{"seconds":74983.042,"value":-0.244},{"seconds":74983.046,"value":-0.258},{"seconds":74983.05,"value":-0.266},{"seconds":74983.054,"value":-0.255},{"seconds":74983.058,"value":-0.243},{"seconds":74983.062,"value":-0.244},{"seconds":74983.066,"value":-0.256},{"seconds":74983.07,"value":-0.259},{"seconds":74983.074,"value":-0.249},{"seconds":74983.078,"value":-0.241},{"seconds":74983.081,"value":-0.247},{"seconds":74983.085,"value":-0.256},{"seconds":74983.089,"value":-0.253},{"seconds":74983.093,"value":-0.24},{"seconds":74983.097,"value":-0.227},{"seconds":74983.101,"value":-0.22},{"seconds":74983.105,"value":-0.218},{"seconds":74983.109,"value":-0.214},{"seconds":74983.113,"value":-0.206},{"seconds":74983.117,"value":-0.194},{"seconds":74983.12,"value":-0.182},{"seconds":74983.124,"value":-0.172},{"seconds":74983.128,"value":-0.162},{"seconds":74983.132,"value":-0.143},{"seconds":74983.136,"value":-0.119},{"seconds":74983.14,"value":-0.092},{"seconds":74983.144,"value":-0.069},{"seconds":74983.148,"value":-0.047},{"seconds":74983.152,"value":-0.018},{"seconds":74983.156,"value":0.018},{"seconds":74983.16,"value":0.053},{"seconds":74983.163,"value":0.079},{"seconds":74983.167,"value":0.101},{"seconds":74983.171,"value":0.124},{"seconds":74983.175,"value":0.143},{"seconds":74983.179,"value":0.154},{"seconds":74983.183,"value":0.153},{"seconds":74983.187,"value":0.151},{"seconds":74983.191,"value":0.159},{"seconds":74983.195,"value":0.175},{"seconds":74983.199,"value":0.191},{"seconds":74983.203,"value":0.197},{"seconds":74983.206,"value":0.189},{"seconds":74983.21,"value":0.177},{"seconds":74983.214,"value":0.168},{"seconds":74983.218,"value":0.16},{"seconds":74983.222,"value":0.15},{"seconds":74983.226,"value":0.136},{"seconds":74983.23,"value":0.117},{"seconds":74983.234,"value":0.092},{"seconds":74983.238,"value":0.058},{"seconds":74983.242,"value":0.017},{"seconds":74983.245,"value":-0.023},{"seconds":74983.249,"value":-0.052},{"seconds":74983.253,"value":-0.066},{"seconds":74983.257,"value":-0.07},{"seconds":74983.261,"value":-0.079},{"seconds":74983.265,"value":-0.09},{"seconds":74983.269,"value":-0.096},{"seconds":74983.273,"value":-0.09},{"seconds":74983.277,"value":-0.087},{"seconds":74983.281,"value":-0.096},{"seconds":74983.285,"value":-0.111},{"seconds":74983.288,"value":-0.121},{"seconds":74983.292,"value":-0.117},{"seconds":74983.296,"value":-0.111},{"seconds":74983.3,"value":-0.108},{"seconds":74983.304,"value":-0.111},{"seconds":74983.308,"value":-0.114},{"seconds":74983.312,"value":-0.114},{"seconds":74983.316,"value":-0.11},{"seconds":74983.32,"value":-0.105},{"seconds":74983.324,"value":-0.105},{"seconds":74983.328,"value":-0.111},{"seconds":74983.331,"value":-0.116},{"seconds":74983.335,"value":-0.114},{"seconds":74983.339,"value":-0.108},{"seconds":74983.343,"value":-0.107},{"seconds":74983.347,"value":-0.113},{"seconds":74983.351,"value":-0.114},{"seconds":74983.355,"value":-0.11},{"seconds":74983.359,"value":-0.105},{"seconds":74983.363,"value":-0.107},{"seconds":74983.367,"value":-0.111},{"seconds":74983.37,"value":-0.113},{"seconds":74983.374,"value":-0.11},{"seconds":74983.378,"value":-0.108},{"seconds":74983.382,"value":-0.111},{"seconds":74983.386,"value":-0.113},{"seconds":74983.39,"value":-0.111},{"seconds":74983.394,"value":-0.11},{"seconds":74983.398,"value":-0.11},{"seconds":74983.402,"value":-0.113},{"seconds":74983.406,"value":-0.113},{"seconds":74983.41,"value":-0.113},{"seconds":74983.413,"value":-0.111},{"seconds":74983.417,"value":-0.108},{"seconds":74983.421,"value":-0.107},{"seconds":74983.425,"value":-0.107},{"seconds":74983.429,"value":-0.108},{"seconds":74983.433,"value":-0.11},{"seconds":74983.437,"value":-0.108},{"seconds":74983.441,"value":-0.107},{"seconds":74983.445,"value":-0.108},{"seconds":74983.449,"value":-0.111},{"seconds":74983.453,"value":-0.111},{"seconds":74983.456,"value":-0.108},{"seconds":74983.46,"value":-0.108},{"seconds":74983.464,"value":-0.111},{"seconds":74983.468,"value":-0.114},{"seconds":74983.472,"value":-0.111},{"seconds":74983.476,"value":-0.105},{"seconds":74983.48,"value":-0.102},{"seconds":74983.484,"value":-0.105},{"seconds":74983.488,"value":-0.108},{"seconds":74983.492,"value":-0.108},{"seconds":74983.495,"value":-0.108},{"seconds":74983.499,"value":-0.108},{"seconds":74983.503,"value":-0.11},{"seconds":74983.507,"value":-0.11},{"seconds":74983.511,"value":-0.108},{"seconds":74983.515,"value":-0.105},{"seconds":74983.519,"value":-0.104},{"seconds":74983.523,"value":-0.104},{"seconds":74983.527,"value":-0.108},{"seconds":74983.531,"value":-0.111},{"seconds":74983.535,"value":-0.111},{"seconds":74983.538,"value":-0.107},{"seconds":74983.542,"value":-0.105},{"seconds":74983.546,"value":-0.107},{"seconds":74983.55,"value":-0.11},{"seconds":74983.554,"value":-0.108},{"seconds":74983.558,"value":-0.104},{"seconds":74983.562,"value":-0.102},{"seconds":74983.566,"value":-0.105},{"seconds":74983.57,"value":-0.107},{"seconds":74983.574,"value":-0.105},{"seconds":74983.578,"value":-0.104},{"seconds":74983.581,"value":-0.105},{"seconds":74983.585,"value":-0.107},{"seconds":74983.589,"value":-0.108},{"seconds":74983.593,"value":-0.107},{"seconds":74983.597,"value":-0.105},{"seconds":74983.601,"value":-0.105},{"seconds":74983.605,"value":-0.107},{"seconds":74983.609,"value":-0.107},{"seconds":74983.613,"value":-0.104},{"seconds":74983.617,"value":-0.102},{"seconds":74983.62,"value":-0.102},{"seconds":74983.624,"value":-0.105},{"seconds":74983.628,"value":-0.11},{"seconds":74983.632,"value":-0.111},{"seconds":74983.636,"value":-0.11},{"seconds":74983.64,"value":-0.107},{"seconds":74983.644,"value":-0.105},{"seconds":74983.648,"value":-0.105},{"seconds":74983.652,"value":-0.104},{"seconds":74983.656,"value":-0.102},{"seconds":74983.66,"value":-0.104},{"seconds":74983.663,"value":-0.107},{"seconds":74983.667,"value":-0.108},{"seconds":74983.671,"value":-0.107},{"seconds":74983.675,"value":-0.105},{"seconds":74983.679,"value":-0.104},{"seconds":74983.683,"value":-0.107},{"seconds":74983.687,"value":-0.107},{"seconds":74983.691,"value":-0.105},{"seconds":74983.695,"value":-0.102},{"seconds":74983.699,"value":-0.101},{"seconds":74983.703,"value":-0.104},{"seconds":74983.706,"value":-0.105},{"seconds":74983.71,"value":-0.104},{"seconds":74983.714,"value":-0.098},{"seconds":74983.718,"value":-0.084},{"seconds":74983.722,"value":-0.066},{"seconds":74983.726,"value":-0.046},{"seconds":74983.73,"value":-0.031},{"seconds":74983.734,"value":-0.018},{"seconds":74983.738,"value":-0.008},{"seconds":74983.742,"value":0.0},{"seconds":74983.745,"value":0.006},{"seconds":74983.749,"value":0.017},{"seconds":74983.753,"value":0.026},{"seconds":74983.757,"value":0.031},{"seconds":74983.761,"value":0.032},{"seconds":74983.765,"value":0.027},{"seconds":74983.769,"value":0.018},{"seconds":74983.773,"value":0.006},{"seconds":74983.777,"value":-0.002},{"seconds":74983.781,"value":-0.008},{"seconds":74983.785,"value":-0.014},{"seconds":74983.788,"value":-0.021},{"seconds":74983.792,"value":-0.035},{"seconds":74983.796,"value":-0.053},{"seconds":74983.8,"value":-0.075},{"seconds":74983.804,"value":-0.096},{"seconds":74983.808,"value":-0.107},{"seconds":74983.812,"value":-0.107},{"seconds":74983.816,"value":-0.101},{"seconds":74983.82,"value":-0.099},{"seconds":74983.824,"value":-0.101},{"seconds":74983.828,"value":-0.104},{"seconds":74983.831,"value":-0.104},{"seconds":74983.835,"value":-0.102},{"seconds":74983.839,"value":-0.102},{"seconds":74983.843,"value":-0.102},{"seconds":74983.847,"value":-0.102},{"seconds":74983.851,"value":-0.101},{"seconds":74983.855,"value":-0.099},{"seconds":74983.859,"value":-0.099},{"seconds":74983.863,"value":-0.101},{"seconds":74983.867,"value":-0.102},{"seconds":74983.87,"value":-0.102},{"seconds":74983.874,"value":-0.104},{"seconds":74983.878,"value":-0.101},{"seconds":74983.882,"value":-0.098},{"seconds":74983.886,"value":-0.096},{"seconds":74983.89,"value":-0.107},{"seconds":74983.894,"value":-0.14},{"seconds":74983.898,"value":-0.182},{"seconds":74983.902,"value":-0.194},{"seconds":74983.906,"value":-0.133},{"seconds":74983.91,"value":-0.002},{"seconds":74983.913,"value":0.16},{"seconds":74983.917,"value":0.319},{"seconds":74983.921,"value":0.47},{"seconds":74983.925,"value":0.632},{"seconds":74983.929,"value":0.806},{"seconds":74983.933,"value":0.981},{"seconds":74983.937,"value":1.155},{"seconds":74983.941,"value":1.328},{"seconds":74983.945,"value":1.488},{"seconds":74983.949,"value":1.607},{"seconds":74983.953,"value":1.646},{"seconds":74983.956,"value":1.604},{"seconds":74983.96,"value":1.489},{"seconds":74983.964,"value":1.268},{"seconds":74983.968,"value":0.873},{"seconds":74983.972,"value":0.336},{"seconds":74983.976,"value":-0.165},{"seconds":74983.98,"value":-0.449},{"seconds":74983.984,"value":-0.491},{"seconds":74983.988,"value":-0.41},{"seconds":74983.992,"value":-0.323},{"seconds":74983.995,"value":-0.278},{"seconds":74983.999,"value":-0.272},{"seconds":74984.003,"value":-0.278},{"seconds":74984.007,"value":-0.278},{"seconds":74984.011,"value":-0.269},{"seconds":74984.015,"value":-0.255},{"seconds":74984.019,"value":-0.247},{"seconds":74984.023,"value":-0.25},{"seconds":74984.027,"value":-0.259},{"seconds":74984.031,"value":-0.262},{"seconds":74984.035,"value":-0.255},{"seconds":74984.038,"value":-0.244},{"seconds":74984.042,"value":-0.246},{"seconds":74984.046,"value":-0.259},{"seconds":74984.05,"value":-0.266},{"seconds":74984.054,"value":-0.255},{"seconds":74984.058,"value":-0.243},{"seconds":74984.062,"value":-0.247},{"seconds":74984.066,"value":-0.262},{"seconds":74984.07,"value":-0.267},{"seconds":74984.074,"value":-0.255},{"seconds":74984.078,"value":-0.243},{"seconds":74984.081,"value":-0.247},{"seconds":74984.085,"value":-0.255},{"seconds":74984.089,"value":-0.252},{"seconds":74984.093,"value":-0.238},{"seconds":74984.097,"value":-0.226},{"seconds":74984.101,"value":-0.22},{"seconds":74984.105,"value":-0.218},{"seconds":74984.109,"value":-0.214},{"seconds":74984.113,"value":-0.204},{"seconds":74984.117,"value":-0.194},{"seconds":74984.12,"value":-0.185},{"seconds":74984.124,"value":-0.177},{"seconds":74984.128,"value":-0.163},{"seconds":74984.132,"value":-0.142},{"seconds":74984.136,"value":-0.116}],"activityLevelDataList":null,"heartRateDataList":null,"ewsDataList":null,"respDataList":null,"analysisArrhythmiaList":null,"tempDataList":null,"spO2DataList":null},"batteryValue":85,"deviceStatusInfo":null,"dateTime":"2021 - 12 - 28 14:23:06"}";
                    var body = SerializeHelper.DeserializeByJsonCamel(message.Body, sub.BodyType);
                    if (body != null)
                        sub.Handler(sender, body);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, messageEventArgs.Data, ex);
            }
        }
    }
}

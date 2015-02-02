using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperWebSocket.PubSub;
using WebSocket4Net;
using SuperWebSocket.PubSubProtocol;

namespace SuperWebSocket.PubSub
{
    public class PubSubWebSocketClient
    {
        public WebSocket Websoket {get;set;}
        protected AutoResetEvent OpenedEvent = new AutoResetEvent(false);
        public PubSubWebSocketClient(string ip, bool autoConnect)
        {
          
            this.Websoket = new WebSocket(ip, "basic", WebSocketVersion.Rfc6455);
            this.Websoket.Opened += Websoket_Opened;

            if (autoConnect)
            {
                this.Websoket.Open();

                if (!OpenedEvent.WaitOne(7000))
                    throw new Exception("Cannot Connect to websocket server");
            }
            

        }


        public void Publish(string topic, string msg, List<Constrain> constrains,bool closeConn)
        {

            if (this.Websoket.State != WebSocketState.Open)
            {
                this.Websoket.Open();
                if (!OpenedEvent.WaitOne(7000))
                    throw new Exception("Cannot Connect to websocket server");
            }
              

            
            ClientRequest request = new ClientRequest();
            request.RequestType = RequestType.PUBLISH;
            request.Topic = topic;
            request.Msg = msg;
            request.Constrains = constrains;

            string requestString = JsonConvert.SerializeObject(request);

            this.Websoket.Send(requestString);

            if (closeConn && this.Websoket.State == WebSocketState.Open)
                this.Websoket.Close();

        }

        public void Publish(string topic, string msg,bool closeConn)
        {

            if (this.Websoket.State != WebSocketState.Open)
            {
                this.Websoket.Open();
                if (!OpenedEvent.WaitOne(7000))
                    throw new Exception("Cannot Connect to websocket server");
            }



            ClientRequest request = new ClientRequest();
            request.RequestType = RequestType.PUBLISH;
            request.Topic = topic;
            request.Msg = msg;
            request.Constrains = null;

            string requestString = JsonConvert.SerializeObject(request);

            this.Websoket.Send(requestString);


            if (closeConn && this.Websoket.State == WebSocketState.Open)
                this.Websoket.Close();

        }





        void Websoket_Opened(object sender, EventArgs e)
        {
            OpenedEvent.Set();
        }


    }
}

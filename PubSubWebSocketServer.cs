using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using SuperSocket.SocketBase;

namespace SuperWebSocket.PubSubProtocol
{
    public class PubSubWebSocketServer : WebSocketServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<string, List<SubscriberConstrains>> Subscriptions { get; set; }
     
        public PubSubWebSocketServer()
        {
       
            this.Subscriptions = new Dictionary<string, List<SubscriberConstrains>>();
            this.SessionClosed += CPWebSocketServer_SessionClosed;
            this.NewMessageReceived += CPWebSocketServer_NewMessageReceived;
               
        }


        public void SetSupportedTopics(List<string> topics)
        {
            foreach (var topic in topics)
            {
                this.Subscriptions.Add(topic, new List<SubscriberConstrains>());

                log.Info("Start listening for topic = " + topic);

            }

        }

        void CPWebSocketServer_NewMessageReceived(WebSocketSession session, string value)
        {

            var clientRequest = ClientRequest.TryParse(value);

            log.Info("Request Received = " + value);

            if (clientRequest == null)
            {
                session.Send("Request not valid");
                return;
            }

            switch (clientRequest.RequestType.ToUpper())
            {
                case RequestType.SUBSCRIBE:

                    bool subscribed = this.SubscribedToTopic(clientRequest, session.SessionID);

                    if (subscribed)
                        session.Send("Successfully Subscribed to " + clientRequest.Topic);
                    else
                        session.Send("Error: Topic " + clientRequest.Topic + " Not Supported ");

                    break;


                case RequestType.UNSUBSCRIBE:

                    bool unsubscribed = this.UnsubribeFromTopic(clientRequest, session.SessionID);

                    if (unsubscribed)
                        session.Send("Successfully UnSubscribed from " + clientRequest.Topic);
                    else
                        session.Send("Error: UnSubscribed from topic " + clientRequest.Topic + " faild.");

                    break;

                case RequestType.PUBLISH:

                    this.Publich(clientRequest, value);

                    break;

            }


        }


        void CPWebSocketServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {

            try
            {
                foreach (var subscriptions in this.Subscriptions)
                {
                    var subscribers = (List<SubscriberConstrains>)subscriptions.Value;
                    subscribers.RemoveAll(item => item.SubsciberId == session.SessionID);
                }
            }
            catch (Exception ex)
            {

                log.Error("Error on SessionClosed ", ex);
            }
            //remove subscriber resources

        }


        public bool SubscribedToTopic(ClientRequest clientRequest, string clientId)
        {


            try
            {
                if (this.Subscriptions.ContainsKey(clientRequest.Topic))
                {
                    var subscribers = (List<SubscriberConstrains>)this.Subscriptions[clientRequest.Topic];
                    subscribers.Add(new SubscriberConstrains(clientId, clientRequest.Constrains));

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {

                log.Error("Error on SubscribedToTopic ", ex);
                return false;
            }




        }



        public bool UnsubribeFromTopic(ClientRequest clientRequest, string clientId)
        {

            try
            {
                if (this.Subscriptions.ContainsKey(clientRequest.Topic))
                {
                    var subscribers = (List<SubscriberConstrains>)this.Subscriptions[clientRequest.Topic];

                    foreach (var item in subscribers)
                    {

                        if (item.SubsciberId == clientId)
                        {



                            if (clientRequest.Constrains != null && item.Constrains != null)
                                foreach (var constrain in clientRequest.Constrains)
                                {
                                    item.Constrains.RemoveAll(con => con.Equals(constrain));

                        }


                        }


                    }

                    //if no contrains exist then remove the subscriber
                    subscribers.RemoveAll(item => (item.Constrains == null || item.Constrains.Count == 0) && item.SubsciberId == clientId);



                    return true;
                }


                return false;

            }
            catch (Exception ex)
            {
                log.Error("Error on UnsubribeFromTopic ", ex);
                return false;
               
            }

        }






        public void Publich(ClientRequest clientRequest, string msg)
        {

            try
            {
                var clients = new List<string>();

                if (this.Subscriptions.ContainsKey(clientRequest.Topic))
                {

                    var subscribers = (List<SubscriberConstrains>)this.Subscriptions[clientRequest.Topic];

                    foreach (var item in subscribers)
                    {
                        //currently support only 1 constrain 

                        if (clientRequest.Constrains != null)
                        {
                            var items = (List<Constrain>)item.Constrains.FindAll(con => con.Equals(clientRequest.Constrains[0]));

                            if (items.Count > 0 && !clients.Exists(el => el == item.SubsciberId))
                                clients.Add(item.SubsciberId);

                        }
                        else
                        {
                            if (!clients.Exists(el => el == item.SubsciberId))
                                clients.Add(item.SubsciberId);
                        }

                    }

                    this.SendToAll(clients, msg);


                }

            }
            catch (Exception ex )
            {
                log.Error("Error on Publich ", ex);
                
              
                
            }

        }




        public void SendToAll(List<string> subscribers, string msg)
        {

            try
            {
                foreach (var sub in subscribers)
                {

                    this.GetAppSessionByID(sub).Send(msg);

                }
            }
            catch (Exception ex)
            {
                log.Error("Error on SendToAll ", ex);
                
            }

        



        }

    }





    public class SubscriberConstrains
    {

        public string SubsciberId { get; set; }
        public List<Constrain> Constrains { get; set; }

        public SubscriberConstrains(string subsciberId, List<Constrain> constrains)
        {

            this.SubsciberId = subsciberId;
            this.Constrains = constrains;

        }





    }


}

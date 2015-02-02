

function WebSocketClient() {
    var self = this;
    this.ws;
    this.subscribersArray = [];


    this.findSubscriber = function (topic, constrains, remove) {
        
        debugger;
        var len = this.subscribersArray.length;

        for (var i = 0; i < len; i++) {

            if (this.subscribersArray[i].topic == topic) {

                for (var j = 0; j < constrains.length; j++) {

                    if ($.inArray(constrains[j], this.subscribersArray[i].constrains)) {

                        var temp = this.subscribersArray[i];
                        break;

                    }

                }


                if (remove == true)
                    this.subscribersArray.splice(i, 1);

                return temp;


            }

        }

        return null;

    };



    this.connect = function (WsServerAddress) {
       
        var support = "MozWebSocket" in window ? 'MozWebSocket' : ("WebSocket" in window ? 'WebSocket' : null);
        if (support == null) {
            return;

        }

        this.ws = new window[support](WsServerAddress);

        // when data is comming from the server, this metod is called


        this.ws.onopen = function () {

            console.log("Websocket Connection Opened " + WsServerAddress);

        };


        this.ws.onclose = function () {

            console.log("Websocket Connection closed");

        }

        this.ws.onmessage = function (evt) {
            //  appendMessage("# " + evt.data + "

            debugger;

            var request = null;

            try {

                var request = JSON.parse(evt.data);

            } catch (e) {

                console.log("cannot desirialize json " + evt.data);
            }

            if (request != null) {

                var subscriber = self.findSubscriber(request.Topic, request.Constrains, false);

                if (subscriber != null)
                    subscriber.callback.call(this, request.Msg);
            }
            

        };
      };



    this.subscribe = function (topic, constrains, callback) {

        var request = {

            'RequestType': 'SUBSCRIBE',
            'Topic': topic,
            'Constrains': constrains

        }

        var requestJson = JSON.stringify(request, null, 4);

        console.log(requestJson);

        this.sendMessage(requestJson);

        var subcriber = { topic: topic, constrains: constrains, callback: callback };

        this.subscribersArray.push(subcriber);

    };


    this.unsubscribe = function (topic, constrains) {

        var request = {

            'RequestType': 'UNSUBSCRIBE',
            'Topic': topic,
            'Constrains': constrains

        }


        var requestJson = JSON.stringify(request, null, 4);
        this.sendMessage(requestJson);

        this.findSubscriber(topic, constrains, "true");

    };


    this.publish = function (topic, constrains, msg) {


        var request = {

            'RequestType': 'PUBLISH',
            'Topic': topic,
            'Constrains': constrains,
            'Msg': msg
        }


        var requestJson = JSON.stringify(request, null, 4);
        this.sendMessage(requestJson);





    };

    this.disconnect = function () {

        if (this.ws) {
            ws.close();
        }

    };

    this.sendMessage = function (msg) {

        if (this.ws) {
            this.ws.send(msg);

        }


    };
}


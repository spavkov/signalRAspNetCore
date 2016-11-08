$(window).on("load", function () {
    // Things that need to happen after full load
    console.log("page load");
    var connectionId = "???";

    var myViewModel = {
        messages: ko.observableArray(["a","b"]),
        chatRoomId: ko.observable()
    };

    myViewModel.messages().push("zzz");

    function newUsersOnline(onlineUsers) {
        console.log("got list of online users for support");
        if (onlineUsers.SupportUsers) {
            console.log("support: " + onlineUsers.SupportUsers.length);
        }

        if (onlineUsers.Clients) {
            console.log("clients: " + onlineUsers.Clients.length);
        }

        onlineUsers.SupportUsers.forEach(function (user) {
            if (myViewModel.onlineSupportUsers.indexOf(user) === -1) {
                myViewModel.onlineSupportUsers.push(user);
            }
        });

        onlineUsers.Clients.forEach(function (user) {
            if (myViewModel.onlineClients.indexOf(user) === -1) {
                myViewModel.onlineClients.push(user);
            }
        });
    }

    function userDisconnected(userId) {
        console.log("removing user ");
        myViewModel.onlineSupportUsers.remove(userId);
        myViewModel.onlineClients.remove(userId);
    }

    function registerClientMethods(chatHub) {
        console.log("registering client events");
        // Calls when user successfully logged in
        chatHub.client.setConnectionId = function (id) {
            console.log("got connectionId " + id);
            connectionId = id;
        }

        chatHub.client.receiveRoomMessage = function(chatRoomId, userId, message) {
            console.log("got room message: [" + chatRoomId + "] " + userId + " : " + message);
            myViewModel.messages().push("v");
        }

        //calls when we have new list of online users
        chatHub.client.setOnlineUsers = function (onlineUsers) {
            console.log("server pushed new users");
            newUsersOnline(onlineUsers);
        }

        //calls when we user stopped being reachable
        chatHub.client.userDisconnected = function (userId) {
            console.log("server pushed dead user " + userId);
            userDisconnected(userId);
        }
    }

    // Declare a proxy to reference the hub.
    $.connection.hub.logging = true;
    $.connection.hub.qs = { 'version': '1.0' };
    $.connection.hub.url = "http://localhost:9999/signalr/hubs";
    var chatHub = $.connection.chatHub;
    registerClientMethods(chatHub);


    function registerEvents(chatHub) {

    }

    // Start Hub
    $.connection.hub.start().done(function () {
        console.log("hub start completed");
        registerEvents(chatHub);
    });
    $.connection.hub.stateChanged(function (state) {
        var stateConversion = { 0: 'connecting', 1: 'connected', 2: 'reconnecting', 4: 'disconnected' };
        console.log('SignalR state changed from: ' + stateConversion[state.oldState]
         + ' to: ' + stateConversion[state.newState]);
    });

    myViewModel.requestSupport = function () {
        console.log("requesting support");
        chatHub.server.requestSupport().done(function (chatRoomId) {
            console.log("received support room id " + chatRoomId);
        });
    };

    ko.applyBindings(myViewModel);
});
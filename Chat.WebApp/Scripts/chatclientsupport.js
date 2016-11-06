$(window).on("load", function () {
    // Things that need to happen after full load
    console.log("page load");
    var connectionId = "???";

    var myViewModel = {
        onlineSupportUsers: ko.observableArray(),
        supportRooms: ko.observableArray()
    };

    ko.applyBindings(myViewModel);

    function newUsersOnline(onlineUsers) {
        console.log("got list of online users for support");
        if (onlineUsers) {
            console.log("support: " + onlineUsers.length);

            onlineUsers.forEach(function (user) {

                if (myViewModel.onlineSupportUsers.indexOf(user) === -1) {
                    myViewModel.onlineSupportUsers.push(user);
                }
            });
        }
    }

    function userDisconnected(userId) {
        console.log("removing user ");
        myViewModel.onlineSupportUsers.remove(userId);

    }

    function newSupportRooms(rooms) {
        console.log("new support rooms");
        rooms.forEach(function(room) {
            myViewModel.supportRooms.push(room);
        });       
    }

    function registerClientMethods(chatHub) {
        console.log("registering client events");
        // Calls when user successfully logged in
        chatHub.client.setConnectionId = function (id) {
            console.log("got connectionId " + id);
            connectionId = id;
        }

        //calls when we have new list of online users
        chatHub.client.addNewOnlineUser = function (userId) {
            console.log("server pushed new online user");
            newUsersOnline([userId]);
        }

        //calls when we have new list of online users
        chatHub.client.setOnlineUsers = function (onlineUsers) {
            console.log("server pushed new users");
            newUsersOnline(onlineUsers);
        }

        chatHub.client.receiveRoomMessage = function (chatRoomId, userId, message) {
            console.log("got room message: [" + chatRoomId + "] " + userId + " : " + message);
        }

        //calls when we user stopped being reachable
        chatHub.client.userDisconnected = function (userId) {
            console.log("server pushed dead user " + userId);
            userDisconnected(userId);
        }

        //support request room created
        chatHub.client.supportRoomCreated = function (roomData) {
            console.log("server pushed new support room");
            newSupportRooms([roomData]);
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
        chatHub.server.getOnlineUsers().done(function (onlineUsers) {
            newUsersOnline(onlineUsers);
        });
        chatHub.server.getSupportRooms().done(function (rooms) {
            newSupportRooms(rooms);
        });
    });
    $.connection.hub.stateChanged(function (state) {
        var stateConversion = { 0: 'connecting', 1: 'connected', 2: 'reconnecting', 4: 'disconnected' };
        console.log('SignalR state changed from: ' + stateConversion[state.oldState]
         + ' to: ' + stateConversion[state.newState]);
    });

    myViewModel.provideSupport = function (room) {
        console.log("providing support for room " + room.RoomId);
        chatHub.server.provideSupport(room.RoomId);
    };

});
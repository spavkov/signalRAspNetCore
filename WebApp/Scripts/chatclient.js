$(window).on("load", function () {
    // Things that need to happen after full load
    console.log("page load");
    var connectionId = "???";

    function registerClientMethods(chatHub) {
        console.log("registering client events");
        // Calls when user successfully logged in
        chatHub.client.setConnectionId = function (id) {
            console.log("got connectionId " + id);
            connectionId = id;
        }

        //calls when we have new list of online users
        chatHub.client.setOnlineUsers = function (users) {
            console.log("got list of online users " + users);
        }

        // On New User Connected
        chatHub.client.onGroupJoined = function (groupName) {
            console.log("joined group " + groupName);
        }
    }

    // Declare a proxy to reference the hub.
    $.connection.hub.url = "http://localhost:9999/signalr/hubs";
    var chatHub = $.connection.chatHub;
    registerClientMethods(chatHub);


    function registerEvents(chatHub) {

    }

    // Start Hub
    $.connection.hub.start().done(function () {
        console.log("hub start completed");
        registerEvents(chatHub);
        //chatHub.server.connect("Aaa");
    });

});
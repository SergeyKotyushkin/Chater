var chat = $.connection.chatHub;

// on group created
chat.client.onGroupCreated = function (name, isCreated) {
    if (isCreated) {
        $("#ulChats").append("<li><input type=\"button\" value=\"" + name +"\" data-chat=\""+name+"\" onclick=\"chooseChat(this)\"/></div>");
    }
}

// on getting message
chat.client.retriveMessage = function (name, message) {
    $("#divMessages").append("<div><strong>" + name + ": </strong>" + message + "</div>");
    $("#divMessages").stop().animate({
        scrollTop: $("#divMessages")[0].scrollHeight
    }, 800);
    $("#textMessage").focus();
};

// on user connected
chat.client.onConnected = function (login, isConnected) {
    if (isConnected) {
        $("#divMessages").append("<div><strong>" + login + " connected to chat</strong></div>");
    }
}

// set connection with server
chat.connection.start().done(function () {
    // on connection setuped
    $("#btnSendMessage").click(function () {
        var message = $("#textMessage").val();
        chat.server.send("Let", message);

        $("#textMessage").val("");
    });

    $("#btnNewChat").click(function() {
        $("#divMessages").empty();
        var chatName = prompt("enter new chat name", "new chat");
        chat.server.createNewGroup(chatName);
        //chat.server.send(chatName, "dsd");
    });
});

function chooseChat(el) {
    $("#divMessages").empty();
    chat.server.connect("User", $(el).attr("data-chat"));
}

//function createNewChat() {
//    $("#divMessages").empty();
//    var chatName = prompt("enter new chat name", "new chat");
//    chat.server.createGroup(chatName);
//}

$(document).ready(function () {
    $("#divMessages").hide();
    $("#divSendMessage").hide();

//var data = {
//    title: "hello world",
//    description: "this is hello world"
//}

//var homeViewModel = {
//    title: ko.observable(data.title),
//    description: ko.observable(data.description)
//}

//$(document).ready(function () {
//    $.getJSON("api/data", function (result) {
//        homeViewModel.title(result[0].Title);
//        homeViewModel.description(result[0].Description);
//    });

//    ko.applyBindings(homeViewModel);
    //});
});
$(document).ready(function() {
    
    var chaterViewModel = {
        users: ko.observableArray(),

        chats: ko.observableArray()
    }


    // Users methods - Start
    chaterViewModel.users.add = function(guid, name, isOnline) {
        this.push({ guid: guid, name: name, isOnline: isOnline });
    };

    chaterViewModel.users.edit = function(guid, name, isOnline) {
        var index = this.findIndex(guid);
        this.replace(this()[index], { guid: guid, name: name, isOnline: isOnline });
    };

    chaterViewModel.users.addOrEdit = function(guid, name, isOnline) {
        this.findIndex(guid) === -1 ? this.add(guid, name, isOnline) : this.edit(guid, name, isOnline);
    };

    chaterViewModel.users.remove = function(guid) {
        this.remove(function(user) { return user.guid === guid; });
    };

    chaterViewModel.users.findIndex = function(guid) {
        var items = this();
        for (var i = 0; i < items.length; i++)
            if (items[i].guid === guid) return i;
        return -1;
    };

    chaterViewModel.users.find = function(guid) {
        return ko.utils.arrayFirst(this(), function(user) { return user.guid === guid; });
    };
    // Users methods - End

    // Chats methods - Start
    chaterViewModel.chats.add = function(guid, name) {
        this.push({ guid: guid, name: name });
    };

    chaterViewModel.chats.edit = function(guid, name) {
        var index = this.findIndex(guid);
        this.replace(this()[index], { guid: guid, name: name });
    };

    chaterViewModel.chats.addOrEdit = function(guid, name) {
        this.findIndex(guid) === -1 ? this.add(guid, name) : this.edit(guid, name);
    };

    chaterViewModel.chats.remove = function(guid) {
        this.remove(function(chat) { return chat.guid === guid; });
    };

    chaterViewModel.chats.findIndex = function(guid) {
        var items = this();
        for (var i = 0; i < items.length; i++)
            if (items[i].guid === guid) return i;
        return -1;
    };

    chaterViewModel.chats.find = function(guid) {
        return ko.utils.arrayFirst(this(), function(chat) { return chat.guid === guid; });
    };
    // Users methods - End

    ko.applyBindings(chaterViewModel);
    
    var chater = $.connection.chaterHub;


    // Registration: On Result
    chater.client.onRegister = function (result, message) {
        var divAlert = $("#divRegistrationAlert");
        divAlert.html(message);
        divAlert.removeClass("alert-success");
        divAlert.removeClass("alert-danger");
        divAlert.addClass(result ? "alert-success" : "alert-danger");
        divAlert.removeClass("hidden");
    };


    // Login: On Result To Caller
    chater.client.onLoginCaller = function (users, chats, result, message) {
        if (result) {
            var jsonUsers = jQuery.parseJSON(users);
            var i;
            for (i = 0; i < jsonUsers.length; i++)
                chaterViewModel.users.addOrEdit(jsonUsers[i].Guid, jsonUsers[i].UserName, jsonUsers[i].IsOnline);

            var jsonChats = jQuery.parseJSON(chats);
            for (i = 0; i < jsonChats.length; i++)
                chaterViewModel.chats.addOrEdit(jsonChats[i].Guid, jsonChats[i].Name);

            $("#divLogin").addClass("hidden");
            $("#divChater").removeClass("hidden");
        } else {
            var divLoginAlert = $("#divLoginAlert");
            divLoginAlert.removeClass("hidden");
            divLoginAlert.html(message);
        }
    }

    // Login: On Result To Others
    chater.client.onLoginOthers = function (guid, userName, isOnline) {
        chaterViewModel.users.addOrEdit(guid, userName, isOnline);
    }

    // Chat: New chat Result
    chater.client.onCreateChat = function(result, chatName, chatGuid, message) {
        if (result) {
            $("#divNewChatButton").removeClass("hidden");
            $("#divNewChat").addClass("hidden");

            $("#tbNewChatName").val("");
            chaterViewModel.chats.addOrEdit(chatGuid, chatName);
        } else {
            $("#divNewChatAlert").removeClass("hidden");
            $("#divNewChatAlert").html(message);
        }
    };

    // Disconnect: On Result To Caller
    chater.client.onDisconnectCaller = function () {
        location.reload(true);
    }

    // Disconnect: On Result To Others
    chater.client.onDisconnectOthers = function (guid) {
        var user = chaterViewModel.users.find(guid);
        user.isOnline = false;
        chaterViewModel.users.edit(user.guid, user.name, user.isOnline);
    }



    chater.connection.start().done(function () {

        // Registration: Back button Click
        $("#btnRegistrationBack").click(function () {
            $("#tbRegistrationLogin").val("");
            $("#tbRegistrationPassword").val("");
            $("#tbRegistrationUserName").val("");
            $("#divLogin").removeClass("hidden");
            $("#divRegistration").addClass("hidden");
            $("#divRegistrationAlert").addClass("hidden");
        });

        // Registration: Register button Click
        $("#btnRegistrationOk").click(function () {
            var login = $("#tbRegistrationLogin").val(),
                password = $("#tbRegistrationPassword").val(),
                userName = $("#tbRegistrationUserName").val();

            var divAlert = $("#divRegistrationAlert");
            if (!login.length || !password.length || !userName.length) {
                divAlert.html("Fields must be filled!");
                divAlert.removeClass("alert-success");
                divAlert.addClass("alert-danger");
                divAlert.removeClass("hidden");
                return;
            }
            
            divAlert.addClass("hidden");
            chater.server.register(login, password, userName);
        });


        // Login: Register button Click
        $("#btnRegister").click(function () {
            $("#divLoginAlert").addClass("hidden");
            $("#divLogin").addClass("hidden");
            $("#divRegistration").removeClass("hidden");
        });

        // Login: Login button Click
        $("#btnLogin").click(function () {
            $("#divLoginAlert").addClass("hidden");
            var login = $("#tbLogin").val();
            var password = $("#tbPassword").val();
            chater.server.login(login, password);
        });


        // Chat: New Chat Open button Click
        $("#btnNewChatOpen").click(function() {
            $("#divNewChatButton").addClass("hidden");
            $("#divNewChat").removeClass("hidden");

        });

        // Chat: New Chat Back button Click
        $("#btnNewChatBack").click(function () {
            $("#divNewChatButton").removeClass("hidden");
            $("#divNewChat").addClass("hidden");
        });

        // Chat: New Chat Back button Click
        $("#btnNewChatCreate").click(function () {
            $("#divNewChatAlert").addClass("hidden");
            chater.server.createChat($("#tbNewChatName").val());
        });

        // Menu -> Exit
        $("#btnClose").click(function () {
            chater.server.disconnect();
        });
    });
});
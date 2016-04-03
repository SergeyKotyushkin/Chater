$(document).ready(function() {
    
    var chater = $.connection.chaterHub;

    // Chats methods
    function getUsersForChat(el) {
        var chatGuid = $($(el).parent()).attr("data-guid");
        chater.server.getUsersForChat(chatGuid);
    }

    function removeChat(el) {
        if (!confirm("Remove this chat?"))
            return;
        var chatGuid = $($(el).parent()).attr("data-guid");
        chater.server.removeChat(chatGuid);
    }


    // KO model
    var chaterViewModel = {
        users: ko.observableArray(),

        usersForChat: ko.observableArray(),

        editChat: { guid: ko.observable(""), name: ko.observable(""), originName: ko.observable("") },

        currentChat: { guid: ko.observable("Default"), name: ko.observable("Since") },

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

    chaterViewModel.chats.add = function(guid, name) {
        this.push({ guid: guid, name: name });
    };

    chaterViewModel.chats.edit = function(guid, name) {
        var index = this.findIndex(guid);
        this.replace(this()[index], { guid: guid, name: name });
    };

    chaterViewModel.chats.addOrEdit = function(guid, name) {
        this.findIndex(guid) === -1 ? this.add(guid, name) : this.edit(guid, name);

        $("#chat" + guid + " .btnChatEdit").click(function () { getUsersForChat(this); });
        $("#chat" + guid + " .btnChatRemove").click(function () { removeChat(this); });
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
    
    

    // Chat: Chat Remove button Click
    ;

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
            
            $(".chat").each(function () {
                $(this).click(function () {
                    $("#loadingImage").removeClass("hidden");
                    $("#divChat").addClass("hidden");
                    $.get("api/ChatData/" + $(this).attr("data-guid"), function(jsonData) {
                            var data = JSON.parse(jsonData);
                            if (!data.Result) {
                                alert(data.Message);
                                return;
                            }

                            chaterViewModel.currentChat.guid(data.Chat.Guid);
                            chaterViewModel.currentChat.name(data.Chat.Name);

                            $("#divChat").removeClass("hidden");
                        })
                        .fail(function() {
                            alert("Some error occured. Try again.");
                        })
                        .always(function() {
                            $("#loadingImage").addClass("hidden");
                        });
                });
            });

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
    
    // Chat: On Remove Result
    chater.client.onRemoveResult = function (result, message) {
        if (!result)
            alert(message);
    }

    // Chat: On Get Users For Chat
    chater.client.onGetUsersForChat = function(result, userGuids, chat, message) {
        if (!result) {
            alert(message);
            return;
        }

        if (userGuids == null)
            userGuids = [];

        chaterViewModel.usersForChat.removeAll();
        for (var i = 0; i < chaterViewModel.users().length; i++) {
            chaterViewModel.usersForChat.push({
                userGuid: chaterViewModel.users()[i].guid,
                userName: chaterViewModel.users()[i].name,
                isInChat: userGuids.indexOf(chaterViewModel.users()[i].guid) !== -1
            });
        }

        var jsonChat = JSON.parse(chat);
        chaterViewModel.editChat.guid(jsonChat.Guid);
        chaterViewModel.editChat.name(jsonChat.Name);
        chaterViewModel.editChat.originName(jsonChat.Name);

        $("#divNewChatButton").addClass("hidden");
        $("#divNewChat").addClass("hidden");
        $("#divEditChat").removeClass("hidden");
    }

    // Chat: On Updated Result
    chater.client.OnUpdateChat = function (result, chat, message) {
        $("#divNewChat").addClass("hidden");
        $("#divEditChat").addClass("hidden");
        $("#divNewChatButton").addClass("hidden");

        if (result) {
            var jsonChat = JSON.parse(chat);
            chaterViewModel.chats.addOrEdit(jsonChat.Guid, jsonChat.Name);
        }

        alert(message);
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

        // Chat: New Chat Create button Click
        $("#btnNewChatCreate").click(function () {
            $("#divNewChatAlert").addClass("hidden");
            chater.server.createChat($("#tbNewChatName").val());
        });

        
        // Edit Chat: Back button Click
        $("#btnEditChatBack").click(function () {
            $("#divNewChat").addClass("hidden");
            $("#divEditChat").addClass("hidden");
            $("#divNewChatButton").removeClass("hidden");
        });

        // Edit Chat: Update button Click
        $("#btnEditChatEdit").click(function () {
            $("#divNewChat").addClass("hidden");
            $("#divEditChat").addClass("hidden");
            $("#divNewChatButton").removeClass("hidden");
            var chatGuid = chaterViewModel.editChat.guid();
            var chatName = chaterViewModel.editChat.name();
            var chatUsers = chaterViewModel.usersForChat();
            var userGuids = [];
            for (var i = 0; i < chatUsers.length; i++) {
                userGuids.push(chatUsers[i].userGuid);
            }
            chater.server.updateChat(chatGuid, chatName, JSON.stringify(userGuids));
        });
        
        // Close
        $("#btnClose").click(function () {
            chater.server.disconnect();
        });
    });
});
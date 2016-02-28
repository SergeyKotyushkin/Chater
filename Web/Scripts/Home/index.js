$(function() {
    
    var funHideAll = function () {
        $("#divChatsContainer").hide();
        $("#divLoginContainer").hide();
        $("#divNewChatContainer").hide();
        $("#divAddToChatContainer").hide();
        $("#divMessengerContainer").hide();
        $("#divMenuContainer").hide(); 
        $("#divRegistrationContainer").hide();
    };

    var funToMenu = function () {
        funHideAll();
        $("#divMenuContainer").show();
    };

    funHideAll();
    $("#divLoginContainer").show();

    var chat = $.connection.chatHub;

    // Login
    // on login result
    chat.client.onLoginResult = function (user, usersOnline, isLoginSuccess, message) {
        if (isLoginSuccess) {
            var jsonUser = jQuery.parseJSON(user);
            $("#hiddenUserGuid").html(jsonUser.Guid);

            var jsonUsersOnline = jQuery.parseJSON(usersOnline);
            $("#divChaters").empty();
            for (var i = 0; i < jsonUsersOnline.length; i++) {
                $("#divChaters").append("<div style=\"float: left; margin-right: 5px;\">[" + jsonUsersOnline[i].UserName + "]</div>");
            }

            funToMenu();
        } else {
            var divAlert = $("#divLoginAlert");
            divAlert.removeClass("hidden");
            divAlert.html(message);
        }
    }

    // say other that i'm is online
    chat.client.onUserLoggedInResult = function(userName) {
        if ($("#divChaters").html().indexOf("[" + userName + "]") === -1)
            $("#divChaters").append("<div style=\"float: left; margin-right: 5px;\">[" + userName + "]</div>");
    }

    // say other that i'm is offline
    chat.client.onDisconnectResult = function (userName) {
        var temp = "<div style=\"float: left; margin-right: 5px;\">[" + userName + "]</div>";
        if ($("#divChaters").html().indexOf(temp) !== -1) {
            var str = $("#divChaters").html().substring(0, $("#divChaters").html().indexOf(temp));
            str += $("#divChaters").html().substring($("#divChaters").html().indexOf(temp) + temp.length);
            $("#divChaters").html(str);
        }
    }

    // refresh after disconnect caller
    chat.client.onDisconnectCallerResult = function () {
        $.connection.hub.stop();
        location.reload(true);
    }

    // Registration
    chat.client.onRegisterResult = function (isRegistered, message) {
        var divAlert = $("#divRegistrationAlert");
        divAlert.removeClass("hidden");
        divAlert.html(message);
        if (isRegistered) {
            divAlert.addClass("alert-success");
        } else {
            divAlert.addClass("alert-danger");
        }
    };

    // Menu -> New Chat
    chat.client.onCreateChatResult = function (isCreated, chatName, chatGuid, message) {
        if (isCreated) {
            funHideAll();
            $("#divMessengerContainer").show();
            $("#labelChatName").html(chatName);
            $("#hiddenChatGuid").html(chatGuid);
        } else {
            alert(message);
        }
    };

    // Messenger : on send
    chat.client.onSendResult = function (messageOutput, chatGuid, result) {
        if ($("#hiddenChatGuid").html() === chatGuid) {
            if (result) {
                $("#divMessages").append("<div><strong>" + messageOutput.UserName + " " +
                    messageOutput.SendTime + "</strong>: " + messageOutput.Text + "</div>");
            } else {
                $("#divMessages").append("<div><span style=\"color: red\"><strong>" + messageOutput.UserName + " " +
                    messageOutput.SendTime + "</strong>: " + messageOutput.Text + "</span></div>");
            }

            $("#divMessages").stop().animate({
                scrollTop: $("#divMessages")[0].scrollHeight
            }, 800);
        }
    }

    // after users added to chat
    var funSetUserForChat = function () {
        var chatGuid = $("#selectAddToChatChat").val();
        $.get("api/chatUsers/" + chatGuid).done(function (result) {
            var chatUsers = jQuery.parseJSON(result);
            $("#divAddToChatUsers :checked").each(function () { $(this).removeAttr("checked"); });
            $("#divAddToChatUsers :disabled").each(function () { $(this).removeAttr("disabled"); });
            for (var i = 0; i < chatUsers.length; i++) {
                $("#divAddToChatUsers").find("[data-guid=\"" + chatUsers[i].UserGuid + "\"]").prop("checked", true);
                $("#divAddToChatUsers").find("[data-guid=\"" + chatUsers[i].UserGuid + "\"]").prop("disabled", true);
            }
        });
    };

    var funAddUsers = function () {
        $.get("api/users/").done(function (result) {
            var users = jQuery.parseJSON(result);
            $("#divAddToChatUsers").empty();
            for (var i = 0; i < users.length; i++) {
                $("#divAddToChatUsers").append("<div class=\"checkbox\"><label><input type=\"checkbox\" data-guid=\"" + users[i].Guid + "\"> " + users[i].UserName + "</label></div>");
            }

            funSetUserForChat();
        });
    };
    
    chat.client.onUsersAddToChatResult = function() { funAddUsers(); }

    // if connection still exists then disconect
    if ($.connection.hub.state !== 4)
        $.connection.hub.stop();

    chat.connection.start().done(function () {

        // Registration
        // open registration page
        $("#btnRegister").click(function () {
            $("#divLoginAlert").addClass("hidden");
            funHideAll();
            $("#divRegistrationContainer").show();
        });

        // cancel the registration
        $("#btnRegistrationCancel").click(function () {
            $("#textRegistrationLogin").val("");
            $("#textRegistrationPassword").val("");
            $("#textRegistrationUserName").val("");
            funHideAll();
            $("#divLoginContainer").show();
        });

        // try to register
        $("#btnRegistrationRegister").click(function () {
            var login = $("#textRegistrationLogin").val(),
                password = $("#textRegistrationPassword").val(),
                userName = $("#textRegistrationUserName").val();

            chat.server.register(login, password, userName);
        });


        // Login
        // to login
        $("#btnLogin").click(function () {
            $("#divLoginAlert").addClass("hidden");
            var login = $("#textLogin").val();
            var password = $("#textPassword").val();
            chat.server.login(login, password);
        });


        // Menu -> Chats
        var funChatChoose = function(el) {
            funHideAll();
            $("#divMessengerContainer").show();
            $("#labelChatName").html($(el).val());
            var chatGuid = $(el).attr("data-guid");

            if ($("#hiddenChatGuid").val() !== chatGuid) {
                $.get("api/messages/" + chatGuid).done(function (result) {
                    var messageOutputed = jQuery.parseJSON(result);
                    $("#divMessages").empty();
                    for (var i = 0; i < messageOutputed.length; i++) {
                        if(messageOutputed[i] === null) continue;
                        $("#divMessages").append("<div><strong>" + messageOutputed[i].UserName +
                            " " + messageOutputed[i].SendTime + "</strong>: " +
                            messageOutputed[i].Text + "</div>");
                    }
                });
                $("#hiddenChatGuid").html(chatGuid);
                $("#divMessages").stop().animate({
                    scrollTop: $("#divMessages")[0].scrollHeight
                }, 800);
            }
        };

        $("#btnMenuChatsList").click(function () {
            funHideAll();
            $("#divChatsContainer").show();

            $.get("api/chats/" + $.connection.hub.id).done(function (result) {
                var chats = jQuery.parseJSON(result);
                $("#divChatsList").empty();
                for (var i = 0; i < chats.length; i++) {
                    $("#divChatsList").append("<div class=\"col-md-12\"><input type=\"button\" value=\"" + chats[i].Name + "\" data-guid=\"" + chats[i].Guid + "\"/></div>");
                }
                $("#divChatsList :input").each(function() { $(this).click(function() { funChatChoose(this) }) });
            });
        });

        // Menu -> New Chat
        $("#btnMenuNewChat").click(function () {
            funHideAll();
            $("#divNewChatContainer").show();
        });

        // Menu -> Add To Chat
        $("#btnMenuAddToChat").click(function () {
            funHideAll();
            $("#divAddToChatContainer").show();

            $.get("api/chats/" + $.connection.hub.id).done(function (result) {
                var chats = jQuery.parseJSON(result);
                $("#selectAddToChatChat").empty();
                for (var i = 0; i < chats.length; i++) {
                    $("#selectAddToChatChat").append("<option value=\"" + chats[i].Guid + "\">" + chats[i].Name + "</option>");
                }

                funAddUsers();

                $("#selectAddToChatChat").change(function () { funSetUserForChat(); });
            });

        });

        $("#btnAddToChatCommit").click(function () {
            if (!confirm("Commit changes")) return;

            var chatGuid = $("#selectAddToChatChat").val();
            var usersGuids = "";
            $("#divAddToChatUsers :checked").each(function() {
                usersGuids += "," + $(this).attr("data-guid");
            });

            if (usersGuids.indexOf(",") === -1) return;

            usersGuids = usersGuids.substring(1);
            chat.server.usersAddToChat(chatGuid, usersGuids);
            //$.post("api/chatUsers", { val: data }).done(function() {
            //    funAddUsers();
            //});
        });

        // Menu -> Exit
        $("#btnMenuExit").click(function () {
            //funHideAll();
            //$("#divLoginContainer").show();

            chat.server.disconnect();
            //$("#hiddenUserGuid").empty();
            //$("#hiddenChatGuid").empty();
        });

        $("#btnChatsToMenu").click(function() { funToMenu(); });
        $("#btnNewChatToMenu").click(function () { funToMenu(); });
        $("#btnAddToChatToMenu").click(function () { funToMenu(); });
        $("#btnMessengerToMenu").click(function () { funToMenu(); });


        // New Chat
        $("#btnNewChat").click(function () {
            var name = $("#textNewChat").val();
            chat.server.createChat(name);
        });


        // Messenger
        // Send
        $("#btnMessengerSend").click(function () {
            chat.server.send($("#hiddenChatGuid").html(), $("#textMessage").val());
            $("#textMessage").val("");
        });

    });

    
});
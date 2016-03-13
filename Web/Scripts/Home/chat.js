$(document).ready(function() {
    
    var chaterViewModel = {
        users: ko.observableArray()
    }


    // Users methods - Start
    chaterViewModel.users.add = function (guid, name, isOnline) {
        this.push({ guid: guid, name: name, isOnline: isOnline });
    };

    chaterViewModel.users.edit = function(guid, name, isOnline) {
        var index = this.findIndex(guid);
        this.replace(this()[index], { guid: guid, name: name, isOnline: isOnline });
    };

    chaterViewModel.users.addOrEdit = function (guid, name, isOnline) {
        this.findIndex(guid) === -1 ? this.add(guid, name, isOnline) : this.edit(guid, name, isOnline);
    };

    chaterViewModel.users.remove = function (guid) {
        this.remove(function (user) { return user.guid === guid; });
    };

    chaterViewModel.users.findIndex = function (guid) {
        var items = this();
        for (var i = 0; i < items.length; i++)
            if (items[i].guid === guid) return i;
        return -1;
    };

    chaterViewModel.users.find = function (guid) {
        var items = this();
        var elem = ko.utils.arrayFirst(items, function (user) { return user.guid === guid; });
        if (elem === undefined || elem === null)
            alert("und");
        return elem;
    };
    // Users methods - End

    ko.applyBindings(chaterViewModel);
    
    var chater = $.connection.chaterHub;

    // Login: On Result To Caller
    chater.client.onLoginCaller = function (users, isLoginSuccess, message) {
        if (isLoginSuccess) {
            var jsonUsers = jQuery.parseJSON(users);
            for (var i = 0; i < jsonUsers.length; i++)
                chaterViewModel.users.addOrEdit(jsonUsers[i].Guid, jsonUsers[i].UserName, jsonUsers[i].IsOnline);

            $("#divLogin").addClass("hidden");
            $("#divChater").removeClass("hidden");
        } else {
            var divLoginError = $("#divLoginError");
            divLoginError.removeClass("hidden");
            divLoginError.html(message);
        }
    }

    // Login: On Result To Others
    chater.client.onLoginOthers = function (guid, userName, isOnline) {
        chaterViewModel.users.addOrEdit(guid, userName, isOnline);
    }


    // Disconnect: On Result To Caller
    chater.client.onDisconnectCaller = function () {
        //$.connection.hub.stop();
        location.reload(true);
    }

    // Disconnect: On Result To Others
    chater.client.onDisconnectOthers = function (guid) {
        var user = chaterViewModel.users.find(guid);
        user.isOnline = false;
        chaterViewModel.users.edit(user.guid, user.name, user.isOnline);
    }



    chater.connection.start().done(function () {

        // Button to login Click
        $("#btnLogin").click(function () {

            $("#divLoginError").addClass("hidden");
            var login = $("#tbLogin").val();
            var password = $("#tbPassword").val();
            chater.server.login(login, password);
        });

        // Menu -> Exit
        $("#btnClose").click(function () {
            chater.server.disconnect();
        });
    });
});
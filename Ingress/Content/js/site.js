angular.module('ingressApp', ['angularGrid', 'ngSanitize', 'cgNotify'])
    .service('itemService', ['$q', '$http', '$httpParamSerializerJQLike', function ($q, $http) {
        this.loadItems = function () {
            return $http.get("/items");
        };
        this.updateItem = function (item) {
            return $http({
                url: "/items/" + item.Id,
                method: 'PUT',
                params: item,
                paramSerializer: '$httpParamSerializerJQLike'
            });
        };
        this.deleteItem = function (item) {
            return $http({
                url: "/items/" + item.Id,
                method: 'DELETE'
            });
        };
        this.createItem = function (item) {
            return $http({
                url: "/items/",
                method: 'POST',
                params: item,
                paramSerializer: '$httpParamSerializerJQLike'
            });
        };
    }])
    .service('notificationService', ['$q', '$http', '$httpParamSerializerJQLike', function ($q, $http) {
        this.loadNotifications = function () {
            return $http.get("/notifications");
        };
        this.updateNotification = function (notification) {
            return $http({
                url: "/notifications/" + notification.Id,
                method: 'PUT',
                params: notification,
                paramSerializer: '$httpParamSerializerJQLike'
            });
        };
        this.deleteNotification = function (notification) {
            return $http({
                url: "/notifications/" + notification.Id,
                method: 'DELETE'
            });
        };
        this.createNotification = function (notification) {
            return $http({
                url: "/notifications/",
                method: 'POST',
                params: notification,
                paramSerializer: '$httpParamSerializerJQLike'
            });
        };
    }])
    .controller('home-notifications', ['$scope', '$sce', 'notificationService', function ($scope, $sce, notificationService) {
        notificationService.loadNotifications().then(function (res) {
            $scope.notifications = res.data;
            console.log(res.data);
        });
    }])
    .controller('home-items', ['$scope', '$sce', 'itemService', function ($scope, $sce, itemService, angularGridInstance) {
        itemService.loadItems().then(function (res) {
            $scope.items = res.data;
        });
    }])
    .controller('admin-items', ['$scope', 'itemService', 'notify', function ($scope, itemService, notify) {
        $scope.reloadItems = function () {
            itemService.loadItems().then(function (res) {
                $scope.data = {
                    itemList: res.data,
                    selectedItem: "-1"
                };
            });
        };
        $scope.reloadItems();
        $scope.saveItem = function (data) {
            if (data.Id) {
                console.log("Updating ID: " + data.Id);
                itemService.updateItem(data).then(function (result) {
                    console.log(result);
                    notify({ message: 'Item updated.', duration: 1500 });
                    $scope.reloadItems();
                });
            } else {
                console.log("Creating a new item");
                // Set a default owner in case one wasn't sent
                if (data.Owner == null) data.Owner = "fd19ffae-1c11-45e5-a618-9defb70154f9";
                itemService.createItem(data).then(function (result) {
                    console.log(result);
                    notify({ message: 'Item created.', duration: 1500 });
                    $scope.reloadItems();
                });
            }
            console.log(data);
        };
        $scope.deleteItem = function (data) {
            if (data.Id) {
                console.log("Deleting ID: " + data.Id);
                itemService.deleteItem(data).then(function (result) {
                    console.log(result);
                    notify({ message: 'Item deleted.', duration: 1500 });
                    $scope.reloadItems();
                });
            } else {
                $scope.reloadItems();
            }
            console.log(data);
        };
    }])
    .controller('admin-notifications', ['$scope', 'notificationService', 'notify', function ($scope, notificationService, notify) {
        $scope.reloadNotifications = function () {
            notificationService.loadNotifications().then(function (res) {
                $scope.data = {
                    messageList: res.data,
                    selectedNotification: "-1"
                };
            });
        };
        $scope.reloadNotifications();
        $scope.saveNotification = function (data) {
            if (data.Id) {
                console.log("Updating ID: " + data.Id);
                notificationService.updateNotification(data).then(function (result) {
                    console.log(result);
                    notify({ message: 'Notification updated.', duration: 1500 });
                    $scope.reloadNotifications();
                });
            } else {
                console.log("Creating a new item");
                // Set a default target in case one wasn't sent
                if (data.Target == null) data.Target = "fd19ffae-1c11-45e5-a618-9defb70154f9";
                notificationService.createNotification(data).then(function (result) {
                    console.log(result);
                    notify({ message: 'Notification created.', duration: 1500 });
                    $scope.reloadNotifications();
                });
            }
            console.log(data);
        };
        $scope.deleteNotification = function (data) {
            if (data.Id) {
                console.log("Deleting ID: " + data.Id);
                notificationService.deleteNotification(data).then(function (result) {
                    console.log(result);
                    notify({ message: 'Notification deleted.', duration: 1500 });
                    $scope.reloadNotifications();
                });
            } else {
                $scope.reloadNotifications();
            }
            console.log(data);
        };
    }])
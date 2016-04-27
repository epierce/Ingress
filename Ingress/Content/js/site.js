angular.module('ingressApp', ['angularGrid', 'ngSanitize'])
    .service('imageService', ['$q', '$http', function ($q, $http) {
        this.loadImages = function () {
            return $http.jsonp("https://api.flickr.com/services/feeds/photos_public.gne?format=json&jsoncallback=JSON_CALLBACK");
        };
    }])
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
        this.createItem = function (item) {
            return $http({
                url: "/items/",
                method: 'POST',
                params: item,
                paramSerializer: '$httpParamSerializerJQLike'
            });
        };
    }])
    .controller('home', ['$scope', '$sce', 'itemService', 'angularGridInstance', function ($scope, $sce, itemService, angularGridInstance) {
        itemService.loadItems().then(function (res) {
            $scope.items = res.data;
        });
    }])
    .controller('admin', ['$scope', 'itemService', function ($scope, itemService) {
        $scope.reloadItems = function () {
            itemService.loadItems().then(function (res) {
                $scope.data = {
                    itemList: res.data,
                    selectedItem: "-1"
                };
            });
        };
        $scope.reloadItems();
        $scope.saveData = function (data) {
            if (data.Id) {
                console.log("Updating ID: " + data.Id);
                itemService.updateItem(data).then(function (result) {
                    console.log(result);
                    $scope.reloadItems();
                });
            } else {
                console.log("Creating a new item");
                // Set a default owner in case one wasn't sent
                if (data.Owner == null) data.Owner = "fd19ffae-1c11-45e5-a618-9defb70154f9";
                itemService.createItem(data).then(function (result) {
                    console.log(result);
                    $scope.reloadItems();
                });
            }
            console.log(data);
        };

        
    }]);
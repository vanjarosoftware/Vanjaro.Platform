app.controller('setting_add', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.loadcontainer = false;
    $scope.onInit = function () {
        if ($scope.ui.data.Connector.Options.Id)
            $scope.Click_GetAllContainers();
    };

    $scope.Click_AddContainer = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_add')) {
            swal({
                title: "[L:AddContainer]",
                type: "input",
                showCancelButton: true,
                closeOnConfirm: false,
                inputPlaceholder: "[L:EnterContainerName]"
            }, function (inputValue) {
                if (inputValue === false)
                    return false;
                if (inputValue === "") {
                    swal.showInputError("[L:ErrorAddingContainer]");
                    return false
                }
                UpdateValue();
                common.webApi.post('add/addcontainer', 'name=' + inputValue, $scope.ui.data.Connector.Options).success(function (Response) {
                    swal.close()
                    if (Response.IsSuccess) {
                        if ($scope.ui.data.Connector.Options.Id)
                            $scope.Click_GetAllContainers();                        
                    }
                    else if (Response.HasErrors) {
                        window.parent.swal(Response.Message);
                    }
                });
            });
        }
    };

    $scope.Click_GetAllContainers = function () {
        common.webApi.get('add/GetAllContainers', 'id=' + parseInt($scope.ui.data.Connector.Options.Id)).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.loadcontainer = true;
                $scope.ui.data.Containers.Options = Response.Data;
                $scope.ui.data.Containers.Value = $scope.ui.data.Connector.Options.Configurations.Container;
            }
            else if (Response.HasErrors)
                window.parent.swal(Response.Message);
        });
    };

    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_add')) {
            UpdateValue();
            common.webApi.post('add/save', '', $scope.ui.data.Connector.Options).success(function (Response) {
                if (Response.IsSuccess) {
                    var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
                    Parentscope.GetAllConnector();
                    if ($scope.loadcontainer) {
                        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                    } else {
                        $scope.ui.data.Connector.Options.Id = Response.Data;
                        $scope.Click_GetAllContainers()
                    }
                }
                else if (Response.HasErrors)
                    window.parent.swal(Response.Message);
            });
        }
    };

    var UpdateValue = function () {
        $scope.ui.data.Connector.Options.Configurations.Container = $scope.ui.data.Containers.Value;
        $scope.ui.data.Connector.Options.Configurations.UseHttps = $scope.ui.data.UseHTTPS.Value;
        $scope.ui.data.Connector.Options.Configurations.DirectLink = $scope.ui.data.UseDirectLink.Value;
        $scope.ui.data.Connector.Options.Configurations.SyncBatchSize = $scope.ui.data.SyncBatchSize.Value;
    };

});
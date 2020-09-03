app.controller('setting_add', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.Portals = [];
    $scope.onInit = function () {
        $scope.Portals = [];
        $scope.ui.data.AvailablePortals.Value = parseInt($scope.ui.data.AvailablePortals.Value);
        if ($scope.ui.data.PortalGroupInfo.Options[0].PortalGroupId != -1) {
            $.each($scope.ui.data.PortalGroupInfo.Options[0].Portals, function (index, value) {
                if ($scope.ui.data.PortalGroupInfo.Options[0].MasterPortal.PortalID != value.PortalID) {
                    $scope.Portals.push({
                        PortalID: value.PortalID,
                        PortalName: value.PortalName,
                        IsChecked: true
                    });
                }
            });
            $.each($scope.ui.data.AvailablePortals.Options, function (index, value) {
                if ($scope.ui.data.AvailablePortals.Value != value.PortalID && value.PortalID != -1) {
                    $scope.Portals.push({
                        PortalID: value.PortalID,
                        PortalName: value.PortalName,
                        IsChecked: false
                    });
                }
            });
        }
    };

    $scope.Click_Update = function () {
        $scope.ui.data.PortalGroupInfo.Options[0].Portals = [];
        if (mnValidationService.DoValidationAndSubmit('', 'setting_add')) {
            $.each($("input[type='checkbox']:checked"), function (index,value) {
                var AssignedPortals = [];
                AssignedPortals = { PortalID: value.id, PortalName: value.value }
                $scope.ui.data.PortalGroupInfo.Options[0].Portals.push(AssignedPortals);
            });
            common.webApi.post('add/update', '', $scope.ui.data.PortalGroupInfo.Options[0]).success(function (Response) {
                var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
                Parentscope.RefreshGrid();
                $(window.parent.document.body).find('[data-dismiss="modal"]').click();
            });
        }
    };

    $scope.ChooseSite = function () {
        $scope.Portals = [];
        if ($scope.ui.data.AvailablePortals.Value != -1) {
            $scope.ui.data.PortalGroupInfo.Options[0].PortalGroupName = $scope.ui.data.AvailablePortals.Options[$("select").children("option:selected").val()].PortalName + " Group";
            $scope.ui.data.PortalGroupInfo.Options[0].MasterPortal = { PortalID: $scope.ui.data.AvailablePortals.Value, PortalName: $scope.ui.data.AvailablePortals.Options[$("select").children("option:selected").val()].PortalName };
            if ($scope.ui.data.PortalGroupInfo.Options[0].PortalGroupId == -1) {
                $.each($scope.ui.data.AvailablePortals.Options, function (index, value) {
                    if ($scope.ui.data.AvailablePortals.Value != value.PortalID && value.PortalID != -1) {
                        $scope.Portals.push({
                            PortalID: value.PortalID,
                            PortalName: value.PortalName,
                            IsChecked: false
                        });
                    }
                });
            }
        };
    };
});
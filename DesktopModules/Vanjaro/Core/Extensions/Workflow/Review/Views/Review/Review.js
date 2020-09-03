app.controller('review_review', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $scope.StateID = $scope.ui.data.States.Value;

            var toggleclose = $('body').find('.settingclosebtn').length;
            if ($(".sidebar").hasClass("settingclosebtn")) {
                alert('hello');
                $("#notificationbackbtn").hide();
            }

    };

    $scope.ShowStateDDL = false;
    $scope.ClickShowStateDDL = function () {
        if ($scope.ShowStateDDL)
            $scope.ShowStateDDL = false;
        else
            $scope.ShowStateDDL = true;
    }

    $scope.OpenNotification = function () {
        setTimeout(function () { parent.OpenPopUp('', 300, 'right', '[LS:Notification]', $scope.ui.data.NotificationExtensionURL.Value); }, 500);
    }

    $scope.Change_BindStates = function () {
        $scope.Pipe_ReviewPages($scope.ReviewPagestableState);
        common.webApi.get('Review/GetStates', '').success(function (data) {
            if (data) {
                $scope.ui.data.States.Options = data.States;
                $scope.StateID = $scope.ui.data.States.Value;
            }
        });

    }

    $scope.BindReviewPages = function (event,id) {
        $scope.StateID = id;
        $scope.Pipe_ReviewPages($scope.ReviewPagestableState);
    }
    $scope.Pipe_ReviewPages = function (tableState) {
        $scope.ReviewPagestableState = tableState;
        $scope.ReviewSearchKeyword = null;
        if ($('.ReviewPages [type="search"]').val() != undefined)
            $scope.ProgressKeyword = $('.ReviewPages [type="search"]').val();

        common.webApi.get('Review/GetPages', 'skip=' + tableState.pagination.start + '&pagesize=' + tableState.pagination.number + '&StateID=' + $scope.StateID).success(function (data) {
            if (data) {
                tableState.pagination.numberOfPages = data.NumberOfPages;
                $scope.ReviewPages = [];
                $scope.ReviewPages = data.Data;
            }
        });
    };

    $scope.OpenModelPopup = function (Row) {
        parent.window.location.href = Row.TabURL;
    }
});
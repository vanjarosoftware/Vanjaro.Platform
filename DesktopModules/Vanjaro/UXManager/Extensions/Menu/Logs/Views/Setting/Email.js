app.controller('setting_email', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);

    $scope.Click_Cancel = function () {
        window.parent.swal.close();
    };

    $scope.Click_Send = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender) && window.parent.document !== undefined) {
            var parentscope = window.parent.angular.element('#SubmitEmailLog').scope();
            if (parentscope !== undefined && parentscope.SelectedLogItems !== undefined) {
                var Message;
                if ($scope.ui.data.Message === undefined)
                    Message = '';
                else
                    Message = $scope.ui.data.Message.Value;
                var EmailLog = {
                    Email: $scope.ui.data.Email.Value,
                    LogIds: parentscope.SelectedLogItems,
                    Subject: $scope.ui.data.Subject.Value,
                    Message: Message
                };
                common.webApi.post('Email/EmailLogItems', '', EmailLog).then(function (data) {
                    if (data !== undefined) {
                        if (data.data.Success) {
                            window.parent.parent.ShowNotification('[L:LogEmail]', data.data.ErrorMessage, 'success');
                            parentscope.SelectedLogItems = [];
                            window.parent.angular.element('.vj-ux-manager .uiengine-wrapper.scrollbar input.select-log').prop('checked', false);
                            $scope.Click_Cancel();
                        }
                        else {
                            window.parent.parent.ShowNotification('[LS:Error]', data.data.Status, 'error');
                        }
                    }
                });
            }
        }
    };
});




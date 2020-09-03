app.controller('setting_stylesheet', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.StylesheetEditor;
    $scope.onInit = function () {
        var StylesheetEditorID = $("textarea.stylesheet").attr("id");
        $scope.StylesheetEditor = CodeMirror.fromTextArea(document.getElementById(StylesheetEditorID), {
            lineNumbers: true,
            mode: "css",
            matchBrackets: true,
            autocorrect: true
        });
        $scope.StylesheetEditor.setSize(null, 600);
    };

    $scope.Click_Update = function () {
        common.webApi.post('stylesheet/save', '', $scope.StylesheetEditor.getValue()).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.Click_Cancel();
                //window.parent.ShowNotification('[L:CustomCSS]', '[L:StyleSheetUpdated]', 'success');
            }
        });
    };

    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});
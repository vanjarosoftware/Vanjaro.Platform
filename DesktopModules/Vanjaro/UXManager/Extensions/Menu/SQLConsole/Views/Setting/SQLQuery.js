app.controller('setting_sqlquery', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $scope.ShowTableResult = false;
        $scope.ShowMessage = false;
        var StylesheetEditorID = $("textarea.sqlscript").attr("id");
        $scope.Script = CodeMirror.fromTextArea(document.getElementById(StylesheetEditorID), {
            lineNumbers: true,
            mode: "sql",
            matchBrackets: true,
            autocorrect: true
        });
        $scope.Script.setSize(null, 290);
    };

    $scope.Click_Run = function () {
        if ($scope.Script.getValue() != "") {
            common.webApi.post('sqlquery/run', 'sqlconnection=' + $scope.ui.data.Connections.Value, $scope.Script.getValue()).then(function (Response) {
                if (Response.data.IsSuccess) {
                    $scope.Heading = [];

                    $scope.Results = Response.data.Data[0];
                    $scope.stResult = $scope.Results;
                    if ($scope.Results.length == 0) {
                        $scope.ShowTableResult = false;
                        $scope.ShowMessage = true;
                    }
                    else {
                        $scope.ShowMessage = false;
                        $scope.ShowTableResult = true;

                        for (var col in $scope.Results[0]) {
                            $scope.Heading.push(col);
                        }
                        $scope.columncount = $scope.Heading.length;
                    }
                }
                else if (Response.data.HasErrors) {
                    window.parent.swal({
                        title: 'Error',
                        text: "<div class='sweetheight'>" + Response.data.Message + "</div>",
                        html: true,
                    });
                }
            });
        }
    }
});
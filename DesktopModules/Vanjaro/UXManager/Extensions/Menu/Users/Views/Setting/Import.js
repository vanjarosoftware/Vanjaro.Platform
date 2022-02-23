app.controller('setting_import', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.ShowStep2 = false;
    $scope.ShowStep3 = false;
    $scope.ImportSecurityAction = false;
    $scope.ImportError = [];
    $scope.isDisabled = false;
    $scope.Disabled_exportToCsv = false;
    $scope.onInit = function () {
        if ($scope.ui.data.UseEmailAsUserName.Value != null && $scope.ui.data.UseEmailAsUserName.Value != '')
            $scope.ui.data.UseEmailAsUserName.Value = Boolean.parse($scope.ui.data.UseEmailAsUserName.Value);
        if ($scope.ui.data.MaximumUsersImportLimit.Value != null && $scope.ui.data.MaximumUsersImportLimit.Value != '')
            $scope.ui.data.MaximumUsersImportLimit.Value = parseInt($scope.ui.data.MaximumUsersImportLimit.Value);

        $scope.Change_ImportAction();
        $scope.SpecifyImportOptions = false;
        $('#ImportCsv>.form-group>a').hide();
        $('#ImportCsv input[type="file"]').parent().click(function () {
            $scope.ShowStep2 = true;
            $scope.ShowStep3 = false;
            $('.toggle_right.CheckDisplayName button').prop('disabled', true);
            $('.toggle_right.CheckUserName button').prop('disabled', true);
            $('.toggle_right.GeneratePassword button').prop('disabled', true);
            $scope.MappedField = [];
        });
    };

    $scope.Change_ImportAction = function () {
        var data = {
            ImportType: $scope.ui.data.ImportType.Value,
            ImportAction: $scope.ui.data.ImportAction.Value
        };
        common.webApi.post('Import/GetDataFields', '', data).then(function (data) {
            if (data.data != "" || data.data != null)
                $scope.ui.data.DataFields.Options = data.data;
        });
    };

    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

    $scope.Click_Next3 = function () {
        if ($scope.MappedField.length > 0) {
            for (var i = 0; i < $scope.MappedField.length; i++) {
                if ($scope.MappedField[i].FieldName == "DisplayName")
                    $('.toggle_right.CheckDisplayName button').prop('disabled', false);
                if ($scope.MappedField[i].FieldName == "Username")
                    $('.toggle_right.CheckUserName button').prop('disabled', false);
                if ($scope.MappedField[i].FieldName == "Password")
                    $('.toggle_right.GeneratePassword button').prop('disabled', false);
            }
        }
        $scope.SpecifyImportOptions = true;
    };

    $scope.PreviewImportData = function (IsSaveCall) {
        var ExcludeFieldName = [];
        ExcludeFieldName.push('SecurityRoles', 'Photo', 'Telephone', 'PreferredTimeZone', 'TimeZone', 'PreferredLocale', 'CreatedOnDate', 'LastModifiedOnDate', 'LastIPAddress');

        var allControls = $('#ImportCsv .container').find('select');
        if (ValidateMappings(allControls)) {
            var MapFields = [];
            $.each(allControls, function (key, Control) {
                var a;
                $.grep($scope.ui.data.DataFields.Options, function (a) {
                    if (a.Name == Control.name) {
                        if (Control.value != '') {
                            var Field = { "FieldName": Control.name, "FieldValue": Control.value, "FieldDisplayName": a.DisplayName };
                            MapFields.push(Field);
                            $scope.MappedField.push(Field);
                        }
                    }
                });
            })

            var dImport = [];
            $.each($scope.csv.result, function (key, Option) {
                var Data = {};
                $.each(MapFields, function (key, Field) {
                    Data[Field.FieldName] = Option[Field.FieldValue].replace(/"/g, '');
                })
                if (!jQuery.isEmptyObject(Data))
                    dImport.push(Data);
            })

            if (!IsSaveCall) {
                $scope.PreviewImportResult = "<div class=\"table-responsive\"><table class=\"table\"><tbody>";
                $scope.PreviewImportResult += "<tr>";
                $.each(MapFields, function (key, Field) {
                    if (!($scope.ui.data.UseEmailAsUserName.Value && Field.FieldName == 'Username') && Field.FieldName.toLocaleLowerCase() != 'firstname' && Field.FieldName.toLocaleLowerCase() != 'lastname')
                        $scope.PreviewImportResult += "<th style='width:20%;'>" + Field.FieldDisplayName + "</th>";
                })
                $scope.PreviewImportResult += "</tr>";
                var ctr = 0;
                $.each(dImport, function (key, value) {
                    ctr++;
                    $scope.PreviewImportResult += "<tr>"
                    $.each(value, function (k, d) {
                        if (!($scope.ui.data.UseEmailAsUserName.Value && k == 'Username') && k.toLocaleLowerCase() != 'firstname' && k.toLocaleLowerCase() != 'lastname')
                            $scope.PreviewImportResult += "<td>" + d + "</td>";
                    })
                    $scope.PreviewImportResult += "</tr>"
                    if (ctr == 10)
                        return false;
                })
                $scope.PreviewImportResult += "</tbody></table><p style=\"float: right;font-weight: bold;\">Showing only 10 records<p></div>";

                $scope.ShowStep2 = false;
                $scope.ShowStep3 = true;
                $scope.ImportSecurityAction = false;
            }
            else {
                $scope.isDisabled = true;
                $scope.ui.data.ImportOption.Options.GenerateDisplayNamePattern = $scope.ui.data.GenerateDisplayName.Value;
                $scope.ui.data.ImportOption.Options.GenerateUsernamePattern = $scope.ui.data.GenerateUserName.Value;

                var data = {
                    ImportOption: $scope.ui.data.ImportOption.Options,
                    ImportType: $scope.ui.data.ImportType.Value,
                    ActionType: $scope.ui.data.ImportAction.Value,
                    DImport: dImport
                }
                if (dImport.length > $scope.ui.data.MaximumUsersImportLimit.Value) {
                    window.parent.swal('[LS:MaximumUsersImportLimit]');
                    $scope.isDisabled = false;
                } else {
                    common.webApi.post('import/importdata', '', data).then(function (data) {
                        if (data.data != null && data.data.IsSuccess) {
                            Click_RefreshFrame();
                            $scope.Click_Cancel();
                        }
                        else {
                            if (data.data != null && data.data.Data != null) {
                                Click_RefreshFrame();
                                $scope.ImportError = data.data.Data;
                                $scope.ShowStep2 = false;
                                $scope.ShowStep3 = false;
                                $scope.ImportSecurityAction = false;
                                $scope.SpecifyImportOptions = false;

                                $scope.Show_ImportError = true;
                            }
                            else if (data.data != null && data.data.Message != null) {
                                window.parent.swal(data.data.Message);
                            }
                            $scope.isDisabled = false;
                        }
                    })
                }
            }
        }
    };

    var ValidateMappings = function (allControls) {
        var Result = true;
        $.each(allControls, function (key, Control) {
            if ($(Control).attr('isrequired') == 'true') {
                if ($(Control).find('option:selected').val() == '') {
                    $(Control).css("border", "2px solid red");
                    $(Control).css("outline", "none");
                    Result = false;
                    $(Control).focus();
                }
                else
                    $(Control).css("border", "");
            }
        })
        return Result;
    };

    var objectToCSVRow = function (dataObject) {
        var dataArray = new Array;
        for (var o in dataObject) {
            if (!dataObject[o].includes("$$hashKey") && !dataObject[o].includes("object:")) {
                var innerValue = dataObject[o] === null ? '' : dataObject[o].toString();
                var result = innerValue.replace(/"/g, '""');
                result = '"' + result + '"';
                dataArray.push(result);
            }
        }
        return dataArray.join(',') + '\r\n';
    }

    $scope.exportToCsv = function () {
        $scope.Disabled_exportToCsv = true;
        var arrayOfObjects = $scope.ImportError;
        if (!arrayOfObjects.length) {
            return;
        }
        var csvContent = "data:text/csv;charset=utf-8,";
        // headers
        csvContent += objectToCSVRow(Object.keys(arrayOfObjects[0]));
        arrayOfObjects.forEach(function (item) {
            csvContent += objectToCSVRow(item);
        });
        var encodedUri = encodeURI(csvContent);
        var link = document.createElement("a");
        link.setAttribute("href", encodedUri);
        link.setAttribute("download", "users.csv");
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        $scope.Disabled_exportToCsv = false;
    }

    var Click_RefreshFrame = function () {
        if (typeof parent.document.getElementById("iframe") != 'undefined' && parent.document.getElementById("iframe") != null && typeof parent.document.getElementById("iframe").src != 'undefined')
            parent.document.getElementById("iframe").src = parent.document.getElementById("iframe").src;
    }

    $scope.Click_Hide = function () {
        if ($scope.ui.data.UseEmailAsUserName.Value)
            $(document.getElementsByName("Username")).closest('div').hide();
        $(document.getElementsByName("Firstname")).closest('div').hide();
        $(document.getElementsByName("Lastname")).closest('div').hide();
    }
});
if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        return this.slice(0, str.length) == str;
    };
}
if (typeof String.prototype.endsWith != 'function') {
    String.prototype.endsWith = function (str) {
        return this.slice(-str.length) == str;
    };
}

hasObject = function (obj, key) {
    return key.split(".").every(function (x) {
        if (typeof obj != "object" || obj === null || !x in obj || typeof obj[x] === "undefined")
            return false;
        obj = obj[x];
        return true;
    });
};

function getElementByScopeID(id) {
    var elem;
    $('.ng-scope').each(function () {
        var s = angular.element(this).scope(),
            sid = s.$id;

        if (sid == id) {
            elem = this;
            return false; // stop looking at the rest
        }
    });
    return elem;
}

//var mnSvc = angular.module('mnCommonService', ['ngRoute', 'angular-loading-bar', 'smart-table', 'ngTagsInput', 'xeditable', 'oitozero.ngSweetAlert', 'ngCkeditor', 'angucomplete-alt', 'angularFileUpload', 'ngCsvImport', 'ui.tree', 'ngDialog']);
var mnSvc = angular.module('mnCommonService', ['ngRoute', 'angular-loading-bar', 'oitozero.ngSweetAlert']);

mnSvc.run(function ($rootScope, $location, CommonSvc) {

    //editableOptions.theme = 'bs3';

    // register listener to watch route changes
    $rootScope.$on("$routeChangeStart", function (event, next, current) {

        if (current) {
            var $el = $(getElementByScopeID(current.scope.$id));
            $el.parent().height($el.height());
        }

        var hasPermission = CommonSvc.isInRole(next.accessRoles);

        if (!hasPermission) {
            $location.path("unauthorized");
        }


    });

});

mnSvc.factory('CommonSvc', function ($rootScope, SweetAlert) {

    var _dataSet = {};
    var appName;
    var isInRole = function (roles) {
        var inRoles = $rootScope.roles == null ? [] : $rootScope.roles.split(',');
        var requiredRoles = (roles == null || roles == "") ? null : roles.split(',');
        var hasPermission = false;

        if (requiredRoles != null) {
            $.each(requiredRoles, function (index, perm) {

                if ($.inArray(perm, inRoles) > -1)
                    hasPermission = true;

            });
        }
        else
            hasPermission = true;

        return hasPermission;
    }

    var initData = function ($rootScope, $scope, $attrs, $http) {
        //bind the module id (key) to the scope
        $scope.moduleid = $attrs.moduleid;
        $scope.appName = $attrs.appName.trim();
        $rootScope.roles = $attrs.roles;
        $rootScope.showMissingKeys = $attrs.showMissingKeys;
        appName = $attrs.appName;
        var common = ___WebAPI.Angular($scope.moduleid, appName, $http);
        common.moduleid = $scope.moduleid;

        _dataSet[$scope.moduleid] = common;


        initUI($scope);
    }

    var getData = function ($scope) {
        return _dataSet[$scope.$parent.moduleid]; // Grab the ModuleID from Parent Scope
    }

    var initUI = function ($scope) {
        $scope.cmpCheckboxlist_ngChecked = function (Item, ItemValue, SelectedOptions) {

            var checked = false;
            if (SelectedOptions != null) {
                SelectedOptions.filter(function (obj) {

                    if (!checked)
                        checked = obj === Item[ItemValue];
                });
            }

            return checked;

        }

        $scope.cmpCheckboxlist_ngClick = function (Item, ItemValue, SelectedOptions) {

            var notfound = true;

            for (var i = 0; i < SelectedOptions.length; i++) {
                if (SelectedOptions[i] === Item[ItemValue]) {
                    SelectedOptions.splice(i, 1);
                    notfound = false;
                    i--;
                }
            }

            if (notfound)
                SelectedOptions.push(Item[ItemValue]);
        }
    }

    return {
        initData: initData,
        getData: getData,
        isInRole: isInRole,
        appName: appName,
        SweetAlert: SweetAlert
    };
});
mnSvc.directive('accessRoles', ['CommonSvc', function (CommonSvc) {
    return {
        restrict: 'A',
        link: function ($scope, $element, $attrs) {
            var hasPermission = CommonSvc.isInRole($attrs.accessRoles);

            if (hasPermission)
                $element.removeClass('d-none');
            else
                $element.addClass('d-none');
        }
    }
}]);
mnSvc.directive('stRatio', function () {
    return {
        link: function (scope, element, attr) {
            var ratio = +(attr.stRatio);

            element.css('width', ratio + '%');

        }
    };
});
mnSvc.directive('showtab',
    function () {
        return {
            link: function (scope, element, attrs) {
                element.click(function (e) {
                    e.preventDefault();
                    $(element).tab('show');
                });
            }
        };
    });
mnSvc.directive('ngaccept', function () {
    return {
        link: function (scope, element, attr) {
            if (element.attr("ngaccept") != undefined) {
                scope.$watch(element.attr("ngaccept"), function (newValue, oldValue) {
                    element.attr("accept", newValue);
                });
            }
        }
    };
});
mnSvc.directive('uiengine', ['$compile', '$timeout', 'CommonSvc', '$routeParams', '$rootScope', '$route', function ($compile, $timeout, CommonSvc, $routeParams, $rootScope, $route) {
    var linker = function ($scope, $element, $attrs) {
        var common = CommonSvc.getData($scope);
        var uienginepath = window[common.ModuleFolder + '_UIEngine' + $attrs.provider + 'Path'];
        var uitemplatepath = window[common.ModuleFolder + '_UITemplatePath'];

        var layout = 'default';
        if ($attrs.layout != undefined && $attrs.layout != "")
            layout = $attrs.layout;

        var formdata = {
            "appname": common.ModuleFolder,
            "provider": $attrs.provider,
            "identifier": $attrs.identifier,
            "uienginepath": uienginepath,
            "uitemplatepath": uitemplatepath,
            "templateurl": $route.current.templateUrl,
            "layout": layout,
            "showmissingkeys": $rootScope.showMissingKeys,
            "parameters": $routeParams,
            "uidata": '',
            "FormData": $('[name="hfFormData"]').val()
        };

        var yourDate = new Date();  // for example

        // the number of .net ticks at the unix epoch
        var epochTicks = 621355968000000000;

        // there are 10000 .net ticks per millisecond
        var ticksPerMillisecond = 10000;

        // calculate the total number of .net ticks for your date
        var yourTicks = epochTicks + (yourDate.getTime() * ticksPerMillisecond);

        common.webApi.post('ui/render', 'identifier=' + $attrs.identifier + "&v=" + yourTicks, formdata).then(function (data) {
            $scope.ui = {
                data: {},
                origData: {}
            };
            $scope.ui.data = JSON.parse(data.data.data);

            if ($attrs.autosave) {
                $scope.ui.origData = angular.copy($scope.ui.data);
                $scope.ui.hasDataChanged = !angular.equals($scope.ui.data, $scope.ui.origData);


                $scope.$watch('ui.data', debounceAutoSave, true);
            }


            $scope.Cancel = function () {
                common.webApi.get('ui/cancelurl').then(function (data) {
                    window.location.href = data.data;
                }, function (data) {
                    swal('error');
                });
            };

            $scope.PreventSearchEnter = function () {
                $('[st-search]').keypress(function (e) {
                    if (e.which == 13) return false;
                });
            };

            $scope.Submit = function () {
                var IsValid = true;

                if (typeof mnValidationService !== 'undefined' && typeof mnValidationService.DoValidationAndSubmit === 'function')
                    IsValid = mnValidationService.DoValidationAndSubmit(null, $attrs.identifier);

                if (IsValid) {
                    formdata.uidata = $scope.ui.data;
                    common.webApi.post('ui/updatedata', 'identifier=' + $attrs.identifier, formdata).then(function (data) {
                        //$scope.Cancel();
                    }, function (data) {
                        swal({
                            title: "Error!",
                            text: data.data.ExceptionMessage,
                            html: true
                        });
                    });
                }
            };

            $element.html(data.data.markup);

            //fix for ckeditor
            eval(data.data.prescript);

            $compile($element.contents())($scope);

            setTimeout(function () { if ($scope.onInit) { $scope.onInit(); $scope.PreventSearchEnter(); } eval(data.data.script); }, 2);

            if ($.isFunction($element.find('[datepicker]').datepicker)) {
                $element.find('[datepicker]').datepicker({
                    beforeShow: function (input, inst) { var $selector = $('#ui-datepicker-div'); if (!$selector.parent().hasClass('container')) $selector.wrap('<div class=\'container\' />') }
                });
            }

            $element.attr("style", "display: block !important; opacity: 0;")
            $element.closest('.view-animate').parent().css('height', 'auto');
            $element.animate({ opacity: 1 });
            initUIHelpers();

        });
        //** grid Permission
        $scope.UserremoteAPI = function (userInputString, timeoutPromise) {
            return common.webApi.get('~permissiosgrid/GetSuggestionUsers?count=10&keyword=' + userInputString);
        };


        var timeout = null;
        var debounceAutoSave = function (newVal, oldVal) {
            if (newVal != oldVal) {
                if (timeout) {
                    $timeout.cancel(timeout)
                }
                timeout = $timeout(AutoSave, 1500); // 1000 = 1 second
            }
        };
        var AutoSave = function () {
            $scope.ui.hasDataChanged = !angular.equals($scope.ui.data, $scope.ui.origData);
            if ($scope.ui.hasDataChanged) {
                $scope.Submit();
                $scope.ui.origData = angular.copy($scope.ui.data);
                $scope.ui.hasDataChanged = !angular.equals($scope.ui.data, $scope.ui.origData);
            }
        }

        // Start:Code to perform import data from csv

        $scope.PreviewImportResult = "";
        $scope.ImportCsv = false;

        $scope.GetFields = function () {
            var Rows = "";
            if ($scope.csv.result != undefined && $scope.csv.result.length > 0) {
                var DropDownOptions = "<option value=\"\"></option>";
                $.each($scope.csv.result[0], function (key, Option) {
                    DropDownOptions += "<option value='" + key + "'>" + key + "</option>";
                })
                var IsFirst = true;
                $.each($scope.ui.data.DataFields.Options, function (key, Field) {

                    if (Field.IsHeader) {
                        if (Rows != "")
                            Rows += "</div>";

                        if (IsFirst)
                            Rows += "<div class=\"panel-heading\" style=\"margin-top: 10px;border-left: 3px solid #337ab7;border-radius: 5px;\"><h4 class=\"panel-title\" style=\"text-transform: capitalize;\"><a data-toggle=\"collapse\" style=\"color: #337ab7;text-decoration: none;\" href=\"#" + Field.Name + "\" onClick=\"return false;\">" + Field.DisplayName + "</a></h4></div><div id=" + Field.Name + " class=\"panel-collapse collapse in\">";
                        else
                            Rows += "<div class=\"panel-heading\" style=\"margin-top: 10px;border-left: 3px solid #337ab7;border-radius: 5px;\"><h4 class=\"panel-title\" style=\"text-transform: capitalize;\"><a data-toggle=\"collapse\" style=\"color: #337ab7;text-decoration: none;\" href=\"#" + Field.Name + "\" onClick=\"return false;\">" + Field.DisplayName + "</a></h4></div><div id=" + Field.Name + " class=\"panel-collapse collapse\">";

                        IsFirst = false;
                    }
                    else if (!Field.IsHeader) {

                        var NameText = Field.DisplayName;
                        if (Field.IsRequired)
                            NameText = "<strong>" + Field.DisplayName + "</strong>";

                        Rows += "<div style=\"padding: 15px;\">" + NameText + ": <select name=" + Field.Name + " IsRequired=" + Field.IsRequired + " style=\"float:right;width:300px;height:37px;\">" + DropDownOptions + "</select><br><em style=\"color: #ccc;\">" + Field.SubLabel + "</em></div>";
                    }
                });
            }
            return "<div class=\"container\"><div class=\"panel-group\"><div class=\"panel panel-default\" style=\"border-color: white;\">" + Rows + "</div></div></div>";
        };

        var ValidateMappings = function (allControls) {
            var Result = true;
            $.each(allControls, function (key, Control) {
                if ($(Control).attr('isrequired') == 'true') {
                    if ($(Control).find('option:selected').val() == '') {
                        $(Control).css("border", "2px solid red");
                        Result = false;
                        $(Control).focus();
                    }
                    else
                        $(Control).css("border", "");
                }
            })
            return Result;
        };

        $scope.CancelImport = function () {
            $scope.ShowImportCsv = false;
            if ($scope.csv != undefined) {
                $scope.csv.result = "";
                $scope.csv.content = "";
                $scope.PreviewImportResult = "";
            }
            $('#ImportCsv [type="file"]').val('');
        };

        $scope.PreviewImport = function (IsSaveCall) {
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
                            }
                        }
                    });
                })

                var dImport = [];
                $.each($scope.csv.result, function (key, Option) {
                    var Data = {};
                    $.each(MapFields, function (key, Field) {
                        if (Option[Field.FieldValue] != '') {
                            Data[Field.FieldName] = Option[Field.FieldValue].replace(/"/g, '');
                        }
                    })
                    if (!jQuery.isEmptyObject(Data))
                        dImport.push(Data);
                })

                if (!IsSaveCall) {
                    $scope.PreviewImportResult = "<div class=\"table-responsive\"><table class=\"table\"><tbody>";
                    $scope.PreviewImportResult += "<tr>";
                    $.each(MapFields, function (key, Field) {
                        $scope.PreviewImportResult += "<th>" + Field.FieldDisplayName + "</th>";
                    })
                    $scope.PreviewImportResult += "</tr>";
                    var ctr = 0;
                    $.each(dImport, function (key, value) {
                        ctr++;
                        $scope.PreviewImportResult += "<tr>"
                        $.each(value, function (k, d) {
                            $scope.PreviewImportResult += "<td>" + d + "</td>";
                        })
                        $scope.PreviewImportResult += "</tr>"
                        if (ctr == 10)
                            return false;
                    })
                    $scope.PreviewImportResult += "</tbody></table><p style=\"float: right;font-weight: bold;\">Showing only 10 records<p></div>";
                }
                else {
                    $scope.ImportParameter = '';
                    if ($scope.ui.data.CsvParameter != undefined)
                        $scope.ImportParameter = $scope.ui.data.CsvParameter.Value;
                    common.webApi.post('' + $scope.ui.data.Controller.Value + '/bulkadd', $scope.ImportParameter, dImport).then(function (data) {
                        if (data.data != null && data.data.IsSuccess) {
                            location.reload();
                        }

                    })
                }
            }
        };

        $scope.MapFields = function () {
            var x = document.createElement("INPUT");
            x.setAttribute("type", "file");
            x.setAttribute("accept", ".txt");

            $(x).change(function () {
                try {
                    var reader = new FileReader();
                    reader.readAsText(x.files[0], "UTF-8");
                    reader.onload = function (evt) {
                        var Result = JSON.parse(evt.target.result);
                        $.each(Result, function (k, Value) {
                            $.each($('#ImportCsv .container').find('select'), function (key, Control) {
                                if (Value.ControlName == Control.name) {
                                    Control.value = Value.ControlValue;
                                }
                            })
                        })
                    }
                    reader.onerror = function (evt) {
                        swal("error reading file");
                    }
                }
                catch (err) {
                    swal("error reading file");
                }
            });
            x.click();
        };

        $scope.SaveMapping = function () {
            var Data = [];
            $.each($('#ImportCsv .container').find('select'), function (key, Control) {
                if (Control.value != '') {
                    var Field = { "ControlName": Control.name, "ControlValue": Control.value };
                    Data.push(Field);
                }
            })

            $.post(mnImportHandler, { 'MapData': JSON.stringify(Data) }, function (key) {
                if (key != 'error') {
                    window.location = mnImportHandler + "&guid=" + key;
                }
            })

        };

        // End:Code to perform import data from csv

        $scope.ManageSlug = function (eid, ename) {
            if (eid != undefined && ename != undefined) {
                common.webApi.get('ui/getlink', 'identifier=' + $attrs.identifier + '&entity=' + ename + '&entityid=' + eid).then(function (data) {
                    if (data.data.length > 0) {
                        if (data.data.indexOf('?') > -1) {
                            data.data += '&';
                        } else {
                            data.data += '?';
                        }
                        data.data += 'popUp=true&v=' + new Date().getTime();
                        parent.OpenPopUp(null, "1200", "center", "View History", data.data + "#!/url/manage/" + ename + "/" + eid, "600", "", false, "");
                    }
                })
            }
        };


        $scope.Click_SelectParent = function (node, nodes) {
            $.each(nodes, function (key, parent) {
                var SelectParent = function (node, parent) {
                    $.each(parent.children, function (key, child) {
                        if (child.Value == node.Value && node.selected) {
                            parent.selected = true;
                            $scope.Click_SelectParent(parent, nodes);
                        }
                        SelectParent(node, child);
                    })
                }
                SelectParent(node, parent);
            })
        };
    };

    //***********************************
    // UI HELPER METHODS
    //***********************************

    var initUIHelpers = function () {

        //Automatically select a tab if its URL matches window.location
        $('ul.nav a').filter(function () {
            return $(this).prop("hash") == window.location.hash || ($(this).attr("hash-start") && window.location.hash.startsWith($(this).attr("hash-start")))
                || ($(this).attr("hash-end") && window.location.hash.endsWith($(this).attr("hash-end")));
        }).parent().addClass('active');
    };

    return {
        restrict: 'E',
        link: linker
    };
}]);
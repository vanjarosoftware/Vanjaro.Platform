app.controller('setting_help', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
	$scope.rid = $routeParams["rid"];
	var common = CommonSvc.getData($scope);
	$scope.onInit = function () {
		$scope.HeaderText = "[L:Help]";
	};

	$scope.Click_Back = function () {
		parent.Click_Back();
	};
});



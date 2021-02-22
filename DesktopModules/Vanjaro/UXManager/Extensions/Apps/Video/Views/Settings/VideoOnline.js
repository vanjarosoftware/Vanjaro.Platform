app.controller('settings_videoonline', function ($scope, $attrs, $http, CommonSvc) {

	var common = CommonSvc.getData($scope);
	$scope.PageNo = 1;
	$scope.Videos = [];
	$scope.SearchKeyword = "";
	$scope.VideoProvidersLink = "";
	$scope.VideoProvidersImage = "";
	$scope.AdditionalData = "";

	$scope.onInit = function () {
        window.parent.window.VJIsSaveCall = false;
		$('.uiengine-wrapper a[data-target="#admin"]').removeClass("active");
		$('.uiengine-wrapper a[data-target="#videoonline"]').addClass("active");
		$scope.ChangeVideoProviders();
		$(window.parent.document.body).find('[data-bs-dismiss="modal"]').on("click", function (e) {
			$scope.SaveVideo();
		});

		$('#VjVideoKeyword').keypress(function (event) {
			var keycode = (event.keyCode ? event.keyCode : event.which);
			if (keycode == '13') {
				if ($scope.ui.data.keyword != undefined && $scope.ui.data.keyword.Value.length >= 3) {
					$scope.DebounceSearch();
				}
				event.preventDefault();
			}
		});
	};

	$scope.SaveOndblClick = function () {
		$(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
	};

	var AutoSaveTimeOutid;
	$scope.DebounceSearch = function () {
		if (AutoSaveTimeOutid) {
			clearTimeout(AutoSaveTimeOutid);
		}
		AutoSaveTimeOutid = setTimeout(function () {
			$scope.searchvideo(false);
		}, 500)

	};

	$scope.SaveVideo = function () {
        window.parent.window.VJIsSaveCall = true;
        window.parent.VjEditor.runCommand("save");
	};

	$scope.searchvideo = function (loadmorecall) {
		if ($scope.ui.data.keyword != undefined && $scope.ui.data.keyword.Value.length >= 3) {
			if ($scope.SearchKeyword != $scope.ui.data.keyword.Value) {
				$scope.Videos = [];
				$scope.PageNo = 1;
			}
			$scope.SearchKeyword = $scope.ui.data.keyword.Value;
			if (!loadmorecall)
				$scope.AdditionalData = "";
			common.webApi.post('Video/Search', 'source=' + $scope.ui.data.VideoProviders.Value + '&keyword=' + $scope.ui.data.keyword.Value + '&PageNo=' + $scope.PageNo, $scope.AdditionalData).success(function (data) {
				$.each(JSON.parse(data), function (key, value) {
					if (value.AdditionalData != null) {
						$scope.AdditionalData = value.AdditionalData;
					}
					$scope.Videos.push(value);
				});
			});
		}

	};

	$scope.ChangeVideoProviders = function () {
		$.each($scope.ui.data.VideoProviders.Options, function (k, v) {
			if (v.Value == $scope.ui.data.VideoProviders.Value) {
				if (v.ShowLogo) {
					$scope.VideoProvidersLink = v.Link;
					$scope.VideoProvidersImage = v.Logo;
				}
				else {
					$scope.VideoProvidersLink = "";
					$scope.VideoProvidersImage = "";
				}
				$scope.Videos = [];
				$scope.PageNo = 1;
				$scope.searchvideo(false);
			}
		});
	};

	$scope.ExpandMoreVideo = function () {
		var scrollimagespage = $('.vj-image-wrapper');
		$scope.PageNo = $scope.PageNo + 1;
		$scope.searchvideo(true);
		setTimeout(function () {
			var scrolllength = scrollimagespage.prop("scrollHeight") - 900;
			scrollimagespage.animate({ scrollTop: scrolllength });
		}, 2000);
	};

    $scope.SelectVideo = function (URL, save) {
        window.parent.window.VJIsSaveCall = false;
		var target = window.parent.document.vj_video_target;
		if (target != undefined) {
			var url = "";
			if ($scope.ui.data.VideoProviders.Value == "Pixabay") {
				target.set({ 'data-thumbnail': URL.Thumbnail, 'data-title': URL.Title, 'provider': 'so', 'src': URL.Url, 'videoId': '' });
				target.components().models[0].set({ 'src': URL.Url });
				target.components().models[0].addAttributes({ 'src': URL.Url });
			}
			else {
				target.set({ 'provider': 'yt', 'videoId': URL.ID });
			}
			if (save != undefined && save == true)
				window.parent.VjEditor.runCommand("save");
		}
	};
});
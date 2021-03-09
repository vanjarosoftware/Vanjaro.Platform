var Login = {
    UserLoginAPIUri: '',
    User: function (data_selector) {
        var sf = $.ServicesFramework(-1);
        var obj = $('#loginbtn');

        $(obj).closest('.Login').find('.show-message').remove();
        if (mnValidationService.DoValidationAndSubmit(data_selector)) {
            var userLogin = {};
            userLogin.Username = $(obj).closest('.Login').find('#UserId').val();
            userLogin.Password = $(obj).closest('.Login').find('#Password').val();
            userLogin.Remember = $('.login-control .rememberme').is(":checked");

            // add spinner to button
            var signinText = $($(obj).closest('.Login').find('#loginbtn')).attr('attr-localize-signin-text');
            var signinLoadingText = $($(obj).closest('.Login').find('#loginbtn')).attr('attr-localize-loading-text');
            if ($.trim(signinText) == "" || signinText == null)
                signinText = "Sign in";

            if ($.trim(signinLoadingText) == "" || signinLoadingText == null)
                signinLoadingText = "Signing in...";

            $(obj).closest('.Login').find('#loginbtn').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> ' + signinLoadingText);
            $(obj).closest('.Login').find('#loginbtn').addClass('disabled');


            $.ajax({
                type: "POST",
                url: Login.UserLoginAPIUri,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(userLogin),
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    Login.processResponse(response);
                }
            });
        }
    },
    processResponse: function (response) {

        var obj = $('#loginbtn');
        var signinText = $($(obj).closest('.Login').find('#loginbtn')).attr('attr-localize-signin-text');
        if ($.trim(signinText) == "" || signinText == null)
            signinText = "Sign in";

        if (response.IsSuccess) {
            if (response.IsRedirect) {
                window.location.href = response.RedirectURL;
            }
            else {
                $(obj).closest('.Login').find('#loginbtn').html(signinText);
                $(obj).closest('.Login').find('#loginbtn').removeClass('disabled');
            }
        }

        if (response.HasErrors) {


            if (response.Errors["MUSTAGREETOTERMS"] != null && typeof response.Errors["MUSTAGREETOTERMS"] != "undefined") {
                $(obj).closest('.Login').find('#loginbtn').html(signinText);
                $('<div class="show-message alert alert-warning">' + response.Message + '</div>').insertAfter($(obj).closest('.login-container').find('.dataconsent').find('.loginhead'));
                $(obj).closest('.Login').find('#loginbtn').removeClass('disabled');
                $(obj).closest('.Login').find('.login-control').hide();
                $(obj).closest('.login-container').find('.form-Login').hide();
                $(obj).closest('.login-container').find('.dataconsent').show();
            }
            else if (response.Errors["ProfileUpdate"] != null && typeof response.Errors["ProfileUpdate"] != "undefined") {
                $(obj).closest('.Login').find('#loginbtn').html(signinText);
                $(obj).closest('.Login').find('#loginbtn').removeClass('disabled');
                if (response.Data != null && response.Data.UserExtensionURL != null) {
                    parent.OpenPopUp(null, 800, 'right', null, response.Data.UserExtensionURL);
                }
            }
            else {
                $(obj).closest('.Login').find('#loginbtn').html(signinText);
                $('<div class="show-message alert alert-danger">' + response.Message + '</div>').insertAfter($(obj).closest('.Login').find('.loginhead'));
                $(obj).closest('.Login').find('#loginbtn').removeClass('disabled');
            }

        }
    },
    ResetPassword: function (obj) {
        var Email = $(obj).closest('.ResetPassword').find('.Email').val();
        var sf = $.ServicesFramework(-1);
        $(obj).closest('.ResetPassword').find('.show-message').remove();
        if (mnValidationService.DoValidationAndSubmit('ResetPassword')) {
            // add spinner to button
            var resetText = $($(obj).closest('.ResetPassword').find('#btnReset')).attr('attr-localize-reset-text');
            if ($.trim(resetText) == "" || resetText == null)
                resetText = "Reset Password";

            $(obj).closest('.ResetPassword').find('#btnReset').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> ' + resetText);
            $(obj).closest('.ResetPassword').find('#btnReset').addClass('disabled');
            $.ajax({
                type: "POST",
                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Login") + "Login/OnSendPasswordClick?" + "Email=" + Email,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    if (response.IsSuccess) {
                        $('<div class="show-message alert alert-success">' + response.Message + '</div>').insertAfter($(obj).closest('.ResetPassword').find('.Resethead'));
                        $(obj).closest('.ResetPassword').find('.Email').val('');
                        $(obj).closest('.ResetPassword').find('.reset-control').hide();
                        $(obj).closest('.ResetPassword').find('.Signinbtn').show();
                        if (response.IsRedirect) {
                            window.parent.location.href = response.RedirectURL;
                        }
                        else {
                            $(obj).closest('.ResetPassword').find('#btnReset').html(resetText);
                            $(obj).closest('.ResetPassword').find('#btnReset').removeClass('disabled');
                        }
                    }
                    else {
                        $(obj).closest('.ResetPassword').find('#btnReset').html(resetText);
                        $('<div class="show-message alert alert-danger">' + response.Message + '</div>').insertAfter($(obj).closest('.ResetPassword').find('.Resethead'));
                        $(obj).closest('.ResetPassword').find('#btnReset').removeClass('disabled');
                    }
                }
            });

        }
    },

    DataConsentcheck: function () {
        if ($('.checktermcondition').is(':checked')) {
            $('.submit a').removeClass('disabled');
        }
        else {
            $('.submit a').addClass('disabled');
        }
    },

    DataConsentSubmit: function (data_selector, obj) {
        var sf = $.ServicesFramework(-1);
        if (mnValidationService.DoValidationAndSubmit('')) {
            // add spinner to button
            var signinText = $($(obj).closest('.dataconsent').find('.signbtn')).attr('attr-localize-signin-text');
            var signinLoadingText = $($(obj).closest('.dataconsent').find('.signbtn')).attr('attr-localize-loading-text');
            if ($.trim(signinText) == "" || signinText == null)
                signinText = "Sign in";

            if ($.trim(signinLoadingText) == "" || signinLoadingText == null)
                signinLoadingText = "Signing in...";

            $(obj).closest('.dataconsent').find('.signbtn').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> ' + signinLoadingText);
            $(obj).closest('.dataconsent').find('.signbtn').addClass('disabled');

            var userLogin = {};
            userLogin.Username = $(obj).closest('.dataconsent').parents('.login-container').find('.Login').find('#UserId').val();
            userLogin.Password = $(obj).closest('.dataconsent').parents('.login-container').find('.Login').find('#Password').val();
            userLogin.Remember = $(obj).closest('.dataconsent').parents('.login-container').find('.Login').find('.rememberme').is(":checked");
            userLogin.HasAgreedToTerms = true;

            $.ajax({
                type: "POST",
                url: Login.UserLoginAPIUri,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(userLogin),
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    if (response.IsSuccess) {
                        if (response.IsRedirect) {
                            window.parent.location.href = response.RedirectURL;
                        }
                    }
                    else {
                        $(obj).closest('.dataconsent').find('.signbtn').html(signinText);
                        $(obj).closest('.dataconsent').find('.signbtn').removeClass('disabled');
                    }
                },
                error: function (textStatus, errorThrown) {
                    $(obj).closest('.Login').find('#loginbtn').html(errorThrown);
                    $('<div class="show-message alert alert-danger">' + response.Message + '</div>').insertAfter($(obj).closest('.Login').find('.loginhead'));
                    $(obj).closest('.Login').find('#loginbtn').removeClass('disabled');
                }
            });
        }
    },

    DataConsentCancel: function () {
        var sf = $.ServicesFramework(-1);
        if (mnValidationService.DoValidationAndSubmit('')) {
            $.ajax({
                type: "POST",
                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Login") + "Login/DataConsentCancel",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    if (response.IsSuccess) {
                        window.parent.location.href = response.RedirectURL;
                        if (response.IsRedirect) {
                            window.parent.location.href = response.RedirectURL;
                        }
                    }
                }
            });
        }
    },

    DataConsentDeleteMe: function () {
        var sf = $.ServicesFramework(-1);
        if (mnValidationService.DoValidationAndSubmit('')) {
            var userLogin = {};
            userLogin.Username = $('.Login').find('#UserId').val();
            userLogin.Password = $('.Login').find('#Password').val();
            userLogin.Remember = $('.login-control .rememberme').is(":checked");
            $.ajax({
                type: "POST",
                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Login") + "Login/DataConsentDeleteMe",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(userLogin),
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    if (response.IsSuccess) {
                        window.parent.location.href = response.RedirectURL;
                        if (response.IsRedirect) {
                            window.parent.location.href = response.RedirectURL;
                        }
                    }
                }
            });
        }
    },

    ShowResetPassword: function (obj) {
        $(obj).closest('.Login').find('.show-message').remove();
        $(obj).closest('.Login').find('#Password').val('');
        $(".login-box").hide();
        $(".ResetPassword").show();
        $(".reset-control").show();
    },
    HideResetPassword: function (obj) {
        $(".login-box").show();
        $(".ResetPassword").hide();
        $(obj).closest('.ResetPassword').find('.show-message').remove();
    }
};

$(document).ready(function () {
    $('.Login').keydown(function (e) {
        if (e.which == 13) {
            $('#loginbtn').focus().click();
            return false;
        }
    });

    setTimeout(function () { Login.UserLoginAPIUri = window.location.origin + $.ServicesFramework(-1).getServiceRoot("Login") + "Login/UserLogin" + window.location.search; });

});
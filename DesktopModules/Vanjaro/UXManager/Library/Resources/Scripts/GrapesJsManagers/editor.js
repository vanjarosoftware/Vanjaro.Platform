import grapesjs from 'grapesjs';
import { jsPanel } from 'jspanel4/es6module/jspanel.js';

global.VjEditor = null;
global.VjLayerpanel = null;
global.VJLandingPage = { components: '', html: '', style: '', css: '' };
global.VJIsSaveCall = true;
global.VJLocalBlocksMarkup = '';
global.GrapesjsInit;
global.CurrentExtTabUrl = '';
global.IsVJEditorSaveCall;

$(document).ready(function () {

    var Responsive = VjLocalized.Responsive.replace(/ /g, '_').toLowerCase();
    var Filters = VjLocalized.Filters.replace(/ /g, '_').toLowerCase();
    var Transform = VjLocalized.Transform.replace(/ /g, '_').toLowerCase();
    var Transitions = VjLocalized.Transitions.replace(/ /g, '_').toLowerCase();
    var Size = VjLocalized.Size.replace(/ /g, '_').toLowerCase();
    var Border = VjLocalized.Border.replace(/ /g, '_').toLowerCase();
    var BorderRadius = VjLocalized.BorderRadius.replace(/ /g, '_').toLowerCase();
    var Text = VjLocalized.Text.replace(/ /g, '').toLowerCase();
    var BackgroundShadow = VjLocalized.BackgroundShadow.replace(/ /g, '_').toLowerCase();
    var Position = VjLocalized.Position.replace(/ /g, '_').toLowerCase();
    var Margin = VjLocalized.Margin.replace(/ /g, '_').toLowerCase();
    var Padding = VjLocalized.Padding.replace(/ /g, '_').toLowerCase();
    var Extra = VjLocalized.Extra.replace(/ /g, '_').toLowerCase();

    if (window.parent.CurrentTabUrl.indexOf('?') > 0)
        CurrentExtTabUrl = window.parent.CurrentTabUrl + '&mid=0&icp=true';
    else
        CurrentExtTabUrl = window.parent.CurrentTabUrl + '?mid=0&icp=true';

    if (typeof TemplateLibraryURL != 'undefined' && TemplateLibraryURL != '') {

        window.addEventListener('message', event => {

            if (TemplateLibraryURL.startsWith(event.origin)) {

                if (typeof event.data != 'undefined') {

                    if (typeof event.data.action != 'undefined') {

                        if (event.data.action == 'preview-open')
                            $('#UXpagerender').addClass('preview-open')
                        else
                            $('#UXpagerender').removeClass('preview-open');
                    }
                    else {
                        var templatePath = '';
                        var templateHash = '';
                        var LibraryUrl = TemplateLibraryURL.split('/templates/')[0];
                        if (event.origin.startsWith(LibraryUrl) && event.data != undefined) {
                            if (typeof event.data.path != 'undefined') {
                                if (!event.data.path.startsWith(LibraryUrl)) {
                                    templatePath = LibraryUrl + event.data.path;
                                    templateHash = event.data.hash;
                                }
                                else {
                                    templatePath = event.data.path;
                                    templateHash = event.data.hash;
                                }
                            }
                        }
                        var sf = $.ServicesFramework(-1);
                        $.ajax({
                            type: "Post",
                            url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/ImportCustomBlock?TemplateHash=" + templateHash + "&TemplatePath=" + templatePath,
                            headers: {
                                'ModuleId': parseInt(sf.getModuleId()),
                                'TabId': parseInt(sf.getTabId()),
                                'RequestVerificationToken': sf.getAntiForgeryValue()
                            },
                            success: function (data) {

                                if (data.Html != undefined && data.Html.length > 0) {
                                    $.each($(data.Html).find('[mid]'), function (k, v) {
                                        var mid = $(v).attr('mid');
                                        var framesrc = CurrentTabUrl;
                                        if (framesrc.indexOf("?") == -1)
                                            framesrc = framesrc + "?mid=" + mid + "&icp=true";
                                        else
                                            framesrc = framesrc + "&mid=" + mid + "&icp=true";
                                        data.Html = data.Html.replace('<app id="' + mid + '"></app>', '<div id="dnn_vj_' + mid + '"><img class="centerloader" src="' + VjDefaultPath + 'loading.gif" /><iframe scrolling="no" onload="window.parent.RenderApp(this);" src="' + framesrc + '" style="width:100%;height:auto;"></iframe></div>');
                                    });
                                    var LibraryBlock = VjEditor.BlockManager.add('LibraryBlock', {
                                        content: data.Html + '<style>' + data.Css + '</style>',
                                        attributes: {
                                            class: 'fas fa-th-large floating',
                                            id: 'LibraryBlock'
                                        }
                                    });

                                    var block = VjEditor.BlockManager.render(LibraryBlock);
                                    $(window.document.body).append(block).find('[data-dismiss="modal"]').trigger('click', [false]);
                                }
                            }
                        });
                    }
                }
            }
            else if (ExtensionStoreURL.startsWith(event.origin)) {
                if (typeof event.data != 'undefined') {
                    if (typeof event.data.action != 'undefined' && event.data.action == 'DownloadPackage') {
                        var sf = $.ServicesFramework(-1);
                        var PackageData = {
                            Packages: JSON.stringify(event.data.data)
                        };
                        $.ajax({
                            type: "POST",
                            url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Extensions") + "InstallPackage/Download",
                            data: PackageData,
                            headers: {
                                'ModuleId': parseInt(sf.getModuleId()),
                                'TabId': parseInt(sf.getTabId()),
                                'RequestVerificationToken': sf.getAntiForgeryValue()
                            },
                            success: function (data) {
                                $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                                parent.OpenPopUp(null, 600, 'center', 'Install', ExtensionURL, 800);
                            }
                        });
                    }
                }
            }
        });
    }

    $(".pubish-btn").click(function (e) {
        e.preventDefault();

        var VjPublishChanges = function () {
            if (VJIsContentApproval == 'True') {
                swal(
                    {
                        title: VjLocalized.PublishAreyousure, text: VjLocalized.PublishCommentlable + ' <span class="SweetAlertBold">' + VJNextStateName + '</span> <textarea class="form-control" id="VJReviewComment" rows="4" style="width: 100%;margin-top: 20px;" type="text" tabindex="3" placeholder="Comments"></textarea>', html: true, confirmButtonText: VjLocalized.Submit, cancelButtonText: VjLocalized.Cancel, showCancelButton: true, closeOnConfirm: false, animation: "slide-from-top", inputPlaceholder: VjLocalized.EnterComment
                    },
                    function (inputValue) {
                        if ($('#VJReviewComment', parent.document).val() != '') {
                            editor.StorageManager.getStorages().remote.attributes.params.Comment = $('#VJReviewComment', parent.document).val();
                            RunSaveCommand();
                            $('#VJBtnPublish').addClass('disabled');
                            $('.gjs-cv-canvas__frames').addClass('lockcanvas');
                            swal.close();
                            setTimeout(function () { RenderMarkup('Register Link'); }, 300);

                        }
                        else {
                            swal.showInputError(VjLocalized.Commentisrequired);
                            return false;
                        }
                    });
            }
            else
                RunSaveCommand();
        };

        var waitForEl = function (selector, size, callback) {
            //Added math.round to fix OptimizeImages issue on small screens
            if (jQuery(selector).length && Math.round(jQuery(selector).width()) == size) {
                callback();
            } else {
                setTimeout(function () {
                    waitForEl(selector, size, callback);
                }, 10);
            }
        };
        var calcSizes = '';
        var resolutionSizes = [1920, 1600, 1366, 1200, 768, 320];
        var OptimizeImages = function (optImages, sizes) {

            if (!$('.optimizing-overlay').length)
                $('.vj-wrapper').prepend('<div class="optimizing-overlay"><h1><span class="spinner-border text-light" role="status"></span>&nbsp;&nbsp;Optimizing Images . . .</h1></div>');

            if (typeof optImages != 'undefined' && optImages.length > 0) {

                var image = optImages[0];

                if (typeof sizes != 'undefined' && sizes.length > 0) {

                    var size = sizes.shift();

                    $('.gjs-frame').width(size).css('transition', 'none');

                    waitForEl('.gjs-frame', size, function () {

                        //Calc Sizes
                        var imgWidth = $(image.getEl()).width();
                        var calcWidth = Math.round((imgWidth / size) * 100);
                        calcSizes += '(min-width:' + size + 'px) ' + calcWidth + 'vw,';

                        OptimizeImages(optImages, sizes);
                    });
                }
                else {

                    //Set Sizes
                    //Trim last character of calcSizes to remove trailing comma
                    image.parent().components().models[0].addAttributes({ 'sizes': calcSizes.slice(0, -1) });
                    image.parent().components().models[1].addAttributes({ 'sizes': calcSizes.slice(0, -1) });

                    calcSizes = '';

                    optImages.shift();
                    OptimizeImages(optImages, resolutionSizes.slice());
                }
            }
            else {

                $('.gjs-frame').removeAttr('style');

                setTimeout(function () {
                    $('.vj-wrapper').find('.optimizing-overlay').remove();
                    VjPublishChanges();
                }, 500);
            }
        };
        var optImages = jQuery.grep(getAllComponents(), function (n, i) {
            return (n.attributes.type == 'image') && (typeof n.getStyle()['width'] == 'undefined') && (n.parent().attributes.type == 'picture-box') && (typeof n.parent().components().models[0] != 'undefined') && (typeof n.parent().components().models[1] != 'undefined');
        });

        if (optImages != undefined && optImages.length > 0) {

            if (!$('.optimizing-overlay').length)
                $('.vj-wrapper').prepend('<div class="optimizing-overlay"><h1><span class="spinner-border text-light" role="status"></span>&nbsp;&nbsp;Optimizing Images . . .</h1></div>');

            if ($('.gjs-frame').contents().find("html").hasClass('responsive'))
                $(".device-manager .device-view#Desktop").trigger("click");

            waitForEl('.gjs-frame-wrapper', VjEditor.Canvas.getCanvasView().$el.innerWidth(), function () {
                OptimizeImages(optImages, resolutionSizes.slice());
            });

        }
        else
            VjPublishChanges();

        VjEditor.UndoManager.stop();
        VjEditor.UndoManager.clear();
        setTimeout(function () {
            $(window.parent.document.body).find('#VJReviewComment').focus();
        }, 100);
    });

    var VJAutoSaveTimeOutid;

    setTimeout(function () {
        if ($("#mode-switcher").length > 0) {
            if ($("#mode-switcher").offset().left <= 0) {
                $('.sidebar').addClass('settingclosebtn');
            }
            else {
                $('.sidebar').removeClass('settingclosebtn');
            }
        }
    }, 500);

    if (typeof VJIsPageDraft !== 'undefined' && VJIsPageDraft != undefined && VJIsPageDraft == "True") {
        $('#VJBtnPublish').removeClass('disabled');
    }

    global.debounce = function (func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };

	global.GrapesjsInit = function () {

	

		if (isEditPage()) {

			if (!vjEditorSettings.EditPage) {

                $(document).on("click", function (e) {
                    if ($(e.target).parents('.sidebar').length <= 0) {
                        VjEditor.select();
                        ShowBlockUI();
                    }
                });

				$('#vjEditor,' + vjEditorSettings.ContainerID).scroll(function () {
					debounce(VjEditor.refresh(), 100);
				});
			}

            if (GetParameterByName('m2v', parent.window.location) != null && GetParameterByName('m2v', parent.window.location).startsWith('true')) {
                $(window.parent.document.body).find('#dnn_ContentPane').prepend('<div class="optimizing-overlay"><h1><span class="spinner-border text-light" role="status"></span>&nbsp;&nbsp;Please Wait . . .</h1></div>');
            }

            if ($('#dnn_ContentPane').length > 0)
                $('#dnn_ContentPane').addClass("sidebar-open");

			$(window.parent.document.body).find('.vj-wrapper').removeClass("m2vDisplayNone");
			if ($.isFunction($.ServicesFramework)) {
				var sf = $.ServicesFramework(-1);
				if (parseInt(sf.getTabId()) <= 0)
					sf = parent.$.ServicesFramework(-1);
				$.ajax({
					type: "GET",
					url: eval(vjEditorSettings.GetContentUrl),
					headers: {
						'ModuleId': parseInt(vjEditorSettings.ModuleId),
						'TabId': parseInt(sf.getTabId()),
						'RequestVerificationToken': sf.getAntiForgeryValue()
					},
					success: function (response) {
						if (response != undefined) {
							VJLandingPage.components = response.ContentJSON;
							VJLandingPage.html = response.Content;
							VJLandingPage.style = response.StyleJSON;
							VJLandingPage.css = response.Style;
						}
						if (VJLandingPage != undefined) {
							$("#mode-switcher").find("em").addClass("fa-chevron-left").removeClass("fa-chevron-right");
							$('.sidebar, #dnn_ContentPane').addClass("sidebar-open").removeClass("sidebar-close");
							BindLinksAndScripts();
							VjLayerpanel = jsPanel.create({
								id: 'layer-manager',
								theme: 'dark',
								headerTitle: VjLocalized.Navigator,
								position: 'right-top -320 20',
								resizeit: false,
								contentSize: {
									width: '300',
								},
								headerControls: {
									minimize: 'remove',
									normalize: 'remove',
									maximize: 'remove',
									smallify: 'remove',
									reset: 'remove',
									add: {
										html: '<span class="panelclosebtn"><em class="fas fa-times"></em></span>',
										name: 'Closebtn',
										handler: function (panel, control) {
											$(".jsPanel-btn-Closebtn").click(function () {
												$('#layer-manager').hide();
											});
										},
									}
								},

                                onclosed: function () {
                                    VjLayerpanel = null;
                                },
                                callback: function () {
                                    this.content.style.padding = '0';
                                },
                            });
                            var vjcomps = eval(VJLandingPage.components);
                            BuildAppComponent(vjcomps);
                            BuildAppComponentFromHtml(vjcomps, VJLandingPage.html);
                            BuildBlockComponent(vjcomps);
                            vjcomps = FilterComponents(vjcomps);
                            if (typeof LoadThemeBlocks != 'undefined')
                                LoadThemeBlocks(grapesjs);
							VjEditor = grapesjs.init({
								protectedCss: '',
								allowScripts: 1,
								panels: {
									defaults: []
								},
								canvas: {
									styles: VjLinks, scripts: VjScriptTags
								},
								modal: {
									backdrop: 0
								},
								components: vjcomps || VJLandingPage.html,
								style: eval(VJLandingPage.style) || VJLandingPage.css,
								showOffsets: 1,
								avoidInlineStyle: 1,
								noticeOnUnload: 0,
								container: vjEditorSettings.ContainerID,
								height: '100%',
								fromElement: vjcomps != undefined ? false : true,
								plugins: ['modulewrapper', 'blockwrapper', 'vjpreset', 'ThemeBlocks'],
								pluginsOpts: {
									'vj-preset': {
										colorPicker: 'default',
										grapickOpts: {
											min: 1,
											max: 99,
										}
									}
								},
								selectorManager: {
									appendTo: '.stylemanager',
								},
								styleManager: {
									clearProperties: true,
									appendTo: '.stylemanager',
									sectors: [{
										name: VjLocalized.Margin,
										open: false,
										properties: [
											{
												type: 'customrange',
												name: 'Margin Left',
												property: 'margin-left',
												units: [
													{ name: 'px', min: -50, max: 200, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: -100, max: 100, step: 1 },
													{ name: 'vh', min: -100, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Margin Right',
												property: 'margin-right',
												units: [
													{ name: 'px', min: -50, max: 200, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: -100, max: 100, step: 1 },
													{ name: 'vh', min: -100, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Margin Top',
												property: 'margin-top',
												units: [
													{ name: 'px', min: -50, max: 200, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: -100, max: 100, step: 1 },
													{ name: 'vh', min: -100, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Margin Bottom',
												property: 'margin-bottom',
												units: [
													{ name: 'px', min: -50, max: 200, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: -100, max: 100, step: 1 },
													{ name: 'vh', min: -100, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											}
										]
									}, {
										name: VjLocalized.Padding,
										open: false,
										properties: [
											{
												type: 'customrange',
												name: 'Padding Left',
												property: 'padding-left',
												units: [
													{ name: 'px', min: 0, max: 200, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Padding Right',
												property: 'padding-right',
												units: [
													{ name: 'px', min: 0, max: 200, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Padding Top',
												property: 'padding-top',
												units: [
													{ name: 'px', min: 0, max: 200, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Padding Bottom',
												property: 'padding-bottom',
												units: [
													{ name: 'px', min: 0, max: 200, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											}
										]
									}, {
										name: VjLocalized.Size,
										open: false,
										properties: [
											{
												type: 'customrange',
												name: 'Width',
												property: 'width',
												units: [
													{ name: 'px', min: 0, max: 1920, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Min Width',
												property: 'min-width',
												units: [
													{ name: 'px', min: 0, max: 1920, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Max Width',
												property: 'max-width',
												units: [
													{ name: 'px', min: 0, max: 1920, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Height',
												property: 'height',
												units: [
													{ name: 'px', min: 0, max: 1080, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Min Height',
												property: 'min-height',
												units: [
													{ name: 'px', min: 0, max: 1080, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Max Height',
												property: 'max-height',
												units: [
													{ name: 'px', min: 0, max: 1080, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}
										]
									}, {
										name: VjLocalized.Responsive,
										open: false,
										properties: [
											{
												type: 'customradio',
												name: 'Hide in Mobile',
												property: 'd-mobile-none',
												defaults: 'd-mobile-show',
												list: [{
													name: 'Yes',
													value: 'd-mobile-none'
												},
												{
													name: 'No',
													value: 'd-mobile-show'
												}],
											}, {
												type: 'customradio',
												name: 'Hide in Tablet',
												property: 'd-tablet-none',
												defaults: 'd-tablet-show',
												list: [{
													name: 'Yes',
													value: 'd-tablet-none'

                                                },
                                                {
                                                    name: 'No',
                                                    value: 'd-tablet-show'

												}],
											}, {
												type: 'customradio',
												name: 'Hide in Desktop',
												property: 'd-desktop-none',
												defaults: 'd-desktop-show',
												list: [{
													name: 'Yes',
													value: 'd-desktop-none'
												},
												{
													name: 'No',
													value: 'd-desktop-show'
												}],
											}]
									}, {
										name: VjLocalized.Border,
										open: false,
										properties: [
											{
												type: 'customradio',
												name: 'Border Postion',
												property: 'border-position',
												defaults: 'sm-border',
												list: [{
													name: 'Border',
													img: 'border.svg',
													value: 'sm-border',
												}, {
													name: 'Border Top',
													img: 'border-top.svg',
													value: 'sm-border-top',
												}, {
													name: 'Border Right',
													img: 'border-right.svg',
													value: 'sm-border-right',
												}, {
													name: 'Border Bottom',
													img: 'border-bottom.svg',
													value: 'sm-border-bottom',
												}, {
													name: 'Border Left',
													img: 'border-left.svg',
													value: 'sm-border-left',
												}],
											},
											{
												type: 'customradio',
												name: 'Border Style',
												property: 'border-style',
												defaults: 'none',
												list: [{
													value: 'solid',
													name: 'Solid',
													img: 'border-solid.png'
												}, {
													value: 'double',
													name: 'Double',
													img: 'border-double.png'
												}, {
													value: 'dotted',
													name: 'Dotted',
													img: 'border-dotted.png'
												}, {
													value: 'dashed',
													name: 'Dashed',
													img: 'border-dashed.png'
												}],
												UpdateStyles: true,
											},
											{
												type: 'color',
												name: 'Color',
												property: 'border-color',
												cssproperty: 'border-color',
												defaults: 'rgb(52, 58, 64)',
											},
											{
												type: 'customrange',
												name: 'Width',
												property: 'border-width',
												units: [
													{ name: 'px', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customradio',
												name: 'Border Style',
												property: 'border-top-style',
												defaults: 'none',
												list: [{
													value: 'solid',
													name: 'Solid',
													img: 'border-solid.png'
												}, {
													value: 'double',
													name: 'Double',
													img: 'border-double.png'
												}, {
													value: 'dotted',
													name: 'Dotted',
													img: 'border-dotted.png'
												}, {
													value: 'dashed',
													name: 'Dashed',
													img: 'border-dashed.png'
												}],
												UpdateStyles: true,
											},
											{
												type: 'color',
												name: 'Color',
												property: 'border-top-color',
												cssproperty: 'border-color',
												defaults: 'rgb(52, 58, 64)',
											},
											{
												type: 'customrange',
												name: 'Width',
												property: 'border-top-width',
												units: [
													{ name: 'px', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customradio',
												name: 'Border Style',
												property: 'border-right-style',
												defaults: 'none',
												list: [{
													value: 'solid',
													name: 'Solid',
													img: 'border-solid.png'
												}, {
													value: 'double',
													name: 'Double',
													img: 'border-double.png'
												}, {
													value: 'dotted',
													name: 'Dotted',
													img: 'border-dotted.png'
												}, {
													value: 'dashed',
													name: 'Dashed',
													img: 'border-dashed.png'
												}],
												UpdateStyles: true,
											},
											{
												type: 'color',
												name: 'Color',
												property: 'border-right-color',
												cssproperty: 'border-color',
												defaults: 'rgb(52, 58, 64)',
											},
											{
												type: 'customrange',
												name: 'Width',
												property: 'border-right-width',
												units: [
													{ name: 'px', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customradio',
												name: 'Border Style',
												property: 'border-bottom-style',
												defaults: 'none',
												list: [{
													value: 'solid',
													name: 'Solid',
													img: 'border-solid.png'
												}, {
													value: 'double',
													name: 'Double',
													img: 'border-double.png'
												}, {
													value: 'dotted',
													name: 'Dotted',
													img: 'border-dotted.png'
												}, {
													value: 'dashed',
													name: 'Dashed',
													img: 'border-dashed.png'
												}],
												UpdateStyles: true,
											},
											{
												type: 'color',
												name: 'Color',
												property: 'border-bottom-color',
												cssproperty: 'border-color',
												defaults: 'rgb(52, 58, 64)',
											},
											{
												type: 'customrange',
												name: 'Width',
												property: 'border-bottom-width',
												units: [
													{ name: 'px', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customradio',
												name: 'Border Style',
												property: 'border-left-style',
												defaults: 'none',
												list: [{
													value: 'solid',
													name: 'Solid',
													img: 'border-solid.png'
												}, {
													value: 'double',
													name: 'Double',
													img: 'border-double.png'
												}, {
													value: 'dotted',
													name: 'Dotted',
													img: 'border-dotted.png'
												}, {
													value: 'dashed',
													name: 'Dashed',
													img: 'border-dashed.png'
												}],
												UpdateStyles: true,
											},
											{
												type: 'color',
												name: 'Color',
												property: 'border-left-color',
												cssproperty: 'border-color',
												defaults: 'rgb(52, 58, 64)',
											},
											{
												type: 'customrange',
												name: 'Width',
												property: 'border-left-width',
												units: [
													{ name: 'px', min: 0, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 }
												],
												unit: 'px',
												defaults: 0,
											}
										]
									}, {
										name: VjLocalized.BorderRadius,
										open: false,
										properties: [
											{
												type: 'customrange',
												name: 'Top Left',
												property: 'border-top-left-radius',
												units: [
													{ name: 'px', min: 0, max: 50, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Top Right',
												property: 'border-top-right-radius',
												units: [
													{ name: 'px', min: 0, max: 50, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Bottom Left',
												property: 'border-bottom-left-radius',
												units: [
													{ name: 'px', min: 0, max: 50, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 0,
											},
											{
												type: 'customrange',
												name: 'Bottom Right',
												property: 'border-bottom-right-radius',
												units: [
													{ name: 'px', min: 0, max: 50, step: 1 },
													{ name: '%', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 0,
											}
										]
									}, {
										name: VjLocalized.BackgroundShadow,
										open: false,
										buildProps: ['background-color', 'background', 'box-shadow'],
									}, {
										name: VjLocalized.Position,
										open: false,
										buildProps: ['z-index', 'position', 'top', 'left', 'bottom', 'right'],
										properties: [
											{
												type: 'customrange',
												name: 'Z Index',
												property: 'z-index',
												step: 1,
												min: -10,
												max: 999,
												defaults: 0,
											}, {
												type: 'customrange',
												name: 'Top',
												property: 'top',
												units: [
													{ name: 'px', min: -200, max: 400, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Left',
												property: 'left',
												units: [
													{ name: 'px', min: -200, max: 400, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Bottom',
												property: 'bottom',
												units: [
													{ name: 'px', min: -200, max: 400, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}, {
												type: 'customrange',
												name: 'Right',
												property: 'right',
												units: [
													{ name: 'px', min: -200, max: 400, step: 1 },
													{ name: '%', min: -100, max: 100, step: 1 },
													{ name: 'vw', min: 0, max: 100, step: 1 },
													{ name: 'vh', min: 0, max: 100, step: 1 },
												],
												unit: 'px',
												defaults: 'auto',
											}
										]
									}, {
										name: VjLocalized.Transform,
										open: false,
										properties: [
											{
												type: 'custom_slider',
												name: 'Transform Rotate X',
												property: 'rotateX',
												cssproperty: 'transform',
												step: 1,
												min: 0,
												max: 360,
												unit: 'deg',
												units: ['deg'],
												defaults: 0
											},
											{
												type: 'custom_slider',
												name: 'Transform Rotate Y',
												property: 'rotateY',
												cssproperty: 'transform',
												step: 1,
												min: 0,
												max: 360,
												unit: 'deg',
												units: ['deg'],
												defaults: 0
											},
											{
												type: 'custom_slider',
												name: 'Transform Rotate Z',
												property: 'rotateZ',
												cssproperty: 'transform',
												step: 1,
												min: 0,
												max: 360,
												unit: 'deg',
												units: ['deg'],
												defaults: 0
											},
											{
												type: 'custom_slider',
												name: 'Transform Scale X',
												property: 'scaleX',
												cssproperty: 'transform',
												step: 0.1,
												min: 0,
												max: 10,
												defaults: 1
											},
											{
												type: 'custom_slider',
												name: 'Transform Scale Y',
												property: 'scaleY',
												cssproperty: 'transform',
												step: 0.1,
												min: 0,
												max: 10,
												defaults: 1
											},
											{
												type: 'custom_slider',
												name: 'Transform Scale Z',
												property: 'scaleZ',
												cssproperty: 'transform',
												step: 0.1,
												min: 0,
												max: 10,
												defaults: 1
											},
											{
												type: 'custom_slider',
												name: 'Transform Skew X',
												property: 'skewX',
												cssproperty: 'transform',
												step: 1,
												min: 0,
												max: 360,
												unit: 'deg',
												units: ['deg'],
												defaults: 0
											},
											{
												type: 'custom_slider',
												name: 'Transform Skew Y',
												property: 'skewY',
												cssproperty: 'transform',
												step: 1,
												min: 0,
												max: 360,
												unit: 'deg',
												units: ['deg'],
												defaults: 0
											}
										]
									}, {
										name: VjLocalized.Transitions,
										open: false,
										buildProps: ['transition'],
									}, {
										name: VjLocalized.Filters,
										open: false,
										properties: [
											{
												type: 'custom_slider',
												name: 'Blur',
												property: 'blur',
												cssproperty: 'filter',
												step: .1,
												min: 0,
												max: 10,
												unit: 'px',
												units: ['px'],
												defaults: 0
											},
											{
												type: 'custom_slider',
												name: 'Brightness',
												property: 'brightness',
												cssproperty: 'filter',
												step: 10,
												min: 0,
												max: 200,
												unit: '%',
												units: ['%'],
												defaults: 100,
											},
											{
												type: 'custom_slider',
												name: 'Contrast',
												property: 'contrast',
												cssproperty: 'filter',
												step: 10,
												min: 0,
												max: 200,
												unit: '%',
												units: ['%'],
												defaults: 100,
											},
											{
												type: 'custom_slider',
												name: 'Grayscale',
												property: 'grayscale',
												cssproperty: 'filter',
												step: 1,
												min: 0,
												max: 100,
												unit: '%',
												units: ['%'],
												defaults: 0,
											},
											{
												type: 'custom_slider',
												name: 'Hue-rotate',
												property: 'hue-rotate',
												cssproperty: 'filter',
												step: 1,
												min: 0,
												max: 360,
												unit: 'deg',
												units: ['deg'],
												defaults: 0,
											},
											{
												type: 'custom_slider',
												name: 'Invert',
												property: 'invert',
												cssproperty: 'filter',
												step: 1,
												min: 0,
												max: 100,
												unit: '%',
												units: ['%'],
												defaults: 0,
											},
											{
												type: 'custom_slider',
												name: 'Opacity',
												property: 'opacity',
												cssproperty: 'filter',
												step: 1,
												min: 0,
												max: 100,
												unit: '%',
												units: ['%'],
												defaults: 100
											},
											{
												type: 'custom_slider',
												name: 'Saturate',
												property: 'saturate',
												cssproperty: 'filter',
												step: 1,
												min: 0,
												max: 100,
												unit: '%',
												units: ['%'],
												defaults: 100,
											},
											{
												type: 'custom_slider',
												name: 'Sepia',
												property: 'sepia',
												cssproperty: 'filter',
												step: 1,
												min: 0,
												max: 100,
												unit: '%',
												units: ['%'],
												defaults: 0,
											}
										]
									}, {
										name: VjLocalized.Extra,
										open: false,
										buildProps: ['display', 'cursor', 'float', 'clear', 'overflow-x', 'overflow-y'],
										properties: [
											{
												type: 'radio',
												name: 'Display',
												property: 'display',
												defaults: 'none',
												list: [{
													value: 'none',
													name: 'none',
												}, {
													value: 'block',
													name: 'block',
												}, {
													value: 'inline',
													name: 'inline',
												}, {
													value: 'inline-block',
													name: 'inline-block',
												}, {
													value: 'flex',
													name: 'flex',
												}],
											}, {
												type: 'radio',
												name: 'Clear',
												property: 'clear',
												defaults: 'none',
												list: [{
													value: 'none',
													name: 'none',
												}, {
													value: 'both',
													name: 'both',
												}],
											}, {
												type: 'radio',
												name: 'Overflow Horizontal',
												property: 'overflow-x',
												defaults: 'visible',
												list: [{
													value: 'visible',
													name: 'visible',
												}, {
													value: 'hidden',
													name: 'hidden',
												}, {
													value: 'scroll',
													name: 'scroll',
												}, {
													value: 'auto',
													name: 'auto',
												}],
											}, {
												type: 'radio',
												name: 'Overflow Vertical',
												property: 'overflow-y',
												defaults: 'visible',
												list: [{
													value: 'visible',
													name: 'visible',
												}, {
													value: 'hidden',
													name: 'hidden',
												}, {
													value: 'scroll',
													name: 'scroll',
												}, {
													value: 'auto',
													name: 'auto',
												}],
											}
										]
									}]
								},
								layerManager: {
									appendTo: '#layer-manager .jsPanel-content'
								},
								colorPicker: {
									appendTo: 'parent',
									offset: {
										top: 26, left: -180,
									},
									showInput: true,
									preferredFormat: "hex",
								},
								traitManager: {
									appendTo: '.traitsmanager'
								},
								deviceManager: {
									devices: [{
										name: 'Desktop',
										width: '', // default size
									}, {
										name: 'Tablet',
										width: '768px', // this value will be used on canvas width
										widthMedia: '768px', // this value will be used in CSS @media
									}, {
										name: 'Mobile',
										width: '360px', // this value will be used on canvas width
										widthMedia: '360px', // this value will be used in CSS @media
									}]
								},
								storageManager: {
									type: 'remote',
									autosave: false,
									autoload: false,
									stepsBeforeSave: 2,
									urlStore: eval(vjEditorSettings.UpdateContentUrl),
									onComplete(jqXHR, status) {
										if (jqXHR.IsSuccess) {
											if (typeof jqXHR.ShowNotification != 'undefined' && jqXHR.ShowNotification)
												ShowNotification('', VjLocalized.PagePublished, 'success');
										}
										else if (jqXHR.Message != undefined && jqXHR.Message != '')
											ShowNotification('', jqXHR.Message, 'error');

										if (jqXHR.SaveContentNotification != undefined && jqXHR.SaveContentNotification != '') {
											eval(jqXHR.SaveContentNotification);
										}
									},
									params: {
										EntityID: vjEditorSettings.EntityID,
										IsPublished: false,
										m2v: false,
										Comment: ""
									},
									headers: {
										'ModuleId': parseInt(vjEditorSettings.ModuleId),
										'TabId': parseInt(sf.getTabId()),
										'RequestVerificationToken': sf.getAntiForgeryValue()
									}
								}
                            });

                            var fontfamilylist = [];
                            $.each(VJFonts, function (k, v) {
                                fontfamilylist.push({ value: v.Value, name: v.Name });
                            });

							//setCustomRte();
							const rte = VjEditor.RichTextEditor;
							rte.remove('link');

                            VjEditor.RichTextEditor.add('link', {
                                icon: '<i class="fa fa-link"/>',
                                attributes: { title: 'Link' },
                                state: (rte, doc) => {
                                    isValidAnchor(rte) ? $(rte.actions[4].btn).addClass('gjs-rte-active') : $(rte.actions[4].btn).addClass('gjs-rte-inactive');
                                    return isValidAnchor(rte);
                                },
                                result: function (rte) {
                                    if (isValidAnchor(rte)) {
                                        var rtetext = `${rte.selection()}`;
                                        rte.selection().anchorNode.parentNode.remove();
                                        rte.insertHTML(rtetext);
                                        VjEditor.getSelected().view.disableEditing();
                                        //rte.exec('unlink');
                                    }
                                    else {
                                        rte.insertHTML(`<a class="link" href="">${rte.selection()}</a>`);
                                        var rtetext = `${rte.selection()}`;
                                        var selected = VjEditor.getSelected();
                                        selected.view.syncContent();
                                        VjEditor.getSelected().view.disableEditing();
                                        $.each(selected.components().models, function (k, v) {
                                            if (v.attributes.content == rtetext) {
                                                VjEditor.select(v);
                                                return;
                                            }
                                        });
                                        $('.href_url').focus();
                                    }
                                },
                            });

                            const isValidAnchor = rte => {
                                const anchor = rte.selection().anchorNode;
                                const parentNode = anchor && anchor.parentNode;
                                return (
                                    (parentNode && parentNode.nodeName == 'A')
                                );
                            };

                            const isValidSpan = rte => {
                                const anchor = rte.selection().anchorNode;
                                const parentNode = anchor && anchor.parentNode;
                                return (
                                    (parentNode && parentNode.classList.contains('text-inner'))
                                );
                            };

                            VjEditor.RichTextEditor.add('span', {
                                icon: '<i class="fas fa-paint-brush"/>',
                                attributes: { title: 'Custom Styling' },
                                state: (rte, doc) => {
                                    isValidSpan(rte) ? $(rte.actions[5].btn).addClass('gjs-rte-active') : $(rte.actions[5].btn).addClass('gjs-rte-inactive');
                                    return isValidSpan(rte);
                                },
                                result: function (rte) {
                                    var e = rte.selection().anchorNode;
                                    if (e.parentNode.tagName.toLowerCase() == "span") {
                                        rte.selection().anchorNode.parentElement.outerHTML = rte.selection().anchorNode.parentElement.innerHTML;
                                        VjEditor.getSelected().view.disableEditing();
                                    }
                                    else {
                                        rte.insertHTML(`<span class="text-inner">${rte.selection()}</span>`);
                                        var rtetext = `${rte.selection()}`;
                                        var selected = VjEditor.getSelected();
                                        selected.view.syncContent();
                                        VjEditor.getSelected().view.disableEditing();
                                        $.each(selected.components().models, function (k, v) {
                                            if (v.attributes.content == rtetext) {
                                                VjEditor.select(v);
                                                return;
                                            }
                                        });
                                    }
                                }
                            });

                            // Fixed editor is not defined with absolute mode
                            window.editor = VjEditor;

                            VjEditor.on('load', function () {

                                $('#BlockManager').find('.block-search').val('');

								if (vjEditorSettings.EditPage) {
									LoadApps();
									LoadDesignBlocks();
								}

                                LoadCustomBlocks();
                                VjEditor.UndoManager.start();
                                if (VJIsPageDraft == "False" || VJIsLocked == "True")
                                    $('#VJBtnPublish').addClass('disabled');

                                if (VJIsContentApproval == "True" && VJIsLocked == "True")
                                    $('.gjs-cv-canvas__frames').addClass('lockcanvas');
                                else
                                    $('.gjs-cv-canvas__frames').removeClass('lockcanvas');

                                setTimeout(function () {
                                    VjEditor.StorageManager.setStepsBeforeSave(1);
                                    if (GetParameterByName('m2v', parent.window.location) != null && GetParameterByName('m2v', parent.window.location).startsWith('true') && (VJLandingPage.components == '' || VJLandingPage.components == '[]')) {
                                        VjEditor.runCommand("save");
                                        VjEditor.destroy();
                                        // Remove All Managers
                                        $('#ContentBlocks, .stylemanager, .traitsmanager').empty();
                                        VjLayerpanel.close();
                                        VjInit();
                                    }
                                    else {
                                        setTimeout(function () { $($(window.parent.document.body).find('#dnn_ContentPane')[0]).find('.optimizing-overlay').remove(); }, 1000);
                                    }
                                }, 500);

                                $('.gjs-frame').contents().find("#wrapper").scroll(function () {
                                    var $tools = $('#gjs-cv-tools')
                                    $tools.hide();
                                    clearTimeout($.data(this, 'scrollTimer'));
                                    $.data(this, 'scrollTimer', setTimeout(function () {
                                        $tools.show();
                                    }, 250));
                                });

                                $('.gjs-sm-sector').on('click', function () {
                                    var $this = $(this);
                                    var sectorName = $this.attr('id').replace("gjs-sm-", "");
                                    if ($this.find('.gjs-sm-properties').is(':visible')) {
                                        $.each(VjEditor.StyleManager.getSectors().models, function (index, model) {
                                            model.set('open', false);
                                        });
                                        VjEditor.StyleManager.getSector(sectorName).set('open', true);
                                    }
                                });
                            });


                            var parentClone = "";
                            var parentRemove = "";
                            var sortermarup = '';
                            //Sorting TabModules
                            VjEditor.on('sorter:drag:start', function (model, bmodel) {

                                $('.tlb-app-setting-panel').hide();

                                if (typeof bmodel != 'undefined' && typeof bmodel.parent != 'undefined') {

                                    if (bmodel.parent().attributes.type == "column" && bmodel.parent().components().length == 1)
                                        $(bmodel.parent().getEl()).attr("data-empty", "true");

                                    var blockwrapper = bmodel.closest('[data-gjs-type="blockwrapper"]');

                                    if (blockwrapper) {
                                        parentRemove = blockwrapper;
                                        parentClone = blockwrapper.clone();
                                    }
                                    else if (bmodel.getName != 'undefined') {
                                        if (bmodel.attributes.type == "button" || bmodel.attributes.type == "icon" || bmodel.attributes.type == "list" || bmodel.attributes.type == "list-text") {
                                            parentRemove = bmodel.parent();
                                            parentClone = bmodel.parent().clone();
                                        }
                                        else if (bmodel.attributes.type == "button-text" || bmodel.attributes.type == "image") {
                                            parentRemove = bmodel.parent().parent();
                                            parentClone = bmodel.parent().parent().clone();
                                        }
                                    }
                                }

                                if (model != undefined && model.attributes != undefined && model.attributes.dmid != undefined)
                                    sortermarup = model.children[0].innerHTML;

                                VjEditor.runCommand('core:component-outline');

                            });

                            VjEditor.on('sorter:drag:end', function (model, bmodel) {

                                if (typeof model != 'undefined' && typeof model.modelToDrop != 'undefined') {

                                    if (typeof model.modelToDrop.parent != 'undefined' && model.modelToDrop.parent() && model.modelToDrop.parent().attributes.type == "column")
                                        $(model.modelToDrop.parent().getEl()).removeAttr("data-empty");

                                    if (parentRemove != '' && parentClone != '') {
                                        model.modelToDrop.replaceWith(parentClone);
                                        parentRemove.remove();
                                        parentRemove = '';
                                        parentClone = '';
                                    }
                                }

                                if (sortermarup != undefined && sortermarup != '' && model != undefined && model.dst != undefined && model.dst.children != undefined) {
                                    $.each(model.dst.children, function (k, v) {
                                        if ($(v).hasClass('gjs-comp-selected')) {
                                            $.each(v.children, function (key, val) {
                                                if ($(val).attr('vjmod') != undefined)
                                                    val.innerHTML = sortermarup;
                                            })
                                        }
                                    });
                                }

                                sortermarup = '';
                                if (!$('.View.Layout').hasClass('active'))
                                    VjEditor.stopCommand('core:component-outline');

                            });

                            VjEditor.Keymaps.add('ns:redo', 'ctrl+y', editor => { console.log('redo call'); VjEditor.UndoManager.redo(); });
                            //VjEditor.Keymaps.add('ns:undo', 'ctrl+z', editor => { console.log('undo call'); VjEditor.UndoManager.undo(); });

                            //Adding TabModules
                            VjEditor.on('block:drag:stop', function (model, bmodel) {

                                if (typeof model != "undefined") {

                                    if (typeof model.attributes != "undefined") {

                                        var Block = model.attributes.type;

                                        if (Block == 'grid' || Block == 'image-block' || Block == 'modulewrapper')
                                            VjEditor.select(model);
                                    }

                                    if (typeof model.components != 'undefined' && typeof model.components().models[0] != 'undefined') {

                                        var childBlock = model.components().models[0].attributes.type;

                                        if (childBlock == 'icon' || childBlock == 'image')
                                            VjEditor.select(model.components().models[0]);
                                    }

                                    if (typeof model.getAttributes != "undefined" && model.getAttributes()["data-block-alignment"] == "true")
                                        model.setStyle({ "display": "inline-block" });
                                }

                                if (typeof VjEditor.BlockManager.get('LibraryBlock') != 'undefined')
                                    VjEditor.BlockManager.remove('LibraryBlock');

                                if (!$('.borderlines').hasClass('active'))
                                    VjEditor.stopCommand('core:component-outline');

                                if (model != undefined && model.attributes != undefined && model.attributes.attributes != undefined && model.attributes.attributes.mid != undefined) {
                                    model.view.$el[0].innerHTML = '<img class="centerloader" src="' + VjDefaultPath + 'loading.gif" />';
                                    var ModuleData = {
                                        UniqueID: model.attributes.attributes.uid != undefined ? parseInt(model.attributes.attributes.uid) : 0,
                                        DesktopModuleId: parseInt(model.attributes.attributes.dmid)
                                    };
                                    var sf = $.ServicesFramework(-1);
                                    $.ajax({
                                        type: "POST",
                                        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Page/AddModule",
                                        data: ModuleData,
                                        headers: {
                                            'ModuleId': parseInt(sf.getModuleId()),
                                            'TabId': parseInt(sf.getTabId()),
                                            'RequestVerificationToken': sf.getAntiForgeryValue()
                                        },
                                        success: function (response) {
                                            VjEditor.StorageManager.setAutosave(false);
                                            model.attributes.attributes.mid = response;
                                            var framesrc = CurrentTabUrl;
                                            if (framesrc.indexOf("?") == -1)
                                                framesrc = framesrc + "?mid=" + response + "&icp=true";
                                            else
                                                framesrc = framesrc + "&mid=" + response + "&icp=true";
                                            model.view.$el[0].innerHTML = '<div data-gjs-type="module" draggable="true" vjmod="true"><div id="dnn_vj_' + response + '"><img class="centerloader" src="' + VjDefaultPath + 'loading.gif" /><iframe scrolling="no" onload="window.parent.RenderApp(this);" src="' + framesrc + '" style="width:100%;height:auto;"></iframe></div></div>';
                                            bmodel.attributes.content = '<div dmid="' + parseInt(model.attributes.attributes.dmid) + '" mid="' + response + '" fname="' + model.attributes.attributes.fname + '"><div vjmod="true"><app id="' + response + '"></app></div></div>';
                                            model.attributes.components.models[0].attributes.content = '<app id="' + response + '"></app>';
                                            model.attributes.name = VjLocalized.PrefixAppName + bmodel.id;
                                            VjEditor.LayerManager.render();

                                            setTimeout(function () {
                                                VjEditor.runCommand("save");
                                            }, 500);
                                        }
                                    });
                                }
                                else if (model != undefined && model.attributes != undefined && model.attributes.attributes != undefined && model.attributes.attributes["data-block-type"] != undefined)
                                    RenderBlock(model, bmodel);
                            });

                            var FilterBorderOptions = function (target, position) {

                                setTimeout(function () {
                                    var val;

                                    switch (position) {
                                        case "sm-border-top":
                                            val = "border-top"
                                            break;
                                        case "sm-border-right":
                                            val = "border-right"
                                            break;
                                        case "sm-border-bottom":
                                            val = "border-bottom"
                                            break;
                                        case "sm-border-left":
                                            val = "border-left"
                                            break;
                                        default:
                                            val = "border"
                                    }

                                    var sm = VjEditor.StyleManager;

                                    $(sm.getProperties('border').models).each(function () {
                                        if (this.attributes.name != 'Border Postion')
                                            this.view.$el.hide();
                                    });

                                    $(sm.getProperty(Border, val + '-style').view.el).show();
                                    $(sm.getProperty(Border, val + '-color').view.el).show();
                                    $(sm.getProperty(Border, val + '-width').view.el).show();

                                });
                            }

                            var ReverseColums = function (model) {

                                var flexDirection = model.getStyle()['flex-direction'];
                                var row = model.components().models[0];

                                var className = 'col-lg-12';
                                var Device = VjEditor.getDevice();

                                if (Device == 'Mobile')
                                    className = 'col-12';
                                else if (Device == 'Tablet')
                                    className = 'col-sm-12';

                                if (typeof flexDirection == 'undefined') {

                                    flexDirection = 'row';

                                    var style = model.getStyle();
                                    style['flex-direction'] = 'row';
                                    model.setStyle(style);
                                }

                                jQuery.each(row.components().models, function (i, column) {
                                    if (column.getEl().classList.contains(className)) {

                                        if (flexDirection.indexOf('reverse') >= 0)
                                            flexDirection = 'column-reverse';
                                        else
                                            flexDirection = 'column';

                                        return false;
                                    }
                                });

                                row.setStyle({ 'flex-direction': flexDirection });
                            }

                            VjEditor.on('component:styleUpdate:border-position', (model, argument) => {
                                FilterBorderOptions(model, event.target.value);
                                model.removeStyle('border-position');
                            });

                            VjEditor.on('component:selected', (model, argument) => {

								if (typeof model.attributes.type == 'undefined')
									return false;
								else if (model.attributes.type == 'carousel-caption') {
									$.each(getAllComponents(model.parent()), function (i, n) {
										if (n.attributes.type == 'carousel-image') {
											VjEditor.select(n);
											return false;
										}
									});
									return;
								}
								else if (model.attributes.type == 'prev' || model.closest('[data-gjs-type="prev"]')) {
									var slider = model.closest('[data-gjs-type="carousel"]');
									VjEditor.select(slider);
									VjEditor.runCommand("slider-prev", { slider });
									return;
								}
								else if (model.attributes.type == 'next' || model.closest('[data-gjs-type="next"]')) {
									var slider = model.closest('[data-gjs-type="carousel"]');
									VjEditor.select(slider);
									VjEditor.runCommand("slider-next", { slider });
									return;
								}
								else if (model.attributes.type == 'indicator') {
									var slider = model.closest('[data-gjs-type="carousel"]');
									VjEditor.select(slider);
									var index = parseInt(model.getAttributes()['data-slide-to']);
									$('.gjs-frame').contents().find('#' + slider.getId()).carousel('dispose').carousel({ interval: false }).carousel(index);
									return;
                                }

                                if (typeof model.attributes.toolbar[0]  != 'undefined' && typeof model.attributes.toolbar[0].attributes['title'] == 'undefined') {

                                    $.each(model.attributes.toolbar, function (k, v) {

                                        if (v.attributes['class'] == 'fa fa-arrow-up')
                                            v.attributes['title'] = VjLocalized.SelectParent;
                                        else if (v.command == 'vj-move' || v.command == 'tlb-move')
                                            v.attributes['title'] = VjLocalized.Move;
                                        else if (v.command == 'vj-copy' || v.command == 'tlb-clone')
                                            v.attributes['title'] = VjLocalized.Copy;
                                        else if (v.command == 'vj-delete' || v.command == 'tlb-delete')
                                            v.attributes['title'] = VjLocalized.Delete;
                                    });
                                }

                                var desktop = 'd-desktop-none';
                                var tablet = 'd-tablet-none';
                                var mobile = 'd-mobile-none';

                                if (typeof model.get(desktop) != 'undefined')
                                    VjEditor.StyleManager.getProperty(Responsive, desktop).setValue(model.get(desktop));

                                if (typeof model.get(tablet) != 'undefined')
                                    VjEditor.StyleManager.getProperty(Responsive, tablet).setValue(model.get(tablet));

                                if (typeof model.get(mobile) != 'undefined')
                                    VjEditor.StyleManager.getProperty(Responsive, mobile).setValue(model.get(mobile));

                                if (typeof model.getStyle()['filter'] != "undefined") {

                                    if (model.getStyle()['filter'].indexOf('blur') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'blur').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('brightness') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'brightness').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('contrast') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'contrast').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('grayscale') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'grayscale').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('hue-rotate') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'hue-rotate').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('invert') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'invert').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('opacity') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'opacity').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('saturate') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'saturate').view.setValue('true');

                                    if (model.getStyle()['filter'].indexOf('sepia') != -1)
                                        VjEditor.StyleManager.getProperty(Filters, 'sepia').view.setValue('true');
                                }

                                if (typeof model.getStyle()['transform'] != "undefined") {

                                    if (model.getStyle()['transform'].indexOf('rotateX') != -1)
                                        VjEditor.StyleManager.getProperty(Transform, 'rotateX').view.setValue('true');

                                    if (model.getStyle()['transform'].indexOf('rotateY') != -1)
                                        VjEditor.StyleManager.getProperty(Transform, 'rotateY').view.setValue('true');

                                    if (model.getStyle()['transform'].indexOf('rotateZ') != -1)
                                        VjEditor.StyleManager.getProperty(Transform, 'rotateZ').view.setValue('true');

                                    if (model.getStyle()['transform'].indexOf('scaleX') != -1)
                                        VjEditor.StyleManager.getProperty(Transform, 'scaleX').view.setValue('true');

                                    if (model.getStyle()['transform'].indexOf('scaleY') != -1)
                                        VjEditor.StyleManager.getProperty(Transform, 'scaleY').view.setValue('true');

                                    if (model.getStyle()['transform'].indexOf('scaleZ') != -1)
                                        VjEditor.StyleManager.getProperty(Transform, 'scaleZ').view.setValue('true');
                                }

                                $('.tlb-app-setting-panel').hide();

                                if (model.attributes.traits.length <= 0) {
                                    $(".traitsmanager > .empty_msg").css({ "display": "block" });
                                    $(".traitsmanager > .gjs-two-color").css({ "display": "none" });
                                }
                                else {
                                    $(".traitsmanager > .empty_msg").css({ "display": "none" });
                                    $(".traitsmanager > .gjs-two-color").css({ "display": "block" });
                                }

                                if (model.attributes.type != 'wrapper') 
                                    Stylemanager();
                                else {
                                    $(".Menupanel-top").hide();
                                    $("#StyleToolManager").hide();
                                    $("#Notification").hide();
                                    $('#DeviceManager,#LanguageManager,#MenuSettings,.ntoolbox').hide();
                                    $("#BlockManager").show();
                                    $(".panel-top").show();
                                    $("#ContentBlocks").show();
                                    $(".block-set").show();
                                    $("#iframeHolder").hide();
                                    setTimeout(function () {
                                        ChangeBlockType();
                                    }, 300);
                                }

								model.set('border-position', 'sm-border');
								FilterBorderOptions(model, 'sm-border');

                                if (model.attributes.type == 'column') {

                                    $(model.parent().getEl()).addClass('gjs-dashed');

                                    var dataPane = model.getTrait('data-pane');
                                    var migrate = GetParameterByName('migrate', parent.window.location.href);

                                    if ((migrate == null || (migrate != null && migrate.indexOf('true') == -1)) && typeof dataPane != 'undefined')
                                        dataPane.destroy();
                                }

                                var flexProperty = VjEditor.StyleManager.getProperty(Responsive, 'flex-direction');

                                if (model.attributes.type == 'grid') {

                                    if (flexProperty == null) {

                                        VjEditor.StyleManager.addProperty(Responsive, {
                                            type: 'customradio',
                                            name: 'Reverse Columns',
                                            property: 'flex-direction',
                                            defaults: 'row',
                                            list: [{
                                                value: 'row-reverse',
                                                name: 'Yes',
                                            }, {
                                                value: 'row',
                                                name: 'No',
                                            }],
                                            UpdateStyles: true,
                                        });

                                        flexProperty = VjEditor.StyleManager.getProperty(Responsive, 'flex-direction');
                                    }

                                    if (model.components().length) {
                                        $(model.components().models[0].getEl()).addClass('gjs-dashed');

                                        if (flexProperty != null)
                                            ReverseColums(model);
                                    }
                                }
                                else {
                                    if (flexProperty != null)
                                        VjEditor.StyleManager.removeProperty(Responsive, 'flex-direction');
                                }

                                if (model.attributes.type == 'heading' || model.attributes.type == 'text' || model.attributes.type == 'button' || model.attributes.type == 'list' || model.attributes.type == 'link') {

                                    VjEditor.StyleManager.addSector(Text, {
                                        name: VjLocalized.Text,
                                        open: false,
                                        buildProps: ['color', 'font-family', 'font-size', 'line-height', 'letter-spacing', 'word-spacing', 'font-weight', 'font-style', 'text-transform', 'text-decoration', 'text-shadow'],
                                        properties: [{
                                            type: 'radio',
                                            name: 'Text decoration',
                                            property: 'text-decoration',
                                            defaults: 'none',
                                            list: [{
                                                value: 'none',
                                                name: 'None',
                                            }, {
                                                value: 'underline',
                                                name: 'Underline',
                                            }, {
                                                value: 'overline',
                                                name: 'Overline',
                                            }, {
                                                value: 'line-through',
                                                name: 'Strikethrough',
                                            }],
                                        }, {
                                            type: 'radio',
                                            name: 'Font style',
                                            property: 'font-style',
                                            defaults: 'normal',
                                            list: [{
                                                value: 'normal',
                                                name: 'Normal',
                                            }, {
                                                value: 'italic',
                                                name: 'Italic',
                                            }],
                                        }, {
                                            type: 'select',
                                            name: 'Font Family',
                                            property: 'font-family',
                                            list: fontfamilylist
                                        }, {
                                            type: 'customrange',
                                            name: 'Font size',
                                            property: 'font-size',
                                            units: [
                                                { name: 'px', min: 1, max: 100, step: 1 },
                                                { name: '%', min: 1, max: 100, step: 1 },
                                                { name: 'em', min: 1, max: 100, step: 1 },
                                                { name: 'rem', min: 1, max: 100, step: 1 },
                                                { name: 'vw', min: 1, max: 100, step: 1 },
                                                { name: 'vh', min: 1, max: 100, step: 1 },
                                            ],
                                            unit: 'px',
                                            defaults: 16
                                        }, {
                                            type: 'customrange',
                                            name: 'Line height',
                                            property: 'line-height',
                                            units: [
                                                { name: 'px', min: 1, max: 100, step: 1 },
                                                { name: '%', min: 1, max: 100, step: 1 },
                                                { name: 'em', min: 1, max: 6, step: 1 },
                                                { name: 'rem', min: 1, max: 6, step: 1 },
                                                { name: 'vw', min: 1, max: 100, step: 1 },
                                                { name: 'vh', min: 1, max: 100, step: 1 },
                                            ],
                                            unit: 'px',
                                            defaults: 24
                                        }, {
                                            type: 'customrange',
                                            name: 'Letter spacing',
                                            property: 'letter-spacing',
                                            units: [
                                                { name: 'px', min: -5, max: 100, step: 1 },
                                                { name: 'em', min: -1, max: 5, step: 1 },
                                                { name: 'rem', min: -1, max: 5, step: 1 },
                                                { name: 'vw', min: -5, max: 100, step: 1 },
                                                { name: 'vh', min: -5, max: 100, step: 1 },
                                            ],
                                            unit: 'px',
                                            defaults: 0
                                        }, {
                                            type: 'customrange',
                                            name: 'Word spacing',
                                            property: 'word-spacing',
                                            units: [
                                                { name: 'px', min: -5, max: 100, step: 1 },
                                                { name: 'em', min: -1, max: 5, step: 1 },
                                                { name: 'rem', min: -1, max: 5, step: 1 },
                                                { name: 'vw', min: -5, max: 100, step: 1 },
                                                { name: 'vh', min: -5, max: 100, step: 1 },
                                            ],
                                            unit: 'px',
                                            defaults: 0
                                        }, {
                                            type: 'radio',
                                            name: 'Font Weight',
                                            property: 'font-weight',
                                            list: [{
                                                value: '100',
                                                name: 'Thin',
                                            }, {
                                                value: '300',
                                                name: 'Light',
                                            }, {
                                                value: '500',
                                                name: 'Medium',
                                            }, {
                                                value: '700',
                                                name: 'Bold',
                                            }, {
                                                value: '900',
                                                name: 'Ultra Bold',
                                            }],
                                        }, {
                                            id: 'text-transform',
                                            type: 'radio',
                                            name: 'Text transform',
                                            property: 'text-transform',
                                            defaults: 'none',
                                            list: [{
                                                value: 'none',
                                                name: 'None',
                                            }, {
                                                value: 'uppercase',
                                                name: 'Uppercase',
                                            }, {
                                                value: 'lowercase',
                                                name: 'Lowercase',
                                            }, {
                                                value: 'capitalize',
                                                name: 'Capitalize',
                                            }],
                                        }],
                                    }, { at: 3 })
                                }
                                else if (typeof VjEditor.StyleManager.getSector(Text) != 'undefined')
                                    VjEditor.StyleManager.removeSector(Text);

                                $(VjEditor.StyleManager.getSectors().models).each(function (index, value) {
                                    $(value.attributes.properties.models).each(function (subIndex, subValue) {
                                        if (subValue.attributes.type == "slider")
                                            if ($(subValue.view.el).find('.gjs-field-integer input').val() == "auto" || $(subValue.view.el).find('.gjs-field-integer input').val() == "")
                                                $(subValue.view.el).find('input[type="range"]').val(0);
                                    });
                                });
                            });

                            VjEditor.on('rte:enable', (model, argument) => {
                                if (model.model.attributes.type == 'button-text') {
                                    const rte = editor.RichTextEditor;
                                    rte.actionbar.parentNode.hidden = true;
                                }
                                else
                                    rte.actionbar.parentNode.hidden = false;
                            });

                            VjEditor.on('component:deselected', (model, argument) => {

                                if (model.attributes.type == 'column')
                                    $(model.parent().getEl()).removeClass('gjs-dashed');

                                if (model.attributes.type == 'grid' && model.components().length)
                                    $(model.components().models[0].getEl()).removeClass('gjs-dashed');
                            });

                            VjEditor.on('styleable:change', () => {

                                var model = VjEditor.getSelected();

                                if (typeof model != 'undefined') {

                                    var $globalblockwrapper = $(model.getEl()).parents('[data-gjs-type="globalblockwrapper"]');

                                    if ($globalblockwrapper.length) {

                                        var result = VjEditor.runCommand("export-css", {
                                            target: model
                                        });

                                        var style = $globalblockwrapper.find('style:contains(' + result.split("{")[0] + '{)');

                                        if (style.length <= 0)
                                            $globalblockwrapper.append('<style>' + result + '</style>');
                                        else
                                            style.replaceWith('<style>' + result + '</style>');

                                        model.removeStyle();
                                    }
                                }
                            });

                            VjEditor.on('component:styleUpdate', (model, property) => {

                                if (property == "color" && typeof event != "undefined" && $(event.target).parents(".gjs-sm-property.gjs-sm-color").length) {
                                    if (model.attributes.type == "heading" || model.attributes.type == "text" || model.attributes.type == "button" || model.attributes.type == "list") {

                                        var classes = model.getClasses();
                                        classes = jQuery.grep(classes, function (className, index) {
                                            return (className.match(/\btext-\S+/g) || []).join(' ');
                                        });

                                        model.removeClass(classes);
                                    }
                                }

                                if (typeof event != "undefined" && event.target.className == "gjs-sm-clear")
                                    model.removeStyle(property);
                            });

                            VjEditor.on('component:styleUpdate:flex-direction', (model) => {

                                if (model.attributes.type == "grid")
                                    ReverseColums(model);
                            });

                            VjEditor.on('component:styleUpdate:float', (model) => {

                                if (model.attributes.type == "button" || model.attributes.type == "icon" || model.attributes.type == "list") {
                                    var style = model.parent().getStyle();
                                    style['float'] = model.getStyle()['float'];
                                    model.parent().setStyle(style);
                                }
                                else if (model.attributes.type == "image") {
                                    var style = model.parent().parent().getStyle();
                                    style['float'] = model.getStyle()['float'];
                                    model.parent().parent().setStyle(style);
                                }
                            });

                            var UpdateBorderStyle = function (target, property) {

                                var style = target.getStyle();

                                if (typeof style[property] == 'undefined' && typeof style['border-width'] == 'undefined') {

                                    VjEditor.StyleManager.getProperty(Border, property).setValue('3px');
                                    style[property] = '3px';
                                }

                                target.setStyle(style);
                            }

                            VjEditor.on('component:update:border-style', (model) => {
                                UpdateBorderStyle(model, 'border-width');
                            });

                            VjEditor.on('component:update:border-top-style', (model) => {
                                UpdateBorderStyle(model, 'border-top-width');
                            });

                            VjEditor.on('component:update:VjEditor.getSelected()border-right-style', (model) => {
                                UpdateBorderStyle(model, 'border-right-width');
                            });

                            VjEditor.on('component:update:border-bottom-style', (model) => {
                                UpdateBorderStyle(model, 'border-bottom-width');
                            });

                            VjEditor.on('component:update:border-left-style', (model) => {
                                UpdateBorderStyle(model, 'border-left-width');
                            });

                            VjEditor.Commands.add('global-delete', {
                                run(editor, sender) {
                                    setTimeout(function () {
                                        VjEditor.runCommand('core:component-delete');
                                    }, 100);
                                }
                            });

                            VjEditor.Commands.add('custom-tui-image-editor', {
                                run(editor, sender) {

                                    VjEditor.runCommand('tui-image-editor');
                                    var applyButton = $('.tui-image-editor__apply-btn');

                                    $('#tie-btn-crop').on('click', function () {
                                        if (applyButton.is(':visible'))
                                            applyButton.hide();
                                        else
                                            applyButton.show();
                                    });

                                    $('#tie-crop-button .tui-image-editor-button.apply, #tie-crop-button .tui-image-editor-button.cancel').on('click', function () {
                                        applyButton.show();
                                    });
                                }
                            });

                            //Custom Block Settings
                            VjEditor.Commands.add('tlb-vjblock-setting', {
                                run(editor, sender) {
                                    var event = window;
                                    var guid = $(event.event.currentTarget).attr('guid');
                                    var url = CurrentExtTabUrl + "&guid=" + guid;
                                    var width = 500;
                                    var currentTargetWidth = $(event.event.currentTarget).attr('width');
                                    if (typeof currentTargetWidth != 'undefined' && currentTargetWidth != null && currentTargetWidth != 'undefined') {
                                        width = parseInt($(event.event.currentTarget).attr('width'));
                                    }
                                    OpenPopUp(null, width, 'right', '', url);
                                }
                            });

                            //TabModules Settings
                            VjEditor.Commands.add('tlb-app-setting', {
                                run(editor, sender) {
                                    var event = window;
                                    var guid = $(event.event.currentTarget).attr('guid');
                                    var width = $(event.event.currentTarget).attr('width');
                                    var url = CurrentExtTabUrl + "&guid=" + guid + "#permissions/" + editor.getSelected().attributes.attributes.mid;
                                    OpenPopUp(null, width, 'right', VjLocalized.Setting, url);
                                }
                            });

                            //TabModules Action Menu
                            VjEditor.Commands.add('tlb-app-actions', {
                                run(editor, sender, opts) {
                                    var event = window;
                                    this.editor = editor;
                                    var target = event.event.currentTarget || editor.getSelected();
                                    var canvas = editor.Canvas;
                                    var setting = this.settingsEl;
                                    if (target) {
                                        var panel = "tlb-app-setting-panel";
                                        setting = editor.$('<div class="' + panel + '"></div>')[0];
                                        canvas.getToolsEl().appendChild(setting);
                                        opts.BlockMenus.forEach(function (t) {
                                            var n = editor.$('<div class="' + panel + '__option">' + t.Title + "</div>")[0];
                                            n.addEventListener("click", function () {
                                                if (typeof t.Command != 'undefined') {
                                                    VjEditor.runCommand(t.Command, {
                                                        'bind': true
                                                    });
                                                }
                                                else {
                                                    if (t.NewWindow == 'True')
                                                        window.open(t.Url, '_blank');
                                                    else {
                                                        var width = '100%';

                                                        if (typeof t.Width != 'undefined')
                                                            width = t.Width;

                                                        var position = 'right';

                                                        if (typeof t.Position != 'undefined')
                                                            position = t.Position;

                                                        if (t.Url.indexOf('javascript:') == -1)
                                                            OpenPopUp(null, width, position, t.Title, t.Url, '', '', '', t.ModuleId, t.Scrollbars, t.TitlePosition);
                                                        else
                                                            eval(t.Url);
                                                    }
                                                }
                                                this.parentElement.style.display = "none";
                                            });
                                            setting.appendChild(n);
                                        });
                                        this.settingsEl = setting;
                                        setting.style.display = 'block';
                                        var ToolbarEl = canvas.getToolbarEl()
                                            , ElementDim = canvas.getTargetToElementDim(setting, target)
                                            , ClientRect = setting.getBoundingClientRect()
                                            , TEClientRect = ToolbarEl.getBoundingClientRect()
                                            , position = {
                                                top: ElementDim.canvasTop,
                                                left: ElementDim.canvasLeft
                                            }
                                            , styletop = parseFloat(ToolbarEl.style.top)
                                            , ClientRectheight = styletop - ClientRect.height
                                            , left = parseFloat(ToolbarEl.style.left);
                                        ClientRectheight <= position.top && (ClientRectheight = styletop + TEClientRect.height),
                                            left <= position.left && (left = position.left),
                                            setting.offsetWidth > ToolbarEl.offsetWidth && (left = left - (setting.offsetWidth - ToolbarEl.offsetWidth)),
                                            setting.style.top = ClientRectheight + "px",
                                            setting.style.left = left + "px"
                                    }
                                },
                            });

                            //Personalization
                            VjEditor.Commands.add('tlb-app-personalization', {
                                run(editor, sender) {
                                    var event = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : {};
                                    var target = event.target || editor.getSelected();
                                    window.document.vj_personalization_target = target;
                                    var perm = target.attributes.attributes.perm;
                                    if (perm == undefined)
                                        perm = 0;
                                    var url = CurrentExtTabUrl + "&guid=b1b28eac-b520-4a20-8c36-f0283e8ca263#/permissions/" + perm + "/" + target.attributes.type;
                                    OpenPopUp(null, 800, 'right', VjLocalized.Setting, url);
                                }
                            });

                            //Image
                            VjEditor.Commands.add('open-assets', {
                                run(editor, sender) {
                                    var event = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : {};
                                    var target = event.target || editor.getSelected();
                                    window.document.vj_image_target = target;
                                    var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505";
                                    OpenPopUp(null, 900, 'right', VjLocalized.Image, url);
                                }
                            });

                            VjEditor.Commands.add("export-component", {
                                run: function (t, e) {
                                    var n = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : {}
                                        , r = {
                                            html: "",
                                            css: ""
                                        }
                                        , i = n.component || t.getSelected();
                                    return (i = Array.isArray(i) ? i : [i]).forEach(function (e) {
                                        e && (r.html += e.toHTML({
                                            attributes: function (e, r) {
                                                var i = e.get("type")
                                                    , o = r.id;

                                                $.each(e.attributes.traits.models, function (a, b) {
                                                    r["data-gjs-" + b.attributes.name] = b.getInitValue();
                                                });

                                                (i && (r["data-gjs-type"] = i),
                                                    n.cleanIds && o && "i" === o[0]) && (t.CssComposer.getAll().filter(function (t) {
                                                        return t.get("selectors").getFullString() === "#".concat(o) && !Gt()(t.getStyle())
                                                    }).length || delete r.id);
                                                return n.cleanClasses && r.class && (r.class = e.get("classes").filter(function (t) {
                                                    return !t.get("private")
                                                }).map(function (t) {
                                                    return t.get("name")
                                                }).join(" "),
                                                    r.class || delete r.class),
                                                    r
                                            }
                                        }),
                                            r.css += t.runCommand("export-css", {
                                                target: e
                                            }))
                                    }),
                                        r.html = CleanGjAttrs(r.html),
                                        r.css = r.css,
                                        r
                                },
                                cleanHtmlIds: function (t, tc) {
                                    tc = tc.split('vjbrk');
                                    var elements = $(t);
                                    if (elements != undefined) {
                                        var atid = elements.attr('id');
                                        if (atid != undefined && atid.length > 0) {
                                            var grp = $.grep(tc, function (i) {
                                                return i.startsWith('#' + atid);
                                            });
                                            if (grp != undefined && grp.length > 0)
                                                elements.attr('style', grp[0].replace('#' + atid + '{', '').replace('}', ''));
                                            elements.removeAttr('id');
                                        }
                                        $.each(elements.find('*'), function (k, v) {
                                            v = $(v);
                                            var atid = v.attr('id');
                                            if (atid != undefined && atid.length > 0) {
                                                var grp = $.grep(tc, function (i) {
                                                    return i.startsWith('#' + atid);
                                                });
                                                if (grp != undefined && grp.length > 0)
                                                    v.attr('style', grp[0].replace('#' + atid + '{', '').replace('}', ''));
                                                v.removeAttr('id');
                                            }
                                        });
                                        return elements[0].outerHTML;
                                    }
                                    else
                                        return "";
                                },
                            });

                            VjEditor.Commands.add("export-css", {
                                run: function (t, e) {
                                    var n = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : {}
                                        , r = n.target || t.getWrapper()
                                        , i = t.CssComposer
                                        , o = i.getAll()
                                        , a = ""
                                        , s = this.splitRules(this.matchedRules(r, o))
                                        , c = s.atRules
                                        , l = s.notAtRules;

                                    return l.forEach(function (t) {
                                        var css = t.toCSS();
                                        if (css.startsWith('#'))
                                            return a += (css.substr(0, 0) + '#' + css.substr(0 + 1));
                                        else
                                            return a;
                                    }),
                                        a
                                },
                                matchedRules: function (t, e) {
                                    var n = this
                                        , r = t.getEl()
                                        , i = [];
                                    return e.forEach(function (t) {
                                        try {
                                            t.selectorsToString().split(",").some(function (t) {
                                                return r.matches(n.cleanSelector(t))
                                            }) && i.push(t)
                                        } catch (t) { }
                                    }),
                                        t.components().forEach(function (t) {
                                            i = i.concat(n.matchedRules(t, e))
                                        }),
                                        i
                                },
                                cleanSelector: function (t) {
                                    return t.split(" ").map(function (t) {
                                        return t.split(":")[0]
                                    }).join(" ")
                                },
                                splitRules: function (t) {
                                    var e = {}
                                        , n = [];
                                    return t.forEach(function (t) {
                                        var r = t.getAtRule();
                                        if (r) {
                                            var i = e[r];
                                            i ? i.push(t) : e[r] = [t]
                                        } else
                                            n.push(t)
                                    }),
                                    {
                                        atRules: e,
                                        notAtRules: n
                                    }
                                },
                                getQueryLength: function (t) {
                                    var e = /(-?\d*\.?\d+)\w{0,}/.exec(t);
                                    return e ? parseFloat(e[1]) : Number.MAX_VALUE
                                },
                                sortMediaObject: function () {
                                    var t = this
                                        , e = arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : {}
                                        , n = [];
                                    return Zt()(e, function (t, e) {
                                        return n.push({
                                            key: e,
                                            value: t
                                        })
                                    }),
                                        n.sort(function (e, n) {
                                            return t.getQueryLength(n.key) - t.getQueryLength(e.key)
                                        })
                                }
                            });

                            $(VjEditor.Canvas.getBody()).on("paste", '[contenteditable="true"]', function (e) {
                                e.preventDefault();
                                var text = e.originalEvent.clipboardData.getData('text');
                                e.target.ownerDocument.execCommand("insertText", false, text);
                            });

                            VjEditor.on('block:drag:start', function (model) {

                                VjEditor.runCommand('core:component-outline');

                                if (typeof model != "undefined") {

                                    if (model.attributes.id == 'LibraryBlock')
                                        $('#LibraryBlock').css('opacity', '0.01');
                                }
                            });

                            VjEditor.on('change:changesCount', e => {
                                if (e != undefined && e.changed != undefined && e.changed.changesCount > 0) {
                                    //if (VJIsModeratorEditPermission != 'True')
                                    //    $('#VJBtnPublish').removeClass('disabled');


                                    if (VJAutoSaveTimeOutid) {
                                        clearTimeout(VJAutoSaveTimeOutid);
                                    }


                                    if (VJIsSaveCall && e.changed.changesCount >= VjEditor.StorageManager.getStepsBeforeSave()) {
                                        VJAutoSaveTimeOutid = setTimeout(function () {
                                            if ($('.sidebar-open.settingclosebtn').length == 0) {
                                                VjEditor.runCommand("save");
                                                //VJIsSaveCall = false;
                                            }

                                        }, 1000)
                                    }
                                    else
                                        VJIsSaveCall = true;
                                }

                            });

                            VjEditor.on('storage:error', (err) => {
                                swal({ title: "Error", text: `${err}`, type: "error", });
                            });

                            VjEditor.on('storage:end:store', function (Data) {
                                if (Data != '' && Data.PageReviewSettings != undefined && Data.PageReviewSettings) {
                                    VJIsContentApproval = Data.PageReviewSettings.IsContentApproval ? "True" : "False";
                                    VJNextStateName = Data.PageReviewSettings.NextStateName;
                                    VJIsPageDraft = Data.PageReviewSettings.IsPageDraft ? "True" : "False";;
                                    VJIsLocked = Data.PageReviewSettings.IsLocked ? "True" : "False";;
                                    VJIsModeratorEditPermission = Data.PageReviewSettings.IsModeratorEditPermission;

                                    if (VJIsPageDraft == "True")
                                        $('#VJBtnPublish').removeClass('disabled');
                                    else
                                        $('#VJBtnPublish').addClass('disabled');

                                    if (VJIsPageDraft == "False" || VJIsLocked == "True")
                                        $('#VJBtnPublish').addClass('disabled');

                                    if (VJIsContentApproval == "True" && VJIsLocked == "True")
                                        $('.gjs-cv-canvas__frames').addClass('lockcanvas');
                                    else
                                        $('.gjs-cv-canvas__frames').removeClass('lockcanvas');

                                    if (Data.PageReviewSettings.IsContentApproval)
                                        eval(Data.ReviewToastMarkup);
                                    $("#VJnotifycount", parent.document).text(Data.NotifyCount);

                                }

                                if (Data != null && Data != '' && Data.RedirectAfterm2v != null && typeof Data.RedirectAfterm2v != "undefined")
                                    window.location.href = Data.RedirectAfterm2v;

                            });

                            var iframeBody = VjEditor.Canvas.getBody();
                            $(iframeBody).on("keydown", "[contenteditable]", e => {
                                // trap the return key being pressed
                                if (e.keyCode === 13) {
                                    e.preventDefault();
                                    // insert 2 br tags (if only one br tag is inserted the cursor won't go to the next line)
                                    e.target.ownerDocument.execCommand("insertHTML", false, "<br>&nbsp;");
                                    e.target.ownerDocument.execCommand("delete", false, "&nbsp;");
                                    // prevent the default behaviour of return key pressed
                                    return false;
                                }
                            });

                            VjEditor.on('component:add', function (model) {
                                if (model.parent() != undefined && model.parent().attributes.type == "column")
                                    $(model.parent().getEl()).removeAttr("data-empty");
                            });

                            VjEditor.on('component:remove', function (model) {

                                if (typeof event != "undefined" && event.keyCode == 46) {

                                    if (model.attributes.type == 'carousel-image')
                                        VjEditor.runCommand('slide-delete', { target: model });

                                    if (model.attributes.type == 'button' || model.attributes.type == 'list' || model.attributes.type == 'icon' || model.attributes.type == 'image' || model.attributes.type == 'image-gallery-item')
                                        VjEditor.runCommand('vj-delete', { target: model });
                                }

                                if (model.parent() != undefined && model.parent().attributes.type == "column" && model.parent().components().length == 0)
                                    $(model.parent().getEl()).attr("data-empty", "true");

								if ((typeof model.getAttributes() != "undefined" && model.getAttributes()["data-bg-video"] == "true") || (model.attributes.type == "video" && (typeof event == "undefined" || event.currentTarget.className == "gjs-trt-trait__wrp")) || (model.attributes.type == "section" && (typeof event == "undefined" || event.currentTarget.className == "gjs-trt-trait__wrp")) || (model && model.view && model.view.el && model.view.el.classList && (model.view.el.classList.contains('carousel-control') || model.view.el.classList.contains('carousel-indicators') || model.view.el.classList.contains('carousel-indicator'))))
									return false;
								else {
									if ($('#iframeHolder iframe').attr('src') == undefined || $('#iframeHolder iframe').attr('src').indexOf(vjEditorSettings.RevisionGUID) < 0)
										ShowBlockUI();
								}
							});

                            //Tooltip
                            $('[data-toggle="tooltip"]').tooltip();

                            $('.block-elements .blockItem').on("click", function (e) {
                                var messagesrc = CurrentExtTabUrl + "&guid=" + $(this).attr('guid');

                                $('#iframeHolder').find('iframe').attr('src', 'about:blank');
                                setTimeout(function () {
                                    $("#BlockManager").hide();
                                    $("#StyleToolManager").hide();
                                    $("#iframeHolder").fadeIn();
                                }, 300);
                                e.preventDefault();
                                $('#iframeHolder').find('iframe').attr('src', messagesrc);
                            });

                            $(VjEditor.Canvas.getDocument()).on("dblclick", "#wrapper", function (e) {
                                Stylemanager();
                            });

                            $('.jsPanel-content').on("mousedown", function () {
                                Stylemanager();
                            });
                        }
                        $(window.parent.document.body).find('.pageloader').remove();
                    }
                });
            }
        }
        else {
            $('.sidebar').removeClass('sidebar-open').addClass('settingclosebtn');
        }
    }

    var ShowBlockUI = function () {
        $("#iframeHolder, #StyleToolManager, .Menupanel-top").hide();
        $(".panel-top, #BlockManager, .block-set, #ContentBlocks").show();
    };

    var GrapesjsDestroy = function () {
        if (VjEditor) {
            VjEditor.destroy();
            $.get(CurrentTabUrl + (CurrentTabUrl.indexOf("?") != -1 ? "&uxmode=true" : "?uxmode=true"), function (data) {
                var html = $.parseHTML(data);
                var dom = $(data);
                var newhtml = $(html).find('#dnn_ContentPane');
                $('#dnn_ContentPane').html('');
                $('#dnn_ContentPane').html($(newhtml).html());
                $('#dnn_ContentPane').removeClass("sidebar-open");
                var Scripts_Links = $(data).find('script');
                $.each(dom, function (i, v) {
                    Scripts_Links.push(v);
                });
                InjectLinksAndScripts(Scripts_Links, window.document);
                InitAppActionMenu();
                $(window.parent.document.body).find('.pageloader').remove();
            });
        }
        // Remove All Managers
        $('#ContentBlocks, .stylemanager, .traitsmanager').empty();
    };

	var VjInit = function () {
		$.get(CurrentTabUrl, function (data) {
			var html = $.parseHTML(data);
			var dom = $(data);
			var newhtml = $(html).find('#dnn_ContentPane');
			$('#dnn_ContentPane').html('');
			$('#dnn_ContentPane').html($(newhtml).html());
			$('#dnn_ContentPane').removeClass("sidebar-open");
			var Scripts_Links = $(data).find('script');
			$.each(dom, function (i, v) {
				Scripts_Links.push(v);
			});
			InjectLinksAndScripts(Scripts_Links, window.document);
			setCookie("vj_InitUX", "true");
			if (window.location.href.indexOf('#') > 0 && window.location.href.split("#")[0] != CurrentTabUrl) {
				window.location.href = CurrentTabUrl;
			}
			$(this).find("em").addClass("fa-chevron-left").removeClass("fa-chevron-right");
			$('#dnn_ContentPane').addClass("sidebar-open").removeClass('sidebar-close');
			$('.sidebar').animate({ "left": "0" }, "fast").removeClass('settingclosebtn');
            $('.panel-top, .add-custom , #ContentBlocks').show();
			window.GrapesjsInit(vjEditorSettings);
			DestroyAppActionMenu();
		});
	};
	$("#mode-switcher").click(function () {

        if ($(window.parent.document.body).find('.pageloader').length <= 0)
            $(window.parent.document.body).append('<div class="pageloader"><div class="modal-backdrop fade show"></div><img class="revisionloader revisionloaderimg" src="' + window.parent.VjDefaultPath + 'loading.gif" /></div>');

		if ($("#mode-switcher").offset().left > 0) {
			setCookie("vj_IsPageEdit", "false");
			setCookie("vj_InitUX", "false");
			GrapesjsDestroy();
			$(this).find("em").addClass("fa-chevron-right").removeClass("fa-chevron-left");
			$('#dnn_ContentPane').removeClass("sidebar-open").addClass('sidebar-close');
			$('.sidebar').animate({ "left": "-300px" }, "fast").addClass('settingclosebtn');
			$('.modal-toggle').hide();
			$('.uxmanager-modal').modal('hide');
			if (VjLayerpanel != null)
				VjLayerpanel.close();
			if (GetParameterByName('m2v', parent.window.location) != null && GetParameterByName('m2v', parent.window.location).startsWith('true')) {
				window.location.href = m2vPageTabUrl;
			}
		}
		else {
			setCookie("vj_IsPageEdit", "true");
			VjInit();
		}
	});

    $(".managetab a").click(function () {
        var $this = $(this).parent();
        if ($this.hasClass('traitstab')) {
            $('.traitmanage').removeClass('active');
            $(this).parent().addClass('active');
            $('.stylemanager').hide();
            $('.traitsmanager').show();
        }
        else {
            $('.traitmanage').removeClass('active');
            $(this).parent().addClass('active');
            $('.traitsmanager').hide();
            $('.stylemanager').show();
            $('.stylemanager .gjs-sm-sector').click(function () {
                var $this = $(this);
                var sectorName = $this.attr('id').replace("gjs-sm-", "");
                if ($this.find('.gjs-sm-properties').is(':visible')) {
                    $.each(VjEditor.StyleManager.getSectors().models, function (index, model) {
                        model.set('open', false);
                    });
                    VjEditor.StyleManager.getSector(sectorName).set('open', true);
                }
            });
        }

        var selected = VjEditor.getSelected();
        VjEditor.select();
        VjEditor.select(selected);
    });

    $(".notification-set a").click(function () {
        var $this = $(this).parent();

        if ($this.hasClass('Texttab')) {
            $('.notificationtab').removeClass('active');
            $(this).parent().addClass('active');
            $('.Messageblock').hide();
            $('.Textblock').show();
        }
        else {
            $('.notificationtab').removeClass('active');
            $(this).parent().addClass('active');
            $('.Textblock').hide();
            $('.Messageblock').show();
        }
    });


    $(".blockstabview a").click(function () {
        var $this = $(this).parent();

        if ($this.hasClass('elementtab')) {
            $('.blockstab').removeClass('active');
            $(this).parent().addClass('active');
            ChangeBlockType();
        }
        else if ($this.hasClass('customtab')) {
            $('.blockstab').removeClass('active');
            $(this).parent().addClass('active');
            ChangeBlockType();
        }
        else if ($this.hasClass('librarytab')) {
            $('.blockstab').removeClass('active');
            $(this).parent().addClass('active');
            parent.OpenPopUp(null, '100%', 'center', VjLocalized.TemplateLibrary, TemplateLibraryURL, '100%', true, false, null, false);
        }
        else {
            $('.blockstab').removeClass('active');
            $(this).parent().addClass('active');
            ChangeBlockType();
        }
    });
    //Show Publish Options 
    //$(".draft-btn").click(function (e) {
    //    $('#DeviceManager').hide();

    //    if ($("#dnn_ContentPane").find(".gjs-frame").attr("style") == undefined)
    //        $("#dnn_ContentPane").find(".gjs-cv-canvas").removeAttr("style");

    //    $('#SettingButton').slideToggle(100);
    //    e.stopPropagation();
    //});

    $(document).click(function (e) {
        if ($(e.target).is('#DeviceManager *,.ntoolbox *')) {
            e.preventDefault();
            return;
        }
        $('#DeviceManager,#LanguageManager').hide();
        if ($("#dnn_ContentPane").find(".gjs-frame").attr("style") == undefined)
            $("#dnn_ContentPane").find(".gjs-cv-canvas").removeAttr("style");

        $('#SettingButton,#DeviceManager,#LanguageManager,.ntoolbox').hide();
    });

    //Change Device
    $(".device-manager .device-view").click(function () {
        var $this = $(this);
        var $body = $('body');
        var $iframe = $('.gjs-frame');

        $(".device-manager .device-view").removeClass("active");
        $this.addClass("active");

        $(".panelfooter .ResponsiveMode").find("em").removeClass(function (index, css) {
            return (css.match(/\bfa-\S+/g) || []).join(' ');
        }).addClass("fa-" + $this.attr("id").toLowerCase());

        //Desktop
        if ($this.attr("id") == "Desktop") {

            ChangeColumnResizeSpeed(0.075);

            if (vjEditorSettings.EditPage) {
                $iframe.removeClass("fixed-height");
                $iframe.contents().find("html").removeClass('responsive');
                $iframe.contents().find("#wrapper").removeClass("scrollbar");
                VjEditor.runCommand('set-device-desktop');
            }
            else
                $body.removeClass('resp-mode tablet mobile').addClass('resp-mode');
        }
        //Tablet Portrait
        else if ($this.attr("id") == "Tablet") {

            ChangeColumnResizeSpeed(0.1);

            if (vjEditorSettings.EditPage) {
                $iframe.removeClass("fixed-height");
                $iframe.contents().find("html").addClass('responsive');
                $iframe.contents().find("#wrapper").addClass("scrollbar");
                VjEditor.runCommand('set-device-tablet');
            }
            else
                $body.removeClass('mobile').addClass('resp-mode tablet');
        }
        //Mobile Portrait
        else if ($this.attr("id") == "Mobile") {

            ChangeColumnResizeSpeed(0.2);

            if (vjEditorSettings.EditPage) {
                $iframe.addClass("fixed-height");
                $iframe.contents().find("html").addClass('responsive');
                $iframe.contents().find("#wrapper").addClass("scrollbar");
                VjEditor.runCommand('set-device-mobile');
            }
            else
                $body.removeClass('tablet').addClass('resp-mode mobile');
        }

        var selected = VjEditor.getSelected();
        VjEditor.select();
        VjEditor.select(selected);
    });

    var Stylemanager = function () {

        $("#BlockManager, #ContentBlocks, #Notification, #iframeHolder, #SettingButton, #DeviceManager,.ntoolbox, .panel-top, .block-set").hide();    
        $("#StyleToolManager, .ssmanager").show();
    }

    $('.jsPanel-content').on("mousedown", function () {
        Stylemanager();
    });
});


if (document.addEventListener) {
    document.addEventListener('webkitfullscreenchange', exitHandler, false);
    document.addEventListener('mozfullscreenchange', exitHandler, false);
    document.addEventListener('fullscreenchange', exitHandler, false);
    document.addEventListener('MSFullscreenChange', exitHandler, false);
}

function RenderMarkup(name) {
    $.each(getAllComponents().filter(function (component) {
        return (component.attributes.name == name || component.attributes["custom-name"] != undefined && component.attributes["custom-name"] == name);
    }), function (index, value) {
        window.parent.RenderBlock(value);
    });
}

function exitHandler() {
    if (!document.webkitIsFullScreen && !document.mozFullScreen && !document.msFullscreenElement) {
        VjEditor.stopCommand("fullscreen");
        $(".Fullscreen").removeClass('active');
    }
}

global.RenderApp = function (iframe) {
    $(iframe).prev().hide();
    var model = window.parent.VjEditor.getSelected();
    iframe.style.height = iframe.contentWindow.document.body.offsetHeight + 'px';
    var AppMenusScript = $(iframe).contents().find('[data-actionmid]');
    if (!$('head').find('[data-actionmid=' + AppMenusScript.data("actionmid") + ']').length)
        $('head').append(AppMenusScript);
    var AppSettingsScript = $(iframe).contents().find('[data-settingsmid]');
    if (!$('head').find('[data-settingsmid=' + AppSettingsScript.data("settingsmid") + ']').length)
        $('head').append(AppSettingsScript);
    $(iframe.contentWindow.document.body).append('<style>.actionMenu {display: none !important;}</style>');
    VjEditor.select();
    model.initToolbar(true);
    setTimeout(function () {
        VjEditor.select(model);
    }, 0);

    iframe.contentWindow.document.body.addEventListener("DOMSubtreeModified", function () {
        iframe.style.height = this.offsetHeight + 'px';
    });

    if (iframe.src.indexOf('m2v') > -1) {
        $(iframe).contents().find(".style-wrapper").remove();
    }
}

global.ChangeColumnResizeSpeed = function (speed) {
    $(VjEditor.getComponents().where(x => x.attributes.type == 'grid')).each(function () {
        $(this.attributes.components.models[0].attributes.components.models).each(function () {
            var resizable = this.attributes.resizable;
            resizable.step = speed;
            this.set({ 'resizable': resizable });
        });
    });
}

global.getAllComponents = function (component) {
    component = component || editor.DomComponents.getWrapper();
    var components = component.get("components").models;
    component.get("components").forEach(function (component) {
        components = components.concat(getAllComponents(component));
    });

    return components;
};

global.ChangeBlockType = function (query) {
    var isGlobal = $('.globaltab').hasClass('active');
    var isCustom = $('.customtab').hasClass('active');
    var isLocal = $('.elementtab').hasClass('active');
    if (typeof VjEditor !== 'undefined' && VjEditor != undefined) {
        const blocks = VjEditor.BlockManager.getAll();
        var localBlocks = [];
        var globalBlocks = [];
        var customblocks = [];
        $.each(blocks.models, function (k, v) {
            if (v.attributes != undefined && v.attributes.attributes != undefined && v.attributes.attributes.isGlobalBlock != undefined && v.attributes.attributes.isGlobalBlock == true)
                globalBlocks.push(v);
            else if (v.attributes.attributes.isGlobalBlock == false)
                customblocks.push(v);
            else
                localBlocks.push(v);
        });

        if (query != undefined && query == 'search') {
            if (!isGlobal && !isCustom && isLocal)
                return localBlocks;
            else if (!isLocal && !isGlobal && isCustom)
                return customblocks;
            else
                return globalBlocks;
        }

        if (!isGlobal && !isCustom && isLocal)
            $('#ContentBlocks').empty().append(VjEditor.BlockManager.render(localBlocks));
        else if (!isLocal && !isGlobal && isCustom) {
            if (customblocks.length <= 0)
                $('#ContentBlocks').empty().append('<div class="empty_msg">' + VjLocalized.NoCustomBlockFound + '</div>');
            else
                $('#ContentBlocks').empty().append(VjEditor.BlockManager.render(customblocks));
        }
        else {
            if (globalBlocks.length <= 0)
                $('#ContentBlocks').empty().append('<div class="empty_msg">' + VjLocalized.NoBlockFound + '</div>');
            else
                $('#ContentBlocks').empty().append(VjEditor.BlockManager.render(globalBlocks));
        }
    }
};

function RunSaveCommand() {
    $.each(getAllComponents(), function (k, v) {
        if (v.attributes.type == "globalblockwrapper" && $(v.getEl()).find('.fa-unlock').length <= 0)
            UpdateGlobalBlock(v);
    });
    editor.StorageManager.getStorages().remote.attributes.params.IsPublished = true;
    if (GetParameterByName('m2v', parent.window.location) != null)
        editor.StorageManager.getStorages().remote.attributes.params.m2v = true;
    editor.runCommand("save");
    editor.StorageManager.getStorages().remote.attributes.params.IsPublished = false;
    $('#VJBtnPublish').addClass('disabled');
    $($('.panelheader .blockItem')[1]).click();
    editor.StorageManager.getStorages().remote.attributes.params.Comment = "";
    $('#VJReviewComment', parent.document).val('');
};

global.ToggleBlockType = function (e, type) {
    if (!$('#' + e.id).hasClass('disabled')) {
        if (type == 'yes') {
            $(e).attr('class', 'btn btn-primary active');
            $(e).next().attr('class', 'btn btn-default');
        }
        else {
            $(e).attr('class', 'btn btn-primary disabled');
            $(e).prev().attr('class', 'btn btn-default');
        }
    }
};

global.UnlockGlobalBlock = function ($this) {

    if (IsAdmin) {
        swal({
            title: VjLocalized.AreYouSure,
            text: VjLocalized.GlobalBlockUnlocking,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: VjLocalized.ReviewYES,
            cancelButtonText: VjLocalized.Cancel,
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    $this.parents('[data-gjs-type="globalblockwrapper"]').css('pointer-events', 'auto');
                    $this.parents('.global-tools').remove();
                }
            });
    }
    else {
        swal(VjLocalized.GlobalBlockUnlockingError);
    }
}

global.getUrlVars = function () {

    var output = new Array();

    if (location.search == "") {
        var qs = location.pathname;
        //strip leading path slash
        qs = qs.substring(1);
        var a = qs.split('/');
        var i;
        for (i = 0; i < a.length; i += 2) {
            //append key->value pairs to output array
            output[a[i]] = a[i + 1];
        }
    }
    else {
        var hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            output.push(hash[0]);
            if (typeof hash[1] == 'undefined')
                output[hash[0]] = null;
            else
                output[hash[0]] = hash[1];
        }
    }

    return output;
}


// start review toast 
global.ConfirmReviewChange = function (FirstStateName) {
    swal({
        title: VjLocalized.ReviewAreYouSure,
        text: VjLocalized.ReviewUnlockingThePage + " " + FirstStateName,
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55", confirmButtonText: VjLocalized.ReviewYES,
        cancelButtonText: VjLocalized.ReviewNO,
        closeOnConfirm: true,
        closeOnCancel: true
    },
        function (isConfirm) {
            if (isConfirm) {
                $('.gjs-cv-canvas__frames').removeClass('lockcanvas');
                VJIsLocked = 'False';
                $('.toast-close-button').click();
                editor.StorageManager.getStorages().remote.attributes.params.IsPublished = false;
                editor.StorageManager.getStorages().remote.attributes.params.Comment = '';
                editor.runCommand("save");
                $('#VJBtnPublish').removeClass('disabled');
            }
        });
}
// end review toast

global.ChangeToWebp = function (target, URLs) {

    if (typeof target != 'undefined' && target.attributes.type == "picture-box") {

        var markup = "", maxWidth = "";

        var webp = jQuery.grep(URLs, function (n, i) {
            return (n.Type == 'webp');
        });

        var srcWebp = "";

        $(webp).each(function (index, value) {

            if (index == 0)
                maxWidth = value.Width;

            srcWebp += value.Url + ' ' + value.Width + 'w';

            if (webp.length != index + 1) {
                srcWebp += ',';
            }

        });

        var sourceWebp = document.createElement('source');
        sourceWebp.setAttribute("class", "source");
        sourceWebp.setAttribute("type", "image/webp");
        sourceWebp.setAttribute("srcset", srcWebp);

        var image = jQuery.grep(URLs, function (n, i) {
            return (n.Type != 'webp');
        });

        var srcImage = "";

        $(image).each(function (index, value) {

            srcImage += value.Url + ' ' + value.Width + 'w';

            if (webp.length != index + 1)
                srcImage += ',';

        });

        var sourceImg = document.createElement('source');
        sourceImg.setAttribute("class", "source");
        sourceImg.setAttribute("srcset", srcImage);

        markup += sourceWebp.outerHTML + sourceImg.outerHTML;

        var img = '', style = '';

        if (target.view.$el.find('img').length)
            img = target.view.$el.find('img')[0].outerHTML

        if (target.components().length) {

            $(target.components().models).each(function (index, item) {

                if (item.attributes.type == "image" || item.attributes.type == "image-gallery-item") {

                    window.document.vj_image_target = target.components().models[2];

                    style = item.getStyle();
                    style['max-width'] = maxWidth + 'px';
                }
            });
        }

        target.components([]);
        target.append(markup + img);

        $(target.components().models).each(function (index, item) {

            if (item.attributes.type == "image" || item.attributes.type == "image-gallery-item")
                target.components().models[2].setStyle(style);

        });
    }
}




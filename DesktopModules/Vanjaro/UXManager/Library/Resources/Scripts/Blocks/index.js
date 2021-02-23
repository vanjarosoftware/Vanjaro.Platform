import grapesjs from 'grapesjs';
import pluginstylefilter from 'grapesjs-style-filter';
import customcode from './custom-code';
import plugintuiimageeditor from 'grapesjs-tui-image-editor';
import plugintouch from 'grapesjs-touch';
import spacer from './spacer';
import divider from './divider';
import button from './button';
import grid from './grid';
import icon from './icon';
import heading from './heading';
import text from './text';
import list from './list';
import video from './video';
import section from './section';
import loadComponents from './components';
import loadTraits from './traits';
import loadStyles from './style-manager';
import link from './link';
import image from './image';
import map from './map';
import imageGallery from './image-gallery';
import carousel from './carousel';

export default grapesjs.plugins.add('vjpreset', (editor, opts = {}) => {

	let config = opts;

	const opts_labels = opts.labels || {};

	const default_labels = {
		// LAYOUT
		container: 'Container',
		row: 'Row',
		column: 'Column',
		column_break: 'Column Break',
		media_object: 'Media Object',
		// COMPONENTS
		alert: 'Alert',
		tabs: 'Tabs',
		tab: 'Tab',
		tabPane: 'Tab Pane',
		badge: 'Badge',
		button: 'Button',
		button_group: 'Button Group',
		button_toolbar: 'Button Toolbar',
		card: 'Card',
		card_container: 'Card Container',
		collapse: 'Collapse',
		dropdown: 'Dropdown',
		dropdown_menu: 'Dropdown Menu',
		dropdown_item: 'Dropdown Item',
		// TYPOGRAPHY
		text: 'Text',
		heading: 'Heading',
		paragraph: 'Paragraph',
		// BASIC
		image: 'Image',
		link: 'Link',
		list: 'Simple List',
		// FORMS
		form: 'Form',
		input: 'Input',
		file_input: 'File',
		input_group: 'Input group',
		textarea: 'Textarea',
		select: 'Select',
		select_option: '- Select option -',
		option: 'Option',
		label: 'Label',
		checkbox: 'Checkbox',
		radio: 'Radio',
		trait_method: 'Method',
		trait_enctype: 'Encoding Type',
		trait_multiple: 'Multiple',
		trait_action: 'Action',
		trait_state: 'State',
		trait_id: 'ID',
		trait_for: 'For',
		trait_name: 'Name',
		trait_placeholder: 'Placeholder',
		trait_value: 'Value',
		trait_required: 'Required',
		trait_type: 'Type',
		trait_options: 'Options',
		trait_checked: 'Checked',
		type_text: 'Text',
		type_email: 'Email',
		type_password: 'Password',
		type_number: 'Number',
		type_date: 'Date',
		type_hidden: 'Hidden',
		type_submit: 'Submit',
		type_reset: 'Reset',
		type_button: 'Button',
	};

	let defaults = {

		blocks: {
			spacer: true,
			divider: true,
			button: true,
			grid: true,
			icon: true,
			heading: true,
			text: true,
			list: true,
			video: true,
			section: true,
			link: true,
			image: true,
			map: true,
			imageGallery: true,
			slider: true,
		},

		labels: Object.assign(default_labels, opts_labels),

		// Modal import title
		modalImportTitle: 'Import',

		// Modal import button text
		modalImportButton: 'Import',

		// Import description inside import modal
		modalImportLabel: '',

		// Default content to setup on import model open.
		// Could also be a function with a dynamic content return (must be a string)
		// eg. modalImportContent: editor => editor.getHtml(),
		modalImportContent: '',

		// Code viewer (eg. CodeMirror) options
		importViewerOptions: {},

		// Confirm text before cleaning the canvas
		textCleanCanvas: 'Are you sure to clean the canvas?',

		// Show the Style Manager on component change
		showStylesOnChange: 1,

		// `grapesjs-navbar` plugin options
		// By setting this option to `false` will avoid loading the plugin
		grapickOpts: {},

		// Custom color picker, check Grapick's repo to get more about it
		// If you leave it empty the native color picker will be used.
		// You can use 'default' string to get the one used by Grapesjs (which
		// is spectrum at the moment of writing)
		colorPicker: '',

		// Show gradient direction input under picker, you can pass an object
		// as a model
		inputDirection: 1,

		// Show gradient type input under picker, you can pass an object as
		// a model
		inputType: 1,

		// `grapesjs-plugin-forms` plugin options
		// By setting this option to `false` will avoid loading the plugin
		stylefilterOpts: {},

		tuiimageeditorOpts: {
			config: {
				includeUI: {
					menu: ['crop', 'flip', 'rotate', 'draw', 'text', 'mask', 'filter'],
				},
				cssMaxWidth: 560,
				cssMaxHeight: 350,
			},
			icons: {
				'menu.normalIcon.path': VjDefaultPath + '/svg/icon-d.svg',
				'menu.activeIcon.path': VjDefaultPath + '/svg/icon-b.svg',
				'menu.disabledIcon.path': VjDefaultPath + '/svg/icon-a.svg',
				'menu.hoverIcon.path': VjDefaultPath + '/svg/icon-c.svg',
				'submenu.normalIcon.path': VjDefaultPath + '/svg/icon-d.svg',
				'submenu.activeIcon.path': VjDefaultPath + '/svg/icon-c.svg',
			},
			onApply: function (imageEditor, imageModel) {
				if (!$('.optimizing-overlay').length)
					$('.vj-wrapper').prepend('<div class="optimizing-overlay"><h1><img class="centerloader" src="' + VjDefaultPath + 'loading.svg" />Optimizing Images</h1></div>');
				var sf = $.ServicesFramework(-1);
				var ImageData = {
					PreviousFileName: imageModel._previousAttributes.attributes.src.substring(imageModel._previousAttributes.attributes.src.lastIndexOf('/') + 1).split('?')[0],
					ImageByte: imageEditor.toDataURL()
				};
				$.ajax({
					type: "POST",
					url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Image") + "Image/Convert",
					data: ImageData,
					headers: {
						'ModuleId': parseInt(sf.getModuleId()),
						'TabId': parseInt(sf.getTabId()),
						'RequestVerificationToken': sf.getAntiForgeryValue()
					},
					success: function (response) {
						if (response != "failed") {
							imageModel.set('src', response.Url);
							var parentmodel = imageModel.parent();
							ChangeToWebp(parentmodel, response.Urls);
							$('.gjs-toolbar').hide();
							VjEditor.Modal.close();
						}
						$('.vj-wrapper').find('.optimizing-overlay').remove();
					}
				});
			}
		},

		touchOpts: {},

		flexboxBlock: {},

		// Classes prefix
		stylePrefix: '',

		// Row label
		labelRow: 'Row',

		// Column label
		labelColumn: 'Column',

		// Label of the custom code block
		blockLabel: VjLocalized.CustomCode,

		// Object to extend the default custom code block, eg. { label: 'Custom Code', category: 'Extra', ... }.
		// Pass a falsy value to avoid adding the block
		blockCustomCode: {},

		// Object to extend the default custom code properties, eg. `{ name: 'Custom Code', droppable: false, ... }`
		propsCustomCode: {},

		// Initial content of the custom code component
		placeholderContent: '<span>Insert here your custom code</span>',

		// Object to extend the default component's toolbar button for the code, eg. `{ label: '</>', attributes: { title: 'Open custom code' } }`
		// Pass a falsy value to avoid adding the button
		toolbarBtnCustomCode: {},

		// Content to show when the custom code contains `<script>`
		placeholderScript: `<div style="pointer-events: none; padding: 10px;">
      <svg viewBox="0 0 24 24" style="height: 30px; vertical-align: middle;">
        <path d="M13 14h-2v-4h2m0 8h-2v-2h2M1 21h22L12 2 1 21z"></path>
        </svg>
      Custom code with <i>&lt;script&gt;</i> can't be rendered on the canvas
    </div>`,

		// Title for the custom code modal
		modalTitle: 'Insert your code',

		// Additional options for the code viewer, eg. `{ theme: 'hopscotch', readOnly: 0 }`
		codeViewOptions: {},

		// Label for the default save button
		buttonLabel: 'Update',

		// Object to extend the default custom code command.
		// Check the source to see all available methods
		commandCustomCode: {},
	};


	// Load defaults
	for (let name in defaults) {
		if (!(name in config))
			config[name] = defaults[name];
	}

	const {
		stylegradientOpts,
		stylefilterOpts,
		tuiimageeditorOpts,
		touchOpts,
	} = config;

	// Load plugins
	//config && pluginstylegradient(editor, config);
	stylefilterOpts && pluginstylefilter(editor, stylefilterOpts);

	tuiimageeditorOpts && plugintuiimageeditor(editor, tuiimageeditorOpts);
	touchOpts && plugintouch(editor, touchOpts);

	// Custom Blocks
	config && section(editor, config);
	config && grid(editor, config);
	config && heading(editor, config);
	config && text(editor, config);
	config && button(editor, config);
	config && icon(editor, config);
	config && link(editor, config);
	config && list(editor, config);
	config && image(editor, config);
	config && imageGallery(editor, config);
	config && carousel(editor, config);
	config && video(editor, config);
	config && map(editor, config);
	config && customcode(editor, config);
	config && spacer(editor, config);
	config && divider(editor, config);

	// Load Componenents
	loadComponents(editor, config);

	// Load Traits
	loadTraits(editor, config);

	loadStyles(editor, config);


	//Add Commands Switch Device 
	editor.Commands.add('set-device-desktop', {
		run: editor => editor.setDevice('Desktop')
	});
	editor.Commands.add('set-device-tablet', {
		run: editor => editor.setDevice('Tablet')
	});
	editor.Commands.add('set-device-mobile', {
		run: editor => editor.setDevice('Mobile')
	});

	editor.Commands.add('save', ed => {
		ed.store();
	});

	//Implement Search
	var inputVal;

	editor.Commands.add('search-filter', {
		run(editor, sender) {
			var r = inputVal
				, i = editor.BlockManager
				, a = ChangeBlockType('search').filter(function (t) {

					if (r && r.length > 1) {

						if (t.get("id") != undefined && t.get("id").toLowerCase().indexOf(r.toLowerCase()) >= 0)
							return true;
						else if (t.get('category').id != undefined && t.get('category').id.toLowerCase().indexOf(r.toLowerCase()) >= 0)
							return true;
						else
							return false;
					}
					return !0
				});
			i.render(a)
		}
	});

	$(".search-block input").keyup(function (e) {
		inputVal = $(this).val();
		editor.runCommand("search-filter");
	});

	$(".close-searchbtn").click(function () {
		inputVal = $(this).val();
		editor.runCommand("search-filter");
	});

	var modalContent = document.getElementById('ModalContent');
	var saveButton = document.getElementById("btn-save");

	var $BlockName = $("#ModalContent").find("#input-name");
	var $BlockCategory = $("#ModalContent").find("#input-category");
	var $CategoryDropdown = $("#ModalContent").find("#CategoryDropdown");

	$CategoryDropdown.on('change', function () {
		$BlockCategory.val($(this).val());
	});

	var AddCategory = function () {

		$("#ModalContent").find(".btn-add").click(function () {

			var categories = editor.BlockManager.getCategories();
			categories.remove(['Basic', 'Apps', 'Design']);

			swal(
				{
					title: VjLocalized.BlockCategory,
					html: true,
					text: '<input class="form-control" style="display:block;" type="text" id="Category" placeholder="' + VjLocalized.AddCategory + '"/>',
					confirmButtonText: VjLocalized.Add,
					cancelButtonText: VjLocalized.Cancel,
					showCancelButton: true,
					closeOnConfirm: false,
				},
				function (inputValue) {

					var category = $('#Category').val();

					if (category != '') {

						var count = 0;

						$(categories.models).each(function (index, item) {

							if (item.attributes.label == category) {

								count = 1;
								swal.showInputError(VjLocalized.CategoryExists);

								return false;
							}
						});

						if (count == 0) {

							$CategoryDropdown.append('<option value="' + category + '">' + category + '</option>');
							$CategoryDropdown.val(category);

							$BlockCategory.val(category);
							swal.close();
						}

					}
					else {
						swal.showInputError(VjLocalized.CategoryRequired);
						return false;
					}
				}
			);
		});
	}

	var waitForChange = function (closed) {

		if (closed) {
			$(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').removeAttr('style');
		} else {
			setTimeout(function () {
				waitForChange($(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').is(":hidden"));
			}, 10);
		}
	};

	//Add Custom Block
	editor.Commands.add('custom-block', {
		run: function (editor, sender, opts) {

			$("#ModalContent").show();

			var Block = VjEditor.runCommand("export-component");

			$BlockName.val('');
			$BlockCategory.val('');
			

			$BlockCategory.hide();
			$CategoryDropdown.parent().hide();

			var categories = editor.BlockManager.getCategories();
			categories.remove(['Basic', 'Apps', 'Design']);

			if (categories.length > 0) {

				$CategoryDropdown.parent().show();

				var select = document.getElementById("CategoryDropdown");
				$(select).empty();

				select.innerHTML = '<option value="none" disabled>Select Category</option>';

				$(categories.models).each(function (index, category) {

					var option = document.createElement("option");
					option.text = category.attributes.label;
					option.value = category.attributes.id;

					select.append(option);

				});

				$(select).html($(select).find('option').sort(function (x, y) {
					return $(x).text() > $(y).text() ? 1 : -1;
				}));

				$CategoryDropdown.val('none');
				AddCategory();
			}
			else
				$BlockCategory.show();

			$('.cbglobal > button:first').attr('class', 'btn btn-default');
			$('.cbglobal > button:last').attr('class', 'btn btn-primary disabled');

			//Update Button
			saveButton.onclick = function (e) {

				e.preventDefault();

				var blockLabel = $BlockName.val();
				var blockCategory = $BlockCategory.val();
				var blockContent = Block.html;
				var blockCSS = Block.css;

				if (blockLabel == "") {
					swal({
						title: "Name is required",
						type: "warning",
						closeOnCancel: true
					});
					return false;
				}

				if (blockCategory == "") {
					swal({
						title: "Category is required",
						type: "warning",
						closeOnCancel: true
					});
					return false;
				}

				var CustomBlock = {
					Name: blockLabel,
					Category: blockCategory,
					Html: blockContent,
					Css: blockCSS,
					IsGlobal: $('.cbglobal > button').hasClass('btn-primary active')

				};

				AddCustomBlock(editor, CustomBlock);

				waitForChange($(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').is(":hidden"));
			};

			$('.gjs-mdl-dialog .gjs-mdl-btn-close').click(function () {
				$(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').removeAttr('style');
			});

			$(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').css('max-width', 450);
			editor.Modal.setTitle(VjLocalized.CustomBlock).setContent(modalContent).open();
		}
	});

	//Edit Custom Block
	editor.Commands.add('edit-custom-block', {

		run: function (editor, sender, block) {

			$("#ModalContent").show();

			$BlockCategory.hide();

			global.BlockID = block.id;

			var Block = editor.BlockManager.get(block.name);

			$BlockName.val(Block.attributes.label);

			var categories = editor.BlockManager.getCategories();
			categories.remove(['Basic', 'Apps', 'Design']);

			var select = document.getElementById("CategoryDropdown");
			$(select).empty();

			$(categories.models).each(function (index, category) {

				var option = document.createElement("option");
				option.text = category.attributes.label;
				option.value = category.attributes.id;

				select.append(option);

			});

			$(select).html($(select).find('option').sort(function (x, y) {
				return $(x).text() > $(y).text() ? 1 : -1;
			}));

			if (Block.attributes.category.id != undefined) {
				$BlockCategory.val(Block.attributes.category.id.toLowerCase());
				$CategoryDropdown.val(Block.attributes.category.id.toLowerCase());
			}
			else {
				$BlockCategory.val(Block.attributes.category.toLowerCase());
				$CategoryDropdown.val(Block.attributes.category.toLowerCase());
			}

			AddCategory();

			//Need Improvement
			var blockType = $(Block.attributes.content).attr('data-block-type')

			if (blockType != undefined) {
				$('.cbglobal > button:first').attr('class', 'btn btn-primary active');
				$('.cbglobal > button:last').attr('class', 'btn btn-default');
			}
			else {
				$('.cbglobal > button:first').attr('class', 'btn btn-default');
				$('.cbglobal > button:last').attr('class', 'btn btn-primary disabled');
			}

			saveButton.onclick = function (e, block) {

				e.preventDefault();

				var blockLabel = $("#ModalContent").find("#input-name").val();
				var blockCategory = $("#ModalContent").find("#input-category").val();

				if (blockLabel == "") {
					swal({
						title: "Name is required",
						type: "warning",
						closeOnCancel: true
					});
					return false;
				}

				if (blockCategory == "") {
					swal({
						title: "Category is required",
						type: "warning",
						closeOnCancel: true
					});
					return false;
				}

				var CustomBlock = {
					Guid: BlockID,
					Name: blockLabel,
					Category: blockCategory,
					IsGlobal: $('.cbglobal > button').hasClass('btn-primary active')
				};

				UpdateCustomBlock(editor, CustomBlock);

				waitForChange($(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').is(":hidden"));
					
			};

			$('.gjs-mdl-dialog .gjs-mdl-btn-close').click(function () {
				$(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').removeAttr('style');
			});

			$(editor.Modal.getContentEl()).parents('.gjs-mdl-dialog').css('max-width', 450);
			editor.Modal.setTitle(VjLocalized.CustomBlock).setContent(modalContent).open();

			$('#ToggelBlockGlobal').addClass('disabled');
			$('#ToggelBlockLocal').addClass('disabled');

			$('#ToggelBlockGlobal').addClass('custom-block-disabled');
			$('#ToggelBlockLocal').addClass('custom-block-disabled');
		}
	});

	//Delete Custom Block
	editor.Commands.add('delete-custom-block', {
		run: function (editor, sender, block) {
			var BlockIDDelete = block.id;
			swal({
				title: VjLocalized.AreYouSure,
				text: VjLocalized.AreYouSureText,
				type: "warning",
				showCancelButton: true,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: VjLocalized.Delete,
				cancelButtonText: VjLocalized.Cancel,
				closeOnConfirm: true,
				closeOnCancel: true
			},

				function (isConfirm) {
					if (isConfirm) {
						DeleteCustomBlock(BlockIDDelete);
					}
				});
		}
	});

	//Export Custom Block
	editor.Commands.add('export-custom-block', {
		run: function (editor, sender, block) {
			var BlockIDExport = block.id;
			swal({
				title: VjLocalized.AreYouSure,
				text: VjLocalized.ExportMessage + block.name,
				type: "warning",
				showCancelButton: true,
				confirmButtonColor: "#8CD4F5",
				confirmButtonText: VjLocalized.Yes,
				cancelButtonText: VjLocalized.Cancel,
				closeOnConfirm: true,
				closeOnCancel: true
			},
				function (isConfirm) {
					if (isConfirm) {
						ExportCustomBlock(BlockIDExport);
					}
				});
		}
	});

	//global to local Custom Block
	editor.Commands.add('custom-block-globaltolocal', {
		run: function (editor, sender, block) {
			swal({
				title: VjLocalized.AreYouSure,
				text: VjLocalized.GlobalBlockUnlink,
				type: "warning",
				showCancelButton: true,
				confirmButtonColor: "#3085d6",
				confirmButtonText: VjLocalized.OK,
				cancelButtonText: VjLocalized.Cancel,
				closeOnConfirm: true,
				closeOnCancel: true
			},

				function (isConfirm) {
					if (isConfirm) {
						var model = VjEditor.getSelected();
						var parentmodel = model.parent();
						$(model.getEl()).find('.global-tools').remove();
						var content = VjEditor.runCommand("export-component", {
							component: model
						});
						if (content != undefined && content.html != undefined && content.html != "") {
							model.replaceWith($(content.html).html());
							$.each(getAllComponents(parentmodel), function (k, v) {
								if (v.attributes.type == 'blockwrapper') {
									if ($(v.getEl().innerHTML).attr('data-gjs-type') == 'blockwrapper')
										v.components($(v.getEl().innerHTML).html());
									else
										v.components(v.getEl().innerHTML);

									if (v.attributes.attributes["data-block-type"].toLowerCase() == "logo") {
										var style = v.attributes.attributes["data-style"];
										if (style != undefined) {
											$(v.getEl()).find('img').attr('style', style);
											v.set('content', v.getEl().innerHTML);
										}
									}
								}
							});
							if (content.css != undefined && content.css != "") {
								VjEditor.addComponents('<style>' + content.css + '</style>');
							}
							VjEditor.runCommand("save");
							window.parent.location.reload();
						}
					}
				});
		}
	});
});

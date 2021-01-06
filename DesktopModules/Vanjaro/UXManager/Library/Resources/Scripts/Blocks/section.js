export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.section) {
		bm.add('section', {
			label: `
				<svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
					<rect class="svg-block" x="2" y="4" width="20" height="15" fill="none" stroke="#000" stroke-width="1.5"></rect>
					<rect x="6" y="9" width="8" height="1" stroke="none"></rect>
					<rect x="6" y="13" width="12" height="1" stroke="none"></rect>
				</svg>
				<div class="gjs-block-label">`+ VjLocalized.Section + `</div>
			`,
			category: VjLocalized.Basic,
			content: `
				<section class="vj-section">
				</section>
			`,
		});

	}

	let domc = editor.DomComponents;
	const defaultType = domc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;

	domc.addType('section', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {

					var GetBlockMenus = function () {
						var Result = [];
						if (IsAdmin)
							Result.push({ 'Title': 'Save As Block', 'Command': 'custom-block' });
						return Result;
					};

					var tb = [];

					if (GetBlockMenus().length > 0) {
						tb.push({
							attributes: { class: 'fa fa-bars' },
							command: function (t) {
								return t.runCommand("tlb-app-actions", {
									BlockMenus: GetBlockMenus()
								})
							}
						});
					}

					tb.push({
						attributes: { class: 'fa fa-arrow-up' },
						command: function (t) {
							return t.runCommand("core:component-exit", {
								force: 1
							})
						}
					});

					if (model.get('draggable')) {
						tb.push({
							attributes: { class: 'fa fa-arrows' },
							command: 'tlb-move',
						});
					}

					if (model.get('copyable')) {
						tb.push({
							attributes: { class: 'fa fa-clone' },
							command: 'tlb-clone',
						});
					}

					tb.push({
						attributes: { class: 'fa fa-cog' },
						command: 'tlb-app-personalization',
					});

					if (model.get('removable')) {
						tb.push({
							attributes: { class: 'fa fa-trash-o' },
							command: 'tlb-delete',
						});
					}

					model.set('toolbar', tb);
				}
			},
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				droppable: true,
				tagName: 'section',
				resizable: {
					tl: 0, // Top left
					tr: 0, // Top right
					cl: 0, // Center left
					cr: 0, // Center right
					bl: 0, // Bottom left
					br: 0, // Bottom right
					keyHeight: 'min-height',
				},
				traits: [{
					label: "Id",
					name: "id",
					type: 'text',
				}, {
					label: "Type",
					name: "tag",
					type: 'select',
					options: [
						{ id: 'article', name: 'Article' },
						{ id: 'div', name: 'Div' },
						{ id: 'footer', name: 'Footer' },
						{ id: 'header', name: 'Header' },
						{ id: 'nav', name: 'Nav' },
						{ id: 'section', name: 'Section' },
					],
					default: 'section',
					changeProp: 1,
				}, {
					label: "Gap",
					name: "gap",
					type: "custom_range",
					cssproperties: [
						{ name: "padding-top" },
						{ name: "padding-bottom" }
					],
					min: "0",
					max: "100",
					value: "50",
					changeProp: 1,
				}, {
					label: "Background",
					name: "background",
					type: "toggle_radio",
					SwitchClass: true,
					options: [
						{ id: 'none', name: 'none', icon: "fas fa-ban", class: '' },
						{ id: 'image', name: 'image', icon: "fas fa-paint-brush", class: '' },
						{ id: 'gradient', name: 'gradient', icon: "fas fa-fill", class: '' },
						{ id: 'video', name: 'video', icon: "fas fa-video", class: '' },
					],
					value: 'none',
					changeProp: 1,
				}, {
					label: "Color",
					name: "backgroundcolor",
					type: "custom_color",
					cssproperties: [
						{ name: "background-color" }
					],
					options: [
						{ id: 'colorprimary', color: 'bg-primary', name: 'Primary', class: 'bg-primary' },
						{ id: 'colorsecondary', color: 'bg-secondary', name: 'Secondary', class: 'bg-secondary' },
						{ id: 'colortertiary', color: 'bg-tertiary', name: 'Tertiary', class: 'bg-tertiary' },
						{ id: 'colorquaternary', color: 'bg-quaternary', name: 'Quaternary', class: 'bg-quaternary' },
						{ id: 'colorsuccess', color: 'bg-success', name: 'Success', class: 'bg-success' },
						{ id: 'colorinfo', color: 'bg-info', name: 'Info', class: 'bg-info' },
						{ id: 'colorwarning', color: 'bg-warning', name: 'Warning', class: 'bg-warning' },
						{ id: 'colordanger', color: 'bg-danger', name: 'Danger', class: 'bg-danger' },
						{ id: 'colorlight', color: 'bg-light', name: 'Light', class: 'bg-light' },
						{ id: 'colordark', color: 'bg-dark', name: 'Dark', class: 'bg-dark' }
					],
					value: "primary",
					changeProp: 1,
				}, {
					label: " ",
					name: "backgroundimage",
					type: "uploader",
					changeProp: 1,
				}, {
					label: " ",
					name: "backgroundvideo",
					type: "uploader",
					changeProp: 1,
				}, {
					label: "Position",
					name: "imageposition",
					type: 'dropdown',
					cssproperties: [
						{ name: "background-position" }
					],
					options: [
						{ id: 'top left', name: 'Top Left' },
						{ id: 'top center', name: 'Top Center' },
						{ id: 'top right', name: 'Top Right' },
						{ id: 'center left', name: 'Middle Left' },
						{ id: 'center center', name: 'Middle Center' },
						{ id: 'center right', name: 'Middle Right' },
						{ id: 'bottom left', name: 'Bottom Left' },
						{ id: 'bottom center', name: 'Bottom Center' },
						{ id: 'bottom right', name: 'Bottom Right' },
					],
					value: 'center center',
					changeProp: 1,
				}, {
					label: "Attachment",
					name: "imageattachment",
					type: 'dropdown',
					cssproperties: [
						{ name: "background-attachment" }
					],
					options: [
						{ id: 'scroll', name: 'Scroll' },
						{ id: 'fixed', name: 'Fixed' },
					],
					value: 'scroll',
					changeProp: 1,
				}, {
					label: "Repeat",
					name: "imagerepeat",
					type: 'dropdown',
					cssproperties: [
						{ name: "background-repeat" }
					],
					options: [
						{ id: 'no-repeat', name: 'No-repeat' },
						{ id: 'repeat', name: 'Repeat' },
						{ id: 'repeat-x', name: 'Repeat-x' },
						{ id: 'repeat-y', name: 'Repeat-y' },
					],
					value: 'no-repeat',
					changeProp: 1,
				}, {
					label: "Size",
					name: "imagesize",
					type: 'dropdown',
					cssproperties: [
						{ name: "background-size" }
					],
					options: [
						{ id: 'auto', name: 'Auto' },
						{ id: 'cover', name: 'Cover' },
						{ id: 'contain', name: 'Contain' },
					],
					value: 'auto',
					changeProp: 1,
				}, {
					label: "Gradient",
					name: 'gradient',
					property: 'background-image',
					type: 'gradient',
				}, {
					label: "Type",
					name: "gradienttype",
					type: "toggle_radio",
					SwitchClass: true,
					options: [
						{ id: 'linear', name: 'linear', class: '' },
						{ id: 'radial', name: 'radial', class: '' },
					],
					value: 'linear',
					changeProp: 1,
				}, {
					label: "Angle",
					name: "angle",
					type: "custom_range",
					min: "0",
					max: "360",
					value: "90",
					changeProp: 1,
				}]
			}),
			init() {
				this.listenTo(this, 'change:tag', this.handleTypeChange);
				this.listenTo(this, 'change:gradient', this.handleGradientChange);
			},
			handleTypeChange() {
				if (typeof this.attributes.tag != 'undefined' && this.attributes.tag != "")
					this.set('tagName', this.attributes.tag);
			},
			handleGradientChange() {
				if (typeof this.attributes.gradient != 'undefined' && this.attributes.gradient != "") {

					var gradient = this.attributes.gradient
					var style = this.getStyle();
					style["background-image"] = gradient;
					this.setStyle(style);
				}
			},
		},
			{
				isComponent(el) {
					if (el && el.tagName && (el.tagName.toLowerCase() == 'section' || el.tagName.toLowerCase() == 'header' || el.tagName.toLowerCase() == 'footer' || (el.tagName.toLowerCase() == 'nav' && el.classList && !el.classList.contains('nav-breadcrumb')) || el.tagName.toLowerCase() == 'article')) {
						return { type: 'section' };
					}
				}
			}),
		view: defaultView.extend({
			onRender() {
				var hasClass = this.model.getClasses().find(v => v == 'vj-section')
				if (typeof hasClass == 'undefined')
					this.model.addClass('vj-section');
			},
			init() {
				this.listenTo(this.model, 'change:src', this.ChangeSrc);
			},
			ChangeSrc() {
				if (this.model.attributes.src != '') {
					if (this.model.attributes.background == "image") {
						var src = this.model.attributes.src;
						var style = this.model.getStyle();

						style["background-image"] = 'url("' + src + '")';
						this.model.setStyle(style);

						this.model.set({ "thumbnail": src });
						$(this.model.getTrait("backgroundimage").el).css('background-image', 'url(' + src + ')');
						$(this.model.getTrait("imageposition").el).parents(".gjs-trt-trait__wrp").show();
						$(this.model.getTrait("imageattachment").el).parents(".gjs-trt-trait__wrp").show();
						$(this.model.getTrait("imagerepeat").el).parents(".gjs-trt-trait__wrp").show();
						$(this.model.getTrait("imagesize").el).parents(".gjs-trt-trait__wrp").show();
					}
					else if (this.model.attributes.background == "video") {

						this.model.components().forEach(item => item.getAttributes()["data-bg-video"] == "true" ? item.remove() : null);
						this.model.append("<video src=" + this.model.attributes.src + " data-gjs-layerable='false' data-gjs-clickable='false' data-gjs-selectable='false' data-gjs-hoverable='false' data-gjs-draggable='false' data-gjs-droppable='false' data-bg-video='true' class='bg-video' autoplay loop muted></video>");

						this.model.components().forEach(item => {
							item.getAttributes()["data-bg-video"] == "true" ? item.set({ 'controls': 0 }) : null
						});

						if (this.model.attributes['videoId'] == "")
							var thumbnail = this.model.attributes['data-thumbnail'];
						else
							var thumbnail = VjDefaultPath + "thumbnail-video.jpg";

						this.model.set({ "thumbnail": thumbnail });
						$(this.model.getTrait("backgroundvideo").el).css('background-image', 'url(' + thumbnail + ')');
						$(this.model.getEl()).find('video').removeAttr('autoplay');

						this.model.addStyle({ 'overflow': 'hidden' });
					}
				}

			},

		}),
	});
}

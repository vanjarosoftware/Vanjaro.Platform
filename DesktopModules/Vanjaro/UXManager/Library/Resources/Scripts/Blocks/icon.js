export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.icon) {
        bm.add('icon', {
            label: VjLocalized.Icon,
            category: VjLocalized.Basic,
			attributes: { class: 'far fa-gem' },
			content: `
				<div class="icon-box">
					<a class="vj-icon text-primary border-primary">
						<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 576 512">
							<path d="M464 0H112c-4 0-7.8 2-10 5.4L2 152.6c-2.9 4.4-2.6 10.2.7 14.2l276 340.8c4.8 5.9 13.8 5.9 18.6 0l276-340.8c3.3-4.1 3.6-9.8.7-14.2L474.1 5.4C471.8 2 468.1 0 464 0zm-19.3 48l63.3 96h-68.4l-51.7-96h56.8zm-202.1 0h90.7l51.7 96H191l51.6-96zm-111.3 0h56.8l-51.7 96H68l63.3-96zm-43 144h51.4L208 352 88.3 192zm102.9 0h193.6L288 435.3 191.2 192zM368 352l68.2-160h51.4L368 352z"/>
						</svg>
					</a>
				</div>`,
			activate: 1
		});
	}

	let domc = editor.DomComponents;
	const defaultType = domc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;

	domc.addType('icon-box', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				'custom-name': 'Icon Box',
				droppable: false,
				traits: []
			}),
		},
			{
				isComponent(el) {
					if (el && el.classList && el.classList.contains('icon-box')) {
						return { type: 'icon-box' };
					}
				}
			}),
		view: defaultView
	});

	domc.addType('icon', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

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
							command: 'vj-copy',
						});
					}

					if (model.get('removable')) {
						tb.push({
							attributes: { class: 'fa fa-trash-o' },
							command: 'vj-delete',
						});
					}

					model.set('toolbar', tb);
				}
			},
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				droppable: false,
				editable: false,
				resizable: {
					tc: 0,
					cl: 0,
					cr: 0,
					bc: 0,
					minDim: 16,
					ratioDefault: 1,
					onMove: function (e) {
						var SelectedCol = VjEditor.getSelected();
						var width = SelectedCol.getStyle().width;
						var height = SelectedCol.getStyle().height;
						SelectedCol.components().models[0].addStyle({ 'width': width, 'height': height });
						SelectedCol.removeStyle('width');
						SelectedCol.removeStyle('height');
					},
					onEnd: function (e) {
						var SelectedCol = VjEditor.getSelected();
						SelectedCol.removeStyle('width');
						SelectedCol.removeStyle('height');
					}
				},
				traits: [
					{
						label: "Title",
						name: "title",
						type: "text",
					},
					{

						label: "Alignment",
						type: "toggle_checkbox",
						name: "alignment",
						UpdateStyles: true,
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
						],
						value: "none",
						changeProp: 1,

					}, {
						label: "Color",
						name: "color",
						type: "custom_color",
						cssproperties: [
							{ name: "color" }
						],
						options: [
							{ id: 'primary', name: 'Primary', class: 'text-primary' },
							{ id: 'secondary', name: 'Secondary', class: 'text-secondary' },
							{ id: 'tertiary', name: 'Tertiary', class: 'text-tertiary' },
							{ id: 'quaternary', name: 'Quaternary', class: 'text-quaternary' },
							{ id: 'success', name: 'Success', class: 'text-success' },
							{ id: 'info', name: 'Info', class: 'text-info' },
							{ id: 'warning', name: 'Warning', class: 'text-warning' },
							{ id: 'danger', name: 'Danger', class: 'text-danger' },
							{ id: 'light', name: 'Light', class: 'text-light' },
							{ id: 'dark', name: 'Dark', class: 'text-dark' },
						],
						value: "primary",
						changeProp: 1,
					}, {
						label: "Background",
						name: "background",
						type: "toggle_radio",
						SwitchClass: true,
						options: [
							{ id: 'empty', name: 'empty', icon: "fas fa-ban", class: '' },
							{ id: 'fill', name: 'fill', icon: "fas fa-fill", class: '' },
						],
						value: 'empty',
						changeProp: 1,
					}, {
						label: "Color",
						name: "backgroundcolor",
						type: "custom_color",
						cssproperties: [
							{ name: "background-color" }
						],
						options: [
							{ id: 'colorprimary', name: 'Primary', class: 'bg-primary' },
							{ id: 'colorsecondary', name: 'Secondary', class: 'bg-secondary' },
							{ id: 'colortertiary', name: 'Tertiary', class: 'bg-tertiary' },
							{ id: 'colorquaternary', name: 'Quaternary', class: 'bg-quaternary' },
							{ id: 'colorsuccess', name: 'Success', class: 'bg-success' },
							{ id: 'colorinfo', name: 'Info', class: 'bg-info' },
							{ id: 'colorwarning', name: 'Warning', class: 'bg-warning' },
							{ id: 'colordanger', name: 'Danger', class: 'bg-danger' },
							{ id: 'colorlight', name: 'Light', class: 'bg-light' },
							{ id: 'colordark', name: 'Dark', class: 'bg-dark' },
						],
						value: "none",
						changeProp: 1,
					}, {
						label: "Border",
						name: "frame",
						type: "toggle_radio",
						UpdateStyles: true,
						cssproperties: [
							{ name: "border-radius" }
						],
						options: [
							{ id: 'none', name: 'none', icon: "fas fa-ban" },
							{ id: 'square', name: 'square', icon: "far fa-square" },
							{ id: 'circle', name: 'circle', icon: "far fa-circle" },
						],
						value: "none",
						changeProp: 1,
					}, {
						label: "Width",
						name: "framewidth",
						type: "custom_range",
						cssproperties: [
							{ name: "border-width" }
						],
						min: "0",
						max: "100",
						value: "10",
						changeProp: 1,
					}, {
						label: "Gap",
						name: "framegap",
						type: "custom_range",
						cssproperties: [
							{ name: "padding" }
						],
						min: "0",
						max: "100",
						value: "10",
						changeProp: 1,
					}, {
						label: "Style",
						name: "framestyle",
						type: "toggle_radio",
						UpdateStyles: true,
						cssproperties: [
							{ name: "border-style" }
						],
						options: [
							{ id: 'solid', name: 'solid', image: 'border-solid' },
							{ id: 'double', name: 'double', image: 'border-double' },
							{ id: 'dotted', name: 'dotted', image: 'border-dotted' },
							{ id: 'dashed', name: 'dashed', image: 'border-dashed' },
						],
						value: "solid",
						changeProp: 1,
					}, {
						label: "Color",
						name: "framecolor",
						type: "custom_color",
						cssproperties: [
							{ name: "border-color" }
						],
						options: [
							{ id: 'frameprimary', name: 'Primary', class: 'border-primary' },
							{ id: 'framesecondary', name: 'Secondary', class: 'border-secondary' },
							{ id: 'frametertiary', name: 'Tertiary', class: 'border-tertiary' },
							{ id: 'framequaternary', name: 'Quaternary', class: 'border-quaternary' },
							{ id: 'framesuccess', name: 'Success', class: 'border-success' },
							{ id: 'frameinfo', name: 'Info', class: 'border-info' },
							{ id: 'framewarning', name: 'Warning', class: 'border-warning' },
							{ id: 'framedanger', name: 'Danger', class: 'border-danger' },
							{ id: 'framelight', name: 'Light', class: 'border-light' },
							{ id: 'framedark', name: 'Dark', class: 'border-dark' },
						],
						value: "primary",
						changeProp: 1,
					}, {
						label: " ",
						name: "href",
						type: "href",
						href: "",
						"data_href_type": "url",
					},
				]
			}),
			init() {
				this.listenTo(this, 'change:attributes:title', this.AddContent); // listen for active event
			},
			AddContent() {
				if (this.findType('text').length > 0)
					this.findType('text')[0].set('content', this.getAttributes().title);
				else
					this.append('<span class="sr-only">' + this.getAttributes().title + '</span>');
			}
		}, {
			isComponent(el) {
				if (el && el.classList && el.classList.contains('vj-icon')) {
					return { type: 'icon' };
				}
			}

		}),
		view: defaultView.extend({
			events: {
				dblclick: function () {
					this.ShowIcons();
				}
			},
			init() {
				this.listenTo(this.model.parent(), 'active', this.ShowIcons); // listen for active event
			},
			ShowIcons() {
				var target = VjEditor.getSelected() || this.model;
				window.document.vj_icon_target = target;
				var url = CurrentExtTabUrl + "&guid=85682CD1-D5FD-4611-B252-3BC1972545A0#/setting";
				OpenPopUp(null, 700, 'right', 'Select Icon', url, '', true);
			},
		}),
	});

	const svgType = domc.getType('svg');
	const svgModel = svgType.model;
	const svgView = svgType.view;

	domc.addType('svg', {
		model: svgModel.extend({
			defaults: Object.assign({}, svgModel.prototype.defaults, {
				draggable: false,
				droppable: false,
				layerable: false,
				selectable: false,
				hoverable: false,
				traits: []
			}),
		},
			{
				isComponent(el) {
					if (el.tagName === 'svg' || el.tagName === 'path') {
						return { type: 'svg' };
					}
				}
			}),
		view: svgView
	});
}

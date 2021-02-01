export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.list) {
		bm.add('list', {
			label: VjLocalized.List,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-list-ol' },
			content: `
				<div class="list-box">
					<ul class="list text-primary">
						<li class="list-item"><span class="list-text">One</span></li>
						<li class="list-item"><span class="list-text">Two</span></li>
						<li class="list-item"><span class="list-text">Three</span></li>
						<li class="list-item"><span class="list-text">Four</span></li>
						<li class="list-item"><span class="list-text">Five</span></li>
					</ul>
				</div>
			`
		});
	}

	const cmd = editor.Commands;

	cmd.add('add-list-item', ed => {
		var Selected = VjEditor.getSelected();

		if (Selected.attributes.type == 'List') {
			var List = `<li class="list-item"><span class="list-text">List Item</span></li>`;
			Selected.components().add(List);
		}
		else {
			var List = `<li class="list-item"><span class="list-text">List Item</span></li>`;
			Selected.parent().components().add(List);
		}

	});

	let domc = editor.DomComponents;
	const defaultType = domc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;

	domc.addType('list-box', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				'custom-name': 'List Box',
				droppable: false,
				traits: []
			}),
		},
			{
				isComponent(el) {
					if (el && el.classList && el.classList.contains('list-box')) {
						return { type: 'list-box' };
					}
				}
			}),
		view: defaultView
	});

	domc.addType('list', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
                        attributes: { class: 'fa fa-plus', title: VjLocalized.AddListItem },
						command: 'add-list-item',
					});

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
				droppable: '.list-item, .list-text',
				traits: [{
					label: 'List Type',
					name: 'list_type',
					type: 'toggle_radio',
					SwitchClass: true,
					options: [
						{ id: 'ul', name: 'ul', class: 'unordered-list', icon: "fas fa-list-ul" },
						{ id: 'ol', name: 'ol', class: 'ordered-list', icon: "fas fa-list-ol" },
					],
					default: 'ul',
					changeProp: 1,
				}, {
					label: 'List Style',
					name: 'ul_list_style',
					type: 'toggle_radio',
					UpdateStyles: true,
					cssproperties: [{ name: "list-style-type" }],
					options: [
						{ id: 'none', name: 'none', icon: 'fas fa-ban' },
						{ id: 'circle', name: 'circle', icon: 'far fa-circle' },
						{ id: 'disc', name: 'disc', icon: 'fas fa-circle' },
						{ id: 'square', name: 'square', icon: 'fas fa-square' },
					],
					default: 'disc',
					changeProp: 1,
				}, {
					label: 'List Style',
					name: 'ol_list_style',
					type: 'toggle_radio',
					UpdateStyles: true,
					cssproperties: [{ name: "list-style-type" }],
					options: [
						{ id: 'decimal', name: '1' },
						{ id: 'lower-alpha', name: 'a' },
						{ id: 'upper-alpha', name: 'A' },
						{ id: 'lower-roman', name: 'i' },
						{ id: 'upper-roman', name: 'I' },
					],
					default: 'decimal',
					changeProp: 1,
				}, {
					label: 'Start',
					name: 'start',
					type: "custom_number",
					min: "1",
					max: "100",
					default: "1",
				}, {
					label: "Alignment",
					type: "toggle_checkbox",
					name: "alignment",
					UpdateStyles: true,
					options: [
						{ id: 'left', name: 'left', image: 'align-left' },
						{ id: 'center', name: 'center', image: 'align-center' },
						{ id: 'right', name: 'right', image: 'align-right' },
					],
					default: "none",
					changeProp: 1,
				}, {
					label: "Font Size",
					name: "fontsize",
					type: "custom_range",
					cssproperties: [{ name: "font-size" }],
					units: [
						{ name: 'px', min: 10, max: 100, step: 1, value: 16 },
						{ name: 'vw', min: 0.5, max: 10, step: 0.1, value: 1 },
					],
					unit: "px",
					changeProp: 1,
				}, {
					label: "Color",
					name: "color",
					type: 'custom_color',
					cssproperties: [{ name: "color" }],
					options: [
						{ id: 'primary', color: 'bg-primary', name: 'Primary', class: 'text-primary' },
						{ id: 'secondary', color: 'bg-secondary', name: 'Secondary', class: 'text-secondary' },
						{ id: 'tertiary', color: 'bg-tertiary', name: 'Tertiary', class: 'text-tertiary' },
						{ id: 'quaternary', color: 'bg-quaternary', name: 'Quaternary', class: 'text-quaternary' },
						{ id: 'success', color: 'bg-success', name: 'Success', class: 'text-success' },
						{ id: 'info', color: 'bg-info', name: 'Info', class: 'text-info' },
						{ id: 'warning', color: 'bg-warning', name: 'Warning', class: 'text-warning' },
						{ id: 'danger', color: 'bg-danger', name: 'Danger', class: 'text-danger' },
						{ id: 'light', color: 'bg-light', name: 'Light', class: 'text-light' },
						{ id: 'dark', color: 'bg-dark', name: 'Dark', class: 'text-dark' }
					],
					default: 'primary',
					changeProp: 1,
				}]
			}),
			init() {
				this.listenTo(this, 'change:list_type', this.handleTypeChange);
			},
			handleTypeChange() {
				if (typeof this.attributes.list_type != 'undefined' && this.attributes.list_type != "") {
					this.attributes.tagName = this.attributes.list_type;
                    this.view.reset();
                    VjEditor.runCommand("save");
				}
			}
		}, {
			isComponent(el) {
				if (el && el.classList && el.classList.contains('list')) {
					return { type: 'list' };
				}
			}
		}),
		view: defaultView
	});

	domc.addType('list-item', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
                        attributes: { class: 'fa fa-plus', title: VjLocalized.AddListItem },
						command: 'add-list-item',
					});

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
				'custom-name': 'List Item',
				draggable: '.list',
				droppable: true,
				editable: true,
				classes: ['list-item'],
				tagName: 'li',
				traits: []
			}),
		}, {
			isComponent(el) {
				if (el && el.classList && el.classList.contains('list-item')) {
					return { type: 'list-item' };
				}
			}
		}),
		view: defaultView
	});

	const textType = domc.getType('text');
	const textModel = textType.model;
	const textView = textType.view;

	domc.addType('list-text', {
		model: textModel.extend({
			defaults: Object.assign({}, textModel.prototype.defaults, {
				'custom-name': 'List Text',
				draggable: '.list',
				droppable: false,
				layerable: false,
				selectable: false,
				hoverable: false,
				traits: [],
			}),
		},
			{
				isComponent(el) {
					if (el && el.classList && el.classList.contains('list-text')) {
						return { type: 'list-text' };
					}
				}
			}),
		view: textView
	});
}

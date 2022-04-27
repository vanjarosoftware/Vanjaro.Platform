export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.text) {
		bm.add('text', {
			label: VjLocalized.Text,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-font' },
			content: {
				type: 'text',
				content: 'Vivamus sagittis lacus vel augue laoreet rutrum faucibus dolor auctor. Duis mollis, est non commodo luctus.',
			}
		});
	}

	let domc = editor.DomComponents;
	const textType = domc.getType('text');
	const textModel = textType.model;
	const textView = textType.view;

	domc.addType('text', {
		model: textModel.extend({
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
			defaults: Object.assign({}, textModel.prototype.defaults, {
				'custom-name': 'Text',
				droppable: false,
				classes: ['vj-text', 'text-dark', 'paragraph-style-1'],
				text: true,
				traits: [
					{
						label: 'Alignment',
						type: 'toggle_checkbox',
						name: 'alignment',
						UpdateStyles: true,
						cssproperties: [{ name: "text-align" }],
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
							{ id: 'justify', name: 'justify', image: 'align-justify' },
						],
						default: 'left',
						changeProp: 1,
					}, {
						label: "Font Size",
						name: "fontsize",
						type: "custom_range",
						cssproperties: [{ name: "font-size" }],
						units: [
							{ name: 'px', min: 10, max: 100, step: 1, value: 16 },
							{ name: '%', min: 10, max: 100, step: 1, value: 100 },
							{ name: 'em', min: 0.5, max: 10, step: 0.1, value: 1 },
							{ name: 'rem', min: 0.5, max: 10, step: 0.1, value: 1 },
							{ name: 'vw', min: 0.5, max: 10, step: 0.1, value: 1 },
							{ name: 'vh', min: 0.5, max: 10, step: 0.1, value: 1.5 },
						],
						unit: "px",
						changeProp: 1
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
						default: 'dark',
						changeProp: 1,
					}, {
						label: 'Styles',
						name: 'styles',
						type: 'preset_radio',
						options: [
							{ id: 'paragraph-style-1', name: 'Style 1', class: 'paragraph-style-1', DisplayName: 'A' },
							{ id: 'paragraph-style-2', name: 'Style 2', class: 'paragraph-style-2', DisplayName: 'B' },
							{ id: 'paragraph-style-3', name: 'Style 3', class: 'paragraph-style-3', DisplayName: 'C' },
							{ id: 'paragraph-style-4', name: 'Style 4', class: 'paragraph-style-4', DisplayName: 'D' },
							{ id: 'paragraph-style-5', name: 'Style 5', class: 'paragraph-style-5', DisplayName: 'E' },
							{ id: 'paragraph-style-6', name: 'Style 6', class: 'paragraph-style-6', DisplayName: 'F' },
							{ id: 'paragraph-style-7', name: 'Style 7', class: 'paragraph-style-7', DisplayName: 'G' },
							{ id: 'paragraph-style-8', name: 'Style 8', class: 'paragraph-style-8', DisplayName: 'H' },
							{ id: 'paragraph-style-9', name: 'Style 9', class: 'paragraph-style-9', DisplayName: 'I' },
							{ id: 'paragraph-style-10', name: 'Style 10', class: 'paragraph-style-10', DisplayName: 'J' },
						],
						default: 'Style 1',
						changeProp: 1,
					}
				]
			}),
		}, {
			isComponent(el) {
				if (el && (el.classList && el.classList.contains('vj-text'))) {
					return { type: 'text' };
				}
			}
		}),
		view: textView.extend({
			init() {
				if (this.model.parent().attributes.type == 'text' || this.model.parent().attributes.type == 'heading' || this.model.parent().attributes.type == 'list-text')
					this.model.removeClass(this.getClasses());
			},
			onRender() {

				var model = this.model;
				var trait = model.getTrait('styles');

				if (trait) {

					var Style = model.attributes.styles || 'Style 1';
					var Option = trait.attributes.options.find(x => x.name === Style);
					var DisplayName = "";

					if (typeof Option != "undefined")
						DisplayName = Option.DisplayName;

					if (model.attributes['custom-name'].indexOf(' - ' + DisplayName) == -1)
						model.set('custom-name', model.getName() + ' - ' + DisplayName);
				}
			}
		})
	});

	domc.addType('text-inner', {
		model: textModel.extend({
			defaults: Object.assign({}, textModel.prototype.defaults, {
				droppable: false,
				'custom-name': 'Text Inner',
                classes: ['vj-text', 'text-dark'],
                text: true,
				tagName: 'p',
				traits: [
					{
						label: 'Alignment',
						type: 'toggle_checkbox',
						name: 'alignment',
						UpdateStyles: true,
						cssproperties: [{ name: "text-align" }],
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
							{ id: 'justify', name: 'justify', image: 'align-justify' },
						],
						default: 'left',
					}, {
						label: "Font Size",
						name: "fontsize",
						type: "custom_range",
						cssproperties: [{ name: "font-size" }],
						units: [
							{ name: 'px', min: 10, max: 100, step: 1, value: 16 },
							{ name: '%', min: 10, max: 100, step: 1, value: 100 },
							{ name: 'em', min: 0.5, max: 10, step: 0.1, value: 1 },
							{ name: 'rem', min: 0.5, max: 10, step: 0.1, value: 1 },
							{ name: 'vw', min: 0.5, max: 10, step: 0.1, value: 1 },
							{ name: 'vh', min: 0.5, max: 10, step: 0.1, value: 1 },
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
					}, {
						label: 'Styles',
						name: 'styles',
						type: 'preset_radio',
						options: [
							{ id: 'paragraph-style-1', name: 'Style 1', class: 'paragraph-style-1', DisplayName: 'A' },
							{ id: 'paragraph-style-2', name: 'Style 2', class: 'paragraph-style-2', DisplayName: 'B' },
							{ id: 'paragraph-style-3', name: 'Style 3', class: 'paragraph-style-3', DisplayName: 'C' },
							{ id: 'paragraph-style-4', name: 'Style 4', class: 'paragraph-style-4', DisplayName: 'D' },
							{ id: 'paragraph-style-5', name: 'Style 5', class: 'paragraph-style-5', DisplayName: 'E' },
							{ id: 'paragraph-style-6', name: 'Style 6', class: 'paragraph-style-6', DisplayName: 'F' },
							{ id: 'paragraph-style-7', name: 'Style 7', class: 'paragraph-style-7', DisplayName: 'G' },
							{ id: 'paragraph-style-8', name: 'Style 8', class: 'paragraph-style-8', DisplayName: 'H' },
							{ id: 'paragraph-style-9', name: 'Style 9', class: 'paragraph-style-9', DisplayName: 'I' },
							{ id: 'paragraph-style-10', name: 'Style 10', class: 'paragraph-style-10', DisplayName: 'J' },
						],
						default: 'Style 1',
					}
				]
			}),
		}, {
			isComponent(el) {
				if (el && el.classList && el.classList.contains('text-inner')) {
					return { type: 'text-inner' };
				}
			}
		}),
		view: textView.extend({
			onRender() {

				var model = this.model;
				var trait = model.getTrait('styles');

				if (trait) {

					var Style = model.attributes.styles || 'Style 1';
					var Option = trait.attributes.options.find(x => x.name === Style);
					var DisplayName = "";

					if (typeof Option != "undefined")
						DisplayName = Option.DisplayName;

					if (model.attributes['custom-name'].indexOf(' - ' + DisplayName) == -1)
						model.set('custom-name', 'Text Inner - ' + DisplayName);
				}
			}
		})
	});
}

export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.heading) {

		bm.add('heading', {
			label: VjLocalized.Heading,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-heading' },
			content: {
				type: 'heading',
				content: 'Heading',
			}
		});
	}

	let domc = editor.DomComponents;
	const textType = domc.getType('text');
	const textModel = textType.model;
	const textView = textType.view;

	domc.addType('heading', {
		model: textModel.extend({
			defaults: Object.assign({}, textModel.prototype.defaults, {
				'custom-name': 'Heading',
				droppable: false,
				tagName: 'h1',
				classes: ['vj-heading', 'text-primary', 'head-style-1'],
				text: true,
				traits: [
					{
						label: 'Importance',
						name: 'importance',
						type: 'toggle_radio',
						UpdateStyles: true,
						options: [
							{ id: 'H1', name: 'H1' },
							{ id: 'H2', name: 'H2' },
							{ id: 'H3', name: 'H3' },
							{ id: 'H4', name: 'H4' },
							{ id: 'H5', name: 'H5' },
							{ id: 'H6', name: 'H6' },
						],
						default: 'H1',
						changeProp: 1,
					}, {
						label: 'Alignment',
						name: 'alignment',
						type: 'toggle_checkbox',
						UpdateStyles: true,
						cssproperties: [{ name: "text-align" }],
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
						],
						default: 'left',
						changeProp: 1,
					}, {
						label: "Font Size",
						name: "fontsize",
						type: "custom_range",
						cssproperties: [{ name: "font-size" }],
						units: [
							{ name: 'px', min: 10, max: 100, step: 1, value: 40 },
							{ name: '%', min: 10, max: 100, step: 1, value: 100 },
							{ name: 'em', min: 0.5, max: 10, step: 0.1, value: 2 },
							{ name: 'rem', min: 0.5, max: 10, step: 0.1, value: 2 },
							{ name: 'vw', min: 0.5, max: 10, step: 0.1, value: 2 },
							{ name: 'vh', min: 0.5, max: 10, step: 0.1, value: 3 },
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
					},
					{
						label: 'Styles',
						name: 'styles',
						type: 'preset_radio',
						options: [
							{ id: 'head-style-1', name: 'Style 1', class: 'head-style-1', DisplayName: 'A' },
							{ id: 'head-style-2', name: 'Style 2', class: 'head-style-2', DisplayName: 'B' },
							{ id: 'head-style-3', name: 'Style 3', class: 'head-style-3', DisplayName: 'C' },
							{ id: 'head-style-4', name: 'Style 4', class: 'head-style-4', DisplayName: 'D' },
							{ id: 'head-style-5', name: 'Style 5', class: 'head-style-5', DisplayName: 'E' },
							{ id: 'head-style-6', name: 'Style 6', class: 'head-style-6', DisplayName: 'F' },
							{ id: 'head-style-7', name: 'Style 7', class: 'head-style-7', DisplayName: 'G' },
							{ id: 'head-style-8', name: 'Style 8', class: 'head-style-8', DisplayName: 'H' },
							{ id: 'head-style-9', name: 'Style 9', class: 'head-style-9', DisplayName: 'I' },
							{ id: 'head-style-10', name: 'Style 10', class: 'head-style-10', DisplayName: 'J' },
						],
						default: 'Style 1',
						changeProp: 1,
					}
				]
			}),
			init() {
				this.listenTo(this, 'change:importance', this.handleTypeChange);
			},
			handleTypeChange() {
				if (typeof this.attributes.importance != 'undefined' && this.attributes.importance != "") {
					this.attributes.tagName = this.attributes.importance;
					this.view.reset();
					this.removeStyle("font-size");
					this.getTrait('fontsize').setTargetValue($(this.getEl()).css('font-size').replace(/[^-\d\.]/g, ''));
					this.getTrait('fontsize').view.render();
					VjEditor.runCommand("save");
				}
			},
		}, {
			isComponent(el) {
				if (el && el.tagName && el.classList && el.classList.contains('vj-heading')) {
					return { type: 'heading' };
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
						model.set('custom-name', model.getName() + ' - ' + DisplayName);
				}
			}
		})
	});
}

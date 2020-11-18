export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;
  
	if (c.blocks.divider) {
        bm.add('divider').set({
            label: VjLocalized.Divider,
            attributes: { class: 'fas fa-divide' },
            category: VjLocalized.Basic,
            content: {
				type: 'divider',
				content: '<div></div>'
			}
		});
	}

	let domc = editor.DomComponents;
	const defaultType = domc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;

	domc.addType('divider', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				droppable: false,
                //stylable: ['width', 'height', 'margin', 'background-color'],
                classes: ['vj-divider', 'border-primary'],
				traits: [
					{
						label: "Weight",
						name: "weight",
						type: "custom_range",
						cssproperties: [
							{ name: "border-top-width" }
						],
						min: "1",
						max: "100",
						value: "10",
                        changeProp: 1,
                    }, {
						label: "Gap",
						name: "gap",
						type: "custom_range",
						cssproperties: [
							{ name: "margin-top" },
							{ name: "margin-bottom" }
						],
						min: "0",
						max: "50",
						value: "15",
                        changeProp: 1,
                    }, {
						label: "Width",
						name: "width",
						type: "custom_range",
						cssproperties: [
							{ name: "width" }
						],
						unitOptions: true,
						units: [
                            { name: 'px' },
                            { name: '%' }
						],
						unit: "%",
						min: "1",
						max: "100",
						value: "100",
                        changeProp: 1,
                    }, {
						label: "Design",
						name: "design",
						type: "toggle_radio",
						UpdateStyles: true,
						cssproperties: [
							{ name: "border-top-style" }
						],
						options: [
						  { id: 'solid', name: 'solid', image: 'border-solid' },
						  { id: 'double', name: 'double', image: 'border-double' },
						  { id: 'dotted', name: 'dotted', image: 'border-dotted' },
						  { id: 'dashed', name: 'dashed', image: 'border-dashed' },
						],
                        value: 'solid',
                        changeProp: 1,
                    }, {
						label: 'Alignment',
						name: 'alignment',
						type: 'toggle_radio',
						UpdateStyles: true,
						options: [ 
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
						],
                        value: 'left',
                        changeProp: 1,
                    }, {
						label: "Color",
						name: "color",
						type: 'custom_color',
						cssproperties: [
							{ name: "border-color" }
						],
						options: [ 
                            { id: 'primary', color: 'bg-primary', name: 'Primary', class: 'border-primary' },
                            { id: 'secondary', color: 'bg-secondary', name: 'Secondary', class: 'border-secondary' },
							{ id: 'tertiary', color: 'bg-tertiary', name: 'Tertiary', class: 'border-tertiary' },
							{ id: 'quaternary', color: 'bg-quaternary', name: 'Quaternary', class: 'border-quaternary' },
							{ id: 'success', color: 'bg-success', name: 'Success', class: 'border-success' },
							{ id: 'info', color: 'bg-info', name: 'Info', class: 'border-info' },
							{ id: 'warning', color: 'bg-warning', name: 'Warning', class: 'border-warning' },
							{ id: 'danger', color: 'bg-danger', name: 'Danger', class: 'border-danger' },
							{ id: 'light', color: 'bg-light', name: 'Light', class: 'border-light' },
							{ id: 'dark', color: 'bg-dark', name: 'Dark', class: 'border-dark' }
						],
                        value: 'primary',
                        changeProp: 1,
					}
				]
			}),
        }, {
			isComponent(el) {
				if (el && el.classList && el.classList.contains('vj-divider')) {
					return { type: 'divider' };
				}
			},
		}),
		view: defaultView
	});
}

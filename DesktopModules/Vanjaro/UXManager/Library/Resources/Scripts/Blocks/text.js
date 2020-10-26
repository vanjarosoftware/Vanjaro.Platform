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
            defaults: Object.assign({}, textModel.prototype.defaults, {
                droppable: false,
                classes: ['vj-text', 'text-primary', 'paragraph-style-1'],
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
                        value: 'left',
                        changeProp: 1,
                    }, {
                        label: "Font Size",
                        name: "fontsize",
                        type: "custom_range",
                        cssproperties: [
                            { name: "font-size" }
                        ],
                        unitOptions: true,
                        units: [
                            { name: 'px' },
                            { name: '%' },
                            { name: 'em' }
                        ],
                        unit: "px",
                        min: "10",
                        max: "100",
                        value: "16",
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
                        value: 'primary',
                        changeProp: 1,
                    }, {
                        label: 'Styles',
                        name: 'styles',
                        type: 'preset_radio',
                        options: [
                            { id: 'paragraph-style-1', name: 'Style 1', class: 'paragraph-style-1' },
                            { id: 'paragraph-style-2', name: 'Style 2', class: 'paragraph-style-2' },
                            { id: 'paragraph-style-3', name: 'Style 3', class: 'paragraph-style-3' },
                            { id: 'paragraph-style-4', name: 'Style 4', class: 'paragraph-style-4' },
                            { id: 'paragraph-style-5', name: 'Style 5', class: 'paragraph-style-5' },
                            { id: 'paragraph-style-6', name: 'Style 6', class: 'paragraph-style-6' },
                            { id: 'paragraph-style-7', name: 'Style 7', class: 'paragraph-style-7' },
                            { id: 'paragraph-style-8', name: 'Style 8', class: 'paragraph-style-8' },
                            { id: 'paragraph-style-9', name: 'Style 9', class: 'paragraph-style-9' },
                            { id: 'paragraph-style-10', name: 'Style 10', class: 'paragraph-style-10' },
                        ],
                        value: 'Normal',
                        changeProp: 1,
                    }
                ]
            }),
        },
            {
				isComponent(el) {
					if (el && ((el.classList && el.classList.contains('vj-text')) || (el.tagName && el.tagName.toLowerCase() == 'p'))) {
                        return { type: 'text' };
                    }
                }
            }),
        view: textView.extend({
            init() {
                if (this.model.parent().attributes.type == 'text' || this.model.parent().attributes.type == 'heading')
                    this.model.removeClass(this.getClasses());
			},
			onRender() {
				var hasClass = this.model.getClasses().find(v => v == 'vj-text' || v == 'text-primary')
				if (typeof hasClass == 'undefined' && this.model.tagName == 'p')
					this.model.addClass('vj-text text-primary');
			}
        })
	});

	domc.addType('text-inner', {
		model: textModel.extend({
			defaults: Object.assign({}, textModel.prototype.defaults, {
				droppable: false,
				'custom-name': 'Text Inner',
				classes: ['vj-text', 'text-primary'],
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
                    	value: 'left',
                    }, {
                    	label: "Font Size",
                    	name: "fontsize",
                    	type: "custom_range",
                    	cssproperties: [
                            { name: "font-size" }
                    	],
                    	unitOptions: true,
                    	units: [
                            { name: 'px' },
                            { name: '%' },
                            { name: 'em' }
                    	],
                    	unit: "px",
                    	min: "1",
                    	max: "100",
                    	value: "16",
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
                    	value: 'primary',
                    }, {
                    	label: 'Styles',
                    	name: 'styles',
                    	type: 'preset_radio',
                    	options: [
                            { id: 'normal', name: 'Normal', class: 'normal' },
                            { id: 'paragraph-style-1', name: 'Style 1', class: 'paragraph-style-1' },
                            { id: 'paragraph-style-2', name: 'Style 2', class: 'paragraph-style-2' },
                            { id: 'paragraph-style-3', name: 'Style 3', class: 'paragraph-style-3' },
                            { id: 'paragraph-style-4', name: 'Style 4', class: 'paragraph-style-4' },
                            { id: 'paragraph-style-5', name: 'Style 5', class: 'paragraph-style-5' },
                            { id: 'paragraph-style-6', name: 'Style 6', class: 'paragraph-style-6' },
                            { id: 'paragraph-style-7', name: 'Style 7', class: 'paragraph-style-7' },
                            { id: 'paragraph-style-8', name: 'Style 8', class: 'paragraph-style-8' },
                            { id: 'paragraph-style-9', name: 'Style 9', class: 'paragraph-style-9' },
                            { id: 'paragraph-style-10', name: 'Style 10', class: 'paragraph-style-10' },
                    	],
                    	value: 'Normal',
                    }
				]
			}),
		},
            {
            	isComponent(el) {
            		if (el && el.classList && el.classList.contains('text-inner')) {
            			return { type: 'text-inner' };
            		}
            	}
            }),
		view: textView
	});
}

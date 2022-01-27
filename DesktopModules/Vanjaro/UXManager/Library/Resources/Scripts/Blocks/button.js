export default (editor, config = {}) => {
    const c = config;
    let bm = editor.BlockManager;

    if (c.blocks.button) {
        bm.add('button', {
            label: `
				<svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
					<path d="M22,9 C22,8.4 21.5,8 20.75,8 L3.25,8 C2.5,8 2,8.4 2,9 L2,15 C2,15.6 2.5,16 3.25,16 L20.75,16 C21.5,16 22,15.6 22,15 L22,9 Z M21,15 L3,15 L3,9 L21,9 L21,15 Z" fill-rule="nonzero"></path>
						<rect x="4" y="11.5" width="16" height="1"></rect>
				</svg>
				<div class="gjs-block-label">`+ VjLocalized.Button + `</div>
			`,
            category: VjLocalized.Basic,
            content: '<div class="button-box"><a role="button" href="#" class="btn btn-primary button-style-1">Button</a></div>',
        });
    }

    let domc = editor.DomComponents;
    const defaultType = domc.getType('default');
    const defaultModel = defaultType.model;
    const defaultView = defaultType.view;

    domc.addType('button-box', {
        model: defaultModel.extend({
            defaults: Object.assign({}, defaultModel.prototype.defaults, {
                'custom-name': 'Button Box',
                droppable: false,
                selectable: false,
                highlightable: false,
                hoverable: false,
                traits: []
            }),
        }, {
            isComponent(el) {
                if (el && el.classList && el.classList.contains('button-box')) {
                    return { type: 'button-box' };
                }
            }
        }),
        view: defaultView
    });

    domc.addType('button', {
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
                'custom-name': 'Button',
                droppable: '[data-gjs-type=icon-box], [data-gjs-type=icon]',
                classes: ['btn', 'btn-primary', 'button-style-1'],
                text: true,
                resizable: {
                    tl: 0, // Top left
                    tc: 0, // Top center
                    tr: 0, // Top right
                    cl: 1, // Center left
                    cr: 1, // Center right
                    bl: 0, // Bottom left
                    bc: 0, // Bottom center
                    br: 0, // Bottom right
                },
                traits: [
                    {
                        label: "Style",
                        name: "stylee",
                        type: 'toggle_radio',
                        SwitchClass: true,
                        options: [
                            { id: 'fill', name: 'fill', class: '' },
                            { id: 'outline', name: 'outline', class: 'outline-' },
                        ],
                        default: 'fill',
                        changeProp: 1,
                    }, {
                        label: "Size",
                        name: "size",
                        type: 'toggle_radio',
                        SwitchClass: true,
                        options: [
                            { id: 'sm', name: 'small', class: 'btn-sm' },
                            { id: 'md', name: 'medium', class: '' },
                            { id: 'lg', name: 'large', class: 'btn-lg' },
                        ],
                        default: 'md',
                        changeProp: 1,
                    }, {
                        label: 'Alignment',
                        name: 'alignment',
                        type: 'toggle_checkbox',
                        UpdateStyles: true,
                        selector: '[data-gjs-type="button-box"]',
                        closest: true,
                        cssproperties: [{ name: "text-align" }],
                        options: [
                            { id: 'left', name: 'left', image: 'align-left' },
                            { id: 'center', name: 'center', image: 'align-center' },
                            { id: 'right', name: 'right', image: 'align-right' },
                            { id: 'justify', name: 'justify', image: 'align-justify' },
                        ],
                        default: 'none',
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
                        changeProp: 1,
                    }, {
                        label: "Color",
                        name: "color",
                        type: 'custom_color',
                        cssproperties: [
                            { name: "background-color" },
                            { name: "border-color" }
                        ],
                        options: [
                            { id: 'primary', color: 'bg-primary', name: 'Primary', class: 'primary' },
                            { id: 'secondary', color: 'bg-secondary', name: 'Secondary', class: 'secondary' },
                            { id: 'tertiary', color: 'bg-tertiary', name: 'Tertiary', class: 'tertiary' },
                            { id: 'quaternary', color: 'bg-quaternary', name: 'Quaternary', class: 'quaternary' },
                            { id: 'success', color: 'bg-success', name: 'Success', class: 'success' },
                            { id: 'info', color: 'bg-info', name: 'Info', class: 'info' },
                            { id: 'warning', color: 'bg-warning', name: 'Warning', class: 'warning' },
                            { id: 'danger', color: 'bg-danger', name: 'Danger', class: 'danger' },
                            { id: 'light', color: 'bg-light', name: 'Light', class: 'light' },
                            { id: 'dark', color: 'bg-dark', name: 'Dark', class: 'dark' }
                        ],
                        default: 'primary',
                        changeProp: 1,
                    }, {
                        label: " ",
                        name: "href",
                        type: "href",
                        href: "#",
                        "data_href_type": "url",
                    }, {
                        label: 'Styles',
                        name: 'styles',
                        type: 'preset_radio',
                        options: [
                            { id: 'button-style-1', name: 'Style 1', class: 'button-style-1', DisplayName: 'A' },
                            { id: 'button-style-2', name: 'Style 2', class: 'button-style-2', DisplayName: 'B' },
                            { id: 'button-style-3', name: 'Style 3', class: 'button-style-3', DisplayName: 'C' },
                            { id: 'button-style-4', name: 'Style 4', class: 'button-style-4', DisplayName: 'D' },
                            { id: 'button-style-5', name: 'Style 5', class: 'button-style-5', DisplayName: 'E' },
                            { id: 'button-style-6', name: 'Style 6', class: 'button-style-6', DisplayName: 'F' },
                            { id: 'button-style-7', name: 'Style 7', class: 'button-style-7', DisplayName: 'G' },
                            { id: 'button-style-8', name: 'Style 8', class: 'button-style-8', DisplayName: 'H' },
                            { id: 'button-style-9', name: 'Style 9', class: 'button-style-9', DisplayName: 'I' },
                            { id: 'button-style-10', name: 'Style 10', class: 'button-style-10', DisplayName: 'J' },
                        ],
                        default: 'Style 1',
                        changeProp: 1,
                    }
                ]
            }),
            init() {
                this.listenTo(this, 'change:size', this.handleSizeChange);
            },
            handleSizeChange() {
                this.removeStyle("font-size");
                this.getTrait('fontsize').setTargetValue($(this.getEl()).css('font-size').replace(/[^-\d\.]/g, ''));
                this.getTrait('fontsize').view.render();
            },
        }, {
            isComponent(el) {
                if (el && el.classList && el.classList.contains('btn')) {
                    return { type: 'button' };
                }
            }
        }),
        view: defaultView.extend({
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

                setTimeout(function () {

                    if (!model.find('.button-text').length) {

                        var content = $(model.getEl()).text();
                        model.attributes.components.models[0].replaceWith('<span class="button-text">' + content + '</span>');
                    }
                });
            },
            events: {
                dblclick: function () {
                    return false;
                }
            }
        }),
    });

    const textType = domc.getType('text');
    const textModel = textType.model;
    const textView = textType.view;

    domc.addType('button-text', {
        model: textModel.extend({
            defaults: Object.assign({}, textModel.prototype.defaults, {
                copyable: false,
                draggable: false,
                droppable: false,
                hoverable: false,
                removable: false,
                selectable: false,
                stylable: false,
            }),
        }, {
            isComponent(el) {
                if (el && (el.classList && el.classList.contains('button-text'))) {
                    return { type: 'button-text' };
                }
            }
        }),
        view: textView
    });
}
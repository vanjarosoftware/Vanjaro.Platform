export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.link) {
		bm.add('link', {
			label: VjLocalized.Link,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-link' },
			content: `
            <a class="vj-link"></a>`
		});
	}

	let domc = editor.DomComponents;
	const linkType = domc.getType('link');
	const linkModel = linkType.model;
	const linkView = linkType.view;

	domc.addType('link', {
        model: linkModel.extend({
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
			defaults: Object.assign({}, linkModel.prototype.defaults, {
				droppable: '[data-gjs-type=section], [data-gjs-type=grid], [data-gjs-type=heading], [data-gjs-type=text], [data-gjs-type=icon-box], [data-gjs-type=list-box], [data-gjs-type=list], [data-gjs-type=spacer], [data-gjs-type=image-box], [data-gjs-type=divider], .image-link, .icon-link',
				tagName: 'a',
				'custom-name': 'Link Group',
				attributes: { href: '#' },
				text: true,
				traits: [
					{
						label: "Alignment",
						type: "toggle_checkbox",
						name: "alignment",
						UpdateStyles: true,
						cssproperties: [{ name: "text-align" }],
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
						],
						default: "left",
						changeProp: 1,
					}, {
						label: " ",
						name: "href",
						type: "href",
						href: "#",
						"data_href_type": "url",
						changeProp: 1,
					}
				]
			}),
		}, {
			isComponent(el) {
                if (el && el.tagName && el.tagName.toLowerCase() == 'a' && el.classList && el.classList.contains('vj-link')) {
					return { type: 'link' };
				}
			}
		}),
		view: linkView.extend({
			onRender() {

				var model = this.model;

				if (typeof model.getEl().classList != "undefined" && model.getEl().classList.contains('vj-link') && !model.components().length)
					$(this.el).attr("data-empty", "true");
            },
            events: {
                dblclick: function () {
                    return false;
                }
            }
		}),
    });

    domc.addType('link-text', {
        model: linkModel.extend({
            defaults: Object.assign({}, linkModel.prototype.defaults, {
                droppable: false,
                tagName: 'a',
                'custom-name': 'Link',
                attributes: { href: '#' },
                text: true,
                traits: [
                    {
                        label: "Alignment",
                        type: "toggle_checkbox",
                        name: "alignment",
                        UpdateStyles: true,
                        cssproperties: [{ name: "text-align" }],
                        options: [
                            { id: 'left', name: 'left', image: 'align-left' },
                            { id: 'center', name: 'center', image: 'align-center' },
                            { id: 'right', name: 'right', image: 'align-right' },
                        ],
                        default: "left",
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
                        default: 'none',
                        changeProp: 1,
                    }, {
                        label: " ",
                        name: "href",
                        type: "href",
                        href: "#",
                        "data_href_type": "url",
                        changeProp: 1,
                    }
                ]
            }),
        }, {
                isComponent(el) {
                if (el && el.tagName && el.tagName.toLowerCase() == 'a' && (!el.classList || (el.classList && !el.classList.contains('vj-link') && !el.classList.contains('btn')))) {
                        return { type: 'link-text' };
                    }
                }
            }),
        view: linkView
    });
}

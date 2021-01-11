export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.image) {
		bm.add('image', {
			label: VjLocalized.Image,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-image' },
			content: `
				<div class="image-box">
					<picture class="picture-box" data-gjs-clickable="false" data-gjs-selectable="false" data-gjs-hoverable="false" data-gjs-draggable="false" data-gjs-droppable="false">
						<img loading="lazy" class="img-fluid" src="`+ VjDefaultPath + `image.png" />
					</picture>
				</div>`,
			activate: 1
		});
	}

	let domc = editor.DomComponents;
	const defaultType = domc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;

	domc.addType('image-box', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				'custom-name': 'Image Box',
				droppable: false,
				traits: []
			}),
		},
			{
				isComponent(el) {
					if (el && el.classList && el.classList.contains('image-box')) {
						return { type: 'image-box' };
					}
				}
			}),
		view: defaultView
	});

	domc.addType('picture-box', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				'custom-name': 'Picture',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				selectable: false,
				editable: false,
				hoverable: false,
			}),
		}, {
				isComponent(el) {
					if (el && el.classList && el.classList.contains('picture-box')) {
						return { type: 'picture-box' };
					}
				}
			}),
		view: defaultView
	});

	domc.addType('source', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				'custom-name': 'Source',
				draggable: false,
				droppable: false,
				stylable: false,
				highlightable: false,
				layerable: false,
				selectable: false,
				hoverable: false,
			}),
		}, {
				isComponent(el) {
					if (el && el.classList && el.classList.contains('source')) {
						return { type: 'source' };
					}
				}
			}),
		view: defaultView
	});

	const imageType = domc.getType('image');
	const imageModel = imageType.model;
	const imageView = imageType.view;

	domc.addType('image', {
		model: imageModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
                        attributes: { class: 'fa fa-pencil', title: VjLocalized.EditImage },
						command: 'custom-tui-image-editor',
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
			defaults: Object.assign({}, imageModel.prototype.defaults, {
				droppable: false,
				resizable: {
					tc: 0,
					cl: 0,
					cr: 0,
					bc: 0,
					ratioDefault: 1,
					onEnd: function (e) {
						var SelectedCol = VjEditor.getSelected();

						SelectedCol.removeStyle('max-width');

						if (!this.keys.shift)
							SelectedCol.removeStyle('height');

						var width = SelectedCol.getStyle()['width'];
						var size = '(min-width:' + width + ') ' + width + ', 100vw';

						if (typeof SelectedCol.parent().components().models[0] != 'undefined' && typeof SelectedCol.parent().components().models[1] != 'undefined') {
							SelectedCol.parent().components().models[0].addAttributes({ 'sizes': size });
							SelectedCol.parent().components().models[1].addAttributes({ 'sizes': size });
						}
					}
				},
				traits: [
					{
						label: 'Alignment',
						type: 'toggle_checkbox',
						name: 'alignment',
						UpdateStyles: true,
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
						],
						changeProp: 1,
					}, {
						type: 'text',
						name: 'alt',
						label: 'Alt',
					}
				]
			}),
		},
			{
				isComponent(el) {
					if (el && el.tagName && el.tagName.toLowerCase() == 'img') {
						return { type: 'image' };
					}
				}
			}),
		view: imageView.extend({

			events: {
				dblclick: function () {
					this.ShowModal()
				}
			},
			init() {

				var model = this.model;

				if (typeof model.parent() != 'undefined' && model.parent().attributes.type == "picture-box")
					this.listenTo(model.parent().parent(), 'active', this.ShowModal); // listen for active event
			},

			onRender() {

				var model = this.model;
				if (event && (event.type == 'load' || event.type == 'drop')) {
					if (typeof model.parent() != 'undefined' && model.parent().attributes.type != "picture-box" && model.parent().parent().attributes.type != 'blockwrapper') {
						setTimeout(function () {
							model.replaceWith('<div class="image-box"><picture class="picture-box">' + model.getEl().outerHTML + '</picture></div>');
						});
					}
				}

				var hasClass = this.model.getClasses().find(v => v == 'img-fluid')
				if (typeof hasClass == 'undefined')
					this.model.addClass('img-fluid');

			},
			ShowModal() {
				var target = VjEditor.getSelected() || this.model;
				window.document.vj_image_target = target;
				var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505#/setting";
				OpenPopUp(null, 900, 'right', 'Image', url, '', true);
			}
		}),
	});
}

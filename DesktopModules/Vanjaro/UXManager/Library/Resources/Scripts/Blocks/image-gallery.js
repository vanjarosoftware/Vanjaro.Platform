export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.imageGallery) {
		bm.add('image-gallery', {
			label: VjLocalized.ImageGallery,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-images' },
			content: `
            <div class="vj-image-gallery">
				<span class="image-frame">
					<picture class="picture-box">
						<img loading="lazy" onclick="typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);" class="img-thumbnail vj-image-gallery-item" src="`+ VjDefaultPath + `image.png"/>
					</picture>
				</span>
				<span class="image-frame">
					<picture class="picture-box">
						<img loading="lazy" onclick="typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);" class="img-thumbnail vj-image-gallery-item" src="`+ VjDefaultPath + `image.png"/>
					</picture>
				</span>
				<span class="image-frame">
					<picture class="picture-box">
						<img loading="lazy" onclick="typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);" class="img-thumbnail vj-image-gallery-item" src="`+ VjDefaultPath + `image.png"/>
					</picture>
				</span>
				<span class="image-frame">
					<picture class="picture-box">
						<img loading="lazy" onclick="typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);" class="img-thumbnail vj-image-gallery-item" src="`+ VjDefaultPath + `image.png"/>
					</picture>
				</span>
			</div>`,
			activate: 1
		});
	}

	const cmd = editor.Commands;

	cmd.add('add-image', ed => {
		var Selected = VjEditor.getSelected();

		if (Selected.attributes.type == 'image-gallery') {
			var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505#!/setting";
			var Img = `
				<span class="image-frame">
					<picture class="picture-box">
						<img loading="lazy" onclick="typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);" style="width:` + Selected.components().models[0].getStyle().width + `; height:` + Selected.components().models[0].getStyle().height + `" class="img-thumbnail vj-image-gallery-item" src="` + VjDefaultPath + `image.png"/>
					</picture>
				</span>
			`;
			Selected.components().add(Img);

			VjEditor.select(Selected.components().last().find('img'));
		}
		else {
			var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505#!/setting";
			var Img = `
				<span class="image-frame">
					<picture class="picture-box">
						<img loading="lazy" onclick="typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);" style="width:` + Selected.getStyle().width + `; height:` + Selected.getStyle().height + `" class="img-thumbnail vj-image-gallery-item" src="` + VjDefaultPath + `image.png"/>
					</picture>
				</span>
			`;
			Selected.closestType('image-gallery').components().add(Img);

			VjEditor.select(Selected.closestType('image-gallery').components().last().find('img'));
		}

		var target = VjEditor.getSelected();
		window.document.vj_image_target = target;
		OpenPopUp(null, 900, 'right', 'Image', url, '', true);
	});

	let domc = editor.DomComponents;
	const defaultType = domc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;

	domc.addType('image-gallery', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
						attributes: { class: 'fa fa-plus', title: VjLocalized.AddImage },
						command: 'add-image',
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
				'custom-name': 'Image Gallery',
				droppable: '.vj-image-gallery-item',
				tagName: 'div',
				traits: [
					{
						label: 'Alignment',
						type: 'toggle_radio',
						name: 'alignment',
						UpdateStyles: true,
						cssproperties: [{ name: "text-align" }],
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' },
						],
						default: 'left',
						changeProp: 1,
					},
				]
			}),
		},
			{
				isComponent(el) {
					if (el && el.classList && el.classList.contains('vj-image-gallery')) {
						return { type: 'image-gallery' };
					}
				}
			}),
		view: defaultView
	});

	const imageType = domc.getType('image');
	const imageModel = imageType.model;
	const imageView = imageType.view;

	domc.addType('image-gallery-item', {
		model: imageModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
						attributes: { class: 'fa fa-plus', title: VjLocalized.AddImage },
						command: 'add-image',
					});

					tb.push({
						attributes: { class: 'fa fa-pencil' },
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
				'custom-name': 'Gallery Item',
				draggable: '.vj-image-gallery',
				droppable: false,
				optimize: true,
				tagName: 'img',
				traits: [{
					type: 'text',
					name: 'title',
					label: 'Title',
				}, {
					type: 'text',
					name: 'alt',
					label: 'Alt',
				}, {
					label: "Width",
					name: "width",
					type: "custom_range",
					cssproperties: [{ name: "width" }],
					units: [
						{ name: 'px', min: 10, max: 1920, step: 1, value: 128 },
						{ name: '%', min: 1, max: 100, step: 1, value: 100 },
					],
					unit: "px",
					changeProp: 1,
				}, {
					label: "Height",
					name: "height",
					type: "custom_range",
					cssproperties: [{ name: "height" }],
					units: [
						{ name: 'px', min: 10, max: 1000, step: 1, value: 128 },
						{ name: '%', min: 1, max: 100, step: 1, value: 100 },
					],
					unit: "px",
					changeProp: 1,
				}, {
					label: " ",
					name: "href",
					type: "href",
					href: "",
					"data_href_type": "url",
					changeProp: 1,
				}],
				resizable: {
					tc: 0,
					cl: 0,
					cr: 0,
					bc: 0,
					ratioDefault: 1,
					onMove: function (e) {

						var SelectedCol = VjEditor.getSelected();
						var width = "";
						var height = "";

						if (typeof SelectedCol.getStyle().width == 'undefined')
							width = SelectedCol.components().models[0].components().models[0].getStyle().width;
						else
							width = SelectedCol.getStyle().width;

						if (typeof SelectedCol.getStyle().height == 'undefined')
							height = SelectedCol.components().models[0].components().models[0].getStyle().height;
						else
							height = SelectedCol.getStyle().height;

						$(SelectedCol.parent().parent().parent().find('img')).each(function () {
							this.setStyle('width:' + width + '; height:' + height + ';');
						});
					},
					onEnd: function (e) {

						var SelectedCol = VjEditor.getSelected();
						var width = "";
						var height = "";

						if (typeof SelectedCol.getStyle().width == 'undefined')
							width = SelectedCol.components().models[0].components().models[0].getStyle().width;
						else
							width = SelectedCol.getStyle().width;

						if (typeof SelectedCol.getStyle().height == 'undefined')
							height = SelectedCol.components().models[0].components().models[0].getStyle().height;
						else
							height = SelectedCol.getStyle().height;

						$(SelectedCol.parent().parent().parent().find('img')).each(function () {

							//Updating Trait on resize
							if (typeof this.view != 'undefined') {

								var valWidth = parseInt(width);
								var valHeight = parseInt(height);

								this.getTrait('width').set({
									value: valWidth
								});

								this.getTrait('height').set({
									value: valHeight
								});

								if (typeof this.getTrait('width').el != 'undefined') {
									this.getTrait('width').el.children[0].value = valWidth
									this.getTrait('width').el.children[1].value = valWidth
								}

								if (typeof this.getTrait('height').el != 'undefined') {
									this.getTrait('height').el.children[0].value = valHeight
									this.getTrait('height').el.children[1].value = valHeight
								}

								this.set({ 'width': valWidth });
								this.set({ 'height': valHeight });
							}
						});
					}
				}
			}),
		},
			{
				isComponent(el) {
					if (el && el.classList && el.classList.contains('vj-image-gallery-item')) {
						return { type: 'image-gallery-item' };
					}
				}
			}),
		view: imageView.extend({
			events: {
				dblclick: function () {
					this.ShowModal()
				},
			},
			init() {
				this.listenTo(this.model.parent(), 'active', this.ActivateModal); // listen for active event
				this.listenTo(this.model, 'change:src', this.ChangeSrc);
				this.listenTo(this.model, 'change:href', this.ChangeHref);
			},
			ActivateModal() {
				var Selected = this.model.collection.first();
				VjEditor.select(Selected);
				var target = Selected;
				window.document.vj_image_target = target;
				var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505#!/setting";
				OpenPopUp(null, 900, 'right', 'Image', url, '', true);
			},
			ShowModal() {
				var target = VjEditor.getSelected() || this.model;
				window.document.vj_image_target = target;
				var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505#!/setting";
				OpenPopUp(null, 900, 'right', 'Image', url, '', true);
			},
			ChangeSrc() {
				var src = this.model.attributes.src;
				this.model.addAttributes({ 'data-src': src });
			},
			ChangeHref() {
				var href = this.model.attributes.href;
				if (href == "") {
					var onclick = "typeof OpenImagePopup != 'undefined' && OpenImagePopup(this);"
					this.model.addAttributes({ onclick: onclick });
				}
				else {
					const attr = this.model.getAttributes();
					delete attr.onclick;
					this.model.setAttributes(attr);
				}

			}
		}),
	});
}

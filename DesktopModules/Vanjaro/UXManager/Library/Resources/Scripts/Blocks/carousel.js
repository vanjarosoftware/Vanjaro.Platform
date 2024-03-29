﻿export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	bm.add(c.blocks.carousel, {
		label: `
      <svg class="gjs-block-svg" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
        <path d="M22 7.6c0-1-.5-1.6-1.3-1.6H3.4C2.5 6 2 6.7 2 7.6v9.8c0 1 .5 1.6 1.3 1.6h17.4c.8 0 1.3-.6 1.3-1.6V7.6zM21 18H3V7h18v11z" fill-rule="nonzero"/><path d="M4 12.5L6 14v-3zM20 12.5L18 14v-3z"/>
      </svg>
      <div class="gjs-block-label">`+ VjLocalized.Carousel + `</div>
    `,
		category: VjLocalized.Basic,
		content: {
			classes: ['carousel', 'vj-carousel', 'slide'],
			attributes: { 'data-bs-ride': 'carousel', 'data-bs-interval': '5000' },
			type: 'carousel'
		}
	});

	let dc = editor.DomComponents;
	const defaultType = dc.getType('default');
	const defaultView = defaultType.view;

	dc.addType('carousel', {

		model: {
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
						attributes: { class: 'fa fa-plus', title: VjLocalized.AddSlide },
						command: function (t) {
							return t.runCommand("add-slide");
						}
					});

					tb.push({
						attributes: { class: 'fa fa-arrow-left', title: VjLocalized.PrevSlide },
						command: function (t) {
							const target = this;
							return t.runCommand("slider-prev", { target });
						}
					});

					tb.push({
						attributes: { class: 'fa fa-arrow-right', title: VjLocalized.NextSlide },
						command: function (t) {
							const target = this;
							return t.runCommand("slider-next", { target });
						}
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
							command: 'slider-clone',
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
			defaults: {
				name: 'Carousel',
				traits: [
					{
						type: 'toggle_radio',
						label: 'Controls',
						name: 'controls',
						options: [
							{ id: 'controlstrue', name: 'Yes', class: '' },
							{ id: 'controlsfalse', name: 'No', class: '' },
						],
						default: 'controlstrue',
						changeProp: 1,
						SwitchClass: true,
					},
					{
						type: 'toggle_radio',
						label: 'Indicators',
						name: 'indicators',
						options: [
							{ id: 'indicatorstrue', name: 'Yes', class: '' },
							{ id: 'indicatorsfalse', name: 'No', class: '' },
						],
						default: 'indicatorstrue',
						changeProp: 1,
						SwitchClass: true,
					},
					{
						type: 'toggle_radio',
						label: 'Animation',
						name: 'animation',
						SwitchClass: true,
						options: [
							{ id: 'slide', name: 'Slide', class: '' },
							{ id: 'fade', name: 'Fade', class: 'carousel-fade' },
						],
						default: 'slide',
						changeProp: 1,
					},
					{
						type: 'toggle_radio',
						label: 'Rotate Automatically',
						name: 'automatically',
						SwitchClass: true,
						options: [
							{ id: 'automaticallytrue', name: 'Yes', class: '' },
							{ id: 'automaticallyfalse', name: 'No', class: '' },
						],
						default: 'automaticallytrue',
						changeProp: 1,
					},
					{
						type: 'custom_number',
						label: 'Interval',
						name: 'datainterval',
						default: '5000',
						changeProp: 1,
					},
				],
			},
			init() {
				this.listenTo(this, 'change:controls', this.ChangeControl);
				this.listenTo(this, 'change:indicators', this.ChangeIndicators);
				this.listenTo(this, 'change:animation', this.ChangeAnimation);
				this.listenTo(this, 'change:automatically', this.ChangeAutomatically);
				this.listenTo(this, 'change:datainterval', this.ChangeInterval);
			},
			ChangeAutomatically() {

				if (this.attributes.automatically == 'automaticallyfalse') {
					this.addAttributes({ 'data-bs-interval': 'false' });
					$(this.getTrait('datainterval').el.closest('.gjs-trt-trait__wrp')).hide();
				}
				else {
					this.addAttributes({ 'data-bs-interval': this.attributes.datainterval });
					$(this.getTrait('datainterval').el.closest('.gjs-trt-trait__wrp')).show();
				}
			},
			ChangeInterval() {
				this.addAttributes({ 'data-bs-interval': this.attributes.datainterval });
			},
			ChangeAnimation() {
				$('.gjs-frame').contents().find('#' + this.getId()).carousel('dispose').carousel({ interval: false });
			},
			ChangeIndicators() {

				if (typeof this.attributes.indicators != 'undefined' && this.attributes.indicators != "") {

					var Indicators = this.components().models.find(m => m.attributes.type == 'indicators');

					if (this.attributes.indicators == 'indicatorsfalse' && typeof Indicators != 'undefined')
						Indicators.remove();
					else if (typeof Indicators == 'undefined')
						this.AddIndicators();
				}
			},
			ChangeControl() {

				if (typeof this.attributes.controls != 'undefined' && this.attributes.controls != "") {

					var controlNext = this.components().models.find(m => m.attributes.type == 'next');
					var controlPrev = this.components().models.find(m => m.attributes.type == 'prev');

					if (this.attributes.controls == 'controlsfalse' && typeof controlNext != 'undefined' && typeof controlPrev != 'undefined') {
						controlNext.remove();
						controlPrev.remove();
					}
					else if (controlNext != 'undefined' && typeof controlPrev == 'undefined')
						this.AddControl();
				}
			},
			AddControl() {

				var modelId = this.getId();

				this.components().add(
					`<button type="button" class="carousel-control carousel-control-prev" data-bs-target="#` + modelId + `" data-bs-slide="prev">
						<span data-gjs-selectable="false" class="carousel-control carousel-control-prev-icon" aria-hidden="true"></span>
						<span class="carousel-control visually-hidden">Previous</span>
					</button>
					<button type="button" class="carousel-control carousel-control-next" data-bs-target="#`+ modelId + `" data-bs-slide="next">
						<span data-gjs-selectable="false" class="carousel-control carousel-control-next-icon" aria-hidden="true"></span>
						<span class="carousel-control visually-hidden">Next</span>
					</button>`
				);
			},
			AddIndicators() {

				var slideInner = this.components().models.find(m => m.attributes.type == 'carousel-inner');
				var slides = slideInner.components().models;
				const comps = this.components();
				var markup = '<ol class="carousel-indicators">';
				var modelId = this.getId();

				$.each(slides, function (k, v) {

					markup += '<li data-bs-target="#' + modelId + '" data-bs-slide-to="' + k + '" class="carousel-indicator';

					var index = $(this.closest('[data-gjs-type="carousel-inner"]').getEl()).find('.carousel-item.active').index();

					if (k == index)
						markup += ' active"';
					else
						markup += '"';

					markup += '></li>';
				});

				markup += '</ol>';
				comps.add(markup);
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel')) {
					return { type: 'carousel' };
				}
			},
		
		view: {
			init() {
	
				const comps = this.model.components();

				if (!comps.length) {

					var modelId = this.model.getId();
					this.model.setId(modelId);

					comps.add(`
						<ol class="carousel-indicators">
                            <li data-bs-target="#`+ modelId + `" data-bs-slide-to="0" class="carousel-indicator active"></li>
                            <li data-bs-target="#`+ modelId + `" data-bs-slide-to="1" class="carousel-indicator"></li>
                             <li data-bs-target="#`+ modelId + `" data-bs-slide-to="2" class="carousel-indicator"></li>
                        </ol>
                        <div class="carousel-inner">
                            <div class="carousel-item active">
                                <span class="carousel-link">
                                    <picture class="picture-box">
                                        <img loading="lazy" class="vj-slide-image img-fluid" src="`+ VjDefaultPath + `image.png" />
                                    </picture>
                                </span>
                            </div>
                            <div class="carousel-item">
                                <span class="carousel-link">
                                    <picture class="picture-box">
                                        <img loading="lazy" class="vj-slide-image img-fluid" src="`+ VjDefaultPath + `image.png" />
                                    </picture>
                                </span>
                            </div>
                            <div class="carousel-item">
                                <span class="carousel-link">
                                    <picture class="picture-box">
                                        <img loading="lazy" class="vj-slide-image img-fluid" src="`+ VjDefaultPath + `image.png" />
                                    </picture>
                                </span>
                            </div>
                        </div>
                        <button type="button" class="carousel-control carousel-control-prev" data-bs-target="#`+ modelId + `" data-bs-slide="prev">
                            <span data-gjs-selectable="false" class="carousel-control carousel-control-prev-icon" aria-hidden="true"></span>
                            <span class="carousel-control visually-hidden">Previous</span>
                        </button>
                        <button type="button" class="carousel-control carousel-control-next" data-bs-target="#`+ modelId + `" data-bs-slide="next">
                            <span data-gjs-selectable="false" class="carousel-control carousel-control-next-icon" aria-hidden="true"></span>
                            <span class="carousel-control visually-hidden">Next</span>
                        </button>
					`);
				}
			},
			onRender() {
				//$(this.el).attr('data-bs-interval', false);
			}
		},
	});

	dc.addType('carousel-inner', {

		model: {
			defaults: {
				name: 'Carousel Inner',
				draggable: false,
				droppable: '[data-gjs-type=carousel-item]',
				selectable: false,
				hoverable: false,
				highlightable: false,
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-inner')) return { type: 'carousel-inner' };
		},
		view: defaultView
	});

	dc.addType('carousel-item', {

		model: {

			defaults: {
				name: 'Carousel Item',
				removable: false,
				draggable: ".carousel-inner",
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				selectable: false,
				editable: false,
				hoverable: false,
				traits: []
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-item')) return { type: 'carousel-item' };
			},

		view: defaultView
	});

	dc.addType('carousel-link', {
		model: {
			defaults: {
				name: 'Carousel Link',
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
			},
		},
				isComponent(el) {
					if (el && el.classList && el.classList.contains('carousel-link')) {
						return { type: 'carousel-link' };
					}
				}
			,
		view: defaultView
	});

	const imageType = dc.getType('image');
	const imageModel = imageType.model;
	const imageView = imageType.view;

	dc.addType('carousel-image', {
		model: {
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

					if (model.get('copyable')) {
						tb.push({
							attributes: { class: 'fa fa-clone', title: VjLocalized.CopyCarouselImage },
							command: 'slide-clone',
						});
					}

					if (model.get('removable')) {
						tb.push({
							attributes: { class: 'fa fa-trash-o' },
							command: 'slide-delete',
						});
					}

					model.set('toolbar', tb);
				}
			},
			defaults:  {
				'custom-name': 'Carousel Image',
				draggable: false,
				droppable: false,
				resizable: false,
				optimize: true,
				editor: true,
				source: true,
				tagName: 'img',
				traits: [{
					type: 'text',
					name: 'alt',
					label: 'Alt',
				}, {
					type: 'text',
					name: 'slidetitle',
					label: 'Title',
					changeProp: 1,
				}, {
					type: 'textarea',
					name: 'caption',
					label: 'Caption',
					changeProp: 1,
				}, {
					label: " ",
					name: "href",
					type: "href",
					href: "",
					"data_href_type": "url",
				}]
			},
			init() {
				this.listenTo(this, 'change:slidetitle', this.ChangeTitle);
				this.listenTo(this, 'change:caption', this.ChangeCaption);
				this.listenTo(this, 'change:attributes:target', this.ChangeTarget);
			},
			ChangeTitle() {

				var title = this.attributes.slidetitle;
				var slide = this.closest('[data-gjs-type="carousel-item"]');
				var carouselCaption = slide.components().models.find(m => m.attributes.type == 'carousel-caption');

				if (typeof carouselCaption == 'undefined') {
					slide.append('<div class="carousel-caption d-none d-md-block"></div>');
					carouselCaption = slide.components().models.find(m => m.attributes.type == 'carousel-caption');
				}

				if (carouselCaption.components().length && typeof carouselCaption.components().models.find(t => t.attributes.type == 'carousel-heading') != 'undefined')
					carouselCaption.components().models.find(t => t.attributes.type == 'carousel-heading').remove();

				if (title != "")
					carouselCaption.append('<h5 class="carousel-heading">' + title + '</h5>', { at: 0 });

				if (carouselCaption.components().length == 0)
					carouselCaption.remove();

			},
			ChangeCaption() {

				var caption = this.attributes.caption;
				var slide = this.closest('[data-gjs-type="carousel-item"]');
				var carouselCaption = slide.components().models.find(m => m.attributes.type == 'carousel-caption');

				if (typeof carouselCaption == 'undefined') {
					slide.append('<div class="carousel-caption d-none d-md-block"></div>');
					carouselCaption = slide.components().models.find(m => m.attributes.type == 'carousel-caption');
				}

				if (carouselCaption.components().length && typeof carouselCaption.components().models.find(t => t.attributes.type == 'carousel-text') != 'undefined')
					carouselCaption.components().models.find(t => t.attributes.type == 'carousel-text').remove();

				if (caption != "")
					carouselCaption.append('<p class="carousel-text">' + caption + '</p>');

				if (carouselCaption.components().length == 0)
					carouselCaption.remove();

			},
			ChangeTarget() {
				if (this.getAttributes().target == '_blank')
					this.parent().parent().addAttributes({ target: this.getAttributes().target });
				else
					this.parent().parent().addAttributes({ target: '_self' });
			}
		},
		isComponent(el) {
			if (el && el.classList && el.classList.contains('vj-slide-image')) {
				return { type: 'carousel-image' };
			}
		}
		,
		view: {
			events: {
				dblclick: function () {
					this.ShowModal()
				},
			},
			init() {
				this.listenTo(this.model.parent(), 'active', this.ActivateModal); // listen for active event
				this.model.set('src', this.model.getAttributes().src);
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
		},
	});

	dc.addType('carousel-caption', {
		model: {
			defaults: {
				name: 'Caption',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				//selectable: false,
				editable: false,
				hoverable: false,
				propagate: ['removable', 'draggable', 'droppable', 'badgable', 'stylable', 'highlightable', 'copyable', 'resizable', 'editable', 'layerable', 'hoverable'],
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-caption')) return { type: 'carousel-caption' };
			}
		,
		view: defaultType.view
	});

	dc.addType('carousel-heading', {
		model: {
			defaults: {
				name: 'Title',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				selectable: false,
				editable: false,
				hoverable: false,
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-heading')) return { type: 'carousel-heading' };
			},

		view: defaultType.view
	});

	dc.addType('carousel-text', {
		model: {
			defaults: {
				name: 'Caption',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				selectable: false,
				editable: false,
				hoverable: false,
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-text')) return { type: 'carousel-text' };
			},

		view: defaultType.view
	});

	dc.addType('next', {

		model: {
			defaults: {
				name: 'Nav Next',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				selectable: true,
				editable: false,
				hoverable: false,
				propagate: ['removable', 'draggable', 'droppable', 'badgable', 'stylable', 'highlightable', 'copyable', 'resizable', 'editable', 'layerable', 'selectable', 'hoverable'],
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-control-next')) return { type: 'next' };
			},

		view: defaultType.view
	});

	dc.addType('prev', {

		model: {
			defaults: {
				name: 'Nav Prev',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				selectable: true,
				editable: false,
				hoverable: false,
				propagate: ['removable', 'draggable', 'droppable', 'badgable', 'stylable', 'highlightable', 'copyable', 'resizable', 'editable', 'layerable', 'selectable', 'hoverable'],
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-control-prev')) return { type: 'prev' };
			},

		view: defaultType.view
	});

	dc.addType('indicators', {

		model: {
			defaults: {
				name: 'Indicators',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				selectable: false,
				editable: false,
				hoverable: false,
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-indicators')) return { type: 'indicators' };
		},

		view: defaultType.view
	});

	dc.addType('indicator', {

		model: {
			defaults: {
				name: 'Indicator',
				removable: false,
				draggable: false,
				droppable: false,
				badgable: false,
				stylable: false,
				highlightable: false,
				copyable: false,
				resizable: false,
				layerable: false,
				selectable: true,
				editable: false,
				hoverable: false,
			},
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('carousel-indicator')) return { type: 'indicator' };
			},

		view: defaultType.view
	});

	const cmd = editor.Commands;

	cmd.add('add-slide', {
		run(editor, sender) {

			var url = CurrentExtTabUrl + "&guid=a7a5e632-a73a-4792-8049-bc15a9435505#!/setting";
			var Selected = VjEditor.getSelected();
			var slideInner = Selected.components().models.find(m => m.attributes.type == 'carousel-inner');

			slideInner.components().add(`
            <div class="carousel-item">
                <span class="carousel-link">
                    <picture class="picture-box">
                        <img loading="lazy" class="vj-slide-image img-fluid" src = "`+ VjDefaultPath + `image.png" />
			        </picture>
                </span>
            </div>`
			);

			var Indicators = Selected.components().models.find(m => m.attributes.type == 'indicators');

			if (typeof Indicators != 'undefined')
				Indicators.components().add('<li data-bs-target="#' + Selected.getId() + '" data-bs-slide-to="' + Indicators.components().length + '" class="carousel-indicator"></li>');

			$('.gjs-frame').contents().find('#' + Selected.getId()).carousel('dispose').carousel(slideInner.components().models.length - 1);

			$.each(getAllComponents(slideInner.components().last()), function (i, n) {
				if (n.attributes.type == 'carousel-image') {
					window.document.vj_image_target = n;
					VjEditor.select(n);
					OpenPopUp(null, 900, 'right', 'Image', url, '', true);
				}
			});
		}
	});

	cmd.add('slide-clone', editor => {
		var selected = editor.getSelected();
		var slider = selected.closest('[data-gjs-type="carousel"]');
		var slideInner = selected.closest('[data-gjs-type="carousel-inner"]');
		var slide = selected.closest('[data-gjs-type="carousel-item"]').clone();
		slideInner.append(slide);
		slideInner.components().last().removeClass('active');

		var Indicators = slider.components().models.find(m => m.attributes.type == 'indicators');
		if (typeof Indicators != 'undefined')
			Indicators.components().add('<li data-bs-target="#' + slider.getId() + '" data-bs-slide-to="' + Indicators.components().length + '"></li>');

		VjEditor.runCommand("slide-select", { slide: slideInner.components().last(), slideIndex: slideInner.components().models.length - 1 });
	});

	cmd.add('slide-select', {
		run(editor, sender, opts = {}) {
			this.editor = editor;
			this.options = opts;
			this.target = opts.target || editor.getSelected();
			const target = this.target;
			var slider = target.closest('[data-gjs-type="carousel"]') || target.parent().closest('[data-gjs-type="carousel"]');

			$('.gjs-frame').contents().find('#' + slider.getId()).carousel('dispose').carousel(opts.slideIndex);

			setTimeout(function () {
				$.each(getAllComponents(opts.slide), function (i, n) {
					if (n.attributes.type == 'carousel-image') {
						VjEditor.select(n);
					}
				});
			}, 1000);
		}
	});

	cmd.add('slide-delete', {
		run(editor, sender, opts = {}) {
			var selected = editor.getSelected() || opts.target;
			var slider = selected.closest('[data-gjs-type="carousel"]') || opts.target.parent().closest('[data-gjs-type="carousel"]');
			var slideInner = selected.closest('[data-gjs-type="carousel-inner"]') || opts.target.parent().closest('[data-gjs-type="carousel-inner"]');

			if (slideInner.components().length > 1) {

				var slide = selected.closest('[data-gjs-type="carousel-item"]') || opts.target.parent().closest('[data-gjs-type="carousel-item"]');
				var Indicators = slider.components().models.find(m => m.attributes.type == 'indicators');

				if (slideInner.components().first().getId() != slide.getId())
					VjEditor.runCommand("slide-select", { target: opts.target, slide: slideInner.components().first(), slideIndex: 0 });
				else
					VjEditor.runCommand("slide-select", { target: opts.target, slide: slideInner.components().models[1], slideIndex: 1 });

				if (typeof Indicators != 'undefined')
					Indicators.components().last().remove();

				slide.remove();
			}
			else
				slider.remove();
		}
	});

	cmd.add('slider-clone', {
		run(editor, sender, opts = {}) {
			this.editor = editor;
			this.options = opts;
			this.target = opts.target || editor.getSelected();
			const target = this.target;
			var clone = target.clone();
			var modelId = clone.getId();
			var Indicators = clone.components().models.find(m => m.attributes.type == 'indicators');

			if (typeof Indicators != 'undefined') {
				$.each(Indicators.components().models, function (i, n) {
					n.addAttributes({ 'data-bs-target': '#' + modelId });
				});
			}

			var controlNext = clone.components().models.find(m => m.attributes.type == 'next');
			var controlPrev = clone.components().models.find(m => m.attributes.type == 'prev');

			if (typeof controlNext != 'undefined')
				controlNext.addAttributes({ 'data-bs-target': '#' + modelId });

			if (typeof controlPrev != 'undefined')
				controlPrev.addAttributes({ 'data-bs-target': '#' + modelId });

			target.parent().append(clone);
		}
	});

	cmd.add('slider-prev', {
		run(editor, sender, opts = {}) {
			this.editor = editor;
			this.options = opts;
			this.target = opts.slider || editor.getSelected();
			const target = this.target;

			$('.gjs-frame').contents().find('#' + target.getId()).carousel('prev');

			setTimeout(function () {
				var image = target.closest('[data-gjs-type="carousel"]').find('.carousel-item.active img');
				editor.select(image);
			}, 1000);
		}
	});

	cmd.add('slider-next', {
		run(editor, sender, opts = {}) {
			this.editor = editor;
			this.options = opts;
			this.target = opts.slider || editor.getSelected();
			const target = this.target;

			$('.gjs-frame').contents().find('#' + target.getId()).carousel('next');

			setTimeout(function () {
				var image = target.closest('[data-gjs-type="carousel"]').find('.carousel-item.active img');
				editor.select(image);
			}, 1000);
		}
	});
}

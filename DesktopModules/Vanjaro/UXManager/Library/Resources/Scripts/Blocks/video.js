export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;

	if (c.blocks.video) {
		bm.add('video', {
			label: VjLocalized.Video,
			category: VjLocalized.Basic,
			attributes: { class: 'fa fa-youtube-play' },
			content: `
				<div class='video-box embed-container'>
					<video controls src='`+ VjDefaultPath + `Flower.mp4' data-gjs-clickable="false" data-gjs-selectable="false" data-gjs-hoverable="false" data-gjs-draggable="false" data-gjs-droppable="false" data-gjs-layerable="false"></video>
				</div>	
			`,
			activate: 1
		});
	}

	let domc = editor.DomComponents;

	const defaultType = domc.getType('default');
	const defaultView = defaultType.view;

	domc.addType('iframe', {
		model: {
			defaults: {
				droppable: false,
				draggable: false,
				hoverable: false,
				selectable: false,
				clickable: false,
				layerable: false,
				traits: [],
			}
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('frame-box')) {
					return { type: 'iframe' };
				}
			}
		,
		view: defaultView
	});

	domc.addType('videobox', {
		model: {
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
			defaults: {
				'custom-name': 'Video',
				droppable: false,
				resizable: {
					tc: 0,
					cl: 0,
					cr: 0,
					bc: 0,
					ratioDefault: 1,
					onMove: function (e) {
						var SelectedCol = VjEditor.getSelected();
						SelectedCol.removeClass('embed-container');
						SelectedCol.components().models[0].addStyle({ 'width': $(SelectedCol.getEl()).css('width'), 'height': $(SelectedCol.getEl()).css('height') });
					}
				},
				attributes: { allowfullscreen: 'allowfullscreen' },
				traits: [],
			},
			init() {
				var traits = [];
				var prov = this.attributes.provider;

				switch (prov) {
					case 'yt':
						traits = this.getYoutubeTraits();
						break;
					default:
						traits = this.getSourceTraits();
				}
				this.set('traits', traits);

				if (typeof prov == "undefined")
					this.set('provider', 'so')

				this.listenTo(this, 'change:provider', this.ChangeProvider);
				this.listenTo(this, 'change:videoId', this.ChangeVideoId);
				this.listenTo(this, 'change:src', this.ChangeSrc);

			},
			getSourceTraits() {
				return [
					this.ProviderTrait(),
					{
						label: 'Source',
						name: 'src',
						type: 'text',
						placeholder: 'eg. ' + VjDefaultPath + 'Flower.mp4',
						changeProp: 1,
					},
					this.AutoplayTrait(),
					this.LoopTrait(),
					this.ControlsTrait()
				]
			},
			getYoutubeTraits() {
				return [
					this.ProviderTrait(),
					{
						label: 'Video ID',
						name: 'videoId',
						type: 'text',
						placeholder: 'eg. jNQXAC9IVRw',
						changeProp: 1,
					},
					this.AutoplayTrait(),
					this.LoopTrait(),
					{
						label: "Show Related Videos",
						name: "rel",
						type: "toggle_radio",
						options: [
							{ id: 'reltrue', name: 'yes' },
							{ id: 'relfalse', name: 'no' },
						],
						default: 'relfalse',
						changeProp: 1,
					},
					{
						label: "Show YouTube Logo",
						name: "logo",
						type: "toggle_radio",
						options: [
							{ id: 'logotrue', name: 'yes' },
							{ id: 'logofalse', name: 'no' },
						],
						default: 'logofalse',
						changeProp: 1,
					}
				]
			},
			ProviderTrait() {
				return {
					label: "Provider",
					name: "provider",
					type: 'dropdown',
					options: [
						{ id: 'so', name: 'HTML5 Source' },
						{ id: 'yt', name: 'Youtube' },
					],
					default: 'so',
					changeProp: 1,
				}
			},
			AutoplayTrait() {
				return {
					label: "Autoplay",
					name: "autoplay",
					type: "toggle_radio",
					options: [
						{ id: 'autoplaytrue', name: 'yes' },
						{ id: 'autoplayfalse', name: 'no' },
					],
					default: 'autoplayfalse',
					changeProp: 1,
				}
			},
			LoopTrait() {
				return {
					label: "Loop",
					name: "loop",
					type: "toggle_radio",
					options: [
						{ id: 'looptrue', name: 'yes' },
						{ id: 'loopfalse', name: 'no' },
					],
					default: 'loopfalse',
					changeProp: 1,
				}
			},
			ControlsTrait() {
				return {
					label: "Controls",
					name: "controls",
					type: "toggle_radio",
					options: [
						{ id: 'controlstrue', name: 'yes' },
						{ id: 'controlsfalse', name: 'no' },
					],
					default: 'controlstrue',
					changeProp: 1,
				}
			},

			ChangeProvider() {

				let traits;

				if (this.attributes.provider == "yt") {
					this.components().models[0].replaceWith('<iframe class="frame-box" src="https://www.youtube.com/embed/"></iframe>');
					traits = this.getYoutubeTraits();
					this.set({ traits });
					this.components().models[0].set({ 'rel': 0, 'logo': 0 });
					this.set({ 'src': 'https://www.youtube.com/embed/' });
				}
				else {
					this.components().models[0].replaceWith('<video src="' + VjDefaultPath + 'Flower.mp4"></video>');
					traits = this.getSourceTraits();
					this.set({ traits });
					this.set({ 'src': '' + VjDefaultPath + 'Flower.mp4' });
					this.getTrait('controls').setTargetValue('controlstrue');
				}

				this.getTrait('autoplay').setTargetValue('autoplayfalse');
				this.getTrait('loop').setTargetValue('loopfalse');

			},
			ChangeSrc() {
				var src = this.get('src');
				this.set({ 'src': src });
				this.components().models[0].addAttributes({ 'src': src });
				this.components().models[0].set({ 'src': src });

				if (this.attributes.provider == "so")
					$(this.components().models[0].getEl()).find('video').attr('src', src);
			},
			ChangeVideoId() {

				var vId = this.attributes.videoId;
				var src = 'https://www.youtube.com/embed/' + vId;

				if (typeof vId != 'undefined' && vId != '') {

					if (this.components().models[0].get('autoplay')) {
						if (src.indexOf('mute') < 0) {
							if (src.indexOf('?') > 0)
								src += "&mute=1&autoplay=1";
							else
								src += "?mute=1&autoplay=1";
						}
					}

					if (this.components().models[0].get('loop')) {
						if (src.indexOf('loop') < 0) {
							if (src.indexOf('?') > 0)
								src += '&loop=1&playlist=' + vId;
							else
								src += '?loop=1&playlist=' + vId;
						}
					}

					if (!this.components().models[0].get('rel')) {
						if (src.indexOf('rel') < 0) {
							if (src.indexOf('?') > 0)
								src += '&rel=0';
							else
								src += '?rel=0';
						}
					}

					if (!this.components().models[0].get('logo')) {
						if (src.indexOf('modestbranding') < 0) {
							if (src.indexOf('?') > 0)
								src += '&modestbranding=1';
							else
								src += '?modestbranding=1';
						}
					}

					this.set({ 'src': src });
					this.components().models[0].set({ 'src': src, 'controls': 1 });
					this.components().models[0].addAttributes({ 'allow': 'autoplay' });
					
				}
			}
		},
			isComponent(el) {
				if (el && el.classList && el.classList.contains('video-box')) {
					return { type: 'videobox' };
				}
			}
		,
		view: {
			events: {
				dblclick: function () {
					this.ShowVideos();
				}
			},
			init() {
				this.listenTo(this.model, 'active', this.ShowVideos); // listen for active event
			},
			ShowVideos() {
				var target = VjEditor.getSelected() || this.model;
				window.document.vj_video_target = target;
				var url = CurrentExtTabUrl + "&guid=a7a5e633-a33a-4792-8149-bc15b9433505";
				OpenPopUp(null, 900, 'right', VjLocalized.Video, url, '', true);
			},
		}
	});
}

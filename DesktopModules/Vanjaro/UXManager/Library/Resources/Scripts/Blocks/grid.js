import traits from "./traits";

export default (editor, config = {}) => {
	const c = config;
	let bm = editor.BlockManager;
	const stylePrefix = config.stylePrefix;
	const labelCell = config.labelColumn;
	const clsRow = `${stylePrefix}row`;
	const clsCell = `${stylePrefix}col-lg-6`;

	const colAttr = {
		class: clsCell + ' col-md-6 col-sm-6 col-12',
		'data-gjs-custom-name': labelCell,
	};

	const attrsToString = attrs => {
		const result = [];

		for (let key in attrs) {
			let value = attrs[key];
			const toParse = value instanceof Array || value instanceof Object;
			value = toParse ? JSON.stringify(value) : value;
			result.push(`${key}=${toParse ? `'${value}'` : `"${value}"`}`);
		}

		return result.length ? ` ${result.join(' ')}` : '';
	}

	const attrsCell = attrsToString(colAttr);

	if (c.blocks.grid) {
		bm.add('grid', {
			label: `<div class="gjs-block-label">` + VjLocalized.Grid + `</div>`,
			category: VjLocalized.Basic,
			attributes: { class: 'fas fa-columns' },
			content: `
            <div class="container">
                <div class="row">
                    <div ${attrsCell}></div>
                    <div ${attrsCell}></div>
                </div>
            </div>`,
			activate: 1
		});
	}

	const cmd = editor.Commands;

	cmd.add('add-column', ed => {
		var Selected = VjEditor.getSelected();
		var Row = '<div class="row"></div>';
		var Column = '<div class="col-lg-1 col-md-1 col-sm-1 col-12"></div>';
		if (Selected.attributes.type == 'grid') {
			if (typeof Selected.components().models[0] != 'undefined')
				Selected.components().models[0].components().add(Column);
			else {
				Selected.components().add(Row);
				Selected.components().models[0].components().add(Column);
			}
		}
		else
			Selected.parent().components().add(Column);
	});

	let dc = editor.DomComponents;
	const defaultType = dc.getType('default');
	const defaultModel = defaultType.model;
	const defaultView = defaultType.view;
	global.arrway = [];

	for (var i = 1; i <= 12; i++) {
		arrway.push((100 / 12) * i);
	}

	global.getColSize = function (userNum) {

		var smallestDiff = Math.abs(userNum - arrway[0]);
		var closest = 0;

		for (i = 1; i < arrway.length; i++) {
			var currentDiff = Math.abs(userNum - arrway[i]);
			if (currentDiff < smallestDiff) {
				smallestDiff = currentDiff;
				closest = i;
			}
		}
		return closest + 1;
	};

	global.setColSize = function (colSize) {

		var colClass = 'col-lg-';
		var Device = VjEditor.getDevice();
		var browserwidth = window.innerWidth - 325;

		if (Device == 'Mobile Portrait' || browserwidth <= '360')
            colClass = 'col-';
        else if (Device == 'Mobile Landscape' || browserwidth <= '767')
            colClass = 'col-sm-';
		else if (Device == 'Tablet' || browserwidth <= '991')
			colClass = 'col-md-';

		var re = new RegExp('(' + colClass + '(\\d+))', 'i');
		var SelectedCol = VjEditor.getSelected();
		var reResult = re.exec(SelectedCol.getClasses());

		if (reResult && parseInt(reResult[2]) !== colSize) {
			SelectedCol.removeClass(reResult[1]);
			SelectedCol.addClass(colClass + colSize);
		}
		else
			SelectedCol.addClass(colClass + colSize);

		SelectedCol.removeStyle('width');
	};

	dc.addType('row', {
		model: defaultModel.extend({
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				'custom-name': 'Row',
				tagName: 'div',
				draggable: true,
				droppable: '[data-gjs-type=column]',
				layerable: true,
				selectable: false,
				hoverable: false,
				highlightable: false,
			})
		}, {
			isComponent(el) {
				if (el && el.tagName && el.tagName.toLowerCase() == 'div' && el.classList && el.classList.contains('row')) {
					return { type: 'row' };
				}
			}
		}),
		view: defaultView.extend({
			onRender() {

				var model = this.model;

				if (typeof model.parent() != 'undefined' && model.parent().attributes.type != "grid") {
					setTimeout(function () {
						model.replaceWith('<div class="container">' + model.getEl().outerHTML + '</div>');
					});
				}
			},
		})
	});

	dc.addType('column', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {
					var tb = [];

					tb.push({
						attributes: { class: 'fa fa-plus', title: VjLocalized.AddColumn },
						command: 'add-column',
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
							command: 'vj-delete',
						});
					}

					model.set('toolbar', tb);
				}
			},
			defaults: Object.assign({}, defaultModel.prototype.defaults, {
				//unstylable: ['width'],
				draggable: '.row',
				selectable: true,
				layerable: true,
				droppable: true,
				'stylable-require': ['flex-basis'],
				resizable: {
					tl: 0,
					tc: 0,
					tr: 0,
					cl: 0,
					bl: 0,
					br: 0,
					bc: 0,
					keyWidth: 'width',
					currentUnit: 1,
					minDim: 1,
					onMove: function (e) {

						var SelectedCol = VjEditor.getSelected();
						var $grid = $(SelectedCol.parent().parent().getEl());

						if ($grid.find('.snap-grid').length <= 0)
							$grid.prepend('<div class="snap-grid" style="display:none;"><div class="snap-grid-row"><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div><div class="snap-grid-column"></div></div></div>');

						$grid.find('.snap-grid-column').height($grid.height());
						$grid.find('.snap-grid').fadeIn();

						var SelectedColWidth = parseInt(SelectedCol.getStyle()['width']);
						var ContainerWidth = $(VjEditor.getSelected().parent().getEl()).width();

						var width = parseFloat(SelectedColWidth, 10) / ContainerWidth * 100

						if (width <= 8.333333)
							SelectedCol.addStyle({ 'width': '8.333333%' });
						else if (width >= 100)
							SelectedCol.addStyle({ 'width': '100%' });
					},
					onEnd: function (e) {
						var SelectedCol = VjEditor.getSelected();
						$(SelectedCol.parent().parent().getEl()).find('.snap-grid').fadeOut();

						var SelectedColWidth = parseInt(SelectedCol.getStyle()['width']);
						var ContainerWidth = $(VjEditor.getSelected().parent().getEl()).width();

						var colSize = getColSize(parseInt(SelectedColWidth, 10) / ContainerWidth * 100);
						setColSize(colSize);
					},
				},
				traits: [
					{
						label: "Horizontal",
						name: "horizontalalignment",
						type: "toggle_radio",
						UpdateStyles: true,
						cssproperties: [{ name: "text-align" }],
						options: [
							{ id: 'left', name: 'left', image: 'align-left' },
							{ id: 'center', name: 'center', image: 'align-center' },
							{ id: 'right', name: 'right', image: 'align-right' }
						],
						default: "left",
						changeProp: 1,
					},
					{
						label: "Vertical",
						name: "verticalalignment",
						type: 'toggle_checkbox',
						UpdateStyles: true,
						cssproperties: [{ name: "align-self" }],
						options: [
							{ id: 'vertical-start', name: 'flex-start', image: 'align-top' },
							{ id: 'vertical-center', name: 'center', image: 'align-middle' },
							{ id: 'vertical-end', name: 'flex-end', image: 'align-bottom' }
						],
						default: 'vertical-start',
						changeProp: 1,
					},
					{
						label: "Pane Id",
						name: "data-pane",
						type: 'text',
						changeProp: 1,
					},
				]
			}),
			init() {
				this.listenTo(this, 'change:data-pane', this.ChangeControl);
			},
			ChangeControl() {

				this.addAttributes({ 'data-pane': this.getTrait('data-pane').getInitValue() });

				if (!this.components().length)
					$(this.getEl()).attr("data-empty", "true");
			}
		}, {
			isComponent(el) {
				let match = false;
				if (el && el.tagName && el.tagName.toLowerCase() == 'div') {
					el.classList.forEach(function (klass) {
						if (klass == "col" || klass.match(/^col-/)) {
							match = true;
						}
					});
				}
				if (match) return { type: 'column' };
			}
		}),
		view: defaultView.extend({
			onRender() {
				if (!this.model.components().length)
					$(this.el).attr("data-empty", "true");
			}
		})
	});

	dc.addType('grid', {
		model: defaultModel.extend({
			initToolbar() {
				var model = this;
				if (!model.get('toolbar')) {

					var GetBlockMenus = function () {
						var Result = [];
						if (IsAdmin)
							Result.push({ 'Title': VjLocalized.SaveBlock, 'Command': 'custom-block' });
						return Result;
					};

					var tb = [];

					if (GetBlockMenus().length > 0) {
						tb.push({
							attributes: { class: 'fa fa-bars', title: VjLocalized.Menu },
							command: function (t) {
								return t.runCommand("tlb-app-actions", {
									BlockMenus: GetBlockMenus()
								})
							}
						});
					}

					tb.push({
						attributes: { class: 'fa fa-plus' },
						command: 'add-column',
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
				'custom-name': 'Grid',
				tagName: 'div',
				copyable: true,
				droppable: '[data-gjs-type=column]',
				traits: [{
					label: 'Width',
					name: 'width',
					type: 'toggle_radio',
					SwitchClass: true,
					options: [
						{ id: 'fixed', name: 'Fixed', class: 'container' },
						{ id: 'fluid', name: 'Fluid', class: 'container-fluid' }
					],
					default: 'fixed',
					changeProp: 1,
				}, {
					label: 'Alignment',
					name: 'horizontalalignment',
					type: 'toggle_radio',
					UpdateStyles: true,
					selector: '[data-gjs-type="row"]',
					cssproperties: [{ name: "justify-content" }],
					options: [
						{ id: 'flex-start', name: 'flex-start', image: 'align-left' },
						{ id: 'center', name: 'center', image: 'align-center' },
						{ id: 'flex-end', name: 'flex-end', image: 'align-right' },
						{ id: 'space-around', name: 'space-around', image: 'align-around' },
						{ id: 'space-between', name: 'space-between', image: 'align-between' }
					],
					default: 'flex-start',
					changeProp: 1,
				},
				]
			})
		}, {
			isComponent(el) {
				if (el && el.tagName && el.tagName.toLowerCase() == 'div' && el.classList && (el.classList.contains('container') || el.classList.contains('container-fluid')) && (el.firstElementChild != null && el.firstElementChild.classList.contains('row'))) {
					return { type: 'grid' };
				}
			}
		}),
		view: defaultView.extend({
			init() {
				this.listenTo(this.model, 'active', this.ShowGrid);
			},
			ShowGrid() {
				var $html = `<div class="select-layout">
<div class="toggle-wrapper" id="width"><input type="radio" checked="checked" onclick="$('#' + $(this).attr('id').replace('m', '')).trigger('click')" name="mwidth" id="mfixed" value="mFixed"><label for="mfixed">Fixed</label><input type="radio" onclick="$('#' + $(this).attr('id').replace('m', '')).trigger('click')" name="mwidth" id="mfluid" value="mFluid"><label for="mfluid">Fluid</label></div>
	<div class="text-center layout-header">Equal Widths</div>
	<div class="container-fluid pt-3 pb-3">
		<div class="row text-center">
			<div class="col-sm-2"> 
				<a onclick="SingleClick([{'size': '12'}])" ondblclick="DoubleClick([{'size': '12'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-12.png">
				    <h5>1 col</h5>
                </a>
			</div>
			<div class="col-sm-2 active">
				<a onclick="SingleClick([{'size': '6'}, {'size': '6'}])" ondblclick="DoubleClick([{'size': '6'}, {'size': '6'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-6.png">
				    <h5>2 col</h5>
                </a>
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '4'}, {'size': '4'}, {'size': '4'}])" ondblclick="DoubleClick([{'size': '4'}, {'size': '4'}, {'size': '4'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-4.png">
				    <h5>3 col</h5>
                </a>
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '3'}, {'size': '3'}, {'size': '3'}, {'size': '3'}])" ondblclick="DoubleClick([{'size': '3'}, {'size': '3'}, {'size': '3'}, {'size': '3'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-3.png">
				    <h5>4 col</h5>
                </a>
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '2'}, {'size': '2'}, {'size': '2'}, {'size': '2'}, {'size': '2'}, {'size': '2'}])" ondblclick="DoubleClick([{'size': '2'}, {'size': '2'}, {'size': '2'}, {'size': '2'}, {'size': '2'}, {'size': '2'}])"><img style="width: 42px;height: 42px;" class="img-responsive" src="`+ VjDefaultPath + `col-2.png">
				    <h5>6 col</h5>
                </a>
			</div>
		</div>
	</div>
	<div class="text-center layout-header">Sidebars</div>
	<div class="container-fluid pt-3 pb-3">
		<div class="row text-center">
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '3'}, {'size': '9'}])" ondblclick="DoubleClick([{'size': '3'}, {'size': '9'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-3.9.png">
				    <h5>33% + 67%</h5>
                </a>    
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '4'}, {'size': '8'}])" ondblclick="DoubleClick([{'size': '4'}, {'size': '8'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-4.8.png">
				    <h5>25% + 75%</h5>
                </a>
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '9'}, {'size': '3'}])" ondblclick="DoubleClick([{'size': '9'}, {'size': '3'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-9.3.png">
				    <h5>67% + 33%</h5>
                </a>
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '8'}, {'size': '4'}])" ondblclick="DoubleClick([{'size': '8'}, {'size': '4'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-8.4.png">
				    <h5>75% + 25%</h5>
                </a>
			</div>
			<div class="col-sm-2">
				<a onclick="SingleClick([{'size': '3'}, {'size': '6'}, {'size': '3'}])" ondblclick="DoubleClick([{'size': '3'}, {'size': '6'}, {'size': '3'}])"><img class="img-responsive" src="`+ VjDefaultPath + `col-3.6.3.png">
				    <h5>25% x2 + 50%</h5>
                </a>    
			</div>
		</div>
	</div>
</div>`;
				OpenPopUp(null, 500, 'right', 'Grid', '', '', true);

				var $modalbody = $('.uxmanager-modal .modal-body');

				$modalbody.find('.loader').hide();

				if (!$modalbody.find('.select-layout').length)
					$modalbody.append($html);
			},
		})
	});
}

var timer = 0;
var delay = 200;
var prevent = false;

global.ChangeGridColumns = function (cols) {
	var compSelected = VjEditor.getSelected();
	var content = `<div class="row">`;
	var i;
	for (i = 0; i < cols.length; i++) {
		content += `<div class="col-lg-` + cols[i].size + ` col-md-` + cols[i].size + ` col-sm-` + cols[i].size + ` col-12"></div>`;
	}
	content += `</div>`;
	compSelected.components(content);
}

global.SingleClick = function (cols) {
	if ($(event.target).prop("tagName").toLowerCase() == "img" || $(event.target).prop("tagName").toLowerCase() == "h5") {
		$(event.target).parents(".select-layout").find(".active").removeClass("active");
		$(event.target).parent().addClass("active");
	}
	timer = setTimeout(function () {
		if (!prevent)
			ChangeGridColumns(cols);
		prevent = false;
	}, delay);
}

global.DoubleClick = function (cols) {
	clearTimeout(timer);
	prevent = true;
	ChangeGridColumns(cols);
	$(window.document.body).find('[data-bs-dismiss="modal"]').click();
}

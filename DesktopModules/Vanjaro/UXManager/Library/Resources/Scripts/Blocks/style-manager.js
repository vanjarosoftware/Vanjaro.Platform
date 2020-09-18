export default (editor, config = {}) => {

	const sm = editor.StyleManager;
	const cmd = editor.Commands;

	sm.addType('customradio', {
		model: {
		},
		view: {
			templateInput() {
				const pfx = this.pfx;
				const ppfx = this.ppfx;
				return `
      <div class="${ppfx}field ${ppfx}field-radio">
      </div>
    `;
			},

			events() {
				return {
					'change [type=radio]': 'inputradioChanged',
					'input [type=radio]': 'inputradioChanged',
					change: ''
				};
			},

			onRender() {
				const pfx = this.pfx;
				const ppfx = this.ppfx;
				const itemCls = `${ppfx}radio-item-label`;
				const model = this.model;
				const prop = model.get('property');
				const options = model.get('list') || model.get('options') || [];

				if (!this.input) {
					if (options && options.length) {
						let inputStr = '';

						options.forEach(el => {
							let cl = el.className ? `${el.className} ${pfx}icon ${itemCls}` : '';
							let id = `${prop}-${el.value}`;
							let labelTxt = el.name || el.value;
							let titleAttr = el.title ? `title="${el.title}"` : '';
							inputStr += `
							<div class="${ppfx}radio-item">
								<input type="radio" class="${pfx}radio" id="${id}" name="${prop}" value="${el.value}"/>
								<label class="${cl || itemCls}" ${titleAttr} for="${id}">${cl ? '' : labelTxt}</label>
							</div>
							`;
						});

						const inputHld = this.el.querySelector(`.${ppfx}field`);
						inputHld.innerHTML = `<div class="${ppfx}radio-items">${inputStr}</div>`;
						this.input = inputHld.firstChild;
					}
				}
			},

			getInputValue() {
				const inputChk = this.getCheckedEl();
				return inputChk ? inputChk.value : '';
			},
			inputradioChanged() {
				var property = this.property;
				if (this.getInputValue() == 'true')
					VjEditor.getSelected().addClass(property);
				else
					VjEditor.getSelected().removeClass(property);
			},

			getCheckedEl() {
				const input = this.getInputEl();
				return input ? input.querySelector('input:checked') : '';
			},

			setValue(value) {

				const model = this.model;
				let val = value || model.get('value') || model.getDefaultValue();
				const input = this.getInputEl();
				const inputIn = input ? input.querySelector(`[value="${val}"]`) : '';

				if (inputIn) {
					inputIn.checked = true;
				} else {
					const inputChk = this.getCheckedEl();
					inputChk && (inputChk.checked = true);
				}
			}
		}
	});

	sm.addType('custom-styleslider', {
		model: {

		},
		view: {
			templateInput(model) {
				const ppfx = this.ppfx;
				const el = document.createElement('div');
				el.setAttribute('class', 'diffmargin');
               

				$(model.attributes.properties).each(function (index, value) {
					const input = document.createElement('input');
					input.setAttribute('type', 'number');
					input.setAttribute('class', 'custominput');
					input.setAttribute('changeProperty', value.CSS);
					input.setAttribute('value', 0);

					const selectunit = document.createElement('select');
					selectunit.setAttribute('class', 'unitselect');

					$(model.attributes.units).each(function (index, value) {
						const unitoption =  document.createElement('option');
						unitoption.innerHTML = value;
						selectunit.appendChild(unitoption);
					});

					const label = document.createElement('label');
					label.innerHTML = value.name;

					const inputdiv = document.createElement('div');
					inputdiv.setAttribute('class', 'inputdiv');

					inputdiv.appendChild(input);
					inputdiv.appendChild(selectunit);
					inputdiv.appendChild(label); 
               

					el.appendChild(inputdiv);
				});
				return `
                        <div class="styleslider-wrapper">
                            
                                <div class="d-flex">
                                    <div class="${ppfx}field ${ppfx}field-range custom-range-sliderInput">
                                        <input class="sliderange" type="range" id="vol" name="vol" min="${model.get('min')}" max="${model.get('max')}" value="${model.get('value')}"></input>
                                    </div>
                                    <div class="${ppfx}field ${ppfx}field-integer custom-range-sliderText">
                                        <input class="rangeinput" type="number" name="rangeinput" value="${model.get('value')}"></input>
                                        <div>
                                            <select class="sliderSelect">
                                                <option>px</option>
                                                <option>%</option>
                                                <option>vh</option>
                                            </select>
                                        </div>
                                    </div>
                                </div>
                                <div class="d-flex justify-content-between">
                                    `+ el.outerHTML + `
                                </div>
                            </div>
                        
                        </div>
                       `;
			},

			events() {
				return {
					'change [type=range]': 'inputValueChanged',
					'input [type=range]': 'inputValueChangedSoft',
					'change [class=rangeinput]': 'inputValueChanged',
					'input [class=rangeinput]': 'inputValueChangedSoft',
					'change [class=custominput]': 'inputMarginChanged',
					'input [class=custominput]': 'inputMarginChanged',
					'change [class=sliderSelect]': 'optionSelectChanged',
					'change [class=unitselect]': 'optionMarginChanged',
					'click [class=gjs-sm-clear]': 'clear',
					change:''
				};
			},

			getSliderEl() {
				if (!this.slider) {
					this.slider = this.el.querySelector('input[type=range]');
				}

				return this.slider;
			},

			clear(ev) {
				ev && ev.stopPropagation();
				this.model.clearValue();
				setTimeout(() => this.targetUpdated());
			},

			inputMarginChanged(){
				var style = VjEditor.getSelected().getStyle();
				var unit = $(event.target.parentNode).find('select').val();
				style[event.target.attributes.changeproperty.value] = event.target.value + unit;
				VjEditor.getSelected().setStyle(style);
			},

			optionMarginChanged(){
				var style = VjEditor.getSelected().getStyle();
				var value = $(event.target.parentNode).find('input').val();
				var unit = event.target.value;
				style[$(event.target.parentNode).find('input').attr('changeproperty')] = value + unit;
				VjEditor.getSelected().setStyle(style);
			},

			optionSelectChanged(){
				const model = this.model;
				const value = this.getInputValue();
				const  unit = this.el.querySelector('.sliderSelect').value;
				this.$el.find('select').val(unit);
				model.set('value', value, { avoidStore: 1 }).set('value', value + unit);

				this.elementUpdated();
			},


			inputValueChanged() {
				const model = this.model;
				const step = model.get('step');

              
				if(event.target.type == 'slider')
					this.getInputEl().value = this.getSliderEl().value;
				else
					this.getInputEl().value = event.target.value;

				const value = this.getInputValue();
				const unit = this.el.querySelector('.sliderSelect').value;
				model.set('value', value, { avoidStore: 1 }).set('value', value + unit);
				this.$el.find('input[type=number]').val(value);
				this.elementUpdated();
			},
			inputValueChangedSoft() {
				if (event.target.type == 'slider')
					this.getInputEl().value = this.getSliderEl().value;
				else
					this.getInputEl().value = event.target.value;

				$( this.model.attributes.properties).each(function (index, value) {
					VjEditor.getSelected().removeStyle(value.CSS);
				});

				this.model.set('value', this.getInputValue() + this.el.querySelector('.sliderSelect').value , { avoidStore: 1 });
				this.$el.find('input[type=number]').val(this.getInputValue());
				this.elementUpdated();
			},

			setValue(value) {
				//const parsed = this.model.parseValue(value);
				const model = this.model;
				const unit = this.el.querySelector('.sliderSelect').value;
				//this.getSliderEl().value = parseFloat(parsed.value);
				this.$el.find('.sliderSelect').val(unit);
				var inputmodel = this.$el;

				var Selected = VjEditor.getSelected();
				if (typeof Selected != "undefined") {

					var compVal = parseInt(VjEditor.getSelected().view.$el.css(this.model.attributes.property));
					this.$el.find('input[type=number]').val(compVal);
					this.$el.find('input[type=range]').val(compVal);

					$(model.attributes.properties).each(function (index, v) {

						if (typeof Selected != "undefined" ){
							var style = Selected.getStyle();
							var styleval = style[v.CSS];
							var stylename =  inputmodel.find('input[changeproperty="'+ v.CSS +'"]');
							if (styleval == undefined) {
								stylename.val(parseInt(VjEditor.getSelected().view.$el.css(v.CSS)));
								stylename.parent().find("select").val(unit);
							}
							else {
								var styleunit = styleval.replace(/[0-9]/g, '');
								stylename.val(parseFloat(styleval));
								stylename.parent().find("select").val(styleunit);
							}
						}
					});
				}

			},

			onRender() {
				if (!this.model.get('showInput')) {
					//this.inputInst.el.style.display = 'none';
				}
			},

			clearCached() {
				this.slider = null;
				this.$el.find('input[type=number]').val('');
				this.$el.find('select').val('');
			}


		},
	})

	sm.addType('custom_slider', {
		model: {

		},
		view: {
			templateInput(model) {
				const ppfx = this.ppfx;
				const el = document.createElement('div');
				el.setAttribute('class', 'd-flex sm-slider-wrapper');

				el.innerHTML = `
					<div class="${ppfx}field ${ppfx}field-range">
						<input type="range" name="` + model.attributes.name + `" step="` + model.attributes.step + `" min="` + model.attributes.min + `" max="` + model.attributes.max + `" class="sm-range" /> 
					</div>
					<div class="${ppfx}field ${ppfx}field-integer">
						<input type="number" name="` + model.attributes.name + `" class="sm-number" />
						<span class="${ppfx}field-units sm-unit-wrapper"></span>
					</div>
				`;

				const select = document.createElement('select');

				if (typeof model.attributes.units != "undefined") {
					$(model.attributes.units).each(function (index, value) {
						var option = document.createElement('option');
						option.setAttribute('class', '${ppfx}input-unit sm-unit-options');
						option.innerHTML = value;
						select.appendChild(option);
					});
				}

				$(el).find(".sm-unit-wrapper").append(select);

				return ``+ el.outerHTML + ``;
			},
			events() {
				return {
					'input': 'inputEvent',
					'click [class=gjs-sm-clear]': 'clear',
				}
			},
			inputEvent() {

				var propArr = [];
				var properties = "";
				var value = event.target.value;
				var unit = this.el.querySelector('select').value;
				var propertyName = this.model.attributes.cssproperty;
				var property = this.model.attributes.property;
				var Selected = VjEditor.getSelected();

				var inputRange = this.el.querySelector('.sm-slider-wrapper .sm-range');
				var inputNumber = this.el.querySelector('.sm-slider-wrapper .sm-number');

				if (event.target.className == "sm-range")
					inputNumber.value = value;
				else if (event.target.className == "sm-number") 
					inputRange.value = value;

				if (propArr.length == 0 && typeof Selected.getStyle()[propertyName] != "undefined") {

					var prop = Selected.getStyle()[propertyName].split(" ");
					
					$(prop).each( function(index, value) {
						if (value != "")
							propArr.push(value + " ");
					});
				}
				
				var search = new RegExp(property , 'i'); 
				let removeItem = propArr.filter(item => search.test(item));
				
				propArr = jQuery.grep(propArr, function(value) {
					return value != removeItem;
				});

				propArr.push(property + "(" + value + unit + ") ");

				$(propArr).each(function(index, value) {
					properties += value;
				});

				var style = Selected.getStyle();
				style[propertyName] = properties;
				Selected.setStyle(style);
				Selected.set(property, event.target.value);
				$(event.target).parents(".gjs-sm-property").find('.gjs-sm-clear').show();
			},
			setValue(value) {
				var model = this.model;
				var val = model.getDefaultValue();
				var Selected = VjEditor.getSelected();

				if (typeof Selected != "undefined" && typeof Selected.attributes[model.attributes.property] != "undefined")
					val = Selected.attributes[this.model.attributes.property];

				this.$el.find('input[type=range]').val(val);
				this.$el.find('input[type=number]').val(val);

				if (value== "true")
					this.$el.find('.gjs-sm-clear').css('display', 'inline-block');
					
			},
			clear(ev) {

				var model = this.model;
				var Selected = VjEditor.getSelected();
				var style = Selected.getStyle();

				model.setValue(model.getDefaultValue());
				$(this.$el).find('input').val(model.getDefaultValue());
				$(this.$el).find('.gjs-sm-clear').hide();

				var string = $.trim(style[model.attributes.cssproperty]),
				preString = this.model.attributes.property,
				preIndex = string.indexOf(preString),
				subString = string.substring(preIndex),
				searchString = subString.slice(subString.indexOf(preString), subString.indexOf(')') + 1).replace(/\s\s+/g, ' ');

				style[model.attributes.cssproperty] = string.replace(searchString, '');
				Selected.setStyle(style);

			},

		}
	})

};
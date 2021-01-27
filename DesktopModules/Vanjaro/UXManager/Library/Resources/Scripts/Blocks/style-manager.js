export default (editor, config = {}) => {

	var sm = editor.StyleManager;

	sm.addType('customradio', {
		create({ props, change }) {

			const el = document.createElement('div');
			el.classList.add("sm-radio-wrapper");

			$(props.list).each(function (index, item) {

				var input = document.createElement('input');
				var label = document.createElement("label");

				input.setAttribute("type", "radio");
				input.setAttribute("name", props.property);
				input.setAttribute("id", item.name + '-' + props.property);
				input.setAttribute("value", item.value);

				label.setAttribute("for", item.name + '-' + props.property);

				if (typeof item.img != 'undefined') {

					var img = document.createElement("img");

					img.setAttribute("src", VjDefaultPath + item.img);
					label.appendChild(img);
				}
				else
					label.innerHTML = item.name;

				el.appendChild(input);
				el.appendChild(label);

				input.addEventListener('change', event => change({ event }));
			});

			return el;
		},
		emit({ props, updateStyle }, { event, complete }) {

			var model = this.model;
			var selected = VjEditor.getSelected();
			var val = event.target.value;
			var property = model.attributes.property;

			if (model.attributes.UpdateStyles) {
				var style = selected.getStyle();
				style[property] = event.target.value;
				selected.setStyle(style);
			}
			else {
				var classes = model.attributes.list.map(opt => opt.value);

				$(classes).each(function (index, className) {
					selected.removeClass(className);
				});

				selected.addClass(val);
			}

			selected.set(property, val);

			model.setValue(val);
		},
		setValue(value) {
			var model = this.model;
			model.view.$el.find('input[value="' + value + '"]').prop('checked', true);

			if (value == model.getDefaultValue())
				$(this.$el).find('.gjs-sm-clear').hide();
			else
				$(this.$el).find('.gjs-sm-clear').show();
		},
		clear() {

			var model = this.model;
			var selected = VjEditor.getSelected();
			var property = model.attributes.property;
			var defaultValue = model.getDefaultValue();

			$(this.$el).find('.gjs-sm-clear').hide();
			model.view.$el.find('input').prop('checked', false);
			model.view.$el.find('input[value="' + defaultValue + '"]').prop('checked', true);

			if (model.attributes.UpdateStyles)
				selected.removeStyle(property);
			else {
				var classes = model.attributes.list.map(opt => opt.value);

				$(classes).each(function (index, className) {
					selected.removeClass(className);
				});
			}

			selected.set(property, defaultValue);
		}
	});

	sm.addType('custom_slider', {
		model: {},
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

				return `` + el.outerHTML + ``;
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

					$(prop).each(function (index, value) {
						if (value != "")
							propArr.push(value + " ");
					});
				}

				var search = new RegExp(property, 'i');
				let removeItem = propArr.filter(item => search.test(item));

				propArr = jQuery.grep(propArr, function (value) {
					return value != removeItem;
				});

				propArr.push(property + "(" + value + unit + ") ");

				$(propArr).each(function (index, value) {
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

				if (value == "true")
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
	});

	var LoadAttr = function (model, unit) {

		var inputControl = model.view.el.querySelectorAll('.sm-input-control.range');

		$(model.attributes.units).each(function (index, option) {
			if (option.name == unit) {
				$(inputControl).attr({
					'min': option.min,
					'max': option.max,
					'step': option.step
				});
			}
		});
	}

	sm.addType('customrange', {
		create({ props, change }) {

			const el = document.createElement('div');
			el.classList.add("sm-range-wrapper");

			el.innerHTML = `
				<input type="range" value="`+ props.defaults + `" name="` + props.property + `" min="` + props.min + `" max="` + props.max + `" class="sm-input-control range" /> 
				<input type="text" value="`+ props.defaults + `" name="` + props.property + `" class="sm-input-control text" />
				<span class="sm-unit-wrapper"></span>
			`;

			var inputRange = el.querySelector('.range');
			var inputText = el.querySelector('.text');

			inputRange.addEventListener('change', event => change({ event }));
			inputRange.addEventListener('input', event => change({ event }));
			inputText.addEventListener('change', event => change({ event }));
			inputText.addEventListener('input', event => change({ event }));
			inputText.addEventListener('keydown', event => change({ event }));

			if (typeof props.units != "undefined" && props.units.length) {

				var select = document.createElement('select');
				select.setAttribute("class", "unit-list");
				select.setAttribute("name", props.property);

				$(props.units).each(function (index, item) {
					var option = document.createElement("option");
					option.setAttribute("name", item.name);
					option.setAttribute("value", item.name);
					option.text = item.name;
					select.append(option);
				})

				$(el).find(".sm-unit-wrapper").append(select);

				select.addEventListener('change', event => change({ event }));
			}
			else if (typeof props.unit != 'undefined')
				$(el).find(".sm-unit-wrapper").append(props.unit);

			return el;
		},
		emit({ props, updateStyle }, { event, complete }) {

			var selected = editor.getSelected(),
				model = this.model,
				property = model.attributes.property,
				val = '',
				unit = '',
				style = selected.getStyle();

			var inputRange = model.view.el.querySelector('.sm-range-wrapper .range');
			var inputText = model.view.el.querySelector('.sm-range-wrapper .text');

			if (model.view.$el.find('select').prop('disabled')) {

				unit = 'px';
				model.view.$el.find('select').prop('disabled', false);
			}

			if (event.target.classList.contains('range') || event.target.classList.contains('text')) {

				val = event.target.value;

				if (typeof model.attributes.units != 'undefined')
					unit = model.view.$el.find('select').val();

				if (val == 'auto') {
					if (typeof selected != 'undefined') {
						if (property == 'width')
							val = parseInt(selected.view.$el.css('width'));
						else if (property == 'height')
							val = parseInt(selected.view.$el.css('height'));
						else
							val = '0';

						unit = 'px';
					}
				}

				if (unit == null || unit == '') {
					if (typeof model.attributes.unit != 'undefined')
						unit = model.attributes.unit;
					else
						unit = '';
				}

				if (event.keyCode === 38)
					val = parseInt(val) + 1;
				else if (event.keyCode === 40)
					val = parseInt(val) - 1;

			}
			else if (event.target.classList.contains('unit-list')) {
				val = inputText.value;
				unit = event.target.value;
				LoadAttr(model, unit);

			}

			inputRange.value = inputText.value = val;

			style[property] = val + unit;
			selected.setStyle(style);

			model.setValue(val + unit);
		},
		setValue(value) {

			var selected = editor.getSelected(),
				model = this.model,
				unit = '';

			if (value == '')
				value = model.getDefaultValue();

			var inputvalue = value;

			if (typeof selected != 'undefined') {

				if (value == 'auto')
					model.view.$el.find('select').prop('disabled', 'disabled');
				else {

					if (typeof value == "string") {
						inputvalue = value.replace(/[^-\d\.]/g, '');

						if (typeof model.attributes.units != 'undefined') {
							$(model.attributes.units).each(function (index, option) {

								if (value.indexOf(option.name) >= 0) {
									unit = option.name
									return false;
								}
							});
						}
					}

					if (unit == '' && typeof model.attributes.unit != 'undefined')
						unit = model.attributes.unit;

					if (typeof model.attributes.units != 'undefined')
						LoadAttr(model, unit);

					model.view.$el.find('select').prop('disabled', false);
				}
			}

			model.view.$el.find('input').val(inputvalue);
			model.view.$el.find('select').val(unit);

			if (value == model.getDefaultValue())
				$(this.$el).find('.gjs-sm-clear').hide();
			else
				$(this.$el).find('.gjs-sm-clear').show();

		},
		clear() {

			var model = this.model,
				property = model.attributes.property,
				value = model.getDefaultValue(),
				unit = '';

			$(this.$el).find('.gjs-sm-clear').hide();

			if (value == 'auto')
				model.view.$el.find('select').prop('disabled', 'disabled');
			else {
				if (typeof model.attributes.unit != 'undefined') {

					unit = model.attributes.unit;

					if (typeof model.attributes.units != 'undefined')
						LoadAttr(model, unit);
				}
			}

			model.view.$el.find('input').val(value);
			model.view.$el.find('select').val(unit);

			var selected = VjEditor.getSelected();
			var style = selected.getStyle();
			style[property] = value + unit;

			selected.setStyle(style);

			if (property == "border-width" || property == "border-top-width" || property == "border-right-width" || property == "border-bottom-width" || property == "border-left-width") {

				selected.removeStyle(property);

				var Border = VjLocalized.Border.replace(/ /g, '_').toLowerCase();
				if (typeof style['border-width'] != 'undefined')
					VjEditor.StyleManager.getProperty(Border, property).setValue(style['border-width']);
				else
					VjEditor.StyleManager.getProperty(Border, property).setValue('3px');
			}
		}
	});
};

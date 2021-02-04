/*
This is a plugin to wrap vanjaro blocks
*/
import grapesjs from 'grapesjs';

export default grapesjs.plugins.add('blockwrapper', (editor, opts = {}) => {
    var comps = editor.DomComponents;
    var defaultType = comps.getType('default');
    var defaultModel = defaultType.model;
    var defaultView = defaultType.view;

    comps.addType('blockwrapper', {
        extendFn: ['initToolbar'],
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

                    if (model.attributes.attributes["data-block-allow-customization"] != undefined && model.attributes.attributes["data-block-allow-customization"] == "true") {
                        tb.push({
                            attributes: { class: 'fa fa-cog', guid: ((typeof model.attributes.attributes["data-block-setting-guid"] != 'undefined') ? model.attributes.attributes["data-block-setting-guid"] : model.attributes.attributes["data-block-guid"]), width: model.attributes.attributes["data-block-width"], title: VjLocalized.Settings },
                            command: 'tlb-vjblock-setting',
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
                name: '',
                traits: [],
                removable: true,
                draggable: true,
                droppable: false,
                badgable: true,
                stylable: true,
                highlightable: true,
                copyable: false,
                editable: false,
                layerable: true,
                selectable: true,
                hoverable: true
            }),
            init() {
                //Trait - Alignment
                if (typeof this.getTrait() == "undefined" && typeof this.getAttributes()["data-block-alignment"] != 'undefined' && this.getAttributes()["data-block-alignment"] == "true") {
                    //this.setStyle({ "display": "inline-block" });
                    this.addTrait({
                        label: "Alignment",
                        type: "toggle_checkbox",
                        name: "alignment",
                        UpdateStyles: true,
                        options: [
                            { id: 'left', name: 'left', image: 'align-left' },
                            { id: 'center', name: 'center', image: 'align-center' },
                            { id: 'right', name: 'right', image: 'align-right' },
                        ],
                        value: "none"
                    });
                }

                //Resizable
                if (typeof this.getAttributes()["data-block-resizable"] != 'undefined' && this.getAttributes()["data-block-resizable"] == "true") {
                    this.set({
                        'resizable': {
                            ratioDefault: 1,
                            tc: 0,
                            cl: 0,
                            cr: 0,
                            bc: 0,
                            onMove: function (e) {
                                var SelectedCol = VjEditor.getSelected();
                                if (SelectedCol.getName() == 'Logo') {
                                    var width = $(SelectedCol.getEl()).width();
                                    var height = $(SelectedCol.getEl()).height();
                                    var attr = SelectedCol.getAttributes();
                                    attr['data-style'] = 'width:' + width + 'px; height:' + height + 'px;';
                                    SelectedCol.setAttributes(attr);
                                    $(SelectedCol.getEl()).find('img').css('width', width).css('height', height);
                                    SelectedCol.removeStyle('width');
                                    SelectedCol.removeStyle('height');
                                }
                            },
                            onEnd: function (e) {
                                var SelectedCol = VjEditor.getSelected();
                                if (SelectedCol.getName() == 'Logo') {
                                    SelectedCol.removeStyle('width');
                                    SelectedCol.removeStyle('height');
                                    var img = SelectedCol.getEl().innerHTML;
                                    SelectedCol.set('content', img);
                                }
                            }
                        }
                    });
                }
            },
        },
            {
                isComponent: function (el) {
                    if (el.tagName == 'div' || el.tagName == 'DIV' && (el.attributes["data-block-type"] != undefined)) {
                        return { type: 'blockwrapper' };
                    }
                }
            }),
        view: defaultView.extend({
            render: function () {
                defaultType.view.prototype.render.apply(this, arguments);
                this.model.set('custom-name', this.model.attributes.attributes["data-block-display-name"] != undefined ? this.model.attributes.attributes["data-block-display-name"] : this.model.attributes.attributes["data-block-type"]);
                this.model.set('name', this.model.attributes.attributes["data-block-display-name"] != undefined ? this.model.attributes.attributes["data-block-display-name"] : this.model.attributes.attributes["data-block-type"]);
                if (this.model.attributes != undefined && this.model.attributes.components != undefined && this.model.attributes.components.models[0] != undefined && this.model.attributes.components.models[0].attributes.content != '') {

                    var compHtml = null;
                    if (this.model.attributes.attributes["data-block-type"].toLowerCase() == 'logo' && !this.model.attributes.components.models[0].attributes.content.startsWith('<div data-block'))
                        compHtml = this.model.attributes.components.models[0].attributes.content;
                    else
                        compHtml = $(this.model.attributes.components.models[0].attributes.content).html();

                    this.model.components('');
                    this.model.set('content', CleanGjAttrs(compHtml));

                    this.model.set({
                        removable: true,
                        draggable: true,
                        droppable: false,
                        badgable: true,
                        stylable: true,
                        highlightable: true,
                        copyable: false,
                        editable: false,
                        layerable: true,
                        selectable: true,
                        hoverable: true
                    });
                    $.each(getAllComponents(this.model), function (k, v) {
                        v.set({
                            removable: false,
                            draggable: true,
                            droppable: false,
                            badgable: false,
                            stylable: false,
                            highlightable: false,
                            copyable: false,
                            editable: false,
                            layerable: false,
                            selectable: false,
                            hoverable: false
                        });
                    });
                }
                return this;
            },
            events: {
                dblclick: function () {
                    if (this.model.attributes.attributes["data-block-type"] == "Logo" && this.model.attributes.attributes["data-block-setting-guid"] != 'undefined') {
                        var guid = this.model.attributes.attributes["data-block-setting-guid"];
                        var url = CurrentExtTabUrl + "&guid=" + guid;
                        var width = 500;
                        if (typeof this.model.attributes.attributes["data-block-width"] != 'undefined' && this.model.attributes.attributes["data-block-width"] != null) {
                            width = parseInt(this.model.attributes.attributes["data-block-width"]);
                        }
                        OpenPopUp(null, width, 'right', '', url);
                    }
                }
            }
        }),
    });

    comps.addType('globalblockwrapper', {
        model: defaultModel.extend({
            initToolbar() {
                var model = this;
                if (!model.get('toolbar')) {

                    var GetBlockMenus = function () {
                        var Result = [];
                        Result.push({ 'Title': 'Unlink from Global', 'Command': 'custom-block-globaltolocal' });
                        return Result;
                    };

                    var tb = [];

                    if (GetBlockMenus().length > 0) {
                        tb.push({
                            attributes: { class: 'fa fa-bars' },
                            command: function (t) {
                                return t.runCommand("tlb-app-actions", {
                                    BlockMenus: GetBlockMenus()
                                })
                            }
                        });
                    }

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
                name: '',
                droppable: IsAdmin,
                highlightable: IsAdmin,
                copyable: false,
                resizable: false,
                editable: IsAdmin,
                layerable: IsAdmin,
                selectable: true,
                hoverable: IsAdmin
            }),
        },
            {
                isComponent: function (el) {
                    if (el.tagName == 'div' || el.tagName == 'DIV' && (el.attributes["data-block-type"] != undefined && el.attributes["data-block-type"].value.toLowerCase() == 'global')) {
                        return {
                            type: 'globalblockwrapper'
                        };
                    }
                }
            }),
        view: defaultType.view.extend({
            render: function () {
                defaultType.view.prototype.render.apply(this, arguments);
                if (this.model.attributes != undefined && this.model.attributes.components != undefined && this.model.attributes.components.models[0] != undefined && this.model.attributes.components.models[0].attributes.content != '') {
                    var contentitems = $(this.model.attributes.components.models[0].attributes.content);
                    if (contentitems != undefined && contentitems.length > 0) {
                        var contentmarkup = '';
                        var contentcss = '';
                        $.each(contentitems, function (ind, itm) {
                            if (itm.localName == 'style')
                                contentcss += itm.outerHTML;
                            else
                                contentmarkup += itm.innerHTML;
                        });
                        this.model.components(contentmarkup);
                        this.model.view.$el[0].innerHTML += contentcss;
                    }
                    $.each(getAllComponents(this.model), function (k, v) {
                        if (v.attributes.type == 'blockwrapper') {
                            v.set('custom-name', v.attributes.attributes["data-block-display-name"] != undefined ? v.attributes.attributes["data-block-display-name"] : v.attributes.attributes["data-block-type"]);
                            v.set('name', v.attributes.attributes["data-block-display-name"] != undefined ? v.attributes.attributes["data-block-display-name"] : v.attributes.attributes["data-block-type"]);

                            var compHtml = null;
                            if ($(v.view.$el[0].innerHTML).attr('data-gjs-type') == 'blockwrapper')
                                compHtml = $(v.view.$el[0].innerHTML).html();
                            else
                                compHtml = v.view.$el[0].innerHTML;

                            v.components('');
                            v.set('content', CleanGjAttrs(compHtml));


                            if (v.attributes.attributes["data-block-type"].toLowerCase() == "logo") {
                                var style = v.attributes.attributes["data-style"];
                                if (style != undefined) {
                                    $(v.view.$el[0]).find('img').attr('style', style);
                                }
                            }

                            $.each(getAllComponents(v), function (k, vv) {
                                if (vv.attributes.type != 'blockwrapper') {
                                    vv.set({
                                        isblockwrappertype: true,
                                        removable: false,
                                        draggable: true,
                                        droppable: false,
                                        badgable: false,
                                        stylable: false,
                                        highlightable: false,
                                        copyable: false,
                                        editable: false,
                                        layerable: false,
                                        selectable: false,
                                        hoverable: false
                                    });
                                }
                            });
                        }
                    });
                }
                else {
                    $.each(getAllComponents(this.model), function (k, v) {
                        if (v.attributes.type == 'blockwrapper' && v.attributes.attributes["data-block-type"].toLowerCase() == "logo") {
                            var style = v.attributes.attributes["data-style"];
                            if (style != undefined) {
                                $(v.view.$el[0]).find('img').attr('style', style);
                            }
                        }
                    })
                }
                StyleGlobal(this.model);
                return this;
            },
        })
    });
});
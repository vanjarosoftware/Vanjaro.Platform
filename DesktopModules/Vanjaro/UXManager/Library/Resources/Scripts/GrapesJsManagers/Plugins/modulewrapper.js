/*
This is a plugin to wrap desktop modules and visualizer apps
while drag and drop on page so that no grapjs editing can
be done on child nodes on main module wrapper div.
*/
import grapesjs from 'grapesjs';

export default grapesjs.plugins.add('modulewrapper', (editor, opts = {}) => {
    var comps = editor.DomComponents;
    var defaultType = comps.getType('default');
    var defaultModel = defaultType.model;
    var defaultView = defaultType.view;

    comps.addType('modulewrapper', {
        extendFn: ['initToolbar'],
        model: defaultModel.extend({
            initToolbar(reinit) {
                var model = this;
                if (!model.get('toolbar') || reinit) {
                    var tb = [];
                    if (model.getAttributes().mid != '') {
                        var AppMenusScript = $('[data-actionmid=' + model.getAttributes().mid + ']');
                        if (AppMenusScript.length > 0) {

                            tb.push({
                                attributes: { class: 'fa fa-bars', title: VjLocalized.Menu},
                                command: function (t) {
                                    return t.runCommand("tlb-app-actions", {
                                        BlockMenus: jQuery.parseJSON(AppMenusScript.html())
                                    })
                                }
                            });
                        }
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
                    if (AppMenus != undefined) {
                        $.each(AppMenus, function (k, v) {
                            tb.push({
                                attributes: { class: v.Class, guid: v.ItemGuid, width: v.Width, title: VjLocalized.Menu },
                                command: v.Command,
                            });
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
                stylable: false,
                highlightable: true,
                copyable: false,
                resizable: false,
                editable: true,
                layerable: true,
                selectable: true,
                hoverable: true
            })
        },
            {
                isComponent: function (el) {
                    if (el.tagName == 'div' || el.tagName == 'DIV' && (el.attributes.dmid != undefined)) {
                        return { type: 'modulewrapper' };
                    }
                }
            })
    });

    comps.addType('module', {
        model: defaultModel.extend({
            defaults: Object.assign({}, defaultModel.prototype.defaults, {
                removable: false,
                draggable: false,
                droppable: false,
                badgable: false,
                stylable: false,
                highlightable: false,
                copyable: false,
                resizable: false,
                editable: true,
                layerable: false,
                selectable: false,
                hoverable: false,
                propagate: ['removable', 'draggable', 'droppable', 'badgable', 'stylable', 'highlightable', 'copyable', 'resizable', 'editable', 'layerable', 'selectable', 'hoverable']
            })
        },
            {
                isComponent: function (el) {
                    if (el != undefined && el.attributes != undefined && el.attributes.vjmod != undefined && el.attributes.vjmod.value == 'true') {
                        return { type: 'module' };
                    }
                }
            }),
        view: defaultType.view.extend({
            render: function () {
                defaultType.view.prototype.render.apply(this, arguments);
                this.model.attributes.components.models = [];
                var parent = this.model.parent();
                var parentelement = parent.getEl();
                if ($(parentelement).attr('data-appname') != undefined)
                    parent.set('name', VjLocalized.PrefixAppName + $(parentelement).attr('data-appname'));
                return this;
            }
        })
    });
});
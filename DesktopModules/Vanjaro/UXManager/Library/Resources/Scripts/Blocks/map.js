export default (editor, config = {}) => {
    const c = config;
    let bm = editor.BlockManager;

    if (c.blocks.map) {
        bm.add('map', {
            label: VjLocalized.Map,
            category: VjLocalized.Basic,
            attributes: { class: 'fa fa-map-o' },
            content: {
                type: 'map',
                classes: ['vj-map'],
                attributes: { title: 'Map' },
                zoom: 10,
            },
        });
    }

    var domComps = editor.DomComponents;
    var dType = domComps.getType('map');
    var dModel = dType.model;
    var dView = dType.view;

    domComps.addType('map', {
        model: dModel.extend({
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
        }),
        view: dView.extend({
            events: {
                click: function () {
                    setTimeout(function () {
                        var MapTimeOutid;
                        $('.traitsmanager .gjs-field-text input').on('keyup', function () {
                            if (MapTimeOutid) {
                                clearTimeout(MapTimeOutid);
                            }
                            MapTimeOutid = setTimeout(function () {
                                $('.traitsmanager .gjs-field-select select').focus();
                                $('.traitsmanager .gjs-field-text input').focus();
                            }, 500);
                        });
                    }, 500);
                }
            }
        })
    });
}
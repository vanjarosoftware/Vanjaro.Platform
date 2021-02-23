window.VJCustomBlockTimeOutid;
window.VJRenderBlockTimer = null;
//Get Desktop Modules and Visualizer apps using api call and create blocks
global.LoadApps = function () {
    var sf = $.ServicesFramework(-1);
    $.ajax({
        type: "Get",
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Page/GetApps",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $.each(data, function (key, value) {
                VjEditor.BlockManager.add(value.ModuleName, {
                    category: VjLocalized.Apps,
                    attributes: { type: 'apps' },
                    label: '<div><img style="width:32px;height:32px;" src="' + value.ModuleImage + '"/><div class="my-label-block">' + value.ModuleName + '</div></div>',
                    content: '<div dmid="' + value.ModuleID + '" mid="" uid="' + value.UniqueID + '" fname="' + value.ModuleName + '"><div vjmod="true">[Module]</div></div>'
                })
            });
            if (VjEditor != undefined && VjEditor.Canvas != undefined && VjEditor.Canvas.getBody() != undefined) {
                const body = VjEditor.Canvas.getBody();
                $(body).append('<script>' + VjScript + '</script>');
                if (VjStyle != undefined && VjStyle.length > 0)
                    $(body).append('<style>' + VjStyle + '</style>');
            }
            else {
                setTimeout(function () {
                    const body = VjEditor.Canvas.getBody();
                    $(body).append('<script>' + VjScript + '</script>');
                    if (VjStyle != undefined && VjStyle.length > 0)
                        $(body).append('<style>' + VjStyle + '</style>');
                }, 2000);
            }
        }
    });
};

global.getAllComponents = function (component) {
    component = component || VjEditor.DomComponents.getWrapper();

    var components = component.get("components").models;
    component.get("components").forEach(function (component) {
        components = components.concat(getAllComponents(component));
    });
    return components;
};

global.LoadCustomBlocks = function () {

    var sf = $.ServicesFramework(-1);

    $.ajax({
        type: "Get",
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/GetAllCustomBlock",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $.each(data, function (key, value) {
                var Content = '';
                if (value.IsGlobal && !Content.includes("7a4be0f2-56ab-410a-9422-6bc91b488150"))
                    Content = "<div data-block-type=\"global\" data-block-guid=\"7a4be0f2-56ab-410a-9422-6bc91b488150\" data-guid=\"" + value.Guid + "\"></div>";
                else
                    Content = "<div data-custom-wrapper=\"true\" data-guid=\"" + value.Guid + "\"></div>";
                VjEditor.BlockManager.add(value.Name, {
                    attributes: { class: 'fa fa-th-large', id: value.ID, type: 'VjCustomBlock', isGlobalBlock: value.IsGlobal, guid: value.Guid },
                    label: value.Name,
                    category: value.Category,
                    content: Content,
                    render: ({ el }) => {
                        const updateblock = document.createElement("span");
                        updateblock.className = "update-custom-block";
                        if (IsAdmin)
                            updateblock.innerHTML = "<div class='addpage-blocks dropdown pull-right dropbtn'><a id='dropdownMenuLink' class='dropdownmenu Customblockdropdown' data-toggle='dropdown' aria-haspopup='true'  aria-expanded='false'><em class='fas fa-ellipsis-v'></em></a><ul class='dropdown-menu' aria-labelledby='dropdownMenuLink'><li><a class='edit-block' onclick='VjEditor.runCommand(\"edit-custom-block\", { name: \"" + value.Name + "\" ,id: \"" + value.Guid + "\" })'><em class='fas fa-pencil-alt'></em><span>Edit</span></a></li><li><a class='export-block' onclick='VjEditor.runCommand(\"export-custom-block\",{ name: \"" + value.Name + "\" , id: \"" + value.Guid + "\", IsGlobal: \"" + value.IsGlobal + "\" })'><em class='fas fa-file-export'></em><span>Export</span></a></li><li><a class='delete-block' onclick='VjEditor.runCommand(\"delete-custom-block\",{ name: \"" + value.Name + "\" ,id: \"" + value.Guid + "\" })'><em class='fas fa-trash-alt'></em><span>Delete</span></a></li></ul></div>";
                        el.appendChild(updateblock);
                    }
                });
            })
            ChangeBlockType();
        }
    });
}

global.LoadDesignBlocks = function () {

    var sf = $.ServicesFramework(-1);

    $.ajax({
        type: "Get",
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/GetAll",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $.each(data, function (key, value) {
                VjEditor.BlockManager.add(value.Name, {
                    category: value.Attributes["data-block-category"] != undefined ? value.Attributes["data-block-category"] : value.Category,
                    attributes: { type: 'blockext' },
                    label: '<div class="' + value.Icon + '"><div class="my-label-block">' + value.DisplayName + '</div></div>',
                    content: GetBlockContent(value)
                })
            })
        }
    });
}

var GetBlockContent = function (Block) {
    var result = "<div";
    $.each(Block.Attributes, function (key, value) {
        result += ' ' + key + '="' + value + '"';
    });
    result += ">" + Block.Name + "</div>";
    return result;
}

global.AddCustomBlock = function (editor, CustomBlock) {
    var ContentJSON = JSON.stringify(VjEditor.getSelected().toJSON());
    if (!ContentJSON.startsWith('['))
        ContentJSON = '[' + ContentJSON + ']';
    CustomBlock.ContentJSON = ContentJSON;
    CustomBlock.StyleJSON = JSON.stringify(VjEditor.Css.getAll().toJSON());
    var sf = $.ServicesFramework(-1);
    if (!CustomBlock.IsGlobal) {
        CustomBlock.Html = '';
        CustomBlock.Css = '';
    }
    $.ajax({
        type: "POST",
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/AddCustomBlock",
        data: CustomBlock,
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        success: function (response) {
            if (response.Status == "Success") {
                editor.Modal.close();
                $("#ModalContent").hide();
                CustomBlock.Guid = response.Guid;
                AddCustom_Block(CustomBlock, response.ID, response.Guid);
                ChangeBlockType();
            }
            else if (response.Status == "Exist")
                swal(CustomBlock.Name + " " + VjLocalized.AlreadyExist);
            else
                swal(response.Status);
        }
    });
}

var AddCustom_Block = function (CustomBlock, ID, Guid) {
    var blocksToRemove = [];
    $.each(VjEditor.BlockManager.getAll().models, function (key, value) {
        if (value != undefined && value.attributes != undefined && value.attributes.attributes != undefined && value.attributes.attributes.guid != undefined && value.attributes.attributes.type == 'VjCustomBlock')
            blocksToRemove.push(value.cid);
    })
    $.each(blocksToRemove, function (key, value) {
        VjEditor.BlockManager.remove(value);
    })
    LoadCustomBlocks();
}

global.UpdateCustomBlock = function (editor, CustomBlock) {
    var sf = $.ServicesFramework(-1);
    var EditBlock = 'EditCustomBlock';

    if (CustomBlock.IsGlobal)
        EditBlock = 'EditGlobalBlock';

    $.ajax({
        type: "POST",
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/" + EditBlock,
        data: CustomBlock,
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        success: function (response) {
            if (response.Status == "Success") {
                editor.Modal.close();
                $("#ModalContent").hide();
                var cid;
                $.each(VjEditor.BlockManager.getAll().models, function (key, value) {
                    if (value.attributes.attributes.id != undefined && value.attributes.attributes.id == CustomBlock.ID && value.attributes.attributes.type == 'VjCustomBlock') {
                        cid = value.cid;
                    }
                })
                VjEditor.BlockManager.remove(cid);
                CustomBlock.Guid = response.Guid;
                CustomBlock = response.CustomBlock;
                AddCustom_Block(CustomBlock, CustomBlock.ID, CustomBlock.Guid);
                //Render Block Manager for updating Categories
                ChangeBlockType();
            }
            else
                swal(response.Status);
        }
    });
}

global.DeleteCustomBlock = function (block) {
    var sf = $.ServicesFramework(-1);
    var ApiMethod = 'DeleteCustomBlock';

    if (Block.IsGlobal)
        ApiMethod = 'DeleteGlobalBlock';

    $.ajax({
        type: "POST",
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/" + ApiMethod + "?" + "CustomBlockGuid=" + block.id,
        //data: CustomBlock,
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        success: function (response) {
            if (response.Status == "Success") {
                $.each(VjEditor.BlockManager.getAll().models, function (key, value) {
                    if (value.attributes.attributes.guid != undefined && value.attributes.attributes.guid == CustomBlockGuid && value.attributes.attributes.type == 'VjCustomBlock')
                        VjEditor.BlockManager.remove(value.cid);
                })
                //Render Block Manager for updating Categories
                ChangeBlockType();
            }
            else
                swal(response);
        }
    });
}

global.ExportCustomBlock = function (CustomBlockGuid) {
    $.ajax({
        type: 'GET',
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + "Block/ExportCustomBlock?CustomBlockGuid=" + CustomBlockGuid,
        dataType: 'binary',
        xhrFields: {
            responseType: 'arraybuffer'
        },
        headers: {
            'ModuleId': -1,
            'TabId': parseInt($.ServicesFramework(-1).getTabId()),
            'RequestVerificationToken': $.ServicesFramework(-1).getAntiForgeryValue()
        },
        success: function (data, status, headers) {
            var arr = headers.getAllResponseHeaders().split('\r\n');
            var headerslist = arr.reduce(function (acc, current, i) {
                var parts = current.split(': ');
                acc[parts[0]] = parts[1];
                return acc;
            }, {});
            var filename = headerslist['x-filename'];
            var contentType = headerslist['content-type'];
            var linkElement = document.createElement('a');
            try {
                var blob = new Blob([data], { type: contentType });
                var url = window.URL.createObjectURL(blob);
                linkElement.setAttribute('href', url);
                linkElement.setAttribute("download", filename);
                var clickEvent = new MouseEvent("click", {
                    "view": window,
                    "bubbles": true,
                    "cancelable": false
                });
                linkElement.dispatchEvent(clickEvent);
            } catch (ex) {
                alert(ex);
            }
        },
        error: function (data) {
            alert(data);
        }
    });
};

global.GetGlobalBlockName = function (guid) {
    var result = '';
    $.each(VjEditor.BlockManager.getAll().models, function (k, v) {
        if (v.attributes != undefined && v.attributes.attributes != undefined && v.attributes.attributes.guid != undefined && v.attributes.attributes.guid == guid)
            result = v.attributes.label;
    });
    return result;
}

global.StyleGlobal = function (model) {
    var modelEl = $(model.getEl());
    if (!modelEl.find('global-tools').length)
        modelEl.append('<div class="global-tools"><div class="backdrop"></div><em title="Global" class="fa fa-globe"></em><div class="toolbar"><em title="Unlock" class="fa fa-unlock" onclick="window.parent.UnlockGlobalBlock($(this))"></em><em title="revisions" class="fa fa-history" onclick="window.parent.ViewBlockRevisions(\'' + model.attributes.attributes["data-guid"] + '\')"></em><em title="Unlink from Global" class="fa fa-unlink" onclick="window.parent.VjEditor.runCommand(\'custom-block-globaltolocal\')"></em><em title="Move" class="fa fa-arrows" onclick="window.parent.VjEditor.runCommand(\'tlb-move\')"></em><em title="Delete" class="fa fa-trash-o" onclick="window.parent.VjEditor.runCommand(\'global-delete\')"></em>');
}

global.UpdateGlobalBlock = function (model) {
    if (model != undefined) {
        try {
            if (model.attributes != undefined)
                model.attributes.content = '';
            var content = VjEditor.runCommand("export-component", {
                component: model.attributes.components.models[0]
            });
            if (content != undefined && content.html != undefined && content.html != "" && $(content.html)[0].innerHTML != "") {
                var Block = VjEditor.BlockManager.get(GetGlobalBlockName(model.attributes.attributes['data-guid']));
                if (Block != undefined) {
                    var CustomBlock = {
                        ID: Block.attributes.attributes.id,
                        Guid: Block.attributes.attributes.guid,
                        Name: Block.attributes.label,
                        Category: Block.attributes.category.id || Block.attributes.category,
                        Html: content.html,
                        Css: content.css,
                        IsGlobal: true
                    };
                    UpdateCustomBlock(VjEditor, CustomBlock);
                }
            }
        }
        catch (err) {
            console.log(err);
        }
    }
}

global.BuildAppComponent = function (vjcomps) {
    $.each(vjcomps, function (k, v) {
        if (v.attributes != undefined && v.attributes.mid != undefined && v.attributes.mid != '') {
            if (v.components[0] != undefined) {
                if ($('#dnn_vj_' + v.attributes.mid)[0] != undefined) {
                    if ($('#dnn_vj_' + v.attributes.mid).find('[ng-controller="Controller"]').length > 0 && $('#dnn_vj_' + v.attributes.mid).find('[ng-controller="Controller"]').html().indexOf('ngView:') > 0)
                        v.components[0].content = "<div class='alert alert-info' role='alert'>" + v.attributes.fname + " will appear here when this page is previewed or published.</div>";
                    else
                        v.components[0].content = $('#dnn_vj_' + v.attributes.mid)[0].outerHTML;
                }
                else {
                    v.include = false;
                }
            }

        }
        else if (v.components != undefined) {
            BuildAppComponent(v.components);
        }
    });
};

global.BuildAppComponentFromHtml = function (vjcomps, html) {
    var mids = [];
    var vjcompsMids = [];

    var dom = $(html);
    $.each(dom, function (k, com) {
        if ($(com).attr('mid') != undefined && $(com).attr('mid').length > 0) {
            mids.push(parseInt($(com).attr('mid')));
        }
    });
    //vjcomps's mids
    $.each(vjcomps, function (x, com) {
        if (com.attributes != undefined && com.attributes.mid != undefined) {
            vjcompsMids.push(parseInt(com.attributes.mid));
        }
    });

    $.each(mids, function (k, mid) {

        if (vjcompsMids.indexOf(mid) === -1) {
            $.each(dom, function (k, com) {
                if ($(com).attr('mid') == mid) {

                    var appName = "";
                    var classes = $($('#dnn_vj_' + mid)[0].outerHTML).find('.DnnModule').attr('class');
                    if (classes != undefined) {
                        $.each(classes.split(' '), function (k, v) {
                            if (v.toLowerCase().startsWith('dnnmodule-') && !$.isNumeric(v.toLowerCase().split('dnnmodule-')[1]))
                                appName = "App: " + v.replace('dnnmodule-', '').replace('DnnModule-', '');
                        });
                    }

                    var addModuleToVjComps = {
                        type: "modulewrapper",
                        name: appName,
                        content: "",
                        attributes: {
                            dmid: $(com).attr('dmid'),
                            mid: mid,
                            uid: $(com).attr('uid'),
                            id: $(com).attr('id'),
                            fname: $(com).attr('fname')
                        },
                        components: [{
                            type: "module",
                            content: $('#dnn_vj_' + mid)[0].outerHTML,
                            attributes: { vjmod: "true" }
                        }],
                        open: false
                    }
                    if (vjcomps != undefined)
                        vjcomps.push(addModuleToVjComps);
                }
            });
        }
    });
};

global.BuildBlockComponent = function (vjcomps) {
    $.each(vjcomps, function (k, v) {
        if (v.attributes != undefined && v.attributes["data-block-guid"] != undefined && v.attributes["data-block-guid"] != '' && v.attributes["data-block-type"].toLowerCase() != "global") {
            var attr = '';
            var attr1 = '';
            $.each(v.attributes, function (key, value) {
                attr += '[' + key + '="' + value + '"]';
                if (key == 'id')
                    attr1 += '[' + key + '="' + value.split('-')[value.split('-').length - 1] + '"]';
                else
                    attr1 += '[' + key + '="' + value + '"]';
            });
            var $this = $(attr)[0];
            if ($this == undefined)
                $this = $(attr1)[0];
            if ($this != undefined) {
                if (v.components == undefined || v.components[0] == undefined) {
                    var component = { components: [], content: '' };
                    v.components = [];
                    v.components.push(component);
                }
                if (v.components != undefined && v.components[0] != undefined) {
                    v.components[0].components = [];
                    if (v.attributes["data-block-type"] == "Logo") {
                        var style = $(v.components[0].content).find('img').attr('style');
                        v.components[0].content = $this.innerHTML;
                        var contentdom = $(v.components[0].content);
                        $(contentdom).find('img').attr('style', style);
                        v.components[0].content = contentdom[0].outerHTML;
                    }
                    else
                        v.components[0].content = $this.outerHTML;
                    var existingcomp = v.components[0];
                    v.components = [];
                    v.components.push(existingcomp);
                    v.content = '';
                }
            }
        }
        else if (v.components != undefined) {
            BuildBlockComponent(v.components);
        }
    });
};

global.GetChildBlocks = function (model) {
    $.each(model.components, function (k, v) {
        if (v.type != undefined && v.type == 'blockwrapper') {
            var attr = '';
            $.each(v.attributes, function (key, value) {
                attr += '[' + key + '="' + value + '"]';
            });
            var $this = $(attr)[0];
            if ($this != undefined)
                VJLocalBlocksMarkup += $this.outerHTML;
        }
        GetChildBlocks(v);
    });
};

global.FilterComponents = function (vjcomps) {
    if (vjcomps != this.undefined) {
        return vjcomps.filter(function f(o) {
            if (o.components) {
                o.components = FilterComponents(o.components);
            }
            if (o.include == undefined || o.include == true) return true;
        })
    }
};

global.RenderCustomBlock = function (model, bmodel) {
    if (model != undefined && model.attributes != undefined && model.attributes.attributes != undefined && model.attributes.attributes["data-block-type"] != undefined && model.attributes.attributes["data-block-type"].toLowerCase() == "global")
        model.attributes.name = "Global: " + bmodel.id;
    //setting style as empty to create id in html and json and pass in save api call
    if (model != undefined)
        model.setStyle('');
    IsVJCBRendered = true;
    if (!$('.optimizing-overlay').length)
        $('.vj-wrapper').prepend('<div class="optimizing-overlay"><h1><img class="centerloader" src="' + VjDefaultPath + 'loading.gif" />Please wait</h1></div>');
};

global.RenderBlock = function (model, bmodel, render) {
    var sf = $.ServicesFramework(-1);
    model.view.$el[0].innerHTML = '<img class="centerloader" src=' + VjDefaultPath + 'loading.gif>';
    $.ajax({
        type: "POST",
        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "Block/RenderItem",
        data: model.attributes.attributes,
        headers: {
            'ModuleId': parseInt(sf.getModuleId()),
            'TabId': parseInt(sf.getTabId()),
            'RequestVerificationToken': sf.getAntiForgeryValue()
        },
        success: function (response) {
            if (response != null) {
                VjEditor.StorageManager.setAutosave(false);

                if (model.attributes.attributes["data-block-type"].toLowerCase() == "global") {

                    const components = getAllComponents(model);
                    components.forEach(item => item.remove());

                    model.view.$el[0].innerHTML = '';
                    model.append($(response.Markup)[0].innerHTML);
                    const newcomponents = getAllComponents(model);
                    $.each(newcomponents, function (k, v) {
                        if (v.attributes != undefined && v.attributes.type != undefined && v.attributes.type == 'blockwrapper')
                            RenderBlock(v, v, false);
                    });
                }
                else {
                    model.components('');
                    model.set('content', $(response.Markup)[0].outerHTML);
                    model.view.$el[0].innerHTML = $(response.Markup)[0].outerHTML;
                    if (render || render == undefined)
                        model.view.render();
                    else {
                        if (model.getName() == 'Logo') {
                            var style = model.attributes.attributes["data-style"];
                            if (style != undefined) {
                                $(model.view.$el[0]).find('img').attr('style', style);
                            }
                        }
                    }
                }

                if (response.Scripts != undefined) {
                    $.each(response.Scripts, function (k, v) {
                        var script = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('script'))[0];
                        script.type = 'text/javascript';
                        script.src = v.replace('~/', VjDefaultPath.split('DesktopModules')[0]);
                        $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(script);
                    });
                }
                if (response.Styles != undefined) {
                    $.each(response.Styles, function (k, v) {
                        var link = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('link'))[0];
                        link.rel = 'stylesheet';
                        link.type = 'text/css';
                        link.href = v.replace('~/', VjDefaultPath.split('DesktopModules')[0]);
                        $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(link);
                    });
                }
                if (response.Script != undefined && response.Script != '') {
                    var script_tag = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('script'))[0];
                    script_tag.type = 'text/javascript';
                    script_tag.text = response.Script;
                    $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(script_tag);
                }
                if (response.Style != undefined && response.Style != '') {
                    var style_tag = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('style'))[0];
                    style_tag.type = 'text/css';
                    style_tag.text = response.Style;
                    if (style_tag.styleSheet)
                        style_tag.styleSheet.cssText = response.Style;
                    else
                        style_tag.appendChild(window.parent.window.VjEditor.Canvas.getDocument().createTextNode(response.Style));
                    $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(style_tag);
                }
                //model.components($($(response.Markup)[0]).html());
                if (bmodel != undefined) {
                    if (model.attributes.attributes["data-block-type"].toLowerCase() == "global") {
                        model.attributes.name = "Global: " + bmodel.id;
                        StyleGlobal(model);
                    }
                    else
                        model.attributes.name = bmodel.id;

                    VjEditor.LayerManager.render();
                }
                setTimeout(function () {
                    VjEditor.StorageManager.setAutosave(true);
                    VjEditor.runCommand("save");
                }, 500);
            }
        }
    });
};

global.CleanGjAttrs = function (html) {
    var result = '';
    var compHtml = $(html)[0];
    if (compHtml != undefined) {
        var attrsList = [];
        $.each(compHtml.attributes, function (k, v) { attrsList.push(v.name); });
        $.each(attrsList, function (k, v) {
            if (v != undefined && v.startsWith('data-gjs')) {
                $(compHtml).removeAttr(v);
            }
        });
        $.each(compHtml.querySelectorAll('*'), function (k, v) {
            var attrsList = [];
            $.each(v.attributes, function (i, va) { attrsList.push(va.name); });
            $.each(attrsList, function (kk, vv) {
                if (vv != undefined && vv.startsWith('data-gjs')) {
                    $(v).removeAttr(vv);
                }
            });
        });
        result = compHtml.outerHTML;
    }
    return result;
}
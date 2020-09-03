import {
    typeCustomCode,
    commandNameCustomCode,
    keyCustomCode,
} from './config';

export default (editor, config = {}) => {
	const c = config;
    let bm = editor.BlockManager;
    const { blockCustomCode, blockLabel } = config;

    blockCustomCode && bm.add(typeCustomCode, {
        label: `<svg viewBox="0 0 24 24">
        <path d="M14.6 16.6l4.6-4.6-4.6-4.6L16 6l6 6-6 6-1.4-1.4m-5.2 0L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4z"></path>
      </svg>
      <div>${blockLabel}</div>`,
        category: VjLocalized.Basic,
        activate: true,
        select: true,
        content: { type: typeCustomCode },
    });

	var domComps = editor.DomComponents;
    var defaultType = domComps.getType('default');
    var defaultModel = defaultType.model;
    const { toolbarBtnCustomCode } = config;
    let timedInterval;

    domComps.addType(typeCustomCode, {

        model: defaultModel.extend({
            defaults: Object.assign({}, defaultModel.prototype.defaults, {
                name: 'Custom Code',
                editable: true,
                traits: []
            }),

            /**
             * Initilize the component
             */
            init() {
                this.listenTo(this, `change:${keyCustomCode}`, this.onCustomCodeChange);
                const initialCode = this.get(keyCustomCode) || config.placeholderContent;
                !this.components().length && this.components(initialCode);
                const toolbar = this.get('toolbar');
                const id = 'custom-code';

                // Add the custom code toolbar button if requested and it's not already in
                if (toolbarBtnCustomCode && !toolbar.filter(tlb => tlb.id === id).length) {
                    toolbar.unshift({
                        id,
                        command: commandNameCustomCode,
                        label: `<svg viewBox="0 0 24 24">
              <path d="M14.6 16.6l4.6-4.6-4.6-4.6L16 6l6 6-6 6-1.4-1.4m-5.2 0L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4z"></path>
            </svg>`,
                    });
                }
            },

            /**
             * Callback to launch on keyCustomCode change
             */
            onCustomCodeChange() {
                this.components(this.get(keyCustomCode));
            },
        }, {
                /**
                 * The component can be used only if requested explicitly via `type` property
                 */
                isComponent() {
                    return false;
                }
            }),

        view: defaultType.view.extend({
            events: {
                dblclick: 'onActive',
            },

            init() {
                this.listenTo(this.model.components(), 'add remove reset', this.onComponentsChange);
                this.onComponentsChange();
            },

            /**
             * Things to do once inner components of custom code are changed
             */
            onComponentsChange() {
                timedInterval && clearInterval(timedInterval);
                timedInterval = setTimeout(() => {
                    const { model } = this;
                    const content = model.get(keyCustomCode) || '';
                    let droppable = 1;

                    // Avoid rendering codes with scripts
                    //if (content.indexOf('<script') >= 0) {
                    //    this.el.innerHTML = opts.placeholderScript;
                    //    droppable = 0;
                    //}

                    model.set({ droppable });
                }, 0);
            },

            onActive() {
                const target = this.model;
                this.em.get('Commands').run(commandNameCustomCode, { target });
            },
        })
    });

    const cmd = editor.Commands;
    const { modalTitle, codeViewOptions, commandCustomCode } = config;
    const appendToContent = (target, content) => {
        if (content instanceof HTMLElement) {
            target.appendChild(content);
        } else if (content) {
            target.insertAdjacentHTML('beforeend', content);
        }
    }

    // Add the custom code command
    cmd.add(commandNameCustomCode, {
        keyCustomCode,

        run(editor, sender, opts = {}) {
            this.editor = editor;
            this.options = opts;
            this.target = opts.target || editor.getSelected();
            const target = this.target;

            if (target && target.get('editable')) {
                this.showCustomCode(target);
            }
        },

        stop(editor) {
            editor.Modal.close();
        },

        /**
         * Method which tells how to show the custom code
         * @param  {Component} target
         */
        showCustomCode(target) {
            const { editor, options } = this;
            const title = options.title || modalTitle;
            const content = this.getContent();
            const code = target.get(keyCustomCode) || '';
            editor.Modal
                .open({ title, content })
                .getModel()
                .once('change:open', () => editor.stopCommand(this.id));
            this.getCodeViewer().setContent(code);
        },

        /**
         * Custom pre-content. Can be a simple string or an HTMLElement
         */
        getPreContent() { },

        /**
         * Custom post-content. Can be a simple string or an HTMLElement
         */
        getPostContent() { },

        /**
         * Get all the content for the custom code
         * @return {HTMLElement}
         */
        getContent() {
            const { editor } = this;
            const content = document.createElement('div');
            const codeViewer = this.getCodeViewer();
            const pfx = editor.getConfig('stylePrefix');
            content.className = `${pfx}custom-code`;
            appendToContent(content, this.getPreContent());
            content.appendChild(codeViewer.getElement());
            appendToContent(content, this.getPostContent());
            appendToContent(content, this.getContentActions());
            codeViewer.refresh();
            setTimeout(() => codeViewer.focus(), 0);

            return content;
        },

        /**
         * Get the actions content. Can be a simple string or an HTMLElement
         * @return {HTMLElement|String}
         */
        getContentActions() {
            const { editor } = this;
            const btn = document.createElement('button');
            const pfx = editor.getConfig('stylePrefix');
            btn.innerHTML = config.buttonLabel;
            btn.className = `${pfx}btn-prim ${pfx}btn-import__custom-code`;
            btn.onclick = () => this.handleSave();

            return btn;
        },

        /**
         * Handle the main save task
         */
        handleSave() {
            const { editor, target } = this;
            const code = this.getCodeViewer().getContent();
            target.set(keyCustomCode, code);
            target.trigger(`change:${keyCustomCode}`);
            var nestedComp = getAllComponents(target);
            if (nestedComp.length > 0) {
                $(nestedComp).each(function (index, value) {
                    value.set({ removable: false, draggable: false, droppable: false, badgable: false, stylable: false, highlightable: false, copyable: false, resizable: false, layerable: false, selectable: false, editable: false, hoverable: false });
                });
            }
            editor.Modal.close();
        },

        /**
         * Return the code viewer instance
         * @return {CodeViewer}
         */
        getCodeViewer() {
            const { editor } = this;

            if (!this.codeViewer) {
                this.codeViewer = editor.CodeManager.createViewer({
                    codeName: 'htmlmixed',
                    theme: 'hopscotch',
                    readOnly: 0,
                });
            }

            return this.codeViewer;
        },
    });
}
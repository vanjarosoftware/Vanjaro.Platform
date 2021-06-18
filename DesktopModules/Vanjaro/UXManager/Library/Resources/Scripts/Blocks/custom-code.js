const keyCustomCode = 'custom-code-plugin__code';
const typeCustomCode = 'custom-code';
const commandNameCustomCode = 'custom-code:open-modal';

LoadCustomCode = function (grapesjs) {
    grapesjs.plugins.add('customcode', (editor, config = {}) => {
        config = {

            // Label of the custom code block
            blockLabel: VjLocalized.CustomCode,

            // Object to extend the default custom code block, eg. { label: 'Custom Code', category: 'Extra', ... }.
            // Pass a falsy value to avoid adding the block
            blockCustomCode: {},

            // Object to extend the default custom code properties, eg. `{ name: 'Custom Code', droppable: false, ... }`
            propsCustomCode: {},

            // Initial content of the custom code component
            placeholderContent: '<span>Insert here your custom code</span>',

            // Object to extend the default component's toolbar button for the code, eg. `{ label: '</>', attributes: { title: 'Open custom code' } }`
            // Pass a falsy value to avoid adding the button
            toolbarBtnCustomCode: {},

            // Content to show when the custom code contains `<script>`
            placeholderScript: `<div style="pointer-events: none; padding: 10px;">
      <svg viewBox="0 0 24 24" style="height: 30px; vertical-align: middle;">
        <path d="M13 14h-2v-4h2m0 8h-2v-2h2M1 21h22L12 2 1 21z"></path>
        </svg>
      Custom code with <i>&lt;script&gt;</i> can't be rendered on the canvas
    </div>`,

            // Title for the custom code modal
            modalTitle: 'Insert your code',

            // Additional options for the code viewer, eg. `{ theme: 'hopscotch', readOnly: 0 }`
            codeViewOptions: {},

            // Label for the default save button
            buttonLabel: 'Update',

            // Object to extend the default custom code command.
            // Check the source to see all available methods
            commandCustomCode: {},
        };

        let bm = editor.BlockManager;

        config.blockCustomCode && bm.add(typeCustomCode, {
            label: `<svg viewBox="0 0 24 24">
        <path d="M14.6 16.6l4.6-4.6-4.6-4.6L16 6l6 6-6 6-1.4-1.4m-5.2 0L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4z"></path>
      </svg>
      <div>${config.blockLabel}</div>`,
            category: VjLocalized.Basic,
            activate: true,
            select: true,
            content: { type: typeCustomCode },
        });

        var domComps = editor.DomComponents;
        var defaultType = domComps.getType('default');
        var defaultModel = defaultType.model;
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
                    this.attributes.content == '' && this.set('content', initialCode);
                    const toolbar = this.get('toolbar');
                    const id = 'custom-code';

                    // Add the custom code toolbar button if requested and it's not already in
                    if (config.toolbarBtnCustomCode && !toolbar.filter(tlb => tlb.id === id).length) {
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
                    this.set('content', this.get(keyCustomCode));
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
                const title = options.title || config.modalTitle;
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
                editor.Modal.close();
                return false;
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
    });
}
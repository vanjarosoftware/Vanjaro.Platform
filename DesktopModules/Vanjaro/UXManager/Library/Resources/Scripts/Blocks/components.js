export default (editor, config = {}) => {
    const c = config;
    let domc = editor.DomComponents;

    const contexts = [
        'primary', 'secondary',
        'success', 'info',
        'warning', 'danger',
        'light', 'dark'
    ];

    const contexts_w_white = contexts.concat(['white']);

    const traits = {
        id: {
            name: 'id',
            label: c.labels.trait_id,
        },
        for: {
            name: 'for',
            label: c.labels.trait_for,
        },
        name: {
            name: 'name',
            label: c.labels.trait_name,
        },
        placeholder: {
            name: 'placeholder',
            label: c.labels.trait_placeholder,
        },
        value: {
            name: 'value',
            label: c.labels.trait_value,
        },
        required: {
            type: 'checkbox',
            name: 'required',
            label: c.labels.trait_required,
        },
        checked: {
            label: c.labels.trait_checked,
            type: 'checkbox',
            name: 'checked',
            changeProp: 1
        }
    };

    const sizes = {
        'lg': 'Large',
        'sm': 'Small'
    };

}
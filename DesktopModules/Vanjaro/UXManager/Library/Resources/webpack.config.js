const path = require('path');
module.exports = [
    {
        mode: 'production',
        performance: {
            maxEntrypointSize: 9999999,
            maxAssetSize: 9999999
        },
        entry: '../../../../../Portals/_default/Skins/Vanjaro/Resources/js/skin.js',
        output: {
            filename: `../../../../../../Portals/_default/Skins/Vanjaro/Resources/js/skin.min.js`
        }
    },
    {
        mode: 'development',
        performance: {
            maxEntrypointSize: 9999999,
            maxAssetSize: 9999999
        },
        entry: './Scripts/',
        output: {
            path: path.resolve(__dirname, 'Scripts'),
            filename: `uxmanager.min.js`
        }
    }
]


// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as assert from 'assert';
import * as Extension from '../../extension';

suite('Extension tests', () => {
    test('Get extension info', () => {
        Extension.ext.info
            .then((info) => {
                assert.strictEqual('dev', info.channel);
                assert.strictEqual('0.0.1', info.version);
                assert.strictEqual('ps-rule.vscode-dev', info.id);
            })
            .catch((reason) => {
                assert.fail(`Failed to get extension info. ${reason}`);
            });
    });
});

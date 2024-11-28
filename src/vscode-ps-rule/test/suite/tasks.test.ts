// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as assert from 'assert';
import * as Extension from '../../extension';

suite('PSRuleTaskProvider tests', () => {
    test('Call taskManager', () => {
        Extension.taskManager
            ?.provideTasks()
            .then((t) => {
                assert.strictEqual(1, t.length);
            })
            .catch((reason) => {
                assert.fail(`Failed to get tasks. ${reason}`);
            });
    });
});

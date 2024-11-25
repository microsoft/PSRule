// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as assert from 'assert';
import { ConfigurationManager, ExecutionActionPreference, OutputAs } from '../../configuration';

suite('ConfigurationManager tests', () => {
    test('Defaults', () => {
        const config = new ConfigurationManager(undefined, 'PSRule_unit_test');

        assert.equal(config.get().codeLensRuleDocumentationLinks, true);
        assert.equal(config.get().documentationCustomSnippetPath, undefined);
        assert.equal(config.get().documentationLocalePath, 'en');
        assert.equal(config.get().documentationPath, undefined);
        assert.equal(config.get().documentationSnippet, 'Rule Doc');
        assert.equal(config.get().executionRuleExcluded, ExecutionActionPreference.None);
        assert.equal(config.get().executionRuleSuppressed, ExecutionActionPreference.None);
        assert.equal(config.get().executionUnprocessedObject, ExecutionActionPreference.None);
        assert.equal(config.get().experimentalEnabled, false);
        assert.equal(config.get().outputAs, OutputAs.Summary);
        assert.equal(config.get().notificationsShowChannelUpgrade, true);
        assert.equal(config.get().notificationsShowPowerShellExtension, true);
        assert.equal(config.get().ruleBaseline, undefined);
        //assert.equal(config.get().languageServerPath, false);
    });
});

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as path from 'path';
import * as fse from 'fs-extra';
import { Position, TextDocument, Uri, window, workspace } from 'vscode';
import { logger } from '../logger';
import { getDocumentationPath, readDocumentationSnippet } from '../utils';
import { configuration } from '../configuration';

const validNameExpression =
    /[^<>:/\\|?*"'`+@._\-\x00-\x1F][^<>:/\\|?*"'`+@\x00-\x1F]{1,126}[^<>:/\\|?*"'`+@._\-\x00-\x1F]+/g;

/**
 * Create or edit documentation for a rule.
 * @param name The name of the rule.
 * @returns A promise for the task.
 */
export async function createOrEditDocumentation(name: string | undefined): Promise<void> {
    if (name === '' || name === undefined) {
        name = await window.showInputBox({
            prompt: 'Enter the name of the rule to create documentation for.',
            validateInput: (value: string) => {
                return validNameExpression.test(value) ? undefined : 'Must be a valid rule name.';
            },
        });
    }
    if (name === '' || name === undefined) return;

    let uri = await getDocumentationPath(name);

    if (uri) {
        let parent = Uri.file(path.dirname(uri.fsPath));

        logger.verbose(`Using documentation path ${uri.fsPath}`);

        let exists = await fse.pathExists(uri.fsPath);
        if (!exists) {
            await fse.ensureDir(parent.fsPath);
            await fse.writeFile(uri.fsPath, '', { encoding: 'utf-8' });
        }
        const document: TextDocument = await workspace.openTextDocument(uri);
        const editor = await window.showTextDocument(document);

        // Populate new documentation with a snippet
        const snippetConfig = configuration.get().documentationSnippet;
        const snippetPathConfig = configuration.get().documentationCustomSnippetPath;
        if (!exists && snippetConfig !== '') {
            let snippet = await readDocumentationSnippet(snippetPathConfig, snippetConfig);
            if (snippet) {
                editor.insertSnippet(snippet, new Position(0, 0));
            }
        }
    }
}

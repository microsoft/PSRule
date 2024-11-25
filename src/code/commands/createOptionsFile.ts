// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as filepath from 'path';
import * as fse from 'fs-extra';
import { Position, TextDocument, Uri, window, workspace } from 'vscode';
import { logger } from '../logger';
import { getActiveOrFirstWorkspace, readOptionsSnippet } from '../utils';

/**
 * Create a new options file.
 * @param path The path to the options file.
 * @returns A promise for the task.
 */
export async function createOptionsFile(path: string | undefined): Promise<void> {
    let defaultUri: Uri | undefined = getActiveOrFirstWorkspace()?.uri;
    if (defaultUri) {
        defaultUri = Uri.joinPath(defaultUri, '/ps-rule.yaml');
    }
    if (path === '' || path === undefined) {
        const response = await window.showSaveDialog({
            defaultUri: defaultUri,
            filters: { 'PSRule options file': ['yaml'] },
            title: 'Where would you like to save the PSRule options file?',
            saveLabel: 'Save options file',
        });
        if (!response || !response.fsPath) return;
        path = response.fsPath;
    }
    if (path === '' || path === undefined) return;

    let uri = Uri.file(path);
    if (uri) {
        let parent = Uri.file(filepath.dirname(uri.fsPath));

        logger.verbose(`Using options path ${uri.fsPath}`);

        let exists = await fse.pathExists(uri.fsPath);
        if (!exists) {
            await fse.ensureDir(parent.fsPath);
            await fse.writeFile(uri.fsPath, '', { encoding: 'utf-8' });
        }
        const document: TextDocument = await workspace.openTextDocument(uri);
        const editor = await window.showTextDocument(document);

        // Populate new options snippet
        if (!exists) {
            let snippet = await readOptionsSnippet('PSRule options');
            if (snippet) {
                editor.insertSnippet(snippet, new Position(0, 0));
            }
        }
    }
}

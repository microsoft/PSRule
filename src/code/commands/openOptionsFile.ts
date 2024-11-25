// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as fse from 'fs-extra';
import { RelativePattern, TextDocument, Uri, window, workspace, WorkspaceFolder } from 'vscode';
import { logger } from '../logger';
import { getActiveOrFirstWorkspace } from '../utils';

/**
 * Open an existing options file.
 * @param path The path to the options file.
 * @returns A promise for the task.
 */
export async function openOptionsFile(path: string | undefined): Promise<void> {
    const optionFilePath = await getOptionFile(path);
    if (optionFilePath === '' || optionFilePath === undefined) return;

    const uri = Uri.file(optionFilePath);
    logger.verbose(`Using options path ${uri.fsPath}`);
    const exists = await fse.pathExists(uri.fsPath);
    if (!exists)
        return;

    const document: TextDocument = await workspace.openTextDocument(uri);
    await window.showTextDocument(document);
}

async function getOptionFile(path: string | undefined): Promise<string | undefined> {
    // Require an active workspace.
    const active: WorkspaceFolder | undefined = getActiveOrFirstWorkspace();
    if (!active) return Promise.resolve(undefined);
    if (!(path === '' || path === undefined)) return Promise.resolve(path);

    const workspaceUri: Uri = active.uri;
    const searchPattern = new RelativePattern(workspaceUri, '**/ps-rule.yaml');
    return new Promise<string | undefined>((resolve) => {
        workspace.findFiles(searchPattern).then(files => {
            if (files === undefined || files.length === 0)
                resolve(undefined);

            const names: string[] = [];
            files.forEach(item => {
                names.push(item.path);
            });
            window.showQuickPick(names, { title: 'Options file' }).then(item => {
                return resolve(item);
            });
        });
    });
}

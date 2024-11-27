// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as fse from 'fs-extra';
import { RelativePattern, TextDocument, Uri, window, workspace, WorkspaceFolder } from 'vscode';
import { logger } from '../logger';
import { getActiveOrFirstWorkspace } from '../utils';
import { configuration } from '../configuration';

/**
 * Open an existing options file.
 * @param path The path to the options file.
 * @returns A promise for the task.
 */
export async function openOptionsFile(path: string | undefined): Promise<void> {
    const optionFilePath = await getOptionFile(path);
    const active: WorkspaceFolder | undefined = getActiveOrFirstWorkspace();
    if (optionFilePath === '' || optionFilePath === undefined || !active) return;

    const uri = Uri.joinPath(active.uri, optionFilePath);
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
    if (!(path === '' || path === undefined)) return path;

    const workspaceUri: Uri = active.uri;

    // Check if options file is overridden in settings.
    const optionsPath = configuration.get().optionsPath;
    let optionsPathUri: Uri | undefined = undefined;
    if (optionsPath !== undefined && optionsPath !== '' && workspaceUri !== undefined) {
        optionsPathUri = Uri.joinPath(workspaceUri, optionsPath);
    }

    const names: string[] = [];

    if (optionsPathUri !== undefined && fse.existsSync(optionsPathUri.fsPath)) {
        names.push(workspace.asRelativePath(optionsPathUri.fsPath));
    }

    // Search for any options files in the workspace.
    const searchPattern = new RelativePattern(workspaceUri, '**/ps-rule.yaml');
    var files = await workspace.findFiles(searchPattern);
    if (files !== undefined && files.length > 0) {
        files.forEach(item => {
            names.push(workspace.asRelativePath(item.path));
        });
    }

    return await window.showQuickPick(names, { title: 'Options file', placeHolder: 'Select an options file' });
}

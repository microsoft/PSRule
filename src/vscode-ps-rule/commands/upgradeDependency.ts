// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as fse from 'fs-extra';
import { ProgressLocation, Uri, window } from 'vscode';
import { logger } from '../logger';
import { ext } from '../extension';
import { ExecuteCommandRequest } from 'vscode-languageclient';
import { getActiveOrFirstWorkspace } from '../utils';

/**
 * Upgrades a PSRule dependency.
 * @param path The path to the options file.
 * @returns A promise for the task.
 */
export async function upgradeDependency(path: string | undefined, name: string | undefined): Promise<void> {
    let workspaceUri: Uri | undefined = getActiveOrFirstWorkspace()?.uri;
    if (workspaceUri !== undefined && path === undefined) {
        path = Uri.joinPath(workspaceUri, '/ps-rule.lock.json').fsPath;
    }

    if (path !== '' && path !== undefined) {
        const exists = await fse.pathExists(path);
        if (!exists) {
            path = undefined;
            return;
        }

        // Try to get possible modules from the lock file.
        if (name === '' || name === undefined) {
            const lockFile = await fse.readJson(path, { encoding: 'utf-8' });
            if (lockFile !== undefined && lockFile.modules !== undefined) {
                const modules = Object.keys(lockFile.modules);

                if (modules == undefined || modules.length === 0) {
                    return;
                }

                // Get quick pick items.
                let items = modules.map((module) => {
                    return {
                        label: module,
                        kind: 0,
                    };
                });

                items.push({
                    label: '',
                    kind: -1,
                });

                items.push({
                    label: 'Upgrade all modules',
                    kind: 0,
                });

                // Prompt user to select a module.
                const selectedModule = await window.showQuickPick(items, {
                    placeHolder: 'Select a module to upgrade',
                });

                if (selectedModule !== undefined) {
                    name = selectedModule.label === 'Upgrade all modules' ? '*' : selectedModule.label;
                }
            }
        }
    }

    // Ignore if no path or name is provided.
    if (path === '' || path === undefined || name === '' || name === undefined) return;

    let uri = Uri.file(path);
    if (uri) {

        await window.withProgress({
            location: ProgressLocation.Window,
            title: 'PSRule',
            cancellable: false,
        }, async (progress) => {

            progress.report({ message: 'Upgrading dependencies' });

            const result = await ext.client.sendRequest(ExecuteCommandRequest.type, {
                command: 'upgradeDependency',
                arguments: [
                    {
                        path: uri.fsPath,
                        module: name
                    }
                ]
            });

            if (result !== undefined) {
                logger.verbose(`Upgraded dependency completed.`);
            }

            progress.report({ message: 'Completed upgrade', increment: 100 });
        });
    }
}

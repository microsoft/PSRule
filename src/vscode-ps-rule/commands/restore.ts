// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as cp from 'child_process';
import { workspace, window, ProgressLocation } from 'vscode';
import { getActiveOrFirstWorkspace } from '../utils';
import { logger } from '../logger';
import { ext } from '../extension';

/**
 * Restore modules in the workspace.
 * @returns A promise for the task.
 */
export async function restore(): Promise<void> {
    const workspace = getActiveOrFirstWorkspace();
    const server = ext.server;
    if (!workspace || !server) return;

    logger.log('Restoring modules.');

    await window.withProgress({
        location: ProgressLocation.Window,
        title: 'PSRule',
        cancellable: false,
    }, async (progress) => {

        progress.report({ message: 'Restoring modules' });

        const tool = cp.spawnSync(server.binPath, [server.languageServerPath, 'module', 'restore', '--verbose'], {
            cwd: workspace.uri.fsPath,
        });

        progress.report({ message: 'Restoring modules' });

        tool.output?.forEach(o => {

            o?.toString().split('\n').forEach(line => {
                if (line.startsWith('VERBOSE:')) {
                    logger.verbose(line);
                }
                else if (line.startsWith('ERROR:')) {
                    logger.error(line);
                }
                else {
                    logger.log(line);
                }
            });
        });

        if (tool.status !== 0) {
            logger.log(`Failed to restore modules. Exit code: ${tool.status}`);
            return;
        }
        else {
            logger.log('Modules restored.');
        }

        progress.report({ message: 'Completed restore', increment: 100 });
    });
}

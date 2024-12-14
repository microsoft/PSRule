// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as cp from 'child_process';
import { workspace, window, ProgressLocation } from 'vscode';
import { getActiveOrFirstWorkspace } from '../utils';
import { logger } from '../logger';
import { ext } from '../extension';

/**
 * Initialize the lock file.
 * @returns A promise for the task.
 */
export async function initLock(): Promise<void> {
    const workspace = getActiveOrFirstWorkspace();
    const server = ext.server;
    if (!workspace || !server) return;

    logger.log('Initializing lock file.');

    await window.withProgress({
        location: ProgressLocation.Window,
        title: 'PSRule',
        cancellable: false,
    }, async (progress) => {

        progress.report({ message: 'Initialize lock file' });

        const tool = cp.spawnSync(server.binPath, [server.languageServerPath, 'module', 'init', '--verbose', '--force'], {
            cwd: workspace.uri.fsPath,
        });

        progress.report({ message: 'Initialize lock file' });

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
            logger.log(`Failed to initialize lock file. Exit code: ${tool.status}`);
            return;
        }
        else {
            logger.log('Lock file created.');
        }

        progress.report({ message: 'Completed initialization', increment: 100 });
    });
}

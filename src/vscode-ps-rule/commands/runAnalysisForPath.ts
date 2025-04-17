// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProgressLocation, window } from 'vscode';
import { ExecuteCommandRequest, integer } from 'vscode-languageclient';
import { getActiveOrFirstWorkspace } from '../utils';
import { ext } from '../extension';
import { logger } from '../logger';
import { scanQuickPicker } from '../features/scanQuickpick/scanQuickpick';

interface RunCommandOutput {
    exitCode: integer;
}

/**
 * Runs the PSRule run analysis for a file or folder.
 * @param path The path to the file or folder to run analysis on.
 * @returns A promise for the task.
 */
export async function runAnalysisForPath(path: string | undefined): Promise<void> {
    const workspace = getActiveOrFirstWorkspace();
    if (!workspace) return;

    if (!path) {
        await scanQuickPicker();
    }


    await window.withProgress({
        location: ProgressLocation.Notification,
        title: 'PSRule',
        cancellable: false,
    }, async (progress) => {

        progress.report({ message: 'Running scan on path...' });

        logger.channel.show(true);

        const result: RunCommandOutput = await ext.client.sendRequest(ExecuteCommandRequest.type, {
            command: 'runAnalysis',
            arguments: [
                {
                    inputPath: [path],
                    workspacePath: workspace.uri.fsPath,
                }
            ]
        });

        progress.report({ message: 'Completed test', increment: 100 });
    });
}

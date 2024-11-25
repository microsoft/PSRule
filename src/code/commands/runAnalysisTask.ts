// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Task, tasks } from 'vscode';
import { getActiveOrFirstWorkspace } from '../utils';
import { ext } from '../extension';

/**
 * Runs the PSRule analysis task.
 * @returns A promise for the task.
 */
export async function runAnalysisTask(): Promise<void> {
    const workspace = getActiveOrFirstWorkspace();
    if (!workspace) return;
    const t = await ext.tasks?.getWorkspaceTasks(workspace);

    if (!t) return;

    const result: Task[] = [];
    t.forEach(task => {
        if (task.name === 'Run analysis' && task.source === 'PSRule') {
            result.push(task);
        }
    });
    await tasks.executeTask(result[0]);
}

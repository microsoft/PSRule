// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { commands } from 'vscode';

/**
 * Shows the quick pick list of available tasks.
 * @returns A promise for the task.
 */
export async function showTasks(): Promise<void> {
    commands.executeCommand('workbench.action.tasks.runTask');
}

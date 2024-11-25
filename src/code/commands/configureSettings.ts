// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { commands } from 'vscode';
import { logger } from '../logger';
import { ext } from '../extension';

/**
 * Open settings automatically filtered to PSRule specific settings.
 * @returns A promise for the task.
 */
export async function configureSettings(): Promise<void> {
    const extensionId = (await ext.info).id
    logger.verbose(`Opening settings for ${extensionId}`);
    commands.executeCommand('workbench.action.openSettings', `@ext:${extensionId}`);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as path from 'path';
import * as fse from 'fs-extra';
import { env } from 'vscode';
import { logger } from '../logger';
import { ext } from '../extension';

/**
 * Copy a walkthrough snippet to the clipboard by name.
 * @param name The name of a snippet to copy.
 * @returns A promise for the task.
 */
export async function walkthroughCopySnippet(name: string | undefined): Promise<void> {
    // Clear the clipboard and load snippets.
    env.clipboard.writeText('');
    const snippets = load('getStarted');

    // Find the correct snippet and copy it to the clipboard.
    snippets.then((value): void => {
        value.forEach((element: { name: string; snippet: string[] | undefined; }) => {
            if (name === element.name && element.snippet) {
                logger.verbose(`Copying snippet for ${name} to the clipboard.`);
                const text = element.snippet.join('\n');
                env.clipboard.writeText(text);
                return;
            }
        });
    });
}

/**
 * Load a snippet for a walkthrough.
 * @param name The name of the walkthrough.
 * @returns A list of named snippets for the walkthrough.
 */
async function load(name: string): Promise<any> {
    const info = await ext.info;
    const helpFile = info ? path.join(info.path, `media/walkthroughs/${name}/snippets.json`) : undefined;
    if (helpFile && (await fse.pathExists(helpFile))) {
        return await fse.readJson(helpFile, { encoding: 'utf-8' });
    }
    return Promise.resolve(undefined);
}

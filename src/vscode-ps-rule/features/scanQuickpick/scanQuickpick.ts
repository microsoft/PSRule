// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as path from 'path';
import * as cp from 'child_process';
import { Uri, window, Disposable } from 'vscode';
import { QuickPickItem } from 'vscode';
import { workspace } from 'vscode';
import * as fs from 'fs';
import * as fse from 'fs-extra';
import { logger } from '../../logger';


/**
 * A file opener using window.createQuickPick().
 * 
 * It shows how the list of items can be dynamically updated based on
 * the user's input in the filter field.
 */
export async function scanQuickPicker(): Promise<Uri | undefined> {
    return await pickFile();
}

class FileItem implements QuickPickItem {

    label: string;
    description: string;

    constructor(public base: Uri, public uri: Uri) {
        this.label = path.basename(uri.fsPath);
        this.description = path.dirname(path.relative(base.fsPath, uri.fsPath));
    }
}

class MessageItem implements QuickPickItem {

    label: string;
    description = '';
    detail: string;

    constructor(public base: Uri, public message: string) {
        this.label = message.replace(/\r?\n/g, ' ');
        this.detail = base.fsPath;
    }
}

async function pickFile(): Promise<Uri | undefined> {
    if (workspace.workspaceFolders == undefined || workspace.workspaceFolders.length == 0) {
        return undefined;
    }

    const disposables: Disposable[] = [];
    const workspacePaths = workspace.workspaceFolders.map(f => f.uri.fsPath);

    try {
        return await new Promise<Uri | undefined>((resolve) => {
            const input = window.createQuickPick<FileItem | MessageItem>();
            input.placeholder = 'Type to search for files';
            disposables.push(
                input.onDidChangeValue(value => {

                    const dirName = value.split('/').slice(0, -1).join('/');
                    const fileName = value.split('/').pop() || '';

                    const re = new RegExp(fileName, 'i');

                    logger.verbose(`Searching for ${fileName} in ${workspacePaths}`);

                    let files = [] as FileItem[];

                    input.busy = true;

                    workspacePaths.map(cwd => {

                        const target = path.join(cwd, `/${dirName}/`)

                        logger.verbose(`Searching in ${target}`);
                        logger.verbose(`Searching using regex ${re}`);

                        fse.readdirSync(target).forEach(file => {
                            logger.verbose(`Found ${file}`);
                            if (file.match(re)) {
                                files.push(new FileItem(Uri.file(cwd), Uri.file(path.join(target, file))));
                            }
                        });



                    });
                    input.items = files

                    input.busy = false;
                }),
                input.onDidChangeSelection(items => {
                    const item = items[0];
                    if (item instanceof FileItem) {
                        resolve(item.uri);
                        input.hide();
                    }
                }),
            );
            input.show();
        });
    } finally {
        disposables.forEach(d => d.dispose());
    }
}

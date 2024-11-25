// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as cp from 'child_process';
import * as fse from 'fs-extra';
import * as os from 'os';
import * as path from 'path';
import { existsSync } from 'fs';
import { SnippetString, Uri, WorkspaceFolder, window, workspace, commands, ExtensionContext } from 'vscode';
import { configuration } from './configuration';
import { ext } from './extension';
import { logger } from './logger';

const dotnetVersion = '8.0';
const packagedPath = 'server/Microsoft.PSRule.EditorServices.dll';

export interface PSRuleLanguageServer {
    binPath: string;
    languageServerPath: string;
}

/**
 * Calculates the file path of rule documentation for a specific rule based on settings.
 * @param name The name of the rule.
 * @returns The path where the rule markdown documentation should be created/ edited from.
 */
export async function getDocumentationPath(name: string): Promise<Uri | undefined> {
    const workspaceRoot = getActiveOrFirstWorkspace()?.uri;
    const docConfigPath = configuration.get().documentationPath;
    let docRootPath =
        docConfigPath && workspaceRoot ? path.join(workspaceRoot.fsPath, docConfigPath) : undefined;
    let lang = configuration.get().documentationLocalePath;

    if (!docRootPath && window.activeTextEditor?.document.uri) {
        docRootPath = path.dirname(window.activeTextEditor.document.uri.path);
    }
    if (docRootPath) {
        let uri = Uri.file(path.join(docRootPath, lang, `${name}.md`));
        return uri;
    }
    return undefined;
}

/**
 * Get a snippet from disk.
 * @param file The path to a file containing a snippet. When not set, the default extension snippets will be used.
 * @param name The name of the snippet to use. When name is an empty string no snippet is returned.
 * @returns A possible matching snippet string.
 */
export async function readDocumentationSnippet(
    file: string | undefined,
    name: string
): Promise<SnippetString | undefined> {
    if (name === '') return undefined;

    // Try custom snippet file
    const workspaceRoot = getActiveOrFirstWorkspace()?.uri;
    let snippetFile = file && workspaceRoot ? path.join(workspaceRoot.fsPath, file) : undefined;

    // Try built-in snippet file
    if (!snippetFile) {
        const info = await ext.info;
        snippetFile = info ? path.join(info.path, 'snippets/markdown.json') : undefined;
    }

    if (snippetFile && (await fse.pathExists(snippetFile))) {
        let json = await fse.readJson(snippetFile, { encoding: 'utf-8' });
        if (json) {
            let body: string[] = json[name].body;
            return new SnippetString(body.join(os.EOL));
        }
    }
}

/**
 * Get options snippet.
 * @param name The name of the snippet to use. When name is an empty string no snippet is returned.
 * @returns A possible matching snippet string.
 */
export async function readOptionsSnippet(name: string): Promise<SnippetString | undefined> {
    if (name === '') return undefined;

    // Try built-in snippet file
    const info = await ext.info;
    const snippetFile = info ? path.join(info.path, 'snippets/options.json') : undefined;

    if (snippetFile && (await fse.pathExists(snippetFile))) {
        let json = await fse.readJson(snippetFile, { encoding: 'utf-8' });
        if (json) {
            let body: string[] = json[name].body;
            return new SnippetString(body.join(os.EOL));
        }
    }
}

export function getActiveOrFirstWorkspace(): WorkspaceFolder | undefined {
    if (window.activeTextEditor) {
        return workspace.getWorkspaceFolder(window.activeTextEditor.document.uri);
    }
    return workspace.workspaceFolders && workspace.workspaceFolders.length > 0
        ? workspace.workspaceFolders[0]
        : undefined;
}

export async function getLanguageServer(context: ExtensionContext): Promise<PSRuleLanguageServer | undefined> {
    const binPath = await acquireDotnet();
    const languageServerPath = getLanguageServerPath(context);

    // Run the language server.
    if (binPath && languageServerPath) {
        const tool = cp.spawnSync(binPath, [languageServerPath, '--version']);
        const installedVersion = tool.stdout.toString().trim();
        const shortVersion = installedVersion.split('+')[0];

        logger.verbose(`Using PSRule ${shortVersion} from: ${languageServerPath}.`);

        return { binPath, languageServerPath };

    }
    return undefined;
}

/**
 * Get the path to the language server.
 * @param context A context for the extension.
 * @returns A path to the language server, or undefined if it does not exist under the extension path.
 */
function getLanguageServerPath(context: ExtensionContext): string | undefined {
    const languageServerPath =
        process.env.PSRULE_LANGUAGE_SERVER_PATH ?? // Local server for debugging.
        context.asAbsolutePath(packagedPath); // Packaged server.

    // Check if the language server binary exists.
    if (!existsSync(languageServerPath)) {
        logger.error(`Failed to find language server at: ${languageServerPath}`);
        return undefined;
    }
    return path.resolve(languageServerPath);
}

/**
 * Attempts to acquire .NET runtime.
 * @returns The path to the .NET runtime.
 */
async function acquireDotnet(): Promise<string> {
    logger.verbose(`Acquiring .NET runtime v${dotnetVersion}.`);
    const extensionId = (await ext.info).id;

    const result = await commands.executeCommand<{ dotnetPath: string }>(
        'dotnet.acquire',
        {
            version: dotnetVersion,
            requestingExtensionId: extensionId,
        }
    );

    if (!result) {
        const errorMessage = `Failed to install .NET runtime v${dotnetVersion}. Please see the .NET install tool error dialog for more detailed information, or to report an issue.`;
        logger.log(errorMessage);
        throw new Error(errorMessage);
    }
    logger.verbose(`Using .NET runtime from: ${result.dotnetPath}`);
    return path.resolve(result.dotnetPath);
}

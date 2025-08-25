// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

'use strict';

import * as vscode from 'vscode';
import * as lsp from 'vscode-languageclient/node';
import { logger } from './logger';
import { ext } from './extension';
import { getActiveOrFirstWorkspace } from './utils';
import { configuration, TraceLevelPreference } from './configuration';

export const StartProgressNotificationType = new lsp.ProgressType<string>();

/**
 * Implements a language server client for communication with PSRule runtime.
 */
export class PSRuleClient implements vscode.Disposable {
    private _configurationWatcher?: vscode.Disposable;
    private _currentOptionsPath?: string;
    public async configure(
        context: vscode.ExtensionContext,
    ): Promise<lsp.LanguageClient> {
        // Prepare options for the language server.
        const serverOptions = this.getServerOptions(context);
        if (!serverOptions) {
            logger.error('Failed to configure server options.');
            return Promise.reject('Failed to configure server options.');
        }

        const clientOptions = this.getClientOptions();

        // Create the language client.
        const client = new lsp.LanguageClient(
            'ps-rule',
            'PSRule Language Server',
            serverOptions,
            clientOptions,
        );
        
        // Get current options path
        this._currentOptionsPath = configuration.get().optionsPath;

        // Register proposed features
        client.registerProposedFeatures();

        // Setup event handlers.
        client.onNotification(lsp.LogTraceNotification.type, (params) => {
            if (params.message === undefined) {
                return;
            }

            if (params.verbose) {
                logger.verbose(params.message);
            }
            else {
                logger.log(params.message);
            }
        });

        client.onProgress(StartProgressNotificationType, 'server/ready', (arg1) => {
            logger.verbose(`Language server ready and connected: v${arg1}.`);
        });

        // Set up configuration change monitoring
        this.setupConfigurationWatcher();

        // Start the server and return the client.
        client.start();
        return client;
    }

    private getClientOptions(): lsp.LanguageClientOptions {
        const outputChannel = logger.channel;
        const clientOptions: lsp.LanguageClientOptions = {
            documentSelector: [{ language: 'yaml' }],
            progressOnInitialization: true,
            outputChannel,
            // middleware: {
            //     provideDocumentFormattingEdits: (document, options, token, next) =>
            //         next(
            //             document,
            //             {
            //                 ...options,
            //                 insertFinalNewline:
            //                     vscode.workspace
            //                         .getConfiguration('files')
            //                         .get('insertFinalNewline') ?? false,
            //             },
            //             token
            //         ),
            // },
            synchronize: {
                // Configure glob patterns to monitor for changes.
                fileEvents: [
                    vscode.workspace.createFileSystemWatcher('**/'), // folder changes
                    vscode.workspace.createFileSystemWatcher('**/*.Rule.yaml'), // Rule file changes
                    vscode.workspace.createFileSystemWatcher('**/ps-rule.lock.json'), // Lock file changes
                    vscode.workspace.createFileSystemWatcher('**/ps-rule.yaml'), // PSRule options file changes
                    vscode.workspace.createFileSystemWatcher('**/ps-rule.yml'), // PSRule options file changes (yml)
                    vscode.workspace.createFileSystemWatcher('**/psrule.yaml'), // PSRule options file changes (alternative name)
                    vscode.workspace.createFileSystemWatcher('**/psrule.yml'), // PSRule options file changes (alternative name, yml)
                ],
                // Configure additional file change handlers
                configurationSection: 'PSRule',
            },
        };
        
        return clientOptions;
    }

    private getServerOptions(
        context: vscode.ExtensionContext,
    ): lsp.ServerOptions | undefined {
        const binPath = ext.server?.binPath;
        const languageServerPath = ext.server?.languageServerPath;

        if (!binPath || !languageServerPath) {
            return undefined;
        }

        // Start the language server with additional arguments based on the configuration.
        let additionalArgs: string[] = [];
        if (configuration.get().traceServer === TraceLevelPreference.Verbose) {
            additionalArgs = ['--verbose'];
        }
        else if (configuration.get().traceServer === TraceLevelPreference.Debug) {
            additionalArgs = ['--verbose', '--debug'];
        }

        const cwd = getActiveOrFirstWorkspace()?.uri.fsPath;
        const serverExecutable: lsp.Executable = {
            command: binPath,
            args: [languageServerPath, 'listen', ...additionalArgs],
            options: { cwd },
            transport: lsp.TransportKind.pipe,
        };

        const serverOptions: lsp.ServerOptions = {
            run: serverExecutable,
            debug: serverExecutable,
        };

        return serverOptions;
    }

    private setupConfigurationWatcher(): void {
        // Watch for configuration changes that affect the options path
        this._configurationWatcher = vscode.workspace.onDidChangeConfiguration((e) => {
            if (e.affectsConfiguration('PSRule.options.path')) {
                const newOptionsPath = configuration.get().optionsPath;
                if (newOptionsPath !== this._currentOptionsPath) {
                    this._currentOptionsPath = newOptionsPath;
                    // The server will automatically reload options when it detects file changes
                    // or when the configuration updates are sent through the LSP protocol
                }
            }
        });
    }

    public run(): void { }

    public dispose(): void {
        this._configurationWatcher?.dispose();
    }
}

export const client = new PSRuleClient();

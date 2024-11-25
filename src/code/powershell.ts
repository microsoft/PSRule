// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

'use strict';

import * as vscode from 'vscode';
import { ext, ExtensionInfo } from './extension';
import { configuration, ConfigurationManager } from './configuration';
import { logger } from './logger';

/**
 * External interface from PowerShell extension.
 */
interface IExternalPowerShellDetails {
    exePath: string;
    version: string;
    displayName: string;
    architecture: string;
}

/**
 * External interface from PowerShell extension.
 */
interface IPowerShellExtensionClient {
    registerExternalExtension(id: string, apiVersion?: string): string;
    unregisterExternalExtension(uuid: string): boolean;
    getPowerShellVersionDetails(uuid: string): Promise<IExternalPowerShellDetails>;
}

export class PowerShellExtension implements vscode.Disposable {
    private readonly extension: vscode.Extension<IPowerShellExtensionClient> | undefined;

    private _version!: string;
    private _path!: string;
    private uuid!: string;

    constructor() {
        this.extension = this.getExtension();
    }

    public get isActive(): boolean {
        return this.extension !== undefined && this.extension?.isActive;
    }

    public get path(): string {
        return this._path;
    }

    public configure(info: ExtensionInfo): void {
        // Determine if the install PowerShell extension notification is displayed
        const showPowerShellExtension: boolean =
            configuration.get().notificationsShowPowerShellExtension;

        if (this.extension !== undefined && !this.extension.isActive) {
            this.extension.activate().then((client) => {
                this.uuid = client.registerExternalExtension(info.id, 'v1');
                client.getPowerShellVersionDetails(this.uuid).then((v) => this.handlePowerShell(v));
            });
        } else if (this.extension === undefined && showPowerShellExtension) {
            logger.verbose(`PowerShell extension is not installed.`);

            const showExtension = 'Show Extension';
            const alwaysIgnore = 'Always Ignore';

            vscode.window
                .showInformationMessage(
                    `Some features require the PowerShell extension to be installed and enabled.`,
                    showExtension,
                    alwaysIgnore
                )
                .then((choice) => {
                    if (choice === showExtension) {
                        vscode.commands.executeCommand(
                            'workbench.extensions.search',
                            'ms-vscode.PowerShell'
                        );
                    }
                    if (choice === alwaysIgnore) {
                        vscode.workspace
                            .getConfiguration('PSRule.notifications')
                            .update(
                                'showPowerShellExtension',
                                false,
                                vscode.ConfigurationTarget.Global
                            );
                    }
                });
        }
    }

    public dispose(): void {
        if (this.extension && this.uuid !== undefined) {
            const powerShellExtensionClient = this.extension!.exports as IPowerShellExtensionClient;
            powerShellExtensionClient.unregisterExternalExtension(this.uuid);
        }
    }

    private handlePowerShell(value: IExternalPowerShellDetails): void {
        this._version = value.version;
        this._path = value.exePath;

        logger.verbose(`Using PowerShell ${this._version} from: ${this._path}`);
    }

    private getExtension(): vscode.Extension<IPowerShellExtensionClient> | undefined {
        return (
            vscode.extensions.getExtension<IPowerShellExtensionClient>(
                'ms-vscode.powershell-preview'
            ) ?? vscode.extensions.getExtension<IPowerShellExtensionClient>('ms-vscode.powershell')
        );
    }
}

export const pwsh = new PowerShellExtension();

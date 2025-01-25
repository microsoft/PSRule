// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

'use strict';

import * as fse from 'fs-extra';
import {
    CancellationToken,
    CodeLens,
    CodeLensProvider,
    Disposable,
    Event,
    EventEmitter,
    ExtensionContext,
    Position,
    Range,
    TextDocument,
    languages,
    workspace,
} from 'vscode';
import { DocumentSelector } from 'vscode-languageclient';
import { configuration } from './configuration';
import { ILogger } from './logger';

interface IContext {
    readonly logger: ILogger;
    readonly extensionContext: ExtensionContext;
}

/**
 * Provides code lenses for upgrading dependencies in a PSRule lock file.
 */
export class DependencyLensProvider implements CodeLensProvider, Disposable {
    // Fields
    private readonly context: IContext;

    private codeLenses: CodeLens[] = [];
    private regexModuleBlock: RegExp;
    private regexModuleName: RegExp;
    private _registration: Disposable | undefined;
    private _onDidChangeCodeLenses: EventEmitter<void> = new EventEmitter<void>();
    public readonly onDidChangeCodeLenses: Event<void> = this._onDidChangeCodeLenses.event;

    constructor(logger: ILogger, extensionContext: ExtensionContext) {
        this.context = { logger, extensionContext };
        this.regexModuleBlock = /"modules":\s*\{([^{}]*\{[^{}]*\}[^{}]*)*\}/g;
        this.regexModuleName =
            /(?<name>[^"]+)":\s*{\s*"version":\s*"[^"]*"/g;

        workspace.onDidChangeConfiguration((_) => {
            this._onDidChangeCodeLenses.fire();
        });
    }

    dispose() {
        this._registration?.dispose();
    }

    public register(): void {
        let filter: DocumentSelector = [
            { language: 'json', pattern: '**/ps-rule.lock.json' },
        ];
        this._registration = languages.registerCodeLensProvider(filter, this);
    }

    public async provideCodeLenses(
        document: TextDocument,
        token: CancellationToken
    ): Promise<CodeLens[]> {
        if (configuration.get().codeLensDependencyManagement) {
            this.codeLenses = [];

            const text = document.getText();

            // Add top of document.
            let topOfDocument = new Range(0, 0, 0, 0);
            this.codeLenses.push(await this.createUpgradeAllCodeLens(topOfDocument, document.uri.fsPath));

            // Get the text range of the module block.
            let outerMatches;
            let outerRange: Range | undefined = undefined;
            while ((outerMatches = this.regexModuleBlock.exec(text)) !== null) {
                const startPos = document.positionAt(outerMatches.index);
                const endPos = document.positionAt(outerMatches.index + outerMatches[0].length);
                outerRange = new Range(startPos, endPos);
            }

            // Add links for each module.
            if (outerRange) {

                let matches;
                while ((matches = this.regexModuleName.exec(text)) !== null) {
                    let name = matches.groups !== undefined ? matches.groups['name'].replace(/\'/g, '') : '';

                    const line = document.lineAt(document.positionAt(matches.index).line);
                    const indexOf = line.text.indexOf(matches[1]);
                    const position = new Position(line.lineNumber, indexOf);
                    const range = document.getWordRangeAtPosition(position);
                    if (range && name && outerRange.contains(range)) {
                        this.codeLenses.push(await this.createUpgradeModuleCodeLens(range, document.uri.fsPath, name));
                    }
                }
            }
            return this.codeLenses;
        }
        return [];
    }

    /**
     * Create a code lens for upgrading a specific dependency.
     * @param range The range in the document the code lens applies to.
     * @param path The file path to lock file.
     * @param name The name of the module.
     * @returns A code lens object.
     */
    private async createUpgradeModuleCodeLens(range: Range, path: string, name: string): Promise<CodeLens> {
        let title = 'Upgrade dependency';
        let tooltip = 'Upgrade the selected module.';

        return new CodeLens(range, {
            title: title,
            tooltip: tooltip,
            command: 'PSRule.upgradeDependency',
            arguments: [path, name],
        });
    }

    /**
     * Create a code lens for upgrading all dependencies.
     * @param range The range in the document the code lens applies to.
     * @param path The file path to lock file.
     * @returns A code lens object.
     */
    private async createUpgradeAllCodeLens(range: Range, path: string): Promise<CodeLens> {
        let title = 'Upgrade all dependencies';
        let tooltip = 'Upgrade all modules.';

        return new CodeLens(range, {
            title: title,
            tooltip: tooltip,
            command: 'PSRule.upgradeDependency',
            arguments: [path, '*'],
        });
    }

    public async resolveCodeLens(
        codeLens: CodeLens,
        token: CancellationToken
    ): Promise<CodeLens | undefined> {
        return undefined;
    }
}

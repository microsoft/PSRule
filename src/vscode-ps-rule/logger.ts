// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

'use strict';

import * as vscode from 'vscode';

export enum LogLevel {
    Normal,
    Warning,
    Error,
    Verbose,
}

// Define a logger
export interface ILogger {
    verbose(message: string, ...additionalMessages: string[]): void;

    error(message: string, ...additionalMessages: string[]): void;

    log(message: string, ...additionalMessages: string[]): void;

    dispose(): void;
}

export class Logger implements ILogger {
    private _LogLevel: LogLevel = LogLevel.Normal;
    private _Output: vscode.OutputChannel;

    constructor(channel: string) {
        this._Output = vscode.window.createOutputChannel(channel);
    }

    public dispose(): void {
        this._Output.dispose();
    }

    public verbose(message: string, ...additionalMessages: string[]): void {
        this.write(LogLevel.Verbose, message, ...additionalMessages);
    }

    public error(message: string, ...additionalMessages: string[]): void {
        this.write(LogLevel.Error, message, ...additionalMessages);
    }

    public log(message: string, ...additionalMessages: string[]): void {
        this.write(LogLevel.Normal, message, ...additionalMessages);
    }

    private write(logLevel: LogLevel, message: string, ...additionalMessages: string[]): void {
        if (logLevel >= this._LogLevel) {
            this.writeLine(message, logLevel);

            additionalMessages.forEach((line) => {
                this.writeLine(line, logLevel);
            });
        }
    }

    private writeLine(message: string, level: LogLevel): void {
        this._Output.appendLine(message);
    }
}

export const logger: ILogger = new Logger('PSRule');

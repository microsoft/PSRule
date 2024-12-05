// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

'use strict';

import * as vscode from 'vscode';
import { ISetting, ExecutionActionPreference, TraceLevelPreference } from './configuration';
import { getActiveOrFirstWorkspace } from './utils';

export function getAnalysisRunner(
    folder: vscode.WorkspaceFolder | undefined,
    configuration: ISetting,
    binPath: string,
    languageServerPath: string,
    path?: string,
    inputPath?: string,
    baseline?: string,
    modules?: string[],
    outcome?: string[],
): vscode.ProcessExecution {
    const executionRuleExcluded = configuration.executionRuleExcluded;
    const executionRuleSuppressed = configuration.executionRuleSuppressed;
    const executionUnprocessedObject = configuration.executionUnprocessedObject;
    const lockRestore = configuration.lockRestore;
    const optionsPath = configuration.optionsPath;
    const outputAs = configuration.outputAs;
    const ruleBaseline = configuration.ruleBaseline;
    const traceTask = configuration.traceTask;

    function getCmdTooling(): string[] {
        let params: string[] = [];

        // Path
        if (path !== undefined && path !== '') {
            params.push('--path');
            params.push(`'${path}'`);
        }

        // Options Path
        if (optionsPath !== undefined && optionsPath !== '') {
            params.push('--option');
            params.push(`'${optionsPath}'`);
        }

        // Input Path
        if (inputPath !== undefined && inputPath !== '') {
            params.push('--input-path');
            params.push(inputPath);
        } else {
            params.push('--input-path');
            params.push('.');
        }

        // Baseline
        if (baseline !== undefined && baseline !== '') {
            params.push('--baseline');
            params.push(`'${baseline}'`);
        } else if (ruleBaseline !== undefined && ruleBaseline !== '') {
            params.push('--baseline');
            params.push(`'${ruleBaseline}'`);
        }

        // Modules
        if (modules !== undefined && modules.length > 0) {
            for (let i = 0; i < modules.length; i++) {
                params.push('--module');
                params.push(`'${modules[i]}'`);
            }
        }

        // Outcome
        if (outcome !== undefined && outcome.length > 0) {
            for (let i = 0; i < outcome.length; i++) {
                params.push('--outcome');
                params.push(outcome[i]);
            }
        }
        else {
            params.push('--outcome');
            params.push('Fail');
            params.push('--outcome');
            params.push('Error');
        }

        // Toggle module restore
        if (!lockRestore) {
            params.push('--no-restore');
        }

        return params;
    }

    // Set environment variables for the task.
    let taskEnv: { [key: string]: string } = {
        PSRULE_OUTPUT_STYLE: 'VisualStudioCode',
        PSRULE_OUTPUT_AS: outputAs,
        PSRULE_OUTPUT_CULTURE: vscode.env.language,
        PSRULE_OUTPUT_BANNER: 'Minimal',
    };

    if (executionRuleExcluded !== undefined && executionRuleExcluded !== ExecutionActionPreference.None) {
        taskEnv.PSRULE_EXECUTION_RULEEXCLUDED = executionRuleExcluded;
    }

    if (executionRuleSuppressed !== undefined && executionRuleSuppressed !== ExecutionActionPreference.None) {
        taskEnv.PSRULE_EXECUTION_RULESUPPRESSED = executionRuleSuppressed;
    }

    if (executionUnprocessedObject !== undefined && executionUnprocessedObject !== ExecutionActionPreference.None) {
        taskEnv.PSRULE_EXECUTION_UNPROCESSEDOBJECT = executionUnprocessedObject;
    }

    const cwd = folder?.uri.fsPath ?? getActiveOrFirstWorkspace()?.uri.fsPath;
    const args = [languageServerPath, 'run'];
    args.push(...getCmdTooling());
    if (traceTask === TraceLevelPreference.Verbose) {
        args.push('--verbose');
    }

    return new vscode.ProcessExecution(
        binPath,
        args,
        { cwd: cwd, env: taskEnv },
    )
}

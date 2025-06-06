// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

'use strict';

import { ConfigurationChangeEvent, ExtensionContext, env, workspace } from 'vscode';
import { configurationItemPrefix } from './consts';

/**
 * The output of analysis tasks.
 */
export enum OutputAs {
    Detail = 'Detail',
    Summary = 'Summary',
}

export enum ExecutionActionPreference {
    /**
     * No preference.
     * This will inherit from the default.
     */
    None = 'None',

    /**
     * Continue to execute silently.
     */
    Ignore = 'Ignore',
    /**
     * Continue to execute but log a warning.
     */
    Warn = 'Warn',

    /**
     * Generate an error.
     */
    Error = 'Error',
}

export enum TraceLevelPreference {
    /**
     * Output no trace information.
     */
    Off = 'Off',

    /**
     * Output verbose information.
     */
    Verbose = 'Verbose',

    /**
     * Output debug information.
     */
    Debug = 'Debug',
}

/**
 * PSRule extension settings.
 */
export interface ISetting {
    /**
     * Determines if code lens rule documentation links are enabled.
     */
    codeLensRuleDocumentationLinks: boolean;

    /**
     * Determines if code lens for dependency management are enabled.
     */
    codeLensDependencyManagement: boolean;

    documentationCustomSnippetPath: string | undefined;
    documentationSnippet: string;
    documentationPath: string | undefined;
    documentationLocalePath: string;

    /**
     * Set `Execution.RuleExcluded`.
     * See help: https://microsoft.github.io/PSRule/v3/concepts/PSRule/en-US/about_PSRule_Options/#executionruleexcluded
     */
    executionRuleExcluded: ExecutionActionPreference;

    /**
     * Set `Execution.RuleSuppressed`.
     * See help: https://microsoft.github.io/PSRule/v3/concepts/PSRule/en-US/about_PSRule_Options/#executionrulesuppressed
     */
    executionRuleSuppressed: ExecutionActionPreference;

    /**
     * Set `Execution.UnprocessedObject`.
     * See help: https://microsoft.github.io/PSRule/v3/concepts/PSRule/en-US/about_PSRule_Options/#executionunprocessedobject
     */
    executionUnprocessedObject: ExecutionActionPreference;

    /**
     * Determines if experimental features are enabled.
     */
    experimentalEnabled: boolean;

    /**
     * Determines if PSRule will automatically restore modules.
     */
    lockRestore: boolean;

    /**
     * The path specifying a PSRule option file.
     * When not set, the default ps-rule.yaml will be used from the current workspace.
     */
    optionsPath: string | undefined;

    outputAs: OutputAs;

    /**
     * Determines if a notification to switch to the stable channel is shown on activation.
     */
    notificationsShowChannelUpgrade: boolean;

    /**
     * Determines if a notification to restore modules is shown on activation.
     */
    notificationsShowModuleRestore: boolean;

    /**
     * Determines if a notification to install the PowerShell extension is shown on activation.
     */
    notificationsShowPowerShellExtension: boolean;

    /**
     * The name of the default baseline to use for executing rules.
     */
    ruleBaseline: string | undefined;

    /**
     * The path to the PSRule language server.
     */
    // languageServerPath: string | undefined;

    /**
     * Determines if diagnostic logging is enabled for task output.
     */
    traceTask: TraceLevelPreference;

    /**
     * Determines if diagnostic logging is enabled for language server.
     */
    traceServer: TraceLevelPreference;
}

/**
 * Default configuration for PSRule extension settings.
 */
const globalDefaults: ISetting = {
    codeLensRuleDocumentationLinks: true,
    codeLensDependencyManagement: true,
    documentationCustomSnippetPath: undefined,
    documentationSnippet: 'Rule Doc',
    documentationPath: undefined,
    documentationLocalePath: env.language,
    executionRuleExcluded: ExecutionActionPreference.None,
    executionRuleSuppressed: ExecutionActionPreference.None,
    executionUnprocessedObject: ExecutionActionPreference.None,
    experimentalEnabled: false,
    lockRestore: false,
    optionsPath: undefined,
    outputAs: OutputAs.Summary,
    notificationsShowChannelUpgrade: true,
    notificationsShowModuleRestore: true,
    notificationsShowPowerShellExtension: true,
    ruleBaseline: undefined,
    // languageServerPath: undefined,
    traceTask: TraceLevelPreference.Off,
    traceServer: TraceLevelPreference.Off,
};

/**
 * A configuration manager class for PSRule.
 */
export class ConfigurationManager {
    private current: ISetting;
    private readonly default: ISetting;
    private readonly configurationItemPrefix: string;

    /**
     * A flag for when setting require reload.
     */
    private pendingLoad: boolean = true;

    constructor(setting?: ISetting, prefix?: string) {
        this.configurationItemPrefix = prefix ?? configurationItemPrefix;
        this.default = setting ?? globalDefaults;
        this.current = { ...this.default };
        this.loadSettings();
    }

    static configure(context: ExtensionContext) {
        if (context) {
            context.subscriptions.push(
                workspace.onDidChangeConfiguration(
                    configuration.onConfigurationChanged,
                    configuration
                )
            );
        }
    }

    public get(): ISetting {
        if (this.pendingLoad) {
            this.loadSettings();
        }
        return this.current;
    }

    private onConfigurationChanged(e: ConfigurationChangeEvent) {
        if (!e.affectsConfiguration(this.configurationItemPrefix)) {
            return;
        }
        this.pendingLoad = true;
    }

    private loadSettings(): void {
        const config = workspace.getConfiguration(this.configurationItemPrefix);

        // Experimental
        let experimental = (this.current.experimentalEnabled = config.get<boolean>(
            'experimental.enabled',
            this.default.experimentalEnabled
        ));

        // Read settings
        this.current.documentationCustomSnippetPath =
            config.get<string>('documentation.customSnippetPath') ??
            this.default.documentationCustomSnippetPath;

        this.current.documentationSnippet =
            config.get<string>('documentation.snippet') ?? this.default.documentationSnippet;

        this.current.documentationPath =
            config.get<string>('documentation.path') ?? this.default.documentationPath;

        this.current.documentationLocalePath =
            config.get<string>('documentation.localePath') ?? this.default.documentationLocalePath;

        this.current.codeLensRuleDocumentationLinks = config.get<boolean>(
            'codeLens.ruleDocumentationLinks',
            this.default.codeLensRuleDocumentationLinks
        );

        this.current.codeLensDependencyManagement = config.get<boolean>(
            'codeLens.dependencyManagement',
            this.default.codeLensDependencyManagement
        );

        this.current.executionRuleExcluded = config.get<ExecutionActionPreference>('execution.ruleExcluded', this.default.executionRuleExcluded);
        this.current.executionRuleSuppressed = config.get<ExecutionActionPreference>('execution.ruleSuppressed', this.default.executionRuleSuppressed);
        this.current.executionUnprocessedObject = config.get<ExecutionActionPreference>('execution.unprocessedObject', this.default.executionUnprocessedObject);

        this.current.lockRestore = config.get<boolean>('lock.restore') ?? this.default.lockRestore;

        this.current.optionsPath = config.get<string>('options.path') ?? this.default.optionsPath;

        this.current.outputAs = config.get<OutputAs>('output.as', this.default.outputAs);

        this.current.notificationsShowChannelUpgrade = config.get<boolean>(
            'notifications.showChannelUpgrade',
            this.default.notificationsShowChannelUpgrade
        );

        this.current.notificationsShowModuleRestore = config.get<boolean>(
            'notifications.showModuleRestore',
            this.default.notificationsShowModuleRestore
        );

        this.current.notificationsShowPowerShellExtension = config.get<boolean>(
            'notifications.showPowerShellExtension',
            this.default.notificationsShowPowerShellExtension
        );

        this.current.ruleBaseline =
            config.get<string>('rule.baseline') ?? this.default.ruleBaseline;

        // this.current.languageServerPath =
        //     config.get<string>('languageServer.path') ??
        //     this.default.languageServerPath;

        this.current.traceTask = config.get<TraceLevelPreference>(
            'trace.task',
            this.default.traceTask
        );

        this.current.traceServer = config.get<TraceLevelPreference>(
            'trace.server',
            this.default.traceServer
        );

        // Clear dirty settings flag
        this.pendingLoad = false;
    }
}

export const configuration = new ConfigurationManager();

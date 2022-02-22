# Contributing to PSRule

Welcome, and thank you for your interest in contributing to PSRule!

There are many ways in which you can contribute, beyond writing code.
The goal of this document is to provide a high-level overview of how you can get involved.

- [Reporting issues](#reporting-issues)
- [Improve documentation](#improving-documentation)
- Fix bugs or add features

## Asking Questions

Have a question? Rather than opening an issue, please ask a question in [discussions][1].
Your well-worded question will serve as a resource to others searching for help.

  [1]: https://github.com/microsoft/PSRule/discussions

## Reporting issues

Have you identified a reproducible problem?
Have a feature request?
We want to hear about it!
Here's how you can make reporting your issue as effective as possible.

### Identify Where to Report

The PSRule project is distributed across multiple repositories.
Try to file the issue against the correct repository.
Check the list of [related projects][2] if you aren't sure which repository is correct.

  [2]: https://microsoft.github.io/PSRule/latest/related-projects/

### Look for an existing issue

Before you create a new issue, please do a search in [open issues][issues] to see if the issue or feature request has already been filed.

If you find your issue already exists, make relevant comments and add your [reaction][3].
Use a reaction in place of a "+1" comment:

* 👍 - upvote
* 👎 - downvote

  [3]: https://github.com/blog/2119-add-reactions-to-pull-requests-issues-and-comments

## Improving documentation

This project contains a wide range of documentation, stored in `docs/`.
Some of the documentation that you might like to improve include:

- Scenarios and examples (`docs/authoring/` and `docs/scenarios/`).
- PowerShell cmdlet and conceptual topics (`docs/commands/` and `docs/concepts/`).

### Markdown formatting

When writing documentation in Markdown, please follow these formatting guidelines:

- Semantically break up long paragraphs into multiple lines, particularly if they contain multiple sentences.
- Add a blank line between paragraphs.
- Add a blank line before and after lists, code blocks, and section headers.

## Contributing to code

- Before writing a fix or feature enhancement, ensure that an issue is logged.
- Be prepared to discuss a feature and take feedback.
- Include unit tests and updates documentation to complement the change.

When you are ready to contribute a fix or feature:

- Start by [forking the PSRule repo][github-fork].
- Create a new branch from main in your fork.
- Add commits in your branch.
  - If you have updated module code also update `CHANGELOG.md`.
  - You don't need to update the `CHANGELOG.md` for changes to unit tests or documentation.
  - Try building your changes locally. See [building from source][build] for instructions.
- [Create a pull request][github-pr-create] to merge changes into the PSRule `main` branch.
  - If you are _ready_ for your changes to be reviewed create a _pull request_.
  - If you are _not ready_ for your changes to be reviewed, create a _draft pull request_.
  - An continuous integration (CI) process will automatically build your changes.
    - You changes must build successfully to be merged.
    - If you have any build errors, push new commits to your branch.
    - Avoid using forced pushes or squashing changes while in review, as this makes reviewing your changes harder.

### Intro to Git and GitHub

When contributing to documentation or code changes, you'll need to have a GitHub account and a basic understanding of Git.
Check out the links below to get started.

- Make sure you have a [GitHub account][github-signup].
- GitHub Help:
  - [Git and GitHub learning resources][learn-git].
  - [GitHub Flow Guide][github-flow].
  - [Fork a repo][github-fork].
  - [About Pull Requests][github-pr].

### Code editor

You should use the multi-platform [Visual Studio Code][vscode] (VS Code).
The project contains a number of workspace specific settings that make it easier to author consistently.

After installing VS Code, install the following extensions:

- [YAML](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml)
- [PowerShell](https://marketplace.visualstudio.com/items?itemName=ms-vscode.PowerShell)
- [Code Spell Checker](https://marketplace.visualstudio.com/items?itemName=streetsidesoftware.code-spell-checker)

### Building and testing

When creating a pull request to merge your changes, a continuous integration (CI) pipeline is run.
The CI pipeline will build then test your changes across MacOS, Linux and Windows configurations.

Before opening a pull request try building your changes locally.
To do this See [building from source][build] for instructions.

### Contributor License Agreement (CLA)

This project welcomes contributions and suggestions. Most contributions require you to
agree to a Contributor License Agreement (CLA) declaring that you have the right to,
and actually do, grant us the rights to use your contribution. For details, visit
https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need
to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the
instructions provided by the bot. You will only need to do this once across all repositories using our CLA.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Thank You!

Your contributions to open source, large or small, make great projects like this possible.
Thank you for taking the time to contribute.

[learn-git]: https://help.github.com/en/articles/git-and-github-learning-resources
[github-flow]: https://guides.github.com/introduction/flow/
[github-signup]: https://github.com/signup/free
[github-fork]: https://help.github.com/en/github/getting-started-with-github/fork-a-repo
[github-pr]: https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/about-pull-requests
[github-pr-create]: https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request-from-a-fork
[build]: https://github.com/microsoft/PSRule/blob/main/docs/install-instructions.md#building-from-source
[issues]: https://github.com/Microsoft/PSRule/issues

# Contributing

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

## How to contribute

- File or vote up issues
- Improve documentation
- Fix bugs or add features

### Intro to Git and GitHub

When contributing to documentation or code changes, you'll need to have a GitHub account and a basic understanding of Git.
Check out the links below to get started.

- Make sure you have a [GitHub account][github-signup].
- GitHub Help:
  - [Git and GitHub learning resources][learn-git].
  - [GitHub Flow Guide][github-flow].
  - [Fork a repo][github-fork].
  - [About Pull Requests][github-pr].

## Contributing to issues

- Check if the issue you are going to file already exists in our GitHub [issues](https://github.com/Microsoft/PSRule/issues).
- If you do not see your problem captured, please file a new issue and follow the provided template.
- If the an open issue exists for the problem you are experiencing, vote up the issue or add a comment.

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

[learn-git]: https://help.github.com/en/articles/git-and-github-learning-resources
[github-flow]: https://guides.github.com/introduction/flow/
[github-signup]: https://github.com/signup/free
[github-fork]: https://help.github.com/en/github/getting-started-with-github/fork-a-repo
[github-pr]: https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/about-pull-requests
[github-pr-create]: https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request-from-a-fork
[build]: https://github.com/microsoft/PSRule/blob/main/docs/install-instructions.md#building-from-source

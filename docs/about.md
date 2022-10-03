---
title: What is PSRule and why should I use it?
author: BernieWhite
discussion: false
---

# What is PSRule?

PSRule is a rules engine geared towards testing Infrastructure as Code (IaC).
Rules you write or import perform static analysis on IaC artifacts such as:
templates, manifests, pipelines, and workflows.

## Why use PSRule?

PSRule aims to provide a rich experience for building and running static analysis tests on IaC.
While this has some similarities to traditional testing frameworks it extends on the following:

- **Reuse and share** &mdash; existing pre-built rules, configure, or write your own.
- **Incremental adoption** &mdash; with baselines allows you to keep moving forward.
- **Handle exceptions** &mdash; and keep exceptions auditable in git history.
- **Documentation** &mdash; provides recommendations and examples instead of just pass or fail.

*[IaC]: Infrastructure as Code

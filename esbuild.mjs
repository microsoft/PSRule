// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Build script for extension

import * as esbuild from 'esbuild';

const channel = process.env['PSRULE_BUILD_CHANNEL'] || 'stable';
const version = process.env['PSRULE_BUILD_VERSION'] || 'latest';
const watch = process.argv.includes('--watch');
const production = process.argv.includes('--production');

console.warn("build extension");

async function main() {
  const ctx = await esbuild.context({
    entryPoints: ['src/code/main.ts'],
    bundle: true,
    format: 'cjs',
    minify: production,
    sourcemap: !production,
    sourcesContent: false,
    platform: 'node',
    outdir: 'out/dist',
    external: ['vscode'],
    plugins: [
      /* add to the end of plugins array */
      esbuildProblemMatcherPlugin
    ]
  });
  if (watch) {
    await ctx.watch();
  } else {
    await ctx.rebuild();
    await ctx.dispose();
  }
}

/**
 * @type {import('esbuild').Plugin}
 */
const esbuildProblemMatcherPlugin = {
  name: 'esbuild-problem-matcher',

  setup(build) {
    build.onStart(() => {
      console.log('[watch] build started');
    });
    build.onEnd(result => {
      result.errors.forEach(({ text, location }) => {
        console.error(`✘ [ERROR] ${text}`);
        console.error(`    ${location.file}:${location.line}:${location.column}:`);
      });
      console.log('[watch] build finished');
    });
  }
};

main().catch(e => {
  console.error(e);
  process.exit(1);
});

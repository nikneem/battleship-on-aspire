import { spawn } from 'node:child_process';
import { mkdtempSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const fallbackApiUrl = 'https://localhost:7223';
const configuredApiUrl = normalizeApiUrl(process.env.BATTLESHIP_API_URL ?? fallbackApiUrl);
const proxyDirectory = mkdtempSync(join(tmpdir(), 'battleship-proxy-'));
const proxyConfigPath = join(proxyDirectory, 'proxy.conf.json');

writeFileSync(
  proxyConfigPath,
  JSON.stringify(
    {
      '/api': {
        target: configuredApiUrl,
        changeOrigin: true,
        secure: false,
        logLevel: 'warn'
      },
      '/hubs': {
        target: configuredApiUrl,
        changeOrigin: true,
        secure: false,
        ws: true,
        logLevel: 'warn'
      }
    },
    null,
    2
  )
);

const child = spawn(process.execPath, [angularCliPath(), 'serve', '--proxy-config', proxyConfigPath, ...process.argv.slice(2)], {
  stdio: 'inherit'
});

child.on('exit', (code) => {
  process.exit(code ?? 0);
});

child.on('error', (error) => {
  console.error(error);
  process.exit(1);
});

function normalizeApiUrl(value) {
  return value.replace(/\/+$/, '');
}

function angularCliPath() {
  return join(process.cwd(), 'node_modules', '@angular', 'cli', 'bin', 'ng.js');
}

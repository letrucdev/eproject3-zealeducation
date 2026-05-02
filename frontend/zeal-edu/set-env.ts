import { mkdirSync, writeFileSync } from 'fs';
import { dirname } from 'path';
import { config } from 'dotenv';

config();

const requiredKeys = ['API_URL', 'APP_NAME'] as const;
const missing = requiredKeys.filter((k) => !process.env[k]);
if (missing.length) {
  console.error(
    `Missing env vars: ${missing.join(', ')}. Copy .env.example to .env and fill in values.`,
  );
  process.exit(1);
}

const content = `export const environment = {
  apiUrl: '${process.env['API_URL']}',
  appName: '${process.env['APP_NAME']}'
};
`;

const target = './src/environments/environment.ts';
mkdirSync(dirname(target), { recursive: true });
writeFileSync(target, content);
console.log('Generated src/environments/environment.ts');

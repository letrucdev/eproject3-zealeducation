import { writeFileSync } from 'fs';
import { config } from 'dotenv';

config();

const requiredKeys = ['API_URL'] as const;
const missing = requiredKeys.filter((k) => !process.env[k]);
if (missing.length) {
  console.error(`Missing env vars: ${missing.join(', ')}. Copy .env.example to .env and fill in values.`);
  process.exit(1);
}

const content = `export const environment = {
  apiUrl: '${process.env['API_URL']}',
};
`;

writeFileSync('./src/environments/environment.ts', content);
console.log('Generated src/environments/environment.ts');

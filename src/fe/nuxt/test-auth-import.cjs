// Test file to check auth store import
const fs = require('fs');
const path = require('path');

// Read the auth store file
const authStorePath = path.join(__dirname, 'stores', 'auth.ts');
const content = fs.readFileSync(authStorePath, 'utf8');

// Check if the export is there
const hasExport = content.includes('export const useAuthStore');
console.log('Has export:', hasExport);

// Check if the closing bracket is there
const hasClosing = content.includes('})');
console.log('Has closing bracket:', hasClosing);

// Check if the defineStore is properly closed
const defineStoreMatch = content.match(/export const useAuthStore = defineStore\('auth', {/);
console.log('DefineStore match:', !!defineStoreMatch);

// Count opening and closing braces
const openBraces = (content.match(/{/g) || []).length;
const closeBraces = (content.match(/}/g) || []).length;
console.log('Open braces:', openBraces);
console.log('Close braces:', closeBraces);
console.log('Balanced:', openBraces === closeBraces);

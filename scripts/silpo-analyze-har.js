const fs = require('fs');
const path = require('path');

const artifactsDir = path.resolve(__dirname, '..', 'artifacts', 'silpo');
const harPath = path.join(artifactsDir, 'silpo.har');
const outputPath = path.join(artifactsDir, 'har-summary.json');

if (!fs.existsSync(harPath)) {
  console.error(`HAR not found at ${harPath}. Run scripts/silpo-capture.js first.`);
  process.exit(1);
}

const har = JSON.parse(fs.readFileSync(harPath, 'utf-8'));
const entries = har.log && har.log.entries ? har.log.entries : [];

const urlMatchesSilpo = (url) => url.includes('silpo.ua');
const isInteresting = (entry) => {
  const url = entry.request.url || '';
  const postText = entry.request.postData ? entry.request.postData.text || '' : '';
  return /cart|basket|checkout|order|graphql/i.test(url) || /cart|basket|checkout|order|graphql/i.test(postText);
};

const maskHeaderValue = (name, value) => {
  const key = name.toLowerCase();
  if (['cookie', 'authorization', 'x-csrf-token', 'x-xsrf-token', 'x-auth-token', 'x-api-key'].includes(key)) {
    return '<redacted>';
  }
  if (key === 'user-agent') {
    return '<redacted>';
  }
  return value;
};

const extractCookieNames = (headers, headerName) => {
  const cookieHeader = headers.find((h) => h.name.toLowerCase() === headerName);
  if (!cookieHeader) return [];
  if (headerName === 'set-cookie') {
    return cookieHeader.value.split(',').map((part) => part.trim().split('=')[0]);
  }
  return cookieHeader.value.split(';').map((part) => part.trim().split('=')[0]);
};

const sanitizePostData = (postData) => {
  if (!postData || !postData.text) return null;
  const text = postData.text;
  if (text.length > 1000) return `${text.slice(0, 1000)}...`;
  return text;
};

const summarizeEntry = (entry) => ({
  url: entry.request.url,
  method: entry.request.method,
  status: entry.response.status,
  requestHeaders: entry.request.headers.map((header) => ({
    name: header.name,
    value: maskHeaderValue(header.name, header.value)
  })),
  cookieNames: extractCookieNames(entry.request.headers, 'cookie'),
  setCookieNames: extractCookieNames(entry.response.headers, 'set-cookie'),
  postData: sanitizePostData(entry.request.postData)
});

const silpoEntries = entries.filter((entry) => urlMatchesSilpo(entry.request.url || ''));
const interestingEntries = silpoEntries.filter(isInteresting).map(summarizeEntry);

const graphqlOperations = silpoEntries
  .filter((entry) => (entry.request.url || '').includes('graphql'))
  .map((entry) => {
    const postData = entry.request.postData && entry.request.postData.text ? entry.request.postData.text : '';
    let operationName = null;
    try {
      const parsed = JSON.parse(postData);
      operationName = parsed.operationName || null;
    } catch (error) {
      // ignore
    }
    return {
      url: entry.request.url,
      method: entry.request.method,
      status: entry.response.status,
      operationName
    };
  });

const summary = {
  totalEntries: entries.length,
  silpoEntries: silpoEntries.length,
  interestingEntriesCount: interestingEntries.length,
  interestingEntries,
  graphqlOperations
};

fs.writeFileSync(outputPath, JSON.stringify(summary, null, 2));
console.log(`Summary written to ${outputPath}`);

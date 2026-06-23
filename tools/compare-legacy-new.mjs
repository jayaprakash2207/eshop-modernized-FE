import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";

const legacyBaseUrl = process.env.LEGACY_BASE_URL;
const newBaseUrl = process.env.NEW_BASE_URL;

if (!legacyBaseUrl || !newBaseUrl) {
  console.error("Set LEGACY_BASE_URL and NEW_BASE_URL before running this script.");
  process.exit(1);
}

const contracts = [
  { method: "GET", path: "/api/catalog-brands" },
  { method: "GET", path: "/api/catalog-types" },
  { method: "GET", path: "/api/catalog-items?pageIndex=0&pageSize=5" },
  { method: "GET", path: "/api_health_check" },
  { method: "GET", path: "/home_page_health_check" }
];

async function fetchContract(baseUrl, contract) {
  const response = await fetch(`${baseUrl}${contract.path}`, {
    method: contract.method
  });

  const text = await response.text();
  return {
    status: response.status,
    bodyPreview: text.slice(0, 1000)
  };
}

const report = {
  generatedAt: new Date().toISOString(),
  legacyBaseUrl,
  newBaseUrl,
  comparisons: []
};

for (const contract of contracts) {
  const [legacy, modern] = await Promise.all([
    fetchContract(legacyBaseUrl, contract).catch((error) => ({ error: String(error) })),
    fetchContract(newBaseUrl, contract).catch((error) => ({ error: String(error) }))
  ]);

  report.comparisons.push({
    ...contract,
    legacy,
    modern,
    statusMatch: legacy.status === modern.status,
    bodyMatch: legacy.bodyPreview === modern.bodyPreview
  });
}

const outputDir = path.join(process.cwd(), "generated-system", "comparison-reports");
await mkdir(outputDir, { recursive: true });
const outputPath = path.join(outputDir, `comparison-${Date.now()}.json`);
await writeFile(outputPath, JSON.stringify(report, null, 2));

console.log(`Comparison report written to ${outputPath}`);

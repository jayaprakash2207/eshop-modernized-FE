# Comparison Tools

## Legacy vs New API Comparison

Run the generated comparison script after both applications are running:

```powershell
$env:LEGACY_BASE_URL="http://localhost:5001"
$env:NEW_BASE_URL="http://localhost:5000"
node generated-system/tools/compare-legacy-new.mjs
```

The script writes a JSON report into `generated-system/comparison-reports/`.

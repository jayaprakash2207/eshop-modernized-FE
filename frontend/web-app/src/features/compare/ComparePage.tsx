export function ComparePage() {
  return (
    <section className="panel">
      <h2>Legacy vs New Comparison</h2>
      <p>
        Use the generated comparison script in <code>generated-system/tools</code> to compare the
        legacy eShopOnWeb endpoints with this forward-engineered application.
      </p>
      <ol>
        <li>Run the legacy application separately and note its base URL.</li>
        <li>Run this generated application and note its base URL.</li>
        <li>Execute the comparison script with both URLs.</li>
        <li>Review the JSON report for route-level matches and differences.</li>
      </ol>
    </section>
  );
}

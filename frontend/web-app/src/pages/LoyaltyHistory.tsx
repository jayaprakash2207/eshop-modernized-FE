import React from 'react';
import { useQuery } from '@tanstack/react-query';

async function fetchHistory(pageIndex = 0, pageSize = 20) {
  const res = await fetch(`/api/loyalty/history?pageIndex=${pageIndex}&pageSize=${pageSize}`, { credentials: 'include' });
  if (!res.ok) throw new Error('Failed to load history');
  return res.json();
}

export default function LoyaltyHistory() {
  const { data, isLoading, error } = useQuery(['loyaltyHistory'], () => fetchHistory());

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error loading loyalty history</div>;

  return (
    <div>
      <h1>Loyalty History</h1>
      <ul>
        {data.map((tx: any) => (
          <li key={tx.id}>{tx.type} {tx.points} on {new Date(tx.createdAtUtc).toLocaleString()}</li>
        ))}
      </ul>
    </div>
  );
}

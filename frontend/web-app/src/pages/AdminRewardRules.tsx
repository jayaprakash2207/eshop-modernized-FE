import React from 'react';
import { useQuery } from '@tanstack/react-query';

async function fetchRules() {
  const res = await fetch('/api/admin/loyalty/rules', { credentials: 'include' });
  if (!res.ok) throw new Error('Failed to load rules');
  return res.json();
}

export default function AdminRewardRules() {
  const { data, isLoading, error } = useQuery(['rewardRules'], fetchRules);

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error loading reward rules</div>;

  return (
    <div>
      <h1>Reward Rules</h1>
      <ul>
        {data.map((r: any) => (
          <li key={r.id}>{r.name} - x{r.earnMultiplier}</li>
        ))}
      </ul>
    </div>
  );
}

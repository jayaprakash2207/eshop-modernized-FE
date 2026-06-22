import React from 'react';
import { useQuery } from '@tanstack/react-query';

async function fetchBalance() {
  const res = await fetch('/api/loyalty/balance', { credentials: 'include' });
  if (!res.ok) throw new Error('Failed to load balance');
  return res.json();
}

export default function LoyaltyDashboard() {
  const { data, isLoading, error } = useQuery(['loyaltyBalance'], fetchBalance);

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error loading loyalty balance</div>;

  return (
    <div>
      <h1>Loyalty Dashboard</h1>
      <p>Points: {data.pointsBalance}</p>
      <p>Tier: {data.tier}</p>
    </div>
  );
}

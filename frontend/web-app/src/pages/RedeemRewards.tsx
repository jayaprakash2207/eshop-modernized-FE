import React from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';

export default function RedeemRewards() {
  const qc = useQueryClient();
  const mutation = useMutation(async (points: number) => {
    const res = await fetch('/api/loyalty/redeem', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ accountId: null, points }), credentials: 'include' });
    if (!res.ok) throw new Error('Redeem failed');
    return res.json();
  }, {
    onSuccess: () => qc.invalidateQueries(['loyaltyBalance', 'loyaltyHistory'])
  });

  return (
    <div>
      <h1>Redeem Rewards</h1>
      <button onClick={() => mutation.mutate(100)}>Redeem 100 Points</button>
    </div>
  );
}

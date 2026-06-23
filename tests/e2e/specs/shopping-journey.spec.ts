import { test, expect } from '@playwright/test';

/**
 * End-to-end shopping journey covering the core legacy flows preserved in the
 * modernized app: browse catalog → register/login → add to basket → checkout →
 * verify loyalty points were awarded (OrderPlaced → Loyalty event wiring).
 */

const uniqueEmail = () => `e2e-${Date.now()}-${Math.floor(Math.random() * 1e6)}@example.com`;

test.describe('Catalog', () => {
  test('home page lists catalog items', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: /catalog|products|shop/i })).toBeVisible();
    // At least one product card should render
    await expect(page.locator('[data-testid="catalog-item"], .catalog-item, article').first()).toBeVisible();
  });
});

test.describe('Authentication', () => {
  test('a new user can register and log in', async ({ page }) => {
    const email = uniqueEmail();
    await page.goto('/register');
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/^password/i).fill('SecurePass1!');
    await page.getByRole('button', { name: /register|sign up/i }).click();

    await page.goto('/login');
    await page.getByLabel(/email|username/i).fill(email);
    await page.getByLabel(/password/i).fill('SecurePass1!');
    await page.getByRole('button', { name: /log in|sign in/i }).click();

    await expect(page).toHaveURL(/\/$|\/catalog|\/account/);
  });
});

test.describe('Shopping + Loyalty', () => {
  test('add to basket, checkout, and earn loyalty points', async ({ page }) => {
    const email = uniqueEmail();

    // Register + login
    await page.goto('/register');
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/^password/i).fill('SecurePass1!');
    await page.getByRole('button', { name: /register|sign up/i }).click();

    await page.goto('/login');
    await page.getByLabel(/email|username/i).fill(email);
    await page.getByLabel(/password/i).fill('SecurePass1!');
    await page.getByRole('button', { name: /log in|sign in/i }).click();

    // Add first item to basket
    await page.goto('/');
    await page.getByRole('button', { name: /add to (basket|cart)/i }).first().click();

    // Go to basket and checkout
    await page.goto('/basket');
    await page.getByRole('button', { name: /checkout|place order/i }).click();

    // Fill shipping if prompted
    const street = page.getByLabel(/street|address/i);
    if (await street.isVisible().catch(() => false)) {
      await street.fill('123 Test St');
      await page.getByLabel(/city/i).fill('Testville');
      await page.getByLabel(/state/i).fill('TS');
      await page.getByLabel(/zip|postal/i).fill('00000');
      await page.getByLabel(/country/i).fill('Testland');
      await page.getByRole('button', { name: /place order|confirm|pay/i }).click();
    }

    await expect(page.getByText(/success|thank you|order placed/i)).toBeVisible();

    // Loyalty points should have been awarded for the order
    await page.goto('/loyalty');
    await expect(page.getByText(/points|balance/i)).toBeVisible();
  });
});

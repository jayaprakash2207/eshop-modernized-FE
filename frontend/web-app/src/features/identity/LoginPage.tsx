import { FormEvent, useState } from "react";
import { authenticate } from "./api";
import { setAccessToken } from "../../app/session";
import { useNavigate } from "react-router-dom";

export function LoginPage() {
  const [error, setError] = useState<string>("");
  const [preview, setPreview] = useState<string>("");
  const navigate = useNavigate();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    const formData = new FormData(event.currentTarget);
    const username = String(formData.get("username") ?? "");
    const password = String(formData.get("password") ?? "");

    try {
      const data = await authenticate(username, password);
      setAccessToken(data.accessToken);
      setPreview(`Signed in as ${data.username} (${data.role})`);
      navigate("/");
    } catch {
      setPreview("");
      setError("Authentication failed. Use admin/Admin123! or buyer/Buyer123!.");
    }
  }

  return (
    <section className="panel auth-panel">
      <h2>Login</h2>
      <p>
        This page exercises the preserved `/api/authenticate` flow and unlocks basket, orders,
        account, and admin testing.
      </p>

      <form className="auth-form" onSubmit={handleSubmit}>
        <label>
          Username
          <input name="username" defaultValue="buyer" />
        </label>
        <label>
          Password
          <input name="password" type="password" defaultValue="Buyer123!" />
        </label>
        <button type="submit">Authenticate</button>
      </form>

      {preview ? <p>{preview}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}

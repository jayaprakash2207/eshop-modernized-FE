import { FormEvent, useState } from "react";
import { apiRequest } from "../../app/http";

type RegisterResponse = {
  userId: string;
  username: string;
};

export function RegisterPage() {
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage("");
    setError("");

    const formData = new FormData(event.currentTarget);
    const payload = {
      username: String(formData.get("username") ?? ""),
      email: String(formData.get("email") ?? ""),
      password: String(formData.get("password") ?? "")
    };

    try {
      const response = await apiRequest<RegisterResponse>("/Account/Register", {
        method: "POST",
        body: JSON.stringify(payload)
      });
      setMessage(`Registered ${response.username} with id ${response.userId}.`);
    } catch (registrationError) {
      setError(registrationError instanceof Error ? registrationError.message : "Registration failed.");
    }
  }

  return (
    <section className="panel auth-panel">
      <h2>Register</h2>
      <p>This mirrors the preserved account registration surface for comparison testing.</p>

      <form className="auth-form" onSubmit={handleSubmit}>
        <label>
          Username
          <input name="username" defaultValue="newbuyer" />
        </label>
        <label>
          Email
          <input name="email" defaultValue="newbuyer@eshop.local" />
        </label>
        <label>
          Password
          <input name="password" type="password" defaultValue="Buyer123!" />
        </label>
        <button type="submit">Register</button>
      </form>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}

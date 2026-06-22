import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { changePassword, fetchProfile, updateProfile } from "./api";
import { FormEvent, useState } from "react";

export function AccountPage() {
  const queryClient = useQueryClient();
  const [message, setMessage] = useState("");

  const profileQuery = useQuery({
    queryKey: ["profile"],
    queryFn: fetchProfile
  });

  const profileMutation = useMutation({
    mutationFn: ({ email, phoneNumber }: { email: string; phoneNumber: string }) => updateProfile(email, phoneNumber),
    onSuccess: () => {
      setMessage("Profile updated.");
      queryClient.invalidateQueries({ queryKey: ["profile"] });
    }
  });

  const passwordMutation = useMutation({
    mutationFn: ({ currentPassword, newPassword }: { currentPassword: string; newPassword: string }) =>
      changePassword(currentPassword, newPassword),
    onSuccess: () => setMessage("Password changed.")
  });

  function submitProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    profileMutation.mutate({
      email: String(formData.get("email") ?? ""),
      phoneNumber: String(formData.get("phoneNumber") ?? "")
    });
  }

  function submitPassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    passwordMutation.mutate({
      currentPassword: String(formData.get("currentPassword") ?? ""),
      newPassword: String(formData.get("newPassword") ?? "")
    });
  }

  if (profileQuery.isLoading) {
    return <section className="panel">Loading account...</section>;
  }

  if (profileQuery.isError || !profileQuery.data) {
    return <section className="panel">Sign in to inspect the preserved account-management flows.</section>;
  }

  return (
    <section className="panel">
      <h2>My Account</h2>
      <p>
        {profileQuery.data.username} / {profileQuery.data.role} / {profileQuery.data.emailConfirmed ? "Email confirmed" : "Pending confirmation"}
      </p>

      <div className="form-grid">
        <form className="auth-form" onSubmit={submitProfile}>
          <label>
            Email
            <input name="email" defaultValue={profileQuery.data.email} />
          </label>
          <label>
            Phone Number
            <input name="phoneNumber" defaultValue={profileQuery.data.phoneNumber} />
          </label>
          <button type="submit">Update Profile</button>
        </form>

        <form className="auth-form" onSubmit={submitPassword}>
          <label>
            Current Password
            <input name="currentPassword" type="password" defaultValue="Buyer123!" />
          </label>
          <label>
            New Password
            <input name="newPassword" type="password" defaultValue="Buyer123!New" />
          </label>
          <button type="submit">Change Password</button>
        </form>
      </div>

      {message ? <p>{message}</p> : null}
    </section>
  );
}

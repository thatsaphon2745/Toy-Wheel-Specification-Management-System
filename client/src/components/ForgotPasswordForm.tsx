// src/pages/ForgotPasswordForm.tsx

import React, { useState } from "react";
import axios from "axios";
import Swal from "sweetalert2";
import mattelLogo from "../assets/MattelLogo.png";

import { User as UserIcon, Lock as LockIcon, Eye, EyeOff } from "lucide-react";


const ForgotPasswordForm: React.FC = () => {
  const [employeeId, setEmployeeId] = useState("");
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!employeeId || !email) {
      return Swal.fire({
        icon: "warning",
        title: "Missing Fields",
        text: "Please enter both Employee ID and Email",
      });
    }

    try {
      setLoading(true);
      const res = await axios.post("/api/auth/forgot-password", {
        employee_id: employeeId,
        email,
      });

      Swal.fire({
        icon: "success",
        title: "Reset Link Sent",
        text: "Check your email for the reset link.",
      });
    } catch (err) {
      Swal.fire({
        icon: "error",
        title: "Failed",
        text: (err as any).response?.data?.message || "An error occurred.",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="p-8 bg-white shadow-lg rounded-lg w-full max-w-lg">
      <img src={mattelLogo} alt="Logo" className="mx-auto mb-6 w-40 h-auto" />
      <h2 className="text-2xl text-center font-semibold mb-6">Forgot Password</h2>

      <input
        type="text"
        placeholder="Employee ID"
        value={employeeId}
        onChange={(e) => setEmployeeId(e.target.value)}
        className="w-full mb-4 p-3 border rounded"
      />
      <input
        type="email"
        placeholder="Email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        className="w-full mb-4 p-3 border rounded"
      />

      <button
        type="submit"
        disabled={loading}
        className="w-full bg-blue-600 text-white py-3 rounded hover:bg-blue-700 transition"
      >
        {loading ? "Sending..." : "Send Reset Link"}
      </button>
    </form>
  );
};

export default ForgotPasswordForm;

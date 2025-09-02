import React, { useState } from "react";
import axios from "axios";
import Swal from "sweetalert2";
import mattelLogo from "../assets/MattelLogo.png";
import { User as UserIcon, Lock as LockIcon, Eye, EyeOff } from "lucide-react";

import { AxiosError } from "axios";

interface LoginFormProps {
  onLoginSuccess: (username: string, role: string) => void; // ✅ เพิ่ม role
  onRegisterClick?: () => void;
}

const LoginForm: React.FC<LoginFormProps> = ({
  onLoginSuccess,
  onRegisterClick,
}) => {
  // const [kpkNum, setKpkNum] = useState("");

  // const [employeeId, setEmployeeId] = useState(""); // 👈 เดิม kpkNum

  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isPasswordActive, setIsPasswordActive] = useState(false);
  const [loading, setLoading] = useState(false);

  const [rememberMe, setRememberMe] = useState(
    localStorage.getItem("remember_me") === "true"
  );

  const [employeeId, setEmployeeId] = useState(
    localStorage.getItem("remembered_employee_id") || ""
  );

  const toggleShowPassword = () => {
    setShowPassword(!showPassword);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (employeeId && password) {
      const payload = {
        employee_id: employeeId,
        password,
      };

      console.log("📤 Submitting Login payload:", payload);

      setLoading(true);

      axios
        .post("/api/auth/login", payload)
        .then((res) => {
          console.log("✅ API Response:", res.data);

          const token = res.data.token;
          if (rememberMe) {
            localStorage.setItem("remembered_employee_id", employeeId);
            localStorage.setItem("remember_me", "true");
          } else {
            localStorage.removeItem("remembered_employee_id");
            localStorage.removeItem("remember_me");
          }

          // ✅ แก้ตรงนี้
          localStorage.setItem("token", token);
          // localStorage.setItem("LMS_User_Kpk", kpkNum);
          localStorage.setItem("LMS_User_EmployeeID", employeeId); // 👈 เปลี่ยนชื่อ key

          localStorage.setItem("LMS_remember_log", "active");

          Swal.fire({
            title: "Login Successful",
            text: "Welcome back!",
            icon: "success",
            confirmButtonText: "OK",
            customClass: {
              confirmButton:
                "bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700",
            },
            buttonsStyling: false,
          }).then(() => {
            // onLoginSuccess(kpkNum);
            const payloadBase64 = token.split(".")[1];
            const payloadJson = atob(payloadBase64);
            const payload = JSON.parse(payloadJson);

            const role = payload.role || "";
            const employee_id_from_token = payload.employee_id || ""; // ✅ ดึงจาก token
            onLoginSuccess(employee_id_from_token, role); // ✅ ใช้ค่าจาก token จริง ๆ
          });
        })
        .catch((err) => {
          console.error("❌ API Error:", err);
          console.log("🪵 err.response.data:", err.response?.data);

          Swal.fire({
            title: "Login Failed",
            text:
              err.response?.data?.message ||
              "Username or password is incorrect.",
            icon: "error",
            confirmButtonText: "Try Again",
            customClass: {
              confirmButton:
                "bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700",
            },
            buttonsStyling: false,
          });
        })
        .finally(() => {
          setLoading(false);
        });
    } else {
      Swal.fire({
        title: "Missing Fields",
        text: "Please enter both username and password.",
        icon: "warning",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 text-white px-4 py-2 rounded hover:bg-yellow-600",
        },
        buttonsStyling: false,
      });
    }
  };

  const handleRegisterClick = () => {
    if (onRegisterClick) onRegisterClick();
    else alert("Register clicked!");
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="p-8 bg-white shadow-lg rounded-lg w-full max-w-lg"
    >
      <img src={mattelLogo} alt="Logo" className="mx-auto mb-6 w-40 h-auto" />
      <h2 className="text-2xl text-center font-semibold mb-6">
        MBK Barbell Data Management
      </h2>

      {/* Username Input */}
      {/* Employee ID Input */}
      <div className="relative mb-4">
        <UserIcon className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
        <input
          type="text"
          value={employeeId}
          onChange={(e) => setEmployeeId(e.target.value)}
          placeholder="Employee ID"
          required
          className="w-full pl-10 p-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-600"
        />
      </div>

      {/* Password Input */}
      <div className="relative mb-4">
        <LockIcon className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />

        <input
          type={showPassword ? "text" : "password"}
          value={password}
          onChange={(e) => {
            setPassword(e.target.value);
            if (e.target.value !== "") {
              setIsPasswordActive(true);
            } else {
              setIsPasswordActive(false);
            }
          }}
          onFocus={() => setIsPasswordActive(true)}
          onBlur={() => {
            if (password === "") {
              setIsPasswordActive(false);
            }
          }}
          placeholder="Password"
          required
          className="w-full pl-10 pr-10 p-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-600"
        />

        {isPasswordActive && (
          <button
            type="button"
            onClick={toggleShowPassword}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
          >
            {showPassword ? (
              <EyeOff className="w-5 h-5" />
            ) : (
              <Eye className="w-5 h-5" />
            )}
          </button>
        )}
      </div>
      <div className="flex items-center mb-4">
        <input
          id="rememberMe"
          type="checkbox"
          checked={rememberMe}
          onChange={(e) => setRememberMe(e.target.checked)}
          className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label
          htmlFor="rememberMe"
          className="ml-2 block text-sm text-gray-700 select-none cursor-pointer"
        >
          Remember Me
        </label>
      </div>

      <button
        type="submit"
        disabled={loading}
        className="w-full bg-blue-600 text-white py-3 rounded hover:bg-blue-700 transition"
      >
        {loading ? "Signing in..." : "Sign in"}
      </button>

      <p className="mt-4 text-center text-sm text-gray-600">
        Don’t have an account?{" "}
        <button
          type="button"
          onClick={handleRegisterClick}
          className="font-semibold text-blue-600 hover:underline"
        >
          Register
        </button>
      </p>
    </form>
  );
};

export default LoginForm;

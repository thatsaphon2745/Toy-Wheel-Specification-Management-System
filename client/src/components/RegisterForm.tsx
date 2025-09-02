// import React, { useState, useEffect } from "react";
// import axios from "axios";
// import mattelLogo from "../assets/MattelLogo.png";

// import Swal from "sweetalert2";

// import Select from "react-select";

// interface RegisterFormProps {
//   onRegisterSuccess: () => void;
//   onBackToLogin?: () => void;
// }

// interface Department {
//   department_id: number;
//   department_name: string;
// }

// const RegisterForm: React.FC<RegisterFormProps> = ({
//   onRegisterSuccess,
//   onBackToLogin,
// }) => {
//   const [departments, setDepartments] = useState<Department[]>([]);
//   const [departmentId, setDepartmentId] = useState<string>("");
//   // const [username, setUsername] = useState("");
//   const [password, setPassword] = useState("");

//   const [employeeId, setEmployeeId] = useState("");
//   const [firstName, setFirstName] = useState("");
//   const [lastName, setLastName] = useState("");
//   const [email, setEmail] = useState("");

//   useEffect(() => {
//     axios
//       .get("/api/Department")
//       .then((res) => {
//         setDepartments(res.data);
//       })
//       .catch((err) => {
//         console.error(err);
//         alert("Error loading departments!");
//       });
//   }, []);

//   const handleSubmit = (e: React.FormEvent) => {
//     e.preventDefault();

//     const isEmployeeIdValid = /^\d+$/.test(employeeId);

//     const isEmailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

//     if (
//       // username &&
//       password &&
//       departmentId &&
//       employeeId &&
//       firstName &&
//       lastName &&
//       email &&
//       isEmployeeIdValid &&
//       isEmailValid
//     ) {
//       const payload = {
//         // username: username,
//         password,
//         first_name: firstName,
//         last_name: lastName,
//         department_id: Number(departmentId),
//         role_id: 3, // viewer role default
//         status_id: 1, // active status default
//         employee_id: employeeId,
//         email: email, // ✅ เพิ่มตรงนี้
//       };

//       console.log("📤 Submitting Register payload:", payload);

//       axios
//         .post("/api/auth/signup", payload)
//         .then((res) => {
//           console.log("✅ API Response:", res.data);

//           Swal.fire({
//             title: "Register Success!",
//             text: "User has been registered successfully.",
//             icon: "success",
//             confirmButtonText: "OK",
//             customClass: {
//               confirmButton:
//                 "bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700",
//             },
//             buttonsStyling: false,
//           }).then(() => {
//             onRegisterSuccess();
//           });
//         })
//         .catch((err) => {
//           console.error("❌ API Error:", err);
//           console.log("🪵 err.response.data:", err.response?.data);

//           Swal.fire({
//             title: "Register Failed",
//             text:
//               err.response?.data?.message ||
//               "Error occurred during registration.",
//             icon: "error",
//             confirmButtonText: "OK",
//             customClass: {
//               confirmButton:
//                 "bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700",
//             },
//             buttonsStyling: false,
//           });
//         });
//     } else {
//       let errorMsg = "";

//       if (!employeeId) {
//         errorMsg = "Employee ID is required!";
//       } else if (!isEmployeeIdValid) {
//         errorMsg = "Employee ID must be numeric only!";
//       } else if (!email) {
//         errorMsg = "Email is required!";
//       } else if (!isEmailValid) {
//         errorMsg = "Email format is invalid!";
//       } else {
//         errorMsg = "Please fill in all fields!";
//       }

//       console.warn("⚠️ Missing or invalid fields", {
//         // username,
//         password,
//         departmentId,
//         employeeId,
//         firstName,
//         lastName,
//         email: email, // ✅ เพิ่มตรงนี้
//       });

//       Swal.fire({
//         title: "Invalid Input",
//         text: errorMsg,
//         icon: "warning",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-yellow-500 text-white px-4 py-2 rounded hover:bg-yellow-600",
//         },
//         buttonsStyling: false,
//       });
//     }
//   };

//   const handleBackToLogin = () => {
//     if (onBackToLogin) onBackToLogin();
//     else alert("Back to Login clicked!");
//   };

//   const departmentOptions = departments.map((dept) => ({
//     value: dept.department_id,
//     label: dept.department_name,
//   }));
//   const [isDropdownOpen, setIsDropdownOpen] = useState(false);

//   return (
//     <form
//       onSubmit={handleSubmit}
//       className="p-8 bg-white shadow-lg rounded-lg w-full max-w-lg max-h-[80vh] overflow-y-auto"
//     >
//       <img src={mattelLogo} alt="Logo" className="mx-auto mb-6 w-40 h-auto" />
//       <h2 className="text-2xl text-center font-semibold mb-6">
//         Register New User
//       </h2>

//       <Select
//         value={departmentOptions.find(
//           (opt) => opt.value === Number(departmentId)
//         )}
//         onChange={(selected) => {
//           console.log("📥 department_id selected:", selected?.value);
//           setDepartmentId(String(selected?.value));
//         }}
//         options={departmentOptions}
//         placeholder="Select Department"
//         className="mb-4"
//         menuPortalTarget={document.body} // ⭐⭐ สำคัญสุด!!
//         styles={{
//           menuPortal: (base) => ({
//             ...base,
//             zIndex: 9999,
//           }),
//           menu: (base) => ({
//             ...base,
//             maxHeight: "auto",
//             overflowY: "auto",
//           }),
//         }}
//       />

//       {/* <input
//         type="text"
//         value={username}
//         onChange={(e) => setUsername(e.target.value)}
//         placeholder="Username"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       /> */}

//       <input
//         type="text"
//         value={employeeId}
//         onChange={(e) => setEmployeeId(e.target.value)}
//         placeholder="Employee ID"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       />

//       <input
//         type="password"
//         value={password}
//         onChange={(e) => setPassword(e.target.value)}
//         placeholder="Password"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       />

//       <input
//         type="email"
//         value={email}
//         onChange={(e) => setEmail(e.target.value)}
//         placeholder="Email"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       />

//       {/* <input
//         type="text"
//         value={employeeId}
//         onChange={(e) => setEmployeeId(e.target.value)}
//         placeholder="Employee ID"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       /> */}

//       <input
//         type="text"
//         value={firstName}
//         onChange={(e) => setFirstName(e.target.value)}
//         placeholder="First Name"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       />

//       <input
//         type="text"
//         value={lastName}
//         onChange={(e) => setLastName(e.target.value)}
//         placeholder="Last Name"
//         required
//         className="mb-4 w-full p-3 border rounded"
//       />

//       <button
//         type="submit"
//         className="w-full bg-blue-600 text-white py-3 rounded hover:bg-blue-700 transition"
//       >
//         Register
//       </button>

//       <p className="mt-4 text-center text-sm text-gray-600">
//         Already have an account?{" "}
//         <button
//           type="button"
//           onClick={handleBackToLogin}
//           className="font-semibold text-blue-600 hover:underline"
//         >
//           Back to Login
//         </button>
//       </p>
//     </form>
//   );
// };

// export default RegisterForm;

import React, { useState, useEffect } from "react";
import axios from "axios";
import mattelLogo from "../assets/MattelLogo.png";
import Swal from "sweetalert2";
import Select from "react-select";
import {
  User,
  Lock,
  Eye,
  EyeOff,
  Mail,
  Building2,
  Hash,
  ArrowLeft,
} from "lucide-react";

interface RegisterFormProps {
  onRegisterSuccess: () => void;
  onBackToLogin?: () => void;
}

interface Department {
  department_id: number;
  department_name: string;
}

const RegisterForm: React.FC<RegisterFormProps> = ({
  onRegisterSuccess,
  onBackToLogin,
}) => {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [departmentId, setDepartmentId] = useState<string>("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [employeeId, setEmployeeId] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");

  // UI state
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isPasswordActive, setIsPasswordActive] = useState(false);
  const [isConfirmPasswordActive, setIsConfirmPasswordActive] = useState(false);
  const [loading, setLoading] = useState(false);
  const [loadingDepartments, setLoadingDepartments] = useState(true);

  // Load departments
  useEffect(() => {
    axios
      .get("/api/Department")
      .then((res) => {
        setDepartments(res.data);
        setLoadingDepartments(false);
      })
      .catch((err) => {
        console.error(err);
        setLoadingDepartments(false);
        Swal.fire({
          title: "Error",
          text: "Error loading departments!",
          icon: "error",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700",
          },
          buttonsStyling: false,
        });
      });
  }, []);

  const toggleShowPassword = () => {
    setShowPassword(!showPassword);
  };
  

  const toggleShowConfirmPassword = () => {
    setShowConfirmPassword(!showConfirmPassword);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const isEmployeeIdValid = /^\d+$/.test(employeeId);
    const isEmailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

    if (password !== confirmPassword) {
      Swal.fire({
        title: "Password Mismatch",
        text: "Password and Confirm Password do not match.",
        icon: "warning",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 text-white px-4 py-2 rounded hover:bg-yellow-600",
        },
        buttonsStyling: false,
      });
      return;
    }

    if (
      password &&
      departmentId &&
      employeeId &&
      firstName &&
      lastName &&
      email &&
      isEmployeeIdValid &&
      isEmailValid
    ) {
      const payload = {
        password,
        first_name: firstName,
        last_name: lastName,
        department_id: Number(departmentId),
        role_id: 3, // viewer role default
        status_id: 1, // active status default
        employee_id: employeeId,
        email: email,
      };

      console.log("📤 Submitting Register payload:", payload);

      setLoading(true);

      axios
        .post("/api/auth/signup", payload)
        .then((res) => {
          console.log("✅ API Response:", res.data);

          Swal.fire({
            title: "Register Success!",
            text: "User has been registered successfully.",
            icon: "success",
            confirmButtonText: "OK",
            customClass: {
              confirmButton:
                "bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700",
            },
            buttonsStyling: false,
          }).then(() => {
            onRegisterSuccess();
          });
        })
        .catch((err) => {
          console.error("❌ API Error:", err);
          console.log("🪵 err.response.data:", err.response?.data);

          Swal.fire({
            title: "Register Failed",
            text:
              err.response?.data?.message ||
              "Error occurred during registration.",
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
      let errorMsg = "";

      if (!employeeId) {
        errorMsg = "Employee ID is required!";
      } else if (!isEmployeeIdValid) {
        errorMsg = "Employee ID must be numeric only!";
      } else if (!email) {
        errorMsg = "Email is required!";
      } else if (!isEmailValid) {
        errorMsg = "Email format is invalid!";
      } else {
        errorMsg = "Please fill in all fields!";
      }

      console.warn("⚠️ Missing or invalid fields", {
        password,
        departmentId,
        employeeId,
        firstName,
        lastName,
        email: email,
      });

      Swal.fire({
        title: "Missing Fields",
        text: errorMsg,
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

  const handleBackToLogin = () => {
    if (onBackToLogin) onBackToLogin();
    else alert("Back to Login clicked!");
  };
  

  const departmentOptions = departments.map((dept) => ({
    value: dept.department_id,
    label: dept.department_name,
  }));

  // Custom styles for react-select to match the form design
  const selectStyles = {
    control: (base: any, state: any) => ({
      ...base,
      minHeight: "48px", // Match input height (p-3 = 12px top/bottom + border)
      borderColor: state.isFocused ? "#2563eb" : "#d1d5db",
      borderWidth: "1px",
      borderRadius: "0.375rem", // Match rounded class
      boxShadow: state.isFocused ? "0 0 0 2px rgba(37, 99, 235, 0.5)" : "none",
      "&:hover": {
        borderColor: state.isFocused ? "#2563eb" : "#9ca3af",
      },
    }),
    placeholder: (base: any) => ({
      ...base,
      color: "#9ca3af",
    }),
    menuPortal: (base: any) => ({
      ...base,
      zIndex: 9999,
    }),
    menu: (base: any) => ({
      ...base,
      borderRadius: "0.375rem",
    }),
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="p-8 bg-white shadow-lg rounded-lg w-full max-w-lg max-h-[85vh] overflow-y-auto"
    >
      <img src={mattelLogo} alt="Logo" className="mx-auto mb-6 w-40 h-auto" />
      <h2 className="text-2xl text-center font-semibold mb-6">
        Register New User
      </h2>

      {/* Department Selection */}
      <div className="relative mb-4">
        
        <Select
          inputId="department"
          value={departmentOptions.find(
            (opt) => opt.value === Number(departmentId)
          )}
          onChange={(selected) =>
            setDepartmentId(String(selected?.value || ""))
          }
          options={departmentOptions}
          placeholder="Select Department"
          isLoading={loadingDepartments}
          isDisabled={loadingDepartments}
          menuPortalTarget={document.body}
          styles={selectStyles}
          className=""
        />
      </div>

      {/* Employee ID Input */}
      <div className="relative mb-4">
        <Hash className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
        <input
          type="text"
          value={employeeId}
          onChange={(e) => setEmployeeId(e.target.value)}
          placeholder="Employee ID"
          required
          className="w-full pl-10 p-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-600"
        />
      </div>

      {/* First Name and Last Name */}
      <div className="flex gap-3 mb-4">
        <div className="relative flex-1">
          <User className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            placeholder="First Name"
            required
            className="w-full pl-10 p-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-600"
          />
        </div>
        <div className="relative flex-1">
          <User className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            placeholder="Last Name"
            required
            className="w-full pl-10 p-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-600"
          />
        </div>
      </div>

      {/* Email Input */}
      <div className="relative mb-4">
        <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="Email"
          required
          className="w-full pl-10 p-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-600"
        />
      </div>

      {/* Password Input */}
      <div className="relative mb-4">
        <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
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

      {/* Confirm Password Input */}
      <div className="relative mb-4">
        <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
        <input
          type={showConfirmPassword ? "text" : "password"}
          value={confirmPassword}
          onChange={(e) => {
            setConfirmPassword(e.target.value);
            if (e.target.value !== "") {
              setIsConfirmPasswordActive(true);
            } else {
              setIsConfirmPasswordActive(false);
            }
          }}
          onFocus={() => setIsConfirmPasswordActive(true)}
          onBlur={() => {
            if (confirmPassword === "") {
              setIsConfirmPasswordActive(false);
            }
          }}
          placeholder="Confirm Password"
          required
          className={`w-full pl-10 pr-10 p-3 border rounded focus:outline-none focus:ring-2 ${
            confirmPassword && confirmPassword !== password
              ? "border-red-300 focus:ring-red-500"
              : "focus:ring-blue-600"
          }`}
        />
        {isConfirmPasswordActive && (
          <button
            type="button"
            onClick={toggleShowConfirmPassword}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
          >
            {showConfirmPassword ? (
              <EyeOff className="w-5 h-5" />
            ) : (
              <Eye className="w-5 h-5" />
            )}
          </button>
        )}
      </div>

      {/* Password mismatch indicator */}
      {confirmPassword && confirmPassword !== password && (
        <p className="text-sm text-red-600 mb-4 -mt-2">
          Passwords do not match.
        </p>
      )}

      {/* Submit Button */}
      <button
        type="submit"
        disabled={loading || loadingDepartments}
        className="w-full bg-blue-600 text-white py-3 rounded hover:bg-blue-700 transition"
      >
        {loading ? "Creating Account..." : "Register"}
      </button>

      {/* Back to Login */}
      <p className="mt-4 text-center text-sm text-gray-600">
        Already have an account?{" "}
        <button
          type="button"
          onClick={handleBackToLogin}
          className="font-semibold text-blue-600 hover:underline inline-flex items-center"
        >

          Back to Login
        </button>
      </p>
    </form>
  );
};

export default RegisterForm;

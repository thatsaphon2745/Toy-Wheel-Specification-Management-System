import React, { useState, useEffect } from "react";
import LoginForm from "./components/LoginForm";
import SummaryDdcTable from "./components/SummaryDdcTable";
import OriginalSpecTable from "./components/OriginalSpecTable";
import TypeTable from "./components/TypeTable";
import ToolTable from "./components/ToolTable";
import SizeRefTable from "./components/SizeRefTable";
import AxleTypeTable from "./components/AxleTypeTable";
import PositionTypeTable from "./components/PositionTypeTable";
import PadTable from "./components/PadTable";
import MachineTable from "./components/MachineTable";
import HstTypeTable from "./components/HstTypeTable";
import BrassTable from "./components/BrassTable";
import PadBrassMapTable from "./components/PadBrassMapTable";
import PadHstMapTable from "./components/PadHstMapTable";
import ToolKeyAllTable from "./components/ToolKeyAllTable";
import ToolMachineMapTable from "./components/ToolMachineMapTable";
import ToolPadMapTable from "./components/ToolPadMapTable";
import ToolPadHstBrassMapTable from "./components/ToolPadHstBrassMapTable";
import LogTable from "./components/LogTable";
import AccessControl from "./components/AccessControl";
import RequestTable from "./components/RequestTable";
import ChatbotIframe from "./components/ChatbotIframe";
import MarqueeBackground from "./components/MarqueeBackground";
import RegisterForm from "./components/RegisterForm";
import UserMenu from "./components/UserMenu";

import LoginBackground from "./components/LoginBackground";

import mattelLogo from "./assets/MattelLogo.png";

import mattelLogoGif from "@/assets/mattel_logo.gif";


import {
  FiBarChart2,
  FiBookOpen,
  FiPackage,
  FiTool,
  FiHardDrive,
  FiMapPin,
  FiMenu,
  FiX,
  FiSettings,
  FiFileText,
  FiMessageSquare,
  FiDatabase,
  FiLayers,
} from "react-icons/fi";

const App: React.FC = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [view, setView] = useState<string>("summary_ddc");
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const [isMobile, setIsMobile] = useState(false);
  const [authPage, setAuthPage] = useState<"login" | "register">("login");
  const [username, setUsername] = useState<string>("");
  const [role, setRole] = useState<string>("");
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      const timeLeft = getTokenTimeLeft(token);
      if (timeLeft > 0) {
        try {
          const payloadBase64 = token.split(".")[1];
          const payloadJson = atob(payloadBase64);
          const payload = JSON.parse(payloadJson);

          setUsername(payload.employee_id);
          setRole(payload.role);
          setIsLoggedIn(true);
        } catch (e) {
          console.error("❌ Invalid token payload", e);
          localStorage.removeItem("token");
        }
      } else {
        localStorage.removeItem("token");
      }
    }
    setIsCheckingAuth(false);
  }, []);

  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
      if (window.innerWidth < 768) {
        setIsSidebarOpen(false);
      }
    };

    checkMobile();
    window.addEventListener("resize", checkMobile);
    return () => window.removeEventListener("resize", checkMobile);
  }, []);

  useEffect(() => {
    if (!isLoggedIn) return;

    const token = localStorage.getItem("token");
    const timeLeft = getTokenTimeLeft(token);

    console.log("⏱️ Time left (seconds):", timeLeft);

    if (timeLeft <= 0) {
      handleSessionExpired();
      return;
    }

    const timer = setTimeout(() => {
      handleSessionExpired();
    }, timeLeft * 1000);

    return () => clearTimeout(timer);
  }, [isLoggedIn]);

  const getTokenTimeLeft = (token: string | null) => {
    if (!token) return 0;

    try {
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);

      if (!payload.exp) return 0;

      const now = Math.floor(Date.now() / 1000);
      const timeLeft = payload.exp - now;

      return timeLeft > 0 ? timeLeft : 0;
    } catch (e) {
      console.error("Invalid token:", e);
      return 0;
    }
  };

  const handleSessionExpired = () => {
    import("sweetalert2").then((Swal) => {
      Swal.default
        .fire({
          icon: "warning",
          title: "Session Expired",
          text: "Your session has expired. Please login again.",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg font-medium transition-colors",
            popup: "text-sm",
          },
        })
        .then(() => {
          setIsLoggedIn(false);
          setUsername("");
          setAuthPage("login");
          localStorage.removeItem("token");
        });
    });
  };

  const menuItems = [
    {
      label: "Summary DDC",
      value: "summary_ddc",
      icon: <FiBarChart2 size={20} />,
      category: "Tool Spec",
    },
    {
      label: "Original Spec",
      value: "original_spec",
      icon: <FiBookOpen size={20} />,
      category: "Tool Spec",
    },
    {
      label: "Type",
      value: "type",
      icon: <FiPackage size={20} />,
      category: "Configuration",
    },
    {
      label: "Tool",
      value: "tool",
      icon: <FiTool size={20} />,
      category: "Configuration",
    },
    {
      label: "Size Ref",
      value: "size_ref",
      icon: <FiHardDrive size={20} />,
      category: "Configuration",
    },
    {
      label: "Axle Type",
      value: "axle_type",
      icon: <FiMapPin size={20} />,
      category: "Configuration",
    },
    {
      label: "Position Type",
      value: "position_type",
      icon: <FiMapPin size={20} />,
      category: "Configuration",
    },
    {
      label: "Pad",
      value: "pad",
      icon: <FiPackage size={20} />,
      category: "Components",
    },
    {
      label: "HST Type",
      value: "hst_type",
      icon: <FiLayers size={20} />,
      category: "Components",
    },
    {
      label: "Brass",
      value: "brass",
      icon: <FiTool size={20} />,
      category: "Components",
    },
    {
      label: "Machine",
      value: "machine",
      icon: <FiSettings size={20} />,
      category: "Components",
    },
    {
      label: "Pad Brass Map",
      value: "pad_brass_map",
      icon: <FiMapPin size={20} />,
      category: "Mapping",
    },
    {
      label: "Pad HST Map",
      value: "pad_hst_map",
      icon: <FiMapPin size={20} />,
      category: "Mapping",
    },
    {
      label: "ToolKey All",
      value: "tool_key_all",
      icon: <FiTool size={20} />,
      category: "Tools",
    },
    {
      label: "Tool-Machine Map",
      value: "tool_machine_map",
      icon: <FiTool size={20} />,
      category: "Mapping",
    },
    {
      label: "Tool-Pad Map",
      value: "tool_pad_map",
      icon: <FiTool size={20} />,
      category: "Mapping",
    },
    {
      label: "Tool-Pad-Hst-Brass Map",
      value: "tool_pad_hst_brass_map",
      icon: <FiTool size={20} />,
      category: "Mapping",
    },
    {
      label: "Log Table",
      value: "LogTable",
      icon: <FiFileText size={20} />,
      category: "System",
    },
    {
      label: "Access Control",
      value: "AccessControl",
      icon: <FiSettings size={20} />,
      category: "System",
    },
    {
      label: "Request",
      value: "RequestTable",
      icon: <FiDatabase size={20} />,
      category: "System",
    },
    {
      label: "ChatBot",
      value: "ChatbotIframe",
      icon: <FiMessageSquare size={20} />,
      category: "Support",
    },
  ];

  const renderView = () => {
    switch (view) {
      case "summary_ddc":
        return <SummaryDdcTable />;
      case "original_spec":
        return <OriginalSpecTable />;
      case "type":
        return <TypeTable />;
      case "tool":
        return <ToolTable />;
      case "size_ref":
        return <SizeRefTable />;
      case "axle_type":
        return <AxleTypeTable />;
      case "position_type":
        return <PositionTypeTable />;
      case "pad":
        return <PadTable />;
      case "hst_type":
        return <HstTypeTable />;
      case "brass":
        return <BrassTable />;
      case "machine":
        return <MachineTable />;
      case "pad_brass_map":
        return <PadBrassMapTable />;
      case "pad_hst_map":
        return <PadHstMapTable />;
      case "tool_key_all":
        return <ToolKeyAllTable />;
      case "tool_machine_map":
        return <ToolMachineMapTable />;
      case "tool_pad_map":
        return <ToolPadMapTable />;
      case "tool_pad_hst_brass_map":
        return <ToolPadHstBrassMapTable />;
      case "LogTable":
        return <LogTable />;
      case "AccessControl":
        return <AccessControl />;
      case "RequestTable":
        return <RequestTable />;
      case "ChatbotIframe":
        return <ChatbotIframe />;
      default:
        return null;
    }
  };

  const groupedMenuItems = menuItems.reduce((acc, item) => {
    if (!acc[item.category]) {
      acc[item.category] = [];
    }
    acc[item.category].push(item);
    return acc;
  }, {} as Record<string, typeof menuItems>);

  if (isCheckingAuth) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 flex items-center justify-center">
        <div className="text-center">
          <div className="relative">
            <div className="w-16 h-16 mx-auto mb-6 rounded-full bg-gradient-to-r from-blue-500 to-indigo-600 shadow-lg animate-pulse"></div>
            <div className="absolute inset-0 w-16 h-16 mx-auto rounded-full border-4 border-blue-200 border-t-blue-500 animate-spin"></div>
          </div>
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 px-6 py-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-1">Loading</h3>
            <p className="text-sm text-gray-600">Checking authentication...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!isLoggedIn) {
    return (
      <div className="relative min-h-screen flex items-center justify-center overflow-hidden bg-gradient-to-br from-slate-900 via-blue-900 to-indigo-900">
        <LoginBackground />
        <div className="z-10 w-full max-w-md mx-4">
          <div className="bg-white rounded-3xl shadow-xl border border-gray-100 overflow-hidden backdrop-blur-sm">
            {authPage === "login" && (
              <LoginForm
                onLoginSuccess={(name, roleFromToken) => {
                  setUsername(name);
                  setRole(roleFromToken);
                  setIsLoggedIn(true);
                }}
                onRegisterClick={() => setAuthPage("register")}
              />
            )}
            {authPage === "register" && (
              <RegisterForm
                onRegisterSuccess={() => setAuthPage("login")}
                onBackToLogin={() => setAuthPage("login")}
              />
            )}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden">
      {/* Mobile Overlay */}
      {isMobile && isSidebarOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity duration-300"
          onClick={() => setIsSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          ${isMobile ? "fixed" : "relative"} 
          left-0 top-0 h-full
          bg-white shadow-xl border-r border-gray-200
          transition-all duration-300 ease-out
          ${
            isSidebarOpen
              ? "w-72 translate-x-0"
              : isMobile
              ? "w-72 -translate-x-full"
              : "w-0"
          }
          ${isMobile ? "z-50" : "z-10"}
          overflow-hidden
        `}
      >
        <div
          className={`h-full flex flex-col ${
            isSidebarOpen ? "opacity-100" : "opacity-0"
          } transition-opacity duration-300`}
        >
          {/* Sidebar Header */}
          <div className="px-6 py-6 bg-gradient-to-r from-blue-600 to-blue-700 relative overflow-hidden">
            <div className="absolute inset-0 bg-gradient-to-r from-blue-600 to-indigo-600 opacity-90"></div>
            <div className="relative z-10">
              <div className="flex items-center justify-between mb-1">
                <div className="flex items-center space-x-3">
                  <div className="w-10 h-10 bg-white rounded-xl shadow-md flex items-center justify-center">
                    {/* <img
                      src="/src/assets/mattel_logo.gif"
                      alt="Logo"
                      className="w-8 h-8 object-contain"
                    /> */}
                    <img src={mattelLogoGif} alt="Logo" className="w-8 h-8 object-contain" />
                  </div>
                  <div>
                    <h1 className="text-lg font-bold text-white">MBK Barbell</h1>
                    <p className="text-blue-100 text-xs opacity-90">Process Engineer</p>
                  </div>
                </div>
                <button
                  onClick={() => setIsSidebarOpen(false)}
                  className="p-1.5 rounded-lg bg-white bg-opacity-20 hover:bg-opacity-30 text-white transition-all duration-200"
                >
                  <FiX size={16} />
                </button>
              </div>
            </div>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-4 py-6 overflow-y-auto scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-transparent">
            <div className="space-y-8">
              {Object.entries(groupedMenuItems).map(([category, items]) => {
                if (category === "System" && role !== "admin") return null;
                return (
                  <div key={category}>
                    <h3 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-4 px-3">
                      {category}
                    </h3>
                    <div className="space-y-1">
                      {items.map(({ label, value, icon }) => (
                        <button
                          key={value}
                          onClick={() => {
                            setView(value);
                            if (isMobile) setIsSidebarOpen(false);
                          }}
                          className={`
                            w-full flex items-center space-x-3 px-3 py-3 rounded-xl text-sm font-medium
                            transition-all duration-200 group relative overflow-hidden
                            ${
                              view === value
                                ? "bg-gradient-to-r from-blue-500 to-blue-600 text-white shadow-lg shadow-blue-500/25 transform scale-[1.02]"
                                : "text-gray-700 hover:bg-gray-50 hover:text-blue-600"
                            }
                          `}
                        >
                          {view === value && (
                            <div className="absolute inset-0 bg-gradient-to-r from-blue-400 to-blue-500 opacity-20 animate-pulse"></div>
                          )}
                          <span
                            className={`
                              transition-all duration-200 relative z-10
                              ${
                                view === value
                                  ? "text-white transform scale-110"
                                  : "text-gray-500 group-hover:text-blue-600 group-hover:scale-110"
                              }
                            `}
                          >
                            {icon}
                          </span>
                          <span className="truncate relative z-10">{label}</span>
                          {/* {view === value && (
                            <div className="ml-auto w-2 h-2 bg-white rounded-full animate-pulse"></div>
                          )} */}
                        </button>
                      ))}
                    </div>
                  </div>
                );
              })}
            </div>
          </nav>

          {/* Sidebar Footer */}
          <div className="px-6 py-4 bg-gray-50 border-t border-gray-200">
            <p className="text-xs text-gray-500 text-center">
              © {new Date().getFullYear()} MBK Corp. All rights reserved.
            </p>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="bg-white shadow-sm border-b border-gray-200 relative z-20">
          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                {/* Menu Button */}
                <button
                  onClick={() => setIsSidebarOpen(true)}
                  className={`
                    p-2.5 rounded-xl bg-blue-600 text-white shadow-md hover:shadow-lg
                    hover:bg-blue-700 transition-all duration-200 transform hover:scale-105
                    ${
                      isSidebarOpen && !isMobile
                        ? "opacity-0 pointer-events-none scale-95"
                        : "opacity-100"
                    }
                  `}
                >
                  <FiMenu size={18} />
                </button>

                {/* Page Info */}
                <div className="flex items-center space-x-4">
                  <div className="w-px h-8 bg-gray-200"></div>
                  <div>
                    <h1 className="text-xl font-semibold text-gray-900">
                      {menuItems.find((m) => m.value === view)?.label}
                    </h1>
                    <p className="text-sm text-gray-500 flex items-center space-x-1">
                      <span>{menuItems.find((m) => m.value === view)?.category}</span>
                      {/* <span className="w-1 h-1 bg-gray-400 rounded-full"></span> */}
                      {/* <span className="text-xs">Dashboard</span> */}
                    </p>
                  </div>
                </div>
              </div>

              {/* User Menu */}
              <div className="flex items-center space-x-4">
                <div className="hidden md:flex items-center space-x-2 px-3 py-2 bg-gray-50 rounded-lg">
                  <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
                  <span className="text-sm text-gray-600">Online</span>
                </div>
                <UserMenu
                  username={username}
                  onLogout={() => {
                    setIsLoggedIn(false);
                    setUsername("");
                    setAuthPage("login");
                    localStorage.removeItem("token");
                  }}
                />
              </div>
            </div>
          </div>
        </header>

        {/* Content Area */}
        <div className="flex-1 overflow-y-auto bg-gray-50">
          <div className="p-6">
            <div className="max-w-full mx-auto">
              {/* Content Card */}
              <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden min-h-[calc(100vh-12rem)]">
                {/* Content Header */}
                {/* <div className="px-6 py-4 bg-gray-50 border-b border-gray-200">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-3">
                      <div className="p-2 bg-blue-100 rounded-lg">
                        <span className="text-blue-600">
                          {menuItems.find((m) => m.value === view)?.icon}
                        </span>
                      </div>
                      <div>
                        <h2 className="font-semibold text-gray-900">
                          {menuItems.find((m) => m.value === view)?.label}
                        </h2>
                        <p className="text-xs text-gray-500">
                          Manage and view {menuItems.find((m) => m.value === view)?.label.toLowerCase()} data
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center space-x-2">
                      <div className="w-2 h-2 bg-blue-500 rounded-full animate-pulse"></div>
                      <span className="text-xs text-gray-500">Live</span>
                    </div>
                  </div>
                </div> */}

                {/* Content Body */}
                <div className="p-6">
                  {renderView()}
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default App;
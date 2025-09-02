// src/components/TopBar.tsx
import React from "react";
import { FiUser, FiLogOut } from "react-icons/fi";

interface TopBarProps {
  username: string;
  onLogout: () => void;
}

const TopBar: React.FC<TopBarProps> = ({ username, onLogout }) => {
  return (
    <div className="w-full h-12 bg-black flex justify-between items-center px-4 text-white">
      {/* Left: Logo and text */}
      <div className="flex items-center gap-2">
        <img
          src="/src/assets/mattel_logo.gif"
          alt="Logo"
          className="h-8 w-auto object-contain"
        />
        <span className="font-semibold text-lg">MBK Barbell</span>
      </div>

      {/* Right: Profile and logout */}
      <div className="flex items-center gap-4">
        <span className="flex items-center gap-2">
          <FiUser size={18} />
          <span className="text-sm">{username}</span>
        </span>
        <button
          onClick={onLogout}
          className="flex items-center gap-2 text-xs bg-red-600 hover:bg-red-700 px-3 py-1 rounded"
        >
          <FiLogOut size={16} />
          Logout
        </button>
      </div>
    </div>
  );
};

export default TopBar;

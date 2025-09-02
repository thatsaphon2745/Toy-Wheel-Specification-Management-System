import React, { useState, useRef, useEffect } from "react";
import { createPortal } from "react-dom";
import { FiLogOut, FiUser } from "react-icons/fi";

interface UserMenuProps {
  username: string;
  onLogout: () => void;
}

const UserMenu: React.FC<UserMenuProps> = ({ username, onLogout }) => {
  const [open, setOpen] = useState(false);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const [position, setPosition] = useState<{ top: number; left: number } | null>(null);

  useEffect(() => {
    if (open && buttonRef.current) {
      const rect = buttonRef.current.getBoundingClientRect();
      setPosition({
        top: rect.bottom + window.scrollY + 8,
        left: rect.right + window.scrollX - 192,
      });
    }
  }, [open]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        open &&
        !buttonRef.current?.contains(event.target as Node) &&
        !dropdownRef.current?.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [open]);

  const dropdown = (
    <div
      ref={dropdownRef}
      className="rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5 z-[9999] w-48"
      style={{
        position: "fixed",
        top: position?.top ?? 0,
        left: position?.left ?? 0,
      }}
    >
      <div className="py-1">
        <div className="px-4 py-2 text-sm text-gray-700 flex items-center gap-2">
          <FiUser className="text-gray-500" /> {username}
        </div>
        <button
          onClick={onLogout}
          className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100 flex items-center gap-2"
        >
          <FiLogOut /> Logout
        </button>
      </div>
    </div>
  );

  return (
    <>
      <button
        ref={buttonRef}
        onClick={() => setOpen((prev) => !prev)}
        className="flex items-center justify-center w-10 h-10 rounded-full bg-blue-600 text-white hover:bg-blue-700 transition"
      >
        <span className="text-sm font-bold">
          {username.charAt(0).toUpperCase()}
        </span>
      </button>

      {open && createPortal(dropdown, document.body)}
    </>
  );
};

export default UserMenu;

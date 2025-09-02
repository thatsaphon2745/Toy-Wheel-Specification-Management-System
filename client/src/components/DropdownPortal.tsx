// components/DropdownPortal.tsx
import React, { useEffect, useState } from "react";
import ReactDOM from "react-dom";

interface DropdownPortalProps {
  anchorRef: React.RefObject<HTMLElement>;
  isOpen: boolean;
  children: React.ReactNode;
}

const DropdownPortal: React.FC<DropdownPortalProps> = ({
  anchorRef,
  isOpen,
  children,
}) => {
  const [position, setPosition] = useState({ top: 0, left: 0 });

  useEffect(() => {
    if (isOpen && anchorRef.current) {
      const rect = anchorRef.current.getBoundingClientRect();
      setPosition({
        top: rect.bottom + window.scrollY + 8,
        left: rect.left + window.scrollX,
      });
    }
  }, [isOpen]);

  if (!isOpen) return null;

  return ReactDOM.createPortal(
    <div
      className="z-[9999] absolute w-96 bg-white border border-gray-200 rounded-xl shadow-xl animate-in fade-in-0 zoom-in-95 duration-200"
      style={{ top: position.top, left: position.left }}
    >
      {children}
    </div>,
    document.body
  );
};

export default DropdownPortal;

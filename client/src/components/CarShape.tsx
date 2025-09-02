// src/components/CarShape.tsx

import React from "react";

const CarShape: React.FC<{
  color?: string;
  style?: React.CSSProperties;
  className?: string;
}> = ({ color = "#FFD700", style, className }) => (
  <svg
    viewBox="0 0 200 60"
    width="120"
    height="36"
    style={style}
    className={className}
  >
    {/* รถแบบสไตล์การ์ตูน */}
    <rect x="10" y="20" width="180" height="20" rx="10" fill={color} />
    <rect x="50" y="5" width="100" height="20" rx="10" fill={color} />
    {/* ล้อหน้า */}
    <circle cx="40" cy="45" r="10" fill="#333" />
    {/* ล้อหลัง */}
    <circle cx="160" cy="45" r="10" fill="#333" />
  </svg>
);

export default CarShape;

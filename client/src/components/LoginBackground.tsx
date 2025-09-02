import React from "react";
import "./LoginBackground.css";

const LoginBackground: React.FC = () => {
  const particles = Array.from({ length: 50 });

  return (
    <div className="login-background">
      {particles.map((_, index) => {
        const left = Math.random() * 100;
        const delay = Math.random() * 20;
        const duration = 15 + Math.random() * 10;
        const size = 4 + Math.random() * 8;

        return (
          <span
            key={index}
            className="particle"
            style={{
              left: `${left}%`,
              animationDelay: `${delay}s`,
              animationDuration: `${duration}s`,
              width: `${size}px`,
              height: `${size}px`,
            }}
          ></span>
        );
      })}
    </div>
  );
};

export default LoginBackground;

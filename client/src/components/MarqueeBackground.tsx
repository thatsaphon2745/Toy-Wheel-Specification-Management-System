// src/components/MarqueeBackground.tsx

import React from "react";
import "./marquee-background.css";
import hotWheelsLogo from "../assets/hotwheels.png";
import matchboxLogo from "../assets/matchbox.png";
import pixarLogo from "../assets/pixar.png";
import marioLogo from "../assets/mariokart.png";
import thomasLogo from "../assets/t&f.png";

const MarqueeBackground: React.FC = () => {
  return (
    <div className="background-container">
      {/* Logo Top-Right */}
      <div className="logo-container">
        <img src={hotWheelsLogo} alt="Hot Wheels" className="logo-img" />
        <img src={matchboxLogo} alt="MatchBox" className="logo-img" />
        <img src={pixarLogo} alt="Pixar" className="logo-img" />
        <img src={marioLogo} alt="Pixar" className="logo-img" />
        <img src={thomasLogo} alt="Pixar" className="logo-img" />
        {/* ถ้าอยากเปลี่ยน logo ให้สลับ comment ด้านล่างได้ */}
        {/* <img src={matchboxLogo} alt="Matchbox" className="logo-img" /> */}
        {/* <img src={carsLogo} alt="Cars" className="logo-img" /> */}
      </div>

      {/* Shape layers */}
      <div
        className="shape shape-rectangle"
        style={{ top: "10%", left: "5%", width: "120px", height: "60px" }}
      ></div>
      <div
        className="shape shape-circle"
        style={{ top: "20%", left: "70%", width: "80px", height: "80px" }}
      ></div>
      <div
        className="shape shape-pill"
        style={{ top: "50%", left: "30%", width: "200px", height: "40px" }}
      ></div>
      <div
        className="shape shape-circle"
        style={{ bottom: "15%", left: "15%", width: "60px", height: "60px" }}
      ></div>
      <div
        className="shape shape-rectangle"
        style={{ bottom: "10%", right: "10%", width: "100px", height: "50px" }}
      ></div>
    </div>
  );
};

export default MarqueeBackground;

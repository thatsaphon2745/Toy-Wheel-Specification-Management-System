import React from "react";
const ChatbotIframe: React.FC = () => {
  return (
    <iframe
      src="https://copilotstudio.microsoft.com/environments/Default-5f40b94d-de92-4c81-a62a-4014455791e6/bots/cre46_mbkBarbellQA/webchat?__version__=2"
      frameBorder={0}
      style={{
        width: "100%",
        height: "80vh", // หรือ 90vh
        border: "2px solid #1890ff",
        borderRadius: "8px",
        boxShadow: "0 4px 10px rgba(0,0,0,0.3)",
      }}
      title="MBK Barbell Copilot Chatbot"
    ></iframe>
  );
};
export default ChatbotIframe;
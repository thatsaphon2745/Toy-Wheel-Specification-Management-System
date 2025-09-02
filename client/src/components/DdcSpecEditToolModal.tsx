import React, { useState } from "react";
import axios from "axios";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";

import { createPortal } from "react-dom";

const MySwal = withReactContent(Swal);

interface Props {
  tool_spec_id: number;
  ref_key_id: number;
  initialChassisSpanOverride: number;
  initialDescription?: string; // ✅ NEW
  displayInfo: {
    tool_type: string;
    tool_name: string;
    position_type: string;
    type_ref: string;
    tool_ref: string;
    size_ref: string;
    axle_type: string;
  };
  onClose: () => void;
  onSubmitSuccess: () => void;
}

const DdcSpecEditToolModal: React.FC<Props> = ({
  tool_spec_id,
  ref_key_id,
  initialChassisSpanOverride,
  initialDescription, // ✅ เพิ่มบรรทัดนี้
  displayInfo,
  onClose,
  onSubmitSuccess,
}) => {
  const [chassisSpan, setChassisSpan] = useState<number | null>(
    initialChassisSpanOverride ?? null
  );
  const [description, setDescription] = useState<string>(
    initialDescription ?? ""
  ); // ✅

  // const handleSubmit = async () => {
  //   console.log("📤 Submitting update for tool_spec_id:", tool_spec_id);
  //   console.log("➡️ Payload:", {
  //     chassis_span_override: chassisSpan,
  //   });

  //   // ✅ Validate
  //   if (chassisSpan === null || isNaN(chassisSpan)) {
  //     await MySwal.fire({
  //       icon: "warning",
  //       title: "Invalid Input",
  //       text: "Please enter a valid chassis span (must be a number).",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });
  //     console.warn("❌ Invalid chassis_span_override:", chassisSpan);
  //     return;
  //   }

  //   const confirm = await MySwal.fire({
  //     title: "Confirm Update?",
  //     icon: "question",
  //     showCancelButton: true,
  //     confirmButtonText: "Yes, Save",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       confirmButton:
  //         "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //       popup: "text-sm",
  //     },
  //   });

  //   if (!confirm.isConfirmed) {
  //     console.log("❌ User cancelled update.");
  //     return;
  //   }

  //   const token = localStorage.getItem("token");
  //   console.log("TOKEN:", token);

  //   if (token) {
  //     const payloadBase64 = token.split(".")[1];
  //     const payloadJson = atob(payloadBase64);
  //     const payload = JSON.parse(payloadJson);
  //     console.log("Decoded Payload:", payload);
  //     console.log("ROLE FROM TOKEN:", payload.role);
  //   }

  //   try {
  //     console.log("🚀 Sending PUT request...");

  //     const res = await axios.put(
  //       `/api/ToolSpecs/${tool_spec_id}`,
  //       {
  //         chassis_span_override: parseFloat(chassisSpan.toString()),
  //       },
  //       {
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     console.log("✅ PUT success:", res.status, res.statusText);

  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Updated",
  //       text: "Chassis span override has been updated successfully.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     console.log("🔁 Triggering onSubmitSuccess + closing modal");
  //     onSubmitSuccess();
  //     onClose();
  //   } catch (err: unknown) {
  //     console.error("🔥 Update failed:", err);

  //     let errorMsg = "Unknown error";

  //     if (axios.isAxiosError(err)) {
  //       if (typeof err.response?.data === "string") {
  //         errorMsg = err.response.data;
  //       } else if (err.response?.data?.message) {
  //         errorMsg = err.response.data.message;
  //       }
  //     } else if (err && typeof err === "object" && "message" in err) {
  //       errorMsg = (err as any).message;
  //     }

  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Error",
  //       text: `Failed to update chassis span override.\n${errorMsg}`,
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });
  //   }
  // };

  // const handleSubmit = async () => {
  //   console.log("📤 Submitting update for tool_spec_id:", tool_spec_id);
  //   console.log("➡️ New chassis_span_override:", chassisSpan);

  //   // ✅ Validate
  //   if (chassisSpan === null || isNaN(chassisSpan)) {
  //     await MySwal.fire({
  //       icon: "warning",
  //       title: "Invalid Input",
  //       text: "Please enter a valid chassis span (must be a number).",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });
  //     console.warn("❌ Invalid chassis_span_override:", chassisSpan);
  //     return;
  //   }

  //   const confirm = await MySwal.fire({
  //     title: "Confirm Update?",
  //     text: "Do you want to submit this update request for approval?",
  //     icon: "question",
  //     showCancelButton: true,
  //     confirmButtonText: "Yes, Submit",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       confirmButton:
  //         "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //       popup: "text-sm",
  //     },
  //   });

  //   if (!confirm.isConfirmed) {
  //     console.log("❌ User cancelled update.");
  //     return;
  //   }

  //   const token = localStorage.getItem("token");
  //   console.log("TOKEN:", token);

  //   if (token) {
  //     const payloadBase64 = token.split(".")[1];
  //     const payloadJson = atob(payloadBase64);
  //     const payload = JSON.parse(payloadJson);
  //     console.log("Decoded Payload:", payload);
  //     console.log("ROLE FROM TOKEN:", payload.role);
  //   }

  //   try {
  //     // ✅ fetch old data
  //     console.log("🔍 Fetching old data for tool_spec_id:", tool_spec_id);
  //     const oldDataRes = await fetch(
  //       `/api/ToolSpecs/${tool_spec_id}`,
  //       {
  //         headers: {
  //           ...(token && { Authorization: `Bearer ${token}` }),
  //         },
  //       }
  //     );

  //     let oldData = null;

  //     if (oldDataRes.ok) {
  //       oldData = await oldDataRes.json();
  //       console.log("✅ Old Data:", oldData);
  //     } else {
  //       console.error("⚠️ Could not fetch old data for ToolSpecs");
  //     }

  //     // ✅ build changed data
  //     const changedData: Record<string, any> = {};

  //     if (oldData) {
  //       const oldVal = oldData["chassis_span_override"];
  //       const newVal = parseFloatSafe(chassisSpan);

  //       let isSame = false;

  //       if (
  //         (oldVal === null || oldVal === undefined) &&
  //         (newVal === null || newVal === undefined)
  //       ) {
  //         isSame = true;
  //       } else {
  //         isSame = Number(oldVal) === Number(newVal);
  //       }

  //       if (!isSame) {
  //         changedData["chassis_span_override"] = newVal;
  //       }
  //     } else {
  //       // ถ้า fetch ไม่ได้ → ส่งทุกค่า
  //       changedData["chassis_span_override"] = parseFloatSafe(chassisSpan);
  //     }

  //     console.log("✅ Changed Data:", changedData);

  //     if (Object.keys(changedData).length === 0) {
  //       await MySwal.fire({
  //         icon: "info",
  //         title: "No Changes",
  //         text: "No data was changed. Nothing to submit.",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
  //           popup: "text-sm",
  //         },
  //       });
  //       return;
  //     }

  //     const requestPayload = {
  //       request_type: "UPDATE",
  //       target_table: "DdcSpec",
  //       target_pk_id: tool_spec_id,
  //       old_data: oldData,
  //       new_data: changedData,
  //       note: null,
  //     };

  //     console.log("🚀 Submitting UPDATE request payload:", requestPayload);

  //     const res = await axios.post(
  //       "/api/Requests",
  //       requestPayload,
  //       {
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     console.log("✅ Request submitted:", res.data);

  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Request Submitted",
  //       text: "Your update request has been submitted and is pending approval.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     console.log("🔁 Triggering onSubmitSuccess + closing modal");
  //     onSubmitSuccess();
  //     onClose();
  //   } catch (err: unknown) {
  //     console.error("🔥 Update request submission failed:", err);

  //     let errorMsg = "Failed to submit the update request.";

  //     if (axios.isAxiosError(err)) {
  //       if (typeof err.response?.data === "string") {
  //         errorMsg = err.response.data;
  //       } else if (err.response?.data?.message) {
  //         errorMsg = err.response.data.message;
  //       }
  //     } else if (err && typeof err === "object" && "message" in err) {
  //       errorMsg = (err as any).message;
  //     }

  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Request Failed",
  //       text: errorMsg,
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });
  //   }
  // };

  const handleSubmit = async () => {
    console.log("📤 Submitting update for tool_spec_id:", tool_spec_id);
    console.log("➡️ New chassis_span_override:", chassisSpan);
    console.log("📄 Description:", description); // ✅ NEW

    if (chassisSpan === null || isNaN(chassisSpan)) {
      await MySwal.fire({
        icon: "warning",
        title: "Invalid Input",
        text: "Please enter a valid chassis span (must be a number).",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });
      return;
    }

    const confirm = await MySwal.fire({
      title: "Confirm Update?",
      text: "Do you want to submit this update request for approval?",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Yes, Submit",
      cancelButtonText: "Cancel",
      customClass: {
        confirmButton:
          "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
        cancelButton:
          "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
        popup: "text-sm",
      },
    });

    if (!confirm.isConfirmed) {
      console.log("❌ User cancelled update.");
      return;
    }

    const token = localStorage.getItem("token");
    console.log("TOKEN:", token);

    if (token) {
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      console.log("Decoded Payload:", payload);
      console.log("ROLE FROM TOKEN:", payload.role);
    }

    try {
      // ✅ fetch old data
      const oldDataRes = await fetch(
        `/api/ToolSpecs/${tool_spec_id}`,
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      const normalize = (val: any) => {
        if (val === null || val === undefined) return "";
        if (typeof val !== "string") return val.toString().trim();
        return val.trim();
      };

      let oldData: Record<string, any> = {};
      if (oldDataRes.ok) {
        oldData = await oldDataRes.json();
        console.log("🔎 old.description (raw):", oldData?.description);
        console.log("🔎 new description (raw):", description);
        console.log("🔎 old normalized:", normalize(oldData?.description));
        console.log("🔎 new normalized:", normalize(description));
        console.log(
          "🧪 Description equal?",
          normalize(oldData?.description) === normalize(description)
        );
        console.log("✅ Old Data:", oldData);
      }

      // ✅ cleaned newData
      const cleanedChassis = parseFloatSafe(chassisSpan);
      const newData: Record<string, any> = {
        tool_type: displayInfo.tool_type,
        tool_name: displayInfo.tool_name,
        position_type: displayInfo.position_type,
        type_ref: displayInfo.type_ref,
        tool_ref: displayInfo.tool_ref,
        size_ref: displayInfo.size_ref,
        axle_type: displayInfo.axle_type,
        chassis_span_override: cleanedChassis,
        description: description, // ✅ ADD THIS
      };

      const changedData: Record<string, any> = {};

      // ✅ ตรวจ chassis_span_override เปลี่ยนมั้ย พร้อม log
      const oldChassis = oldData?.chassis_span_override;
      console.log("🔍 old chassis_span_override (raw):", oldChassis);
      console.log("🔍 new chassis_span_override (cleaned):", cleanedChassis);

      const isChassisSame =
        (oldChassis === null || oldChassis === undefined) &&
        (cleanedChassis === null || cleanedChassis === undefined)
          ? true
          : Number(oldChassis) === Number(cleanedChassis);

      console.log("🧪 Chassis override equal?", isChassisSame);

      if (!isChassisSame) {
        changedData["chassis_span_override"] = cleanedChassis;
      }

      // ✅ ตรวจ description เปลี่ยนมั้ย (แบบ normalize)
      if (normalize(oldData?.description) !== normalize(description)) {
        changedData["description"] = description;
      }

      if (Object.keys(changedData).length === 0) {
        await MySwal.fire({
          icon: "info",
          title: "No Changes",
          text: "No data was changed. Nothing to submit.",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
            popup: "text-sm",
          },
        });
        return;
      }

      // ✅ include readable fields in changedData + oldData
      const readableKeys = [
        "tool_type",
        "tool_name",
        "type_ref",
        "tool_ref",
        "size_ref",
        "axle_type",
        "position_type",
      ];

      readableKeys.forEach((key) => {
        const value = displayInfo[key as keyof typeof displayInfo];
        if (value !== undefined) {
          if (!(key in oldData)) oldData[key] = value;
          if (!(key in changedData)) changedData[key] = value;
        }
      });

      // ✅ submit
      const requestPayload = {
        request_type: "UPDATE",
        target_table: "DdcSpec",
        target_pk_id: tool_spec_id,
        old_data: oldData,
        new_data: newData,
        note: null,
      };

      console.log("🚀 Submitting UPDATE request payload:", requestPayload);

      const res = await axios.post(
        "/api/Requests",
        requestPayload,
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      // await MySwal.fire({
      //   icon: "success",
      //   title: "Request Submitted",
      //   text: "Your update request has been submitted and is pending approval.",
      //   confirmButtonText: "OK",
      //   customClass: {
      //     confirmButton:
      //       "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
      //     popup: "text-sm",
      //   },
      // });

      await MySwal.fire({
        icon: "success",
        title: "Submitted!",
        html: `Your request has been submitted and is pending approval.<br/><br/>
         <b>Request ID:</b> <span class="text-blue-600">${res.data.request_id}</span>`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });

      onSubmitSuccess();
      onClose();
    } catch (err: any) {
      console.error("🔥 Update request submission failed:", err);

      let errorMsg = "Failed to submit the update request.";
      if (axios.isAxiosError(err)) {
        if (typeof err.response?.data === "string") {
          errorMsg = err.response.data;
        } else if (err.response?.data?.message) {
          errorMsg = err.response.data.message;
        }
      } else if (err?.message) {
        errorMsg = err.message;
      }

      await MySwal.fire({
        icon: "error",
        title: "Request Failed",
        text: errorMsg,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });
    }
  };

  const renderReadOnly = (label: string, value: string) => (
    <div>
      <label className="text-xs font-medium text-gray-600 mb-1 block">
        {label}
      </label>
      <input
        value={value}
        disabled
        className="w-full border px-3 py-1.5 rounded text-sm bg-gray-100 text-gray-600"
      />
    </div>
  );

  const renderNumberInput = () => (
    <div>
      <label className="text-xs font-medium text-gray-700 mb-1 block">
        Chassis Span Override
      </label>
      <input
        type="number"
        step="any"
        min="0"
        value={chassisSpan ?? ""}
        onChange={(e) =>
          setChassisSpan(
            e.target.value === "" ? null : parseFloat(e.target.value)
          )
        }
        className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
      />
    </div>
  );
  const renderDescriptionInput = () => (
    <div>
      <label className="text-xs font-medium text-gray-700 mb-1 block">
        Description (optional)
      </label>
      <textarea
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
        rows={3}
        placeholder="Enter description or note..."
      />
    </div>
  );

  const parseFloatSafe = (val: any): number | null => {
    if (val === null || val === undefined || val === "") return null;

    const num = parseFloat(val.toString());
    return isNaN(num) ? null : num;
  };

  return createPortal(
    <div className="fixed inset-0 bg-black bg-opacity-30 z-[999] flex justify-center items-center">
      <div className="bg-white p-6 rounded-2xl w-full max-w-5xl max-h-[90vh] overflow-auto shadow-xl">
        <h2 className="text-xl font-semibold text-gray-800 mb-6">
          Edit DDC Spec
        </h2>

        {/* 🔍 Tool Info */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">🛠 Tool Info</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {renderReadOnly("Tool Type", displayInfo.tool_type)}
            {renderReadOnly("Tool Name", displayInfo.tool_name)}
            {renderReadOnly("Position Type", displayInfo.position_type)}
            {renderReadOnly("Type Ref", displayInfo.type_ref)}
            {renderReadOnly("Tool Ref", displayInfo.tool_ref)}
            {renderReadOnly("Size Ref", displayInfo.size_ref)}
            {renderReadOnly("Axle Type", displayInfo.axle_type)}
          </div>
        </div>

        {/* ✏️ Editable */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            ✏️ Chassis Span
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {renderNumberInput()}
          </div>
          {renderDescriptionInput()} {/* ✅ ADD THIS */}
        </div>

        {/* 🎯 Action Buttons */}
        <div className="flex justify-end gap-3 mt-6">
          <button
            onClick={onClose}
            className="border border-gray-400 px-5 py-1.5 rounded text-sm hover:bg-gray-100"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            className="bg-blue-600 text-white px-5 py-1.5 rounded text-sm hover:bg-blue-700"
          >
            Save
          </button>
        </div>
      </div>
    </div>,
    document.body
  );
};

export default DdcSpecEditToolModal;

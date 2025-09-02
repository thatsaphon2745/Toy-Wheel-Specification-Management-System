import React, { useState } from "react";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
import axios from "axios";
import { createPortal } from "react-dom";

const MySwal = withReactContent(Swal);

interface Props {
  initialData: {
    tool_ref_spec_id: number;
    overall_a: string | number;
    overall_b: string | number;
    overall_c: string | number;
    tolerance_a: string | number;
    tolerance_b: string | number;
    tolerance_c: string | number;
    f_shank_min: string | number;
    f_shank_max: string | number;
    b2b_min: string | number;
    b2b_max: string | number;
    h2h_min: string | number;
    h2h_max: string | number;
    chassis_span1: string | number;
    chassis_span2: string | number;
    source: string;
    tool_type: string;
    tool_name: string;
    size_ref: string;
    axle_type: string;
    is_original_spec: number;
    knurling_type: number; // 👈 ใส่อันนี้เพิ่มเข้าไป
    description?: string; // 👈 ใส่อันนี้เพิ่มเข้าไป
  };
  onClose: () => void;
  onSubmitSuccess: () => void;
}

const OriginalSpecEditToolModal: React.FC<Props> = ({
  initialData,
  onClose,
  onSubmitSuccess,
}) => {
  const [form, setForm] = useState<{ [key: string]: number | string }>({
    ...initialData,
  });

  const renderFloatInput = (key: string, label: string) => (
    <div className="flex flex-col gap-1">
      <label className="text-xs font-medium text-gray-600">{label}</label>
      <input
        type="number"
        step="any"
        min="0"
        value={form[key] === null ? "" : form[key]}
        onChange={(e) =>
          setForm({
            ...form,
            [key]: e.target.value,
          })
        }
        className="border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
      />
    </div>
  );

  const renderReadOnlyInput = (
    label: string,
    value: string | number | null
  ) => (
    <div>
      <label className="text-xs font-medium text-gray-600 mb-1 block">
        {label}
      </label>
      <input
        value={value ?? "(Blanks)"}
        disabled
        className="w-full border px-3 py-1.5 rounded text-sm bg-gray-100 text-gray-600"
      />
    </div>
  );

  // const handleSubmit = async () => {
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
  //     const cleanedForm = Object.fromEntries(
  //       Object.entries(form).map(([key, value]) => [
  //         key,
  //         value === "" || value === undefined || value === null
  //           ? null
  //           : !isNaN(Number(value))
  //           ? parseFloatSafe(value)
  //           : value,
  //       ])
  //     );

  //     const res = await fetch(
  //       `/api/ToolRefSpecs/${form.tool_ref_spec_id}`,
  //       {
  //         method: "PUT",
  //         headers: {
  //           "Content-Type": "application/json",
  //           ...(token && { Authorization: `Bearer ${token}` }),
  //         },
  //         body: JSON.stringify(cleanedForm),
  //       }
  //     );

  //     if (!res.ok) {
  //       const text = await res.text();
  //       let errorMsg = text;

  //       try {
  //         const json = JSON.parse(text);
  //         errorMsg = json?.message || text;
  //       } catch {
  //         console.error("❌ Response not JSON:", text);
  //       }

  //       await MySwal.fire({
  //         icon: "error",
  //         title: "Update Failed",
  //         text: errorMsg,
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //           popup: "text-sm",
  //         },
  //       });

  //       return;
  //     }

  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Updated",
  //       text: "Tool spec has been updated successfully.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     onSubmitSuccess();
  //     onClose();
  //   } catch (err: any) {
  //     console.error("🔥 Update failed:", err);

  //     let errorMsg = "Failed to update the tool spec.";
  //     if (err?.message) {
  //       errorMsg = err.message;
  //     }

  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Error",
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
  const fallbackIfBlank = (val: any) =>
    val === null || val === undefined || val === "" ? "(Blanks)" : val;

  // const handleSubmit = async () => {
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
  //     // ✅ clean form
  //     const cleanedForm = Object.fromEntries(
  //       Object.entries(form).map(([key, value]) => [
  //         key,
  //         value === "" || value === undefined || value === null
  //           ? null
  //           : !isNaN(Number(value))
  //           ? parseFloatSafe(value)
  //           : value,
  //       ])
  //     );

  //     // ✅ remove read-only fields
  //     const {
  //       tool_type,
  //       tool_name,
  //       size_ref,
  //       axle_type,
  //       is_original_spec,
  //       ...editableCleanedForm
  //     } = cleanedForm;

  //     // ✅ fetch old data
  //     const oldDataRes = await fetch(
  //       `/api/ToolRefSpecs/${form.tool_ref_spec_id}`,
  //       {
  //         headers: {
  //           ...(token && { Authorization: `Bearer ${token}` }),
  //         },
  //       }
  //     );

  //     // let oldData = null;

  //     let oldData: Record<string, any> = {};

  //     if (oldDataRes.ok) {
  //       oldData = await oldDataRes.json();
  //       console.log("✅ Old Data:", oldData);
  //     } else {
  //       console.error("⚠️ Could not fetch old data");
  //     }

  //     // ✅ always send full new_data
  //     const newData = {
  //       ...form,
  //       ...editableCleanedForm, // (optional: แต่ถ้า cleaned แล้วก็ใช้ได้)
  //     };

  //     const readableKeys = ["tool_type", "tool_name", "size_ref", "axle_type"];

  //     readableKeys.forEach((key) => {
  //       const value = form[key]; // ✅ ใช้ form แทน displayInfo
  //       const fallback = fallbackIfBlank(value);

  //       if (oldData[key] == null) oldData[key] = fallback;
  //       if (newData[key] == null) newData[key] = fallback; // ✅ ใช้ newData แทน changedData
  //     });

  //     const requestPayload = {
  //       request_type: "UPDATE",
  //       target_table: "OriginalSpec",
  //       target_pk_id: form.tool_ref_spec_id,
  //       old_data: oldData,
  //       new_data: newData,
  //       note: form.note || null,
  //     };

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

  //     onSubmitSuccess();
  //     onClose();
  //   } catch (err: any) {
  //     console.error("🔥 Update request submission failed:", err);

  //     let errorMsg = "Failed to submit the update request.";
  //     if (axios.isAxiosError(err)) {
  //       if (typeof err.response?.data === "string") {
  //         errorMsg = err.response.data;
  //       } else if (err.response?.data?.message) {
  //         errorMsg = err.response.data.message;
  //       }
  //     } else if (err?.message) {
  //       errorMsg = err.message;
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
      const parseFloatSafe = (val: any) => {
        const num = parseFloat(val);
        return isNaN(num) ? null : num;
      };

      const normalize = (val: any) => {
        if (val === null || val === undefined || val === "") return "";
        if (typeof val !== "string") return val.toString().trim();
        return val.trim();
      };

      const cleanedForm = Object.fromEntries(
        Object.entries(form).map(([key, value]) => [
          key,
          value === "" || value === undefined || value === null
            ? null
            : !isNaN(Number(value))
            ? parseFloatSafe(value)
            : value,
        ])
      );

      const {
        tool_type,
        tool_name,
        size_ref,
        axle_type,
        is_original_spec,
        ...editableCleanedForm
      } = cleanedForm;

      const oldDataRes = await fetch(
        `/api/ToolRefSpecs/${form.tool_ref_spec_id}`,
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      let oldData: Record<string, any> = {};
      if (oldDataRes.ok) {
        oldData = await oldDataRes.json();
        console.log("✅ Old Data:", oldData);
      } else {
        console.error("⚠️ Could not fetch old data");
      }

      // ✅ ตรวจว่าไม่มี field ไหนเปลี่ยน → ไม่ต้องส่ง request
      const hasChanges = Object.entries(editableCleanedForm).some(
        ([key, value]) => normalize(oldData[key]) !== normalize(value)
      );

      if (!hasChanges) {
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

      // ✅ เตรียม newData เต็ม
      const newData = {
        ...form,
        ...editableCleanedForm,
      };

      const readableKeys = ["tool_type", "tool_name", "size_ref", "axle_type"];
      const fallbackIfBlank = (val: any) =>
        val === null || val === undefined || val === "" ? "(Blanks)" : val;

      readableKeys.forEach((key) => {
        const value = form[key];
        const fallback = fallbackIfBlank(value);

        if (oldData[key] == null) oldData[key] = fallback;
        if (newData[key] == null) newData[key] = fallback;
      });

      const requestPayload = {
        request_type: "UPDATE",
        target_table: "OriginalSpec",
        target_pk_id: form.tool_ref_spec_id,
        old_data: oldData,
        new_data: newData,
        note: form.note || null,
      };

      const res = await axios.post(
        "/api/Requests",
        requestPayload,
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      console.log("✅ Request submitted:", res.data);

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
        icon: "info",
        title: "Request Submitted",
        html: `Your update request to edit original tool has been submitted.<br>Request ID: <b>${res.data.request_id}</b><br>It will be effective after admin approval.`,
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

  // ✅ Helper function
  const parseFloatSafe = (val: any) => {
    const num = parseFloat(val);
    return isNaN(num) ? null : num;
  };

  return createPortal(
    <div className="fixed inset-0 bg-black bg-opacity-30 z-[999] flex justify-center items-center">
      <div className="bg-white p-6 rounded-2xl w-full max-w-5xl max-h-[90vh] overflow-auto shadow-xl">
        <h2 className="text-xl font-semibold text-gray-800 mb-6">
          Edit Tool Spec
        </h2>

        {/* 🛠 Tool Info */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">🛠 Tool Info</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {renderReadOnlyInput("Tool Type", form.tool_type)}
            {renderReadOnlyInput("Tool Name", form.tool_name)}
            {renderReadOnlyInput("Size Ref", form.size_ref)}
            {renderReadOnlyInput("Axle Type", form.axle_type)}
          </div>
        </div>

        {/* ⚙️ Configuration */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            ⚙️ Configuration
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            {renderReadOnlyInput(
              "Is Original Spec",
              form.is_original_spec === 1 ? "Yes" : "No"
            )}
            {renderReadOnlyInput("Knurling Type", form.knurling_type)}
          </div>
        </div>

        {/* ✏️ Editable Fields */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            📐 Over All Dimension (mm)
          </h3>
          <div className="grid grid-cols-3 gap-4">
            {renderFloatInput("overall_a", "Overall A")}
            {renderFloatInput("overall_b", "Overall B")}
            {renderFloatInput("overall_c", "Overall C")}
            {renderFloatInput("tolerance_a", "Tolerance A")}
            {renderFloatInput("tolerance_b", "Tolerance B")}
            {renderFloatInput("tolerance_c", "Tolerance C")}
          </div>
        </div>

        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            🧱 F-Shank (Inch)
          </h3>
          <div className="grid grid-cols-2 gap-4">
            {renderFloatInput("f_shank_min", "F-Shank Min")}
            {renderFloatInput("f_shank_max", "F-Shank Max")}
          </div>
        </div>

        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            🔄 Bump to Bump (Inch)
          </h3>
          <div className="grid grid-cols-2 gap-4">
            {renderFloatInput("b2b_min", "B2B Min")}
            {renderFloatInput("b2b_max", "B2B Max")}
          </div>
        </div>

        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            ↔️ Head to Head (Inch)
          </h3>
          <div className="grid grid-cols-2 gap-4">
            {renderFloatInput("h2h_min", "H2H Min")}
            {renderFloatInput("h2h_max", "H2H Max")}
          </div>
        </div>

        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            🧩 Chassis Span
          </h3>
          <div className="grid grid-cols-2 gap-4">
            {renderFloatInput("chassis_span1", "Chassis Span 1")}
            {renderFloatInput("chassis_span2", "Chassis Span 2")}
          </div>
        </div>

        <div className="mb-6">
          <label className="text-xs font-medium text-gray-600 mb-1 block">
            Source
          </label>
          <input
            value={form.source}
            onChange={(e) => setForm({ ...form, source: e.target.value })}
            className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
          />
        </div>
        {/* <div className="mb-6">
          <label className="text-xs font-medium text-gray-600 mb-1 block">
            Description
          </label>
          <input
            value={form.description ?? ""}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
            placeholder="Enter description"
          />
        </div> */}
        <div>
          <label className="text-xs font-medium text-gray-700 mb-1 block">
            Description (optional)
          </label>
          <textarea
            value={form.description ?? ""}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
            rows={3}
            placeholder="Enter description or note..."
          />
        </div>

        {/* 🎯 Actions */}
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

export default OriginalSpecEditToolModal;

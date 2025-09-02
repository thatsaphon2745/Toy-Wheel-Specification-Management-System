// import Swal from "sweetalert2";
// import withReactContent from "sweetalert2-react-content";

// const MySwal = withReactContent(Swal);

// export const ToolPadMapEditModal = async (
//   row: {
//     map_id: number;
//     tool_type: string;
//     tool_name: string;
//     type_ref: string | null;
//     tool_ref: string | null;
//     size_ref: string | null;
//     pad_name: string;
//     hst_type: string;
//   },
//   onSuccess: () => void
// ) => {
//   const inputPadId = "swal-pad-search";
//   const inputHstId = "swal-hst-search";

//   try {
//     console.log("🔧 Editing Pad Map", row);

//     const token = localStorage.getItem("token");
//     console.log("TOKEN:", token);

//     if (token) {
//       const payloadBase64 = token.split(".")[1];
//       const payloadJson = atob(payloadBase64);
//       const payload = JSON.parse(payloadJson);
//       console.log("Decoded Payload:", payload);
//       console.log("ROLE FROM TOKEN:", payload.role);
//     }

//     // 👉 Load pads & hstTypes
//     const [padsRes, hstRes] = await Promise.all([
//       fetch("/api/Pads", {
//         headers: { Authorization: `Bearer ${token}` },
//       }),
//       fetch("/api/HstTypes", {
//         headers: { Authorization: `Bearer ${token}` },
//       }),
//     ]);

//     const pads = await padsRes.json();
//     const hstTypes = await hstRes.json();

//     const padOptions: { pad_id: number; pad_name: string }[] = pads || [];
//     const hstOptions: { hst_type_id: number; hst_type: string }[] =
//       hstTypes || [];

//     if (!padOptions.length || !hstOptions.length) {
//       await MySwal.fire({
//         icon: "warning",
//         title: "No Data",
//         text: "Missing pad or HST type data.",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded",
//         },
//       });
//       return;
//     }

//     const escapeHTML = (str: string | null) =>
//       (str ?? "–").replace(/</g, "&lt;").replace(/>/g, "&gt;");

//     const label = (label: string, value: string | null) =>
//       `<tr><td style="padding:4px 8px; vertical-align:top;"><b>${label}:</b></td><td>${
//         escapeHTML(value)
//       }</td></tr>`;

//     const { isConfirmed, value: selected } = await MySwal.fire({
//       title: "<strong>Edit Pad Mapping</strong>",
//       html: `
//         <div style="font-size:15px; text-align:left;">
//           <table style="width:100%; margin-bottom:16px;">
//             ${label("Tool Type", row.tool_type)}
//             ${label("Tool Name", row.tool_name)}
//             ${label("Type Ref", row.type_ref)}
//             ${label("Tool Ref", row.tool_ref)}
//             ${label("Size Ref", row.size_ref)}
//           </table>

//           <div style="margin-bottom:14px;">
//             <label style="font-weight:bold; display:block; margin-bottom:6px;">Pad Name:</label>
//             <input id="${inputPadId}" list="pad-list" value="${escapeHTML(
//         row.pad_name
//       )}"
//               class="swal2-input" style="max-width: 350px; width: 100%;" />
//             <datalist id="pad-list">
//               ${padOptions
//                 .map(
//                   (p) => `<option value="${escapeHTML(p.pad_name)}"></option>`
//                 )
//                 .join("")}
//             </datalist>
//           </div>

//           <div>
//             <label style="font-weight:bold; display:block; margin-bottom:6px;">HST Type:</label>
//             <input id="${inputHstId}" list="hst-list" value="${escapeHTML(
//         row.hst_type
//       )}"
//               class="swal2-input" style="max-width: 350px; width: 100%;" />
//             <datalist id="hst-list">
//               ${hstOptions
//                 .map(
//                   (h) => `<option value="${escapeHTML(h.hst_type)}"></option>`
//                 )
//                 .join("")}
//             </datalist>
//           </div>
//         </div>
//       `,
//       showCancelButton: true,
//       confirmButtonText: "💾 Save",
//       cancelButtonText: "Cancel",
//       focusConfirm: false,
//       customClass: {
//         popup: "swal2-rounded text-sm",
//         confirmButton:
//           "bg-[#2563eb] hover:bg-[#1d4ed8] text-white font-semibold px-5 py-2 rounded-md text-sm",
//         cancelButton:
//           "bg-[#f1f5f9] hover:bg-[#e2e8f0] text-black font-normal px-5 py-2 rounded-md text-sm ml-2",
//       },
//       buttonsStyling: false,
//       preConfirm: () => {
//         const padInput = (
//           document.getElementById(inputPadId) as HTMLInputElement
//         )?.value.trim();
//         const hstInput = (
//           document.getElementById(inputHstId) as HTMLInputElement
//         )?.value.trim();
//         if (!padInput || !hstInput) {
//           MySwal.showValidationMessage("Please select both Pad and HST Type");
//           return null;
//         }
//         return { pad_name: padInput, hst_type: hstInput };
//       },
//     });

//     if (!isConfirmed || !selected) return;

//     const matchedPad = padOptions.find(
//       (p) => p.pad_name === selected.pad_name
//     );
//     const matchedHst = hstOptions.find(
//       (h) => h.hst_type === selected.hst_type
//     );

//     if (!matchedPad || !matchedHst) {
//       await MySwal.fire({
//         icon: "error",
//         title: "Invalid Selection",
//         text: "Pad or HST type not found.",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
//         },
//       });
//       return;
//     }

//     const payload = {
//       pad_id: matchedPad.pad_id,
//       hst_type_id: matchedHst.hst_type_id,
//     };

//     const res = await fetch(
//       `/api/ToolPadMap/${row.map_id}`,
//       {
//         method: "PUT",
//         headers: {
//           "Content-Type": "application/json",
//           Authorization: `Bearer ${token}`,
//         },
//         body: JSON.stringify(payload),
//       }
//     );

//     if (!res.ok) {
//       const error = await res.text();
//       console.error("❌ Update error:", error);
//       await MySwal.fire({
//         icon: "error",
//         title: "Update Failed",
//         text: error || "Server error",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
//         },
//       });
//       return;
//     }

//     console.log(
//       "✅ Update successful! map_id:",
//       row.map_id,
//       "pad_id:",
//       matchedPad.pad_id,
//       "hst_type_id:",
//       matchedHst.hst_type_id
//     );

//     await MySwal.fire({
//       icon: "success",
//       title: "Success",
//       text: "Pad mapping updated!",
//       confirmButtonText: "OK",
//       customClass: {
//         confirmButton:
//           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
//       },
//     });
//     onSuccess();
//   } catch (err: any) {
//     console.error("🔥 Edit pad map error:", err);
//     await MySwal.fire({
//       icon: "error",
//       title: "Error",
//       text: err?.message || "Unexpected error occurred",
//       confirmButtonText: "OK",
//       customClass: {
//         confirmButton:
//           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
//       },
//     });
//   }
// };

import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";

import React, { useEffect, useState } from "react";

const MySwal = withReactContent(Swal);

export interface Option {
  value: number;
  label: string;
}

export const ToolPadMapEditModal = async (
  row: {
    map_id: number;
    tool_type: string;
    tool_name: string;
    type_ref: string | null;
    tool_ref: string | null;
    size_ref: string | null;
    pad_name: string;
    hst_type: string;
    description?: string;
  },
  onSuccess: () => void
) => {
  const inputPadId = "swal-pad-search";
  const inputHstId = "swal-hst-search";

  // const [types, setTypes] = useState<Option[]>([]);
  // const [tools, setTools] = useState<Option[]>([]);
  // const [typeRefs, setTypeRefs] = useState<Option[]>([]);
  // const [toolRefs, setToolRefs] = useState<Option[]>([]);
  // const [sizeRefs, setSizeRefs] = useState<Option[]>([]);
  // const [hstTypes, setHstTypes] = useState<Option[]>([]);
  // const [pads, setPads] = useState<Option[]>([]);

  const getIdByLabel = (
    options: { value: number; label: string }[],
    label: string | null
  ): number | null => {
    return options.find((opt) => opt.label === label)?.value ?? null;
  };

  try {
    console.log("🔧 Editing Pad Map", row);

    const token = localStorage.getItem("token");
    console.log("TOKEN:", token);

    if (token) {
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      console.log("Decoded Payload:", payload);
      console.log("ROLE FROM TOKEN:", payload.role);
    }

    // ✅ Load types, tools, typeRefs, toolRefs, sizeRefs
    const [types, tools, typeRefs, toolRefs, sizeRefs] = await Promise.all([
      fetch("/api/TypeModels", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),
      fetch("/api/Tools", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),
      fetch("/api/TypeModels", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),
      fetch("/api/Tools", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),
      fetch("/api/SizeRefs", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),
    ]);

    const typeOptions: Option[] = types.map((t: any) => ({
      value: t.type_id,
      label: t.type_name,
    }));
    const toolOptions: Option[] = tools.map((t: any) => ({
      value: t.tool_id,
      label: t.tool_name,
    }));
    const typeRefOptions = typeOptions;
    const toolRefOptions = toolOptions;
    const sizeRefOptions: Option[] = sizeRefs.map((s: any) => ({
      value: s.size_ref_id,
      label: s.size_ref,
    }));

    // 👉 Load pads & hstTypes
    const [padsRes, hstRes] = await Promise.all([
      fetch("/api/Pads", {
        headers: { Authorization: `Bearer ${token}` },
      }),
      fetch("/api/HstTypes", {
        headers: { Authorization: `Bearer ${token}` },
      }),
    ]);

    const pads = await padsRes.json();
    const hstTypes = await hstRes.json();

    const padOptions: { pad_id: number; pad_name: string }[] = pads || [];
    const hstOptions: { hst_type_id: number; hst_type: string }[] =
      hstTypes || [];

    if (!padOptions.length || !hstOptions.length) {
      await MySwal.fire({
        icon: "warning",
        title: "No Data",
        text: "Missing pad or HST type data.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const escapeHTML = (str: string | null) =>
      (str ?? "–")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");

    const label = (label: string, value: string | null) =>
      `<tr><td style="padding:4px 8px; vertical-align:top;"><b>${label}:</b></td><td>${escapeHTML(
        value
      )}</td></tr>`;

    const { isConfirmed, value: selected } = await MySwal.fire({
      title: "Edit Pad Mapping",
      html: `
        <div style="font-size:15px; text-align:left;">
          <table style="width:100%; margin-bottom:16px;">
            ${label("Tool Type", row.tool_type)}
            ${label("Tool Name", row.tool_name)}
            ${label("Type Ref", row.type_ref)}
            ${label("Tool Ref", row.tool_ref)}
            ${label("Size Ref", row.size_ref)}
          </table>

          <div style="margin-bottom:14px;">
            <label style="font-weight:bold; display:block; margin-bottom:6px;">Pad Name:</label>
            <input id="${inputPadId}" list="pad-list" value="${escapeHTML(
        row.pad_name
      )}" 
              class="swal2-input" style="max-width: 350px; width: 100%;" />
            <datalist id="pad-list">
              ${padOptions
                .map(
                  (p) => `<option value="${escapeHTML(p.pad_name)}"></option>`
                )
                .join("")}
            </datalist>
          </div>

          <div>
            <label style="font-weight:bold; display:block; margin-bottom:6px;">HST Type:</label>
            <input id="${inputHstId}" list="hst-list" value="${escapeHTML(
        row.hst_type
      )}" 
              class="swal2-input" style="max-width: 350px; width: 100%;" />
            <datalist id="hst-list">
              ${hstOptions
                .map(
                  (h) => `<option value="${escapeHTML(h.hst_type)}"></option>`
                )
                .join("")}
            </datalist>
          </div>
          <div style="margin-top:16px;">
            <label style="font-weight:bold; margin-top:12px;">Description:</label><br/>
            <input id="swal-description" value="${escapeHTML(
              row.description ?? ""
            )}" placeholder="Optional description"
              class="swal2-input" style="max-width: 350px; width: 100%;"/>
          </div>

        </div>
      `,
      showCancelButton: true,
      confirmButtonText: "💾 Save",
      cancelButtonText: "Cancel",
      focusConfirm: false,
      customClass: {
        popup: "swal2-rounded text-sm",
        confirmButton:
          "bg-[#2563eb] hover:bg-[#1d4ed8] text-white font-semibold px-5 py-2 rounded-md text-sm",
        cancelButton:
          "bg-[#f1f5f9] hover:bg-[#e2e8f0] text-black font-normal px-5 py-2 rounded-md text-sm ml-2",
      },
      buttonsStyling: false,
      preConfirm: () => {
        const padInput = (
          document.getElementById(inputPadId) as HTMLInputElement
        )?.value.trim();
        const hstInput = (
          document.getElementById(inputHstId) as HTMLInputElement
        )?.value.trim();
        if (!padInput || !hstInput) {
          MySwal.showValidationMessage("Please select both Pad and HST Type");
          return null;
        }
        const descriptionInput = (
          document.getElementById("swal-description") as HTMLInputElement
        )?.value?.trim();

        return {
          pad_name: padInput,
          hst_type: hstInput,
          description: descriptionInput ?? "",
        };
      },
    });

    if (!isConfirmed || !selected) return;

    const matchedPad = padOptions.find(
      (p) => p.pad_name.toLowerCase() === selected.pad_name.toLowerCase()
    );
    const matchedHst = hstOptions.find(
      (h) => h.hst_type.toLowerCase() === selected.hst_type.toLowerCase()
    );

    const selectedPadName = selected.pad_name.trim();
    const selectedHstType = selected.hst_type.trim();
    const newDescription = selected.description?.trim() ?? "";

    // ✅ เช็คว่าไม่มีการเปลี่ยนแปลง
    const isSame =
      selectedPadName === row.pad_name.trim() &&
      selectedHstType === row.hst_type.trim() &&
      (newDescription || "") === (row.description?.trim() || "");

    if (isSame) {
      await MySwal.fire({
        icon: "info",
        title: "No Changes",
        text: "You didn't change anything. No request submitted.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    if (!matchedPad || !matchedHst) {
      await MySwal.fire({
        icon: "error",
        title: "Invalid Selection",
        text: "Pad or HST type not found.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }
    const old_data = {
      // tool_key_id: row.tool_key_id,
      // type_id: row.type_id,
      // tool_id: row.tool_id,
      // type_ref_id: row.type_ref_id,
      // tool_ref_id: row.tool_ref_id,
      // size_ref_id: row.size_ref_id,
      tool_type: row.tool_type,
      tool_name: row.tool_name,
      type_ref: row.type_ref,
      tool_ref: row.tool_ref,
      size_ref: row.size_ref,
      // pad_id: matchedPad.pad_id,
      pad_name: row.pad_name,
      // hst_type_id: matchedHst.hst_type_id,
      hst_type: row.hst_type,
      description: row.description ?? "",
    };

    // ✅ STEP 1: Resolve tool_key_id จาก ToolKeyAlls
    const resolveToolKeyRes = await fetch(
      "/api/ToolKeyAlls/check?" +
        new URLSearchParams({
          type_id: getIdByLabel(typeOptions, row.tool_type)?.toString() ?? "",
          tool_id: getIdByLabel(toolOptions, row.tool_name)?.toString() ?? "",
          type_ref_id:
            getIdByLabel(typeRefOptions, row.type_ref)?.toString() ?? "",
          tool_ref_id:
            getIdByLabel(toolRefOptions, row.tool_ref)?.toString() ?? "",
          size_ref_id:
            getIdByLabel(sizeRefOptions, row.size_ref)?.toString() ?? "",
        }),
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );
    const toolKey = await resolveToolKeyRes.json();
    if (!toolKey?.tool_key_id) {
      await MySwal.fire({
        icon: "error",
        title: "Cannot Resolve Tool",
        text: "Could not find matching tool_key_id for this tool + ref combination.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const payload = {
      tool_key_id: toolKey.tool_key_id, // ✅ เพิ่มตรงนี้
      tool_type: row.tool_type,
      tool_name: row.tool_name,
      type_ref: row.type_ref,
      tool_ref: row.tool_ref,
      size_ref: row.size_ref,
      pad_id: matchedPad.pad_id,
      pad_name: matchedPad.pad_name,
      hst_type_id: matchedHst.hst_type_id,
      hst_type: matchedHst.hst_type,
      description: selected.description ?? "",
    };

    console.log("📝 Payload for request:", payload);

    // ✅ เปลี่ยนจาก PUT → POST /api/Requests
    const res = await fetch("/api/Requests", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        request_type: "UPDATE",
        target_table: "ToolPadMap",
        target_pk_id: row.map_id,
        old_data: old_data,
        new_data: payload,
        note: null,
      }),
    });

    if (!res.ok) {
      const error = await res.text();
      console.error("❌ Request submission error:", error);
      await MySwal.fire({
        icon: "error",
        title: "Submission Failed",
        text:
          error ||
          "Request submission failed. Please contact your administrator.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    console.log("✅ Request submitted successfully!");

    await MySwal.fire({
      icon: "success",
      title: "Submitted!",
      text: "Your update request for Tool-Pad Map has been submitted for approval.",
      confirmButtonText: "OK",
      customClass: {
        confirmButton:
          "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
      },
    });
    onSuccess();
  } catch (err: any) {
    console.error("🔥 Edit pad map error:", err);
    await MySwal.fire({
      icon: "error",
      title: "Error",
      text: err?.message || "Unexpected error occurred",
      confirmButtonText: "OK",
      customClass: {
        confirmButton:
          "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
      },
    });
  }
};

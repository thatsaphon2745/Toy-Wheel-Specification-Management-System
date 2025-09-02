// import Swal from "sweetalert2";
// import withReactContent from "sweetalert2-react-content";

// const MySwal = withReactContent(Swal);

// export const ToolMachineMapEditModal = async (
//   row: {
//     map_id: number;
//     tool_type: string;
//     tool_name: string;
//     type_ref: string | null;
//     tool_ref: string | null;
//     size_ref: string;
//     machine_no: string;
//   },
//   onSuccess: () => void
// ) => {
//   const inputId = "swal-machine-search";

//   try {
//     console.log("🛠️ Editing Machine Map");
//     console.log("🔎 Selected map_id:", row.map_id);
//     console.log("🔎 Current machine_no:", row.machine_no);

//     // 👉 โหลด machine list
//     const loading = MySwal.fire({
//       title: "Loading machines...",
//       allowOutsideClick: false,
//       didOpen: () => {
//         MySwal.showLoading();
//       },
//     });

//     const token = localStorage.getItem("token");
//     console.log("TOKEN:", token);

//     if (token) {
//       const payloadBase64 = token.split(".")[1];
//       const payloadJson = atob(payloadBase64);
//       const payload = JSON.parse(payloadJson);
//       console.log("Decoded Payload:", payload);
//       console.log("ROLE FROM TOKEN:", payload.role);
//     }

//     const res = await fetch("/api/Machines", {
//       headers: {
//         Authorization: `Bearer ${token}`,
//       },
//     });
//     const json = await res.json();

//     await MySwal.close();

//     const options: { machine_id: number; machine_no: string }[] = Array.isArray(
//       json
//     )
//       ? json.map((m: any) => ({
//           machine_id: m.machine_id,
//           machine_no: m.machine_no,
//         }))
//       : [];

//     if (!options.length) {
//       await MySwal.fire({
//         icon: "warning",
//         title: "No Machines",
//         text: "No machine data available.",
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

//     const { isConfirmed, value: selectedMachine } = await MySwal.fire({
//       title: "<strong>Edit Machine Mapping</strong>",
//       html: `
//         <div style="font-size:15px; text-align:left;">
//           <table style="width:100%; margin-bottom:16px;">
//             ${label("Tool Type", row.tool_type)}
//             ${label("Tool Name", row.tool_name)}
//             ${label("Type Ref", row.type_ref)}
//             ${label("Tool Ref", row.tool_ref)}
//             ${label("Size Ref", row.size_ref)}
//           </table>

//           <label style="font-weight:bold;">Machine No:</label><br/>
//           <input id="${inputId}" list="machine-list" value="${
//         escapeHTML(row.machine_no)
//       }" placeholder="Search..." class="swal2-input" style="margin-top:6px;" />
//           <datalist id="machine-list">
//             ${options
//               .map((m) => `<option value="${escapeHTML(m.machine_no)}"></option>`)
//               .join("")}
//           </datalist>
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
//         const val = (
//           document.getElementById(inputId) as HTMLInputElement
//         )?.value?.trim();
//         if (!val) {
//           MySwal.showValidationMessage("Please select a machine");
//           return null;
//         }
//         return val;
//       },
//     });

//     if (
//       !isConfirmed ||
//       !selectedMachine ||
//       selectedMachine === row.machine_no
//     ) {
//       console.log("ℹ️ Edit cancelled or machine not changed.");
//       return;
//     }

//     const selectedMachineNo = selectedMachine.trim();
//     const matched = options.find(
//       (opt) => opt.machine_no === selectedMachineNo
//     );

//     if (!matched) {
//       await MySwal.fire({
//         icon: "error",
//         title: "Invalid Machine",
//         text: "Selected machine not found.",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
//         },
//       });
//       return;
//     }

//     const machine_id = matched.machine_id;
//     console.log("✅ Selected machine_id:", machine_id);

//     const updateRes = await fetch(
//       `/api/ToolMachineMap/${row.map_id}`,
//       {
//         method: "PUT",
//         headers: {
//           "Content-Type": "application/json",
//           Authorization: `Bearer ${token}`,
//         },
//         body: JSON.stringify(machine_id),
//       }
//     );

//     if (!updateRes.ok) {
//       const error = await updateRes.text();
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
//       "machine_no:",
//       selectedMachine
//     );
//     await MySwal.fire({
//       icon: "success",
//       title: "Success",
//       text: "Machine mapping updated!",
//       confirmButtonText: "OK",
//       customClass: {
//         confirmButton:
//           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
//       },
//     });
//     onSuccess();
//   } catch (err: any) {
//     console.error("🔥 Edit machine map error:", err);
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

const MySwal = withReactContent(Swal);

export interface Option {
  value: number;
  label: string;
}

export const ToolMachineMapEditModal = async (
  row: {
    map_id: number;
    tool_type: string;
    tool_name: string;
    type_ref: string | null;
    tool_ref: string | null;
    size_ref: string;
    machine_no: string;
    description?: string | null; // ✅ เพิ่มบรรทัดนี้
  },
  onSuccess: () => void
) => {
  const inputId = "swal-machine-search";

  const getIdByLabel = (
    options: { value: number; label: string }[],
    label: string | null
  ): number | null => {
    return options.find((opt) => opt.label === label)?.value ?? null;
  };

  try {
    console.log("🛠️ Editing Machine Map");
    console.log("🔎 Selected map_id:", row.map_id);
    console.log("🔎 Current machine_no:", row.machine_no);

    // 👉 โหลด machine list
    const loading = MySwal.fire({
      title: "Loading machines...",
      allowOutsideClick: false,
      didOpen: () => {
        MySwal.showLoading();
      },
    });

    const token = localStorage.getItem("token");

    const res = await fetch("/api/Machines", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    const json = await res.json();

    await MySwal.close();

    const options: { machine_id: number; machine_no: string }[] = Array.isArray(
      json
    )
      ? json.map((m: any) => ({
          machine_id: m.machine_id,
          machine_no: m.machine_no,
        }))
      : [];

    if (!options.length) {
      await MySwal.fire({
        icon: "warning",
        title: "No Machines",
        text: "No machine data available.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const escapeHTML = (str: string | null) =>
      (str ?? "–").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    const label = (label: string, value: string | null) =>
      `<tr><td style="padding:4px 8px; vertical-align:top;"><b>${label}:</b></td><td>${escapeHTML(
        value
      )}</td></tr>`;

    const { isConfirmed, value: selectedMachine } = await MySwal.fire({
      title: "Edit Machine Mapping",
      html: `
        <div style="font-size:15px; text-align:left;">
          <table style="width:100%; margin-bottom:16px;">
            ${label("Tool Type", row.tool_type)}
            ${label("Tool Name", row.tool_name)}
            ${label("Type Ref", row.type_ref)}
            ${label("Tool Ref", row.tool_ref)}
            ${label("Size Ref", row.size_ref)}
          </table>

          <label style="font-weight:bold;">Machine No:</label><br/>
          <input id="${inputId}" list="machine-list" value="${escapeHTML(
        row.machine_no
      )}" placeholder="Search..." class="swal2-input" style="margin-top:12px;" />
          <br/><label style="font-weight:bold; margin-top:12px;">Description:</label><br/>
            <input id="swal-description" value="${escapeHTML(
              row.description ?? ""
            )}" placeholder="Optional description"
              class="swal2-input" style="margin-top:6px;"/>
          <datalist id="machine-list">
            ${options
              .map(
                (m) => `<option value="${escapeHTML(m.machine_no)}"></option>`
              )
              .join("")}
          </datalist>
        </div>
      `,
      showCancelButton: true,
      confirmButtonText: "💾 Submit Request",
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
        const val = (
          document.getElementById(inputId) as HTMLInputElement
        )?.value?.trim();
        if (!val) {
          MySwal.showValidationMessage("Please select a machine");
          return null;
        }
        const descVal = (
          document.getElementById("swal-description") as HTMLInputElement
        )?.value?.trim();

        return {
          machine_no: val,
          description: descVal || null,
        };
      },
    });

    if (!isConfirmed || !selectedMachine) {
      console.log("ℹ️ Edit cancelled or machine not changed.");
      return;
    }

    const selectedMachineNo = selectedMachine.machine_no.trim();
    const newDescription = selectedMachine.description;
    const matched = options.find((opt) => opt.machine_no === selectedMachineNo);

    const isSame =
      selectedMachineNo === row.machine_no.trim() &&
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

    if (!matched) {
      await MySwal.fire({
        icon: "error",
        title: "Invalid Machine",
        text: "Selected machine not found.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const machine_id = matched.machine_id;
    console.log("✅ Selected machine_id:", machine_id);

    const confirm = await MySwal.fire({
      icon: "question",
      title: "Confirm Update?",
      text: `Submit a request to update machine to ${selectedMachineNo}?`,
      showCancelButton: true,
      confirmButtonText: "Yes, Submit",
      cancelButtonText: "Cancel",
      customClass: {
        confirmButton:
          "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        cancelButton:
          "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
      },
    });

    if (!confirm.isConfirmed) {
      console.log("❌ User cancelled confirmation.");
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
      machine_no: row.machine_no,
      description: row.description ?? null, // ✅
    };

    // ✅ STEP: Resolve tool_key_id จาก ToolKeyAlls
    const [typeModels, toolModels, typeRefs, toolRefs, sizeRefs] =
      await Promise.all([
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

    const toOption = (data: any[], idKey: string, nameKey: string): Option[] =>
      data.map((d) => ({
        value: d[idKey],
        label: d[nameKey],
      }));

    const typeOptions = toOption(typeModels, "type_id", "type_name");
    const toolOptions = toOption(toolModels, "tool_id", "tool_name");
    const typeRefOptions = typeOptions;
    const toolRefOptions = toolOptions;
    const sizeRefOptions = toOption(sizeRefs, "size_ref_id", "size_ref");

    // ✅ เรียกหา tool_key_id
    const resolveRes = await fetch(
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
        headers: { Authorization: `Bearer ${token}` },
      }
    );

    const toolKey = await resolveRes.json();
    if (!toolKey?.tool_key_id) {
      await MySwal.fire({
        icon: "error",
        title: "Cannot Resolve Tool",
        text: "Could not resolve tool_key_id for this mapping.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const payload = {
      tool_key_id: toolKey.tool_key_id, // ✅ ใส่เพิ่มตรงนี้
      tool_type: row.tool_type,
      tool_name: row.tool_name,
      type_ref: row.type_ref,
      tool_ref: row.tool_ref,
      size_ref: row.size_ref,
      machine_id: matched.machine_id,
      machine_no: matched.machine_no,
      description: newDescription, // ✅
    };

    console.log("📝 Payload for request:", payload);

    const updateRes = await fetch("/api/Requests", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        request_type: "UPDATE",
        target_table: "ToolMachineMap",
        target_pk_id: row.map_id,
        old_data: old_data,
        new_data: payload,
        note: null,
      }),
    });

    if (!updateRes.ok) {
      const error = await updateRes.text();
      console.error("❌ Update request error:", error);
      await MySwal.fire({
        icon: "error",
        title: "Submission Failed",
        text: error || "Server error",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    await MySwal.fire({
      icon: "success",
      title: "Request Submitted",
      text: "Your update request has been submitted successfully.",
      confirmButtonText: "OK",
      customClass: {
        confirmButton:
          "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
      },
    });

    onSuccess();
  } catch (err: any) {
    console.error("🔥 Edit machine map error:", err);
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

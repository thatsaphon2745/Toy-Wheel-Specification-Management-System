// import Swal from "sweetalert2";
// import withReactContent from "sweetalert2-react-content";

// const MySwal = withReactContent(Swal);

// export const PadHstMapEditModal = async (
//   row: {
//     pad_hst_id: number;
//     pad_name: string;
//     hst_type: string;
//   },
//   onSuccess: () => void
// ) => {
//   const inputId = "swal-hst-type";

//   try {
//     console.log("🛠️ Editing PadHstMap");
//     console.log("🔎 Selected pad_hst_id:", row.pad_hst_id);

//     const loading = MySwal.fire({
//       title: "Loading HST types...",
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

//     const res = await fetch("/api/HstTypes");
//     const json = await res.json();

//     await MySwal.close();

//     const options: { hst_type_id: number; hst_type: string }[] = Array.isArray(
//       json
//     )
//       ? json.map((h: any) => ({
//           hst_type_id: h.hst_type_id,
//           hst_type: h.hst_type,
//         }))
//       : [];

//     if (!options.length) {
//       await MySwal.fire("No HST Type", "No HST types available.", "warning");
//       return;
//     }

//     const safePadName = row.pad_name
//       .replace(/</g, "&lt;")
//       .replace(/>/g, "&gt;");

//     const { isConfirmed, value: selectedHst } = await MySwal.fire({
//       title: "<strong>Edit Pad-HST Mapping</strong>",
//       html: `
//         <div style="font-size:15px;">
//           <table style="width:100%; border-collapse:collapse;">
//             <tr>
//               <td style="padding:4px 8px; vertical-align:middle; white-space:nowrap;"><b>Pad Name:</b></td>
//               <td style="padding:4px 8px;">${safePadName}</td>
//             </tr>
//             <tr>
//               <td style="padding:4px 8px;"><b>HST Type:</b></td>
//               <td style="padding:4px 8px;">
//                 <input
//                   id="${inputId}"
//                   list="hst-type-list"
//                   value="${row.hst_type}"
//                   placeholder="Search..."
//                   class="swal2-input"
//                   style="width:100%; margin:0;"
//                 />
//                 <datalist id="hst-type-list">
//                   ${options
//                     .map((h) => `<option value="${h.hst_type}"></option>`)
//                     .join("")}
//                 </datalist>
//               </td>
//             </tr>
//           </table>
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
//           MySwal.showValidationMessage("Please select a HST type");
//           return null;
//         }
//         return val;
//       },
//     });

//     if (!isConfirmed || !selectedHst || selectedHst === row.hst_type) {
//       console.log("ℹ️ Edit cancelled or HST type not changed.");
//       return;
//     }

//     const matched = options.find((opt) => opt.hst_type === selectedHst.trim());

//     if (!matched) {
//       await MySwal.fire({
//         icon: "error",
//         title: "Invalid HST Type",
//         text: "Selected HST type not found.",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
//         },
//       });
//       return;
//     }

//     const hst_type_id = matched.hst_type_id;
//     console.log("✅ Selected hst_type_id:", hst_type_id);

//     const updateRes = await fetch(
//       `/api/PadHstMap/${row.pad_hst_id}`,
//       {
//         method: "PUT",
//         headers: {
//           "Content-Type": "application/json",
//           Authorization: `Bearer ${token}`,
//         },
//         body: JSON.stringify({
//           pad_hst_id: row.pad_hst_id,
//           pad_id: -1, // dummy, actual fetched server-side
//           hst_type_id,
//         }),
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
//       "✅ Update successful! pad_hst_id:",
//       row.pad_hst_id,
//       "hst_type:",
//       selectedHst
//     );

//     await MySwal.fire({
//       icon: "success",
//       title: "Success",
//       text: "Pad-HST mapping updated!",
//       confirmButtonText: "OK",
//       customClass: {
//         confirmButton:
//           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
//       },
//     });

//     onSuccess();
//   } catch (err: any) {
//     console.error("🔥 Edit pad-hst map error:", err);
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

export const PadHstMapEditModal = async (
  row: {
    pad_hst_id: number;
    pad_id: number;
    pad_name: string;
    hst_type_id: number;
    hst_type: string;
    description?: string | null;
  },
  onSuccess: () => void
) => {
  const inputId = "swal-hst-type";

  try {
    console.log("🛠️ Editing PadHstMap");
    console.log("🔎 Selected pad_hst_id:", row.pad_hst_id);

    const loading = MySwal.fire({
      title: "Loading HST types...",
      allowOutsideClick: false,
      didOpen: () => {
        MySwal.showLoading();
      },
    });

    const token = localStorage.getItem("token");
    console.log("TOKEN:", token);

    if (token) {
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      console.log("Decoded Payload:", payload);
      console.log("ROLE FROM TOKEN:", payload.role);
    }

    const res = await fetch("/api/HstTypes");
    const json = await res.json();

    await MySwal.close();

    const options: { hst_type_id: number; hst_type: string }[] = Array.isArray(
      json
    )
      ? json.map((h: any) => ({
          hst_type_id: h.hst_type_id,
          hst_type: h.hst_type,
        }))
      : [];

    if (!options.length) {
      await MySwal.fire("No HST Type", "No HST types available.", "warning");
      return;
    }

    const safePadName = row.pad_name
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

    const descId = "swal-description"; // ✅ id สำหรับ input description
    const safeDesc = (row.description ?? "")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

    const { isConfirmed, value: swalResult } = await MySwal.fire({
      title: "Edit Pad-HST Mapping",
      html: `
        <div style="font-size:15px;">
          <table style="width:100%; border-collapse:collapse;">
            <tr>
              <td style="padding:4px 8px; vertical-align:middle; white-space:nowrap;"><b>Pad Name:</b></td>
              <td style="padding:4px 8px;">${safePadName}</td>
            </tr>
            <tr>
              <td style="padding:4px 8px;"><b>HST Type:</b></td>
              <td style="padding:4px 8px;">
                <input
                  id="${inputId}"
                  list="hst-type-list"
                  value="${row.hst_type}"
                  placeholder="Search..."
                  class="swal2-input"
                  style="width:100%; margin:0;"
                />
                <datalist id="hst-type-list">
                  ${options
                    .map((h) => `<option value="${h.hst_type}"></option>`)
                    .join("")}
                </datalist>
              </td>
            </tr>
            <tr>
              <td style="padding:4px 8px;"><b>Description:</b></td>
              <td style="padding:4px 8px;">
                <input
                  id="${descId}"  // ✅ FIXED: ใช้ตัวแปร
                  class="swal2-input"
                  placeholder="Optional description"
                  value="${safeDesc}"
                  style="width:100%; margin:0;"
                />
              </td>
            </tr>
          </table>
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
        // const val = (
        //   document.getElementById(inputId) as HTMLInputElement
        // )?.value?.trim();
        // if (!val) {
        //   MySwal.showValidationMessage("Please select a HST type");
        //   return null;
        // }
        // return val;
        const hstVal = (
          document.getElementById(inputId) as HTMLInputElement
        )?.value?.trim();

        const descVal = (
          document.getElementById(descId) as HTMLInputElement
        )?.value?.trim();

        if (!hstVal) {
          MySwal.showValidationMessage("Please select a HST type");
          return null;
        }

        return {
          hstVal,
          description: descVal || null,
        };
      },
    });

    if (!isConfirmed || !swalResult) {
      console.log("ℹ️ Edit cancelled or modal closed.");
      return;
    }

    const selectedHst = swalResult.hstVal.trim();
    const newDescription = swalResult.description?.trim() || null;

    const isSameHst = selectedHst === row.hst_type;
    const isSameDesc = (newDescription ?? "") === (row.description ?? "");

    if (!isConfirmed || (isSameHst && isSameDesc)) {
      console.log("ℹ️ Edit cancelled or nothing changed.");
      return;
    }

    const matched = options.find((opt) => opt.hst_type === selectedHst.trim());

    if (!matched) {
      await MySwal.fire({
        icon: "error",
        title: "Invalid HST Type",
        text: "Selected HST type not found.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const hst_type_id = matched.hst_type_id;
    console.log("✅ Selected hst_type_id:", hst_type_id);

    // Prepare request DTO
    const requestDto = {
      request_type: "UPDATE",
      target_table: "PadHstMap",
      target_pk_id: row.pad_hst_id,
      old_data: {
        pad_hst_id: row.pad_hst_id,
        pad_id: row.pad_id,
        pad_name: row.pad_name,
        hst_type_id: row.hst_type_id,
        hst_type: row.hst_type,
        description: row.description ?? null, // ✅ เพิ่มตรงนี้
      },
      new_data: {
        pad_hst_id: row.pad_hst_id,
        pad_id: row.pad_id,
        pad_name: row.pad_name,
        hst_type_id: matched.hst_type_id,
        hst_type: matched.hst_type,
        description: newDescription ?? null, // ✅ เพิ่มตรงนี้
      },
      note: "Updated via frontend",
    };

    console.log("📦 Sending update request DTO:", requestDto);

    const updateRes = await fetch("/api/Requests", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(requestDto),
    });

    if (!updateRes.ok) {
      const errorData = await updateRes.json();
      console.error("❌ Update request error:", errorData);
      await MySwal.fire({
        icon: "error",
        title: "Update Request Failed",
        text: errorData.message || "Server error",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const resultJson = await updateRes.json();

    console.log("✅ Update request submitted:", resultJson);

    await MySwal.fire({
      icon: "info",
      title: "Request Submitted",
      html: `Your request to update Pad-HST mapping has been submitted.<br>Request ID: <b>${resultJson.request_id}</b><br>It will be effective after admin approval.`,
      confirmButtonText: "OK",
      customClass: {
        confirmButton:
          "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
      },
    });

    onSuccess();
  } catch (err: any) {
    console.error("🔥 Edit pad-hst map error:", err);
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

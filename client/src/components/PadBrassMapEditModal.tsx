// import Swal from "sweetalert2";
// import withReactContent from "sweetalert2-react-content";

// const MySwal = withReactContent(Swal);

// export const PadBrassMapEditModal = async (
//   row: {
//     pad_brass_id: number;
//     pad_name: string;
//     brass_no: string;
//   },
//   onSuccess: () => void
// ) => {
//   const inputId = "swal-brass-search";

//   try {
//     console.log("🛠️ Editing PadBrassMap");
//     console.log("🔎 Selected pad_brass_id:", row.pad_brass_id);

//     const loading = MySwal.fire({
//       title: "Loading brasses...",
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

//     const res = await fetch("/api/Brass");
//     const json = await res.json();

//     await MySwal.close();

//     const options: { brass_id: number; brass_no: string }[] = Array.isArray(
//       json
//     )
//       ? json.map((b: any) => ({
//           brass_id: b.brass_id,
//           brass_no: b.brass_no,
//         }))
//       : [];

//     if (!options.length) {
//       await MySwal.fire("No Brass", "No brass data available.", "warning");
//       return;
//     }

//     const safePadName = row.pad_name.replace(/</g, "&lt;").replace(/>/g, "&gt;");

//     const { isConfirmed, value: selectedBrass } = await MySwal.fire({
//       title: "<strong>Edit Pad-Brass Mapping</strong>",
//       html: `
//         <div style="font-size:15px;">
//           <table style="width:100%; border-collapse:collapse;">
//             <tr>
//               <td style="padding:4px 8px; vertical-align:middle; white-space:nowrap;"><b>Pad Name:</b></td>
//               <td style="padding:4px 8px;">${safePadName}</td>
//             </tr>
//             <tr>
//               <td style="padding:4px 8px; vertical-align:middle;"><b>Brass No:</b></td>
//               <td style="padding:4px 8px;">
//                 <input
//                   id="${inputId}"
//                   list="brass-list"
//                   value="${row.brass_no}"
//                   placeholder="Search..."
//                   class="swal2-input"
//                   style="width:100%; margin:0;"
//                 />
//                 <datalist id="brass-list">
//                   ${options
//                     .map(
//                       (b) =>
//                         `<option value="${b.brass_no}"></option>`
//                     )
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
//           MySwal.showValidationMessage("Please select a brass");
//           return null;
//         }
//         return val;
//       },
//     });

//     if (!isConfirmed || !selectedBrass || selectedBrass === row.brass_no) {
//       console.log("ℹ️ Edit cancelled or brass not changed.");
//       return;
//     }

//     const selectedBrassNo = selectedBrass.trim();
//     const matched = options.find(
//       (opt) => opt.brass_no === selectedBrassNo
//     );

//     if (!matched) {
//       await MySwal.fire("Invalid Brass", "Selected brass not found.", "error");
//       return;
//     }

//     const brass_id = matched.brass_id;
//     console.log("✅ Selected brass_id:", brass_id);

//     const updateRes = await fetch(
//       `/api/PadBrassMap/${row.pad_brass_id}`,
//       {
//         method: "PUT",
//         headers: {
//           "Content-Type": "application/json",
//           Authorization: `Bearer ${token}`,
//         },
//         body: JSON.stringify({
//           pad_brass_id: row.pad_brass_id,
//           pad_id: -1, // dummy, actual will be fetched from DB
//           brass_id,
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
//       "✅ Update successful! pad_brass_id:",
//       row.pad_brass_id,
//       "brass_no:",
//       selectedBrass
//     );

//     await MySwal.fire({
//       icon: "success",
//       title: "Success",
//       text: "Pad-brass mapping updated!",
//       confirmButtonText: "OK",
//       customClass: {
//         confirmButton:
//           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
//       },
//     });

//     onSuccess();
//   } catch (err: any) {
//     console.error("🔥 Edit pad-brass map error:", err);
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

export const PadBrassMapEditModal = async (
  row: {
    pad_brass_id: number;
    pad_id: number;
    pad_name: string;
    brass_no: string;
    description?: string | null; // ✅ เพิ่มตรงนี้
  },
  onSuccess: () => void
) => {
  const inputId = "swal-brass-search";
  const descId = "swal-description-input"; // ✅ เพิ่ม id สำหรับ description

  try {
    console.log("🛠️ Editing PadBrassMap");
    console.log("🔎 Selected pad_brass_id:", row.pad_brass_id);

    const loading = MySwal.fire({
      title: "Loading brasses...",
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

    const res = await fetch("/api/Brass");
    const json = await res.json();

    await MySwal.close();

    const options: { brass_id: number; brass_no: string }[] = Array.isArray(
      json
    )
      ? json.map((b: any) => ({
          brass_id: b.brass_id,
          brass_no: b.brass_no,
        }))
      : [];

    if (!options.length) {
      await MySwal.fire("No Brass", "No brass data available.", "warning");
      return;
    }

    const safePadName = row.pad_name
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

    const { isConfirmed, value: selectedBrass } = await MySwal.fire({
      title: "Edit Pad-Brass Mapping",
      html: `
        <div style="font-size:15px;">
          <table style="width:100%; border-collapse:collapse;">
            <tr>
              <td style="padding:4px 8px; vertical-align:middle; white-space:nowrap;"><b>Pad Name:</b></td>
              <td style="padding:4px 8px;">${safePadName}</td>
            </tr>
            <tr>
              <td style="padding:4px 8px; vertical-align:middle;"><b>Brass No:</b></td>
              <td style="padding:4px 8px;">
                <input
                  id="${inputId}"
                  list="brass-list"
                  value="${row.brass_no}"
                  placeholder="Search..."
                  class="swal2-input"
                  style="width:100%; margin:0;"
                />
                <datalist id="brass-list">
                  ${options
                    .map((b) => `<option value="${b.brass_no}"></option>`)
                    .join("")}
                </datalist>
              </td>
            </tr>
            <tr>
          <td style="padding:4px 8px;"><b>Description:</b></td>
          <td style="padding:4px 8px;">
            <input
              id="${descId}"
              value="${row.description ?? ""}"
              placeholder="Enter description"
              class="swal2-input"
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
        const brassVal = (
          document.getElementById(inputId) as HTMLInputElement
        )?.value?.trim();
        const descVal = (
          document.getElementById(descId) as HTMLInputElement
        )?.value?.trim();

        if (!brassVal) {
          MySwal.showValidationMessage("Please select a brass");
          return null;
        }

        return {
          brass_no: brassVal,
          description: descVal || null,
        };
      },
    });

    if (!isConfirmed || !selectedBrass || selectedBrass === row.brass_no) {
      console.log("ℹ️ Edit cancelled or brass not changed.");
      return;
    }

    const selectedBrassNo = selectedBrass.brass_no.trim();
    const newDescription = selectedBrass.description?.trim() ?? null;

    // ✅ เช็คว่าข้อมูลไม่ได้เปลี่ยน
    const isSame =
      selectedBrassNo === row.brass_no.trim() &&
      (newDescription || "") === (row.description?.trim() || "");

    if (isSame) {
      console.log("ℹ️ No changes detected. Skipping request.");
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

    const matched = options.find((opt) => opt.brass_no === selectedBrassNo);

    if (!matched) {
      await MySwal.fire("Invalid Brass", "Selected brass not found.", "error");
      return;
    }

    const brass_id = matched.brass_id;
    console.log("✅ Selected brass_id:", brass_id);

    // const newDescription =
    //   (
    //     document.getElementById("swal-description-input") as HTMLInputElement
    //   )?.value?.trim() || null;

    // Prepare request DTO
    const requestDto = {
      request_type: "UPDATE",
      target_table: "PadBrassMap",
      target_pk_id: row.pad_brass_id,
      old_data: {
        pad_brass_id: row.pad_brass_id,
        pad_id: row.pad_id,
        pad_name: row.pad_name,
        // brass_id: row.brass_id, // dummy, actual will be fetched from DB
        brass_no: row.brass_no,
        description: row.description ?? null,
      },
      new_data: {
        pad_brass_id: row.pad_brass_id,
        pad_id: row.pad_id, // dummy, fetched from DB backend
        pad_name: row.pad_name, // ✅ de-id
        brass_id,
        brass_no: matched.brass_no, // ✅ de-id
        description: newDescription, // ✅ ส่งเข้าไป
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

    const result = await updateRes.json();

    console.log("✅ Update request submitted:", result);

    await MySwal.fire({
      icon: "info",
      title: "Request Submitted",
      html: `Your request to update Pad-Brass mapping has been submitted.<br>Request ID: <b>${result.request_id}</b><br>It will be effective after admin approval.`,
      confirmButtonText: "OK",
      customClass: {
        confirmButton:
          "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
      },
    });

    onSuccess();
  } catch (err: any) {
    console.error("🔥 Edit pad-brass map error:", err);
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

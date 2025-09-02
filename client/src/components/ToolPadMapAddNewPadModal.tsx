// import React, { useEffect, useState } from "react";
// import Select from "react-select";
// import axios from "axios";
// import Swal from "sweetalert2";
// import withReactContent from "sweetalert2-react-content";

// const MySwal = withReactContent(Swal);

// interface Option {
//   value: number;
//   label: string;
// }

// interface Props {
//   onClose: () => void;
//   onSubmitSuccess: () => void;
// }

// const ToolPadMapAddNewPadModal: React.FC<Props> = ({
//   onClose,
//   onSubmitSuccess,
// }) => {
//   const [types, setTypes] = useState<Option[]>([]);
//   const [tools, setTools] = useState<Option[]>([]);
//   const [typeRefs, setTypeRefs] = useState<Option[]>([]);
//   const [toolRefs, setToolRefs] = useState<Option[]>([]);
//   const [sizeRefs, setSizeRefs] = useState<Option[]>([]);
//   const [hstTypes, setHstTypes] = useState<Option[]>([]);
//   const [pads, setPads] = useState<Option[]>([]);
//   const [loading, setLoading] = useState(false);

//   const [form, setForm] = useState<any>({
//     type_id: null,
//     tool_id: null,
//     type_ref_id: null,
//     tool_ref_id: null,
//     size_ref_id: null,
//     hst_type_id: null,
//     pad_id: null,
//   });

//   useEffect(() => {
//     Promise.all([
//       axios.get("/api/TypeModels"),
//       axios.get("/api/Tools"),
//       axios.get("/api/SizeRefs"),
//       axios.get("/api/HstTypes"),
//       axios.get("/api/Pads"),
//     ]).then(([type, tool, size, hst, pad]) => {
//       const toOpt = (arr: any[], label: string, value: string) =>
//         arr.map((d) => ({ value: d[value], label: d[label] ?? "(Blanks)" }));

//       setTypes(toOpt(type.data, "type_name", "type_id"));
//       setTypeRefs(toOpt(type.data, "type_name", "type_id"));
//       setTools(toOpt(tool.data, "tool_name", "tool_id"));
//       setToolRefs(toOpt(tool.data, "tool_name", "tool_id"));
//       setSizeRefs(toOpt(size.data, "size_ref", "size_ref_id"));
//       setHstTypes(toOpt(hst.data, "hst_type", "hst_type_id"));
//       setPads(toOpt(pad.data, "pad_name", "pad_id"));
//     });
//   }, []);

//   const handleSubmit = async () => {
//     const {
//       type_id,
//       tool_id,
//       type_ref_id,
//       tool_ref_id,
//       size_ref_id,
//       hst_type_id,
//       pad_id,
//     } = form;

//     if (
//       !type_id ||
//       !tool_id ||
//       !type_ref_id ||
//       !tool_ref_id ||
//       !size_ref_id ||
//       !hst_type_id ||
//       !pad_id
//     ) {
//       await MySwal.fire("Missing", "Please fill in all fields", "warning");
//       return;
//     }

//     setLoading(true);

//     try {
//       const token = localStorage.getItem("token");

//       const { data: toolKey } = await axios.get(
//         "/api/ToolKeyAlls/check",
//         {
//           params: {
//             type_id,
//             tool_id,
//             type_ref_id,
//             tool_ref_id,
//             size_ref_id,
//           },
//           headers: {
//             Authorization: `Bearer ${token}`,
//           },
//         }
//       );

//       if (!toolKey || !toolKey.tool_key_id) {
//         throw new Error("ToolKeyAlls not found");
//       }

//       await axios.post(
//         "/api/ToolPadMap",
//         {
//           tool_key_id: toolKey.tool_key_id,
//           hst_type_id,
//           pad_id,
//         },
//         {
//           headers: {
//             Authorization: `Bearer ${token}`,
//           },
//         }
//       );

//       await MySwal.fire({
//         icon: "success",
//         title: "Success",
//         text: "Pad mapped successfully",
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
//         },
//       });

//       onSubmitSuccess();
//       onClose();
//     } catch (err: any) {
//       console.error(err);

//       let errorMsg = "Something went wrong";

//       if (axios.isAxiosError(err) && err.response?.data) {
//         if (typeof err.response.data === "string") {
//           errorMsg = err.response.data;
//         } else if (err.response.data.message) {
//           errorMsg = err.response.data.message;
//         }
//       } else if (err?.message) {
//         errorMsg = err.message;
//       }

//       await MySwal.fire({
//         icon: "error",
//         title: "Error",
//         text: errorMsg,
//         confirmButtonText: "OK",
//         customClass: {
//           confirmButton:
//             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
//         },
//       });
//     } finally {
//       setLoading(false);
//     }
//   };

//   const renderSelect = (key: string, label: string, options: Option[]) => (
//     <div className="mb-3">
//       <label className="text-xs font-medium text-gray-600 block mb-1">
//         {label}
//       </label>
//       <Select
//         options={options}
//         isClearable
//         value={options.find((o) => o.value === form[key]) || null}
//         onChange={(o) => setForm({ ...form, [key]: o?.value || null })}
//         menuPortalTarget={document.body}
//         styles={{ menuPortal: (base) => ({ ...base, zIndex: 9999 }) }}
//       />
//     </div>
//   );

//   return (
//     <div className="fixed inset-0 bg-black bg-opacity-30 z-[999] flex justify-center items-center">
//       <div className="bg-white p-6 rounded-2xl w-full max-w-3xl max-h-[90vh] overflow-auto shadow-xl">
//         <h2 className="text-xl font-semibold text-gray-800 mb-6">
//           Add Tool Pad Mapping
//         </h2>

//         <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
//           {renderSelect("type_id", "Tool Type", types)}
//           {renderSelect("tool_id", "Tool Name", tools)}
//           {renderSelect("type_ref_id", "Type Ref", typeRefs)}
//           {renderSelect("tool_ref_id", "Tool Ref", toolRefs)}
//           {renderSelect("size_ref_id", "Size Ref", sizeRefs)}
//           {renderSelect("hst_type_id", "HST Type", hstTypes)}
//           {renderSelect("pad_id", "Pad", pads)}
//         </div>

//         <div className="flex justify-end gap-3">
//           <button
//             onClick={onClose}
//             disabled={loading}
//             className="border px-5 py-1.5 rounded text-sm hover:bg-gray-100"
//           >
//             Cancel
//           </button>
//           <button
//             onClick={handleSubmit}
//             disabled={loading}
//             className={`${
//               loading
//                 ? "bg-blue-300 cursor-not-allowed"
//                 : "bg-blue-600 hover:bg-blue-700"
//             } text-white px-5 py-1.5 rounded text-sm`}
//           >
//             {loading ? "Saving..." : "Submit"}
//           </button>
//         </div>
//       </div>
//     </div>
//   );
// };

// export default ToolPadMapAddNewPadModal;

import React, { useEffect, useState } from "react";
import Select from "react-select";
import axios from "axios";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";

const MySwal = withReactContent(Swal);

export interface Option {
  value: number;
  label: string;
}

interface Props {
  onClose: () => void;
  onSubmitSuccess: () => void;
  initialData?: any;
  fromClone?: boolean;
}

const ToolPadMapAddNewPadModal: React.FC<Props> = ({
  onClose,
  onSubmitSuccess,
  initialData,
  fromClone,
}) => {
  const [types, setTypes] = useState<Option[]>([]);
  const [tools, setTools] = useState<Option[]>([]);
  const [typeRefs, setTypeRefs] = useState<Option[]>([]);
  const [toolRefs, setToolRefs] = useState<Option[]>([]);
  const [sizeRefs, setSizeRefs] = useState<Option[]>([]);
  const [hstTypes, setHstTypes] = useState<Option[]>([]);
  const [pads, setPads] = useState<Option[]>([]);
  const [loading, setLoading] = useState(false);

  // const [form, setForm] = useState<any>({
  //   type_id: null,
  //   tool_id: null,
  //   type_ref_id: null,
  //   tool_ref_id: null,
  //   size_ref_id: null,
  //   hst_type_id: null,
  //   pad_id: null,
  // });
  const [form, setForm] = useState<any>({
    type_id: initialData?.type_id ?? null,
    tool_id: initialData?.tool_id ?? null,
    type_ref_id: initialData?.type_ref_id ?? null,
    tool_ref_id: initialData?.tool_ref_id ?? null,
    size_ref_id: initialData?.size_ref_id ?? null,
    pad_id: initialData?.pad_id ?? null, // หรือปล่อย null ถ้าไม่อยาก clone machine
    hst_type_id: initialData?.hst_type_id ?? null,
    description: initialData?.description ?? "", // ✅ เพิ่ม description
  });

  useEffect(() => {
    Promise.all([
      axios.get("/api/TypeModels"),
      axios.get("/api/Tools"),
      axios.get("/api/SizeRefs"),
      axios.get("/api/HstTypes"),
      axios.get("/api/Pads"),
    ]).then(([type, tool, size, hst, pad]) => {
      const toOpt = (arr: any[], label: string, value: string) =>
        arr.map((d) => ({ value: d[value], label: d[label] ?? "(Blanks)" }));

      setTypes(toOpt(type.data, "type_name", "type_id"));
      setTypeRefs(toOpt(type.data, "type_name", "type_id"));
      setTools(toOpt(tool.data, "tool_name", "tool_id"));
      setToolRefs(toOpt(tool.data, "tool_name", "tool_id"));
      setSizeRefs(toOpt(size.data, "size_ref", "size_ref_id"));
      setHstTypes(toOpt(hst.data, "hst_type", "hst_type_id"));
      setPads(toOpt(pad.data, "pad_name", "pad_id"));
    });
  }, []);

  const getLabel = (list: Option[], id: number) =>
    list.find((o) => o.value === id)?.label ?? String(id);

  const handleSubmit = async () => {
    const {
      type_id,
      tool_id,
      type_ref_id,
      tool_ref_id,
      size_ref_id,
      hst_type_id,
      pad_id,
    } = form;

    if (
      !type_id ||
      !tool_id ||
      !type_ref_id ||
      !tool_ref_id ||
      !size_ref_id ||
      !hst_type_id ||
      !pad_id
    ) {
      await MySwal.fire({
        icon: "warning",
        title: "Missing Fields",
        text: "Please fill in all fields before submitting.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const token = localStorage.getItem("token");

    const confirm = await MySwal.fire({
      title: "Confirm submission?",
      text: "Do you want to submit this request to add Tool-Pad mapping?",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Yes, Submit",
      cancelButtonText: "Cancel",
      customClass: {
        confirmButton:
          "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        cancelButton:
          "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
        popup: "text-sm",
      },
    });

    if (!confirm.isConfirmed) return;

    setLoading(true);

    try {
      // ✅ STEP 1: Find tool_key_id
      const { data: toolKey } = await axios.get(
        "/api/ToolKeyAlls/check",
        {
          params: {
            type_id,
            tool_id,
            type_ref_id,
            tool_ref_id,
            size_ref_id,
          },
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      if (!toolKey?.tool_key_id) {
        throw new Error("ToolKeyAlls not found with selected values.");
      }

      console.log("✅ tool_key_id:", toolKey.tool_key_id);

      // ✅ STEP 2: Send request to /api/Request
      await axios.post(
        "/api/Requests",
        {
          request_type: "INSERT",
          target_table: "ToolPadMap",
          target_pk_id: null,
          old_data: null,
          new_data: {
            tool_key_id: toolKey.tool_key_id,
            // 🟢 readable info (de-id)
            tool_type: getLabel(types, type_id),
            tool_name: getLabel(tools, tool_id),
            type_ref: getLabel(typeRefs, type_ref_id),
            tool_ref: getLabel(toolRefs, tool_ref_id),
            size_ref: getLabel(sizeRefs, size_ref_id),
            pad_id,
            pad_name: getLabel(pads, pad_id),
            hst_type_id,
            hst_type: getLabel(hstTypes, hst_type_id),
            description: form.description ?? "", // ✅ ส่งไปที่ backend
          },
          note: null,
        },
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      await MySwal.fire({
        icon: "success",
        title: "Submitted!",
        text: "Your request to add Tool-Pad mapping has been submitted.",
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
      console.error(err);

      let errorMsg = "Something went wrong during submission.";

      if (axios.isAxiosError(err) && err.response?.data) {
        if (typeof err.response.data === "string") {
          errorMsg = err.response.data;
        } else if (err.response.data.message) {
          errorMsg = err.response.data.message;
        }
      } else if (err?.message) {
        errorMsg = err.message;
      }

      await MySwal.fire({
        icon: "error",
        title: "Submission Failed",
        text: errorMsg,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
    } finally {
      setLoading(false);
    }
  };

  const renderSelect = (key: string, label: string, options: Option[]) => (
    <div className="mb-3">
      <label className="text-xs font-medium text-gray-600 block mb-1">
        {label}
      </label>
      <Select
        options={options}
        isClearable
        value={options.find((o) => o.value === form[key]) || null}
        onChange={(o) => setForm({ ...form, [key]: o?.value || null })}
        menuPortalTarget={document.body}
        styles={{ menuPortal: (base) => ({ ...base, zIndex: 9999 }) }}
      />
    </div>
  );

  return (
    <div className="fixed inset-0 bg-black bg-opacity-30 z-[999] flex justify-center items-center">
      <div className="bg-white p-6 rounded-2xl w-full max-w-3xl max-h-[90vh] overflow-auto shadow-xl">
        <h2 className="text-xl font-semibold text-gray-800 mb-6">
          Add Tool Pad Mapping (Request)
        </h2>

        <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
          {renderSelect("type_id", "Tool Type", types)}
          {renderSelect("tool_id", "Tool Name", tools)}
          {renderSelect("type_ref_id", "Type Ref", typeRefs)}
          {renderSelect("tool_ref_id", "Tool Ref", toolRefs)}
          {renderSelect("size_ref_id", "Size Ref", sizeRefs)}
          {renderSelect("hst_type_id", "HST Type", hstTypes)}
          {renderSelect("pad_id", "Pad", pads)}
        </div>
        {/* <div className="mb-4 col-span-2 md:col-span-3">
          <label className="text-xs font-medium text-gray-600 block mb-1">
            Description (Optional)
          </label>
          <input
            type="text"
            className="w-full border px-3 py-2 rounded text-sm"
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            placeholder="Enter description here..."
          />
        </div> */}
        <div className="mb-6">
          <label className="text-xs font-medium text-gray-600 block mb-1">
            Description (optional)
          </label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            rows={3}
            placeholder="Enter description..."
            className="w-full border rounded px-3 py-2 text-sm"
          />
        </div>

        <div className="flex justify-end gap-3">
          <button
            onClick={onClose}
            disabled={loading}
            className="border px-5 py-1.5 rounded text-sm hover:bg-gray-100"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={loading}
            className={`${
              loading
                ? "bg-blue-300 cursor-not-allowed"
                : "bg-blue-600 hover:bg-blue-700"
            } text-white px-5 py-1.5 rounded text-sm`}
          >
            {loading ? "Submitting..." : "Submit Request"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ToolPadMapAddNewPadModal;

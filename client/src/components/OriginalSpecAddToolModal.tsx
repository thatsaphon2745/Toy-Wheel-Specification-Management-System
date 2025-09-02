import React, { useEffect, useState } from "react";
import Select from "react-select";
import axios from "axios";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
import type { OriginalSpecRowInput } from "./OriginalSpecTable";

const MySwal = withReactContent(Swal);

export interface Option {
  value: number;
  label: string;
}

// interface Props {
//   onClose: () => void;
//   onSubmitSuccess: () => void;
//   initialData?: OriginalSpecRowInput | null;
// }

interface Props {
  onClose: () => void;
  onSubmitSuccess: () => void;
  initialData?: OriginalSpecRowInput | null;
  fromClone?: boolean;
}

const OriginalSpecAddToolModal: React.FC<Props> = ({
  onClose,
  onSubmitSuccess,
  initialData,
  fromClone = false,
}) => {
  const [types, setTypes] = useState<Option[]>([]);
  const [tools, setTools] = useState<Option[]>([]);
  const [sizes, setSizes] = useState<Option[]>([]);
  const [axles, setAxles] = useState<Option[]>([]);

  const normalizeInitialValue = (
    options: Option[],
    value: number | null,
    fromClone: boolean,
    defaultId: number
  ): Option | null => {
    let usedValue = value;

    if (value == null && fromClone) {
      usedValue = defaultId;
    }

    return options.find((t) => t.value === usedValue) || null;
  };

  // const [form, setForm] = useState<any>({
  //   type_id: null,
  //   tool_id: null,
  //   size_ref_id: null,
  //   axle_type_id: null,
  //   knurling_type: 0,
  //   is_original_spec: 1,
  //   overall_a: "",
  //   overall_b: "",
  //   overall_c: "",
  //   tolerance_a: "",
  //   tolerance_b: "",
  //   tolerance_c: "",
  //   f_shank_min: "",
  //   f_shank_max: "",
  //   b2b_min: "",
  //   b2b_max: "",
  //   h2h_min: "",
  //   h2h_max: "",
  //   chassis_span1: "",
  //   chassis_span2: "",
  //   source: "",
  // });
  const [form, setForm] = useState<any>(() => ({
    type_id: initialData?.type_id ?? null,
    tool_id: initialData?.tool_id ?? null,
    size_ref_id: initialData?.size_ref_id ?? null,
    axle_type_id: initialData?.axle_type_id ?? null,
    knurling_type: initialData?.knurling_type ?? null,
    is_original_spec: initialData?.is_original_spec ?? null,
    overall_a: initialData?.overall_a ?? "",
    overall_b: initialData?.overall_b ?? "",
    overall_c: initialData?.overall_c ?? "",
    tolerance_a: initialData?.tolerance_a ?? "",
    tolerance_b: initialData?.tolerance_b ?? "",
    tolerance_c: initialData?.tolerance_c ?? "",
    f_shank_min: initialData?.f_shank_min ?? "",
    f_shank_max: initialData?.f_shank_max ?? "",
    b2b_min: initialData?.b2b_min ?? "",
    b2b_max: initialData?.b2b_max ?? "",
    h2h_min: initialData?.h2h_min ?? "",
    h2h_max: initialData?.h2h_max ?? "",
    chassis_span1: initialData?.chassis_span1 ?? "",
    chassis_span2: initialData?.chassis_span2 ?? "",
    source: initialData?.source ?? "",
    description: initialData?.description ?? "", // 🆕 เพิ่มตรงนี้
  }));

  useEffect(() => {
    Promise.all([
      axios.get("/api/TypeModels"),
      axios.get("/api/Tools"),
      axios.get("/api/SizeRefs"),
      axios.get("/api/AxleTypes"),
    ]).then(([t, tool, s, a]) => {
      setTypes(
        t.data.map((d: any) => ({
          value: d.type_id ?? null,
          label: d.type_name ?? "(Blanks)",
        }))
      );
      setTools(
        tool.data.map((d: any) => ({
          value: d.tool_id ?? null,
          label: d.tool_name ?? "(Blanks)",
        }))
      );
      setSizes(
        s.data.map((d: any) => ({
          value: d.size_ref_id ?? null,
          label: d.size_ref ?? "(Blanks)",
        }))
      );
      setAxles(
        a.data.map((d: any) => ({
          value: d.axle_type_id ?? null,
          label: d.axle_type ?? "(Blanks)",
        }))
      );
    });
  }, []);

  const getLabelById = (list: Option[], id: number | null) => {
    return list.find((x) => x.value === id)?.label || null;
  };

  // const handleSubmit = async () => {
  //   const requiredFields = [
  //     form.type_id,
  //     form.tool_id,
  //     form.size_ref_id,
  //     form.axle_type_id,
  //     form.knurling_type,
  //     form.is_original_spec,
  //   ];

  //   if (requiredFields.some((v) => v === null || v === undefined)) {
  //     await MySwal.fire({
  //       icon: "warning",
  //       title: "Missing Fields",
  //       text: "Please fill in all required fields marked with *.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
  //       },
  //     });
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

  //   const confirm = await MySwal.fire({
  //     title: "Confirm submission?",
  //     text: "Do you want to add this new tool?",
  //     icon: "question",
  //     showCancelButton: true,
  //     confirmButtonText: "Yes, Add",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       confirmButton:
  //         "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //       popup: "text-sm",
  //     },
  //   });

  //   if (!confirm.isConfirmed) return;

  //   try {
  //     // ✅ STEP 0: Check for duplicate ToolRefSpecs
  //     const checkDuplicate = await axios.get(
  //       "/api/ToolRefSpecs/check-duplicate",
  //       {
  //         params: {
  //           type_id: form.type_id,
  //           tool_id: form.tool_id,
  //           size_ref_id: form.size_ref_id,
  //           axle_type_id: form.axle_type_id,
  //         },
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     if (checkDuplicate.data.exists) {
  //       await MySwal.fire({
  //         icon: "warning",
  //         title: "Duplicate Found",
  //         text: "This tool spec already exists with the same Tool + Type + Size + Axle.",
  //       });
  //       return;
  //     }

  //     // ✅ STEP 1: Check or Create ToolKeyOriginal
  //     const checkOriginalKey = await axios.get(
  //       "/api/ToolKeyOriginals/check",
  //       {
  //         params: {
  //           tool_id: form.tool_id,
  //           type_id: form.type_id,
  //           size_ref_id: form.size_ref_id,
  //           knurling_type: form.knurling_type,
  //         },
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     let newToolKeyId = null;

  //     if (checkOriginalKey.data.exists) {
  //       newToolKeyId = checkOriginalKey.data.tool_key_id;
  //       console.log("🔁 Using existing tool_key_id:", newToolKeyId);
  //     } else {
  //       const res1 = await axios.post(
  //         "/api/ToolKeyOriginals",
  //         {
  //           tool_id: form.tool_id,
  //           type_id: form.type_id,
  //           size_ref_id: form.size_ref_id,
  //           knurling_type: form.knurling_type,
  //         },
  //         {
  //           headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //         }
  //       );
  //       newToolKeyId = res1.data.tool_key_id;
  //       console.log("✅ Created new tool_key_id:", newToolKeyId);
  //     }

  //     // ✅ STEP 2: Insert ToolRefSpecs
  //     await axios.post(
  //       "/api/ToolRefSpecs",
  //       {
  //         tool_key_id: newToolKeyId,
  //         axle_type_id: form.axle_type_id,
  //         overall_a: parseFloatSafe(form.overall_a),
  //         overall_b: parseFloatSafe(form.overall_b),
  //         overall_c: parseFloatSafe(form.overall_c),
  //         tolerance_a: parseFloatSafe(form.tolerance_a),
  //         tolerance_b: parseFloatSafe(form.tolerance_b),
  //         tolerance_c: parseFloatSafe(form.tolerance_c),
  //         f_shank_min: parseFloatSafe(form.f_shank_min),
  //         f_shank_max: parseFloatSafe(form.f_shank_max),
  //         chassis_span1: parseFloatSafe(form.chassis_span1),
  //         chassis_span2: parseFloatSafe(form.chassis_span2),
  //         b2b_min: parseFloatSafe(form.b2b_min),
  //         b2b_max: parseFloatSafe(form.b2b_max),
  //         h2h_min: parseFloatSafe(form.h2h_min),
  //         h2h_max: parseFloatSafe(form.h2h_max),
  //         source: form.source ?? "",
  //         is_original_spec: 1,
  //         knurling_type: form.knurling_type,
  //       },
  //       {
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     // ✅ STEP 3: Check for duplicate ToolKeyAlls
  //     const checkToolKeyAll = await axios.get(
  //       "/api/ToolKeyAlls/check-original",
  //       {
  //         params: {
  //           type_id: form.type_id,
  //           tool_id: form.tool_id,
  //           type_ref_id: 13,
  //           tool_ref_id: 2,
  //           size_ref_id: form.size_ref_id,
  //           source_original_key_id: newToolKeyId,
  //         },
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     if (!checkToolKeyAll.data.exists) {
  //       await axios.post(
  //         "/api/ToolKeyAlls",
  //         {
  //           type_id: form.type_id,
  //           tool_id: form.tool_id,
  //           type_ref_id: 13,
  //           tool_ref_id: 2,
  //           size_ref_id: form.size_ref_id,
  //           source_original_key_id: newToolKeyId,
  //           original_spec: 1,
  //         },
  //         {
  //           headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //         }
  //       );
  //       console.log("✅ Inserted into toolKeyAlls");
  //     } else {
  //       console.log("🔁 toolKeyAll already exists — skipping insert");
  //     }

  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Added!",
  //       text: "New tool has been successfully added.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     onSubmitSuccess();
  //     onClose();
  //   } catch (error: any) {
  //     console.error("Insert failed:", error);

  //     let errorMsg = "Something went wrong while adding the tool.";
  //     if (axios.isAxiosError(error)) {
  //       if (typeof error.response?.data === "string") {
  //         errorMsg = error.response.data;
  //       } else if (error.response?.data?.message) {
  //         errorMsg = error.response.data.message;
  //       }
  //     } else if (error?.message) {
  //       errorMsg = error.message;
  //     }

  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Insert Failed",
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
    const requiredFields = [
      form.type_id,
      form.tool_id,
      form.size_ref_id,
      form.axle_type_id,
      form.knurling_type,
      form.is_original_spec,
    ];

    if (requiredFields.some((v) => v === null || v === undefined)) {
      await MySwal.fire({
        icon: "warning",
        title: "Missing Fields",
        text: "Please fill in all required fields marked with *.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
        },
      });
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

    // ✅ Add validation before confirm dialog
    if (form.is_original_spec === 1 && form.knurling_type !== 0) {
      await MySwal.fire({
        icon: "warning",
        title: "Invalid Knurling Type",
        text: "Original Spec must have Knurling Type = 0.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    if (form.is_original_spec === 0 && form.knurling_type === 0) {
      await MySwal.fire({
        icon: "warning",
        title: "Invalid Knurling Type",
        text: "A original spec cannot have Knurling Type = 0. Only original specs are allowed to use knurling type 0.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded",
        },
      });
      return;
    }

    const confirm = await MySwal.fire({
      title: "Confirm submission?",
      text: "Do you want to submit this request for adding new tool spec?",
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

    try {
      const payload = {
        tool_id: form.tool_id,
        type_id: form.type_id,
        size_ref_id: form.size_ref_id,
        axle_type_id: form.axle_type_id,

        tool_type: getLabelById(types, form.type_id), // 🟢 เพิ่ม
        tool_name: getLabelById(tools, form.tool_id), // 🟢 เพิ่ม
        size_ref: getLabelById(sizes, form.size_ref_id), // 🟢 เพิ่ม
        axle_type: getLabelById(axles, form.axle_type_id), // 🟢 เพิ่ม

        overall_a: parseFloatSafe(form.overall_a),
        overall_b: parseFloatSafe(form.overall_b),
        overall_c: parseFloatSafe(form.overall_c),
        tolerance_a: parseFloatSafe(form.tolerance_a),
        tolerance_b: parseFloatSafe(form.tolerance_b),
        tolerance_c: parseFloatSafe(form.tolerance_c),
        f_shank_min: parseFloatSafe(form.f_shank_min),
        f_shank_max: parseFloatSafe(form.f_shank_max),
        chassis_span: form.chassis_span ?? null,
        chassis_span1: parseFloatSafe(form.chassis_span1),
        chassis_span2: parseFloatSafe(form.chassis_span2),
        b2b_min: parseFloatSafe(form.b2b_min),
        b2b_max: parseFloatSafe(form.b2b_max),
        h2h_min: parseFloatSafe(form.h2h_min),
        h2h_max: parseFloatSafe(form.h2h_max),
        source: form.source ?? "",
        description: form.description ?? "", // 🆕 ส่งไปด้วย
        is_original_spec: form.is_original_spec,
        knurling_type: form.knurling_type,
      };

      const res = await axios.post(
        "/api/Requests",
        {
          request_type: "INSERT",
          target_table: "OriginalSpec",
          target_pk_id: null,
          old_data: null,
          new_data: payload,
          note: form.note || null,
        },
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      console.log("✅ Request submitted:", res.data);

      await MySwal.fire({
        icon: "info",
        title: "Request Submitted",
        html: `Your request to add a new original tool has been submitted.<br>Request ID: <b>${res.data.request_id}</b><br>It will be effective after admin approval.`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });

      onSubmitSuccess();
      onClose();
    } catch (error: any) {
      console.error("Request submission failed:", error);

      let errorMsg = "Something went wrong while submitting the request.";
      if (axios.isAxiosError(error)) {
        if (typeof error.response?.data === "string") {
          errorMsg = error.response.data;
        } else if (error.response?.data?.message) {
          errorMsg = error.response.data.message;
        }
      } else if (error?.message) {
        errorMsg = error.message;
      }

      await MySwal.fire({
        icon: "error",
        title: "Submission Failed",
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
    if (val === null || val === undefined || val === "") return null;
    const num = parseFloat(val);
    return isNaN(num) ? null : num;
  };

  const renderFloatInput = (key: string, label: string) => (
    <div className="flex flex-col gap-1">
      <label className="text-xs font-medium text-gray-600">{label}</label>
      <input
        type="number"
        step="any"
        min="0"
        value={form[key]}
        onChange={(e) => {
          const val = e.target.value;
          setForm({ ...form, [key]: val });
        }}
        className="border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
      />
    </div>
  );

  return (
    <div className="fixed inset-0 bg-black bg-opacity-30 z-[999] flex justify-center items-center">
      <div className="bg-white p-6 rounded-2xl w-full max-w-5xl max-h-[90vh] overflow-auto shadow-xl">
        <h2 className="text-xl font-semibold text-gray-800 mb-6">
          Add New Tool
        </h2>

        {/* 🛠 Tool Info */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">🛠 Tool Info</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Tool Type <span className="text-red-500">*</span>
              </label>
              <Select
                options={types}
                value={normalizeInitialValue(
                  types,
                  form.type_id,
                  fromClone || false,
                  2002
                )}
                onChange={(o) => setForm({ ...form, type_id: o?.value })}
              />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Tool Name <span className="text-red-500">*</span>
              </label>
              <Select
                options={tools}
                value={normalizeInitialValue(
                  tools,
                  form.tool_id,
                  fromClone || false,
                  1002
                )}
                onChange={(o) => setForm({ ...form, tool_id: o?.value })}
              />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Size Ref <span className="text-red-500">*</span>
              </label>
              <Select
                options={sizes}
                value={normalizeInitialValue(
                  sizes,
                  form.size_ref_id,
                  fromClone || false,
                  -1 //edit this row.
                )}
                onChange={(o) => setForm({ ...form, size_ref_id: o?.value })}
              />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Axle Type <span className="text-red-500">*</span>
              </label>
              <Select
                options={axles}
                value={normalizeInitialValue(
                  axles,
                  form.axle_type_id,
                  fromClone || false,
                  -1
                )}
                onChange={(o) => setForm({ ...form, axle_type_id: o?.value })}
              />
            </div>
          </div>
        </div>

        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            ⚙️ Specification Info
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            <div>
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Knurling Type <span className="text-red-500">*</span>
              </label>
              <Select
                options={[
                  { value: 0, label: "No" },
                  { value: 1, label: "Yes" },
                ]}
                value={
                  form.knurling_type != null
                    ? {
                        value: form.knurling_type,
                        label: form.knurling_type === 1 ? "Yes" : "No",
                      }
                    : null
                }
                onChange={(o) => setForm({ ...form, knurling_type: o?.value })}
              />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Is Original Spec <span className="text-red-500">*</span>
              </label>
              <Select
                options={[
                  { value: 0, label: "No" },
                  { value: 1, label: "Yes" },
                ]}
                value={
                  form.is_original_spec != null
                    ? {
                        value: form.is_original_spec,
                        label: form.is_original_spec === 1 ? "Yes" : "No",
                      }
                    : null
                }
                onChange={(o) =>
                  setForm({ ...form, is_original_spec: o?.value })
                }
              />
            </div>
          </div>
        </div>

        {/* 📐 Wheel: Over All Dimension (mm) */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            📐 Wheel: Over All Dimension (mm)
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            {renderFloatInput("overall_a", "Overall A")}
            {renderFloatInput("overall_b", "Overall B")}
            {renderFloatInput("overall_c", "Overall C")}
            {renderFloatInput("tolerance_a", "Tolerance A")}
            {renderFloatInput("tolerance_b", "Tolerance B")}
            {renderFloatInput("tolerance_c", "Tolerance C")}
          </div>
        </div>

        {/* 🧱 Wheel: F-Shank (Inch) */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            🧱 Wheel: F-Shank (Inch)
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-2 gap-4">
            {renderFloatInput("f_shank_min", "F-Shank Min")}
            {renderFloatInput("f_shank_max", "F-Shank Max")}
          </div>
        </div>

        {/* 🔄 Bump to Bump (Inch) */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            🔄 Bump to Bump (Inch)
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-2 gap-4">
            {renderFloatInput("b2b_min", "B2B Min")}
            {renderFloatInput("b2b_max", "B2B Max")}
          </div>
        </div>

        {/* ↔️ Head to Head (Inch) */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            ↔️ Head to Head (Inch)
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-2 gap-4">
            {renderFloatInput("h2h_min", "H2H Min")}
            {renderFloatInput("h2h_max", "H2H Max")}
          </div>
        </div>

        {/* 🧩 Chassis Span */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            🧩 Chassis Span
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-2 gap-4">
            {renderFloatInput("chassis_span1", "Chassis Span 1")}
            {renderFloatInput("chassis_span2", "Chassis Span 2")}
          </div>
        </div>

        {/* ⚙️ Config */}
        <div className="mb-6">
          <h3 className="text-sm font-bold text-gray-700 mb-2">
            ⚙️ Configuration
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            <div className="md:col-span-1 col-span-2">
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Source
              </label>
              <input
                value={form.source}
                onChange={(e) => setForm({ ...form, source: e.target.value })}
                className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
                placeholder="Source"
              />
            </div>
            <div className="md:col-span-2 col-span-2">
              <label className="text-xs font-medium text-gray-600 mb-1 block">
                Description
              </label>
              <textarea
                value={form.description}
                onChange={(e) =>
                  setForm({ ...form, description: e.target.value })
                }
                className="w-full border px-3 py-1.5 rounded text-sm focus:outline-blue-500"
                placeholder="Optional description"
                rows={2}
              />
            </div>
          </div>
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
            Submit
          </button>
        </div>
      </div>
    </div>
  );
};

export default OriginalSpecAddToolModal;

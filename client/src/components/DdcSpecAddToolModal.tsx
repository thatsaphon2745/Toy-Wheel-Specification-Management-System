import React, { useEffect, useState } from "react";
import Select from "react-select";
import axios from "axios";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
import { createPortal } from "react-dom";

const MySwal = withReactContent(Swal);

const knurlingOptions: Option[] = [
  { value: 1, label: "Yes" },
  { value: 0, label: "No" },
];

export interface Option {
  value: number;
  label: string;
}

// interface Props {
//   onClose: () => void;
//   onSubmitSuccess: () => void;
// }

interface DdcSpecAddToolModalProps {
  initialData?: any;
  fromClone?: boolean;
  onClose: () => void;
  onSubmitSuccess: () => void;
}

const DdcSpecAddToolModal: React.FC<DdcSpecAddToolModalProps> = ({
  initialData,
  fromClone = false,
  onClose,
  onSubmitSuccess,
}) => {
  const [types, setTypes] = useState<Option[]>([]);
  const [tools, setTools] = useState<Option[]>([]);
  const [positions, setPositions] = useState<Option[]>([]);
  const [typeRefs, setTypeRefs] = useState<Option[]>([]);
  const [toolRefs, setToolRefs] = useState<Option[]>([]);
  const [sizeRefs, setSizeRefs] = useState<Option[]>([]);
  const [axles, setAxles] = useState<Option[]>([]);

  const [submitted, setSubmitted] = useState(false);

  const [matchingChassisSpans, setMatchingChassisSpans] = useState<{
    span1?: number;
    span2?: number;
  } | null>(null);

  const [selectedSpanOption, setSelectedSpanOption] = useState<
    "span1" | "span2" | "custom" | null
  >(null);

  const [form, setForm] = useState<any>(() => ({
    type_id: initialData?.type_id ?? null,
    tool_id: initialData?.tool_id ?? null,
    position_type_id: initialData?.position_type_id ?? null,
    type_ref_id: initialData?.type_ref_id ?? null,
    tool_ref_id: initialData?.tool_ref_id ?? null,
    size_ref_id: initialData?.size_ref_id ?? null,
    axle_type_id: initialData?.axle_type_id ?? null,
    chassis_span_override: initialData?.chassis_span_override ?? "",
    knurling_type: initialData?.knurling_type ?? null,
    description: initialData?.description ?? "", // ✅ เพิ่มตรงนี้
  }));

  useEffect(() => {
    Promise.all([
      axios.get("/api/TypeModels"),
      axios.get("/api/Tools"),
      axios.get("/api/PositionTypes"),
      axios.get("/api/SizeRefs"),
      axios.get("/api/AxleTypes"),
    ]).then(([t, tool, pos, size, axle]) => {
      const toOption = (arr: any[], label: string, value: string) =>
        arr.map((d) => ({ value: d[value], label: d[label] ?? "(Blanks)" }));

      setTypes(toOption(t.data, "type_name", "type_id"));
      setTypeRefs(toOption(t.data, "type_name", "type_id"));
      setTools(toOption(tool.data, "tool_name", "tool_id"));
      setToolRefs(toOption(tool.data, "tool_name", "tool_id"));
      setPositions(toOption(pos.data, "position_type", "position_type_id"));
      setSizeRefs(toOption(size.data, "size_ref", "size_ref_id"));
      setAxles(toOption(axle.data, "axle_type", "axle_type_id"));
    });
  }, [submitted]);

  useEffect(() => {
    const { type_ref_id, tool_ref_id, size_ref_id, axle_type_id } = form;

    const getLabel = (options: Option[], value: number) =>
      options.find((o) => o.value === value)?.label ?? null;

    const match = (label: string | null, value: any) => {
      if (label === "(Blanks)") return value === null;
      return (value ?? "").toLowerCase().trim() === label?.toLowerCase().trim();
    };

    if (type_ref_id && tool_ref_id && size_ref_id && axle_type_id) {
      const type_ref = getLabel(typeRefs, type_ref_id);
      const tool_ref = getLabel(toolRefs, tool_ref_id);
      const size_ref = getLabel(sizeRefs, size_ref_id);
      const axle_type = getLabel(axles, axle_type_id);

      console.log("🔍 Searching for span with:", {
        tool_type: type_ref,
        tool_name: tool_ref,
        size_ref,
        axle_type,
      });

      axios
        .get("/api/ResolvedOriginals/spec")
        .then((res) => {
          const matched = res.data.find((item: any) => {
            return (
              match(type_ref, item.tool_type) &&
              match(tool_ref, item.tool_name) &&
              match(size_ref, item.size_ref) &&
              match(axle_type, item.axle_type)
            );
          });

          console.log("✅ Matched Result:", matched);

          if (matched) {
            const span1 = parseFloat(matched.chassis_span1);
            const span2 = parseFloat(matched.chassis_span2);

            console.log("✅ Chassis Span 1:", span1);
            console.log("✅ Chassis Span 2:", span2);

            setMatchingChassisSpans({
              span1: isNaN(span1) ? undefined : span1,
              span2: isNaN(span2) ? undefined : span2,
            });
          } else {
            setMatchingChassisSpans(null);
          }
        })
        .catch((err) => {
          console.error("❌ Failed to fetch chassis spans:", err);
          setMatchingChassisSpans(null);
        });
    } else {
      setMatchingChassisSpans(null);
    }
  }, [form.type_ref_id, form.tool_ref_id, form.size_ref_id, form.axle_type_id]);

  const getLabel = (options: Option[], value: number) =>
    options.find((o) => o.value === value)?.label ?? null;

  const handleSubmit = async () => {
    const required = [
      form.type_id,
      form.tool_id,
      form.position_type_id,
      form.type_ref_id,
      form.tool_ref_id,
      form.size_ref_id,
      form.axle_type_id,
      form.knurling_type,
      form.chassis_span_override,
    ];

    if (required.some((v) => v === null || v === undefined)) {
      await MySwal.fire({
        icon: "warning",
        title: "Missing fields",
        text: "Please fill all required fields.",
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

    const confirm = await MySwal.fire({
      title: "Confirm submission?",
      text: "Do you want to submit this request for adding new DDC tool?",
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
        type_id: form.type_id,
        tool_id: form.tool_id,
        position_type_id: form.position_type_id,
        type_ref_id: form.type_ref_id,
        tool_ref_id: form.tool_ref_id,
        size_ref_id: form.size_ref_id,
        axle_type_id: form.axle_type_id,

        tool_type: getLabel(types, form.type_id),
        tool_name: getLabel(tools, form.tool_id),
        position_type: getLabel(positions, form.position_type_id),
        type_ref: getLabel(typeRefs, form.type_ref_id),
        tool_ref: getLabel(toolRefs, form.tool_ref_id),
        size_ref: getLabel(sizeRefs, form.size_ref_id),
        axle_type: getLabel(axles, form.axle_type_id),

        chassis_span_override: parseFloatSafe(form.chassis_span_override),
        knurling_type: form.knurling_type,
        description: form.description,
      };

      const res = await axios.post(
        "/api/Requests",
        {
          request_type: "INSERT",
          target_table: "DdcSpec",
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

      // await MySwal.fire({
      //   icon: "success",
      //   title: "Submitted!",
      //   text: "Your request has been submitted and is pending approval.",
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
    } catch (error: unknown) {
      console.error("Request submission failed:", error);

      let errorMsg = "Something went wrong while submitting the request.";

      if (axios.isAxiosError(error)) {
        if (typeof error.response?.data === "string") {
          errorMsg = error.response.data;
        } else if (error.response?.data?.message) {
          errorMsg = error.response.data.message;
        }
      } else if (error && typeof error === "object" && "message" in error) {
        errorMsg = (error as any).message;
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

  const parseFloatSafe = (val: any): number | null => {
    if (val === null || val === undefined || val === "") return null;

    const str = String(val).trim();
    const num = parseFloat(str);
    if (isNaN(num)) return null;

    const decimalPlaces = str.includes(".") ? str.split(".")[1].length : 0;
    return parseFloat(num.toFixed(decimalPlaces));
  };

  // const renderSelect = (key: string, label: string, options: Option[]) => (
  //   <div>
  //     <label className="text-xs font-medium text-gray-600 block mb-1">
  //       {label}
  //     </label>
  //     <Select
  //       options={options}
  //       onChange={(o) => setForm({ ...form, [key]: o?.value })}
  //       isClearable
  //       menuPortalTarget={document.body}
  //       styles={{
  //         menuPortal: (base) => ({ ...base, zIndex: 9999 }),
  //       }}
  //     />
  //   </div>
  // );
  const renderSelect = (key: string, label: string, options: Option[]) => (
    <div>
      <label className="text-xs font-medium text-gray-600 block mb-1">
        {label}
      </label>
      <Select
        options={options}
        value={options.find((o) => o.value === form[key]) || null}
        onChange={(o) => setForm({ ...form, [key]: o?.value })}
        isClearable
        menuPortalTarget={document.body}
        styles={{
          menuPortal: (base) => ({ ...base, zIndex: 9999 }),
        }}
      />
    </div>
  );

  return createPortal(
    <div className="fixed inset-0 bg-black bg-opacity-30 z-[999] flex justify-center items-center">
      <div className="bg-white p-6 rounded-2xl w-full max-w-4xl max-h-[90vh] overflow-auto shadow-xl">
        <h2 className="text-xl font-semibold text-gray-800 mb-6">
          Add DDC Tool Spec
        </h2>

        <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
          {renderSelect("type_id", "Tool Type", types)}
          {renderSelect("tool_id", "Tool Name", tools)}
          {renderSelect("position_type_id", "Position Type", positions)}
          {renderSelect("type_ref_id", "Type Ref", typeRefs)}
          {renderSelect("tool_ref_id", "Tool Ref", toolRefs)}
          {renderSelect("size_ref_id", "Size Ref", sizeRefs)}
          {renderSelect("axle_type_id", "Axle Type", axles)}
          {renderSelect("knurling_type", "Knurling", knurlingOptions)}
          {/* <div>
            <label className="text-xs font-medium text-gray-600 block mb-1">
              Chassis Span Override
            </label>
            <input
              type="number"
              step="any"
              value={form.chassis_span_override}
              onChange={(e) =>
                setForm({ ...form, chassis_span_override: e.target.value })
              }
              className="border px-3 py-1.5 rounded text-sm w-full"
            />
          </div> */}
          <div className="col-span-2 md:col-span-1">
            <label className="text-xs font-medium text-gray-600 block mb-1">
              Chassis Span Override
            </label>

            {matchingChassisSpans?.span1 || matchingChassisSpans?.span2 ? (
              <div className="space-y-2">
                {matchingChassisSpans?.span1 && (
                  <label className="flex items-center gap-2 text-sm">
                    <input
                      type="radio"
                      name="chassis_span_option"
                      checked={selectedSpanOption === "span1"}
                      onChange={() => {
                        setSelectedSpanOption("span1");
                        setForm({
                          ...form,
                          chassis_span_override: matchingChassisSpans.span1,
                        });
                      }}
                    />
                    Chassis Span 1:{" "}
                    <span className="text-blue-600">
                      {matchingChassisSpans.span1}
                    </span>
                  </label>
                )}

                {matchingChassisSpans?.span2 && (
                  <label className="flex items-center gap-2 text-sm">
                    <input
                      type="radio"
                      name="chassis_span_option"
                      checked={selectedSpanOption === "span2"}
                      onChange={() => {
                        setSelectedSpanOption("span2");
                        setForm({
                          ...form,
                          chassis_span_override: matchingChassisSpans.span2,
                        });
                      }}
                    />
                    Chassis Span 2:{" "}
                    <span className="text-blue-600">
                      {matchingChassisSpans.span2}
                    </span>
                  </label>
                )}

                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="radio"
                    name="chassis_span_option"
                    checked={selectedSpanOption === "custom"}
                    onChange={() => {
                      setSelectedSpanOption("custom");
                      setForm({ ...form, chassis_span_override: "" });
                    }}
                  />
                  Enter custom:
                </label>

                <input
                  type="number"
                  step="any"
                  value={form.chassis_span_override}
                  onChange={(e) =>
                    setForm({ ...form, chassis_span_override: e.target.value })
                  }
                  className="border px-3 py-1.5 rounded text-sm w-full"
                  disabled={selectedSpanOption !== "custom"}
                />
              </div>
            ) : (
              <input
                type="number"
                step="any"
                value={form.chassis_span_override}
                onChange={(e) =>
                  setForm({ ...form, chassis_span_override: e.target.value })
                }
                className="border px-3 py-1.5 rounded text-sm w-full"
              />
            )}
          </div>
          <div className="col-span-3">
            <label className="text-xs font-medium text-gray-600 block mb-1">
              Description
            </label>
            <textarea
              value={form.description}
              onChange={(e) =>
                setForm({ ...form, description: e.target.value })
              }
              className="border px-3 py-1.5 rounded text-sm w-full"
              rows={3}
              placeholder="Enter description or notes"
            />
          </div>
        </div>

        <div className="flex justify-end gap-3">
          <button
            onClick={onClose}
            className="border px-5 py-1.5 rounded text-sm hover:bg-gray-100"
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
    </div>,
    document.body
  );
};

export default DdcSpecAddToolModal;

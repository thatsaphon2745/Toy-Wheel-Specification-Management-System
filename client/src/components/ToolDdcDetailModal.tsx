import React, { useEffect, useState } from "react";
import { Copy, X, Settings, Ruler, Info, CheckCircle } from "lucide-react";
import type { LucideIcon } from "lucide-react";

import { createPortal } from "react-dom";

export interface DdcDetailData {
  tool_type: string;
  tool_name: string;
  position_type: string;
  type_ref: string;
  tool_ref: string;
  size_ref: string;
  axle_type: string;
  machine_no: string;
  source?: string;
  chassis_span: string | number;
  b2b_min: string | number;
  b2b_max: string | number;
  h2h_min: string | number;
  h2h_max: string | number;
  f_shank_min: string | number;
  f_shank_max: string | number;
  overall_a: string | number;
  tolerance_a: string | number;
  overall_b: string | number;
  tolerance_b: string | number;
  overall_c: string | number;
  tolerance_c: string | number;
  knurling_type: number;
}

export interface ToolDdcDetailModalProps {
  showDetailModal: boolean;
  setShowDetailModal: (show: boolean) => void;
  detailData: DdcDetailData | null;
  definitions1: string;
  definitions2: string;
}

const ToolDdcDetailModal: React.FC<ToolDdcDetailModalProps> = ({
  showDetailModal,
  setShowDetailModal,
  detailData,
  definitions1,
  definitions2,
}) => {
  const [copiedField, setCopiedField] = useState<string | null>(null);

  const copyToClipboard = async (value: string, fieldName: string) => {
    try {
      await navigator.clipboard.writeText(value);
      setCopiedField(fieldName);
      setTimeout(() => setCopiedField(null), 2000);
    } catch (err) {
      console.error("Failed to copy: ", err);
    }
  };

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === "Escape") {
      setShowDetailModal(false);
    }
  };

  useEffect(() => {
    if (showDetailModal) {
      document.addEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "hidden";
      return () => {
        document.removeEventListener("keydown", handleKeyDown);
        document.body.style.overflow = "unset";
      };
    }
  }, [showDetailModal]);

  if (!showDetailModal || !detailData) return null;

  interface SpecFieldProps {
    label: string;
    value: string | number;
    unit?: string;
    icon?: LucideIcon;
  }

  const SpecField: React.FC<SpecFieldProps> = ({
    label,
    value,
    unit = "",
    icon: Icon,
  }) => {
    const fieldKey = `${label}-${value}`;
    const isCopied = copiedField === fieldKey;

    return (
      <div className="group relative">
        <div className="flex items-center justify-between p-3 bg-white rounded-lg border border-gray-200 hover:border-blue-300 hover:shadow-sm transition-all duration-200">
          <div className="flex items-center space-x-3">
            {Icon && <Icon className="h-4 w-4 text-gray-400" />}
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                {label}
              </p>
              <p className="text-sm font-semibold text-gray-900 mt-1">
                {value} {unit}
              </p>
            </div>
          </div>
          <button
            onClick={() => copyToClipboard(`${value} ${unit}`.trim(), fieldKey)}
            className="opacity-0 group-hover:opacity-100 p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-all duration-200"
            title="Copy value"
          >
            {isCopied ? (
              <CheckCircle className="h-4 w-4 text-green-600" />
            ) : (
              <Copy className="h-4 w-4" />
            )}
          </button>
        </div>
        {isCopied && (
          <div className="absolute -top-8 right-0 bg-green-600 text-white text-xs px-2 py-1 rounded shadow-lg">
            Copied!
          </div>
        )}
      </div>
    );
  };

  interface RangeFieldProps {
    label: string;
    min: string | number;
    max: string | number;
    unit?: string;
    icon?: LucideIcon;
  }

  const RangeField: React.FC<RangeFieldProps> = ({
    label,
    min,
    max,
    unit = "",
    icon: Icon,
  }) => {
    const fieldKey = `${label}-${min}-${max}`;
    const isCopied = copiedField === fieldKey;

    return (
      <div className="group relative">
        <div className="flex items-center justify-between p-3 bg-white rounded-lg border border-gray-200 hover:border-blue-300 hover:shadow-sm transition-all duration-200">
          <div className="flex items-center space-x-3">
            {Icon && <Icon className="h-4 w-4 text-gray-400" />}
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                {label}
              </p>
              <p className="text-sm font-semibold text-gray-900 mt-1">
                {min}
                {unit} - {max}
                {unit}
              </p>
            </div>
          </div>
          <button
            onClick={() =>
              copyToClipboard(`${min}${unit} - ${max}${unit}`, fieldKey)
            }
            className="opacity-0 group-hover:opacity-100 p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-all duration-200"
            title="Copy range"
          >
            {isCopied ? (
              <CheckCircle className="h-4 w-4 text-green-600" />
            ) : (
              <Copy className="h-4 w-4" />
            )}
          </button>
        </div>
        {isCopied && (
          <div className="absolute -top-8 right-0 bg-green-600 text-white text-xs px-2 py-1 rounded shadow-lg">
            Copied!
          </div>
        )}
      </div>
    );
  };

  interface ToleranceFieldProps {
    label: string;
    value: string | number;
    tolerance: string | number;
    unit?: string;
    icon?: LucideIcon;
  }

  const ToleranceField: React.FC<ToleranceFieldProps> = ({
    label,
    value,
    tolerance,
    unit = "mm",
    icon: Icon,
  }) => {
    const fieldKey = `${label}-${value}-${tolerance}`;
    const isCopied = copiedField === fieldKey;

    return (
      <div className="group relative">
        <div className="flex items-center justify-between p-3 bg-white rounded-lg border border-gray-200 hover:border-blue-300 hover:shadow-sm transition-all duration-200">
          <div className="flex items-center space-x-3">
            {Icon && <Icon className="h-4 w-4 text-gray-400" />}
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                {label}
              </p>
              <p className="text-sm font-semibold text-gray-900 mt-1">
                {value} ± {tolerance} {unit}
              </p>
            </div>
          </div>
          <button
            onClick={() =>
              copyToClipboard(`${value} ± ${tolerance} ${unit}`, fieldKey)
            }
            className="opacity-0 group-hover:opacity-100 p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-all duration-200"
            title="Copy tolerance"
          >
            {isCopied ? (
              <CheckCircle className="h-4 w-4 text-green-600" />
            ) : (
              <Copy className="h-4 w-4" />
            )}
          </button>
        </div>
        {isCopied && (
          <div className="absolute -top-8 right-0 bg-green-600 text-white text-xs px-2 py-1 rounded shadow-lg">
            Copied!
          </div>
        )}
      </div>
    );
  };

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-6xl max-h-[95vh] mx-4 overflow-hidden">
        <div className="sticky top-0 z-10 bg-white border-b border-gray-200 px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="p-2 bg-blue-100 rounded-lg">
                <Settings className="h-5 w-5 text-blue-600" />
              </div>
              <h2 className="text-xl font-bold text-gray-900">
                Tool DDC Detail View
              </h2>
            </div>
            <button
              onClick={() => setShowDetailModal(false)}
              className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg"
              title="Close (Esc)"
            >
              <X className="h-5 w-5" />
            </button>
          </div>
        </div>

        <div
          className="overflow-y-auto px-6 py-6"
          style={{ maxHeight: "calc(95vh - 80px)" }}
        >
          {/* Tool Info */}
          <div className="mb-8">
            <div className="flex items-center space-x-2 mb-4">
              <Info className="h-5 w-5 text-blue-600" />
              <h3 className="text-lg font-semibold text-gray-900">
                Tool Information
              </h3>
            </div>
            <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
              <SpecField
                label="Tool Type"
                value={detailData.tool_type}
                icon={Settings}
              />
              <SpecField
                label="Tool Name"
                value={detailData.tool_name}
                icon={Info}
              />
              <SpecField
                label="Position Type"
                value={detailData.position_type}
                icon={Info}
              />
              <SpecField
                label="Type Ref"
                value={detailData.type_ref}
                icon={Settings}
              />
              <SpecField
                label="Tool Ref"
                value={detailData.tool_ref}
                icon={Settings}
              />
              <SpecField
                label="Size Ref"
                value={detailData.size_ref}
                icon={Ruler}
              />
              <SpecField
                label="Axle Type"
                value={detailData.axle_type}
                icon={Settings}
              />
              <SpecField
                label="Knurling Type"
                value={detailData.knurling_type}
                icon={Settings}
              />
              {/* <SpecField
                label="Machine No"
                value={detailData.machine_no}
                icon={Settings}
              /> */}
              {detailData.source && (
                <SpecField
                  label="Source"
                  value={detailData.source}
                  icon={Info}
                />
              )}
            </div>
          </div>

          {/* Diagrams */}
          <div className="mb-8">
            <div className="flex items-center space-x-2 mb-4">
              <Ruler className="h-5 w-5 text-blue-600" />
              <h3 className="text-lg font-semibold text-gray-900">
                Technical Diagrams
              </h3>
            </div>
            <div className="grid gap-6 grid-cols-1 lg:grid-cols-2">
              {/* Diagram 1 */}
              <div className="bg-gray-50 rounded-xl p-4 border border-gray-200">
                <h4 className="text-sm font-medium text-gray-700 mb-3">
                  Chassis Span & F-Shank Specifications
                </h4>
                <div className="relative">
                  <img
                    src={definitions1}
                    alt="Diagram 1"
                    className="w-full h-auto rounded-lg border shadow-sm"
                  />

                  <div className="absolute top-[12%] left-[50%] transform -translate-x-1/2">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.chassis_span}"
                    </div>
                  </div>

                  <div className="absolute top-[80%] left-[51%] transform -translate-x-1/2">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.b2b_min}" - {detailData.b2b_max}"
                    </div>
                  </div>

                  <div className="absolute top-[93%] left-[51%] transform -translate-x-1/2">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.h2h_min}" - {detailData.h2h_max}"
                    </div>
                  </div>

                  <div className="absolute top-[70%] left-[79.5%] transform -translate-x-1/2">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.f_shank_min}" - {detailData.f_shank_max}"
                    </div>
                  </div>
                </div>
              </div>

              {/* Diagram 2 */}
              <div className="bg-gray-50 rounded-xl p-4 border border-gray-200">
                <h4 className="text-sm font-medium text-gray-700 mb-3">
                  Overall Dimensions
                </h4>
                <div className="relative">
                  <img
                    src={definitions2}
                    alt="Diagram 2"
                    className="w-full h-auto rounded-lg border shadow-sm"
                  />

                  <div className="absolute top-[45%] left-[57%]">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.overall_a} ± {detailData.tolerance_a} mm
                    </div>
                  </div>

                  <div className="absolute top-[72%] left-[25%]">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.overall_b} ± {detailData.tolerance_b} mm
                    </div>
                  </div>

                  <div className="absolute top-[20%] left-[28%] transform -translate-x-1/2">
                    <div className="bg-white/80 text-gray-800 text-xs font-semibold px-3 py-1.5 rounded-full shadow-md">
                      {detailData.overall_c} ± {detailData.tolerance_c} mm
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Detailed Specs */}
          <div className="grid gap-8 lg:grid-cols-2">
            <div>
              <h4 className="text-md font-semibold text-gray-900 mb-4">
                Chassis Specifications
              </h4>
              <div className="space-y-3">
                <RangeField
                  label="Chassis Span"
                  min={detailData.chassis_span}
                  max={detailData.chassis_span}
                  unit={`"`}
                  icon={Ruler}
                />
                <RangeField
                  label="B2B Range"
                  min={detailData.b2b_min}
                  max={detailData.b2b_max}
                  unit={`"`}
                  icon={Ruler}
                />
                <RangeField
                  label="H2H Range"
                  min={detailData.h2h_min}
                  max={detailData.h2h_max}
                  unit={`"`}
                  icon={Ruler}
                />
                <RangeField
                  label="F Shank Range"
                  min={detailData.f_shank_min}
                  max={detailData.f_shank_max}
                  unit={`"`}
                  icon={Ruler}
                />
              </div>
            </div>
            <div>
              <h4 className="text-md font-semibold text-gray-900 mb-4">
                Overall Dimensions
              </h4>
              <div className="space-y-3">
                <ToleranceField
                  label="Overall A"
                  value={detailData.overall_a}
                  tolerance={detailData.tolerance_a}
                  icon={Ruler}
                />
                <ToleranceField
                  label="Overall B"
                  value={detailData.overall_b}
                  tolerance={detailData.tolerance_b}
                  icon={Ruler}
                />
                <ToleranceField
                  label="Overall C"
                  value={detailData.overall_c}
                  tolerance={detailData.tolerance_c}
                  icon={Ruler}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>,
    document.body
  );
};

export default ToolDdcDetailModal;

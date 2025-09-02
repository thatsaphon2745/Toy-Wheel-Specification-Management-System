import React, { useState } from "react";
import { X, Info } from "lucide-react";

import { createPortal } from "react-dom";

export interface DefModalProps {
  showDefModal: boolean;
  setShowDefModal: (show: boolean) => void;
  definitions1: string;
  definitions2: string;
}

const DefModal: React.FC<DefModalProps> = ({
  showDefModal,
  setShowDefModal,
  definitions1,
  definitions2,
}) => {
  const [viewMode, setViewMode] = useState<"field" | "axle">("field");

  if (!showDefModal) return null;

  const fieldDefinitions = [
    ["Tool Type", "Series or model of the tool (e.g. HW, MB, Pixar)"],
    ["Tool Name", "Specific name of the tool or part."],
    ["Size Ref", "Reference size code for the tool."],
    ["Axle Type", "Type of axle used in the barbell assembly."],
    ["A", "Wheel Diameter (mm)."],
    ["B", "Wheel Thickness (mm)."],
    ["C", "Wheel Thickness (included wheel bump) (mm)."],
    ["Tolerance A", "Allowed deviation for A."],
    ["Tolerance B", "Allowed deviation for B."],
    ["Tolerance C", "Allowed deviation for C."],
    ["F-shank Min", "Minimum length from wheel hole bottom to bump."],
    ["F-shank Max", "Maximum length from wheel hole bottom to bump."],
    ["B2B Min", "Minimum span wheel bump to bump."],
    ["B2B Max", "Maximum span wheel bump to bump."],
    ["H2H Min", "Minimum span axle head to axle head."],
    ["H2H Max", "Maximum span axle head to axle head."],
    ["Chassis Span 1", "Lower chassis span (inches)."],
    ["Chassis Span 2", "Upper chassis span (inches)."],
    ["Pad", "Pad type used (e.g. HST, RIM)."],
    ["Brass", "Brass no. or description."],
    ["Pad Source Key", "Reference key for pad spec."],
    ["Machine No", "Machine ID used for manufacturing."],
    ["Machine Source Key", "Reference key for machine details."],
    ["Source", "Data source of the spec."],
    ["Original Spec", "Whether spec is original or derived."],
  ];

  const axleDefinitions = [
    ["SA", "Short Axle"],
    ["MA", "Medium Axle"],
    ["IA", "Intermediate Axle"],
    ["RA", "Racing Axle"],
    ["SRA", "Step Racing Axle"],
    ["LA", "Long Axle"],
    ["TSA", "Top Speed Axle"],
    ["CA", "Camaro Axle"],
    ["SCA", "Step Camaro Axle"],
    ["XLA", "Extra Long Axle"],
    ["S2", "(no definition)"],
    ["DA", "Direct Axle"],
    ["S3", "(no definition)"],
    ["PSA", "Pixar Short Axle"],
    ["PMA", "Pixar Medium Axle"],
    ["PIA", "Pixar Intermediate Axle"],
  ];

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-6xl max-h-[95vh] mx-4 overflow-hidden">
        {/* Sticky Header */}
        <div className="sticky top-0 z-10 bg-white border-b border-gray-200 px-6 py-4">
          <div className="flex justify-between items-center">
            <div className="flex items-center space-x-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <Info className="h-5 w-5 text-blue-600" />
              </div>
              <h2 className="text-xl font-bold text-gray-900">
                Field & Axle Type Definitions
              </h2>
            </div>

            <div className="flex items-center space-x-3">
              {/* Toggle View */}
              <div className="flex bg-gray-100 rounded-lg p-1">
                <button
                  onClick={() => setViewMode("field")}
                  className={`px-3 py-1.5 text-sm font-medium rounded-md ${
                    viewMode === "field"
                      ? "bg-white text-gray-900 shadow-sm"
                      : "text-gray-600 hover:text-gray-900"
                  }`}
                >
                  Field
                </button>
                <button
                  onClick={() => setViewMode("axle")}
                  className={`px-3 py-1.5 text-sm font-medium rounded-md ${
                    viewMode === "axle"
                      ? "bg-white text-gray-900 shadow-sm"
                      : "text-gray-600 hover:text-gray-900"
                  }`}
                >
                  Axle Type
                </button>
              </div>

              <button
                onClick={() => setShowDefModal(false)}
                className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg"
                title="Close"
              >
                <X className="h-5 w-5" />
              </button>
            </div>
          </div>
        </div>

        {/* Scrollable Content */}
        <div
          className="overflow-y-auto px-6 py-6"
          style={{ maxHeight: "calc(95vh - 80px)" }}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Images */}
            <div className="space-y-6">
              <img
                src={definitions2}
                alt="Definitions Diagram 2"
                className="max-w-[350px] w-full h-auto mx-auto rounded-lg border border-gray-300 shadow-sm object-contain"
              />
              <img
                src={definitions1}
                alt="Definitions Diagram 1"
                className="max-w-[350px] w-full h-auto mx-auto rounded-lg border border-gray-300 shadow-sm object-contain"
              />
            </div>

            {/* Tables */}
            <div>
              {viewMode === "field" && (
                <div className="mb-8">
                  <h3 className="text-lg font-semibold text-gray-800 mb-4">
                    Field Definitions
                  </h3>
                  <table className="min-w-full border border-gray-300 text-sm">
                    <thead className="bg-gray-100">
                      <tr>
                        <th className="py-2 px-4 text-left text-gray-700 font-semibold w-48">
                          Field
                        </th>
                        <th className="py-2 px-4 text-left text-gray-700 font-semibold">
                          Description
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {fieldDefinitions.map(([field, desc], idx) => (
                        <tr
                          key={field}
                          className={
                            idx % 2 === 0 ? "bg-white" : "bg-gray-50"
                          }
                        >
                          <td className="py-2 px-4 font-semibold text-gray-800">
                            {field}
                          </td>
                          <td className="py-2 px-4 text-gray-600">
                            {desc}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {viewMode === "axle" && (
                <div className="mb-8">
                  <h3 className="text-lg font-semibold text-gray-800 mb-4">
                    Axle Type Definitions
                  </h3>
                  <table className="min-w-full border border-gray-300 text-sm">
                    <thead className="bg-gray-100">
                      <tr>
                        <th className="py-2 px-4 text-left text-gray-700 font-semibold w-32">
                          Axle Type
                        </th>
                        <th className="py-2 px-4 text-left text-gray-700 font-semibold">
                          Description
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {axleDefinitions.map(([code, desc], idx) => (
                        <tr
                          key={code}
                          className={
                            idx % 2 === 0 ? "bg-white" : "bg-gray-50"
                          }
                        >
                          <td className="py-2 px-4 font-semibold text-gray-800">
                            {code}
                          </td>
                          <td className="py-2 px-4 text-gray-600">
                            {desc}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>,
    document.body
  );
};

export default DefModal;

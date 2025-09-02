import React, { useState, useRef, useEffect } from "react";
import {
  Calendar,
  Clock,
  X,
  Check,
  RotateCcw,
  ChevronDown,
  Filter,
  CalendarDays,
} from "lucide-react";

interface EnhancedDateTimeFilterProps {
  label?: string;
  filterMode: "datetime" | "date" | "time";
  onFilterApply: (filters: {
    requested_at_start: string;
    requested_at_end: string;
    approved_at_start: string;
    approved_at_end: string;
  }) => void;
  initialFilters?: {
    requested_at_start: string;
    requested_at_end: string;
    approved_at_start: string;
    approved_at_end: string;
  };
}

const EnhancedDateTimeFilter: React.FC<EnhancedDateTimeFilterProps> = ({
  filterMode: initialFilterMode = "datetime",
  onFilterApply,
  initialFilters = {
    requested_at_start: "",
    requested_at_end: "",
    approved_at_start: "",
    approved_at_end: "",
  },
}) => {
  const [filterMode, setFilterMode] = useState(initialFilterMode);
  const [isOpen, setIsOpen] = useState(false);
  const [tempFilters, setTempFilters] = useState({ ...initialFilters });
  const [appliedFilters, setAppliedFilters] = useState({ ...initialFilters });
  const [validationErrors, setValidationErrors] = useState<string[]>([]);

  const triggerRef = useRef<HTMLButtonElement>(null);
  const [dropdownStyle, setDropdownStyle] = useState<React.CSSProperties>({});
  const [hasCalculatedPosition, setHasCalculatedPosition] = useState(false);

  useEffect(() => {
    if (isOpen && triggerRef.current) {
      const rect = triggerRef.current.getBoundingClientRect();
      const scrollY = window.scrollY;
      const dropdownWidth = 420; // Slightly wider for better spacing
      const viewportWidth = window.innerWidth;

      let left = rect.right - dropdownWidth;

      // Adjust for mobile/small screens
      if (viewportWidth < 480) {
        left = Math.max(16, Math.min(left, viewportWidth - dropdownWidth - 16));
      }

      const top = rect.bottom + scrollY + 8;

      setDropdownStyle({
        top,
        left,
        width: Math.min(dropdownWidth, viewportWidth - 32),
        maxHeight: "calc(100vh - 100px)",
        overflowY: "auto",
      });

      setHasCalculatedPosition(true); // ✅ เพิ่มตรงนี้
    }
  }, [isOpen]);

  const validateDateRanges = () => {
    const errors: string[] = [];

    const parseDate = (val: string) => (val ? new Date(val).getTime() : null);

    const reqStart = parseDate(tempFilters.requested_at_start);
    const reqEnd = parseDate(tempFilters.requested_at_end);
    const appStart = parseDate(tempFilters.approved_at_start);
    const appEnd = parseDate(tempFilters.approved_at_end);

    if (reqStart && reqEnd && reqStart >= reqEnd) {
      errors.push("Requested start date must be before end date");
    }

    if (appStart && appEnd && appStart >= appEnd) {
      errors.push("Approved start date must be before end date");
    }

    setValidationErrors(errors);
    return errors.length === 0;
  };

  const handleApply = () => {
    if (!validateDateRanges()) return;

    const applied = { ...tempFilters };
    console.log("[Apply Filter]", applied); // ✅ log ที่นี่
    setAppliedFilters({ ...tempFilters });
    setIsOpen(false);
    onFilterApply({ ...tempFilters });
  };

  const handleReset = () => {
    const empty = {
      requested_at_start: "",
      requested_at_end: "",
      approved_at_start: "",
      approved_at_end: "",
    };

    console.log("[Reset Filter]", empty); // ✅ log ที่นี่
    setTempFilters(empty);
    setAppliedFilters(empty);
    setValidationErrors([]);
    setIsOpen(false);
    setHasCalculatedPosition(false); // ✅ เพิ่ม
    onFilterApply(empty);
  };

  const handleCancel = () => {
    setTempFilters({ ...appliedFilters });
    setValidationErrors([]);
    setIsOpen(false);
    setHasCalculatedPosition(false); // ✅ เพิ่ม
  };

  const getInputType = () => {
    switch (filterMode) {
      case "date":
        return "date";
      case "time":
        return "time";
      case "datetime":
        return "datetime-local";
      default:
        return "datetime-local";
    }
  };

  const getModeIcon = (mode: string) => {
    switch (mode) {
      case "date":
        return <Calendar className="w-3.5 h-3.5" />;
      case "time":
        return <Clock className="w-3.5 h-3.5" />;
      case "datetime":
        return <CalendarDays className="w-3.5 h-3.5" />;
      default:
        return <Calendar className="w-3.5 h-3.5" />;
    }
  };

  const hasActiveFilters = () =>
    Object.values(appliedFilters).some((v) => v !== "");

  const hasUnappliedChanges = () =>
    JSON.stringify(tempFilters) !== JSON.stringify(appliedFilters);

  const getActiveFilterCount = () =>
    Object.values(appliedFilters).filter((v) => v !== "").length;

  // Close on outside click
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        isOpen &&
        triggerRef.current &&
        !triggerRef.current.contains(event.target as Node)
      ) {
        const dropdown = document.querySelector(
          '[data-dropdown="datetime-filter"]'
        );
        if (dropdown && !dropdown.contains(event.target as Node)) {
          handleCancel();
        }
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      return () =>
        document.removeEventListener("mousedown", handleClickOutside);
    }
  }, [isOpen]);

  const dropdown = (
    <div
      data-dropdown="datetime-filter"
      className="fixed z-[9999] bg-white border border-gray-200 rounded-2xl shadow-2xl backdrop-blur-sm animate-in fade-in-0 zoom-in-95 duration-200"
      style={dropdownStyle}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-gradient-to-r from-gray-50 to-white rounded-t-2xl">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-blue-100 rounded-lg">
            <Filter className="w-4 h-4 text-blue-600" />
          </div>
          <div>
            <h3 className="font-semibold text-gray-900">Date & Time Filters</h3>
            <p className="text-xs text-gray-500 mt-0.5">
              Configure your filter criteria
            </p>
          </div>
        </div>
        <button
          onClick={() => setIsOpen(false)}
          className="p-2 hover:bg-gray-100 rounded-xl transition-colors duration-200"
        >
          <X className="w-4 h-4 text-gray-500" />
        </button>
      </div>

      {/* Filter Mode Selection */}
      <div className="px-6 py-5 border-b border-gray-100">
        <label className="block text-sm font-medium text-gray-700 mb-3">
          Filter Type
        </label>
        <div className="grid grid-cols-3 gap-2">
          {(["datetime", "date"] as const).map((mode) => (
            <button
              key={mode}
              onClick={() => setFilterMode(mode)}
              className={`flex items-center justify-center space-x-2 px-4 py-3 text-sm rounded-xl border transition-all duration-200 ${
                filterMode === mode
                  ? "bg-blue-50 text-blue-700 border-blue-200 shadow-sm"
                  : "bg-white text-gray-600 border-gray-200 hover:bg-gray-50 hover:border-gray-300"
              }`}
            >
              {getModeIcon(mode)}
              <span className="capitalize font-medium">{mode}</span>
            </button>
          ))}
        </div>
      </div>

      {/* Date Range Inputs */}
      <div className="px-6 py-6 space-y-8">
        {(["requested", "approved"] as const).map((type, index) => (
          <div key={type} className="space-y-4">
            <div className="flex items-center space-x-3">
              <div
                className={`w-3 h-3 rounded-full ${
                  type === "requested" ? "bg-blue-500" : "bg-emerald-500"
                }`}
              />
              <h4 className="font-semibold text-gray-900">
                {type.charAt(0).toUpperCase() + type.slice(1)} Date Range
              </h4>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="block text-sm font-medium text-gray-600">
                  Start {filterMode === "datetime" ? "Date & Time" : "Date"}
                </label>
                <div className="relative">
                  <input
                    type={getInputType()}
                    value={
                      tempFilters[
                        `${type}_at_start` as keyof typeof tempFilters
                      ]
                    }
                    onChange={(e) =>
                      setTempFilters((prev) => ({
                        ...prev,
                        [`${type}_at_start`]: e.target.value,
                      }))
                    }
                    className="w-full px-4 py-3 text-sm border border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all duration-200 bg-white"
                    placeholder="Select start..."
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="block text-sm font-medium text-gray-600">
                  End {filterMode === "datetime" ? "Date & Time" : "Date"}
                </label>
                <div className="relative">
                  <input
                    type={getInputType()}
                    value={
                      tempFilters[`${type}_at_end` as keyof typeof tempFilters]
                    }
                    onChange={(e) =>
                      setTempFilters((prev) => ({
                        ...prev,
                        [`${type}_at_end`]: e.target.value,
                      }))
                    }
                    className="w-full px-4 py-3 text-sm border border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all duration-200 bg-white"
                    placeholder="Select end..."
                  />
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Validation Errors */}
      {validationErrors.length > 0 && (
        <div className="px-6 py-4 border-t border-red-100 bg-red-50">
          <div className="space-y-2">
            {validationErrors.map((error, index) => (
              <p key={index} className="text-sm text-red-600 flex items-center">
                <div className="w-1.5 h-1.5 bg-red-500 rounded-full mr-2" />
                {error}
              </p>
            ))}
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100 bg-gray-50 rounded-b-2xl">
        <button
          onClick={handleReset}
          disabled={
            !hasActiveFilters() &&
            !Object.values(tempFilters).some((v) => v !== "")
          }
          className="flex items-center space-x-2 px-4 py-2 text-sm text-gray-600 hover:text-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-200"
        >
          <RotateCcw className="w-4 h-4" />
          <span>Reset</span>
        </button>

        <div className="flex space-x-3">
          <button
            onClick={handleCancel}
            className="px-5 py-2.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-xl hover:bg-gray-50 hover:border-gray-400 transition-all duration-200 focus:ring-2 focus:ring-gray-500 focus:ring-offset-2"
          >
            Cancel
          </button>
          <button
            onClick={handleApply}
            disabled={!hasUnappliedChanges() || validationErrors.length > 0}
            className="flex items-center space-x-2 px-5 py-2.5 text-sm font-medium text-white bg-blue-600 rounded-xl hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200 focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 shadow-sm"
          >
            <Check className="w-4 h-4" />
            <span>Apply Filters</span>
          </button>
        </div>
      </div>
    </div>
  );

  return (
    <>
      <div className="relative">
        <button
          ref={triggerRef}
          onClick={() => setIsOpen(!isOpen)}
          className={`inline-flex items-center px-4 py-2.5 text-sm font-medium rounded-xl border transition-all duration-200 focus:ring-2 focus:ring-offset-2 ${
            hasActiveFilters()
              ? "bg-blue-50 text-blue-700 border-blue-200 hover:bg-blue-100 focus:ring-blue-500 shadow-sm"
              : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50 hover:border-gray-400 focus:ring-gray-500"
          }`}
        >
          <Filter className="w-4 h-4 mr-2" />
          <span className="hidden sm:inline">Date & Time Filters</span>
          <span className="sm:hidden">Filters</span>

          {hasActiveFilters() && (
            <span className="ml-2 px-2 py-0.5 text-xs font-semibold bg-blue-100 text-blue-800 rounded-full">
              {getActiveFilterCount()}
            </span>
          )}

          <ChevronDown
            className={`w-4 h-4 ml-2 transition-transform duration-200 ${
              isOpen ? "rotate-180" : ""
            }`}
          />
        </button>

        {isOpen && hasCalculatedPosition && (
          <div className="fixed inset-0 z-[9998]">{dropdown}</div>
        )}
      </div>

      {/* Demo Toast Container for validation messages */}
      <div className="fixed top-4 right-4 z-[10000] space-y-2">
        {validationErrors.map((error, index) => (
          <div
            key={index}
            className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-xl shadow-lg animate-in slide-in-from-right-5 duration-300"
          >
            <p className="text-sm font-medium">{error}</p>
          </div>
        ))}
      </div>
    </>
  );
};

export default EnhancedDateTimeFilter;

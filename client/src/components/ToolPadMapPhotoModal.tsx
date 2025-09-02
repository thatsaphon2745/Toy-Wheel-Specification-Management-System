// ✅ ToolPadMapPhotoModal.tsx — Production-grade image manager with zoom functionality
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";

const MySwal = withReactContent(Swal);

interface Props {
  mapId: number;
  currentUrl?: string | null;
  currentName?: string | null;
  onClose: () => void;
  onSuccess: () => void;
}

interface Position {
  x: number;
  y: number;
}

const MySwalTop = withReactContent(
  Swal.mixin({
    customClass: {
      // ใช้ Tailwind arbitrary value ได้เลย
      container: "z-[20000]", // สูงกว่า 11000 ที่ fullscreen ใช้
    },
  })
);

const apiFor = (mapId: number) => ({
  uploadAfterHst: `/api/ToolPadMap/${mapId}/after-hst`,
  deleteAfterHst: `/api/ToolPadMap/${mapId}/after-hst`,
});

const bytesToPretty = (n: number): string => {
  if (n < 1024) return `${n} B`;
  if (n < 1024 ** 2) return `${(n / 1024).toFixed(1)} KB`;
  if (n < 1024 ** 3) return `${(n / 1024 ** 2).toFixed(1)} MB`;
  return `${(n / 1024 ** 3).toFixed(1)} GB`;
};

const allowedTypes = new Set(["image/jpeg", "image/png", "image/webp"]);
const maxBytes = 8 * 1024 * 1024; // 8 MB
const MIN_ZOOM = 0.5;
const MAX_ZOOM = 5;
const ZOOM_STEP = 0.2;

const ZoomableImage: React.FC<{
  src: string;
  alt: string;
  className?: string;
  onLoad?: () => void;
  onError?: () => void;
}> = ({ src, alt, className = "", onLoad, onError }) => {
  const [zoom, setZoom] = useState(1);
  const [position, setPosition] = useState<Position>({ x: 0, y: 0 });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState<Position>({ x: 0, y: 0 });
  const [isLoaded, setIsLoaded] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const imageRef = useRef<HTMLImageElement>(null);

  const resetZoom = useCallback(() => {
    setZoom(1);
    setPosition({ x: 0, y: 0 });
  }, []);

  const handleWheel = useCallback((e: React.WheelEvent) => {
    e.preventDefault();
    const delta = e.deltaY > 0 ? -ZOOM_STEP : ZOOM_STEP;
    setZoom((prev) => Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, prev + delta)));
  }, []);

  const handleMouseDown = useCallback(
    (e: React.MouseEvent) => {
      if (zoom <= 1) return;
      e.preventDefault();
      setIsDragging(true);
      setDragStart({
        x: e.clientX - position.x,
        y: e.clientY - position.y,
      });
    },
    [zoom, position]
  );

  const handleMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (!isDragging || zoom <= 1) return;
      e.preventDefault();
      setPosition({
        x: e.clientX - dragStart.x,
        y: e.clientY - dragStart.y,
      });
    },
    [isDragging, dragStart, zoom]
  );

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
  }, []);

  const handleImageLoad = useCallback(() => {
    setIsLoaded(true);
    onLoad?.();
  }, [onLoad]);

  const handleImageError = useCallback(() => {
    onError?.();
  }, [onError]);

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (!containerRef.current?.contains(document.activeElement)) return;

      switch (e.key) {
        case "+":
        case "=":
          e.preventDefault();
          setZoom((prev) => Math.min(MAX_ZOOM, prev + ZOOM_STEP));
          break;
        case "-":
          e.preventDefault();
          setZoom((prev) => Math.max(MIN_ZOOM, prev - ZOOM_STEP));
          break;
        case "0":
          e.preventDefault();
          resetZoom();
          break;
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [resetZoom]);

  return (
    <div
      ref={containerRef}
      className={`relative overflow-hidden bg-gray-900 ${className}`}
      tabIndex={0}
      role="img"
      aria-label={`${alt} - Use mouse wheel to zoom, drag to pan when zoomed`}
      onWheel={handleWheel}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
    >
      <img
        ref={imageRef}
        src={src}
        alt={alt}
        className={`transition-transform duration-200 ${
          isDragging
            ? "cursor-grabbing"
            : zoom > 1
            ? "cursor-grab"
            : "cursor-pointer"
        }`}
        style={{
          transform: `scale(${zoom}) translate(${position.x / zoom}px, ${
            position.y / zoom
          }px)`,
          transformOrigin: "center center",
        }}
        onLoad={handleImageLoad}
        onError={handleImageError}
        draggable={false}
      />

      {/* Zoom controls */}
      {isLoaded && (
        <div className="absolute top-2 right-2 flex flex-col gap-1 bg-black/70 rounded-lg p-1">
          <button
            onClick={() =>
              setZoom((prev) => Math.min(MAX_ZOOM, prev + ZOOM_STEP))
            }
            className="w-8 h-8 flex items-center justify-center text-white hover:bg-white/20 rounded transition-colors"
            title="Zoom in (+)"
            disabled={zoom >= MAX_ZOOM}
          >
            +
          </button>
          <button
            onClick={() =>
              setZoom((prev) => Math.max(MIN_ZOOM, prev - ZOOM_STEP))
            }
            className="w-8 h-8 flex items-center justify-center text-white hover:bg-white/20 rounded transition-colors"
            title="Zoom out (-)"
            disabled={zoom <= MIN_ZOOM}
          >
            −
          </button>
          <button
            onClick={resetZoom}
            className="w-8 h-8 flex items-center justify-center text-white hover:bg-white/20 rounded transition-colors text-xs"
            title="Reset zoom (0)"
          >
            1:1
          </button>
        </div>
      )}

      {/* Loading indicator */}
      {!isLoaded && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
        </div>
      )}
    </div>
  );
};

const ImagePreview: React.FC<{
  src: string;
  alt: string;
  size?: "small" | "large";
  onClick?: () => void;
}> = ({ src, alt, size = "small", onClick }) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [hasError, setHasError] = useState(false);

  const dimensions = size === "small" ? "w-24 h-24" : "w-full h-64";

  if (hasError) {
    return (
      <div
        className={`${dimensions} rounded-md bg-gray-100 border flex items-center justify-center`}
      >
        <div className="text-xs text-gray-400">Failed to load</div>
      </div>
    );
  }

  return (
    <div
      className={`${dimensions} rounded-md overflow-hidden border bg-gray-50 flex items-center justify-center relative ${
        onClick ? "cursor-pointer" : ""
      }`}
      onClick={onClick}
    >
      {!isLoaded && (
        <div className="absolute inset-0 flex items-center justify-center">
          <div className="animate-spin w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full"></div>
        </div>
      )}
      <img
        src={src}
        alt={alt}
        className={`w-full h-full object-cover transition-opacity ${
          isLoaded ? "opacity-100" : "opacity-0"
        }`}
        onLoad={() => setIsLoaded(true)}
        onError={() => setHasError(true)}
        loading="lazy"
      />
    </div>
  );
};

const ToolPadMapPhotoModal: React.FC<Props> = ({
  mapId,
  currentUrl,
  currentName,
  onClose,
  onSuccess,
}) => {
  const [file, setFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [isUploading, setUploading] = useState(false);
  const [isDragOver, setIsDragOver] = useState(false);
  const [showFullscreen, setShowFullscreen] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  // Cleanup preview URL on unmount
  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  // Handle escape key
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        if (showFullscreen) {
          setShowFullscreen(false);
        } else {
          onClose();
        }
      }
    };

    document.addEventListener("keydown", handleEscape);
    return () => document.removeEventListener("keydown", handleEscape);
  }, [onClose, showFullscreen]);

  const validateFile = useCallback((file: File): string | null => {
    if (!allowedTypes.has(file.type)) {
      return "Please upload JPG, PNG, WEBP, or GIF files only.";
    }
    if (file.size > maxBytes) {
      return `File size must be under ${bytesToPretty(
        maxBytes
      )}. Your file is ${bytesToPretty(file.size)}.`;
    }
    return null;
  }, []);

  const onPickFile = useCallback(
    (f: File) => {
      const error = validateFile(f);
      if (error) {
        MySwalTop.fire({
          icon: "warning",
          title: "Invalid File",
          text: error,
          confirmButtonColor: "#3b82f6",
        });
        return;
      }

      // Clean up previous preview
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }

      setFile(f);
      setPreviewUrl(URL.createObjectURL(f));
    },
    [previewUrl, validateFile]
  );

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragOver(false);

      const files = Array.from(e.dataTransfer.files);
      const imageFile = files.find((f) => allowedTypes.has(f.type));

      if (imageFile) {
        onPickFile(imageFile);
      } else if (files.length > 0) {
        MySwalTop.fire({
          icon: "warning",
          title: "Unsupported File",
          text: "Please drop an image file (JPG, PNG, WEBP, or GIF).",
          confirmButtonColor: "#3b82f6",
        });
      }
    },
    [onPickFile]
  );

  const doUpload = useCallback(async () => {
    if (!file) return;

    try {
      setUploading(true);
      setUploadProgress(0);

      abortControllerRef.current = new AbortController();

      const token = localStorage.getItem("token");
      const fd = new FormData();
      fd.append("file", file); // ชื่อ field ต้องเป็น 'file'

      const xhr = new XMLHttpRequest();

      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
          setUploadProgress((e.loaded / e.total) * 100);
        }
      };

      const uploadPromise = new Promise<void>((resolve, reject) => {
        xhr.onreadystatechange = () => {
          if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status >= 200 && xhr.status < 300) {
              resolve();
            } else {
              reject(
                new Error(xhr.responseText || `Upload failed (${xhr.status})`)
              );
            }
          }
        };
        xhr.onerror = () => reject(new Error("Network error occurred"));
        xhr.onabort = () => reject(new Error("Upload cancelled"));

        // ⬇️ เปลี่ยนเป็น POST ให้ตรงกับ API
        xhr.open("POST", apiFor(mapId).uploadAfterHst);

        if (token) xhr.setRequestHeader("Authorization", `Bearer ${token}`);
        xhr.send(fd); // อย่าตั้ง Content-Type เอง ให้บราวเซอร์ตั้ง boundary ให้
      });

      abortControllerRef.current.signal.addEventListener("abort", () =>
        xhr.abort()
      );

      await uploadPromise;

      await MySwalTop.fire({
        icon: "success",
        title: "Upload Successful",
        text: "The image has been saved successfully.",
        confirmButtonText: "Great!",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white font-semibold px-4 py-2 rounded-lg shadow-md focus:outline-none focus:ring-2 focus:ring-green-500",
          popup: "rounded-xl shadow-2xl border border-gray-200",
          container: "z-[20000]", // สูงกว่า 11000 ที่ fullscreen ใช้
        },
      });

      onSuccess();
      onClose();
    } catch (err: any) {
      if (err.message !== "Upload cancelled") {
        await MySwalTop.fire({
          icon: "error",
          title: "Upload Failed",
          text: err?.message || "An unexpected error occurred during upload.",
          confirmButtonColor: "#ef4444",
        });
      }
    } finally {
      setUploading(false);
      setUploadProgress(0);
      abortControllerRef.current = null;
    }
  }, [file, mapId, onSuccess, onClose]);

  const cancelUpload = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
  }, []);

  const doDelete = useCallback(async () => {
    const result = await MySwalTop.fire({
      icon: "warning",
      title: "Delete Photo",
      text: "Are you sure you want to permanently delete this image? This action cannot be undone.",
      showCancelButton: true,
      confirmButtonText: "Delete",
      cancelButtonText: "Cancel",
      confirmButtonColor: "#ef4444",
      cancelButtonColor: "#6b7280",
      customClass: {
        confirmButton:
          "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg font-semibold",
        cancelButton:
          "bg-gray-300 hover:bg-gray-400 text-gray-800 px-4 py-2 rounded-lg font-medium",
        popup: "rounded-xl shadow-2xl border border-gray-200",
        container: "z-[20000]", // สูงกว่า 11000 ที่ fullscreen ใช้
      },
    });

    if (!result.isConfirmed) return;

    try {
      setUploading(true);
      const token = localStorage.getItem("token");

      const res = await fetch(apiFor(mapId).deleteAfterHst, {
        method: "DELETE",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });

      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg || `Delete failed (${res.status})`);
      }

      await MySwalTop.fire({
        icon: "success",
        title: "Deleted",
        text: "The image has been successfully removed.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white font-semibold px-4 py-2 rounded-lg shadow-md focus:outline-none focus:ring-2 focus:ring-green-500",
          popup: "rounded-xl shadow-2xl border border-gray-200",
          container: "z-[20000]", // สูงกว่า 11000 ที่ fullscreen ใช้
        },
      });

      onSuccess();
      onClose();
    } catch (err: any) {
      await MySwalTop.fire({
        icon: "error",
        title: "Delete Failed",
        text: err?.message || "Failed to delete the image. Please try again.",
        confirmButtonColor: "#ef4444",
      });
    } finally {
      setUploading(false);
    }
  }, [mapId, onSuccess, onClose]);

  const clearFile = useCallback(() => {
    setFile(null);
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }, [previewUrl]);

  const dropZoneClasses = useMemo(
    () =>
      `border-2 border-dashed rounded-xl p-6 text-center transition-all duration-200 ${
        isDragOver
          ? "border-blue-500 bg-blue-50"
          : "border-gray-300 bg-gray-50 hover:bg-gray-100"
      }`,
    [isDragOver]
  );

  return (
    <>
      {/* Main Modal */}
      <div
        className="fixed inset-0 z-[10000] flex items-center justify-center bg-black/50 backdrop-blur-sm"
        onClick={(e) => {
          if (e.target === e.currentTarget) onClose();
        }}
      >
        <div className="w-full max-w-2xl mx-4 rounded-2xl bg-white shadow-2xl border border-gray-200 max-h-[90vh] overflow-hidden">
          {/* Header */}
          <div className="px-6 py-4 border-b bg-gray-50/50 flex items-center justify-between">
            <h3 className="text-xl font-semibold text-gray-900">
              Photo Manager
            </h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 p-1 rounded-full hover:bg-gray-100 transition-colors"
              aria-label="Close modal"
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          {/* Body */}
          <div className="p-6 space-y-6 max-h-[calc(90vh-8rem)] overflow-y-auto">
            {/* Current Image */}
            <div>
              <div className="text-sm font-medium text-gray-700 mb-3">
                Current Image
              </div>
              <div className="flex items-center gap-4">
                {currentUrl ? (
                  <ImagePreview
                    src={currentUrl}
                    alt={currentName ?? "Current image"}
                    onClick={() => setShowFullscreen(true)}
                  />
                ) : (
                  <div className="w-24 h-24 rounded-md border-2 border-dashed border-gray-200 bg-gray-50 flex items-center justify-center">
                    <div className="text-xs text-gray-400">No image</div>
                  </div>
                )}

                <div className="flex-1 min-w-0">
                  <div className="text-sm text-gray-600 truncate">
                    {currentName || "No image uploaded"}
                  </div>
                  {currentUrl && (
                    <div className="flex gap-2 mt-2">
                      <button
                        onClick={() => setShowFullscreen(true)}
                        className="text-blue-600 text-xs underline hover:text-blue-700"
                      >
                        View full size
                      </button>
                      <a
                        href={currentUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 text-xs underline hover:text-blue-700"
                      >
                        Open in new tab
                      </a>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Upload New */}
            <div>
              <div className="text-sm font-medium text-gray-700 mb-3">
                Upload New Image
              </div>
              <div
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                className={dropZoneClasses}
              >
                <input
                  ref={fileInputRef}
                  id="photo-upload"
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={(e) => {
                    const f = e.target.files?.[0];
                    if (f) onPickFile(f);
                  }}
                />

                {!previewUrl ? (
                  <label
                    htmlFor="photo-upload"
                    className="cursor-pointer block"
                  >
                    <div className="flex flex-col items-center gap-3">
                      <svg
                        className="w-12 h-12 text-gray-400"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                        />
                      </svg>
                      <div className="text-center">
                        <div className="text-base font-medium text-gray-700">
                          {isDragOver
                            ? "Drop your image here"
                            : "Click to upload or drag & drop"}
                        </div>
                        <div className="text-sm text-gray-500 mt-1">
                          JPG, PNG, WEBP • up to {bytesToPretty(maxBytes)}
                        </div>
                      </div>
                    </div>
                  </label>
                ) : (
                  <div className="flex flex-col items-center gap-4">
                    <ImagePreview
                      src={previewUrl}
                      alt="Preview"
                      onClick={() => setShowFullscreen(true)}
                    />
                    <div className="text-center">
                      <div className="text-sm font-medium text-gray-700">
                        {file?.name}
                      </div>
                      <div className="text-xs text-gray-500">
                        {file && bytesToPretty(file.size)}
                      </div>
                    </div>
                    <button
                      onClick={clearFile}
                      className="text-sm px-4 py-2 border border-gray-300 rounded-lg bg-white hover:bg-gray-50 transition-colors"
                    >
                      Remove
                    </button>
                  </div>
                )}
              </div>
            </div>

            {/* Upload Progress */}
            {isUploading && uploadProgress > 0 && (
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Uploading...</span>
                  <span>{Math.round(uploadProgress)}%</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                    style={{ width: `${uploadProgress}%` }}
                  ></div>
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-4 border-t bg-gray-50/50 flex items-center justify-between">
            <button
              onClick={doDelete}
              disabled={!currentUrl || isUploading}
              className={`px-4 py-2 text-sm rounded-lg font-medium transition-colors ${
                currentUrl && !isUploading
                  ? "border border-red-300 text-red-700 hover:bg-red-50"
                  : "border border-gray-200 text-gray-400 cursor-not-allowed"
              }`}
            >
              Delete Current
            </button>

            <div className="flex items-center gap-3">
              {isUploading && (
                <button
                  onClick={cancelUpload}
                  className="px-4 py-2 text-sm rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </button>
              )}
              <button
                onClick={onClose}
                disabled={isUploading}
                className="px-4 py-2 text-sm rounded-lg border border-gray-300 bg-white hover:bg-gray-50 disabled:opacity-50 transition-colors"
              >
                Close
              </button>
              <button
                onClick={doUpload}
                disabled={!file || isUploading}
                className={`px-6 py-2 text-sm rounded-lg font-medium transition-colors ${
                  file && !isUploading
                    ? "bg-blue-600 text-white hover:bg-blue-700"
                    : "bg-gray-300 text-gray-500 cursor-not-allowed"
                }`}
              >
                {isUploading ? "Saving..." : "Save Image"}
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Fullscreen Image Viewer */}
      {showFullscreen && (currentUrl || previewUrl) && (
        <div
          className="fixed inset-0 z-[11000] bg-black/95 flex items-center justify-center"
          onClick={(e) => {
            if (e.target === e.currentTarget) setShowFullscreen(false);
          }}
        >
          <button
            onClick={() => setShowFullscreen(false)}
            className="absolute top-4 right-4 z-10 text-white/80 hover:text-white p-2 rounded-full hover:bg-white/20 transition-colors"
            aria-label="Close fullscreen view"
          >
            <svg
              className="w-8 h-8"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>

          <div className="w-full h-full max-w-7xl max-h-full p-4">
            <ZoomableImage
              src={(previewUrl || currentUrl)!}
              alt={file?.name || currentName || "Image"}
              className="w-full h-full rounded-lg"
            />
          </div>

          <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2 bg-black/70 text-white text-sm px-4 py-2 rounded-full">
            Use mouse wheel to zoom • Drag to pan • Press ESC to close
          </div>
        </div>
      )}
    </>
  );
};

export default ToolPadMapPhotoModal;

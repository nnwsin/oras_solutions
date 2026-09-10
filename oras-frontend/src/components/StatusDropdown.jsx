import React, { useState, useRef, useEffect } from "react";
import { createPortal } from "react-dom";

const STATUS_OPTIONS = [
    { value: 1, label: "Pending", className: "status-opt-pending" },
    { value: 2, label: "In Progress", className: "status-opt-progress" },
    { value: 3, label: "Completed", className: "status-opt-completed" },
    { value: 4, label: "Cancelled", className: "status-opt-cancelled" },
];

export default function StatusDropdown({ value, onChange, disabled = false, className = "" }) {
    const [isOpen, setIsOpen] = useState(false);
    const buttonRef = useRef(null);
    const menuRef = useRef(null);
    const [coords, setCoords] = useState({ top: 0, left: 0, minWidth: 120 });

    const numVal = typeof value === "string"
        ? (value === "Pending" ? 1 : value === "InProgress" ? 2 : value === "Completed" ? 3 : value === "Cancelled" ? 4 : Number(value) || 1)
        : Number(value) || 1;

    const currentOption = STATUS_OPTIONS.find((o) => o.value === numVal) || STATUS_OPTIONS[0];

    const getBadgeClass = (val) => {
        switch (val) {
            case 1: return "badge-pending";
            case 2: return "badge-progress";
            case 3: return "badge-completed";
            case 4: return "badge-cancelled";
            default: return "";
        }
    };

    const updateCoords = () => {
        if (buttonRef.current) {
            const rect = buttonRef.current.getBoundingClientRect();
            setCoords({
                top: rect.bottom + 4,
                left: rect.left,
                minWidth: Math.max(rect.width, 130),
            });
        }
    };

    const handleToggle = (e) => {
        e.stopPropagation();
        if (disabled) return;
        if (!isOpen) {
            updateCoords();
        }
        setIsOpen((prev) => !prev);
    };

    useEffect(() => {
        if (!isOpen) return;

        const handleClickOutside = (e) => {
            if (
                menuRef.current && !menuRef.current.contains(e.target) &&
                buttonRef.current && !buttonRef.current.contains(e.target)
            ) {
                setIsOpen(false);
            }
        };

        const handleKeyDown = (e) => {
            if (e.key === "Escape") {
                setIsOpen(false);
            }
        };

        const handleScrollOrResize = () => {
            setIsOpen(false);
        };

        document.addEventListener("mousedown", handleClickOutside);
        document.addEventListener("keydown", handleKeyDown);
        window.addEventListener("scroll", handleScrollOrResize, true);
        window.addEventListener("resize", handleScrollOrResize);

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
            document.removeEventListener("keydown", handleKeyDown);
            window.removeEventListener("scroll", handleScrollOrResize, true);
            window.removeEventListener("resize", handleScrollOrResize);
        };
    }, [isOpen]);

    const menuContent = isOpen ? (
        <div
            ref={menuRef}
            className="status-dropdown-menu"
            style={{
                position: "fixed",
                top: `${coords.top}px`,
                left: `${coords.left}px`,
                minWidth: `${coords.minWidth}px`,
            }}
            onClick={(e) => e.stopPropagation()}
        >
            {STATUS_OPTIONS.map((opt) => (
                <button
                    key={opt.value}
                    type="button"
                    className={`status-dropdown-item ${opt.className} ${opt.value === numVal ? "selected" : ""}`}
                    onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        onChange(opt.value);
                        setIsOpen(false);
                    }}
                >
                    {opt.label}
                </button>
            ))}
        </div>
    ) : null;

    return (
        <div className={`status-dropdown-wrapper ${className}`}>
            <button
                ref={buttonRef}
                type="button"
                className={`status-select ${getBadgeClass(numVal)}`}
                onClick={handleToggle}
                disabled={disabled}
                aria-expanded={isOpen}
            >
                <span>{currentOption.label}</span>
                <span className="dropdown-chevron">▾</span>
            </button>
            {typeof document !== "undefined" && menuContent && createPortal(menuContent, document.body)}
        </div>
    );
}

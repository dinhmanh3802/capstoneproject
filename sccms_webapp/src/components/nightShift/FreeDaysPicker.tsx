import React, { useState, useEffect } from "react"
import { DayPicker } from "react-day-picker"
import { vi } from "date-fns/locale"
import "react-day-picker/dist/style.css"
import "./FreeDaysPicker.css"
import { formatInTimeZone } from "date-fns-tz" // Import thêm

interface FreeDaysPickerProps {
    startDate: Date
    endDate: Date
    preselectedDates: Date[]
    onSubmit: (selectedDates: string[]) => void
    timeZone: string // Thêm dòng này
}

const FreeDaysPicker: React.FC<FreeDaysPickerProps> = ({
    startDate,
    endDate,
    preselectedDates,
    onSubmit,
    timeZone,
}) => {
    const [selectedDays, setSelectedDays] = useState<Date[]>(preselectedDates)
    const [isEditing, setIsEditing] = useState(preselectedDates.length === 0) // Edit mode nếu không có preselectedDates
    const [hasSubmitted, setHasSubmitted] = useState(preselectedDates.length > 0)

    useEffect(() => {
        setSelectedDays(preselectedDates)
        setHasSubmitted(preselectedDates.length > 0)
        setIsEditing(preselectedDates.length === 0)
    }, [preselectedDates])

    const handleSelect = (days: Date[] | undefined) => {
        if (!isEditing) return // Ngăn không cho thay đổi nếu không ở chế độ edit
        if (days) {
            setSelectedDays(days)
        } else {
            setSelectedDays([])
        }
    }

    const handleSubmit = () => {
        // Sử dụng formatInTimeZone để định dạng ngày đúng múi giờ
        const dates = selectedDays?.map((day) => formatInTimeZone(day, timeZone, "yyyy-MM-dd"))
        onSubmit(dates)
        setHasSubmitted(true)
        setIsEditing(false)
    }

    const handleEdit = () => {
        setIsEditing(true)
    }

    const handleCancel = () => {
        setSelectedDays(preselectedDates)
        setIsEditing(false)
    }

    return (
        <div className={`free-days-picker ${!isEditing && hasSubmitted ? "disabled" : ""}`}>
            <table className="picker-table">
                <tbody>
                    <tr>
                        <td>
                            <div className="calendar-and-table">
                                <DayPicker
                                    mode="multiple"
                                    selected={selectedDays}
                                    onSelect={handleSelect}
                                    fromDate={startDate}
                                    toDate={endDate}
                                    required
                                    locale={vi}
                                    disabled={(day) => day < startDate || day > endDate}
                                    classNames={{
                                        day_selected: "selected-day",
                                        day_disabled: "disabled-day",
                                    }}
                                />
                            </div>
                        </td>
                    </tr>
                    <tr className="text-center">
                        <td>
                            {hasSubmitted ? (
                                isEditing ? (
                                    <>
                                        <button onClick={handleSubmit} className="btn btn-success me-2">
                                            Cập nhật
                                        </button>
                                        <button onClick={handleCancel} className="btn btn-secondary">
                                            Hủy
                                        </button>
                                    </>
                                ) : (
                                    <button onClick={handleEdit} className="btn btn-primary">
                                        Sửa
                                    </button>
                                )
                            ) : (
                                <button onClick={handleSubmit} className="btn btn-primary submit-button me-3">
                                    Gửi
                                </button>
                            )}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    )
}

export default FreeDaysPicker

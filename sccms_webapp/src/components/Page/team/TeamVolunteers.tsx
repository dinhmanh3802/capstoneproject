import React, { useEffect, useState } from "react"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import { toastNotify } from "../../../helper"
import ConfirmationPopup from "../../commonCp/ConfirmationPopup"
import VolunteerListByTeam from "./VolunteerListByTeam"
import VolunteerSearchByTeam from "./VolunteerSearchByTeam"
import AddVolunteerPopup from "./AddVolunteerPopup"
import { useAddVolunteersToTeamMutation, useRemoveVolunteersFromTeamMutation } from "../../../apis/teamApi"
import { useExportVolunteersByTeamIdMutation } from "../../../apis/volunteerApi"

const TeamVolunteers = ({ teamId, team }) => {
    const [searchParams, setSearchParams] = useState({})
    const [isPopupOpen, setIsPopupOpen] = useState(false)
    const [selectedRows, setSelectedRows] = useState<number[]>([])
    const [isConfirmationOpen, setIsConfirmationOpen] = useState(false)

    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)

    const [addVolunteersToTeam] = useAddVolunteersToTeamMutation()
    const [removeVolunteersFromTeam] = useRemoveVolunteersFromTeamMutation()
    const [exportVolunteersByTeam] = useExportVolunteersByTeamIdMutation()

    const handleSearch = (params) => {
        setSearchParams(params)
    }

    const handleAddVolunteers = async (volunteerIds: number[]) => {
        try {
            await addVolunteersToTeam({ volunteerIds, teamId }).unwrap()
            toastNotify("Thêm tình nguyện viên vào ban thành công!", "success")
            setIsPopupOpen(false) // Close the popup after adding volunteers
            return true
        } catch (error) {
            toastNotify(error.data.errorMessages.join(", "), "error")
            return false
        }
    }

    const handleDeleteVolunteers = async () => {
        if (selectedRows.length === 0) {
            toastNotify("Vui lòng chọn tình nguyện viên để xóa", "error")
            return
        }

        try {
            await removeVolunteersFromTeam({ volunteerIds: selectedRows, teamId }).unwrap()
            toastNotify("Xóa tình nguyện viên thành công!", "success")
            setSelectedRows([])
        } catch (error) {
            toastNotify("Không thể xóa tình nguyện viên.", "error")
        } finally {
            setIsConfirmationOpen(false)
        }
    }

    const handleSelectRows = (selected: any) => {
        setSelectedRows(selected)
    }

    const handleExportExcel = async () => {
        try {
            const response = await exportVolunteersByTeam(currentCourse?.id).unwrap()
            const blob = await response.blob()
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", `Danh sách tình nguyện viên ban ${team.teamName}.xlsx`)
            document.body.appendChild(link)
            link.click()
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
            toastNotify("Xuất file Excel thành công!", "success")
        } catch (error) {
            toastNotify("Không thể xuất file Excel.", "error")
        }
    }

    return (
        <div className="team-volunteers">
            <h5 className="mt-4">Danh sách tình nguyện viên</h5>
            <VolunteerSearchByTeam onSearch={handleSearch} />
            <div className="container text-end mt-4 mb-2">
                {(currentUserRole === "manager" || currentUserRole === "secretary") &&
                    currentCourse?.id == team.courseId && (
                        <>
                            <button className="btn btn-primary btn-sm ms-2" onClick={() => setIsPopupOpen(true)}>
                                <i className="bi bi-person-plus-fill me-1"></i> Thêm tình nguyện viên
                            </button>
                        </>
                    )}
                <button className="btn btn-primary btn-sm ms-2" onClick={handleExportExcel}>
                    <i className="bi bi-file-earmark-spreadsheet me-1"></i> Tải Excel
                </button>
                {(currentUserRole === "manager" || currentUserRole === "secretary") &&
                    currentCourse?.id == team.courseId && (
                        <>
                            <button
                                className="btn btn-danger btn-sm ms-2"
                                onClick={() => {
                                    if (selectedRows.length > 0) setIsConfirmationOpen(true)
                                    else {
                                        toastNotify("Vui lòng chọn tình nguyện viên để xóa", "error")
                                    }
                                }}
                            >
                                <i className="bi bi-trash-fill me-1"></i> Xoá
                            </button>
                        </>
                    )}
            </div>
            <VolunteerListByTeam teamId={teamId} onSelectRows={handleSelectRows} />
            <ConfirmationPopup
                isOpen={isConfirmationOpen}
                onClose={() => setIsConfirmationOpen(false)}
                onConfirm={handleDeleteVolunteers}
                message="Bạn có chắc chắn muốn xóa các tình nguyện viên đã chọn không?"
                title="Xác nhận xóa hàng loạt"
            />
            <AddVolunteerPopup
                show={isPopupOpen}
                onHide={() => setIsPopupOpen(false)}
                onAddVolunteers={handleAddVolunteers}
                team={team}
            />
        </div>
    )
}

export default TeamVolunteers

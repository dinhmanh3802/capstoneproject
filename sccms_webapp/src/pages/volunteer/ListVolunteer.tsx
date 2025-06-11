import React, { useEffect, useState } from "react"
import { useNavigate, useLocation } from "react-router-dom"
import { MainLoader, SendEmailPopup } from "../../components/Page"
import {
    useGetVolunteerApplicationsQuery,
    usePrintVolunteerCardsMutation,
    usePrintVolunteerCertificateMutation,
    useSendVolunteerApplicationResultMutation,
} from "../../apis/volunteerApplicationApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useGetUsersQuery } from "../../apis/userApi"
import { SD_Role, SD_Role_Name } from "../../utility/SD"
import { toastNotify } from "../../helper"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { apiResponse } from "../../interfaces"
import { useGetAllEmailTemplateQuery } from "../../apis/emailTemplateApi"
import emailTemplateModel from "../../interfaces/emailTemplateModel"
import VolunteerSearch from "../../components/Page/volunteer/VolunteerSearch"
import { useExportVolunteersByCourseIdMutation } from "../../apis/volunteerApi"
import VolunteerList from "../../components/Page/volunteer/VolunteerList"
import { useGetTeamsByCourseIdQuery } from "../../apis/teamApi"

function ListVolunteer() {
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? "")
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)
    const [printVolunteerCards] = usePrintVolunteerCardsMutation()
    const [printVolunteerCertificate] = usePrintVolunteerCertificateMutation()
    const [exportVolunteersByCourse] = useExportVolunteersByCourseIdMutation()
    const [sendEmail] = useSendVolunteerApplicationResultMutation()
    const { data: listSecretary, isLoading: reviewerLoading } = useGetUsersQuery({ roleId: SD_Role.SECRETARY })
    const { data: listEmailTemplate, isLoading: emailTemplateLoading } = useGetAllEmailTemplateQuery({})
    const isLoading = !listCourseFromStore || !currentCourse || reviewerLoading || !currentUserRole || !currentUserId
    const [isPrintCard, setIsPrintCard] = useState(false)
    const [isPrintCertificate, setIsPrintCertificate] = useState(false)
    const [isExportExcel, setIsExportExcel] = useState(false)

    const navigate = useNavigate()
    const location = useLocation()

    const [isModalOpenSendResult, setIsModalOpenSendResult] = useState(false)
    const [clearRowsFlag, setClearRowsFlag] = useState(false)
    const [selectedRows, setSelectedRows] = useState<number[]>([])
    const listTemplate: emailTemplateModel[] = listEmailTemplate?.result || []

    const searchParamsFromUrl = new URLSearchParams(location.search)
    const initialSearchParams = {
        courseId: searchParamsFromUrl.get("courseId") || currentCourse?.id,
        volunteerCode: searchParamsFromUrl.get("volunteerCode") || "",
        name: searchParamsFromUrl.get("name") || "",
        status: searchParamsFromUrl.get("status") || "",
        teamId: searchParamsFromUrl.get("teamId") || "",
        dateOfBirthFrom: searchParamsFromUrl.get("dateOfBirthFrom") || "",
        dateOfBirthTo: searchParamsFromUrl.get("dateOfBirthTo") || "",
        gender: searchParamsFromUrl.get("gender") || "",
        phoneNumber: searchParamsFromUrl.get("phoneNumber") || "",
    }

    const [searchParams, setSearchParams] = useState(initialSearchParams)
    useEffect(() => {
        setSearchParams((prevParams) => ({
            ...prevParams,
            courseId: currentCourse?.id ?? 0,
        }))
    }, [currentCourse, listSecretary, currentUserRole])

    useEffect(() => {
        const newSearchParams = {
            courseId: searchParamsFromUrl.get("courseId") || currentCourse?.id,
            volunteerCode: searchParamsFromUrl.get("volunteerCode") || "",
            name: searchParamsFromUrl.get("name") || "",
            status: searchParamsFromUrl.get("status") || "",
            teamId: searchParamsFromUrl.get("teamId") || "",
            dateOfBirthFrom: searchParamsFromUrl.get("dateOfBirthFrom") || "",
            dateOfBirthTo: searchParamsFromUrl.get("dateOfBirthTo") || "",
            gender: searchParamsFromUrl.get("gender") || "",
            phoneNumber: searchParamsFromUrl.get("phoneNumber") || "",
        }

        setSearchParams(newSearchParams)
    }, [location.search, currentCourse?.id])

    const { data: applicationData, isLoading: applicationLoading } = useGetVolunteerApplicationsQuery(searchParams)
    const { data: teamData, isLoading: teamLoading } = useGetTeamsByCourseIdQuery(
        Number(searchParamsFromUrl.get("courseId")) || Number(currentCourse?.id),
    )
    const handleSearch = (params: any) => {
        setSearchParams(params)
        const queryParams = new URLSearchParams(params).toString()
        navigate(`/volunteers?${queryParams}`)
    }

    const handlePrintCards = async () => {
        setIsPrintCard(true)
        if (selectedRows.length === 0) {
            toastNotify("Vui lòng chọn tình nguyện viên để in thẻ", "error")
            return
        }

        try {
            const response = await printVolunteerCards({
                volunteerIds: selectedRows,
                courseId: Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
            }).unwrap()
            const contentDisposition = response.headers.get("Content-Disposition")
            let fileName = "Thẻ tình nguyện viên.pdf"

            if (contentDisposition) {
                const fileNameMatch = contentDisposition.match(/filename\*=UTF-8''(.+)/)
                if (fileNameMatch && fileNameMatch[1]) {
                    fileName = decodeURIComponent(fileNameMatch[1])
                } else {
                    const fallbackMatch = contentDisposition.match(/filename="(.+)"/)
                    if (fallbackMatch && fallbackMatch[1]) {
                        fileName = fallbackMatch[1]
                    }
                }
            }

            const blob = await response.blob()
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", fileName)
            document.body.appendChild(link)
            link.click()
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
            clearSelectedRows()
            toastNotify("Tải ảnh thẻ thành công!", "success")
            setSelectedRows([])
        } catch (error) {
            toastNotify("Không thể in thẻ tình nguyện viên.", "error")
        }
        setIsPrintCard(false)
    }

    const handlePrintCertificate = async () => {
        setIsPrintCertificate(true)
        if (selectedRows.length === 0) {
            toastNotify("Vui lòng chọn tình nguyện viên để in giấy chứng nhận", "error")
            return
        }

        try {
            const response = await printVolunteerCertificate({
                volunteerIds: selectedRows,
                courseId: Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
            }).unwrap()
            const contentDisposition = response.headers.get("Content-Disposition")
            let fileName = "Giấy chứng nhận.pdf"

            if (contentDisposition) {
                const fileNameMatch = contentDisposition.match(/filename\*=UTF-8''(.+)/)
                if (fileNameMatch && fileNameMatch[1]) {
                    fileName = decodeURIComponent(fileNameMatch[1])
                } else {
                    const fallbackMatch = contentDisposition.match(/filename="(.+)"/)
                    if (fallbackMatch && fallbackMatch[1]) {
                        fileName = fallbackMatch[1]
                    }
                }
            }

            const blob = await response.blob()
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", fileName)
            document.body.appendChild(link)
            link.click()
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
            clearSelectedRows()
            setSelectedRows([])
        } catch (error) {
            toastNotify("Không thể in thẻ tình nguyện viên.", "error")
        }
        setIsPrintCertificate(false)
    }

    const handleExportExcel = async () => {
        setIsExportExcel(true)
        try {
            const response = await exportVolunteersByCourse(
                Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
            ).unwrap()
            const contentDisposition = response.headers.get("Content-Disposition")
            let fileName = "download.xlsx"

            if (contentDisposition) {
                let fileNameMatch = contentDisposition.match(/filename\*=UTF-8''(.+)/)
                if (fileNameMatch && fileNameMatch[1]) {
                    fileName = decodeURIComponent(fileNameMatch[1])
                } else {
                    fileNameMatch = contentDisposition.match(/filename="(.+)"/)
                    if (fileNameMatch && fileNameMatch[1]) {
                        fileName = fileNameMatch[1]
                    }
                }
            }

            const blob = await response.blob()
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", fileName)
            document.body.appendChild(link)
            link.click()
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
            toastNotify("Tải file thành công!", "success")
        } catch (error) {
            console.error("Lỗi khi tải file:", error)
            toastNotify("Không thể tải file.", "error")
        }
        setIsExportExcel(false)
    }

    const handleSendResult = () => {
        if (selectedRows.length === 0) {
            toastNotify("Chọn tình nguyện viên để gửi email", "error")
            return
        }
        setIsModalOpenSendResult(true)
    }

    const handleSendResultConfirm = async (title: string, content: string) => {
        try {
            const response: apiResponse = await sendEmail({
                ListVolunteerApplicationId: selectedRows,
                CourseId: Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
                Subject: title,
                Message: content,
            })

            if (response.data?.isSuccess) {
                toastNotify("Email đã được gửi thành công!", "success")
            } else {
                toastNotify(response?.error?.data?.errorMessages?.join(", ") || "Gửi email thất bại", "error")
            }
        } catch (error) {
            toastNotify("Đã xảy ra lỗi khi gửi email", "error")
        }
        clearSelectedRows()
        setIsModalOpenSendResult(false)
    }

    const handleSelectRows = (selected: any) => {
        const selectedIds = selected.selectedRows?.map((row: any) => row.id)
        setSelectedRows(selectedIds)
    }

    const clearSelectedRows = () => {
        setClearRowsFlag((prev) => !prev)
    }

    if (isLoading || applicationLoading || emailTemplateLoading || teamLoading) {
        return <MainLoader />
    }
    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách tình nguyện viên</h3>
            </div>
            <VolunteerSearch
                onSearch={handleSearch}
                courseList={listCourseFromStore}
                currentCourse={currentCourse}
                teamList={teamData?.result}
            />
            <div className="container text-end mt-4">
                {(currentUserRole == SD_Role_Name.SECRETARY || currentUserRole == SD_Role_Name.MANAGER) && (
                    <div>
                        <button
                            className="btn btn-primary btn-sm ms-2"
                            disabled={isPrintCertificate}
                            onClick={() => handlePrintCertificate()}
                        >
                            <i className="bi bi-person-vcard me-1"></i>
                            {isPrintCertificate ? "Đang tải" : "In chứng nhận"}
                        </button>
                        <button
                            className="btn btn-primary btn-sm ms-2"
                            disabled={isPrintCard}
                            onClick={() => handlePrintCards()}
                        >
                            <i className="bi bi-person-vcard me-1"></i>
                            {isPrintCard ? "Đang tải" : "In thẻ"}
                        </button>
                        <button className="btn btn-primary btn-sm ms-2" onClick={handleSendResult}>
                            <i className="bi bi-envelope me-1"></i>
                            Gửi email
                        </button>
                        <button
                            className="btn btn-primary btn-sm ms-2"
                            disabled={isExportExcel}
                            onClick={() => handleExportExcel()}
                        >
                            <i className="bi bi-arrow-bar-down me-1"></i>
                            {isExportExcel ? "Đang tải" : "Tải Excel"}
                        </button>
                    </div>
                )}
                {currentUserRole != SD_Role_Name.SECRETARY && currentUserRole != SD_Role_Name.MANAGER && (
                    <div>
                        <button className="btn btn-primary btn-sm ms-2" onClick={() => handleExportExcel()}>
                            <i className="bi bi-arrow-bar-down me-1"></i>
                            Tải Excel
                        </button>
                    </div>
                )}
            </div>
            <div className="mt-2">
                <VolunteerList
                    applications={applicationData?.result.filter((app: any) => app.status !== 2 && app.status !== 0)}
                    teamList={teamData?.result}
                    onSelectUser={handleSelectRows}
                    clearSelectedRows={clearRowsFlag}
                    currentCourse={currentCourse}
                    searchParams={searchParams}
                />
            </div>
            <SendEmailPopup
                isOpen={isModalOpenSendResult}
                onClose={() => setIsModalOpenSendResult(false)}
                onConfirm={handleSendResultConfirm}
                listTemplate={listTemplate}
                select={3}
                onClearSelectedRows={clearSelectedRows}
            />
        </div>
    )
}

export default ListVolunteer

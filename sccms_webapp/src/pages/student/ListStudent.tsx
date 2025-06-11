import React, { useEffect, useState } from "react"
import { Link, useNavigate, useLocation } from "react-router-dom"
import { MainLoader, SendEmailPopup, StudentApplicationList, StudentApplicationSearch } from "../../components/Page"
import {
    useAutoAssignApplicationsMutation,
    useGetStudentApplicationsQuery,
    useGetStudentCourseQuery,
    usePrintStudentCardsMutation,
    useSendStudentApplicationResultMutation,
} from "../../apis/studentApplicationApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useGetUsersQuery } from "../../apis/userApi"
import { SD_Role, SD_Role_Name } from "../../utility/SD"
import { toastNotify } from "../../helper"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { apiResponse } from "../../interfaces"
import { useGetAllEmailTemplateQuery } from "../../apis/emailTemplateApi"
import emailTemplateModel from "../../interfaces/emailTemplateModel"
import StudentSearch from "../../components/Page/student/StudentSearch"
import { useExportStudentsByCourseMutation } from "../../apis/studentApi"
import StudentList from "../../components/Page/student/StudentList"
import { useGetStudentGroupsQuery } from "../../apis/studentGroupApi"
import { useGetCourseByIdQuery } from "../../apis/courseApi"

function ListStudent() {
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? "")
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)
    const [printStudentCards] = usePrintStudentCardsMutation()
    const [exportStudentsByCourse] = useExportStudentsByCourseMutation()
    const [sendEmail] = useSendStudentApplicationResultMutation()
    const { data: listSecretary, isLoading: reviewerLoading } = useGetUsersQuery({ roleId: SD_Role.SECRETARY })
    const { data: listEmailTemplate, isLoading: emailTemplateLoading } = useGetAllEmailTemplateQuery({})
    const isLoading = !listCourseFromStore || !currentCourse || reviewerLoading || !currentUserRole || !currentUserId

    const navigate = useNavigate()
    const location = useLocation()

    const [isModalOpenSendResult, setIsModalOpenSendResult] = useState(false)
    const [clearRowsFlag, setClearRowsFlag] = useState(false) // Cờ để xóa các hàng đã chọn
    const [selectedRows, setSelectedRows] = useState<number[]>([])
    const [istLoadingCard, setIsLoadingCard] = useState(false)
    const [isLoadingExcel, setIsLoadingExcel] = useState(false)
    const listTemplate: emailTemplateModel[] = listEmailTemplate?.result || []

    const searchParamsFromUrl = new URLSearchParams(location.search)
    const initialSearchParams = {
        courseId: searchParamsFromUrl.get("courseId") || currentCourse?.id,
        studentCode: searchParamsFromUrl.get("studentCode") || "",
        name: searchParamsFromUrl.get("name") || "",
        phoneNumber: searchParamsFromUrl.get("phoneNumber") || "",
        status: searchParamsFromUrl.get("status") || "",
        studentGroup: searchParamsFromUrl.get("studentGroup") || "",
        dateOfBirthFrom: searchParamsFromUrl.get("dateOfBirthFrom") || "",
        dateOfBirthTo: searchParamsFromUrl.get("dateOfBirthTo") || "",
        gender: searchParamsFromUrl.get("gender") || "",
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
            studentCode: searchParamsFromUrl.get("studentCode") || "",
            name: searchParamsFromUrl.get("name") || "",
            phoneNumber: searchParamsFromUrl.get("phoneNumber") || "",
            status: searchParamsFromUrl.get("status") || "",
            studentGroup: searchParamsFromUrl.get("studentGroup") || "",
            dateOfBirthFrom: searchParamsFromUrl.get("dateOfBirthFrom") || "",
            dateOfBirthTo: searchParamsFromUrl.get("dateOfBirthTo") || "",
            gender: searchParamsFromUrl.get("gender") || "",
        }

        setSearchParams(newSearchParams)
    }, [location.search, currentCourse?.id])
    const {
        data: applicationData,
        isLoading: applicationLoading,
        refetch,
    } = useGetStudentCourseQuery(searchParams, {
        refetchOnMountOrArgChange: true, // This enables refetching on mount or when arguments change
    })
    const { data: studentGroup, isLoading: studentGroupLoading } = useGetStudentGroupsQuery(
        searchParamsFromUrl.get("courseId") || currentCourse?.id,
    )
    const handleSearch = (params: any) => {
        setSearchParams(params)
        const queryParams = new URLSearchParams(params).toString()
        navigate(`/students?${queryParams}`)
    }

    const handlePrintCards = async () => {
        if (selectedRows.length === 0) {
            toastNotify("Vui lòng chọn khóa sinh để in thẻ", "error")
            return
        }
        setIsLoadingCard(true)
        try {
            const response = await printStudentCards({
                studentIds: selectedRows,
                courseId: searchParamsFromUrl.get("courseId") || currentCourse?.id,
            }).unwrap()

            // Kiểm tra nếu phản hồi không thành công
            if (!response.ok) {
                throw new Error("Failed to print student cards")
            }

            // Lấy tên file từ header "Content-Disposition"
            const contentDisposition = response.headers.get("Content-Disposition")
            let fileName = "the_hoc_sinh.pdf" // Tên mặc định nếu không tìm thấy tên file

            if (contentDisposition) {
                // Kiểm tra xem header có chứa filename*=UTF-8 không
                let fileNameMatch = contentDisposition.match(/filename\*=UTF-8''(.+)/)

                if (fileNameMatch && fileNameMatch[1]) {
                    // Giải mã tên file từ encoding UTF-8
                    fileName = decodeURIComponent(fileNameMatch[1])
                } else {
                    // Nếu không có filename*=UTF-8, thử lấy filename thông thường
                    fileNameMatch = contentDisposition.match(/filename="(.+)"/)
                    if (fileNameMatch && fileNameMatch[1]) {
                        fileName = fileNameMatch[1]
                    }
                }
            }

            // Lấy file blob từ API (PDF)
            const blob = await response.blob()

            // Tạo URL từ blob
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", fileName) // Sử dụng tên file lấy từ header
            document.body.appendChild(link)
            link.click()

            // Cleanup sau khi tải file xong
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
            clearSelectedRows()
        } catch (error) {
            toastNotify("Không thể in thẻ với khóa sinh chưa được phân vào chánh.", "error")
        }

        setIsLoadingCard(false)
    }

    const handleExportExcel = async () => {
        setIsLoadingExcel(true)
        try {
            const response = await exportStudentsByCourse(
                searchParamsFromUrl.get("courseId") || currentCourse?.id,
            ).unwrap()

            // Lấy tên file từ header "Content-Disposition"
            const contentDisposition = response.headers.get("Content-Disposition")
            let fileName = "download.xlsx" // Tên mặc định nếu không tìm thấy tên file

            if (contentDisposition) {
                // Kiểm tra xem header có chứa filename*=UTF-8 không
                let fileNameMatch = contentDisposition.match(/filename\*=UTF-8''(.+)/)

                if (fileNameMatch && fileNameMatch[1]) {
                    // Giải mã tên file từ encoding UTF-8
                    fileName = decodeURIComponent(fileNameMatch[1])
                } else {
                    // Nếu không có filename*=UTF-8, thử lấy filename thông thường
                    fileNameMatch = contentDisposition.match(/filename="(.+)"/)
                    if (fileNameMatch && fileNameMatch[1]) {
                        fileName = fileNameMatch[1]
                    }
                }
            }

            // Lấy file blob từ API
            const blob = await response.blob()

            // Tạo URL từ blob
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", fileName) // Sử dụng tên file lấy từ header
            document.body.appendChild(link)
            link.click()

            // Cleanup sau khi tải file xong
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
        } catch (error) {
            console.error("Lỗi khi tải file mẫu:", error)
            toastNotify("Không thể tải file mẫu.", "error")
        }
        setIsLoadingExcel(false)
    }

    const handleSendResult = () => {
        if (selectedRows.length === 0) {
            toastNotify("Chọn khóa sinh để gửi email", "error")
            return
        }
        setIsModalOpenSendResult(true)
    }

    const handleSendResultConfirm = async (title: string, content: string) => {
        try {
            const response: apiResponse = await sendEmail({
                ListStudentApplicationId: selectedRows,
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

    if (isLoading || applicationLoading || emailTemplateLoading || studentGroupLoading) {
        return <MainLoader />
    }
    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách khóa sinh</h3>
            </div>
            <StudentSearch
                onSearch={handleSearch}
                courseList={listCourseFromStore}
                currentCourse={currentCourse}
                studentGroupList={studentGroup?.result}
            />
            <div className="container text-end mt-4">
                {(currentUserRole == SD_Role_Name.SECRETARY || currentUserRole == SD_Role_Name.MANAGER) && (
                    <div>
                        <button className="btn btn-primary btn-sm ms-2 me-2" onClick={handleSendResult}>
                            <i className="bi bi-envelope me-1"></i>
                            Gửi gmail
                        </button>
                        <button
                            className="btn btn-primary btn-sm"
                            disabled={istLoadingCard}
                            onClick={() => handlePrintCards()}
                        >
                            <i className="bi bi-person-vcard me-1"></i>
                            {istLoadingCard ? "Đang tải..." : "In thẻ"}
                        </button>
                        <button
                            className="btn btn-primary btn-sm ms-2"
                            onClick={() => handleExportExcel()}
                            disabled={isLoadingExcel}
                        >
                            <i className="bi bi-arrow-bar-down me-1"></i>
                            {isLoadingExcel ? "Đang tải..." : "Tải Excel"}
                        </button>
                    </div>
                )}
                {currentUserRole != SD_Role_Name.SECRETARY && currentUserRole != SD_Role_Name.MANAGER && (
                    <div>
                        <button
                            className="btn btn-primary btn-sm ms-2"
                            onClick={() => handleExportExcel()}
                            disabled={isLoadingExcel}
                        >
                            <i className="bi bi-arrow-bar-down me-1"></i>
                            {isLoadingExcel ? "Đang tải..." : "Tải Excel"}
                        </button>
                    </div>
                )}
            </div>
            <div className="mt-2">
                <StudentList
                    applications={applicationData?.result}
                    studentGroupList={studentGroup?.result}
                    onSelectUser={handleSelectRows}
                    clearSelectedRows={clearRowsFlag}
                    currentCourse={currentCourse}
                    searchParams={searchParams}
                    isGroupDetail={false}
                />
            </div>
            <SendEmailPopup
                isOpen={isModalOpenSendResult}
                onClose={() => setIsModalOpenSendResult(false)}
                onConfirm={handleSendResultConfirm}
                listTemplate={listTemplate}
                select={3} // Template mặc định
                onClearSelectedRows={clearSelectedRows}
            />
        </div>
    )
}

export default ListStudent

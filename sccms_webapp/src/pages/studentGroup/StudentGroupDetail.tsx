import { useState } from "react"
import { useNavigate, useLocation, useParams } from "react-router-dom"
import { MainLoader, SendEmailPopup, StudentGroupAddPopup } from "../../components/Page"
import {
    useGetStudentCourseQuery,
    usePrintStudentCardsMutation,
    useSendStudentApplicationResultMutation,
} from "../../apis/studentApplicationApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { SD_Gender, SD_Role_Name } from "../../utility/SD"
import { toastNotify } from "../../helper"
import { apiResponse } from "../../interfaces"
import { useGetAllEmailTemplateQuery } from "../../apis/emailTemplateApi"
import emailTemplateModel from "../../interfaces/emailTemplateModel"
import { useExportStudentsByCourseMutation, useExportStudentsByStudentGroupMutation } from "../../apis/studentApi"
import StudentList from "../../components/Page/student/StudentList"
import { useAddStudentsIntoGroupMutation, useGetStudentGroupQuery } from "../../apis/studentGroupApi"
import StudentGroupSearch from "../../components/Page/studentGroup/StudentGroupSearch"
import { Card, CardBody, CardHeader } from "react-bootstrap"

function StudentGroupDetail() {
    const { id } = useParams<{ id: string }>()
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? "")
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)
    const [printStudentCards] = usePrintStudentCardsMutation()
    const [ExportStudentsByGroup] = useExportStudentsByStudentGroupMutation()
    const [sendEmail] = useSendStudentApplicationResultMutation()
    const [addStudentToGroup] = useAddStudentsIntoGroupMutation()
    const { data: studentGroup, isLoading: studentGroupLoading } = useGetStudentGroupQuery(id)
    const { data: listEmailTemplate, isLoading: emailTemplateLoading } = useGetAllEmailTemplateQuery({})
    const isLoading =
        !listCourseFromStore ||
        !currentCourse ||
        !currentUserRole ||
        !currentUserId ||
        studentGroupLoading ||
        emailTemplateLoading

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
        studentCode: searchParamsFromUrl.get("studentCode") || "",
        name: searchParamsFromUrl.get("name") || "",
        phone: searchParamsFromUrl.get("phone") || "",
        status: searchParamsFromUrl.get("status") || "",
        dateOfBirthFrom: searchParamsFromUrl.get("dateOfBirthFrom") || "",
        dateOfBirthTo: searchParamsFromUrl.get("dateOfBirthTo") || "",
        gender: searchParamsFromUrl.get("gender") || "",
    }

    const [searchParams, setSearchParams] = useState(initialSearchParams)

    const combinedSearchParams = {
        ...searchParams,
        courseId: studentGroup?.result.courseId ?? 0,
        studentGroup: id,
    }

    const {
        data: applicationData,
        isLoading: applicationLoading,
        refetch,
    } = useGetStudentCourseQuery(combinedSearchParams, {
        refetchOnMountOrArgChange: true, // This enables refetching on mount or when arguments change
        skip: !studentGroup?.result,
    })

    const handleSearch = (params: any) => {
        setSearchParams(params)
        const queryParams = new URLSearchParams(params).toString()
        navigate(`/student-groups/${id}?${queryParams}`, { replace: false })
    }
    //----------------Add student----------------
    const [isModalOpenAddStudent, setIsModalOpenAddStudent] = useState(false)
    const handleAddStudent = () => {
        setIsModalOpenAddStudent(true)
    }

    const handleAddStudents = async (studentIds: number[]) => {
        const payload = {
            studentIds: studentIds,
            studentGroupId: studentGroup?.result.id,
            courseId: studentGroup?.result.courseId,
        }
        try {
            const response: apiResponse = await addStudentToGroup(payload)
            if (response.data?.isSuccess) {
                toastNotify("Thêm khóa sinh thành công", "success")
                refetch()
            } else {
                console.log(response)
                console.log(response?.error?.data)
                const errorMessage = response.error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                toastNotify(errorMessage, "error")
            }
        } catch (error) {
            toastNotify("Có lỗi xảy ra", "error")
        }
    }
    //----------------

    const handlePrintCards = async () => {
        if (selectedRows.length === 0) {
            toastNotify("Vui lòng chọn khóa sinh để in thẻ", "error")
            return
        }
        setIsLoadingCard(true)
        try {
            const response = await printStudentCards({ studentIds: selectedRows, courseId: currentCourse?.id }).unwrap()

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
            toastNotify("Có lỗi xảy ra.", "error")
        }

        setIsLoadingCard(false)
    }

    const handleExportExcel = async () => {
        setIsLoadingExcel(true)
        try {
            const response = await ExportStudentsByGroup(id).unwrap()

            // Lấy tên file từ header "Content-Disposition"
            const contentDisposition = response.headers.get("Content-Disposition")
            let fileName = "Danh_sach_khoa_sinh.xlsx" // Tên mặc định nếu không tìm thấy tên file

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
                CourseId: currentCourse?.id ?? null,
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
                <h3 className="fw-bold primary-color">Chi tiết chánh</h3>
            </div>
            <Card className="mb-3">
                <CardHeader>Thông tin chánh</CardHeader>
                <CardBody>
                    <div className="row">
                        <div className="col-md-4">
                            <div className="row mb-2">
                                <div className="col-md-4 fw-bold">Tên chánh:</div>
                                <div className="col-md-8">{studentGroup?.result.groupName}</div>
                            </div>
                            <div className="row mb-2">
                                <div className="col-md-4 fw-bold">Giới tính:</div>
                                <div className="col-md-8">
                                    {studentGroup?.result?.gender === SD_Gender.Female ? "Nữ" : "Nam"}
                                </div>
                            </div>
                        </div>
                        <div className="col-md-8">
                            <div className="row mb-2">
                                <div className="col-md-2 fw-bold">Sĩ số:</div>
                                <div className="col-md-10">{studentGroup?.result.students.length}</div>
                            </div>
                            <div className="row mb-2">
                                <div className="col-md-2 fw-bold">Huynh trưởng:</div>
                                <div className="col-md-10">
                                    {studentGroup?.result?.supervisors?.length > 0
                                        ? studentGroup.result.supervisors?.map((supervisor, index) => (
                                              <span
                                                  key={index}
                                                  style={{ cursor: "pointer" }}
                                                  className="user-div"
                                                  onClick={() => navigate(`/user/${supervisor.id}`)}
                                              >
                                                  {`${supervisor.userName} - ${supervisor.fullName}`}
                                                  {index < studentGroup.result.supervisors.length - 1 && ", "}
                                              </span>
                                          ))
                                        : "Chưa có"}
                                </div>
                            </div>
                        </div>
                    </div>
                </CardBody>
            </Card>
            <StudentGroupSearch onSearch={handleSearch} studentGroup={studentGroup?.result} />
            <div className="container text-end mt-4">
                {(currentUserRole == SD_Role_Name.SECRETARY || currentUserRole == SD_Role_Name.MANAGER) && (
                    <div>
                        <button className="btn btn-primary btn-sm ms-2 me-2" onClick={handleAddStudent}>
                            <i className="bi bi-plus-lg"></i>Thêm khóa sinh
                        </button>
                        <button className="btn btn-primary btn-sm me-2" onClick={handleSendResult}>
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
                    isGroupDetail={true}
                />
            </div>
            <button className="btn btn-secondary mt-4" onClick={() => navigate("/student-groups")}>
                Quay lại
            </button>
            <SendEmailPopup
                isOpen={isModalOpenSendResult}
                onClose={() => setIsModalOpenSendResult(false)}
                onConfirm={handleSendResultConfirm}
                listTemplate={listTemplate}
                select={3} // Template mặc định
                onClearSelectedRows={clearSelectedRows}
            />
            <StudentGroupAddPopup
                show={isModalOpenAddStudent}
                onHide={() => setIsModalOpenAddStudent(false)}
                onAddStudents={handleAddStudents}
                currentStudentGroup={studentGroup?.result}
            />
        </div>
    )
}

export default StudentGroupDetail
